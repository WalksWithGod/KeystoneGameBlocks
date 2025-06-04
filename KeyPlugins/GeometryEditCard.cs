using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Keystone.Types;

namespace KeyPlugins
{
    public partial class GeometryEditCard :  NotecardBase
    {
        private string mEntityID;
        private IntPtr mHandle;
        System.Windows.Forms.Control mViewport;

        public GeometryEditCard(IntPtr viewportHandle, string entityID)
        {
            InitializeComponent();

            mEntityID = entityID;
            mHandle = viewportHandle;

            mViewport = System.Windows.Forms.Control.FromHandle(mHandle);

            groupPanel.Controls.Add(mViewport);
           // this.Controls.Add(mViewport);
            //mViewport.Left = this.Left;
            //mViewport.Top = this.Top;
            mViewport.Dock = DockStyle.Fill;
        }

        // triangle count
        //- MeshFormat
        //- FaceCulling
        //- ComputeNormals
        //- GetVertexCount
        //- GetTriangleCount
        //- Get
        //- LOD Sub Edit
        //    - Create New LOD
        //    - Distance Setting
        //    - Mask
        //- Geometry Switch Edit
        //    - CSG Stencil Mask

    }
}
