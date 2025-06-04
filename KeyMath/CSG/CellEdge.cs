using System;

namespace Keystone.CSG
{
	
	
	
	
	
	
	#region Tile Edge - Makes CellEdge Obsolete
	public struct TileEdge
    {

    	// TODO: 
    	//       bitflag designator ala marching cubes is interesting but it would not allow for shared edgeIDs.
    	//       http://upload.wikimedia.org/wikipedia/commons/d/d5/Marching-squares-isoband-2.png
    	//       - edgeID are used as indices into dictionary mEdgeStructures
    	//	     - the edgeID is the same for a left cell's right wall, and for the adjacent cell to the right's
    	//         left wall.
    	//       - TODO: we have to be able to handle shared edges that are on boundary of zone 
        public enum EdgeOrientation : byte
        {
            // note: an edge should not be considered as being a "side" of a cell since it suggests
            // the two sides of a single wall are seperate.
            // edges are shared boundaries so interior edges are shared by a left
            // cell and a right cell and so would be considered a "side" to both of those cells. 
      		// But those two sides make up a single logical edge construct.
            Vertical = 0,
            Horizontal = 1,
            DiagonalLR = 2,  // For Diagonal LR since origin is always smallest, the bottom left is 
            // always the origin for left to right diagonal
            DiagonalRL = 3   // For Diagonal RL since origin is always smallest, the bottom right
            // is always the origin for right to left diagonal
        }

        public enum EdgeLocation : byte
        {
            Inner = 0,
            Exterior_TopRight = 1,
            Exterior_BottomLeft = 2
        }


        public uint ID;
        public uint Row;    // z
        public uint Column; // x
        public uint Layer;  // y (deck height)
        public EdgeOrientation Orientation;
        public uint Origin; // index of origin vertex
        public uint Dest;
        // if both Cells are -1, this must be diagonal edge.  If only one cell is -1, then it's border with one cell out of bounds.
        public int BottomLeftCell;  // Bottom/Left cell (-z/-x).  -1 means out of bounds
        public int TopRightCell; // Top/Right cell (+z/+x). -1 means out of bounds


        public struct TileEdgeStatistics
        {
            public uint EdgesPerDeck;
            public uint TilesPerLevel;
            public uint HorizontalEdgeRows;
            public uint VerticalEdgeColumns;
            public uint HorizontalEdgesCount; // horizontals edges take up first set of indices in a deck level
            public uint VerticalEdgesCount;  // veritcal edges take up second set of indices in a deck level
            public uint DiagonalEdgesCount;
            public uint VertexCountX;
            public uint VertexCountZ;

            public uint VerticesPerDeck;

            public TileEdgeStatistics(uint cellCountX, uint cellCountZ)
            {

                TilesPerLevel = cellCountX * cellCountZ;
                HorizontalEdgeRows = cellCountZ + 1;
                VerticalEdgeColumns = cellCountX + 1;
                HorizontalEdgesCount = cellCountX * HorizontalEdgeRows; // horizontals edges take up first set of indices in a deck level
                VerticalEdgesCount = cellCountZ * VerticalEdgeColumns;  // veritcal edges take up second set of indices in a deck level
                DiagonalEdgesCount = TilesPerLevel * 2;                 // diagonal edges take up final set of indices in a level
                
                EdgesPerDeck = HorizontalEdgesCount + VerticalEdgesCount + DiagonalEdgesCount;

                VertexCountX = cellCountX + 1;
                VertexCountZ = cellCountZ + 1;
                VerticesPerDeck = VertexCountX * VertexCountZ;
            }

        }

        public static TileEdgeStatistics GetTileEdgeStatistics(uint cellCountX, uint cellCountZ)
        {
            return new TileEdgeStatistics(cellCountX, cellCountZ);
        }

