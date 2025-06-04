// (c) Copyright Crainiate Software 2009
// This source code is distributed under the terms of the GNU Lesser General Public License (LGPL).
// See http://www.gnu.org/licenses/lgpl-3.0.html and the (Licence).txt file for details.
// All other rights reserved.

using System;
using System.Xml;

namespace Open.Diagramming.Web.Svg
{
	public abstract class Formatter
	{
		private XmlNode _node;

		public virtual XmlNode Node
		{
			get
			{
				return _node;
			}
		}

		protected virtual void SetNode(XmlNode node)
		{
			_node = node;
		}
		
		public virtual void Reset()
		{
			_node = null;
		}

		public abstract void WriteElement(SvgDocument document, Element element);
		
	}
}
