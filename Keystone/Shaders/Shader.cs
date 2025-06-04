using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Keystone.Elements;
using Keystone.IO;
using Keystone.RenderSurfaces;
using Keystone.Resource;
using MTV3D65;
using Keystone.Traversers;

namespace Keystone.Shaders
{
    /// <summary>
    /// 
    /// </summary>
    ///<remarks>http://forums.create.msdn.com/forums/p/90000/539613.aspx</remarks>
    public class Shader : Node, IPageableTVNode
    {
        private static readonly string[] NUMBERED_SEMANTICS = new string[]
        {
            "LIGHTDIRx_DIRECTION",
            "LIGHTPOINTx_POSITION",
            "LIGHTPOINTx_RANGE",
            "LIGHTDIRx_COLOR",
            "LIGHTPOINTx_COLOR",
            "TEXTUREx"
        };

        private static readonly string[] SEMANTICS = new string[] 
        {  
         "WORLD", "WORLDMATRIX", "MWORLD", "MATWORLD",
         "VIEW", "VIEWMATRIX",
         "PROJ", "PROJECTION", "PROJECTIONMATRIX", "PROJMATRIX",
         "WORLDVIEW", "WORLDVIEWMATRIX", "WV", "WVIEWMATRIX",
         "WORLDVIEWPROJ", "WVP", "WORLDVIEWPROJECTION", "WORLDVIEWPROJECTIONMATRIX",
         "VIEWPROJ", "VIEWPROJECTION", "VIEWPROJMATRIX", "VIEWPROJECTIONMATRIX",
         "VIEWI", "VIEWINVERSE", "VIEWINVERSEMATRIX", "VI",
         "WORLDIT", "WORLDINVERSETRANSPOSE", "WORLDTRANSPOSEINVERSE", "WIT",
         "VIEWIT", "VIEWINVERSETRANSPOSE", "VIEWTRANSPOSEINVERSE", "VIT",
         "CAMERAPOS", "CAMERAPOSITION", "VIEWPOS", "VIEWPOSITION",
         "TIME", "TIMECOUNT", "TICKCOUNT", "SECONDS",
         "LIGHTPOINT_NUM",
         "AMBIENT",
         "DIFFUSE",
         "EMISSIVE",
         "SPECULAR",
         "SPECULARPOWER",
         "FOGSTART",
         "FOGEND",
         "FOGDENSITY",
         "FOGCOLOR",
         "FOGTYPE", "FOG_TYPE"
        };
        
        protected RenderSurface _rs;
        private List<FXRenderStage> _stages = new List<FXRenderStage>();
        protected TVShader _tvShader;
        protected Dictionary<string, Settings.PropertySpec> mParameters;
        private KeyValuePair<string, string>[] mDefines; // used on creation of shader to disable/enable various parts of the code
        protected string mShaderResourceDescriptor; // not the same as ID since "ID" includes defines
        private readonly object mSyncRoot; 
        protected PageableNodeStatus _resourceStatus;
        private int mLastHashCode;

        static readonly char DEFINES_DELIMITER = '@';
                
        public Shader(string id)
            : base(id)
        {
        	string resourcePath;
        	
        	string[] substrings = id.Split (DEFINES_DELIMITER);
        	resourcePath = substrings[0];
        	
        	if (substrings.Length > 1)
        	{
        		System.Diagnostics.Debug.Assert (substrings.Length == 2);
        		// create the defines
        		mDefines = Appearance.GroupAttribute.GetDefinesFromString (substrings[1]);
        	}
        	
            mShaderResourceDescriptor = resourcePath;
            _resourceStatus = PageableNodeStatus.NotLoaded;

            // these are internally managed by GroupAttribute (similar to DomainObjects being managed by Entity)
            // however this flag can be set to true if needed for shaders that are not Model "appearance" in nature
            // but something like a post fx.
            SetFlagValue("serializable", false); 
            Shareable = true; // always shareable if the id's are the same.  keep in mind ids may contain serialized list of "defines"
            mSyncRoot = new object();
        }

