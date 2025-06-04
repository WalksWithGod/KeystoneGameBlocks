// (c) Copyright Crainiate Software 2009
// This source code is distributed under the terms of the GNU Lesser General Public License (LGPL).
// See http://www.gnu.org/licenses/lgpl-3.0.html and the (Licence).txt file for details.
// All other rights reserved.

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections;

namespace Open.Diagramming
{
	public class Handle
	{
		//Property variables
		private GraphicsPath _graphicsPath;
		private HandleType _type;
		private bool _dock;

		public Handle()
		{
			Type = HandleType.Move;
			CanDock = false;
		}

		public Handle(HandleType type)
		{
			Type = type;
			CanDock = false;
		}

		public Handle(GraphicsPath path, HandleType type)
		{
			Type = type;
			Path = path;
			CanDock = false;
		}

		public Handle(GraphicsPath path, HandleType type, bool canDock)
		{
			Type = type;
			Path = path;
			CanDock = canDock;
		}

		public virtual HandleType Type
		{
			get
			{
				return _type;
			}
			set
			{
				_type = value;
			}
		}

		public virtual GraphicsPath Path
		{
			get
			{
				return _graphicsPath;
			}
			set
			{
				_graphicsPath = value;
			}
		}

		public virtual bool CanDock
		{
			get
			{
				return _dock;
			}
			set
			{
				_dock = value;
			}
		}
	}

	public class ConnectorHandle: Handle
	{
		private int mIndex;

		public ConnectorHandle()
		{
			Type = HandleType.Move;
		}

		public ConnectorHandle(GraphicsPath path, HandleType type, int index): base(path, type)
		{
			mIndex = index;
		}

		public virtual int Index
		{
			get
			{
				return mIndex;
			}
			set
			{
				mIndex = value;
			}
		}
	}

	public class ExpandHandle: Handle
	{
		private Segment mSegment;
		private int mIndex;

		public ExpandHandle(Segment segment)
		{
			Type = HandleType.Expand;
			Segment = segment;
		}

		public virtual Segment Segment
		{
			get
			{
				return mSegment;
			}
			set
			{
				mSegment = value;
			}
		}

		public virtual int Index
		{
			get
			{
				return mIndex;
			}
			set
			{
				mIndex = value;
			}
		}
	}

	sealed public class Handles: CollectionBase
	{
		public Handles()
		{

		}

		//Collection indexers
		public Handle this[int index]  
		{
			get  
			{
				return (Handle) List[index];
			}
			set  
			{
				List[index] = value;
			}
		}

		//Adds an Handle to the list 
		public int Add(Handle value)  
		{
			if (value == null) throw new ArgumentNullException("Handle parameter cannot be null reference.","value");
			return List.Add(value);
		}

		//Inserts an elelemnt into the list
		public void Insert(int index, Handle value)  
		{
			if (value == null) throw new ArgumentNullException("Handle parameter cannot be null reference.","value");
			List.Insert(index, value);
		}

		//Removes an Handle from the list
		public void Remove(Handle value )  
		{
			List.Remove(value);
		}

		//Returns true if list contains Handle
		public bool Contains(Handle value)  
		{
			return List.Contains(value);
		}

		//Returns the index of an Handle
		public int IndexOf(Handle value)  
		{
			return List.IndexOf(value);
		}
	}
}
