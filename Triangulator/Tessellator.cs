#region "Credits"
    // Translation to VB.NET by Hypnotron June 30th 2005
#endregion
#region "License"
//BSD License
//Copyright ?2003-2004 Randy Ridge
//http://www.randyridge.com/Tao/Default.aspx
//All rights reserved.

//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions
//are met:

//1. Redistributions of source code must retain the above copyright notice,
//   this list of conditions and the following disclaimer.

//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.

//3. Neither Randy Ridge nor the names of any Tao contributors may be used to
//   endorse or promote products derived from this software without specific
//   prior written permission.

//   THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
//   "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
//   LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
//   FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE
//   COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
//   INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
//   BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//   LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
//   CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
//   LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
//   ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
//   POSSIBILITY OF SUCH DAMAGE.

#endregion

#region "Original Credits / License"

// Copyright (c) 1993-1997, Silicon Graphics, Inc.
// ALL RIGHTS RESERVED 
// Permission to use, copy, modify, and distribute this software for 
// any purpose and without fee is hereby granted, provided that the above
// copyright notice appear in all copies and that both the copyright notice
// and this permission notice appear in supporting documentation, and that 
// the name of Silicon Graphics, Inc. not be used in advertising
// or publicity pertaining to distribution of the software without specific,
// written prior permission. 
//
// THE MATERIAL EMBODIED ON THIS SOFTWARE IS PROVIDED TO YOU "AS-IS"
// AND WITHOUT WARRANTY OF ANY KIND, EXPRESS, IMPLIED OR OTHERWISE,
// INCLUDING WITHOUT LIMITATION, ANY WARRANTY OF MERCHANTABILITY OR
// FITNESS FOR A PARTICULAR PURPOSE.  IN NO EVENT SHALL SILICON
// GRAPHICS, INC.  BE LIABLE TO YOU OR ANYONE ELSE FOR ANY DIRECT,
// SPECIAL, INCIDENTAL, INDIRECT OR CONSEQUENTIAL DAMAGES OF ANY
// KIND, OR ANY DAMAGES WHATSOEVER, INCLUDING WITHOUT LIMITATION,
// LOSS OF PROFIT, LOSS OF USE, SAVINGS OR REVENUE, OR THE CLAIMS OF
// THIRD PARTIES, WHETHER OR NOT SILICON GRAPHICS, INC.  HAS BEEN
// ADVISED OF THE POSSIBILITY OF SUCH LOSS, HOWEVER CAUSED AND ON
// ANY THEORY OF LIABILITY, ARISING OUT OF OR IN CONNECTION WITH THE
// POSSESSION, USE OR PERFORMANCE OF THIS SOFTWARE.
// 
// US Government Users Restricted Rights 
// Use, duplication, or disclosure by the Government is subject to
// restrictions set forth in FAR 52.227.19(c)(2) or subparagraph
// (c)(1)(ii) of the Rights in Technical Data and Computer Software
// clause at DFARS 252.227-7013 and/or in similar or successor
// clauses in the FAR or the DOD or NASA FAR Supplement.
// Unpublished-- rights reserved under the copyright laws of the
// United States.  Contractor/manufacturer is Silicon Graphics,
// Inc., 2011 N.  Shoreline Blvd., Mountain View, CA 94039-7311.
//
// OpenGL(R) is a registered trademark of Silicon Graphics, Inc.
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Triangulator;

unsafe public class TessVertexData
{
    public double* Points;
    public int* Index;
    
    public TessVertexData (int index, double a, double b, double c)
    {
        unsafe
        {
            Index = (int*)Marshal.AllocCoTaskMem(4);
            
            //Index = new int();
            *Index = index;
            //*Points = new double[3] ;  
            
            //*Points[0] = a;
            //*Points[1] = b;
            //*Points[2] = c;

            Points = (double*)Marshal.AllocCoTaskMem(24);
            *Points = a;
            *(Points + 1) = b;
            *(Points + 2) = c;
        }
    }

    ~TessVertexData()
    {
        Index = null;
        Points = null;
    }
}

public class Tessellator : IDisposable
{

    public enum GL_PRIMITIVE
    {
        GL_POINTS = 0x0000,
        GL_LINES = 0x0001,
        GL_LINE_LOOP = 0x0002,
        GL_LINE_STRIP = 0x0003,
        GL_TRIANGLES = 0x0004,
        GL_TRIANGLE_STRIP = 0x0005,
        GL_TRIANGLE_FAN = 0x0006,
        GL_QUADS = 0x0007,
        GL_QUAD_STRIP = 0x0008,
        GL_POLYGON = 0x0000
    }

