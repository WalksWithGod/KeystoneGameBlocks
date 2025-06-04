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
    public class NavPointPlacer_Old : Keystone.EditTools.PlacementTool
    {
        private Entity mNavPointsOwner;
        private bool mPlaceXZPlane = false;
        private bool mPlaceYAltitude = false;


        // the Entity for which we are creating the waypoint must be passed in here
        // 
        public NavPointPlacer_Old(Keystone.Network.NetworkClientBase netClient, Entity navPointsOwner)
            : base(netClient)
        {

            if (navPointsOwner == null) throw new ArgumentNullException();
            // TODO: it should be impossible to bring up the waypoint placement menu item
            //       if no vehicle was clicked because that is what should spawn the menus in the first
            //       place.
            if (navPointsOwner is Keystone.Vehicles.Vehicle == false) throw new ArgumentOutOfRangeException();

            mNavPointsOwner = navPointsOwner;

            // waypoint entities are non visible game objects
            // which can be rendered as proxies by the HUD.
            // But before a waypoint game object has been created when attempting to place one
            // we can still load a previewable waypoint that the HUD can render an immediate representation of

            // Load Preview Waypoint as an Entity
            this.mControl = LoadNavPointPlacementPreviewObject();
            Keystone.Resource.Repository.IncrementRef(this.mControl);
            //mSource = LoadPreviewWaypoint();
            //mSourceEntry = mSource.ID;
            // PlacementTool.DisposeUnmanagedResources DecrementRef's this
            //Keystone.Resource.Repository.IncrementRef(mSource);


            // we start immediatley in XZ plane placment mode when the tool is enabled
            mPlaceXZPlane = true;
            mPlaceYAltitude = false;
        }

        private Keystone.Controls.Control LoadNavPointPlacementPreviewObject()
        {
            // string id = Keystone.Resource.Repository.GetNewName(typeof(ModeledEntity));
            // ModeledEntity entity = new Controls.control(id);

            string id = Keystone.Resource.Repository.GetNewName(typeof(Keystone.Controls.Control));
            Keystone.Controls.Control entity = new Keystone.Controls.Control(id);
            entity.SetEntityAttributesValue((uint)KeyCommon.Flags.EntityAttributes.HUD, true);

            id = Keystone.Resource.Repository.GetNewName(typeof(Keystone.Elements.ModelSequence));
            Keystone.Elements.ModelSelector sequence = new Keystone.Elements.ModelSequence(id);

            id = Keystone.Resource.Repository.GetNewName(typeof(Keystone.Elements.Model));
            Keystone.Elements.Model model = new Keystone.Elements.Model(id);

            Keystone.Elements.Mesh3d mesh = Keystone.Elements.Mesh3d.CreateSphere(1f, 25, 25, MTV3D65.CONST_TV_MESHFORMAT.TV_MESHFORMAT_NOTEX, false);

            id = Keystone.Resource.Repository.GetNewName(typeof(Keystone.Elements.Model));
            Keystone.Elements.Model lineModel = new Keystone.Elements.Model(id);
            lineModel.InheritScale = false; // TODO: is inherit scale here = false desired?  wont know until i get waypoints/nav working again

            id = Keystone.Resource.Repository.GetNewName(typeof(Keystone.Elements.LinesGeometry3D));
            Keystone.Elements.LinesGeometry3D retainedLine = Keystone.Elements.LinesGeometry3D.Create(id);
            Line3d line = new Line3d(0, 0, 0, 0, 0, 0); // initialize LinesGeometry3D with a line we can then modify endpoints for
            retainedLine.AddLine(line);

            Keystone.Appearance.Appearance appearance = Keystone.Celestial.ProceduralHelper.CreateAppearance(MTV3D65.CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED,
                null, null, null, null, null);

            sequence.AddChild(model);
            sequence.AddChild(lineModel);

            model.AddChild(appearance);
            model.AddChild(mesh);
            lineModel.AddChild(retainedLine);

            entity.AddChild(sequence);
            //entity.AddChild(model);
            entity.Serializable = false;
            entity.Dynamic = false;
            entity.CollisionEnable = false;
            entity.Attributes |= EntityAttributes.HUD;
            entity.UseFixedScreenSpaceSize = true;
            entity.ScreenSpaceSize = 0.005f;

            Keystone.IO.PagerBase.LoadTVResource(entity, true); // assetplacementTool and WaypointPlacer should always .LoadTVResourceSynchronously() immediately so long as either reader.ReadSychronous or directly loading geometry nodes such as .CreateSphere()

            return entity;
        }


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

            // NOTE: the pickParamaters used are from mouseArgs.Viewport.RenderingContext.PickParameters
            mPickResult = Pick(_viewport, mouseArgs.ViewportRelativePosition.X, mouseArgs.ViewportRelativePosition.Y);

            // TODO: for waypoint placement even when we do implement input catpure when mouse if off viewport
            //       we should restrain the waypoint to the FOV.  This should be a policy in all tool use
            //       beyond camera rotation and target rotation
            if (_viewport == null)
            {
                System.Diagnostics.Debug.WriteLine("WaypointPlacer.HandleEvent() - No viewport selected");
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
                            System.Diagnostics.Debug.WriteLine("WaypointPlacer.HandleEvent() - Error -" + ex.Message);

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
                    System.Diagnostics.Debug.WriteLine("WaypointPlacer.HandleEvent() - MouseEnter...");
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
                    System.Diagnostics.Debug.WriteLine("WaypointPlacer.HandleEvent() - Mouse Up.");
                    On_MouseUp(currentContext, parentEntity, MousePosition3D);

                    break;

                case EventType.MouseLeave:  // TODO: verify this event occurs when we switch tools
                    System.Diagnostics.Debug.WriteLine("WaypointPlacer.HandleEvent() - Mouse Leave.");
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

            if (mControl != null)
            {
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
                    p = Plane.GetPlane(AxisFlags.XZ);
                }

                // compute a region relative position.  Hud.Update() will compute camera space position prior to adding to PVS
                Vector3d pos = context.Viewport.Pick_PlaneRayIntersection(context.Camera.View, context.Camera.Projection, context.Position, p, mMouseStart.X, mMouseStart.Y);

                // grab the line geometry from our source and modify the start and end points
                Keystone.Elements.LinesGeometry3D lineGeometry = (Keystone.Elements.LinesGeometry3D)((ModeledEntity)mControl).SelectModel(1).Geometry;

                if (mPlaceYAltitude)
                {
                    // keep existing translation but update y component only
                    ComponentTranslation.y = pos.y;

                    // re-use start but modify the stop endpoint to follow the mouse for altitude adjustments
                    Vector3d endpoint = Vector3d.Zero();
                    endpoint.y = -ComponentTranslation.y;

                    lineGeometry.SetEndPoints(0, Vector3d.Zero(), endpoint);

                    // TODO: i think it'd be even more useful if during waypoint placement the camera stayed behind your ship
                    //       and rotated to follow the waypoint marker such that the waypoint stayed in center of screen and only camera
                    //       orbited.... like crosshairs on a FPS.

                    // modify line color depending on whether altitude is above or below the plane
                    if (pos.y > 0)
                        lineGeometry.SetColor(0, Color.Green);
                    else
                        lineGeometry.SetColor(0, Color.Red);
                }
                else
                {
                    ComponentTranslation = pos;

                    // when no longer in altitude mode, reset interior line points local space coords to origin
                    // line is in local space with start and end point always at origin when not in atltitude mode
                    // so we do not need to modify the endpoints!
                    lineGeometry.SetEndPoints(0, Vector3d.Zero(), Vector3d.Zero());
                }
            }
        }


        private void On_MouseUp(RenderingContext context, Entity parentEntity, Vector3d mousePosition3D)
        {
            if (mPlaceXZPlane)
            {
                mPlaceYAltitude = true;
                mPlaceXZPlane = false;
            }
            else if (mPlaceYAltitude)
            {
                mPlaceYAltitude = false;
                mPlaceXZPlane = true;

                //// TODO: what about if we want to add a waypoint when there are
                //// already waypoints on this entity?  first we must grab the existing
                //// waypoints yes?

                // Send Waypoint Creation Request
                // GameObjects use CreateGameObject() request 
                // along with game01.dll enum for the gameobject type
                // we must re-send this message everytime we wish to create a waypoint
                // because gameObject guid's like Entity guids, must be created server side
                Game01.GameObjects.NavPoint wp = new Game01.GameObjects.NavPoint(mNavPointsOwner.ID, mNavPointsOwner.Region.ID, ComponentTranslation);

                Lidgren.Network.IRemotableType msg =
                    new KeyCommon.Messages.GameObject_Create_Request(wp.TypeName, mNavPointsOwner.ID, wp);

                mNetClient.SendMessage(msg);

            }
        }
    }

}
