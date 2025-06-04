using System;
using System.Collections.Generic;
using System.Text;
using MTV3D65;

namespace Keystone.Loaders
{
    public class WaveFrontObjSmoothingGroup
    {
        private TV_3DVECTOR _normal;
        private bool _dirty;
        private List<WaveFrontObjIndexedFace>  _faces;

        public WaveFrontObjSmoothingGroup()
        {
            _faces = new List<WaveFrontObjIndexedFace>();
            _dirty = false;
        }

        public void AddFace(WaveFrontObjIndexedFace face)
        {
            _faces.Add(face);
            face.SmoothingGroup = this;
            _dirty = true;
        }

        public TV_3DVECTOR GetSmoothNormal (uint index)
        {
            // get all faces that share this vertex.  Remember that just because a face is in the same smoothing group
            // doesn't mean it shares any particular vertex
            int count = 0;
            foreach (WaveFrontObjIndexedFace face in _faces)
            {
                if (face.ContainsVertex(index))
                {
                    _normal += face.GetFaceNormal();
                    count++;
                }
            }
            return _normal /= count;
        }

        public TV_3DVECTOR Normal
        {
            get
            {
                if (_dirty)
                {
                    foreach (WaveFrontObjIndexedFace face in _faces)
                    {
                        _normal += face.GetFaceNormal();
                    }
                    _normal /= _faces.Count;
                }
                return _normal;
            }
        }
    }
}
