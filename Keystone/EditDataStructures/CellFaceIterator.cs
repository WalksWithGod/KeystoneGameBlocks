using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Keystone.EditDataStructures
{
    /// <summary>
    /// Enumerates the faces of a given cell in arbitrary order.
    /// </summary>
    internal sealed class CellFaceIterator
    {
        // The cell whose faces are being iterated.
        // Nonnull.
        private Cell mCell;

        // The number of faces left to iterate.
        private int mCount;

        // Initialize this face iterator over a given cell.
        // cell -> the cell to iterate the faces of;
        //         must be nonnull

        public CellFaceIterator(Cell cell)
        {
            this.mCell = cell;
            this.mCount = cell.FaceCount;
        }

        // Release the storage occupied by this face iterator.
        ~CellFaceIterator()
        {
        }

        // Return the next face of this face iterator, if any.
        // <- the next face of this face iterator;
        //    null if none
        public Face Next()
        {
            // iterate the array in reverse order so that the current face can be
            // removed during iteration

            if (mCount < 1)
                return null;

            return mCell.Faces[--mCount];
        }
    }
}