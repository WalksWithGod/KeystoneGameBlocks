using System;
using System.Xml;
using Keystone.Elements;
using Keystone.IO;
using Keystone.Traversers;
using Keystone.Resource;
using Keystone.Types;
using Keystone.Enums;

namespace Keystone.Appearance
{
    //http://msdn.microsoft.com/en-us/library/bb147175(VS.85).aspx
    // The Diffuse and Ambient members of the D3DMATERIAL9 structure describe how a material reflects 
    // the ambient and diffuse light in a scene. Because most scenes contain much more diffuse light than 
    //ambient light, diffuse reflection plays the largest part in determining color. Additionally, 
    //because diffuse light is directional, the angle of incidence for diffuse light affects the overall
    //intensity of the reflection. Diffuse reflection is greatest when the light strikes a vertex parallel
    //to the vertex normal. As the angle increases, the effect of diffuse reflection diminishes. 
    //The amount of light reflected is the cosine of the angle between the incoming light and the vertex 
    //normal, as shown here.
    // 
    //Ambient reflection, like ambient light, is nondirectional. Ambient reflection has a lesser 
    //impact on the apparent color of a rendered object, but it does affect the overall color and 
    //is most noticeable when little or no diffuse light reflects off the material. A material's
    //ambient reflection is affected by the ambient light set for a scene by calling the 
    //IDirect3DDevice9::SetRenderState method with the D3DRS_AMBIENT flag.
    //
    // Diffuse and ambient reflection work together to determine the perceived color of an object, and are
    // usually identical values. For example, to render a blue crystalline object, you create a material that 
    // reflects only the blue component of diffuse and ambient light. When placed in a room with a white light, 
    // the crystal appears to be blue. However, in a room that has only red light, the same crystal would appear 
    // to be black, because its material doesn't reflect red light.
    //
    // Emission
    // ========
    // Materials can be used to make a rendered object appear to be self-luminous. The Emissive member of the 
    // D3DMATERIAL9 structure is used to describe the color and transparency of the emitted light. Emission 
    // affects an object's color and can, for example, make a dark material brighter and take on part of the emitted color.

    // You can use a material's emissive property to add the illusion that an object is emitting light, without 
    // incurring the computational overhead of adding a light to the scene. In the case of the blue crystal, the 
    // emissive property is useful if you want to make the crystal appear to light up, but not cast light on other 
    // objects in the scene. Remember, materials with emissive properties don't emit light that can be reflected by 
    // other objects in a scene. To achieve this reflected light, you need to place an additional light within the scene.
    //
    // Specular Reflection
    // ==================
    // Specular reflection creates highlights on objects, making them appear shiny. The D3DMATERIAL9 structure contains
    // two members that describe the specular highlight color as well as the material's overall shininess. You establish 
    // the color of the specular highlights by setting the Specular member to the desired RGBA color - the most common 
    // colors are white or light gray. The values you set in the Power member control how sharp the specular effects are.
    // Specular highlights can create dramatic effects. Drawing again on the blue crystal analogy: a larger Power value 
    // creates sharper specular highlights, making the crystal appear to be quite shiny. Smaller values increase the area 
    // of the effect, creating a dull reflection that make the crystal look frosty. To make an object truly matte, set the 
    // Power member to zero and the color in Specular to black. Experiment with different levels of reflection to produce 
    // a realistic appearance for your needs. The following illustration shows two identical models.
    public class Material : Node, IPageableTVNode
    {
        public enum DefaultMaterials
        {
            matte, // matte is purely diffuse material that spreads all light evenly in all directions
            white,
            red,
            green,
            blue,
            yellow,
            white_fullbright,
            red_fullbright,
            green_fullbright,
            blue_fullbright,
            yellow_fullbright,
            gold,
            silver,
            bronze,
            copper,
            iron,
            gunmetal,
            asphalt,
            pool,
            lake,
            pond,
            ocean,
            tapwater
        }

        private float _specularPower = 12f; // 2 is low, 8, 16, 32 are higher values. specular power range is 0 to +infinity
        private float _opacity = 1.0f;
        private Color _diffuse;
        private Color _ambient;
        private Color _specular;
        private Color _emissive;
        protected int _tvfactoryIndex = -1;
        protected string _resourcePath;
        protected PageableNodeStatus _resourceStatus;
        private readonly object mSyncRoot;


        internal Material(string id)
            : base(id)
        {
            Shareable = true;
            mSyncRoot = new object();
        }


