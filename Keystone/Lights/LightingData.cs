using System;
using System.Collections.Generic;
using Keystone.Utilities;

namespace Keystone.Lights
{
    // TODO: Is this class obsolete?  or what?  
    // used in the Subscriber data
    public class LightingData
    {
        private bool _hashCodeIsDirty = true;
        private int _hashCode;
        private int _lightCount;
        private List<Light> _lights;


        public int Count
        {
            get { return _lightCount; }
        }

        public void Add(Light light)
        {
            if (_lights == null) _lights = new List<Light>();

            // same light cannot be added twice
            if (_lights.Contains(light)) throw new ArgumentException("Light already exists.");
            _lights.Add(light);
            _lightCount++;
            _hashCodeIsDirty = true;
        }

        public void Remove(Light light)
        {
            try
            {
                _lights.Remove(light);
                _lightCount--;
                if (_lights.Count == 0) _lights = null;
            }
            catch
            {
            }
            _hashCodeIsDirty = true;
        }

        public override int GetHashCode()
        {
            if (_hashCodeIsDirty) ComputeHashCode();
            return _hashCode;
        }

        private void ComputeHashCode()
        {
            if (_lights == null || _lights.Count == 0)
            {
                _hashCode = 0;
                return;
            }

            byte[] data = new byte[4*_lights.Count];
            byte[] tmp;

            // sort array to ensure that ashcodes are always the same no matter what order the lights are in
            _lights.Sort(); //TODO: verify this is calling our Lights CompareTo method

            for (int i = 0; i < _lights.Count; i++)
            {
                tmp = BitConverter.GetBytes(_lights[i].TVIndex);
                int index = i*4;
                Array.Copy(tmp, 0, data, index, 4);
            }

            _hashCode = BitConverter.ToInt32(CRC32.Crc32(data), 0);
            _hashCodeIsDirty = false;
        }
    }
}