        // for resources, i probably really should bind their "id" to the resourcePaths...
        // hrm, although if we want to use the same shader but with different vars set for rippling water, well actually the same underlying TVShader
        // could be used but we'd want to switch the params used on it... similar to applying changed Appearances to a Mesh3d... so this is kind of odd
        // because i was already considering just treating Shader like a Texture or Material that gets added to an Appearance.  Clearly there is "instance"
        // data as it relates to the current usage of a particular shader on a particular mesh/meshes.  A water shader used on one pond might need to look
        // different when used elsewhere.  So it's like, maybe we want a Shader as generic TVShader wrapper and then the other classes which we refer to
        // say SkyShader are like the Appearance nodes that will set the proper vars on the TVShader that is loaded.  The downside there is
        // we'd have to track all these vars change states or if we dont do that, then we wind up every frame for every mesh re-applying the settings it needs.
        // so in that light it's better to just load a completely different shader if we know we need seperate variables set in the shader for a particular
        // mesh instance.  So how do we know when to load a different shader?  Well, one way woudl be via the editor to explicitly load a new instance as
        // opposed to sharing...
        // SOLUTION:  Treat "shaders" as resources like Textures.  A user can "share" a shader by selecting it from the list of resources already added to the
        // the game world via the Resource browser, or they can import it again with a different name and set different shader params for it.
        public static string  CreateShaderName(string resourcePath, KeyValuePair<string, string>[] defines)
        {
            // in this case, the ID is resourcePath combined with defines
            // This is possible because the _id will never be converted
            // to a resourceDescriptor because
            // a) shaders with defines are never saved to disk or zip as seperate shaders
            //    The are only ever loaded dynamically from text.
            // b) the xml for these shaders are never saved to disk because they only
            //    exist as composite items under a ProceduralShader node.  Therefore
            //    never do we need to try and recreate this shader from an "id" that
            //    would not match any disk or zip path because the defines are included in the
            //    _id.
            // c) this doesn't mean these shaders are not shared... they are shared however 
            //   the _id is computed on the fly first.
            string id = resourcePath ;
            
            if (defines != null && defines.Length >  0)
                // compute the id and then we can see if the shader exists in the repository
            	id += DEFINES_DELIMITER + Appearance.GroupAttribute.GetDefinesToString(defines);

            return id;
        }


        #region ITraversable Members
        public override object Traverse(ITraverser target, object data)
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
            Settings.PropertySpec[] properties = new Settings.PropertySpec[2 + tmp.Length];
            tmp.CopyTo(properties, 2);

            // 'parmatercount' is readonly and  never serialized. Mostly helpful for user plugin and scripts to have consistant way for getting the property
            properties[0] = new Settings.PropertySpec("parmatercount", typeof(int).Name); 
            properties[1] = new Settings.PropertySpec("shader_parameters", typeof(Settings.PropertySpec[]).Name);
            
