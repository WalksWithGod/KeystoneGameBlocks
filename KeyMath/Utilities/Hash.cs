using System;

namespace Keystone.Utilities
{
    // Jenkins One at a time hash.
    // http://stackoverflow.com/questions/1079192/is-it-possible-to-combine-hash-codes-for-private-members-to-generate-a-new-hash
    // http://home.comcast.net/~bretm/hash/

    //  you can then use this like so:
    //  uint h = 0; 
    //  foreach(string item in collection)  
    //  { 
    //      Hash(ref h, item); 
    //  } 
    //  return Avalanche(h); 

    public class JenkinsHash
    {
        /// <summary>
        /// Hash an array of byte data
        /// </summary>
        /// <param name="d"></param>
        /// <param name="len"></param>
        /// <param name="h"></param>
        private static unsafe void Hash(byte* d, int len, ref uint h) 
        { 
            for (int i = 0; i < len; i++) 
            { 
                h += d[i]; 
                h += (h << 10); 
                h ^= (h >> 6); 
            } 
        }

        public unsafe static void Hash(byte[] d, ref uint h)
        {
            fixed (byte* b = d)
            {
                Hash(b, d.Length, ref h);
            }

        }

        public unsafe static void Hash(string s, ref uint h) 
        { 
            fixed (char* c = s)             
            { 
                byte* b = (byte*)(void*)c; 
                Hash(b, s.Length * 2, ref h); 
            } 
        } 
         
        public unsafe static int Avalanche(uint h) 
        { 
            h += (h<< 3);    
            h ^= (h>> 11);   
            h += (h<< 15);   
            return *((int*)(void*)&h); 
        } 
    }
}
