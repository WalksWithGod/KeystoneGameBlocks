using System.Diagnostics;
using Keystone.Elements;
using Keystone.Entities;
using Keystone.IO;
using Keystone.Portals;
using Keystone.Traversers;
using KeyCommon.Flags;
using System;

namespace Keystone.Celestial
{
    // abstract base class that handles common functionality for all celestial bodys
    public abstract class Body : ModeledEntity
    {
    	protected int mSeed; // body can be reproduced from seed, type, and parent body/system stats.
    	
        // basic celestial body physical properties
        protected float _density;
        protected float _diameter; // stars can be up to 1 trillion meters. 
        protected double _mass;
        protected float _surfaceGravity;

        // orbit info
        protected Orbit mOrbit;  // TODO: maybe i should switch to using an orbit obj here?  It could contain extra data for pathing of the orbit?
        protected double _orbitalRadius;
        protected float _orbitalEccentricity; // perfectly circular is 0
        protected float _orbitalPeriod;
        protected float _orbitalInclination;
        protected float _orbitalProcession;
        protected double _orbitalEpoch;
        protected float _orbitalVelocity;
        

        // axial tile and rate of rotation
        protected float _axialTilt = 0f;
        protected float _axialRotationRate = 0f; // either degrees per second or perhaps a period with duration

        public Body(string id) : base(id)
        {
            // Body's move in elipitcal orbits using an eliptical animation.
            // Thus they have Dynamic = false.
            // however, they can respond to collision.  
            SetEntityAttributesValue((uint)EntityAttributes.CollisionEnabled, true);
            SetEntityAttributesValue((uint)EntityAttributes.Dynamic, false); 
            SetEntityAttributesValue ((uint)EntityAttributes.LargeFrustum , true);
        }

        #region ITraversable Members
        internal override Keystone.Traversers.ChildSetter GetChildSetter()
        {
            throw new NotImplementedException();
        }
        #endregion 

        #region ResourceBase members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="specOnly">True returns the properties without any values assigned</param>
        /// <returns></returns>
        public override Settings.PropertySpec[] GetProperties(bool specOnly)
        {
            Settings.PropertySpec[] tmp = base.GetProperties(specOnly);
            Settings.PropertySpec[] properties = new Settings.PropertySpec[10 + tmp.Length];
            tmp.CopyTo(properties, 10);

            properties[0] = new Settings.PropertySpec("density", _density.GetType().Name);
            properties[1] = new Settings.PropertySpec("diameter", _diameter.GetType().Name);
            properties[2] = new Settings.PropertySpec("mass", _mass.GetType().Name);
            properties[3] = new Settings.PropertySpec("surfacegravity", _surfaceGravity.GetType().Name);
            properties[4] = new Settings.PropertySpec("orbitalradius", _orbitalRadius.GetType().Name);
            properties[5] = new Settings.PropertySpec("orbitaleccentricity", _orbitalEccentricity.GetType().Name);
            properties[6] = new Settings.PropertySpec("orbitalperiod", _orbitalPeriod.GetType().Name);
            properties[7] = new Settings.PropertySpec("orbitalinclination", _orbitalInclination.GetType().Name);
            properties[8] = new Settings.PropertySpec("orbitalprocession", _orbitalProcession.GetType().Name);
            properties[9] = new Settings.PropertySpec("orbitalepoch", _orbitalEpoch.GetType().Name);
			// TODO: surface radiation
			// TODO: escape velocity
			
            if (!specOnly)
            {
                properties[0].DefaultValue = _density;
                properties[1].DefaultValue = _diameter;
                properties[2].DefaultValue = _mass;
                properties[3].DefaultValue = _surfaceGravity;
                properties[4].DefaultValue = _orbitalRadius;
                properties[5].DefaultValue = _orbitalEccentricity;
                properties[6].DefaultValue = _orbitalPeriod;
                properties[7].DefaultValue = _orbitalInclination;
                properties[8].DefaultValue = _orbitalProcession;
                properties[9].DefaultValue = _orbitalEpoch;
            }

            return properties;
        }

