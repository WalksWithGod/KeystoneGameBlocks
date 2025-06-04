using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Keystone.EditDataStructures
{
    // Enumerates the vertices of a given cell in arbitrary order.
    //
    public sealed class CellVertexIterator
    {
        // The cell whose vertices are being iterated.
        // Nonnull.
        private Cell mCell;

        //The number of vertices left to iterate.
        private int mCount;


        // Initialize this vertex iterator over a given cell.
        // cell -> the cell to iterate the vertices of;
        //         must be nonnull
        public CellVertexIterator(Cell cell)
        {
            this.mCell = cell;
            this.mCount = cell.VertexCount;
        }


        // Release the storage occupied by this vertex iterator.
        ~CellVertexIterator()
        {
        }


        // Return the next vertex of this vertex iterator, if any.
        // <- the next vertex of this vertex iterator;
        //    null if none
        public Vertex Next()
        {
            // iterate the array in reverse order so that the current vertex can be
            // removed during iteration

            if (mCount < 1)
                return null;

            return mCell.Vertices[--mCount];
        }
    }
}