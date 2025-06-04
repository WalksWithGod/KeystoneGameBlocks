// (c) Copyright Crainiate Software 2009
// This source code is distributed under the terms of the GNU Lesser General Public License (LGPL).
// See http://www.gnu.org/licenses/lgpl-3.0.html and the (Licence).txt file for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Open.Diagramming.Forms
{
    //Interface for a designer control for editing labels at runtime
	public interface ILabelEdit
	{
		StringFormat StringFormat {get; set;}
		string Text {get; set;}
		bool Visible {get; set;}
		bool Cancelled {get;}
		float Zoom {get; set;}
		AutoSizeMode AutoSizeMode {get; set;}

		void SendEnd();
		void SendHome();

		event EventHandler Complete;
		event EventHandler Cancel;
	}
}

