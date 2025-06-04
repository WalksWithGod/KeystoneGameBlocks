using System;
using System.Collections.Generic;


namespace KeyEdit.Workspaces
{
    public class AnimationEditWorkspace : EditorWorkspace
    {

        public AnimationEditWorkspace(string name) 
            : base(name)
        {
            mDisplayMode = KeyEdit.Controls.ViewportsDocument.DisplayMode.Quad;

            
            // TODO: does the scene need to be prevented from being disposed if we're going to use it
            // this way?  Because this is sharing the same underlying everything still...
            // er... ahhhhh! what to do?
            //
            // use cases
            // - solar system view
            //
            // - interior view 
            //
            // - preview window
            // - target telescopic view \ missile chase view
            // - 
        }

        public override void Configure(Keystone.Workspaces.IWorkspaceManager manager, Keystone.Scene.Scene scene)
        {
            base.Configure(manager, scene); // handles most, but we have some additional changes to make...


            // set viewports 1 - 3 to orthographic, but leave 0 as perspective
            for (int i = 1; i < 4; i++)
            {
                mViewportControls[i].Projection = Keystone.Cameras.Viewport.ProjectionType.Orthographic;
                if (i == 1) mViewportControls[i].View = Keystone.Cameras.Viewport.ViewType.Top;
                if (i == 2) mViewportControls[i].View = Keystone.Cameras.Viewport.ViewType.Right;
                if (i == 3) mViewportControls[i].View = Keystone.Cameras.Viewport.ViewType.Front;
            }

            //// hide the toolbars
            //for (int i = 0; i < 4; i++)
            //    mViewportControls[i].ShowToolbar = false;


            // - add the hinge to the scene here on activation and remove it on close OR loss focus?
            // public override Show()
            // {
            // 
            //      // our "Hinge tool" really shouldn't be a tool at all.  It must never receive
            //      // direct input results.  That will be from Rotate and Translation widgets.
            //      // Since we can respond to the hinge tool's events here in this Workspace
            //      // we can ensure that modifications of the hinge affects the offset translation
            //      // of the animation as well as set it's rotation axis!
            //      
            //      // if the hinge isn't loaded, do so
                   
            //      // assign the hinge to the target entity
            //
            //      // set the tool and prevent it from being replaced... how? we can
            //      // not allow any of the other toolbar tools to be visible.
            //      // then set a flag in the tool to prevent the user from selecting a new target
            //      // including null
            //
            //      // now if the Hinge Tool is not set as the current tool.. but instead just maintained
            //      // here, we should then be able to select the translate tool 
            //      // but we need to fix it's target to the hinge itself... that is
            //      // we can use the exact same tactic as for the hinge fixing to the entity
            //      // and have the MoveTool fixed to the hinge entity... but wait, we also need
            //      // it to have affect from RotateTool!  so... we must temporarily at least make the
            //      // target entity NON PICKABLE.  
            //
            //      // Move and Rotate then must be regular tools and selected from toolbar...
            //      //
            //      
            // 
            // }
            // public override Hide()
            // {
            // }
            // - assign the widget to the hinge and prevent the re-assigning of that widget
            // to anything else.
            //      - how to prevent it?  
            // - scripts on the widget will enable parts of the widget (like certain axis on ortho views)
            // on render
            // - hinge should also line axis indicators... do this by
            //   creating an immediate mode Axis indicator to the HUD for each viewport
            // - the grid should draw perpendicular to each view... 
            //   Hud.Add (grid)
            //   Hud.Add (axisIndicator)
            // 

            // assign the target and zoom extents

            // prevent changing of the target (unless user deletes or it falls out of scope somehow?)
            // in which case this, if the target is null, we stop rendering the viewport and so it's
            // gray?



            // set flag on RenderingContext to render Target Only

            // TODO: verify zoom's in each view dont allow you to flip around
        }
    }
}
