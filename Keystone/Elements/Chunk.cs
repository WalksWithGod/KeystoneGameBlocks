using System;
using Keystone.Entities;
using Keystone.IO;
using Keystone.Traversers;
using Keystone.Types;
using MTV3D65;

namespace Keystone.Elements
{
    // the only purpose of chunks is to allow us to get some per chunk info such as bounding bolumes and such
    // TODO: Why doesnt this just inherit from BoundGroup which inherits from Group.  Same as Chunk.cs?
    public class Chunk : BoundNode, IPageableTVNode
    {
        private readonly object mSyncRoot;
        private Terrain _terrain;
        private TVLandscape _landscape;

        private int _xOffset;
        private int _zOffset;
        private Vector3d _center;
        private int VerticesPerChunk;
        protected int _tvfactoryIndex = -1;

        internal Chunk(string name, Terrain terrain, int xOffset, int zOffset) : base(name)
        {
            if (terrain == null) throw new ArgumentNullException();
            _landscape = CoreClient._CoreClient.Globals.GetLandscapeFromID(terrain.TVIndex);
            _terrain = terrain;
            _xOffset = xOffset;
            _zOffset = zOffset;

            VerticesPerChunk = 256/(int) terrain.Precision;
            _center = GetChunkCenter();
            _tvfactoryIndex = _landscape.GetChunkID(xOffset, zOffset);
            SetChangeFlags(Enums.ChangeStates.Translated | Enums.ChangeStates.BoundingBoxDirty, Enums.ChangeSource.Self);
        }

        #region IPageableTVNode Members
        public object SyncRoot { get { return mSyncRoot; } }

        public int TVIndex
        {
            get { return _tvfactoryIndex; }
        }

        public bool TVResourceIsLoaded
        {
            get { return _tvfactoryIndex > -1; }
        }

        public PageableNodeStatus PageStatus { get { return _terrain.PageStatus; } set { } } // no set reqt

        public void UnloadTVResource()
        {
        	//DisposeManagedResources();
        }
                
        public void LoadTVResource()
        {
            // there's really nothing to do here because a chunk isnt saved to disk and is only created at runtime 
            // so here there's just nothing to do
            // SetChangeFlags(Keystone.Enums.ChangeStates.GeometryAdded);
        }

        public void SaveTVResource(string filepath)
        {
            /* nothing to save here with a chunk*/
        }

        public string ResourcePath // nothing to do here for chunks 
        {
            get { return ""; }
            set { return; }
        }

        #endregion

        internal override ChildSetter GetChildSetter()
        {
            throw new NotImplementedException();
        }

        public Terrain Terrain
        {
            get { return _terrain; }
        }

        private Vector3d GetChunkCenter()
        {
        	throw new NotImplementedException ();
//        	
//            return
//                new Vector3d(
//                     _terrain.Translation.x + _xOffset*_terrain.ChunkWorldWidth + _terrain.ChunkWorldWidth / 2d, 0,
//                     _terrain.Translation.z + _zOffset*_terrain.ChunkWorldHeight + _terrain.ChunkWorldHeight / 2d);
        }

        public Vector3d Center
        {
            get { return _center; }
        }

        public bool IsVisible
        {
            get { return _landscape.IsChunkVisible(_tvfactoryIndex); }
        }

        public void Enable(bool value)
        {
            _landscape.SetTerrainChunkEnable(_xOffset, _zOffset, value);
        }

        //internal override void AttachTo(CONST_TV_NODETYPE type, int objIndex, int subIndex, bool keepMatrix, bool removeScale)
        //{
        //    throw new Exception("AttachTo not allowed for Terrain or Chunks.");
        //}


        public override object Traverse(ITraverser target, object data)
        {
            return target.Apply(this, data);
        }

        //#region Geometry Members
        //public override void Update()
        //{
        //    throw new NotImplementedException();
        //}

        //public override void Render()
        //{
        //    throw new NotImplementedException();
        //}
        //#endregion

        #region IBoundVolume Members

        protected override void UpdateBoundVolume()
        {
        	throw new NotImplementedException ();
//            float[] HeightArray;
//
//            /// itterate through all the chunks, get the height arrays, and find out the max
//            _box = BoundingBox.Initialized();
//            Vector3d min = new Vector3d();
//            Vector3d max = new Vector3d();
//            min.x = _terrain.ChunkWorldWidth*_xOffset + (double) _terrain.Translation.x;
//            min.z = _terrain.ChunkWorldHeight*_zOffset + (double) _terrain.Translation.z;
//            max.x = min.x + _terrain.ChunkWorldWidth;
//            max.z = min.z + _terrain.ChunkWorldHeight;
//
//            min.y = Single.MaxValue;
//            max.y = Single.MinValue;
//
//            for (int Row = 0; Row < VerticesPerChunk; Row++)
//            {
//                HeightArray = _terrain.GetHeightArray(_xOffset*VerticesPerChunk + Row, _zOffset*VerticesPerChunk, 1,
//                                                      VerticesPerChunk);
//                Array.Sort(HeightArray);
//
//                min.y = Math.Min(min.y, HeightArray[0]);
//                max.y = Math.Max(max.y, HeightArray[VerticesPerChunk - 1]);
//            }
//
//            min.y *= (double) _terrain.Scale.y;
//            max.y *= (double) _terrain.Scale.y;
//
//            _box.Min = min;
//            _box.Max = max;
//            DisableChangeFlags (Keystone.Enums.ChangeStates.BoundingBoxDirty);
        }

        #endregion
    }
}