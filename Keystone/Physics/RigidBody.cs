using System;
using Keystone.Types;
using System.Collections.Generic;
using Keystone.Elements;
using Keystone.Traversers;

namespace Keystone.Physics
{
    /// <summary> 
    /// /// TODO: I think i was wrong about RigidBody hosting colliders in Unity3d.  Seems only the gameobject hosts both RigidBody and BoxCollider (for instance).
    /// But maybe it's ok for us since collider's need to reference a RigidBody for tvphysics.
    /// </summary>
    public class RigidBody : Group
    {
        bool mIsStatic;
        double mDrag;
        double mMass; // TODO: is "weight" calculated by our Component's Script (along with cost, volume surface area, etc) tied into this?  Should the mass be calculated for us in script, and then assigned to this RigidBody?
        bool mKinematic;
        Vector3d mAngularVelocity;
        Vector3d mLinearVelocity;
        public Jitter.Dynamics.RigidBody Body;
        internal int TVIndex; // for use if we're using TVPhysics instead of Jitter

        // todo: we can add more properties as needed. we need to get to the testing phase of this process of adding Physics .

        internal RigidBody(string id) : base(id)
        {
            Shareable = false;
            mMass = 1d; // default value
        }

        #region ITraverser
        public override object Traverse(ITraverser target, object data)
        {
            throw new NotImplementedException();
        }

        internal override ChildSetter GetChildSetter()
        {
            throw new NotImplementedException();
        }
        #endregion

        // TODO: when the translation of the entity changes for instance,
        //       I think in Simulation.Update() for all active entities with RigidBody nodes
        //       attached, we need to update the physics engine with this information.

        // TODO: add to ChildSetter and other traverser objects
        #region ResourceBase members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="specOnly">True returns the properties without any values assigned</param>
        /// <returns></returns>
        public override Settings.PropertySpec[] GetProperties(bool specOnly)
        {

            Settings.PropertySpec[] tmp = base.GetProperties(specOnly);
            Settings.PropertySpec[] properties = new Settings.PropertySpec[4 + tmp.Length];
            tmp.CopyTo(properties, 4);

            properties[0] = new Settings.PropertySpec("mass", typeof(double).Name);
            properties[1] = new Settings.PropertySpec("static", typeof(bool).Name);
            properties[2] = new Settings.PropertySpec("drag", typeof(double).Name);
            properties[3] = new Settings.PropertySpec("kinematic", typeof(bool).Name);

            if (!specOnly)
            {
                properties[0].DefaultValue = mMass;
                properties[1].DefaultValue = mIsStatic;
                properties[2].DefaultValue = mDrag;
                properties[3].DefaultValue = mKinematic;
            }

            return properties;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// SetProperties does not need to do validation or rules checking because these properties
        /// are not game specific.  These are engine properties predominaly.  Furthermore,
        /// translation, scale and rotation are not modified here for real-time purposes. The physics engine
        /// and simulation engine does that through direct internal accessors.  This is also good for performance.
        /// Thus, change events that need to be raised in script during entity transforms will be done directly from those
        /// accessors.         
        /// </remarks>
        /// <param name="properties"></param>
        public override void SetProperties(Settings.PropertySpec[] properties)
        {

            if (properties == null) return;
            base.SetProperties(properties);

            for (int i = 0; i < properties.Length; i++)
            {
                if (properties[i].DefaultValue == null) continue;
                // use of a switch allows us to pass in all or a few of the propspecs depending
                // on whether we're loading from xml or changing a single property via server directive
                switch (properties[i].Name)
                {
                    case "mass":
                        mMass = (double)properties[i].DefaultValue;
                        if (mMass <= 0) mMass = 1d;
                        break;
                    case "static":
                        mIsStatic = (bool)properties[i].DefaultValue;
                        break;
                    case "drag":
                        mDrag = (double)properties[i].DefaultValue;
                        break;
                    case "kinematic":
                        // TODO:  is kinematic and static same thing?
                        mKinematic = (bool)properties[i].DefaultValue;
                        break;
                }
            }
        }
        #endregion

        public bool Static { get { return mIsStatic; } }

        public double Mass { get { return mMass; } }


    }
}
