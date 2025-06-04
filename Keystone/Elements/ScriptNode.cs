using System;
using System.Xml;
using Keystone.IO;
using Keystone.Resource;
using System.Reflection;
using System.Collections.Generic;

namespace Keystone.Elements
{
    //NOTE: There is a seperate Behavior.Actions.Script that is derived from Action.cs
    // and used with the BehaviorTree.  This ScriptNode is an independant script node
    // used with Entities and DomainObjects
    public class DomainObjectScript : Node, IPageableTVNode 
    {
        private CSScriptLibrary.CSScript.LoadedScript mScript;
        private Dictionary<string, CSScriptLibrary.MethodDelegate> mMethods;

 
        protected PageableResourceStatus _resourceStatus;

        protected DomainObjectScript(string resourcePath)
            : base(resourcePath)
        {
        }


        public static DomainObjectScript Create(string resourcePath)
        {
            if (String.Empty == resourcePath) throw new ArgumentNullException();
            DomainObjectScript node;
            node = (DomainObjectScript)Repository.Get(resourcePath);
            if (node != null) return node;

            node = new DomainObjectScript( resourcePath);
            // todo: should we compile the script here first and set the mScript property?
            // or should it be paged in like a resource (queued and loaded in seperate thread)

            // not, that should be called from LoadScript which in turn is called from LoadTVResource()
            return node;
        }

        public object Execute(string eventName, object[] args)
        {
            if (args == null || args.Length == 0)
            {
                // call overloaded version that accepts no args
                // or maybe throw exception if null args passed is better?
                // or maybe just returning error is best?
                //return Execute(eventName);
                //throw new ArgumentNullException();
                return null; // BehaviorResult.Error_Script_Invalid_Arguments;
            }

            if (!_enable) return null; // BehaviorResult.Fail;
            if (!TVResourceIsLoaded) return null; // BehaviorResult.Fail;
            if (mMethods == null) return null; // BehaviorResult.Fail;

            CSScriptLibrary.MethodDelegate result;
            if (!mMethods.TryGetValue(eventName, out result))
                // todo: our scripts dont return BehaviorResults, only our
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
                result(args);
                return null; // BehaviorResult.Success;
                //return (BehaviorResult)result(args);
            }
            catch (Exception ex)
            {
                return null; // BehaviorResult.Error_Script_Exception;
            }
        }

        #region ITraversable members
        public override void Traverse(Keystone.Traversers.ITraverser target)
        {
            target.Apply(this);
        }

        public override Keystone.Traversers.ChildSetter GetChildSetter()
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
            get { return _id ; }
            set { _id = value; } 
        }

        public PageableResourceStatus ResourceStatus { get { return _resourceStatus; } set { _resourceStatus = value; } }

        public void LoadTVResource()
        {
            KeyCommon.IO.ResourceDescriptor descriptor = new KeyCommon.IO.ResourceDescriptor(_id);
            if (descriptor.IsArchivedResource)
            {
            }
            else
            { 
            }

            string zipFilePath = Keystone.Core.FullArchivePath(descriptor.RelativePathToArchive);
            string code = KeyCommon.IO.ArchiveIOHelper.GetTextFromArchive(descriptor.ArchiveEntryName, "", zipFilePath);
            CSScriptLibrary.CSScript.LoadedScript loadedScript = Core._Core.ScriptLoader.LoadCode(code, _id, true);

            if (loadedScript == null) return;

            // todo: we can determine the classname easily enough, but its problematic if there's more than one in the assembly
            Module[] modules = loadedScript.asm.GetModules();
            Type[] types = modules[0].GetTypes();
            if (modules == null || modules.Length != 1) return; // only one module allowed in the assembly (for now?)
            if (types == null || types.Length != 1) return; // only one public type allowed (for now?)
            MethodInfo[] methods = types[0].GetMethods();

            // todo: below i use .GetStaticMethod (fullMethodName) which uses an mEventName
            // but can't i instead iterate thru all methods and find the ones with the proper names
            // and add those to a dictionary of methods?
            if (methods != null && methods.Length > 0)
                for (int i = 0; i < methods.Length; i++)
                {
                    string fullMethodName = "";
                    object[] args = new object[] { "", 0 };
                    System.Diagnostics.Debug.WriteLine(methods[i].Name);
                    switch (methods[i].Name)
                    {
                        case "Initialize":
                        case "Validate":
                        case "Enter":
                        case "Execute":
                            if (mMethods == null)
                                mMethods = new Dictionary<string, CSScriptLibrary.MethodDelegate>();
                            fullMethodName = types[0].Name + "." + methods[i].Name;

                            mMethods.Add(methods[i].Name, loadedScript.asm.GetStaticMethod(fullMethodName, args));
                            break;

                        default:
                            // ToObject();
                            // Equals()
                            // GetHashCode();
                            // ToString();
                            break;
                    }
                }
            // I think a simple rule of enforcing the method name == the event name shoudl make this easy.
            //string fullMethodName = types[0].Name + "." + mEventName;
            //object[] args = new object[] { "", 0 }; // OnUpdate args // todo: this has to be a var that is serialized and also public to be set (either property or our static create constructor)
            //mMethods[] = loadedScript.asm.GetStaticMethod(fullMethodName, args);

            mScript = loadedScript;

            SetChangeFlags(Keystone.Enums.ChangeStates.ScriptLoaded, Keystone.Enums.ChangeSource.Self);

            //mScript 
            // todo compile and assign event methods here?  actually cant really assign methods here
            // that has to be done by the entity... or wait, no the entity calls this mScript.Method.Execute() right?

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


        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();

            SetChangeFlags(Keystone.Enums.ChangeStates.ScriptUnloaded, Keystone.Enums.ChangeSource.Self);
                        
            //if (mScript != null)
            //{
            //   mScript.as
            //}
            mMethods = null;
            mScript = null;
        }
    }
}
