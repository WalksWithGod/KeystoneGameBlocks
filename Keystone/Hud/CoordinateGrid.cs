using System;
using Keystone.Types;
using Keystone.Immediate_2D;

namespace Keystone.Hud
{
    /// <summary>
    /// Helper class for drawing a 2d grid in the world using lines
    /// It is a fair question i think, why have nested grids when I could just use
    /// two seperate CoordinateGrid instances?  That makes it easier to fade grids
    /// when transitioning between grid scales.
    /// </summary>
    public class CoordinateGrid
    {
        public bool UseFarFrustum;
        public bool AutoScale;
        public float ColumnSpacing; // X axis
        public float RowSpacing;    // Z axis

        public uint OuterRowCount;
        public uint InnerRowCount;       // the spacing betweeen the inner rows and columns is computed automatically based on the inner row and inner column counts and outer row and outer column spacing
        public uint OuterColumnCount;
        public uint InnerColumnCount;
        public uint OuterLineThickness;
        public uint InnerLineThickness;

        public bool Enable; // false by default
        public bool DrawAxis;
        public bool DrawInnerRows;
        public bool InfiniteGrid;

        public Color InnerColor;
        public Color OuterColor;
        public Vector3d Position; // offset to draw the grid
        private Vector3d mLastPosition; 

        public CoordinateGrid(float rowSpacing, float columnSpacing)
        {
            RowSpacing = rowSpacing;
            ColumnSpacing = columnSpacing;

            OuterRowCount = 200;
            InnerRowCount = 3;
            OuterColumnCount = 200;
            InnerColumnCount = 3;

            OuterLineThickness = 1;
            InnerLineThickness = 1;

            Enable = false;
            DrawAxis = true;
            DrawInnerRows = true;
            InfiniteGrid = true;

            InnerColor =  new Color(0, 51, 102, 255);
            OuterColor = new Color(204, 204, 204,255);
            Position = new Vector3d();
        }


