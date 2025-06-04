using System;
using Keystone.Types;

namespace Keystone.Physics.Entities
{
    public interface ICollisionPrimitive
    {
        Triangle[] Faces { get; set; }
        Vector3d[] Vertices { get; }
        void RayTest();

        void getExtremePoint(ref Vector3d d, ref Vector3d positionToUse, ref Quaternion orientationToUse, float margin,
                             out Vector3d extremePoint);
        
    }
}
