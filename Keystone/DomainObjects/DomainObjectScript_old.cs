using System;
using System.Xml;
using Keystone.IO;
using Keystone.Resource;
using System.Reflection;
using System.Collections.Generic;
using Keystone.Types;

namespace Keystone.Elements
{
    // OBSOLETE - Script is now merged into DomainObject.cs
    //NOTE: There is a seperate Behavior.Actions.Script that is derived from Action.cs
    // and used with the BehaviorTree.  This DomainObjectScript is an independant script node
    // used with Entities and DomainObjects
    public class DomainObjectScript : Node, IPageableTVNode 
    {
        private CSScriptLibrary.CSScript.LoadedScript mScript;
        private Dictionary<string, CSScriptLibrary.MethodDelegate> mMethods;
        private readonly object mSyncRoot;

        protected PageableNodeStatus _resourceStatus;

        protected DomainObjectScript(string resourcePath)
            : base(resourcePath)
        {
            mSyncRoot = new object();
        }

        public static DomainObjectScript Create(string resourcePath)
        {
            if (String.Empty == resourcePath) throw new ArgumentNullException();
            DomainObjectScript node;
            node = (DomainObjectScript)Repository.Get(resourcePath);
            if (node != null) return node;

            node = new DomainObjectScript( resourcePath);
            // TODO: should we compile the script here first and set the mScript property?
            // or should it be paged in like a resource (queued and loaded in seperate thread)

            // not, that should be called from LoadScript which in turn is called from LoadTVResource()
            return node;
        }

        #region ITraversable members
        public override object Traverse(Keystone.Traversers.ITraverser target, object data)
        {
            return target.Apply(this, data);
        }

        internal override Keystone.Traversers.ChildSetter GetChildSetter()
        {
            throw new NotImplementedException();
        }
        #endregion


        #region ResourceBase members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="specOnly">True returns the properties without any values assigned</param>
        /// <returns></returns>
        //public override Settings.PropertySpec[] GetProperties(bool specOnly)
        //{
        //    Settings.PropertySpec[] tmp = base.GetProperties(specOnly);
        //    Settings.PropertySpec[] properties = new Settings.PropertySpec[1 + tmp.Length];
        //    tmp.CopyTo(properties, 1);

        //    properties[0] = new Settings.PropertySpec("eventname", mEventName.GetType().Name);

        //    if (!specOnly)
        //    {
        //        properties[0].DefaultValue = mEventName;
        //    }

        //    return properties;
        //}

        //public override void SetProperties(Settings.PropertySpec[] properties)
        //{
        //    if (properties == null) return;
        //    base.SetProperties(properties);

        //    for (int i = 0; i < properties.Length; i++)
        //    {
        //        // use of a switch allows us to pass in all or a few of the propspecs depending
        //        // on whether we're loading from xml or changing a single property via server directive
        //        switch (properties[i].Name)
        //        {
        //            case "eventname":
        //                mEventName = (string)properties[i].DefaultValue;
        //                break;
        //        }
        //    }
        //}
        #endregion

        #region IPageableTVNode Members
        public object SyncRoot { get { return mSyncRoot; } }

        public int TVIndex
        {
            get { if (mScript == null || mScript.asm == null) return -1; else return 0; }
        }

        public bool TVResourceIsLoaded
        {
            get { return mScript != null && mScript.asm != null; }
        }

        public string ResourcePath
        {
            get { return _id; }
            set { _id = value; }
        }

        public PageableNodeStatus PageStatus { get { return _resourceStatus; } set { _resourceStatus = value; } }

        public void UnloadTVResource()
        {
        	// TODO: UnloadTVResource()
        }
                
