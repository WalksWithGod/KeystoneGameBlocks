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
	public class LabelSerialize: ISerializationSurrogate
	{
		public virtual void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			Label label = (Label) obj;

            info.AddValue("Text", label.Text);
            info.AddValue("Offset", Serialization.Serialize.AddPointF(label.Offset));
            info.AddValue("Opacity", label.Opacity);
            info.AddValue("Color", label.Color.ToArgb().ToString());
            info.AddValue("Visible", label.Visible);

            info.AddValue("Font", Serialize.AddFont(label.Font));
		}

		public virtual object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
            Label label = (Label) obj;
            SerializationInfoEnumerator enumerator = info.GetEnumerator();

            //Enumerate the info object and apply to the appropriate properties
            while (enumerator.MoveNext())
            {
                if (enumerator.Name == "Text") label.Text = info.GetString("Text");
                else if (enumerator.Name == "Offset") label.Offset = Serialization.Serialize.GetPointF(info.GetString("Offset"));
                else if (enumerator.Name == "Opacity") label.Opacity = info.GetByte("Opacity");
                else if (enumerator.Name == "Color") label.Color = Color.FromArgb(Convert.ToInt32(info.GetString("Color")));
                else if (enumerator.Name == "Visible") label.Visible = info.GetBoolean("Visible");

                else if (enumerator.Name == "Font") label.SetFont(Serialize.GetFont(info.GetString("Font")));
            }

            return label;
		}
	}
}
