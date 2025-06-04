using System;
using System.Collections.Generic;
using Keystone.Elements;
using Keystone.Extensions;
using Keystone.IO;
using Keystone.Shaders;
using Keystone.Traversers;
using MTV3D65;

namespace Keystone.Appearance
{
    // TODO: I should probably just rename this to GroupAppearance

    // GroupAttribute objects are added to Appearance nodes.
    // Appearance node's can contain multiple GroupAttribute nodes.
    // A single GroupAttribute node corresponds to a single Mesh or Actor group or Terrain chunk
    public class GroupAttribute : Group, IPageableTVNode 
    {
        // NOTE: no seperate lighting modes per group allowed.  Only one under the primary
        //       Appearance node.
        internal int GroupID = -1;
        //protected int mGeometryType = 0; // Mesh = 0, actor = 1, TexturedQuad = 2, Minimesh = 3
        protected string mTVName; // the group name which can be read/set by Get/SetGroupName 

        protected string mShaderResourceDescriptor; // this does NOT include any DEFINES
        internal KeyValuePair<string, string>[] mDefines;
        protected Material mMaterial;
        protected Shader mShader;
        protected CONST_TV_BLENDINGMODE mBlendingMode;

        protected Dictionary<string, Settings.PropertySpec> mShaderParameters;
        //protected Dictionary<string, object> mShaderParameterValues;
        private bool mShaderParameterChanged = false;
        private string mShaderParamValuesPersistString;
        private string mShaderParamTypesPersistString;
        private string mShaderParamNamesPersistString;
        
        private int mAppearanceFlags; // NORMAL_MAPPING_ENABLED, PARALLAX_ENABLED, TERRAIN_ATLAS_TEXTURING, TERRAIN_GRID_ENABLE
        
        protected Layer[] mLayers;
        internal List<int> mDeletedLayers;

        protected object mSyncLock;
        protected int mHashCode;

        internal GroupAttribute(string id)
            : base(id)
        {
            mSyncLock = new object();
            mResourceStatus = PageableNodeStatus.NotLoaded;
            BlendingMode = CONST_TV_BLENDINGMODE.TV_BLEND_NO;
            Shareable = false;
        }

        #region ITraversable Members
        public override object Traverse(ITraverser target, object data)
        {
            return target.Apply(this, data);
        }
        #endregion

        internal override ChildSetter GetChildSetter()
        {
            throw new NotImplementedException();
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
            Settings.PropertySpec[] properties = new Settings.PropertySpec[7 + tmp.Length];
            tmp.CopyTo(properties, 7);

            properties[0] = new Settings.PropertySpec("blendingmode", typeof(int).Name);
            //properties[1] = new Settings.PropertySpec("targetgeometrytype", typeof(int).Name);
            properties[1] = new Settings.PropertySpec("defines", typeof(string).Name);
            properties[2] = new Settings.PropertySpec("targetresource", typeof(string).Name); // This is the shader relative path (mShaderResourceDescriptor) without the DEFINES that make up the full shaderID
            properties[3] = new Settings.PropertySpec("shader_param_names", typeof(string).Name);
            properties[4] = new Settings.PropertySpec("shader_param_types", typeof(string).Name);
            properties[5] = new Settings.PropertySpec("shader_param_values", typeof(string).Name);
	        properties[6] = new Settings.PropertySpec("shader_parameters", typeof(Settings.PropertySpec[]).Name);
 
            if (!specOnly)
            {
                properties[0].DefaultValue = (int)mBlendingMode;
                //properties[1].DefaultValue = mGeometryType;
                properties[1].DefaultValue = GetDefinesToString(mDefines);  // IMPORTANT: we get defines ahead of mShaderResourceDescriptor so impossible to start paging in before the defines are restored
                properties[2].DefaultValue = mShaderResourceDescriptor; // NOTE: this is just the resource path without the DEFINES included which make up the full shaderID
               
// TODO: the problem here is that when we add the node to the scene, a "SaveNode" occurs to update the scenegraph db.
// and if the tvshader is still background loading during this process, the GetShaderParameters() call following will return null
// - I think the SaveNode() issue is something that should not happen - we should never be saving a node that is still essentially loading!
// which will wipe our persist string
// TODO: the second problem is that we're not persisting the parameters we define in the shader!  That means we need to persist
// the typename, parameterName and defaultvalue for each parameter! Without parameters, GetShaderParameters() will return null after deserializing 
// - the shader parameters never get saved because we don't save shaders! we only save the resourceDescriptor!  ugh...
// TODO: eg: during node.Clone() we are not saving the parameters and since we don't save shaders (only the resourceDescriptor in the GroupAttribute parent node)
// we are not saving the shader parameters!  So how do/should we work around this.  We just have to save the parameters...  and allow them to be cloned... or 
// come up with some other way of storing the parameters... for instance, maybe we store them as param# as normal properties.... the problem there is, we don't know
// how many and our loop only checks for full name... and would hate to have to modify to check for a type of "param#"
            
                properties[3].DefaultValue = mShaderParamNamesPersistString;
                properties[4].DefaultValue = mShaderParamTypesPersistString;
                properties[5].DefaultValue = mShaderParamValuesPersistString;
                // note: this is only for Plugin to be able to get shader params as PropertySpec array
                // but these should never be serialized.  We must prevent that from occuring in WriteXML
                Settings.PropertySpec[] shaderParameters = null;
                if (mShaderParameters != null && mShaderParameters.Count > 0)
                {
                    shaderParameters = new Settings.PropertySpec[mShaderParameters.Count];
                    mShaderParameters.Values.CopyTo(shaderParameters, 0);
                }
       
                properties[6].DefaultValue = shaderParameters;
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
                    //case "targetgeometrytype":
                        // TODO: why is this commented out? i suspect to temporarily get around
                        //       old prefab file format incompatibility
                        //mGeometryType = (int)properties[i].DefaultValue;
                    //    break;
                    case "targetresource":
                        mShaderResourceDescriptor = (string)properties[i].DefaultValue; // This is just the shader resource path without the DEFINES 
                        break;
                    case "blendingmode":
                        mBlendingMode = (CONST_TV_BLENDINGMODE)((int)properties[i].DefaultValue);
                        break;
                    case "defines":
                        // here in GroupAttributes, we store the defines as delimited string
                        mDefines = GetDefinesFromString((string)properties[i].DefaultValue);
                        break;
                        // NOTE: Unlike Entity.CustomProperties, we can't just store the values in the host Entity's output.
                        //       Thats because CustomProperties only persist the VALUES and we load the PropertySpec[] names and typenames from the script itself during script Initialize() of the script.
                        // NOTE: when we serialize, we only ever update the persist strings on save. During ApplyShaderParameterValues() we work directly with the cached mShaderParameters PropertySpec[] dictionary
                        // NOTE: the only time these persist strings should be "set" is during DESERIALIZATION
                    case "shader_param_names":
                        mShaderParamNamesPersistString = (string)properties[i].DefaultValue;
	                   	mShaderParameterChanged = true;
                        SetChangeFlags (Keystone.Enums.ChangeStates.ShaderParameterValuesChanged, Keystone.Enums.ChangeSource.Self);
                        break;
                   case "shader_param_types":
                        mShaderParamTypesPersistString = (string)properties[i].DefaultValue;
                        mShaderParameterChanged = true;
                        SetChangeFlags (Keystone.Enums.ChangeStates.ShaderParameterValuesChanged, Keystone.Enums.ChangeSource.Self);
                        break;
                   case "shader_param_values": // changing just values
                        mShaderParamValuesPersistString = (string)properties[i].DefaultValue;
                    	mShaderParameterChanged = true;
                        SetChangeFlags (Keystone.Enums.ChangeStates.ShaderParameterValuesChanged, Keystone.Enums.ChangeSource.Self);
                        break;
                   case "shader_parameters": // changing via propertyspec collection of parameter values.
                        SetShaderParameterValues ((Settings.PropertySpec[])properties[i].DefaultValue);
	                   	mShaderParameterChanged = true;
                        SetChangeFlags (Keystone.Enums.ChangeStates.ShaderParameterValuesChanged, Keystone.Enums.ChangeSource.Self);
                        break;
                }
            }
            SetChangeFlags(Enums.ChangeStates.AppearanceParameterChanged, Enums.ChangeSource.Self);
            //SetDefines();
        }
        #endregion

        private int _tvIndex = -1;
        #region IPageableTVNode Members
        public int TVIndex
        {
            get
            {

                if (!string.IsNullOrEmpty(mShaderResourceDescriptor))
                {
                    if (PageStatus == PageableNodeStatus.Loaded)
                        _tvIndex = mShader.TVIndex;
                }
                    //    return 0; // no shader is set, or maybe default is set.  Must always use a default shader since we no longer use TV's shader at all?
                    // TODO: grab Zak's phong/blinn shaders for colors... see how they compare?  At least they
                    //       do Materials correctly though yes?
                    // E:\dev\_projects\_TV\Zak_IsotropicLightingModels\IsotropicLightingModels
                    // we want to move zak's code into ours starting with materials and one dir light and just the Lyon lighting model
                    // then we'll add the branching and the other model options
                    // - we may also merge our mesh, actor and mini files into one and using DEFINES and perhaps using
                    // TECHNIQUES for switching between those 3 rather than DEFINES? or are technqiues mostly for 
                    // things like DEPTH pass vs NORMAL pass vs DEFERRED etc?
                 return _tvIndex;
            }
        }

        public bool TVResourceIsLoaded
        {
            get { return _tvIndex > -1; }
        }

        public string ResourcePath
        {
            get { return mShaderResourceDescriptor; }
            set
            {
                mShaderResourceDescriptor = value;
                SetChangeFlags(Keystone.Enums.ChangeStates.ShaderParameterValuesChanged | 
                    Keystone.Enums.ChangeStates.AppearanceParameterChanged, Keystone.Enums.ChangeSource.Self);
            }
        }

