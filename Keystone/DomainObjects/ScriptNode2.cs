//using System;
//using System.Xml;
//using Keystone.IO;
//using Keystone.Resource;
//using System.Reflection;
//using System.Collections.Generic;

//namespace Keystone.Elements
//{
//    // TODO: this should inherit from Action behavior node!
//    // thus our ScriptNode2 is a specific type of Action node
//    // and this i think resolves some questions 
//    // for instance, since this is now a type of Action, we do know
//    // rather than return object Exectue() we return BehaviorResult Execute()
//    public class ScriptNode2 : Node, IPageableTVNode 
//    {
//        public enum EventNames
//        {
//            Execute,
//            Validate,
//            Enter,
//            Exit
//        }

//        private CSScriptLibrary.CSScript.LoadedScript mScript;
//        private Dictionary <string, CSScriptLibrary.MethodDelegate>  mMethods;

//        protected PageableNodeStatus _resourceStatus;

//        protected ScriptNode2(string resourcePath)
//            : base(resourcePath)
//        {
//        }


//        public static ScriptNode2 Create(string resourcePath)
//        {
//            if (String.Empty == resourcePath) throw new ArgumentNullException();
//            ScriptNode2 node;
//            node = (ScriptNode2)Repository.Get(resourcePath);
//            if (node != null) return node;

//            node = new ScriptNode2(resourcePath);
//            // TODO: should we compile the script here first and set the mScript property?
//            // or should it be paged in like a resource (queued and loaded in seperate thread)

//            // not, that should be called from LoadScript which in turn is called from LoadTVResource()
//            return node;
//        }

//        public static ScriptNode2 Create(string id)
//        {
//            ScriptNode2 node;
//            node = (ScriptNode2)Repository.Get(id);
//            if (node != null) return node;
//            node = new ScriptNode2(id);
//            return node;
//        }

//        // since this now becomes a type of 
//        // valid event names are Validate/Execute/Enter/Exist

//        // Validate() <-- are preconditions met such that this Action script can be invoked?
//        // Enter() <-- First time the script is Entered ONLY.  If it was previously "Running"
//        //             then the Enter is not called again.
//        // Exit()  <-- Occurs when the Behavior was accessed previous Tick, but not the following.
//        // So I think the above answers our question regarding scripts returning
//        // BehaviorResults.  They will.  
//        public Behavior.BehaviorResult Execute(string eventName)
//        {
//            if (!_enable) return Behavior.BehaviorResult.Fail;
//            if (!TVResourceIsLoaded) return Behavior.BehaviorResult.Fail;
//            if (mMethods == null) return Behavior.BehaviorResult.Fail;

//            CSScriptLibrary.MethodDelegate result;
//            if (!mMethods.TryGetValue(eventName, out result))
//                return Behavior.BehaviorResult.Success;
            
//            try 
//            {
//                // TODO: lets imagine it's a MoveTo() script
//                // as long as the bool Exit() condition evaluates to false
//                // we return Running, else Success?  Is that proper?  Hrm
//                // Is there ever a situation where the script would need to return
//                // differnet conditions depending on the context in which it was used?
//                // That would suggest the script itself should never return BehaviorResult
//                // but such scenario seems unlikely or so extremely rare as to be inconsequential
//                // The single main issue for the return type is it seems to add yet another thing
//                // that the script writer could screw up... or is it really a problem?
//                // WHAT ARE THE STEPS TO ANY ACTION
//                // - Should we run it?  <-- validate
//                // - Run it        
//                //      - Did we fail?  <-- how is this determined if not as at least true/false return result
//                // 
//                //      - else not Fail, 
//                //          - did we finish?
//                //          - else still Running  <-- if havent failed, check Exit condition to determine if completed?

