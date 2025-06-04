using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Keystone.EditDataStructures
{
    internal sealed class QuadEdge : IDisposable
    {
        private Edge[] mEdges;
        //private uint mId = Edge.NextID;

        //public QuadEdge()
        //{
        //    mEdges = new Edge[4];

        //    mEdges[0] = new Edge();
        //    mEdges[1] = new Edge();
        //    mEdges[2] = new Edge();
        //    mEdges[3] = new Edge();

        //    mEdges[0].Index = 0;
        //    mEdges[1].Index = 1;
        //    mEdges[2].Index = 2;
        //    mEdges[3].Index = 3;

        //    mEdges[0].ONext = mEdges[0]; // directed edge is itself in a closed loop
        //    mEdges[1].ONext = mEdges[3]; // RoT's next  
        //    mEdges[2].ONext = mEdges[2]; // sym's next is also itself
        //    mEdges[3].ONext = mEdges[1]; // InvRot's next is assigned to it's RoT

        //    mEdges[0].ID = mId + 0;
        //    mEdges[1].ID = mId + 1;
        //    mEdges[2].ID = mId + 2;
        //    mEdges[3].ID = mId + 3;

        //    Edge.NextID = mId + 4;
        //}

        //public Edge[] Edges
        //{
        //    get { return mEdges; }
        //}

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool explicitDispose)
        {
            if (explicitDispose)
            {
                for (uint i = 0; i < mEdges.Length; i++)
                {
                    mEdges[i].Dispose();
                    mEdges[i] = null;
                }
            }
        }

        #endregion
    }
}