        // TODO: if i define the edge by it's origin and destination indices? 
        //       with the origin always being the smallest of the two values.  
        //       
        public static TileEdge CreateTileEdge(uint edgeID, uint cellCountX, uint cellCountZ)
        {
            TileEdgeStatistics es = GetTileEdgeStatistics(cellCountX, cellCountZ);
            uint currentDeck = edgeID / es.EdgesPerDeck; // 0 based deck indices
            System.Diagnostics.Debug.Assert (currentDeck == (uint)System.Math.Ceiling ((double)(edgeID / es.EdgesPerDeck)));
                                             
            if (edgeID < 0 || edgeID > es.EdgesPerDeck) throw new ArgumentOutOfRangeException();

            // the deckspace edgeID is used temporarily here to compute adjacent cellID's
            uint deckSpaceEdgeID = edgeID - (es.EdgesPerDeck * currentDeck);
            EdgeOrientation orientation;

            // compute the origin and dest then we can call our other CreateEdge function
            uint originID;
            uint destID;

            if (deckSpaceEdgeID < es.HorizontalEdgesCount)
            {
                // this is a horizontal edge
                uint row = deckSpaceEdgeID / cellCountX; // 0 based row indices
                originID = deckSpaceEdgeID + row + (currentDeck * es.VerticesPerDeck);  // origin index is shifted edgeID + row index 
                destID = originID + 1;
                orientation = EdgeOrientation.Horizontal;
            }
            else if (deckSpaceEdgeID < es.HorizontalEdgesCount + es.VerticalEdgesCount)
            {
                // this is a vertical edge
                uint column = (deckSpaceEdgeID - es.HorizontalEdgesCount) % es.VerticalEdgeColumns; // 0 based column indices

                originID = deckSpaceEdgeID - es.HorizontalEdgesCount + (currentDeck * es.VerticesPerDeck);
                destID = originID + es.VerticalEdgeColumns; // verticalEdeColumns == verts along X side
                orientation = EdgeOrientation.Vertical;
            }
            else // diagonal // TODO:
            {
                orientation = EdgeOrientation.DiagonalLR;
                orientation = EdgeOrientation.DiagonalRL;
                throw new NotImplementedException();
            }

            TileEdge result = CreateTileEdge(originID, destID, cellCountX, cellCountZ, orientation);
            System.Diagnostics.Debug.Assert(result.ID == edgeID);
            return result;
        }


