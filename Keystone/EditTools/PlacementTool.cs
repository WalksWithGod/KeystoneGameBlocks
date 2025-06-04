using System;
using Keystone.Cameras;
using Keystone.Collision;
using Keystone.Elements;
using Keystone.Entities;
using Keystone.Events;
using Keystone.Types;
using Keystone.Appearance;
using System.Collections.Generic;
using Keystone.CSG;
using Keystone.Portals;

namespace Keystone.EditTools
{
    public class PlacementTool : TransformTool
    {
        #region brush style constants - These MUST match enum in scripts\UserConstants.cs
        public const uint BRUSH_SINGLE_DROP = 0;
        public const uint BRUSH_FLOOR_TILE = 1;
        public const uint BRUSH_CEILING = 2;
        public const uint BRUSH_EDGE_SEGMENT = 3;  // rubber band drag for wall, fence, railing
        public const uint BRUSH_EDGE_SINGLE_SEGMENT = 4; // doors, windows
        public const uint BRUSH_CUSTOM = 5;  // shrubs,trees,grass,rocks
        public const uint BRUSH_CSG_MESH = 6;
        public const uint BRUSH_TERRAIN_SEGMENT = 7; // a single auto-tile terran element. (not an entire tvlandscape)
        public const uint BRUSH_TERRAIN_VEG_PAINT = 8;
        public const uint BRUSH_HATCH = 9;
        public const uint BRUSH_DOOR = 10;
        public const uint BRUSH_AUTO_TILE = 11;
        public const uint BRUSH_EXTERIOR_STRUCTURE = 12; // exterior minecraft style terrain as opposed to interior tilemap
        public const uint BRUSH_TERRAIN = 13; // tvlandscape 
        public const uint BRUSH_WATER = 14;
        public const uint BRUSH_FLOOR_MESH = 15; // mesh created with Mesh3d.CreateFloor()
        public const uint BRUSH_STAIRS = 16; // should all components that must snap to center of Cell use a single BRUSH_TYPE? so hatch, stairs, and others all use BRUSH_CELL_CENTERED_OBJECT? (NOTE: ladders snap too, but the ladder Model is offset so that it is close to the cell edge.
        public const uint BRUSH_LADDER = 17;
        public const uint BRUSH_LIFT = 18; // elevator
        #endregion

        public uint _brushStyle = BRUSH_SINGLE_DROP;
        protected string mSourceEntry;


        public PlacementTool(Keystone.Network.NetworkClientBase netClient) : base(netClient)
        {
             ComponentRotation = new Quaternion();
        }


        public string SourceEntry { get { return mSourceEntry; } }

        public uint BrushStyle {get {return _brushStyle;}}


        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();


            if (mSource != null)
            {   Keystone.Resource.Repository.IncrementRef(mSource);
                Keystone.Resource.Repository.DecrementRef(mSource);
            }
        }
    }

    
}
