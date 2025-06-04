// (c) Copyright Crainiate Software 2009
// This source code is distributed under the terms of the GNU Lesser General Public License (LGPL).
// See http://www.gnu.org/licenses/lgpl-3.0.html and the (Licence).txt file for details.
// All other rights reserved.

using System;
using System.Drawing;
using System.Runtime.Serialization;

using Open.Diagramming.Forms;

namespace Open.Diagramming.Serialization
{
	public class DiagramSerialize: ViewSerialize
	{
		//Property variables
		private Controller _controller;

		//Constructors
		public DiagramSerialize(): base()
		{
		}

		//Properties
		public Controller Controller
		{
			get
			{
				return _controller;
			}
		}

		//Implement ISerializable
		public override void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(obj,info,context);

			Diagram diagram = (Diagram) obj;
			
			info.AddValue("AlignGrid",diagram.AlignGrid);
			info.AddValue("DragScroll",diagram.DragScroll);
			info.AddValue("DragSelect",diagram.DragSelect);
			info.AddValue("DrawGrid",diagram.DrawGrid);
			info.AddValue("DrawSelections",diagram.DrawSelections);
			info.AddValue("GridColor",diagram.GridColor.ToArgb().ToString());
			info.AddValue("GridSize",Serialize.AddSize(diagram.GridSize));
			info.AddValue("GridStyle",Convert.ToInt32(diagram.GridStyle).ToString());

			info.AddValue("Controller",diagram.Controller);
		}

		public override object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			base.SetObjectData(obj,info,context,selector);

			Diagram diagram = (Diagram) obj;

			diagram.Suspend();

			diagram.AlignGrid = info.GetBoolean("AlignGrid");
			diagram.DragScroll = info.GetBoolean("DragScroll");
			diagram.DrawGrid = info.GetBoolean("DrawGrid");
			diagram.DrawSelections = info.GetBoolean("DrawSelections");
			diagram.GridColor = Color.FromArgb(Convert.ToInt32(info.GetString("GridColor")));
			diagram.GridSize = Serialize.GetSize(info.GetString("GridSize"));
			diagram.GridStyle = (GridStyle) Enum.Parse(typeof(GridStyle), info.GetString("GridStyle"));

			if (Serialize.ContainsString(info,"DragSelect")) diagram.DragSelect = info.GetBoolean("DragSelect");

			_controller = (Controller) info.GetValue("Controller",typeof(Controller));

			diagram.Resume();
			
			return diagram;
		}
	}
}
