//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Drawing;
//using Keystone.Cameras;
//using Keystone.Types;
//using MTV3D65;
//using Color = Keystone.Types.Color;

//namespace Keystone
//{
//    internal interface IDisplayList2D
//    {
//      int Color {get; set;}
//      void Draw();
//    }

//    //public struct VertexList
//    //{
//    //    private Vector3d[] verts;
//    //    private CONST_TV_COLORKEY color;
//    //}

//    public struct StringList : IDisplayList2D
//    {
//        public int Color;
//        public string Text;
//        public int Top;
//        public int Left;

//        public StringList(string text, int left, int top, int color)
//        {
//            Text = text;
//            Color = color;
//            Top = top;
//            Left = left;
//        }
//    }

//    public struct ArrowHead
//    {
//        public Triangle[] Triangles;

//        public ArrowHead(Vector3d p1, Vector3d p2, float arrowSize)
//        {
//            const float MIN_SIZE = 0.000001f;

//            Vector3d dir = p2 - p1;

//            double mag;
//            Vector3d.Normalize(dir, out mag);

//            if (mag < MIN_SIZE)
//            {
//                mag = MIN_SIZE;
//                dir.x = 0;
//                dir.y = 1;
//                dir.z = 0;
//            }

//            //if ( arrowSize > (mag * 0.3f) )
//            //{
//            //    arrowSize = (float)mag * 0.3f;
//            //}

//            Vector3d tmp = new Vector3d(0, 1, 0);
//            TV_3DQUATERNION quat = Helpers.TVTypeConverter.ToTVQuaternion(Matrix.RotationArc(tmp, dir));

//            TV_3DMATRIX tvmat = new TV_3DMATRIX();
//            CoreClient._CoreClient.Maths.TVMatrixRotationQuaternion(ref tvmat, quat);
//            Matrix matrix = Helpers.TVTypeConverter.FromTVMatrix(tvmat);
//            matrix.SetTranslation(p2);

//            float lx = (float) Math.Cos(0);
//            float ly = (float) Math.Sin(0);

//            uint pcount = 0;
//            Vector3d[] points = new Vector3d[24];

//            for (float a = 30; a <= 360; a += 30)
//            {
//                float rotationRadians = a*(float) Utilities.MathHelper.DEGREES_TO_RADIANS;
//                float x = (float) Math.Cos(rotationRadians)*arrowSize;
//                float z = (float) Math.Sin(rotationRadians)*arrowSize;

//                points[pcount] = new Vector3d(x, -3*arrowSize, z);
//                pcount++;
//                Debug.Assert(pcount < 24);
//                    // no increment smaller than 15 degrees, because that is all the room we reserved for...
//            }

//            Vector3d prev = points[(pcount - 1)];
//            Vector3d center = new Vector3d(0, -2.5f*arrowSize, 0);
//            Vector3d top = new Vector3d(0, 0, 0);

//            Vector3d _center = Vector3d.TransformCoord(center, matrix);
//            Vector3d _top = Vector3d.TransformCoord(top, matrix);


//            uint tcount = 0;
//            Triangles = new Triangle[2*pcount]; // reserve room for the maximum number of triangles
//            for (uint i = 0; i < pcount; i++)
//            {
//                Vector3d _p;
//                Vector3d _prev;
//                _p = Vector3d.TransformCoord(points[i], matrix);
//                _prev = Vector3d.TransformCoord(prev, matrix);

//                Triangles[tcount] = new Triangle(_p, _center, _prev);
//                tcount++;
//                Triangles[tcount] = new Triangle(_prev, _top, _p);
//                tcount++;

//                prev = points[i];
//            }
//        }
//    }

//    public struct PolygonList : IDisplayList2D
//    {
//        public Vector3d[] Points;
//        public int Color;

//        public PolygonList(Polygon t, int color)
//        {
//            Color = color;
//            Points = t.Points;
//        }
//    }

//    public struct SphereList : IDisplayList2D
//    {
//        public Vector3d Center;
//        public double Radius;
//        public int Color;

//        public SphereList(Vector3d center, double radius, int color)
//        {
//            Center = center;
//            Radius = radius;
//            Color = color;
//        }
//    }

//    public struct BoxList : IDisplayList2D
//    {
//        public BoundingBox Box;
//        public int Color;

//        public BoxList(BoundingBox box, int color)
//        {
//            Box = box;
//            Color = color;
//        }
//    }

//    public struct LineList : IDisplayList2D
//    {
//        public Line3d[] Lines;
//        public int Color;

//        public LineList(Line3d[] lines, int color)
//        {
//            Lines = lines;
//            Color = color;
//        }
//    }

//    // 1) 2d elements need to be drawn last or they will get overwritten by the rest of the scene.  
//    //    this class provides caching of 2d display commands which can then be executed at the end of the frame
//    //    This way you can call the draw command at anytime during scene traversal and not worry about handling
//    //    the order of these draw calls at the same time.  

//    // 2) Often times you just want to draw a wireframe primitives for visual debugging without having to create a TVMesh.
//    // also, tv3d can only draw everything in wireframe or everything solid.  You cant just draw a wireframe sphere 
//    // without turning wireframing on for all the regular meshes in the entire scene.  
//    // e.g. Core.Scene.SetRenderMode( MTV3D65.CONST_TV_RENDERMODE.TV_LINE )

