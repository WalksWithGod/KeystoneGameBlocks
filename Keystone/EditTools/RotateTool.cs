using System;
using Keystone.Collision;
using Keystone.Commands;
using Keystone.EditOperations;
using System.Collections.Generic;
using Keystone.Events;
using Keystone.Types;
using Keystone.Controls;
using Keystone.Controllers;
using System.Diagnostics;

namespace Keystone.EditTools
{

    /// <summary>
    /// </summary>
    public class RotateTool : TransformTool
    {

	    public RotateTool(Keystone.Network.NetworkClientBase netClient) 
            :base(netClient)
        {
            mActiveMode = TransformationMode.Rotation;

            mControl = EditTools.Widgets.LoadRotationWidget();

            mControl.MouseEnter += OnMouseEnter;
            mControl.MouseLeave += OnMouseLeave;
            mControl.MouseDown += OnMouseDown;
            mControl.MouseUp += OnMouseUp;
            mControl.MouseClick += OnMouseClick;
            mControl.MouseDrag += OnMouseDrag;

            Resource.Repository.IncrementRef(mControl);
        }

        #region EventHandlers       
        
        // TODO: this works well for orthogonal views, but i dont think for perspective.
        protected override void OnMouseDrag(object sender, EventArgs args)
        {
            InputCaptureEventArgs e = (InputCaptureEventArgs)args;
            Keystone.Cameras.Viewport vp = e.MouseEventArgs.Viewport;
            // TODO: i think mouseStart should not be modified here actually, just mouseEnd
            mMouseStart = mMouseEnd;
            mMouseEnd = e.MouseEventArgs.ViewportRelativePosition;
                       
            Quaternion result = Keystone.EditTools.RotationFunctions.Rotate(mSource, vp, mMouseStart, mMouseEnd, mSelectedAxes, VectorSpace.World);
            mSource.Rotation = result;
            ComponentRotation = result;
            ComponentTranslation = mSource.Translation ;
            ComponentScale = mSource.Scale ;
        }
        
        protected override void OnMouseUp(object sender, EventArgs args)
        {
            Trace.WriteLine("Controller.OnMouseUp - " + ((Keystone.Entities.Entity)sender).ID);

            InputCaptureEventArgs e = (InputCaptureEventArgs)args;
            Keystone.Cameras.Viewport vp = e.MouseEventArgs.Viewport;
            // TODO: i think mouseStart should not be modified here actually, just mouseEnd
            mMouseStart = mMouseEnd;
            mMouseEnd = e.MouseEventArgs.ViewportRelativePosition;
                       
            Quaternion result = Keystone.EditTools.RotationFunctions.Rotate(mSource, vp, mMouseStart, mMouseEnd, mSelectedAxes, VectorSpace.World);
            mSource.Rotation = result;
            
            mActiveControl = null;
        }
        #endregion
    }
}
