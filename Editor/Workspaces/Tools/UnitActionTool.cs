using System;
using System.Collections.Generic;
using Keystone.Types;
using Keystone.Cameras;
using Keystone.Entities;
using Keystone.Events;
using Keystone.EditTools;
using KeyCommon.Flags;

namespace KeyEdit.Workspaces.Tools
{

    public class UnitActionTool : SelectionTool
    {
        private Entity mSelectedUnit;

        

        // the Entity for which we are assigning the action must be passed in here
        // 
        public UnitActionTool(Keystone.Network.NetworkClientBase netClient, Entity selectedUnit)
            : base (netClient)
        {

            if (selectedUnit == null) throw new ArgumentNullException();

            mSelectedUnit = selectedUnit;
			_selectionMode = SelectionMode.Object;
        }
//
//        private Keystone.Controls.Control LoadPreviewWaypoint()
//        {
//           // string id = Keystone.Resource.Repository.GetNewName(typeof(ModeledEntity));
//           // ModeledEntity entity = new Controls.control(id);
//
//            string id = Keystone.Resource.Repository.GetNewName(typeof(Keystone.Controls.Control));
//            Keystone.Controls.Control entity = new Keystone.Controls.Control(id);
//             entity.SetEntityFlagValue ( (uint)KeyCommon.Flags.EntityFlags.HUD, true);
//             
//            id = Keystone.Resource.Repository.GetNewName(typeof(Keystone.Elements.ModelSequence));
//            Keystone.Elements.ModelSelector sequence = new Keystone.Elements.ModelSequence(id);
//
//            id = Keystone.Resource.Repository.GetNewName(typeof(Keystone.Elements.Model));
//            Keystone.Elements.Model model = new Keystone.Elements.Model(id);
//            
//            Keystone.Elements.Mesh3d mesh = Keystone.Elements.Mesh3d.CreateSphere(1f, 25, 25, MTV3D65.CONST_TV_MESHFORMAT.TV_MESHFORMAT_NOTEX, false);
//            
//            id = Keystone.Resource.Repository.GetNewName(typeof(Keystone.Elements.Model));
//            Keystone.Elements.Model lineModel = new Keystone.Elements.Model(id);
//            lineModel.InheritScale = false; // TODO: is inherit scale here = false desired?  wont know until i get waypoints/nav working again
//
//            id = Keystone.Resource.Repository.GetNewName(typeof(Keystone.Elements.LinesGeometry3D));
//            Keystone.Elements.LinesGeometry3D retainedLine = Keystone.Elements.LinesGeometry3D.Create(id);
//            Line3d line = new Line3d(0, 0, 0, 0, 0, 0); // initialize LinesGeometry3D with a line we can then modify endpoints for
//            retainedLine.AddLine(line);
//
//            Keystone.Appearance.Appearance appearance = Keystone.Celestial.ProceduralHelper.CreateAppearance(MTV3D65.CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED,
//                null, null, null, null, null);
//
//            sequence.AddChild(model);
//            sequence.AddChild(lineModel);
//
//            model.AddChild(appearance);
//            model.AddChild(mesh);
//            lineModel.AddChild (retainedLine);
//
//            entity.AddChild(sequence);
//            //entity.AddChild(model);
//            entity.Serializable = false;
//            entity.Dynamic = false;
//            entity.CollisionEnable = false;
//            entity.Flags |= EntityFlags.HUD;
//            entity.UseFixedScreenSpaceSize = true;
//            entity.ScreenSpaceSize = 0.005f;
//
//            Keystone.IO.PagerBase.LoadTVResourceSynchronously (entity, true); // assetplacementTool and WaypointPlacer should always .LoadTVResourceSynchronously() immediately so long as either reader.ReadSychronous or directly loading geometry nodes such as .CreateSphere()
//
//            return entity;
//        }