//    // 3) Since all of these commands are batched at once, we get optimal efficiency by using a single Action_Begin / Action_End block.
//    public class DebugDraw
//    {
//        private static List<IDisplayList2D> _displayList = new List<IDisplayList2D>();
//        private static List<IDisplayList2D> _textList = new List<IDisplayList2D>();
//        private static int textureFontId;

//        private static bool initialized;
//        //private static EDGE[] tmp;
//        private static List<Line3d> _edges = new List<Line3d>();
//        private static Vector3d[] _points;
//        private static SlimDX.Direct2D.DeviceContextRenderTarget mSlimDX2DRenderTarget;
//        private static SlimDX.Direct3D9.Device mSlimDXDevice;

//        static DebugDraw()
//        {
//            textureFontId = CoreClient._CoreClient.Text.TextureFont_Create("TextureFont", "Arial Narrow", 11, true, false, false,
//                                                               false);

//            //SlimDX.Direct3D9.Direct3D d3d =
//            //    SlimDX.Direct3D9.Direct3D.FromPointer(CoreClient._CoreClient.Internals.GetDevice3D());
//            mSlimDXDevice = 
//                SlimDX.Direct3D9.Device.FromPointer(CoreClient._CoreClient.Internals.GetDevice3D());

//            // TODO: so that i can specify the matrix to use including viewmatrix, these
//            // three-d lines along with our line4 code
//            // is perfect.


            
//        }

//        private static void SlimTest()
//        {
//            Debug.Assert(mSlimDX2DRenderTarget != null);

//        }

//        private static void SlimDraw()
//        {
//            //rt.Transform = bleh;
//            //rt.FillGeometry();
//            //rt.EndDraw();
//        }

//        #region Actual Rendering Occurs
//        public static void CommitDisplayList(Camera camera)
//        {
//            CoreClient._CoreClient.Engine.SetCamera (camera.TVCamera);

//            // note: i think the 2d stuff in D3D is mostly sugar syntax over D3D transformed lines
//            // but automatically handles the projection of those lines to 2d and with a constant single
//            // pixel width.

//            bool prevEnable = false;
            
//            CONST_TV_BLENDEX prevSrcBlend, prevDestBlend = CONST_TV_BLENDEX.TV_BLENDEX_ONE;
//            prevSrcBlend = prevDestBlend;

//            CoreClient._CoreClient.Screen2D.Settings_GetAlphaBlendingEx(ref prevEnable, ref prevSrcBlend, ref prevDestBlend);
            
//            CoreClient._CoreClient.Screen2D.Settings_SetAlphaBlending(false, CONST_TV_BLENDINGMODE.TV_BLEND_NO);
//            CoreClient._CoreClient.Screen2D.Action_Begin2D();
//            foreach (IDisplayList2D entry in _displayList)
//            {
//                if (entry is SphereList) Draw((SphereList) entry);
//                //if (entry is BoxList) DrawBox(BoundingBox.GetVertices(((BoxList)entry).Box), ((BoxList)entry).Color);
//                if (entry is LineList) DrawLines(((LineList) entry).Lines, ((LineList) entry).Color);
//                if (entry is PolygonList)
//                    DrawTriangle(((PolygonList) entry).Points, ((PolygonList) entry).Color, false);
//            }

//            CoreClient._CoreClient.Screen2D.Action_End2D();
//            CoreClient._CoreClient.Screen2D.Settings_SetAlphaBlendingEx(prevEnable, prevSrcBlend, prevDestBlend);
//        }

//        public static void CommitTextList()
//        {

//            CoreClient._CoreClient.Text.Action_BeginText();
//            foreach (IDisplayList2D entry in _textList)
//            {
//                if (entry is StringList) Draw((StringList) entry);
//            }
//            CoreClient._CoreClient.Text.Action_EndText();
//        }

//        private static void Draw(StringList entry)
//        {
//            CoreClient._CoreClient.Text.TextureFont_DrawText(entry.Text, entry.Left, entry.Top, entry.Color, textureFontId);                     
//        }

//        private static void DrawText(StringList entry)
//        {
 
//        }

//        private static void DrawTriangle(Vector3d[] p, int color, bool filled)
//        {
//            PointF[] points2d = new PointF[p.Length];
//            float x = 0, y = 0;
//            for (int i = 0; i < p.Length; i++)
//            {
//                // because these lines have to be projected to 2d, it depends on viewport and because these draw commands can
//                // occur during things (mostly for debug) like AdvancedCollide, which is not specific to any viewport, then the current
//                // projection used might be off.  Thus we will project now during the CommitDebugDraw where we're guaranteed to
//                // have the proper current viewport set prior to doing the projection call
//                CoreClient._CoreClient.Screen2D.Math_3DPointTo2D(Helpers.TVTypeConverter.ToTVVector(p[i]), ref x, ref y);
//                points2d[i] = new PointF(x, y);
//            }


