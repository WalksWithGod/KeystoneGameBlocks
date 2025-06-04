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
    // normally placement tool is selected by asset browser entity selection
    // here, we want to right mouse click on an item that can utilize waypoints, select it
    // show its waypoints, and popup menu to append waypoint.  or select an existing waypoint
    // and delete it, move it, or insert a waypoint before it.
    // So can we get the EditorController to switch the tools for us?
    public class NavPointPlacer : Keystone.EditTools.PlacementTool 
    {
        private Entity mNavPointsOwner;
        private bool mPlaceXZPlane = false;
        private bool mPlaceYAltitude = false;
        

        // the Entity for which we are creating the waypoint must be passed in here
        // 
        public NavPointPlacer(Keystone.Network.NetworkClientBase netClient)
            : base (netClient)
        {
            // we start immediatley in XZ plane placment mode when the tool is enabled
            mPlaceXZPlane = true;
            mPlaceYAltitude = false;
        }

        


        public override void HandleEvent(EventType type, EventArgs args)
        {
            MouseEventArgs mouseArgs = args as MouseEventArgs;

            // keyboard related event
            if (type == EventType.KeyboardCancel)
            {

                return; // TODO:
            }
            else if (type == EventType.KeyboardKeyDown)
            {
                KeyboardEventArgs keyboardArgs = (KeyboardEventArgs)args;
                if (keyboardArgs.Key == "LeftControl" || keyboardArgs.Key == "RightControl")
                {
                    this.mPlaceYAltitude = true;
                    this.mPlaceXZPlane = false;
                }
                return;
                
            }
            else if (type == EventType.KeyboardKeyUp)
            {
                KeyboardEventArgs keyboardArgs = (KeyboardEventArgs)args;
                if (keyboardArgs.Key == "LeftControl" || keyboardArgs.Key == "RightControl")
                {
                    this.mPlaceYAltitude = false;
                    this.mPlaceXZPlane = true;
                }
                return;
            }
            // mouse related event
            MousePosition3D = mouseArgs.UnprojectedCameraSpacePosition;
            _viewport = mouseArgs.Viewport;

            // NOTE: the pickParamaters used are from mouseArgs.Viewport.RenderingContext.PickParameters
            mPickResult = Pick(_viewport, mouseArgs.ViewportRelativePosition.X, mouseArgs.ViewportRelativePosition.Y);

            // TODO: for waypoint placement even when we do implement input catpure when mouse if off viewport
            //       we should restrain the waypoint to the FOV.  This should be a policy in all tool use
            //       beyond camera rotation and target rotation
            if (_viewport == null)
            {
                System.Diagnostics.Debug.WriteLine("NavPointPlacer.HandleEvent() - No viewport selected");
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
                            On_MouseDown(mouseArgs);
                        }
                        catch (Exception ex)
                        {
                            // if the command execute fails we still want to end the input capture
                            System.Diagnostics.Debug.WriteLine("NavPointPlacer.HandleEvent() - ERROR -" + ex.Message);

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
                    System.Diagnostics.Debug.WriteLine("NavPointPlacer.HandleEvent() - MouseEnter...");
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
                    System.Diagnostics.Debug.WriteLine("NavPointPlacer.HandleEvent() - Mouse Up.");
                    On_MouseUp(currentContext, parentEntity, MousePosition3D);

                    break;

                case EventType.MouseLeave:  // TODO: verify this event occurs when we switch tools
                    System.Diagnostics.Debug.WriteLine("NavPointPlacer.HandleEvent() - Mouse Leave.");
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// The box to which we constrain mouse positioning of the waypoint tabs.
        /// </summary>
        public BoundingBox Bounds { get; set; }

        private bool mDragging = false;
        private Entity mDraggedWaypoint;
        private int mDraggedWaypointIndex;
        internal Vector3d mRegionDimensions;
        internal Vector3d mCoordinateScaling = new Vector3d(1, 1, 1);
        private void On_MouseDown(MouseEventArgs args)
        {
#if DEBUG
            Viewport vp = args.Viewport;
            System.Diagnostics.Debug.Assert(vp.Contains(args.ViewportRelativePosition.X, args.ViewportRelativePosition.Y));
#endif
            // have we mouse downed over a waypoint?
            Entity entity = mPickResult.Entity;
            if (entity == null) return;

            if (!string.IsNullOrEmpty (entity.Name) && entity.Name.Contains("#WAYPOINT"))
            {
                // TODO: we need to cap number of waypoints allowed to 99 or 999
                string[] s = entity.Name.Split(new char[] { ',' });
                
                mDraggedWaypointIndex = int.Parse(s[1]);
                mMouseStart = args.ViewportRelativePosition;
                mPlaceXZPlane = true;   
                mDragging = true;
                mDraggedWaypoint = entity;

                // grab the Vehicle ID from the waypoint's Name
                string vehicleID = s[2];
                mNavPointsOwner = (Entity)Keystone.Resource.Repository.Get(vehicleID);
                System.Diagnostics.Debug.WriteLine("NavPointPlacer.On_MouseDown() - " + entity.Name);
            }
        }


        private void On_MouseMove(Entity parent, MouseEventArgs args)
        {
            RenderingContext context = args.Viewport.Context;
            Viewport vp = context.Viewport;

            mMouseStart = mMouseEnd;
            mMouseEnd = args.ViewportRelativePosition;

            if (mDragging)
            {
                BoundingBox bounds = context.Scene.Root.BoundingBox;
                Plane p;

                if (mPlaceYAltitude)
                {
                    // Determine HEIGHT / altitude to position the waypoint.
                    
                    // We use an XY billboarding plane that is always orthogonal to camera at 
                    // distance of ComponentTranslation.x, ComponentTranslation.z and height
                    // at mouse pick Y value against this new plane
                    Matrix billboardMatrix = Matrix.CreateBillboardRotationMatrix(Vector3d.Up(), context.LookAt);
                    // translate plane to the world space position of our waypoint marker
                    billboardMatrix.SetTranslation(ComponentTranslation);

                    p = Plane.GetPlane(AxisFlags.YX);
                    p.Transform(billboardMatrix);

                    // TODO: in RegionPVS we can compute and draw distance labels.  Is there a way to do this
                    //       all the time with waypoint place?

                }
                else
                {
                	// this plane is designed to allow us to select DEPTH of the waypoint 
                    // origin centered plane is in world space (no translation to camera space!)
                    // TODO: eventually i may want to adjust translation of this plane to always be in a world position
                    // that is relative to the camera because if the camera is below the origin we may
                    // never pick it successfully.  Indeed, the picking plane + grid when activated should adjust itself with respect
                    // to camera automatically.
                    // TODO: the mouse movement is not constrained to the XZ plane.  It seems to be constrained only to the Screen plane
                    //       which means it's only XZ constrained if we're looking down the -Y axis.  Or is it constrained but is
                    //       moving beyond the min/max of the Zone?
                    p = Plane.GetPlane(AxisFlags.XZ);                      
                }

                // compute a region relative position.  Hud.Update() will compute camera space position prior to adding to PVS
                Vector3d pos = context.Viewport.Pick_PlaneRayIntersection(context.Camera.View, context.Camera.Projection, context.Position, p, mMouseStart.X, mMouseStart.Y);

                // grab the line geometry from our source and modify the start and end points
 //               Keystone.Elements.LinesGeometry3D lineGeometry = (Keystone.Elements.LinesGeometry3D)((ModeledEntity)mControl).SelectModel(1).Geometry;

                if (mPlaceYAltitude)
                {
                    // keep existing translation but update y component only
                    // constrain the Y axis to the min/max bounds of our galaxy in scaled galactic coordinates
                    // TODO: the scaling must match the current zoom level (eg galactic or system)
                    pos.y = Math.Min (pos.y, Bounds.Max.y * mCoordinateScaling.y);
                    pos.y = Math.Max (pos.y, Bounds.Min.y * mCoordinateScaling.y);


                    ComponentTranslation.y = pos.y;
                    mDraggedWaypoint.Translation = ComponentTranslation;

                    // re-use start but modify the stop endpoint to follow the mouse for altitude adjustments
                    Vector3d endpoint = Vector3d.Zero();
                    endpoint.y = -ComponentTranslation.y;

//                    lineGeometry.SetEndPoints(0, Vector3d.Zero(), endpoint);

                    // TODO: i think it'd be even more useful if during waypoint placement the camera stayed behind your ship
                    //       and rotated to follow the waypoint marker such that the waypoint stayed in center of screen and only camera
                    //       orbited.... like crosshairs on a FPS.

                    // modify line color depending on whether altitude is above or below the plane
//                    if (pos.y > 0)
//                        lineGeometry.SetColor(0, Color.Green);
//                    else
//                        lineGeometry.SetColor(0, Color.Red);
                }
                else
                {
                    // constrain XZ coordinates to the bounds of the Galaxy or System
                    // TODO: need to vary boundingbox we use based on zoom level (galactic or system)
                    pos.x = Math.Min(pos.x, Bounds.Max.x * this.mCoordinateScaling.x);
                    pos.x = Math.Max(pos.x, Bounds.Min.x * this.mCoordinateScaling.x);
                    pos.z = Math.Min(pos.z, Bounds.Max.z * this.mCoordinateScaling.z);
                    pos.z = Math.Max(pos.z, Bounds.Min.z * this.mCoordinateScaling.z);

                    ComponentTranslation = pos;
                    mDraggedWaypoint.Translation = ComponentTranslation; // HACK?  since we do not use a dedicated preview graphic controlled by Hud
                    System.Diagnostics.Debug.WriteLine("NavPointPlacer.On_MouseMove() - " + pos.ToString());

                    // when no longer in altitude mode, reset interior line points local space coords to origin
                    // line is in local space with start and end point always at origin when not in atltitude mode
                    // so we do not need to modify the endpoints!
//                    lineGeometry.SetEndPoints(0, Vector3d.Zero(), Vector3d.Zero());
                }
            }
        }

        internal string mSingleSystemZoneID;
        internal Vector3d mStarOffset;
        internal KeyEdit.Workspaces.Huds.NavigationHud.MapMode mMode = Huds.NavigationHud.MapMode.Galaxy;
        private void On_MouseUp(RenderingContext context, Entity parentEntity, Vector3d mousePosition3D)
        {
            
            if (mDragging)
            {
                string regionID = null;
                Vector3d zoneCoordinates = Vector3d.Zero();
                // unlike with other Tool implementations

                // we need:
                // - the scaling value of the current view.  The nav hud can update this for us
                // - whether we are in galactic or region zoom levels
                // - find code to convert galactic to region...
                if (mPlaceXZPlane || mPlaceYAltitude)
                {
                    Keystone.Portals.ZoneRoot root = (Keystone.Portals.ZoneRoot)AppMain._core.SceneManager.Scenes[0].Root;

                    Vector3d scaledCoords = mDraggedWaypoint.Translation;
                    if (mMode ==  Huds.NavigationHud.MapMode.Galaxy)
                    {  
                        Vector3d galacticCoords = scaledCoords;
                        galacticCoords.x *= root.RegionsAcross * root.RegionDiameterX;
                        galacticCoords.y *= root.RegionsHigh * root.RegionDiameterY;
                        galacticCoords.z *= root.RegionsDeep * root.RegionDiameterZ;
                        zoneCoordinates = root.GlobalCoordinatesToZoneCoordinates(galacticCoords.x, galacticCoords.y, galacticCoords.z, out regionID);
                    }
                    else
                    {
                        // TODO: 
                        scaledCoords *= (mRegionDimensions / 10d);
                        //scaledCoords -= mStarOffset;
                        regionID = mSingleSystemZoneID;
                        zoneCoordinates = scaledCoords; 
                    }
                }
                // can we convert the pos to region space coordinates?
                // first we have to scale the position to galactic coordinates and then
                // to region coordinates?  But if setting coordinates within a Region (Stellar System view)
                // then we just convert pos directly to region coords and bounds check them and constrain them.


                //if (mPlaceXZPlane)
                //{
                    mPlaceYAltitude = false;
                    mPlaceXZPlane = false;
                //}
                //else if (mPlaceYAltitude)
                //{

                // TODO: how do we switch from XZ plane movement to Y altitude movement?
                //       Can we CTLR + drag to switch to Y axis movemet?
                    mPlaceYAltitude = false;
                    mPlaceXZPlane = true;

                    //// TODO: what about if we want to add a waypoint when there are
                    //// already waypoints on this entity?  first we must grab the existing
                    //// waypoints yes?  
                    // TODO: what about inserting waypoints so that once we've added a waypoint
                    // on the galactic scale to take us to a system, when we then zoom into the
                    // system, we want to add waypoints to take us to worlds within that system.

                    // Send Waypoint Creation Request
                    // GameObjects use CreateGameObject() request 
                    // along with game01.dll enum for the gameobject type
                    // TODO: on Add Waypoint mouse click, we must send this a GameObject_Create_Request() message to create a waypoint
                    // because gameObject guid's like Entity guids, must be created server side
                    Game01.GameObjects.NavPoint waypointGameObject = new Game01.GameObjects.NavPoint(mNavPointsOwner.ID, regionID, zoneCoordinates);

                    Lidgren.Network.IRemotableType msg =
                        new KeyCommon.Messages.GameObject_ChangeProperties(mNavPointsOwner.ID, waypointGameObject);
                    ((KeyCommon.Messages.GameObject_ChangeProperties)msg).GameObjectIndex = mDraggedWaypointIndex;

                    mNetClient.SendMessage(msg);

                //}
            }
            mDragging = false;
            mDraggedWaypoint = null;
        }
    }

}
