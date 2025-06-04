using System;
using System.Xml;
using Keystone.Resource;
using Keystone.Traversers;

namespace Keystone.Appearance
{
    public class Specular : Layer
    {
        //public static Specular Create( string resourcePath)
        //{
        //    Specular t = (Specular)Repository.Get(resourcePath);
        //    if (t != null) return t;

        //    t = new Specular(resourcePath);
        //    t._id = resourcePath;
        //    return t;
        //}

        //public static Specular Create(int index)
        //{
        //    if (Texture.IsInFactory(index))
        //    {
        //        string resourceID = ImportLib.AbsolutePathToRelative(CoreClient._CoreClient.TextureFactory.GetTextureInfo(index).Filename);
        //        Specular t = (Specular)Repository.Get(resourceID);
        //        if (t != null) return t;

        //        return new Specular(index, CoreClient._CoreClient.TextureFactory.GetTextureInfo(index));
        //    }
        //    else throw new Exception("Texture does not exist in factory.");
        //}

        public Specular(string id) : base(id)
        {
            _layerID = 2;
        }


        //private Specular(int index, MTV3D65.TV_TEXTURE textureInfo) : base(index, textureInfo)
        //{
        //    _layerID = 2; //specular textures always get added to LAYER_2  (range is LAYER_0 to LAYER_3)
        //}

        /// <summary>
        /// temp function to create both the Diffuse Layer and a child Texture
        /// </summary>
        /// <param name="resourcePath"></param>
        public static Specular Create(string resourcePath)
        {
            Texture t = Texture.Create(resourcePath);
            string name = Repository.GetNewName(typeof(Layer).Name);
            Specular s = new Specular(name);
            s.AddChild(t);
            return s;
        }

        #region ITraversable Members
        public override void Traverse(ITraverser target)
        {
            target.Apply(this);
        }
        #endregion 
    }
}