//            if (points2d.Length == 3)
//            {
//                if (!filled)
//                    CoreClient._CoreClient.Screen2D.Draw_Triangle(points2d[0].X, points2d[0].Y, points2d[1].X, points2d[1].Y, points2d[2].X, points2d[2].Y, color);
//                else
//                    CoreClient._CoreClient.Screen2D.Draw_FilledTriangle(points2d[0].X, points2d[0].Y, points2d[1].X, points2d[1].Y, points2d[2].X, points2d[2].Y, color);
//            }
//            else
//            {
//                TV_CUSTOM2DVERTEX[] verts = new TV_CUSTOM2DVERTEX[p.Length + 1];

//                for (int i = 0; i < p.Length; i++)
//                {
//                    verts[i].color = (uint)color;
//                    verts[i].x = points2d[i].X;
//                    verts[i].y = points2d[i].Y;
//                }
//                verts[p.Length].color = (uint)color;
//                verts[p.Length].x = points2d[0].X;
//                verts[p.Length].y = points2d[0].Y;

//                CoreClient._CoreClient.Screen2D.Draw_Custom(0, CONST_TV_PRIMITIVETYPE.TV_LINESTRIP, verts, verts.Length);
//            }
//        }

//        private static void DrawLines(Line3d[] linelist, int color)
//        {
//            for (int i = 0; i < linelist.Length; i++)
//                CoreClient._CoreClient.Screen2D.Draw_Line3D((float)linelist[i].Point[0].x,
//                                                (float)linelist[i].Point[0].y,
//                                                (float)linelist[i].Point[0].z,
//                                                (float)linelist[i].Point[1].x,
//                                                (float)linelist[i].Point[1].y,
//                                                (float)linelist[i].Point[1].z,
//                                                color);
//        }

//        private static void Draw(SphereList entry)
//        {
//            // NOTE: For some reason trying to get a copy of the edges into a tmp 
//            // is resulting in a shallow copy and modifying tmp ends up modifying edges.
//            // So, ive switched to storing a copy of the points and then itterating in steps of 2
//            // get tmp copy
//            //EDGE[] tmp = new EDGE[_edges.Count];
//            //_edges.CopyTo(tmp, 0);

//            Vector3d[] tmp = new Vector3d[_points.Length];
//            _points.CopyTo(tmp, 0);

//            for (int i = 0; i < tmp.Length; i++)
//            {
//                //// scale by radius
//                //tmp[i].Point[0] *= entry.Radius;
//                //tmp[i].Point[1] *= entry.Radius;

//                //// translate to the _center vector
//                //tmp[i].Point[0] += entry.Center;
//                //tmp[i].Point[1] += entry.Center;

//                // scale by radius
//                tmp[i] *= entry.Radius;
//                // translate to the _center vector
//                tmp[i] += entry.Center;
//            }
//            // draw centered at the _center vector
//            //for (int i = 0; i < tmp.Length; i++)
//            //    Core._CoreClient.Screen2D.Draw_Line3D(tmp[i].Point[0].x,
//            //                                    tmp[i].Point[0].y,
//            //                                    tmp[i].Point[0].z,
//            //                                    tmp[i].Point[1].x,
//            //                                    tmp[i].Point[1].y,
//            //                                    tmp[i].Point[1].z,
//            //                                    entry.Color);

//            for (int i = 0; i < tmp.Length; i += 2)
//                CoreClient._CoreClient.Screen2D.Draw_Line3D((float)tmp[i].x,
//                                                (float)tmp[i].y,
//                                                (float)tmp[i].z,
//                                                (float)tmp[i + 1].x,
//                                                (float)tmp[i + 1].y,
//                                                (float)tmp[i + 1].z,
//                                                entry.Color);
//        }
//        #endregion 

//        public static void Clear()
//        {
//            _displayList.Clear();
//            _textList.Clear();

//        }

//        #region public cached draw commands
//        // The idea when Rendering (as opposed to picking) is that we keep all Region's
//        // fixed and we instead update the camera's to be in the region's coordinate system
//        // and then we only have to apply a floating origin camera offset to anything we want to render.
//        // For picking, we always pick in model space.
//        // But back to rendering, the idea is that we would not have to transform every entity in
//        // every region, every frame and instead simply transform the camera.  Thus we can send
//        // the _camera space translated_ region matrix transformed vertices of any box
//        // and it was good to go.  
//        // However this now means that we need multiple 2d commit lists that are divided by 
//        // view matrix and which should be drawn after main geometry, but prior to 
//        public static void DrawText(string text, int left, int top, CONST_TV_COLORKEY color)
//        {
//            _textList.Add(new StringList(text, left, top, (int) color));
//        }


//        public static void Draw (BoundingBox box, CONST_TV_COLORKEY color)
//        {
            
//        }
//        public static void DrawArrow(ArrowHead a, CONST_TV_COLORKEY color)
//        {
//            foreach (Triangle t in a.Triangles)
//                Draw(t, color);
//        }

//        public static void Draw(Polygon p, CONST_TV_COLORKEY color, Matrix matrix)
//        {
 
//        }