        public static Material Create(string id, Color diffuse, Color ambient, Color specular, Color emissive)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException("Material name cannot be null or empty.");
            // NOTE: this should match with thekey also already existing in the repository.
            // The way this works is, when creating an object during "load" you should check the repository first
            // and only attempt a new instance if a previous doesnt exist.  We don't let this Create() call
            // assume you meant to get a reference to a shared copy.  You must do that explicitly through the repository.
            //if (CoreClient._CoreClient.MaterialFactory.GetMaterialByName(id) > 0)
            //    throw new ArgumentException("Material with this name already exists.")

            Material material = (Material)Repository.Create(id, "Material");
            // TODO: the setting of Diffuse, Ambient, Specular, Emissive results in propgation of flags
            //       Is there a good way to know if the node returned is shared or not and whether
            //       there is a need to set these values on it?  I could try Repository.Get() first and see if it returns null
            if (material.RefCount == 0)
            {
                material.Diffuse = diffuse;
                material.Ambient = ambient;
                material.Specular = specular;
                material.Emissive = emissive;
            }
            return material;
        }

        private Material(string id, Color diffuse, Color ambient, Color specular, Color emissive)
            : this(id, diffuse, ambient, specular, emissive, 1.0f, 10f)
        {
            // assign defaults disregarding the dummy values passed in constructor
            Opacity = _opacity;
            SpecularPower = _specularPower;
        }

        private Material(string id, Color diffuse, Color ambient, Color specular, Color emissive, float opacity,
                 float power)
            : this(id)
        {
            _tvfactoryIndex = CoreClient._CoreClient.MaterialFactory.CreateMaterial(_id);

            Diffuse = diffuse;
            Ambient = ambient;
            Specular = specular;
            Emissive = emissive;
            Opacity = opacity;
            SpecularPower = power;
        }

        // constructor for an already created TVMATERIAL?  Probably shouldnt be allowed
        // because we want to funnel creation of materials to a single point so we can properly
        // track references.  
        // TODO: Actually materials can sometimes get added via the bLoadMaterials , bloadTextures of tvm and .x files
        // but maybe there we ccan still make them instance this constructor... for textures too.
        private Material(int index, string id, Color diffuse, Color ambient, Color specular, Color emissive,
                         float opacity, float power)
            : this(id)
        {
            _tvfactoryIndex = index;
            Diffuse = diffuse;
            Ambient = ambient;
            Specular = specular;
            Emissive = emissive;
            Opacity = opacity;
            SpecularPower = power;
        }


        /// <summary>
        /// Instances a Material object from a TVMaterialFactory index value. 
        /// Commonly this is used when auto-loading Materials and Textures with Mesh / Actor loading.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static Material Create(int index)
        {
            string id = CoreClient._CoreClient.MaterialFactory.GetMaterialName(index);

            Material material = (Material)Repository.Get(id);
            if (material != null) return material;

            return new Material(index,
                                CoreClient._CoreClient.MaterialFactory.GetMaterialName(index),
                                Helpers.TVTypeConverter.FromTVColor(CoreClient._CoreClient.MaterialFactory.GetDiffuse(index)),
                                Helpers.TVTypeConverter.FromTVColor(CoreClient._CoreClient.MaterialFactory.GetAmbient(index)),
                                Helpers.TVTypeConverter.FromTVColor(CoreClient._CoreClient.MaterialFactory.GetSpecular(index)),
                                Helpers.TVTypeConverter.FromTVColor(CoreClient._CoreClient.MaterialFactory.GetEmissive(index)),
                                CoreClient._CoreClient.MaterialFactory.GetOpacity(index),
                                CoreClient._CoreClient.MaterialFactory.GetPower(index));
        }


