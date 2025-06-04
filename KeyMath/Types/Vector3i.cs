using System;

namespace Keystone.Types
{
	public struct Vector3i
    {
        public int X;
        public int Y;
        public int Z;

        public Vector3i(int Vx, int Vy, int Vz)
        {
            X = Vx;
            Y = Vy;
            Z = Vz;
        }

        public static Vector3i Parse(string delimitedString)
        {
            if (string.IsNullOrEmpty(delimitedString)) throw new ArgumentNullException();

            char[] delimiterChars = keymath.ParseHelper.English.XMLAttributeDelimiterChars;
            string[] values = delimitedString.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);

            if (values == null || values.Length != 3) throw new ArgumentException();
            Vector3i results;
            results.X = int.Parse(values[0]);
            results.Y = int.Parse(values[1]);
            results.Z = int.Parse(values[2]);
            return results;
        }

        public override string ToString()
        {
            string delimiter = keymath.ParseHelper.English.XMLAttributeDelimiter;
            return string.Format("{0}{1}{2}{3}{4}", X, delimiter,
                                                       Y, delimiter,
                                                       Z);
        }

        public static Vector3i Zero()
        {
            Vector3i v;
            v.X = v.Y = v.Z = 0;
            return v;
        }
        
        /// <summary>
        /// Flips a vector.  Note: To avoid confusion, I've deleted the Vector3d.Inverse() function altogether
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector3i Negate(Vector3i v)
        {
            Vector3i result;
            result.X = -v.X;
            result.Y = -v.Y;
            result.Z = -v.Z;
            return result;
        }
        
        public static Vector3i Subtract(Vector3i v1, Vector3i v2)
        {
            //return new Vector3i(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
            Vector3i result;
            result.X = v1.X - v2.X;
            result.Y = v1.Y - v2.Y;
            result.Z = v1.Z - v2.Z;
            return result;
        }

        public static Vector3i Add(Vector3i v1, Vector3i v2)
        {
            //   return new Vector3i(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
            Vector3i result;
            result.X = v1.X + v2.X;
            result.Y = v1.Y + v2.Y;
            result.Z = v1.Z + v2.Z;
            return result;
        }

        public static Vector3i Scale(Vector3i v1, int scale)
        {
            Vector3i result;
            result.X = v1.X * scale;
            result.Y = v1.Y * scale;
            result.Z = v1.Z * scale;
            return result;
            //return new Vector3i(v1.x*scale, v1.y*scale, v1.z*scale);
        }

        public static Vector3i Scale(int scale)
        {
            Vector3i result;
            result.X = scale;
            result.Y = scale;
            result.Z = scale;
            return result;
        }
        
        public static Vector3i operator -(Vector3i v1)
        {
            return Negate(v1);
        }

        public static Vector3i operator -(Vector3i v1, Vector3i v2)
        {
            return Subtract(v1, v2);
        }

        public static Vector3i operator +(Vector3i v1, Vector3i v2)
        {
            return Add(v1, v2);
        }


        public static Vector3i operator *(Vector3i v1, Vector3i v2)
        {
            Vector3i result;
            result.X = v1.X * v2.X;
            result.Y = v1.Y * v2.Y;
            result.Z = v1.Z * v2.Z;

            return result;
        }

        public static Vector3i operator *(Vector3i v1, int value)
        {
            return Scale(v1, value);
        }
        public static Vector3i operator *(int value, Vector3i v1)
        {
            return Scale(v1, value);
        }
        public static Vector3i operator /(Vector3i v1, int value)
        {
        	if (value == 0) return Vector3i.Zero();
        
			Vector3i result;        	
        	result.X = v1.X / value;
        	result.Y = v1.Y / value;
        	result.Z = v1.Z / value;
        	
        	return result;
        }

        public static Vector3i operator /(int value, Vector3i v1)
        {
        	Vector3i result;
        	result.X = value / v1.X;
        	result.Y = value / v1.Y;
        	result.Z = value / v1.Z;
        	
            // July.10.2012 to avoid divide by zero use ternary ?: to assign 0 or 1 / v 
        	result.X = (v1.X == 0) ? 0 : value / v1.X;
        	result.Y = (v1.Y == 0) ? 0 : value / v1.Y;
        	result.Z = (v1.Z == 0) ? 0 : value / v1.Z;
        	
        	return result;
        	
            //return Scale(v1, 1.0d / value);
        }

        public static bool operator ==(Vector3i v1, Vector3i v2)
        {
            return (v1.X == v2.X && 
        	        v1.Y == v2.Y && 
        	        v1.Z == v2.Z);
        }

        public static bool operator !=(Vector3i v1, Vector3i v2)
        {
            return !(v1 == v2);
        }
        
        public override bool Equals(object obj)
        {
        	if (obj == null || !(obj is Vector3i))
                return false;
            
            return (this == (Vector3i)obj);
        }

        public bool Equals(Vector3i v)
        {
           return this.X == v.X && 
           		  this.Y == v.Y && 
           		  this.Z == v.Z;
        }
        
    }
}
