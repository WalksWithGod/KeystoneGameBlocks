using Keystone.IO;
using Keystone.Resource;
using Keystone.Types;
using MTV3D65;

namespace Keystone.Lights
{
    public class DirectionalLight : Light
    {
        /// <summary>
        /// Allows us to simulate a pointlight for each renderable item
        /// for cases such as Sun/Star light shining on Worlds and Vehicles.
        /// This is superior to using a PointLight with a huge range.
        /// </summary>
        public bool IsBillboard;

        public DirectionalLight(string id, Vector3d direction, float r, float g, float b, float specularlevel, bool indoorLight) 
            : base(id, direction, r, g, b, indoorLight)
        {
            _specularLevel = specularlevel;
            InheritScale = false;
            // NOTE: since we do use DirectionalLighth bounding boxes for determining when to enable/disable a DirectionalLight, we do want to inherit ROTATION
        }

        public DirectionalLight(string id)
            : base(id)
        {
            InheritScale = false;
            // NOTE: since we do use DirectionalLighth bounding boxes for determining when to enable/disable a DirectionalLight, we do want to inherit ROTATION
        }

        #region ResourceBase members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="specOnly">True returns the properties without any values assigned</param>
        /// <returns></returns>
        public override Settings.PropertySpec[] GetProperties(bool specOnly)
        {
            Settings.PropertySpec[] tmp = base.GetProperties(specOnly);
            Settings.PropertySpec[] properties = new Settings.PropertySpec[1 + tmp.Length];
            tmp.CopyTo(properties, 1);


            properties[0] = new Settings.PropertySpec("isbillboard", IsBillboard.GetType().Name);

            if (!specOnly)
            {
                properties[0].DefaultValue = IsBillboard;
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
                    case "isbillboard":
                        IsBillboard = (bool)properties[i].DefaultValue;
                        break;
                }
            }
        }
        #endregion

        #region IPageableTVNode Members
        public override void  LoadTVResource()
        {
            // NOTE: it's importnt that DirectionalLight has "range" property set because
            //       we use it to help determine what objects are affected by this light!
            ////_tvfactoryIndex = CoreClient._CoreClient.Light.CreateDirectionalLight(Helpers.TVTypeConverter.ToTVVector(_direction), 
            ////                                                                      _diffuse.r, 
            ////                                                                      _diffuse.g,
            ////                                                                      _diffuse.b, 
            ////                                                                      _id, 
            ////                                                                      _specularLevel);
            TV_LIGHT l = new TV_LIGHT();
            TV_COLOR c;
            c.r = 0;
            c.g = 0;
            c.b = 0;
            c.a = 1.0F;
            l.ambient = c;

            c.r = _diffuse.r;
            c.g = _diffuse.g;
            c.b = _diffuse.b;
            c.a = 1.0f;

            l.specular = c;
            l.diffuse =c;
            l.direction = Helpers.TVTypeConverter.ToTVVector( _direction);

            l.type = CONST_TV_LIGHTTYPE.TV_LIGHT_DIRECTIONAL;
            l.bManaged = true;
           
            _tvfactoryIndex = CoreClient._CoreClient.Light.CreateLight(ref l);
            CoreClient._CoreClient.Light.EnableLight (_tvfactoryIndex, false);

            System.Diagnostics.Debug.WriteLine("Light.LoadTVResource() - Light loaded at index " + _tvfactoryIndex.ToString());
                        
            //CoreClient._CoreClient.Light.SetLightType(_tvfactoryIndex, CONST_TV_LIGHTTYPE.TV_LIGHT_DIRECTIONAL);
            CoreClient._CoreClient.Light.SetLightProperties(_tvfactoryIndex, TV_MANAGES_LIGHTS, false, false);

            // TODO: bump map specular and such should be init'ed here as well, not just Specular
            SpecularLightingEnabled = _specularEnabled;
            Specular = _specular;
            SetChangeFlags(Keystone.Enums.ChangeStates.GeometryAdded |
                           Keystone.Enums.ChangeStates.BoundingBoxDirty, Keystone.Enums.ChangeSource.Self);
            CastShadows = _castShadows;

            // important to set Active to false
            base.LoadTVResource();
        }
#endregion

        public override void Activate(bool value)
        {
            // when this DIRECITONAL light is attached to the scene, we DO NOT
            // set it's range to diameter because it is possible to have a simulated directional light
            // position outside of even the Root Zone's dimensions.  Classic example is the sun light used in
            // atmosphere scattering simulation.  So instead, just keep the range as the value
            // already set in _range on creation
            
            //if (value == true)
            //    Range = (float)Region.BoundingBox.Diameter;
            // else it is being detached and the Region is null!


            base.Activate(value);
        
        }
    }
}
