using System;
using System.Collections.Generic;
using Keystone.Elements;
using Keystone.Traversers;
using Keystone.Types;

namespace Keystone.Physics.Colliders
{
    public class CapsuleCollider : Node
    {
        bool mIsTrigger;
        double mRadius;
        double mHeight;
        Vector3d mCenter;
        Vector3d mDirection;
        public Jitter.Collision.Shapes.Shape Shape;

        internal CapsuleCollider(string id) : base(id)
        {
            Shareable = false;
            mRadius = 1d;
            mHeight = 2d;
            mIsTrigger = false;
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

        #region ResourceBase members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="specOnly">True returns the properties without any values assigned</param>
        /// <returns></returns>
        public override Settings.PropertySpec[] GetProperties(bool specOnly)
        {

            Settings.PropertySpec[] tmp = base.GetProperties(specOnly);
            Settings.PropertySpec[] properties = new Settings.PropertySpec[5 + tmp.Length];
            tmp.CopyTo(properties, 5);

            properties[0] = new Settings.PropertySpec("trigger", typeof(bool).Name);
            properties[1] = new Settings.PropertySpec("radius", typeof(double).Name);
            properties[2] = new Settings.PropertySpec("height", typeof(double).Name);
            properties[3] = new Settings.PropertySpec("center", typeof(Vector3d).Name);
            properties[4] = new Settings.PropertySpec("direction", typeof(Vector3d).Name);


            if (!specOnly)
            {
                properties[0].DefaultValue = mIsTrigger;
                properties[1].DefaultValue = mRadius;
                properties[2].DefaultValue = mHeight;
                properties[3].DefaultValue = mCenter;
                properties[4].DefaultValue = mDirection;
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
                    case "trigger":
                        mIsTrigger = (bool)properties[i].DefaultValue;
                        break;
                    case "radius":
                        mRadius = (double)properties[i].DefaultValue;
                        break;
                    case "height":
                        mHeight = (double)properties[i].DefaultValue;
                        break;
                    case "center":
                        mCenter  = (Vector3d)properties[i].DefaultValue;
                        break;
                    case "direction":
                        mDirection = (Vector3d)properties[i].DefaultValue;
                        break;
                }
            }
        }
        #endregion

        public bool Trigger { get { return mIsTrigger; } }

        public Vector3d Center { get { return mCenter; } }

        public Vector3d Direction { get { return mDirection; } }

        public double Radius { get { return mRadius; } }

        public double Height { get { return mHeight; } }
    }
}
