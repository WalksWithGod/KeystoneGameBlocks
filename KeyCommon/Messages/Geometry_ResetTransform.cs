using System;
using KeyCommon.Messages;
using Lidgren.Network;
using KeyCommon.Helpers;
using Settings;

namespace KeyCommon.Messages
{
    public class Geometry_ResetTransform : MessageBase 
    {
        public string GeometryID;
        public Keystone.Types.Matrix Transform;

        public Geometry_ResetTransform()
            : base ((int)Enumerations.Geometry_ResetTransform)
        {
        }

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);

            GeometryID = buffer.ReadString();
            Transform = new Keystone.Types.Matrix(
                buffer.ReadDouble(),
                buffer.ReadDouble(),
                buffer.ReadDouble(),
                buffer.ReadDouble(),
                buffer.ReadDouble(),
                buffer.ReadDouble(),
                buffer.ReadDouble(),
                buffer.ReadDouble(),
                buffer.ReadDouble(),
                buffer.ReadDouble(),
                buffer.ReadDouble(),
                buffer.ReadDouble(),
                buffer.ReadDouble(),
                buffer.ReadDouble(),
                buffer.ReadDouble(),
                buffer.ReadDouble());
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);

            buffer.Write(GeometryID);
            buffer.Write(Transform.M11);
            buffer.Write(Transform.M12);
            buffer.Write(Transform.M13);
            buffer.Write(Transform.M14);
            buffer.Write(Transform.M21);
            buffer.Write(Transform.M22);
            buffer.Write(Transform.M23);
            buffer.Write(Transform.M24);
            buffer.Write(Transform.M31);
            buffer.Write(Transform.M32);
            buffer.Write(Transform.M33);
            buffer.Write(Transform.M34);
            buffer.Write(Transform.M41);
            buffer.Write(Transform.M42);
            buffer.Write(Transform.M43);
            buffer.Write(Transform.M44);

        }
        #endregion
    }
}
