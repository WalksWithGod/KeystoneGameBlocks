Public Structure BoundingBox
    Public Min As MTV3D65.TV_3DVECTOR
    Public Max As MTV3D65.TV_3DVECTOR

    Public Sub New(ByVal vMin As MTV3D65.TV_3DVECTOR, ByVal vMax As MTV3D65.TV_3DVECTOR)
        Min = vMin
        Max = vMax
    End Sub

    Public ReadOnly Property Height() As Single
        Get
            Return Max.y - Min.y
        End Get
    End Property
    Public ReadOnly Property Depth() As Single
        Get
            Return Max.z - Min.z
        End Get
    End Property
    Public ReadOnly Property Width() As Single
        Get
            Return Max.x - Min.x
        End Get
    End Property

    Public Sub Draw(ByVal color As Integer)
        Draw(BoundingBox.GetVertices(Me), color)
    End Sub
    Private Sub Draw(ByVal vArray() As MTV3D65.TV_3DVECTOR, ByVal color As Integer)
        ' the bottom 4 coords of the box form a square
        globals.Screen2D.Draw_Line3D(vArray(0).x, vArray(0).y, vArray(0).z, vArray(1).x, vArray(1).y, vArray(1).z, color)
        globals.Screen2D.Draw_Line3D(vArray(1).x, vArray(1).y, vArray(1).z, vArray(3).x, vArray(3).y, vArray(3).z, color)
        globals.Screen2D.Draw_Line3D(vArray(3).x, vArray(3).y, vArray(3).z, vArray(2).x, vArray(2).y, vArray(2).z, color)
        globals.Screen2D.Draw_Line3D(vArray(2).x, vArray(2).y, vArray(2).z, vArray(0).x, vArray(0).y, vArray(0).z, color)

        ' the top 4 coords of the box form a square
        globals.Screen2D.Draw_Line3D(vArray(4).x, vArray(4).y, vArray(4).z, vArray(5).x, vArray(5).y, vArray(5).z, color)
        globals.Screen2D.Draw_Line3D(vArray(5).x, vArray(5).y, vArray(5).z, vArray(7).x, vArray(7).y, vArray(7).z, color)
        globals.Screen2D.Draw_Line3D(vArray(7).x, vArray(7).y, vArray(7).z, vArray(6).x, vArray(6).y, vArray(6).z, color)
        globals.Screen2D.Draw_Line3D(vArray(6).x, vArray(6).y, vArray(6).z, vArray(4).x, vArray(4).y, vArray(4).z, color)

        ' lines to connect the top with the bottom
        globals.Screen2D.Draw_Line3D(vArray(0).x, vArray(0).y, vArray(0).z, vArray(4).x, vArray(4).y, vArray(4).z, color)
        globals.Screen2D.Draw_Line3D(vArray(1).x, vArray(1).y, vArray(1).z, vArray(5).x, vArray(5).y, vArray(5).z, color)
        globals.Screen2D.Draw_Line3D(vArray(2).x, vArray(2).y, vArray(2).z, vArray(6).x, vArray(6).y, vArray(6).z, color)
        globals.Screen2D.Draw_Line3D(vArray(3).x, vArray(3).y, vArray(3).z, vArray(7).x, vArray(7).y, vArray(7).z, color)
    End Sub

    Public Shared Function CombineBoundingBoxes(ByVal b1 As BoundingBox, ByVal b2 As BoundingBox) As BoundingBox
        Dim bmin, bmax As MTV3D65.TV_3DVECTOR
        bmin.x = Math.Min(b1.Min.x, b2.Min.x)
        bmin.y = Math.Min(b1.Min.y, b2.Min.y)
        bmin.z = Math.Min(b1.Min.z, b2.Min.z)

        bmax.x = Math.Max(b1.Max.x, b2.Max.x)
        bmax.y = Math.Max(b1.Max.y, b2.Max.y)
        bmax.z = Math.Max(b1.Max.z, b2.Max.z)
        Return New BoundingBox(bmin, bmax)
    End Function

    Public Shared Function GetVertices(ByVal box As BoundingBox) As MTV3D65.TV_3DVECTOR()
        Dim vertices(8) As MTV3D65.TV_3DVECTOR
        ' the top 4
        vertices(0) = New MTV3D65.TV_3DVECTOR(box.Min.x, box.Min.y, box.Min.z)
        vertices(1) = New MTV3D65.TV_3DVECTOR(box.Max.x, box.Min.y, box.Min.z)
        vertices(2) = New MTV3D65.TV_3DVECTOR(box.Min.x, box.Min.y, box.Max.z)
        vertices(3) = New MTV3D65.TV_3DVECTOR(box.Max.x, box.Min.y, box.Max.z)

        ' the bottom 4
        vertices(4) = New MTV3D65.TV_3DVECTOR(box.Min.x, box.Max.y, box.Min.z)
        vertices(5) = New MTV3D65.TV_3DVECTOR(box.Max.x, box.Max.y, box.Min.z)
        vertices(6) = New MTV3D65.TV_3DVECTOR(box.Min.x, box.Max.y, box.Max.z)
        vertices(7) = New MTV3D65.TV_3DVECTOR(box.Max.x, box.Max.y, box.Max.z)
        Return vertices
    End Function

    Public Shared Function MaxVec(ByVal v1 As MTV3D65.TV_3DVECTOR, ByVal v2 As MTV3D65.TV_3DVECTOR) As MTV3D65.TV_3DVECTOR
        Dim v As MTV3D65.TV_3DVECTOR
        v.x = Math.Max(v1.x, v2.x)
        v.y = Math.Max(v1.y, v2.y)
        v.z = Math.Max(v1.z, v2.z)
        Return v
    End Function

    Public Shared Function MinVec(ByVal v1 As MTV3D65.TV_3DVECTOR, ByVal v2 As MTV3D65.TV_3DVECTOR) As MTV3D65.TV_3DVECTOR
        Dim v As MTV3D65.TV_3DVECTOR
        v.x = Math.Min(v1.x, v2.x)
        v.y = Math.Min(v1.y, v2.y)
        v.z = Math.Min(v1.z, v2.z)
        Return v
    End Function

    Public Shared Function PointInBounds(ByVal box As BoundingBox, ByVal point As MTV3D65.TV_3DVECTOR) As Boolean
        Return (point.x >= box.Min.x AndAlso point.x <= box.Max.x AndAlso _
        point.y >= box.Min.y AndAlso point.y <= box.Max.y AndAlso _
        point.z >= box.Min.z AndAlso point.z <= box.Max.z)
    End Function

    Public ReadOnly Property Center() As MTV3D65.TV_3DVECTOR
        Get
            Return GetCenter(Me)
        End Get
    End Property
    Public Shared Function GetCenter(ByVal box As BoundingBox) As MTV3D65.TV_3DVECTOR
        Dim v As MTV3D65.TV_3DVECTOR
        v.x = box.Min.x + (box.Width / 2)
        v.y = box.Min.y + (box.Height / 2)
        v.z = box.Min.z + (box.Depth / 2)
        Return v
    End Function
End Structure