        public void LoadTVResource()
        {
            KeyCommon.IO.ResourceDescriptor descriptor = new KeyCommon.IO.ResourceDescriptor(_id);
            if (descriptor.IsArchivedResource)
            {
            }
            else
            {
            }

            string zipFilePath = Keystone.Core.FullNodePath(descriptor.ModName);
            // TODO: error check if code returns ""
            string code = KeyCommon.IO.ArchiveIOHelper.GetTextFromArchive(descriptor.EntryName, "", zipFilePath);
            // TODO: error check when trying to load the same code using same path twice
            // i know this errors but im not sure why it's not trying to recycle the existing and
            // perhaps notify of the error,... something ACTUALLY i think it errors more specifically
            // if somehow due to some loading error on a previous attempt, its not locked and you 
            // cant change it with a subsequent successful laod... i dunno, needs tons of error handling
            CSScriptLibrary.CSScript.LoadedScript loadedScript = Core._Core.ScriptLoader.LoadCode(code, _id, true);

            if (loadedScript == null) return;

            // TODO: we can determine the classname easily enough, but its problematic if there's more than one in the assembly
            Module[] modules = loadedScript.asm.GetModules();
            Type[] types = modules[0].GetTypes();
            if (modules == null || modules.Length != 1) return; // only one module allowed in the assembly (for now?)
            if (types == null) return; // TODO: I dont know why it's returning 2 array elements so that the following lenth test of 1 fails... || types.Length != 1) return; // only one public type allowed (for now?)
            MethodInfo[] methods = types[0].GetMethods();


            if (methods != null && methods.Length > 0)
                for (int i = 0; i < methods.Length; i++)
                {
                    string fullMethodName = "";
                    object[] args = null;
                    System.Diagnostics.Debug.WriteLine(methods[i].Name);
                    fullMethodName = types[0].Name + "." + methods[i].Name;

                    CSScriptLibrary.MethodDelegate foundMethod = null;

                    bool supportedMethod = true;
                    // NOTE: the only point of this switch is to customize the expected list
                    // of arguments that each script function requires because to search
                    // for the method when calling assembly.GetStaticMethod requires the list
                    // of parameter types
                    switch (methods[i].Name)
                    {
                        case "Initialize":
						case "InitializeEntity":
                    		args = new object[] { "" };
                            break;
                        case "OnRemovedFromParent":
                        case "OnAddedToParent":
                            args = new object[] { "", "" };
                            break;

                        case "RegisterChild":
                        case "UnRegisterChild":
                            args = new object[] { "", "" };
                            break;

                        case "Validate":
                            args = new object[] { "", "", new object() };
                            break;
                        case "QueryPlacementBrushType":
                            args = new object[] { };  // no args, verified can't just pass null (though i could write a custom overload in CSScript to allow so)
                            break;
                        case "QueryCellPlacement":
                            args = new object[] { "", "", new Vector3d(), (byte)0};
                            break;
                        case "QueryPlacement":
                            args = new object[] { "", new Vector3d(), new Vector3d(), new Vector3d() };
                            break;

                        default:
                            supportedMethod = false;
                            // ToObject();
                            // Equals()
                            // GetHashCode();
                            // ToString();
                            System.Diagnostics.Debug.WriteLine("DomainObjectScript.LoadTVResource() - Unsupported method '" + methods[i].Name + "'");
                            break;
                    }

                    if (supportedMethod)
                    {
                        foundMethod = loadedScript.asm.GetStaticMethod(fullMethodName, args);
                        if (foundMethod != null)
                        {
                            if (mMethods == null)
                                mMethods = new Dictionary<string, CSScriptLibrary.MethodDelegate>();

                            mMethods.Add(methods[i].Name, foundMethod);
                            foundMethod = null;
                        }
                    }
                }


            mScript = loadedScript;
            SetChangeFlags(Keystone.Enums.ChangeStates.EntityScriptLoaded, Keystone.Enums.ChangeSource.Self);

            //  // below method if for loading an entire object instances rather than the static method version we will mostly use
            //  //var update = mScriptLoader.Assembly.GetStaticMethod("*.Update");
            // // KeyScript.Interfaces.IDynamicEntity mod ;//= mScriptLoader.Assembly.GetModule("data.csc").AlignToInterface<KeyScript.Interfaces.IDynamicEntity>();
            ////  mod = (KeyScript.Interfaces.IDynamicEntity)mScriptLoader.Assembly.CreateObject("DefaultDynamicEntity");
            //  //Module[] modules = mScriptLoader.Assembly.GetLoadedModules();
            //  //mod = (KeyScript.Interfaces.IDynamicEntity)modules[0];
            // // mod.Update();
        }

        public void SaveTVResource(string filepath)
        {
            throw new NotImplementedException();
        }
        #endregion


        public object Execute(string eventName, object[] args)
        {
            if (!Enable) return null; // BehaviorResult.Fail;
            if (!TVResourceIsLoaded) return null; // BehaviorResult.Fail;
            if (mMethods == null) return null; // BehaviorResult.Fail;

            CSScriptLibrary.MethodDelegate result;
            if (!mMethods.TryGetValue(eventName, out result))
                // TODO: our scripts dont return BehaviorResults, only our
                // nodes do.  It's up to our nodes to define when
                // a particular script is Running or Success/Completed
                // not our ScriptNodes... i need to verify that is right way to implement
                // and that it makes sense from the BehaviorTree perspective
                // WAIT, how on earth is for instance a "MoveTo" script supposed to know
                // when the script has completed moving an entity to a waypoint without
                // the script notifying?
                return null; // BehaviorResult.Success;

            try
            {
                if (args == null || args.Length == 0)
                {
                    // call overloaded version that accepts no args
                    // or maybe throw exception if null args passed is better?
                    // or maybe just returning error is best?
                    return result();
                    //throw new ArgumentNullException();
                    // BehaviorResult.Error_Script_Invalid_Arguments;
                }
                else
                {
                    return result(args);
                    // BehaviorResult.Success;
                    //return (BehaviorResult)result(args);
                }
            }
            catch (Exception ex)
            {
                return null; // BehaviorResult.Error_Script_Exception;
            }
        }

        #region IDisposable Members
        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();

            SetChangeFlags(Keystone.Enums.ChangeStates.DomainScriptUnloaded, Keystone.Enums.ChangeSource.Self);
                        
            //if (mScript != null)
            //{
            //   mScript.as
            //}
            mMethods = null;
            mScript = null;
        }
        #endregion

    }
}
