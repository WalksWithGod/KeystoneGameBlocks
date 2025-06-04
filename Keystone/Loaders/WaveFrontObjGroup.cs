using System.Collections.Generic;

namespace Keystone.Loaders
{
    public class WaveFrontObjGroup
    {
        public string _name;        
        public List<WaveFrontObjIndexedFace> Faces;
        public WaveFrontObjMaterial Material;

        public WaveFrontObjGroup(string name)
        {
            _name = name;
        }

        public void AddFace(WaveFrontObjIndexedFace face)
        {
            if (Faces == null) Faces = new List<WaveFrontObjIndexedFace>();
            Faces.Add(face);
        }        
    }
}
