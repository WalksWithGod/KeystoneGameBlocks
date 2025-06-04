using System;
using System.Collections.Generic;
using KeyCommon.Messages;
using Lidgren.Network;
using KeyCommon.Helpers;

namespace KeyCommon.Messages
{
    public class Node_ChangeFlag : MessageBase
    {
        public string NodeID;
        public string FlagType;
        
        public Dictionary<string, bool> Flags;

        public Node_ChangeFlag()
            : base((int)Enumerations.NodeChangeFlag)
        {
            Flags = new Dictionary<string, bool>();
        }

        // NOTE: We can't simply swap the entity's current flags with a new one
        // unless we start with the current state of entity flags.  
        // For now let's store key/value pair
        public void Add(string flagName, bool value)
        {
            if (Flags.ContainsKey(flagName))
                Flags.Remove(flagName);

            
            Flags.Add(flagName, value);
        }

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            NodeID = buffer.ReadString();
            FlagType = buffer.ReadString();

            int count = buffer.ReadInt32();

            Flags = new Dictionary<string, bool>();

            for (int i = 0; i < count; i++)
            {
                string key = buffer.ReadString();
                bool value = buffer.ReadBoolean();
                Flags.Add(key, value);
            }
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(NodeID);
            buffer.Write(FlagType);

            int count = 0;
            if (Flags != null)
                count = Flags.Count ;

            buffer.Write(count);
            foreach (string key in Flags.Keys)
            {
                buffer.Write(key);
                buffer.Write(Flags[key]);
            }
        }
        #endregion
    }
}
