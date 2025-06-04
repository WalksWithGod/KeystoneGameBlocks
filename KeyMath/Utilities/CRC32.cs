using System;
using System.Text;

namespace Keystone.Utilities
{
    /// <summary>
    /// Provides a C# implementaiton of the cyclic-redundancy check algorithm in C#.
    /// </summary>
    /// <remarks>
    /// This type is entirely CLS-compliant unless otherwise noted.  It will operate on types 
    /// of System.UInt32, System.Int64, and System.Int32.  All data is represented internally 
    /// by a UInt32 variable.
    /// </remarks>
    [CLSCompliant(true)]
    public sealed class CRC32
    {
        #region constants

        /// <summary>
        /// The size of the CRC table (256).
        /// </summary>
        public const int TABLE_SIZE = 256;

        /// <summary>
        /// The standard CRC32 algorithm's polynomial.
        /// </summary>
        [CLSCompliant(false)] public const uint STANDARD_POLYNOMIAL = 0xedb88320;

        #endregion

        #region constructors

        /// <summary>
        /// Prevent this type from being instantiated.
        /// </summary>
        private CRC32()
        {
        }

        /// <summary>
        /// Initializes static variables.
        /// </summary>
        static CRC32()
        {
            table = new uint[TABLE_SIZE];
            polynomial = STANDARD_POLYNOMIAL;
        }

        #endregion

        #region static properties

        /// <summary>
        /// Gets whether or not the CRC table has been initialized.
        /// </summary>
        public static bool Initialized
        {
            get { return initialized; }
        }

        /// <summary>
        /// Gets the currently-used polynomial.
        /// </summary>
        /// <remarks>
        /// This property is not CLS-compliant.
        /// </remarks>
        [CLSCompliant(false)]
        public static uint Polynomial
        {
            get { return polynomial; }
        }

        #endregion

        #region members

        /// <summary>
        /// Stores the CRC table.
        /// </summary>
        private static uint[] table;

        /// <summary>
        /// Provides internal storage of the CRC polynomial to be used during checking.
        /// </summary>
        private static uint polynomial;

        /// <summary>
        /// Stores whether or not the initialization routine has been called.
        /// </summary>
        private static bool initialized;

        #endregion

        #region Initialization functions (overloaded)

        /// <summary>
        /// Initializes the CRC table with the specified CRC polynomial.
        /// </summary>
        /// <remarks>
        /// One of the Init variants must be called before you are able to call the Crc32 methods.
        /// This is to initially populate the tables with a provided polynomial.
        /// </remarks>
        /// <param name="Polynomial">The polynomial to use for initialization of the
        /// table.</param>
        [CLSCompliant(false)]
        public static void Init(uint Polynomial)
        {
            polynomial = Polynomial;
            // counters
            uint i = 0, j = 0;
            // table creation variables
            uint dwCRC = 0;

            for (i = 0; i < TABLE_SIZE; i++)
            {
                dwCRC = i;
                for (j = 8; j > 0; j--)
                {
                    if ((dwCRC & 1) != 0)
                        dwCRC = (dwCRC >> 1) ^ polynomial;
                    else
                        dwCRC >>= 1;
                }
                table[i] = dwCRC;
            }

            initialized = true;
        }

        /// <summary>
        /// Initializes the CRC table with the specified CRC polynomial.
        /// </summary>
        /// <remarks>
        /// One of the Init variants must be called before you are able to call the Crc32 methods.
        /// This is to initially populate the tables with a provided polynomial.
        /// </remarks>
        /// <param name="Polynomial">The polynomial to use for initialization of the
        /// table.</param>
        public static void Init(int Polynomial)
        {
            // convert the signed value into unsigned value using BitConverter class,
            // because we don't want to raise an exception.
            byte[] intBytes = BitConverter.GetBytes(Polynomial);
            uint poly = BitConverter.ToUInt32(intBytes, 0);
            // now that we've obtained the unsigned representation, initialize the class
            // with that.
            Init(poly);
        }

