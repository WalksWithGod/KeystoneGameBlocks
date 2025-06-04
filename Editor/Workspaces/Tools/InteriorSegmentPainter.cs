using System;
using System.Collections.Generic;
using Keystone.EditTools;
using Keystone.Entities;
using Keystone.Collision;
using Keystone.Events;
using Keystone.Types;
using Keystone.Portals;
using Keystone.Cameras;
using Keystone.Utilities;
using Keystone.Extensions;

namespace KeyEdit.Workspaces.Tools
{
    /// <summary>
    /// Inherited by TileSegmentPainter, WallSegmentPainter, and LinkPainter tools
    /// </summary>
    public class InteriorSegmentPainter : Tool 
    {
        protected object mValue;
        public string LayerName;
        protected uint[] mPrevCells;
        private Keystone.Elements.Mesh3d mGridMesh;

        public InteriorSegmentPainter(Keystone.Network.NetworkClientBase netClient)
            : base(netClient)
        {
            //LoadVisuals();
        }

        private void LoadVisuals(Interior celledRegion)
        {
            Keystone.Appearance.Appearance appearance = Keystone.Celestial.ProceduralHelper.CreateAppearance(MTV3D65.CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED, null, null, null, null, null, true);
            appearance.Material.Ambient = Keystone.Types.Color.Green;
            appearance.Material.Opacity = 0.5f;

            bool reverseWindingOrder = false;
            string meshID = "HUD PREVIEW";
            mGridMesh = Keystone.Elements.Mesh3d.CreateCellGrid((float)celledRegion.CellSize.x, (float)celledRegion.CellSize.z,
                celledRegion.CellCountX,
                celledRegion.CellCountZ,
                1.0f, reverseWindingOrder, meshID); ;
            Keystone.Elements.Model model = (Keystone.Elements.Model)Keystone.Resource.Repository.Create("Model");

            // TODO: do we need a texture as we use for OOB grid?
            model.AddChild(appearance);
            model.AddChild(mGridMesh);

            Keystone.Entities.ModeledEntity gridEntity = (Keystone.Entities.ModeledEntity)Keystone.Resource.Repository.Create("ModeledEntity");
            gridEntity.AddChild(model);

            Keystone.IO.ClientPager.LoadTVResource(gridEntity, true);
            mSource = gridEntity;
            
        }

        private void UpdateVisuals(Interior celledRegion, uint[] cellIndices)
        {
            if (cellIndices == null) return;

            uint currentFloor = (uint)mPickResult.CellLocation.Y;
            double floorHeight = celledRegion.GetFloorHeight(currentFloor);

            // TODO: Shouldn't we use the original grid placement grid to show selected cells? just use a different part of the texture
            // grid is always at x,z origin but at cellLocation.y floor height
            mSource.Translation = new Vector3d(0, floorHeight, 0);

            CollapseAll(celledRegion.CellCountX, celledRegion.CellCountZ, mGridMesh);

            // collapse all cells that are not cointained within the cellIndices.  UnCollapse the ones that are.
            for (int i = 0; i < cellIndices.Length; i++)
            {
                uint x,y,z;
                celledRegion.UnflattenCellIndex(cellIndices[i], out x, out y, out z);
                Keystone.Celestial.ProceduralHelper.CellGrid_IsCellCollapsed(celledRegion.CellCountX, celledRegion.CellCountZ, (uint)x, (uint)z, mGridMesh);
                // Keystone.Celestial.ProceduralHelper.CellGrid_IsCellCollapsed(celledRegion.CellCountX, celledRegion.CellCountZ, cellIndices[i], mGridMesh);


                // todo: create overloaded method that takes in the cellIndex and not just x,z location
                bool collapse = false;
                //Keystone.Celestial.ProceduralHelper.CellGrid_CellSetCollapseState(celledRegion.CellCountX, celledRegion.CellCountZ, cellIndices[i], mGridMesh, collapse);
                Keystone.Celestial.ProceduralHelper.CellGrid_CellSetCollapseState(celledRegion.CellCountX, celledRegion.CellCountZ, x, z, mGridMesh, collapse);

            }

        }

        private void CollapseAll(uint cellCountX, uint cellCountZ, Keystone.Elements.Mesh3d mesh)
        {
            int cellCount = (int)(cellCountX * cellCountZ);

            for (int i = 0; i < cellCount; i++)
            {
                int x, y, z;
                Keystone.Utilities.MathHelper.UnflattenIndex((uint)i, cellCountX, out x, out z);
                bool collapse = true;
                Keystone.Celestial.ProceduralHelper.CellGrid_CellSetCollapseState(cellCountX, cellCountZ, (uint)x, (uint)z, mGridMesh, collapse);
            }

        }

