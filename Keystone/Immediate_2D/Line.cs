// Line.cs
// Copyright 2007 Michael Anderson
// Version 1.0 -- January 13, 2007

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion


namespace StrokeFontDemo
{
    /// <summary>
    /// Represents a single line segment.  Drawing is handled by the LineManager class.
    /// </summary>
    public class Line
    {
        public Vector2 p1; // Begin point of the line
        public Vector2 p2; // End point of the line
        public float radius = 0.1f; // The line's total thickness is twice its radius
        public Vector2 rhoTheta; // Length and angle of the line
        public float rho { get { return rhoTheta.X; } }
        public float theta { get { return rhoTheta.Y; } }
        public Color color = Color.White;


        public Line(Vector2 p1, Vector2 p2)
        {
            this.p1 = p1;
            this.p2 = p2;
            Recalc();
        }


        public Line(Vector2 p1, Vector2 p2, float radius)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.radius = radius;
            Recalc();
        }


        public Line(Vector2 p1, Vector2 p2, float radius, Color color)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.radius = radius;
            this.color = color;
            Recalc();
        }


        public void Move(Vector2 p1, Vector2 p2)
        {
            this.p1 = p1;
            this.p2 = p2;
            Recalc();
        }


        public void Recalc()
        {
            Vector2 delta = p2 - p1;
            float rho = delta.Length();
            float theta = (float)Math.Atan2(delta.Y, delta.X);
            rhoTheta = new Vector2(rho, theta);
        }


        /// <summary>
        /// Distance squared from an arbitrary point p to this line.
        /// </summary>
        /// <remarks>
        /// See http://geometryalgorithms.com/Archive/algorithm_0102/algorithm_0102.htm, near the bottom.
        /// </remarks>
        public float DistanceSquaredToPoint(Vector2 p)
        {
            Vector2 v = p2 - p1; // Vector from line's p1 to p2
            Vector2 w = p - p1; // Vector from line's p1 to p

            // See if p is closer to p1 than to the segment
            float c1 = Vector2.Dot(w, v);
            if (c1 <= 0)
                return Vector2.DistanceSquared(p, p1);

            // See if p is closer to p2 than to the segment
            float c2 = Vector2.Dot(v, v);
            if (c2 <= c1)
                return Vector2.DistanceSquared(p, p2);

            // p is closest to point pB, between p1 and p2
            float b = c1 / c2;
            Vector2 pB = p1 + b * v;
            return Vector2.DistanceSquared(p, pB);
        }


        public Matrix WorldMatrix()
        {
            Matrix rotate = Matrix.CreateRotationZ(theta);
            Matrix translate = Matrix.CreateTranslation(p1.X, p1.Y, 0);
            return rotate * translate;
        }
    };


    /// <summary>
    /// Class to handle drawing a list of lines.
    /// </summary>
    class LineManager
    {
        GraphicsDevice device;
        private Effect effect;
        private EffectParameter wvpMatrixParameter;
        private EffectParameter timeParameter;
        private EffectParameter lengthParameter;
        private EffectParameter radiusParameter;
        private EffectParameter lineColorParameter;
        private VertexBuffer vb;
        private IndexBuffer ib;
        private VertexDeclaration vdecl;
        private int numVertices;
        private int numIndices;
        private int numPrimitives;
        private int bytesPerVertex;

        public void Init(GraphicsDevice device, ContentManager content)
        {
            this.device = device;
            effect = content.Load<Effect>("Line");
            wvpMatrixParameter = effect.Parameters["worldViewProj"];
            timeParameter = effect.Parameters["time"];
            lengthParameter = effect.Parameters["length"];
            radiusParameter = effect.Parameters["radius"];
            lineColorParameter = effect.Parameters["lineColor"];

            CreateLineMesh();
        }


        /// <summary>
        /// Create a mesh for a line.
        /// </summary>
        /// <remarks>
        /// The lineMesh has 3 sections:
        /// 1.  Two quads, from 0 to 1 (left to right)
        /// 2.  A half-disc, off the left side of the quad
        /// 3.  A half-disc, off the right side of the quad
        ///
        /// The X and Y coordinates of the "normal" encode the rho and theta of each vertex
        /// The "texture" encodes how much to scale and translate the vertex horizontally by length and radius
        /// </remarks>
        private void CreateLineMesh()
        {
            const int MAXRES = 15; // A higher MAXRES produces rounder endcaps at the cost of more vertices

            numVertices = 6 + (MAXRES + 2) + (MAXRES + 2);
            numPrimitives = 4 + MAXRES + MAXRES;
            numIndices = 3 * numPrimitives;
            short[] indices = new short[numIndices];
            bytesPerVertex =  VertexPositionNormalTexture.SizeInBytes;
            VertexPositionNormalTexture[] tri = new VertexPositionNormalTexture[numVertices];

            // quad vertices
            int iVertexBase = 0;
            tri[0] = new VertexPositionNormalTexture(new Vector3(0.0f, -1.0f, 0), new Vector3(1, 0, 0), new Vector2(0, 0));
            tri[1] = new VertexPositionNormalTexture(new Vector3(0.0f, -1.0f, 0), new Vector3(1, 0, 0), new Vector2(0, 1));
            tri[2] = new VertexPositionNormalTexture(new Vector3(0.0f, 0.0f, 0), new Vector3(0, 0, 0), new Vector2(0, 1));
            tri[3] = new VertexPositionNormalTexture(new Vector3(0.0f, 0.0f, 0), new Vector3(0, 0, 0), new Vector2(0, 0));
            tri[4] = new VertexPositionNormalTexture(new Vector3(0.0f, 1.0f, 0), new Vector3(1, 0, 0), new Vector2(0, 1));
            tri[5] = new VertexPositionNormalTexture(new Vector3(0.0f, 1.0f, 0), new Vector3(1, 0, 0), new Vector2(0, 0));

            // quad indices
            int iIndexBase = 0;
            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            indices[3] = 2;
            indices[4] = 3;
            indices[5] = 0;
            indices[6] = 2;
            indices[7] = 4;
            indices[8] = 3;
            indices[9] = 4;
            indices[10] = 5;
            indices[11] = 3;

            iVertexBase = 6;
            iIndexBase = 12;

            // left halfdisc vertices
            for (int i = 0; i < (MAXRES + 2); i++)
            {
                float x;
                float y;
                float theta;
                float distFromCenter;
                if (i == 0)
                {
                    x = 0;
                    y = 0;
                    theta = 0;
                    distFromCenter = 0;
                }
                else
                {
                    theta = (float)(i - 1) / (2 * MAXRES) * MathHelper.TwoPi + MathHelper.PiOver2;
                    x = (float)Math.Cos(theta);
                    y = (float)Math.Sin(theta);
                    distFromCenter = 1;
                }
                tri[iVertexBase + i] = new VertexPositionNormalTexture(new Vector3(x, y, 0), new Vector3(distFromCenter, 0, 0), new Vector2(1, 0));
            }

            // left halfdisc indices
            int iIndex = 0;
            for (int iPrim = 0; iPrim < MAXRES; iPrim++)
            {
                indices[iIndexBase + iIndex++] = (short)(iVertexBase + 0);
                indices[iIndexBase + iIndex++] = (short)(iVertexBase + iPrim + 1);
                indices[iIndexBase + iIndex++] = (short)(iVertexBase + iPrim + 2);
            }

            iVertexBase += (MAXRES + 2);
            iIndexBase += MAXRES * 3;

            // right halfdisc vertices
            for (int i = 0; i < (MAXRES + 2); i++)
            {
                float x;
                float y;
                float theta;
                float distFromCenter;
                if (i == 0)
                {
                    x = 0.0f;
                    y = 0;
                    theta = 0;
                    distFromCenter = 0;
                }
                else
                {
                    theta = (float)(i - 1) / (2 * MAXRES) * MathHelper.TwoPi - MathHelper.PiOver2;
                    x = (float)Math.Cos(theta);
                    y = (float)Math.Sin(theta);
                    distFromCenter = 1;
                }
                tri[iVertexBase + i] = new VertexPositionNormalTexture(new Vector3(x, y, 0), new Vector3(distFromCenter, 0, 0), new Vector2(1, 1));
            }

            // right halfdisc indices
            iIndex = 0;
            for (int iPrim = 0; iPrim < MAXRES; iPrim++)
            {
                indices[iIndexBase + iIndex++] = (short)(iVertexBase + 0);
                indices[iIndexBase + iIndex++] = (short)(iVertexBase + iPrim + 1);
                indices[iIndexBase + iIndex++] = (short)(iVertexBase + iPrim + 2);
            }

            vb = new VertexBuffer(device, numVertices * bytesPerVertex, ResourceUsage.None, ResourceManagementMode.Automatic);
            vb.SetData<VertexPositionNormalTexture>(tri);
            vdecl = new VertexDeclaration(device, VertexPositionNormalTexture.VertexElements);

            ib = new IndexBuffer(device, numIndices * 2, ResourceUsage.None, ResourceManagementMode.Automatic, IndexElementSize.SixteenBits);
            ib.SetData<short>(indices);
        }


        /// <summary>
        /// Draw a list of Lines.
        /// </summary>
        /// <remarks>
        /// Set globalRadius = 0 to use the radius stored in each Line.
        /// Set globalColor to Color.TransparentBlack to use the color stored in each Line.
        /// </remarks>
        public void Draw(List<Line> lineList, float globalRadius, Color globalColor, Matrix viewMatrix, Matrix projMatrix, float time, string techniqueName, Matrix globalWorldMatrix)
        {
            Vector4 lineColor;
            bool uniqueColors = false;
            if (techniqueName == null)
                effect.CurrentTechnique = effect.Techniques[0];
            else
                effect.CurrentTechnique = effect.Techniques[techniqueName];
            effect.Begin();
            EffectPass pass = effect.CurrentTechnique.Passes[0];
            device.VertexDeclaration = vdecl;
            device.Vertices[0].SetSource(vb, 0, bytesPerVertex);
            device.Indices = ib;

            pass.Begin();

            timeParameter.SetValue(time);
            if (globalColor == Color.TransparentBlack)
            {
                uniqueColors = true;
            }
            else
            {
                lineColor = new Vector4((float)globalColor.R / 255, (float)globalColor.G / 255, (float)globalColor.B / 255, (float)globalColor.A / 255);
                lineColorParameter.SetValue(lineColor);
            }
            if (globalRadius != 0)
                radiusParameter.SetValue(globalRadius);

            foreach (Line line in lineList)
            {
                Matrix worldViewProjMatrix = line.WorldMatrix() * globalWorldMatrix * viewMatrix * projMatrix;
                wvpMatrixParameter.SetValue(worldViewProjMatrix);
                lengthParameter.SetValue(line.rho);
                if (globalRadius == 0)
                    radiusParameter.SetValue(line.radius);
                if (uniqueColors)
                {
                    lineColor = new Vector4((float)line.color.R / 255, (float)line.color.G / 255, (float)line.color.B / 255, (float)line.color.A / 255);
                    lineColorParameter.SetValue(lineColor);
                }
                effect.CommitChanges();
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, numVertices, 0, numPrimitives);
            }
            pass.End();

            effect.End();
        }
    }
}
