using System;

namespace Celestial
{
    // a world is any habitable body be it an asteroid, moon, or terrestial planet
    public class World : Body 
    {
        
        public World (string name) : base(name){}

        Biosphere _biosphere;
        WorldType _worldType;
        
        
        byte ZoneType; // this is mostly just for generation data.  Zone type is for determining habital or not zone
        byte Size ;

        
        public void AddChild(Moon planet)
        {
            AddChild((Body)planet);
        }
        
        public Biosphere Biosphere { get { return _biosphere; } set { _biosphere = value; } }
        public WorldType WorldType { get { return _worldType; } set { _worldType = value; } }

        public override void Traverse(Core.Traversers.ITraverser target)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