        PageableNodeStatus mResourceStatus;
        public PageableNodeStatus PageStatus
        {
            get  { return mResourceStatus; }
            set { mResourceStatus = value; }
        }

        public void UnloadTVResource()
        {
        	// TODO: UnloadTVResource()
        }
        
        /// <summary>
        /// NOTE: GroupAttributes can never be shared, but their Shader child can.
		/// NOTE: if there are multiple viewports open and if we dont fix thread synchronization
        /// you still can get multiple calls to LoadTVResource() which can cause problems in the 
        /// internal shader loading and assignment.
        /// </summary>
        public void LoadTVResource()
        {
        	string pathOfShaderToUse = null;
        	

            if (string.IsNullOrEmpty(mShaderResourceDescriptor))
            {
            	// TODO: if no shader file is assigned, try to lookup the "tvdefault" shader (eg. so we never use tv's shader?)?
            	string defaultMeshShader = CoreClient._CoreClient.Settings.settingRead ("shaders", "default_mesh");
            	if (string.IsNullOrEmpty (defaultMeshShader))
            	{
            		
            	}
                _tvIndex = 0;
                mResourceStatus = PageableNodeStatus.Loaded;
                return;
            }
            else if (mShaderResourceDescriptor == "tvdefault")
            {
                _tvIndex = 0;
                mResourceStatus = PageableNodeStatus.Loaded;
            	return;
            }

            Shader tmpShader = null;
            	
            // we still create the shader name here dynamically which is why our shaderResourceDescriptor is
            // seperate from our defines here, and only becomes a single "id" in the Shader node itself.
                        
            string shaderID = Shader.CreateShaderName (mShaderResourceDescriptor, mDefines);
            // TODO: we are not sharing the shaders here.  We need to be calling Repository.Get(shaderID) first.
            // Shaders are shareable but Appearance and GroupAttributes are not which is why shader parameters are serialized to Appearance/GroupAttribute
 //           tmpShader = (Shader)Resource.Repository.Get(shaderID);
 //           if (tmpShader == null)
 //           {
                tmpShader = (Shader)Resource.Repository.Create(shaderID, "Shader");
                //if (tmpShader.PageStatus == PageableNodeStatus.Loaded)
                //    System.Diagnostics.Debug.WriteLine("Shader '" + shaderID + "' loaded.  RefCount == " + this.RefCount.ToString());
                // must not be able to save this inner Shader.cs to xml in either prefab or scene
                // NOTE: the shader parameters are stored and serialized / deserialized here in GroupAttribute and NOT in shader object so shader's CAN be shared.
                tmpShader.SetFlagValue("serializable", false);
 //           }
            System.Diagnostics.Debug.Assert(tmpShader != null, "GroupAttribute.LoadTVResource() -- Why is Shader.cs instance null?");


            // do not wait for it to be paged later, causes problems
            IO.PagerBase.LoadTVResource (tmpShader , false);
            
            // we MUST IncrementRef the target shader (regardless of whether it's successfully loaded or not)
            // because Creation adds it to the Repository but does not increment it's key until an AddChild() 
            // occurs which doesnt happen here because ProceduralShader is not a true group node and the mTarget 
            // is not treated as a true child (eg. is never serialized via xml)
            AddChild(tmpShader); // must AddChild() so it becomes a child we can traverse

            _tvIndex = tmpShader.TVIndex;

            Settings.PropertySpec[] parameters = GetShaderParametersFromPersistStrings (mShaderParamNamesPersistString, mShaderParamTypesPersistString, mShaderParamValuesPersistString);
			if (parameters != null && parameters.Length > 0)
            {
                for (int i = 0; i < parameters.Length; i++)
                    SetShaderParameterValue(parameters[i].Name, parameters[i].DefaultValue);
            }
            // NOTE: following change flags are set in AddChild() call 
			// TODO: but wait, if the geometry has changed and the defines have changed as a result, we'll need these flags set again
			// SetChangeFlags(Keystone.Enums.ChangeStates.ShaderFXLoaded | Enums.ChangeStates.AppearanceNodeChanged | Enums.ChangeStates.AppearanceParameterChanged |Enums.ChangeStates.ShaderParameterValuesChanged, Enums.ChangeSource.Self);
        
        }

        internal void ReloadResource ()
        {
            string shaderResource = "tvdefault";
            if (Shader != null)
                shaderResource = Shader.ResourcePath;
            else if (this.ResourcePath != "tvdefault")
                shaderResource = this.ResourcePath;

           //System.Diagnostics.Debug.Assert(shaderResource == this.ResourcePath);

        	if (Shader != null)
        		RemoveChild (Shader);

            this.ResourcePath = shaderResource;
        	mResourceStatus = PageableNodeStatus.NotLoaded;
        	
        	IO.PagerBase.LoadTVResource (this , false);        	
        }
        
        public void SaveTVResource(string filepath)
        {
            throw new NotImplementedException();
        }

        public object SyncRoot
        {
            get { return mSyncLock;}
        }
        #endregion


        /// <summary>
        /// BlendingMode is read from Model.
        /// BlendingMode must be set to Alpha or AddAlpha for Material.Opacity to be used.
        /// </summary>
        /// <remarks>
        /// TODO: this does not yet take into account sub-groups that may require blending.
        /// although typically we should not be using/creating meshes that mix transparency
        /// and non transparency, sometimes the models we find online are that way and we just
        /// want to be able to use them.  In those cases, the transparent groups should be rendered 
        /// in a seperate pass with all the other groups disabled and vice versa.
        /// </remarks>
        public virtual CONST_TV_BLENDINGMODE BlendingMode
        {
            get { return mBlendingMode; }
            set
            {
                mBlendingMode = value;
                SetChangeFlags(Enums.ChangeStates.AppearanceParameterChanged, Enums.ChangeSource.Self);
            }
        }

        public Shader Shader { get { return mShader; } /* set { set must remain read only! shaders are not serializable as individual nodes anymore either.  They are always loaded from _resourcepath } */ }
        public Material Material { get { return mMaterial; } }
        public Layer[] Layers { get { return mLayers; } }

        #region Shader Defines
        // NOTE: Defines are normally added or removed by GroupAttribute.cs which monitors
        // changes to the Appearance
        public void AddDefine(string key, string value)
        {
            // remove any existing define at this key
            RemoveDefine(key);

            // add the new
            mDefines = mDefines.ArrayAppend(new KeyValuePair<string, string>(key, value));
        }

        public void RemoveDefine(string key)
        {
            if (mDefines == null) return;

            int index = 0;
            bool found = false;
            
            foreach (KeyValuePair<string, string> kvp in mDefines)
            {
                if (kvp.Key == key)
                {
                	found = true;
                    break;
                }
            	index++;
            }
            
            if (found)
	            mDefines = mDefines.ArrayRemoveAt(index);
        }

        internal void ClearDefines()
        {
            mDefines = null;
        }

        internal static string GetDefinesToString(KeyValuePair<string, string>[] defines)
        {
            if (defines == null) return null;
            string result = null;

            foreach (KeyValuePair<string, string> p in defines)
            {
            	if (string.IsNullOrEmpty (result) == false) result += ",";
                if (string.IsNullOrEmpty (p.Value))
                    result+= p.Key;
                else
                	result += p.Key + "=" + p.Value;
            }

            return result;
        }

        internal static KeyValuePair<string, string>[] GetDefinesFromString(string serializedString)
        {
            if (string.IsNullOrEmpty(serializedString)) return null;

            KeyValuePair<string, string>[] result = null;

            string[] tmp = serializedString.Split(',');
            if (tmp == null || tmp.Length == 0) return null;

            for (int i = 0; i < tmp.Length; i++)
            {
                string[] s = tmp[i].Split('=');
                KeyValuePair<string, string> kvp;
                if (s.Length == 1)
            		kvp = new KeyValuePair<string, string>(s[0], null);
                else
                	kvp = new KeyValuePair<string, string>(s[0], s[1]);
                result = result.ArrayAppend (kvp);
            }

            return result;
        }
        #endregion 

        #region Shader Parameter Storage
		public virtual void DefineShaderParameter(string parameterName, string typeName, object value = null)
		{
			if (string.IsNullOrEmpty (mShaderParamNamesPersistString) == false)
				mShaderParamNamesPersistString += keymath.ParseHelper.English.XMLAttributeNestedDelimiter;
			
			if (string.IsNullOrEmpty (mShaderParamTypesPersistString) == false)
				mShaderParamTypesPersistString += keymath.ParseHelper.English.XMLAttributeNestedDelimiter;
			
			mShaderParamNamesPersistString += parameterName;
			mShaderParamTypesPersistString += typeName;
			
			if (string.IsNullOrEmpty (mShaderParamValuesPersistString) == false)
			    mShaderParamValuesPersistString += keymath.ParseHelper.English.XMLAttributeNestedDelimiter;
				   

			if (value == null)
			{
				if (string.IsNullOrEmpty (mShaderParamValuesPersistString))
				{
					mShaderParamValuesPersistString += "";
				}
			}
			else
				mShaderParamValuesPersistString += value.ToString();	
		}
        		

       	private Settings.PropertySpec[] GetShaderParametersFromPersistStrings (string names, string types, string values)
       	{
        	if (string.IsNullOrEmpty (names) || string.IsNullOrEmpty(types) || string.IsNullOrEmpty(values)) return null;
        	
        	string[] delimitedNames = names.Split(new string[]{keymath.ParseHelper.English.XMLAttributeNestedDelimiter}, StringSplitOptions.None);
        	string[] delimitedTypes = types.Split(new string[]{keymath.ParseHelper.English.XMLAttributeNestedDelimiter}, StringSplitOptions.None);
        	string[] delimitedValues = values.Split(new string[]{keymath.ParseHelper.English.XMLAttributeNestedDelimiter}, StringSplitOptions.None);
        	
        	System.Diagnostics.Debug.Assert (delimitedNames.Length == delimitedTypes.Length && delimitedNames.Length == delimitedValues.Length);
        	
  
        	Settings.PropertySpec[] specs = new Settings.PropertySpec[delimitedValues.Length];
        	for (int i = 0; i < specs.Length; i++)
        	{
        		specs[i] = new Settings.PropertySpec(delimitedNames[i], delimitedTypes[i]);
        		specs[i].DefaultValue = KeyCommon.Helpers.ExtensionMethods.ReadXMLAttribute(specs[i].TypeName, delimitedValues[i]);
        	}
        	
        	 
        	return specs;
        
        }

