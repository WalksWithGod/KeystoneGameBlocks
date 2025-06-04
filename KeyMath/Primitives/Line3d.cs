using System;
namespace Keystone.Types
{
    public struct Line3d
    {
        private Vector3d[] _p;


        public Line3d(Vector3d v1, Vector3d v2)  :
            this(v1.x, v1.y, v1.z, v2.x, v2.y, v2.z)
        {

        }

        public Line3d(double x1, double y1, double z1, double x2, double y2, double z2) 
        {
            _p = new Vector3d[2];
            _p[0].x = x1;
            _p[0].y = y1;
            _p[0].z = z1;

            _p[1].x = x2;
            _p[1].y = y2;
            _p[1].z = z2;       
        }
        public Vector3d[] Point
        {
            get { return _p; }
        }

        public void SetEndPoints(Vector3d start, Vector3d end)
        {
            if (_p == null) throw new Exception("Line3D.SetEndPoints() - Line3d not initialized.");

            _p[0] = start;
            _p[1] = end;
        }

        // Overloaded the == operator in EDGE to return as true any two edges that have same endpoints regardless of order.
        // i.e.  AB=AB && AB = BA
        public static bool operator ==(Line3d e1, Line3d e2)
        {
            return (e1.Point[0].x == e2.Point[0].x
                    && e1.Point[0].y == e2.Point[0].y
                    && e1.Point[0].z == e2.Point[0].z
                    && e1.Point[1].x == e2.Point[1].x
                    && e1.Point[1].y == e2.Point[1].y
                    && e1.Point[1].z == e2.Point[1].z)
                   ||
                   (e1.Point[0].x == e2.Point[1].x
                    && e1.Point[0].y == e2.Point[1].y
                    && e1.Point[0].z == e2.Point[1].z
                    && e1.Point[1].x == e2.Point[0].x
                    && e1.Point[1].y == e2.Point[0].y
                    && e1.Point[1].z == e2.Point[0].z);
        }

        public static bool operator !=(Line3d e1, Line3d e2)
        {
            return !(e1 == e2);
        }

        public override bool Equals(object obj)
        {
            if (obj is Line3d)
                return this == (Line3d) obj;
            else
                return base.Equals(obj);
        }

        public Vector3d ClosestPoint(Vector3d p)
        {
            return ClosestPointOnLine(this._p[0], this._p[1], p);
        }

        public double DistanceSquared (Vector3d p, out Vector3d closestPoint)
        {
            return DistanceSquared(this._p[0], this._p[1], p, out closestPoint);
        }

        public double Distance (Vector3d p, out Vector3d closestPoint)
        {
            return System.Math.Sqrt( DistanceSquared(p, out closestPoint));
        }

        public static Vector3d Center (Vector3d start, Vector3d end)
        {
            return end + ((start - end) / 2);
        }
        // ----------------------------------------------------------------------
        // Name  : closestPointOnLine()
        // Input : a - first end of line segment
        //         b - second end of line segment
        //         p - point we wish to find closest point on line from 
        // Notes : Helper function for closestPointOnTriangle()
        // Return: closest point on line segment
        // -----------------------------------------------------------------------  
        /// <summary>
        /// Returns a point on the line that is closest to point P. Thus this does not just return a or b, but any point in between that 
        /// will result in a perpendicular line from p to the line segment
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static Vector3d ClosestPointOnLine(Vector3d a, Vector3d b, Vector3d p)
        {
            if (a == b) return a; // zero length line segment

            // Determine t (the length of the vector from ‘a’ to ‘p’)
            Vector3d c = p - a;
            Vector3d V = b - a;

            double d = V.Length;

            V = Vector3d.Normalize(V);
            double t = Vector3d.DotProduct(V, c);
            
            // Check to see if ‘t’ is beyond the extents of the line segment
            if (t < 0.0f) return (a);
            if (t > d) return (b);
            
            // Return the point between ‘a’ and ‘b’
            //set length of V to t. V is normalized so this is easy
            V.x = V.x * t;
            V.y = V.y * t;
            V.z = V.z * t;

            return (a + V);
        }

