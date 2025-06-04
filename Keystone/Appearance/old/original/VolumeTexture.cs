using System;
using Keystone.Resource;
using Keystone.Traversers;
using MTV3D65;

namespace Keystone.Appearance
{
    public class VolumeTexture : Layer
    {
        //public static VolumeTexture Create(string id, string relativeFilePath, CONST_TV_COLORKEY colorKey,
        //                                   bool textureFilter)
        //{
        //    int index =
        //        CoreClient._CoreClient.TextureFactory.LoadVolumeTexture(relativeFilePath, id, colorKey, textureFilter);
        //    return Create(index);
        //}

        //internal VolumeTexture(string id) : base(id)
        //{
        //}

        //public static VolumeTexture Create(int index)
        //{
        //    if (Texture.IsInFactory(index))
        //    {
        //        string resourceID = ImportLib.AbsolutePathToRelative(CoreClient._CoreClient.TextureFactory.GetTextureInfo(index).Filename);
        //        VolumeTexture t = (VolumeTexture)Repository.Get(resourceID );
        //        if (t != null) return t;

        //        return new VolumeTexture(index, CoreClient._CoreClient.TextureFactory.GetTextureInfo(index));
        //    }
        //    else throw new Exception("Texture does not exist in factory.");
        //}

        public VolumeTexture(string id)
            : base(id)
        {
            _layerID = 1; //normalmap textures always get added to LAYER_1  (range is LAYER_0 to LAYER_3)
            //NOTE: Offset Bumpmapping requires a Layer for HeightMap. 
        }

        #region ITraversable Members
        public override void Traverse(ITraverser target)
        {
            target.Apply(this);
        }
        #endregion 
    }
}