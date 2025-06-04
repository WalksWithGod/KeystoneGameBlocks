using System;
using System.Collections.Generic;
using System.IO;
using Keystone.Controllers;
using Keystone.Entities;
using Keystone.Resource;
using Keystone.Scene;
using Keystone.Elements;
using System.Windows.Forms;

namespace KeyEdit
{
    partial class FormMain : FormMainBase
    {

        // KeyScript.Delegates.EntityDelegates.SpawnEventHandler mOnSpawn;
        //http://www.codeplex.com/wikipage?ProjectName=Dynamic  // dynamic reflection & dynamic code generation


        
        private void ribbonTabItemNavigation_Click(object sender, EventArgs e)
        {
            // TODO: If this document tab is already active, switch to it
            // no need to create it

            //TODO: this is temp procedure to test bringing up the animation workspace
            // for now all we really want is to bring up new viewport document with three views
            // and sharing same scene
            this.EditHinge((Model)null, "");

        }
                
        // TODO: this can also be a workspace method for tweaking ship interiors, or setting 
        // up new models as saves will automatically update the underlying archive entry
        private void EditHinge(Model model, string hingAnimation)
        {

            if (AppMain.PluginService.CurrentPlugin == null || AppMain.PluginService.CurrentPlugin.Instance == null || 
                string.IsNullOrEmpty(AppMain.PluginService.CurrentPlugin.Instance.TargetID))
            {
                MessageBox.Show("No target selected.");
                return;
            }

            string targetID = AppMain.PluginService.CurrentPlugin.Instance.TargetID;

            // create a simple temp scene that is based off of a live model or Entity in the Repository
            // as opposed to from disk.  Actual saves can be done
            // in the other workspace since changes in the realtime model or Entity
            // will be instant
            Workspaces.WorkspaceBase3D ws = (Workspaces.WorkspaceBase3D)mWorkspaceManager.GetWorkspace(PRIMARY_WORKSPACE_NAME);
            Scene temp = ws.Scene;


            Workspaces.AnimationEditWorkspace aws = new KeyEdit.Workspaces.AnimationEditWorkspace("Animation Editor");
            mWorkspaceManager.Add(aws, temp);
            mWorkspaceManager.ChangeWorkspace("Animation Editor", false);

            // TODO: set the projection and view orientation for each viewport 


            // TODO: have each viewport zoom extents

            
            // TODO: how do we activate the widget?
            ws.SelectTool(Keystone.EditTools.ToolType.HingeEdit);


            //  - widget is restricted to plains in any orthographic view
            // -  maybe we no longer ever attach the widget to the scene, but rather
            //    we use the parent target to compute the position of the child as normal.
            //    If we have to, we can even add a Proxy parent, then widget renders 
            //    independantly of any... BUT.. BUT... BUT... we lose picking ability without
            //    hacks... but Pick() is called through the context, we could maybe insert that
            //    test in arguments passed to picker.cs

            // One question is if widget is a control and user can always create custom controls
            // in gameplay that can trigger functions... then why not with widgets? I think
            // in theory sure, but for now we will hardcode.  but there should be no reason
            // any widget can't be wired just like animations are wired to transformables.
            // PERHAPS MOVE WIDGET to HUD or KeyEdit\\GUI folder?
            // (see Keystone\EditTools\Widget.cs)

            // the EXE writer can access workspace manager and such
            // but plugin writers cannot.  So one question is, can the plugin writer
            // delcare a generic workspace and then tailor it?  Maybe... within limits

            // TODO: AssignTarget could be a function of our API.... 
            // GUIAPI.Workspace_Activate("Animation Editor");
            // GUIAPI.Viewpoint_Translate
            // GUIAPI.Viewpoint_LookAt
            // GUIAPI.Viewpoint_TranslateAnimated (speed);
            // GUIAPI.Viewpoint_MoveTo (target) // overloads for vectors with orientations
            // GUIAPI.Viewpoint_MoveToAnimated (speed, target, orientation..)
            // --
            // GUIAPI.Viewport_AcceptInput(true/false); // can be used to prevent non focussed viewports from responding to the InputController (eg. EditorController)
            // GUIAPI.Viewport_SetProjection ()
            // GUIAPI.Viewport_SetViewType();
            // GUIAPI.Viewport_ZoomEents (viewportIndex);
            // GUIAPI.Viewport_SetTarget (targetID, viewportIndex); // MFD might have seperate target on some workspace
            //  - flags for target lock_lookat | exclusive_render | not_manipulatable_by_widgets
            // 
            // TODO: viewport index for the specific workspace name 
            //       or viewport friendly name for the specific workspace name
            //
            // GUIAPI.Viewport_SetRenderMode (scene | targets)

            // TODO: This is not right.  We shouldn't lock a context this way, if we wanted that
            // we should instead use a sort of tracking camera controller or viewpoint controller.
// TODO: FIX            ws.ViewportControls[0].Viewport.Context.AssignTarget(targetID);
            // TODO: after assigning, move to and fill extents

            // TODO: Perhaps this workspace can define a lot of the editor related functions we want
            // to implement...
//            AppMain._core.Console.Register("tool_activate", ((Keystone.Controllers.EditorController)AppMain._core.CurrentIOController).SelectTool);

            // TODO: I think it is ok for the incoming command from user to be all strings
            // but here the app has the capability to directly call functions...
            // So the plugin writer, key binders, and command console can be easy to use
            // but then internally, we parse and call the appropriate commands to carry out
            // the user's instructions .

            // So plugin writers wont have direct access... they will have to go thru client.exe
            // but users of the engine (ex applicatoin developers) can directly call certain functions
            // after they've interpreted the commnads the user has sent.
           
//            AppMain._core.Console.Execute ("tool_activate", new object[]"selector"});
            // or in keeping with commands that can modify state
            
            // the following is a ChangePropertySpec style of command where we set the tool 
            // that is currently to be used.
            // the visual of the widget can be modified assuming we just load from xml
            // but then there's the issue of wiring up it's events... well it should be similar
            // to how we wire up animations using friendly names...
//            context.IOController.SetProperty ("active_tool", "selector");


            // - assign target for the animationeditor
            //      - entity target
            //          - Transformable target (can be a model or selector node)
            //      - animationID 
            // - add an axis widget where i can set it's position


            // - how do i get the shared ClientScene in the AnimationEditor workspace to only render
            //   our target entity?  Perhaps it's a RenderingContext property we can set?
            //      - eg render selected entity only RenderingContext.flag
            //        this would mean that .SelectedEntity should be RenderingContext specific
            //        not scene specific?
            //             // TODO: OR, WHAT IF THE SCENE ONLY EDITS THE ENTITY ATTACHED TO THE 
            //             // AXIS EDIT TOOL??? WHEN A CERTAIN FLAG IS SET...
            //
            // - FIXED. button to zoom extents of target (similar to moveto)
            // - hide toolbar of viewports document viewports and have render area size properly
            // - when initializing ortho views with a target set, each view should auto
            //   set up to view the target to extents
            // - restrict the animation editor iso views from being changed
            
            // - restrict which items in the animation editor scene can be manipulated.
            //      - target entity cannot be translated or rotated, only the hinge widget 
            //        can which is stand in for the hinge rotation axis 
            // - add min/max angle arms to the axis widget
            //   - this widget's properties gets bounded to the target rotation interpolation animation
            
            // - normally to engage an edit tool, user clicks icon on viewport editor and 
            // it calls 
//            ((Keystone.Controllers.EditorController)AppMain._core.CurrentIOController).SelectTool(Keystone.EditTools.ToolType.Position);
            // which is EditController.SelectTool
            // so our editor host could do a similar thing of invoking .SelectTool (hingeEditorTool or something
            // that could be simplest way...    
            

            //
        }

        private void EditHinge(Entity target, string hingAnimation)
        {
        }

       
    }
}
