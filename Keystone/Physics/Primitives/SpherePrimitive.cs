using System;
using Keystone.Types;

namespace Keystone.Physics.Primitives
{
    public class SpherePrimitive : CollisionPrimitive
    {
        public BoundingSphere BoundingSphere;
        public double Radius
        {
            get {return this.BoundingSphere.Radius;}
        }

        public SpherePrimitive(PhysicsBody body, BoundingSphere sphere)
            : base(body)
        {

            BoundingSphere = sphere;
            CenterPosition = Body.Entity.SceneNode.BoundingBox.Center; // _boundingBox.Center;
            // TODO: uncomment these
            //Body.myInternalCenterPosition = pos;

            Body.volume = (float)(((4.18878915758884 * Radius) * Radius) * Radius);

            if (Body.isPhysicallySimulated)
            {

                Body.density = Body.mass / Body.volume;
                base.findBoundingBox(0.00F);

                //Body.myInternalLinearMomentum = Toolbox.zeroVector; // init in the call to iniitliazeNonDynamic data
                //Body.myInternalAngularMomentum = Toolbox.zeroVector;
                //Body.internalInertiaTensorInverse = Matrix.Identity();
                Body.myInternalOrientationQuaternion = Quaternion.Identity();
                Body.myInternalOrientationMatrix = Matrix.Identity();
                Body.myInternalLinearVelocity = Toolbox.zeroVector;
                Body.myInternalAngularVelocity = Toolbox.zeroVector;
                Body.force = Toolbox.zeroVector;
                Body.torque = Toolbox.zeroVector;
                
                
                //Moment of inertia, also called mass moment of inertia or the angular mass, 
                //(SI units kg m2) is a measure of an object's resistance to changes in its 
                //rotation rate. It is the rotational analog of mass. That is, it is the 
                //inertia of a rigid rotating body with respect to its rotation. The moment 
                //of inertia plays much the same role in rotational dynamics as mass does in 
                //basic dynamics, determining the relationship between angular momentum and 
                //angular velocity, torque and angular acceleration, and several other quantities. 
                //While a simple scalar treatment of the moment of inertia suffices for many 
                //situations, a more advanced tensor treatment allows the analysis of such 
                //complicated systems as spinning tops and gyroscopic motion.
                double single1 = ((0.40F * Body.mass) * Radius) * Radius;
                Body.localInertiaTensor = Matrix.Identity();
                Body.localInertiaTensor.M11 = single1;
                Body.localInertiaTensor.M22 = single1;
                Body.localInertiaTensor.M33 = single1;
                Body.localInertiaTensorInverse = Matrix.Inverse(Body.localInertiaTensor);
                if (SimulationSettings.padInertiaTensors)
                {
                    Body.padInertiaTensor();
                }
                Body.scaleInertiaTensor(SimulationSettings.inertiaTensorScale);
                Body.initializePhysicalData();
            }
            else
            {
                Body.density = float.PositiveInfinity;
                base.findBoundingBox(0.00F);
                Body.initializeNonDynamicData();
            }
            base.myMaximumRadius = Radius;
        }
        
        internal override bool rayTest(Vector3d origin, Vector3d direction, double maximumLength, bool withMargin,out Vector3d hitLocation, out Vector3d hitNormal, out double t)
        {
            double single1;
            hitLocation = Toolbox.noVector;
            hitNormal = Toolbox.noVector;
            t = double.NegativeInfinity;
            if (withMargin)
            {
                single1 = Radius + Math.Max(0.00F, base.myCollisionMargin);
            }
            else
            {
                single1 = Radius;
            }
            Vector3d vector1 = origin - Body.myInternalCenterPosition;
            double single2 = Vector3d.DotProduct(vector1, direction);
            double single3 = vector1.LengthSquared() - (single1 * single1);
            if ((single3 > 0.00F) && (single2 > 0.00F))
            {
                return false;
            }
            double single4 = (single2 * single2) - single3;
            if (single4 < 0.00F)
            {
                return false;
            }
            t = -single2 - Math.Sqrt(single4);
            if (t < 0.00F)
            {
                t = 0.00F;
            }
            if (t > maximumLength)
            {
                return false;
            }
            hitLocation = origin + (t * direction);
            hitNormal = Vector3d.Normalize(hitLocation - Body.myInternalCenterPosition);
            return true;
        }
        internal override bool rayTest(Vector3d origin, Vector3d direction, double maximumLength, out Vector3d hitLocation, out Vector3d hitNormal, out double t)
        {
            throw new NotImplementedException();
        }
        public override void getExtremePoint(ref Vector3d d, ref Vector3d positionToUse, ref Quaternion orientationToUse, double margin, out Vector3d extremePoint)
        {
            Vector3d vector1 = Vector3d.Normalize(d);
            Vector3d vector2 = vector1 * (Radius + margin);
            extremePoint = positionToUse + vector2;
        }
    }
}
