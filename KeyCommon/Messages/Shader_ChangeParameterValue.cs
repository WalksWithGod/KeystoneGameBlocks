using System;
using System.Collections.Generic;
using KeyCommon.Messages;
using Lidgren.Network;
using KeyCommon.Helpers;


namespace KeyCommon.Messages
{
    public class Shader_ChangeParameterValue : MessageBase
    {
        public string AppearanceID;
        private List<Settings.PropertySpec> mShaderParameterSpecs;

        public Shader_ChangeParameterValue()
            : base ((int)Enumerations.Shader_ChangeParameterValue)
        {
            mShaderParameterSpecs = new List<Settings.PropertySpec>();
        }

        public void Add(Settings.PropertySpec spec)
        {
            mShaderParameterSpecs.Add(spec);
        }

        public Settings.PropertySpec[] ShaderParameters { get { if (mShaderParameterSpecs == null) return null; return mShaderParameterSpecs.ToArray(); } }

        #region IRemotableType Members
        public override void Read(NetBuffer buffer)
        {
            base.Read(buffer);
            AppearanceID = buffer.ReadString();
            int count = buffer.ReadInt32();

            mShaderParameterSpecs = new List<Settings.PropertySpec>();
            for (int i = 0; i < count; i++)
            {
                Settings.PropertySpec spec = new Settings.PropertySpec();
                spec.Read(buffer);
                mShaderParameterSpecs.Add(spec);
            }
        }

        public override void Write(NetBuffer buffer)
        {
            base.Write(buffer);
            buffer.Write(AppearanceID);

            int count = 0;
            if (mShaderParameterSpecs != null)
                count = mShaderParameterSpecs.Count;

            buffer.Write(count);
            for (int i = 0; i < count; i++)
            {
                mShaderParameterSpecs[i].Write(buffer);
            }
        }
        #endregion
    }
}
