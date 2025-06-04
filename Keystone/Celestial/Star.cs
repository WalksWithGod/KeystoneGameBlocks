using System;
using System.Xml;
using Keystone.Elements;
using Keystone.IO;
using Keystone.Resource;
using Keystone.Types;

namespace Keystone.Celestial
{
    public class Star : Body
    {
        private LUMINOSITY _luminosityClass;
        private SPECTRAL_TYPE _spectralType;
        private SPECTRAL_SUB_TYPE _spectralSubType;

        private int _temperature;
        private float _luminosity;
        private float _age;
        private float _stableLifespan;
        private float _innerLimitDistance;
        private float _outerLimitDistance;
        private float _lifeZoneInnerEdge;
        private float _lifeZoneOuterEdge;
        private float _snowLine;

        public Star(string id)
            : base(id)
        {
        }


        #region ITraversable Members
        public override object Traverse(Traversers.ITraverser target, object data)
        {
            return target.Apply(this, data);
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
            Settings.PropertySpec[] properties = new Settings.PropertySpec[12 + tmp.Length];
            tmp.CopyTo(properties, 12);

            properties[0] = new Settings.PropertySpec("luminosityclass", typeof(int).Name);
            properties[1] = new Settings.PropertySpec("spectraltype", typeof(int).Name);
            properties[2] = new Settings.PropertySpec("spectralsubtype", typeof(int).Name);
            properties[3] = new Settings.PropertySpec ("luminosity", typeof (float).Name);
            properties[4] = new Settings.PropertySpec("temperature", _temperature.GetType().Name);
            properties[5] = new Settings.PropertySpec("age", _age.GetType().Name);
            properties[6] = new Settings.PropertySpec("stablelifespan", _stableLifespan.GetType().Name);
            properties[7] = new Settings.PropertySpec("innerlimitdistance", _innerLimitDistance.GetType().Name);
            properties[8] = new Settings.PropertySpec("outerlimitdistance", _outerLimitDistance.GetType().Name);
            properties[9] = new Settings.PropertySpec("lifezoneinneredge", _lifeZoneInnerEdge.GetType().Name);
            properties[10] = new Settings.PropertySpec("lifezoneouteredge", _lifeZoneOuterEdge.GetType().Name);
            properties[11] = new Settings.PropertySpec("snowline", _snowLine.GetType().Name);

            if (!specOnly)
            {
                properties[0].DefaultValue = (int)_luminosityClass;
                properties[1].DefaultValue = (int)_spectralType;
                properties[2].DefaultValue = (int)_spectralSubType;
                properties[3].DefaultValue = _luminosity;
                properties[4].DefaultValue = _temperature;
                properties[5].DefaultValue = _age;
                properties[6].DefaultValue = _stableLifespan;
                properties[7].DefaultValue = _innerLimitDistance;
                properties[8].DefaultValue = _outerLimitDistance;
                properties[9].DefaultValue = _lifeZoneInnerEdge;
                properties[10].DefaultValue = _lifeZoneOuterEdge;
                properties[11].DefaultValue = _snowLine;
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
                    case "luminosityclass":
                        _luminosityClass = (LUMINOSITY)((int)properties[i].DefaultValue);
                        break;
                    case "spectraltype":
                        _spectralType = (SPECTRAL_TYPE)((int)properties[i].DefaultValue);
                        break;
                    case "spectralsubtype":
                        _spectralSubType = (SPECTRAL_SUB_TYPE)((int)properties[i].DefaultValue);
                        break;
                   case "luminosity":
                        _luminosity = (float)properties[i].DefaultValue;
                        break;
                    case "temperature":
                        _temperature = (int)properties[i].DefaultValue;
                        break;
                    case "age":
                        _age = (float)properties[i].DefaultValue;
                        break;
                    case "stablelifespan":
                        _stableLifespan = (float)properties[i].DefaultValue;
                        break;

                    case "innerlimitdistance":
                        _innerLimitDistance = (float)properties[i].DefaultValue;
                        break;
                    case "outerlimitdistance":
                        _outerLimitDistance = (float)properties[i].DefaultValue;
                        break;
                    case "lifezoneinneredge":
                        _lifeZoneInnerEdge = (float)properties[i].DefaultValue;
                        break;
                    case "lifezoneouteredge":
                        _lifeZoneOuterEdge = (float)properties[i].DefaultValue;
                        break;
                    case "snowline":
                        _snowLine =(float)properties[i].DefaultValue;
                        break;
                }
            }
        }
        #endregion
        
