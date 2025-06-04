using System;
using System.Collections.Generic;
using KeyCommon.Messages;
using Lidgren.Network;
using KeyCommon.Helpers;

namespace KeyCommon.Messages
{
    /// <summary>
    /// Typically this is used to transfer a .kgbentity hiearchy complete with server generatede node.IDs.
    /// For instance, the server can load an entire Vehicle or Container complete with crew, furniture, components and 
    /// save it to a temp file and then transfer the temp file.  This is easier than sending a huge hierarchy of NodeStates.
    /// HOWEVER, it is always assumed that this hierarchy is based on the server having loaded a prefab. 
    /// This means the client already has the prefab (client should always have correct mods installed) so we don't
    /// ever have to transferf the .Interior file, the client can just rename a copy it has locally.
    /// </summary>
    /// <remarks>This does not yet currently support Vehicles or Containers nested inside other Vehicles or Containers. 
    /// To support that, we would need to store the hierarchy of relative prefab paths so that we can restore
    /// and assign .interior database files</remarks>
    public class Transfer_Entity_File : MessageBase
    {
        public bool IsFragmented;
        public int FragmentIndex;
        private int mDataSize;
        private byte[] mData; // NOTE: the max size is set by application constructing the commands using dataSize = AppMain._core.Settings.settingReadInteger("lidgren", "receivebuffersize") - MAX_HEADER_SIZE 

        // relevant properties are only written to / read from the NetBuffer when IsFragmented == false OR FragmentIndex == 0
        // so there is no significant overhead using this single Transfer_File command for the initial packet and then the subsequent packets
        public string Guid; // this is assigned by the server only for Transfer's that require more than one packet
        public long FileLength; // the complete file length 
        public string mParentID;
        public string mRootNodeID; // todo: why is this necessary?  THe root node.ID should be set in the file we are transferring.  All we really need is the relativePath of the prefab this file is derivved from so that we can for Vehicles and Containers, copy the .interior file
        public string mRootNodeTypename;
        public string mPrefabRelativePath;
        public string mNewRelativeDataPath;
        

        public Transfer_Entity_File() : base ((int)Enumerations.TransferEntityFile)
        {
            FragmentIndex = 0; // initialize to 0 even though its not necessary. I just want to make a point that default for single packet transfer is always 0 and does not get overwritten by buffer.ReadInt32()
        }
        
        public byte[] Data
        {
            get { return mData; }
            set
            {
                mData = value;
                if (mData == null)
                    mDataSize = 0;
                else
                    mDataSize = mData.Length;
                
            }
        }

        public int DataSize { get { return mDataSize; } }

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            
            IsFragmented = buffer.ReadBoolean();
            
            mDataSize = buffer.ReadInt32();
            mData = buffer.ReadBytes(mDataSize);

            if (IsFragmented)
                FragmentIndex = buffer.ReadInt32(); // NOTE: for transfers that only require a single packet FragmentIndex is already == 0 so we don't need to read it

            if (IsFragmented = false || FragmentIndex == 0)
            { 
                if (IsFragmented)
                    Guid = buffer.ReadString();

                FileLength = buffer.ReadInt64(); // full lenth of the file
                mParentID = buffer.ReadString();
                mRootNodeID = buffer.ReadString();
                mRootNodeTypename = buffer.ReadString();
                mPrefabRelativePath = buffer.ReadString();
                mNewRelativeDataPath = buffer.ReadString();
            } 
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(IsFragmented);
            buffer.Write(mDataSize);
            buffer.Write(mData);

            if (IsFragmented)
                buffer.Write(FragmentIndex);

            if (IsFragmented == false || FragmentIndex == 0)
            {
                if (IsFragmented)
                    buffer.Write(Guid);

                buffer.Write(FileLength);
                buffer.Write(mParentID);
                buffer.Write(mRootNodeID);
                buffer.Write(mRootNodeTypename);
                buffer.Write(mPrefabRelativePath);
                buffer.Write(mNewRelativeDataPath); 
            }
            
        }
        #endregion
    }

    
   
}
