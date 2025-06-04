using System;
using System.Xml;
using Keystone.Resource;
using Keystone.Traversers;

namespace Keystone.Appearance
{
    public class NormalMap : Layer
    {
        private int _alphaChannelTexture;
        private int _combinedTexture;


        public NormalMap(string id) : base(id)
        {
            _layerID = 1;
        }

        /// <summary>
        /// Function to create both the NormalMap Layer and a child Texture
        /// 
        /// NOTE: Even though this is a static Create, note that the actual
        /// NormalMap layer is never shared but always created with a unique name.
        /// Only the underlying Texture is shared if available.
        /// </summary>
        /// <param name="resourcePath"></param>
        public static NormalMap Create(string resourcePath)
        {
            Texture t = Texture.Create(resourcePath);
            string name = Repository.GetNewName(typeof(Layer).Name);
            NormalMap n = new NormalMap(name);
            n.AddChild(t);
            return n;
        }

        //public static NormalMap Create(int index) // todo: no alphachanneltexture param version of this constructor
        //{
        //    if (Texture.IsInFactory(index))
        //    {
        //        string resourceID = ImportLib.AbsolutePathToRelative (CoreClient._CoreClient.TextureFactory.GetTextureInfo(index).Filename);
        //        NormalMap t = (NormalMap)Repository.Get(resourceID );
        //        if (t != null) return t;

        //        return new NormalMap(index, CoreClient._CoreClient.TextureFactory.GetTextureInfo(index));
        //    }
        //    else throw new Exception("Texture does not exist in factory.");


        //    // this class will store tvfactory indices for all three. noting that the final output
        //    // texture does not exist as a file, only in memory
        //    //_alphaChannelTexture = alphaChannelTexture;
        //    //add an alpha channel to your normal map, this creates a NEW texture (here named, test) 
        //    //_combinedTexture = Core._CoreClient.TextureFactory.AddAlphaChannel(index, _alphaChannelTexture, "newname");


        //    // for for creating a specular bump map for tangent bumpmapping, do the following
        //    //add an alpha channel to your normal map, this creates a NEW texture (here named, test)
        //    //tex.AddAlphaChannel GetTex("norm"), GetTex("spec"), "test" 

        //    // add diffuse+height maps to the mesh (using offset mapping, so heightmap is a requirement) 
        //    // mesh.SetTexture GL.GetTex("floor")
        //    // mesh.SetTextureEx TV_LAYER_HEIGHTMAP, GetTex("norm") 

        //    //now, here's the part that took a while to figure out. You no longer add a normal map to the normal map layer, 
        //    //instead you add the new composite texture (test) to the SPECULAR layer
        //    //mesh.SetTextureEx TV_LAYER_SPECULARMAP, GetTex("test") 

        //    // the reason why applying a specular map over-writes a normal map, is because they both
        //    //reference the same texture layer. This is useful info for anyone writing an editor/custom shaders.
        //    //Also, to use per-pixel lighting and specular mapping without normal mapping, you just need to set 
        //    // the RGB channels of the Specular/Normal map to (0.5f, 0.5f, 1) or (127, 127, 255) which gives a "flat" normal-map.
        //    // NOTE: i believe this is what Lux used... just Specular mapping?
        //}

        //private NormalMap(int index, MTV3D65.TV_TEXTURE textureInfo) : base(index, textureInfo)
        //{
        //    _layerID = 1; //normalmap textures always get added to LAYER_1  (range is LAYER_0 to LAYER_3)
        //    //NOTE: Offset Bumpmapping requires a Layer for HeightMap. 
        //}

        public override void Traverse(ITraverser target)
        {
            target.Apply(this);
        }

        // the alpha channel of the normal map can be used for things like a specular map
        // this cannot be changed once it is done so we provide just a get param.
        // otherwise this must be done in the constructor
        public int AlphaChannelTexture
        {
            get { return _alphaChannelTexture; }
        }
    }
}