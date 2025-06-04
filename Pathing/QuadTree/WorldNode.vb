''' <summary>
''' World Node is a quad tree group node.  Although there are
''' methods for retreiving all entities from all leaf nodes
''' under this node, those lists are not maintained here.
''' </summary>
''' <remarks></remarks>
Public Class WorldNode : Inherits QTreeNodeBranch


    Public Sub New(ByVal bound As RectangleF, ByRef parent As QTreeNodeBranch)
        MyBase.New(bound, parent)
    End Sub

    Public Overrides Sub ComputeBoundingBox()
        ' note: each class inheriting from group may need to update this differently.
        Dim initialized As Boolean = False
        For Each child As QTreeNode In Children
            _boundingBox.Max = BoundingBox.MaxVec(child.BBox.Max, _boundingBox.Max)

            ' minimum must be initialized to the first max, otherwise it will always be 0,0,0
            If Not initialized Then
                _boundingBox.Min = _boundingBox.Max
                initialized = True
            End If
            _boundingBox.Min = BoundingBox.MinVec(child.BBox.Min, _boundingBox.Min)
        Next
        BoundingVolumeIsDirty = False
    End Sub

End Class

''' <summary>
''' World Area is a leaf node. This means it has no children.
''' </summary>
''' <remarks></remarks>
Public Class WorldArea : Inherits QTreeNodeLeaf

    Private _page As Page

    Public Sub New(ByVal bound As RectangleF, ByRef parent As QTreeNodeBranch)
        MyBase.New(bound, parent)
    End Sub

    Public Property Page() As Page
        Get
            Return _page
        End Get
        Set(ByVal value As Page)
            _page = value
        End Set
    End Property

    Public Overrides Sub ComputeBoundingBox()
        If _page Is Nothing Then Exit Sub
        Try
            _boundingBox = _page.BBox
            BoundingVolumeIsDirty = False
            'End If
        Catch ex As NullReferenceException
            Debug.Print("Page reference not set")

        End Try
    End Sub
End Class
