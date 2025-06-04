using MTV3D65;

namespace Keystone.Shaders
{
    public class SkyGradient : Shader
    {
        private TV_2DVECTOR _translation;
        private TV_2DVECTOR _cloudsTiling;
        private TV_COLOR _cloudsColor;

        //public SkyGradient(string filepath, string[] defines) : base( filepath, defines)
        //{
        //}

        protected SkyGradient(string id, string resourcePath)
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

        public TV_COLOR CloudsColor
        {
            set
            {
                _cloudsColor = value;
                _tvShader.SetEffectParamColor("cloudsColor", _cloudsColor);
            }
        }

        public TV_2DVECTOR CloudsTiling
        {
            set
            {
                _cloudsTiling = value;
                _tvShader.SetEffectParamVector2("cloudsTiling", _cloudsTiling);
            }
        }

        public TV_2DVECTOR CloudsTranslation
        {
            set
            {
                _translation = value;
                _tvShader.SetEffectParamVector2("cloudsTranslation", _translation);
            }
        }
    }
}