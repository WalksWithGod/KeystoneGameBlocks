using System;
using System.Xml;
using Keystone.Resource;
using Keystone.Traversers;


namespace Keystone.Appearance
{
    public class Emissive : Layer
    {

        public Emissive(string id) : base(id)
        {
            _layerID = 3;
        }

        /// <summary>
        /// Function to create both the Emissive texture Layer and a child Texture node.
        /// 
        /// NOTE: Even though this is a static Create, note that the actual
        /// Emissive layer is never shared but always created with a unique name.
        /// Only the underlying Texture is shared if available.
        /// </summary>
        /// <param name="resourcePath"></param>
        public static Emissive Create(string resourcePath)
        {
            Texture t = Texture.Create(resourcePath);
            string name = Repository.GetNewName(typeof(Layer).Name);
            Emissive e = new Emissive(name);
            e.AddChild(t);
            return e;
        }

        #region ITraversable Members
        public override void Traverse(ITraverser target)
        {
            target.Apply(this);
        }
        #endregion 
    }
}