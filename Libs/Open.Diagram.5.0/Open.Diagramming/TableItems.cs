// (c) Copyright Crainiate Software 2009
// This source code is distributed under the terms of the GNU Lesser General Public License (LGPL).
// See http://www.gnu.org/licenses/lgpl-3.0.html and the (Licence).txt file for details.
// All other rights reserved.

using System;
using System.Runtime.Serialization;
using Open.Diagramming.Collections;

namespace Open.Diagramming
{
	public abstract class TableItems<T>: List<T>
        where T: TableItem
	{

		public event TableItemsEventHandler InsertItem;
		public event TableItemsEventHandler RemoveItem;
		public event EventHandler ClearList;

		public override void  Clear()
        {
 	        base.Clear();
			if (ClearList != null) ClearList(this,EventArgs.Empty);
		}

        protected internal override void OnInserted(T item)
        {
            if (InsertItem != null) InsertItem(this, new TableItemsEventArgs(item));
        }

        protected internal override void OnRemove(T item)
        {
            if (RemoveItem != null) RemoveItem(this, new TableItemsEventArgs(item));
        }
	}
}