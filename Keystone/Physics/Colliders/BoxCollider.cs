using System;
using System.Collections.Generic;
using Keystone.Elements;
using Keystone.Traversers;
using Keystone.Types;

namespace Keystone.Physics.Colliders
{
    public class BoxCollider : Node
    {
        bool mIsTrigger;
        Vector3d mCenter;
        Vector3d mSize;
        public Jitter.Collision.Shapes.Shape Shape;

        internal BoxCollider(string id) : base(id)
        {
            Shareable = false;
            mSize = new Vector3d(2.5d, 0.1d, 2.5d);
            mIsTrigger = true;
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
            Settings.PropertySpec[] properties = new Settings.PropertySpec[3 + tmp.Length];
            tmp.CopyTo(properties, 3);

            properties[0] = new Settings.PropertySpec("trigger", typeof(bool).Name);
            properties[1] = new Settings.PropertySpec("center", typeof(Vector3d).Name);
            properties[2] = new Settings.PropertySpec("size", typeof(Vector3d).Name);


            if (!specOnly)
            {
                properties[0].DefaultValue = mIsTrigger;
                properties[1].DefaultValue = mCenter;
                properties[2].DefaultValue = mSize;
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
                    case "center":
                        mCenter = (Vector3d)properties[i].DefaultValue;
                        break;
                    case "size":
                        mSize = (Vector3d)properties[i].DefaultValue;
                        break;
                }
            }
        }
        #endregion

        public bool Trigger { get { return mIsTrigger; } }

        public Vector3d Center { get { return mCenter; } }

        public Vector3d Size { get { return mSize; } }
    }
}
