// (c) Copyright Crainiate Software 2009
// This source code is distributed under the terms of the GNU Lesser General Public License (LGPL).
// See http://www.gnu.org/licenses/lgpl-3.0.html and the (Licence).txt file for details.
// All other rights reserved.

namespace Open.Diagramming
{
	public class LabelException : System.Exception 
	{
		public LabelException(string message) : base(message)
		{}
	}

	public class AttributeRequiredException : System.Exception 
	{
		public AttributeRequiredException(string message) : base(message)
		{}
	}

	public class ComponentException : System.Exception 
	{
		public ComponentException(string message) : base(message)
		{}
	}

	public class ElementsException : System.Exception 
	{
		public ElementsException(string message) : base(message)
		{}
	}

	public class CollectionNotModifiableException: System.Exception
	{
		public CollectionNotModifiableException(string message) :base(message)
		{}
	}

	public class LayerException : System.Exception 
	{
		public LayerException(string message) :base(message)
		{}
	}

	public class InvalidPointException : System.ArgumentException 
	{
		public InvalidPointException(string message) : base(message)
		{
		}
	}

	public class GroupException : System.Exception 
	{
		public GroupException(string message) : base(message)
		{
		}
	}

	public class ComplexShapeException : System.Exception 
	{
		public ComplexShapeException(string message) : base(message)
		{
		}
	}

	public class ComplexLineException : System.Exception 
	{
		public ComplexLineException(string message) : base(message)
		{
		}
	}

	public class CurveException : System.Exception 
	{
		public CurveException(string message) : base(message)
		{
		}
	}

	public class ConnectorException : System.Exception 
	{
		public ConnectorException(string message) : base(message)
		{
		}
	}

	public class MetafileException : System.Exception 
	{
		public MetafileException(string message) : base(message)
		{
		}
	}

	public class DiagramException : System.Exception 
	{
		public DiagramException(string message) : base(message)
		{
		}
	}

	public class TableItemsException : System.Exception 
	{
		public TableItemsException(string message) : base(message)
		{
		}
	}
}