        /// <summary>
        /// Initializes the CRC table with the specified CRC polynomial.
        /// </summary>
        /// <remarks>
        /// One of the Init variants must be called before you are able to call the Crc32 methods.
        /// This is to initially populate the tables with a provided polynomial.
        /// </remarks>
        /// <param name="Polynomial">The polynomial to use for initialization of the
        /// table.</param>
        public static void Init(long Polynomial)
        {
            const uint Mask = 0xffffffff;
            uint poly = (uint) (Polynomial & Mask);
            Init(poly);
        }

        /// <summary>
        /// Initializes the CRC table with the standard polynomial.
        /// </summary>
        /// <remarks>
        /// One of the Init variants must be called before you are able to call the Crc32 methods.
        /// This is to initially populate the tables with a provided polynomial.
        /// </remarks>
        public static void Init()
        {
            Init(STANDARD_POLYNOMIAL);
        }

        #endregion

        #region Crc32 functions

        /// <summary>
        /// Calculates the cyclic redundancy check on an array of bytes.
        /// </summary>
        /// <param name="data">The data to check</param>
        /// <returns>A 4-byte array in little-endian format containing the CRC of the data.</returns>
        /// <remarks>
        /// Use the <see cref="System.BitConverter">System.BitConverter</see> class to convert
        /// this array of bytes back to usable format.
        /// </remarks>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if the CRC table is not initialized (by calling one of the Init() overloads
        /// before calling the Crc32 overloads.
        /// </exception>
        public static byte[] Crc32(byte[] data)
        {
            if (!initialized)
                throw new InvalidOperationException(
                    "You must initialize the CRC table before attempting to calculate the check on data.");

            // the array to be returned
            byte[] crc32_result;
            // storage for the result, and a length identifier (so we don't need to make constant
            // calls to the property function)
            uint _result = 0, len = (uint) unchecked(data.Length);
            // counter
            uint i = 0;

            // used to ensure that we only look into 0-255 of the lookup table.
            const uint tableIndexMask = 0xff;

            uint dwCRC32 = 0xffffffff;
            for (i = 0; i < len; i++)
            {
                dwCRC32 = (dwCRC32 >> 8) ^ table[(uint) data[i] ^ (dwCRC32 & tableIndexMask)];
            }

            _result = dwCRC32 ^ 0xffffffff;

            crc32_result = BitConverter.GetBytes(_result);
            return crc32_result;
        }

        /// <summary>
        /// Calculates the cyclic redundancy check on an array of bytes.
        /// </summary>
        /// <remarks>
        /// When possible, one should use the data that accepts simply a byte array as a parameter,
        /// as this method simply converts the pointer of bytes into a byte array and calls that
        /// function as is.
        /// </remarks>
        /// <param name="pData">The pointer to the first byte of the array.</param>
        /// <param name="dwLength">The length of the array.</param>
        /// <returns>A 4-byte array in little-endian format containing the CRC of the data.</returns>
        /// <remarks>
        /// Use the <see cref="System.BitConverter">System.BitConverter</see> class to convert
        /// this array of bytes back to usable format.
        /// </remarks>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if the CRC table is not initialized (by calling one of the Init() overloads
        /// before calling the Crc32 overloads.
        /// </exception>
        [CLSCompliant(false)]
        public static unsafe byte[] Crc32(byte* pData, uint dwLength)
        {
            byte* ptr = pData;
            byte[] data = new byte[dwLength];
            for (uint i = 0; i < dwLength; i++)
            {
                data[i] = *ptr;
                ptr++;
            }

            return Crc32(data);
        }

