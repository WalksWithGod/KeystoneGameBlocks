using System;
using System.Collections.Generic;
using Keystone.Cameras;
using Keystone.Entities;
using Keystone.Events;
using Keystone.Extensions;
using Keystone.Portals;

namespace KeyEdit.Workspaces.Tools
{
	/// <summary>
	/// AutoTileSegmentPainter paints segments which auto-tile themselves to ensure
	/// the correct model is in the proper spot given the placement of tiles and their adjacents.
	/// </summary>
	public class AutoTileSegmentPainter : AssetPlacementTool
	{

		protected object mValue;
        public string LayerName;


        public AutoTileSegmentPainter(Keystone.Network.NetworkClientBase netClient, KeyCommon.IO.ResourceDescriptor descriptor)
            : base(netClient, null, Keystone.EditTools.PlacementTool.BRUSH_AUTO_TILE, null)
        {
        	throw new NotImplementedException();
        }

        public AutoTileSegmentPainter(Keystone.Network.NetworkClientBase netClient, string relativeArchivePath, string prefabPathInArchive)
            : base(netClient, relativeArchivePath, prefabPathInArchive, Keystone.EditTools.PlacementTool.BRUSH_AUTO_TILE, null)
        {
        	throw new NotImplementedException();
        }
        
        /// <summary>
        /// The value here is simply a path to the segment entity.  
        /// </summary>
        /// <param name="layerName"></param>
        /// <param name="index"></param>
        public void SetValue(string layerName, string segmentPath)
        {
            LayerName = layerName;
            mValue = segmentPath;
        }

        public string GetValue()
        {
            if (mValue == null) return null;
            return (string)mValue;
        }
        
