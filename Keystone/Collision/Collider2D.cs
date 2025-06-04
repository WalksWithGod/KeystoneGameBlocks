using System;
using System.Collections.Generic;
using Keystone;
using Keystone.Elements;
using Keystone.Traversers;

namespace Keystone.Collision
{
    public class Collider2D : Node
    {
        bool mIsTrigger = false;

        internal Collider2D(string id) : base(id)
        {
            Shareable = true;
        }

        public static Collider2D Create(string id)
        {
            Collider2D node = (Collider2D)Keystone.Resource.Repository.Get(id);
            if (node != null) return node;

            return new Collider2D(id);
        }

        #region Traversable
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
        public override Settings.PropertySpec[] GetProperties(bool specOnly)
        {
            Settings.PropertySpec[] tmp = base.GetProperties(specOnly);
            Settings.PropertySpec[] properties = new Settings.PropertySpec[1 + tmp.Length];
            tmp.CopyTo(properties, 1);

            properties[0] = new Settings.PropertySpec("istrigger", mIsTrigger.GetType().Name);
            //properties[1] = new Settings.PropertySpec("forward", _forward.GetType().Name);
            //properties[2] = new Settings.PropertySpec("loop", _looping.GetType().Name);

            if (!specOnly)
            {
                properties[0].DefaultValue = mIsTrigger;
                //properties[1].DefaultValue = _forward;
                //properties[2].DefaultValue = _looping;
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

                    case "istrigger":
                        mIsTrigger = (bool)properties[i].DefaultValue;
                        break;
                    //case "forward":
                    //    _forward = (bool)properties[i].DefaultValue;
                    //    break;
                    //case "loop":
                    //    _looping = (bool)properties[i].DefaultValue;
                    //    break;
                }
            }
        }
        #endregion

        // TODO: AddChild (Collider3D child) {} // Add this method to Entity
        // TODO: AddChild (Collider2D child) {} // Add this method to Entity
        // TODO: AddChild (ColliderTileMap2D child) {} // Add this method to Entity
    }
}
