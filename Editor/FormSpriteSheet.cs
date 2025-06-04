using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KeyEdit
{
    public partial class FormSpriteSheet : Form
    {
        private int mSheetWidth, mSheetHeight;
        private string mSavePath = @"E:\dev\c#\KeystoneGameBlocks\Data\pool\textures\exp05_atlas.dds";
        private int mSpriteWidth;
        private int mSpriteHeight; 
        private int mSpritesAcross;
        private int mSpritesHigh;

        public FormSpriteSheet()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            mSheetWidth = 4096;
            mSheetHeight = 768;
            mSpriteWidth = 256;
            mSpriteHeight = 256;
            mSpritesAcross = 16;
            mSpritesHigh = 16;

            // TODO: Perhaps register an AutoResetEvent
            // TODO: somehow this functions needs to be registered with rendering thread
            // so that this rendersurface render can be done...
            // the saveToTexture can then be done elsewhere... but damn... this is
            // just so inconvenient.  And unfortunately I dont think i can do any of this
            // without creating a viewport... hrm...

            // create a rendersurface that will hold all our individual images
            MTV3D65.TVRenderSurface surface = AppMain._core.Scene.CreateRenderSurface(mSheetWidth, mSheetHeight);
            
            // for now we will hardcode paths
            string path = @"E:\dev\c#\KeystoneGameBlocks\Data\pool\textures\";
            int count = 47;
            string filePrefix = "exp05_";
            string extension = ".dds";

            float left, top, right, bottom;
            left = top = 0;
            right = bottom = 256;

            surface.StartRender();
            // open each texture one by one and blit them to our RS
            for (int i = 0; i < count; i++)
            {
                string num = string.Format("{0:0000}", i);
                string spritePath = System.IO.Path.Combine(path, filePrefix + num + extension);
                
                int textureIndex = AppMain._core.TextureFactory.LoadTexture(spritePath);
                AppMain._core.Screen2D.Draw_Texture(textureIndex, left, top, right, bottom);
                AppMain._core.TextureFactory.DeleteTexture(textureIndex);

                left += mSpriteWidth;
                right += mSpriteWidth;
                if (left >= mSheetWidth)
                {
                    top += mSpriteHeight;
                    bottom += mSpriteHeight;
                    left = 0;
                    right = mSpriteWidth;
                }
            }
            surface.EndRender();    

            // save the RS to file
            surface.SaveTexture(mSavePath, MTV3D65.CONST_TV_IMAGEFORMAT.TV_IMAGE_DDS);
            surface.Destroy();
        }

        //private int Generate(string bakedTextureName)
        //{

        //// When the user is ready to bake out a texture, we make sure there are layers ready to be baked, then...
        //if (iLayers > -1)
        //{
        //    // We create our render surface and begin painting on each layer
        //    MTV3D65.TVRenderSurface bakeSurface;
        //    bakeSurface = tvSCENE.CreateRenderSurface(bakeWidth, bakeHeight);
        //    bakeSurface.StartRender();

        //    for (int i = 0; < iLayers; i++)
        //    {
        //        // If a layer has a mask assigned to it, we take the diffuse texture and add an alpha channel to it
        //        if (bakeLayer(i).maskID <> -1)
        //            bakeLayer(i).texID = tvTEXTURES.AddAlphaChannel(bakeLayer(i).texID, bakeLayer(i).maskID, "Fex's texBaker's temporary alpha'd texture")
                
        //        // We paint on each layer with the appropriate color and opacity
        //        tvSCREEN.Draw_Texture(bakeLayer(i).texID, 0, 0, bakeWidth - 1, bakeHeight - 1, tvGLOBALS.RGBA(bakeLayer(i).texColor.x, bakeLayer(i).texColor.y, bakeLayer(i).texColor.z, bakeLayer(i).texAlpha), tvGLOBALS.RGBA(bakeLayer(i).texColor.x, bakeLayer(i).texColor.y, bakeLayer(i).texColor.z, bakeLayer(i).texAlpha), tvGLOBALS.RGBA(bakeLayer(i).texColor.x, bakeLayer(i).texColor.y, bakeLayer(i).texColor.z, bakeLayer(i).texAlpha), tvGLOBALS.RGBA(bakeLayer(i).texColor.x, bakeLayer(i).texColor.y, bakeLayer(i).texColor.z, bakeLayer(i).texAlpha))
                
        //            // If we created a new texture with an alpha channel just a second ago, we clean it up here
        //        if (bakeLayer(i).maskID <> -1)
        //            tvTEXTURES.DeleteTexture(tvGLOBALS.GetTex("Fex's texBaker's temporary alpha'd texture"))
                
        //    }
        //    bakeSurface.EndRender();
        //    // Once we're done baking down all of our layers, we save out our baked render surface as a texture
        //    if (tvTEXTURES.TextureExists(bakedTextureName))
        //        tvTEXTURES.DeleteTexture(tvGLOBALS.GetTex(bakedTextureName));

        //    bake = bakeSurface.CreateStaticTextureFromRenderSurface(bakeWidth, bakeHeight, MTV3D65.CONST_TV_COLORKEY.TV_COLORKEY_USE_ALPHA_CHANNEL, bakedTextureName);
        //    // Cleanup time!
        //    bakeSurface.Destroy();
        //}


        //}

    }
}
