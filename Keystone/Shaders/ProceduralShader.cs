//using System;
//using System.Collections.Generic;
//using Keystone.Resource;
//using Keystone.Elements;
//using MTV3D65;


//namespace Keystone.Shaders
//{
//    public class ProceduralShader : Shader
//    {
//        public enum GeometryMode
//        {
//            Mesh,
//            Actor,
//            Minimesh
//        }

//        public enum LightingModes
//        {
//            Forward,
//            Deferred
//        }

//        List <KeyValuePair<string, string>> mDefines;
        

//        private Shader mTargetShader; // NOTE: The target shader is NOT a child and will not be duplicated when an Appearance is copied.
//                                //       In that way, this ProceduralShader which _CAN_ be shared by
//        // other Appearances/GroupAttributes that use same shader src file, geometry type, 
//        // with same defines set, can load and compile a shader that is specific to it's needs and graphics settings


//        /// <summary>
//        /// The id is a random GUID since ProceduralShader cannot be duplicated.
//        /// However it will host a Shader that is built from defines.  These Shaders
//        /// can be shared as normal, however they are never saved as child nodes.  
//        /// That's because thy are not stored as child nodes and that's why this ProceduralShader
//        /// class is not a IGroup node.
//        /// </summary>
//        /// <param name="id"></param>
//        public ProceduralShader(string id, string targetResource)
//            : base(id, targetResource)
//        {
//            _resourceStatus = PageableResourceStatus.NotLoaded;
//            Shareable = false; // non procedural ARE shareable, but this ProceduralShader is NOT
//        }

        
//        #region ITraversable Members
//        public override object Traverse(global::Keystone.Traversers.ITraverser target, object data)
//        {
//            throw new System.Exception("The method or operation is not implemented.");
//        }
//        #endregion 

//        internal override Keystone.Traversers.ChildSetter GetChildSetter()
//        {
//            throw new System.Exception("The method or operation is not implemented.");
//        }

//        private string GetDefinesToString (List <KeyValuePair<string, string>> defines)
//        {
//            if (defines == null) return "";
//            string result = "";

//            foreach (KeyValuePair<string, string> p in defines)
//            {
//                if (result != "") result += ",";
//                result += p.Key + "=" + p.Value;    
//            }

//            return result;
//        }

//        private List<KeyValuePair<string, string>> GetDefinesFromString(string serializedString)
//        {
//            if (string.IsNullOrEmpty(serializedString)) return null;

//            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();

//            string[] tmp = serializedString.Split(',');
//            if (tmp == null || tmp.Length == 0) return null;

//            for (int i = 0; i < tmp.Length; i++)
//            {
//                string[] s = tmp[i].Split('=');
//                KeyValuePair<string, string> kvp = new KeyValuePair<string, string>(s[0], s[1]);
//                result.Add(kvp);
//            }

//            return result;
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
//            Settings.PropertySpec[] properties = new Settings.PropertySpec[2 + tmp.Length];
//            tmp.CopyTo(properties, 2);

//            properties[0] = new Settings.PropertySpec("targetresource", typeof(string).Name);
//            properties[1] = new Settings.PropertySpec("defines", typeof(string).Name);
            
//            if (!specOnly)
//            {
//                properties[0].DefaultValue = mShaderResourceDescriptor;
//                properties[1].DefaultValue = GetDefinesToString(mDefines);
//            }
//            return properties;
//        }

//        public override void SetProperties(Settings.PropertySpec[] properties)
//        {
//            if (properties == null) return;
//            base.SetProperties(properties);

//            for (int i = 0; i < properties.Length; i++)
//            {
//                // use of a switch allows us to pass in all or a few of the propspecs depending
//                // on whether we're loading from xml or changing a single property via server directive
//                switch (properties[i].Name)
//                {
//                    case "targetresource":
//                        mShaderResourceDescriptor  = (string)properties[i].DefaultValue;
//                        break;
//                    case "defines":
//                        mDefines = GetDefinesFromString ((string)properties[i].DefaultValue);
//                        break;
//                }
//            }
//        }
//        #endregion

//        #region IPageableTVNode Members
//        public override int TVIndex
//        {
//            get
//            {
//                if (mTargetShader == null) return -1;
//                else return mTargetShader.TVIndex;
//            }
//        }

//        public override bool TVResourceIsLoaded
//        {
//            get { return TVIndex > -1; }
//        }


//        public override void LoadTVResource()
//        {
//            // TODO: i think the following assert is potentially incorrect...
//            // if we change the defines, then we would want to unload the current mTarget shader
//            // and replace it with one with new defines... but only if the defines have changed
//            // since last time.
//            lock (mShaderLoaderLock)
//            {
//                System.Diagnostics.Debug.Assert(mTargetShader == null);

