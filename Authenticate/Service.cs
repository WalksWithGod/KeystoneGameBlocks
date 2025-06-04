using System;

namespace Authentication
{
    public class Service
    {
        public string Name;
        byte[] Password;
        string Address; // does a service running somewhere have to stay at a fixed IP?  what if the service uses clustering?
        int Port;

        const int MAX_SERVICENAME_LENGTH = 256;
        const int MAX_PASSWORD_LENGTH = 32;

        public Service(string name, string password) : this(name, Hash.StringToBytes(password)) { }

        public Service(string name, byte[] password)
        {
            if (name.Length > MAX_SERVICENAME_LENGTH  || password.Length > MAX_PASSWORD_LENGTH )
                throw new ArgumentOutOfRangeException ();

           Name = name;
            Password = password ;
        }
    }
}
