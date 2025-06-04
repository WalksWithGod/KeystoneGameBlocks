using System;
using Lidgren.Network;



namespace KeyCommon.Messages
{
    public class Archive_AddFiles : MessageBase 
    {
        public string ModName;      // relative zip file path
        public string[] SourceFiles;            // full path + filename of source files (note: cant use relative because no guarantee these are actually relative
        // NOTE: If SourceFiles is empty but EntryDestinationPaths and EntryNewFilenames is not
        //       then we will create 0 byte ZipEntries for those files.  It's treated as if user 
        //       wants to create new files and then manually edit them such as .fx or .css script.
        public string[] EntryDestinationPaths;  // just the path for the entry in the archive and not including the filename
        public string[] EntryNewFilenames;      // just the new filename to use in the entry, not the path

        public Archive_AddFiles()
            : base((int)Enumerations.AddFileToArchive)
        { }

        public Archive_AddFiles(string relativeZipPath, string[] sourceFiles, string[] targetPaths, string[] newNames)
            : base ((int)Enumerations.AddFileToArchive )
        {
            ModName = relativeZipPath;
            SourceFiles = sourceFiles;
            EntryDestinationPaths = targetPaths;
            EntryNewFilenames = newNames;
        }

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            ModName = buffer.ReadString();
            int count;

            // source files
            count = buffer.ReadInt32();
            if (count > 0)
            {
                SourceFiles = new string[count];
                for (int i = 0; i < count; i++)
                    SourceFiles[i] = buffer.ReadString();
            }

            // entry destination paths
            count = buffer.ReadInt32();
            if (count > 0)
            {
                EntryDestinationPaths = new string[count];
                for (int i = 0; i < count; i++)
                    EntryDestinationPaths[i] = buffer.ReadString();
            }

            // entry new names
            count = buffer.ReadInt32();
            if (count > 0)
            {
                EntryNewFilenames = new string[count];
                for (int i = 0; i < count; i++)
                    EntryNewFilenames[i] = buffer.ReadString();
            }
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(ModName);
            int count;
            
            // source files
            count = 0;
            if (SourceFiles != null)
                count = SourceFiles.Length;

            buffer.Write(count);
            for (int i = 0; i < count; i++)
                buffer.Write(SourceFiles[i]);

            // entry destination paths
            count = 0;
            if (EntryDestinationPaths != null)
                count = EntryDestinationPaths.Length;

            buffer.Write(count);
            for (int i = 0; i < count; i++)
                buffer.Write(EntryDestinationPaths[i]);

            // entry new names
            count = 0;
            if (EntryNewFilenames != null)
                count = EntryNewFilenames.Length;

            buffer.Write(count);
            for (int i = 0; i < count; i++)
                buffer.Write(EntryNewFilenames[i]);
        }
        #endregion
    }
}
