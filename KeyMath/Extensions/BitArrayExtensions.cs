using System;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;

namespace Keystone.Extensions
{
    public static class BitArrayExtensions
    {
        static FieldInfo _internalArrayNativeField = GetInternalArrayNativeField();

        static FieldInfo GetInternalArrayNativeField()
        {
            return typeof(BitArray).GetField("m_array", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        static int[] GetInternalArray(BitArray array)
        {
            return (int[])_internalArrayNativeField.GetValue(array);
        }

        static void SetInternalArray(BitArray array, int[] values)
        {
            _internalArrayNativeField.SetValue(array, values);
        }

        public static IEnumerable<int> GetInternalValues(this BitArray array)
        {
            return GetInternalArray(array);
        }

        public static void SetInternalValues (this BitArray array, int[] values)
        {
            SetInternalArray(array, values);
        }

        // this should reflect the size on disk which must include
        // not how many int32's but also the actual number of elements
        // since if all 32bits of final dword are not used, it's fewer elements
        public static uint SizeOnDisk(this BitArray array)
        {
            if (array.Length == 0) throw new ArgumentNullException();

            int[] data = GetInternalArray(array);

            uint BYTES_PER_INT32 = 4;
            return (uint)data.Length * BYTES_PER_INT32 + 4; // 4 bytes to store numElements
        }

        public static BitArray Read(System.IO.BinaryReader reader)
        {
            int numElements = reader.ReadInt32();
            int dwordCount = reader.ReadInt32();
            int[] data = new int[dwordCount];

            for (int i = 0; i < data.Length; i++)
                data[i] = reader.ReadInt32();


            BitArray array = new BitArray(numElements);
            SetInternalArray(array, data);

            return array;
        }

        public static void Write (this BitArray array, System.IO.BinaryWriter writer)
        {
            if (array.Length == 0) throw new ArgumentNullException ();

            int[] data = GetInternalArray(array);

            // element count
            writer.Write(array.Count);
            // dword count
            writer.Write(BitConverter.GetBytes(data.Length));

            // data
            for (int i = 0; i < data.Length; i++)
                writer.Write(BitConverter.GetBytes (data[i]));
           
        }
    }
}