        public override void SetProperties(Settings.PropertySpec[] properties)
        {
            if (properties == null) return;
            base.SetProperties(properties);

            for (int i = 0; i < properties.Length; i++)
            {
                // use of a switch allows us to pass in all or a few of the propspecs depending
                // on whether we're loading from xml or changing a single property via server directive
                switch (properties[i].Name)
                {
                    case "density":
                        _density = (float)properties[i].DefaultValue;
                        break;
                    case "diameter":
                        // note: use Setter to force update of visibileDistanceSq
                        Diameter = (float)properties[i].DefaultValue;
                        break;
                    case "mass":
                        _mass = (double)properties[i].DefaultValue;
                        break;
                    case "surfacegravity":
                        _surfaceGravity = (float)properties[i].DefaultValue;
                        break;
                    case "orbitalradius":
                        _orbitalRadius = (double)properties[i].DefaultValue;
                        break;
                    case "orbitaleccentricity":
                        _orbitalEccentricity = (float)properties[i].DefaultValue;
                        break;
                    case "orbitalperiod":
                        _orbitalPeriod = (float)properties[i].DefaultValue;
                        break;
                    case "orbitalinclination":
                        _orbitalInclination = (float)properties[i].DefaultValue;
                        break;
                    case "orbitalprocession":
                        _orbitalProcession = (float)properties[i].DefaultValue;
                        break;
                    case "orbitalepoch":
                        _orbitalEpoch = (double)properties[i].DefaultValue;
                        break;
                }
            }
        }
        #endregion

        public float Density
        {
            get { return _density; }
            set { _density = value; }
        }

        public float Diameter
        {
            get { return _diameter; }
            set
            {
                _diameter = value;
                SetChangeFlags(Keystone.Enums.ChangeStates.BoundingBoxDirty, Keystone.Enums.ChangeSource.Self);

            }
        }

        public float Radius { get { return _diameter * .5f; } }

        /// <summary>
        /// Mass in kilograms.
        /// </summary>
        public double MassKg
        {
            get { return _mass; }
            set { _mass = value; }
        }

        public float SurfaceGravity
        {
            get { return _surfaceGravity; }
            set { _surfaceGravity = value; }
        }

        public double OrbitalRadius
        {
            get { return _orbitalRadius; }
            set { _orbitalRadius = value; }
        }

        /// <summary>
        /// Perfectly circular == 0.  
        /// 1 == parabolic. https://en.wikipedia.org/wiki/Parabolic_trajectory
        /// >1 == hyperbolic. https://en.wikipedia.org/wiki/Hyperbolic_trajectory
        /// </summary>
        public float OrbitalEccentricity
        {
            get { return _orbitalEccentricity; }
            set { _orbitalEccentricity = value; }
        }

        public float OrbitalPeriod
        {
            get { return _orbitalPeriod; }
            set { _orbitalPeriod = value; }
        }

        public float OrbitalInclination
        {
            get { return _orbitalInclination; }
            set { _orbitalInclination = value; }
        }

        public float OrbitalProcession
        {
            get { return _orbitalProcession; }
            set { _orbitalProcession = value; }
        }

        public double OrbitalEpoch // in seconds
        {
            get { return _orbitalEpoch; }
            set { _orbitalEpoch = value; }
        }

        public float OrbitalVelocity
        {
            get { return _orbitalVelocity; }
            set { _orbitalVelocity = value; }
        }

        public float AxialTilt
        {
            get { return _axialTilt; }
            set { _axialTilt = value; }
        }

        public float AxialRotationRate // in seconds
        {
            get { return _axialRotationRate; }
            set { _axialRotationRate = value; }
        }

        // TODO: all entity name's must be unique and I dont htink we're enforcing that anywhere.
        // in our scenegraph it's enforced via a central node repository
        public string GetFreeChildName()
        {
            int count = 0;
            string newname = mFriendlyName;

            if (mChildren != null) 
            	count = mChildren.Count;

            for (int i = 0; i < count; i++)
            {
            	if (i > 0)
	           		newname = mFriendlyName + " " + i.ToString();
    			else
    				newname = mFriendlyName;
    			
            	if (!ContainsChildNamed(newname))
                    return newname;
            }

            return newname;
        }

        private bool ContainsChildNamed(string name)
        {
            // use Node instead of Body because a body can contain a ModelBase as well as other Body's in orbit
            foreach (Node child in mChildren)
            {
                if (child.Name == name) return true;
            }
            return false;
        }

        #region IEntityGroup Members
        public void AddChild(Body child)
        {
            base.AddChild(child);

            // the mass of any sub-system or star or any other body get's added to the system's overall mass.  
            _mass += child.MassKg;
        }
        

        public override void RemoveChild(Node child)
        {
            try
            {
                base.RemoveChild(child);
                // subtract combined mass of children
                if (child is Body) _mass -= ((Body) child).MassKg;
            }
            catch (Exception ex)
            {
            	System.Diagnostics.Debug.WriteLine ("Body.RemoveChild() - " + ex.Message);
            }
        }
        #endregion
    }
}