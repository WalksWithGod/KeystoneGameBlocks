using System;
using System.Collections.Generic;
using KeyCommon.Messages;
using Lidgren.Network;

namespace Game01.Messages
{
    public class PerformRangedAttack : MessageBase
    { 


        public string StationID;
        public string WeaponID;
        public string TargetID;

        public PerformRangedAttack() : base((int)Enums.UserMessage.Game_PerformRangedAttack)
        {
        }

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);

            StationID = buffer.ReadString();
            WeaponID = buffer.ReadString();
            TargetID = buffer.ReadString();

            ////mAssignedDateTime = DateTime.FromBinary (buffer.ReadInt64());
            ////mAssignedByID = buffer.ReadString();
            ////mAssignedStationID = buffer.ReadString();

            ////mPriority = buffer.ReadInt32();
            ////mTaskType = buffer.ReadInt32();
            ////mArgs = buffer.ReadString();
            ////mNotes1 = buffer.ReadString();



        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);

            //            buffer.Write(OrderID);
            //           Order.Write(buffer);

            buffer.Write(StationID);
            buffer.Write(WeaponID);
            buffer.Write(TargetID);

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