//        public static void Draw(Polygon p, CONST_TV_COLORKEY color)
//        {
//            _displayList.Add(new PolygonList(p, (int)color));
//        }

        
//        public static void DrawHull(Vector3d[] vertices, CONST_TV_COLORKEY color)
//        {
//            for (int i = 0; i < vertices.Length - 1; i++)
//            {
//                Line3d[] e = new Line3d[1] {new Line3d(vertices[i], vertices[i + 1])};
//                _displayList.Add(new LineList(e, (int) color));
//            }
//        }

//        public static void DrawLines(Line3d[] linelist, Color color)
//        {
//            _displayList.Add(new LineList(linelist, color.ToInt32()));
//        }

//        public static void DrawLines(Line3d[] linelist, CONST_TV_COLORKEY color)
//        {
//            _displayList.Add(new LineList(linelist, (int)color));
//        }

//        // sometimes all you want to do is draw a line by passing in its position,angle and length without knowing its actual endpoint
//        // Assumes normal is already normalized
//        public static void DrawLine(Vector3d position, Vector3d normal, double lengthWorldSpace)
//        {
//            // compute the end point based on length
//            Vector3d endpoint, vScale;

//            vScale = normal * lengthWorldSpace;
//            endpoint = position + vScale;
//            //Core._CoreClient.Screen2D.Draw_Line3D(position.x, position.y, position.z, endpoint.x, endpoint.y, endpoint.z);
//            DrawLines(new Line3d[] { new Line3d(position, endpoint) }, CONST_TV_COLORKEY.TV_COLORKEY_GREEN);
//        }

//        public static void DrawLine (Vector3d start, Vector3d end)
//        {
//            DrawLines(new Line3d[ ]{new Line3d(start, end)}, CONST_TV_COLORKEY.TV_COLORKEY_YELLOW );
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="rows">number of rows in grid</param>
//        /// <param name="columns">number of columns in grid</param>
//        /// <param name="spacing">spacing between primary grid cells</param>
//        /// <param name="color"></param>
//        /// <param name="offset">World position offset by which to draw the grid</param>
//        /// <param name="drawAxis"></param>
//        public static void DrawGrid(uint rows, uint columns, float rowSpacing, float columnSpacing, uint innerRows, uint innerColumns, Color outerColor, Color innerColor, Vector3d offset, bool drawAxis)
//        {
//            if (rows == 0 || columns == 0 || rowSpacing <= 0.0 || columnSpacing <= 0.0) return;

//            List<Line3d> lines = new List<Line3d>();

//            //float innerSpacing = spacing / innerMultiplier;
//            float innerSpacingZ = rowSpacing / innerRows;
//            float innerSpacingX = columnSpacing / innerColumns;

//            float height = (float)offset.y; // height above origin to draw this grid

//            int halfColumns = (int)(columns / 2);
//            int halfRows = (int)(rows / 2);

//            // the small inner columns and rows
//            if (innerColumns > 0)
//            {
//                int innerHalfColumns = halfColumns * (int)innerColumns; // innerMultiplier;
//                for (int x = -innerHalfColumns; x <= innerHalfColumns; x++)
//                {
//                    float component1 = (float)((x * innerSpacingX) + offset.x);
//                    float component2 = (float)((innerHalfColumns * innerSpacingX) + offset.z);
//                    float component3 = (float)((-innerHalfColumns * innerSpacingX) + offset.z);
//                    lines.Add(new Line3d(component1, height, component3, component1, height, component2));
//                }
//            }
//            if (innerRows > 0)
//            {
//                int innerHalfRows = halfRows * (int)innerRows;// innerMultiplier;
//                for (int z = -innerHalfRows; z <= innerHalfRows; z++)
//                {
//                    float component1 = (float)((z * innerSpacingZ) + offset.z);
//                    float component2 = (float)((innerHalfRows * innerSpacingZ) + offset.x);
//                    float component3 = (float)((-innerHalfRows * innerSpacingZ) + offset.x);
//                    lines.Add(new Line3d(component3, height, component1, component2, height, component1));
//                }
//            }

//            DrawLines(lines.ToArray(), innerColor);
//            lines.Clear();

//            // the large columns and rows
//            for (int x = -halfColumns; x <= halfColumns; x++)
//            {
//                float component1 = (float)((x * columnSpacing) + offset.x);
//                float component2 = (float)((halfColumns * columnSpacing) + offset.z);
//                float component3 = (float)((-halfColumns * columnSpacing) + offset.z);
//                lines.Add(new Line3d(component1, height, component3, component1, height, component2));
//            }
//            for (int z = -halfRows; z <= halfRows; z++)
//            {
//                float component1 = (float)((z * rowSpacing) + offset.z);
//                float component2 = (float)((halfRows * rowSpacing) + offset.x);
//                float component3 = (float)((-halfRows * rowSpacing) + offset.x);
//                lines.Add(new Line3d(component3, height, component1, component2, height, component1));
//            }

//            DrawLines(lines.ToArray(), outerColor);

//            // draw the red, green, blue axis lines across the center
//            if (drawAxis)
//            {
//                DrawAxisIndicator(offset, columnSpacing,halfColumns);
//            }
//        }

