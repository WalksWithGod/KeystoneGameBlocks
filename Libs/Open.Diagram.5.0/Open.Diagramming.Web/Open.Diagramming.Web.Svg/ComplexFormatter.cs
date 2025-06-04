// (c) Copyright Crainiate Software 2009
// This source code is distributed under the terms of the GNU Lesser General Public License (LGPL).
// See http://www.gnu.org/licenses/lgpl-3.0.html and the (Licence).txt file for details.
// All other rights reserved.

using System;
using System.Xml;
using System.Text;

namespace Open.Diagramming.Web.Svg
{
	public class ComplexFormatter: SolidFormatter
	{
		private XmlElement _groupElement;

		public virtual XmlElement GroupElement
		{
			get
			{
				return _groupElement;
			}
		}

		public override void WriteElement(SvgDocument document, Element element)
		{
			base.WriteElement(document, element);

			XmlNode node = Node;
			
			ComplexShape complex = (ComplexShape) element;

			//Add a group for the complex shape
			XmlElement newElement = null;

			StringBuilder builder = new StringBuilder();
			builder.Append("translate(");
			builder.Append(XmlConvert.ToString(complex.X));
			builder.Append(",");
			builder.Append(XmlConvert.ToString(complex.Y));
			builder.Append(")");

			newElement = document.CreateElement("g");
			newElement.SetAttribute("id", complex.Key + "Children");
			newElement.SetAttribute("transform", builder.ToString());

			document.ContainerNode.AppendChild(newElement);

			//Set the element as the temporary container node
			XmlNode temp = document.ContainerNode;
			string tempKey = document.ContainerKey;

			document.ContainerNode = newElement;
			document.ContainerKey = complex.Key;

			//Add each child as an element
			foreach (Solid solid in complex.Children.Values)
			{
				document.AddElement(solid);
			}

			document.ContainerNode = temp;
			document.ContainerKey = tempKey;

			//Write the ports
			
			//Restore the XmlElement
			SetNode(node);
			
			//Set the xml element for the group
			_groupElement = newElement;
		}
	}
}