        public static TileEdge CreateTileEdge(uint originID, uint destID, uint cellCountX, uint cellCountZ, EdgeOrientation orientation)
        {
            if (originID > destID || originID == destID) throw new ArgumentOutOfRangeException();

            TileEdgeStatistics es = GetTileEdgeStatistics(cellCountX, cellCountZ);

            uint currentDeck = originID / es.VerticesPerDeck; 


            TileEdge edge = new TileEdge();
            edge.Origin = originID;
            edge.Dest = destID;
            
            // to compute the edgeID, we start by assuming currentDeck == 0 then we add
            // the numberVertsPerDeck to the final result.  But for this to work
            // the originID and destID's must also be transformed to deckspace
            // This is only required for easily finding the edgeID.  The leftCell and rightCell
            // we can use the original ID's
            uint deckSpaceOriginID = originID - (es.VerticesPerDeck * currentDeck);
            uint deckSpaceDestID = destID - (es.VerticesPerDeck * currentDeck);

            // we're assigning column and row to help us compute
            // position of this edge later based on cellSize and map dimensions. 
            // KNOWING row and column is the only way to find the position.
            edge.Row = deckSpaceOriginID / es.VertexCountX;
            edge.Column = deckSpaceOriginID % es.VertexCountX;
            edge.Layer = currentDeck;
            //System.Diagnostics.Debug.WriteLine("Row = " + edge.Row.ToString() + " Column = " + edge.Column.ToString());

            uint edgeID = 0;
            if (orientation == EdgeOrientation.Horizontal)
            {
                edgeID = deckSpaceOriginID - edge.Row + (es.EdgesPerDeck * currentDeck); 
            }
            else if (orientation == EdgeOrientation.Vertical)
            {
                edgeID = deckSpaceOriginID + es.HorizontalEdgesCount + (es.EdgesPerDeck * currentDeck);
            }
            else
                throw new NotImplementedException();

            edge.ID = edgeID; 

            // Horizontal has walls parallel with X axis 
            if (orientation == EdgeOrientation.Horizontal)
            {
                // find the cell above and below this horizontal edge.
                // the cell below will have the lower ID and thus is the Left cell.
                System.Diagnostics.Debug.Assert(destID - originID == 1); // horizontal edge origin and dest vertices always seperated by 1 (unless boundary)

                bool bottomBorder = deckSpaceOriginID < es.VertexCountX;
                bool topBorder = deckSpaceOriginID >= (es.VertexCountX * (es.VertexCountZ - 1));
                edge.Orientation = EdgeOrientation.Horizontal;

                if (bottomBorder)
                {
                    edge.BottomLeftCell = -1;
                    edge.TopRightCell = (int)(deckSpaceOriginID + currentDeck * es.TilesPerLevel);
                }
                else if (topBorder)
                {
                    // easier to compute the left cell for the top row of verts
                    // as if we were computing the Right Cell for the row of verts below
                    // so simply subtract vertex count
                    edge.BottomLeftCell = (int)(deckSpaceDestID - es.VertexCountX - edge.Row + currentDeck * es.TilesPerLevel);
                    edge.TopRightCell = -1;
                }
                else
                {
                    edge.BottomLeftCell = (int)(deckSpaceDestID - es.VertexCountX - edge.Row + currentDeck * es.TilesPerLevel);
                    edge.TopRightCell = (int)(edge.BottomLeftCell + cellCountX);
                }
            }
                // Vertical has walls parallel with Z Axis
            else if (orientation == EdgeOrientation.Vertical)
            {
                // find the cell to the left and right of this vertical edge
                // the cell to the left will have the lower ID thus is the left cell

                // for vertical edge, the diff between origin and dest should be number of vertices along x
                if (destID - originID != es.VertexCountX) throw new Exception("Invalid edge origin and dest.  No such edge is possible.");

                bool leftBorder = (deckSpaceOriginID % es.VertexCountX) == 0;
                bool rightBorder = (deckSpaceOriginID % es.VertexCountX) == es.VertexCountX - 1;
                edge.Orientation = EdgeOrientation.Vertical;

                if (leftBorder)
                {
                    edge.BottomLeftCell = -1;
                    edge.TopRightCell = (int)(deckSpaceOriginID - edge.Row + currentDeck * es.TilesPerLevel);
                }
                else if (rightBorder)
                {
                    edge.BottomLeftCell = (int)(deckSpaceOriginID - edge.Row - 1 + currentDeck * es.TilesPerLevel);
                    edge.TopRightCell = -1;
                }
                else
                {
                    edge.BottomLeftCell = (int)(deckSpaceOriginID - edge.Row - 1 + currentDeck * es.TilesPerLevel);
                    edge.TopRightCell = edge.BottomLeftCell + 1;
                }
            }
            else if (orientation == EdgeOrientation.DiagonalLR)
            {
                throw new NotImplementedException();
            }
            else if (orientation == EdgeOrientation.DiagonalRL)
            {
                throw new NotImplementedException();
            }

            //System.Diagnostics.Debug.WriteLine("Edge " + originID.ToString() + " - " + destID.ToString() + " created.  Left Cell = " + edge.LeftCell.ToString() + " Right Cell = " + edge.RightCell.ToString());
            // we can also compute the next left edge given an existing edge?
            return edge;
        }
	}

	
	#endregion
	
	
	
    public struct CellEdge
    {

    	// TODO: 
    	//       bitflag designator ala marching cubes is interesting but it would not allow for shared edgeIDs.
    	//       http://upload.wikimedia.org/wikipedia/commons/d/d5/Marching-squares-isoband-2.png
    	//       - edgeID are used as indices into dictionary mEdgeStructures
    	//	     - the edgeID is the same for a left cell's right wall, and for the adjacent cell to the right's
    	//         left wall.
    	//       - TODO: we have to be able to handle shared edges that are on boundary of zone 
        public enum EdgeOrientation : byte
        {
            // note: an edge should not be considered as being a "side" of a cell since it suggests
            // the two sides of a single wall are seperate.
            // edges are shared boundaries so interior edges are shared by a left
            // cell and a right cell and so would be considered a "side" to both of those cells. 
      		// But those two sides make up a single logical edge construct.
            Vertical = 0,
            Horizontal = 1,
            DiagonalLR = 2,  // For Diagonal LR since origin is always smallest, the bottom left is 
            // always the origin for left to right diagonal
            DiagonalRL = 3   // For Diagonal RL since origin is always smallest, the bottom right
            // is always the origin for right to left diagonal
        }

