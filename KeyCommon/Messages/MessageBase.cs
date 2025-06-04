using System;
using System.Collections.Generic;
using System.Threading;

namespace KeyCommon.Messages
{
    [Flags] // ToString()  will then return something like WriteToScene, AdminOnly  where a comma seperates all flags that are enabled.  Otherwise without the Flags attribute, it just returns a number.
    public enum Flags : int
    {
        None = 0,
        AdminOnly = 1 << 0,    // also equivalent to just 1
        WriteToScene = 1 << 1,
        Placeholder = 1 << 2,
        CanUndo = 1 << 3,
        ConfirmationRequired = 1 << 4,
        SourceIsClient = 1 << 5,
        SourceIsServer = 1 << 6,
        SourceIsClientScript = 1 << 7
    }

    public enum Enumerations : int
    {
        RequestConfirmation = 1,
        // edit related
        NotifyPlugin_NodeSelected,
        NotifyPlugin_ProcessEventQueue,
           
        // can check for permissions when in edit mode if range is allowed
        
        AddFolderToArchive = 999,  // start at 1,000 as all command enums should start at a certain range
        AddGeometryToArchive,
        AddFileToArchive,
        DeleteFileFromArchive,
        RenameEntryInArchive,
        //AddNode,
        //AddEntity,
        //AddLight, // obsolete: users should use Node_Create instead
        Terrain_Paint,
        InsertPrefab_Interior,
        InsertPrefab_Structure,
        PlaceEntity_EdgeSegment,
        PlaceWall_CelledRegion,
        CelledRegion_UpdateConnectivity,
        RegionPageComplete,
        TileMapStructure_PaintCell,
        CelledRegion_PaintCell,
        PrefabLoad,
        PrefabSave,
        GenerateScene,
        NewScene,
        NewUniverse,
        NewTerrainScene,
        NewFloorplan,
        LoadScene,
        LoadSceneRequest,
        GenerateUniverse,

        // Transfer large data
        FileTransferRequest,
        
        TransferEntityFile,
        StreamTransferRequest,
        StreamTransferBegin,
        StreamTransfer,
        // TODO: I think i need a Node_Stream // for hierarchies extending from a senior parent node. Maybe it's just what StreamTransfer.cs is for

        // Nodes
        RemoveNode,  // TODO: Rename as Entity_Kill.  Node_Remove is remove from scene while editing map, remove is "Kill" and just removes element during gameplay.. although wouldnt delete also allow for persistant changes to game map during gameplay?
        NodeGetChildren,
        NodeChangeState, // NOTE: this means ChangeProperty
        NodeGetState,
        Node_RenameResource,
        Node_ReplaceResource, // only replaces resources since these names are fixed and don't require GUID generation on server
        Node_ChangeParent,
        Node_Create_Request, // request to create a specific tpe and receive a GUID from server
        Node_Create,         // create along with list of properties to restore
        Node_MoveChildOrder,
        Node_InsertUnderNew_Request,
        Node_InsertUnderNew,
        Node_Remove,
        Node_Copy,
        Node_Paste,

        Geometry_Add,
        GeometrySave,
        Geometry_CreateGroup,
        Geometry_RemoveGroup,
        Geometry_ResetTransform,
        Geometry_ChangeProperty,

        // tasks
        Task_Create_Request,
        Task_Create,
        Task_Delete_Request,
        Task_Delete,
        Task_Edit_Request,
        Task_Edit,
        
        // appearance
        Shader_ChangeParameterValue,

        // entities
        Entity_Move,
        Entity_GetCustomProperties,
        Entity_SetCustomProperties,
        Entity_ChangeCustomPropertyValue,

        // game objects
        GameObject_Create,
        GameObject_Create_Request,
        GameObject_ChangeProperties,

        // Missions
        MissionResult,


        // authentication related
        AuthenticationLogin,
        ServiceLogin,
        RequestTicket,
        TicketReply,

        // database entities
        User,
        Host,
        Game,