        // TODO: many of these default materials we should reserve their names
        //       "KBG_MATERIAL_********"
        //      this way, having a "library" of default materials is easier to manage and initialize
        //      without needing any archive to load them from
        public static Material Create(DefaultMaterials material)
        {
            Material mat = null;
            // TODO: is locking the entire Repository cache from the outside a good way to go?
            lock (Keystone.Resource.Repository.mCache)
            {
                string name = "KBG_MATERIAL_" + material.ToString();
                // determine if the material is already in the repository
                mat = (Material)Repository.Get(name);

                if (mat == null)
                {
                    // March.20.2017 - DO NOT CREATE TVMATERIAL INSIDE TV3D MaterialFactory here!  Allow LoadTVResource() to handle that.
                    //int index = CoreClient._CoreClient.MaterialFactory.CreateMaterial(name);

                    mat = (Material)Repository.Create(name, "Material");

                    // http://www.foszor.com/blog/xna-color-chart/
                    // http://www.tayloredmktg.com/rgb/
                    switch (material)
                    {
                        case DefaultMaterials.matte:
                            mat._diffuse = Color.White;
                            mat._ambient = Color.Black;
                            mat._specular = Color.Black;
                            mat._emissive = Color.Black;
                            mat._opacity = 1f;
                            mat._specularPower = 0f;
                            break;
                        //material.Diffuse.ToString ();
                        //"0.4156862,0.4470589,0.5450981,1"
                        //material.Ambient.ToString ();
                        //"0,0,0,1"
                        //material.Emissive.ToString ();
                        //"0,0,0,1"
                        //material.Specular.ToString ();
                        //"0.04490196,0.04490196,0.04490196,1"
                        //material.Power.ToString ();
                        //"3"
                        //material.Opacity.ToString ();
                        //"1"
                        case DefaultMaterials.white:
                            mat._diffuse = Color.White;
                            mat._ambient = Color.Black;
                            mat._specular = Color.Black;
                            mat._emissive = Color.White;
                            mat._opacity = 1f;
                            mat._specularPower = 0f;
                            break;
                        case DefaultMaterials.red:
                            mat._diffuse = Color.Red;
                            mat._ambient = Color.Black;
                            mat._specular = Color.Black;
                            mat._emissive = Color.Black;
                            mat._opacity = 1f;
                            mat._specularPower = 0f;
                            break;
                        case DefaultMaterials.green:
                            mat._diffuse = Color.Green;
                            mat._ambient = Color.Black;
                            mat._specular = Color.Black;
                            mat._emissive = Color.Black;
                            mat._opacity = 1f;
                            mat._specularPower = 0f;
                            break;
                        case DefaultMaterials.blue:
                            mat._diffuse = Color.Blue;
                            mat._ambient = Color.Black;
                            mat._specular = Color.Black;
                            mat._emissive = Color.Black;
                            mat._opacity = 1f;
                            mat._specularPower = 0f;
                            break;
                        case DefaultMaterials.yellow:
                            mat._diffuse = Color.Yellow;
                            mat._ambient = Color.Black;
                            mat._specular = Color.Black;
                            mat._emissive = Color.Black;
                            mat._opacity = 1f;
                            mat._specularPower = 0f;
                            break;
                        case DefaultMaterials.white_fullbright:
                            mat._diffuse = Color.Black;
                            mat._ambient = Color.Black;
                            mat._specular = Color.Black;
                            mat._emissive = Color.White;
                            mat._opacity = 1f;
                            mat._specularPower = 0f;
                            break;
                        case DefaultMaterials.red_fullbright:
                            mat._diffuse = Color.Black;
                            mat._ambient = Color.Black;
                            mat._specular = Color.Black;
                            mat._emissive = Color.Red;
                            mat._opacity = 1f;
                            mat._specularPower = 0f;
                            break;
                        case DefaultMaterials.green_fullbright:
                            mat._diffuse = Color.Black;
                            mat._ambient = Color.Black;
                            mat._specular = Color.Black;
                            mat._emissive = Color.Green;
                            mat._opacity = 1f;
                            mat._specularPower = 0f;
                            break;
                        case DefaultMaterials.blue_fullbright:
                            mat._diffuse = Color.Black;
                            mat._ambient = Color.Black;
                            mat._specular = Color.Black;
                            mat._emissive = Color.Blue;
                            mat._opacity = 1f;
                            mat._specularPower = 0f;
                            break;

                        case DefaultMaterials.yellow_fullbright:
                            mat._diffuse = Color.Black;
                            mat._ambient = Color.Black;
                            mat._specular = Color.Black;
                            mat._emissive = Color.Yellow;
                            mat._opacity = 1f;
                            mat._specularPower = 0f;
                            break;

                        case DefaultMaterials.silver:
                            mat._diffuse = Color.Silver;
                            mat._ambient = Color.Silver;
                            mat._specular = Color.White;
                            mat._emissive = Color.Black;
                            mat._opacity = 1f;
                            mat._specularPower = 10f;
                            break;

                        case DefaultMaterials.gold:
                            mat._diffuse = Color.Gold;
                            mat._ambient = Color.Gold;
                            mat._specular = Color.White;
                            mat._emissive = Color.Black;
                            mat._opacity = 1f;
                            mat._specularPower = 10f;
                            break;

                        case DefaultMaterials.bronze: 
                            mat._diffuse = Color.Bronze;
                            mat._ambient = Color.Bronze;
                            mat._specular = Color.White;
                            mat._emissive = Color.Black;
                            mat._opacity = 1f;
                            mat._specularPower = 10f;
                            break;

                        case DefaultMaterials.copper: 
                            mat._diffuse = Color.Copper;
                            mat._ambient = Color.Copper;
                            mat._specular = Color.White;
                            mat._emissive = Color.Black;
                            mat._opacity = 1f;
                            mat._specularPower = 10f;
                            break;
                        case DefaultMaterials.iron:
                            mat._diffuse = Color.Iron;
                            mat._ambient = Color.Iron;
                            mat._specular = Color.Grey;
                            mat._emissive = Color.Black;
                            mat._opacity = 1f;
                            mat._specularPower = 8f;
                            break;
                        default:
                            throw new Exception("DefaultMaterial " + material.ToString() + " not yet implemented.");

                    }
                }

                return mat;
            }
        }

