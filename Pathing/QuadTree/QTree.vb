Public Enum QT_NODE_NEIGHBOR
    EAST    ' siblings
    WEST
    SOUTH
    NORTH
    NE
    NW
    SE
    SW
End Enum

Public Enum QT_NODE_CHILD As Integer
    NE_CHILD = 0 ' children
    NW_CHILD = 1
    SE_CHILD = 2
    SW_CHILD = 3
End Enum

''' <summary>
''' Spatial Linear Quadtree that also maintains references to its neigbors
''' in other quadrants.  This consumes more memory than a typical quadtree
''' implementation, but we gain speed when performing range limited nearby searches.
''' NOTE: We save 3 operations for each sibling query that is located in another
''' quadrant, and 2 operations for each sibling in the same quadrant.  So
''' when we potentially need to do these searches for thousands of entities each "tick"
''' its a huge saving and results in less recursive overhead.
''' </summary>
''' <remarks>Thread safety shouldn't be a problem since updates aren't allowed after initialization.
''' In other words, for all intents and purposes this is a read only tree.</remarks>
Public MustInherit Class QTreeNode : Implements ITraversable

    Protected _depth As Int32
    Protected _bounds As RectangleF
    Protected _boundingBox As BoundingBox
    Private _boundingVolumeIsDirty As Boolean = True
    Protected _isRoot As Boolean = False

    'Protected _center As PointF
    Protected _index As Int32
    Protected _quadrant As QT_NODE_CHILD
    Protected _parent As QTreeNode

    Public MustOverride Sub traverse(ByRef traverser As IQTreeTraverser) Implements ITraversable.traverse


    Public ReadOnly Property Parent() As QTreeNode
        Get
            Return _parent
        End Get
    End Property

    Public ReadOnly Property Bounds() As RectangleF
        Get
            Return _bounds
        End Get
    End Property

   
    ' Having to constantly update bounding volumes can get expensive
    ' so we want to limit it to changes in static mesh (terrain, buildings, etc)
    ' TVActor's and moving items should override the ComputeBoundingBox
    ' and do nothing since their movement and changes have no impact
    ' on what we're mostly interested which is entire Page culling.
    Public Property BoundingVolumeIsDirty() As Boolean
        Get
            Return _boundingVolumeIsDirty
        End Get
        Set(ByVal value As Boolean)
            _boundingVolumeIsDirty = value
            If value Then
                'If Not _isRoot Then
                If Parent IsNot Nothing Then
                    Parent.BoundingVolumeIsDirty = True
                End If
                'End If
            End If
        End Set
    End Property

    Public ReadOnly Property BBox() As BoundingBox
        Get
            If _boundingVolumeIsDirty Then ComputeBoundingBox()
            Return _boundingBox
        End Get
    End Property

    Public MustOverride Sub ComputeBoundingBox()
    

    Public ReadOnly Property Center() As PointF
        Get
            Return New PointF(_bounds.X + _bounds.Width / 2, _bounds.Y + _bounds.Height / 2)
        End Get
    End Property
    Public Property Index() As Int32
        Get
            Return _index
        End Get
        Set(ByVal value As Int32)
            _index = value
        End Set
    End Property
    Public ReadOnly Property Depth() As Int32
        Get
            Return _depth
        End Get
    End Property

    Public Property Quadrant() As QT_NODE_CHILD
        Get
            Return _quadrant
        End Get
        Set(ByVal value As QT_NODE_CHILD)
            _quadrant = value
        End Set
    End Property

    ''' <summary>
    ''' BuildQuadTree is a routine to divide a quadtree root node into more nodes
    ''' It uses a max depth contstant to determine when to stop looping. Note that
    ''' we do not use recursion since i'm not a math genius and the easiest method
    ''' I could come up with to derive cross-quadrant neighbor index values is used.
    ''' Using recursion would result in index values that I couldnt formulate a routine
    ''' to derive neighbor relationships for.
    ''' </summary>
    ''' <param name="root"></param>
    ''' <remarks>The order in which we index the children is important to the routine which later
    ''' must establish the proper neighbor references for each node</remarks>
    Public Shared Function BuildQuadtree(ByRef root As QTreeNodeBranch, ByVal treeDepth As Int32) As Int32
        Dim index As Int32 = 1 ' our root node is index 0 so our first new node starts at 1
        Dim allNodes As New Generic.List(Of QTreeNode)
        root._isRoot = True
        allNodes.Add(root)
        Dim currentParent As QTreeNodeBranch = root
        Dim currentQuadrant As Int32 = 0

        Do While currentParent.Depth < treeDepth

            ' Create the children each with a quarter of the bounds of their parent.  
            ' The parent node keeps reference to its children, and our static node list does
            ' as well so that we can build the neighbor relationships in the second pass
            allNodes.Add(New WorldNode(CreateBound(currentParent.Bounds, QT_NODE_CHILD.NW_CHILD), DirectCast(currentParent, QTreeNodeBranch)))
            allNodes(index).Index = index
            allNodes(index).Quadrant = QT_NODE_CHILD.NW_CHILD
            currentParent.Children(QT_NODE_CHILD.NW_CHILD) = allNodes(index) : index += 1

            allNodes.Add(New WorldNode(CreateBound(currentParent.Bounds, QT_NODE_CHILD.NE_CHILD), DirectCast(currentParent, QTreeNodeBranch)))
            allNodes(index).Index = index
            allNodes(index).Quadrant = QT_NODE_CHILD.NE_CHILD
            currentParent.Children(QT_NODE_CHILD.NE_CHILD) = allNodes(index) : index += 1

            allNodes.Add(New WorldNode(CreateBound(currentParent.Bounds, QT_NODE_CHILD.SW_CHILD), DirectCast(currentParent, QTreeNodeBranch)))
            allNodes(index).Index = index
            allNodes(index).Quadrant = QT_NODE_CHILD.SW_CHILD
            currentParent.Children(QT_NODE_CHILD.SW_CHILD) = allNodes(index) : index += 1

            allNodes.Add(New WorldNode(CreateBound(currentParent.Bounds, QT_NODE_CHILD.SE_CHILD), DirectCast(currentParent, QTreeNodeBranch)))
            allNodes(index).Index = index
            allNodes(index).Quadrant = QT_NODE_CHILD.SE_CHILD
            currentParent.Children(QT_NODE_CHILD.SE_CHILD) = allNodes(index) : index += 1

            currentQuadrant += 1
            currentParent = DirectCast(allNodes(currentQuadrant), QTreeNodeBranch)
        Loop

        ' bit of a hack, but we want leafs instanced as such since our traversals can
        ' overload the apply() for the different node types.
        Dim maxLeaf As Int32 = index - 1
        ' now for the leaf nodes
        For i As Int32 = currentQuadrant To maxLeaf
            currentParent = DirectCast(allNodes(i), QTreeNodeBranch)

            Dim nwLeaf As New WorldArea(CreateBound(currentParent.Bounds, QT_NODE_CHILD.NW_CHILD), DirectCast(currentParent, QTreeNodeBranch))

            allNodes.Add(nwLeaf)
            allNodes(index).Index = index
            allNodes(index).Quadrant = QT_NODE_CHILD.NW_CHILD
            currentParent.Children(QT_NODE_CHILD.NW_CHILD) = allNodes(index) : index += 1

            Dim neLeaf As New WorldArea(CreateBound(currentParent.Bounds, QT_NODE_CHILD.NE_CHILD), DirectCast(currentParent, QTreeNodeBranch))
            allNodes.Add(neLeaf)
            allNodes(index).Index = index
            allNodes(index).Quadrant = QT_NODE_CHILD.NE_CHILD
            currentParent.Children(QT_NODE_CHILD.NE_CHILD) = allNodes(index) : index += 1

            Dim swLeaf As New WorldArea(CreateBound(currentParent.Bounds, QT_NODE_CHILD.SW_CHILD), DirectCast(currentParent, QTreeNodeBranch))
            allNodes.Add(swLeaf)
            allNodes(index).Index = index
            allNodes(index).Quadrant = QT_NODE_CHILD.SW_CHILD
            currentParent.Children(QT_NODE_CHILD.SW_CHILD) = allNodes(index) : index += 1

            Dim seLeaf As New WorldArea(CreateBound(currentParent.Bounds, QT_NODE_CHILD.SE_CHILD), DirectCast(currentParent, QTreeNodeBranch))
            allNodes.Add(seLeaf)
            allNodes(index).Index = index
            allNodes(index).Quadrant = QT_NODE_CHILD.SE_CHILD
            currentParent.Children(QT_NODE_CHILD.SE_CHILD) = allNodes(index) : index += 1

            ' hackish, but lets just assign these siblings while we can
            nwLeaf.Neighbors(QT_NODE_NEIGHBOR.EAST) = neLeaf
            nwLeaf.Neighbors(QT_NODE_NEIGHBOR.SE) = seLeaf
            nwLeaf.Neighbors(QT_NODE_NEIGHBOR.SOUTH) = swLeaf

            neLeaf.Neighbors(QT_NODE_NEIGHBOR.WEST) = nwLeaf
            neLeaf.Neighbors(QT_NODE_NEIGHBOR.SW) = swLeaf
            neLeaf.Neighbors(QT_NODE_NEIGHBOR.SOUTH) = seLeaf

            swLeaf.Neighbors(QT_NODE_NEIGHBOR.EAST) = seLeaf
            swLeaf.Neighbors(QT_NODE_NEIGHBOR.NE) = neLeaf
            swLeaf.Neighbors(QT_NODE_NEIGHBOR.NORTH) = nwLeaf

            seLeaf.Neighbors(QT_NODE_NEIGHBOR.WEST) = swLeaf
            seLeaf.Neighbors(QT_NODE_NEIGHBOR.NW) = nwLeaf
            seLeaf.Neighbors(QT_NODE_NEIGHBOR.NORTH) = neLeaf
        Next

        ' After the initial quadtree is built, we now assign the neighbors in this second stage
        Dim Initializer As New QTreeInit(allNodes)
        root.traverse(DirectCast(Initializer, IQTreeTraverser))

        ' we use a second traversal to assign other neighbors... not pretty, but i think its faster than anything else
        Dim SecondStage As New InitStageTwo(allNodes)
        root.traverse(DirectCast(SecondStage, IQTreeTraverser))

        Return allNodes.Count
    End Function

   

    Private Shared Function CreateBound(ByVal parentBound As RectangleF, ByVal quadrant As QT_NODE_CHILD) As RectangleF
        ' Divide the children up.
        Dim height, width As Single
        Dim r As RectangleF

        height = parentBound.Height / 2
        width = parentBound.Width / 2

        Select Case quadrant
            Case QT_NODE_CHILD.NW_CHILD
                r = New RectangleF(parentBound.Left, parentBound.Top, width, height)
            Case QT_NODE_CHILD.NE_CHILD
                r = New RectangleF(parentBound.Left + width, parentBound.Top, width, height)

            Case QT_NODE_CHILD.SW_CHILD
                r = New RectangleF(parentBound.Left, parentBound.Top + height, width, height)

            Case QT_NODE_CHILD.SE_CHILD
                r = New RectangleF(parentBound.Left + width, parentBound.Top + height, width, height)
        End Select
        Return r
    End Function
