using Keystone.Types;

namespace Steering.Helpers
{
    public static class LocalSpaceBasisHelpers
    {
        /// <summary>
        /// Transforms a direction in global space to its equivalent in local space.
        /// </summary>
        /// <param name="basis">The basis which this should operate on</param>
        /// <param name="globalDirection">The global space direction to transform.</param>
        /// <returns>The global space direction transformed to local space .</returns>
        public static Vector3d LocalizeDirection(this ILocalSpaceBasis basis, Vector3d globalDirection)
        {
            // dot offset with local basis vectors to obtain local coordiantes
            return new Vector3d(Vector3d.DotProduct(globalDirection, basis.Side), Vector3d.DotProduct(globalDirection, basis.Up), Vector3d.DotProduct(globalDirection, basis.Forward));
        }

        /// <summary>
        /// Transforms a point in global space to its equivalent in local space.
        /// </summary>
        /// <param name="basis">The basis which this should operate on</param>
        /// <param name="globalPosition">The global space position to transform.</param>
        /// <returns>The global space position transformed to local space.</returns>
        public static Vector3d LocalizePosition(this ILocalSpaceBasis basis, Vector3d globalPosition)
        {
            // global offset from local origin
            Vector3d globalOffset = globalPosition - basis.Position;

            // dot offset with local basis vectors to obtain local coordiantes
            return LocalizeDirection(basis, globalOffset);
        }

        /// <summary>
        /// Transforms a point in local space to its equivalent in global space.
        /// </summary>
        /// <param name="basis">The basis which this should operate on</param>
        /// <param name="localPosition">The local space position to tranform.</param>
        /// <returns>The local space position transformed to global space.</returns>
        public static Vector3d GlobalizePosition(this ILocalSpaceBasis basis, Vector3d localPosition)
        {
            return basis.Position + GlobalizeDirection(basis, localPosition);
        }

        /// <summary>
        /// Transforms a direction in local space to its equivalent in global space.
        /// </summary>
        /// <param name="basis">The basis which this should operate on</param>
        /// <param name="localDirection">The local space direction to tranform.</param>
        /// <returns>The local space direction transformed to global space</returns>
        public static Vector3d GlobalizeDirection(this ILocalSpaceBasis basis, Vector3d localDirection)
        {
        	return ((basis.Side * localDirection.x) +
                    (basis.Up * localDirection.y) +
                    (basis.Forward * localDirection.z));
        }

        /// <summary>
        /// Rotates, in the canonical direction, a vector pointing in the
        /// "forward" (+Z) direction to the "side" (+/-X) direction as implied
        /// by IsRightHanded.
        /// </summary>
        /// <param name="basis">The basis which this should operate on</param>
        /// <param name="value">The local space vector.</param>
        /// <returns>The rotated vector.</returns>
        public static Vector3d LocalRotateForwardToSide(this ILocalSpaceBasis basis, Vector3d value)
        {
            return new Vector3d(-value.z, value.y, value.x);
        }

        public static void ResetLocalSpace(out Vector3d forward, out Vector3d side, out Vector3d up, out Vector3d position)
        {
        	forward = -Vector3d.Forward();
        	side = Vector3d.Right();
        	up = Vector3d.Up();
            position = Vector3d.Zero();
        }

        /// <summary>
        /// set "side" basis vector to normalized cross product of forward and up
        /// </summary>
        /// <param name="forward"></param>
        /// <param name="side"></param>
        /// <param name="up"></param>
        public static void SetUnitSideFromForwardAndUp(ref Vector3d forward, out Vector3d side, ref Vector3d up)
        {
            // derive new unit side basis vector from forward and up
            side = Vector3d.Normalize(Vector3d.CrossProduct(forward, up));
        }

        /// <summary>
        /// regenerate the orthonormal basis vectors given a new forward
        /// (which is expected to have unit length)
        /// </summary>
        /// <param name="newUnitForward"></param>
        /// <param name="forward"></param>
        /// <param name="side"></param>
        /// <param name="up"></param>
        public static void RegenerateOrthonormalBasisUF(Vector3d newUnitForward, out Vector3d forward, out Vector3d side, ref Vector3d up)
        {
            forward = newUnitForward;

            // derive new side basis vector from NEW forward and OLD up
            SetUnitSideFromForwardAndUp(ref forward, out side, ref up);

            // derive new Up basis vector from new Side and new Forward
            //(should have unit length since Side and Forward are
            // perpendicular and unit length)
            up = Vector3d.CrossProduct(side, forward);
        }

        /// <summary>
        /// for when the new forward is NOT know to have unit length
        /// </summary>
        /// <param name="newForward"></param>
        /// <param name="forward"></param>
        /// <param name="side"></param>
        /// <param name="up"></param>
        public static void RegenerateOrthonormalBasis(Vector3d newForward, out Vector3d forward, out Vector3d side, ref Vector3d up)
        {
            RegenerateOrthonormalBasisUF(Vector3d.Normalize(newForward), out forward, out side, ref up);
        }

        /// <summary>
        /// for supplying both a new forward and and new up
        /// </summary>
        /// <param name="newForward"></param>
        /// <param name="newUp"></param>
        /// <param name="forward"></param>
        /// <param name="side"></param>
        /// <param name="up"></param>
        public static void RegenerateOrthonormalBasis(Vector3d newForward, Vector3d newUp, out Vector3d forward, out Vector3d side, out Vector3d up)
        {
            up = newUp;
            RegenerateOrthonormalBasis(Vector3d.Normalize(newForward), out forward, out side, ref up);
        }

        public static Matrix ToMatrix(this ILocalSpaceBasis basis)
        {
            return ToMatrix(basis.Forward, basis.Side, basis.Up, basis.Position);
        }

        public static Matrix ToMatrix(Vector3d forward, Vector3d side, Vector3d up, Vector3d position)
        {
        	Matrix m = Matrix.Identity();
        	m.SetTranslation (position);
            MatrixHelpers.Right(ref m, ref side);
            MatrixHelpers.Up(ref m, ref up);
            MatrixHelpers.Right(ref m, ref forward);

            return m;
        }

        public static void FromMatrix(Matrix transformation, out Vector3d forward, out Vector3d side, out Vector3d up, out Vector3d position)
        {
        	position = transformation.GetTranslation();
            side = MatrixHelpers.Right(ref transformation);
            up = MatrixHelpers.Up(ref transformation);
            forward = MatrixHelpers.Backward(ref transformation);
        }
    }
}