            if (!specOnly)
            {
                properties[0].DefaultValue = ParametersCount;
                properties[1].DefaultValue = Parameters;
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
                   // NOTE: since Shader is never serialized and only ever loaded from ResourceDescriptor within
                   // GroupAttribute/Appearance nodes, this SetProperties() never gets called.  Instead, parameters
                   // must be grabbed either from parsing the HLSL file or from TV3D helper functions.  Unfortunately
                   // TV3D helper functions are lacking in usefulness when it comes to float2, float3 and float4 types.
                   case "shader_parameters":
                        Settings.PropertySpec[] specs = (Settings.PropertySpec[])properties[i].DefaultValue;
                        if (specs != null)
                        	for(int j = 0; j < specs.Length; j++)
                        		mParameters.Add (specs[j].Name, specs[j]);
                        
                        break;
                }
            }
        }
        #endregion
        
        internal override Keystone.Traversers.ChildSetter GetChildSetter()
        {
            throw new System.Exception("The method or operation is not implemented.");
        }

        #region IPageableTVNode Members
        public object SyncRoot { get { return mSyncRoot; } }

        public virtual int TVIndex
        {
            get
            {
                if (_tvShader == null) return -1;
                else return _tvShader.GetIndex();
            }
        }

        public virtual bool TVResourceIsLoaded
        {
            get { return TVIndex > -1; }
        }

        public virtual string ResourcePath
        {
            get { return mShaderResourceDescriptor; }
            set { mShaderResourceDescriptor = value; }
        }

        public PageableNodeStatus PageStatus
        {
            get { return _resourceStatus; }
            set { _resourceStatus = value; }
        }

        public virtual void UnloadTVResource()
        {          
            DisposeUnmanagedResources();
        }
                
        public virtual void LoadTVResource()
        {

        	// NOTE: GroupAttribute.LoadTVResource() is responsible for loading correct shader
        	//       based on GroupAttribute.ResourcePath.  
            if (_resourceStatus == PageableNodeStatus.Loaded) return;

            int testIndexValue = -1;
            bool result = false;
            string lastErr = "";
            try
            {
                _tvShader = CoreClient._CoreClient.Scene.CreateShader(_id);
                testIndexValue = _tvShader.GetIndex();
            }
            catch (Exception ex)
            {
                // TODO: im not sure why this fails sometimes
                lastErr = _tvShader.GetLastError();
                System.Diagnostics.Debug.WriteLine("Shader.LoadTVResource() - Error creating Shader: " + _id + " " + lastErr);
                PageStatus = PageableNodeStatus.Error;
                return;
            }

            if (mDefines != null && mDefines.Length > 0)
                for (int i = 0; i < mDefines.Length; i++)
                {
                    _tvShader.AddDefine(mDefines[i].Key, mDefines[i].Value);
                }

            KeyCommon.IO.ResourceDescriptor descriptor = new KeyCommon.IO.ResourceDescriptor(mShaderResourceDescriptor);
            
            
            string file = mShaderResourceDescriptor;
            
            if (descriptor.IsArchivedResource)
            {
                System.Runtime.InteropServices.GCHandle gchandle;
                string memoryFile = Keystone.ImportLib.GetTVDataSource(descriptor.EntryName, "", Keystone.Core.FullNodePath(descriptor.ModName), out gchandle);
                if (string.IsNullOrEmpty(memoryFile)) throw new Exception(string.Format("Shader.LoadTVResource() - Error importing '{0}' from archive.", memoryFile));
                // tdoo: if the memoryFile is empty, rather than throw exception, we should set
                // ResourceLoadStatus to error or something

                file = memoryFile;
                result = _tvShader.CreateFromEffectFile(file);

                if (gchandle != null)
                    gchandle.Free();
            }
            else
            {
                string path = Keystone.Core.FullNodePath (descriptor.EntryName);
                //if (path.Contains("pssm"))
                //    path = @"D:\dev\c#\KeystoneGameBlocks\Data\pool\shaders\pssm\toaster_pssm.fx";
                file = path;
            	result = _tvShader.CreateFromEffectFile(file);
            }
            System.Diagnostics.Debug.Assert (testIndexValue == _tvShader.GetIndex());
            
            if (!result)
            {

                if (_tvShader != null)
                {
                    lastErr = _tvShader.GetLastError();
                    _tvShader = null;
                }
                throw new Exception("Shader.LoadTVResource() - Error creating '" + _id + "' " + lastErr);
            }
            else
            {
                //_tvShader.SetTechnique("primary");
                //_tvShader.SetTechnique("stencil_target");
                //_tvShader.SetTechnique("stencil_source");
                // NOTE: All our shaders must contain all of the techniques
                //       But what about transparency pass that is always done in forward?
                //       An easy way might be too just have all those techniques still point
                //       to same vertex and pixel shaders
                // CoreClient._CoreClient.Settings.settingRead("graphics", "technique");
                // TODO: technique should be stored in mValues and be a parameter 
                // that AppearanceGroup can define
                string technique = "primary";
                _tvShader.SetTechnique(technique);

// March.10.2015 - obsolete.  We manually define parameters ourselves.
//                if (ParametersCount > 0)
//                {
//                    mParameters = GetParameters();
//                }
                System.Diagnostics.Debug.WriteLine("Shader.LoadTVResource() - SUCCESS. '" + _id + "'");
                SetChangeFlags(Keystone.Enums.ChangeStates.ShaderFXLoaded | 
                               Keystone.Enums.ChangeStates.ShaderParameterValuesChanged, Keystone.Enums.ChangeSource.Self);
            }
            
        }

        public void SaveTVResource(string filepath)
        {
            throw new NotImplementedException();
        }
        #endregion

        public virtual RenderSurface RenderSurface
        {
            get { return _rs; }
            set { _rs = value; }
        }

        public FXRenderStage[] Stages
        {
            get { return _stages.ToArray(); }
        }

        public virtual TVShader TVShader
        {
            get { return _tvShader; }
        }

        public virtual int PassCount
        {
            get
            {
                if (_tvShader == null) return 0;
                return _tvShader.GetPassCount();
            }
        }

        public virtual int TechniqueCount
        {
            get
            {
                if (_tvShader == null) return 0;
                return _tvShader.GetTechniqueCount();
            }
        }

        public virtual string[] Techniques
        {
            get
            {
                if (_tvShader == null || TechniqueCount == 0) return null;
                string[] results = new string[TechniqueCount];
                for (int i = 0; i < results.Length; i++)
                    results[i] = _tvShader.GetTechniqueName(i);

                return results;
            }
        }

        public void SetTechnique (string name)
        {
        	_tvShader.SetTechnique (name);
        }

        public virtual int ParametersCount 
        {
            get
            {
                if (_tvShader == null) return 0;
                return _tvShader.GetEffectParameterCount();
            }
        }

        protected bool IsNonTextureSemantic(int index, string name)
        {
            // iterate against all semantics and see if this is one
            for (int i = 0; i < SEMANTICS.Length; i++)
            {
                int foundIndex = _tvShader.GetEffectParamBySemantic(SEMANTICS[i]);
                if (foundIndex == index) return true;
            }

            // iterate against the numbered semantics replacing "x" with number
            // and see if find one at same index
            for (int i = 0; i < NUMBERED_SEMANTICS.Length; i++)
            {
                for (int j = 0; j <= 9; j++)
                {
                    string temp = NUMBERED_SEMANTICS[i].Replace ("x", j.ToString());
                    int foundIndex = _tvShader.GetEffectParamBySemantic(temp);
                    if (foundIndex == index) return true;
                }
            }

            // string regexResult = Regex.Match(name, @"\d+").Value;
            // if (string.IsNullOrEmpty(regexResult) == false) 
            //  int num = int.Parse(regexResult, NumberFormatInfo.InvariantInfo); 

            return false;
        }


        #region Parameters        
        public Settings.PropertySpec[] Parameters
        {
            get 
            { 
            	if (mParameters == null || mParameters.Count == 0) return null;
            	
            	Settings.PropertySpec[] results = new Settings.PropertySpec[mParameters.Count];
            	mParameters.Values.CopyTo (results, 0);
            	return results;
            }
        }
