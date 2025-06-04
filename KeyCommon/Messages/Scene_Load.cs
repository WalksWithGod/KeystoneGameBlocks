//using System;
//using Lidgren.Network;
//using KeyCommon.Messages;

// Oct.1.2024 - Sim_Join.cs is now used to load Scenes for Edit mode as well as Arcade mode. 
//              The way we know if it's an Arcade mode load is if there is a Mission assigned even
//              if the Mission is a default mission with one objective to wait for user to exit the simulation.

//namespace KeyCommon.Messages
//{
//    public class Scene_Load_Request : KeyCommon.Messages.MessageBase
//    {
//        public string FolderName; // scene name


//        public bool Approved;

//        public Scene_Load_Request()
//            : base((int)KeyCommon.Messages.Enumerations.LoadSceneRequest)
//        {
//        }

//        #region IRemotableType Members
//        public override void Read(NetBuffer buffer)
//        {
//            base.Read(buffer);

//            FolderName = buffer.ReadString();
//            Approved = buffer.ReadBoolean();
//        }

//        public override void Write(NetBuffer buffer)
//        {
//            base.Write(buffer);

//            buffer.Write(FolderName);
//            buffer.Write(Approved);
//        }
//        #endregion
//    }

//    public class Scene_Load : KeyCommon.Messages.MessageBase 
//    {
//        public bool UnloadPreviousScene;

//        public string FolderName;
//        public string CampaignName;
//        public string MissionName;

//        public string StartingRegionID;

//        public Scene_Load()
//            : base((int)KeyCommon.Messages.Enumerations.LoadScene)
//        { 
//        }

//        #region IRemotableType Members
//        public override void Read(NetBuffer buffer)
//        {
//            base.Read(buffer);

//            FolderName = buffer.ReadString();
//            CampaignName = buffer.ReadString();
//            MissionName = buffer.ReadString();
//            StartingRegionID = buffer.ReadString();
//            UnloadPreviousScene = buffer.ReadBoolean ();
//        }

//        public override void Write(NetBuffer buffer)
//        {
//            base.Write(buffer);

//            buffer.Write(FolderName);
//            buffer.Write(CampaignName);
//            buffer.Write(MissionName);
//            buffer.Write(StartingRegionID);
//            buffer.Write(UnloadPreviousScene);
//        }
//        #endregion
//    }
//}
