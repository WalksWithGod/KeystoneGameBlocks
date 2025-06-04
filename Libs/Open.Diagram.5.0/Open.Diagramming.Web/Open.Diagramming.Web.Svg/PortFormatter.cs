// (c) Copyright Crainiate Software 2009
// This source code is distributed under the terms of the GNU Lesser General Public License (LGPL).
// See http://www.gnu.org/licenses/lgpl-3.0.html and the (Licence).txt file for details.
// All other rights reserved.

using System;
using System.Xml;
using System.Text;

namespace Open.Diagramming.Web.Svg
{
	public class PortFormatter: SolidFormatter
	{
		public override void WriteElement(SvgDocument document, Element element)
		{
			base.WriteElement(document, element);

			//Reset the transform to the port transform
			XmlElement xmlelement = (XmlElement) Node;
			Port port = element as Port;

			xmlelement.SetAttribute("x", (port.X + port.Offset.X).ToString());
			xmlelement.SetAttribute("y", (port.Y + port.Offset.Y).ToString());
		}
	}
}
