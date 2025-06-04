Public Class Page

    Private _bounds As BoundingBox
    Private _navnet As NavNet

    ' TODO: maybe instead of hte below, just publicly expose and reference the _cg 
    '      it will need added vars for stating if _cg.EdgesCreated, _cg.AreasCreated, _cg.PortalsCreated,
    Private _areas() As Area
    Private _links() As Link


    Public ReadOnly Property Areas() As Area()
        Get
            Return _areas
        End Get
    End Property

    Public Property BBox() As BoundingBox
        Get
            Return _bounds
        End Get
        Set(ByVal value As BoundingBox)
            _bounds = value
        End Set
    End Property

    Public Function FindEdge(ByVal areaID As Int32, ByVal direction As CardinalDirection) As Edge
        If _areas IsNot Nothing Then
            For i As Int32 = 0 To _areas.Length - 1
                If _areas(i).ID = areaID Then


                End If
            Next
        End If
        Return Nothing
    End Function
End Class
