////------------------------------------------------------------------------------
////           Name: dx9cs_multiple_vertex_buffers.cs
////         Author: Kevin Harris
////  Last Modified: 06/15/05
////    Description: This sample demonstrates how to create 3D geometry with 
////                 Direct3D by loading vertex data into a multiple Vertex 
////                 Buffers.
////------------------------------------------------------------------------------
//using System;
//using System.Drawing;
//using Microsoft.DirectX;
//using Microsoft.DirectX.Direct3D;

//namespace Keystone.Loaders
//{
//    public class MultistreamVertexBuffer
//    {
//        private Device d3dDevice = null;
//        private VertexBuffer vertexBuffer = null;
//        private VertexBuffer colorBuffer = null;
//        private VertexBuffer texCoordBuffer = null;
//        Texture texture = null;

//        struct Vertex
//        {
//            float x, y, z;

//            public Vertex( float _x, float _y, float _z )
//            {
//                x = _x;
//                y = _y;
//                z = _z;
//            }
//        };

//        Vertex[] cubeVertices =
//        {
//            new Vertex(-1.0f, 1.0f,-1.0f ),
//            new Vertex( 1.0f, 1.0f,-1.0f ),
//            new Vertex(-1.0f,-1.0f,-1.0f ),
//            new Vertex( 1.0f,-1.0f,-1.0f ),

//            new Vertex(-1.0f, 1.0f, 1.0f ),
//            new Vertex(-1.0f,-1.0f, 1.0f ),
//            new Vertex( 1.0f, 1.0f, 1.0f ),
//            new Vertex( 1.0f,-1.0f, 1.0f ),

//            new Vertex(-1.0f, 1.0f, 1.0f ),
//            new Vertex( 1.0f, 1.0f, 1.0f ),
//            new Vertex(-1.0f, 1.0f,-1.0f ),
//            new Vertex( 1.0f, 1.0f,-1.0f ),

//            new Vertex(-1.0f,-1.0f, 1.0f ),
//            new Vertex(-1.0f,-1.0f,-1.0f ),
//            new Vertex( 1.0f,-1.0f, 1.0f ),
//            new Vertex( 1.0f,-1.0f,-1.0f ),

//            new Vertex( 1.0f, 1.0f,-1.0f ),
//            new Vertex( 1.0f, 1.0f, 1.0f ),
//            new Vertex( 1.0f,-1.0f,-1.0f ),
//            new Vertex( 1.0f,-1.0f, 1.0f ),

//            new Vertex(-1.0f, 1.0f,-1.0f ),
//            new Vertex(-1.0f,-1.0f,-1.0f ),
//            new Vertex(-1.0f, 1.0f, 1.0f ),
//            new Vertex(-1.0f,-1.0f, 1.0f )
//        };

//        struct DiffuseColor
//        {
//            int color;

//            public DiffuseColor( int _color )
//            {
//               color = _color;
//            }
//        };

//        DiffuseColor[] cubeColors =
//        {
//            new DiffuseColor( Color.Red.ToArgb() ),
//            new DiffuseColor( Color.Red.ToArgb() ),
//            new DiffuseColor( Color.Red.ToArgb() ),
//            new DiffuseColor( Color.Red.ToArgb() ),

//            new DiffuseColor( Color.Green.ToArgb() ),
//            new DiffuseColor( Color.Green.ToArgb() ),
//            new DiffuseColor( Color.Green.ToArgb() ),
//            new DiffuseColor( Color.Green.ToArgb() ),

//            new DiffuseColor( Color.Blue.ToArgb() ),
//            new DiffuseColor( Color.Blue.ToArgb() ),
//            new DiffuseColor( Color.Blue.ToArgb() ),
//            new DiffuseColor( Color.Blue.ToArgb() ),

//            new DiffuseColor( Color.Yellow.ToArgb() ),
//            new DiffuseColor( Color.Yellow.ToArgb() ),
//            new DiffuseColor( Color.Yellow.ToArgb() ),
//            new DiffuseColor( Color.Yellow.ToArgb() ),

//            new DiffuseColor( Color.Magenta.ToArgb() ),
//            new DiffuseColor( Color.Magenta.ToArgb() ),
//            new DiffuseColor( Color.Magenta.ToArgb() ),
//            new DiffuseColor( Color.Magenta.ToArgb() ),

//            new DiffuseColor( Color.Cyan.ToArgb() ),
//            new DiffuseColor( Color.Cyan.ToArgb() ),
//            new DiffuseColor( Color.Cyan.ToArgb() ),
//            new DiffuseColor( Color.Cyan.ToArgb() )
//        };

//        struct TexCoord
//        {
//            float tu, tv;

//            public TexCoord( float _tu, float _tv )
//            {
//                tu = _tu;
//                tv = _tv;
//            }
//        };

//        TexCoord[] cubeTexCoords =
//        {
//            new TexCoord( 0.0f, 0.0f ),
//            new TexCoord( 1.0f, 0.0f ),
//            new TexCoord( 0.0f, 1.0f ),
//            new TexCoord( 1.0f, 1.0f ),

