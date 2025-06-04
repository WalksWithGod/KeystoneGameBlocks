using Keystone.Types;
using Helper = Keystone.Helpers.TVTypeConverter;
using MTV3D65;
using Keystone.Resource;

namespace Keystone.Lights
{
    public class SpotLight : Light
    {
        // TODO: currently tv3d doesnt support spotlights on meshes with bumpmapped TVLightingmodes set.
        //       Friday. April,11,2008 1:15pm Sylvain said he will add it for ps2.0+ paths, but not enough instructions for ps1.1.
        //       He said it would require 1 extra pass to render with spotlight though so it will be slower than point which is slower than directional
        //TODO: verify phi and theta are in degrees and not radians?
        private float _falloff;
        //not to be confused with SetAttenuation, falloff defines the decrease in illumination between the outside of the inner cone and the outside of the outer cone.

        private float _phi = 45; // angle that defines the outer cone
        private float _theta = 15; // angle that defines the inner cone

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="position"></param>
        /// <param name="direction"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        /// <param name="range"></param>
        /// <param name="phi">Angle that defines the outer cone</param>
        /// <param name="theta">Angle that defines the inner cone</param>
        /// <param name="indoorLight"></param>
        public SpotLight(string name, Vector3d position, Vector3d direction, float r, float g, float b,
                         float range, float phi, float theta, bool indoorLight)
            : base(name, direction, r, g, b, indoorLight)
        {
            Translation = position;
            Range = range;
            _phi = phi;
            _theta = theta;
            InheritScale = false;
        }

        public SpotLight(string id)
            : base(id)
        {
            InheritScale = false;
        }

        #region ResourceBase members
        public override Settings.PropertySpec[] GetProperties(bool specOnly)
        {
            Settings.PropertySpec[] tmp = base.GetProperties(specOnly);
            Settings.PropertySpec[] properties = new Settings.PropertySpec[3 + tmp.Length];
            tmp.CopyTo(properties, 3);

            properties[0] = new Settings.PropertySpec("falloff", typeof(float).Name);
            properties[1] = new Settings.PropertySpec("theta", typeof(float).Name);
            properties[2] = new Settings.PropertySpec("phi", typeof(float).Name);

            if (!specOnly)
            {
                properties[0].DefaultValue = _falloff;
                properties[1].DefaultValue = _theta;
                properties[2].DefaultValue = _phi;
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
                    case "falloff":
                        _falloff = (float)properties[i].DefaultValue;
                        break;
                    case "theta":
                        _theta = (float)properties[i].DefaultValue;
                        break;
                    case "phi":
                        _phi = (float)properties[i].DefaultValue;
                        break;
                }
            }
        }
        #endregion

        #region IPageableTVNode Members
        public override void LoadTVResource()
        {
            CoreClient._CoreClient.Light.CreateSpotLight(Helper.ToTVVector(mTranslation), 
        	                                             Helper.ToTVVector(_direction), 
        	                                             _diffuse.r, 
        	                                             _diffuse.g, 
        	                                             _diffuse.b, 
        	                                             _range,
        	                                             _phi, 
        	                                             _theta, 
        	                                             _id, 
        	                                             _specularLevel);
        	
            CoreClient._CoreClient.Light.SetLightType(_tvfactoryIndex, CONST_TV_LIGHTTYPE.TV_LIGHT_SPOT );
            CoreClient._CoreClient.Light.SetLightProperties(_tvfactoryIndex, TV_MANAGES_LIGHTS, false, false);
            Enable = true;

            mMatrix = Types.Matrix.Identity();

            // June.9.2017 - Important to call base method to set Active = false on load.
            base.LoadTVResource();
            SetChangeFlags(Keystone.Enums.ChangeStates.GeometryAdded | Keystone.Enums.ChangeStates.BoundingBoxDirty, Keystone.Enums.ChangeSource.Self); // TODO: is this ok?

        }
		#endregion

        public float InnerCone
        {
            get { return _theta; }
            set
            {
                _theta = value;
                CoreClient._CoreClient.Light.SetLightSpotAngles(_tvfactoryIndex, _phi, _theta);
            }
        }

        public void SetSpotAngles(float outerCone, float innerCone)
        {
            CoreClient._CoreClient.Light.SetLightSpotAngles(_tvfactoryIndex, outerCone, innerCone);
        }

        // falloff is the attenuation of the light between the innercone and outercone.
        // a searchlight will have very little falloff from the inner cone to outercone 
        // whereas a desk lamp will have significant falloff
        public float FallOff
        {
            get { return _falloff; }
            set
            {
                _falloff = value;
                CoreClient._CoreClient.Light.SetLightSpotFalloff(_tvfactoryIndex, value);
            }
        }
    }
}