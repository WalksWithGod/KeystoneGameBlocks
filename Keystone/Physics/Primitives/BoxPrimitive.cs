using System;
using System.Collections.Generic;
using Keystone.Types;

namespace Keystone.Physics.Primitives
{
    public class BoxPrimitive : CollisionPrimitive
    {
        public BoxPrimitive(PhysicsBody physicsBody, BoundingBox box)
            : base(physicsBody)
        {
            _boundingBox = box;

            // TODO: there's no point setting this.  right now i just need to fix a bug so i can delete this crap
            CenterPosition = physicsBody.Entity.SceneNode.BoundingBox.Center; // _boundingBox.Center;


            //Body.initialize(volume, den);
            // initialization main things that are different depending on the Primitive is volume and the localInertia Tensor
            //TODO: also, the physicallySimulated needs to update and compute things when it's changed... shouldnt have to
            // set this paremeter before the constructor is called just so we wont skip doing this init
            if (Body.isPhysicallySimulated)
            {
                base.findBoundingBox(0.00F);
                Body.volume = (float)(_boundingBox.Depth * _boundingBox.Width * _boundingBox.Height);
                Body.density = Body.mass / Body.volume;
                
                double single1 = HalfWidth * HalfWidth;
                double single2 = HalfHeight * HalfHeight;
                double single3 = HalfLength * HalfLength;
                double single4 = 0.33d;
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
                Body.localInertiaTensor = Matrix.Identity();
                Body.localInertiaTensor.M11 = (Body.mass * (single2 + single3)) * single4;
                Body.localInertiaTensor.M22 = (Body.mass * (single1 + single3)) * single4;
                Body.localInertiaTensor.M33 = (Body.mass * (single1 + single2)) * single4;
                
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
                base.findBoundingBox(0.00F);
                Body.volume = (float)(_boundingBox.Depth * _boundingBox.Width * _boundingBox.Height);
                Body.density = float.PositiveInfinity;
                Body.initializeNonDynamicData();
            }
            // get hypotenus length for max radius
            myMaximumRadius =  Math.Sqrt((((HalfHeight * HalfHeight) + (HalfLength * HalfLength)) + (HalfWidth * HalfWidth)));

            // these primitives shoudl be allowed to directly reference child primitives so that we effectively have collision trees?
            if (!(Body is CompoundBody)) return;
            foreach (PhysicsBody child in ((CompoundBody)Body).subBodies)
                myMaximumRadius += child.CollisionPrimitive.myMaximumRadius;

        }

        internal override bool rayTest(Vector3d origin, Vector3d direction, double maximumLength, bool withMargin, out Vector3d hitLocation, out Vector3d hitNormal, out double t)
        {
            throw new NotImplementedException();
        }
        internal override bool rayTest(Vector3d origin, Vector3d direction, double maximumLength, out Vector3d hitLocation, out Vector3d hitNormal, out double t)
        {
            throw new NotImplementedException();
        }

        public override void getExtremePoint(ref Keystone.Types.Vector3d d, ref  Keystone.Types.Vector3d positionToUse, ref Keystone.Types.Quaternion orientationToUse, double margin, out Keystone.Types.Vector3d extremePoint)
        {
            Quaternion quaternion1 = Quaternion.Conjugate(orientationToUse);
             Vector3d vector1 = Vector3d.TransformCoord(d, quaternion1);
             extremePoint = new Vector3d(Math.Sign(vector1.x) * HalfWidth, Math.Sign(vector1.y) * HalfHeight, Math.Sign(vector1.z) * HalfLength);
             extremePoint = Vector3d.TransformCoord(extremePoint, orientationToUse);
             extremePoint =  extremePoint + positionToUse;
             Vector3d vector2 = Vector3d.Normalize(d);
             vector2 *=  margin;
             extremePoint  += vector2;
        }
    }
}
