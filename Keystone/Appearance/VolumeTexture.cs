using System;
using Keystone.Resource;
using Keystone.Traversers;
using MTV3D65;

namespace Keystone.Appearance
{
    public class VolumeTexture : Layer
    {

        public VolumeTexture(string id)
            : base(id)
        {
            // TODO: is specular (layer 2) a good layerID for volume textures?
            _layerID = 2; // (range is LAYER_0 to LAYER_3)
        }

//        /// <summary>
//        /// Function to create both the VolumeTexture texture Layer and a child Texture node.
//        /// 
//        /// NOTE: Even though this is a static Create, note that the actual
//        /// VolumeTexture layer is never shared but always created with a unique name.
//        /// Only the underlying Texture is shared if available.
//        /// </summary>
//        /// <param name="resourcePath"></param>
//        public static VolumeTexture Create(string resourcePath)
//        {
//            Texture t = Texture.CreateVolumeTexture(resourcePath);
//            string name = Repository.GetNewName(typeof(Layer).Name);
//            VolumeTexture e = new VolumeTexture(name);
//            e.AddChild(t);
//            return e;
//        }

        #region ITraversable Members
        public override object Traverse(ITraverser target, object data)
        {
            return target.Apply(this, data);
        }
        #endregion 
    }
}