    private AutoResetEvent synchOperation = new AutoResetEvent(false);
    private Glu.GLUtesselator _tess;
    private IntPtr[] vPtr;
    unsafe private TessVertexData[] _inPut;
    private List<int> _outPut;
    
    private bool _IsReady = false;
    private GL_PRIMITIVE _Mode;
    private bool _isDisposed ;


    public  Tessellator()
    {
        _tess = Glu.gluNewTess();

        Glu.gluTessCallback(_tess, Glu.GLU_TESS_VERTEX, new Glu.TessVertexCallback(Vertex));
        Glu.gluTessCallback(_tess, Glu.GLU_TESS_BEGIN, new Glu.TessBeginCallback(Begin));
        Glu.gluTessCallback(_tess, Glu.GLU_TESS_END, new Glu.TessEndCallback(End));
        Glu.gluTessCallback(_tess, Glu.GLU_TESS_ERROR, new Glu.TessErrorCallback(Error));
        Glu.gluTessCallback(_tess, Glu.GLU_TESS_COMBINE, new Glu.TessCombineCallback1(Combine));
        Glu.gluTessCallback(_tess, Glu.GLU_TESS_EDGE_FLAG, new Glu.TessEdgeFlagCallback(Edge));
        _isDisposed = false;
    }

    ~Tessellator()
    {
        if (!_isDisposed) Dispose();
    }
    public void Dispose() 
    {
        Glu.gluDeleteTess(_tess);
        _isDisposed = true;
    }

    public bool TessellatePolygon (double[] coordinates, int[] indices, bool returnVertices, out int[] resultIndices, out double[] newVertices)
    {
        // to simply useage of this library, the user simply provides the x,y,z coordinates of each vertex and the indices of each vertex
        // thus indices.Length must equal coordinates.Length / 3
        if (coordinates.Length/3 != indices.Length) throw new ArgumentOutOfRangeException ("Coordinates array length divided by 3 must equal indices length");
        if (!PolygonIndicesUnique(indices)) throw new ArgumentOutOfRangeException ("Values for indices must all be unique");

        Clear();
        
        // create a vertex list to persist this data
        _inPut = new TessVertexData[indices.Length];
        int j = 0;
        for (int i = 0; i < coordinates.Length; i += 3)
        {
            TessVertexData v = new TessVertexData(indices[j], coordinates[i], coordinates[i + 1], coordinates[i + 2]);
            _inPut[j++] = v;
        }

        // use a wait mechanism here to allow tesselation to occur 
        System.Threading.WaitHandle _wait = synchOperation;
        
        BeginTessellation();
        _wait.WaitOne(1000, false);

        newVertices = null;
        resultIndices = _outPut.ToArray();

        for (int i = 0; i < _inPut.Length; i++)
        {
               _inPut[i] = null;
        }
        
        return true;
    }
    
    unsafe private void BeginTessellation()
    {
        //        Dim coords(2) As Double
        //        Dim index As Int32
        //        ReDim vPtr(0 To face.vertexCount - 1)

        Glu.gluTessBeginPolygon(_tess, IntPtr.Zero);
        Glu.gluTessBeginContour(_tess);

        for (int i = 0; i < _inPut.Length; i++)
        {
            
            //        For i As Int32 = 0 To face.vertexCount - 1
            //            coords(0) = face.vertex(i).point.x
            //            coords(1) = face.vertex(i).point.y
            //            coords(2) = face.vertex(i).point.z

            //            ' allocate an array of unmanaged blocks of memory to hold the index of the current vertex
            //            vPtr(i) = System.Runtime.InteropServices.Marshal.AllocHGlobal(System.Runtime.InteropServices.Marshal.SizeOf(index))
            //            System.Runtime.InteropServices.Marshal.WriteInt32(vPtr(i), 0, face.vertex(i).index)

            //            'System.Runtime.InteropServices.Marshal.StructureToPtr(face.vertex(i), vPtr(i), False)

            //            'The first parameter, tess, is the handle to the tesselation object. The second parameter, coords, 
            //            'is a pointer to an array of three doubles specifying the vertex coordinates. Note that you can pass 
            //            'a pointer to any custom vertex structure, as long as the first entries contain the x, y, and z coordinates
            //            ' of the vertex as double-precision floating-point values. (our SFVertex does not.  It uses singles)

            //            'The third parameter, data, is an opaque pointer that is passed to the vertex callback as is. 
            //            'While the coords parameter is used to pass the position of the vertex to the GLU tesselator, 
            //            'you can use the data parameter to identify the vertex after the triangulation took place. 
            //            'The accompanying sample application, for example, stores the vertex data in a vector of custom vertex structures. 
            //            'In addition to the vertex position, these structures store a normal vector and a pair of texture coordinates. 
            //            ' But the only requirement is that the ptr point to data that we can use to identify the vertex so that we can
            //            ' build new triangles from the tesselated face which uses 99.9% of the time pre-existing vertices of that face.
            //            ' Very rarely does it have to generate a new vertex to complete tessellation.
            Glu.gluTessVertex(_tess, _inPut[i].Points, _inPut[i].Index);
        }

        Glu.gluTessEndContour(_tess);
        Glu.gluTessEndPolygon(_tess);

        //        tempVertices = generateTriangleFaces(returnTriangleLists)

        //        ' free unmanaged memory used by our array of pointers
        //        For i As Int32 = 0 To face.vertexCount - 1
        //            System.Runtime.InteropServices.Marshal.FreeHGlobal(vPtr(i))
        //        Next

        //        Return tempVertices
    }

