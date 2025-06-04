using System;
using System.Collections.Generic;
using KeyCommon.Messages;
using Lidgren.Network;
using KeyCommon.Helpers;

namespace KeyCommon.Messages
{
    public class Entity_ChangeCustomPropertyValue : MessageBase
    {
        // the only difference between this and Node_ChangeProperty
        // is here we know specifically that these properties are custom properties        
        public string EntityID;
        private List<Settings.PropertySpec> mCustomPropertySpecs;
        private List<KeyCommon.Simulation.PropertyOperation> mOperations;

        public Entity_ChangeCustomPropertyValue()
            : base((int)Enumerations.Entity_ChangeCustomPropertyValue)
        {
            mCustomPropertySpecs = new List<Settings.PropertySpec>();
            mOperations = new List<KeyCommon.Simulation.PropertyOperation>();
            mFlags |= Flags.ConfirmationRequired;
        }

        public void Add(string propertyName, string typeName, object value, KeyCommon.Simulation.PropertyOperation op)
        {
            Add (new Settings.PropertySpec (propertyName, typeName, value), op);
        }

        public void Add(Settings.PropertySpec spec, KeyCommon.Simulation.PropertyOperation op)
        {
            mCustomPropertySpecs.Add(spec);
            mOperations.Add(op);
        }

        public Settings.PropertySpec[] CustomProperties { get { if (mCustomPropertySpecs == null) return null; return mCustomPropertySpecs.ToArray(); } }
        public KeyCommon.Simulation.PropertyOperation[] Operations { get { if (mOperations == null) return null; return mOperations.ToArray(); } }

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            EntityID = buffer.ReadString();
            int count = buffer.ReadInt32();

            mCustomPropertySpecs = new List<Settings.PropertySpec>();
            for (int i = 0; i < count; i++)
            {
                Settings.PropertySpec spec = new Settings.PropertySpec();
                spec.Read(buffer);
                mCustomPropertySpecs.Add(spec);
            }
            mOperations = new List<KeyCommon.Simulation.PropertyOperation>();
            for (int i = 0; i < count; i++)
                mOperations.Add((KeyCommon.Simulation.PropertyOperation)buffer.ReadByte());
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(EntityID);

            int count = 0;
            if (mCustomPropertySpecs != null)
                count = mCustomPropertySpecs.Count;

            buffer.Write(count);

            for (int i = 0; i < count; i++)
            {
                mCustomPropertySpecs[i].Write(buffer);
            }

            for (int i = 0; i < count; i++)
                buffer.Write ((int)mOperations[i]);
        }
        #endregion
    }
}
