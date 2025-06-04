using System;
using System.Collections.Generic;


namespace Keystone.Types
{
    /// <summary>
    /// A transform can be shared by multiple objects.  An Entity's Transformation object
    /// can be shared by the physics Body and by a IK Link node.  In this way Physics or IK solver
    /// need know nothing about the Entity itself.  
    /// </summary>
    public class Transformation
    {
        private Vector3d mPosition;
        private Vector3d mScale;
        private Vector3d mRotation;

        // http://unity3d.com/support/documentation/ScriptReference/Transform.html
        
        private Matrix mMatrix;

        public static Transformation Multiply(Transformation t1, Transformation t2)
        {
            return t1 * t2;
        }

        public static Transformation operator *(Transformation t1, Transformation t2)
        {
            // usually used for computing result of hierarchical transforms
            return new Transformation();
        }
    }
}
