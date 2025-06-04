using System;
using Core.Types;
using MTV3D65;

namespace Core.Math2
{
    public class Math2
    {
        public const float Epsilon = 1.0e-5f;
                           // Epsilon: A constant representing the lower threshold of single-precision accuracy.

        public const double DEGREES_TO_RADIANS = (double) (Math.PI/180d);
        public const double RADIANS_TO_DEGREES = (double) (180d/Math.PI);
       
        private const double PI_OVER_2 = (double) (System.Math.PI/2f);

        public static double PiOver2
        {
            get { return PI_OVER_2; }
        }

        public static void BarycentricCoordinate(Triangle tri, Vector3d point, out double u, out double v, out double w)
        {
            BarycentricCoordinate(tri.Points[0], tri.Points[1], tri.Points[2], point, out u, out v, out w);
        }

        public static void BarycentricCoordinate(Vector3d a, Vector3d b, Vector3d c, Vector3d point, out double u, out double v, out double w)
        {
            Vector3d v0 = b - a;
            Vector3d v1 = c - a;
            Vector3d v2 = point - a;

            double d00 = Vector3d.DotProduct(v0, v0);
            double d01 = Vector3d.DotProduct(v0, v1);
            double d11 = Vector3d.DotProduct(v1, v1);
            double d20 = Vector3d.DotProduct(v2, v0);
            double d21 = Vector3d.DotProduct(v2, v1);
            double denom = d00 * d11 - d01 * d01;

            v = (d11 * d20 - d01 * d21) / denom; // todo: is the proper order for these u, v, w instead of v, w, u?
            w = (d00 * d21 - d01 * d20) / denom;
            u = 1.0f - v - w;
        }

        public static Vector3d MoveAroundPoint (Vector3d origin, double radius, double horzAngleDegrees, double vertAngleDegrees)
        {

            double horzAngle = DEGREES_TO_RADIANS*horzAngleDegrees;
            double vertAngle = DEGREES_TO_RADIANS*vertAngleDegrees;

            return new Vector3d(origin.x + (radius*Math.Sin(horzAngle)*Math.Cos(vertAngle)),
                origin.y - (radius * Math.Sin(vertAngle)), 
                origin.z + (radius*Math.Cos(horzAngle)*Math.Cos(vertAngle)));

        }

        public static Vector3d RandomRotation (Random random)
        {
            return new Vector3d(RandomNumber(random,0d, 360d)*DEGREES_TO_RADIANS, RandomNumber(random,0d, 360d)*DEGREES_TO_RADIANS,
                                RandomNumber(random, 0d, 360d)*DEGREES_TO_RADIANS);
        }
        public static Vector3d RandomVector (Random random, Vector3d min, Vector3d max)
        {
            Vector3d v;
            v.x = RandomNumber(random,min.x, max.x);
            v.y = RandomNumber(random,min.y, max.y);
            v.z = RandomNumber(random,min.z, max.z); 
            return v;   
        }

        public static double Clamp (double value, double min, double max)
        {
            if (value < min ) return min;
            return value > max ? max : value;
        }
        
        public static double RandomNumber(Random random, double min, double max)
        {
            return (max - min) * random.NextDouble() + min;
        }


    //    ''' <summary>
    //''' Simple quad mesh useful for FIXED 2d billboards (i.e. billboards that dont rotate to face the camera)
    //''' </summary>
    //''' <param name="height"></param>
    //''' <param name="width"></param>
    //''' <returns></returns>
    //''' <remarks></remarks>
    //Public Shared Function CreateQuadMesh(ByVal height As Single, ByVal width As Single) As MTV3D65.TVMesh
    //    Dim quadmesh As MTV3D65.TVMesh
    //    quadmesh = _Scene.CreateMeshBuilder()

    //    ' 6 vertices for our quad
    //    quadmesh.AddVertex(-width, 0, 0, 0, 1, 0, 0, 1)
    //    quadmesh.AddVertex(width, height, 0, 0, 1, 0, 1, 0)
    //    quadmesh.AddVertex(-width, height, 0, 0, 1, 0, 0, 0)

    //    quadmesh.AddVertex(-width, 0, 0, 0, 1, 0, 0, 1)
    //    quadmesh.AddVertex(width, 0, 0, 0, 1, 0, 1, 1)
    //    quadmesh.AddVertex(width, height, 0, 0, 1, 0, 1, 0)

    //    'Dim height, width As Single
    //    'height = 10 : width = 5

    //    'width = width * 0.5!
    //    'Dim quadmesh As MTV3D65.TVMesh = CreateQuadMesh(height, width)

    //    '' to give our grass depth, we will cross two more grass planes to make a single 18 vertex grass clump
    //    'Dim depth As Single = width
    //    '' this grass plane crosses perpendicular to the previous
    //    'quadmesh.AddVertex(0, 0, depth, 0, 1, 0, 0, 1)
    //    'quadmesh.AddVertex(0, height, -depth, 0, 1, 0, 1, 0)
    //    'quadmesh.AddVertex(0, height, depth, 0, 1, 0, 0, 0)
    //    'quadmesh.AddVertex(0, 0, depth, 0, 1, 0, 0, 1)
    //    'quadmesh.AddVertex(0, 0, -depth, 0, 1, 0, 1, 1)
    //    'quadmesh.AddVertex(0, height, -depth, 0, 1, 0, 1, 0)