        public enum EdgeLocation : byte
        {
            Inner = 0,
            Exterior_TopRight = 1,
            Exterior_BottomLeft = 2
        }

        /// <summary>
        /// Edge ID is unique, even across decks
        /// </summary>
        public uint ID;
        /// <summary>
        /// Row is along Z axis starting at row 0 closest to the screen
        /// </summary>
        public uint Row;    // z axis
        /// <summary>
        /// Column is along X axis starting at Column 0 at left most of screen
        /// </summary>
        public uint Column; // x
        /// <summary>
        /// Deck height starting at lowest deck 0.
        /// </summary>
        public uint Layer; 
        public EdgeOrientation Orientation;
        /// <summary>
        /// Index of origin vertex.  This is a global index value and is unique across all decks.
        /// </summary>
        public uint Origin;
        /// <summary>
        /// Index of destination vertex.  This is a global index value and is unique across all decks.
        /// </summary>
        public uint Dest;
        // if both Cells are -1, this must be diagonal edge.  If only one cell is -1, then it's border with one cell out of bounds.
        public int BottomLeftCell;  // Bottom/Left cell (-z/-x).  -1 means out of bounds
        public int TopRightCell; // Top/Right cell (+z/+x). -1 means out of bounds


        public struct EdgeStatistics
        {
            public uint EdgesPerDeck;
            public uint MaxEdgeCount;

            public uint TilesCount;
            public uint TilesPerLevel;
            public uint HorizontalEdgeRows;
            public uint VerticalEdgeColumns;
            public uint HorizontalEdgesCount; // horizontals edges take up first set of indices in a deck level
            public uint VerticalEdgesCount;  // veritcal edges take up second set of indices in a deck level
           // public uint DiagonalEdgesCount;
            public uint VertexCountX;
            public uint VertexCountZ;

            public uint VerticesPerDeck;

            public EdgeStatistics(uint cellCountX, uint cellCountY, uint cellCountZ)
            {
                TilesCount = cellCountX * cellCountZ * cellCountY;
                TilesPerLevel = cellCountX * cellCountZ;
                HorizontalEdgeRows = cellCountZ + 1;
                VerticalEdgeColumns = cellCountX + 1;
                HorizontalEdgesCount = cellCountX * HorizontalEdgeRows; // horizontals edges take up first set of indices in a deck level
                VerticalEdgesCount = cellCountZ * VerticalEdgeColumns;  // veritcal edges take up second set of indices in a deck level
                //DiagonalEdgesCount = TilesPerLevel * 2;                 // diagonal edges take up final set of indices in a level

                EdgesPerDeck = HorizontalEdgesCount + VerticalEdgesCount; // + DiagonalEdgesCount; // TODO: NOTE diagonals are not used
                MaxEdgeCount = EdgesPerDeck * cellCountY; // TODO: this ignores the top most ceiling i think? 

                VertexCountX = cellCountX + 1;
                VertexCountZ = cellCountZ + 1;
                VerticesPerDeck = VertexCountX * VertexCountZ;
            }

        }

        public static EdgeStatistics GetEdgeStatistics(uint cellCountX, uint cellCountY, uint cellCountZ)
        {
            return new EdgeStatistics(cellCountX, cellCountY, cellCountZ);
        }



