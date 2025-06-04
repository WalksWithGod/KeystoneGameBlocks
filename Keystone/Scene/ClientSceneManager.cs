using System;
using System.Collections.Generic;
using System.Diagnostics;
using Keystone.Cameras;
using Keystone.Controllers;
using Keystone.Entities;
using Keystone.FX;
using Keystone.IO;
using Keystone.Traversers;

namespace Keystone.Scene
{
    // NOTE: Hrm.  I had this inheriting FXBase because when SceneManager elements move, I wanted the SceneManager to be notifiied in case it needs to
    // move that object within the scene to a different sector or octree node.  Or is there another mechanism we want to use for that?
    // Hrm... but if i do do something whereby Update() will handle this sort of thing when its sending those translation commands to the
    // the scene nodes, then it brings into question the Notify() model in general for FXBase... i mean, the Update() here can
    // check for subscriptions and notify the FX directly?  Meh. I dunno.  Need to contemplate this.  
    // On the one hand, it's only Shadows that really utilized subscribers the most... and Water generally doesnt move at runtime except in edit mode.
    // 

    /// <summary>
    /// Only one SceneManager can exist which contains all graphs which are rendered together as a single logical "scene"
    /// Because of the nature of how RenderBeforeClear, Render, PostRender work, only one logical scene can exist.
    /// There would be no easy way to do two seperate renders where a second SceneManager also needs to RenderBeforeClear 
    ///  That's why I think the best approach is to simply Add the second "scene" as just another graph that exists in 
    /// a seperate "layer" and to include logic to handle the additional transitioning to/rendering of layers of graphs in addition to a list of graphs
    /// </summary>
    public class ClientSceneManager : SceneManagerBase 
    {
        private CoreClient mClientCore; 

        private IFXProvider[] _fxProviders = new IFXProvider[System.Enum.GetNames(typeof(FX_SEMANTICS)).Length];
        //private Traversers.Picker _picker;


        public ClientSceneManager(CoreClient core)
            : base()
        {
            mCore = core;
            mClientCore = (CoreClient)core;

            mCore.SceneManager = this;
            //_drawer = new ScaleDrawer();
            //_picker = new Traversers.Picker();

            LoadFXProviders();

            // TODO: So, in order to handle some GUI element's updates for like progress meters
            // we'll need constants for some SCENE_EVENT_LOAD_PROGRESS or something that can be
            // be used to "register" GUI element's appearance to those events...
            // Consider my keybinder, there I use a script where the "constant" function to bind too
            // is referred to via an alias that is "hooked" at runtime.  So I think a similar
            // function can exist for GUI where we'll have some hard coded routines, but the GUI elements
            // can "bind" to those routines when they are loaded.
            // There are other types of handlers we can specify too that GUI elements can bind too.
            // NetworkIn/Out events so we can update any labels.
            // Paging/Load events so we can update any debug progress meters and such.

            // So one question is, should the GUI map directly to those handlers in say Network.Events or Reader.Events, Loader.Events?
            //      
        }

        // TODO: events here that can be wired too?

        // this is tricky.  Some FXProviders like Bloom we want to share with each
        // RenderContext, but others... hrm...
        public IFXProvider[] FXProviders { get { return _fxProviders; } }

        private void LoadFXProviders()
        {
            //FX.FXImposters imposters = new FXImposters(Keystone.RenderSurfaces.RSResolution.R_2048x2048, 16, 16, 1);
            //Add(imposters);
            Keystone.EntitySystems.HeavyParticleSystemManager heavyParticles = new Keystone.EntitySystems.HeavyParticleSystemManager();
            Add (heavyParticles);
            
            if (CoreClient._CoreClient.Settings.settingReadBool("graphics", "bloom"))
            {
                FX.FXBloom bloom = new FXBloom();
                Add(bloom);
            }
            
            // TODO: this register/unregister should be done in the object's constructor and destructor since it's only required by
            // certain types that make direct dx calls and need to handle the reset event
            //Core._CoreClient.RegisterForDeviceResetNotification(_imposterSystem);

            // i was thinking that as a procedural matter, what we should (?) do is while recursiving during culling
            // we cull as normal and then we should check the entity.Model.UseInstancing = true and if so
            // we dont render it.  I think this model to be used with Instancing must already be associated with
            // the FXInstancer. In other words, if we load a scene and this FX isnt enabled and thus not loaded
            // this entity.Model.UseInstancing will evaluate to False.  If the FX is loaded, then it implies
            // that entity.Model.FXProvider[FX_SEMANTICS.INSTANCER] != null)
            // so here yes, the Model directly tells us its associated with a minimesh.  In fact, when a Mesh3d or filepath is
            // pass to FXInstancer.Load() or .Add() then a Model should be created to wrap it and Model.Minimesh should be set
            // to the Minimesh that's created from the Mesh3d as well.

            // so the reference to the mini should be set immediately and so yes
            // then during cull, we can directly do  entity.Model.Minimesh.AddInstanceData(cameraRelativePosition, entity.Scale, entity.Rotation)

            // so then we can also do Minimesh.Model to get the exact model used so in our Toolbox or Generation() code
            // we can always use the proper one.

            // The thing to watchis when we want to add some existing meshes to use mini, then we must get the model reference
            // and pass that to the FXInstancer
        }
        