//        //public static void DrawAxisIndicator(Vector3d offset, Camera camera, float length)
//        public static void DrawAxisIndicator(Vector3d offset, float columnSpacing, float halfColumns)
//        {
//            //// using 2d X,Y offset for the axis indicator on our screen, we want
//            //// to take these projected coords and turn them into world coords

//            //// typically you want the axis indicator in the same spot relative to the camera.  So to get
//            //// the actual position of the indicator it's 
//            //Vector3d position = camera.Position + offset;

//            //// simply, the axis indicator is 3 lines that meet at a common point.  Each of the three lines
//            //// only has one axis value different for its other endpoint.
//            //// so if the common point is 0,0,0
//            //// then endX = 10,0,0  (position.x + length, position.y, position.z)
//            ////      endY = 0,10,0  (position.x, position.y + length, position.z)
//            ////      endZ = 0,0,10  (position.x, position.y, position.z + length)
//            //Core._CoreClient.Screen2D.Draw_Line3D((float)position.x, (float)position.y, (float)position.z,
//            //                                (float)position.x + length, (float)position.y,
//            //                                (float)position.z);
//            //Core._CoreClient.Screen2D.Draw_Line3D((float)position.x, (float)position.y, (float)position.z,
//            //                                (float)position.x, (float)position.y + length,
//            //                                (float)position.z);
//            //Core._CoreClient.Screen2D.Draw_Line3D((float)position.x, (float)position.y, (float)position.z,
//            //                                (float)position.x, (float)position.y,
//            //                                (float)position.z + length);

//            //float r = 64;
//            //CoreClient._CoreClient.Screen2D.Draw_Line3D(0, 0, 0, 10 * r, 0, 0, CoreClient._CoreClient.Globals.RGBA(1, 0, 0, 1));
//            //CoreClient._CoreClient.Screen2D.Draw_Line3D(0, 0, 0, 0, 10 * r, 0, CoreClient._CoreClient.Globals.RGBA(0, 1, 0, 1));
//            //CoreClient._CoreClient.Screen2D.Draw_Line3D(0, 0, 0, 0, 0, 10 * r, CoreClient._CoreClient.Globals.RGBA(0, 0, 1, 1));

//            // TODO: i think i just used halfCOlumns and columnSpacing for all when i should have
//            // used appropriate ones where applicable?
//            Line3d currentLine = new Line3d((float)offset.x, (float)((halfColumns * columnSpacing) + offset.y), (float)offset.z,
//                                                          (float)offset.x, (float)((-halfColumns * columnSpacing) + offset.y), (float)offset.z); //  green for up axis
//            DrawLines(new Line3d[] { currentLine }, new Color(0f, 1.0f, 0f, 1.0f));
//            currentLine = new Line3d((float)((halfColumns * columnSpacing) + offset.x), (float)offset.y, (float)+offset.z,
//                                                 (float)((-halfColumns * columnSpacing) + offset.x), (float)offset.y, (float)+offset.z); //  red for right axis
//            DrawLines(new Line3d[] { currentLine }, new Color(1.0f, 0.0f, 0.0f, 1f));
//            currentLine = new Line3d((float)offset.x, (float)offset.y, (float)((halfColumns * columnSpacing) + offset.z),
//                                                (float)offset.x, (float)offset.y, (float)((-halfColumns * columnSpacing) + offset.z)); //  blue for front axis
//            DrawLines(new Line3d[] { currentLine }, new Color(0.0f, 0.0f, 1.0f, 1f));
//        }

//        // like drawline only it will compute the normal given 3 coordinates and draw a normal from each point
//        public static void DrawNormalVector(Vector3d v1, Vector3d v2, Vector3d v3, double lengthWorldSpace)
//        {
//            // compute the normal 
//            Vector3d normal = Triangle.FaceNormal(v1, v2, v3);

//            // draw a normal extending from the middle of each triangle in the direction of the normal
//            Vector3d c1;
//            c1.x = (v1.x + v2.x + v3.x) / 3;
//            c1.y = (v1.y + v2.y + v3.y) / 3;
//            c1.z = (v1.z + v2.z + v3.z) / 3;

//            DrawLine(c1, normal, lengthWorldSpace);
//        }

        
//        public static void DrawBox(BoundingBox box,Matrix transform, CONST_TV_COLORKEY color)
//        {
//            DebugDraw.DrawBox(BoundingBox.Transform(box, transform), color);
//        }

//        public static void DrawBox(BoundingBox box, Vector3d translation, CONST_TV_COLORKEY color)
//        {
//            Vector3d newMin = box.Min + translation;
//            Vector3d newMax = box.Max + translation;
//            DebugDraw.DrawBox(new BoundingBox(newMin, newMax), color);
//        }

//        // sometimes you dont want to have to add code to your rendering pipeline to enable/disable bounding volumes for a specific mesh.
//        // sometimes you just want to have a simple function to draw the box manually.
//        public static void DrawBox(Vector3d min, Vector3d max, CONST_TV_COLORKEY color)
//        {
//            DrawBox(new BoundingBox(min, max), color);
//        }

//        public static void DrawBox(Vector3d center, Vector3d rotation, float width, float height, float depth,
//                                   CONST_TV_COLORKEY color)
//        {
//            DrawBox(new BoundingBox(center, width), color);
//        }

