//using System;
//using Amib.Threading;
//using Keystone.IO;
//using Keystone.Lights;
//using Keystone.Portals;
//using Keystone.Resource;
//using Keystone.Types;
//using Keystone.Appearance;

//namespace Keystone.Commands
//{
//    public class AddSkyBoxTV : AsychronousCommand 
//    {

//        private string[] mTexturePaths;
//        private float mRadius;

//        public AddSkyBoxTV(string[] texturePaths, float radius, PostExecuteWorkItemCallback completionCB): base(completionCB)
//        {
//            if (texturePaths == null || texturePaths.Length != 6) throw new ArgumentOutOfRangeException();
//            // store the paths to 6 textures 
//            mTexturePaths = texturePaths;
//            mRadius = radius;
//        }
               

//        #region ICommand Members
//        public override object Execute()
//        {
//            //_state = CommandState.ExecuteCompleted;

//            //// these three lines i suspect should be handled automatically in the command.Execute() method
//            //// and if it's an asychronous command, it should queue the work item
//            //WorkItemInfo item = new WorkItemInfo();
//            //item.PostExecuteWorkItemCallback = new PostExecuteWorkItemCallback(AsychronousJobCompletedHandler);
//            //CommandProcessor.ThreadedWorkQueue.QueueWorkItem(item, new WorkItemCallback(Worker), this);

//            return null;
//        }

//        // TODO:
//        public override void UnExecute()
//        {
//            if (_state == CommandState.ExecuteCompleted)
//            {
//                //_parent.RemoveChild(_entity);

//                _state = CommandState.ExecuteCompleted;
//            }
//            //    // call to remove should handle derefercing the node and such
//            //    Core._CoreClient.SceneManager.CurrentScene.Remove(Model);

//            //    Core._CoreClient.Simulation.RemoveEntity(Entity);
//        }
//        #endregion

//        private void AsychronousJobCompletedHandler(IWorkItemResult result)
//        {
            
//            //FileManager.WriteNewNode(_parent.Parent, true); // TODO: writes shouldnt be done automaticall when the item is added to the scene correct?

//            ////FileManager.WriteNewNode(_parent); // maybe the key is the parent of the parent isnt rewritten
//            //// TODO: however should a seperate Entity save directly to the library be done?
//            //if (_writeToLibrary)
//            //{

//            //}

//            if (_completionCB != null) _completionCB.Invoke(result);
//            _state = CommandState.ExecuteCompleted;
//        }

//        protected object Worker(object obj)
//        {
          
//            try
//            {
//                // load the 6 textures
//                Texture[] textures = new Texture[6];

//                for (int i = 0; i < textures.Length; i++)
//                {
//                    textures[i] = Texture.Create(mTexturePaths[i]);
//                }
//                FX.FXSkyBoxTV provider = new Keystone.FX.FXSkyBoxTV(textures, mRadius );
//                Core._Core.SceneManager.Add(provider);
//            }
//            // TODO: we have to unroll anything we've done here if the command fails we want it to fail completely, not partially
//            catch (Exception ex)
//            {
//                // TODO: need to trap the exception, unroll any changes and then throw a new exception for the IWorkItemResult.Exception()
//                System.Diagnostics.Trace.WriteLine("AddSkyBoxTV.Worker() -- Error creating TV Skybox FX'{0}'.");
//            }

//            return null;
//        }
//    }
//}
