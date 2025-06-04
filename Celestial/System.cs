using System;
using System.Diagnostics;
using Core.Entities;

namespace Celestial
{
    // IMPORTANT: The following explains why a "System" is not a Region or directly inherits from it.
    // A system's center should always be at 0,0,0 local coordinates and any world position is computed via the traversal's matrix stack.
    // But this is typically only needed for cross system communication because we want to run our simulation within zones in their local
    // spaces so that our double precision is sufficient.
    // A system has either child stars or child sub-systems but those children all orbit the center of the system.
    public class System : Body, ISystem 
    {
        public System(string name) : base(name) { }
        
        public override void Traverse(Core.Traversers.ITraverser target)
        {
            throw new Exception("The method or operation is not implemented.");
        
        }
        public void AddChild(Star star)
        {
            if (StarCount > 3) throw new ArgumentOutOfRangeException("No more than 3 stars allowed in a subsystem.");
            AddChild((Body)star);
        }

        public void AddChild(System subsystem)
        {
            if (SubSystemCount > 2) throw new ArgumentOutOfRangeException("No more than 2 sub-systems allowed in a system.");
            AddChild((Body)subsystem);
        }

        public void AddChild (Planet planet)
        {
            AddChild((Body) planet);
        }
        
        public uint SubSystemCount
        {
            get
            {
                if (_children == null) return 0;

                uint count = 0;
                foreach (EntityBase child in _children)
                    if (child is System) count++;

                return count;
            }
        }
        
        public uint StarCount
        {
            get
            {
                if (_children == null) return 0;

                uint count = 0;
                foreach (EntityBase child in _children)
                    if (child is Star) count++;

                return count;
            }
        }
        
        public Star[] Stars
        {
            get
            {
                Star[] stars = new Star[StarCount];
                uint count = 0;
                foreach (EntityBase child in _children)
                {
                    if (child is Star)
                    {
                        stars[count] = (Star)child;
                        count++;
                    }
                }
                return stars;
            }
        }
        public System[] Systems
        {
            get
            {
                System[] systems = new System[SubSystemCount];
                uint count = 0;
                foreach (EntityBase child in _children)
                {
                    if (child is System)
                    {
                        systems[count] = (System)child;
                        count++;
                    }
                }
                return systems;
            }
        }
    }
}
