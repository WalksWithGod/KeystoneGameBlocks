using System;
using Keystone.Resource;
using Keystone.Traversers;

namespace Keystone.Appearance
{
    public class DUDVTexture : Layer
    {
        //public static DUDVTexture Create(string id, string relativeFilePath, int width, int height, float amplitude,
        //                                 bool mipMapDecrease)
        //{
        //    // TODO: commented out the follow just to make sure the code works... that is,
        //    // texturefactory does store the indexes for DUDV type and thus
        //    // we can use the GetTextureInfo() in the subsequent Create call
        //    //DUDVTexture t = (DUDVTexture)Repository.Get(id);
        //    //if (t != null) return t;

        //    int index = CoreClient._CoreClient.TextureFactory.LoadDUDVTexture(relativeFilePath, id, width, width, amplitude, false);
        //    //return new DUDVTexture(index, Core._CoreClient.TextureFactory.GetTextureInfo(index));
        //    return Create(index);
        //}

        //public static DUDVTexture Create(int index)
        //{
        //    if (Texture.IsInFactory(index))
        //    {
        //        string resourceID = ImportLib.AbsolutePathToRelative(CoreClient._CoreClient.TextureFactory.GetTextureInfo(index).Filename);
        //        DUDVTexture t = (DUDVTexture)Repository.Get(resourceID );
        //        if (t != null) return t;

        //        return new DUDVTexture(index, CoreClient._CoreClient.TextureFactory.GetTextureInfo(index));
        //    }
        //    else throw new Exception("Texture does not exist in factory.");
        //}

        public DUDVTexture(string id)
            : base(id)
        {
            _layerID = 1; // what later to use for DUDV? It's a type of bumpmap isn't it?
        }

        #region ITraversable Members
        public override object Traverse(ITraverser target, object data)
        {
            return target.Apply(this, data);
        }
        #endregion 
    }
}