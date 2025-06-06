using Keystone.Physics;
using Keystone.Types;
using System;

namespace Keystone.Physics
{
	internal class SortableEntity : IComparable<SortableEntity>
	{
        // Instance Fields
        internal PhysicsBody e;
        internal Vector3d axis;

		// Constructors
		internal SortableEntity (PhysicsBody e)
		{
			this.e = e;
			axis = Toolbox.noVector;
		}
		
		
		// Methods
		public int CompareTo (SortableEntity s)
		{
            double single1 = Math.Max(Vector3d.DotProduct(axis, e.CollisionPrimitive.BoundingBox.Min),
                Vector3d.DotProduct(axis, e.CollisionPrimitive.BoundingBox.Max));
            double single2 = Math.Max(Vector3d.DotProduct(axis, s.e.CollisionPrimitive.BoundingBox.Min),
                Vector3d.DotProduct(axis, s.e.CollisionPrimitive.BoundingBox.Max));
			if (single1 > single2)
			{
				return 1;
			}
			if (single1 < single2)
			{
				return -1;
			}
			return 0;
		}
	}
}
