using KeyCommon.DatabaseEntities;
using Lidgren.Network;
using System;


namespace Game01.Messages
{
    public class OrderRequest : KeyCommon.Messages.MessageBase
    {
        public DateTime AssignedDateTime;
        public string AssignedByID;       // can be an NPC?
        public string AssignedStationID;
        //public string StationOperatorID;

        public int Priority;
        public int Task;
        public string Args;               // 256 max chars, comma seperated values
        public string Notes1;

        // TODO: is this Enumeration value correct?  Is an "Order" same as a "Task"?
        public OrderRequest() : base((int)KeyCommon.Messages.Enumerations.Task_Create_Request)
        {
            AssignedDateTime = DateTime.Now;
            long bin = AssignedDateTime.ToBinary();
            DateTime tmp = DateTime.FromBinary(bin);
        }

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);

            AssignedDateTime = DateTime.FromBinary(buffer.ReadInt64());
            AssignedByID = buffer.ReadString();
            AssignedStationID = buffer.ReadString();
            Priority = buffer.ReadInt32();
            Task = buffer.ReadInt32();
            Args = buffer.ReadString();
            Notes1 = buffer.ReadString();
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);

            buffer.Write(AssignedDateTime.ToBinary());
            buffer.Write(AssignedByID);
            buffer.Write(AssignedStationID);
            buffer.Write(Priority);
            buffer.Write(Task);
            buffer.Write(Args);
            buffer.Write(Notes1);
        }
        #endregion

        // TODO: do i need these methods?  
        #region GameObject methods

        //public override PropertySpec[] GetProperties(bool specOnly)
        //{
        //    Settings.PropertySpec[] tmp = base.GetProperties(specOnly);
        //    Settings.PropertySpec[] properties = new Settings.PropertySpec[7 + tmp.Length];
        //    tmp.CopyTo(properties, 7);

        //    properties[0] = new Settings.PropertySpec("assignedby", typeof(string).Name);
        //    properties[1] = new Settings.PropertySpec("assignedstation", typeof(string).Name);
        //    properties[2] = new Settings.PropertySpec("assigneddatetime", typeof(long).Name);
        //    properties[3] = new Settings.PropertySpec("task", typeof(int).Name);
        //    properties[4] = new Settings.PropertySpec("priority", typeof(int).Name);
        //    properties[5] = new Settings.PropertySpec("args", typeof(string).Name);
        //    properties[6] = new Settings.PropertySpec("notes", typeof(string).Name);

        //    if (!specOnly)
        //    {
        //        properties[0].DefaultValue = mAssignedByID;
        //        properties[1].DefaultValue = mAssignedStationID;
        //        properties[2].DefaultValue = mAssignedDateTime.ToBinary();
        //        properties[3].DefaultValue = (int)mTask;
        //        properties[4].DefaultValue = mPriority;
        //        properties[5].DefaultValue = mArgs;
        //        properties[6].DefaultValue = mNotes1;
        //    }

        //    return properties;
        //}

        //public override void SetProperties(PropertySpec[] properties)
        //{
        //    if (properties == null) return;
        //    base.SetProperties(properties);

        //    for (int i = 0; i < properties.Length; i++)
        //    {
        //        if (properties[i].DefaultValue == null) continue;
        //        // use of a switch allows us to pass in all or a few of the propspecs depending
        //        // on whether we're loading from xml or changing a single property via server directive
        //        switch (properties[i].Name)
        //        {
        //            case "assigneddatetime":
        //                mAssignedDateTime = DateTime.FromBinary ((long)properties[i].DefaultValue);
        //                break;
        //            case "assignedby":
        //                mAssignedByID = (string)properties[i].DefaultValue;
        //                break;
        //            case "assignedstation":
        //                mAssignedStationID = (string)properties[i].DefaultValue;
        //                break;
        //            case "task":
        //                mTask = (TaskType)(int)properties[i].DefaultValue;
        //                break;
        //            case "priority":
        //                mPriority = (int)properties[i].DefaultValue;
        //                break;
        //            case "args":
        //                mArgs = (string)properties[i].DefaultValue;
        //                break;
        //            case "notes":
        //                mNotes1  = (string)properties[i].DefaultValue;
        //                break;
        //        }
        //    }
        //}
        #endregion

    }

}
