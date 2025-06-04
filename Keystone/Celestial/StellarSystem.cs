using System;
using System.Xml;
using Keystone.Entities;
using Keystone.IO;
using Keystone.Resource;
using Keystone.Traversers;

namespace Keystone.Celestial
{
    // IMPORTANT: The following explains why a "System" is not a Region or directly inherits from it.
    // A system's center should always be at 0,0,0 local coordinates and any world position is computed
    // via the traversal's matrix stack.
    // But this is typically only needed for cross system communication because we want to run our 
    // simulation within zones in their local spaces so that our double precision is sufficient.
    // A system has either child stars or child sub-systems but those children all orbit the center 
    // of the system.
    public class StellarSystem : Body, ISystem
    {
        public StellarSystem(string id) : base(id)
        {
        }


        #region ITraversable Members
        public override object Traverse(Traversers.ITraverser target, object data)
        {
            return target.Apply(this, data);
        }
        #endregion

        internal override Keystone.Traversers.ChildSetter GetChildSetter()
        {
            throw new NotImplementedException();
        }

        #region IEntityGroup Members
        public void AddChild(Star star)
        {
            if (StarCount > 3) throw new ArgumentOutOfRangeException("No more than 3 stars allowed in a subsystem.");
            AddChild((Body) star);

            // the diameter of an entire system is the max extents of all bodies in all orbits from sub-systems to moons and asteroid belts
            // TODO: the temp hack way for now tho is to just add diamter 
            _diameter += star.Diameter;
        }

        public void AddChild(StellarSystem subsystem)
        {
            if (SubSystemCount > 2)
                throw new ArgumentOutOfRangeException("No more than 2 sub-systems allowed in a system.");
            AddChild((Body) subsystem);
            _diameter += subsystem.Diameter;
        }

        public void AddChild(World world)
        {
            AddChild((Body)world);
            _diameter += world.Diameter;
        }
        #endregion

        public Star Primary 
        {
            get 
            {
                if (mChildren == null || mChildren.Count == 0) return null;

                // first child if a star is primary.  if first child is a subsystem, then the first child of that is primary
                if (mChildren[0] is Star)
                    return (Star)mChildren[0];
                else
                    return (Star)((StellarSystem)mChildren[0]).mChildren[0];
            }
        }

        public uint SubSystemCount
        {
            get
            {
                if (mChildren == null) return 0;

                uint count = 0;
                foreach (Entity child in mChildren)
                    if (child is StellarSystem) count++;

                return count;
            }
        }

        /// <summary>
        /// Returns number of direct star children, not stars in sub-systems
        /// </summary>
        public uint StarCount
        {
            get
            {
                if (mChildren == null) return 0;

                uint count = 0;
                foreach (Entity child in mChildren)
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
                foreach (Entity child in mChildren)
                {
                    if (child is Star)
                    {
                        stars[count] = (Star) child;
                        count++;
                    }
                }
                return stars;
            }
        }

        public StellarSystem[] Systems
        {
            get
            {
                StellarSystem[] systems = new StellarSystem[SubSystemCount];
                uint count = 0;
                foreach (Entity child in mChildren)
                {
                    if (child is StellarSystem)
                    {
                        systems[count] = (StellarSystem) child;
                        count++;
                    }
                }
                return systems;
            }
        }
    }
}