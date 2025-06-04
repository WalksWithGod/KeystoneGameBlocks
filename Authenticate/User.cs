namespace Authentication
{
    /// <summary>
    /// Class is used in the UserCache list so that some database queries for password can be skipped.
    /// </summary>
    public class User
    {
        public enum AuthenticationStatus
        {
            Authenticating,
            AuthenticationFailed,
            Authenticated,
            Offline
        }

        public uint ID;             // unique ID for authenticated users
        public uint CustomerID; // correlates with an ID in a customer database
        public AuthenticationStatus Status;
        public string Nickname;
        public string Password;

        // these three fields should be derivived from CustomerID and the Customers table
        //public string FirstName;
        //public string LastName;
        //public string MiddleInitial;
        
    }
}
