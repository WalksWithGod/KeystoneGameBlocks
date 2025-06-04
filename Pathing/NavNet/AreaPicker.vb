Public Class AreaPicker
    Private _pick As PointF
    Delegate Sub AreaPicked(ByRef node As Area)
    Private _areaPickedCallback As AreaPicked

    Public Sub New(ByVal pickedCallback As AreaPicked)
        If pickedCallback Is Nothing Then Throw New ArgumentNullException
        _areaPickedCallback = pickedCallback
    End Sub

    '    ' Here the server simply must use a quadtree as the main structure holding the areas or else
    '    ' linear searches are not practical.  Each page boundary in the world will be a leaf node (just as 
    '    ' how it is currently with our terrain pages) and during area generation from the cells, only cells
    '    ' within the same page can be merged into areas.  So each qtree leaf would contain a list of all
    '    ' areas in that page.  This would drastically reduce search times.  But ultimatley to fnd a area
    '    ' a linear search within the page must be done.  Hopefully "picks" are relatively are... certainly not
    '    ' something you'd perform every loop for every player.
    '    Public WriteOnly Property Pick() As PointF
    '        Set(ByVal value As PointF)
    '            _pick = value
    '        End Set
    '    End Property
    '    Public Function Apply(ByVal node As qtreenodeBranch) As Area
    '        If node.Bounds.Contains(_pick) Then
    '            For Each child As QTreeNode In node.Children
    '                child.traverse(Me)
    '            Next
    '        End If
    '    End Function

    '    Public Sub Apply(ByRef node As QTreeNodeLeaf) Implements IQTreeTraverser.Apply
    '        For Each item As Area In node.Areas
    '            If item.ContainsPoint(_pick) Then
    '                _areaPickedCallback.Invoke(item)
    '            End If
    '        Next
    '    End Sub

    Public Shared Function FindArea(ByVal areas() As Area, ByVal position As MTV3D65.TV_3DVECTOR) As Area
        Dim found As Area = Nothing

        For i As Int32 = 0 To areas.Length - 1
            If areas(i).ContainsPoint(position) Then
                found = areas(i)
                Exit For
            End If
        Next
        Return found
    End Function
End Class


