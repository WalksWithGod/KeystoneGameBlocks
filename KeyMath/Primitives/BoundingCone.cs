using System;

namespace Keystone.Types
{
    public class BoundingCone
    {
        private float mSinReciprocal;
        private float mSinSquared;
        private float mCosReciprocal;
        private float mCosSquared;
        private float mFovRadians;

        private Vector3d mAxis, mPosition;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="axis">Direction vector</param>
        /// <param name="fovRadians"></param>
        public BoundingCone(Vector3d position, Vector3d axis, float fovRadians)
        {
            Init(position, axis, fovRadians);
        }

        public BoundingCone(float fovRadians, Vector3d cameraPosition,
                            Vector3d cameraLookAt, float viewportWidth, float viewportHeight)
        {
           double halfViewportHeight = viewportHeight * 0.5;
           double halfViewportWidth = viewportWidth * 0.5;

            // calculate the length of the field of view triangle
            double depth = (double) (halfViewportHeight / Math.Tan(fovRadians * 0.5f));

            // calc the corner of the screen
            double corner = (double) Math.Sqrt(halfViewportWidth * halfViewportWidth) +
                                               halfViewportHeight * halfViewportHeight;

            // update the new fovRadians
            fovRadians = (float) Math.Atan(corner / depth);

            Init(cameraPosition, cameraLookAt - cameraPosition, fovRadians);
        }

        private void Init(Vector3d position, Vector3d axis, float fovRadians)
        {
            mFovRadians = fovRadians;
            mAxis = axis;
            mPosition = position;

            mSinReciprocal = (float) (Math.Sin(fovRadians));
            mSinSquared = mSinReciprocal * mSinReciprocal;
            mCosReciprocal = (float) (Math.Cos(fovRadians));
            mCosSquared = mCosReciprocal * mCosReciprocal;
        }

        public Vector3d Position
        {
            get { return mPosition; }
        }

        public Vector3d Axis
        {
            get { return mAxis; }
        }

        public float SinReciprocal
        {
            get { return mSinReciprocal; }
        }

        public double CosReciprocal
        {
            get { return mCosReciprocal; }
        }

        public double SinSquared
        {
            get { return mSinSquared; }
        }

        public double CosSquared
        {
            get { return mCosSquared; }
        }

        public double FOVRadians
        {
            get { return mFovRadians; }
        }

        public IntersectResult Intersects(BoundingSphere sphere)
        {
            double dotSquare;
            double e;
            Vector3d D, U;

            U = mAxis*(sphere.Radius - mSinReciprocal); // TODO: replace with non tv dot product 

            U.x = mPosition.x - U.x;
            U.y = mPosition.y - U.y;
            U.z = mPosition.z - U.z;

            D = sphere.Center - U;
                // TODO: replace with non tv dot product // TODO: this comment is here, did i accidentally delete something here?  maybe was just a TVMath.VSubtract before and then forgot to delete the comment when i changed to use overloaded -

            dotSquare = Vector3d.DotProduct(D, D); // TODO: replace with non tv dot product 

            e = Vector3d.DotProduct(mAxis, D); // TODO: replace with non tv dot product 

            if (e > 0 && e * e >= dotSquare * mCosSquared)
            {
                D = sphere.Center - mPosition;

                dotSquare = Vector3d.DotProduct(D, D); // TODO: replace with non tv dot product 

                e = -(Vector3d.DotProduct(mAxis, D)); // TODO: replace with non tv dot product 

                if (e > 0 && e * e >= dotSquare * mSinSquared)
                    if (dotSquare <= sphere.Radius * sphere.Radius)
                        return IntersectResult.INSIDE;
                    else
                        return IntersectResult.OUTSIDE;
                else
                    return IntersectResult.INSIDE;
            }
            return IntersectResult.OUTSIDE;
        }

//        public Vector3d [] GetVertices(int numSlices, double length)
//        {
//            // to create the vertices we simply find one point
//            // and then rotate it about the cone's axis

//            Vector3d axisEndPoint = Core._Core.Maths.VSubtract(Translation, (Core._Core.Maths.VScale(_axis, length))); 

//            // now find a 3rd point such that the angle to it from the position is theta and that is also 90 to the endpoint
//            // axis is the unit vector in the direction of axisEndPoint-->Translation

//            // then newV in the direction of axisEndPoint --> newPoint =
//            // newV = (Math.Cos(axisEndPoint.X) 

//            //Well, then it's easy. I assume the angle B goes clockwise from
//            //BA to BC. If u=(u1,u2) is the unit vector
//            //(x1-x2, y1-y2)/sqrt((x1-x2)^2+(y1-y2)^2) in the direction of the
//            //vector BA, then the unit vector in the direction of BC is
//            //v = (cos(B) u1 - sin(B) u2, cos(B) u2 + sin(B) u1),
//            //and C = B + |BC| v.

//        }
    }
}