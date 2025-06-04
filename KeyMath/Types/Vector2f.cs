using System;

namespace Keystone.Types
{
    public struct Vector2f
    {
        public float x, y;

        public Vector2f(string s)
        {
            char[] delimiterChars = { ' ', ',' };
            string[] sToks = s.Split(delimiterChars);
            x = float.Parse(sToks[0]);
            y = float.Parse(sToks[1]);
        }

        public static Vector2f Parse(string s)
        {
            char[] delimiterChars = { ' ', ',' };
            string[] sToks = s.Split(delimiterChars);
            return new Vector2f(float.Parse(sToks[0]), float.Parse(sToks[1]));
        }

        public Vector2f(float Vx, float Vy)
        {
            x = Vx;
            y = Vy;
        }

        public static Vector2f Zero()
        {
            Vector2f v;
            v.x = v.y = 0f;
            return v;
        }

        //private static Vector2f RandomVector()
        //{
        //    Random rand = new Random();
        //    float angle = (float)(rand.NextDouble() * Utilities.MathHelper.TWO_PI);

        //    Vector2f velocity = new Vector2f((float)Math.Cos(angle), (float)Math.Sin(angle));
        //    velocity *= initialVelocity;

        //}

        public float Length
        {
            get { return (float)Math.Sqrt(LengthSquared); }
        }

        public float LengthSquared
        {
            get {return (x * x + y * y);}
        }

        public static float GetDistance2D(Vector2f v1, Vector2f v2)
        {
            return (v1 - v2).Length;
        }


        public static float GetDistance2DSquared(Vector2f v1, Vector2f v2)
        {
            return (v1 - v2).LengthSquared;
        }

        public static Vector2f Normalize(Vector2f vec)
        {
            float t = vec.Length;
            return new Vector2f(vec.x / t, vec.y / t);
        }

        public static Vector2f Normalize(Vector2f vec, out float length)
        {
            length = vec.Length;;
            return new Vector2f(vec.x / length, vec.y / length);
        }

        // TODO: I THINK using the .m2X parameters is required for some other
        //       methods and .m3X is for interior rotating our footprints.  But as i recall
        //       we use Vector2f TransformCoord for some 2d elements like star billboards
        public static Vector2f TransformCoord(Vector2f v, Matrix m)
        {
            Vector2f result;
            //result.x = (float)((v.x * m.M11) + ((v.y * m.M21) + m.M41));
            //result.y = (float)((v.x * m.M12) + ((v.y * m.M22) + m.M42));

            // TODO: below is proper _if_ i'm ignoring the y component of the matrix and treating
            //       result.x and result.y as result.x and result.z with a missing y component if this were
            //       a 3d vector.  thus we multiply with the x and z components of the matrix
            result.x = (float)((v.x * m.M11) + ((v.y * m.M31) + m.M41));
            result.y = (float)((v.x * m.M13) + ((v.y * m.M33) + m.M43));
            return result;
        }

        public static Vector2f Inverse(Vector2f v)
        {
            return new Vector2f(-v.x, -v.y);
        }

        // dot productive is commutative (i.e.  v1 dot v2 == v2 dot v1)
        public static float DotProduct(Vector2f v1, Vector2f v2)
        {
            return (v1.x * v2.x + v1.y * v2.y);
        }

        public static Vector2f Subtract(Vector2f v1, Vector2f v2)
        {
            return new Vector2f(v1.x - v2.x, v1.y - v2.y);
        }

        public static Vector2f Add(Vector2f v1, Vector2f v2)
        {
            return new Vector2f(v1.x + v2.x, v1.y + v2.y);
        }

        public static Vector2f Scale(Vector2f v, float scale)
        {
            return new Vector2f(v.x * scale, v.y * scale);
        }


        // clamp the vector's magnitude or length to the limit length
        public static Vector2f Limit(Vector2f vec, float limit)
        {
            if (vec.Length > limit)
                return Normalize(vec) * limit;

            return vec;
        }


        public static Vector2f Lerp(Vector2f start, Vector2f end, float weight)
        {
            return (start * (1 - weight)) + (end * weight);
        }

        public static bool operator ==(Vector2f v1, Vector2f v2)
        {
            return (v1.x == v2.x && v1.y == v2.y );
        }

        public static bool operator !=(Vector2f v1, Vector2f v2)
        {
            return !(v1 == v2);
        }

        public static Vector2f operator -(Vector2f v1, Vector2f v2)
        {
            return Subtract(v1, v2);
        }

        public static Vector2f operator +(Vector2f v1, Vector2f v2)
        {
            return Add(v1, v2);
        }

        public static Vector2f operator *(Vector2f v1, float value)
        {
            return Scale(v1, value);
        }

        public static Vector2f operator /(Vector2f v1, float value)
        {
            return Scale(v1, 1 / value);
        }

        public static Vector2f FromTV2DVector(Vector2f v)
        {
            return new Vector2f(v.x, v.y);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Vector2f))
                return base.Equals(obj);
            else
                return (this == (Vector2f)obj);
        }

        //public override int GetHashCode()
        //{
        //    return base.GetHashCode();
        //}
        public override string ToString()
        {
            return string.Format("{0},{1}", x, y);
        }
    }
}