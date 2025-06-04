using System;
using System.Collections.Generic;
using System.Diagnostics;
using Core.Elements;
using Core.Entities;
using Core.Traversers;
using Core.Types;

namespace Celestial
{
    public class BodyPositioner : ITraverser
    {
        Stack<Body> _stars = new Stack<Body>();
        Stack<System> _systems = new Stack<System>();
        Stack<float> _diameters = new Stack<float>();
        Random _rand;
        
        public BodyPositioner (Random rand)
        {
            if (rand == null) throw new ArgumentNullException();
            _rand = rand;
        }
        
        #region IEntityTraverser Members
        public void Apply(Core.Entities.EntityBase entity)
        {
            
        }



        public void Apply (System system)
        {
            // get the diameter of the system from top to bottom.  Subsystem and child star diameters must be less than or equal to 1/4 the parent system
            float diameter = GetOrbitalSeperationAU((uint)_systems.Count - 1) * 2;
            if (_systems.Count > 0 )
            {
                while (diameter > _systems.Peek ().Diameter * .25)
                    diameter = GetOrbitalSeperationAU((uint)_systems.Count - 1) * 2;

                system.Diameter = diameter;
            }
            
            _systems.Push(system);

            // compute all the child subsystem's positions and seperations first
            foreach (Body body in system.Children)
            {
                if (body is System)
                {
                    Apply((System)body);
                    // now position any child systems.  if there was a child star already, it's position will be set and we can
                    // place the child system opposite it.  If not, we just place it like we would a child star
                    Star[] childstars = system.Stars;
                    if (childstars != null && childstars.Length > 0)
                    {
                        body.Translation = childstars[0].Translation * -1; // todo: CRAP.  I think i need to include it's parent's position offset 
                        body.OrbitalEccentricity = 0f;               //              position = childsars[0].Translation * -1 + _systems.Peek().Translation; ?
                        body.OrbitalRadius = childstars[0].OrbitalRadius;
                        //todo: min seperation! ack   
                    }
                    else
                    {
                        body.Translation = Vector3d.RandomVector(_rand) * _systems.Peek().Diameter * .5f;
                    }
                }   
            }
            
            
            uint starCount = system.StarCount;
            if (starCount > 0)
            {
                Star[] childstars = system.Stars;

                foreach (Star star in childstars )
                {
                    // apply same orbital radius to all children
                    star.OrbitalRadius = (float)_systems.Peek().Diameter * .5f;
                    star.OrbitalEccentricity = 0;
                }
                // pick a random point at that distance for the first child star that is at half the diameter of the parent system
                childstars[0].Translation = Vector3d.RandomVector(_rand) * _systems.Peek().Diameter * .5f;
                
                // if there are more than one children, place them at opposite ends
                if (starCount > 1)
                {
                    childstars[1].Translation = childstars[1].Translation * -1;
                    Debug.Assert(starCount == 2);
                }
            } 
            _systems.Pop();
        }
        

        private float GetOrbitalSeperationAU(uint depth)
        {
            // this function determines how far from the center of the system a star is.
            // if the star is the 2nd companion star in a Trinary star system,
            float x = (float)_rand.NextDouble();
            float y;

            // each depth down into the system hierarchy will have a smaller and smaller seperation
            float depthModifer = 0.3333f * depth;
            x -= depthModifer;
            
            if (x <= 0.3333)
                y = 0.025f;
            else if (x <= 0.4999)
                y = 0.25f;
            else if (x <= 0.611)
                y = 1;
            else if (x <= 0.7776)
                y = 5;
            else
                y = 25;

            // multiply this value by a random values between 2 and 12
            y *= _rand.Next(2, 12);
            // make this value either positive or minus value
            if (_rand.NextDouble() < 0.5)
                y *= -1;

            return y;
        }
        #endregion

        #region ITraverser Members

        public void Apply(Core.Elements.Node scene)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Apply(SceneNode sn)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Apply(OctreeHost host)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public void Apply(Core.Octree.OctreeNode o)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public void Apply(EntityNode en)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Apply(RegionNode rn)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Apply(Core.Portals.Region sector)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public void Apply(ModeledEntity entity)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public void Apply(Core.Portals.Portal p)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Apply(Core.Elements.Terrain terrain)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Apply(Core.Elements.ModelBase model)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Apply(Core.Elements.LODSwitch lod)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Apply(Core.Elements.Geometry element)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Apply(Core.Lights.DirectionalLight directionalLight)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
