using System;
using KeyCommon.Messages;
using Lidgren.Network;

namespace KeyCommon.Messages
{
	/// <summary>
	/// Description of Terrain_Paint.
	/// </summary>
	public class Terrain_Paint : MessageBase
	{
		private string mTerrainID;
		private Keystone.Types.Vector3d[] mVertices;
		
	
		 // results and cancel arrays are for CommandCompleted and so do not need to be serialized
        public bool[] Cancel; // if null, all elements are assumed NOT canceled


        // change: results are now saved in command.WorkerProduct;
        //public Keystone.Portals.MinimeshMap[] WorkerResults;

        public Terrain_Paint()
            : base ((int)Enumerations.Terrain_Paint)
        { }



        public string TerrainID {get {return mTerrainID;} set { mTerrainID = value;}}
        
        public Keystone.Types.Vector3d[] Vertices {get {return mVertices;} set{ mVertices = value;}}
        
        
        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            mTerrainID = buffer.ReadString();

            // vertices
            uint count = buffer.ReadUInt32();
            mVertices = new Keystone.Types.Vector3d[count];

            for (int i = 0; i < count; i++)
            {
                mVertices[i].x = buffer.ReadFloat();
                mVertices[i].y = buffer.ReadFloat();
                mVertices[i].z = buffer.ReadFloat();
            }

        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(mTerrainID);

            // vertices
            uint count = 0;
            if (mVertices != null)
                count = (uint)mVertices.Length;

            buffer.Write(count);
            for (int i = 0; i < count; i++)
            {
                buffer.Write((float)mVertices[i].x);
                buffer.Write((float)mVertices[i].y);
                buffer.Write((float)mVertices[i].z);
            }

        }
        #endregion
	}
}