        #region ITraversable
        public override object Traverse(ITraverser target, object data)
        {
            return target.Apply(this, data);
        }
        #endregion

        internal override ChildSetter GetChildSetter()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="specOnly">True returns the properties without any values assigned</param>
        /// <returns></returns>
        public override Settings.PropertySpec[] GetProperties(bool specOnly)
        {
            Settings.PropertySpec[] tmp = base.GetProperties(specOnly);
            Settings.PropertySpec[] properties = new Settings.PropertySpec[6 + tmp.Length];
            tmp.CopyTo(properties, 6);

            properties[0] = new Settings.PropertySpec("diffuse", _diffuse.GetType().Name);
            properties[1] = new Settings.PropertySpec("ambient", _ambient.GetType().Name);
            properties[2] = new Settings.PropertySpec("specular", _specular.GetType().Name);
            properties[3] = new Settings.PropertySpec("emissive", _emissive.GetType().Name);
            properties[4] = new Settings.PropertySpec("opacity", _opacity.GetType().Name);
            properties[5] = new Settings.PropertySpec("power", _specularPower.GetType().Name);

            if (!specOnly)
            {
                properties[0].DefaultValue = _diffuse;
                properties[1].DefaultValue = _ambient;
                properties[2].DefaultValue = _specular;
                properties[3].DefaultValue = _emissive;
                properties[4].DefaultValue = _opacity;
                properties[5].DefaultValue = _specularPower;
            }

            return properties;
        }

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
                    case "diffuse":
                        Diffuse = (Color)properties[i].DefaultValue;
                        break;
                    case "ambient":
                        Ambient = (Color)properties[i].DefaultValue;
                        break;
                    case "specular":
                        Specular = (Color)properties[i].DefaultValue;
                        break;
                    case "emissive":
                        Emissive = (Color)properties[i].DefaultValue;
                        break;
                    case "opacity":
                        Opacity = (float)properties[i].DefaultValue;
                        break;
                    case "power":
                        SpecularPower = (float)properties[i].DefaultValue;
                        break;
                }
            }
            SetChangeFlags(Keystone.Enums.ChangeStates.AppearanceParameterChanged, Keystone.Enums.ChangeSource.Self);
        }

        #region IPageableTVNode Members
        public object SyncRoot { get { return mSyncRoot; } }

        public int TVIndex
        {
            get { return _tvfactoryIndex; }
        }

        public bool TVResourceIsLoaded
        {
            get { return _tvfactoryIndex > -1; }
        }

        public PageableNodeStatus PageStatus { get { return _resourceStatus; } set { _resourceStatus = value; } }

        public void UnloadTVResource()
        {
            // TODO: UnloadTVResource()
        }

        public void LoadTVResource()
        {
            _tvfactoryIndex = CoreClient._CoreClient.MaterialFactory.CreateMaterial(_id);
            // NOTE: CreateLightMaterial() doesnt support all the parameters we need so best to skip it 
            //_tvfactoryIndex = CoreClient._CoreClient.MaterialFactory.CreateLightMaterial (
            //    _diffuse.r,
            //    _diffuse.g, 
            //    _diffuse.b,
            //    _opacity,
            //    0f,
            //    1.0f,
            //    _id);
            System.Diagnostics.Debug.WriteLine("Material.LoadTVResource() - Material created at index '" + _tvfactoryIndex.ToString() + "'");
            // even with the private vars loaded, we still need to call the actual properties to get the TVMaterialFactory to set the colors for the material
            Apply();
            SetChangeFlags(Keystone.Enums.ChangeStates.AppearanceParameterChanged, Keystone.Enums.ChangeSource.Self);
        }

        /// <summary>
        /// Apply the material settings to the TV3D Material at the appropriate index.
        /// </summary>
        internal void Apply()
        {
            // we test TVResourceIsLoaded and not PageStatus since colors are set DURING LoadTVResource() 
            // before PageStatus can be changed by Pager
            if (TVResourceIsLoaded)
            {
                // TODO: Verify this only ever occurs outside of the render loop!
                CoreClient._CoreClient.MaterialFactory.SetDiffuse(_tvfactoryIndex, _diffuse.r, _diffuse.g, _diffuse.b, _diffuse.a);
                CoreClient._CoreClient.MaterialFactory.SetAmbient(_tvfactoryIndex, _ambient.r, _ambient.g, _ambient.b, _ambient.a);
                CoreClient._CoreClient.MaterialFactory.SetSpecular(_tvfactoryIndex, _specular.r, _specular.g, _specular.b, _specular.a);
                CoreClient._CoreClient.MaterialFactory.SetEmissive(_tvfactoryIndex, _emissive.r, _emissive.g, _emissive.b, _emissive.a);
                CoreClient._CoreClient.MaterialFactory.SetOpacity(_tvfactoryIndex, _opacity);
                CoreClient._CoreClient.MaterialFactory.SetPower(_tvfactoryIndex, _specularPower);
                //#if DEBUG
                //                    // note: if we check the diffuse after setting opacity, it should reflect
                //                    // in the .a component of the diffuse
                //                    MTV3D65.TV_COLOR diffuse = CoreClient._CoreClient.MaterialFactory.GetDiffuse(_tvfactoryIndex);
                //                    System.Diagnostics.Debug.Assert(diffuse.a == _opacity);
                //#endif
            }
        }

        public void SaveTVResource(string filepath)
        {
            throw new NotImplementedException();
        }

        public string ResourcePath
        {
            get { return _resourcePath; }
            set { _resourcePath = value; }
        }
        #endregion

        #region ResourceBase members
        protected override void DisposeUnmanagedResources()
        {
            // NOTE: Material being deleted must NOT be in use upon calling DeleteMaterial()
            CoreClient._CoreClient.MaterialFactory.DeleteMaterial(_tvfactoryIndex);
            base.DisposeUnmanagedResources();
        }
        #endregion

        public Color Diffuse
        {
            get { return _diffuse; }
            set
            {
                _diffuse = value;
                SetChangeFlags(Keystone.Enums.ChangeStates.AppearanceParameterChanged, Keystone.Enums.ChangeSource.Self);
            }
        }

        public Color Ambient
        {
            get { return _ambient; }
            set
            {
                _ambient = value;
                SetChangeFlags(Keystone.Enums.ChangeStates.AppearanceParameterChanged, Keystone.Enums.ChangeSource.Self);
            }
        }

        public Color Emissive
        {
            get { return _emissive; }
            set
            {
                _emissive = value;
                SetChangeFlags(Keystone.Enums.ChangeStates.AppearanceParameterChanged, Keystone.Enums.ChangeSource.Self);
            }
        }

        /// <summary>
        /// Requires that the light have enable specular set (eg. TVLightEngine.SetSpecularLighting(true);
        /// </summary>
        /// <remarks>
        /// 2D Draw calls will disable specular lighting and the specular lighting will only
        /// be re-enabled at the start of the next render loop. So if you need to draw 3D meshes 
        /// with specular, then 2D items, then 3D again, you will need to explicitly call 
        /// LightEngine.SetSpecularLighting(true) after the 2D items or the final 3D items will 
        /// have specular disabled.
        /// </remarks>
        public Color Specular
        {
            get { return _specular; }
            set
            {
                _specular = value;
                SetChangeFlags(Keystone.Enums.ChangeStates.AppearanceParameterChanged, Keystone.Enums.ChangeSource.Self);
            }
        }

        /// <summary>
        /// Specular Power (shinyness) is used to set a highlight "shine" on a mesh.  
        /// Value between 0 and 100.  Though MSDN says a float from 0.0 to +infinity.
        /// Specular highlights almost double the cost of a light. Use them only when you must. 
        /// Set the D3DRS_SPECULARENABLE render state to 0, the default value, whenever possible. 
        /// When defining materials, you must set the specular power value to zero to turn off specular 
        /// highlights for that material; just setting the specular color to 0,0,0 is not enough.
        /// </summary>
        public float SpecularPower
        {
            get { return _specularPower; }
            set
            {
                _specularPower = value;
                SetChangeFlags(Keystone.Enums.ChangeStates.AppearanceParameterChanged, Keystone.Enums.ChangeSource.Self);
            }
        }

        /// <summary>
        /// Valid Range 0.0 - 1.0f.  0.0 == completely transparent. 1.0 == completely opaque.
        /// Equivalent to setting the diffuse alpha of the Material.  
        /// Since only the diffuse alpha is used in Material SetOpacity is a shortcut.  
        /// For Mesh and Actors, must also set .SetBlendingMode  (CONST_TV_BLENDINGMODE.TV_BLEND_ALPHA)
        /// </summary>
        public float Opacity
        {
            get { return _opacity; }
            set
            {
                _opacity = value;
                SetChangeFlags(Keystone.Enums.ChangeStates.AppearanceParameterChanged, Keystone.Enums.ChangeSource.Self);
            }
        }

        protected override void PropogateChangeFlags(ChangeStates flags, ChangeSource source)
        {
            base.PropogateChangeFlags(flags, source);

            if ((flags & Keystone.Enums.ChangeStates.AppearanceNodeChanged) != 0 || (flags & Keystone.Enums.ChangeStates.AppearanceParameterChanged) != 0)
            {

                Apply(); // apply changes to the tvMaterialFactory

                // clear flag
                DisableChangeFlags(Keystone.Enums.ChangeStates.AppearanceNodeChanged | ChangeStates.AppearanceParameterChanged);
            }


        }


        public override int GetHashCode()
        {
            uint result = 0;
            byte[] data;

            data = BitConverter.GetBytes(_diffuse.ToInt32());
            Keystone.Utilities.JenkinsHash.Hash(data, ref result);

            data = BitConverter.GetBytes(_ambient.ToInt32());
            Keystone.Utilities.JenkinsHash.Hash(data, ref result);

            data = BitConverter.GetBytes(_emissive.ToInt32());
            Keystone.Utilities.JenkinsHash.Hash(data, ref result);

            data = BitConverter.GetBytes(_specular.ToInt32());
            Keystone.Utilities.JenkinsHash.Hash(data, ref result);

            data = BitConverter.GetBytes(_opacity);
            Keystone.Utilities.JenkinsHash.Hash(data, ref result);

            data = BitConverter.GetBytes(_specularPower);
            Keystone.Utilities.JenkinsHash.Hash(data, ref result);

            return (int)result;
        }

        public static Material Parse(string delimitedString)
        {
            // TODO: repository thread sycn issues is hurting us here until we can create thread safe repository

            // return existing Material if in Repository
            Material m = (Material)Repository.Get(delimitedString);
            if (m != null) return m;

            m = new Material(delimitedString);

            char[] delimiterChars = keymath.ParseHelper.English.XMLAttributeDelimiterChars;
            string[] values = delimitedString.Split(delimiterChars);

            // note: same delimiter is used between material elements and color elements
            m._diffuse = ColorFromTokens(new string[]{values[0], values[1], values[2], values[3]});
            m._ambient = ColorFromTokens(new string[] { values[4], values[5], values[6], values[7] });
            m._emissive = ColorFromTokens(new string[] { values[8], values[9], values[10], values[11] });
            m._specular = ColorFromTokens(new string[] { values[12], values[13], values[14], values[15] });
            m._specularPower = float.Parse(values[16]);
            m._opacity = float.Parse(values[17]);

            return m;
        }

        private static Color ColorFromTokens(string[] tokens)
        {
            Color c;
            c.r = float.Parse(tokens[0]);
            c.g = float.Parse(tokens[1]);
            c.b = float.Parse(tokens[2]);
            c.a = float.Parse(tokens[3]);

            return c;
        }

        public override string ToString()
        {
            string delimiter = keymath.ParseHelper.English.XMLAttributeDelimiter;

            // note: same delimiter is used between material elements and color elements since color.ToString() uses same delimiter
            return string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}",
                                 _diffuse.ToString(), delimiter,
                                 _ambient.ToString(), delimiter,
                                 _emissive.ToString(), delimiter,
                                 _specular.ToString(), delimiter,
                                 _specularPower.ToString(), delimiter,
                                 _opacity.ToString (), delimiter);

        }
    }
}