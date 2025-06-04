using System;
using Keystone.Entities;
using Keystone.Events;
using Keystone.EditTools;
using Keystone.Types;
using Keystone.Portals;
using Keystone.Utilities;

namespace KeyEdit.Workspaces.Tools
{
    /// <summary>
    /// Tool used to transform (position and rotation only) components that have already been placed
    /// within the Interior of a Container (eg. vehicle)
    /// </summary>
    public class InteriorTransformTool : TransformTool
    {
        public enum Alignments
        {
            Tops,
            Bottoms,
            Lefts,
            Rights,
            Centers
        }

        private bool mDragging;
        private bool mMultiSelect = false; // NOTE: for v1.0, no multiselect + drag allowed.  
        private bool mMouseDown = false;
        private Interior mInterior;
        
        System.Diagnostics.Stopwatch mStopwatch;
        const int mDragBeginIntervalMS = 100;
        public ModeledEntity mClonedSource = null;
        public System.Collections.Generic.List<ModeledEntity> mSelections = new System.Collections.Generic.List<ModeledEntity>();

        public InteriorTransformTool(Keystone.Network.NetworkClientBase netClient)
            : base(netClient)
        {
            ComponentRotation = new Keystone.Types.Quaternion();
        }

        public ModeledEntity[] Selections { get { if (mSelections.Count > 0) return mSelections.ToArray(); return null; } }
        public bool MultiSelect { get { return mMultiSelect; } }

        public bool Dragging { get { return mDragging; } }

        // todo: i think this is unnecessary, especially considering multiselect=true
        private ModeledEntity CloneSource()
        {
            bool recurse = true;
            bool delayResourceLoading = true;
            string id = Keystone.Resource.Repository.GetNewName("ModeledEntity");
            ModeledEntity clone = (ModeledEntity)mSource.Clone(id, recurse, false, delayResourceLoading);

            // since we delay resource loading, find the Geometry nodes only and force load them
            Keystone.Elements.Geometry geometry = clone.SelectModel(0).Geometry;
            Keystone.IO.PagerBase.LoadTVResource(geometry);

            return clone;
        }

        // The mSource entity needs to be the actual entity that is selected
        // from within the Interior.  We must not unload that entity and load 
        // a clone on MouseUp because we don't want the GUID to change or any
        // custom property values to be wiped out.  But we do need to 
        // Remove then AddChild the entity for simulating a "Move" behavior
        // because we need the footprint data to be cleared and then re-set
        // to the new TileLocation.  We also need for the Component to be
        // reset within the ISpatialNode scenenode.

        // So first thing is, how do we get SelectTool to switch to InteriorTransformTool 
        // upon MouseDown event?  Well, the first way to do that is to simply
        // make the InteriorTransformTool the default tool.  Have it behave
        // like a selection tool when no Entity is selected.  Then on mousedown
        // have it behave like InteriorTransformTool and on MouseUp back to no 
        // move behavior.

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

                if (keyboardArgs.Key == "LeftShift" || keyboardArgs.Key == "RightShift")
                {
                    if (keyboardArgs.IsPressed)
                    {
                        mMultiSelect = true;
                        //mSelections.Add((ModeledEntity)mSource);
                    }
                    else
                    {
                        // todo: if previous mMultiSelect == true and mSelections.Count > 1, then we should not clear the Selections
                        // clearing of mSelections should only occur when mousedown pickResult.Entity == null.
                        // note: we should ignore BonedEntities or any Entity that does not have a footprint
                        mMultiSelect = false;
                       // mSelections.Clear();
                    }
                }
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
                            if (mDragging == false)
                            {
                                Entity newMouseOver = mPickResult.Entity;
                                _viewport.Context.Workspace.MouseOverItem = mPickResult;
                            }

                            Component_MouseMove(mouseArgs);


                        }
                        break;
                    case EventType.MouseDown:
                        // pickparameters.ExcludedObjectTypes should be used to prevent certain types of Entities here
                        if (mPickResult.HasCollided == false || mPickResult.Entity is Interior || mPickResult.Entity is Root || mPickResult.Entity is Zone)
                        {
                            mSource = null;

                            // TODO: if we misclick and hit the Interior, i dont think we want to clear all selections do we?
                            mSelections.Clear();
                            return;
                        }