        public override void HandleEvent(EventType type, EventArgs args)
        {
            MouseEventArgs mouseArgs = (MouseEventArgs)args;

            // keyboard related event
            if (type == EventType.KeyboardCancel)
            {
                return; // TODO:
            }

            // mouse related event
            MousePosition3D = mouseArgs.UnprojectedCameraSpacePosition;
            _viewport = mouseArgs.Viewport;

            KeyCommon.Traversal.PickParameters pickParameters = mouseArgs.Viewport.Context.PickParameters;
            pickParameters.FloorLevel = int.MinValue; // search all floors 
            
            pickParameters.Accuracy |= KeyCommon.Traversal.PickAccuracy.Tile; 
            pickParameters.T0 = AppMain.MINIMUM_PICK_DISTANCE;
            pickParameters.T1 = AppMain.MAXIMUM_PICK_DISTANCE;
	            
            // NOTE: the pickParamaters used are from mouseArgs.Viewport.RenderingContext.PickParameters
            mPickResult = Pick(_viewport, mouseArgs.ViewportRelativePosition.X, mouseArgs.ViewportRelativePosition.Y, pickParameters);

            // TODO: for waypoint placement even when we do implement input catpure when mouse if off viewport
            //       we should restrain the waypoint to the FOV.  This should be a policy in all tool use
            //       beyond camera rotation and target rotation
            if (_viewport == null)
            {
                System.Diagnostics.Debug.WriteLine("UnitActionTool.HandleEvent() - No viewport selected");
                return;
            }


            // determine the parentEntity based on pickResult
            // do not just assume it's currentContextRegion
            RenderingContext currentContext = _viewport.Context;
            Entity parentEntity = currentContext.Region;
            if (mPickResult.Entity != null)
            {
                // NOTE: Currently there are too many problems accidentally placing new entities as children to Background3d entities
                // Lights, Stars, Moons, etc to allow use of the mouse over target.  Maybe if we let the user
                // hold down SHIFT key first so that it's explicit and not accidental.
                //parentEntity = mPickResult.Entity;
            }
            // TODO: parentEntity should not change if an operation is in progress
            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            // but as you can see above, that's exactly what im doing... MUST FIX
            if (parentEntity == null) return;



            // TODO: now we must also limit this position to any bounds of the region
            // that the item is being placed so we do need information about the "drop" point
            // System.Diagnostics.Debug.Assert();
            // TODO: One way we could achieve that is similar to with the CellPainter where we only
            // even allow the CellPainter tool to function if the parent entity is CelledRegion
            // in the pickResults arg.  Otherwise we exit early.  We can do the same with 
            // the throwobjecttool if we make sure that a Region parent is set in the PickResult only if
            // the mouse is in bounds.
            switch (type)
            {
                // this mousedown can(?) handles editablemesh geometry moving.  Currently there's code
                // for a manipulator to translate an Entity
                case EventType.MouseDown:
                    if (mouseArgs.Button == Keystone.Enums.MOUSE_BUTTONS.XAXIS) // TODO: wtf xasix?  why wont .LEFT work?!! localization testing needed
                    {
                        try
                        {
                        	System.Diagnostics.Debug.WriteLine("UnitActionTool.HandleEvent() - Mouse Down.");
                            On_MouseDown(mouseArgs);
                        }
                        catch (Exception ex)
                        {
                            // if the command execute fails we still want to end the input capture
                            System.Diagnostics.Debug.WriteLine("UnitActionTool.HandleEvent() - Error -" + ex.Message);

                        }
                        finally
                        {
                            // TODO: This i believe is probably false, we don't
                            // need to stop input capture do we?
                            //_hasInputCapture = false;
                            //DeActivate();

                        }
                    }
                    break;

                case EventType.MouseEnter:
                    System.Diagnostics.Debug.WriteLine("UnitActionTool.HandleEvent() - MouseEnter...");
                    break;

                case EventType.MouseMove:

                    On_MouseMove(mPickResult.Entity, mouseArgs);

                    // are we in orthographic view?
                    // switch (_viewport.Context.ProjectionType)
                    // {
                    //      case ProjectionType.Orthographic:
                    //          // is this a deck plan?
                    //          // is the mouse down?
                    //          // are we defining a rubber band area to drop
                    //          // an entire room of floorplan tiles?
                    // }
                    //


                    // 
                    //pickResult.ImpactPoint
                    //System.Diagnostics.Debug.WriteLine("WaypointPlacer.HandleEvent() - Mouse Move...");
                    break;

                // TODO: i think input capture is required... that might be part of the issue
                // with propogation of change flag recursion issues.  we'll have to logically
                // work this out but actually i think thats easy once we're ready toclean that up
                case EventType.MouseUp:
                    System.Diagnostics.Debug.WriteLine("UnitActionTool.HandleEvent() - Mouse Up.");
                    On_MouseUp(currentContext, parentEntity, MousePosition3D);

                    break;

                case EventType.MouseLeave:  // TODO: verify this event occurs when we switch tools
                    System.Diagnostics.Debug.WriteLine("UnitActionTool.HandleEvent() - Mouse Leave.");
                    break;

                default:
                    break;
            }
        }



        private void On_MouseDown(MouseEventArgs args)
        {

                    
        }

        private void On_MouseMove(Entity parent, MouseEventArgs args)
        {
            RenderingContext context = args.Viewport.Context;
            Viewport vp = context.Viewport;

            mMouseStart = mMouseEnd;
            mMouseEnd = args.ViewportRelativePosition;

            
            // TODO: draw visual HUD path showing route entity will take  
        }


        private void On_MouseUp(RenderingContext context, Entity parentEntity, Vector3d mousePosition3D)
        {
			// TODO: if simulation is enabled, then we need to ignore atempts
			//       at selecting non player controlled units.
        	
        	if (mPickResult.Entity == null) return;
            if (mPickResult.Entity == mSelectedUnit) return;
            // Send Waypoint Creation Request
            // GameObjects use CreateGameObject() request 
            // along with game01.dll enum for the gameobject type
            // we must re-send this message everytime we wish to create a waypoint
            // because gameObject guid's like Entity guids, must be created server side
            
            System.Diagnostics.Debug.Assert (mPickResult.Entity is Keystone.TileMap.Structure);
            string destinationRegionID = mPickResult.Entity.Region.ID;
            
            
            Game01.GameObjects.NavPoint wp = new Game01.GameObjects.NavPoint(mSelectedUnit.ID, destinationRegionID, mPickResult.ImpactPointLocalSpace);

            Lidgren.Network.IRemotableType msg =
                new KeyCommon.Messages.GameObject_Create_Request(wp.TypeName, mSelectedUnit.ID, wp);

            mNetClient.SendMessage(msg);            
        }
    }

}
