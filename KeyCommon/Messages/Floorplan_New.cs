using System;
using Lidgren.Network;
using KeyCommon.Messages;

namespace KeyCommon.Messages
{
    public class Floorplan_New : MessageBase
    {
        public string FolderName;
        public float CellWidth;
        public float CellDepth;
        public float CellHeight;
        public uint CellCountX;
        public uint CellCountY;
        public uint CellCountZ;

        // change: result is now stored in command.WorkerProduct
        //public Scene.Scene ResultScene; // this is only a result after scene is loaded
        // and can be queried in the CommandCompleted threadpool 
        // callback

        public Floorplan_New()
            : base ((int)Enumerations.NewFloorplan)
        { }

        public Floorplan_New(string folderName, float cellWidth, float cellDepth, float cellHeight, uint cellCountX, uint cellCountY, uint cellCountZ)
            : this()
        {
            FolderName = folderName;
            CellCountX = cellCountX;
            CellCountY = cellCountY;
            CellCountZ = cellCountZ;

            CellWidth = cellWidth;
            CellDepth = cellDepth;
            CellHeight = cellHeight;
        }

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);

            FolderName = buffer.ReadString();
            CellWidth = buffer.ReadFloat();
            CellDepth = buffer.ReadFloat();
            CellHeight = buffer.ReadFloat();
            CellCountX = buffer.ReadUInt32();
            CellCountY = buffer.ReadUInt32();
            CellCountZ = buffer.ReadUInt32();
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);

            buffer.Write(FolderName);
            buffer.Write(CellWidth);
            buffer.Write(CellDepth);
            buffer.Write(CellHeight);
            buffer.Write(CellCountX);
            buffer.Write(CellCountY);
            buffer.Write(CellCountZ);
        }
        #endregion
    }
}
