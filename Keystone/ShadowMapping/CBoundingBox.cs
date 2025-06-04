using System;
using MTV3D65;


namespace Keystone.PSSM
{
    internal class CBoundingBox
    {
        private TV_3DVECTOR m_vMinimum;
        private TV_3DVECTOR m_vMaximum;
        private TV_3DVECTOR[] m_vaVertices;
        private CLocatedVector[] m_vaEdges;
        private bool m_bIsContructed;

        // TODO: vertices and edge should use isDirty flag and only update when caller attempts to retreive 
        // Vertices while isDirty = true 
        // m_bIsContructed is kinda that but its not used properly
        public TV_3DVECTOR[] Vertices
        {
            get { return m_vaVertices; }
        }

        // TODO: vertices and edge should use isDirty flag and only update when caller attempts to retreive 
        // Edges while isDirty = true
        // m_bIsContructed is kinda that but its not used properly
        public CLocatedVector[] Edges
        {
            get { return m_vaEdges; }
        }

        public TV_3DVECTOR Minimum
        {
            get { return m_vMinimum; }
            set { m_vMinimum = value; }
        }

        public TV_3DVECTOR Maximum
        {
            get { return m_vMaximum; }
            set { m_vMaximum = value; }
        }

        public float Height 
        {
            get { return m_vMaximum.y - m_vMinimum.y; }
        }

        /// <summary>
        /// 
        /// </summary>
        public CBoundingBox()
        {
            m_vaVertices = new TV_3DVECTOR[8];
            m_vaEdges = new CLocatedVector[12];
            Reset();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vMin"></param>
        /// <param name="vMax"></param>
        public CBoundingBox(TV_3DVECTOR vMin, TV_3DVECTOR vMax)
            : this()
        {
            m_vMaximum = vMax;
            m_vMinimum = vMin;
            ConstructEverything();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vPoints"></param>
        /// <param name="n"></param>
        public CBoundingBox(TV_3DVECTOR[] vPoints, int n)
            : this()
        {
            Merge(vPoints, n);
            ConstructEverything();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Reset()
        {
            m_vMinimum = new TV_3DVECTOR(float.MaxValue, float.MaxValue, float.MaxValue);
            m_vMaximum = new TV_3DVECTOR(float.MinValue, float.MinValue, float.MinValue);
            m_bIsContructed = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vPoints"></param>
        /// <param name="n"></param>
        public void Merge(TV_3DVECTOR[] vPoints, int n)
        {
            for (int i = 0; i < n; i++)
                Merge(vPoints[i]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        public void Merge(TV_3DVECTOR v)
        {
            m_vMinimum.x = Math.Min(m_vMinimum.x, v.x);
            m_vMinimum.y = Math.Min(m_vMinimum.y, v.y);
            m_vMinimum.z = Math.Min(m_vMinimum.z, v.z);
            m_vMaximum.x = Math.Max(m_vMaximum.x, v.x);
            m_vMaximum.y = Math.Max(m_vMaximum.y, v.y);
            m_vMaximum.z = Math.Max(m_vMaximum.z, v.z);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public TV_3DVECTOR Centroid()
        {
            return m_vMinimum + (m_vMaximum - m_vMinimum) * 0.5f;
        }

        /// <summary>
        /// 
        /// </summary>
        public void ConstructEverything()
        {
        	// MPJ
        	// TODO: is it necessary to do this every time we add a new mesh?  Can't we set an IsDirty flag
        	// and then "ConstructEverything()" when we're ready to use the vertex corners and edges? 
            ConstructVertices();
            ConstructEdges();
            m_bIsContructed = true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void ConstructVertices()
        {
            m_vaVertices[0] = new TV_3DVECTOR(m_vMinimum.x, m_vMinimum.y, m_vMinimum.z);
            m_vaVertices[1] = new TV_3DVECTOR(m_vMaximum.x, m_vMinimum.y, m_vMinimum.z);
            m_vaVertices[2] = new TV_3DVECTOR(m_vMinimum.x, m_vMinimum.y, m_vMaximum.z);
            m_vaVertices[3] = new TV_3DVECTOR(m_vMaximum.x, m_vMinimum.y, m_vMaximum.z);
            m_vaVertices[4] = new TV_3DVECTOR(m_vMinimum.x, m_vMaximum.y, m_vMinimum.z);
            m_vaVertices[5] = new TV_3DVECTOR(m_vMaximum.x, m_vMaximum.y, m_vMinimum.z);
            m_vaVertices[6] = new TV_3DVECTOR(m_vMinimum.x, m_vMaximum.y, m_vMaximum.z);
            m_vaVertices[7] = new TV_3DVECTOR(m_vMaximum.x, m_vMaximum.y, m_vMaximum.z);
        }

        /// <summary>
        /// 
        /// </summary>
        public void ConstructEdges()
        {
            // X-aligned lines on both sides, both heights
            m_vaEdges[0] = new CLocatedVector(m_vaVertices[0], m_vaVertices[1]);
            m_vaEdges[1] = new CLocatedVector(m_vaVertices[2], m_vaVertices[3]);
            m_vaEdges[2] = new CLocatedVector(m_vaVertices[4], m_vaVertices[5]);
            m_vaEdges[3] = new CLocatedVector(m_vaVertices[6], m_vaVertices[7]);

            // Y-aligned lines at each corner
            m_vaEdges[4] = new CLocatedVector(m_vaVertices[0], m_vaVertices[4]);
            m_vaEdges[5] = new CLocatedVector(m_vaVertices[2], m_vaVertices[6]);
            m_vaEdges[6] = new CLocatedVector(m_vaVertices[1], m_vaVertices[5]);
            m_vaEdges[7] = new CLocatedVector(m_vaVertices[3], m_vaVertices[7]);

            // Z-aligned lines on both sides, both heights
            m_vaEdges[8] = new CLocatedVector(m_vaVertices[0], m_vaVertices[2]);
            m_vaEdges[9] = new CLocatedVector(m_vaVertices[1], m_vaVertices[3]);
            m_vaEdges[10] = new CLocatedVector(m_vaVertices[4], m_vaVertices[6]);
            m_vaEdges[11] = new CLocatedVector(m_vaVertices[5], m_vaVertices[7]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nColor"></param>
        public void Draw(int nColor)
        {
            if (m_bIsContructed)
                foreach (CLocatedVector tEdge in m_vaEdges)
                    CoreClient._CoreClient.Screen2D.Draw_Line3D(
                        tEdge.m_vPosition.x, tEdge.m_vPosition.y, tEdge.m_vPosition.z,
                        tEdge.m_vDirection.x, tEdge.m_vDirection.y, tEdge.m_vDirection.z,
                        nColor);
        }
    }
}
