using System;

namespace Authentication
{
    public class Key
    {
       byte[] Bytes;

       public Key()
       {
           // generate a random initialization vector (IV) 
            Bytes = System.Security.Cryptography.RijndaelManaged.Create().IV;
       }

        public Key(byte[] data)
        {
            if (data == null || data.Length == 0) throw new ArgumentOutOfRangeException ();
            Bytes = data;
        }
    }
}