        public void Add(IFXProvider p)
        {
            int semanticID = (int)p.Semantic;
            if (_fxProviders[semanticID] != null)
            {
                Trace.WriteLine(
                    "Scene.Add (IFXProvider) - Provider at this semantic slot already occupited.  It must first be removed.");
                return;
            }
            if (p.Semantic == FX_SEMANTICS.FX_NONE)
                throw new ArgumentException("Scene.Add (IFXProvider) - Provider Semantic not set.  Cannot add Provider.");
            // TODO: not sure how to swap though because we do want to register the proper scene elements with 
            // the new semantic.  We might need a traversal and the individual elements might need a sort of bitflag
            // indicating which semantics they should be registered too.
            // also some elements like lights i think might not need these... though actually maybe they do because
            // consider a pointlight may not necessarily be used as a shadowmap volume.  So we would need the semantics bitflag.
            _fxProviders[(int)p.Semantic] = p;
        }


        public override void Update(Keystone.Simulation.GameTime gameTime)
        {
            // NOTE: Our IOControllers (aka InputControllers such as EditController.cs and
            // SystemController.cs) update via input events during game loops CheckInput() where
            // input since previous frame is updated.  
            // There is no need for those controllers to have their own Update() method!

            
            double fixedTimeStepRatio = 0;

            // seperate scenes are used when showing Preview windows to give one example
            // thus each scene has it's own simulations and EntitySystem instances.
            foreach (Scene scene in mScenes.Values)
            {
                if (scene == null || scene.Simulation == null)
                    continue;



                // TODO: should elapsedSeconds passed to scene.Simulation.Update() be
                //       in GameTime by now instead of actual elapsed?  We need for animations, game logic (including scripts)
                //       to use gameTime elapsed not real loop time elapsed right?

                // update audio listener position 
                // if (scene.Simulation.CurrentTarget != null)
                //     mClientCore.AudioManager.Listener.Position = _activeSimulation.CurrentTarget.Translation;

                // update simulation (TODO: perhaps update all "EntitySystems" here 
                // NOTE: Simulation.Update calls
                //  activeEntities[i].Update(elapsedSeconds); 
                //     Entity.Update(elapsed) calls
                //      - mAnimationController.Update (doesn't apply to BonedAnimations. BoneAnimations must be suspended in Actor3d.Render(_actor.Render(){simulation.Running}))
                //      - mBehaviorRoot.Perform(mBehaviorContext, elapsedSeconds);
                //      - mDomainObject.Execute("OnUpdate", new object[] { this.ID, elapsedSeconds });
                if (scene.Simulation.Running)
                {
                    fixedTimeStepRatio = scene.Simulation.Update(gameTime);
                }
                // simulation may be paused but the graphics may still be rendering so we continue to call scene.Update()
                scene.Update(gameTime.ElapsedSeconds); // todo: should i be caching this for the below GraphicsLoop? maybe update runs a frame behind and we pass accumulated gameTime.ElapsedSeconds from all RenderingContext GraphicsLoop
            }

            // TODO: seems to me this minimesh update needs to occur
            // in the foreach loop below for each context...
            CoreClient._CoreClient.InstanceRenderer.Update();

            // TODO: also every viewport should be locked prior to entering this loop
            // no viewports can be unloaded mid-render
            foreach (Viewport vp in CoreClient._CoreClient.Viewports.Values)
            {
                if (vp.Enabled && vp.Context.Workspace.IsActive) // if (vp.Enabled && vp.Visible)
                {
                	// TODO: we should vp.Lock() so that if user tries to close viewport while already
                	//       in this loop, it will be forced to wait and so renderingcontext's and cameras and
                	//       such wont get unloaded underneath our feat
                    RenderingContext context = vp.Context;
                    if (context == null) continue;
                                        
                    context.GraphicsLoop(gameTime.ElapsedSeconds); // TODO: if 2 viewports are visible in the window docking system, then the ElapsedSeconds winds up being halfway wrong as BonedEntities visibly move slower than they are supposed to.
                }
             }
                        

            // TODO: having elapsed for anything except the simulation seems wrong because
            //       if we dont render (minimized) but are still running the simulation, then
            //        the elapsed since it's shared with _simulation's will be too small.  we'd need to track seperate elapses right?
            //        but really, i shouldnt have to track elapsed.  Anything that should need elapsed should be updated via _simulation right?
            //        Well from the looks of things, elapsed is only needed for the Editor camera and for some FX.Update() (e.g. updating waves )
            //        and can quite possibly just use a seperate elapsed counter that always resets when suspended so there's never any "jump"
        }


        #region IDisposable Members
        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
         
            Array.Clear(_fxProviders, 0, _fxProviders.Length);
            // TODO: Should fx be apart of scene then or apart of our core engine?

            // now destroy the rest of the scene manager objets
            //_picker = null;
        }
        #endregion
    }
}