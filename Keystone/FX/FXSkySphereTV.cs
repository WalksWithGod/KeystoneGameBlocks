using Keystone.Appearance;
using Keystone.Cameras;
using Keystone.Types;

namespace Keystone.FX
{
    public class FXSkySphereTV : FXBase
    {
        private Texture _texture;
        
        private float _radius;


        public FXSkySphereTV(Texture tex, float radius)
        {
            _semantic = FX_SEMANTICS.FX_SKY;
            _layout = FXLayout.Background;
            //TODO: needs to implement IDisposeable ... the FXBase should and this should do the ManagedResource disposes.
            Radius = radius;
            Enable = true;
            Texture = tex;
        }

        public override bool Enable
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                CoreClient._CoreClient.Atmosphere.SkySphere_Enable(value);
            }
        }

        public float Radius
        {
            get { return _radius; }
            set
            {
                _radius = value;
                CoreClient._CoreClient.Atmosphere.SkySphere_SetRadius(value);
            }
        }

        public Texture Texture
        {
            get { return _texture; }
            set
            {
                //TODO: if set is allowed, then we have to 
                Resource.Repository.DecrementRef(_texture);

                _texture = value;
                if (_texture != null)
                {
                    Resource.Repository.IncrementRef(_texture);
                    CoreClient._CoreClient.Atmosphere.SkySphere_SetTexture(_texture.TVIndex);
                }
            }
        }



        public override void Render(RenderingContext context)
        {
            CoreClient._CoreClient.Atmosphere.SkySphere_Render();
        }
    }
}