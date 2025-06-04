using System;
using Keystone.Types;


namespace Keystone.Primitives
{
    public class Ellipse
    {
        // http://forums.create.msdn.com/forums/t/7414.aspx
        //http://www.xnawiki.com/index.php/Shape_Generation
        /// <summary>
        /// 
        /// </summary>
        /// <param name="semimajor_axis"></param>
        /// <param name="eccentricity">perfect circular orbit == 0</param>
        /// <param name="numSegments"></param>
        /// <returns></returns>
        public static Vector3d[] CreateEllipse(float semimajor_axis, float eccentricity, int numSegments)
        {
            //float semiminor_axis = (float)Math.Sqrt(semimajor_axis * semimajor_axis * (1d - eccentricity * eccentricity));

            float semiminor_axis = (float)(semimajor_axis * Math.Sqrt(1d - eccentricity * eccentricity));

            float foci = (float)Math.Sqrt(semimajor_axis * semimajor_axis - semiminor_axis * semiminor_axis);

            Vector3d[] points = new Vector3d[numSegments];

            float max = 2.0f * (float)Math.PI;
            float step = max / ((float)numSegments - 1);
            float h = -foci;
            float k = 0.0f;

            int i = 0;
            double x;
            double z;
            for (float t = 0.0f; t < max; t += step)
            {
                // center point: (h,k); add as argument if you want (to circumvent modifying this.Position)        
                // x = h + a*cos(t)  -- a is semimajor axis, b is semiminor axis        
                // y = k + b*sin(t)   
                x = h + semimajor_axis * Math.Cos(t);
                z = k + semiminor_axis * Math.Sin(t);
                points[i] = new Vector3d(z, 0, x);
                i++;
            }

            //then add the first vector again so it's a complete loop
            x = h + semimajor_axis * Math.Cos(step);
            z = k + semiminor_axis * Math.Sin(step);
            points[numSegments - 1] = new Vector3d(z, 0, x);

            /// now rotate it as necessary    
            //Matrix m = Matrix.CreateRotationZ(angle_offset);
            //for (int i = 0; i < vectors.Count; i++)
            // {
            //     points[i] = Vector2.Transform((Vector2)vectors[i], m);
            //}

            return points;
        }


        public static Vector3d[] CreateCirclePoints(int numSegments, float radius)
        {
            if (radius <= 0) throw new ArgumentOutOfRangeException();

            Vector3d[] points = new Vector3d[numSegments + 1];
            double radiansPerSeg = Math.PI * 2 / numSegments;

            for (int i = 0; i < numSegments; i++)
            {
                double angle = radiansPerSeg * i;
                points[i].x = Math.Sin(angle);
                points[i].y = 0;
                points[i].z = Math.Cos(angle);
            }

            // add a final point to bring the circle back to the start
            points[numSegments].x = Math.Sin(0);
            points[numSegments].y = 0;
            points[numSegments].z = Math.Cos(0);

            //            // create a translation matrix using the center
            //            Matrix translation = Matrix.Translation(center + offset);
            //            Matrix rotation = Matrix.Rotation(axis, angleRadians);
            //            Matrix scaling = Matrix.Scaling(new Vector3d(radius, radius, radius));

            //            // transform all the points
            //            Matrix transform = scaling * rotation * translation;

            //            points = Vector3d.TransformCoordArray(points, transform);
            //            DrawHull(points, color);

            return points;
        }
    }
}
