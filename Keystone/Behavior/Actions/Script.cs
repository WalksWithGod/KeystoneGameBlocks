using System;
using System.Collections.Generic;
using KeyCommon.Traversal;
using Keystone.Elements;
using Keystone.IO;
using Keystone.Resource;

namespace Keystone.Behavior.Actions
{
	// http://www.csscript.net/help/Image_processor.html
    public class Script : Action, IPageableTVNode 
    {
        private CSScriptLibrary.CSScript.LoadedScript mScript;
        private Dictionary <string, CSScriptLibrary.MethodDelegate>  mMethods;
        private readonly object mSyncRoot;

        protected PageableNodeStatus _resourceStatus;

        internal Script(string resourcePath)
            : base(resourcePath)
        {
            mSyncRoot = new object();
            Shareable = true;
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
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="specOnly">True returns the properties without any values assigned</param>
        ///// <returns></returns>
        //public override Settings.PropertySpec[] GetProperties(bool specOnly)
        //{
        //    Settings.PropertySpec[] tmp = base.GetProperties(specOnly);
        //    Settings.PropertySpec[] properties = new Settings.PropertySpec[1 + tmp.Length];
        //    tmp.CopyTo(properties, 1);

        //    properties[0] = new Settings.PropertySpec("eventname", mEventName.GetType());

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

            string code;
            if (descriptor.IsArchivedResource)
            {
                string zipFilePath = Keystone.Core.FullNodePath(descriptor.ModName);
                code = KeyCommon.IO.ArchiveIOHelper.GetTextFromArchive(descriptor.EntryName, "", zipFilePath);
            }
            else
            {
                string filePath = System.IO.Path.Combine(Keystone.Core._Core.ModsPath, descriptor.EntryName);
                code = System.IO.File.ReadAllText(filePath);
            }

            if (string.IsNullOrEmpty(code)) goto error;

            CSScriptLibrary.CSScript.LoadedScript loadedScript = Core._Core.ScriptLoader.LoadCode(code, _id, true);

            if (loadedScript == null) goto error;

            Dictionary<string, object[]> supportedMethodSignatures = new Dictionary<string, object[]>();

            // note: supported methods for behavior scripts always match the Behavior Tree Node interface model.
            // TODO: But these dont!  They don't return BehaviorResult and they don't take as argument a BehaviorContext (although it shouldn't get a BehaviorContext.  Just entityID because we can grab entity.CustomData from EntityAPI.GetAIBlackboardData(entityID).
            supportedMethodSignatures.Add("Validate", new object[] { "", 0d });
            supportedMethodSignatures.Add("Exit", new object[] { "", 0d });
            supportedMethodSignatures.Add("Enter", new object[] { "", 0d });
            // NOTE: the signature doesn't need a KeyCommon.Traversal.BehaviorResult return value, but it does work.
            supportedMethodSignatures.Add("Execute", new object[] { "", 0d });
            mMethods = Core._Core.ScriptLoader.GetSupportedMethods(loadedScript, supportedMethodSignatures);

            if (mMethods != null)
            {
                mScript = loadedScript;
                // TODO: the below i think is important for 
                //        throw new Exception("Parent behavior group node I think should override PropogateChangeFlags so it can respond to ScriptLoaded change event, including scriptunloaded");
                SetChangeFlags(Keystone.Enums.ChangeStates.BehaviorScriptLoaded, Keystone.Enums.ChangeSource.Self);
                return;
            }

        error:
            _resourceStatus = PageableNodeStatus.Error;
            System.Diagnostics.Debug.WriteLine("Keystone.Behavior.Actions.Script.LoadTVResource() - Error creating script");
        }

        public void SaveTVResource(string filepath)
        {
            throw new NotImplementedException();
        }
        #endregion

        // TODO: why isn't Perform and Execute the same thing?
        public override BehaviorResult Perform(Entities.Entity entity, double elapsedSeconds)
        {
            if (entity == null)
                return BehaviorResult.Error_Script_Invalid_Arguments;

            if (Validate(entity, elapsedSeconds) == BehaviorResult.Fail)
                return BehaviorResult.Fail;

            if (mScript == null) throw new Exception("Script.Perform() - Script is null.");

            return Execute (entity, elapsedSeconds);
        }


        protected override BehaviorResult Validate(Entities.Entity entity, double elapsedSeconds)
        {
            try
            {
                return Execute("Validate", new object[] { entity.ID, elapsedSeconds });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Script.Validate() - " + ex.Message);
                return BehaviorResult.Error_Script_Exception;
            }
        }

