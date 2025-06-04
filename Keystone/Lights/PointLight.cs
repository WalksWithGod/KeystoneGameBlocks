using Keystone.IO;
using Keystone.Resource;
using Keystone.Types;
using MTV3D65;
using Helper = Keystone.Helpers.TVTypeConverter;

namespace Keystone.Lights
{
    public class PointLight : Light
    {
        private float[] mAttenuation;


        public PointLight(string name, Vector3d position, float r, float g, float b, float range, 
            float attenuation0, float attenuation1, float attenuation2, bool indoorLight)
            : base(name, new Vector3d(), r, g, b, indoorLight)
        {
            Translation = position;
            // range has an upper limit.  Trying to use float.MaxValue results in a point light that silently fails
            // not sure what the actual upper limit is.
            LatestStepTranslation = position;
            Range = range;

            mAttenuation = new float[] { attenuation0, attenuation1, attenuation2 };
            SetAttentuation(mAttenuation);

            InheritScale = false;
        }

        public PointLight(string id)
            : base(id)
        {
            InheritScale = false;
        }

        #region ResourceBase members
        public override Settings.PropertySpec[] GetProperties(bool specOnly)
        {
            Settings.PropertySpec[] tmp = base.GetProperties(specOnly);
            Settings.PropertySpec[] properties = new Settings.PropertySpec[1 + tmp.Length];
            tmp.CopyTo(properties, 1);

            properties[0] = new Settings.PropertySpec("attenuation", typeof(float[]).Name);

            if (!specOnly)
            {
                properties[0].DefaultValue = mAttenuation;
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
                        mAttenuation = (float[])properties[i].DefaultValue;
                        SetAttentuation(mAttenuation);
                        break;
                }
            }
        }
        #endregion

        ///<summary >
        /// PointLight node's illumination falls off with distance as specified by three attenuation coefficients. 
        /// The attenuation factor is:  1/max(attenuation[0] + attenuation[1] × r + attenuation[2] × r2, 1)
        /// where r is the distance from the light to the surface being illuminated. 
        /// The default is no attenuation. An attenuation value of (0, 0, 0) is identical to (1, 0, 0)
        /// </summary>
        /// Range Constant Linear Quadratic  - values suggested by Ogre to have a point light fall of to near 0 at the end of it's range
        /// 3250, 1.0, 0.0014, 0.000007
        /// 600, 1.0, 0.007, 0.0002
        /// 325, 1.0, 0.014, 0.0007
        /// 200, 1.0, 0.022, 0.0019
        /// 160, 1.0, 0.027, 0.0028
        /// 100, 1.0, 0.045, 0.0075
        /// 65, 1.0, 0.07, 0.017
        /// 50, 1.0, 0.09, 0.032
        /// 32, 1.0, 0.14, 0.07
        /// 20, 1.0, 0.22, 0.20
        /// 13, 1.0, 0.35, 0.44
        /// 7, 1.0, 0.7, 1.8
        public void SetAttentuation(float a0, float a1, float a2)
        {
            mAttenuation = new float[3];
            mAttenuation[0] = a0;
            mAttenuation[1] = a1;
            mAttenuation[2] = a2;

            //if (mAttenuation[0] == 0f && mAttenuation[1] == 0f &&  mAttenuation[2] == 0f)
            //    mAttenuation[0] = 1f;  // this is required because TV does not like 0,0,0.  The light will basically stop working completely if so.
           
            // TODO: Im having problems setting any attenuation values here.  The pointlight just totally stops working. 
            //        not exactly sure what the issue is.
            CoreClient._CoreClient.Light.SetLightAttenuation(_tvfactoryIndex, mAttenuation[0], mAttenuation[1], mAttenuation[2]);
        }

        public void SetAttentuation(float[] attenuation) 
        {
            if (attenuation == null || attenuation.Length != 3) return;
            SetAttentuation(attenuation[0], attenuation[1], attenuation[2]);
        }

        #region IPageableTVNode Members
        public override void LoadTVResource()
        {
            SpecularLightingEnabled = _specularEnabled;

            _tvfactoryIndex = CoreClient._CoreClient.Light.CreatePointLight (Helpers.TVTypeConverter.ToTVVector(mTranslation ), 
                                                                             _diffuse.r, 
                                                                             _diffuse.g, 
                                                                             _diffuse.b,
                                                                             _range,  
                                                                             _id,  
                                                                             _specularLevel);
            // WARNING: Must disable light in tvLightEngine until we set all properties. 
            // I think its good practice to always disable prior to modifying from other thread! otherwise seems to cause accessviolations
            // TODO: in fact, i might want to try to prevent changes to light properties from other thread altogether if accessviolations persist
            //       i could maybe treat like appearance and modify on apply of light or smthn
            Enable = false; // TODO: this is rather confusing because enable node does not mean same thing as light enabled in actual scene!
            
            //CoreClient._CoreClient.Light.SetLightType(_tvfactoryIndex, CONST_TV_LIGHTTYPE.TV_LIGHT_POINT );
            CoreClient._CoreClient.Light.SetLightProperties(_tvfactoryIndex, false, false, false);

            // CoreClient._CoreClient.Light.SetBumpLightProperties(_tvfactoryIndex, true, false);


            SetAttentuation(mAttenuation);

            Ambient = _diffuse;
            Specular = _specular;
            
            Range = _range;
            Enable = true;
            //System.Diagnostics.Debug.WriteLine("Light.LoadTVResource() - Light loaded at index " + _tvfactoryIndex.ToString());

            // June.9.2017 - Important to call base method to set Active = false on load.
            base.LoadTVResource();

            SetChangeFlags(Keystone.Enums.ChangeStates.BoundingBoxDirty, Keystone.Enums.ChangeSource.Self); 
        }
        #endregion
    }
}