//                // Performance wise easiest to just return the Result in the script
//                // rather than make second call to a "bool CompletionSuccessful()" function
//                // Consider Steer()... that really is a function, not a script
//                // we Steer() towards something, but the script would contain the rules 
//                // for higher actions like "Follow" or "Evade"
//                // Let's say our Follow() action script typically returns "Running" unless
//                // the target it's following is null (fails precondition) (perhaps the target was destroyed)
//                // or exceeds some max distance and thus is beyond detection range 
//                // So if your ship for instance had a standing order to it's helsman to Intercept()
//                // some target, and maybe it transitions to "Persuit_At_Distance" when it becomes close enough
//                // and when it doesn't want to overshoot,
//                // 
//                return mMethods[eventName]();
//            }
//            catch (Exception ex)
//            {
//                return Behavior.BehaviorResult.Error_Script_Exception;
//            }
//        }

//        public Behavior.BehaviorResult Execute(string eventName, object[] args)
//        {
//            if (args == null || args.Length == 0)
//            {
//                // call overloaded version that accepts no args
//                // or maybe throw exception if null args passed is better?
//                // or maybe just returning error is best?
//               //return Execute(eventName);
//               //throw new ArgumentNullException();
//                return Behavior.BehaviorResult.Error_Script_Invalid_Arguments;
//            }

//            if (!_enable) return Behavior.BehaviorResult.Fail;
//            if (!TVResourceIsLoaded) return Behavior.BehaviorResult.Fail;
//            if (mMethods == null) return Behavior.BehaviorResult.Fail;

//            CSScriptLibrary.MethodDelegate result;
//            if (!mMethods.TryGetValue(eventName, out result))
//                // TODO: our scripts dont return BehaviorResults, only our
//                // nodes do.  It's up to our nodes to define when
//                // a particular script is Running or Success/Completed
//                // not our ScriptNodes... i need to verify that is right way to implement
//                // and that it makes sense from the BehaviorTree perspective
//                // WAIT, how on earth is for instance a "MoveTo" script supposed to know
//                // when the script has completed moving an entity to a waypoint without
//                // the script notifying?
//                return Behavior.BehaviorResult.Success;

//            try
//            {
//                return mMethods[eventName](args);
//            }
//            catch (Exception ex)
//            {
//                return Behavior.BehaviorResult.Error_Script_Exception;
//            }
//        }

//        #region ITraversable members
//        public override object Traverse(Keystone.Traversers.ITraverser target, object data)
//        {
//            return target.Apply(this, data);
//        }

//        internal override Keystone.Traversers.ChildSetter GetChildSetter()
//        {
//            throw new NotImplementedException();
//        }
//        #endregion


//        #region ResourceBase members


//        ///// <summary>
//        ///// 
//        ///// </summary>
//        ///// <param name="specOnly">True returns the properties without any values assigned</param>
//        ///// <returns></returns>
//        //public override Settings.PropertySpec[] GetProperties(bool specOnly)
//        //{
//        //    Settings.PropertySpec[] tmp = base.GetProperties(specOnly);
//        //    Settings.PropertySpec[] properties = new Settings.PropertySpec[1 + tmp.Length];
//        //    tmp.CopyTo(properties, 1);

//        //    properties[0] = new Settings.PropertySpec("eventname", mEventName.GetType());

//        //    if (!specOnly)
//        //    {
//        //        properties[0].DefaultValue = mEventName;
//        //    }

//        //    return properties;
//        //}

//        //public override void SetProperties(Settings.PropertySpec[] properties)
//        //{
//        //    if (properties == null) return;
//        //    base.SetProperties(properties);

//        //    for (int i = 0; i < properties.Length; i++)
//        //    {
//        //        // use of a switch allows us to pass in all or a few of the propspecs depending
//        //        // on whether we're loading from xml or changing a single property via server directive
//        //        switch (properties[i].Name)
//        //        {
//        //            case "eventname":
//        //                mEventName = (string)properties[i].DefaultValue;
//        //                break;
//        //        }
//        //    }
//        //}
//        #endregion

//        #region IPageableTVNode Members
//        public int TVIndex
//        {
//            get { if (mScript == null || mScript.asm == null) return -1; else return 0; }
//        }

//        public bool TVResourceIsLoaded
//        {
//            get { return mScript != null && mScript.asm != null; }
//        }

//        public string ResourcePath
//        {
//            get { return _id; }
//            set { _id = value; }
//        }

//        public PageableNodeStatus PageStatus { get { return _resourceStatus; } set { _resourceStatus = value; } }

