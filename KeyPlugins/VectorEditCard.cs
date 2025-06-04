using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Keystone.Types;

namespace KeyPlugins
{
    public partial class VectorEditCard : NotecardBase
    {
        public EventHandler VectorChanged;

        public VectorEditCard()
        {
            InitializeComponent();
        }

        public Vector3d Value 
        {
            get
            {
                Vector3d result;
                result.x = numX.Value;
                result.y = numY.Value;
                result.z = numZ.Value;
                return result;
            }
            set
            {
               
                numX.Value = value.x;
                numY.Value = value.y;
                numZ.Value = value.z;
            }
        }

        private void numX_ValueChanged(object sender, EventArgs e)
        {
            if (VectorChanged != null)
                VectorChanged.Invoke(this, e);
        }

        private void numY_ValueChanged(object sender, EventArgs e)
        {
            if (VectorChanged != null)
                VectorChanged.Invoke(this, e);
        }

        private void numZ_ValueChanged(object sender, EventArgs e)
        {
            if (VectorChanged != null)
                VectorChanged.Invoke(this, e);
        }

        // Snapping i think is equivalent to rounding .
//  If snap is enabled then the increment 
//amount must be a multiple of that snap as 
//well.  The second reason why snapping is
// not equivalent to increment is that manipulating
// with mouse is also affected by this snap setting
        private void cbSnap_CheckedChanged(object sender, EventArgs e)
        {

        }


    }
}