    //    '' this one crosses at 45 degree angle to both the others
    //    'quadmesh.AddVertex(-width * 0.5!, 0, depth * 0.5!, 0, 1, 0, 0, 1)
    //    'quadmesh.AddVertex(width * 0.5!, height, -depth * 0.5!, 0, 1, 0, 1, 0)
    //    'quadmesh.AddVertex(-width * 0.5!, height, depth * 0.5!, 0, 1, 0, 0, 0)
    //    'quadmesh.AddVertex(-width * 0.5!, 0, depth * 0.5!, 0, 1, 0, 0, 1)
    //    'quadmesh.AddVertex(width * 0.5!, 0, -depth * 0.5!, 0, 1, 0, 1, 1)
    //    'quadmesh.AddVertex(width * 0.5!, height, -depth * 0.5!, 0, 1, 0, 1, 0)


    //    'quadmesh.WeldVertices()
    //    'm.CreateFromMesh(quadmesh)
    //    'quadmesh.Destroy()
    //    Return quadmesh
    //End Function

        public static float Lerp(float start, float end, float weight)
        {
            return start * (1.0f - weight) + weight * end;
        }

        public static double Lerp(double start, double end, double weight)
        {
            return start * (1.0d - weight) + weight * end;
        }

        //http://www.truevision3d.com/forums/showcase/procedural_terrain_heightmap_generation_with_erosion-t18085.0.html
        //You can mess around with the Pi and the 0.5 number for different results too
        // todo: MPJ -but why is he using Acos here and not Cos?
        public double Cosine_Interpolate(float a, float b, float x)
        {
            double ft = x * 3.1415927;
            double f = (1.0d - Math.Acos(ft)) * 0.5d;

            return a * (1.0d - f) + b * f;
        }

//http://www.truevision3d.com/forums/tv3d_sdk_65/rotation_and_normals-t18209.0.html
//        public TV_3DMATRIX BindMatrixToLand(TV_3DMATRIX InMatrix, TV_3DVECTOR Scale, TV_3DVECTOR Position)
//                {
//                    {
//                        TV_3DVECTOR TopVector = World.Landscape.GetNormal(InMatrix.m41, InMatrix.m43);
//                        TV_3DVECTOR TentativeForwardVector = new TV_3DVECTOR(InMatrix.m31, InMatrix.m32, InMatrix.m33);
//                        TV_3DVECTOR RightVector = new TV_3DVECTOR();
//                        TV_3DVECTOR ForwardVector = new TV_3DVECTOR();
//                        TVMathLibrary MathLibrary = new TVMathLibrary();
//                        MathLibrary.TVVec3Cross(ref RightVector, TopVector, TentativeForwardVector);
//                        MathLibrary.TVVec3Cross(ref ForwardVector, RightVector, TopVector);
//                        MathLibrary.TVVec3Normalize(ref RightVector, RightVector);
//                        MathLibrary.TVVec3Normalize(ref ForwardVector, ForwardVector);
//                        InMatrix.m11 = RightVector.x * Scale.x;
//                        InMatrix.m12 = RightVector.y * Scale.x;
//                        InMatrix.m13 = RightVector.z * Scale.x;
//                        InMatrix.m21 = TopVector.x * Scale.y;
//                        InMatrix.m22 = TopVector.y * Scale.y;
//                        InMatrix.m23 = TopVector.z * Scale.y;
//                        InMatrix.m31 = ForwardVector.x * Scale.z;
//                        InMatrix.m32 = ForwardVector.y * Scale.z;
//                        InMatrix.m33 = ForwardVector.z * Scale.z;
//                        InMatrix.m41 = Position.x;
//                        InMatrix.m42 = Position.y;
//                        InMatrix.m43 = Position.z;

//                    }

//I store the matrix information returned this routine and I apply teh matrix to the mesh with setmatrix array: _MiniMesh.SetMatrixArray(_DisplayAmount, FoilageMatrix);

        public static double DegreesToRadians(double degrees)
        {
            return degrees*DEGREES_TO_RADIANS;
        }

        public static double RadiansToDegrees(double radians)
        {
            return radians*RADIANS_TO_DEGREES;
        }

        public static double Heading2DRadians(Vector3d vec)
        {
            // .NET already handles the various cases so we dont need to compensate using -vec.x or anything
            //double angle = (double)System.Math.Atan2(-vec.x, vec.z);
            //return -1 * angle;
            return (double) Math.Atan2(vec.x, vec.z);
        }

        public static double Heading2DDegrees(Vector3d vec)
        {
            return RadiansToDegrees(Heading2DRadians(vec));
        }
    }
}