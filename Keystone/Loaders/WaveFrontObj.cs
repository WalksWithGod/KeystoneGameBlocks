using System;
using System.Collections.Generic;
using MTV3D65;

namespace Keystone.Loaders
{
    public class WaveFrontObj
    {
        public Dictionary<string, WaveFrontObjMaterial> Materials;
        public Dictionary<uint,WaveFrontObjSmoothingGroup> SmoothingGroups; // a dictionary of face indices
        public List<WaveFrontObjIndexedFace> Faces;
        public List<TV_3DVECTOR> Normals;
        public List<TV_3DVECTOR> Points;
        public List<TV_2DVECTOR> UVs;
        public List<WaveFrontObjGroup> Groups = new List<WaveFrontObjGroup>();
        private WaveFrontObjGroup _currentGroup;
        private WaveFrontObjMaterial _currentMaterial;
        private WaveFrontObjSmoothingGroup _currentSmoothingGroup = null; // if null, no active smoothing group is set
        //private WaveFrontObjMergingGroup _currentMergingGroup = null;
        private string _filename;
        
        public WaveFrontObj (string filename)
        {
            _filename = filename;
        }

        public void SaveResource()
        {
            
        }

        public void Clear()
        {
            Faces = null;
            Normals = null;
            Points = null;
            UVs = null;
            Groups = null;
            _currentGroup = null;
            _currentMaterial = null;
        }

        /// <summary>
        /// This should be called only AFTER the geometry is loaded and we wish to
        /// merge the materials that have identicle values but different names.  
        /// We must re-map every group that uses this material to now use the existing one.
        /// </summary>
        public void MergeMaterials()
        {
            if (Materials == null || Materials.Count == 0) return;

            List<WaveFrontObjMaterial> uniqueStore = new List<WaveFrontObjMaterial>();
            Dictionary<string, WaveFrontObjMaterial> duplicates = new Dictionary<string, WaveFrontObjMaterial>();

            // iterate through our copied array of materials
            foreach (WaveFrontObjMaterial material in Materials.Values)
            {
                // if this material BY VALUE does not exist in our tracker list of unique materials
                // add it.
                WaveFrontObjMaterial existingMaterial;
                if (IsMaterailUnique(uniqueStore, material, out existingMaterial))
                {
                    uniqueStore.Add(material);
                }
                else
                {
                    duplicates.Add(material.Name, existingMaterial);
                }
            }

            // for any material in the original materials that is not in the unique
            // we have to reassign the references to any Group that uses them
            foreach (KeyValuePair<string, WaveFrontObjMaterial> pair in duplicates)
            {
                // every group that uses the duplicate material reassign it to the shared unique
                foreach (WaveFrontObjGroup group in Groups)
                {
                    if (group.Material != null && group.Material.Name == pair.Key)
                        group.Material = pair.Value;
                }
            }

            // re-assign Materials dictionary
            Materials = new Dictionary<string, WaveFrontObjMaterial>();
            for (int i = 0; i < uniqueStore.Count; i++)
            {
                Materials.Add(uniqueStore[i].Name, uniqueStore[i]);
            }

            MergeGroups();
        }

        /// <summary>
        /// Merge groups which share the same material
        /// </summary>
        private void MergeGroups()
        {
            // TODO:
        }

        private bool IsMaterailUnique(List<WaveFrontObjMaterial> input, WaveFrontObjMaterial material, out WaveFrontObjMaterial existingMaterial)
        {
            existingMaterial = null;

            if (input == null || input.Count == 0) return true;
            foreach (WaveFrontObjMaterial mat in input)
            {
                if (mat.Equals(material)) 
                {
                    existingMaterial = mat;
                    return false;
                }
            }           
            return true;
        }

        public WaveFrontObjMaterial CurrentMaterial
        {
            get { return _currentMaterial; }
            set
            { 
                // if the current group's face count > 0 then it means
                // we're changing the material midway in the .obj file without
                // having create a new group first.  This is legal for .obj spec
                // but in tv3d we will need seperate groups because that's how we associate new materials is through groups.
                //
                // it seems most likely that a material change without a group change occurs when we are working on a specific part
                // of a mesh in an .obj scene where it's still part of that submesh so we'll create a group name that reflects that
                if (_currentGroup != null)
                {
                    if (_currentGroup.Faces != null && _currentGroup.Faces.Count > 0)
                    {
                        string newSubGroupName = "";
                        AddGroup(newSubGroupName);
                    }
                }
                _currentMaterial = value;

                if (_currentGroup != null)
                    _currentGroup.Material = _currentMaterial;
            }
        }

        public void AddGroup(string name)
        {
            _currentGroup = new WaveFrontObjGroup(name);
            _currentGroup.Material = _currentMaterial;
            Groups.Add(_currentGroup);
        }
        

