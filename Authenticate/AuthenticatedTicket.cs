using System;

namespace Authentication
{
    /// <summary>
    /// An AuthenticatedTicket is sent from a ticket granting server to a client in response to getting a request from that client
    /// to get an access ticket to some other service that is governed by this same authentication system.
    /// An AuthenticatedTicket consists of 
    /// 1) encrypted reply header which the client can decrypt 
    /// 2) an encrypted ticket which only the service the user wants to authenticate with can decrypt.  
    /// When that service decrypts the ticket, it will be able to obtain a "sessionkey" which it can use to 
    /// decrypt the "authenticator" which proves to that Service that not only is the ticket genuine, 
    /// but also that the ticket was NOT sent by an imposter who intercepted the ticket.
    /// </summary>
    public class AuthenticatedTicket
    {
        public byte[] SessionKey;
        public byte[] UserIV;
        public byte[] TicketData;
        private byte[] _data;

        /// <summary>
        /// Constructor used by ticket granting server to send to a client who's identity is not currently known such
        /// as when initially connecting to the Authentication server or Lobby server.
        /// </summary>
        /// <param name="userIP"></param>
        /// <param name="userName"></param>
        /// <param name="userPassword"></param>
        /// <param name="serviceName"></param>
        /// <param name="servicePassword"></param>
        /// <param name="expiration"></param>
        public AuthenticatedTicket(string userIP, string userName, string userPassword, string serviceName, string servicePassword, int expiration, string tag)
        : this(userIP, userName, Authentication.RijndaelSimple.GetKeyFromPassphrase(userPassword, 16 * 8), serviceName, servicePassword , expiration, tag)
        {           
        }


        /// <summary>
        /// Constructor used by a ticket granting server to create a ticket for another service on behalf of an _already_authenticated_user
        /// Such as a client wanting a filedownload authorization from a gameserver to which they've already established an authenticated connection.
        /// The only difference is the userPassword is a byte array because the client and server agree to use the original sessionkey as the
        /// password.  NOTE: The sessionkey is never sent in the clear in the initial Lobby\AuthenticationServer reply either, nor is it here.
        /// So an attacker who guessed the user's password and decrypted the reply header and obtained a session key would still also
        /// have to guess the server's password in order to construct a new Ticket that would match any phony authenticator.
        /// NOTE: This is why SessionKey is used to encrypt and decrypt authenticators!  They are basically a shared password that is never
        /// sent in the clear. 
        /// </summary>
        /// <param name="userIP"></param>
        /// <param name="userName"></param>
        /// <param name="userPassword"></param>
        /// <param name="serviceName"></param>
        /// <param name="servicePassword"></param>
        /// <param name="expiration"></param>
        /// <param name="tag">Mostly unused, but useful for authorizing specific files on a fileserver</param>
        public AuthenticatedTicket(string userIP, string userName, byte[] userPassword, string serviceName, string servicePassword, int expiration, string tag)
        {
            const int HEADER_LENGTH_BYTE = 1;

            // we create a unique session key
            byte[] sessionKey = PasswordGenerator.GenerateRandomKey(16);
            // will the encrypted session key wind up being exactly the same size?  16 bytes?
            UserIV = Authentication.RijndaelSimple.GenerateIV();

            byte[] usersPassKeyBytes = userPassword;
            byte[] servicePassKeyBytes = Authentication.RijndaelSimple.GetKeyFromPassphrase(servicePassword, 16 * 8);

            // add the session key to the reply header prior to encrypting it
            byte[] replyHeader = new byte[16];
            Array.Copy(sessionKey, replyHeader, 16);
            // reply header allows the recipient of the ticket request reply to verify that the ticket they've been sent back is authentic and 
            // ok to then pass along the ticket portion in their login to another service
            byte[] encryptedReplyHeader = Authentication.RijndaelSimple.Encrypt(replyHeader, usersPassKeyBytes, UserIV);

            Ticket ticket = new Ticket(sessionKey, userName, userIP, serviceName, servicePassKeyBytes, expiration, tag );
            TicketData = ticket.ToBytes();

            // append the encrypted ticket data to our reply which consists of a replyHeader followed by the ticket data
            // the reply header is simply the session key encrypted with the user's password
            _data = new byte[UserIV.Length + HEADER_LENGTH_BYTE + encryptedReplyHeader.Length + TicketData.Length];
            int pos = 0;
            Array.Copy(UserIV, 0, _data, pos, UserIV.Length);
            pos += UserIV.Length;
            if (encryptedReplyHeader.Length > 255) throw new Exception();
            _data[pos] = (byte)encryptedReplyHeader.Length;
            pos += HEADER_LENGTH_BYTE;
            Array.Copy(encryptedReplyHeader, 0, _data, pos, encryptedReplyHeader.Length);
            pos += encryptedReplyHeader.Length;
            Array.Copy(TicketData, 0, _data, pos, TicketData.Length);
        }

        /// <summary>
        /// Constructor used by the client to reconstruct a requested ticket that has been received over the wire.
        /// </summary>
        /// <param name="replyData"></param>
        /// <param name="password"></param>
        public AuthenticatedTicket (byte[] replyData, string password) : this(replyData, Authentication.RijndaelSimple.GetKeyFromPassphrase(password, 16 * 8))
        {           
        }

        public AuthenticatedTicket (byte[] replyData, byte[] password)
        {
            byte[] keyBytes = password;

            _data = replyData;

            // first 16 bytes is the UserIV we use to decrypt the header
            UserIV = new byte[16];
            Array.Copy(replyData, UserIV, 16);

            int pos = 16;

            // next byte is our encrypted header size
            byte size = replyData[16];
            pos += 1;

            // next 'size' bytes is our  SessionKey which is encrypted with our password + UserIV.

            byte[] encryptedBytes = new byte[size];
            Array.Copy(replyData, pos, encryptedBytes, 0, size);
            pos += encryptedBytes.Length;

            byte[] decryptedHeader = Authentication.RijndaelSimple.Decrypt(encryptedBytes, keyBytes, UserIV);

            // first 16 bytes is our decrypted header is the session key which we can now use to build an authenticator
            SessionKey = new byte[16];
            Array.Copy(decryptedHeader, SessionKey, 16);

            // the rest is the actual ticket
            TicketData = new byte[replyData.Length - pos];
            Array.Copy(replyData, pos, TicketData, 0, TicketData.Length);
        }
        public byte[] ToBytes() { return _data; }
    }
}
