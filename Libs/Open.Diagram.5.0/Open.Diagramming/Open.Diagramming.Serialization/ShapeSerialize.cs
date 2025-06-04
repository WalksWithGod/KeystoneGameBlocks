// (c) Copyright Crainiate Software 2009
// This source code is distributed under the terms of the GNU Lesser General Public License (LGPL).
// See http://www.gnu.org/licenses/lgpl-3.0.html and the (Licence).txt file for details.
// All other rights reserved.

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;

namespace Open.Diagramming.Serialization
{
	public class ShapeSerialize: SolidSerialize
	{
		public override void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
            base.GetObjectData(obj, info, context);

            Shape shape = (Shape) obj;

            info.AddValue("AllowMove", shape.AllowMove);
            info.AddValue("AllowScale", shape.AllowScale);
            info.AddValue("AllowRotate", shape.AllowRotate);
            info.AddValue("DrawSelected", shape.DrawSelected);
            info.AddValue("Selected", shape.Selected);
            info.AddValue("KeepAspect", shape.KeepAspect);
            info.AddValue("MinimumSize", Serialize.AddSizeF(shape.MinimumSize));
            info.AddValue("MaximumSize", Serialize.AddSizeF(shape.MaximumSize));
            info.AddValue("Direction", Convert.ToInt32(shape.Direction).ToString());
            info.AddValue("Interaction", Convert.ToInt32(shape.Interaction).ToString());

            info.AddValue("Ports", shape.Ports);
		}

		public override object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
            base.SetObjectData(obj, info, context, selector);

            Shape shape = (Shape)obj;
            SerializationInfoEnumerator enumerator = info.GetEnumerator();            

            //Enumerate the info object and apply to the appropriate properties
            while (enumerator.MoveNext())
            {
                if (enumerator.Name == "AllowMove") shape.AllowMove = info.GetBoolean("AllowMove");
                else if (enumerator.Name == "AllowScale") shape.AllowScale = info.GetBoolean("AllowScale");
                else if (enumerator.Name == "AllowRotate") shape.AllowRotate = info.GetBoolean("AllowRotate");
                else if (enumerator.Name == "DrawSelected") shape.DrawSelected = info.GetBoolean("DrawSelected");
                else if (enumerator.Name == "Selected") shape.Selected = info.GetBoolean("Selected");
                else if (enumerator.Name == "KeepAspect") shape.Selected = info.GetBoolean("KeepAspect");
                else if (enumerator.Name == "MinimumSize") shape.MinimumSize = Serialize.GetSizeF(info.GetString("MinimumSize"));
                else if (enumerator.Name == "MaximumSize") shape.MaximumSize = Serialize.GetSizeF(info.GetString("MaximumSize"));

                else if (enumerator.Name == "Direction") shape.Direction = (Direction) Enum.Parse(typeof(Direction), info.GetString("Direction"), true);
                else if (enumerator.Name == "Interaction") shape.Interaction = (UserInteraction) Enum.Parse(typeof(UserInteraction), info.GetString("Interaction"), true);

                else if (enumerator.Name == "Ports") shape.Ports = (Ports)info.GetValue("Ports", typeof(Ports));
            }

            return shape;
		}
	}
}
