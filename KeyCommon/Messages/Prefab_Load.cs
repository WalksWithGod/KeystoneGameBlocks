using System;
using Lidgren.Network;
using Keystone.Types;
using KeyCommon.Messages;

namespace KeyCommon.Messages
{
    // todo: i think this is obsolete. keystone.dll should not need to know ComponentType.  That is for exe and loopback only.
    public enum ComponentType : byte
    {
        Floor,
        Ceiling,
        Common,
        EdgeComponent,
        Door,
        Stair,
        Ladder,
        Elevator,
        Wall_Left,
        Wall_Right,
        Wall_Front,
        Wall_Back
    }

    public interface IFragmentable
    {
        bool IsFragmented { get; }
        string Guid { get; }
        int Index { get; }
        int Length { get; } // can be length of payload or total number of elements in array.  What about the amount of array elements in just the current packet?

        void MergeFragment(IFragmentable fragment);
        int GetHeaderSize();
    }
    
    
    // todo: it seems to me that Simulation_Spawn and Prefab_Load are the same thing
    public class Prefab_Load : MessageBase, IFragmentable
    {
        
        public string RelativeArchivePath; // this is the path of the Archive that contains our .kgbentity files. We don't really use this anymore since switching to just .kgbentity's existing seperately on disk under a mod path hierarchy
        public string EntryPath;    // if this is an Archive, this is the Entry name within the archive. Otherwise this is the relative disk path of the kgbentity after the MODS_PATH e.g. caesar\\entities\\vehicles\\yorktown.kgbentity"
        public string ParentID;
        public bool GenerateIDs; // TODO: we can't always use CloneEntityIDs because that would result in some nodes that are not Resources from having unique IDs.  There are only two options.  Either loopback instances the prefab and assigns IDs and then serializes it over the wire for the client to load.

        public bool IsSavedEntity; // Indicates this is not a prefab, but is in //saves// path

        // todo: should i implement some type of IFragmentable?


        public Vector3d Position; // todo: i also need other properties like linear velocity and scale for lasers for example.
        public string[] NodeIDsToUse; // this is assigned by the server // todo: do we need to implement fragment handling in case the actual number of NodeIDs we want to assign exceed the MAX_BUFFER_SIZE?

        // todo: i don't think Recurse and DelayResourceLoading are needed here.  These aspects need to be handled
        // differently by the client or server.
        public bool Recurse;
        public bool DelayResourceLoading;
        
        
        

        public Prefab_Load(string resourceDescriptor)
            : this(new KeyCommon.IO.ResourceDescriptor(resourceDescriptor))
        { }

        public Prefab_Load(KeyCommon.IO.ResourceDescriptor descriptor)
            : this(descriptor.ModName, descriptor.EntryName)
        {
        }

        public Prefab_Load(string relativeArchivePath, string pathInArchive)
            : this()
        {
            RelativeArchivePath = relativeArchivePath;
            EntryPath = pathInArchive;
        }

        public Prefab_Load() : base ((int)Enumerations.PrefabLoad)
        { }



        #region IFragentable
        private bool mIsFragmented;
        private string mGUID;
        private int mIndex;
        private int mLength;

        public bool IsFragmented { get { return mIsFragmented; } }
        public string Guid { get { return mGUID; } }
        public int Index { get { return mIndex; } }
        public int Length { get { return mLength; } } // can be length of payload or total number of elements in array.  What about the amount of array elements in just the current packet?

        public void MergeFragment(IFragmentable fragment)
        {
            Prefab_Load f = (Prefab_Load)fragment;

            NodeIDsToUse = Keystone.Extensions.ArrayExtensions.ArrayAppendRange(NodeIDsToUse, f.NodeIDsToUse) ;

            throw new NotImplementedException(); 
        }
        public int GetHeaderSize()
        {
            return 0;
        }
        #endregion

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);

            // todo: these strings could be null in all fragments after the initial fragment. We need to handle read/write here specially
            RelativeArchivePath = buffer.ReadString();
            EntryPath =  buffer.ReadString();
            ParentID = buffer.ReadString();

            //GenerateIDs = buffer.ReadBoolean();
            int count = buffer.ReadInt32();
            if (count > 0)
            {
                NodeIDsToUse = new string[count];
                for (int i = 0; i < count; i++)
                    NodeIDsToUse[i] = buffer.ReadString();
            }
            Position.x = buffer.ReadDouble();
            Position.y = buffer.ReadDouble();
            Position.z = buffer.ReadDouble();
            
            Recurse = buffer.ReadBoolean();
            DelayResourceLoading = buffer.ReadBoolean();
            IsSavedEntity = buffer.ReadBoolean();
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(RelativeArchivePath);
            buffer.Write(EntryPath);
            buffer.Write(ParentID);

            //buffer.Write(GenerateIDs);
            int count = 0;
            if (NodeIDsToUse != null)
            {
                count = NodeIDsToUse.Length;
            }
            buffer.Write(count);
                
            for (int i = 0; i < count; i++)
                buffer.Write(NodeIDsToUse[i]);

            
            buffer.Write(Position.x);
            buffer.Write(Position.y);
            buffer.Write(Position.z);
            
            buffer.Write(Recurse);
            buffer.Write(DelayResourceLoading);
            buffer.Write(IsSavedEntity);
        }
        #endregion
    }
}
