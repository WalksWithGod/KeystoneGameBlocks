using System.Diagnostics;

namespace Celestial
{
    public class Planet : World
    {
        public Planet (string name) : base(name)
        {
            
        }

        
        public override void Traverse(Core.Traversers.ITraverser target)
        {
            throw new global::System.Exception("The method or operation is not implemented.");
        }
    }
}
