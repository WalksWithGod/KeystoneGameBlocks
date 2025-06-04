
''' <summary>
''' Recurses the quad tree and assigns neighbors.  This allows for easy navigation from one leaf node to any of its 8 neighbors.
''' </summary>
''' <remarks></remarks>
Public Class QTreeInit : Implements IQTreeTraverser

    Private _nodesTraversed As Int32
    Private _allNodes As Generic.List(Of QTreeNode)
    Private _rootbounds As RectangleF

    Public Sub New(ByRef allNodes As Generic.List(Of QTreeNode))
        If allNodes.Count = 0 Then Throw New ArgumentNullException
        _allNodes = allNodes
        _rootbounds = allNodes(0).Bounds
    End Sub

    Public ReadOnly Property NodesTraversed() As Integer Implements IQTreeTraverser.NodesTraversed
        Get
            Return _nodesTraversed
        End Get
    End Property

    Public Sub Apply(ByRef node As QTreeNodeBranch) Implements IQTreeTraverser.Apply
        _nodesTraversed += 1
        For Each child As QTreeNode In node.Children
            child.traverse(Me)
        Next
    End Sub

    Public Sub Apply(ByRef node As QTreeNodeLeaf) Implements IQTreeTraverser.Apply
        ' two neigbhors are derived from the node's current depth, the
        ' type of boundary, and its ancestorial distance (how many
        ' nodes up til it shares the same parent with the neighbor in question)
        Dim depth As Int32 = node.Depth
        Dim neighborIndex As Int32 = -1

        _nodesTraversed += 1
        Try
            'TODO: i think we can get rid of the second stage traverser by assigning these two sets of found
            ' nodes, to the proper neighbor positions for our direct siblings.  E.G the NW_CHILD's
            ' east sibling is a direct sibling.  That which is the North sibling to the NW_CHILD 
            ' is also the NW sibling to our East sibling.  (and in turn, this found nodes NE neibhor is the East sibling)
            ' NOTE: it helps to draw this out on graph paper to visualize.
            Select Case node.Quadrant

                Case QT_NODE_CHILD.NW_CHILD
                    ' north 
                    depth = CommonAncestorDistance(node, QT_NODE_NEIGHBOR.NORTH)
                    neighborIndex = getNeighborIndex(node, QT_NODE_NEIGHBOR.NORTH, depth)
                    node.Neighbors(QT_NODE_NEIGHBOR.NORTH) = DirectCast(_allNodes(node.Index + neighborIndex), QTreeNodeLeaf)
                    node.Neighbors(QT_NODE_NEIGHBOR.NORTH).Neighbors(QT_NODE_NEIGHBOR.SOUTH) = node

                    ' west
                    depth = CommonAncestorDistance(node, QT_NODE_NEIGHBOR.WEST)
                    neighborIndex = getNeighborIndex(node, QT_NODE_NEIGHBOR.WEST, depth)
                    node.Neighbors(QT_NODE_NEIGHBOR.WEST) = DirectCast(_allNodes(node.Index + neighborIndex), QTreeNodeLeaf)
                    node.Neighbors(QT_NODE_NEIGHBOR.WEST).Neighbors(QT_NODE_NEIGHBOR.EAST) = node


                Case QT_NODE_CHILD.SE_CHILD

                    ' South
                    depth = CommonAncestorDistance(node, QT_NODE_NEIGHBOR.SOUTH)
                    neighborIndex = getNeighborIndex(node, QT_NODE_NEIGHBOR.SOUTH, depth)
                    node.Neighbors(QT_NODE_NEIGHBOR.SOUTH) = DirectCast(_allNodes(node.Index + neighborIndex), QTreeNodeLeaf)
                    node.Neighbors(QT_NODE_NEIGHBOR.SOUTH).Neighbors(QT_NODE_NEIGHBOR.NORTH) = node

                    ' East
                    depth = CommonAncestorDistance(node, QT_NODE_NEIGHBOR.EAST)
                    neighborIndex = getNeighborIndex(node, QT_NODE_NEIGHBOR.EAST, depth)
                    node.Neighbors(QT_NODE_NEIGHBOR.EAST) = DirectCast(_allNodes(neighborIndex + node.Index), QTreeNodeLeaf)
                    node.Neighbors(QT_NODE_NEIGHBOR.EAST).Neighbors(QT_NODE_NEIGHBOR.WEST) = node

            End Select
        Catch ex As Exception
            Debug.Print(ex.Message)
        End Try
    End Sub

    ''' <summary>
    '''  Determine how many jumps up the tree until a parent's border no longer matches 
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>Not pretty, but effective.</remarks>
    Private Shared Function CommonAncestorDistance(ByRef node As QTreeNodeLeaf, ByVal neighbor As QT_NODE_NEIGHBOR) As Int32
        ' depending on the node's quadrant position as a child to its parent we recurse up the parents until the parent/*grandparents bounds 
        'is > than that boundary and that means we've found the common parent.

        ' knowing the common parent, we can find the index for that neighbor using a home brewed formula
        ' (there's probably some logarithm that can do this but for now this works fine too)
        Dim distance As Int32 = 0
        Dim parent As QTreeNode = node.Parent

        Select Case node.Quadrant
            Case QT_NODE_CHILD.NW_CHILD
                ' neighbor should only be north or west for a NW child
                If neighbor = QT_NODE_NEIGHBOR.NORTH Then
                    Do
                        distance += 1
                        If parent Is Nothing Then Exit Do
                        If node.Bounds.Top <> parent.Bounds.Top Then
                            Exit Do
                        Else
                            parent = parent.Parent
                        End If
                    Loop
                ElseIf neighbor = QT_NODE_NEIGHBOR.WEST Then
                    Do
                        distance += 1
                        If parent Is Nothing Then Exit Do
                        If node.Bounds.Left <> parent.Bounds.Left Then
                            Exit Do
                        Else
                            parent = parent.Parent
                        End If
                    Loop
                Else
                    Throw New Exception
                End If

            Case QT_NODE_CHILD.SE_CHILD
                ' neighbor should only be south or east for a SE child
                If neighbor = QT_NODE_NEIGHBOR.SOUTH Then
                    Do
                        distance += 1
                        If parent Is Nothing Then Exit Do
                        If node.Bounds.Bottom <> parent.Bounds.Bottom Then
                            Exit Do
                        Else
                            parent = parent.Parent
                        End If
                    Loop

                ElseIf neighbor = QT_NODE_NEIGHBOR.EAST Then
                    Do
                        distance += 1
                        If parent Is Nothing Then Exit Do
                        If node.Bounds.Right <> parent.Bounds.Right Then
                            Exit Do
                        Else
                            parent = parent.Parent
                        End If
                    Loop
                Else
                    Throw New Exception
                End If
        End Select

        Return distance
    End Function

    Private Function getNeighborIndex(ByRef node As QTreeNodeLeaf, ByVal neighborQuadrant As QT_NODE_NEIGHBOR, ByVal depth As Int32) As Int32
        ' only question is to how to  know which boundary we are against at a given depth
        ' and thus, know how many times to recurse to find the correct neighbor index across that boundary
        Dim westeast, northsouth As Int32
        westeast = 1
        northsouth = 2

        Select Case node.Quadrant

            Case QT_NODE_CHILD.NW_CHILD
                ' should only have north and west for this type
                If neighborQuadrant = QT_NODE_NEIGHBOR.WEST Then
                    If node.Bounds.Left = _rootbounds.Left Then
                        For i As Int32 = 1 To depth - 2
                            westeast = westeast * 4 + 1
                        Next
                        Return westeast
                    Else
                        For i As Int32 = 1 To depth - 1
                            westeast = westeast * 4 - 1
                        Next
                        Return -westeast
                    End If
                ElseIf neighborQuadrant = QT_NODE_NEIGHBOR.NORTH Then
                    If node.Bounds.Top = _rootbounds.Top Then
                        For i As Int32 = 1 To depth - 2
                            northsouth = northsouth * 4 + 2
                        Next
                        Return northsouth
                    Else
                        For i As Int32 = 1 To depth - 1
                            northsouth = northsouth * 4 - 2
                        Next
                        Return -northsouth
                    End If
                Else
                    Throw New Exception
                End If

            Case QT_NODE_CHILD.SE_CHILD
                ' should only have north and west for this type
                If neighborQuadrant = QT_NODE_NEIGHBOR.EAST Then
                    If node.Bounds.Right = _rootbounds.Right Then
                        For i As Int32 = 1 To depth - 2
                            westeast = westeast * 4 + 1
                        Next
                        Return -westeast
                    Else
                        For i As Int32 = 1 To depth - 1
                            westeast = westeast * 4 - 1
                        Next
                        Return westeast
                    End If
                ElseIf neighborQuadrant = QT_NODE_NEIGHBOR.SOUTH Then
                    If node.Bounds.Bottom = _rootbounds.Bottom Then
                        For i As Int32 = 1 To depth - 2
                            northsouth = northsouth * 4 + 2
                        Next
                        Return -northsouth
                    Else
                        For i As Int32 = 1 To depth - 1
                            northsouth = northsouth * 4 - 2
                        Next
                        Return northsouth
                    End If
                Else
                    Throw New Exception
                End If
        End Select
    End Function

   
End Class

''' <summary>
''' Recurses the quad tree and assigns neighbors.
''' </summary>
''' <remarks></remarks>
Public Class InitStageTwo : Implements IQTreeTraverser

    Private _nodesTraversed As Int32
    Private _allNodes As Generic.List(Of QTreeNode)
    Private _rootbounds As RectangleF

    Public Sub New(ByRef allNodes As Generic.List(Of QTreeNode))
        If allNodes.Count = 0 Then Throw New ArgumentNullException
        _allNodes = allNodes
        _rootbounds = allNodes(0).Bounds
    End Sub

    Public ReadOnly Property NodesTraversed() As Integer Implements IQTreeTraverser.NodesTraversed
        Get
            Return _nodesTraversed
        End Get
    End Property

    Public Sub Apply(ByRef node As QTreeNodeBranch) Implements IQTreeTraverser.Apply
        _nodesTraversed += 1
        For Each child As QTreeNode In node.Children
            child.traverse(Me)
        Next
    End Sub

    Public Sub Apply(ByRef node As QTreeNodeLeaf) Implements IQTreeTraverser.Apply
        _nodesTraversed += 1
        Select Case node.Quadrant

            Case QT_NODE_CHILD.NW_CHILD
                ' north east 
                node.Neighbors(QT_NODE_NEIGHBOR.NE) = node.Neighbors(QT_NODE_NEIGHBOR.NORTH).Neighbors(QT_NODE_NEIGHBOR.EAST)
                node.Neighbors(QT_NODE_NEIGHBOR.NE).Neighbors(QT_NODE_NEIGHBOR.SW) = node

                ' north west 
                node.Neighbors(QT_NODE_NEIGHBOR.NW) = node.Neighbors(QT_NODE_NEIGHBOR.NORTH).Neighbors(QT_NODE_NEIGHBOR.WEST)
                node.Neighbors(QT_NODE_NEIGHBOR.NW).Neighbors(QT_NODE_NEIGHBOR.SE) = node

                ' south west 
                node.Neighbors(QT_NODE_NEIGHBOR.SW) = node.Neighbors(QT_NODE_NEIGHBOR.SOUTH).Neighbors(QT_NODE_NEIGHBOR.WEST)
                node.Neighbors(QT_NODE_NEIGHBOR.SW).Neighbors(QT_NODE_NEIGHBOR.NE) = node

            Case QT_NODE_CHILD.SW_CHILD
                node.Neighbors(QT_NODE_NEIGHBOR.NW) = node.Neighbors(QT_NODE_NEIGHBOR.WEST).Neighbors(QT_NODE_NEIGHBOR.NORTH)
                node.Neighbors(QT_NODE_NEIGHBOR.NW).Neighbors(QT_NODE_NEIGHBOR.SE) = node

                node.Neighbors(QT_NODE_NEIGHBOR.SE) = node.Neighbors(QT_NODE_NEIGHBOR.SOUTH).Neighbors(QT_NODE_NEIGHBOR.EAST)
                node.Neighbors(QT_NODE_NEIGHBOR.SE).Neighbors(QT_NODE_NEIGHBOR.NW) = node

                node.Neighbors(QT_NODE_NEIGHBOR.SW) = node.Neighbors(QT_NODE_NEIGHBOR.WEST).Neighbors(QT_NODE_NEIGHBOR.SOUTH)
                node.Neighbors(QT_NODE_NEIGHBOR.SW).Neighbors(QT_NODE_NEIGHBOR.NE) = node

            Case QT_NODE_CHILD.SE_CHILD
                ' southWest 
                node.Neighbors(QT_NODE_NEIGHBOR.SW) = node.Neighbors(QT_NODE_NEIGHBOR.SOUTH).Neighbors(QT_NODE_NEIGHBOR.WEST)
                node.Neighbors(QT_NODE_NEIGHBOR.SW).Neighbors(QT_NODE_NEIGHBOR.NE) = node

                'soutEast
                node.Neighbors(QT_NODE_NEIGHBOR.SE) = node.Neighbors(QT_NODE_NEIGHBOR.EAST).Neighbors(QT_NODE_NEIGHBOR.SOUTH)
                node.Neighbors(QT_NODE_NEIGHBOR.SE).Neighbors(QT_NODE_NEIGHBOR.NW) = node

                'northEast 
                node.Neighbors(QT_NODE_NEIGHBOR.NE) = node.Neighbors(QT_NODE_NEIGHBOR.NORTH).Neighbors(QT_NODE_NEIGHBOR.EAST)
                node.Neighbors(QT_NODE_NEIGHBOR.NE).Neighbors(QT_NODE_NEIGHBOR.SW) = node
        End Select
    End Sub

  
End Class


