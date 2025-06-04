using System.Collections.Generic;
using Lidgren.Network;
using KeyCommon.Helpers;

namespace KeyCommon.Messages
{

    public class Geometrty_ChangeGroupProperty : MessageBase
    {
        public string GeometryNodeID;
        public int GroupIndex;
        public int GroupClass;
        private List<Settings.PropertySpec> mPropertySpecs;
        private List<KeyCommon.Simulation.PropertyOperation> mOperations;

        public Geometrty_ChangeGroupProperty(string geometryID)
            : this()
        {
            GeometryNodeID = geometryID;
        }

        public Geometrty_ChangeGroupProperty() : base((int)Enumerations.Geometry_ChangeProperty)
        {

            // mFlags |= Flags.ConfirmationRequired;   todo?
        }

        // groupClass 0 = emitter, 1 = attractor
        public void Add(int groupIndex, string propertyName, string typeName, object value, int groupClass = 0, KeyCommon.Simulation.PropertyOperation operation = KeyCommon.Simulation.PropertyOperation.Replace)
        {
            Add(groupIndex, new Settings.PropertySpec(propertyName, typeName, value), groupClass, operation);
        }

        // groupClass 0 = emitter, 1 = attractor
        public void Add(int groupIndex, Settings.PropertySpec spec, int groupClass = 0, KeyCommon.Simulation.PropertyOperation operation = KeyCommon.Simulation.PropertyOperation.Replace)
        {
            if (groupIndex != GroupIndex) throw new System.Exception();

            GroupClass = groupClass;

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
            get { if (mPropertySpecs == null) return null; return mPropertySpecs.ToArray(); }
        }

        public KeyCommon.Simulation.PropertyOperation[] Operations
        {
            get { if (mOperations == null) return null; return mOperations.ToArray(); }
        }


        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            GeometryNodeID = buffer.ReadString();
            GroupIndex = buffer.ReadInt32();
            GroupClass = buffer.ReadInt32();

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
                mOperations.Add((KeyCommon.Simulation.PropertyOperation)buffer.ReadByte());
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(GeometryNodeID);
            buffer.Write(GroupIndex);
            buffer.Write(GroupClass);

            int count = 0;
            if (mPropertySpecs != null)
                count = mPropertySpecs.Count;

            buffer.Write(count);
            for (int i = 0; i < count; i++)
            {
                mPropertySpecs[i].Write(buffer);
            }

            for (int i = 0; i < count; i++)
                buffer.Write((byte)mOperations[i]);
        }
        #endregion
    }
}