//        public void LoadTVResource2()
//        {
//        }

//        public void LoadTVResource()
//        {
//            KeyCommon.IO.ResourceDescriptor descriptor = new KeyCommon.IO.ResourceDescriptor(_id);
//            if (descriptor.IsArchivedResource)
//            {
//            }
//            else
//            {
//            }

//            string zipFilePath = Keystone.Core.FullArchivePath(descriptor.RelativePathToArchive);
//            string code = KeyCommon.IO.ArchiveIOHelper.GetTextFromArchive(descriptor.ArchiveEntryName, "", zipFilePath);
//            CSScriptLibrary.CSScript.LoadedScript loadedScript = Core._Core.ScriptLoader.LoadCode(code, _id, true);

//            // TODO: we can determine the classname easily enough, but its problematic if there's more than one in the assembly
//            Module[] modules = loadedScript.asm.GetModules();
//            Type[] types = modules[0].GetTypes();
//            if (modules == null || modules.Length != 1) return; // only one module allowed in the assembly (for now?)
//            if (types == null || types.Length != 1) return; // only one public type allowed (for now?)
//            MethodInfo[] methods = types[0].GetMethods();

//            // TODO: below i use .GetStaticMethod (fullMethodName) which uses an mEventName
//            // but can't i instead iterate thru all methods and find the ones with the proper names
//            // and add those to a dictionary of methods?
//            if (methods != null && methods.Length > 0)
//                for (int i = 0; i < methods.Length; i++)
//                {
//                    string fullMethodName = "";
//                    object[] args = new object[] { "", 0 };
//                    System.Diagnostics.Debug.WriteLine(methods[i].Name);
//                    switch (methods[i].Name)
//                    {
//                        case "Validate":
//                        case "Exit":
//                        case "Enter":
//                        case "Execute":
//                            if (mMethods == null)
//                                mMethods = new Dictionary<string, CSScriptLibrary.MethodDelegate>();
//                            fullMethodName = types[0].Name + "." + methods[i].Name;

//                            mMethods.Add(methods[i].Name, loadedScript.asm.GetStaticMethod(fullMethodName, args));
//                            break;

//                        default:
//                            // ToObject();
//                            // Equals()
//                            // GetHashCode();
//                            // ToString();
//                            break;
//                    }
//                }
//            // I think a simple rule of enforcing the method name == the event name shoudl make this easy.
//            //string fullMethodName = types[0].Name + "." + mEventName;
//            //object[] args = new object[] { "", 0 }; // OnUpdate args // TODO: this has to be a var that is serialized and also public to be set (either property or our static create constructor)
//            //mMethods[] = loadedScript.asm.GetStaticMethod(fullMethodName, args);

//            mScript = loadedScript;

//            //mScript 
//            // todo compile and assign event methods here?  actually cant really assign methods here
//            // that has to be done by the entity... or wait, no the entity calls this mScript.Method.Execute() right?

//            //  // below method if for loading an entire object instances rather than the static method version we will mostly use
//            //  //var update = mScriptLoader.Assembly.GetStaticMethod("*.Update");
//            // // KeyScript.Interfaces.IDynamicEntity mod ;//= mScriptLoader.Assembly.GetModule("data.csc").AlignToInterface<KeyScript.Interfaces.IDynamicEntity>();
//            ////  mod = (KeyScript.Interfaces.IDynamicEntity)mScriptLoader.Assembly.CreateObject("DefaultDynamicEntity");
//            //  //Module[] modules = mScriptLoader.Assembly.GetLoadedModules();
//            //  //mod = (KeyScript.Interfaces.IDynamicEntity)modules[0];
//            // // mod.Update();
//        }

//        public void SaveTVResource(string filepath)
//        {
//            throw new NotImplementedException();
//        }
//        #endregion


//        protected override void DisposeManagedResources()
//        {
//            base.DisposeManagedResources();

//            if (mMethods != null)
//                mMethods.Clear();

//            mMethods = null;
//            mScript = null;

//            //if (mScript != null)
//            //{
//            //   mScript.as
//            //}
//        }
//    }
//}
