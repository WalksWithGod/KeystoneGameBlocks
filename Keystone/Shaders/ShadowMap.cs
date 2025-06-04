using Keystone.Types;
using MTV3D65;

namespace Keystone.Shaders
{
    // TODO: Shaders like shadowmap depth pass can be integrated into deferred geometry pass
    // and i think these should be more about viewport settings.  it's complicated... not sure yet
    public class ShadowMap : Shader
    {
        private int _lightspaceTexture;
        private TV_3DMATRIX _lightViewProjection;
        private TV_3DMATRIX _lightViewProjectionTexture;
        private Vector3f _ambientColor;

        protected ShadowMap(string id, string resourcePath)
            : base(id)
        {
        }

        public override object Traverse(Traversers.ITraverser target, object data)
        {
            throw new System.Exception("The method or operation is not implemented.");
        }

        internal override Traversers.ChildSetter GetChildSetter()
        {
            throw new System.Exception("The method or operation is not implemented.");
        }

        public int LightSpaceTexture
        {
            get { return _lightspaceTexture; }
            set
            {
                if (!_lightspaceTexture.Equals(value))
                {
                    _lightspaceTexture = value;
                    _tvShader.SetEffectParamTexture("texLightSpaceMap", value);
                }
            }
        }

        public Vector3f AmbientColor
        {
            get { return _ambientColor; }
            set
            {
                if (!_ambientColor.Equals(value))
                {
                    _ambientColor = value;
                    _tvShader.SetEffectParamVector3("ambientColor", Helpers.TVTypeConverter.ToTVVector(value));
                }
            }
        }

        public TV_3DMATRIX LightCameraViewProjection
        {
            get { return _lightViewProjection; }
            set
            {
                if (!_lightViewProjection.Equals(value))
                {
                    _lightViewProjection = value;
                    //matLightViewProjection
                    _tvShader.SetEffectParamMatrix("matLightViewProjection", value);
                }
            }
        }

        public TV_3DMATRIX LightCameraViewProjectionTexture
        {
            get { return _lightViewProjectionTexture; }
            set
            {
                if (!_lightViewProjectionTexture.Equals(value))
                {
                    _lightViewProjectionTexture = value;
                    //matLightViewProjectionTexture
                    _tvShader.SetEffectParamMatrix("matLightViewProjectionTexture", value);
                }
            }
        }

        public TV_2DVECTOR TexelSize
        {
            set { _tvShader.SetEffectParamVector2("texelSize", value); }
        }

        public void SetPCFOffset(int index, TV_2DVECTOR offset)
        {
            _tvShader.SetEffectParamVector2("pcfOffsets[" + index + "]", offset);
        }
    }
}