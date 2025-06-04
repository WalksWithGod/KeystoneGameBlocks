using System;
using Lidgren.Network;
using KeyCommon.Messages;


namespace KeyCommon.Messages
{
    /// <summary>
    /// Typically initiated when a user drags a tree branch onto a location in the Gallery.
    /// Or when they click "Save Current Node as Prefab" gallery button.
    /// Additionally, on plugin Right Mouse Click menu to save as prefab, we can popup
    /// a mini custom treeview with leaf nodes and allow the user to browse and select 
    /// as well as perhaps select the current mod to store it in as well.  In any case, this command
    /// is unchanged.
    /// </summary>
    public class Prefab_Save : MessageBase 
    {
        public string NodeID;
        public string ModName; 
        public string EntryPath;  // Destination path in zip without the filename (the existing filename will be used
        public string EntryName; // New file name in zip without the path to use inplace of the one used by the temporary file


        public Prefab_Save()
            : base ((int)Enumerations.PrefabSave)
        { }

        

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            ModName = buffer.ReadString();
            NodeID = buffer.ReadString();
            EntryPath = buffer.ReadString();
            EntryName = buffer.ReadString();
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(ModName);
            buffer.Write(NodeID);
            buffer.Write(EntryPath);
            buffer.Write(EntryName);
        }
        #endregion
    }


}
