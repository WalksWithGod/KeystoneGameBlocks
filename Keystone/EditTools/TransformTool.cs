using System;
using Keystone.Collision;
using Keystone.Commands;
using Keystone.EditOperations;
using System.Collections.Generic;
using Keystone.Events;
using Keystone.Types;
using Keystone.Controllers ;
using Keystone.Controls;
using System.Diagnostics;

namespace Keystone.EditTools
{
    
	public abstract class TransformTool : Tool 
	{
		protected TransformationMode mActiveMode;
        protected AxisFlags mSelectedAxes;
	
        protected EditTools.ManipFunctions2 mManipulatorFunctions; // still used by ScaleTool
        
        // HUD preview values for translation/scale/rotation
        public Vector3d ComponentTranslation;
        public Vector3d ComponentScale;
        public Quaternion ComponentRotation;
        
        
        protected TransformTool (Keystone.Network.NetworkClientBase netClient) :
        	base (netClient)
        {
        }
        
        #region EventHandlers
        protected virtual void OnMouseEnter(object sender, EventArgs args)
        {
            // set the transform/axis mode

            // TODO: here we might do something like change the material color of the control to it's RollOver state color
            //   Trace.WriteLine("Controller.OnMouseEnter - " + ((EntityBase)sender).ID);

            // change the material to roll over
        }

        protected virtual void OnMouseLeave(object sender, EventArgs args)
        {
            //Trace.WriteLine("Controller.OnMouseLeave - " + ((EntityBase)sender).ID);

            // change the material's back to default
        }


        protected virtual void OnMouseDown(object sender, EventArgs args)
        {
            Trace.WriteLine("Controller.OnMouseDown - " + ((Keystone.Entities.Entity)sender).ID);
            
            InputCaptureEventArgs e = (InputCaptureEventArgs)args;

            Keystone.Cameras.Viewport vp = e.MouseEventArgs.Viewport;

            if (e.PickResults == null || e.PickResults.Model == null) return;

            // NOTE: the Model.Names of our widgets must match 
            switch (e.PickResults.Model.Name)
            {
                case "xAxisModel":
                    mSelectedAxes = AxisFlags.X;
                    break;
                case "yAxisModel":
                    mSelectedAxes = AxisFlags.Y;
                    break;
                case "zAxisModel":
                    mSelectedAxes = AxisFlags.Z;
                    break;
                default:
                    break;
            }


            // initialize start and end to same current coord
            mMouseStart = mMouseEnd = e.MouseEventArgs.ViewportRelativePosition;
            
            mStartPickResults = mPickResult;
            //StartTile = mPickResult.TileLocation;
            ComponentTranslation = mSource.Translation; // (mPickResult.FacePoints[0] + mPickResult.FacePoints[2]) * .5d;
            //ComponentTranslation = RotationFunctions.Position (mSource, vp, mMouseStart, mMouseEnd, mSelectedAxes, VectorSpace.World); 
            mStartTranslation = ComponentTranslation;
    		ComponentScale = mSource.Scale;
            ComponentRotation = mSource.Rotation;
        }

        private Vector3d mStartTranslation;
        protected virtual void OnMouseClick(object sender, EventArgs args)
        {
            Trace.WriteLine("Controller.OnMouseClick - " + ((Keystone.Entities.Entity)sender).ID);
        }

        // TODO: this works well for orthogonal views, but i dont think for perspective.
        protected virtual void OnMouseDrag(object sender, EventArgs args)
        {
            Trace.WriteLine("Controller.OnMouseDrag - " + ((Keystone.Entities.Entity)sender).ID);
            InputCaptureEventArgs e = (InputCaptureEventArgs)args;
            Keystone.Cameras.Viewport vp = e.MouseEventArgs.Viewport;
            //mouseStart = mouseEnd;
            mMouseEnd = e.MouseEventArgs.ViewportRelativePosition;
               
			// TODO: hud needs to read ComponentTranslation and ComponentScale and ComponentRotation and update.  We should not be
			// directly modifying mSource in the manipulator call
            // NOTE: the "difference" value that returns from RotationFunctions.Position() is typically 0,0,0 on first mouse movement
            //       because the start and end values are nearly identicle.  As we drag, the difference is absolute from mMouseStart not cumulative.  So we cache the mStartTranslation and add the difference to it.	
            ComponentTranslation = mStartTranslation + RotationFunctions.Position (mSource, vp, mMouseStart, mMouseEnd, mSelectedAxes, VectorSpace.World); 
        }
        
        protected virtual void OnMouseUp(object sender, EventArgs args)
        {
            Trace.WriteLine("Controller.OnMouseUp - " + ((Keystone.Entities.Entity)sender).ID);

            mActiveControl = null;
            
            mStartPickResults = null;
            //ComponentRotation = new Quaternion();
            //ComponentTranslation = Vector3d.Zero();

            return;
        }
        #endregion

       
        protected void SendCommand(string nodeID, string propertyName, Type type, object newValue)
        {
        	Settings.PropertySpec spec = new Settings.PropertySpec(propertyName, type.Name);
            spec.DefaultValue = newValue;

            KeyCommon.Messages.Node_ChangeProperty changeProperty = new KeyCommon.Messages.Node_ChangeProperty();
            changeProperty.Add(spec);
            changeProperty.NodeID = nodeID;

            mNetClient.SendMessage(changeProperty);
		}
	}
	
}
