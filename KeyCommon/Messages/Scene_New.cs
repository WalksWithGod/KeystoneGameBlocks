using System;
using Lidgren.Network;
using KeyCommon.Messages;

namespace KeyCommon.Messages
{
    public class Scene_New : MessageBase 
    {
    	
		[Flags]
        public enum CreationFlags : int
        {
            None = 0,
            MultiZone = 1 << 0,
            DefaultTerrain = 1 << 1,
            Water = 1 << 2,
            DefaultTerrain_Water = 1 << 3,
            SerializeEmptyZones = 1 << 4
            	
        }
        
        
        public string FolderName;    // TODO: what about the scene file itself?  do we want to stream all entities over the wire seperately, or do we send entire generated scene file?
                                   //       a new empty scene is one thing, but some scenes will have generated stellar systems or terrains and water
        
        public float RegionDiameterX;
        public float RegionDiameterY;
        public float RegionDiameterZ;
        
        // change: result is now saved in command.WorkerProduct
        //public Scene.Scene ResultScene; // this is only a result after scene is loaded
                                            // and can be queried in the CommandCompleted threadpool 
                                            // callback

        public Scene_New()
            : base ((int)Enumerations.NewScene)
        { }

        public Scene_New(string folderName, float regionDiameterX, float regionDiameterY, float regionDiameterZ)
            : this()
        {
            FolderName = folderName;
            RegionDiameterX = regionDiameterX;
            RegionDiameterY = regionDiameterY;
            RegionDiameterZ = regionDiameterZ;
        }

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            FolderName = buffer.ReadString();
            RegionDiameterX = buffer.ReadFloat();
            RegionDiameterY = buffer.ReadFloat();
            RegionDiameterZ = buffer.ReadFloat();
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(FolderName);
            buffer.Write(RegionDiameterX);
            buffer.Write(RegionDiameterY);
            buffer.Write(RegionDiameterZ);
        }
        #endregion
    }
}
