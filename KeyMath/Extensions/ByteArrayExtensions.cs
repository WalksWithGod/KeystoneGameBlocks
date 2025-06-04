using System;
using System.IO;
using System.Text; // required for UTF8 encoding
using Ionic.Zlib;

namespace Keystone.Extensions
{
    public static class ByteArrayExtensions
    {
        public static string Compress(this string s)
        {
            byte[] bytesToEncode = Encoding.UTF8.GetBytes(s);
            return bytesToEncode.Compress();
        }

        public static string Compress(this byte[] bytesToEncode)
        {
            const int bufferSize = 1024 * 8;
            using (var output = new MemoryStream())
            using (var input = new MemoryStream (bytesToEncode))
            using (var zinput = new ZlibStream(input, CompressionMode.Compress))
            {
                // copy the compressed input to the output stream
                StreamCopy(zinput, output, bufferSize);
                return Convert.ToBase64String(output.ToArray());
            }

        }

        private static void StreamCopy(Stream source, Stream target, int bufferSize)
        {
            var temp = new byte[bufferSize];
            while (true)
            {
                var read = source.Read(temp, 0, temp.Length);
                if (read <= 0) break;
                target.Write(temp, 0, read);
            }
        }

        public static byte[] Explode(this string s)
        {
            byte[] compressedBytes = Convert.FromBase64String(s);
            return compressedBytes.Explode();
        }

        public static byte[] Explode(this byte[] compressedBytes)
        {
            const int bufferSize = 1024 * 8;
            using (var output = new MemoryStream())
            using (var input = new MemoryStream(compressedBytes))
            using (var zinput = new ZlibStream(input, CompressionMode.Decompress))
            {
                // copy the compressed input to the output stream
                StreamCopy(zinput, output, bufferSize);
                return output.ToArray();
            }

        }
    }
}
