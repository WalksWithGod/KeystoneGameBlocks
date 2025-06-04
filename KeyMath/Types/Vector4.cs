using System;


namespace Keystone.Types
{
    public struct Vector4
    {
        public double x;
        public double y;
        public double z;
        public double w;

        public static Vector4 Parse(string delimitedString)
        {
            if (string.IsNullOrEmpty(delimitedString)) throw new ArgumentNullException();

            char[] delimiterChars = keymath.ParseHelper.English.XMLAttributeDelimiterChars;
            string[] values = delimitedString.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
           
            if (values == null || values.Length != 4) throw new ArgumentException();
            Vector4 results;
            results.x = double.Parse(values[0]);
            results.y = double.Parse(values[1]);
            results.z = double.Parse(values[2]);
            results.w = double.Parse(values[3]);
            return results;
        }

        public Vector4(double Vx, double Vy, double Vz, double Vw)
        {
            x = Vx;
            y = Vy;
            z = Vz;
            w = Vw;
        }

        public Vector4(Vector3d v, double vW)
        {
            x = v.x;
            y = v.y;
            z = v.z;
            w = vW;
        }

        public static Vector4 Zero()
        {
            Vector4 v;
            v.x = v.y = v.z = v.w = 0;
            return v;
        }

        public static Vector4 Transform(Vector4 vector, Matrix matrix)
        { 
            Vector4 result = 
                new Vector4((vector.x * matrix.M11) + (vector.y * matrix.M21) + (vector.z * matrix.M31) + (vector.w * matrix.M41),
                    (vector.x * matrix.M12) + (vector.y * matrix.M22) + (vector.z * matrix.M32) + (vector.w * matrix.M42),
                    (vector.x * matrix.M13) + (vector.y * matrix.M23) + (vector.z * matrix.M33) + (vector.w * matrix.M43), 
                    (vector.x * matrix.M14) + (vector.y * matrix.M24) + (vector.z * matrix.M34) + (vector.w * matrix.M44)); 
        
            return result ;
        }

        public override string ToString()
        {
            string delimiter = keymath.ParseHelper.English.XMLAttributeDelimiter;
            return string.Format("{0}{1}{2}{3}{4}{5}{6}", x, delimiter,
                                                       y, delimiter,
                                                       z, delimiter,
                                                       w);
        }
    }

}