//                // TODO: I think what we're doing here is stupid... why are we hosting another
//                //       Shader object and not just direct TVShader?
//                //       - i cant remember why i thought this was a good idea.  I know that
//                //       the underlying Shader here is not loaded as a child node.  It's a direct resource
//                //       it loads.
//                //       - i think we should try to do what we do for Mesh Primitives and that is construct
//                //       an "id" where the defines are stored there so that ProceduralShaders are really
//                //       not procedural anymore...  
//                //
//                // the target shader is always created using same key so that it will
//                // search for existing instances it can share, otherwise it will create
//                // a new target.  But note that these shaders are not treated as children
//                // and so are never saved.  All the info we need to recreate those shaders
//                // instances are here in ProceduralShader.cs and saved into the xml db.
//                if (mDefines != null)
//                {
//                    KeyValuePair<string, string>[] defines = mDefines.ToArray();
//                    mTargetShader = Keystone.Shaders.Shader.Create(mShaderResourceDescriptor, defines);
//                }
//                else
//                {
//                    mTargetShader = Keystone.Shaders.Shader.Create(mShaderResourceDescriptor, mShaderResourceDescriptor);
//                }

//                System.Diagnostics.Trace.Assert(mTargetShader != null, "ProceduralShader.LoadTVResource() -- Why is Shader.cs instance null?");

//                // must not be able to save this inner Shader.cs to xml in either prefab or scene
//                mTargetShader.SetFlagValue("serializable", false);

//                if (mTargetShader.TVResourceIsLoaded == false) mTargetShader.LoadTVResource();
//                if (mTargetShader.TVResourceIsLoaded)
//                    // the target is not a real child so no change flag notification will propogate from it so we do it here
//                    SetChangeFlags(Keystone.Enums.ChangeStates.ShaderFXLoaded, Keystone.Enums.ChangeSource.Self);
//                //else
//                //    if it fails, this ProceduralShader uses it's child's TVResourceIsLoaded and ResourceStatus
//                //    so we don't need to set anything like that here


//                // we MUST increment the target shader (regardless of whether it's successfully loaded or not)
//                // because Creation adds it to the
//                // REpository but does not increment it's key until an AddChild() occurs which doesnt
//                // happen here because ProceduralShader is not a true group node and the mTarget is not treated
//                // as a true child (eg. is never serialized via xml)
            
//                Repository.IncrementRef(mTargetShader);
//            }
//        }
//#endregion

//        public override TVShader TVShader
//        {
//            get
//            {
//                if (mTargetShader == null) return null;
//                return mTargetShader.TVShader;
//            }
//        }

//        public override int PassCount
//        {
//            get
//            {
//                if (mTargetShader == null) return 0;
//                return mTargetShader.PassCount;
//            }
//        }

//        public override int TechniqueCount
//        {
//            get
//            {
//                if (mTargetShader == null) return 0;
//                return mTargetShader.TechniqueCount;
//            }
//        }

//        public override string[] Techniques
//        {
//            get
//            {
//                if (mTargetShader == null) return null;
//                return mTargetShader.Techniques;
//            }
//        }


//        public override int ParametersCount  
//        {
//            get
//            {
//                if (mTargetShader == null) return 0;
//                return mTargetShader.ParametersCount;
//            }
//        }

//        public override Settings.PropertySpec[] Parameters // see domain object for how to handle these
//        {
//            get
//            {
//                if (mTargetShader == null) return null;
//                return mTargetShader.Parameters;
//            }
//        }

//        public override void SetShaderParameter(string typeName, string parameterName, object value)
//        {
//            if (mTargetShader == null) return;
//            mTargetShader.SetShaderParameter(typeName, parameterName, value);
//        }


//        public void ClearDefines()
//        {
//            mDefines = new List<KeyValuePair<string, string>>();
//            DisposeManagedResources(); // unload any existing child target shader
//        }

//        // NOTE: Defines are normally added or removed by GroupAttribute.cs which monitors
//        // changes to the Appearance
//        public void AddDefine(string key, string value)
//        {
//            if (mDefines == null) mDefines = new List<KeyValuePair<string, string>>();

//            // remove any existing define at this key
//            RemoveDefine(key);

//            // add the new
//            mDefines.Add(new KeyValuePair<string, string>(key, value));
            
//        }

//        public void RemoveDefine(string key)
//        {
//            if (mDefines == null) return;

//            foreach (KeyValuePair<string, string> kvp in mDefines)
//                if (kvp.Key == key)
//                {
//                    mDefines.Remove(kvp);
//                    break;
//                }
//        }

//        private object mShaderLoaderLock = new object();
//        protected override void DisposeManagedResources()
//        {
//            // NOTE: when changing appearance attributes in GroupAttribute
//            // a new shader may need to be set or built.  If that is the case
//            // then if were previously loading a shader while also attempting to modify
//            // it's GroupAttributes (which changes the shader DEFINES) then we need to sychronize
//            // thread access
//            lock (mShaderLoaderLock)
//                if (mTargetShader != null)
//                {
//                    Repository.DecrementRef(mTargetShader);
//                    // TODO: are we somehow disposing when we shouldnt?
//                    mTargetShader = null;
//                    _resourceStatus = PageableResourceStatus.NotLoaded;
//                    _changeStates = Keystone.Enums.ChangeStates.ShaderFXUnloaded;
//                }
//        }
//    }
//}
