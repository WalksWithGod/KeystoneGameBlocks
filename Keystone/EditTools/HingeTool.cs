using System;
using Keystone.Collision;
using Keystone.Commands;
using Keystone.EditOperations;
using System.Collections.Generic;
using Keystone.Events;
using Keystone.Types;
using Keystone.Controllers;
using System.Diagnostics;
using Keystone.Entities;
using Keystone.Controls;

namespace Keystone.EditTools
{
    public class HingeTool : Tool
    {

        private Keystone.EditTools.ManipFunctions2 mManipulatorFunctions;

        public HingeTool(Keystone.Network.NetworkClientBase netClient) : base(netClient)
        {
            mIsActive = false;
            mHasInputCapture = false;

            mControl = EditTools.Widgets.LoadHingeWidget();

            mManipulatorFunctions = new ManipFunctions2();
            WireEvents(mControl);

            Resource.Repository.IncrementRef(mControl);
        }


        private void WireEvents(Control control)
        {
            // can we wire up these controls in such a way that
            // the root control can respond to all events... that is
            // we can supply a single script control or set of method delegate
            // handlers primarily so that the root control can manage
            // state including InputCatpure
            // In this way, a "Tool" is just a group of those events and nothing more really...
            // We can add as additional arguments to the "Tool's" HandleEvent
            // the specific child sub-model index that was clicked... this way
            // we dont need to have seperate child Entities... but just a sequence of child
            // models.  They dont need event handlers, they'll be handled in the single event handler
            // which will respond differently based on the index of the child model hit!


            // TODO: I think the HINGE itself must be a TARGET entity that is temporarily placed
            // in the scene....   then we use or normal gizmos for ortho views to actually position
            // and orient the hinge.
            // TODO: The entity itself should be fixed at origin and non moveable.

            // TODO: i think our translation axis 
            // axis control can be translated
            Control child = (Control)control.Children[0];
            child.MouseEnter += OnMouseEnter;
            child.MouseLeave += OnMouseLeave;
            child.MouseDown += OnMouseDown;
            child.MouseUp += OnMouseUp;
            child.MouseDrag += OnMouseDrag;
            child.MouseClick += OnMouseClick;

 

        }

        private void OnMouseEnter(object sender, EventArgs e)
        {

        }

        private void OnMouseLeave(object sender, EventArgs e)
        {
        }

        private void OnMouseDown(object sender, EventArgs e)
        {
            Trace.WriteLine("Controller.OnMouseDown - " + ((Entity)sender).ID);

            Keystone.Cameras.Viewport vp = ((Keystone.Events.MouseEventArgs)e).Viewport;
            // mouseDelta = new System.Drawing.Point(mLastMousePosition.X - ((MouseEventArgs)args).ViewportRelativePosition.X, mLastMousePosition.Y - ((MouseEventArgs)args).ViewportRelativePosition.Y);

            //Debug.WriteLine("Mouse delta = " + mouseDelta.ToString());

            // swap the end with last start, and new start with current
            mMouseStart = mMouseEnd = ((Keystone.Events.MouseEventArgs)e).ViewportRelativePosition;

            // if no change, return after we've updated the mouseStart and mouseEnd
            //if (mouseDelta.X == 0 && mouseDelta.Y == 0) return;
        }

        private void OnMouseUp(object sender, EventArgs e)
        {
        }

        private void OnMouseClick(object sender, EventArgs e)
        {
        }

        private void OnMouseDrag(object sender, EventArgs e)
        {
            Keystone.Cameras.Viewport vp = ((Keystone.Events.MouseEventArgs)e).Viewport;
            //mouseDelta = new System.Drawing.Point(mouseEnd.X - ((Keystone.Events.MouseEventArgs)args).ViewportRelativePosition.X, mouseEnd.Y - ((MouseEventArgs)args).ViewportRelativePosition.Y);
            mMouseStart = mMouseEnd;
            mMouseEnd = ((Keystone.Events.MouseEventArgs)e).ViewportRelativePosition;

            // TODO: the active mode and selected axes has to be determined from
            // viewport ortho mode
            TransformationMode mode = TransformationMode.TranslationPlane;
            AxisFlags flags = AxisFlags.None;

            if (vp.Context.ProjectionType == Keystone.Cameras.Viewport.ProjectionType.Orthographic)
            {
                switch (vp.Context.ViewType)
                {
                    case Keystone.Cameras.Viewport.ViewType.Front:
                    case Keystone.Cameras.Viewport.ViewType.Back:
                        flags = AxisFlags.YX;
                        break;
                    case Keystone.Cameras.Viewport.ViewType.Top:
                    case Keystone.Cameras.Viewport.ViewType.Bottom:
                        flags = AxisFlags.XZ;
                        break;
                    case Keystone.Cameras.Viewport.ViewType.Left:
                    case Keystone.Cameras.Viewport.ViewType.Right:
                        flags = AxisFlags.YZ;
                        break;
                }
                // TODO: Make sure this is not trying to use the TranslationPlane on a 
                // viewport that is NOT orthographic
                System.Drawing.Point diff = new System.Drawing.Point(mMouseEnd.X - mMouseStart.X, mMouseEnd.Y - mMouseStart.Y);
                System.Diagnostics.Debug.WriteLine("Dragging...." + diff.ToString());
            }
            else
            {
                flags = AxisFlags.X;
                mode = TransformationMode.TranslationAxis;
            }

            mManipulatorFunctions[mode][flags](mSource, vp, mMouseStart, mMouseEnd, flags, VectorSpace.World);
        }
    }
}
