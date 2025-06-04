using System;
using System.Collections.Generic;
using Keystone.Types;


namespace Keystone.Primitives
{
    /// <summary>
    /// 2D Version of a Bounding Box
    /// </summary>
    public struct BoundingRect
    {

        public Vector2f Min;
        public Vector2f Max;


        public static BoundingRect Parse(string s)
        {
            char[] delimiterChars = { ' ', ',' };
            string[] sToks = s.Split(delimiterChars);

            Vector2f min, max;
            min.x = float.Parse(sToks[0]);
            min.y = float.Parse(sToks[1]);

            max.x = float.Parse(sToks[2]);
            max.y = float.Parse(sToks[3]);

            BoundingRect result;
            result.Min = min;
            result.Max = max;
            return result;
        }

        public static BoundingRect FromBoundingBox(BoundingBox box)
        {

            BoundingRect result;
            Vector2f min, max;
            min.x = (float)box.Min.x;
            min.y = (float)box.Min.z;
            max.x = (float)box.Max.x;
            max.y = (float)box.Max.z;
            result.Min = min;
            result.Max = max;
            return result;
        }

        /// <summary>
        /// Takes a rectangular bounding rect and returns a perfect square that fully
        /// contains the rectangular bounding rect.
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static BoundingRect Square(BoundingRect rect)
        {
            
            Vector2f min, max;

            if (rect.Min.x < rect.Min.y)
                min.x = rect.Min.x;
            else
                min.x = rect.Min.y;

            min.y = min.x;

            if (rect.Max.x > rect.Max.y)
                max.x = rect.Max.x;
            else
                max.x = rect.Max.y;

            max.y = max.x;

            BoundingRect result;
            result.Min = min;
            result.Max = max;
            return result;
        }

        public BoundingRect(Vector2f min, Vector2f max)
        {
            // TODO: assert if width/height/depth of this box is > double.MaxValue 
            // TODO: assert if box is inversed such that any component of max is smaller than any component of max
            Min = min;
            Max = max;
        }

                // construct a square bounding box who's ceter is at "position" and who's
        // center points on each face are "radius" distance from the center.
        public BoundingRect(Vector2f position, float radius)
            :
                this(position.x - radius, position.y - radius,
                     position.x + radius, position.y + radius)
        {
        }

        public BoundingRect(float minX, float minY, float maxX, float maxY) 
        {
            Vector2f min, max;
            min.x = minX;
            min.y = minY;
            max.x = maxX;
            max.y = maxY;

            Min = min;
            Max = max;
        }

        public override string ToString()
        {
            string s = string.Format("{0},{1},{2},{3},{4},{5}", Min.x, Min.y, Max.x, Max.y);
            return s;
        }

        public Vector2f Center
        {
            get
            {
                Vector2f result;
                result.x = Min.x + (Width * .5f);
                result.y = Min.y + (Height * .5f);

                return result;
            }
        }

        public float Height
        {
            get { return Max.y - Min.y; }
        }

        public float Width
        {
            get { return Max.x - Min.x; }
        }

        public Vector2f[] Vertices { get { return GetVertices(this); } }

        public bool Contains(BoundingRect rect)
        {
            return Contains(rect.Vertices);
        }

        public bool Contains(int x, int y)
        {
            return (x >= Min.x && x <= Max.x &&
                    y >= Min.y && y <= Max.y);
        }

        public bool Contains(Vector2f point)
        {
            return (point.x >= Min.x && point.x <= Max.x &&
                    point.y >= Min.y && point.y <= Max.y);
        }
        
        // returns true if _all_ points are contained
        public bool Contains(Vector2f[] points)
        {
            for (int i = 0; i < points.Length; i++)
            {
                if (!Contains(points[i])) return false;
            }
            return true;
        }


        private static Vector2f[] GetVertices(BoundingRect rect)
        {
            Vector2f[] vertices = new Vector2f[4];

            vertices[0].x = rect.Min.x;
            vertices[0].y = rect.Min.y;

            vertices[1].x = rect.Max.x;
            vertices[1].y = rect.Min.y;

            vertices[2].x = rect.Max.x;
            vertices[2].y = rect.Max.y;

            vertices[3].x = rect.Min.x;
            vertices[3].y = rect.Max.y;
            return vertices;
        }
    }
}
