using System;
using Keystone.Types;
using Keystone.Extensions;

namespace Keystone.Appearance
{
    // http://www.gamasutra.com/view/feature/130940/practical_texture_atlases.php?print=1
    // "The second modification is in the vertex shaders. For each set of texture coordinates 
    // a simple remapping function should be called to scale and offset them with the 
    // remapping constants for the appropriate sampler. 
    // 
    // With these two changes the introduction of atlases remains completely transparent 
    // to the rest of the system. The basic operations with textures remain the same as 
    // without atlases: load texture by filename, and assign loaded texture to sampler. 
    // The actual benefits of texture atlases are reaped in the third integration point, 
    // where this transparency must be broken.""
    public class TextureAtlas : Texture
    {
        // TODO: Mesh3dAtlas : Mesh3d 
        //          - similar to TextureAtlas
        //          - maybe this takes the form of minimeshes too though?
        //          - in other words, our grids and our walls, part of the problem we had with making them
        //            seperate entities is the culling slow and maybe the actual minimesh rendering was
        //            sufficiently fast.  In this way our inbound/out of bounds become enable/disable
        //            of the minimesh elements.

        // TODO: what about the dynamic management of texture atlases?
        // NOTE: apparenly sims3 when you dress your Sim, it will create an atlas of all the 
        //       sub-meshes textures so no texture switches when rendering each sim.
        //      - is there a way to use an atlas in such a way that it's invisible from the scenegraph
        //      and is like "use instancing" minimesh rendering where it occurs automatically in our
        //      SceneDrawer.
        //      - if say the appearance has "use atlas" as models have "use instancing" then the problem here is
        //        how do you chose the atlas to use?
        //          - i think for now, the best course is to have a Texture that is an Atlas
        //          and then to allow the painter to set the atlas index 

        // TODO: if there is an "atlas" version for a texture
        //       is there a way we can fasciliate 
        // int type;  // eg. 2D, 3D texture, etc
        // string[] filename;
        private float[] mWidthOffset;  // ideally when loading the texture, we can  
        private float[] mHeightOffset; // define the atlas items
        private new float[] mWidth;
        private new float[] mHeight;
        private float[] mDepthOffset;


        internal TextureAtlas(string resourcePath)
            : base(resourcePath)
        {
        	_textureType = TEXTURETYPE.Default;
        }

