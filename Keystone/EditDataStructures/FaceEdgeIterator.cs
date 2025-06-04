using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Keystone.EditDataStructures
{
    /* ----------------------------------------------------------------------------
 * FaceEdgeIterator
 * ------------------------------------------------------------------------- */

/*
 * Enumerates the bounding edges of a given face in counterclockwise order.
 */

    internal class FaceEdgeIterator
    {
        /*
   * The first edge to be iterated.
   * Nonnull.
   */
        private Edge start;

        /*
   * The next edge to be iterated.
   * Null if exhausted.
   */
        private Edge edge;


        /*
   * Initialize this edge iterator over a given face.
   * face -> the face to iterate the edges of;
   *         must be nonnull
   */

        public FaceEdgeIterator(Face face)
        {
            // pick an arbitrary edge in the face orbit

            start = face.Edge;
            edge = start;
        }

        /*
   * Release the storage occupied by this edge iterator.
   */

        ~FaceEdgeIterator()
        {
        }

        /*
   * Return the next edge of this edge iterator, if any.
   * <- the next edge of this edge iterator;
   *    null if none
   */

        public Edge Next()
        {
            // check for degeneracy or exhausted iteration

            Edge current = edge;

            if (current == null)
                return null;

            // get the next edge in the left orbit of the face, but return the current
            // edge
            // reset to null if we've come back to the start

            Edge next = current.LNext;

            edge = next != start ? next : null;

            return current;
        }
    }
}