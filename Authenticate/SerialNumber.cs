using System;

namespace Authentication
{
    public enum SNKeyLength
    {
        SN16 = 16, SN20 = 20, SN24 = 24, SN28 = 28, SN32 = 32
    }
    public enum SNKeyNumLength
    {
        SN4= 4 , SN8 = 8, SN12 =    12
    }
    public static class RandomSNKGenerator
    {
        /// Generate serial key with only numaric
        /// the supported length of the serial key
        /// returns formated serial key
        public static string GetSerialKeyNumaric(SNKeyNumLength keyLength)
        {
            System.Random rn = new System.Random(PasswordGenerator.GetRandomSeed());
            double sd = Math.Round(rn.NextDouble() * Math.Pow(10, (int)keyLength) + 4);
            return sd.ToString().Substring(0, (int)keyLength);
        }

        /// Generate standard serial key with alphanumaric format
        ///
        ///the supported length of the serial key
        /// returns formated serial key
        public static string GetSerialKeyAlphaNumaric(SNKeyLength keyLength)
        {
            System.Guid newguid = System.Guid.NewGuid();
            string randomStr = newguid.ToString("N");
            string tracStr = randomStr.Substring(0,(int)keyLength);
            tracStr = tracStr.ToUpper();
            char[] newKey =tracStr.ToCharArray();
            string newSerialNumber = "";
            switch (keyLength)
            {
                case SNKeyLength.SN16:
                    newSerialNumber = AppendSpecifiedStr(16,"-", newKey);
                    break;
                case SNKeyLength.SN20:
                    newSerialNumber =AppendSpecifiedStr(20, "-", newKey);
                    break;
                case SNKeyLength.SN24:
                    newSerialNumber = AppendSpecifiedStr(24, "-",newKey);
                    break;
                case SNKeyLength.SN28:
                    newSerialNumber =AppendSpecifiedStr(28, "-", newKey);
                    break;
                case SNKeyLength.SN32:
                    newSerialNumber = AppendSpecifiedStr(32, "-",newKey);
                    break;
            }

            return newSerialNumber;
        }

        private static string AppendSpecifiedStr(int length, string str, char[] newKey)
        {
            string newKeyStr = "";
            for (int i = 0; i < length; i++)
            {
                int k;
                for (k = i; k < 4 + i; k++)
                {
                    newKeyStr += newKey[k];
                }
                if (k == length)
                {
                    break;
                }
                else
                {
                    i = (k) - 1;
                    newKeyStr += str;
                }
            }
            return newKeyStr;
        }
    }

}