        public LUMINOSITY LuminosityClass
        {
            get { return _luminosityClass; }
            set { _luminosityClass = value; }
        }

        public SPECTRAL_TYPE SpectralType
        {
            get { return _spectralType; }
            set { _spectralType = value; }
        }

        public SPECTRAL_SUB_TYPE SpectralSubType
        {
            get { return _spectralSubType; }
            set { _spectralSubType = value; }
        }

        public int Temperature
        {
            get { return _temperature; }
            set { _temperature = value; }
        }

        public float Luminosity
        {
            get { return _luminosity; }
            set { _luminosity = value; }
        }

        public float Age
        {
            get { return _age; }
            set { _age = value; }
        }

        public float StableLifespan
        {
            get { return _stableLifespan; }
            set { _stableLifespan = value; }
        }

        // TODO: these may not be needed necessarily.  Usually are just for generation.
        public float InnerLimitDistance
        {
            get { return _innerLimitDistance; }
            set { _innerLimitDistance = value; }
        }

        public float OuterLimitDistance
        {
            get { return _outerLimitDistance; }
            set { _outerLimitDistance = value; }
        }

        public float LifeZoneInnerEdge
        {
            get { return _lifeZoneInnerEdge; }
            set { _lifeZoneInnerEdge = value; }
        }

        public float LifeZoneOuterEdge
        {
            get { return _lifeZoneOuterEdge; }
            set { _lifeZoneOuterEdge = value; }
        }

        public float SnowLine
        {
            get { return _snowLine; }
            set { _snowLine = value; }
        }

        // note: I believe this is obsolete.  See coments in EntityBase.UpdateBoundVolume()
        // about how Entities (with the exception of "Regions") only care about their own geometry 
        // bounding volume and do not include bounding volume of child entities.  It
        // is the SceneNode's that worry about hierarchical bounding volumes.
        // Thus when picking, we'll need logic that says when picking either the billboard halo or
        // sphere of the star, that we've picked the main parent entity that represents it.  In a sesnse
        // it's like flagging the child as deferring to parent when it is picked.

        //#region IBoundVolume
        //protected override void UpdateBoundVolume()
        //{
        //    // see if we can get away with just translating the existing bounding box?
        //    if ((_changeStates & Enums.ChangeStates.BoundingBox_TranslatedOnly & Enums.ChangeStates.BoundingBoxDirty) == Keystone.Enums.ChangeStates.BoundingBox_TranslatedOnly)
        //    {
        //        DisableChangeFlags(Keystone.Enums.ChangeStates.BoundingBox_TranslatedOnly);

        //        _box.Max -= _translationDelta;
        //        _box.Min -= _translationDelta;
        //        _sphere = new BoundingSphere(_box);
        //        return;
        //    }

        //    _box = new BoundingBox( _translation, Radius);

        //    if (_children != null && _children.Count > 0)
        //    {
        //        BoundingBox childboxes = BoundingBox.Initialized();
        //        for (int i = 0; i < _children.Count; i++)
        //        {
        //            // TODO: this is somewhat insane isnt it?  Our Star's bounding box
        //            // is including worlds and moons?!  That would make it difficult to 
        //            // not render these things.... but if we ignore can the scenenode's still
        //            // be potentially rendered for visible child worlds?
        //            // 
        //            if (_children[i] is World) // child entities bbox are already in region coords
        //                childboxes = BoundingBox.Combine(childboxes, ((BoundTransformGroup)_children[i]).BoundingBox);
        //            // TODO: this is wrong because our corona might be bigger
        //            // ignore any child model since we use the actual star radius for bounds
        //            //else if (_children[i] is ModelBase)
        //            //    // child model's will need their box's transformed using region matrix
        //            //    childboxes = BoundingBox.Combine(childboxes,
        //            //                                     BoundingBox.TransformCoords(
        //            //                                         ((BoundElementGroup)_children[i]).BoundingBox,
        //            //                                         new Matrix(RegionMatrix)));
        //        }
        //        _box = BoundingBox.Combine(_box, childboxes);
        //    }
        //    DisableChangeFlags(Keystone.Enums.ChangeStates.BoundingBox_TranslatedOnly | Keystone.Enums.ChangeStates.BoundingBoxDirty);

        //    if (_box == null) return; // if the box still hasnt been set
        //    _sphere = new BoundingSphere(_box);
        //}
        //#endregion

    }
}