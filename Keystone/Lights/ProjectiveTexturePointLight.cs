using System;
using Keystone.Appearance;
using Keystone.Types;
using MTV3D65;

namespace Keystone.Lights
{
    public class ProjectiveTexturePointLight : PointLight
    {
        private Texture _texture;

        public ProjectiveTexturePointLight(string name, Vector3d position, float r, float g, float b, 
                                           float range, float specularlevel, float attenuation0, float attenuation1, float attenuation2, Texture texture, bool indoorLight)
            
            : base(name, position, r, g, b,  range, attenuation0, attenuation1, attenuation2, indoorLight)
        {
            if (texture == null) throw new ArgumentNullException();
            _texture = texture;
        }

        public Texture ProjectiveTexture
        {
            get { return _texture; }
            set { _texture = value; }
        }
    }
}