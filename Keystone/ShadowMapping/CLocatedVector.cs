using System;
using MTV3D65;

namespace Keystone.PSSM
{
    internal class CLocatedVector
    {
        public TV_3DVECTOR m_vPosition;
        public TV_3DVECTOR m_vDirection;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vPosition"></param>
        /// <param name="vDirection"></param>
        public CLocatedVector(TV_3DVECTOR vPosition, TV_3DVECTOR vDirection)
        {
            m_vPosition = vPosition;
            m_vDirection = vDirection;
        }
    };
}