//            new TexCoord( 1.0f, 0.0f ),
//            new TexCoord( 1.0f, 1.0f ),
//            new TexCoord( 0.0f, 0.0f ),
//            new TexCoord( 0.0f, 1.0f ),

//            new TexCoord( 0.0f, 0.0f ),
//            new TexCoord( 1.0f, 0.0f ),
//            new TexCoord( 0.0f, 1.0f ),
//            new TexCoord( 1.0f, 1.0f ),

//            new TexCoord( 0.0f, 0.0f ),
//            new TexCoord( 1.0f, 0.0f ),
//            new TexCoord( 0.0f, 1.0f ),
//            new TexCoord( 1.0f, 1.0f ),

//            new TexCoord( 0.0f, 0.0f ),
//            new TexCoord( 1.0f, 0.0f ),
//            new TexCoord( 0.0f, 1.0f ),
//            new TexCoord( 1.0f, 1.0f ),

//            new TexCoord( 1.0f, 0.0f ),
//            new TexCoord( 1.0f, 1.0f ),
//            new TexCoord( 0.0f, 0.0f ),
//            new TexCoord( 0.0f, 1.0f )
//        };

//        /// <summary>
//        /// This event-handler is a good place to create and initialize any 
//        /// Direct3D related objects, which may become invalid during a 
//        /// device reset.
//        /// </summary>
//        public void OnResetDevice(object sender, EventArgs e)
//        {
//            Device device = (Device)sender;
//            device.Transform.Projection = 
//                Matrix.PerspectiveFovLH( Geometry.DegreeToRadian( 45.0f ),
//                (float)this.ClientSize.Width / this.ClientSize.Height,
//                0.1f, 100.0f );

//            device.RenderState.ZBufferEnable = true;
//            device.RenderState.Lighting = false;

//            //
//            // Create a vertex buffer that contains only the cube's vertex data
//            //
//            vertexBuffer = new VertexBuffer( typeof(Vertex),
//                                             cubeVertices.Length, device,
//                                             Usage.Dynamic | Usage.WriteOnly,
//                                             VertexFormats.Position,
//                                             Pool.Default );

//            // graphic stream write  vs SetData article
//            // http://www.gamedev.net/community/forums/topic.asp?topic_id=346308
//            GraphicsStream gStream = vertexBuffer.Lock( 0, 0, LockFlags.None );
//            gStream.Write( cubeVertices );
//            vertexBuffer.Unlock();
//            //
//            // Create a vertex buffer that contains only the cube's color data
//            //
//            colorBuffer = new VertexBuffer( typeof(DiffuseColor),
//                                            cubeVertices.Length, device,
//                                            Usage.Dynamic | Usage.WriteOnly,
//                                            VertexFormats.Diffuse,
//                                            Pool.Default );

//            gStream = null;
//            gStream = colorBuffer.Lock( 0, 0, LockFlags.None );
//            gStream.Write( cubeColors );
//            colorBuffer.Unlock();

//            //
//            // Create a vertex buffer that contains only the cube's texture coordinate data
//            //
//            texCoordBuffer = new VertexBuffer( typeof(TexCoord),
//                                               cubeVertices.Length, device,
//                                               Usage.Dynamic | Usage.WriteOnly,
//                                               VertexFormats.Texture1,
//                                               Pool.Default );

//            gStream = null;
//            gStream = texCoordBuffer.Lock( 0, 0, LockFlags.None );
//            gStream.Write( cubeTexCoords );
//            texCoordBuffer.Unlock();

//            //
//            // Create a vertex declaration so we can describe to Direct3D how we'll 
//            // be passing our data to it.
//            //

//            // Create the vertex element array.
//            VertexElement[] elements = new VertexElement[]
//            {
//                //		        Stream  Offset        Type                    Method                 Usage                      Usage Index
//                new VertexElement( 0,     0,  DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Position,          0),
//                new VertexElement( 1,     0,  DeclarationType.Color,  DeclarationMethod.Default, DeclarationUsage.Color,             0),
//                new VertexElement( 2,     0,  DeclarationType.Float2, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 0),

//                VertexElement.VertexDeclarationEnd 
//            };

//            // Use the vertex element array to create a vertex declaration.
//            d3dDevice.VertexDeclaration = new VertexDeclaration( d3dDevice, elements );
//        }

//        /// <summary>
//        /// This method is dedicated completely to rendering our 3D scene and is
//        /// is called by the OnPaint() event-handler.
//        /// </summary>
//        private void Render()
//        {

//            d3dDevice.SetStreamSource( 0, vertexBuffer,   0 );
//            d3dDevice.SetStreamSource( 1, colorBuffer,    0 );
//            d3dDevice.SetStreamSource( 2, texCoordBuffer, 0 );

//            d3dDevice.SetTexture( 0, texture );

//            d3dDevice.DrawPrimitives( PrimitiveType.TriangleStrip,  0, 2 );
            
//        }
//    }
//}

