using System;
using Lidgren.Network;
using KeyCommon.Messages;

namespace KeyCommon.Messages
{
    /// <summary>
    /// Renaming and Moving a folder or entry is logically the same operation.
    /// </summary>
    public class Archive_RenameEntry : MessageBase
    {
        public string RelativeZipFilePath;
        public string ZipEntry;
        public string NewEntryName;

        public Archive_RenameEntry() : base((int)Enumerations.RenameEntryInArchive) { }

        public Archive_RenameEntry(string relativeZipPath, string zipEntryToRename, string newEntryName)
            : this()
        {
            if (string.IsNullOrEmpty (relativeZipPath) || string.IsNullOrEmpty(zipEntryToRename)
                || string.IsNullOrEmpty(newEntryName)) throw new ArgumentNullException();

            RelativeZipFilePath = relativeZipPath;
            ZipEntry = zipEntryToRename;
            NewEntryName = newEntryName;
        }

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            RelativeZipFilePath = buffer.ReadString();
            ZipEntry = buffer.ReadString();
            NewEntryName = buffer.ReadString();
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(RelativeZipFilePath);
            buffer.Write(ZipEntry);
            buffer.Write(NewEntryName);
        }
        #endregion

    }

    public class Archive_DeleteEntry : MessageBase 
    {
        public string RelativeZipFilePath;
        public string[] ZipEntries;

        public Archive_DeleteEntry() : base((int)Enumerations.DeleteFileFromArchive) { }

        public Archive_DeleteEntry (string relativeZipPath, string zipEntryToDelete)
            : this(relativeZipPath, new string[]{zipEntryToDelete})
        {
        }

        public Archive_DeleteEntry(string relativeZipPath, string[] zipEntryiesToDelete)
            : this()
        {
            RelativeZipFilePath = relativeZipPath;
            ZipEntries = zipEntryiesToDelete;
        }

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            RelativeZipFilePath = buffer.ReadString();
            int count = buffer.ReadInt32();

            if (count > 0)
            {
                ZipEntries = new string[count];
                for (int i = 0; i < count; i++)
                    ZipEntries[i] = buffer.ReadString();
            }
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(RelativeZipFilePath);
            int count = 0;
            if (ZipEntries != null)
                count = ZipEntries.Length;

            buffer.Write(count);
            for (int i =0 ; i < count; i++)
                buffer.Write(ZipEntries[i]);
        }
        #endregion
    }
}
