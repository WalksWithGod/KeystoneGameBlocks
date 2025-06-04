using System;
using System.Collections.Generic;

namespace Keystone.Modeler
{
    public class MeshGroup
    {
        public string _name;
        public List<IndexedFace> Faces;
        public Microsoft.DirectX.Direct3D.Material  Material;

        public MeshGroup(string name)
        {
            _name = name;
        }

        public void AddFace(IndexedFace face)
        {
            if (Faces == null) Faces = new List<IndexedFace>();
            Faces.Add(face);
        }
    }
}