       	public Settings.PropertySpec[] GetShaderParameters()
       	{
            if (mShaderParameters == null || mShaderParameters.Count == 0) return null;
            Settings.PropertySpec[] parameters = new Settings.PropertySpec[mShaderParameters.Count];
            mShaderParameters.Values.CopyTo(parameters, 0);
            return parameters;

       		//return GetShaderParametersFromPersistStrings (mShaderParamNamesPersistString, mShaderParamTypesPersistString, mShaderParamValuesPersistString);
       	}
       	
       	
        // shader parameters are handled the same was as custom domainobject parameters
        private object GetShaderParamaterValue(string parameterName)
        {
            if (mShaderParameters == null) return null;

            //System.Diagnostics.Debug.Assert(mParameterValues.ContainsKey(parameterName));
            Settings.PropertySpec result = null;
            bool found = mShaderParameters.TryGetValue(parameterName, out result);

            if (found)
                return result.DefaultValue;
            return null;
        }

        // TODO: our hud will sometimes set these one at a time for a shader like our tilemask UV shader
        // and eventually we'll wind up trying to apply those shader values while we're still adding them
        // and the mShaderParameterValues will throw an exception that colleciton was modified.  It'd be nice
        // if we could make the list of values an array, but we need key value pairs
        public void SetShaderParameterValue(string parameter, object value)
        {
            if (mShaderParameters == null) mShaderParameters = new Dictionary<string, Settings.PropertySpec>();

#if DEBUG
            //if (mShader != null && 
            //    mShader.PageStatus == PageableNodeStatus.Loaded && 
            //    mShader.Parameters != null);
#endif     

            // TODO: i should probably have a seperate Add() method rather than use SetShaderParameterValue() for both Add() _and_ Update() functionality
            Settings.PropertySpec property = new Settings.PropertySpec(parameter, value.GetType().Name, value);
            Settings.PropertySpec existing;
            bool found = mShaderParameters.TryGetValue(parameter, out existing);
            if (!found)
                mShaderParameters.Add(parameter, property);
            else
                mShaderParameters[parameter] = property;
            
            
            // TODO: we don't want to set this flag whenever a parameter has changed, I think mostly we care
            // if all parameters have been deserialized or changed in a similar "batch" manner... but runtime
            // parameter changes shouldn't need this i dont think... it's needless overhead.  we do still need
            // to be able to change parameters if necessary between instances, but that's a different concept
            SetChangeFlags(Enums.ChangeStates.ShaderParameterValuesChanged, Enums.ChangeSource.Self);
        }


        
        // shader parameters are handled similarly to Entity's domain object custom properties
        // however unlike DomainObject, here we do not validate using rules
        public void SetShaderParameterValues(Settings.PropertySpec[] parameters)
        {
            if (mShaderParameters == null) mShaderParameters = new Dictionary<string, Settings.PropertySpec>();

            for (int i = 0; i < parameters.Length; i++)
            {
                System.Diagnostics.Debug.Assert(parameters[i].DefaultValue != null);
                // NOTE: We do not assign shader parameter values to shader itself.  Those are 
                // done on Appearance.Apply()
                mShaderParameters[parameters[i].Name] = parameters[i];                
            }

            // HACK: modify each of the three ersist strings after assigning new parameter values via propertyspec[]
            // WARNING: this overrides any and all previous existing persist Name, Types, and Values!
            mShaderParamNamesPersistString = KeyCommon.Helpers.ExtensionMethods.CustomPropertyNamesToString (parameters);
            mShaderParamTypesPersistString = KeyCommon.Helpers.ExtensionMethods.CustomPropertyTypesToString (parameters);
            mShaderParamValuesPersistString = KeyCommon.Helpers.ExtensionMethods.CustomPropertyValuesToString (parameters);
            
            // assert all persist strings have same count of items in the delimited strings
            
            // TODO: we don't want to set this flag whenever a parameter has changed, I think mostly we care
            // if all parameters have been deserialized or changed in a similar "batch" manner... but runtime
            // parameter changes shouldn't need this i dont think... it's needless overhead.  we do still need
            // to be able to change parameters if necessary between instances, but that's a different concept
            SetChangeFlags(Enums.ChangeStates.ShaderParameterValuesChanged, Enums.ChangeSource.Self);
        }
        #endregion

        #region IGroup members
        /// <summary>
        /// Flags are used for Lazy Updates.
        /// This method is alled when SetChangeFlags is called.  It is critical that 
        /// no other actions except flag propogation occurs here because
        /// some flags will get passed along multiple times and that would result in 
        /// multiple unnecessary actions.  Instead, responses to various flag settings
        /// should always occur during Update or Render or reading of a property value like 
        /// BoundingBox and then the flag can be cleared.
        /// </summary>
        /// <param name="flags"></param>
        /// <param name="source"></param>
        protected override void PropogateChangeFlags(global::Keystone.Enums.ChangeStates flags, Enums.ChangeSource source)
        {

        	Keystone.Enums.ChangeStates filter = Keystone.Enums.ChangeStates.ChildNodeAdded;
            if ((flags & filter) != 0)
            {
                // notify parent so that this reaches the Entity ultimately and clear flag
                NotifyParents(filter);
                DisableChangeFlags(filter);
            }
            else if ((flags & Keystone.Enums.ChangeStates.ChildNodeRemoved) != 0)
            {
                // notify parent so that this reaches the Entity ultimately and clear flag
                NotifyParents(Keystone.Enums.ChangeStates.ChildNodeRemoved);
                DisableChangeFlags(Keystone.Enums.ChangeStates.ChildNodeRemoved);
            }

            // NOTE: We do know exactly when a .ShaderFXLoaded occurs _if_ we are
            //       loading our own shader in LoadTVResource() however in the case of
            //       special pass shaders like lightmasking for shadowmaps, we ned to temporarily
            //       set a shader and that shader might need to notify via ChangeStates.ShaderFXLoaded 
            filter = Keystone.Enums.ChangeStates.ShaderParameterValuesChanged |
                	              Keystone.Enums.ChangeStates.ShaderFXLoaded;
            if ((flags & filter) != 0)
            {
            	// Shader propogating up to GroupAttribute will still show "Loading" until LoadTVResource() 
            	// completes so it's ok to also allow PageStatus == Loading.
            	if (PageStatus == PageableNodeStatus.Loaded || PageStatus == PageableNodeStatus.Loading)
	                // GroupAttribute DOES need to NotifyParents(flags), but not DefaultAppearance
    	            //if (this is GroupAttribute)
            		NotifyParents(filter);

                mShaderParameterChanged = true;
                
                //SetChangeFlags (filter, Keystone.Enums.ChangeSource.Self);
                // NOTE: do not disable these values because the Apply(geometry) will need it.
                // DisableChangeFlags(filter);
                                
            }

			filter = Keystone.Enums.ChangeStates.AppearanceParameterChanged;
            if ((flags & filter) != 0)
            {
                // if source of the flag is a child or self (and not a parent), notify parents
                // note: also only if parent is an overall Appearance node do we need to propogate this particular
                // changeState.  
                if (mParents != null && mParents[0] is GroupAttribute && (source == Enums.ChangeSource.Child || source == Enums.ChangeSource.Self))
                    NotifyParents(filter);
            }

            // OBSOLETE - never use switch for testing for certain flags.
            //switch (flags)
            //{
            //    case Keystone.Enums.ChangeStates.ScriptLoaded:
            //        AssignParamaterValues();
            //        // GroupAttribute DOES need to NotifyParents(flags);
            //        NotifyParents(flags);
            //        // check registered flag, and if not, but parent != null, then register
            //        break;
            //    case Enums.ChangeStates.AppearanceChanged :
            //        // if source of the flag is a child or self (and not a parent), notify parents
            //        if (_parents != null && source == Enums.ChangeSource.Child || source == Enums.ChangeSource.Self)
            //            NotifyParents(flags);

            //        break;
            //}
            //note: no need to notify parents because our traverser will compare the HashCode of 
            // this appearance with the appearance currently set on the Geometry in question
            // to determine if it needs to be re-Apply(geometry).   Or is this the incorrect way?  Maybe we should
            // not use a HashCode and instead set the actual flag?  

            // NOTE: if the HashCode == 0 then the Apply will be skipped since clearly no children exist
            // (TODO: well, lightingmode will?  could maybe special case lightingMode to always
            // apply and to have the Geometry check its previous lighting modes before changing..
        }

        public void AddChild(Material child)
        {
            if (mMaterial != null)
                throw new ArgumentException("Node of type ' " + child.TypeName +
                                            "' already exists. Only one instance of this type allowed under parent type '" +
                                            TypeName);

            base.AddChild(child);
            mMaterial = child;

            SetChangeFlags(Enums.ChangeStates.AppearanceParameterChanged, Enums.ChangeSource.Child);
        }

        /// <summary>
        /// </summary>
        /// <param name="child"></param>
        public void AddChild(Shader child)
        {
            if (mShader != null)
                throw new ArgumentException("Node of type ' " + child.TypeName +
                                            "' already exists. Only one instance of this type allowed under parent type '" +
                                            TypeName);
            // TODO: there are times when a lightmap or a post shader will be assigned
            //       to replace the normal shader... when that happens we do need to SetChangeFlags
            //       however ideally, those shaders would not need to be permanently assigned via
            //       AddChild() .  i need to investigate a temporary way of swapping shaders during
            //       things like lightmapping passes
            base.AddChild(child);
            mShader = child;

            SetChangeFlags(Enums.ChangeStates.AppearanceParameterChanged | 
                          Enums.ChangeStates.ShaderParameterValuesChanged |
                          Keystone.Enums.ChangeStates.ShaderFXLoaded, Enums.ChangeSource.Child);
        }

