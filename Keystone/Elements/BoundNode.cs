using Keystone.Types;
using Keystone.IO;

namespace Keystone.Elements
{
    /// <summary>
    /// Bound Nodes have a bounding volume but no Transform (Such as Geometry).
    /// Bound Nodes are NOT Group nodes and thus do not inherit Group or IGroup.
    /// </summary>
    public abstract class BoundNode : Node, IBoundVolume
    {
        // TODO: the specific cull mode override should be in ModeledEntity.  
        // For instance, a planet, moon, star, etc, should only ever
        // have sphere culling.  There's no need to ever use box culling.

        protected BoundingBox _box;
        protected BoundingSphere _sphere;

        protected BoundNode(string id) : base(id)
        {
        }


        #region IResource Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="specOnly">True returns the properties without any values assigned</param>
        /// <returns></returns>
        public override Settings.PropertySpec[] GetProperties(bool specOnly)
        {
            Settings.PropertySpec[] tmp = base.GetProperties(specOnly);
            Settings.PropertySpec[] properties;

            properties = new Settings.PropertySpec[1 + tmp.Length];
            tmp.CopyTo(properties, 1);

            properties[0] = new Settings.PropertySpec("boundingbox", typeof(BoundingBox).Name);

            if (!specOnly)
            {
                // this value can be null and still be serialized.  OUr serializer will
                // precede BoundingBox with a byte 0=null, 1= not null so it knows whether to
                // procede with the rest
                properties[0].DefaultValue = BoundingBox;
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
                    case "boundingbox":
                        _box = (BoundingBox)properties[i].DefaultValue;
                        break;
                }
            }
        }
        #endregion

        #region IBoundVolume Members
        public virtual BoundingBox BoundingBox // virutal mostly needed for special behavior for Actor3d which uses internal duplicates that must be individually specified
        {
            get
            {
                if (BoundVolumeIsDirty)
                    UpdateBoundVolume();

                return _box;
            }
        }

        public virtual BoundingSphere BoundingSphere// virutal mostly needed for special behavior for Actor3d which uses internal duplicates that must be individually specified
        {
            get
            {
                if (BoundVolumeIsDirty)
                    UpdateBoundVolume();

                return _sphere;
            }
        }


        public virtual bool BoundVolumeIsDirty
        {
            get
            {
                return ((mChangeStates &
                         (Enums.ChangeStates.BoundingBox_TranslatedOnly | 
                         Enums.ChangeStates.BoundingBoxDirty)) != 0);
            }
        }

        protected virtual void UpdateBoundVolume()
        {
            //throw new System.Exception("Not yet implemented.  Need to switch to new tv3d sdk where these methods are added.");
            DisableChangeFlags(Keystone.Enums.ChangeStates.BoundingBox_TranslatedOnly | 
                Keystone.Enums.ChangeStates.BoundingBoxDirty);
        }
        #endregion
    }
}