        /// <summary>
        /// Computes the actual grid line coordinates based on camera space position and
        /// whether InfinteGrid is enabled
        /// </summary>
        /// <param name="cameraSpacePosition">The position of the grid in camera space.  Typically this is equal to -context.Position</param>
        public Renderable3DLines[] Update(Vector3d cameraSpacePosition, bool drawAxis)
        {
            if (Enable == false) return null;

            Vector3d gridPosition = cameraSpacePosition;
            
            float scale = 1.0f;

            // TODO: is a bit buggy
            // TODO: it also seems to be showing that far from origin, camera has difficulties translating
            // diagonally.
            // if infinite grid, the grid position will appear to move at x,z with camera and NOT be fixed
            // at origin except at y axis.  However, it should be noted that the grid is never more than 
            // ColumnSpacing x RowSpacing distance away from the camera's position!  So this will work no matter how far
            // from the origin we are
            if (InfiniteGrid)
            {
                // https://godotshaders.com/shader/infinite-ground-grid/
                gridPosition.x = Math.IEEERemainder(gridPosition.x, ColumnSpacing);
                gridPosition.z = Math.IEEERemainder(gridPosition.z, RowSpacing);

            }

            if (AutoScale) // NOTE: AutoScale will also automatically set UseFarFrustum as needed
            {
                // scale in whole units every power of 2 starting at 2^16
                Vector3d diff = gridPosition - mLastPosition;
                double height = Math.Abs(gridPosition.y); // we only want height above distance, not camera distance to origin
                if (height > 90000) 
                	UseFarFrustum = true; 
                else UseFarFrustum = false;

                double distance = Math.Max(height, diff.Length);
                double log = Math.Log(distance, 2);
                log = Math.Max(1.0, log);
                log = Math.Truncate(log);
                //if (log > 16) log -= 16;
                scale = (float)Math.Pow(2, log);
            }

            gridPosition.x *= scale;
            gridPosition.z *= scale;
            //scale = 1.0f;
            mLastPosition = gridPosition;
            return GeneratedLines(OuterRowCount, OuterColumnCount, 
                                  RowSpacing * scale, 
                                  ColumnSpacing * scale, 
                                  InnerRowCount, InnerColumnCount, scale, 
                                  OuterColor, InnerColor, gridPosition);
        }

      
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rows">number of rows in grid</param>
        /// <param name="columns">number of columns in grid</param>
        /// <param name="spacing">spacing between primary grid cells</param>
        /// <param name="color"></param>
        /// <param name="offset">World position offset by which to draw the grid</param>
        /// <param name="drawAxis"></param>
        private Renderable3DLines[] GeneratedLines(uint rows, uint columns, 
                                                   float rowSpacing, float columnSpacing,
                                                   uint innerRows, uint innerColumns, float scale, 
                                                   Color outerColor, Color innerColor, 
                                                   Vector3d offset)
        {
            if (rows == 0 || columns == 0 || rowSpacing <= 0.0 || columnSpacing <= 0.0) return null;

            Line3d[] linesArray;

            float height = (float)offset.y; // height above origin to draw this grid

            // the small inner columns and rows. These can be 0 x 0 count or "DrawInnerRows" set to false
            Renderable3DLines innerLines = null;
            if (DrawInnerRows && InnerRowCount > 0 && InnerColumnCount > 0)
            {
                float innerRowSpacing = rowSpacing / innerRows;
                float innerColumnSpacing = columnSpacing / innerColumns;
          
                linesArray = CreateGrid(rows * InnerRowCount, columns * InnerColumnCount, innerRowSpacing, innerColumnSpacing, (float)offset.x, (float)offset.y, (float)offset.z);
                innerLines = new Renderable3DLines(linesArray, innerColor);
            }

            // create the larger outer columns and rows
            linesArray = CreateGrid(rows, columns, rowSpacing, columnSpacing, (float)offset.x, (float)offset.y, (float)offset.z);
            Renderable3DLines outerLines = new Renderable3DLines(linesArray, outerColor);

            // draw the red, green, blue axis lines across the center
            Renderable3DLines[] axisLines = null;
            if (DrawAxis)
            {
                if (columns > rows)
                    axisLines = AxisIndicator.Update(offset, columnSpacing, columns / 2f);
                else
                    axisLines = AxisIndicator.Update(offset, rowSpacing, rows / 2f);
            }

            if (axisLines != null && innerLines != null)
                return new Renderable3DLines[] { innerLines, outerLines, axisLines[0], axisLines[2]}; // skip green line, axisLines[1]};
            else if (axisLines != null)
                    return new Renderable3DLines[] {outerLines, axisLines[0], axisLines[2] }; // skip green line, axisLines[1]};
            else if (innerLines != null)
                return new Renderable3DLines[] { innerLines, outerLines };
            else
                return new Renderable3DLines[] { outerLines };
        }

        /// <summary>
        /// Creates a grid that is centered at the origin.  We can transform this grid later.
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="columns"></param>
        /// <param name="rowSpacing"></param>
        /// <param name="columnSpacing"></param>
        /// <returns></returns>
        private Line3d[] CreateGrid(uint rows, uint columns, float rowSpacing, float columnSpacing, float offsetX, float offsetY, float offsetZ)
        {
            int capacity = (int)(rows + columns) + 2; // plus to for one extra line at end of colums and end of rows to close the grid
            System.Collections.Generic.List<Line3d> lines = new System.Collections.Generic.List<Line3d>(capacity);
            
            float totalWidth = columnSpacing * columns;
            float totalDepth = rowSpacing * rows;
            float halfTileWidth = columnSpacing / 2f;
            float halfTileDepth = rowSpacing / 2f;
            float startX = totalWidth / -2f + offsetX; // offset is for camera position offset i think
            float startZ = totalDepth / -2f + offsetZ; // offset is for camera position offset i think

            float componentX1 = startX;
            float componentX2;
            float componentZ2;
            float componentZ1;

            float height = offsetY; // height above origin to draw this grid

            // the large columns and rows
            for (int x = 0; x <= columns; x++)
            {
                componentZ1 = startZ;
                componentZ2 = startZ + totalDepth;
                lines.Add(new Line3d(componentX1, height, componentZ1, componentX1, height, componentZ2));
                componentX1 += columnSpacing;
            }

            componentZ1 = startZ;
            for (int z = 0; z <= rows; z++)
            {
                componentX1 = startX;
                componentX2 = startX + totalWidth;
                lines.Add(new Line3d(componentX1, height, componentZ1, componentX2, height, componentZ1));
                componentZ1 += rowSpacing;
            }

            return lines.ToArray();
        }
    }
}