        /// <summary>
        /// Add a new layer.  Diffuse, NormalMap, Specular, Emissive, TextureCycle are some valid layer types.
        /// </summary>
        /// <param name="child"></param>
        public void AddChild(Layer child)
        {
            base.AddChild(child);
            AddTexture(child);
            //SetDefines();
            SetChangeFlags(Enums.ChangeStates.AppearanceParameterChanged, Enums.ChangeSource.Child);
        }

        public override void RemoveChild(Node child)
        {
            if (child is Material)
            {
                System.Diagnostics.Debug.Assert(mMaterial == child);
                mMaterial = null;
            }
            else if (child is Shader)
            {
               // System.Diagnostics.Debug.Assert(_shader == child);
                mShader = null;
                this.ResourcePath = "tvdefault";
            }
            else if (child is Layer)
            {
                RemoveTexture((Layer)child);
            }
            base.RemoveChild(child);
            // TODO: as we upage, 
            //SetDefines();
            SetChangeFlags(Enums.ChangeStates.AppearanceParameterChanged, Enums.ChangeSource.Child);
        }

        internal void RemoveShader() 
        { 
        	if (mShader != null) 
        	{
        		mResourceStatus = PageableNodeStatus.NotLoaded;
        		RemoveChild(mShader);
        		mResourceStatus = PageableNodeStatus.Loaded;
        		SetChangeFlags (Keystone.Enums.ChangeStates.ShaderFXUnloaded, Keystone.Enums.ChangeSource.Child);
        	}
        }
        
        public void RemoveMaterial() { if (mMaterial != null) RemoveChild(mMaterial); }


        private void AddTexture(Layer tex)
        {
            mLayers = mLayers.ArrayAppend(tex);
        }

        private void RemoveTexture(Layer tex)
        {

            if (mLayers == null) throw new ArgumentOutOfRangeException();

            // TODO: this seems to be failing... removing of the layer is not
            //       removing it from the array
            mLayers = mLayers.ArrayRemove(tex);

            if (mDeletedLayers == null)
                mDeletedLayers = new List<int>();

            mDeletedLayers.Add(tex.LayerID);
        }
        #endregion

        #region Updates
        /// <summary>
        /// This is always called during GroupAttribute.Apply() for the various geometries. This is because
        /// if a shader is shared, the parameters for a given instance must be restored.  Otherwise we get
        /// errors such as all TexturedAnimation billboards "exploding" at the same time even though we only
        /// "play" one instance.
        /// </summary>
        protected void ApplyShaderParameterValues()
        {
            if (mShader == null)
                return;
            if (mShader.PageStatus != PageableNodeStatus.Loaded)
            {
                System.Diagnostics.Debug.WriteLine("GroupAttribute.ApplyShaderParameterValues() - Shader not loaded.");
                return;
            }

            // March.12.2024 - SetShaderParameters must be called every frame on Appearance.Apply() regardless of whether
            //                a particular shared instances parameters values has changed. 
            if (mShaderParameters != null && mShaderParameters.Count > 0)
            {
                foreach (Settings.PropertySpec spec in mShaderParameters.Values)
                {
                    mShader.SetShaderParameter(spec.Name, spec.TypeName, spec.DefaultValue);
                }
            }

            // NOTE: we only disable this flag after the changed parameter values have been applied
            // TODO: but what if shader errors or never loads?  
            // We must restore shader parameters for each geometry that is using it 
            // DisableChangeFlags(Keystone.Enums.ChangeStates.ShaderFXLoaded | Keystone.Enums.ChangeStates.ShaderParameterValuesChanged);
        }

        internal void Update(double elapsedSeconds)
        {
            // TODO: This is wrong.  GroupAttribute will not update Layer TextureCycles anymore.
            // Instead, AnimatedTextures will automatically be retreivable as Animations
            // that can be broken up into sub-animations just like normal Actor.Animations are.
            // Adding of a SpriteSheet to an Appearance will notify up the chain to the entity so that
            // default Animations can be extracted.
            if (mLayers != null)
            {
                for (int i = 0; i < mLayers.Length; i++)
                    if (mLayers[i] is TextureCycle)
                        ((TextureCycle)mLayers[i]).Update(elapsedSeconds);

            }

            // update any shader parameters.  This must be done for all groups too
            // TODO: i think this entire Update() can be deleted.  Shader parameters
            // are now updated on Apply() if the hashcode is changed so just like everything else
            // Now eventually we may modify our HashCode to instead be a bit flag to denote what specifically
            // has changed whether it's JUST the shader or JUST the material, or both  or neither, etc
            if (mShader != null)
                mShader.Update(elapsedSeconds);
        }
        #endregion

        #region Appliance of Appearance to Geometry
        internal virtual int Apply(ParticleSystemDuplicate ps, EmitterProperties[] emitters, double elapsedSeconds)
        {

            int emitterIndex = this.GroupID;

            throw new NotImplementedException();
            return 0;
        }

        public virtual int Apply(Terrain land, double elapsedSeconds) // chunk?  I might make it so terrains are always handled per chunk
        {
        	throw new NotImplementedException();
            return 0;
        }

        public virtual int Apply(TexturedQuad2D quad, double elapsedSeconds)
        {
        	throw new NotImplementedException();
            return 0;
        }

        public virtual int Apply(Actor3dDuplicate actor, double elapsedSeconds)
        {

            // TODO: can't some things be set by actor group?
            //actor.BlendingMode 
            throw new NotImplementedException();
            return 0;
        }

        //public virtual int Apply(Actor3d actor, double elapsedSeconds)
        //{
        //    return 0;
        //}

        public virtual int Apply (InstancedGeometry geometry, double elapsedSeconds)
        {
            // if the shader's current parameters do not match the ones used here
            // we must update them.. problem is ChangeStates.ShaderParameterValuesChanged
            // is not going to cover that case
            // NOTE: We must always re-apply shader parameters because if more than one instance hosts 
            // the same shader, the assignment of parameters of one, will affect all the other instances.
            // So we must apply the unique parameters to all.
            //if ((mChangeStates & (Keystone.Enums.ChangeStates.ShaderParameterValuesChanged | Keystone.Enums.ChangeStates.ShaderFXLoaded)) != 0)
            ApplyShaderParameterValues();

        	
        	if (geometry.LastAppearance == GetHashCode()) return mHashCode;
        	
            geometry.Shader = mShader;
            
            
            if (mShader == null || 
                mLayers == null || 
                mLayers[0] == null || 
                mLayers[0].Texture == null || 
                mLayers[0].TVResourceIsLoaded == false) 
            	return mHashCode;
            
            // TODO: assign Material parameters to shader
            
            // set the textureDiffuse in shader directly since we're not using TVMesh or TVMinimesh
            // there is no way to have TV automatically assign via semantic TEXTURE0
            this.mShader.SetShaderParameterTexture("textureDiffuse", mLayers[0].Texture.TVIndex);
        	return mHashCode;
        }
                
        public virtual int Apply(Mesh3d mesh, double elapsedSeconds)
        {
            // NOTE: This GroupAttribute.Apply() is called by DefaultAppearance.Apply() since it
            // uses exact same code 

            // if the shader's current parameters do not match the ones used here
            // we must update them.. problem is ChangeStates.ShaderParameterValuesChanged
            // is not going to cover that case
            // NOTE: We must always re-apply shader parameters because if more than one instance hosts 
            // the same shader, the assignment of parameters of one, will affect all the other instances.
            // So we must apply the unique parameters to all.
            //if ((mChangeStates & (Keystone.Enums.ChangeStates.ShaderParameterValuesChanged | Keystone.Enums.ChangeStates.ShaderFXLoaded)) != 0) if all we really just need
            //       to do is change params for the current mesh instance

            ApplyShaderParameterValues();

            // NOTE: call to GetHashCode() tests our _changeStates flag
            if (mesh.LastAppearance == GetHashCode()) return mHashCode;

            // NOTE: mesh.Shader here applies per group if applicable
            mesh.Shader = mShader;
            
            mesh.SetBlendingMode(mBlendingMode, GroupID);

            if (mMaterial != null)
            {
                // Updating the material just prior to it's use is only safe place
                // to do it.  Otherwise we get exceptions in TVMesh.Render() if we
                // are changing TVMaterialFactory material while rendering a mesh.
                // This is unique for Material PageableResource.
                if (mMaterial.PageStatus == PageableNodeStatus.NotLoaded)
                {
                    PagerBase.LoadTVResource(mMaterial);
                }
                else
                    mMaterial.Apply();

                mesh.SetMaterial(mMaterial.TVIndex, GroupID); // NOTE: we must never use material index -1.  Instead use 0.  We cannot use -1 for materialIndex or else we'll get an access violation on tvmesh.Render()
            }
            else
                // NOTE: we use material index 0 which is the default tv3d material.  We cannot use -1 for materialIndex or else we'll get an access violation on tvmesh.Render()
                mesh.SetMaterial(0, -1);

            if (mLayers != null)
                for (int i = 0; i < mLayers.Length; i++)
                {
                    if (mLayers[i].Texture == null) continue; 
                    if (mLayers[i].Texture.PageStatus == PageableNodeStatus.Loaded)
                    {
                        System.Diagnostics.Debug.Assert(mLayers[i].TextureIndex != -1);
                        System.Diagnostics.Debug.Assert(mLayers[i].LayerID != -1);
                        // texture's assigned here will get set to any shader via TEXTURE# semantics.
                        mesh.SetTextureEx(mLayers[i].LayerID, mLayers[i].TextureIndex, GroupID);
                        
                        // alpha test
                        if (mLayers[i] is Diffuse)
                        {
                        	Diffuse diffuseLayer = (Diffuse)mLayers[i];
							mesh.AlphaTestDepthWriteEnable = diffuseLayer.AlphaTestDepthWriteEnable;
							mesh.AlphaTestRefValue = diffuseLayer.AlphaTestRefValue ;                        	
							mesh.SetAlphaTest ( diffuseLayer.AlphaTest, this.GroupID);
                        }
                        
                        // texture mod
                        if (mLayers[i].TextureModEnabled)
                        {
                            mesh.SetTextureMod(mLayers[i].LayerID, GroupID, mLayers[i].TranslationU, mLayers[i].TranslationV, mLayers[i].TileU, mLayers[i].TileV);
                            mesh.SetTextureRotation(mLayers[i].LayerID, GroupID, mLayers[i].Rotation);
                        }
                    }
            		else if (mLayers[i].Texture.PageStatus == PageableNodeStatus.Loading)
            			continue;
//                    else // TODO: the problem here is, this SetTextureEx will keep firing while loading is occurring
//                    	// when what we really want is to only SetTextureEx( LayerID, -1, GroupID) when a texture starts to unload.
//                    	// But even in that case, what I would need to do is not RemoveChild() the texture and/or layer nodes
//                    	// from a mesh until i had SetTextureEx(layerID, -1, GroupID); first.
//                        mesh.SetTextureEx(_layers[i].LayerID, -1, GroupID);
                }


            if (mDeletedLayers != null)
            {
                for (int i = 0; i < mDeletedLayers.Count; i++)
                    mesh.SetTextureEx(mDeletedLayers[i], -1, GroupID);

                mDeletedLayers.Clear();
                mDeletedLayers = null;
            }

            return mHashCode;
        }

