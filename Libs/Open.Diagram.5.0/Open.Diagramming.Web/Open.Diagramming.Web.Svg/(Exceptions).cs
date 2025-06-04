// (c) Copyright Crainiate Software 2009
// This source code is distributed under the terms of the GNU Lesser General Public License (LGPL).
// See http://www.gnu.org/licenses/lgpl-3.0.html and the (Licence).txt file for details.
// All other rights reserved.

using System;

namespace Open.Diagramming.Web.Svg
{
	public class DefinitionException: Exception 
	{
		public DefinitionException(string message) : base(message)
		{}
	}
	public class SvgDocumentException: Exception 
	{
		public SvgDocumentException(string message) : base(message)
		{}
	}
	public class PolylineException: Exception 
	{
		public PolylineException(string message) : base(message)
		{}
	}
	public class TextException: Exception 
	{
		public TextException(string message) : base(message)
		{}
	}
}
