// (c) Copyright Crainiate Software 2009
// This source code is distributed under the terms of the GNU Lesser General Public License (LGPL).
// See http://www.gnu.org/licenses/lgpl-3.0.html and the (Licence).txt file for details.
// All other rights reserved.

using System;

namespace Open.Diagramming.Layouts
{
	public class GraphException : System.Exception 
	{
		public GraphException(string message) : base(message)
		{}
	}
	public class LayoutException : System.Exception 
	{
		public LayoutException(string message) : base(message)
		{}
	}
}