        public static TextureAtlas Create(string resourcePath, AtlasTextureTools.AtlasRecord[] records)
        {
        	TextureAtlas t = (TextureAtlas)Keystone.Resource.Repository.Create(resourcePath, "TextureAtlas");
            for (int i = 0; i < records.Length; i++)
                t.AddSubTexture(records[i].WidthOffset, records[i].HeightOffset, records[i].DepthOffset, records[i].Width, records[i].Height);

            return t;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="specOnly">True returns the properties without any values assigned</param>
        /// <returns></returns>
        public override Settings.PropertySpec[] GetProperties(bool specOnly)
        {
            Settings.PropertySpec[] tmp = base.GetProperties(specOnly);
            Settings.PropertySpec[] properties = new Settings.PropertySpec[5 + tmp.Length];
            tmp.CopyTo(properties, 5);

            properties[0] = new Settings.PropertySpec("woffsets", typeof(float[]).Name);
            properties[1] = new Settings.PropertySpec("hoffsets", typeof(float[]).Name);
            properties[2] = new Settings.PropertySpec("doffsets", typeof(float[]).Name);
            properties[3] = new Settings.PropertySpec("widths", typeof(float[]).Name);
            properties[4] = new Settings.PropertySpec("heights", typeof(float[]).Name);

            if (!specOnly)
            {
                properties[0].DefaultValue = mWidthOffset;
                properties[1].DefaultValue = mHeightOffset;
                properties[2].DefaultValue = mDepthOffset;
                properties[3].DefaultValue = mWidth;
                properties[4].DefaultValue = mHeight;
            }

            return properties;
        }

        public override void SetProperties(Settings.PropertySpec[] properties)
        {
            if (properties == null) return;
            base.SetProperties(properties);

            for (int i = 0; i < properties.Length; i++)
            {
                if (properties[i].DefaultValue == null) continue;
                // use of a switch allows us to pass in all or a few of the propspecs depending
                // on whether we're loading from xml or changing a single property via server directive
                switch (properties[i].Name)
                {
                    case "woffsets":
                        mWidthOffset = (float[])properties[i].DefaultValue;
                        break;
                    case "hoffsets":
                        mHeightOffset = (float[])properties[i].DefaultValue;
                        break;
                    case "doffsets":
                        mDepthOffset = (float[])properties[i].DefaultValue;
                        break;
                    case "widths":
                        mWidth = (float[])properties[i].DefaultValue;
                        break;
                    case "heights":
                        mHeight  = (float[])properties[i].DefaultValue;
                        break;

                }
            }
            SetChangeFlags(Keystone.Enums.ChangeStates.AppearanceParameterChanged, Keystone.Enums.ChangeSource.Self);
        }

        // TODO: when do we specify the subtexture count or do we just use a collection?
        // TODO: we can manually create these subtextures in code 
        public void AddSubTexture(float wOffset, float hOffset, float depthOffset, float width, float height)
        {
            mWidthOffset = mWidthOffset.ArrayAppend(wOffset);
            mHeightOffset = mHeightOffset.ArrayAppend(hOffset);
            mDepthOffset = mDepthOffset.ArrayAppend(depthOffset);
            mWidth = mWidth.ArrayAppend(width);
            mHeight = mHeight.ArrayAppend(height);
        }

        public Vector2f[] GetTileDimensions(uint atlasIndex)
        {
            return GetTileDimensions(0f, 0f, 1f, 1f, atlasIndex);
        }

        public Vector2f[] GetTileDimensions(float originalU1, float originalV1, uint atlasIndex)
        {
            return GetTileDimensions(originalU1, originalV1, 1f, 1f, atlasIndex);
        }

        /// <summary>
        /// Used to compute new UV1 and UV2 coordinates for a vertex that is going from standard single
        /// texture to using a texture at a specific index in an atlas where the single texture
        /// is now part of the atlas.
        /// </summary>
        /// <remarks>
        /// This computes correct UV1 and UV2 even when the atlas contains sub-textures of varied
        /// individual dimensions!
        /// </remarks>
        /// <param name="originalU1">original vertex U1 value when still using a single texture instead of atlas sub-texture</param>
        /// <param name="originalV1"></param>
        /// <param name="originalU2"></param>
        /// <param name="originalV2"></param>
        /// <param name="atlasIndex"></param>
        /// <returns></returns>
        public Vector2f[] GetTileDimensions(float originalU1, float originalV1, float originalU2, float originalV2, uint atlasIndex)
        {
            float ratioU = originalU2 - originalU1;
            float ratioV = originalV2 - originalV1;

            Vector2f min; // will contain u1, v1
            min.x = mWidthOffset[atlasIndex];
            min.y = mHeightOffset[atlasIndex];

            Vector2f max; // will contain u2, v2
            max.x = min.x + (mWidth[atlasIndex] * ratioU);
            max.y = min.y + (mHeight[atlasIndex] * ratioV);

            return new Vector2f[] { min, max };
        }

        // such that now, we can can pass in a U,V value
        // that assumes 0 - 1.0 and the atlas index, and return
        // the U,V into the atlas.
        // - how would we load this atlas and how would we know it's an atlas
        //   in case of celledregion tile painting?
        // - lets say that we parse the nvidia .tai file as a type of texture
        //   extension that we special case and thus we know how to 
        //   load and how to import the filenames into our mod.dbs and such.
        //      - then, this means i think we need a new way to represent this texture
        //      visually when viewing it.  Thus when we are in floor painting mode
        //      we dont show the asset browser, we show a viewer that will show us the seperate
        //      textures in the atlas and allow us to select.
        //          - thus in floor painting mode, there is perhaps a knowledge in the workspace that
        //            says, we must be working with an atlas 



    }
}
