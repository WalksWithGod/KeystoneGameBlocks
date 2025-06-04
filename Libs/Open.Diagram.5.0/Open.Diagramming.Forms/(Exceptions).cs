// (c) Copyright Crainiate Software 2009
// This source code is distributed under the terms of the GNU Lesser General Public License (LGPL).
// See http://www.gnu.org/licenses/lgpl-3.0.html and the (Licence).txt file for details.
// All other rights reserved.

namespace Open.Diagramming.Forms
{
    public class RenderException : System.Exception
    {
        public RenderException(string message): base(message)
		{}
    }
	public class UndoPointException : System.Exception 
	{
		public UndoPointException(string message) : base(message)
		{}
	}

	public class TabsException : System.Exception 
	{
		public TabsException(string message) : base(message)
		{}
	}
}