//        public static void DrawBox(BoundingBox box, CONST_TV_COLORKEY color)
//        {
//            Line3d[] edges = BoundingBox.GetEdges(box);
//            _displayList.Add(new LineList(edges, (int) color));
//            // _displayList.Add(new BoxList(box, (int) color));
//        }

//        ///// <summary>
//        ///// Draws a bounding box with Quad faces, not triangles.
//        ///// </summary>
//        ///// <param name="vArray"></param>
//        ///// <param name="color"></param>
//        //private static  void DrawBox(Vector3d[] vArray, int color)
//        //{
//        //    // NOTE: These 3d line draws must be done last or else they will get overwritten.  They are just
//        //    // projected 2d lines and without vertices they dont get Zbuffer depth sorting

//        //    // the bottom 4 coords of the box form a square
//        //    Core._CoreClient.Screen2D.Draw_Line3D(vArray[0].x, vArray[0].y, vArray[0].z, vArray[1].x, vArray[1].y, vArray[1].z,
//        //                                    color);
//        //    Core._CoreClient.Screen2D.Draw_Line3D(vArray[1].x, vArray[1].y, vArray[1].z, vArray[3].x, vArray[3].y, vArray[3].z,
//        //                                    color);
//        //    Core._CoreClient.Screen2D.Draw_Line3D(vArray[3].x, vArray[3].y, vArray[3].z, vArray[2].x, vArray[2].y, vArray[2].z,
//        //                                    color);
//        //    Core._CoreClient.Screen2D.Draw_Line3D(vArray[2].x, vArray[2].y, vArray[2].z, vArray[0].x, vArray[0].y, vArray[0].z,
//        //                                    color);

//        //    // the top 4 coords of the box form a square
//        //    Core._CoreClient.Screen2D.Draw_Line3D(vArray[4].x, vArray[4].y, vArray[4].z, vArray[5].x, vArray[5].y, vArray[5].z,
//        //                                    color);
//        //    Core._CoreClient.Screen2D.Draw_Line3D(vArray[5].x, vArray[5].y, vArray[5].z, vArray[7].x, vArray[7].y, vArray[7].z,
//        //                                    color);
//        //    Core._CoreClient.Screen2D.Draw_Line3D(vArray[7].x, vArray[7].y, vArray[7].z, vArray[6].x, vArray[6].y, vArray[6].z,
//        //                                    color);
//        //    Core._CoreClient.Screen2D.Draw_Line3D(vArray[6].x, vArray[6].y, vArray[6].z, vArray[4].x, vArray[4].y, vArray[4].z,
//        //                                    color);

//        //    // lines to connect the top with the bottom
//        //    Core._CoreClient.Screen2D.Draw_Line3D(vArray[0].x, vArray[0].y, vArray[0].z, vArray[4].x, vArray[4].y, vArray[4].z,
//        //                                    color);
//        //    Core._CoreClient.Screen2D.Draw_Line3D(vArray[1].x, vArray[1].y, vArray[1].z, vArray[5].x, vArray[5].y, vArray[5].z,
//        //                                    color);
//        //    Core._CoreClient.Screen2D.Draw_Line3D(vArray[2].x, vArray[2].y, vArray[2].z, vArray[6].x, vArray[6].y, vArray[6].z,
//        //                                    color);
//        //    Core._CoreClient.Screen2D.Draw_Line3D(vArray[3].x, vArray[3].y, vArray[3].z, vArray[7].x, vArray[7].y, vArray[7].z,
//        //                                    color);
//        //}
       

//        public static void DrawSphere(Vector3d center, double radius, CONST_TV_COLORKEY color)
//        {
//            if (!initialized)
//            {
//                // TODO: debug for now we'll just build every frame
//                Triangle[] tri = GenerateTriangleFacetSphere(2);
//                GenerateCompactEdgeList(tri);

//                //convert to a point list
//                int j = 0;
//                _points = new Vector3d[_edges.Count*2];
//                for (int i = 0; i < _edges.Count; i++)
//                {
//                    _points[j] = _edges[i].Point[0];
//                    j++;
//                    _points[j] = _edges[i].Point[1];
//                    j++;
//                }
//                initialized = true;
//            }

//            // add to display list
//            _displayList.Add(new SphereList(center, radius, (int) color));
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="center"></param>
//        /// <param name="axis"></param>
//        /// <param name="angleRadians"></param>
//        /// <param name="radius"></param>
//        /// <param name="numPoints"></param>
//        /// <param name="color"></param>
//        /// <param name="offset">For camera space rendering, usually -camera position</param>
//        public static void DrawCircle3D(Vector3d center, Vector3d axis, double angleRadians, double radius, uint numPoints, CONST_TV_COLORKEY color, Vector3d offset)
//        {
//            return; // do nothing for now.
//            if (radius <= 0) throw new ArgumentOutOfRangeException();
//            int numSegments = 32; // How many segments we want, higher the number, higher the lines. higher the precision against terrain 
//            double radiansPerSeg = Math.PI  * 2 / numSegments;
//            Vector3d[] points;
//            points = new Vector3d[numSegments + 1];
         
