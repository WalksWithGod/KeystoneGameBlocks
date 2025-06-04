using System;


namespace Authentication
{
    /// <summary>
    /// A kerberos like authentication library
    /// </summary>
    public class Server
    {
        private Algorithm _hashAlgorithm = Algorithm.SHA256;
        private const string TICKET_GRANTING_SERVICE_PASSWORD = "apqo-3018dzzxvb15555gG";
        private int m_Expiration;

        public Server()
        {
        }

        public Server (Algorithm algorithm)
        {
        }

#region registration
        ///// <summary>
        ///// Registers a new service to the Authentication service. This simply means
        ///// that the service is added to the "services" table with a generated password
        ///// 
        ///// All services registered can be used by a client/user once
        ///// they are authenticated unless the access priveledges of that user are too low.
        ///// </summary>
        ///// <param name="name"></param>
        //public void RegisterService (string name)
        //{
        //    // verify the service isn't already registered
        //    byte[] password = Hash.GenerateRandomPassword(8, 12);
        //    Service service = new Service (name, password);
        //}

        //public void RegisterUser(string name)
        //{

        //}
#endregion

        private string ServiceName(int serviceID) 
        {
            return TICKET_GRANTING_SERVICE_PASSWORD;
        }

        //public static byte[] EncryptUser(ref User user, ref Key sessionkey)
        //{  
        //    if (user == null || sessionkey == null) throw new ArgumentNullException();
        //    return null;
        //}

        //public static User DecryptUser(ref Key sessionKey)
        //{
        //    //if it fails, it returns Nothing
        //    return null;

        //}

        private string ServicePassword(int serviceID) 
        {
            return "";
        }
        
        public bool Authenticate(string password, string hash) 
        {
            bool authentic = Hash.VerifyHash(password, _hashAlgorithm, hash);
            if (!authentic) return false;
            return true;
        }

        public Ticket CreateTicketForService(string servicename) 
        {
            return null;
        }
    
        private int TicketIndex()
        {
            return 0;
        }

        private bool RemoveExpiredTickets()
        {
            return true;
        }
    }
}
