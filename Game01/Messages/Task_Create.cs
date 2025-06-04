using System;
using KeyCommon.Messages;
using Lidgren.Network;

using KeyCommon.DatabaseEntities;
using Game01.GameObjects;

namespace Game01.Messages
{
    public class Task_Create : MessageBase
    {
        public Order Order;
        public long OrderID;

        public Task_Create()
            : base((int)Enumerations.Task_Create) // todo: these enumerations should come from HERE not KeyCommon.  KeyCommon should have a .User_Message  enum where all game01 enums must come after.  This way client and server.exe will no where to send message handling to.
        {
        }


        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            //// we could make the Request GameObject part of KeyCommon.DatabaseEntities
            //// The key being, Game01 and EXE and Scripts can parse these requests and orders then
            //// but their are no derived classes from OrderRequest and Order.  It's up to EXE and Scripts
            //// to parse out the game specific meaning held by OrderRequest and Order.
            //OrderID = buffer.ReadInt64();  
            //Order = new Game01.GameObjects.Order(OrderID);
            //Order.Read(buffer);

            ////OwnerID = buffer.ReadString();

            ////mAssignedDateTime = DateTime.FromBinary (buffer.ReadInt64());
            ////mAssignedByID = buffer.ReadString();
            ////mAssignedStationID = buffer.ReadString();

            ////mPriority = buffer.ReadInt32();
            ////mTaskType = buffer.ReadInt32();
            ////mArgs = buffer.ReadString();
            ////mNotes1 = buffer.ReadString();


            //// which gameobject type are we trying to read?
            //// we need a factory method to create it


        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);

            buffer.Write(OrderID);
            Order.Write(buffer);

            //buffer.Write(OwnerID);

            //buffer.Write(mAssignedDateTime.ToBinary());
            //buffer.Write(mAssignedByID);
            //buffer.Write(mAssignedStationID);

            //buffer.Write(mPriority);
            //buffer.Write(mTaskType);
            //buffer.Write(mArgs);
            //buffer.Write(mNotes1);

        }
        #endregion
    }
}