//            for( int i = 0; i < numSegments; i++ )
//            {
//                double angle = radiansPerSeg  * i;
//                points[i].x = Math.Sin(angle) ;
//                points[i].y = 0;
//                points[i].z = Math.Cos(angle) ;
//            }

//            // add a final point to bring the circle back to the start
//            points[numSegments].x = Math.Sin(0);
//            points[numSegments].y = 0;
//            points[numSegments].z = Math.Cos(0);

//            // create a translation matrix using the center
//            Matrix translation = Matrix.Translation(center + offset);
//            Matrix rotation = Matrix.Rotation(axis, angleRadians);
//            Matrix scaling = Matrix.Scaling(new Vector3d(radius, radius, radius));

//            // transform all the points
//            Matrix transform = scaling * rotation * translation;

//            points = Vector3d.TransformCoordArray(points, transform);
//            DrawHull(points, color);
//        }

//        //public static void DrawCircleOnTerrain(TV_3DVECTOR point, TVLandscape Landscape, float siz)
//        //{
//        //    float fRadius = size * (int)Landscape.GetPrecision();
//        //    int numSegments = 32; // How many segments we want, higher the number, higher the lines. higher the precision against terrain 
//        //    float degreesPerSeg = 360.0f / (float)numSegments;
//        //    float radiansPerSeg = degreesPerSeg * (3.141592f / 180.0f);
            
//        //    // Remember centre 
//        //    TVScreen2D.Draw_Line3D(point.x, point.y, point.z, point.x, point.y + 5f, point.z, Color.Green.ToArgb(), Color.Yellow.ToArgb());

//        //    // Track first point 
//        //    float thisRadian = 0.0f;
//        //    TV_3DVECTOR firstPoint = new TV_3DVECTOR(point.x, Landscape.GetHeight(point.x, point.z + fRadius), point.z + fRadius);

//        //    // This is to remember where we were without recalculating it. 
//        //    TV_3DVECTOR lastPoint = firstPoint;
//        //    for (int seg = 0; seg < numSegments; seg++)
//        //    {
//        //        // Increment point 
//        //        thisRadian += radiansPerSeg;
//        //        TV_3DVECTOR thisPointOffset;

//        //        // Calculate next point 
//        //        thisPointOffset.x = (float)Math.Sin(thisRadian) * fRadius;
//        //        thisPointOffset.z = (float)Math.Cos(thisRadian) * fRadius;

//        //        TV_3DVECTOR thisPoint = point;
//        //        thisPoint.x += thisPointOffset.x;
//        //        thisPoint.z += thisPointOffset.z;

//        //        thisPoint.y = Landscape.GetHeight(thisPoint.x, thisPoint.z);

//        //        // Render 
//        //        TVScreen2D.Draw_Line3D(lastPoint.x, lastPoint.y, lastPoint.z,thisPoint.x, thisPoint.y, thisPoint.z,Color.Yellow.ToArgb(), Color.Yellow.ToArgb());

//        //        lastPoint = thisPoint;
//        //    }
//        //}        

       
//        //http://local.wasp.uwa.edu.au/~pbourke/modelling_rendering/sphere_cylinder/
//        //  Create a triangular facet approximation to a sphere
//        //  Return the number of facets created.
//        //  The number of facets will be (4^iterations) * 8
//        //  - Written by Paul Bourke 

//        // - Translation to c# for use with tv3d by Hypnotron Feb, 2007
//        public static Triangle[] GenerateTriangleFacetSphere(uint itterations)
//            // itterations 0 is valid.  its the 0 level object
//        {
//            int size = (int) (Math.Pow(4, itterations)*8);
//            Triangle[] f = new Triangle[size];
//            double a = (double) (1F/Math.Sqrt(2.0));

//            // Create the level 0 object. 
//            Vector3d[] p = new Vector3d[]
//                               {
//                                   new Vector3d(0, 0, a), new Vector3d(0, 0, -a), new Vector3d(-a, -a, 0),
//                                   new Vector3d(a, -a, 0), new Vector3d(a, a, 0), new Vector3d(-a, a, 0)
//                               };

//            // create the 8 triangles that make up our initial opposing pyramids  
//            // <>  <-- opposing pyramids is on its side thus.  After one itteration its symetrical with respect to all 3 axis
//            f[0] = new Triangle(p[0], p[3], p[4]);
//            f[1] = new Triangle(p[0], p[4], p[5]);
//            f[2] = new Triangle(p[0], p[5], p[2]);
//            f[3] = new Triangle(p[0], p[2], p[3]);
//            f[4] = new Triangle(p[1], p[4], p[3]);
//            f[5] = new Triangle(p[1], p[5], p[4]);
//            f[6] = new Triangle(p[1], p[2], p[5]);
//            f[7] = new Triangle(p[1], p[3], p[2]);
//            int nt = 8, ntold;

//            Vector3d pa, pb, pc;
//            // Bisect each edge and move to the surface of a unit sphere 
//            for (int it = 0; it < itterations; it++)
//            {
//                ntold = nt;
//                for (int i = 0; i < ntold; i++)
//                {
//                    pa = (f[i].Points[0] + f[i].Points[1])/2;
//                    pb = (f[i].Points[1] + f[i].Points[2])/2;
//                    pc = (f[i].Points[2] + f[i].Points[0])/2;