//        // parameters are fixed for any given shader, but the values
//        // can be unique per instance and thus must be stored in the AppearanceGroup node
//        // which cannot be shared.
//        // also scripts can directly update parameters.
//        // next, an AppearanceGroup should determine if the current values assigned
//        // in the Shader.Parameters matches those in it's group, and if so, it does not
//        // have to assigned those parameters at all.
//        // A hashcode of the values could be used to make this test fast.
//        // In fact, if the AppearanceGroup has the script, then the .Apply()
//        // where it tests for hashcode could do this job too and only update
//        // parameters if they're changed.. in other words, the scripts may
//        // assign new values, but most times those values wont change...
//        // although the downside is running scripts when nothing has changed... not sure
//        // how that would be prevented since there's no way of knowing if something will change
//        // before running the script...
//        private Dictionary<string, Settings.PropertySpec> GetParameters() // see domain object for how to handle these
//        {
//            return Parameters ;
//            
//// March.10.2015 - Trying to rely on TV3D to grab the parameter types is a cluster fuck.  Especially with regards to 
//// TV_SHADERPARAMETER_VECTOR.  TV3D shows float2, float3 and float4 all as TV_SHADERPARAMETER_VECTOR
//// so we don't know which "real" type it is whether Color, Vector3f, Vector2f, etc
//// We're better off manually defining the available parameters ourselves.  See "this.DefineShaderParameter()"
//            
////            // TODO: rather than rebuild Parameters[] every Get() we should cache
////            // and update only when the shader code itself has changed.(eg been reompiled 
////            // and saved at same file path)
////            if (ParametersCount == 0) return null;
////
////            Dictionary<string, Settings.PropertySpec> results = new Dictionary<string, Settings.PropertySpec>();
////            for (int i = 0; i < ParametersCount; i++)
////            {
////                Settings.PropertySpec spec = new Settings.PropertySpec();
////                spec.Name = _tvShader.GetEffectParamName(i);
////
////                // skip this parameter if it's a SEMANTIC that is NOT used for Texture slots. eg TEXTURE# semantic
////                if (IsNonTextureSemantic(i, spec.Name)) continue;
////               
////
////                CONST_TV_SHADERPARAMETERTYPE type = _tvShader.GetEffectParamType(i);
////               
////             
////                // TODO: The defaultValue can come from shader when the shader is loaded
////                // and referenced by a new parent (AppearanceGroup object)however
////                // once that occurs, subsequent values MUST come from the cached PropertySpec[]
////                // parameters.
////
////                switch (type)
////                {
////                    case CONST_TV_SHADERPARAMETERTYPE.TV_SHADERPARAMETER_STRING:
////                        spec.TypeName = typeof(string).Name;
////                        // TODO: ask Sylvain why not .SetEffectParamString() !?
////                        spec.DefaultValue = _tvShader.GetEffectParamString(spec.Name);
////                        break;
////                    case CONST_TV_SHADERPARAMETERTYPE.TV_SHADERPARAMETER_bool:
////                        spec.TypeName = typeof(bool).Name;
////                        spec.DefaultValue = _tvShader.GetEffectParamBoolean(spec.Name);
////                        break;
////                    case CONST_TV_SHADERPARAMETERTYPE.TV_SHADERPARAMETER_INTEGER:
////                        spec.TypeName = typeof(int).Name;
////                        spec.DefaultValue = _tvShader.GetEffectParamInteger(spec.Name);
////                        break;
////                    case CONST_TV_SHADERPARAMETERTYPE.TV_SHADERPARAMETER_FLOAT:
////                        spec.TypeName = typeof(float).Name;
////                        spec.DefaultValue = _tvShader.GetEffectParamFloat(spec.Name);
////                        break;
////                    case CONST_TV_SHADERPARAMETERTYPE.TV_SHADERPARAMETER_VECTOR:
////                        // TODO: this should be a float[4] array whereas our Vector3d is only
////                        // storing x,y,z... doesn't restore w !!!!
////                        // TODO: also, TV is doing a lousy job.. 
////                        spec.TypeName = typeof(Keystone.Types.Vector4).Name;
////                        spec.DefaultValue = Helpers.TVTypeConverter.FromTVVector(_tvShader.GetEffectParamVector(spec.Name));
////                        break;
////                    case CONST_TV_SHADERPARAMETERTYPE.TV_SHADERPARAMETER_MATRIX:
////                        spec.TypeName = typeof(Keystone.Types.Matrix).Name;
////                        spec.DefaultValue = Helpers.TVTypeConverter.FromTVMatrix (_tvShader.GetEffectParamMatrix(spec.Name));
////                        break;
////
////                    case CONST_TV_SHADERPARAMETERTYPE.TV_SHADERPARAMETER_TEXTURE:
////                        continue; // i think these end up showing the Samplers and not the Texture slots
////                        spec.TypeName = typeof(int).Name;
////
////                        // we want the semantic name so we know which texture
////                        // we dont really care about the name used
////
////                        // so loop thru 0 - 7 TEXTURE# semantic names to find the correct one
////                        int layerID = -1;
////                        for (int j = 0; j <= 7; j++)
////                        {
////                            int result = _tvShader.GetEffectParamBySemantic("TEXTURE" + j.ToString());
////                            if (result == i)
////                            {
////                                layerID = j;
////                                break;
////                            }
////                        }
////                        spec.DefaultValue = layerID;
////                        // for textures the spec's name is the semantic name and not
////                        // the user's custom parameter name
////                        spec.Name = "TEXTURE" + layerID.ToString();
////                        if (layerID == -1)
////                            spec.Name = spec.Name; // if something went wrong, use the spec name
////                        break;
////                    //  fill our appearance tab with the texture slots that are available
////                    //  when using this AppearanceFX
////                    
////                    case CONST_TV_SHADERPARAMETERTYPE.TV_SHADERPARAMETER_UNKNOWN:
////                    default:
////                        continue; // UNKNOWN is usually a sampler.  Ignore it.
////                                     // don't add it because setting unknown types 
////                                  // via the plugin may break the shader and odds are 
////                                  // this is some type that should only be set by semantic
////                        break;
////
////
////                }
////
////                results.Add(spec.Name, spec);
////            }
////            return results;
//            
//        }
        

        internal virtual void SetShaderParameter(string parameterName, string typeName, object value)
        {
            // the shader may have no value by default and thus attempt to set null value
            if (value == null) return;

     
            switch (typeName)
            {
                case "Single":
                case "Float":
                    SetShaderParameterFloat(parameterName, (float)value);
                    break;
               case "Color":
                    SetShaderParameterColor (parameterName, (Keystone.Types.Color)value);
                    break;
                case "Vector4": // used for our explosion texture animation
                    SetShaderParameterVector(parameterName, (Keystone.Types.Vector4)value);
                    break;
                case "Vector3d":
                    SetShaderParameterVector(parameterName, (Keystone.Types.Vector3d)value);
                    break;
               case "Vector2f":
                    SetShaderParameterVector2 (parameterName, (Keystone.Types.Vector2f)value);
                    break;
                default:
                    //System.Diagnostics.Debug.WriteLine("Shader parameter type = " + typeName + " value = " + value.ToString()); 
                   // System.Diagnostics.Debug.Assert(false, "Type Not implemented.");
                    break;
            }
        }

        // HACK - The following SetShaderParemetXXXXX should be "internal" but to make it easier for VisualFXAPI to directly
        // set them quickly and without actually saving the value in the mParameters, we just leave them "public" 
        // TODO: in order for a shader to know if it should skip a certain call
        // because it knows the shader already contains that same value for a parameter
        // it must keep a dictionary of those parameter values.  This way at least
        // the script can make this call directly, but there's still a good chance the
        // call wont actually have to be made to the tvshader
        public void SetShaderParameterString(string parameterName, string value)
        {
            throw new NotImplementedException ("TV has no Setter for strings even though it does have Getter");
            // about the only time i think this would be used is to set the texture path
            // but that really shouldn't even be done.  You dont want the shader to load textures
            // at runtime.
        }

        public void SetShaderParameterTexture(string parameterName, int value)
        {
            _tvShader.SetEffectParamTexture (parameterName, value);
        }

        public void SetShaderParameterColor(string parameterName, Types.Color color)
        {

            TV_COLOR c;
            c.r = color.r;
            c.g = color.g;
            c.b = color.b;
            c.a = color.a;

            _tvShader.SetEffectParamColor(parameterName, c);
            
        }

        public void SetShaderParameterBool(string parameterName, bool value)
        {
            _tvShader.SetEffectParamBoolean(parameterName, value);
        }

        public void SetShaderParameterInteger(string parameterName, int value)
        {
            _tvShader.SetEffectParamInteger(parameterName, value);
        }

        public void SetShaderParameterFloat(string parameterName, float value)
        {
            _tvShader.SetEffectParamFloat(parameterName, value);
        }

        public void SetShaderParameterFloatArray(string parameterName, float[] value)
        {
            _tvShader.SetEffectParamFloatArray(parameterName, value, value.Length);
        }

        /// <summary>
        /// Array of 32 bit raw values can be fitted into any vars on the target shader as
        //  long as it can hold the exact number of bits.  In other words, you could send
        //  an array of 16 floats here and have those values sent to a 4x4 matrix.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        public void SetShaderParameterRawFloatArray(string parameterName, float[] value)
        {
            _tvShader.SetEffectParamRawFloatArray(parameterName, value, value.Length);
        }

        public void SetShaderParameterVector(string parameterName, Keystone.Types.Vector3d value)
        {
            TV_3DVECTOR v;
            v.x = (float)value.x;
            v.y = (float)value.y;
            v.z = (float)value.z;
            _tvShader.SetEffectParamVector3(parameterName, v);
        }

        public void SetShaderParameterVector(string parameterName, Keystone.Types.Vector3f value)
        {
            TV_3DVECTOR v;
            v.x = value.x;
            v.y = value.y;
            v.z = value.z;
            _tvShader.SetEffectParamVector3(parameterName, v);
        }
                
        public void SetShaderParameterVectorArray(string parameterName, TV_3DVECTOR[] value)
        {
            _tvShader.SetEffectParamVectorArray3(parameterName, value, value.Length);
        }

        public void SetShaderParameterVectorArray(string parameterName, Keystone.Types.Vector3d[] value)
        {
            throw new NotImplementedException();
        }
        
        public void SetShaderParameterVector2(string parameterName, float x, float y)
        {
            TV_2DVECTOR v;
            v.x = x;
            v.y = y;
            _tvShader.SetEffectParamVector2(parameterName, v);
        }
        
        public void SetShaderParameterVector2(string parameterName, Keystone.Types.Vector2f value)
        {
            TV_2DVECTOR v;
            v.x = (float)value.x;
            v.y = (float)value.y;
            _tvShader.SetEffectParamVector2(parameterName, v);
        }

        public void SetShaderParameterVector2Array(string parameterName, Keystone.Types.Vector2f[] value)
        {
            throw new NotImplementedException();
        }

        public void SetShaderParameterVector(string parameterName, Keystone.Types.Vector4 value)
        {
            TV_4DVECTOR v;
            v.x = (float)value.x;
            v.y = (float)value.y;
            v.z = (float)value.z;
            v.w = (float)value.w;
          
            _tvShader.SetEffectParamVector(parameterName, v);
        }

        public void SetShaderParameterVector4(string parameterName, Keystone.Types.Vector3d[] value)
        {
            throw new NotImplementedException();
        }

        public void SetShaderParameterMatrix(string parameterName, Keystone.Types.Matrix value)
        {
            TV_3DMATRIX matrix = Helpers.TVTypeConverter.ToTVMatrix(value);
            _tvShader.SetEffectParamMatrix(parameterName, matrix);
        }

        public void SetShaderParameterMatrixArray(string parameterName, Keystone.Types.Matrix[] value)
        {
            TV_3DMATRIX[] matrix = Helpers.TVTypeConverter.ToTVMatrix(value);
            _tvShader.SetEffectParamMatrixArray(parameterName, matrix, matrix.Length);
        }
        
        public void SetShaderParameterMatrixArray(string parameterName,TV_3DMATRIX[] matrix)
        {
            _tvShader.SetEffectParamMatrixArray(parameterName, matrix, matrix.Length);
        }
        #endregion

        // TODO: following UpdateParameters methods may be obsolete as now
        // we will allow the KeystoneGameBlocks\KeyScripts\Interfaces\IvisualFX.cs  
        // API to be used so that the parameters can be assigned from user's entity scripts.
        // Thus no need to have hardcoded app code.
        //
        // signature
        protected delegate void UpdateParametersFunction(Node target);
        // pointer
        protected UpdateParametersFunction mUpdateMethod;
        // accessor/wrapper method
        public void UpdateParameters(Node target)
        {
            if (mUpdateMethod != null)
                mUpdateMethod.Invoke(target);
        }

        // so now, what if potentially we allow our App.Exe to define these new functions...
        // This problem is essentially identicle to the one with Controller Widgets where
        // we want our keystone.dll library fixed, but ability to assign event handlers to them
        // from our app.exe


        public virtual void Update(double elapsedSeconds)
        {
            if (Enable)
            {
                // TODO: our scripts perhaps replace these FXRenderStages?
                foreach (FXRenderStage stage in _stages)
                {
                    stage.Update();
                }
            }
        }

        public void AddStage(FXRenderStage stage)
        {
            _stages.Add(stage);
        }

        public void RemoveStage(FXRenderStage stage)
        {
            if (_stages.Contains(stage))
            {
                _stages.Remove(stage);
            }
            else
            {
                throw new ArgumentException("Shader:removeStage() -- Stage does not exist in this shader.");
            }
        }

        public Int32 StageCount()
        {
            return _stages.Count;
        }

        

        public override int GetHashCode()
        {
            uint result = 0;

            // keep passing the same "result" to combine the hash of subsequent parameters            
            Keystone.Utilities.JenkinsHash.Hash(_id, ref result);

            if (mParameters != null && mParameters.Count > 0)
                foreach (Settings.PropertySpec spec in mParameters.Values)
                { 
                    // TODO: You know, when we set these values we should be able to
                    // just set the hashcode because we already know that if a groupattribute
                    // moves all its values to this shader, that the hashcode should match!
                    // So there's only the need to compute the hashcode when they are changed
                    // in the GroupAttribute/Appearance!
                }

            mLastHashCode = (int)result;
            return mLastHashCode;
        }
        
        #region IDisposable Members
        protected override void DisposeManagedResources()
        {
            //try
            //{
            //    if (_tvShader != null)
            //        _tvShader.Destroy();
            //}
            //catch { System.Diagnostics.Trace.WriteLine("Shader.DisposeUnmanagedResources() -- Error destroying shader '" + _id + "'"); }

        }

        protected override void DisposeUnmanagedResources()
        {
            try
            {
                if (_tvShader != null)
                    _tvShader.Destroy();
            }
            catch { System.Diagnostics.Trace.WriteLine("Shader.DisposeUnmanagedResources() -- Error destroying shader '" + _id + "'"); }

        }
        #endregion
    }
}