        public virtual int Apply(MinimeshGeometry mini, double elapsedSeconds)
        {
            // NOTE: This GroupAttribute.Apply() is called by DefaultAppearance.Apply() since it
            // uses exact same code 
            // if the shader's current parameters do not match the ones used here
            // we must update them.. problem is ChangeStates.ShaderParameterValuesChanged
            // is not going to cover that case
            // NOTE: We must always re-apply shader parameters because if more than one instance hosts 
            // the same shader, the assignment of parameters of one, will affect all the other instances.
            // So we must apply the unique parameters to all.
            //if ((mChangeStates & (Keystone.Enums.ChangeStates.ShaderParameterValuesChanged | Keystone.Enums.ChangeStates.ShaderFXLoaded)) != 0)
                ApplyShaderParameterValues();

            // in our Sol System, some gas giants seem to be trying to apply before shader is loaded
            // NOTE: call to GetHashCode() tests our _changeStates flag
            if (mini.LastAppearance == GetHashCode()) return mHashCode;
            
            mini.SetBlendingMode(mBlendingMode);

            // NOTE: mesh.Shader here applies to main shader and not by group.
            mini.Shader = mShader;


            if (mMaterial != null)
            {
                // Updating the material just prior to it's use is only safe place
                // to do it.  Otherwise we get exceptions in TVMesh.Render() if we
                // are changing TVMaterialFactory material while rendering a mesh.
                // This is unique for Material PageableResource.
                if (mMaterial.PageStatus == PageableNodeStatus.NotLoaded)
                {
                    PagerBase.LoadTVResource(mMaterial);
                }
                mMaterial.Apply();
                mini.SetMaterial(mMaterial.TVIndex);
            }

            // NOTE: Minimeshes can have multiple texture layers but only ONE group.
            if (mLayers != null)
                for (int i = 0; i < mLayers.Length; i++)
                {
                    if (mLayers[i].Texture.PageStatus == PageableNodeStatus.Loaded)
                    {
                        System.Diagnostics.Debug.Assert(mLayers[i].TextureIndex != -1);
                        System.Diagnostics.Debug.Assert(mLayers[i].LayerID != -1);
                        // texture's assigned here will get set to any shader via TEXTURE# semantics.
                        mini.SetTextureEx(mLayers[i].LayerID, mLayers[i].TextureIndex);
                        // NOTE: No texture mods built into tv3d for minimeshes
                        //if (_layers[i].TextureModEnabled)
                        //{
                        //mini.SetTextureMod(_layers[i].LayerID, GroupID, _layers[i].TranslationU, _layers[i].TranslationV, _layers[i].TileU, _layers[i].TileV);
                        //mini.SetTextureRotation(_layers[i].LayerID, GroupID, _layers[i].Rotation);
                        //}
                        
                        // alpha test
                        if (mLayers[i] is Diffuse)
                        {
                        	Diffuse diffuseLayer = (Diffuse)mLayers[i];
							mini.AlphaTestDepthWriteEnable = diffuseLayer.AlphaTestDepthWriteEnable;
							mini.AlphaTestRefValue = diffuseLayer.AlphaTestRefValue ;                        	
							mini.SetAlphaTest ( diffuseLayer.AlphaTest, this.GroupID);
                        }
                    }
                    else if (mLayers[i].Texture.PageStatus == PageableNodeStatus.Loading)
            			continue;
                    
//                    else // see notes regarding Mesh3d and SetTextureEx (-1) while PageStatus == NotLoaded.
//                        mini.SetTextureEx(_layers[i].LayerID, -1);
                }


            if (mDeletedLayers != null)
            {
                for (int i = 0; i < mDeletedLayers.Count; i++)
                    mini.SetTextureEx(mDeletedLayers[i], -1);

                mDeletedLayers.Clear();
                mDeletedLayers = null;
            }

            return mHashCode;
        }

        public virtual int Apply(Minimesh2 mini, double elapsedSeconds)
        {
            return 0;
        }
        #endregion

        public override int GetHashCode()
        {
            if ((mChangeStates & (Enums.ChangeStates.AppearanceNodeChanged | 
                                  Enums.ChangeStates.AppearanceParameterChanged | 
                                  Enums.ChangeStates.ShaderFXLoaded | 
                                  Enums.ChangeStates.ShaderFXUnloaded)) != 0)
        	{
                ComputeHashCode();
        	
            	DisableChangeFlags(Keystone.Enums.ChangeStates.AppearanceNodeChanged  | 
                                   Keystone.Enums.ChangeStates.AppearanceParameterChanged);
        	}
            return mHashCode;
        }

        internal int LastHashCode { get { return mHashCode; } }
        // The purpose of a hashcode
        // is NOT for tracking changes, it's for quickly tracking the difference in
        // appearance on a mesh between one instance render and the next so we know if we
        // have to update the textures/shaders/materials on a mesh/actor/mini/etc or not.
        internal void ComputeHashCode()
        {
            mHashCode = (int)GetHashData();
        }

        protected virtual uint GetHashData()
        {
            uint result = 0;

            byte[] bytes = BitConverter.GetBytes((int)mBlendingMode);
            Keystone.Utilities.JenkinsHash.Hash(bytes, ref result);

            if (_children != null)
            {
                for (int i = 0; i < _children.Count; i++)
                {
                	if (_children[i] is IPageableTVNode)
                	{
                		if (((IPageableTVNode)_children[i]).PageStatus != PageableNodeStatus.Loaded)
                			continue;
                	}
                	
                    if (_children[i] is Material)
                        Keystone.Utilities.JenkinsHash.Hash(BitConverter.GetBytes(_children[i].GetHashCode()), ref result);

                    else if (_children[i] is Shader)
                    {
                        // TODO: shouldn't this take into account shader parameter changes? or never
                        // because parameters get updated seperately
                        // TODO: i think yes it should take into account parameter changes so we can
                        // still avoid shader changes between similarly rendered items
                        if (mShaderParameters != null && mShaderParameters.Count > 0)
                        {
                            // TODO: can we/should we use seperate shader parameter hashcodes
                            //       so we dont needlessly re-apply materials and textures
                            //       just cuz shader parameter has changed?
                            // TODO: can we createa  .JeninksHash helper to hashcode 
                            // an array of propertyspec?
                            // TODO: I think we should only update the hashcode for propertySpecs if 
                            // the specs themselves are changed and not merely their .DefaultValues changed
                            //Keystone.Helpers.Functions.JenkinsHash.Hash (GetShaderParameters(false), ref result);
                        }
                        Keystone.Utilities.JenkinsHash.Hash((((Shader)_children[i]).ID), ref result);
                    }
                    else if (_children[i] is Layer)
                        Keystone.Utilities.JenkinsHash.Hash(BitConverter.GetBytes(_children[i].GetHashCode()), ref result);
                    else if (_children[i] is GroupAttribute)
                        Keystone.Utilities.JenkinsHash.Hash(BitConverter.GetBytes(((GroupAttribute)_children[i]).GetHashData()), ref result);
                }
            }

            return result;
        }

        #region IDisoposable members
        protected override void DisposeManagedResources()
        {
            // NOTE: when changing appearance attributes in GroupAttribute
            // a new shader may need to be set or built.  If that is the case
            // then if were previously loading a shader while also attempting to modify
            // it's GroupAttributes (which changes the shader DEFINES) then we need to sychronize
            // thread access
           // lock (mShaderLoaderLock) // TODO: do we need lock ehre?
                if (mShader != null)
                {
                    // TODO: why am i .DecrementRef this shader? It's a child node NOT a direct resource like tvshader 
                    // and so will unload normally as we remove children.  There's no need
                    //       for us to force this
                    //Keystone.Resource.Repository.DecrementRef(_shader);
                    // TODO: are we somehow disposing when we shouldnt?
                    // TODO: In fact, if this shader is still assigned to a Mesh or Actor when
                    // the ref count in Repository reaches 0, this will error.  The key is to ensure
                    // we remove this shader before DecrementRef... but how do we know?!  The shader if it is not
                    // tracking, i dont see how this GroupAttribute will know.  This must be resolved.
                    //_shader = null;

                }
        }
        #endregion
    }


    #region OBSOLETE July.14.2014 - Shader will now only be an interior node of GroupAttribute
//    // TODO: I should probably just rename this to GroupAppearance

