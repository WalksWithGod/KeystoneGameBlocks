// (c) Copyright Crainiate Software 2009
// This source code is distributed under the terms of the GNU Lesser General Public License (LGPL).
// See http://www.gnu.org/licenses/lgpl-3.0.html and the (Licence).txt file for details.
// All other rights reserved.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Xml;

namespace Open.Diagramming.Web.Svg
{
	public class Polyline
	{
		//Property variables
		private Link _line;
		
		#region  Interface 

		//Create a new polyline class from a line
		public Polyline(Link line)
		{
			Line = line;
		}

		//Sets or gets the line used to extract the polyline
		public virtual Link Line
		{
			get
			{
				return _line;
			}
			set
			{
				_line = value;
			}
		}

		//Extracts a polyline definition for this ERM line
		public virtual string ExtractPolyline()
		{
			return ExtractPolylineImplementation();
		}

		#endregion

		#region  Implementation 

		private string ExtractPolylineImplementation()
		{
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();

			stringBuilder.Append("<polyline id=\"");
			stringBuilder.Append(_line.Key);
			stringBuilder.Append("\" class=\"\" points=\"");

			foreach (PointF point in _line.Points)
			{
                stringBuilder.Append(XmlConvert.ToString(point.X));
				stringBuilder.Append(",");
				stringBuilder.Append(XmlConvert.ToString(point.Y));
				stringBuilder.Append(" ");
			}

			stringBuilder.Append("\" ");

			//Close tag
			stringBuilder.Append("/>");

			return stringBuilder.ToString();
		}

		#endregion
	}
}