        public static CellEdge CreateEdge(uint edgeID, uint cellCountX, uint cellCountY, uint cellCountZ)
        {
            EdgeStatistics es = GetEdgeStatistics(cellCountX, cellCountY, cellCountZ);
            uint currentDeck = edgeID / es.EdgesPerDeck; // 0 based deck indices
            System.Diagnostics.Debug.Assert (currentDeck == (uint)System.Math.Ceiling ((double)(edgeID / es.EdgesPerDeck)));
                                             
            if (edgeID < 0 || edgeID > es.MaxEdgeCount) throw new ArgumentOutOfRangeException();

            // the deckspace edgeID is used temporarily here to compute adjacent cellID's
            uint deckSpaceEdgeID = edgeID - (es.EdgesPerDeck * currentDeck);
            EdgeOrientation orientation;

            // compute the origin and dest then we can call our other CreateEdge function
            uint originID;
            uint destID;

            if (deckSpaceEdgeID < es.HorizontalEdgesCount)
            {
                // this is a horizontal edge
                uint row = deckSpaceEdgeID / cellCountX; // 0 based row indices
                originID = deckSpaceEdgeID + row + (currentDeck * es.VerticesPerDeck);  // origin index is shifted edgeID + row index 
                destID = originID + 1;
                orientation = EdgeOrientation.Horizontal;
            }
            else if (deckSpaceEdgeID < es.HorizontalEdgesCount + es.VerticalEdgesCount)
            {
                // this is a vertical edge
                uint column = (deckSpaceEdgeID - es.HorizontalEdgesCount) % es.VerticalEdgeColumns; // 0 based column indices

                originID = deckSpaceEdgeID - es.HorizontalEdgesCount + (currentDeck * es.VerticesPerDeck);
                destID = originID + es.VerticalEdgeColumns; // verticalEdeColumns == verts along X side
                orientation = EdgeOrientation.Vertical;
            }
            else // diagonal // TODO:
            {
                orientation = EdgeOrientation.DiagonalLR;
                orientation = EdgeOrientation.DiagonalRL;
                throw new NotImplementedException();
            }

            CellEdge result = CreateEdge(originID, destID, cellCountX, cellCountY, cellCountZ);
            System.Diagnostics.Debug.Assert(result.ID == edgeID);
            return result;
        }