        public static Vector3d ClosestPointOnAxis (Vector3d a, Vector3d b, Vector3d p)
        {
            if (a == b) return a; // zero length line segment

            // Determine t (the length of the vector from ‘a’ to ‘p’)
            Vector3d c = p - a;
            Vector3d axis = b - a;

            axis = Vector3d.Normalize(axis);
            double t = Vector3d.DotProduct(axis, c);

            // scale axis 
            axis *= t; 

            return (a + axis);
        }


        public static double Distance(Line3d line, Vector3d p, out Vector3d closestPoint)
        {
            return System.Math.Sqrt(DistanceSquared(line, p, out closestPoint));
        }
        public static double DistanceSquared(Line3d line, Vector3d p, out Vector3d closestPoint)
        {
            return DistanceSquared(line._p[0], line._p[1], p, out closestPoint);
        }

        public static double DistanceSquared (Vector3d a, Vector3d b, Vector3d p, out Vector3d closestPoint)
        {
            closestPoint = ClosestPointOnLine(a, b, p);
            return (p - closestPoint).LengthSquared();
        }

        public static double Distance(Vector3d a, Vector3d b, Vector3d p, out Vector3d closestPoint)
        {
            return System.Math.Sqrt(DistanceSquared(a, b, p, out closestPoint));
        }

      //      Bresenham's Line Algorithm - VB XNA style
        // http://ilovevb.net/Web/blogs/vbxna/archive/2008/04/15/bresenham-s-line-algorithm-vb-xna-style.aspx
     
      // 1:  Imports Microsoft.Xna.Framework
      // 2:  Imports System.Math
      // 3:   
      // 4:  Public Class Utils
      // 5:      ''' <summary>
      // 6:      ''' This function uses Bresenham's Line Algorithm to find the 
      // 7:      ''' most direct path between two points on a 2D grid and stores 
      // 8:      ''' all the points in a list.
      // 9:      ''' </summary>
      //10:      ''' <param name="StartPosition">Starting X,Y coordinates</param>
      //11:      ''' <param name="EndPosition">Starting X,Y coordinates</param>
      //12:      ''' <returns>List(Of Vector2)</returns>
      //13:      ''' <remarks></remarks>
      //14:      Public Function DeterminePath(ByVal StartPosition As Vector2, _
      //15:                     ByVal EndPosition As Vector2) As List(Of Vector2)

      //16:          Dim myPoint As Vector2 = StartPosition ' current point
      //17:          Dim myPath As New List(Of Vector2) ' collection of path points
      //18:   
      //19:          ' Get the difference between 2 points
      //20:          Dim deltaX As Integer = EndPosition.X - StartPosition.X
      //21:          Dim deltaY As Integer = EndPosition.Y - StartPosition.Y
      //22:          Dim leftover As Integer
      //23:   
      //24:          ' Figure out direction based on the +/- value of the deltas
      //25:          Dim dirX As Integer = IIf(deltaX < 0, -1, 1)
      //26:          Dim dirY As Integer = IIf(deltaY < 0, -1, 1)
      //27:   
      //28:          ' Get absolute, we'll decide whether to add/subtract later 
      //29:          deltaX = Abs(deltaX)
      //30:          deltaY = Abs(deltaY)
      //31:   
      //32:          ' Uncomment this to add the first point to the path (list)
      //33:          ' myPath.Add(myPoint)
      //34:   
      //35:          ' iterate through whichever axis is longest
      //36:          If deltaX > deltaY Then
      //37:              leftover = (deltaY * 2) - deltaX

      //38:              While myPoint.X <> EndPosition.X
      //39:                  If leftover >= 0 Then
      //40:                      myPoint.Y = myPoint.Y + dirY
      //41:                      leftover = leftover - deltaX
      //42:                  End If

      //43:                  myPoint.X = myPoint.X + dirX
      //44:                  leftover = leftover + deltaY
      //45:                  myPath.Add(myPoint)
      //46:              End While
      //47:          Else

      //48:              leftover = (deltaX * 2) - deltaY

      //49:              While myPoint.Y <> EndPosition.Y
      //50:                  If leftover >= 0 Then
      //51:                      myPoint.X = myPoint.X + dirX
      //52:                      leftover = leftover - deltaY
      //53:                  End If
      //54:                  myPoint.Y = myPoint.Y + dirY
      //55:                  leftover = leftover + deltaX
      //56:                  myPath.Add(myPoint)
      //57:              End While
      //58:          End If

      //59:   
      //60:          Return myPath
      //61:      End Function
      //62:  End Class
    }
}