        /// <summary>
        /// Calculates the cyclic redundancy check on a string of data, assuming ASCII encoding.
        /// </summary>
        /// <param name="data">The string to check</param>
        /// <returns>A 4-byte array in little-endian format containing the CRC of the data.</returns>
        /// <remarks>
        /// Use the <see cref="System.BitConverter">System.BitConverter</see> class to convert
        /// this array of bytes back to usable format.
        /// </remarks>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if the CRC table is not initialized (by calling one of the Init() overloads
        /// before calling the Crc32 overloads.
        /// </exception>
        public static byte[] Crc32(string data)
        {
            return Crc32(data, Encoding.ASCII);
        }

        /// <summary>
        /// Calculates the cyclic redundancy check on a string of data with the specified 
        /// encoding scheme.
        /// </summary>
        /// <param name="data">The string to check.</param>
        /// <param name="encoding">The encoding to use.</param>
        /// <returns>A 4-byte array in little-endian format containing the CRC of the data.</returns>
        /// <remarks>
        /// Use the <see cref="System.BitConverter">System.BitConverter</see> class to convert
        /// this array of bytes back to usable format.
        /// </remarks>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if the CRC table is not initialized (by calling one of the Init() overloads
        /// before calling the Crc32 overloads.
        /// </exception>
        public static byte[] Crc32(string data, Encoding encoding)
        {
            byte[] encData = encoding.GetBytes(data);
            return Crc32(encData);
        }

        #endregion
    }


    ///// <summary>
    ///// Calculates a 32bit Cyclic Redundancy Checksum (CRC) using the
    ///// same polynomial used by Zip.
    ///// Status: freeware
    ///// Author: Steve McMahon
    ///// http://www.vbaccelerator.com/home/NET/Code/Libraries/CRC32/Crc32_zip_CRC32_CRC32_cs.asp
    ///// </summary>
    //public class CRC32
    //{
    //    private UInt32[] crc32Table;
    //    private const int BUFFER_SIZE = 1024;

    //    /// <summary>
    //    /// Returns the CRC32 for the specified stream.
    //    /// </summary>
    //    /// <param name="stream">The stream to calculate the CRC32 for</param>
    //    /// <returns>An unsigned integer containing the CRC32 calculation</returns>
    //    public UInt32 GetCrc32(Stream stream)
    //    {
    //        unchecked //dont check for overflow
    //        {
    //            UInt32 crc32Result;
    //            crc32Result = 0xFFFFFFFF;
    //            byte[] buffer = new byte[BUFFER_SIZE];
    //            int readSize = BUFFER_SIZE;

    //            int count = stream.Read(buffer, 0, readSize);
    //            while (count > 0)
    //            {
    //                for (int i = 0; i < count; i++)
    //                {
    //                    crc32Result = ((crc32Result) >> 8) ^ crc32Table[(buffer[i]) ^
    //                     ((crc32Result) & 0x000000FF)];
    //                }
    //                count = stream.Read(buffer, 0, readSize);
    //            }

    //            return ~crc32Result;
    //        }
    //    }


    //    /// <summary>
    //    /// Construct an instance of the CRC32 class, pre-initialising the table
    //    /// for speed of lookup.
    //    /// </summary>
    //    public CRC32()
    //    {
    //        unchecked //dont check for overflow
    //        {
    //            // This is the official polynomial used by CRC32 in PKZip.
    //            // Often the polynomial is shown reversed as 0x04C11DB7.
    //            UInt32 dwPolynomial = 0xEDB88320;
    //            UInt32 i, j;

    //            crc32Table = new UInt32[256];

    //            UInt32 dwCrc;
    //            for (i = 0; i < 256; i++)
    //            {
    //                dwCrc = i;
    //                for (j = 8; j > 0; j--)
    //                {
    //                    if ((dwCrc & 1) == 1)
    //                    {
    //                        dwCrc = (dwCrc >> 1) ^ dwPolynomial;
    //                    }
    //                    else
    //                    {
    //                        dwCrc >>= 1;
    //                    }
    //                }
    //                crc32Table[i] = dwCrc;
    //            }
    //        }
    //    }
    //}
}