using System;
using Lidgren.Network;
using Keystone.Types;
using KeyCommon.Messages;

namespace KeyCommon.Messages
{
    public class Simulation_Join_Request : MessageBase
    {
        public string FolderName; // the relative //Scene// folder name in case our fast dedicated server is used to host more than 1 simulation.
        public string MissionName;
        public string HostName; // the user name of the host account hosting this game
        public string UserName;
        public string Password;
        public bool EditMode;
        public string VehiclePrefab;
        public bool Resume; // if this is the first run of the simulation, resume = false


        public Simulation_Join_Request() : base ((int)Enumerations.Simulation_Join_Request)
        {
        }


        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            FolderName = buffer.ReadString();
            MissionName = buffer.ReadString();
            HostName = buffer.ReadString();
            UserName = buffer.ReadString();
            Password = buffer.ReadString();
            VehiclePrefab = buffer.ReadString();
            EditMode = buffer.ReadBoolean();
            Resume = buffer.ReadBoolean();
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(FolderName);
            buffer.Write(MissionName);
            buffer.Write(HostName);
            buffer.Write(UserName);
            buffer.Write(Password);
            buffer.Write(VehiclePrefab);
            buffer.Write(EditMode);
            buffer.Write(Resume);
        }
        #endregion
    }


    public class Simulation_Join : MessageBase
    {
        public string FolderName; // the relative //Scene// folder name in case our fast dedicated server is used to host more than 1 simulation.
        public string CampaignName;
        public string MissionName;

        public string HostName; // the user name of the host account hosting this game
        public string UserName;
        public string Password;
        public bool EditMode;
        public string VehicleID;
        public string VehicleSaveRelativePath;
        public string RegionID;
        public Vector3d Translation;

        // for resumed games  i.e. we use the userName to query a db table to find their Player record which has Vehicle prefab specified.
        // otherwise VehiclePrefab is passed in and used for first time entry.
        public Simulation_Join() : base ((int)Enumerations.Simulation_Join)
        {
        }

    #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            FolderName = buffer.ReadString();
            CampaignName = buffer.ReadString();
            MissionName = buffer.ReadString();

            HostName = buffer.ReadString();
            UserName = buffer.ReadString();
            Password = buffer.ReadString();
            RegionID = buffer.ReadString();
            VehicleID = buffer.ReadString();
            VehicleSaveRelativePath = buffer.ReadString();
            EditMode = buffer.ReadBoolean();
            Translation.x = buffer.ReadDouble();
            Translation.y = buffer.ReadDouble();
            Translation.z = buffer.ReadDouble();
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(FolderName);
            buffer.Write(CampaignName);
            buffer.Write(MissionName);

            buffer.Write(HostName);
            buffer.Write(UserName);
            buffer.Write(Password);
            buffer.Write(RegionID);
            buffer.Write(VehicleID);
            buffer.Write(VehicleSaveRelativePath);
            buffer.Write(EditMode);
            buffer.Write(Translation.x);
            buffer.Write(Translation.y);
            buffer.Write(Translation.z);
        }
     #endregion
    }
}
