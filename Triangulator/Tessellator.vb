#Region "Credits"
' Translation to VB.NET by Hypnotron June 30th 2005
#End Region
#Region "License"

'BSD License
'Copyright ?2003-2004 Randy Ridge
'http://www.randyridge.com/Tao/Default.aspx
'All rights reserved.

'Redistribution and use in source and binary forms, with or without
'modification, are permitted provided that the following conditions
'are met:

'1. Redistributions of source code must retain the above copyright notice,
'   this list of conditions and the following disclaimer.

'2. Redistributions in binary form must reproduce the above copyright notice,
'   this list of conditions and the following disclaimer in the documentation
'   and/or other materials provided with the distribution.

'3. Neither Randy Ridge nor the names of any Tao contributors may be used to
'   endorse or promote products derived from this software without specific
'   prior written permission.

'   THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
'   "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
'   LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
'   FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE
'   COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
'   INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
'   BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
'   LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
'   CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
'   LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
'   ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
'   POSSIBILITY OF SUCH DAMAGE.

#End Region
#Region "Original Credits / License"

' Copyright (c) 1993-1997, Silicon Graphics, Inc.
' ALL RIGHTS RESERVED 
' Permission to use, copy, modify, and distribute this software for 
' any purpose and without fee is hereby granted, provided that the above
' copyright notice appear in all copies and that both the copyright notice
' and this permission notice appear in supporting documentation, and that 
' the name of Silicon Graphics, Inc. not be used in advertising
' or publicity pertaining to distribution of the software without specific,
' written prior permission. 
'
' THE MATERIAL EMBODIED ON THIS SOFTWARE IS PROVIDED TO YOU "AS-IS"
' AND WITHOUT WARRANTY OF ANY KIND, EXPRESS, IMPLIED OR OTHERWISE,
' INCLUDING WITHOUT LIMITATION, ANY WARRANTY OF MERCHANTABILITY OR
' FITNESS FOR A PARTICULAR PURPOSE.  IN NO EVENT SHALL SILICON
' GRAPHICS, INC.  BE LIABLE TO YOU OR ANYONE ELSE FOR ANY DIRECT,
' SPECIAL, INCIDENTAL, INDIRECT OR CONSEQUENTIAL DAMAGES OF ANY
' KIND, OR ANY DAMAGES WHATSOEVER, INCLUDING WITHOUT LIMITATION,
' LOSS OF PROFIT, LOSS OF USE, SAVINGS OR REVENUE, OR THE CLAIMS OF
' THIRD PARTIES, WHETHER OR NOT SILICON GRAPHICS, INC.  HAS BEEN
' ADVISED OF THE POSSIBILITY OF SUCH LOSS, HOWEVER CAUSED AND ON
' ANY THEORY OF LIABILITY, ARISING OUT OF OR IN CONNECTION WITH THE
' POSSESSION, USE OR PERFORMANCE OF THIS SOFTWARE.
' 
' US Government Users Restricted Rights 
' Use, duplication, or disclosure by the Government is subject to
' restrictions set forth in FAR 52.227.19(c)(2) or subparagraph
' (c)(1)(ii) of the Rights in Technical Data and Computer Software
' clause at DFARS 252.227-7013 and/or in similar or successor
' clauses in the FAR or the DOD or NASA FAR Supplement.
' Unpublished-- rights reserved under the copyright laws of the
' United States.  Contractor/manufacturer is Silicon Graphics,
' Inc., 2011 N.  Shoreline Blvd., Mountain View, CA 94039-7311.
'
' OpenGL(R) is a registered trademark of Silicon Graphics, Inc.

#End Region
Imports Tao.OpenGl

