using System.Collections.Generic;
using Lidgren.Network;
using KeyCommon.Helpers;

namespace KeyCommon.Messages
{

    public class Node_ChangeProperty : MessageBase
    {
        public string NodeID;
        private List<Settings.PropertySpec> mPropertySpecs;
        private List<KeyCommon.Simulation.PropertyOperation> mOperations;

        public Node_ChangeProperty(string nodeID)
            : this()
        {
            NodeID = nodeID;
        }

        public Node_ChangeProperty() : base ((int)Enumerations.NodeChangeState) 
        {

            // mFlags |= Flags.ConfirmationRequired;   todo?
        }

        public void Add(string propertyName, string typeName, object value, KeyCommon.Simulation.PropertyOperation operation = KeyCommon.Simulation.PropertyOperation.Replace)
        {
            Add(new Settings.PropertySpec(propertyName, typeName, value), operation);
        }

        public void Add(Settings.PropertySpec spec, KeyCommon.Simulation.PropertyOperation operation = KeyCommon.Simulation.PropertyOperation.Replace)
        {
            if (mPropertySpecs == null)
            {
                mPropertySpecs = new List<Settings.PropertySpec>();
                mOperations = new List<KeyCommon.Simulation.PropertyOperation>();
            }
            mPropertySpecs.Add(spec);
            mOperations.Add(operation);
        }

        public Settings.PropertySpec[] Properties
        { 
            get { if (mPropertySpecs == null) return null;  return mPropertySpecs.ToArray (); } 
        }

        public KeyCommon.Simulation.PropertyOperation[] Operations
        {
            get { if (mOperations == null) return null; return mOperations.ToArray(); }
        }


        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            NodeID = buffer.ReadString();
            int count = buffer.ReadInt32();

            mPropertySpecs = new List<Settings.PropertySpec>();
            for (int i = 0; i < count; i++)
            {
                Settings.PropertySpec spec = new Settings.PropertySpec();
                spec.Read(buffer);
                mPropertySpecs.Add(spec);
            }

            mOperations = new List<KeyCommon.Simulation.PropertyOperation>();
            for (int i = 0; i < count; i++)
                mOperations.Add((KeyCommon.Simulation.PropertyOperation) buffer.ReadByte());
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(NodeID);

            int count = 0;
            if (mPropertySpecs != null)
                count = mPropertySpecs.Count;

            buffer.Write(count);
            for (int i = 0; i < count; i++)
            {
                mPropertySpecs[i].Write(buffer);
            }

            for (int i = 0; i < count; i++)
                buffer.Write ((byte)mOperations[i]);
        }
        #endregion
    }
}
