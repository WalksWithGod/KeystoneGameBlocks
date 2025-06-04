using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using KeyPlugins;
using Keystone.Types;

namespace KeyPlugins
{
    public partial class OnboardComponents : BasePluginCtl
    {
        public OnboardComponents()
        {
            InitializeComponent();
        }

        public OnboardComponents(IPluginHost host, string modpaths, string modname )
            : base(host, modpaths, modname)
        {
            mName = "Engineering Plugin";
            mSupportedTypenames = new string[] {"Vehicle"};
            mDescription = "Engineering station interface.";
        }

        // how would we get this plugin to appear instead of the default
        // plugin for entities?

        // is there a way that we can select between a list of potential plugins
        // for a given type and then choose the best one?  rather than
        // always just pick the one of the exact type?



        // let's say that the "simulation" profile application state is what allows for
        // OnBoardComponents plugin to appear when a Vehicle is selected.
        // - simulation mode must attach to an existing vehicle from Edit mode
        // - or simlation mode must spawn a vehicle into the specified world
        //   and then attach to it.   
        // - Is the "Vehicle" the primary part however?  What about the "avatar" and then
        //   allowing the user to "create vehicle" with only an "avatar" shell to start out with.
        // 


        // The player is much more limited on what they can select...
        // They can select a star, world, moon, and return to their normal vehicle plugin
        // afterwards.  SmartPlugins will show the user information they need on current
        // selected targets, but always allow quick navigation back to their own ship
        // which is the default target.  
        // We must also remember the state of the vehicle plugin when switching.. so in other words
        // a way to store the state and restore it.
        // 
        // In our MVC design, the plugin is view and interface to the server which hosts the controllers
        // or in loopback, the localhost does.

        //
        // SIMULATION MODE
        // - when switching to simulation mode from EDIT, a default ship will be loaded
        //   from ProceduralHelper
        // - upon existing Simulation Mode and returning to EDIT, the ship is unloaded
        // - where is the simulation mode user ship spawned and at what velocity, direction, orbit?
        // - should all vehicles require client to "register" with it prior to be given access
        // to all of it's systems in the plugin?  
        //      - user must have ship's access codes
        //
        // ViewportDocument vs Plugin
        

        public override void SelectTarget(string sceneName, string id, string typeName)
        {
            System.Diagnostics.Debug.WriteLine("Engineering.SelectTarget() - Begin.");
            this.SuspendLayout();
            try
            {
                base.SelectTarget(sceneName, id, typeName);              

                // TODO: For now we only implement memory over the current node
                // If we want to implement persistance as the user navigates different entities
                // then we would store their panelstate instances collections by Entity ID.
                //
                //
                // Stack <Dictionary<string, PanelState>> mStateStack;  // maintains stack across navigating child entities
                //
                // Dictionary<string, PanelState> mPanelStates; // stores each panel with a key
                //                                              // such as Model, Appearance, Behavior

                // 
                // class PanelState
                // {
                //      SuperTabItem mTabItem;
                //      DevComponents.AdvTree.AdvTree mTreeView;
                //      DevComponents.AdvTree.Node mSelected; 
                //      Dictionary <string, object> mUserStates; // a collection of other state info that the user can manually decide what is tracked under what key name
                //      public delegate void RefreshDelegate();
                //      private RefreshDelegate mRefreshHandler;
                //      public PanelState (RefreshDelegate refreshHandler)
                //      {
                //          mRefreshHandler = refreshHandler;
                //      }
                //
                //      public void Refresh()
                //      {
                //          // 
                //          if (mRefreshHandler != null) mRefreshHandler.Invoke();
                //      }
                // }


                //// get all components in vehicle that are flagged as engineering components
                //// eg. engines, reactors, batteries, thrusters
                //string[] resultIDs;
                //int[] resultTypes;
                //const int EMITTER_THRUST = 2;
                //const int EMITTER_POWER = 3;
                //int emitterTypes = EMITTER_THRUST | EMITTER_POWER;

                //// find emitters within this container (this will ignore fighter vehicles inside a carrier vehicle)
                //mHost.Container_FindEmitters (vehicleID, emitterTypes, out resultIDs, out resultTypes);

                //if (resultIDs != null && resultIDs.Length > 0)
                //{
                //    System.Diagnostics.Trace.WriteLine("");
                //    for (int i = 0; i < resultIDs.Length; i++)
                //    {
                //        int emitterType = resultTypes[i];
                //        string nodeID = resultIDs[i];

                //        switch (emitterType)
                //        {
                //            // add it to one of the engineering grids for this type
                //            case EMITTER_POWER :
                //                break;
                //            case EMITTER_THRUST :
                //                break;
                //            default:
                //                System.Diagnostics.Debug.WriteLine ("Engineering.SelectTarget() -Unsupported type...");
                //                break;
                //        }
                //    }
                //}
                
                //// intiial state.  Later they will be triggered on Notify_NodeRemoved
                //ClearTabPanel(superTabControlPanel2);
                

                //// contains just the sub panel since the "General" is always there
                //// although potentially with some hidden options
                //mTabPanelNodes = GetTabNodes(mTargetNodeID);

                //if (mTabPanelNodes != null && mTabPanelNodes.Length > 0)
                //    for (int i = 0; i < mTabPanelNodes.Length; i++)
                //        RebuildPanel(mTabPanelNodes[i]);

                System.Diagnostics.Debug.WriteLine("Engineering.SelectTarget() - Complete.");
            }
            catch (Exception ex)
            {
                this.ResumeLayout();
            }
            System.Diagnostics.Debug.WriteLine("Engineering.SelectTarget() - Layout Resumed.");
        }

    }
}