//    // GroupAttribute objects are added to Appearance nodes.
//    // Appearance node's can contain multiple GroupAttribute nodes.
//    // A single GroupAttribute node corresponds to a single Mesh or Actor group or Terrain chunk
//    public class GroupAttribute : Group
//    {
//        // NOTE: no seperate lighting modes per group allowed.  Only one under the primary
//        //       Appearance node.
//        internal int GroupID = -1;
//        protected int mGeometryType = -1;
//        protected string mTVName; // the group name which can be read/set by Get/SetGroupName 
//        protected CONST_TV_BLENDINGMODE _blendingMode;
//        // alphaTest is for two things
//        // 1) filtering pixels such that if alpha channel value on texture is >= some value 
//        //    (eg. must be less than or equal to max value of 255 and is commonly 128) then
//        //    that that pixel will get skipped. This is useful for rendering leaves or fences
//        //    where high alpha is set on the empty areas of a leaf texture or a chain link fence
//        //    texture and we want to skip rendering pixels that are mapped to those alpha texels.
//        // 2) texture color keying.  
//        // NOTE:  In neither of these 2 cases is it used by Material opacity style apha blending
//        protected bool _alphaTest;
//        protected int _alphaTestRefValue;
//        protected bool _alphaTestDepthBufferWriteEnable;


//        protected Material _material;
//        protected Shader _shader;
//        // TODO: why aren't i storing these as PropertySpec? That way i could even persist these
//        //       values so long as they didnt need to change each frame.
//        protected Dictionary<string, object> mParameterValues;
//        protected string mPersistedParameterValues; // string only used for serialization

//        protected Layer[] _layers;
//        protected int _hashCode;

//        internal List<int> mDeletedLayers;


//        public GroupAttribute(string id) : base(id)
//        {
//            BlendingMode = CONST_TV_BLENDINGMODE.TV_BLEND_NO;
//        }

//        #region ITraversable Members
//        public override object Traverse(ITraverser target, object data)
//        {
//            return target.Apply(this, data);
//        }
//        #endregion 

//        internal override ChildSetter GetChildSetter()
//        {
//            throw new NotImplementedException();
//        }

//        #region ResourceBase members
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="specOnly">True returns the properties without any values assigned</param>
//        /// <returns></returns>
//        public override Settings.PropertySpec[] GetProperties(bool specOnly)
//        {
//            Settings.PropertySpec[] tmp = base.GetProperties(specOnly);
//            Settings.PropertySpec[] properties = new Settings.PropertySpec[3 + tmp.Length];
//            tmp.CopyTo(properties, 3);

//            properties[0] = new Settings.PropertySpec("targetgeometrytype", typeof(int).Name);
//            properties[1] = new Settings.PropertySpec("parameters", typeof(string).Name);
//            properties[2] = new Settings.PropertySpec("blendingmode", typeof(int).Name);

//            if (!specOnly)
//            {
//                properties[0].DefaultValue = mGeometryType;
//                // TODO: this is wrong... we should only be able to get a spec of VALUES here
//                // and not a sub- array of propertyspecs from the shader.  This is part of what
//                // has caused us issues when deserializng/serializng this "parameters" property
//                properties[1].DefaultValue = GetShaderParameters(false);
//                properties[2].DefaultValue = (int)_blendingMode;
//            }
//            return properties;
//        }

//        public override void SetProperties(Settings.PropertySpec[] properties)
//        {
//            if (properties == null) return;
//            base.SetProperties(properties);


//            for (int i = 0; i < properties.Length; i++)
//            {
//                if (properties[i].DefaultValue == null) continue;
//                // use of a switch allows us to pass in all or a few of the propspecs depending
//                // on whether we're loading from xml or changing a single property via server directive
//                switch (properties[i].Name)
//                {
//                    case "targetgeometrytype":
//                        // TODO: why is this commented out?
//                        //mGeometryType = (int)properties[i].DefaultValue;
//                        break;
//                    case "blendingmode":
//                        _blendingMode = (CONST_TV_BLENDINGMODE)((int)properties[i].DefaultValue);
//                        break;
//                    case "parameters":

//                        // TODO: when loading a second or more instancec when the shader is shared
//                        // causes a problem here.  Something is not occuring that does occur the very first
//                        // time the shader is created and not happening when it's shared.
//                        // but also....
//                        // TODO: using same "parameters" key for either array of Specs
//                        // or persisted values is wrong... we should only be able to get a spec of VALUES here
//                        // and never a sub- array of propertyspecs from the shader.  This is part of what
//                        // has caused us issues when deserializng/serializng this "parameters" property.
//                        // If a caller wants the specs, it can get them from the shader!
//                        if (properties[i].DefaultValue is Settings.PropertySpec[])
//                            SetShaderParameterValues((Settings.PropertySpec[])properties[i].DefaultValue);
//                        else if (properties[i].DefaultValue is string)
//                        {
//                            mPersistedParameterValues = (string)properties[i].DefaultValue;
//                            RestoreCustomParameterValuesFromPersistString();
//                        } 
//                        break;

//                }
//            }
//            SetChangeFlags(Enums.ChangeStates.AppearanceChanged, Enums.ChangeSource.Self);
//            SetDefines();
//        }
//        #endregion

//        /// <summary>
//        /// BlendingMode is read from Model.
//        /// BlendingMode must be set to Alpha or AddAlpha for Material.Opacity to be used.
//        /// </summary>
//        /// <remarks>
//        /// TODO: this does not yet take into account sub-groups that may require blending.
//        /// although typically we should not be using/creating meshes that mix transparency
//        /// and non transparency, sometimes the models we find online are that way and we just
//        /// want to be able to use them.  In those cases, the transparent groups should be rendered 
//        /// in a seperate pass with all the other groups disabled and vice versa.
//        /// </remarks>
//        public virtual CONST_TV_BLENDINGMODE BlendingMode
//        {
//            get { return _blendingMode; }
//            set
//            {
//                _blendingMode = value;
//                SetChangeFlags(Enums.ChangeStates.AppearanceChanged, Enums.ChangeSource.Self);
//            }
//        }

//        // TODO: we need to make "Shader" internal and only loadable by
//        //       GroupAttribute.  I think perhaps we copy paste a new GroupAppearance
//        //       that does this... get rid of ProceduralShader and move that code to here.
//        //       - same should be done for our behavior script node? maybe not though since
//        //       the behavior script node is already what we're doign here right? afterall
//        //       our DomainObject is the same way... it hosts the script directly.
//        public Shader Shader { get { return _shader; } }
//        public Material Material { get { return _material; } }
//        public Layer[] Layers { get { return _layers; } }

//        public int TargetType 
//        {
//            set
//            {
//                // TargetType will get the ultimate parent geometry to which
//                // this appearance will be applied.  This is ok because appearances
//                // cannot be shared and are guaranteed to only have one parent which will
//                // either be another Appearance (in the case of GroupAttribute) or an Entity
//                // and from that we will gain the Geometry type.  
//                // But how does this work with LOD?  And how does this work with Appearance
//                // in FXInstanceRenderer
//                mGeometryType = value;
//                SetDefines();
//            }
//        }

//        // TODO: This is problematic as far as existing inside here...
//        //       I think if possible, it's preferable to put this into the Shader
//        //       I mean... i acknowledge that we might want our procedural shader to change
//        //       however, 
//        private void SetDefines()
//        {
//            if (_shader == null) return;
//            if (mGeometryType == -1) return;

//            if (_shader is ProceduralShader)
//            {
//                ProceduralShader procShader = (ProceduralShader)_shader;

//                // ClearDefines causes the current shader to be removed and de-ref counted
//                procShader.ClearDefines();

//                // TODO: begin temp hack adding of defines for moon satellite mode
//                // procShader.AddDefine("NORMALMAP", null);
//                //procShader.AddDefine("SPECULAR_IN_DIFFUSEMAP_ALPHA", null);
//                // end temp hack for moon satellite mode


//                // start rebuilding list of defines
//                procShader.AddDefine("GEOMETRY", mGeometryType.ToString());

//                // NOTE: all viewports must use same graphics setting
//                bool deferredEnabled = CoreClient._CoreClient.Settings.settingReadBool("graphics", "deferred");
//                if (deferredEnabled)
//                    procShader.AddDefine("DEFERRED", null);
//                else
//                    procShader.AddDefine("FORWARD", null);

//                if (_material != null && deferredEnabled == false)
//                {
//                    procShader.AddDefine("MATERIAL", null);
//                }

//                //    procShader.AddDefine("PARALLAX_IN_NORMALMAP_ALPHA", null);

//                // TODO: here we have a problem.  Typically the ProceduralShader is added
//                // to the DefaultAppearance, but the textures and such are added individually to each
//                // Group.  Since the existing ProceduralShader is applied to the entire mesh
//                // those textures will get assigned to proper semantics but this shader will never
//                // have those proper defines set because it's only looking at it's own layers, not of
//                // child groups!
//                // How can child layers, result in SetDefines of hte parent shader?
//                // I think that is the key issue...  perhaps if they dont have their own shader
//                // they use their parents?

//                // Option 1 - Each group does in fact have it's own ProceduralShader
//                //          although most of those wind up sharing internal Shader.cs objects instances.
//                // Option 2 - Only overall can have shader and only there can we set shader effect options
//                //          which alter the defines.  
//                //          - this option is safer in a way because it allows us to select the shader file
//                //          and to optionally not use ANY defines.  Just the assigned shader.
//                //          - but when using the default shader, and "auto select" options, it auto selects.

//                //  And what if each GroupAttribute now has the shader path to use...?
//                //  with option to grab the default from CoreClient.CoreClient.DefaultShader;
//                //  Then no more ProceduralShader... each GroupAttribute is now a shader loader
//                //  that will compile defines and select the type of Shader to load.
//                //
//                //  And then I think we need to alter how our Apply() method works so that group
//                //  attributes children will also have their apply called on their specific group
//                //  Rather than how it's done now...  But maybe we maintain seperate hashcodes per group
//                //
//                // I guess the other option is to now all this automatic determination based on 
//                if (_layers != null)
//                {
//                    foreach (Layer l in _layers)
//                    {
//                        if (l.LayerID == 0) // diffuse
//                        {
//                            // use diffuse color instead of just white 
//                            procShader.AddDefine("DIFFUSEMAP", null);
//                            // SPECULAR_INTENSITY_IN_DIFFUSE_ALPHA
//                        }
//                        else if (l.LayerID == 1) // normalmap
//                        {
//                            procShader.AddDefine("NORMALMAP", null);
//                            // parallax heightmap in normal_map alpha?