    private void Clear()
    {
        _inPut = null;
        if (_outPut != null) _outPut.Clear();
        synchOperation.Reset();
    }
    
    private int GetNewIndex()
    {
        return 999999999;
    }
    private bool PolygonIndicesUnique(int[] indices)
    {
        return true;
    }
    //Public Function TessellateFace(ByRef face As X3D.SFFace, ByVal returnTriangleLists As Boolean) As X3D.SFVertex()
    //    Dim tempVertices() As X3D.SFVertex
    //    Debug.Assert(face.vertexCount >= 3, "Tessellator:TesselateFace() -- Fewer than three vertices specified.")
    //    If face.vertexCount = 3 Then
    //        ' NOTE: oIndexedFaceSet.ccw = TRUE  because the face indices are in counter-clockwise order
    //        '       So whenever ccw=TRUE (which is always for now but we need proper testing here)
    //        '       we need to reverse them since TV3D is a DirectX renderer and not OpengGL.
    //        ' swap to clockwise order. 
    //        ReDim tempVertices(2)
    //        For i As Int32 = 0 To 2
    //            tempVertices(i) = face.vertex(i)
    //        Next
    //        Return tempVertices

    //    ElseIf face.vertexCount = 4 Then
    //        '  ' we'll tesselate these ourselves since its a trivial case.  
    //        ReDim tempVertices(0 To 5)
    //        tempVertices(0) = face.vertex(0)  'cw=0  'ccw = 0
    //        tempVertices(1) = face.vertex(2)  'cw=3  'ccw = 2
    //        tempVertices(2) = face.vertex(3)  'cw=2  'ccw = 3

    //        tempVertices(3) = face.vertex(2) 'cw=2  'ccw = 0 
    //        tempVertices(4) = face.vertex(0) 'cw=1  'ccw = 1
    //        tempVertices(5) = face.vertex(1) 'cw=0  'ccw = 2
    //        Return tempVertices

    //        'Debug.Print("VisitorInit:CreateTVMesh() -- Tesselating a quad into two triangles.")
    //    ElseIf face.vertexCount > 4 Then
    //        Dim coords(2) As Double
    //        Dim index As Int32
    //        ReDim vPtr(0 To face.vertexCount - 1)
    //        _vertexList.Clear()

    //        Glu.gluTessBeginPolygon(_tess, IntPtr.Zero)
    //        Glu.gluTessBeginContour(_tess)

    //        For i As Int32 = 0 To face.vertexCount - 1
    //            coords(0) = face.vertex(i).point.x
    //            coords(1) = face.vertex(i).point.y
    //            coords(2) = face.vertex(i).point.z

    //            ' allocate an array of unmanaged blocks of memory to hold the index of the current vertex
    //            vPtr(i) = System.Runtime.InteropServices.Marshal.AllocHGlobal(System.Runtime.InteropServices.Marshal.SizeOf(index))
    //            System.Runtime.InteropServices.Marshal.WriteInt32(vPtr(i), 0, face.vertex(i).index)

    //            'System.Runtime.InteropServices.Marshal.StructureToPtr(face.vertex(i), vPtr(i), False)

