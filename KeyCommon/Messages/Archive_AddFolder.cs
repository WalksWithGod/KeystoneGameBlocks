using System;
using Lidgren.Network;
using KeyCommon.Messages;

namespace KeyCommon.Messages
{
    public class Archive_AddFolder : MessageBase 
    {
        public string ModName;
        public string FolderNameToAdd;

        public Archive_AddFolder()
            : base((int)Enumerations.AddFolderToArchive)
        { 
        }

        public Archive_AddFolder(string relativeZipPath, string folderNameToAdd)
            : this()
        {
            ModName = relativeZipPath;
            FolderNameToAdd = folderNameToAdd;
        }

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            ModName = buffer.ReadString();
            FolderNameToAdd = buffer.ReadString();
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(ModName);
            buffer.Write(FolderNameToAdd);
        }
        #endregion

    }
}