                        // it's ok if mPickResult.Entity is null here.  That will properly
                        // set mSource null and .HandleEvent will get called with null entity and we can
                        // de-select and clear HUDs due to null selection.
                        mSource = mPickResult.Entity;
                        if (mMultiSelect)
                        {
                            // if the picked entity already exists in the mSelections list, remove it. This acts as a de-select
                            if (mSelections.Contains((ModeledEntity)mPickResult.Entity))
                                mSelections.Remove((ModeledEntity)mPickResult.Entity);
                            else 
                                mSelections.Add((ModeledEntity)mPickResult.Entity);
                        }
                        else
                        {
                            mSelections.Clear();
                            mSelections.Add((ModeledEntity)mPickResult.Entity);
                            Component_MouseDown(mouseArgs);
                        }
                        break;

                    case EventType.KeyboardCancel:
                    case EventType.MouseUp:
                        Component_MouseUp();

                        break;

                    default:
                        break;
                }
            }
        }

        public void AlignSelectedComponents(Alignments alignment)
        {
            if (mSelections == null || mSelections.Count <= 1) return;

           
            // todo: i have no "undo()" command which would be helpful if we make a mistake while aligning (eg. we align left when we meant to click right)
            // todo: we don't know the performance impact until we try it.  no point in thinking of how to optimize removal and re-inserting until we implement it in those most naive way first.
            switch (alignment)
            {
                case Alignments.Lefts:
                    break;

                case Alignments.Rights:
                    break;

                case Alignments.Tops:
                    break;

                case Alignments.Bottoms:
                    break;

                case Alignments.Centers: // todo: i think this option makes no sense and should be removed. "center" has to be relative to either x or z axis
                    break;
                default:
                    throw new Exception();
            }

            // NOTE: the first selection is not moved, it is the selection we are aligning with
            Vector3d[] positions = new Vector3d[mSelections.Count - 1];
            string[] targetIDs = new string[mSelections.Count - 1];
            mInterior = (Interior)mSelections[0].Parent;

            for (int i = 1; i < mSelections.Count; i++)
            {
                // TODO: if the position.x or position.z is already aligned, skip it
                Vector3d position = mSelections[i].Translation;
                position.x = mSelections[0].Translation.x;
                targetIDs[i - 1] = mSelections[i].ID;
                positions[i - 1] = position;
                System.Diagnostics.Debug.Assert(mSelections[i].Parent == mInterior);           
            }

            
            //  server validates placement. if not in loopback, client should validate prior to sending the Entity_Move command
            KeyCommon.Messages.Entity_Move moveMessage = new KeyCommon.Messages.Entity_Move(mInterior.ID, targetIDs, positions);
            Entity tmp = (Entity)Keystone.Resource.Repository.Get(mInterior.ID);
            System.Diagnostics.Debug.Assert(tmp != null);

            mNetClient.SendMessage(moveMessage);
        }

        private void RemoveSourceComponent()
        {
            // undo the footprint
            Keystone.Resource.Repository.IncrementRef(mSource); // do not let this fall out of scope on RemoveChild()
            mInterior = (Interior)mSource.Parent;
            mInterior.RemoveChild(mSource);

        }

        /// <summary>
        /// Restores the source component to it's starting position.
        /// NOTE: The server never moved the component in the first place,
        /// so this is the client restoring the state to match the server.
        /// </summary>
        private void RestoreSourceComponent()
        {
            System.Diagnostics.Debug.Assert(mInterior != null);

            // TODO: do we not want InitializeEntity script to run? Or is it ok?
            mInterior.AddChild(mSource);

        }

        private void Component_MouseDown(MouseEventArgs args)
        {
            if (mSource == null) return;

            // start delay timer
            mStopwatch = new System.Diagnostics.Stopwatch();
            mStopwatch.Reset(); 
            mStopwatch.Start();

            mMouseDown = true;

            mMouseStart = args.ViewportRelativePosition;
        }


        private void Component_MouseMove(MouseEventArgs args)
        {
            if (mMouseDown == false) return;

            if (mDragging == false && mStopwatch.ElapsedMilliseconds >= mDragBeginIntervalMS)
            {
                mDragging = true;
                mClonedSource = CloneSource();
                RemoveSourceComponent();
            }

            if (mDragging == false) return;
            mMouseEnd = args.ViewportRelativePosition;
            if (mPickResult == null) return;

            Vector3d translation = mPickResult.ImpactPointLocalSpace;
            Quaternion rotation = new Quaternion();

            if (mPickResult.Entity as Interior != null && mPickResult.FaceID >= 0)
            {
                // TODO: if right mouse click dragging, do rotation
                if (mSource.Script == null)
                {
                    //args.Button ==  Keystone.Enums.MOUSE_BUTTONS.RIGHT
                    // TODO: for interior, we should be picking the center of a TILE not a CELL
                    translation = (mPickResult.FacePoints[0] + mPickResult.FacePoints[2]) * .5f;
                }
                else
                {
                    Interior interior = (Interior)mPickResult.Entity;
                    
                    byte cellRotation = 0;
                    //if (ComponentRotation != null) ComponentRotation = Rotations[0];
                    Vector3i cellLocation = mPickResult.CellLocation;

                    // query the interior's script to find placement
                    Vector3d[] vecs = (Vector3d[])mPickResult.Entity.Execute("QueryCellPlacement", new object[] { mSource.ID, interior.ID, mPickResult.ImpactPointLocalSpace, cellRotation });
                    translation = vecs[0];
                    //entity.Scale = vecs[1]; // our current wall entities have super tiny scales, we dont actually want to modify those
                    if (vecs.Length == 3)
                        rotation = new Quaternion(vecs[2].y * MathHelper.DEGREES_TO_RADIANS,
                                                  vecs[2].x * MathHelper.DEGREES_TO_RADIANS,
                                                  vecs[2].z * MathHelper.DEGREES_TO_RADIANS);

                    Keystone.Cameras.Viewport vp = args.Viewport;
                    Quaternion result = Keystone.EditTools.RotationFunctions.Rotate(mSource, vp, mMouseStart, mMouseEnd, AxisFlags.Y, VectorSpace.World);
                    ComponentRotation = rotation = result;
                }
            }

            System.Diagnostics.Debug.WriteLine("InteriorTransformTool.Component_MouseMove() - Dragging rotation = " + ComponentRotation.ToString());
            ComponentTranslation = translation;


        }

        private void Component_MouseUp()
        {
            mMouseDown = false;
            if (mStopwatch != null)
                mStopwatch.Reset();

            if (mClonedSource != null)
            {
                System.Diagnostics.Debug.Assert(mDragging == true);
                Keystone.Resource.Repository.IncrementRef(mClonedSource);
                Keystone.Resource.Repository.DecrementRef(mClonedSource);
                mClonedSource = null;


                System.Diagnostics.Debug.Assert(mSource.Footprint != null);
                int[,] footprint = mSource.Footprint.Data;
                int[,] destFootprint;
                // NOTE: rotation of footprint is performed within the call to IsChildPlaceableWithinInterior
                bool IsValidLocation = mInterior.IsChildPlaceableWithinInterior(footprint, ComponentTranslation, ComponentRotation, out destFootprint);
                if (IsValidLocation)
                {
                    throw new NotImplementedException();
                    // send "Move" command to server.  server must validate placement as well
                    // NOTE: we pass mSource.ID and not mClonedSource since we want the original
                    //       Entity component to be "moved."
                    // TODO: update the Translation and Rotation of the mSource entity?
                    //mSource.Translation = ComponentTranslation;
                    //mSource.Rotation = ComponentRotation;
                    //KeyCommon.Messages.Entity_Move moveMessage = new KeyCommon.Messages.Entity_Move(mSource.ID, mInterior.ID);
                    //Entity tmp = (Entity)Keystone.Resource.Repository.Get(mInterior.ID);
                    //System.Diagnostics.Debug.Assert(tmp != null);
                    //mNetClient.SendMessage(moveMessage);
                }
                else
                {
                    // return original component to it's location so that it matches the server
                    RestoreSourceComponent();
                }


                Keystone.Resource.Repository.DecrementRef(mSource); // restore refcount to = 1
                mDragging = false;
                mSource = null;
            }
        }
    }
}