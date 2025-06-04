using System;
using Keystone.Appearance;
using Keystone.Cameras;
using Keystone.Types;

namespace Keystone.FX
{
    public class FXSkyBoxTV :FXBase 
    {
        private Texture[] mTextures;
        private float mRadius;

        public FXSkyBoxTV (Texture[] sides, float radius)
        {
            if (sides == null) throw new ArgumentNullException();
            _semantic = FX_SEMANTICS.FX_SKY;
            _layout = FXLayout.Background;
            //TODO: needs to implement IDisposeable ... the FXBase should and this should do the ManagedResource disposes to
            // decrementref textures[]
            Enable = true;
            Radius = radius;
            Textures = sides;

        }

        public override bool Enable
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                CoreClient._CoreClient.Atmosphere.SkyBox_Enable(value);
            }
        }

        public float Radius
        {
            get { return mRadius; }
            set
            {
                mRadius = value;
                CoreClient._CoreClient.Atmosphere.SkyBox_SetScale (value, value, value);
            }
        }

        public Texture[] Textures
        {
            get { return mTextures; }
            set
            {
                if (value == null)
                {
                    // remove any existing textures
                    if (mTextures != null )
                        for (int i = 0; i < mTextures.Length; i++)
                            Resource.Repository.DecrementRef(mTextures[i]);
                        
                    CoreClient._CoreClient.Atmosphere.SkyBox_SetTexture( -1, -1, -1, -1, -1, -1);
                }
                else
                {
                    if (value.Length != 6) throw new ArgumentOutOfRangeException();
                    mTextures = value;

                    for (int i = 0; i < mTextures.Length; i++)
                        Resource.Repository.IncrementRef(mTextures[i]);

                    CoreClient._CoreClient.Atmosphere.SkyBox_SetTexture(mTextures[0].TVIndex, mTextures[1].TVIndex, mTextures[2].TVIndex, mTextures[3].TVIndex, mTextures[4].TVIndex, mTextures[5].TVIndex);
                }
            }
        }



        public override void Render(RenderingContext context)
        {
            CoreClient._CoreClient.Atmosphere.SkyBox_Render();
        }
    }
}
