// (c) Copyright Crainiate Software 2009
// This source code is distributed under the terms of the GNU Lesser General Public License (LGPL).
// See http://www.gnu.org/licenses/lgpl-3.0.html and the (Licence).txt file for details.
// All other rights reserved.

using System;
using System.Runtime.Serialization;
using Open.Diagramming.Flowcharting;

namespace Open.Diagramming.Serialization
{
	public class FlowchartSerialize: DiagramSerialize
	{
		public FlowchartSerialize()
		{
		}

		public override void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(obj,info,context);
			
			Flowchart flowchart = (Flowchart) obj;

			info.AddValue("Orientation",Convert.ToInt32(flowchart.Orientation).ToString());
			info.AddValue("Spacing",Serialize.AddSizeF(flowchart.Spacing));
		}

		public override object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			base.SetObjectData(obj,info,context,selector);

			Flowchart flowchart = (Flowchart) obj;
			
			flowchart.Orientation = (FlowchartOrientation) Enum.Parse(typeof(FlowchartOrientation), info.GetString("Orientation"));
			flowchart.Spacing = Serialize.GetSizeF(info.GetString("Spacing"));

			return flowchart;
		}
	}
}