        // TODO: if i define the edge by it's origin and destination indices? 
        //       with the origin always being the smallest of the two values.  
        //  TODO: i think orientation argument here can be deduced from originID and destID.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="originID">A unique vertex ID that is unique across decks</param>
        /// <param name="destID">A unique vertex ID that is unique across decks.</param>
        /// <param name="cellCountX"></param>
        /// <param name="cellCountY"></param>
        /// <param name="cellCountZ"></param>
        /// <returns></returns>
        public static CellEdge CreateEdge(uint originID, uint destID, uint cellCountX, uint cellCountY, uint cellCountZ)
        {
            if (originID > destID || originID == destID) throw new ArgumentOutOfRangeException();

            EdgeStatistics es = GetEdgeStatistics(cellCountX, cellCountY, cellCountZ);

            uint currentDeck = originID / es.VerticesPerDeck; 


            CellEdge edge = new CellEdge();
            edge.Origin = originID;
            edge.Dest = destID;
            
            // to compute the edgeID, we start by assuming currentDeck == 0 then we add
            // the numberVertsPerDeck to the final result.  But for this to work
            // the originID and destID's must also be transformed to deckspace
            // This is only required for easily finding the edgeID.  The leftCell and rightCell
            // we can use the original ID's
            uint deckSpaceOriginID = originID - (es.VerticesPerDeck * currentDeck);
            uint deckSpaceDestID = destID - (es.VerticesPerDeck * currentDeck);
            
            // we're assigning column and row to help us compute
            // position of this edge later based on cellSize and map dimensions. 
            // KNOWING row and column is the only way to find the position.
            edge.Row = deckSpaceOriginID / es.VertexCountX;
            edge.Column = deckSpaceOriginID % es.VertexCountX;
            edge.Layer = currentDeck;
            //System.Diagnostics.Debug.WriteLine("Row = " + edge.Row.ToString() + " Column = " + edge.Column.ToString());
            EdgeOrientation orientation;
            uint edgeID = 0;
            if (Math.Abs(destID - originID) == 1)
            {
                orientation = EdgeOrientation.Horizontal;
                edgeID = deckSpaceOriginID - edge.Row + (es.EdgesPerDeck * currentDeck); 
            }
            else if (Math.Abs(destID - originID) == es.VertexCountX)
            {
                orientation = EdgeOrientation.Vertical;
                edgeID = deckSpaceOriginID + es.HorizontalEdgesCount + (es.EdgesPerDeck * currentDeck);
            }
            else
                throw new NotImplementedException();

            edge.ID = edgeID; 

            // Horizontal has walls parallel with X axis 
            if (orientation == EdgeOrientation.Horizontal)
            {
                // find the cell above and below this horizontal edge.
                // the cell below will have the lower ID and thus is the Left cell.
                System.Diagnostics.Debug.Assert(destID - originID == 1); // horizontal edge origin and dest vertices always seperated by 1 (unless boundary)

                bool bottomBorder = deckSpaceOriginID < es.VertexCountX;
                bool topBorder = deckSpaceOriginID >= (es.VertexCountX * (es.VertexCountZ - 1));
                edge.Orientation = EdgeOrientation.Horizontal;

                if (bottomBorder)
                {
                    edge.BottomLeftCell = -1;
                    edge.TopRightCell = (int)(deckSpaceOriginID + currentDeck * es.TilesPerLevel);
                }
                else if (topBorder)
                {
                    // easier to compute the left cell for the top row of verts
                    // as if we were computing the Right Cell for the row of verts below
                    // so simply subtract vertex count
                    edge.BottomLeftCell = (int)(deckSpaceDestID - es.VertexCountX - edge.Row + currentDeck * es.TilesPerLevel);
                    edge.TopRightCell = -1;
                }
                else
                {
                    edge.BottomLeftCell = (int)(deckSpaceDestID - es.VertexCountX - edge.Row + currentDeck * es.TilesPerLevel);
                    edge.TopRightCell = (int)(edge.BottomLeftCell + cellCountX);
                }
            }
                // Vertical has walls parallel with Z Axis
            else if (orientation == EdgeOrientation.Vertical)
            {
                // find the cell to the left and right of this vertical edge
                // the cell to the left will have the lower ID thus is the left cell

                // for vertical edge, the diff between origin and dest should be number of vertices along x
                if (destID - originID != es.VertexCountX) throw new Exception("Invalid edge origin and dest.  No such edge is possible.");

                bool leftBorder = (deckSpaceOriginID % es.VertexCountX) == 0;
                bool rightBorder = (deckSpaceOriginID % es.VertexCountX) == es.VertexCountX - 1;
                edge.Orientation = EdgeOrientation.Vertical;

                if (leftBorder)
                {
                    edge.BottomLeftCell = -1;
                    edge.TopRightCell = (int)(deckSpaceOriginID - edge.Row + currentDeck * es.TilesPerLevel);
                }
                else if (rightBorder)
                {
                    edge.BottomLeftCell = (int)(deckSpaceOriginID - edge.Row - 1 + currentDeck * es.TilesPerLevel);
                    edge.TopRightCell = -1;
                }
                else
                {
                    edge.BottomLeftCell = (int)(deckSpaceOriginID - edge.Row - 1 + currentDeck * es.TilesPerLevel);
                    edge.TopRightCell = edge.BottomLeftCell + 1;
                }
            }
            else if (orientation == EdgeOrientation.DiagonalLR)
            {
                throw new NotImplementedException();
            }
            else if (orientation == EdgeOrientation.DiagonalRL)
            {
                throw new NotImplementedException();
            }

            //System.Diagnostics.Debug.WriteLine("Edge " + originID.ToString() + " - " + destID.ToString() + " created.  Left Cell = " + edge.LeftCell.ToString() + " Right Cell = " + edge.RightCell.ToString());
            // we can also compute the next left edge given an existing edge?
            return edge;
        }


        public void GetByteRotation(out byte bottomLeftRotation, out byte topRightRotation)
        {
            switch (this.Orientation)
            {
                case Keystone.CSG.CellEdge.EdgeOrientation.Horizontal:
                    {
                        // TODO: i want to verify that our wall rotation matches this as well
                        //       as the layout of the footprint in the 16x16 grid.
                        bottomLeftRotation = 0; // it's important here that the footprint data layout in our 16x16 grid matches the rotation for them to match up properly
                        topRightRotation = 128;
                        break;
                    }
                case Keystone.CSG.CellEdge.EdgeOrientation.Vertical:
                    {
                        bottomLeftRotation = 192;
                        topRightRotation = 64;
                        break;
                    }
                default:
                    {
                        bottomLeftRotation = 0;
                        topRightRotation = 0;
                        break;
                    }
            }
        }
    }
}
