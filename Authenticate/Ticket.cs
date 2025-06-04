using System;
using System.Text;

namespace Authentication
{
    // note: all times are server side times.  therefore its critical that all services that need to authenticate users
    // by sychronized to the same time.  This is easy if we have all our servers periodically synch to network time servers
    // every 8 hours or so.  NOTE: Trying to standardize on GMT(0) doesnt help because the client's computer
    // maybe still be too fast, too slow, or just plain wrong.  We "could" have the charon client connect to a 
    //time server first...

    /// <summary>
    /// A ticket contains the name of the service for which the ticket is to be used
    /// a session key, an expiration for that session, a timestamp on when the ticket was issued
    /// the user's login name, ip address.  The ticket is encrypted using the service's own password so that
    /// only the service itself can decrypt it.  When the user is authenticated and wishes to access another service
    /// such as a master server, they submit that ticket which only the service can decrypt.
    /// </summary>
    public class Ticket
    {
        const int MAX_TAG_LENGTH = 1024;
        const UInt16 SizeOfUint16 = 2;
        public static readonly long TICKS_UNTIL_TICKET_EXPIRATION = 60 * 10000; // A single tick represents one hundred nanoseconds or one ten-millionth of a second. There are 10,000 ticks in a millisecond.
        DateTime TimeStamp;
        long Expiration; // milliseconds that ticket is good for

        public byte[] InitializationVector;

        // fixed length fields
        public byte[] SessionKey;
        byte[] m_Address = new byte[4];
        byte[] m_Expiration = new byte[8];
        byte[] m_Time = new byte[8];

        // variable length fields
        byte[] m_UserName ;
        byte[] m_ServiceName;
        byte[] m_Tag;

        byte[] m_ticketData;
     
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sessionKey"></param>
        /// <param name="userName"></param>
        /// <param name="userAddress"></param>
        /// <param name="serviceName"></param>
        /// <param name="passKeyBytes"></param>
        /// <param name="expiration"></param>
        /// <param name="tag">Mostly not used, but useful for authorization specific files on a webserver</param>
        public Ticket (byte[] sessionKey, string userName, string userAddress, string serviceName, byte[] passKeyBytes, long expiration, string tag)
        {
            if (userName.Length > 32 || serviceName.Length > 32 || tag.Length > MAX_TAG_LENGTH) throw new ArgumentOutOfRangeException();
          
            // TODO: Critical that we verify userName, userAddress, serviceName, and tag all contain valid ASCII or 
            // there might be conversion errors(?) using Encoding.UTF8

            SessionKey = sessionKey;
            InitializationVector = RijndaelSimple.GenerateIV();
            
            // session key = 16 bytes
            if (sessionKey.Length != 16) throw new ArgumentOutOfRangeException ("invalid session key length");
            
            
            // userAddress = 4 bytes
            System.Net.IPAddress address;
            if (System.Net.IPAddress.TryParse (userAddress, out address))
            {
                m_Address = address.GetAddressBytes();
            }
            else throw new Exception ("invalid user address '" + userAddress + "'");

            // username, servicename and tag are variable length
            m_UserName = Encoding.UTF8.GetBytes(userName); // PadByteArray(Encoding.UTF8.GetBytes(userName), 32);
            m_ServiceName = System.Text.Encoding.UTF8.GetBytes(serviceName); // PadByteArray(System.Text.Encoding.UTF8.GetBytes(serviceName), 32);     
            m_Tag = System.Text.Encoding.UTF8.GetBytes (tag);
            
            // timestamp = 8 bytes
            long ticks = DateTime.Now.Ticks;
            m_Time = BitConverter.GetBytes (ticks);
            
            // expiration = 8 bytes
            m_Expiration = BitConverter.GetBytes(ticks + TICKS_UNTIL_TICKET_EXPIRATION);
            
            // delimiters suck so lets just write fixed field lengths.  Our strings will be padded with trailing nulls to maintain the fixed field sizes
            // our "Tag" will be the only string field that is preceeded by a two byte length followed by the payload.  Tag is mostly unused
            // but it's very useful for authorizing specific files on our webserver

            byte[] clearData = new byte[sessionKey.Length + m_Address.Length + 
                                                    m_Expiration.Length + m_Time.Length + 
                                                    m_UserName.Length + SizeOfUint16  +
                                                    m_ServiceName.Length  + SizeOfUint16 +
                                                    m_Tag.Length + SizeOfUint16]; 

            int offset = 0;
           // allocate and copy to our final ticket array
            Array.Copy(sessionKey, 0, clearData, offset, sessionKey.Length);  offset+= sessionKey.Length ;
            Array.Copy(m_Address, 0, clearData, offset, m_Address.Length); offset += m_Address.Length ;
            Array.Copy(m_Expiration, 0, clearData, offset, m_Expiration.Length); offset += m_Expiration.Length;
            Array.Copy(m_Time, 0, clearData, offset, m_Time.Length); offset += m_Time.Length;

            WriteVariableLengthData(m_UserName, clearData, ref offset);
            WriteVariableLengthData(m_ServiceName, clearData, ref offset);
            WriteVariableLengthData(m_Tag, clearData, ref offset);
         
            
            // now encrypt the clear data using the service password and iv
            byte[] encryptedData = Authentication.RijndaelSimple.Encrypt(clearData, passKeyBytes, InitializationVector);

            // init a new array m_ticketData[] that will contain the final results of everything.  
            // Copy the IV in the clear to the very front followed by the encrypted data
            m_ticketData = new byte[16 + encryptedData.Length];
            Array.Copy (InitializationVector , 0, m_ticketData , 0, 16);
            Array.Copy(encryptedData, 0, m_ticketData, 16, encryptedData.Length);

        }