Public Class Tessellator : Implements IDisposable
    Public Enum GL_PRIMITIVE
        GL_POINTS = &H0
        GL_LINES = &H1
        GL_LINE_LOOP = &H2
        GL_LINE_STRIP = &H3
        GL_TRIANGLES = &H4
        GL_TRIANGLE_STRIP = &H5
        GL_TRIANGLE_FAN = &H6
        GL_QUADS = &H7
        GL_QUAD_STRIP = &H8
        GL_POLYGON = &H9
    End Enum

    Private _tess As Glu.GLUtesselator
    Private vPtr() As System.IntPtr
    Private _vertexList As New Generic.List(Of X3D.SFVertex)
    Private _IsReady As Boolean = False
    Private _Mode As GL_PRIMITIVE
    Private _isDisposed As Boolean = True


    Public Sub New()
        _tess = Glu.gluNewTess

        Glu.gluTessCallback(_tess, Glu.GLU_TESS_VERTEX, New Glu.TessVertexCallback(AddressOf Me.Vertex))
        Glu.gluTessCallback(_tess, Glu.GLU_TESS_BEGIN, New Glu.TessBeginCallback(AddressOf Me.Begin))
        Glu.gluTessCallback(_tess, Glu.GLU_TESS_END, New Glu.TessEndCallback(AddressOf Me.End))
        Glu.gluTessCallback(_tess, Glu.GLU_TESS_ERROR, New Glu.TessErrorCallback(AddressOf Me.Error))
        Glu.gluTessCallback(_tess, Glu.GLU_TESS_COMBINE, New Glu.TessCombineCallback1(AddressOf Me.Combine))
        Glu.gluTessCallback(_tess, Glu.GLU_TESS_EDGE_FLAG, New Glu.TessEdgeFlagCallback(AddressOf Me.Edge))
        _isDisposed = False
    End Sub

    Public Shadows Sub Finalize()
        If Not _isDisposed Then Dispose()
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Glu.gluDeleteTess(_tess)
        _isDisposed = True
    End Sub

    Public Function TessellateFace(ByRef face As X3D.SFFace, ByVal returnTriangleLists As Boolean) As X3D.SFVertex()
        Dim tempVertices() As X3D.SFVertex
        Debug.Assert(face.vertexCount >= 3, "Tessellator:TesselateFace() -- Fewer than three vertices specified.")
        If face.vertexCount = 3 Then
            ' NOTE: oIndexedFaceSet.ccw = TRUE  because the face indices are in counter-clockwise order
            '       So whenever ccw=TRUE (which is always for now but we need proper testing here)
            '       we need to reverse them since TV3D is a DirectX renderer and not OpengGL.
            ' swap to clockwise order. 
            ReDim tempVertices(2)
            For i As Int32 = 0 To 2
                tempVertices(i) = face.vertex(i)
            Next
            Return tempVertices

        ElseIf face.vertexCount = 4 Then
            '  ' we'll tesselate these ourselves since its a trivial case.  
            ReDim tempVertices(0 To 5)
            tempVertices(0) = face.vertex(0)  'cw=0  'ccw = 0
            tempVertices(1) = face.vertex(2)  'cw=3  'ccw = 2
            tempVertices(2) = face.vertex(3)  'cw=2  'ccw = 3

            tempVertices(3) = face.vertex(2) 'cw=2  'ccw = 0 
            tempVertices(4) = face.vertex(0) 'cw=1  'ccw = 1
            tempVertices(5) = face.vertex(1) 'cw=0  'ccw = 2
            Return tempVertices

            'Debug.Print("VisitorInit:CreateTVMesh() -- Tesselating a quad into two triangles.")
        ElseIf face.vertexCount > 4 Then
            Dim coords(2) As Double
            Dim index As Int32
            ReDim vPtr(0 To face.vertexCount - 1)
            _vertexList.Clear()

            Glu.gluTessBeginPolygon(_tess, IntPtr.Zero)
            Glu.gluTessBeginContour(_tess)

            For i As Int32 = 0 To face.vertexCount - 1
                coords(0) = face.vertex(i).point.x
                coords(1) = face.vertex(i).point.y
                coords(2) = face.vertex(i).point.z

                ' allocate an array of unmanaged blocks of memory to hold the index of the current vertex
                vPtr(i) = System.Runtime.InteropServices.Marshal.AllocHGlobal(System.Runtime.InteropServices.Marshal.SizeOf(index))
                System.Runtime.InteropServices.Marshal.WriteInt32(vPtr(i), 0, face.vertex(i).index)

                'System.Runtime.InteropServices.Marshal.StructureToPtr(face.vertex(i), vPtr(i), False)

                'The first parameter, tess, is the handle to the tesselation object. The second parameter, coords, 
                'is a pointer to an array of three doubles specifying the vertex coordinates. Note that you can pass 
                'a pointer to any custom vertex structure, as long as the first entries contain the x, y, and z coordinates
                ' of the vertex as double-precision floating-point values. (our SFVertex does not.  It uses singles)

                'The third parameter, data, is an opaque pointer that is passed to the vertex callback as is. 
                'While the coords parameter is used to pass the position of the vertex to the GLU tesselator, 
                'you can use the data parameter to identify the vertex after the triangulation took place. 
                'The accompanying sample application, for example, stores the vertex data in a vector of custom vertex structures. 
                'In addition to the vertex position, these structures store a normal vector and a pair of texture coordinates. 
                ' But the only requirement is that the ptr point to data that we can use to identify the vertex so that we can
                ' build new triangles from the tesselated face which uses 99.9% of the time pre-existing vertices of that face.
                ' Very rarely does it have to generate a new vertex to complete tessellation.
                Glu.gluTessVertex(_tess, coords, vPtr(i))
            Next

            Glu.gluTessEndContour(_tess)
            Glu.gluTessEndPolygon(_tess)

            tempVertices = generateTriangleFaces(returnTriangleLists)
            
            ' free unmanaged memory used by our array of pointers
            For i As Int32 = 0 To face.vertexCount - 1
                System.Runtime.InteropServices.Marshal.FreeHGlobal(vPtr(i))
            Next

            Return tempVertices
        Else
            Return Nothing
        End If
    End Function

    Private Function generateTriangleFaces(ByVal returnTriangleLists As Boolean) As X3D.SFVertex()
        Dim vertexCount As Int32 = _vertexList.Count
        Dim tempVertices() As X3D.SFVertex

        ' depending on the type of tesselated primitive, create triangle lists from them
        Select Case _Mode
            Case Tessellator.GL_PRIMITIVE.GL_TRIANGLE_FAN

                Dim j As Int32 = 0
                ReDim tempVertices(0 To vertexCount * 3 - 1)
                For i As Int32 = 1 To vertexCount - 1
                    tempVertices(j) = _vertexList(0) : j += 1
                    tempVertices(j) = _vertexList(i) : j += 1
                    If i = 1 Then
                        tempVertices(j) = _vertexList(vertexCount - 1)
                    Else
                        tempVertices(j) = _vertexList(i - 1)
                    End If
                    j += 1
                Next
                ' Debug.Print("VisitorInit:CreateTVMesh() -- Tesselating a fan into a list of triangles.")

            Case Tessellator.GL_PRIMITIVE.GL_TRIANGLE_STRIP
                ReDim tempVertices(0 To vertexCount * 3 - 2)
                Dim j As Int32 = 0

                tempVertices(j) = _vertexList(0) : j += 1 'cw = 2
                tempVertices(j) = _vertexList(1) : j += 1 'cw = 1
                tempVertices(j) = _vertexList(2) : j += 1 'cw = 0

                For i As Int32 = 3 To vertexCount - 1
                    tempVertices(j) = _vertexList(i - 1) : j += 1 'cw = i
                    tempVertices(j) = _vertexList(i - 2) : j += 1 'cw = i - 2
                    tempVertices(j) = _vertexList(i) : j += 1  'cw = i -1 
                Next

                'Debug.Print("VisitorInit:CreateTVMesh() -- Tesselating a triangle strip into a list of triangles.")

            Case Tessellator.GL_PRIMITIVE.GL_TRIANGLES
                Dim j As Int32 = 0
                Debug.Assert(vertexCount Mod 3 = 0, "VisitorInit.CreateTVMesh() - Tesselettor.GL_TRIANGLE primitive returning non power of 3 vertex count.  Count = " & CStr(vertexCount))

                'Debug.Print("VisitorInit:CreateTVMesh() -- Tesselator returning a list of triangles.")

                'ReDim tempfaces(0 To vertexCount \ 3 - 1)
                'For i As Int32 = 0 To vertexCount - 1 Step 3
                '    tempfaces(j).addVertex(_vertexList(i))
                '    tempfaces(j).addVertex(_vertexList(i + 1))
                '    tempfaces(j).addVertex(_vertexList(i + 2))
                '    j += 1
                'Next
                tempVertices = _vertexList.ToArray
            Case Else
                Debug.Assert(False, "Tessellator:generateTriangleFaces() -- Unsupported GL Primitive.")
                Return Nothing
        End Select

        Return tempVertices
    End Function

    '''
    ''' --- Callbacks ---
    '''
    Sub Begin(ByVal which As Int32)
        ' this tells us what type of primitive is returned back

        _IsReady = False
        _Mode = CType(which, GL_PRIMITIVE)
        Select Case _Mode
            Case GL_PRIMITIVE.GL_TRIANGLE_FAN
                'Debug.WriteLine("WHICH = TRIANLGE_FAN")

            Case GL_PRIMITIVE.GL_TRIANGLE_STRIP
                'Debug.WriteLine("WHICH = TRIANLGE_STRIP")

            Case GL_PRIMITIVE.GL_TRIANGLES
                'Debug.WriteLine("WHICH = TRIANLGE_LIST")

            Case Else
                Debug.Write("WHICH TYPE = UNSUPPORTED!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!")
                MsgBox("Tessellator:Begin() -- primitive type not supported.")
        End Select
    End Sub


    ''' <summary>
    '''     <para>
    '''         The Combine callback is used to create a new vertex when edges intersect.
    '''         coordinate location is trivial to calculate, but weight[4] may be used to
    '''         average color, normal, or texture coordinate data.  In this program, color
    '''         is weighted.
    '''     </para>
    ''' </summary>
    Private Sub Combine(ByVal coordinates() As Double, ByVal vertexData() As Double, ByVal weight() As Single, ByVal dataOut() As Double)
        Dim vertex(2) As Double
        Dim i As Int32

        'todo: i need to fix that such that new vertices that are created can be flagged and have it added to the vertexlookup 
        '      so it can be given an index which can be added to the SFVertex before its added to the _facesList 
        vertex(0) = coordinates(0)
        vertex(1) = coordinates(1)
        vertex(2) = coordinates(2)

        'For i = 3 To 5
        '    vertex(i) = weight(0) * vertexData(i) + weight(1) * vertexData(i) + weight(2) * vertexData(i) + weight(3) * vertexData(i)
        'Next
        Debug.Print("Tessellator:Combine() -- New vertex created.")
        System.Threading.Thread.Sleep(100)
        dataOut = vertex
    End Sub

    ' this seems critical.  In fact, it wasnt a problem with the pointers not de-allocating (which didnt really make sense to me anyway)
    ' but rather, its this edge callback that needs handling.
    Private Sub Edge(ByVal flag As Integer)
        ' Debug.Print("E D G E   C A L L B A C K")
        If flag = Glu.GLU_TRUE Then

        Else

        End If
    End Sub

    Private Sub [End]()
        _IsReady = True
    End Sub



    Private Sub [Error](ByVal errorCode As Int32)
        Debug.WriteLine("Tessellator:Error() -- Tessellation Error: {0}", Glu.gluErrorString(errorCode))
    End Sub



    Public Sub Vertex(ByVal vPtr As System.IntPtr)
        'This callback is invoked any number of times between calls to the GLU_TESS_BEGIN and GLU_TESS_END callbacks. 
        'For a triangle list, for example, the GLU_TESS_VERTEX callback is called three times for each triangle.
        Dim v As New X3D.SFVertex
        Dim index As Int32

        index = System.Runtime.InteropServices.Marshal.ReadInt32(vPtr, 0) ' (vPtr, GetType(X3D.SFVertex)), X3D.SFVertex)
        v.index = index
        ' todo: right here the vertex is returned and the question is, if its a new vertex as a result
        ' of the Combine callback, then we need a way to add it

        _vertexList.Add(v)


        'Gl.glColor3dv(pointer)
        'Gl.glColor3f(1.0f, 1.0f, 1.0f)
        'Gl.glVertex3dv(vertex)
        'Debug.WriteLine("Vertex callback received")
    End Sub


End Class