        protected override BehaviorResult Enter(Entities.Entity entity, double elapsedSeconds)
        {
            return Execute("Enter", new object[] { entity.ID, elapsedSeconds });
        }

        protected override BehaviorResult Exit(Entities.Entity entity, double elapsedSeconds)
        {
            return Execute("Exit", new object[] { entity.ID, elapsedSeconds });
        }

        protected override BehaviorResult Execute(Entities.Entity entity, double elapsedSeconds)
        {
            return Execute("Execute", new object[] { entity.ID, elapsedSeconds });
        }

        // since this now becomes a type of Behavior node
        // valid event names are Validate/Execute/Enter/Exist

        // Validate() <-- are preconditions met such that this Action script can be invoked?
        // Enter() <-- First time the script is Entered ONLY.  If it was previously "Running"
        //             then the Enter is not called again.
        // Exit()  <-- Occurs when the Behavior was accessed previous Tick, but not the following.
        // So I think the above answers our question regarding scripts returning
        // BehaviorResults.  They will.  
        private BehaviorResult Execute(string eventName)
        {
            if (!Enable) return BehaviorResult.Fail;
            if (!TVResourceIsLoaded) return BehaviorResult.Fail;
            if (mMethods == null) return BehaviorResult.Fail;

            CSScriptLibrary.MethodDelegate result;
            if (!mMethods.TryGetValue(eventName, out result))
                return BehaviorResult.Success;
            
            try 
            {
                // TODO: lets imagine it's a MoveTo() script
                // as long as the bool Exit() condition evaluates to false
                // we return Running, else Success?  Is that proper?  Hrm
                // Is there ever a situation where the script would need to return
                // differnet conditions depending on the context in which it was used?
                // That would suggest the script itself should never return BehaviorResult
                // but such scenario seems unlikely or so extremely rare as to be inconsequential
                // The single main issue for the return type is it seems to add yet another thing
                // that the script writer could screw up... or is it really a problem?
                // WHAT ARE THE STEPS TO ANY ACTION
                // - Should we run it?  <-- validate
                // - Run it        
                //      - Did we fail?  <-- how is this determined if not as at least true/false return result
                // 
                //      - else not Fail, 
                //          - did we finish?
                //          - else still Running  <-- if havent failed, check Exit condition to determine if completed?

                // Performance wise easiest to just return the Result in the script
                // rather than make second call to a "bool CompletionSuccessful()" function
                // Consider Steer()... that really is a function, not a script
                // we Steer() towards something, but the script would contain the rules 
                // for higher actions like "Follow" or "Evade"
                // Let's say our Follow() action script typically returns "Running" unless
                // the target it's following is null (fails precondition) (perhaps the target was destroyed)
                // or exceeds some max distance and thus is beyond detection range 
                // So if your ship for instance had a standing order to it's helsman to Intercept()
                // some target, and maybe it transitions to "Persuit_At_Distance" when it becomes close enough
                // and when it doesn't want to overshoot,
                // 
                // I think for "Steer" example, we'd compute the velocity and apply it to the NPC and then
                // in Simulation.Update() entity.Update(elapsedSeconds) we would update the position each frame since AI isn't necessarily updating each frame.
                return (BehaviorResult)mMethods[eventName]();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Script.Execute() - " + ex.Message);
                return BehaviorResult.Error_Script_Exception;
            }
        }

        private BehaviorResult Execute(string eventName, object[] args)
        {
            if (args == null || args.Length == 0)
            {
                // call overloaded version that accepts no args
                // or maybe throw exception if null args passed is better?
                // or maybe just returning error is best?
               //return Execute(eventName);
               //throw new ArgumentNullException();
                return BehaviorResult.Error_Script_Invalid_Arguments;
            }

            if (!Enable) return BehaviorResult.Fail;
            if (!TVResourceIsLoaded) return BehaviorResult.Fail;
            if (mMethods == null) return BehaviorResult.Fail;

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
                return BehaviorResult.Success;

            try
            {
                result(args);
                return BehaviorResult.Success;
                //return (BehaviorResult)result(args);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Script.Execute() - " + ex.Message);
                return BehaviorResult.Error_Script_Exception;
            }
        }

        #region Resource Members
        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();

            if (mMethods != null)
                mMethods.Clear();

            mMethods = null;
            mScript = null;

            //if (mScript != null)
            //{
            //   mScript.as
            //}
        }
        #endregion
    }
}
