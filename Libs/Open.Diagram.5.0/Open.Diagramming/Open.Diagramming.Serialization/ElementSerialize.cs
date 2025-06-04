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
	public class ElementSerialize: ISerializationSurrogate
	{
		public virtual void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			Element element = (Element) obj;

            //info.AddValue("Key", element.Key);
            info.AddValue("BorderColor", element.BorderColor.ToArgb().ToString());
            info.AddValue("BorderStyle", Convert.ToInt32(element.BorderStyle).ToString());
            info.AddValue("BorderWidth", element.BorderWidth);
            info.AddValue("DrawShadow", element.DrawShadow);
            info.AddValue("Opacity", element.Opacity);
            info.AddValue("SmoothingMode", Convert.ToInt32(element.SmoothingMode).ToString());
            info.AddValue("Visible", element.Visible);
            info.AddValue("ZOrder", element.ZOrder);

            info.AddValue("Tooltip", element.Tooltip);
            info.AddValue("Path", Serialize.AddPath(element.GetPath()));

            //Check if tag can be added
            Serialize.SerializeTag(info, element.Tag);
		}

		public virtual object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
            Element element = (Element) obj;
            SerializationInfoEnumerator enumerator = info.GetEnumerator();

            //Reset reference properties
            element.ResetPath();

            //Enumerate the info object and apply to the appropriate properties
            while (enumerator.MoveNext())
            {
                if (enumerator.Name == "Key") element.SetKey(info.GetString("Key"));
                else if (enumerator.Name == "Path") element.SetPath(Serialize.GetPath(info.GetString("Path")));
                else if (enumerator.Name == "ZOrder") element.SetOrder(info.GetInt32("ZOrder"));
                else if (enumerator.Name == "BorderColor") element.BorderColor = Color.FromArgb(Convert.ToInt32(info.GetString("BorderColor")));
                else if (enumerator.Name == "BorderStyle") element.BorderStyle = (DashStyle) Enum.Parse(typeof(DashStyle), info.GetString("BorderStyle"));
                else if (enumerator.Name == "BorderWidth") element.BorderWidth = info.GetSingle("BorderWidth");
                else if (enumerator.Name == "DrawShadow") element.DrawShadow = info.GetBoolean("DrawShadow");
                else if (enumerator.Name == "SmoothingMode") element.SmoothingMode = (SmoothingMode)Enum.Parse(typeof(SmoothingMode), info.GetString("SmoothingMode"));
                else if (enumerator.Name == "Opacity") element.Opacity = info.GetByte("Opacity");
                else if (enumerator.Name == "Tooltip") element.Tooltip = info.GetString("Tooltip");
                else if (enumerator.Name == "Visible") element.Visible = info.GetBoolean("Visible");
                else if (enumerator.Name == "Tag") element.Tag = info.GetValue("Tag", typeof(object));
            }

            return element;
		}
	}
}
