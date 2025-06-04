using System;
using System.Text;

namespace Authentication
{
    /// <summary>
    /// An authenticated login message contains an authenticator followed by the Ticket.  
    /// The authenticater can only be decrypted using the sessionKey.  The user obtained
    /// his copy of the session key encrypted using his own password and then uses that
    /// session key to build this authenticator.
    /// </summary>
    public class AuthenticatedLogin
    {
        const int HEADER_LENGTH_BYTE = 1;

        private byte[] m_UserName;
        public byte[] SessionKey;
        public Ticket Ticket;
        private byte[] m_TicketData;
        byte[] m_Data;

        // Authenticator - {username:address} encrypted with session key
        
        /// <summary>
        /// Constructor used by client prior to sending the authenticated reply to a service for access
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="sessionKey"></param>
        /// <param name="ticketData"></param>
        public AuthenticatedLogin(string userName,  byte[] sessionKey, byte[] ticketData)
        {
            if (string.IsNullOrEmpty(userName) || sessionKey == null || sessionKey.Length == 0 || ticketData == null || ticketData.Length == 0) throw new ArgumentNullException();
            if (userName.Length > 32) throw new ArgumentOutOfRangeException();

            m_TicketData = ticketData;
            m_UserName = Ticket.PadByteArray(Encoding.UTF8.GetBytes(userName), 32);
            SessionKey = sessionKey;
            
            // create an authenticator which is just the user name, then
            // encrypt the authenticator with the session key that was sent to the user by the ticket granter 
            // encrypted with the user's own password and a server generated iv that was sent in the clear at the 
            // beginning of the ticket
            // What this proves to the service that a user then wants to authenticate with is that the user also had a copy of the session key
            // or else it would never have been able to construct an authenticator
            byte[] authenticator = new byte[32];
            Array.Copy(m_UserName, authenticator, 32);
            byte[] iv = new byte[16];
            // the first 16 bytes of the ticketData contain an initialization vector which we'll also use along with the session key to encrypt the authenticator
            Array.Copy(ticketData, iv, 16);
            byte[] encryptedAuthenticator = RijndaelSimple.Encrypt(authenticator, sessionKey, iv);

            // append the ticket to the encrypted bytes of the authenticator
            m_Data = new byte[encryptedAuthenticator.Length + m_TicketData.Length + HEADER_LENGTH_BYTE];
            m_Data[0] = (byte)encryptedAuthenticator.Length;
            int pos = HEADER_LENGTH_BYTE;
            Array.Copy(encryptedAuthenticator, 0, m_Data, pos, encryptedAuthenticator.Length);
            pos += encryptedAuthenticator.Length;
            Array.Copy(m_TicketData, 0, m_Data, pos, m_TicketData.Length);
        }

        /// <summary>
        /// Constructor used by a service when receiving an authenticated login request
        /// </summary>
        /// <param name="data"></param>
        /// <param name="servicePassword"></param>
        /// <param name="salt"></param>
        public AuthenticatedLogin(byte[] data, string servicePassword)
        {
            if (data == null || data.Length == 0 ||  string.IsNullOrEmpty(servicePassword)) throw new ArgumentNullException();
            m_Data = data;

            // ENCRYPTED authenticator length in first byte so we can compute the offset of the actual ticket
            int authenticatorLength = data[0];
            
            // store out the ticket
            m_TicketData = new byte[data.Length - authenticatorLength - 1];
            Array.Copy(data, authenticatorLength + 1, m_TicketData, 0, m_TicketData.Length);
            
            // decrypt the ticket first to get the session key and the iv 
            Ticket = new Ticket(m_TicketData, servicePassword);

            // decrypt the authenticator using the session key and IV from the ticket
            byte[] encryptedAuthenticator = new byte[authenticatorLength];
            Array.Copy(data, 1, encryptedAuthenticator, 0, authenticatorLength);
            byte[] authenticator = Authentication.RijndaelSimple.Decrypt(encryptedAuthenticator, Ticket.SessionKey, Ticket.InitializationVector);

            SessionKey = Ticket.SessionKey;
            m_UserName = new byte[32];
            Array.Copy(authenticator, 0, m_UserName, 0, 32);
        }

        public string UserName 
        { 
            get 
            { 
                if (m_UserName  == null || m_UserName.Length == 0) return null;
                return UTF8Encoding.UTF8.GetString(Ticket.TrimTrailingNullBytes(m_UserName)); 
            } 
        }
        public byte[] ToBytes() { return m_Data; }
    }
}