        public override void HandleEvent(EventType type, EventArgs args)
        {
            MouseEventArgs mouseArgs = (MouseEventArgs)args;

            // keyboard related event
            if (type == EventType.KeyboardCancel)
            {
                return; // TODO:
            }

            // else continue with mouse related event
            MousePosition3D = mouseArgs.UnprojectedCameraSpacePosition;

            // NOTE: normally the pickParamaters used are from mouseArgs.Viewport.RenderingContext.PickParameters, but here
            //       we want this tool to also allow Tile picking per Deck
            KeyCommon.Traversal.PickParameters pickParameters = mouseArgs.Viewport.Context.PickParameters;
            pickParameters.Accuracy |= KeyCommon.Traversal.PickAccuracy.Tile; 
            pickParameters.T0 = AppMain.MINIMUM_PICK_DISTANCE;
            pickParameters.T1 = AppMain.MAXIMUM_PICK_DISTANCE;
            mPickResult = Pick(mouseArgs.Viewport, mouseArgs.ViewportRelativePosition.X, mouseArgs.ViewportRelativePosition.Y, pickParameters);
			
            
            _viewport = mouseArgs.Viewport;
            if (_viewport == null)
            {
            	// TODO: I think viewport is only necessary to start an operation, should not affect one that is in progress
            	//       including MouseUp to end that operatin.
                System.Diagnostics.Debug.WriteLine("AutoTileSegmentPainter.HandleEvent() - No viewport selected");
                return;
            }


            // determine the parentEntity based on pickResult
            // do not just assume it's currentContextRegion
            RenderingContext currentContext = _viewport.Context;
            Entity parentEntity = currentContext.Region;
        	Keystone.TileMap.Structure pickedStructure = null;
        	
            if (mPickResult.Entity != null)
            {
                // NOTE: Currently there are too many problems accidentally placing new entities as children to Background3d entities
                // Lights, Stars, Moons, etc to allow use of the mouse over target.  Maybe if we let the user
                // hold down SHIFT key first so that it's explicit and not accidental.
                //parentEntity = mPickResult.Entity;
                
                // did we pick a structure
		        if (mPickResult.Entity is Keystone.TileMap.Structure)
                	// use impact point instead of mouse pick vector (what about if ALT is pressed?)
                	pickedStructure = (Keystone.TileMap.Structure)mPickResult.Entity;
            }
            // TODO: parentEntity should not change if an operation is in progress
            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            // but as you can see above, that's exactly what im doing... MUST FIX
            if (parentEntity == null) return;


            switch (type)
            {

                case EventType.MouseDown:
                    if (mouseArgs.Button == Keystone.Enums.MOUSE_BUTTONS.XAXIS) // TODO: wtf xasix?  why wont .LEFT work?!! localization testing needed
                    {
                        try
                        {
                        	if (pickedStructure != null)
                            {                           	
                                // NOTE: entity fit is done during command processing when received
                                // by server.  We do not have to worry about this here except perhaps to
                                // minimize commands sent
                                MouseDown(pickedStructure);
                            }
                            else
                                System.Diagnostics.Debug.WriteLine ("AutoTileSegmentPainter.HandleEvent() - Structure parent expected.");


                            // TODO: when are we saving the scene?
                        }
                        catch (Exception ex)
                        {
                            // if the command execute fails we still want to end the input capture
                            System.Diagnostics.Debug.WriteLine("AutoTileSegmentPainter.HandleEvent() - Error -" + ex.Message);
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
                    System.Diagnostics.Debug.WriteLine("AutoTileSegmentPainter.HandleEvent() - MouseEnter...");

                    break;
                case EventType.MouseMove:
                    // if (mStartPickResults == null) return; // don't return immediately because for door placer
                    // for instance, on mousemove we want to be able to compute
                    // potential drop locations even if we don't actually decide on them
                    // but our Hud.cs can use the potential locations to draw preview visual

                    if (pickedStructure != null)
                    {

                        MouseMove(pickedStructure);
                    }
                    else if (mPickResult.Entity == null)
                        return;
                    else
                        return; // throw new Exception("AutoTileSegmentPainter.HandleEvent() - Structure parent expected.");
 
                    //System.Diagnostics.Debug.WriteLine("AutoTileSegmentPainter.HandleEvent() - Mouse Move...");
                    break;

                // TODO: i think input capture is required... that might be part of the issue
                // with propogation of change flag recursion issues.  we'll have to logically
                // work this out but actually i think thats easy once we're ready toclean that up
                case EventType.MouseUp:
                    System.Diagnostics.Debug.WriteLine("AutoTileSegmentPainter.HandleEvent() - Mouse Up.");
                    if (pickedStructure != null)
                    {
                        MouseUp(pickedStructure);
                    }
                    else { }// do nothing.  mouse has slipped off of Structure so ignore.

                    break;

                case EventType.MouseLeave:  // TODO: verify this event occurs when we switch tools
                    System.Diagnostics.Debug.WriteLine("AutoTileSegmentPainter.HandleEvent() - Mouse Leave.");

                    break;

                default:
                    break;
            }
        }

        protected virtual void MouseDown(Keystone.TileMap.Structure structure)
        {
            mStartPickResults = mPickResult;

            // TODO: here our pick result needs to tell us which StructureLevel and MapLayer (MapLayer will be 0 for structural_layout)
            //       and from that mapLayer we can get the .GetTileList since it's map dependant since MapLayers can be of seperate
            //       resolutions even within the same StructureLevel
            //       - FloorLevel - this should match the level we are attempting to paint on
            //       - LayerName ("terrain_layout")
            int floorLevel = 1; // mPickResult.TileLocation.Y
            string layerName = "layout";
            uint[] tileIndices = structure.GetTileList(layerName, mStartPickResults.TileLocation, mPickResult.TileLocation);
            if (tileIndices == null) return;

            PaintTiles(structure.ID, tileIndices);
        }

        protected virtual void MouseMove(Keystone.TileMap.Structure structure)
        {
            // if mouse down == false, then mStartPickResults will be null
            if (mStartPickResults == null) return;

            int floorLevel = 1; // mPickResult.TileLocation.Y
            string layerName = "layout";
            
            uint[] tileIndices = structure.GetTileList(layerName, mStartPickResults.TileLocation, mPickResult.TileLocation);
            if (tileIndices == null) return;

            PaintTiles(structure.ID, tileIndices);
        }


        protected virtual void MouseUp(Keystone.TileMap.Structure structure)
        {
            mStartPickResults = null;
            mPrevTiles = null; // clear on mouse up
        }

        /// <summary>
        /// Find subset array of indices within tileIndices that do not already exist previously
        /// </summary>
        /// <param name="cellIndices"></param>
        /// <returns></returns>
        protected uint[] PruneTiles(uint[]newIndices, uint[] previousIndices)
        {
            List<uint> temp = new List<uint>();
 
            //prune those cells that have already been painted previously
            for (int i = 0; i < newIndices.Length; i++)
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
        protected virtual void PaintTiles(string structureID, uint[] tileIndices)
        {
            System.Diagnostics.Debug.Assert(mValue != null, "AutoTileSegmentPainter.PaintTiles() - Null value not allowed.  A command for deletion of wall or floor should use a default empty SegmentStyle or -1 respectively."); 
            System.Diagnostics.Debug.Assert(tileIndices.Length > 0);

            
            // note: in the case of walls, tileIndices are actually individual edge indices
            // and a single edge index represents an edge defined by 2 verts
            if (tileIndices.Length < 1) return; 
 
            // TODO: we should prune out those areas where a floor can't go
            // this is mainly useful for when changing just the texture, color or mesh but not
            // changing whether there is a floor tile or not already there.
            // I mean, i have some rules perhaps regarding frame strength and how many tiles can
            // span before a pillar or brace or wall or bulkhead is encountered.  But that could be
            // future stuff.
            uint[] prunedTiles = PruneTiles(tileIndices, mPrevTiles);
            if (prunedTiles == null || prunedTiles.Length == 0) return;

            mPrevTiles = mPrevTiles.ArrayAppendRange(prunedTiles);

            //for (int i = 0; i < temp.Count; i++)
            //    System.Diagnostics.Debug.WriteLine("cell[" + i.ToString() + "] = " + temp[i].ToString());

            // LayerNames will indicate if we are painting "tile style" or "wall style"
            // TODO: August.27.2017 - need to add new case for TileMapStructure.PaintCell()
            KeyCommon.Messages.MessageBase mNetworkMessage = new KeyCommon.Messages.PaintCellOperation();
            ((KeyCommon.Messages.PaintCellOperation)mNetworkMessage).Indices = prunedTiles;
            ((KeyCommon.Messages.PaintCellOperation)mNetworkMessage).LayerName = LayerName;
            ((KeyCommon.Messages.PaintCellOperation)mNetworkMessage).PaintValue = mValue;
            ((KeyCommon.Messages.PaintCellOperation)mNetworkMessage).ParentCelledRegionID = structureID;

            mNetClient.SendMessage(mNetworkMessage);
        }
	}
}
