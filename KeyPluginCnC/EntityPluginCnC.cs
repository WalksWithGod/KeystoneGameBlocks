using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using KeyPlugins;
using Keystone.Types;

namespace KeyPluginCnC
{
    public partial class EntityPluginCnC :  BasePluginCtl
    {
        public EntityPluginCnC() : base()
        {
            this.InitializeComponent();
        }

        public EntityPluginCnC(IPluginHost host, string modspath, string modname)
            : base(host, modspath, modname)
        {
            InitializeComponent();


            mName = "Command & Control";
            mProfile = "C&C";
            mSupportedTypenames = new string[] { "Vehicle"};
            mDescription = "A plugin for issuing Helm and Tactical Commands.";
        }

        private void EntityPluginCnC_Load(object sender, EventArgs e)
        {

        }

        // issues a command to Helm to proceed to next waypoint
        private void buttonX1_Click(object sender, EventArgs e)
        {
            if (mHost != null)
            {
                System.Diagnostics.Debug.WriteLine("EntityPluginC&C.buttonX1_Click()");
                System.Diagnostics.Debug.Assert(string.IsNullOrEmpty(mTargetNodeID) == false && mTargetTypename == "Vehicle");

                // we do need access to the VehicleID for this Plugin's target
                // when tactical loads, the vehicle is not necessarily loaded yet
                // but wait, we do bind to the vehicle in the workspace so that could
                // be a place where we inform the plugin 

                // do we need to gain access to the Helm Entity ID first?
                // what if the Helm station Entity is destroyed?  Helm controls
                // would need to go to another station and at a proficiency cost for
                // multi-tasking (unless there are no other stations left)

                // the purpose here is to form a command and send it
                // through to the server where it's validated, and returned to
                // client for execution. Perhaps an OrderRequest which
                // server sends back as just Order object.
                //
                // How does an NPC create an order request?  It must go through API right?
                // Or does the NPC AI run only on server with client only receiving results (eg dumb client)?
                Game01.Messages.OrderRequest request = new Game01.Messages.OrderRequest();
                request.AssignedDateTime = DateTime.Now;
                request.AssignedByID = mTargetNodeID;
                request.AssignedStationID = "Helm"; // todo: helm is an Entity?  Wouldn't this make serializing a vehicle very expensive?
                request.Task = (int)Game01.GameObjects.TaskType.HELM_ENABLE_MAIN_ENGINES; // TODO: HELM_TOGGLE_MAIN_ENGINES with Args= "true" or "fasle"
                request.Args = null;
                request.Priority = 1;
                request.Notes1 = null;

                mHost.Task_Create (request);
            }
        }

        private void sliderHeading_ValueChanged(object sender, EventArgs e)
        {

        }

        private void setCourseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // select Stellar System, Star, World as well as orbital altitude and speed (different speed will consume varying amounts of fuel)
            // Higher speeds for extended periods of time will also put more stress on engines requiring more frequent maintenance and increasing odds of a critical failure

            // todo: do these checks constitute rules? we should have the plugin call game00.dll???


            // SYSTEMS CHECK
            // -------------

            // reactors must have enough power

            // ship must have enough fuel given the destination and selected speed

            // ship must have enough working engines

            // ship must have enough structural integrity for the selected speed


            // CREW CHECK
            // -------------

            //  Interior must have a working "helm" and an "operator" with enough health. If no operator, first officer should assign an operator even if it's the first officer 
            //  Engineering crews must be at stations



            // ENGAGING
            // -------------

            // course plotted (mostly to avoid running into stars or worlds)
            // maneuvering thrusters power on 
            // ship orients toward destination
            // main engines power on
            // ship travels to destination
            // main engines power off

            string destinationName = "Moon";
            string root = mHost.Scene_GetRoot();
            string targetID = mHost.Node_GetDescendantByName(root, destinationName);
            mHost.Vehicle_TravelTo(targetID);
        }

        private void attackTargetToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void scanToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
