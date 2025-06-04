using System;
using MTV3D65;

namespace Keystone.PSSM
{
    internal class CCamera
    {
        private TVCamera m_pCamera;
        private float m_fFarPlaneLastFrame;
        private float m_fNearPlane;
        private float m_fFarPlane;
        private float m_fMinFarPlane;
        private float m_fMaxFarPlane;
        private float m_fFieldOfView;
        private float m_fAspectRatio;

        public TV_3DVECTOR Position
        {
            get { return m_pCamera.GetPosition(); }
            set 
            {
                m_pCamera.SetPosition(value.x, value.y, value.z);
            }
        }

        public TV_3DVECTOR LookAt
        {
            get { return m_pCamera.GetLookAt(); }
            set
            {
                m_pCamera.SetLookAt(value.x, value.y, value.z);
            }
        }

        public float FarPlane 
        {
            get { return m_fFarPlane; }
            set { m_fFarPlane = value; }
        }

        public float FarPlaneLastFrame
        {
            get { return m_fFarPlaneLastFrame; }
            set { m_fFarPlaneLastFrame = value; }
        }

        public float NearPlane
        {
            get { return m_fNearPlane; }
            set { m_fNearPlane = value; }
        }

        public float MinFarPlane
        {
            get { return m_fMinFarPlane; }
            set { m_fMinFarPlane = value; }
        }

        public float MaxFarPlane
        {
            get { return m_fMaxFarPlane; }
            set { m_fMaxFarPlane = value; }
        }

        /// <summary>
        /// Field of View in radians.
        /// </summary>
        public float FieldOfView
        {
            get { return m_fFieldOfView; }
            set { m_fFieldOfView = value; }
        }

        public float AspectRatio
        {
            get { return m_fAspectRatio; }
            set { m_fAspectRatio = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCamera"></param>
        public CCamera(TVCamera pCamera)
        {
            m_pCamera = pCamera;
            m_pCamera.GetViewFrustum(ref m_fFieldOfView, ref m_fFarPlane, ref m_fNearPlane);
        }
    }
}