        public void EnableMergingGroup (uint index, float epsilon)
        {
            throw new Exception("WaveFrontObj.EnableMergingGroup() - method not implemented");
            //if (MergingGroups == null) MergingGroups = new Dictionary<uint, WaveFrontObjMergingGroup>();
            //WaveFrontObjMergingGroup mg;
            //MergingGroups.TryGetValue(index, out mg);
            //if (mg == null) MergingGroups.Add(index, new WaveFrontObjMergingGroup(epsilon));
            //_currentMergingGroup = mg;
        }

        public void EnableSmoothingGroup (uint index)
        {
            if (SmoothingGroups == null) SmoothingGroups = new Dictionary<uint, WaveFrontObjSmoothingGroup>();
            WaveFrontObjSmoothingGroup sg ;
            SmoothingGroups.TryGetValue(index, out sg);
            if (sg == null) SmoothingGroups.Add(index, new WaveFrontObjSmoothingGroup());
            _currentSmoothingGroup = sg;
        }

        public void DisableCurrentSmoothingGroup()
        {
            _currentSmoothingGroup = null;
        }

        public void DisableCurrentMergingGroup()
        {
            throw new Exception("WaveFrontObj.DisableCurrentMergingGroup() - method not implemented");
            //_currentMergingGroup = null;
        }
        
        private Dictionary<Tuple<uint, uint, uint>, int> mIndexedVertices;
        private List<short> mIndices;
        private int mIndicesCount;
        
        public void AddFace (WaveFrontObjIndexedFace face)
        {
            // in free file sections of a file, faces can just start appearing without any group
            if (_currentGroup == null)
                AddGroup("default");

            System.Diagnostics.Trace.Assert(face.Points.Length >= 3);
            
            face.Triangulate();

            if (Faces == null) Faces = new List<WaveFrontObjIndexedFace>();
            Faces.Add(face);
            face.Index = (uint)Faces.Count - 1;
            _currentGroup.AddFace(face);
            
            if (_currentSmoothingGroup != null) _currentSmoothingGroup.AddFace(face);
            
            
            if (mIndexedVertices == null)
            {
            	mIndexedVertices = new Dictionary<Tuple<uint, uint, uint>, int>();
            	mIndices = new List<short>();
            }
            
            // Add each vertex one at a time
            for (int j = 0; j < face.TriangulatedPoints.Length; j++)
            {
            	uint pIndex = face.TriangulatedPoints[j];
            	uint nIndex = 0;
            	if (face.Normals != null)
            		nIndex = face.Normals[j];
            	uint tIndex = 0;
            	if (face.Textures != null)
            		tIndex = face.Textures[j];
            	
            	Tuple<uint, uint, uint> t = new Tuple<uint, uint, uint>(pIndex, nIndex, tIndex);
            	int vertexIndex;
            	if (mIndexedVertices.TryGetValue (t, out vertexIndex) == false)
            	{
            		vertexIndex = mIndexedVertices.Count;
            		mIndexedVertices.Add (t, vertexIndex);
            	}
            	
            	mIndices.Add ((short)vertexIndex);
            	
            }
        }

        public short[] Indices 
        {
        	get 
        	{
        		short[] results = mIndices.ToArray();
        		return results;
        	}
        }
        
        public Tuple<uint, uint, uint>[] IndexedVertices
        {
        	get 
        	{
        		Tuple<uint, uint, uint>[] tuples = new Tuple<uint, uint, uint>[mIndexedVertices.Count];
        		mIndexedVertices.Keys.CopyTo (tuples, 0);
        		return tuples;
        	}
        }
        

        
        public void AddPoint(float x, float y, float z)
        {
        	TV_3DVECTOR v;
        	v.x = x;
        	v.y = y;
        	v.z = z;
            AddPoint(v);
        }

        public void AddPoint(TV_3DVECTOR coord)
        {
            if (Points == null) Points = new List<TV_3DVECTOR>();
            Points.Add(coord);
        }

        public void AddUV(float u, float v)
        {
        	TV_2DVECTOR vec;
        	vec.x = u;
        	vec.y = v;
            AddUV(vec);
        }

        public void AddUV(TV_2DVECTOR textureCoord)
        {
            if (UVs == null) UVs = new List<TV_2DVECTOR>();
            UVs.Add(textureCoord);
        }

        public void AddNormal(float x, float y, float z)
        {
            TV_3DVECTOR normal;
			normal.x = x;
			normal.y = y;
			normal.z = z;
            normal = CoreClient._CoreClient.Maths.VNormalize(normal);
            AddNormal(normal);
        }

        public void AddNormal(TV_3DVECTOR normal)
        {
            if (Normals == null) Normals = new List<TV_3DVECTOR>();
            Normals.Add(normal);
        }
    }
}
