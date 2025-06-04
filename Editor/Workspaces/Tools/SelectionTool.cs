using System;
using Keystone.Entities;
using Keystone.Events;
using Keystone.EditTools;

namespace KeyEdit.Workspaces.Tools
{

    /// <summary>
    /// Just a generic picking tool.  The Selection Tool can be used in Arcade mode because we do not
    /// default by showing the Move Widget.  For that, use the MoveTool which will bring up that widget
    /// and handle Entity movement as well as low level editableMesh geometry
    /// </summary>
    public class SelectionTool : Tool
    {
        public enum SelectionMode 
        { 
            Object,
            Face,
            Edge,
            Vertex 
        }

        protected SelectionMode _selectionMode;
        protected Entity _mouseOver; // tracks current mouse over entities
        // TODO: Tool.mSource is for an entity that is selected and to which
        //       the gui widget created by the Tool (eg translation widget control)
        //       is added to dynamically.
        //       Here we can still use .mSource but we never "Activate" the widget
        //       because we have none.
        public SelectionTool(Keystone.Network.NetworkClientBase netClient) : base(netClient)
        {
            _selectionMode = SelectionMode.Object;
        }

        public SelectionMode Mode { get { return _selectionMode; } set { _selectionMode = value; } }

        
        public override void HandleEvent(EventType type, EventArgs args)
        {
            MouseEventArgs mouseArgs = args as MouseEventArgs;
            KeyboardEventArgs keyboardArgs = args as KeyboardEventArgs;

            // keyboard related event
            if (type == EventType.KeyboardCancel)
            {
                return; // TODO:
            }

            if (keyboardArgs != null)
            {
                // OBSOLETE - delete is handled by interpreter executing a key bind. Make sure keybind config file has DELETE bound
                //if (keyboardArgs.IsPressed && keyboardArgs.Key == "Delete")
                //{
                //    if (mPickResult != null && mPickResult.Entity != null)
                //    {
                //        // send command to delete this entity from the scene
                //        // NOTE: this does work on components placed in Interior and TileMap.
                //        KeyCommon.Messages.Node_Remove remove = new KeyCommon.Messages.Node_Remove(mPickResult.Entity.ID, mPickResult.Entity.Parent.ID);
                //        mNetClient.SendMessage(remove);
                //    }
                //}

                return;
            }

            if (mouseArgs != null)
            {
                // mouse related event
                _viewport = mouseArgs.Viewport;

                KeyCommon.Traversal.PickParameters pickParameters = mouseArgs.Viewport.Context.PickParameters;
                pickParameters.FloorLevel = int.MinValue; // search all floors 

                pickParameters.Accuracy |= KeyCommon.Traversal.PickAccuracy.Tile;

                // NOTE: the pickParamaters used are from mouseArgs.Viewport.RenderingContext.PickParameters
                mPickResult = Pick(_viewport, mouseArgs.ViewportRelativePosition.X, mouseArgs.ViewportRelativePosition.Y);


                switch (type)
                {
                    case EventType.MouseMove:
                        if (mPickResult.HasCollided)
                        {
                            //mouse.Viewport.Context.Scene.DebugPickLine = new Keystone.Types.Line3d(mPickResult.PickOrigin, mPickResult.PickEnd);

                            // pick to determine if the mouse previous mouse over is the same as the new and if so, relay events
                            Entity newMouseOver = mPickResult.Entity;
                            _viewport.Context.Workspace.MouseOverItem = mPickResult;

                            // Handle MouseEnter and MouseLeave events to any current controls
                            if (_mouseOver != newMouseOver)
                            {
                                if (_mouseOver != null)
                                {
                                    if (_mouseOver is Keystone.Events.IInputCapture)
                                        ((Keystone.Events.IInputCapture)_mouseOver).HandleEvent(EventType.MouseLeave, new InputCaptureEventArgs(mouseArgs, mPickResult));
                                }
                                if (newMouseOver != null)
                                {
                                    if (newMouseOver is Keystone.Events.IInputCapture)
                                        ((Keystone.Events.IInputCapture)newMouseOver).HandleEvent(EventType.MouseEnter, new InputCaptureEventArgs(mouseArgs, mPickResult));

                                    _mouseOver = newMouseOver;
                                }
                                else
                                {
                                    _mouseOver = null;
                                }
                            }

                            if (mSource != null)
                                if (mSource is Keystone.Events.IInputCapture)
                                    ((Keystone.Events.IInputCapture)mSource).HandleEvent(EventType.MouseMove, new InputCaptureEventArgs(mouseArgs, mPickResult));

                        }
                        break;
                    case EventType.MouseDown:
                        if (mPickResult.HasCollided == false)
                        {
                            mSource = null;
                            return;
                        }
                        // it's ok if mPickResult.Entity is null here.  That will properly
                        // set mSource null and .HandleEvent will get called with null entity and we can
                        // de-select and clear HUDs do to null selection.
                        mSource = mPickResult.Entity;
                        if (mSource is Keystone.Events.IInputCapture)
                            // TODO: rts unit "move to" "patrol" "fire at" goes through entity's script and whether it has any modifiers to it's "right mouse click" keypress
                            //         - perhaps even when Selection occurs, we then switch to a new Tool... a "Unit Mouse Control Tool"   
                            //           - so it's similar to how we initiate a WaypointPlacer for instance...   
                            ((Keystone.Events.IInputCapture)mSource).HandleEvent(EventType.MouseDown, new InputCaptureEventArgs(mouseArgs, mPickResult));

                        break;

                    case EventType.KeyboardCancel:
                    case EventType.MouseUp:
                        if (mSource != null)
                            if (mSource is Keystone.Events.IInputCapture)
                                ((Keystone.Events.IInputCapture)mSource).HandleEvent(EventType.MouseUp, new InputCaptureEventArgs(mouseArgs, mPickResult));

                        mSource = null;
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
