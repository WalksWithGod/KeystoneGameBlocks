

' Area is a volume that is connected to other areas via links.
' A link has only one destination (one way).  In this sense, if a user traverse one
' link and arrives in a new area and tries to traverse the link there, that
' link may not necessarily connect back to where they came since they are
' two different links.
Public Class Area : Implements IBinaryWriter

    Private _links As New Generic.List(Of Link)
    Private _bounds As BoundingBox
    Private _root As Cell ' always the NW most cell in the entire area
    Private _cells As New Generic.List(Of Cell)
    Private _cellsMerged As Boolean = False
    Private _id As Int32

    Public Sub New(ByVal id As Int32)
        _id = id
    End Sub
    ' builds the area based on the volumes of all the cells passed in	
    Public Sub New(ByVal cells() As Cell, ByVal id As Int32)
        Me.New(id)
        addCell(cells)
    End Sub

    Public ReadOnly Property ID() As Int32
        Get
            Return _id
        End Get
    End Property

    Public ReadOnly Property BBox() As BoundingBox
        Get
            Return _bounds
        End Get
    End Property

    Public ReadOnly Property Links() As Generic.List(Of Link)
        Get
            Return _links
        End Get
    End Property

    Public Function ContainsPoint(ByVal p As MTV3D65.TV_3DVECTOR) As Boolean
        Return BoundingBox.PointInBounds(_bounds, p)
    End Function
    Public Property Cells(ByVal index As Int32) As Cell
        Get
            If index < 0 OrElse index > _cells.Count - 1 Then Throw New ArgumentOutOfRangeException
            Return _cells(index)

        End Get
        Set(ByVal value As Cell)
            If index < 0 OrElse index > _cells.Count - 1 Then Throw New ArgumentOutOfRangeException
            _cells(index) = value
        End Set
    End Property
    Public Sub addCell(ByVal c() As Cell)
        If c.Length > 0 Then
            For i As Int32 = 0 To c.Length - 1
                addCell(c(i))
            Next
        End If
    End Sub

    Public Sub addCell(ByVal c As Cell)
        Debug.Assert(Not _cells.Contains(c), "Cell is already a member of this area")
        If _cellsMerged Then Throw New Exception("Cells cannot be added after they've been merged.")

        c.AreaID = _id
        If _root Is Nothing Then

            _root = c
        End If
        _cells.Add(c)


        If _cells.Count > 1 Then
            _bounds = BoundingBox.CombineBoundingBoxes(_bounds, c.BBox)
        Else
            _bounds = c.BBox ' bounding box must always init to that of the first cell added.
        End If
    End Sub

    Public Property NWCell() As Cell
        Get
            Return _root
        End Get
        Set(ByVal value As Cell)
            If value Is Nothing Then Throw New ArgumentNullException("NWCell cannot be null")
            _root = value
        End Set
    End Property

    Public ReadOnly Property NECell() As Cell
        Get
            Dim c As Cell
            c = _root
            Do While c.Neighbor(CardinalDirection.East) IsNot Nothing
                If c.Neighbor(CardinalDirection.East).AreaID <> _id Then Exit Do
                c = c.Neighbor(CardinalDirection.East)
            Loop
            'If c Is _root Then Return Nothing Else Return c
            Return c
        End Get
    End Property

    Public ReadOnly Property SWCell() As Cell
        Get
            Dim c As Cell
            c = _root
            Do While c.Neighbor(CardinalDirection.South) IsNot Nothing
                If c.Neighbor(CardinalDirection.South).AreaID <> _id Then Exit Do
                c = c.Neighbor(CardinalDirection.South)
            Loop
            'If c Is _root Then Return Nothing Else Return c
            Return c
        End Get
    End Property

    Public ReadOnly Property SECell() As Cell
        Get
            Dim c As Cell
            Dim sw As Cell
            sw = SWCell
            c = sw

            If c Is Nothing Then Return Nothing

            Do While c.Neighbor(CardinalDirection.East) IsNot Nothing
                If c.Neighbor(CardinalDirection.East).AreaID <> _id Then Exit Do
                c = c.Neighbor(CardinalDirection.East)
            Loop
            'If c Is _root OrElse c Is sw Then Return Nothing Else Return c
            Return c
        End Get
    End Property

    Public ReadOnly Property CellCount() As Int32
        Get
            Return _cells.Count
        End Get
    End Property

    Public ReadOnly Property AreaWidth() As Int32
        Get
            Dim x As Int32 = 1
            Dim c As Cell = _root

            Debug.Assert(_root.AreaID = _id)
            Do While c.Neighbor(CardinalDirection.East) IsNot Nothing
                If c.Neighbor(CardinalDirection.East).AreaID <> _id Then Exit Do
                c = c.Neighbor(CardinalDirection.East)
                x += 1
            Loop
            'Debug.Assert(c IsNot _root)
            If c Is _root Then x = 1
            Return x
        End Get
    End Property

    Public ReadOnly Property AreaHeight() As Int32
        Get
            Dim x As Int32 = 1
            Dim c As Cell = _root

            Debug.Assert(_root.AreaID = _id)
            Do While c.Neighbor(CardinalDirection.South) IsNot Nothing
                If c.Neighbor(CardinalDirection.South).AreaID <> _id Then Exit Do
                c = c.Neighbor(CardinalDirection.South)
                x += 1
            Loop
            'Debug.Assert(c IsNot _root)
            If c Is _root Then x = 1
            Return x
        End Get
    End Property

    Public Sub addLink(ByRef p As Link)
        Debug.Assert(Not _links.Contains(p), "Portal is already a member of this area")
        _links.Add(p)
    End Sub

    Public Sub removeLink(ByRef p As Link)
        If _links.Contains(p) Then _links.Remove(p)
    End Sub

    ''' <summary>
    ''' A area is comprised of a bunch of cells.   This routine performs cell compression by
    ''' merging all the inner cells into a single large cell. Edge cells will all update their approriate neighbor to
    ''' reference the new combined inner cell.  NOTE: Remember that cells reference neighbor cells and links reference
    ''' areas.  They should not be confused.  This is why we continue to say that the merged inner cells combined to
    ''' form a larger cell (and not a area). This routine generally results in a 5:1 cell compression ratio.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub MergeInnerCells()
        If _cellsMerged Then Throw New Exception("Cells already merged.  This can only be done once.")

        'TODO: must this be done after any joining of areas? (remember joining areas is for lowering the total number
        ' of areas by finding neighboring areas that share the same height or width and thus can be joined to create a rectangle
        ' Recall that initial area creation only creates perfect square areas.
        ' i think so and if thats the case, the assert isnt valid
        ' first determine our x, y width in cells.  assert xWidth  = yWidth
        ' then we know that interior cells are always between the extents of our boundaries

        Dim x As Int32 = AreaWidth : Dim y As Int32 = AreaHeight
        Debug.Assert(x = y)

        ' Call DebugLoop()

        ' no non edge cells to merge if less than 16 cells 
        If _cells.Count < 16 Then
            _cellsMerged = True
            Exit Sub
        End If

        'starting at the SE cell from the root NW cell, 
        Dim innerRoot As Cell = _root.Neighbor(CardinalDirection.South).Neighbor(CardinalDirection.East)
        Dim nextCell As Cell
        Dim dir As Int32 '0 = east , 1=west
        nextCell = innerRoot
        ' edge cells must all now point to the innerRoot for the appropriate side
        ' remove inner cells once added into the inner root. inner root's 
        For i As Int32 = 1 To y - 3
            For j As Int32 = 1 To x - 3
                If dir = 0 Then
                    nextCell = nextCell.Neighbor(CardinalDirection.East)
                Else
                    Debug.Assert(dir = 1)
                    nextCell = nextCell.Neighbor(CardinalDirection.West)
                End If
                ' merge the volumes of this cell with the innerRoot
                innerRoot.Combine(nextCell)
                _cells.Remove(nextCell)
            Next

            If i < y - 2 Then
                dir = DirectCast(IIf(dir = 0, 1, 0), Int32)
                nextCell = nextCell.Neighbor(CardinalDirection.South)
                innerRoot.Combine(nextCell)
                _cells.Remove(nextCell)
            End If
        Next

        ' itterate along the edge cells and re-assign their inward neighbors to reference the inner root
        Dim c As Cell = NWCell
        For i As Int32 = 2 To y - 1
            Debug.Assert(c.AreaID = _id)
            c = c.Neighbor(CardinalDirection.South)
            c.Neighbor(CardinalDirection.East) = innerRoot
        Next

        c = NECell
        For i As Int32 = 2 To y - 1
            Debug.Assert(c.AreaID = _id)
            c = c.Neighbor(CardinalDirection.South)
            c.Neighbor(CardinalDirection.West) = innerRoot
        Next

        c = NWCell
        For i As Int32 = 2 To x - 1
            Debug.Assert(c.AreaID = _id)
            c = c.Neighbor(CardinalDirection.East)
            c.Neighbor(CardinalDirection.South) = innerRoot
        Next

        c = SWCell
        For i As Int32 = 2 To x - 1
            Debug.Assert(c.AreaID = _id)
            c = c.Neighbor(CardinalDirection.East)
            c.Neighbor(CardinalDirection.North) = innerRoot
        Next

        ' Call DebugLoop()

        _cellsMerged = True
    End Sub

    ' just to validate that all cells have the proper neighboring cells. It runs very fast but
    ' this sub and calls to it can be disabled in final build if preferred.
    ' generally speaking, its good to keep just as sanity check that nothing got broken.
    Private Sub DebugLoop()
        Dim x As Int32 = AreaWidth : Dim y As Int32 = AreaHeight
        Debug.Assert(x = y)


        Dim c As Cell = NWCell
        For i As Int32 = 2 To y - 1
            Debug.Assert(c.AreaID = _id)
            c = c.Neighbor(CardinalDirection.South)
        Next

        c = NECell
        For i As Int32 = 2 To y - 1
            Debug.Assert(c.AreaID = _id)
            c = c.Neighbor(CardinalDirection.South)
        Next

        c = NWCell
        For i As Int32 = 2 To x - 1
            Debug.Assert(c.AreaID = _id)
            c = c.Neighbor(CardinalDirection.East)
        Next

        c = SWCell
        For i As Int32 = 2 To x - 1
            Debug.Assert(c.AreaID = _id)
            c = c.Neighbor(CardinalDirection.East)
        Next
    End Sub
    ''' <summary>
    ''' Visual debugging aids.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Draw()
        Dim white As Integer = globals.Globals.RGBA(1, 1, 1, 1)
        Dim black As Integer = globals.Globals.RGBA(0, 0, 0, 1)
        Dim pink As Integer = globals.Globals.RGBA(1, 0, 1, 1)

        Dim yellow As Integer = globals.Globals.RGBA(1, 1, 0, 1)


        ' draw the bounding box for this area 
        _bounds.Draw(white)

        If _links.Count = 0 Then
            _bounds.Draw(black)
            Exit Sub
        End If

        ' draw the links
        For Each p As Link In _links
            p.BBox.Draw(yellow)

            ' draw opposite destination area for the link
            p.Destination(Me).BBox.Draw(pink)
        Next

    End Sub


    Public Sub Read(ByVal reader As System.IO.BinaryReader) Implements IBinaryWriter.Read

        Dim count As Int32 = reader.ReadInt32
        'For i As Int32 = 0 To count - 1
        '    Dim p As Portal = New Portal
        '    _links.Add(p)
        'Next
    End Sub

    Public Sub Write(ByVal writer As System.IO.BinaryWriter) Implements IBinaryWriter.Write

        Debug.Assert(_cellsMerged = True) ' should not be able to save without the inner cells having been merged


        ' else just write the area and link data
        writer.Write(_links.Count)
        For Each p As Link In _links
            p.Write(writer)
        Next

    End Sub
End Class