//                    pa = Vector3d.Normalize(pa);
//                    pb = Vector3d.Normalize(pb);
//                    pc = Vector3d.Normalize(pc);

//                    // create new triangles from the bisected points and the existing points
//                    f[nt] = new Triangle(f[i].Points[0], pa, pc);
//                    nt++;
//                    f[nt] = new Triangle(pa, f[i].Points[1], pb);
//                    nt++;
//                    f[nt] = new Triangle(pb, f[i].Points[2], pc);
//                    nt++;
//                    f[i] = new Triangle(pa, pb, pc);
//                }
//            }
//            return f;
//        }
//#endregion

//        // we dont want to create a TVMesh.  Hell we'd create a sphere using TVMesh.CreateSphere() if we did.
//        // however, if we did want to create a TVMesh, we would want to itterate and remove shared vertices
//        // this would reduce by 66% the number of verts and then we'd use SetGeometryEx() with the indexed list of verts.
//        // Actually, for that we'd be better off updating the Generation code to create the indexed primitive off the bat
//        // we just want a line list we can draw
//        // this code copied from my OcclusionFrustum.cs class. TODO: Should share this code instead of duplicating it
//        private static void GenerateCompactEdgeList(Triangle[] tri)
//        {
//            if (tri == null) throw new ArgumentException();
//            _edges.Clear();
//            foreach (Triangle t in tri)
//            {
//                Line3d e = new Line3d(t.Points[0], t.Points[1]);
//                UpdateEdges(e);
//                e = new Line3d(t.Points[1], t.Points[2]);
//                UpdateEdges(e);
//                e = new Line3d(t.Points[2], t.Points[0]);
//                UpdateEdges(e);
//            }
//        }

//        // copied from my OcclusionFrustum.cs class.  TODO: Should share this code instead of duplicating it...
//        private static void UpdateEdges(Line3d e)
//        {
//            foreach (Line3d edge in _edges)
//            {
//                if (edge == e)
//                {
//                    // _edges.Remove(edge); // except for trianglulated sphere edge list , we dont remove!
//                    return;
//                }
//            }
//            _edges.Add(e);
//        }

//        // http://paulbourke.net/miscellaneous/sphere_cylinder/
//        // Polar Coordinate Sphere
//        //Create a unit sphere centered at the origin 
//        //This code illustrates the concept rather than implements it efficiently
//        //It is called with two arguments, the theta and phi angle increments in degrees
//        //Note that at the poles only 3 vertex facet result while the rest of the sphere has 4 point facets
//        private void CreateUnitSphere(int theta_degrees, int phi_degrees)
//        {
//            int n;
//            int theta, phi;
//            Vector3d[] p;
//            double DTOR = Utilities.MathHelper.DEGREES_TO_RADIANS;

//            for (theta = -90; theta <= 90 - theta_degrees; theta += theta_degrees)
//            {
//                for (phi = 0; phi <= 360 - phi_degrees; phi += phi_degrees)
//                {
//                    double cos_theta_dtor = Math.Cos(theta*DTOR);
//                    double sin_theta_dtor = Math.Sin(theta*DTOR);
//                    double cos_phi_dtor = Math.Cos(phi*DTOR);
//                    double sin_ph_dtor = Math.Sin(phi*DTOR);
//                    double cos_thetaplusdtheta_dtor = Math.Cos((theta + theta_degrees)*DTOR);
//                    double sin_thetaplusdtheta_dtor = Math.Sin((theta + theta_degrees)*DTOR);
//                    double cos_phiplusdphi_dtor = Math.Cos((phi + phi_degrees)*DTOR);
//                    double sin_phiplusdphi_dtor = Math.Sin((phi + phi_degrees)*DTOR);

//                    if (theta > -90 && theta < 90)
//                        p = new Vector3d[4];
//                    else
//                        p = new Vector3d[3];

//                    n = 0;
//                    p[n].x = cos_theta_dtor*cos_phi_dtor;
//                    p[n].y = cos_theta_dtor*sin_ph_dtor;
//                    p[n].z = sin_theta_dtor;
//                    n++;
//                    p[n].x = cos_thetaplusdtheta_dtor*cos_phi_dtor;
//                    p[n].y = cos_thetaplusdtheta_dtor*sin_ph_dtor;
//                    p[n].z = sin_thetaplusdtheta_dtor;
//                    n++;
//                    p[n].x = cos_thetaplusdtheta_dtor*cos_phiplusdphi_dtor;
//                    p[n].y = cos_thetaplusdtheta_dtor*sin_phiplusdphi_dtor;
//                    p[n].z = sin_thetaplusdtheta_dtor;
//                    n++;
//                    if (theta > -90 && theta < 90)
//                    {
//                        p[n].x = cos_theta_dtor*cos_phiplusdphi_dtor;
//                        p[n].y = cos_theta_dtor*sin_phiplusdphi_dtor;
//                        p[n].z = sin_theta_dtor;
//                        n++;
//                    }

//                    // Do something with the n vertex facet p 
//                }
//            }
//        }
//    }
//}