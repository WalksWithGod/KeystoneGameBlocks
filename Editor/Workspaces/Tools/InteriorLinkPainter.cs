using System;
using System.Collections.Generic;
using Keystone.EditTools;
using Keystone.Entities;
using Keystone.Collision;
using Keystone.Events;
using Keystone.Types;
using Keystone.Portals;
using Keystone.Cameras;
using Keystone.CSG;
using Keystone.Utilities;
using Keystone.Extensions;

namespace KeyEdit.Workspaces.Tools
{

    public class InteriorLinkPainter : InteriorSegmentPainter
    {
        protected ComponentDesignWorkspace mWorkspace;
        public uint[] PreviewTiles; // for HUDs to render previews of operations for visualization aid

        public InteriorLinkPainter(Keystone.Network.NetworkClientBase netClient)
            : base(netClient)
        {

        }

        protected override void MouseDown(Interior celledRegion)
        {
            mStartPickResults = mPickResult;

            Vector3i tileLocation = celledRegion.TileLocationFromPoint(mStartPickResults.ImpactPointLocalSpace);
            uint tileIndex = celledRegion.FlattenTile((uint)tileLocation.X, (uint)tileLocation.Y, (uint)tileLocation.Z);
            

            uint[] tileIndices = celledRegion.GetTileList(tileIndex, tileIndex, true);
            if (tileIndices == null) return;

            PreviewPaintCell(celledRegion.ID, tileIndices);
        }

        protected override void MouseMove(Interior celledRegion)
        {
            // if mouse down == false, then mStartPickResults will be null
            if (mStartPickResults == null) return;


            Vector3i tileLocation = celledRegion.TileLocationFromPoint(mStartPickResults.ImpactPointLocalSpace);
            Vector3i endTileLocation = celledRegion.TileLocationFromPoint(mPickResult.ImpactPointLocalSpace);
            uint tileIndex = celledRegion.FlattenTile((uint)tileLocation.X, (uint)tileLocation.Y, (uint)tileLocation.Z);
            uint endTileIndex = celledRegion.FlattenTile((uint)endTileLocation.X, (uint)endTileLocation.Y, (uint)endTileLocation.Z);

            uint[] tileIndices = celledRegion.GetTileList(tileIndex, endTileIndex, true);
            if (tileIndices == null) return;

            PreviewPaintCell (celledRegion.ID, tileIndices);
        }

        protected override void MouseUp(Interior celledRegion)
        {
        	if (mStartPickResults == null) return;

            // TODO: is pickResult.FaceID holding a tile ID in this case?  very suspect.
            //       we could maybe compute the TileLocation from ImpactPointLocalSpace
            Vector3i tileLocation = celledRegion.TileLocationFromPoint(mStartPickResults.ImpactPointLocalSpace);
            Vector3i endTileLocation = celledRegion.TileLocationFromPoint(mPickResult.ImpactPointLocalSpace);
            uint tileIndex = celledRegion.FlattenTile((uint)tileLocation.X, (uint)tileLocation.Y, (uint) tileLocation.Z);
            uint endTileIndex = celledRegion.FlattenTile((uint)endTileLocation.X, (uint)endTileLocation.Y, (uint)endTileLocation.Z);

            uint[] tileIndices = celledRegion.GetTileList(tileIndex, endTileIndex, true);
            if (tileIndices == null) return;
            // TODO: verify a command gets sent that actually updates permanently, the obstacle footprint array
            PaintCell(celledRegion.ID, tileIndices);
            
            mStartPickResults = null;
            mPrevCells = null; // clear on mouse up
            
            PreviewTiles = null; // clear preview as well
        }
        
             
        /// <summary>
        /// Called By WallSegmentPainter and TileSegmentPainter
        /// </summary>
        /// <param name="celledRegionID"></param>
        /// <param name="cellIndices"></param>
        protected virtual void PreviewPaintCell(string celledRegionID, uint[] cellIndices)
        {
            System.Diagnostics.Debug.Assert(mValue != null, "InteriorLinkPainter.PreviewPaintCell() - Null value not allowed.  A command for deletion of wall or floor should use a default empty SegmentStyle or -1 respectively."); 
            System.Diagnostics.Debug.Assert(cellIndices.Length > 0);

            
            // note: in the case of walls, cellIndices are actually individual edge indices
            // and a single edge index represents an edge defined by 2 verts
            if (cellIndices.Length < 1) return; 
 
            // TODO: we should prune out those areas where a floor can't go
            
            uint[] prunedCells = PruneCells(cellIndices, PreviewTiles);
            if (prunedCells == null || prunedCells.Length == 0) return;

            // TODO: perhaps this can be replaced by an (yet to be designed) "IPreview" object instance?
            PreviewTiles = PreviewTiles.ArrayAppendRange(prunedCells);

            
        }
    }
}