//                        }
//                        else if (l.LayerID == 2) // specular map - typically this can be added into diffuse alpha
//                        {
//                            // "SPECULAR_COLOR_MAP"
//                            // "SPECULAR_COLOR_MAP_WITH_INTENSITY_MAP"
//                            // procShader.AddDefine("SPECULARMAP", null);
//                        }
//                        else if (l.LayerID == 3) // emissive map
//                        {
//                            procShader.AddDefine("EMISSIVEMAP", null);
//                        }
//                    }
//                }
//            }   
//            // Initiate paging of the new shader using the new list of defines
//            // TODO: problem is, everytime we change the targettype, this SetDefines() is called
//            // and the item is queued for paging again!?  isn't this true? or is it?
//            // TODO: Yes, if the _shader != null we must instruct that shader to
//            // change it's underlying shader resource...  
//            // the following call results in _shader.LoadTVResource() but im not sure it can work
//            // if the original tvshader isn't unloaded first and it's resource status set back to unload
//            // and tvindex back to -1
//            // TODO: coudl we even be trying to queue during our universe generation, the same shaders using
//            // the same defines multiple times?  i think yes...  we MUST modify our Repository.Get(key)
//            // to do a type of interlock where while we're getting we're also creating if it's not there and
//            // returning the type we want... perhaps we can pass a create function to use to it.  
//            //
//            IO.PagerBase.QueuePageableResource((IPageableTVNode)_shader, null);
//        }

//        #region IGroup members
//        /// <summary>
//        /// Flags are used for Lazy Updates.
//        /// This method is alled when SetChangeFlags is called.  It is critical that 
//        /// no other actions except flag propogation occurs here because
//        /// some flags will get passed along multiple times and that would result in 
//        /// multiple unnecessary actions.  Instead, responses to various flag settings
//        /// should always occur during Update or Render or reading of a property value like 
//        /// BoundingBox and then the flag can be cleared.
//        /// </summary>
//        /// <param name="flags"></param>
//        /// <param name="source"></param>
//        protected override void PropogateChangeFlags(global::Keystone.Enums.ChangeStates flags, Enums.ChangeSource source)
//        {

//            if ((flags & Keystone.Enums.ChangeStates.ChildNodeAdded) != 0)
//            {
//                // notify parent so that this reaches the Entity ultimately and clear flag
//                NotifyParents(Keystone.Enums.ChangeStates.ChildNodeAdded);
//                DisableChangeFlags(Keystone.Enums.ChangeStates.ChildNodeAdded);
//            }
//            else if ((flags & Keystone.Enums.ChangeStates.ChildNodeRemoved) != 0)
//            {
//                // notify parent so that this reaches the Entity ultimately and clear flag
//                NotifyParents(Keystone.Enums.ChangeStates.ChildNodeRemoved);
//                DisableChangeFlags(Keystone.Enums.ChangeStates.ChildNodeRemoved);
//            }


//            if ((flags & Keystone.Enums.ChangeStates.ShaderFXLoaded) != 0)
//            {
//                RestoreCustomParameterValuesFromPersistString();
//                // GroupAttribute DOES need to NotifyParents(flags);
//                NotifyParents(Keystone.Enums.ChangeStates.ShaderFXLoaded);
//                // check registered flag, and if not, but parent != null, then register
//            }
//            if ((flags & Keystone.Enums.ChangeStates.AppearanceChanged) != 0)
//            {
//                // if source of the flag is a child or self (and not a parent), notify parents
//                if (_parents != null && source == Enums.ChangeSource.Child || source == Enums.ChangeSource.Self)
//                    NotifyParents(Keystone.Enums.ChangeStates.AppearanceChanged);
//            }

//            // OBSOLETE - never use switch for testing for certain flags.
//            //switch (flags)
//            //{
//            //    case Keystone.Enums.ChangeStates.ScriptLoaded:
//            //        AssignParamaterValues();
//            //        // GroupAttribute DOES need to NotifyParents(flags);
//            //        NotifyParents(flags);
//            //        // check registered flag, and if not, but parent != null, then register
//            //        break;
//            //    case Enums.ChangeStates.AppearanceChanged :
//            //        // if source of the flag is a child or self (and not a parent), notify parents
//            //        if (_parents != null && source == Enums.ChangeSource.Child || source == Enums.ChangeSource.Self)
//            //            NotifyParents(flags);
//            //        break;
//            //}
//        }

//        public void AddChild(Material child)
//        {
//            if (_material != null)
//                throw new ArgumentException("Node of type ' " + child.TypeName +
//                                            "' already exists. Only one instance of this type allowed under parent type '" +
//                                            TypeName);

//            base.AddChild(child);
//            _material = child;
//            SetDefines();
//            SetChangeFlags(Enums.ChangeStates.AppearanceChanged, Enums.ChangeSource.Self);
//        }

//        public void AddChild(Shader child)
//        {
//            if (_shader != null)
//                throw new ArgumentException("Node of type ' " + child.TypeName +
//                                            "' already exists. Only one instance of this type allowed under parent type '" +
//                                            TypeName);
//            base.AddChild(child);
//            _shader = child;
//            SetDefines();
//            SetChangeFlags(Enums.ChangeStates.AppearanceChanged, Enums.ChangeSource.Self);

//            if (_shader.TVResourceIsLoaded)
//                RestoreCustomParameterValuesFromPersistString();
//        }

//        /// <summary>
//        /// Add a new layer.  Diffuse, NormalMap, Specular, Emissive, TextureCycle are some valid layer types.
//        /// </summary>
//        /// <param name="child"></param>
//        public void AddChild(Layer child)
//        {
//            base.AddChild(child);
//            AddTexture(child);
//            SetDefines();
//            SetChangeFlags(Enums.ChangeStates.AppearanceChanged, Enums.ChangeSource.Self);
//        }

//        public override void RemoveChild(Node child)
//        {
//            if (child is Material)
//            {
//                System.Diagnostics.Debug.Assert(_material == child);
//                _material = null;
//            }
//            else if (child is Shader)
//            {
//                System.Diagnostics.Debug.Assert(_shader == child);
//                _shader = null;
//            }
//            else if (child is Layer)
//            {
//                RemoveTexture((Layer)child);
//            }
//            base.RemoveChild(child);
//            SetDefines();
//            SetChangeFlags(Enums.ChangeStates.AppearanceChanged, Enums.ChangeSource.Self);
//        }

//        internal void RemoveShader() { if (_shader != null) RemoveChild(_shader); }
//        public void RemoveMaterial() { if (_material != null) RemoveChild(_material); }
        

//        private void AddTexture(Layer tex)
//        {
//            _layers = _layers.ArrayAppend(tex);
//        }

//        private void RemoveTexture(Layer tex)
//        {

//            if (_layers == null) throw new ArgumentOutOfRangeException();

//            _layers.ArrayRemove(tex);

//            if (mDeletedLayers == null)
//                mDeletedLayers = new List<int>();

//            mDeletedLayers.Add(tex.LayerID);
//        }
//        #endregion

//        #region Shader Parameter Storage
//        /// <summary>
//        /// Shader Parameters are stored as PropertySpec[] in the Shader.cs.
//        /// But here in GroupAttribute, we only store the values.  
//        /// </summary>
//        /// <param name="specOnly"></param>
//        /// <returns></returns>
//        public Settings.PropertySpec[] GetShaderParameters(bool specOnly)
//        {
//            if (_shader == null || _shader.Parameters == null) return null;

//            Settings.PropertySpec[] specs = _shader.Parameters;

//            if (specOnly) return specs;
            
//            for (int i = 0; i < specs.Length; i++)
//                specs[i].DefaultValue = GetShaderParamaterValue(specs[i].Name);

//            return specs;
//        }

//        // shader parameters are handled the same was as custom domainobject parameters
//        private object GetShaderParamaterValue(string parameterName)
//        {
//            if (mParameterValues == null) return null;

//            //System.Diagnostics.Debug.Assert(mParameterValues.ContainsKey(parameterName));
//            object result = null;
//            bool found = mParameterValues.TryGetValue(parameterName, out result);

//            return result;
//        }

//        public void SetShaderParameterValue(string parameter, object value)
//        {
//            if (mParameterValues == null) mParameterValues = new Dictionary<string, object>();


//            // TODO: if the parameter values are restored before the shader is loaded and compiled
//            // then mParameterValues will still be null. 
//            // TODO: recall that we oftne programmatically will try to SetShaderParameterValue when 
//            // for instance, loading our tilemaskgrid using atlas texture
//            mParameterValues[parameter] = value;
//            SetChangeFlags(Enums.ChangeStates.AppearanceChanged 
//                | Enums.ChangeStates.ShaderParameterValuesChanged, Enums.ChangeSource.Self);
//        }

//        // shader parameters are handled similarly to Entity's domain object custom properties
//        // however unlike DomainObject, here we do not validate using rules
//        public void SetShaderParameterValues(Settings.PropertySpec[] parameters)
//        {
//            if (mParameterValues == null) mParameterValues = new Dictionary<string, object>();

//            for (int i = 0; i < parameters.Length; i++)
//            {
//                //System.Diagnostics.Debug.Assert(mParameterValues.ContainsKey(parameters[i].Name));
//                // NOTE: We do not assign shader parameter values to shader itself.  Those are 
//                // done on Appearance.Apply()
//                mParameterValues[parameters[i].Name] = parameters[i].DefaultValue;
//            }

//            SetChangeFlags(Enums.ChangeStates.AppearanceChanged 
//                | Enums.ChangeStates.ShaderParameterValuesChanged, Enums.ChangeSource.Self);
//        }

