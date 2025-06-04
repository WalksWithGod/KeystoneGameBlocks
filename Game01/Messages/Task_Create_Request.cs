using System;
using KeyCommon.Messages;
using Lidgren.Network;
//using KeyCommon.Helpers; // Extension Methods for PropertySpec.Read/Write
using System.Collections.Generic;

using KeyCommon.DatabaseEntities;

namespace Game01.Messages
{

	/// <summary>
	/// Request a task be created for the specified command to be assigned to the specified
	/// department or crew member.
	/// </summary>
    public class Task_Create_Request : MessageBase
    {
        public Messages.OrderRequest Request;
        //public string OwnerID;  // the captains user ID or the VehicleID?
        //public DateTime mAssignedDateTime;
        //public string mAssignedByID;       // can be an NPC?

        //public string mAssignedStationID;
        //public string mStationOperatorID;

        //public int mPriority;
        //public int mTaskType;
        //public string mArgs;               // comma seperated values
        //public string mNotes1;



        public Task_Create_Request()
            : base((int)Enumerations.Task_Create_Request)
        {
        }

 
        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            // we could make the Request GameObject part of KeyCommon.DatabaseEntities
            // The key being, Game01 and EXE and Scripts can parse these requests and orders then
            // but their are no derived classes from OrderRequest and Order.  It's up to EXE and Scripts
            // to parse out the game specific meaning held by OrderRequest and Order.  
            Request = new Game01.Messages.OrderRequest();
            Request.Read(buffer);

            //OwnerID = buffer.ReadString();

            //mAssignedDateTime = DateTime.FromBinary (buffer.ReadInt64());
            //mAssignedByID = buffer.ReadString();
            //mAssignedStationID = buffer.ReadString();

            //mPriority = buffer.ReadInt32();
            //mTaskType = buffer.ReadInt32();
            //mArgs = buffer.ReadString();
            //mNotes1 = buffer.ReadString();


            // which gameobject type are we trying to read?
            // we need a factory method to create it


        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);

            Request.Write(buffer);

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