        private void WriteVariableLengthData(byte[] data, byte[] destination, ref int offset)
        {
            ushort variableLength = (ushort)data.Length;
            byte[] len = BitConverter.GetBytes(variableLength);
            System.Diagnostics.Trace.Assert(len.Length == 2);
            Array.Copy(len, 0, destination, offset, (int)SizeOfUint16); offset += SizeOfUint16;
            if (variableLength > 0)
            {
                Array.Copy(data, 0, destination, offset, data.Length); offset += data.Length;
            }
        }

        private byte[] ReadVariableLengthData(byte[] source,  ref int offset)
        {
            if (source == null || offset <= 0) throw new ArgumentOutOfRangeException();
            byte[] variableLength = new byte[2];
            Array.Copy(source, offset, variableLength, 0, SizeOfUint16); offset += SizeOfUint16;
            ushort len = BitConverter.ToUInt16(variableLength, 0);
            if (len > 0)
            {
                // TODO: verify app that is trying to recreate a ticket should wrap Ticket constructor in try/catch
                if (len > source.Length - offset) throw new Exception();
                byte[] result = new byte[len];
                Array.Copy(source, offset, result, 0, len); offset += len;
                return result;
            }
            return null;
        }

        public static byte[] PadByteArray(byte[] data, int size)
        {
            if (data == null) throw new ArgumentNullException();
            if (size <= 0) throw new ArgumentOutOfRangeException();
            if (data.Length > size) throw new ArgumentOutOfRangeException();

            byte[] result = new byte[size];
            Array.Copy(data, 0, result, 0, data.Length);
            return result;
        }
        
        public static byte[] TrimTrailingNullBytes(byte[] data)
        {
            int i = data.Length - 1;
            // starting from the end of the src array and working forward
            // find the first non null character
            while (data[i] == 0)
                --i;

            byte[] result = new byte[i + 1];
            Array.Copy(data, result, i + 1);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ticketData"></param>
        /// <param name="servicePassword">Used to decrypt the ticketData</param>
        public Ticket (byte[] ticketData, string servicePassword)
        {
            // a network packet containing a ticket can either be preceded by an authenticator 
            // or it can be preceded by just a sessionKey depending on the context of it's creation (reply or authenticatedlogin)
            // that is why we assume that ticketData passed in here is _just_ the relevant ticket portion
            // of the packet
            m_ticketData = ticketData;

            InitializationVector = new byte[16];
            Array.Copy(m_ticketData, 0, InitializationVector, 0, 16);

            byte[] ticketWithoutIV = new byte[m_ticketData.Length - 16];
            Array.Copy(ticketData, 16, ticketWithoutIV, 0, ticketWithoutIV.Length);
            byte[] keyBytes = Authentication.RijndaelSimple.GetKeyFromPassphrase(servicePassword, 16  * 8);
            byte[] clearData = Authentication.RijndaelSimple.Decrypt(ticketWithoutIV, keyBytes, InitializationVector);

            // so a ticket really should just contain it's own data
            SessionKey = new byte[16];
            m_Address = new byte[4];
            m_Expiration = new byte[8];
            m_Time = new byte[8];

            int offset = 0;
            // copy the decrypted data into seperate variables
            Array.Copy(clearData, offset, SessionKey, 0, SessionKey.Length ); offset += SessionKey.Length ;
            Array.Copy(clearData, offset, m_Address, 0, m_Address.Length); offset += m_Address.Length ;
            Array.Copy(clearData, offset, m_Expiration, 0, m_Expiration.Length); offset += m_Expiration.Length;
            Array.Copy(clearData, offset, m_Time, 0, m_Time.Length); offset += m_Time.Length;

            m_UserName = ReadVariableLengthData(clearData, ref offset); 
            m_ServiceName = ReadVariableLengthData(clearData, ref offset); 
            m_Tag = ReadVariableLengthData(clearData, ref offset);
            
        }

        public byte[] ToBytes()
        {
            return m_ticketData;
        }

        public bool IsExpired()
        {
            DateTime testTime = new DateTime(BitConverter.ToInt64(m_Time, 0));
            DateTime expiredTime = new DateTime(BitConverter.ToInt64(m_Expiration, 0));
            long remaining = (expiredTime - testTime).Ticks;

            return remaining < 0;
        }

        public string Tag
        {
            get 
            { 
                if (m_Tag == null) return null;
                return Encoding.UTF8.GetString(m_Tag);
            }
        }

        public bool IsAuthentic(string userName, string userAddress, string serviceName)
        {
            if (userName != Encoding.UTF8.GetString(TrimTrailingNullBytes(m_UserName)))
                return false;

            System.Net.IPAddress convertedAddress = new System.Net.IPAddress(m_Address);
#if !DEBUG
            if (convertedAddress.ToString() != userAddress)
            {
                System.Diagnostics.Debug.WriteLine ("Ticket.IsAuthentic() - FAILED.  IP Address mismatch.");
                return false;
            }
#else
            // below only occurs during debug builds so it's ok for testing
            if (convertedAddress.ToString() != userAddress)
                System.Diagnostics.Trace.Assert(false,"SAFE TO IGNORE", "Client running on same machine as greater than 0 but less than all sever components will result in IP address disparity.  Ok to ignore this error while testing.");
#endif 
            if (serviceName != Encoding.UTF8.GetString(TrimTrailingNullBytes(m_ServiceName)))
                return false;
            
            if (IsExpired())
                return false;

            return true;
        }
    }
}