//        protected void ApplyShaderParameterValues()
//        {
//            if (_shader == null || _shader.PageStatus != PageableNodeStatus.Loaded)
//            {
//                System.Diagnostics.Debug.WriteLine("GroupAttribute.ApplyShaderParameterValues() - Shader not loaded.");
//                return;
//            }
//            if (mParameterValues != null && mParameterValues.Count > 0)
//            {
//                foreach (string key in mParameterValues.Keys)
//                {
//                    _shader.SetShaderParameter(key, mParameterValues[key]);
//                } 
//            }

//            // NOTE: we only disable this flag after the changed parameter values have been applied
//            // TODO: but what if shader errors or never loads?  
//            DisableChangeFlags(Keystone.Enums.ChangeStates.ShaderParameterValuesChanged);
//        }

//        // XML Persisted shader property values are parse from a single persist string
//        // and stored into the mParameterValues dictionary.
//        protected void RestoreCustomParameterValuesFromPersistString()
//        {
//            if (_shader != null && _shader.TVResourceIsLoaded)
//            {
//                mParameterValues = Helpers.ExtensionMethods.ParseCustomPropertiesPersistString(_shader.Parameters, mPersistedParameterValues);
//                SetChangeFlags(Keystone.Enums.ChangeStates.ShaderParameterValuesChanged, Keystone.Enums.ChangeSource.Self);
//            }
//        }
//        #endregion

//        #region Updates
//        internal void Update(double elapsedSeconds)
//        {
//            // TODO: This is wrong.  GroupAttribute will not update Layer TextureCycles anymore.
//            // Instead, AnimatedTextures will automatically be retreivable as Animations
//            // that can be broken up into sub-animations just like normal Actor.Animations are.
//            // Adding of a SpriteSheet to an Appearance will notify up the chain to the entity so that
//            // default Animations can be extracted.
//            if (_layers != null)
//            {
//                for (int i = 0; i < _layers.Length; i++)
//                    if (_layers[i] is TextureCycle)
//                        ((TextureCycle)_layers[i]).Update(elapsedSeconds);

//            }

//            // update any shader parameters.  This must be done for all groups too
//            // TODO: i think this entire Update() can be deleted.  Shader parameters
//            // are now updated on Apply() if the hashcode is changed so just like everything else
//            // Now eventually we may modify our HashCode to instead be a bit flag to denote what specifically
//            // has changed whether it's JUST the shader or JUST the material, or both  or neither, etc
//            if (_shader != null)
//                _shader.Update(elapsedSeconds);
//        }
//        #endregion

//        #region Appliance of Appearance to Geometry
//        public virtual int Apply(Keystone.Entities.Terrain land, double elapsedSeconds) // chunk?  I might make it so terrains are always handled per chunk
//        {
//            return 0;
//        }

//        public virtual int Apply(TexturedQuad2D quad, double elapsedSeconds)
//        {
//            return 0;
//        }

//        public virtual int Apply(Actor3dDuplicate actor, double elapsedSeconds)
//        {
            
//            // TODO: can't some things be set by actor group?
//            //actor.BlendingMode 
//            return 0;
//        }

//        //public virtual int Apply(Actor3d actor, double elapsedSeconds)
//        //{
//        //    return 0;
//        //}

//        public virtual int Apply(Mesh3d mesh, double elapsedSeconds)
//        {

//            // NOTE: mesh.Shader here applies per group if applicable
//            // NOTE: This GroupAttribute.Apply() is called by DefaultAppearance.Apply() since it
//            // uses exact same code 
//            mesh.Shader = _shader;

//            if ((_changeStates & Keystone.Enums.ChangeStates.ShaderParameterValuesChanged) != 0)
//            {
//                ApplyShaderParameterValues();
//            }

//            mesh.SetBlendingMode(_blendingMode, GroupID);

//            if (_material != null)
//                mesh.SetMaterial(_material.TVIndex, GroupID);

//            if (_layers != null)
//                for (int i = 0; i < _layers.Length; i++)
//                {
//                    if (_layers[i].TVResourceIsLoaded)
//                    {
//                        System.Diagnostics.Debug.Assert(_layers[i].TextureIndex != -1);
//                        System.Diagnostics.Debug.Assert(_layers[i].LayerID != -1);
//                        mesh.SetTextureEx(_layers[i].LayerID, _layers[i].TextureIndex, GroupID);
//                        if (_layers[i].TextureModEnabled)
//                        {
//                            mesh.SetTextureMod(_layers[i].LayerID, GroupID, _layers[i].TranslationU, _layers[i].TranslationV, _layers[i].TileU, _layers[i].TileV);
//                            mesh.SetTextureRotation(_layers[i].LayerID, GroupID, _layers[i].Rotation);
//                        }
//                    }
//                    else
//                        mesh.SetTextureEx(_layers[i].LayerID, -1, GroupID);
//                }
            

//            if (mDeletedLayers != null)
//            {
//                for (int i = 0; i < mDeletedLayers.Count; i++)
//                    mesh.SetTextureEx(mDeletedLayers[i], -1, GroupID);

//                mDeletedLayers.Clear();
//                mDeletedLayers = null;
//            }

//            return _hashCode;
//        }

//        public virtual int Apply(MinimeshGeometry mini, double elapsedSeconds)
//        {
//            mini.SetBlendingMode(_blendingMode);

//            // NOTE: mesh.Shader here applies to main shader and not by group.
//            // NOTE: This GroupAttribute.Apply() is called by DefaultAppearance.Apply() since it
//            // uses exact same code 
//            mini.Shader = _shader;

//            //Settings.PropertySpec[] specs = GetShaderParameters(false);
//            //if (_shader != null && specs != null)
//            //{
//                //for (int i = 0; i < specs.Length; i ++)
//                //    _shader.SetShaderParameter(specs[i].TypeName, specs[i].Name, specs[i].DefaultValue);
//            //}



//            if (_material != null && _material.TVResourceIsLoaded)
//                mini.SetMaterial(_material.TVIndex);

//            // NOTE: Minimeshes can have multiple texture layers but only ONE group.
//            if (_layers != null)
//                for (int i = 0; i < _layers.Length; i++)
//                {
//                    if (_layers[i].TVResourceIsLoaded)
//                    {
//                        System.Diagnostics.Debug.Assert(_layers[i].TextureIndex != -1);
//                        System.Diagnostics.Debug.Assert(_layers[i].LayerID != -1);
//                        mini.SetTextureEx(_layers[i].LayerID, _layers[i].TextureIndex);
//                        // NOTE: No texture mods built into tv3d for minimeshes
//                        //if (_layers[i].TextureModEnabled)
//                        //{
//                            //mini.SetTextureMod(_layers[i].LayerID, GroupID, _layers[i].TranslationU, _layers[i].TranslationV, _layers[i].TileU, _layers[i].TileV);
//                            //mini.SetTextureRotation(_layers[i].LayerID, GroupID, _layers[i].Rotation);
//                        //}
//                    }
//                    else
//                        mini.SetTextureEx(_layers[i].LayerID, -1);
//                }


//            if (mDeletedLayers != null)
//            {
//                for (int i = 0; i < mDeletedLayers.Count; i++)
//                    mini.SetTextureEx(mDeletedLayers[i], -1);

//                mDeletedLayers.Clear();
//                mDeletedLayers = null;
//            }

//            return _hashCode;
//        }

//        public virtual int Apply(Minimesh2 mini, double elapsedSeconds)
//        {
//            return 0;
//        }
//        #endregion

//        public override int GetHashCode()
//        {
//            if ((_changeStates & Enums.ChangeStates.AppearanceChanged | Enums.ChangeStates.ShaderFXLoaded | Enums.ChangeStates.ShaderFXUnloaded) > 0)
//                ComputeHashCode();

//            return _hashCode;
//        }

//        internal int LastHashCode { get { return _hashCode; } }
//        // The purpose of a hashcode
//        // is NOT for tracking changes, it's for quickly tracking the difference in
//        // appearance on a mesh between one instance render and the next so we know if we
//        // have to update the textures/shaders/materials on a mesh/actor/mini/etc or not.
//        internal void ComputeHashCode()
//        {
//            _hashCode = (int)GetHashData();
//        }

//        protected virtual uint GetHashData()
//        {
//            uint result = 0;
            
//            byte[] bytes = BitConverter.GetBytes((int)_blendingMode);
//            Keystone.Utilities.JenkinsHash.Hash(bytes, ref result);

//            if (_children != null)
//            {
//                for (int i = 0; i < _children.Count; i++)
//                {
//                    if (_children[i] is Material)
//                        Keystone.Utilities.JenkinsHash.Hash(BitConverter.GetBytes(_children[i].GetHashCode()), ref result);
//                    else if (_children[i] is Shader)
//                    {
//                        // TODO: shouldn't this take into account shader parameter changes? or never
//                        // because parameters get updated seperately
//                        // TODO: i think yes it should take into account parameter changes so we can
//                        // still avoid shader changes between similarly rendered items
//                        if (mParameterValues != null && mParameterValues.Count > 0)
//                        {
//                            // TODO: can we/should we use seperate shader parameter hashcodes
//                            //       so we dont needlessly re-apply materials and textures
//                            //       just cuz shader parameter has changed?
//                            // TODO: can we createa  .JeninksHash helper to hashcode 
//                            // an array of propertyspec?
//                            //Keystone.Helpers.Functions.JenkinsHash.Hash (GetShaderParameters(false), ref result);
//                        }
//                        Keystone.Utilities.JenkinsHash.Hash((((Shader)_children[i]).ID), ref result);
//                    }
//                    else if (_children[i] is Layer)
//                        Keystone.Utilities.JenkinsHash.Hash(BitConverter.GetBytes(_children[i].GetHashCode()), ref result);
//                    else if (_children[i] is GroupAttribute)
//                        Keystone.Utilities.JenkinsHash.Hash(BitConverter.GetBytes(((GroupAttribute)_children[i]).GetHashData()), ref result);
//                }
//            }

//            return result;
//        }
//    }
#endregion
}
