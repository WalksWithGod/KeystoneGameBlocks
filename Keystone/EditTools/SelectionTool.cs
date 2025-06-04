// OBSOLETE - Moved to KeyEdit.Workspaces.Tools
// 
//using System;
//using Keystone.Collision;
//using Keystone.Entities;
//using Keystone.Events;
//using Keystone.Modeler;
//using Keystone.Resource;

//namespace Keystone.EditTools
//{
//    /// <summary>
//    /// Just a generic picking tool.  The Selection Tool can be used in Arcade mode because we do not
//    /// default by showing the Move Widget.  For that, use the MoveTool which will bring up that widget
//    /// and handle Entity movement as well as low level editableMesh geometry
//    /// </summary>
//    public class SelectionTool : Tool
//    {
//        public enum SelectionMode 
//        { 
//            Object,
//            Face,
//            Edge,
//            Vertex 
//        }

//        private EditTools.SelectionTool.SelectionMode _selectionMode;
//        private Entity _mouseOver; // tracks current mouse over entities
//        // TODO: Tool.mSource is for an entity that is selected and to which
//        //       the gui widget created by the Tool (eg translation widget control)
//        //       is added to dynamically.
//        //       Here we can still use .mSource but we never "Activate" the widget
//        //       because we have none.
//        public SelectionTool(Keystone.Network.NetworkClientBase netClient) : base(netClient)
//        {
//            _selectionMode = EditTools.SelectionTool.SelectionMode.Object;
//        }

//        public SelectionMode Mode { get { return _selectionMode; } set { _selectionMode = value; } }

        
//        public override void HandleEvent(EventType type, EventArgs args)
//        {
//            MouseEventArgs mouse = (MouseEventArgs)args;

//            // keyboard related event
//            if (type == EventType.KeyboardCancel)
//            {
//                return; // TODO:
//            }

//            // mouse related event
//            mPickResult = Pick(mouse.Viewport, mouse.ViewportRelativePosition.X, mouse.ViewportRelativePosition.Y);

//            switch (type)
//            {
//                case EventType.MouseMove :
//                    if (mPickResult.HasCollided)
//                    {
//                        //mouse.Viewport.Context.Scene.DebugPickLine = new Keystone.Types.Line3d(mPickResult.PickOrigin, mPickResult.PickEnd);
                        
//                        // pick to determine if the mouse previous mouse over is the same as the new and if so, relay events
//                        Entity newMouseOver = mPickResult.Entity;
//                        mouse.Viewport.Context.Workspace.MouseOverItem = mPickResult;

//                        // Handle MouseEnter and MouseLeave events to any current controls
//                        if (_mouseOver != newMouseOver)
//                        {
//                            if (_mouseOver != null)
//                            {
//                                if (_mouseOver is Keystone.Events.IInputCapture)
//                                    ((Keystone.Events.IInputCapture)_mouseOver).HandleEvent(EventType.MouseLeave, new InputCaptureEventArgs(mouse, mPickResult));
//                            }
//                            if (newMouseOver != null)
//                            {
//                                if (newMouseOver is Keystone.Events.IInputCapture)
//                                    ((Keystone.Events.IInputCapture)newMouseOver).HandleEvent(EventType.MouseEnter, new InputCaptureEventArgs(mouse, mPickResult));

//                                _mouseOver = newMouseOver;
//                            }
//                            else
//                            {
//                                _mouseOver = null;
//                            }
//                        }

//                        if (mSource != null)
//                            if (mSource is Keystone.Events.IInputCapture)
//                                ((Keystone.Events.IInputCapture)mSource).HandleEvent(EventType.MouseMove, new InputCaptureEventArgs(mouse, mPickResult));

//                    }
//                    break;
//                case EventType.MouseDown:
//                    if (mPickResult.HasCollided == false)
//                    {
//                        mSource = null;
//                        return;  
//                    }
//                    // it's ok if mPickResult.Entity is null here.  That will properly
//                    // set mSource null and .HandleEvent will get called with null entity and we can
//                    // de-select and clear HUDs do to null selection.
//                    mSource = mPickResult.Entity;
//                    if (mSource is Keystone.Events.IInputCapture)
//                        ((Keystone.Events.IInputCapture)mSource).HandleEvent(EventType.MouseDown, new InputCaptureEventArgs(mouse, mPickResult));

//                    break;

//                case EventType.KeyboardCancel:
//                case EventType.MouseUp :
//                    if (mSource != null)
//                        if (mSource is Keystone.Events.IInputCapture)
//                            ((Keystone.Events.IInputCapture)mSource).HandleEvent(EventType.MouseUp, new InputCaptureEventArgs(mouse, mPickResult));

//                    mSource = null;
//                    break;

//                default:
//                    break;
//            }
//        }
//    }
//}
