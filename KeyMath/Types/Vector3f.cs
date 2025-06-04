using System;
using System.ComponentModel;

namespace Keystone.Types
{
    [TypeConverter(typeof(Keystone.TypeConverters.Vector3fConverter))]
    public struct Vector3f
    {
        public float x, y, z;

        public Vector3f(float Vx, float Vy, float Vz)
        {
            x = Vx;
            y = Vy;
            z = Vz;
        }

        public Vector3f(string delimitedString)
        {
            Vector3f parse = Vector3f.Parse(delimitedString);
            this.x = parse.x;
            this.y = parse.y;
            this.z = parse.z;
        }

        public static Vector3f Parse(string delimitedString)
        {
            if (string.IsNullOrEmpty(delimitedString)) throw new ArgumentNullException();

            char[] delimiterChars = keymath.ParseHelper.English.XMLAttributeDelimiterChars;
            string[] values = delimitedString.Split(delimiterChars);

            if (values == null || values.Length != 3) throw new ArgumentException();
            Vector3f results;
            results.x = float.Parse(values[0]);
            results.y = float.Parse(values[1]);
            results.z = float.Parse(values[2]);
            return results;
        }

        public static Vector3f Zero()
        {
            Vector3f v;
            v.x = v.y = v.z = 0f;
            return v;
        }
                
        public float Length
        {
            get { return (float) Math.Sqrt(x*x + y*y + z*z); }
        }

        public static Vector3f Normalize(Vector3f vec)
        {
            float t = (float) Math.Sqrt(LengthSquared(vec));
            return new Vector3f(vec.x/t, vec.y/t, vec.z/t);
        }

        public static Vector3f Normalize(Vector3f vec, out float length)
        {
            float t = (float) Math.Sqrt(LengthSquared(vec));
            length = t;
            return new Vector3f(vec.x/t, vec.y/t, vec.z/t);
        }

        public static Vector3f TransformCoord(Vector3f v1, Matrix m)
        {
            Vector3f result;
            result.x = (float) ((v1.x*m.M11) + ((v1.y*m.M21) + ((v1.z*m.M31) + m.M41)));
            result.y = (float) ((v1.x*m.M12) + ((v1.y*m.M22) + ((v1.z*m.M32) + m.M42)));
            result.z = (float) ((v1.x*m.M13) + ((v1.y*m.M23) + ((v1.z*m.M33) + m.M43)));
            return result;
        }

        public static float GetDistance3d(Vector3f v1, Vector3f v2)
        {
            //float dx = (v1.x - v2.x);
            //float dy = (v1.y - v2.y);
            //float dz = (v1.z - v2.z);
            //return Math.Sqrt((dx*dx) + (dy*dy) + (dz*dz));
            return (float) Math.Sqrt(LengthSquared(v1 - v2));
        }

        public static float GetDistance3dSquared(Vector3f v1, Vector3f v2)
        {
            return LengthSquared(v1 - v2);
        }

        public static Vector3f Inverse(Vector3f v)
        {
            return new Vector3f(-v.x, -v.y, -v.z);
        }

        // dot productive is commutative (i.e.  v1 dot v2 == v2 dot v1)
        public static float DotProduct(Vector3f v1, Vector3f v2)
        {
            return (v1.x*v2.x + v1.y*v2.y + v1.z*v2.z);
        }

        // cross product is NOT commutative (ie.. v1 cross v2 != v2 cross v1)
        public static Vector3f CrossProduct(Vector3f v1, Vector3f v2)
        {
            //Vector3f vResult;
            //vResult.x = v1.y * v2.z- v1.z * v2.y;
            //vResult.y = v1.z * v2.x - v1.x * v2.z;
            //vResult.z = v1.x * v2.y - v1.y * v2.x;
            //return vResult;
            return new Vector3f(v1.y*v2.z - v1.z*v2.y, v1.z*v2.x - v1.x*v2.z, v1.x*v2.y - v1.y*v2.x);
        }

        public static Vector3f Subtract(Vector3f v1, Vector3f v2)
        {
            return new Vector3f(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
        }

        public static Vector3f Add(Vector3f v1, Vector3f v2)
        {
            return new Vector3f(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
        }

        public static Vector3f Scale(Vector3f v1, float scale)
        {
            return new Vector3f(v1.x*scale, v1.y*scale, v1.z*scale);
        }

        public static float LengthSquared(Vector3f v1)
        {
            return (v1.x*v1.x + v1.y*v1.y + v1.z*v1.z);
        }

        // clamp the vector's magnitude or length to the limit length
        public static Vector3f Limit(Vector3f vec, float limit)
        {
            if (vec.Length > limit)
                return Normalize(vec)*limit;

            return vec;
        }


        public static Vector3f Lerp(Vector3f start, Vector3f end, float weight)
        {
            return (start*(1 - weight)) + (end*weight);
        }

        public static bool operator ==(Vector3f v1, Vector3f v2)
        {
            return (v1.x == v2.x && v1.y == v2.y && v1.z == v2.z);
        }

        public static bool operator !=(Vector3f v1, Vector3f v2)
        {
            return !(v1 == v2);
        }

        public static Vector3f operator -(Vector3f v1, Vector3f v2)
        {
            return Subtract(v1, v2);
        }

        public static Vector3f operator +(Vector3f v1, Vector3f v2)
        {
            return Add(v1, v2);
        }

        public static Vector3f operator *(Vector3f v1, float value)
        {
            return Scale(v1, value);
        }

        public static Vector3f operator /(Vector3f v1, float value)
        {
            return Scale(v1, 1/value);
        }

        public static Vector3f FromTV3DVector(Vector3f v)
        {
            return new Vector3f(v.x, v.y, v.z);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Vector3f))
                return false;
            else
                return (this == (Vector3f) obj);
        }

        //public override int GetHashCode()
        //{
        //    return base.GetHashCode();
        //}
        public override string ToString()
        {
            return string.Format("{0},{1},{2}", x, y, z);
        }
    }
}