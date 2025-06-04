using System;
using System.Collections.Generic;
using DevComponents.DotNetBar;

namespace KeyEdit.Workspaces
{
    public partial class CrewManagementWorkspace: WorkspaceBase
    {

        private bool mQuickLookHostPanelDocked = false;
        private System.Windows.Forms.Control mQuickLookHostPanel;

        public CrewManagementWorkspace(string name)
            : base(name)
        {
            mQuickLookHostPanel = new KeyEdit.GUI.QuickLookHostPanel();

            // for implementing the schedule itself and getting AI crew to switch tasks.
            // essentially, an AI crew can always be involved in a task with some tasks
            // having a priority that allows them to avoid advancing to the next one even
            // if they are overdue.
            //
            // http://www.codeproject.com/Articles/18754/Flexible-Time-Schedule-in-C-2-0

            // Representation of Service being for
            //  - smart people, not dumb poor grunts with no choices in life
            //  - honorable
            //  - showing a version of service (at least for military crews as opposed to pirates and freelancers)
            //    that is noble and righteous.. like wwI and II british soldiers 
            //  - showng that to hire grunts, children, the poor is no volunteer army at all
            //    that it's exploitation and shows that the culture of the society that has such
            //    an army is crumbling
            // Duty Schedule
            //      - Attendance Record
            //      - Training schedule 
            //      - Mandatory Fitness schedules
            //  - group sports (if rec fascility on board)
            //  - treadmills/weights
            // Skill & Proficiencies
            //      - connected to Bio
            // Communication
            //      - direct link to the filtered communications to / from this crew member
            //      - time off requets (deny/grant)
            //      - recall from leave 
            // Subordinate Evals (only available for officers)
            //      - these are the evals this officer has made about a subordinate 
            // Jacket
            //  - Commendations (Award, Revoke)
            //  - Reprimands - go in report/service record
            //      - gambling
            //      - late for duty x times in x period
            //      - poor performance, inattentiveness, sloppiness
            //      - dress code / hygeine 
            //      - cowardice
            //      - disobeying
            //      - assaulting another crew
            //      - assaulting superior
            // Medical History
            //      - report to medical bay
            //      Pysch Eval
            // Biography
            // Security Clearance
            // Notes
            //  Crew Members holding grudges against others can result in fights and such or murder
            //
            // Security Access
            // - mess / dining fascilities
            // - recreational / excercise fascilities
            // - personal quarters
            // - 
            // - cargo bays
            // - weapon lockers
            // - private communications channel for chatting with family
            //    - captain can configure how\when the crew can schedule these
            //      - captain can give senior officers more access whereas enlisted have to go
            //        round robin and no more than x minutes of calls per month.
            //        - increased comms can help boost morale, but can be a security risk
            //          both in terms of spies communicating outside and in terms of detection
            //          even though comms will use beam lasered transmission to a relay, it is
            //          still possible to intercept this beam if you know where it's coming from
            //          approximately.  
            //          - intercepting multiple beams can then give you a trajectory and velocity
        
            // Sept.20.2016 - Efficiency
            //	- fully staffed ship can run at anywhere from 100% to 1% efficiency
            //	depending on the morale of the crew, their experience, their loyalty and dedication
            //  - partially staffed crew may run at high efficiency until their fatigue and morale drops
            //    due to insufficient shifts (working too many hours).  Captain can give ship wide communications
            //    to try and boost their morale but the effects are temporary.
            
        }

        public override void Configure(Keystone.Workspaces.IWorkspaceManager manager, Keystone.Scene.Scene scene)
        {
            base.Configure(manager, scene);

            if (mDocument == null)
            {
                mDocument = new Controls.CrewManagementControl();
                mDocument.Name = Name;
                // CreateWorkspaceDocumentTab also tells the dockbar to make this tab the active tab
                //string name = string.Format("Code Editor[{0}]", count);

                mWorkspaceManager.CreateWorkspaceDocumentTab(mDocument, GotFocus);

            }

        }

        public override void UnConfigure()
        {
            if (mWorkspaceManager == null) throw new Exception("WorkspaceBase.Unconfigure() - No ViewManager set.");
        }

        private void GotFocus(Object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("EditorWorkspace.GotFocus() - ");
        }

        #region IWorkspace Members
        public override void Show()
        {
            base.Show();

            if (mQuickLookHostPanelDocked == false)
            {
                DevComponents.DotNetBar.DockContainerItem container = 
                    ((WorkspaceManager)mWorkspaceManager).DockControl(mQuickLookHostPanel, "Crew Management", "leftDockSiteBar", "Crew Management", eDockSide.Left);
                //container.Text = "";
                mQuickLookHostPanelDocked = true;
            }
        }

        public override void Hide()
        {
            base.Hide();

            if (mQuickLookHostPanelDocked == true)
            {
                ((WorkspaceManager)mWorkspaceManager).UnDockControl("Crew Management", "leftDockSiteBar");
                //container.Text = "";
                mQuickLookHostPanelDocked = false;
            }
        }
        #endregion
    }
}
