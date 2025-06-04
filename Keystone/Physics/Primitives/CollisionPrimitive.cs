using System;
using Keystone.Types;

namespace Keystone.Physics.Primitives
{
    public abstract class CollisionPrimitive // : ICollisionPrimitive ?
    {
        public PhysicsBody Body;
        protected BoundingBox _boundingBox;

        private double _halfHeight;
        private double _halfWidth;
        private double _halfLength;

        public double HalfHeight { get { return Body.Entity.SceneNode.BoundingBox.Height * .5d; } }
        public double HalfWidth { get { return Body.Entity.SceneNode.BoundingBox.Width * .5d; } }
        public double HalfLength { get { return Body.Entity.SceneNode.BoundingBox.Depth * .5d; } }
        internal double myMaximumRadius; // this is the  hypotenuse of the axis aligned bounding box and represents the distance from center of box to any corner

        internal float myCollisionMargin;
        internal float myCollisionMarginSquared;
        internal float myAllowedPenetration;
       
        public CollisionPrimitive (PhysicsBody body)
        {
            if (body == null) throw new ArgumentNullException();
            if (body.CollisionPrimitive != null)
                throw new Exception("Physics body already has a collision primitive.  Remove the existing first!");
            Body = body;
            Body.CollisionPrimitive = this;

        }

        // TODO: his code has so many methods that accept this param byref and properties cannot be passed by ref
        // so we just make it a public variable
        public Vector3d CenterPosition;

        //public void Translate (Vector3d translation)
        //{
        //    _boundingBox.Min +=  translation;
        //    _boundingBox.Max +=  translation;
        //    CenterPosition = _boundingBox.Center;

        //}
       
        //// the reason this is failing is because the physics should be moving the entity and not the damn box
        //public void Move (Vector3d position)
        //{
        //    Vector3d min;
        //    Vector3d max;
            
        //    min.x = _boundingBox.Min.x -  HalfWidth;
        //    max.x = min.x + _boundingBox.Width;

        //    min.y = _boundingBox.Min.y - HalfHeight;
        //    max.y = min.y + _boundingBox.Height ;

        //    min.z = _boundingBox.Min.z - HalfLength;
        //    max.z = min.z + _boundingBox.Depth;

        //    _boundingBox = new BoundingBox(min, max);
        //    CenterPosition = _boundingBox.Center;
        //}
        internal abstract bool rayTest(Vector3d origin, Vector3d direction, double maximumLength, bool withMargin,
                                       out Vector3d hitLocation, out Vector3d hitNormal, out double t);

        internal abstract bool rayTest(Vector3d origin, Vector3d direction, double maximumLength,
                                       out Vector3d hitLocation, out Vector3d hitNormal, out double t);
 
        public BoundingBox BoundingBox
        {
            get { return _boundingBox; }

        }

        // TODO: i might make it perhaps so that a collision primitive of a certain type
        // will host both it's local model space faces and verts as well as
        // the world (or region) space versions.  Not sure yet... long ways to
        // go before we get this shit working
        public virtual void getExtremePoints(ref Vector3d d, ref Vector3d positionToUse, ref Quaternion orientationToUse, out Vector3d min, out Vector3d max, double margin)
        {
            Vector3d vector1 = Vector3d.Negate(d);
            getExtremePoint(ref vector1, ref positionToUse, ref orientationToUse, margin, out min);
            getExtremePoint(ref d, ref positionToUse, ref orientationToUse, margin, out max);
        }

        public virtual void getExtremePoints(ref Vector3d d, out Vector3d min, out Vector3d max, double margin)
        {
            Vector3d vector1 = Vector3d.Negate(d);
            Vector3d center = _boundingBox.Center; // TODO: temp because byref arg needs to have a setter and _boundingBox.Center does not
            getExtremePoint(ref vector1, ref center, ref Body.myInternalOrientationQuaternion, margin, out min);
            getExtremePoint(ref d, ref center, ref Body.myInternalOrientationQuaternion, margin, out max);
        }

        public void getExtremePoint(ref Vector3d d, double margin, out Vector3d extremePoint)
        {
            Vector3d center = _boundingBox.Center; // TODO: temp because byref arg needs to have a setter and _boundingBox.Center does not

            getExtremePoint(ref d, ref center, ref Body.myInternalOrientationQuaternion, margin, out extremePoint);
        }

        public abstract void getExtremePoint(ref Vector3d d, ref Vector3d positionToUse, ref Quaternion orientationToUse, double margin, out Vector3d extremePoint);


