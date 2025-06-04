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
	public class TablePort: Port
	{
		//Property variables
		TableItem _tableItem;

        public TablePort(TableItem tableItem): base(0F)
        {
            TableItem = tableItem;
            Fixed = true;
        }

        public virtual TableItem TableItem
        {
            get
            {
                return _tableItem;
            }
            set
            {
                _tableItem = value;
            }
        }

        public override bool AllowMove
        {
            get
            {
                return false;
            }
            set
            {
                throw new ArgumentException("Table ports cannot be moved.");
            }
        }

        public override bool Fixed
        {
            get
            {
                return base.Fixed;
            }
            set
            {
                if (!value) throw new ArgumentException("Table ports must be fixed.");
                base.Fixed = true;
            }
        }
	}
}
