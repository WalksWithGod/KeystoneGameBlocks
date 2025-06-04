using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Keystone.EditDataStructures
{
/* ----------------------------------------------------------------------------
 * VertexEdgeIterator
 * ------------------------------------------------------------------------- */

/*
 * Enumerates the outgoing edges of a given vertex in counterclockwise order.
 */

    internal class VertexEdgeIterator
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
   * Initialize this edge iterator over a given vertex.
   * vertex -> the vertex to iterate the edges of;
   *           must be nonnull
   */

        public VertexEdgeIterator(Vertex vertex)
        {
            // pick an arbitrary edge in the vertex orbit

            start = vertex.Edge;
            edge = start;
        }

        /*
   * Release the storage occupied by this edge iterator.
   */

        ~VertexEdgeIterator()
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

            // get the next edge in the counterclockwise orbit of the vertex, but
            // return the current edge
            // reset to null if we've come back to the start

            Edge next = current.ONext;

            edge = next != start ? next : null;

            return current;
        }
    }
}