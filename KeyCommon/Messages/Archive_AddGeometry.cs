using System;
using Lidgren.Network;
using KeyCommon.Messages;

namespace KeyCommon.Messages
{
    public class Archive_AddGeometry : MessageBase 
    {
        public string ModName;                  // relative zip file path
        public string SourceFilePath;// full path + filename of source files (note: cant use relative because no guarantee these are actually relative
        public string EntryDestinationPath;// just the path for the entry in the archive and not including the filename
        public string EntryNewFilename;// just the new filename to use in the entry, not the path

        public bool InteriorContainer; 
        public bool LoadXFileAsActor = false; // TODO: need GUI checkbox to indicate if this .x file should be loaded as boned actor
        public bool LoadTextures;
        public bool LoadMaterials;


        public Archive_AddGeometry() : base((int)Enumerations.AddGeometryToArchive) 
        { }

        public Archive_AddGeometry(string modName, string sourceFilePath, string entryDestPath, string entryDestFilename, bool loadTextures, bool loadMaterials, bool interiorContainer)
            : this()
        {
            ModName = modName;
            SourceFilePath = sourceFilePath;
            EntryDestinationPath = entryDestPath;
            EntryNewFilename = entryDestFilename;
            LoadTextures = loadTextures;
            LoadMaterials = loadMaterials;
            InteriorContainer = interiorContainer;
        }

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            ModName = buffer.ReadString();
            SourceFilePath = buffer.ReadString();
            EntryDestinationPath = buffer.ReadString();
            EntryNewFilename = buffer.ReadString();  // we can't really use this if loading textures or materials or the "entryNewName" array wont have the same number of elements
            LoadXFileAsActor = buffer.ReadBoolean();
            LoadTextures = buffer.ReadBoolean();
            LoadMaterials = buffer.ReadBoolean();
            InteriorContainer = buffer.ReadBoolean();
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(ModName);
            buffer.Write(SourceFilePath);
            buffer.Write(EntryDestinationPath);
            buffer.Write(EntryNewFilename);
            buffer.Write(LoadXFileAsActor);
            buffer.Write(LoadTextures);
            buffer.Write(LoadMaterials);
            buffer.Write(InteriorContainer);
        }
        #endregion
    }
}