        //Time between frames to use in expanding the bounding box based on the object's velocity. Pass in 0 to ignore velocity.
        /// <summary>
        /// Computes a swept bounding box based on the direction and velocity of the object and the time elapsed in seconds
        /// </summary>
        /// <param name="dt"></param>
        public void findBoundingBox(float dt)
        {
            if (this.Body is CompoundBody)
            {
                CompoundBody body1 = (CompoundBody)this.Body;
                if (body1.subBodies.Count > 0)
                {
                    foreach (PhysicsBody entity1 in body1.subBodies)
                    {
                        entity1.CollisionPrimitive.findBoundingBox(dt);
                    }
                    BoundingBox box1 = body1.subBodies[0].CollisionPrimitive.BoundingBox;
                    for (int i = 1; i < body1.subBodies.Count; i++)
                    {
                        box1.Combine(body1.subBodies[i].CollisionPrimitive.BoundingBox);
                    }
                   _boundingBox = box1;
                }
            }
            else
            {
                double margin;
                if (((this is BoxPrimitive) && (Body.space != null)) && Body.space.simulationSettings.useSpecialCaseBoxBox)
                {
                    margin = myCollisionMargin * 1.42F;
                }
                else
                {
                    margin = myCollisionMargin;
                }
                                
                // getExtremePoints here i think means it's computing an Object Oriented Bounding Box
                // TODO: im not sure how often this gets called but it should update whenever the orientation matrix changes...
                // TODO: and also it seems here i take world bounding box and i orient it but seems i should be using the local space box
                // and then grab the extreme points and then translate it??  hrm... dunno yet.  This could be part of why the incoming
                // demo fails... if the bounding boxes arent correct
                // in fact now i see that it does use local space but then in the override getExtremePoint() inside each collisionPrimitive type
                // it uses the halfwidth, halfheight, halflength to compute the world OOBB
                Vector3d minX, maxX, minY, maxY, minZ, maxZ;


                Vector3d center = Body.Entity.SceneNode.BoundingBox.Center; // Body.myInternalCenterPosition; // 
                getExtremePoints(ref Toolbox.rightVector, ref center, ref Body.myInternalOrientationQuaternion, out minX, out maxX, margin);
                getExtremePoints(ref Toolbox.upVector, ref center, ref Body.myInternalOrientationQuaternion, out minY, out maxY, margin);
                getExtremePoints(ref Toolbox.backVector, ref center, ref Body.myInternalOrientationQuaternion, out minZ, out maxZ, margin);

                Vector3d min = new Vector3d(minX.x, minY.y, minZ.z);
                Vector3d max = new Vector3d(maxX.x, maxY.y, maxZ.z);

                if (dt != 0.00d)
                {
                    // find's swept box by adding based on the velocity and delta time in seconds
                    if (Body.space != null)
                    {
                        if ((Body.space.simulationSettings.collisionDetectionType == CollisionDetectionType.fullyContinuous)
                        && !(this is SpherePrimitive))
                        {
                            double single8 = Math.Min((Body.myInternalAngularVelocity.Length * dt *
                                (myMaximumRadius + myCollisionMargin)), myMaximumRadius);
                            double single8_x_two = single8*2d;

                            if ((max.x - min.x) < single8_x_two)
                            {
                                min.x = ((min.x + max.x) * 0.5d) - single8;
                                max.x = min.x + single8_x_two;
                            }
                            if ((max.y - min.y) < single8_x_two)
                            {
                                min.y = ((min.y + max.y) * 0.5d) - single8;
                                max.y = min.y + single8_x_two;
                            }
                            if ((max.z - min.z) < single8_x_two)
                            {
                                min.z = ((min.z + max.z) * 0.5d) - single8;
                                max.z = min.z + single8_x_two;
                            }
                        }
                        else
                        {
                            Vector3d vector7 = Body.myInternalLinearVelocity * dt;
                            if (vector7.x > 0.00D)
                            {
                                max.x += vector7.x;
                            }
                            else
                            {
                                min.x += vector7.x;
                            }
                            if (vector7.y > 0.00D)
                            {
                                max.y += vector7.y;
                            }
                            else
                            {
                                min.y += vector7.y;
                            }
                            if (vector7.z > 0.00D)
                            {
                                max.z += vector7.z;
                            }
                            else
                            {
                                min.z += vector7.z;
                            }
                        }
                    }
                }

                
                _boundingBox.Min = min;
                _boundingBox.Max = max;
            }
            CenterPosition =  _boundingBox.Center;
        }

    }
}