End Class


Public MustInherit Class QTreeNodeLeaf : Inherits QTreeNode

    Private _neighbors(0 To 7) As QTreeNodeLeaf

    Public Sub New(ByVal bound As RectangleF, ByRef parent As QTreeNodeBranch)
        If Not parent Is Nothing Then
            _depth = parent.Depth + 1
            _parent = parent
            _isRoot = True
        Else
            _index = 0
        End If
        ' TODO: might want to test the validity of the bounds?
        _bounds = bound
    End Sub

    Public Overrides Sub traverse(ByRef traverser As IQTreeTraverser)
        traverser.Apply(Me)
    End Sub

    Public Function ContainsNeighbor(ByRef node As QTreeNode) As Boolean
        For i As Int32 = 0 To 7
            If _neighbors(i) Is node Then
                Debug.Assert(_neighbors(i) IsNot Nothing)
                Return True
            End If
        Next
    End Function
    Public Property Neighbors(ByVal index As Int32) As QTreeNodeLeaf
        Get
            Return _neighbors(index)
        End Get
        Set(ByVal value As QTreeNodeLeaf)
            _neighbors(index) = value
        End Set
    End Property
End Class


Public MustInherit Class QTreeNodeBranch : Inherits QTreeNodeLeaf
    Private _children(0 To 3) As QTreeNode

    Public Sub New(ByVal bound As RectangleF, ByRef parent As QTreeNodeBranch)

        MyBase.New(bound, parent)
    End Sub

    Public Overrides Sub traverse(ByRef traverser As IQTreeTraverser)
        traverser.Apply(Me)
    End Sub

    Public ReadOnly Property Children() As QTreeNode()
        Get
            Return _children
        End Get
    End Property

End Class