    //            'The first parameter, tess, is the handle to the tesselation object. The second parameter, coords, 
    //            'is a pointer to an array of three doubles specifying the vertex coordinates. Note that you can pass 
    //            'a pointer to any custom vertex structure, as long as the first entries contain the x, y, and z coordinates
    //            ' of the vertex as double-precision floating-point values. (our SFVertex does not.  It uses singles)

    //            'The third parameter, data, is an opaque pointer that is passed to the vertex callback as is. 
    //            'While the coords parameter is used to pass the position of the vertex to the GLU tesselator, 
    //            'you can use the data parameter to identify the vertex after the triangulation took place. 
    //            'The accompanying sample application, for example, stores the vertex data in a vector of custom vertex structures. 
    //            'In addition to the vertex position, these structures store a normal vector and a pair of texture coordinates. 
    //            ' But the only requirement is that the ptr point to data that we can use to identify the vertex so that we can
    //            ' build new triangles from the tesselated face which uses 99.9% of the time pre-existing vertices of that face.
    //            ' Very rarely does it have to generate a new vertex to complete tessellation.
    //            Glu.gluTessVertex(_tess, coords, vPtr(i))
    //        Next

    //        Glu.gluTessEndContour(_tess)
    //        Glu.gluTessEndPolygon(_tess)

    //        tempVertices = generateTriangleFaces(returnTriangleLists)

    //        ' free unmanaged memory used by our array of pointers
    //        For i As Int32 = 0 To face.vertexCount - 1
    //            System.Runtime.InteropServices.Marshal.FreeHGlobal(vPtr(i))
    //        Next

    //        Return tempVertices
    //    Else
    //        Return Nothing
    //    End If
    //End Function

    //Private Function generateTriangleFaces(ByVal returnTriangleLists As Boolean) As X3D.SFVertex()
    //    Dim vertexCount As Int32 = _vertexList.Count
    //    Dim tempVertices() As X3D.SFVertex

    //    ' depending on the type of tesselated primitive, create triangle lists from them
    //    Select Case _Mode
    //        Case Tessellator.GL_PRIMITIVE.GL_TRIANGLE_FAN

    //            Dim j As Int32 = 0
    //            ReDim tempVertices(0 To vertexCount * 3 - 1)
    //            For i As Int32 = 1 To vertexCount - 1
    //                tempVertices(j) = _vertexList(0) : j += 1
    //                tempVertices(j) = _vertexList(i) : j += 1
    //                If i = 1 Then
    //                    tempVertices(j) = _vertexList(vertexCount - 1)
    //                Else
    //                    tempVertices(j) = _vertexList(i - 1)
    //                End If
    //                j += 1
    //            Next
    //            ' Debug.Print("VisitorInit:CreateTVMesh() -- Tesselating a fan into a list of triangles.")

    //        Case Tessellator.GL_PRIMITIVE.GL_TRIANGLE_STRIP
    //            ReDim tempVertices(0 To vertexCount * 3 - 2)
    //            Dim j As Int32 = 0

    //            tempVertices(j) = _vertexList(0) : j += 1 'cw = 2
    //            tempVertices(j) = _vertexList(1) : j += 1 'cw = 1
    //            tempVertices(j) = _vertexList(2) : j += 1 'cw = 0

    //            For i As Int32 = 3 To vertexCount - 1
    //                tempVertices(j) = _vertexList(i - 1) : j += 1 'cw = i
    //                tempVertices(j) = _vertexList(i - 2) : j += 1 'cw = i - 2
    //                tempVertices(j) = _vertexList(i) : j += 1  'cw = i -1 
    //            Next

    //            'Debug.Print("VisitorInit:CreateTVMesh() -- Tesselating a triangle strip into a list of triangles.")

    //        Case Tessellator.GL_PRIMITIVE.GL_TRIANGLES
    //            Dim j As Int32 = 0
    //            Debug.Assert(vertexCount Mod 3 = 0, "VisitorInit.CreateTVMesh() - Tesselettor.GL_TRIANGLE primitive returning non power of 3 vertex count.  Count = " & CStr(vertexCount))

    //            'Debug.Print("VisitorInit:CreateTVMesh() -- Tesselator returning a list of triangles.")

    //            'ReDim tempfaces(0 To vertexCount \ 3 - 1)
    //            'For i As Int32 = 0 To vertexCount - 1 Step 3
    //            '    tempfaces(j).addVertex(_vertexList(i))
    //            '    tempfaces(j).addVertex(_vertexList(i + 1))
    //            '    tempfaces(j).addVertex(_vertexList(i + 2))
    //            '    j += 1
    //            'Next
    //            tempVertices = _vertexList.ToArray
    //        Case Else
    //            Debug.Assert(False, "Tessellator:generateTriangleFaces() -- Unsupported GL Primitive.")
    //            Return Nothing
    //    End Select

