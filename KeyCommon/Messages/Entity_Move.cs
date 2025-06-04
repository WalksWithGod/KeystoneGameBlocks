using System;
using KeyCommon.Messages;
using Lidgren.Network;
using KeyCommon.Helpers;

namespace KeyCommon.Messages
{
    /// <summary>
    /// Move a node within a region.  Moving does not require a new nodeID 
    /// to be generated.  The same nodeID is kept.  For Interiors (since
    /// they use spatial scenenodes) we do use Add/RemoveChild with the updated position
    /// to achieve the Move behavior but without the Node falling out of scope 
    /// when the initial RemoveChild is called.
    /// </summary>
    public class Entity_Move : MessageBase
    {
        public string mParentID;
        public string[] mTargetIDs;
        public Keystone.Types.Vector3d[] Positions;

        public Entity_Move(string parentID, string[] targetIDs, Keystone.Types.Vector3d[] newPositions)
            : base((int)Enumerations.Entity_Move)
        {
            if (targetIDs == null || targetIDs.Length == 0 || targetIDs.Length != newPositions.Length) throw new ArgumentOutOfRangeException();
            mTargetIDs = targetIDs;
            mParentID = parentID;
            Positions = newPositions;
        }

        public Entity_Move()
            : base((int)Enumerations.Entity_Move)
        { }

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);

            mParentID = buffer.ReadString();
            int targetCount = buffer.ReadInt32();
            if (targetCount > 0)
            {
                mTargetIDs = new string[targetCount];
                for (int i = 0; i < targetCount; i++)
                    mTargetIDs[i] = buffer.ReadString();

                Positions = new Keystone.Types.Vector3d[targetCount];
                for (int i = 0; i < targetCount; i++)
                {
                    Positions[i].x = buffer.ReadDouble();
                    Positions[i].y = buffer.ReadDouble();
                    Positions[i].z = buffer.ReadDouble();
                }
            }
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);

            buffer.Write(mParentID);

            int count = 0;
            if (mTargetIDs.Length > 0)
                count = mTargetIDs.Length;

            System.Diagnostics.Debug.Assert(mTargetIDs.Length == Positions.Length);
            buffer.Write(count);

            for (int i = 0; i < count; i++)
                buffer.Write(mTargetIDs[i]);

            for (int i = 0; i < count; i++)
            {
                buffer.Write(Positions[i].x);
                buffer.Write(Positions[i].y);
                buffer.Write(Positions[i].z);
            }
            
        }
        #endregion
    }
}

