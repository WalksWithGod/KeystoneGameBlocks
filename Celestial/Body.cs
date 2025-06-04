using System;
using System.Collections.Generic;
using System.Diagnostics;
using Core.Elements;
using Core.Entities;

namespace Celestial
{
    
    // abstract base class that handles common functionality for all celestial bodys
    public abstract class Body : EntityBase, IGroup
    {
        protected float _density;
        protected float _diameter;
        protected float _mass;
        protected float _surfaceGravity;
        protected float _orbitalRadius;
        protected float _orbitalEccentricity;
        protected float _orbitalPeriod;
        protected float _orbitalvelocity;
        
        //protected Path _path;  // must always contain relative coordinates and each frame, the actual position is multipled by parent matrix
        public Body(string name) : base(name) { }
        
        public float Density { get { return _density; } set { _density = value; } }
        public float Diameter { get { return _diameter; } set { _diameter = value; } }

        public float Mass { get { return _mass; } set { _mass = value; } }
        public float SurfaceGravity { get { return _surfaceGravity; } set { _surfaceGravity = value; } }
        public float OrbitalRadius { get { return _orbitalRadius; } set { _orbitalRadius = value; } }
        public float OrbitalEccentricity { get { return _orbitalEccentricity; } set { _orbitalEccentricity = value; } }
        public float OrbitalPeriod { get { return _orbitalPeriod; } set { _orbitalPeriod = value; } }
        public float OrbitalVelocity { get { return _orbitalvelocity; } set { _orbitalvelocity = value; } }

        public override void AddChild(EntityBase child)
        {
            base.AddChild (child);

            // the mass of any sub-system or star or any other body get's added to the system's overall mass.  
            _mass += ((Body)child).Mass;
        }
        
        // todo: all entity name's must be unique and I dont htink we're enforcing that anywhere.
        // in our scenegraph it's enforced via a central node repository
        public string GetFreeChildName()
        {
            int count = 0;
            string newname = "";
            if (_children != null && _children.Count > 0) count = _children.Count;
            
            for (int i = 0; i < count; i++)
            {
                newname = _name + " " + i.ToString();
                if (!ContainsChildNamed(newname))
                    return newname;
            }

            return _name + " " + count.ToString();
        }

        private bool ContainsChildNamed( string name)
        {
            foreach (Body child in _children )
            {
                if (child.Name == name) return true;
            }
            return false;
        }
        
        #region IEntityGroup Members
        public void NotifyChange(Node child)
        {
            //based on the type of child that is notifying us, determine
            // if we need to (for instance) to set relevant flags so that 
            // on traversal,etc any lazy updates can occur

        }

        public override void RemoveChild(Node child)
        {
            try
            {
                if (child is EntityGroup)
                    ((EntityGroup)child).RemoveChildren();

                _children.Remove(child);
                child.RemoveParent(this);
                _mass -= ((Body)child).Mass;
                if (_children.Count == 0)
                {
                    _children = null;
                    Debug.Assert(_mass == 0);
                }
            }
            catch { }
        }
        #endregion

        #region IGroup Members

        #endregion
    }
}