    //    Return tempVertices
    //End Function

    ///
    /// --- Callbacks ---
    ///
    private void Begin(int which)
    {
        // this tells us what type of primitive is returned back

        _IsReady = false;
        _Mode = (GL_PRIMITIVE)which; 
        switch (_Mode)
        {
            case GL_PRIMITIVE.GL_TRIANGLE_FAN:
                //Debug.WriteLine("WHICH = TRIANLGE_FAN")
                break;
            case GL_PRIMITIVE.GL_TRIANGLE_STRIP:
                //Debug.WriteLine("WHICH = TRIANLGE_STRIP")
                break;
            case GL_PRIMITIVE.GL_TRIANGLES:
                //Debug.WriteLine("WHICH = TRIANLGE_LIST")
                break;
            default:
                //Debug.Write("WHICH TYPE = UNSUPPORTED!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!")
                //MsgBox("Tessellator:Begin() -- primitive type not supported.")
                break;
        }
    }


    /// <summary>
    ///     <para>
    ///         The Combine callback is used to create a new vertex when edges intersect.
    ///         coordinate location is trivial to calculate, but weight[4] may be used to
    ///         average color, normal, or texture coordinate data.  In this program, color
    ///         is weighted.
    ///     </para>
    /// </summary>
    unsafe private void Combine(double[] coords, double[] vertexData, float[] weight, double[] dataOut)
    {
        double[] vertex = new double [6];
        vertex[0] = coords[0];
        vertex[1] = coords[1];
        vertex[2] = coords[2];
        System.Diagnostics.Trace.WriteLine("Combine callback");
        for (int i = 3; i < 6; i++)
        {
           // vertex[i] = weight[0] * vertexData[i] + weight[1] * vertexData[i] + weight[2] * vertexData[i] + weight[3] * vertexData[i];
        }

        TessVertexData newVertex = new TessVertexData (GetNewIndex(), coords[0], coords[1], coords[2]);
        dataOut = coords; // newVertex.Index; // a pointer to another 
    }
    // this seems critical.  In fact, it wasnt a problem with the pointers not de-allocating (which didnt really make sense to me anyway)
    // but rather, its this edge callback that needs handling.  Seems the Edge callback occurs whenever an entire "face" has completed.
    private void Edge(int flag)
    {
        Trace.Write("Edge callback");
        if (flag == Glu.GLU_TRUE)
        {

        }
        else
        {
        }
    }
    
    private void End()
    {
        _IsReady = true;
        synchOperation.Set();
    }
    
    private void Error( int errorCode)
    {
        Trace.WriteLine("Tessellator:Error() -- Tessellation Error: {0}", Glu.gluErrorString(errorCode));
    }


    unsafe private void Vertex (IntPtr pIndex)
    {
        unsafe
        {
            double[] vertex = new double[3];
            TessVertexData v = FindVertex(Marshal.ReadInt32 (pIndex));

            if (_outPut == null) _outPut = new List<int>();
            _outPut.Add(*v.Index);
        }
    }

    unsafe private TessVertexData FindVertex (int index)
    {
        for (int i = 0; i < _inPut.Length; i++)
            if (*_inPut[i].Index == index) return _inPut[i];

        return null;
    }

    //Public Sub Vertex(ByVal vPtr As System.IntPtr)
    //    'This callback is invoked any number of times between calls to the GLU_TESS_BEGIN and GLU_TESS_END callbacks. 
    //    'For a triangle list, for example, the GLU_TESS_VERTEX callback is called three times for each triangle.
    //    Dim v As New X3D.SFVertex
    //    Dim index As Int32

    //    index = System.Runtime.InteropServices.Marshal.ReadInt32(vPtr, 0) ' (vPtr, GetType(X3D.SFVertex)), X3D.SFVertex)
    //    v.index = index
    //    ' TODO: right here the vertex is returned and the question is, if its a new vertex as a result
    //    ' of the Combine callback, then we need a way to add it

    //    _vertexList.Add(v)


    //    'Gl.glColor3dv(pointer)
    //    'Gl.glColor3f(1.0f, 1.0f, 1.0f)
    //    'Gl.glVertex3dv(vertex)
    //    'Debug.WriteLine("Vertex callback received")
    //End Sub


}
    
