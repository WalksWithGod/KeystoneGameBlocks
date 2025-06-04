using Keystone.Types;
using System.Runtime.CompilerServices;

namespace Steering.Helpers
{
    public static class MatrixHelpers
    {
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Right(ref Matrix m, ref Vector3d v)
        {
            m.M11 = v.x;
            m.M12 = v.y;
            m.M13 = v.z;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d Right(ref Matrix m)
        {
            return new Vector3d {
                x = m.M11,
                y = m.M12,
                z = m.M13
            };
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Up(ref Matrix m, ref Vector3d v)
        {
            m.M21 = v.x;
            m.M22 = v.y;
            m.M23 = v.z;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d Up(ref Matrix m)
        {
            return new Vector3d {
                x = m.M21,
                y = m.M22,
                z = m.M23
            };
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Backward(ref Matrix m, ref Vector3d v)
        {
            m.M31 = v.x;
            m.M32 = v.y;
            m.M33 = v.z;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3d Backward(ref Matrix m)
        {
            return new Vector3d {
                x = m.M31,
                y = m.M32,
                z = m.M33
            };
        }
    }
}
