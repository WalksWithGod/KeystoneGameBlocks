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
    /// </summary>
    public class HeightmapTool : Tool 
    {
        protected object mValue;
        public string LayerName;
        protected uint[] mPrevCells;

        protected ModeledEntity mCurrentTarget;

        
        public HeightmapTool(Keystone.Network.NetworkClientBase netClient)
            : base(netClient)
        {
        }

        public void SetValue(string layerName, bool value)
        {
            LayerName = layerName;
            mValue = value;
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

            // NOTE: the pickParamaters used are from mouseArgs.Viewport.RenderingContext.PickParameters
            mPickResult = Pick(mouseArgs.Viewport, mouseArgs.ViewportRelativePosition.X, mouseArgs.ViewportRelativePosition.Y);

            _viewport = mouseArgs.Viewport;

            
            
            switch (type)
            {

                case EventType.MouseDown:
                    if (mouseArgs.Button == Keystone.Enums.MOUSE_BUTTONS.XAXIS) // TODO: wtf xasix?  why wont .LEFT work?!! localization testing needed
                    {
                        try
                        {
                        	if (ValidateMouse())
                        		MouseDown((ModeledEntity)mPickResult.Entity);

                            // TODO: when are we saving the scene?
                        }
                        catch (Exception ex)
                        {
                            // if the command execute fails we still want to end the input capture
                            System.Diagnostics.Debug.WriteLine("HeightmapTool.HandleEvent() - Error -" + ex.Message);

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
                    System.Diagnostics.Debug.WriteLine("HeightmapTool.HandleEvent() - MouseEnter...");

                    break;
                case EventType.MouseMove:
                    // if (mStartPickResults == null) return; // don't return immediately because for door placer
                    // for instance, on mousemove we want to be able to compute
                    // potential drop locations even if we don't actually decide on them
                    // but our Hud.cs can use the potential locations to draw preview visual

                        MouseMove(mPickResult.Entity);
                        // throw new Exception("HeightmapTool.HandleEvent() - CelledRegion parent expected.");
 
                    //System.Diagnostics.Debug.WriteLine("HeightmapTool.HandleEvent() - Mouse Move...");
                    break;

                // TODO: i think input capture is required... that might be part of the issue
                // with propogation of change flag recursion issues.  we'll have to logically
                // work this out but actually i think thats easy once we're ready toclean that up
                case EventType.MouseUp:
                    System.Diagnostics.Debug.WriteLine("HeightmapTool.HandleEvent() - Mouse Up.");
                    MouseUp(mPickResult.Entity);
                    break;

                case EventType.MouseLeave:  // TODO: verify this event occurs when we switch tools
                    System.Diagnostics.Debug.WriteLine("HeightmapTool.HandleEvent() - Mouse Leave.");

                    break;

                default:
                    break;
            }
        }

        private bool ValidateMouse ()
        {
            if (_viewport == null)
            {
            	// NOTE: Viewport is only necessary to start an operation, should not affect one that is in progress
            	//       including MouseUp to end that operatin.
                System.Diagnostics.Debug.WriteLine("HeightmapTool.ValidateMouse() - No viewport selected");
                return false;
            }          

            if (mPickResult.Entity == null) return false;
            
            Entity pickedEntity = mPickResult.Entity;
            
            if (pickedEntity is ModeledEntity == false) return false;
            ModeledEntity targetEntity = (ModeledEntity)pickedEntity;
	
            if (targetEntity.Model == null || targetEntity.Model.Geometry == null) return false;
            
            return true;
            
        }
        
        protected virtual void MouseDown(ModeledEntity target)
        {
            mStartPickResults = mPickResult;
            mCurrentTarget = target;
            
            Keystone.Elements.Terrain t = (Keystone.Elements.Terrain)target.Model.Geometry;
            
            int[,] mask = new int[,]
            {
            	{1,1},
            	{1,1}
            };
            
            // convert world pick location to modelspace
            Vector3d modelSpacePosition = mStartPickResults.ImpactPointLocalSpace;
            Keystone.Types.Vector3d[] vertices = t.GetVertices (modelSpacePosition, mask);
            
            // now let's raise those same vertices by +1.0f
            for (int i  = 0; i < vertices.Length; i++)
            {
            	vertices[i].y += 8.0f;
            }

            AdjustVertices(t.ID, vertices);
        }

        protected virtual void MouseMove(Entity target)
        {
        	// TODO: using shader to draw terrain brush
        	// http://www.gamedev.net/topic/509174-creating-a-terrain-editor-vertex-brush/
        	// - problem is, what if the brush shape changes?  Easy to do a sphere, but harder to
        	//   do a specific shape.  I suppose the brush texture would be used...  hrm... then any texture
        	//   can easily be used.  Well.. at least.. if not texture then a matrix representing the layout
        	//   of the "bristles"
        	
        	return;
            // if mouse down == false, then mStartPickResults will be null
            if (mCurrentTarget == null && mStartPickResults == null) return;

            // TODO: unless, can the operation span over to an adjacent terrain? 
            // i think for benefit of networked messaging, two seperate messages should be constructed
            // But neither have to be sent until operation is completed... but the vertex changes get tracked
            // independantly for the terrains that are affected.  
            
            if (target != mCurrentTarget) return;
            
            // TODO: here we have a concept of a "floor" mesh what about a "Floor" entity?  
            //       Yes, we do have models that are apart of the interior, but what if rather than
            //       make them as if we're picking the interior CelledRegion, instead we're picking a "Floor"
            //       which is an Entity that contains visuals for it's floor, walls and ceiling?
            
            //      This can be used in exteriors too.  Allowing us to have top side terrains, underground areas, etc.
            //      The main question though is, how do we connect seperate floors together?  They still have to relate to
            //      the parent region's cell/tile structure.
            //      - I guess really, the "Interior" (internalStructure) entity is something we could add to any Region
            //      to provide a grid structure that is comprised of floor/ceiling segments, wall segments and components
            //      all of which can modify an underlying tileMask
            
            //     So I don't know yet... i mean, our normal zones dont have an exterior mesh representation like a vehicle.
            //     The "Interior" structure is really just a TileStructure interior.
            // 
            //    When we look at something like The Elder Scrolls Construction Set, we see it's basically pages (they call cells)
            //    where you place entities freely.  There is no underlying structure that is finer in resolution than these pages/cells.
            //
            //    Terrain modifying should be more or less unrestricted.   We can vary the vertices used in a given region/zone but
            //    otherwise we must be able to contour mountains, hills, shorelines, rivers freely or it just wont look right.
            //    Similarly, trees and objects should be placeable anywehre so long as they don't overlap inappropriately.  
            //    To do that, it's a question of do we still allow enforcement of a type of "tilemask."  I want to think it should be ok since
            //    again, we can page that in/out and there are times where it's only needed for AI and if we are not doing any AI planning
            //    then at run-time the tilemask does not need to be shown (eg when doing overhead view or when viewing far distance areas)
            //    There's nothing wrong conceptually with a tilemask as far as limiting what can go where. It's an elegant way for pathing,
            //    it's elegant for enforcing placement restrictions.  So I think perhaps we should start and just have a Zone with a 
            //    floor Entity (using terrain or mesh3d geometry) that can directly accept the heightmap tool's changes.
            //    We'll worry about the structure later.
            //
            //    So, we want our Structure for a Region.  Is it an Entity that sits in Region or is it part of the Region as in CelledRegion?
            //    
            //	  Also keep in mind, we can have one floor and we can have multiple floors if we want overhangs or tunnels.  
            //    So we can compute exactly how many tiles we need based on our resolution.... 
            //    - some of the tile data perhaps can be stored in a texture that can lookup...  
            //
            //
            //  TILE DATA LAYERS - with the exception of entity stacks, we sort of screwed up a bit with our tilemask data layers
            //                What i should have done is treat it more like image raw data where i can define the width/height/depth
            //                and also the bits per tile.  This way we can have masks for power, fuel links, collision, where some of those overlays
            //                are at different resolutions, but can still be projected onto entire floor
            //                - so perhaps here we have a  chance to do things better
            //  				- 
            //  AppearanceGroup - Layers <-- so why not DataLayers?  Which go under Entity.Data.Layers[] ?
            //  ILayer { Width, Height, BitsPerTile} 
            
            // based on brush radius or brush mask, get the vertices that are affected
  //          uint[] cellIndices = celledRegion.GetVertexList((uint)mStartPickResults.FaceID, (uint)mPickResult.FaceID);
  //          if (cellIndices == null) return;

  //          AdjustVertices(celledRegion.ID, cellIndices);
        }


        protected virtual void MouseUp(Entity target)
        {
        	// if during mouse up we were still over the target in previos operation, we can include
        	// the latest mouse position to affect the terrain.
        	// if the target is different, we'll just end the operation as is.
        	
        	if (target is ModeledEntity)
        	{
        		if (mCurrentTarget == target)
        		{
        		}
        	}
        	
        	return;
            ModeledEntity targetEntity = (ModeledEntity)target;
	
            if (targetEntity.Model == null || targetEntity.Model.Geometry == null) return;
            
            
            mStartPickResults = null;
            mPrevCells = null; // clear on mouse up
            mCurrentTarget = null;
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
        protected virtual void AdjustVertices(string terrainID, Keystone.Types.Vector3d[] vertices)
        {
        	// mValue == RaiseTerrain
        	// mValue == LowerTerrain
            //System.Diagnostics.Debug.Assert(mValue != null, "HeightmapTool.AdjustVertices() - Null value not allowed."); 
            System.Diagnostics.Debug.Assert(vertices.Length > 0);

            if (vertices.Length < 1) return; 
 
            // TODO: we should prune out those areas where a floor can't go
            // this is mainly useful for when changing just the texture, color or mesh but not
            // changing whether there is a floor tile or not already there.
            // I mean, i have some rules perhaps regarding frame strength and how many tiles can
            // span before a pillar or brace or wall or bulkhead is encountered.  But that could be
            // future stuff.
//            uint[] prunedCells = PruneCells(cellIndices, mPrevCells);
//            if (prunedCells == null || prunedCells.Length == 0) return;
//
//            mPrevCells = mPrevCells.ArrayAppendRange(prunedCells);


            KeyCommon.Messages.MessageBase mNetworkMessage = new KeyCommon.Messages.Terrain_Paint();
            ((KeyCommon.Messages.Terrain_Paint)mNetworkMessage).Vertices = vertices;
            ((KeyCommon.Messages.Terrain_Paint)mNetworkMessage).TerrainID = terrainID;

            mNetClient.SendMessage(mNetworkMessage);
        }
    }
}
