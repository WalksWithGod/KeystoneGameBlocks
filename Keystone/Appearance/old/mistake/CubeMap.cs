using System;
using Keystone.Resource;
using Keystone.Traversers;

namespace Keystone.Appearance
{
    public class CubeMap : Layer
    {

        public CubeMap(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Function to create both the CubeMap Layer and a child Texture
        /// 
        /// NOTE: Even though this is a static Create, note that the actual
        /// CubeMap layer is never shared but always created with a unique name.
        /// Only the underlying Texture is shared if available.
        /// </summary>
        public static CubeMap Create(string resourcePath)
        {
            Texture t = Texture.Create(resourcePath);
            string name = Repository.GetNewName(typeof(Layer).Name);
            CubeMap d = new CubeMap(name);
            d.AddChild(t);
            return d;
        }

        #region ITraversable Members
        public override void Traverse(ITraverser target)
        {
            target.Apply(this);
        }
        #endregion 
    }
}