using System;
namespace Keystone.Shaders
{
    public class LightSpaceMap : Shader
    {
        //public static LightSpaceMap Create (string filepath, string[] defines)
           
        //{
        //    throw new NotImplementedException();
        //}

        protected LightSpaceMap(string id, string resourcePath)
            : base(id)
        {
        }

        public override object Traverse(Traversers.ITraverser target, object data)
        {
            throw new System.Exception("The method or operation is not implemented.");
        }

        internal override Traversers.ChildSetter GetChildSetter()
        {
            throw new System.Exception("The method or operation is not implemented.");
        }
    }
}