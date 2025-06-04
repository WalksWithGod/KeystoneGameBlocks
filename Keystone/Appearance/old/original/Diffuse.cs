using System;
using System.Xml;
using Keystone.Resource;
using Keystone.Traversers;

namespace Keystone.Appearance
{

    public class Diffuse : Layer
    {

        public Diffuse(string id) : base(id)
        {
            _layerID = 0;
        }

        /// <summary>
        /// Function to create both the Diffuse Layer and a child Texture
        /// 
        /// NOTE: Even though this is a static Create, note that the actual
        /// Diffuse layer is never shared but always created with a unique name.
        /// Only the underlying Texture is shared if available.
        /// </summary>
        /// <param name="resourcePath"></param>
        public static Diffuse Create (string resourcePath)
        {
            Texture t = Texture.Create (resourcePath );
            string name = Repository.GetNewName  (typeof(Layer).Name);
            Diffuse d = new Diffuse (name);
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