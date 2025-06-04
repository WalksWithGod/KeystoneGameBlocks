using System;
using Keystone.Collision;
using Keystone.Entities;
using Keystone.IO;
using Keystone.Traversers;
using Keystone.Types;
using MTV3D65;

namespace Keystone.Elements
{
    public class Attractor : Geometry
    {
        private TVParticleSystem _particle;
        private Vector3d _attenuation;
        private float _radius;

        public Attractor(string id, TVParticleSystem p)
            : base(id)
        {
            _particle = p;
            _tvfactoryIndex = p.CreateAttractor();
        }

        #region ITraversable Members
        public override object Traverse(ITraverser target, object data)
        {
            return target.Apply(this, data);
        }
        #endregion


        /// <summary>
        /// 
        /// </summary>
        /// <param name="specOnly">True returns the properties without any values assigned</param>
        /// <returns></returns>
        public override Settings.PropertySpec[] GetProperties(bool specOnly)
        {
            Settings.PropertySpec[] tmp = base.GetProperties(specOnly);
            Settings.PropertySpec[] properties = new Settings.PropertySpec[2 + tmp.Length];
            tmp.CopyTo(properties, 2);

            properties[0] = new Settings.PropertySpec("attenuation", _attenuation.GetType().Name);
            properties[1] = new Settings.PropertySpec("radius", _radius.GetType().Name);
            if (!specOnly)
            {
                properties[0].DefaultValue = _attenuation;
                properties[0].DefaultValue = _radius;
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
                    case "attenuation":
                        _attenuation = (Vector3d)properties[i].DefaultValue;
                        break;
                    case "radius":
                        _radius = (float)properties[i].DefaultValue;
                        break;
                }
            }
        }

        #region IPageableNode Members
        public override void UnloadTVResource()
        {
        	// TODO: UnloadTVResource()
        }
                
        public override void LoadTVResource()
        {
            throw new NotImplementedException();
            SetChangeFlags(Keystone.Enums.ChangeStates.GeometryAdded | 
                Keystone.Enums.ChangeStates.BoundingBoxDirty |
            Keystone.Enums.ChangeStates.MatrixDirty, Keystone.Enums.ChangeSource.Self);
        }

        public override void SaveTVResource(string filepath)
        {
            throw new NotImplementedException();
        }
        #endregion


        // Attractor

        public int CreateAttractor(bool bDirectionalField)
        {
            return _particle.CreateAttractor(bDirectionalField);
        }

        public void FieldDirection(Vector3d vDirection)
        {
            _particle.SetAttractorFieldDirection(_tvfactoryIndex, Helpers.TVTypeConverter.ToTVVector(vDirection));
        }

        public void Parameters(float fAttractionRepulsionConstant)
        {
            _particle.SetAttractorParameters(_tvfactoryIndex, fAttractionRepulsionConstant);
        }

        public void Parameters(float fAttractionRepulsionConstant, CONST_TV_ATTRACTORVELOCITYPOWER eVelocityDependency)
        {
            _particle.SetAttractorParameters(_tvfactoryIndex, fAttractionRepulsionConstant, eVelocityDependency);
        }

        public Vector3d Attenuation
        {
            get { return Helpers.TVTypeConverter.FromTVVector(_particle.GetAttractorAttenuation(_tvfactoryIndex)); }
            set
            {
                _attenuation = value;
                _particle.SetAttractorAttenuation(_tvfactoryIndex, Helpers.TVTypeConverter.ToTVVector(_attenuation));
            }
        }

        public override bool Enable
        {
            get { throw new NotImplementedException(); }
            set
            {
                base.Enable = value;
                _particle.SetAttractorEnable(_tvfactoryIndex, value);
            }
        }

        //public override Vector3d  Position
        //{
        //    get{return _particle.GetAttractorPosition(_tvfactoryIndex);}
        //    set {
        //    //    _position  = value;
        //    //    _particle.SetAttractorPosition(_tvfactoryIndex, _position); 

        //    }
        //}

        public float Radius
        {
            get { return _particle.GetAttractorRadius(_tvfactoryIndex); }
            set
            {
                _radius = value;
                _particle.SetAttractorRadius(_tvfactoryIndex, _radius);
            }
        }


        #region Geometry Members
        internal override PickResults AdvancedCollide(Vector3d start, Vector3d end, KeyCommon.Traversal.PickParameters parameters)
        {
        	// picking done with model space ray in model space
            throw new NotImplementedException();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="scene">Usually Context.Scene rather than Entity.Scene since AssetPlacementTool preview Entity's are not connected to Scene.</param>
        /// <param name="model"></param>
        /// <param name="elapsedSeconds"></param>
        internal override void Render(Matrix matrix, Scene.Scene scene, Model model, double elapsedSeconds)
        {
        	// TODO: it should be impossible for _resourceStatus to be "Loaded" before it's actually loaded
        	//       HOWEVER if paging out, we could start to render here first since it's not synclocked and then
        	//       while minimesh.Render() we page out and set _resourceStatus to Unloading but we're already in .Render()!
            	
        	// NOTE: we check PageableNodeStatus.Loaded and NOT TVResourceIsLoaded because that 
        	// TVIndex is set after Scene.CreateMesh() and thus before we've finished adding vertices
        	// via .AddVertex()  or .SetGeometry() or even loaded the mesh from file.
            if (_resourceStatus != PageableNodeStatus.Loaded ) return;
                
            // i dont believe these things need to be touched.  Just the particle systems overall relative camera position really...
            //_particle.SetAttractorMatrix(_tvfactoryIndex, model.Matrix * entityInstance.Matrix);
            _particle.SetGlobalMatrix(Helpers.TVTypeConverter.ToTVMatrix(matrix));
            //_particle.SetGlobalPosition((float)cameraSpacePosition.x, (float)cameraSpacePosition.y,
            //                            (float)cameraSpacePosition.z);

        }

        #endregion


        #region IBoundVolume Members
        protected override void UpdateBoundVolume()
        {
            // TODO: An attractor isnt really visible so it shouldn't be geometry right?  This is more of an animation
            // controller...
            float radius = 0;
            radius = _particle.GetAttractorRadius (_tvfactoryIndex);

            DisableChangeFlags( Keystone.Enums.ChangeStates.BoundingBoxDirty);
        }
        #endregion

        #region ResourceBase members
        protected override void DisposeUnmanagedResources()
        {
            base.DisposeUnmanagedResources();
            try
            {
                // destroy the emitter
                _particle.DestroyAttractor(_tvfactoryIndex);
            }
            catch
            {
            }
        }
        #endregion
    }
}