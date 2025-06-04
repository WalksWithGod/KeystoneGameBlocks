using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Open.Diagramming;
using Open.Diagramming.Forms;

namespace Open.Diagramming.Testing
{
    public partial class frmPath: Form
    {
        public frmPath()
        {
            InitializeComponent();

            Model model = diagram1.Model;
          
            Shape shape = new Shape();
            shape.Location = new PointF(100, 100);
            model.Shapes.Add(shape);

            shape.Ports.Add(new Port(PortOrientation.Top));
        }
    }
}