        public void SetValue(string layerName, bool value)
        {
            LayerName = layerName;
            mValue = value;
        }

        public override void HandleEvent(EventType type, EventArgs args)
        {

            
            // keyboard related event
            if (type == EventType.KeyboardCancel)
            {
                return; // TODO:
            }

            KeyboardEventArgs keyArgs = args as KeyboardEventArgs;
            if (keyArgs != null)
            {
                switch ((keyArgs.Key))
                {
                    case "Escape":
                        MouseUp(null);
                        // this._viewport.Context.Workspace.ToolCancel(); // we don't want to switch tools in this case, we just want to cancel the segment placement because we might want to start another somewhere else
                        return;
                        break;
                    default:
                        return;
                }
            }
            MouseEventArgs mouseArgs = (MouseEventArgs)args;
            // else continue with mouse related event
            MousePosition3D = mouseArgs.UnprojectedCameraSpacePosition;

           // System.Diagnostics.Debug.WriteLine("Picking floor level " + mouseArgs.Viewport.Context.Floor);
            // NOTE: the pickParamaters used are from mouseArgs.Viewport.RenderingContext.PickParameters
            mPickResult = Pick(mouseArgs.Viewport, mouseArgs.ViewportRelativePosition.X, mouseArgs.ViewportRelativePosition.Y);

            _viewport = mouseArgs.Viewport;
            if (_viewport == null)
            {
            	// TODO: I think viewport is only necessary to start an operation, should not affect one that is in progress
            	//       including MouseUp to end that operatin.
                System.Diagnostics.Debug.WriteLine("InteriorSegmentPainter.HandleEvent() - No viewport selected");
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



            switch (type)
            {
                case EventType.MouseDown:
                    if (mouseArgs.Button == Keystone.Enums.MOUSE_BUTTONS.LEFT) // TODO: this was using XAXIS but changing it to .LEFT fixees it.  Why wasn't it LEFT all along?  localization testing needed
                    {
                        try
                        {
                            if (mPickResult.Entity as Interior != null)
                            {
                                // NOTE: entity fit is done during command processing when received
                                // by server.  We do not have to worry about this here except perhaps to
                                // minimize commands sent
                                MouseDown((Interior)mPickResult.Entity);
                            }
                            else
                                System.Diagnostics.Debug.WriteLine ("InteriorSegmentPainter.HandleEvent() - CelledRegion parent expected.");


                            // TODO: when are we saving the scene?
                        }
                        catch (Exception ex)
                        {
                            // if the command execute fails we still want to end the input capture
                            System.Diagnostics.Debug.WriteLine("InteriorSegmentPainter.HandleEvent() - Error -" + ex.Message);

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
                    System.Diagnostics.Debug.WriteLine("InteriorSegmentPainter.HandleEvent() - MouseEnter...");

                    break;
                case EventType.MouseMove:
                    // if (mStartPickResults == null) return; // don't return immediately because for door placer
                    // for instance, on mousemove we want to be able to compute
                    // potential drop locations even if we don't actually decide on them
                    // but our Hud.cs can use the potential locations to draw preview visual

                    if (mPickResult.Entity as Interior != null)
                    {
                        MouseMove((Interior)mPickResult.Entity);
                    }
                    else if (mPickResult.Entity == null)
                        return;
                    else
                        return; // throw new Exception("CellPainter.HandleEvent() - CelledRegion parent expected.");
 
                    //System.Diagnostics.Debug.WriteLine("PlacementTool.HandleEvent() - Mouse Move...");
                    break;

                // TODO: i think input capture is required... that might be part of the issue
                // with propogation of change flag recursion issues.  we'll have to logically
                // work this out but actually i think thats easy once we're ready toclean that up
                case EventType.MouseUp:
                    System.Diagnostics.Debug.WriteLine("InteriorSegmentPainter.HandleEvent() - Mouse Up.");
                    if (mPickResult.Entity as Interior != null)
                    {
                        MouseUp((Interior)mPickResult.Entity);
                    }
                    else { }// do nothing.  mouse has slipped off of CelledRegion so ignore.

                    break;

                case EventType.MouseLeave:  // TODO: verify this event occurs when we switch tools
                    System.Diagnostics.Debug.WriteLine("InteriorSegmentPainter.HandleEvent() - Mouse Leave.");

                    break;

                default:
                    break;
            }
        }

        protected virtual void MouseDown(Interior celledRegion)
        {
            mStartPickResults = mPickResult;
            System.Diagnostics.Debug.Assert(celledRegion == mStartPickResults.Entity);

            uint[] cellIndices = celledRegion.GetCellList((uint)mStartPickResults.FaceID, (uint)mPickResult.FaceID);
            if (cellIndices == null) return;

            if (mSource == null || !mSource.TVResourceIsLoaded)
                LoadVisuals(celledRegion);



           // PaintCell(celledRegion.ID, cellIndices);
        }

        protected virtual void MouseMove(Interior celledRegion)
        {
            if (mStartPickResults == null) return;


            uint[] cellIndices = celledRegion.GetCellList((uint)mStartPickResults.FaceID, (uint)mPickResult.FaceID);

            UpdateVisuals(celledRegion, cellIndices);

            if (cellIndices == null) return;
            mPrevCells = cellIndices;
        }


        protected virtual void MouseUp(Interior celledRegion)
        {

            // if mouse down == false, then mStartPickResults will be null
            if (mStartPickResults != null)
            {
                uint[] cellIndices = celledRegion.GetCellList((uint)mStartPickResults.FaceID, (uint)mPickResult.FaceID);
                if (cellIndices == null) return;
                mPrevCells = cellIndices;
                PaintCell(celledRegion.ID, cellIndices);
            }

            mStartPickResults = null;
            mPrevCells = null; // clear on mouse up
        }

        /// <summary>
        /// Find subset array of indices within cellIndices that do not already exist previously
        /// </summary>
        /// <param name="cellIndices"></param>
        /// <returns></returns>
        protected uint[] PruneCells(uint[]newIndices, uint[] previousIndices)
        {
            List<uint> temp = new List<uint>();
 
            //prune those cells that have already been painted previously
            for (int i = 0; i < newIndices.Length; i++)
                // if (mPrevCells.ArrayContains(u => u == cellIndices[i]) == false)
                if (previousIndices.ArrayContains(newIndices[i]) == false)
                    temp.Add(newIndices[i]);

            if (temp.Count == 0) return null;
            return temp.ToArray();
        }

        /// <summary>
        /// Called By WallSegmentPainter and TileSegmentPainter
        /// </summary>
        /// <param name="celledRegionID"></param>
        /// <param name="cellIndices"></param>
        protected virtual void PaintCell(string celledRegionID, uint[] cellIndices)
        {
            System.Diagnostics.Debug.Assert(mValue != null, "Null value not allowed.  A command for deletion of wall or floor should use a default empty SegmentStyle or -1 respectively."); 
            System.Diagnostics.Debug.Assert(cellIndices.Length > 0);

            
            // note: in the case of walls, cellIndices are actually individual edge indices
            // and a single edge index represents an edge defined by 2 verts
            if (cellIndices.Length < 1) return; 
 
            // NOTE: Pruning cells is no longer required since we only update edges or tiles on MouseUp() and not MouseMove()
            // TODO: we should prune out those areas where a floor can't go
            // this is mainly useful for when changing just the texture, color or mesh but not
            // changing whether there is a floor tile or not already there.
            // I mean, i have some rules perhaps regarding frame strength and how many tiles can
            // span before a pillar or brace or wall or bulkhead is encountered.  But that could be
            // future stuff.
         //   uint[] prunedCells = PruneCells(cellIndices, mPrevCells);
         //   if (prunedCells == null || prunedCells.Length == 0) return;

         //   mPrevCells = mPrevCells.ArrayAppendRange(prunedCells);

            //for (int i = 0; i < temp.Count; i++)
            //    System.Diagnostics.Debug.WriteLine("cell[" + i.ToString() + "] = " + temp[i].ToString());

            // LayerNames will indicate if we are painting "tile style" or "wall style"
            // TODO: August.27.2017 - need to add new case for TileMapStructure.PaintCell()
            KeyCommon.Messages.MessageBase mNetworkMessage = new KeyCommon.Messages.PaintCellOperation();
            ((KeyCommon.Messages.PaintCellOperation)mNetworkMessage).Indices = mPrevCells; // prunedCells;
            ((KeyCommon.Messages.PaintCellOperation)mNetworkMessage).LayerName = LayerName;
            ((KeyCommon.Messages.PaintCellOperation)mNetworkMessage).PaintValue = mValue;
            ((KeyCommon.Messages.PaintCellOperation)mNetworkMessage).ParentCelledRegionID = celledRegionID;

            mNetClient.SendMessage(mNetworkMessage);
        }

        internal uint[] Cells { get { return mPrevCells; } } 
    }
}