        // general commands
        CommandSuccess,
        CommandFail,
        // lobby commands
        RequestGamesList,
        GamesList,
        GameSummaryList,

        Simulation_Register,
        Simulation_Join_Request,
        Simulation_Join,
        Simulation_Leave,
        Simulation_GenerateCharacters,
        Simulation_Spawn,
        GameSummary,
        GameServerInfo,
        ChatMessage,
        UserJoined,
        UserLeft,
        Table_Create,
        Table_Close,
        Table_Join,
        Table_Leave,
        Table_ReadyStatusChanged,
        Table_StatusChanged,
        QueryGames,
        //QueryUsers,
        RetreiveTableList,
        RetreiveUserList,
        EntityList,

        // game commands
        UserStatusChanged,  // TODO: why not just delete UserTableStatus changed and use this for both?
        EntitySpawn,
        EntityDestroy,
        EntityStateChange,
        NodeChangeFlag,

        // navigation
        MoveToTarget,

        // Create
        // Delete 
        UserMessages = 99999
    }

    /// <summary>
    /// KeyCommon is always referenced by Keystone.DLL.  Thus Authentication server which has no need for Keystone.dll
    /// can reference the common classes here instead.  However, we should use a seperate dll for implementing specific messages
    /// I think?  Or specific commands.  For instance, we could call it CannonSaga.dll 
    /// </summary>
    public class MessageBase : Lidgren.Network.IRemotableType
    {
        private static long mLastUUID;

        public string Name;
        protected long mUUID;
        protected int mCommand;
        protected Flags mFlags; // not serialized for security because client can't be trusted.  These are set only in the Init so server always recreates proper flags
        // flags should probably contain "Undo" and such but for now we'll use a bool

        public MessageBase (int commandID)
         {
             mCommand = commandID;
             Name = this.GetType().Name;
             mFlags = Flags.None;  // flags should only ever be set in constructor and never deserialized.  Clients cannot be trusted.
             mUUID = GetUUID();
        }

        //This method handles an overflow condition by wrapping: if location = Int32.MaxValue, location + 1 = Int32.MinValue. No exception is thrown.

        private static long GetUUID()
        {
            return Interlocked.Increment(ref mLastUUID);
        }

        public bool CanUndo { get { return HasFlag(Flags.CanUndo); } }

        
        public void SetFlag(Flags flags)
        {
            mFlags |= flags;
        }

        public bool HasFlag(Flags flag)
        {
            // TODO: c# 4.0 has the new enum.HasFlag() method but we dont.
            // as soon as we fully ditch MDX 1.1 for SLimDX we can use c# 4.0
            //  note however that .HasFlag() will always return true if testing for None  = 0.
            //if ((flag & Flags.None) != 0)
            //    return false;

            //if ((flag & Flags.WriteToScene) != 0)
            //    return true;
            //else if ((flag & Flags.AdminOnly) != 0)
            //    return true;

            // return mFlags.HasFlag (flag); // TODO: <- not tested
            return (mFlags & flag) == flag; // TODO: does this work correclty? perhaps with one flag in the parameter and not multiple
            //return false;
        }


        #region IRemotableType Members
        public int CommandID
        {
            get { return mCommand; }
        }

        public int Type
        {
            get { return (int)CommandID; }
        }

        public long UUID
        {
            get { return mUUID; }
        }

        public virtual Lidgren.Network.NetChannel Channel
        {
            get { return Lidgren.Network.NetChannel.ReliableInOrder1; }
        }

        public virtual void Read(Lidgren.Network.NetBuffer buffer)
        {
            mFlags = (Flags)buffer.ReadInt32();
            mUUID = buffer.ReadInt64();

//#if DEBUG
//            Name = buffer.ReadString();
//#endif
        }

        public virtual void Write(Lidgren.Network.NetBuffer buffer)
        {
            buffer.Write((int)mFlags);
            buffer.Write(mUUID);
//#if DEBUG
//            buffer.Write(Name);
//#endif

        }
        #endregion
    }
}
