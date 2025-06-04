// (c) Copyright Crainiate Software 2009
// This source code is distributed under the terms of the GNU Lesser General Public License (LGPL).
// See http://www.gnu.org/licenses/lgpl-3.0.html and the (Licence).txt file for details.
// All other rights reserved.

using System;
using System.Xml;
using System.Text;

namespace Open.Diagramming.Web.Svg
{
	public class ShapeFormatter: SolidFormatter
	{
		public override void WriteElement(SvgDocument document, Element element)
		{
			base.WriteElement(document, element);
			
			XmlNode node = Node;

			Shape shape = (Shape) element;
			
			//Set the element as the temporary container node
			XmlNode temp = document.ContainerNode;
			string tempKey = document.ContainerKey;

			document.ContainerKey = shape.Key + "Ports";

			//Add each child as an element
			foreach (Port port in shape.Ports.Values)
			{
				if (port.Visible) document.AddElement(port);
			}

			document.ContainerKey = tempKey;

			//Restore shape element
			SetNode(node);
		}
	}
}
