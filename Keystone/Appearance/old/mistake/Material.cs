using System;
using System.Xml;
using Keystone.Elements;
using Keystone.IO;
using Keystone.Traversers;
using Keystone.Resource;
using Keystone.Types;


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
    //Ambient reflection, like ambient light, is nondirectional. Ambient reflection has a lesser 
    //impact on the apparent color of a rendered object, but it does affect the overall color and 
    //is most noticeable when little or no diffuse light reflects off the material. A material's
    //ambient reflection is affected by the ambient light set for a scene by calling the 
    //IDirect3DDevice9::SetRenderState method with the D3DRS_AMBIENT flag.
    // Diffuse and ambient reflection work together to determine the perceived color of an object, and are usually identical values. For example, to render a blue crystalline object, you create a material that reflects only the blue component of diffuse and ambient light. When placed in a room with a white light, the crystal appears to be blue. However, in a room that has only red light, the same crystal would appear to be black, because its material doesn't reflect red light.
    // Emission
    // ========
    // Materials can be used to make a rendered object appear to be self-luminous. The Emissive member of the D3DMATERIAL9 structure is used to describe the color and transparency of the emitted light. Emission affects an object's color and can, for example, make a dark material brighter and take on part of the emitted color.

    // You can use a material's emissive property to add the illusion that an object is emitting light, without incurring the computational overhead of adding a light to the scene. In the case of the blue crystal, the emissive property is useful if you want to make the crystal appear to light up, but not cast light on other objects in the scene. Remember, materials with emissive properties don't emit light that can be reflected by other objects in a scene. To achieve this reflected light, you need to place an additional light within the scene.
    // Specular Reflection
    // ==================
    // Specular Reflection
    // Specular reflection creates highlights on objects, making them appear shiny. The D3DMATERIAL9 structure contains two members that describe the specular highlight color as well as the material's overall shininess. You establish the color of the specular highlights by setting the Specular member to the desired RGBA color - the most common colors are white or light gray. The values you set in the Power member control how sharp the specular effects are.
    // Specular highlights can create dramatic effects. Drawing again on the blue crystal analogy: a larger Power value creates sharper specular highlights, making the crystal appear to be quite shiny. Smaller values increase the area of the effect, creating a dull reflection that make the crystal look frosty. To make an object truly matte, set the Power member to zero and the color in Specular to black. Experiment with different levels of reflection to produce a realistic appearance for your needs. The following illustration shows two identical models.
    public class Material : Node, IPageableTVNode
    {
        public enum DefaultMaterials
        {
            matte, // matte is purely diffuse material that spreads all light evenly in all directions
            red,
            green,
            blue,
            yellow,
            red_fullbright,
            green_fullbright,
            blue_fullbright,
            yellow_fullbright,
            gold,
            silver,
            bronze, 
            copper,
            gunmetal,
            asphalt,
            pool,
            lake,
            pond,
            ocean,
            tapwater
        }

        private float _specularPower = 20;
        private float _opacity;
        private Color _diffuse;
        private Color _ambient;
        private Color _specular;
        private Color _emissive;
        protected int _tvfactoryIndex = -1;
        protected string _resourcePath;
        protected PageableResourceStatus _resourceStatus;

        // constructor for an already created TVMATERIAL?  Probably shouldnt be allowed
        // because we want to funnel creation of materials to a single point so we can properly
        // track references.  
        // TODO: Actually materials can sometimes get added via the bLoadMaterials , bloadTextures of tvm and .x files
        // but maybe there we ccan still make them instance this constructor... for textures too.
        public static Material Create(string id, Color diffuse, Color ambient, Color specular, Color emissive)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException("Material name cannot be null or empty.");
            // NOTE: this should match with thekey also already existing in the repository.
            // The way this works is, when creating an object during "load" you should check the repository first
            // and only attempt a new instance if a previous doesnt exist.  We don't let this Create() call
            // assume you meant to get a reference to a shared copy.  You must do that explicitly through the repository.
            //if (CoreClient._CoreClient.MaterialFactory.GetMaterialByName(id) > 0)
            //    throw new ArgumentException("Material with this name already exists.");


            Material material = (Material)Repository.Get(id);
            if (material != null) return material;

            material = new Material(id, diffuse, ambient, specular, emissive, 0.888f, 0.009f);
            return material;
        }

        /// <summary>
        /// Instances a Material object from a TVMaterialFactory index value
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static Material Create(int index)
        {
            Material material = (Material)Repository.Get(CoreClient._CoreClient.MaterialFactory.GetMaterialName(index));
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

        public static Material Create(DefaultMaterials material)
        {
            Material tmp = null;
            string name = Repository.GetNewName(typeof (Material)); // "DefaultMaterials_" + material.ToString();
            // determine if the material is already in the repository
            tmp = (Material)Repository.Get(name);

            // create a new material
            if (tmp == null)
            {
                int index = CoreClient._CoreClient.MaterialFactory.CreateMaterial(name);
                
                switch (material)
                {
                    case DefaultMaterials.matte:
                        CoreClient._CoreClient.MaterialFactory.SetDiffuse(index, 1, 1, 1, 1);
                        CoreClient._CoreClient.MaterialFactory.SetAmbient(index, 0, 0, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetSpecular(index, 0, 0, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetEmissive(index, 0, 0, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetOpacity(index, 1);
                        CoreClient._CoreClient.MaterialFactory.SetPower(index, 0);
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
                   case DefaultMaterials.red:
                        CoreClient._CoreClient.MaterialFactory.SetDiffuse(index, 1, 0, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetAmbient(index, 0, 0, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetSpecular(index, 0, 0, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetEmissive(index, 0, 0, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetOpacity(index, 1);
                        CoreClient._CoreClient.MaterialFactory.SetPower(index, 0);
                        break;
                    case DefaultMaterials.green :
                        CoreClient._CoreClient.MaterialFactory.SetDiffuse(index, 0, 1, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetAmbient(index, 0, 0, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetSpecular(index, 0, 0, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetEmissive(index, 0, 0, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetOpacity(index, 1);
                        CoreClient._CoreClient.MaterialFactory.SetPower(index, 0);
                        break;
                    case DefaultMaterials.blue:
                        CoreClient._CoreClient.MaterialFactory.SetDiffuse(index, 0, 0, 1, 1);
                        CoreClient._CoreClient.MaterialFactory.SetAmbient(index, 0, 0, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetSpecular(index, 0, 0, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetEmissive(index, 0, 0, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetOpacity(index, 1);
                        CoreClient._CoreClient.MaterialFactory.SetPower(index, 0);
                        break;
                    case DefaultMaterials.yellow:
                        CoreClient._CoreClient.MaterialFactory.SetDiffuse(index, 1, 1, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetAmbient(index, 0, 0, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetSpecular(index, 0, 0, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetEmissive(index, 0, 0, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetOpacity(index, 1);
                        CoreClient._CoreClient.MaterialFactory.SetPower(index, 0);
                        break;
                    case DefaultMaterials.red_fullbright:
                        CoreClient._CoreClient.MaterialFactory.SetDiffuse(index, 0, 0, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetAmbient(index, 0, 0, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetSpecular(index, 0, 0, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetEmissive(index, 1, 0, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetOpacity(index, 1);
                        CoreClient._CoreClient.MaterialFactory.SetPower(index, 0);
                        break;
                    case DefaultMaterials.green_fullbright :
                        CoreClient._CoreClient.MaterialFactory.SetDiffuse(index, 0, 0, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetAmbient(index, 0, 0, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetSpecular(index, 0, 0, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetEmissive(index, 0, 1, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetOpacity(index, 1);
                        CoreClient._CoreClient.MaterialFactory.SetPower(index, 0);
                        break;
                    case DefaultMaterials.blue_fullbright:
                        CoreClient._CoreClient.MaterialFactory.SetDiffuse(index, 0, 0, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetAmbient(index, 0, 0, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetSpecular(index, 0, 0, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetEmissive(index, 0, 0, 1, 1);
                        CoreClient._CoreClient.MaterialFactory.SetOpacity(index, 1);
                        CoreClient._CoreClient.MaterialFactory.SetPower(index, 0);
                        break;

                    case DefaultMaterials.yellow_fullbright :
                        CoreClient._CoreClient.MaterialFactory.SetDiffuse(index, 0, 0, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetAmbient(index, 0, 0, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetSpecular(index, 0, 0, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetEmissive(index, 1, 1, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetOpacity(index, 1);
                        CoreClient._CoreClient.MaterialFactory.SetPower(index, 0);
                        break;

                    case DefaultMaterials.silver: //192, 192, 192
                        CoreClient._CoreClient.MaterialFactory.SetDiffuse(index, 0.75f, 0.75f, 0.75f, 1);
                        CoreClient._CoreClient.MaterialFactory.SetAmbient(index, 0, 0, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetSpecular(index, 0, 0, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetEmissive(index, 0, 0, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetOpacity(index, 1);
                        CoreClient._CoreClient.MaterialFactory.SetPower(index, 0);
                        break;

                    case DefaultMaterials.gold: // 255, 215, 0
                        CoreClient._CoreClient.MaterialFactory.SetDiffuse(index, 1.0f, 0.83984375f, 0.0f, 1);
                        CoreClient._CoreClient.MaterialFactory.SetAmbient(index, 0, 0, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetSpecular(index, 0, 0, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetEmissive(index, 0, 0, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetOpacity(index, 1);
                        CoreClient._CoreClient.MaterialFactory.SetPower(index, 0);
                        break;

                    case DefaultMaterials.bronze: // 205, 127, 50
                        CoreClient._CoreClient.MaterialFactory.SetDiffuse(index, 0.80078125f, 49609375f, 0.1953125f, 1);
                        CoreClient._CoreClient.MaterialFactory.SetAmbient(index, 0, 0, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetSpecular(index, 0, 0, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetEmissive(index, 0, 0, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetOpacity(index, 1);
                        CoreClient._CoreClient.MaterialFactory.SetPower(index, 0);
                        break;

                    case DefaultMaterials.copper: // 184, 115, 51
                        CoreClient._CoreClient.MaterialFactory.SetDiffuse(index, 0.71875f, 44921875f, 0.19921875f, 1);
                        CoreClient._CoreClient.MaterialFactory.SetAmbient(index, 0, 0, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetSpecular(index, 0, 0, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetEmissive(index, 0, 0, 0, 1);
                        CoreClient._CoreClient.MaterialFactory.SetOpacity(index, 1);
                        CoreClient._CoreClient.MaterialFactory.SetPower(index, 0);
                        break;

                    default:
                        throw new Exception("DefaultMaterial not yet implemented.");

                }
            
                tmp = Material.Create(index);
            }

            return tmp;
        }

        public static Material Create(string id)
        {
            Material mat;
            mat = (Material) Repository.Get(id);
            if (mat != null) return mat;
            mat = new Material(id);
            return mat;
        }

        private Material(string id) : base(id)
        {
        }

        private Material(string id, Color diffuse, Color ambient, Color specular, Color emissive, float opacity,
                         float power)
            : base(id)
        {
            _tvfactoryIndex = CoreClient._CoreClient.MaterialFactory.CreateMaterial(_id);
            Diffuse = diffuse;
            Ambient = ambient;
            Specular = specular;
            Emissive = emissive;
            Opacity = opacity;
            SpecularPower = power;
        }

        private Material(int index, string id, Color diffuse, Color ambient, Color specular, Color emissive,
                         float opacity, float power)
            : base(id)
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
            SetChangeFlags(Keystone.Enums.ChangeStates.AppearanceChanged, Keystone.Enums.ChangeSource.Self);
        }

        #region IPageableTVNode Members
        public int TVIndex
        {
            get { return _tvfactoryIndex; }
        }

        public bool TVResourceIsLoaded
        {
            get { return _tvfactoryIndex > -1; }
        }

        public PageableResourceStatus ResourceStatus { get { return _resourceStatus; } set { _resourceStatus = value; } }

        public void LoadTVResource()
        {
            _tvfactoryIndex = CoreClient._CoreClient.MaterialFactory.CreateMaterial(_id);
            // even with the private vars loaded, we still need to call the actual properties to get the TVMaterialFactory to set the colors for the material
            Diffuse = _diffuse;
            Ambient = _ambient;
            Specular = _specular;
            Emissive = _emissive;
            Opacity = _opacity;
            SpecularPower = _specularPower;
            SetChangeFlags(Keystone.Enums.ChangeStates.AppearanceChanged, Keystone.Enums.ChangeSource.Self);
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
            CoreClient._CoreClient.MaterialFactory.DeleteMaterial(_tvfactoryIndex);
            base.DisposeUnmanagedResources();
        }
        #endregion

        public override void Traverse(ITraverser target)
        {
            target.Apply(this);
        }

        public override ChildSetter GetChildSetter()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Valid Range 0.0 - 1.0f.  Equivalent to setting the diffuse alpha of the Material.  Since only the diffuse alpha is used in Material
        /// SetOpacity is a shortcut.  For Mesh and Actors, must also set .SetBlendingMode  (CONST_TV_BLENDINGMODE.TV_BLEND_ALPHA)
        /// </summary>
        public float Opacity
        {
            get { return _opacity; }
            set
            {
                _opacity = value;
                if (TVResourceIsLoaded)
                {
                    CoreClient._CoreClient.MaterialFactory.SetOpacity(_tvfactoryIndex, value);
                    // note: if we check the diffuse after setting opacity, it should reflect
                    // in the .a component of the diffuse
                    MTV3D65.TV_COLOR diffuse = CoreClient._CoreClient.MaterialFactory.GetDiffuse(_tvfactoryIndex);
                }
                SetChangeFlags(Keystone.Enums.ChangeStates.AppearanceChanged, Keystone.Enums.ChangeSource.Self);
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
                if (TVResourceIsLoaded)
                   CoreClient._CoreClient.MaterialFactory.SetPower(_tvfactoryIndex, value);
                SetChangeFlags(Keystone.Enums.ChangeStates.AppearanceChanged, Keystone.Enums.ChangeSource.Self);
            }
        }

        public Color Diffuse
        {
            get { return _diffuse; }
            set
            {
                _diffuse = value;
                //System.Diagnostics.Debug.WriteLine("RGB= " + value.r.ToString() + ", " + value.g.ToString() + ", " + value.b.ToString());
                if (TVResourceIsLoaded )
                    CoreClient._CoreClient.MaterialFactory.SetDiffuse(_tvfactoryIndex, value.r, value.g, value.b, value.a);
                SetChangeFlags(Keystone.Enums.ChangeStates.AppearanceChanged, Keystone.Enums.ChangeSource.Self);
            }
        }

        public Color Ambient
        {
            get { return _ambient; }
            set
            {
                _ambient = value;
                if (TVResourceIsLoaded)
                    CoreClient._CoreClient.MaterialFactory.SetAmbient(_tvfactoryIndex, value.r, value.g, value.b, value.a);
                SetChangeFlags(Keystone.Enums.ChangeStates.AppearanceChanged, Keystone.Enums.ChangeSource.Self);
            }
        }

        public Color Emissive
        {
            get { return _emissive; }
            set
            {
                _emissive = value;
                if (TVResourceIsLoaded)
                    CoreClient._CoreClient.MaterialFactory.SetEmissive(_tvfactoryIndex, value.r, value.g, value.b, value.a);
                SetChangeFlags(Keystone.Enums.ChangeStates.AppearanceChanged, Keystone.Enums.ChangeSource.Self);
            }
        }

        /// <summary>
        /// Requires that the light have enable specular set (eg. TVLightEngine.SetSpecularLighting(true);
        /// 
        /// </summary>
        public Color Specular
        {
            get { return _specular; }
            set
            {
                _specular = value;
                if (TVResourceIsLoaded)
                    CoreClient._CoreClient.MaterialFactory.SetSpecular(_tvfactoryIndex, value.r, value.g, value.b, value.a);
                SetChangeFlags(Keystone.Enums.ChangeStates.AppearanceChanged, Keystone.Enums.ChangeSource.Self);
            }
        }
    }
}