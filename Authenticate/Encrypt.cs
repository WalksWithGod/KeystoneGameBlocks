///////////////////////////////////////////////////////////////////////////////
// SAMPLE: Symmetric key encryption and decryption using Rijndael algorithm.
// 
// To run this sample, create a new Visual C# project using the Console
// Application template and replace the contents of the Class1.cs file with
// the code below.
//
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
// WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// 
// Copyright (C) 2002 Obviex(TM). All rights reserved.
// 
using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace Authentication
{
    /// <summary>
    /// This class uses a symmetric key algorithm (Rijndael/AES) to encrypt and 
    /// decrypt data. As long as encryption and decryption routines use the same
    /// parameters to generate the keys, the keys are guaranteed to be the same.
    /// The class uses static functions with duplicate code to make it easier to
    /// demonstrate encryption and decryption logic. In a real-life application, 
    /// this may not be the most efficient way of handling encryption, so - as
    /// soon as you feel comfortable with it - you may want to redesign this class.
    /// </summary>
    public class RijndaelSimple
    {
        /// <param name="passPhrase">
        /// Passphrase from which a pseudo-random password will be derived. The
        /// derived password will be used to generate the encryption key.
        /// Passphrase can be any string. In this example we assume that this
        /// passphrase is an ASCII string.
        /// </param>
        /// <param name="saltValue">
        /// Salt value used along with passphrase to generate password. Salt can
        /// be any string. In this example we assume that salt is an ASCII string.
        /// </param>
        /// <param name="hashAlgorithm">
        /// Hash algorithm used to generate password. Allowed values are: "MD5" and
        /// "SHA1". SHA1 hashes are a bit slower, but more secure than MD5 hashes.
        /// </param>
        /// <param name="passwordIterations">
        /// Number of iterations used to generate password. One or two iterations
        /// should be enough.
        /// </param>
        /// /// <param name="keySize">
        /// Size of encryption key in bits. Allowed values are: 128, 192, and 256. 
        /// Longer keys are more secure than shorter keys.
        /// </param>
        public static byte[] GetKeyFromPassphrase(string passPhrase, byte[] saltValueBytes, string hashAlgorithm, int passwordIterations, int keySizeInBits)
        {
            // First, we must create a password, from which the key will be derived.
            // This password will be generated from the specified passphrase and 
            // salt value. The password will be created using the specified hash 
            // algorithm. Password creation can be done in several iterations.
            PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, saltValueBytes,hashAlgorithm, passwordIterations);

            // Use the password to generate pseudo-random bytes for the encryption
            // key. Specify the size of the key in bytes (instead of bits).
            byte[] keyBytes = password.GetBytes(keySizeInBits / 8);
            return keyBytes;
        }

        /// <summary>
        /// A password key from a pass phrase.  Since we use our pass keys with an encryption cipher along with an IV
        /// there is no need for to supply a salt to this function.
        /// </summary>
        /// <param name="passPhrase"></param>
        /// <returns></returns>
        public static byte[] GetKeyFromPassphrase(string passPhrase, int keySizeInBits)
        {
            PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null);
            byte[] keyBytes = password.GetBytes(keySizeInBits / 8);
            return keyBytes;
        }
        
        //public static byte[] GenerateIV(string initVector)
        //{
        //    if (string.IsNullOrEmpty(initVector)) throw new ArgumentNullException();
        //    if (initVector.Length != 16) throw new ArgumentOutOfRangeException();
        //    // assume that IV only contains exactly 16 character ASCII codes.
        //    return Encoding.ASCII.GetBytes(initVector);           
        //}

        public static byte[] GenerateIV(string salt)
        {
            return GetKeyFromPassphrase(salt, 16 * 8);
        }

        public static byte[] GenerateIV()
        {
            return PasswordGenerator.GenerateRandomKey(16);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="plainTextBytes">byte array converted from UTF8 encoded string</param>
        /// <param name="keyBytes"></param>
        /// <param name="initVectorBytes"></param>
        /// <returns></returns>
        public static byte[] Encrypt(byte[] plainTextBytes, byte[] keyBytes, byte[] initVectorBytes)
        {

            RijndaelManaged symmetricKey = new RijndaelManaged();

            // It is reasonable to set encryption mode to Cipher Block Chaining
            // (CBC). Use default options for other symmetric key parameters.
            symmetricKey.Mode = CipherMode.CBC;

            // Generate encryptor from the existing key bytes and initialization 
            // vector. Key size will be defined based on the number of the key 
            // bytes.
            ICryptoTransform encryptor = symmetricKey.CreateEncryptor(
                                                             keyBytes,
                                                             initVectorBytes);

            // Define memory stream which will be used to hold encrypted data.
            MemoryStream memoryStream = new MemoryStream();

            // Define cryptographic stream (always use Write mode for encryption).
            CryptoStream cryptoStream = new CryptoStream(memoryStream,
                                                         encryptor,
                                                         CryptoStreamMode.Write);

            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.FlushFinalBlock();
            byte[] cipherTextBytes = memoryStream.ToArray();

            memoryStream.Close();
            cryptoStream.Close();

            return cipherTextBytes;

            //// Convert encrypted data into a base64-encoded string.
            //string cipherText = Convert.ToBase64String(cipherTextBytes);

            //// Return encrypted string.
            //return cipherText;
        }

        /// <summary>
        /// Encrypts specified plaintext using Rijndael symmetric key algorithm
        /// and returns a base64-encoded result.
        /// </summary>
        /// <param name="plainText">
        /// Plaintext value to be encrypted.
        /// </param>
        /// <param name="keyBytes">
        /// </param>
        /// <param name="initVector">
        /// Initialization vector (or IV). This value is required to encrypt the
        /// first block of plaintext data. For RijndaelManaged class IV must be 
        /// exactly 16 ASCII characters long.
        /// </param>
        /// <returns>
        /// Encrypted byte array.
        /// </returns>
        public static byte[] Encrypt(string plainText, byte[] keyBytes, byte[] initVectorBytes)
        {
            if (string.IsNullOrEmpty(plainText)) throw new ArgumentNullException();
            return Encrypt(Encoding.UTF8.GetBytes(plainText), keyBytes, initVectorBytes);
        }


        /// <summary>
        /// Decrypts specified ciphertext using Rijndael symmetric key algorithm.
        /// </summary>
        /// <param name="cipherText">
        /// Base64-formatted ciphertext value.
        /// </param>
        /// <param name="keyBytes">
        /// </param>
        /// <param name="initVector">
        /// Initialization vector (or IV). This value is required to encrypt the
        /// first block of plaintext data. For RijndaelManaged class IV must be
        /// exactly 16 ASCII characters long.
        /// </param>
        /// <returns>
        /// Decrypted string value.
        /// </returns>
        /// <remarks>
        /// Most of the logic in this function is similar to the Encrypt
        /// logic. In order for decryption to work, all parameters of this function
        /// - except cipherText value - must match the corresponding parameters of
        /// the Encrypt function which was called to generate the
        /// ciphertext.
        /// </remarks>
        public static byte[] Decrypt(byte[] cipherTextBytes, byte[] keyBytes, byte[] initVectorBytes)
        {
            // Convert our ciphertext into a byte array.
            // byte[] cipherTextBytes = Convert.FromBase64String(cipherText);

            RijndaelManaged symmetricKey = new RijndaelManaged();

            // It is reasonable to set encryption mode to Cipher Block Chaining
            // (CBC). Use default options for other symmetric key parameters.
            symmetricKey.Mode = CipherMode.CBC;

            // Generate decryptor from the existing key bytes and initialization 
            // vector. Key size will be defined based on the number of the key 
            // bytes.
            ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);

            MemoryStream memoryStream = new MemoryStream(cipherTextBytes);

            // always use Read mode for encryption.
            CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);

            // Since at this point we don't know what the size of decrypted data
            // will be, allocate the buffer long enough to hold ciphertext;
            // plaintext is never longer than ciphertext.
            byte[] plainTextBytes = new byte[cipherTextBytes.Length];

            // this line will throw an exception if the password is incorrect.  
            int decryptedByteCount;
            try
            {
                decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
            }
            catch (Exception ex)
            {
                // typically means the password was incorrect
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return null;
            }
            finally
            {
                memoryStream.Close();
                cryptoStream.Close();
            }
            
            byte[] finalBytes = new byte[decryptedByteCount];
            Array.Copy(plainTextBytes, finalBytes, decryptedByteCount);
            return finalBytes;
        }
    }
}