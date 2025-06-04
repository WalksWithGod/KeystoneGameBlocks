// (c) Copyright Crainiate Software 2009
// This source code is distributed under the terms of the GNU Lesser General Public License (LGPL).
// See http://www.gnu.org/licenses/lgpl-3.0.html and the (Licence).txt file for details.
// All other rights reserved.

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;

namespace Open.Diagramming
{
	public class TableRow: TableItem, ICloneable
	{
		//Property variables
		private Open.Diagramming.Image _image;
				
		//Constructors
		public TableRow()
		{
			
		}

        public TableRow(string text)
        {
            Text = text;
        }

		public TableRow(TableRow prototype): base(prototype)
		{
			_image = prototype.Image;
		}

		//Properties
		public virtual Open.Diagramming.Image Image
		{
			get
			{
				return _image;
			}
			set
			{
				_image = value;
				OnTableItemInvalid();
			}
		}
	
		public virtual object Clone()
		{
			return new TableRow(this);
		}
	}
}
