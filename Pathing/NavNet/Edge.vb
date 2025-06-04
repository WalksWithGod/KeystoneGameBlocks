Public Class Edge : Implements IBinaryWriter

    Private _key As String  ' the dictionary key of areaID + outwardDirection.toString 
    Private _cells As Generic.Dictionary(Of MTV3D65.TV_3DVECTOR, Cell)
    Private _assignedStatus As Generic.List(Of Boolean) ' used for determining which cells in the edge have already been assigned to a link
    Private _area As Area
    Private _outwardDirection As CardinalDirection

    Public Sub New(ByRef area As Area, ByVal direction As CardinalDirection)
        If area Is Nothing Then Throw New ArgumentNullException
        _area = area
        _outwardDirection = direction
        _key = CreateKey(area.ID, direction)
        _cells = New Generic.Dictionary(Of MTV3D65.TV_3DVECTOR, Cell)
        _assignedStatus = New Generic.List(Of Boolean)

        GetCells()
    End Sub

    Private Function CreateKey(ByVal id As Integer, ByVal dir As CardinalDirection) As String
        Return id.ToString & "_" & dir.ToString
    End Function
    ''' <summary>
    ''' Depending on which outward direction this edge is with respect to its owner Area 
    ''' it finds and assigns the cells that belong to this edge.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub GetCells()
        Dim c As Cell = Nothing

        Debug.Assert(_area.AreaWidth = _area.AreaHeight)

        Select Case _outwardDirection
            Case CardinalDirection.West
                c = _area.NWCell
                For i As Int32 = 0 To _area.AreaHeight - 1
                    Debug.Assert(c.AreaID = _area.ID)
                    _cells.Add(c.Position, c)
                    _assignedStatus.Add(False)
                    c = c.Neighbor(CardinalDirection.South)
                Next
            Case CardinalDirection.East
                c = _area.NECell
                For i As Int32 = 0 To _area.AreaHeight - 1
                    Debug.Assert(c.AreaID = _area.ID)
                    _cells.Add(c.Position, c)
                    _assignedStatus.Add(False)
                    c = c.Neighbor(CardinalDirection.South)
                Next
            Case CardinalDirection.North
                c = _area.NWCell
                For i As Int32 = 0 To _area.AreaWidth - 1
                    Debug.Assert(c.AreaID = _area.ID)
                    _cells.Add(c.Position, c)
                    _assignedStatus.Add(False)
                    c = c.Neighbor(CardinalDirection.East)
                Next
            Case CardinalDirection.South
                c = _area.SWCell
                For i As Int32 = 0 To _area.AreaWidth - 1
                    Debug.Assert(c.AreaID = _area.ID)
                    _cells.Add(c.Position, c)
                    _assignedStatus.Add(False)
                    c = c.Neighbor(CardinalDirection.East)
                Next
        End Select
        Debug.Assert(c IsNot Nothing)
    End Sub

    Public ReadOnly Property Key() As String
        Get
            Return _key
        End Get
    End Property

    Public ReadOnly Property OutwardDirection() As CardinalDirection
        Get
            Return _outwardDirection
        End Get
    End Property

    Public ReadOnly Property InnerDirection() As CardinalDirection
        Get
            Return NavNetBuilder.GetOppositeDirection(_outwardDirection)
        End Get
    End Property

    Public ReadOnly Property Area() As Area
        Get
            Return _area
        End Get
    End Property
    Public ReadOnly Property areaID() As Int32
        Get
            Return _area.ID
        End Get
    End Property

    Public ReadOnly Property CellCount() As Int32
        Get
            Return _cells.Count
        End Get
    End Property

    Public Property IsCellAssignedToPortal(ByVal key As MTV3D65.TV_3DVECTOR) As Boolean
        Get
            Return _assignedStatus(GetCellIndex(key))
        End Get
        Set(ByVal value As Boolean)
            _assignedStatus(GetCellIndex(key)) = value
        End Set
    End Property

    Public ReadOnly Property Cells() As Generic.Dictionary(Of MTV3D65.TV_3DVECTOR, Cell)
        Get
            Return _cells
        End Get
    End Property

    Public ReadOnly Property Cell(ByVal key As MTV3D65.TV_3DVECTOR) As Cell
        Get
            Dim c As Cell = Nothing
            _cells.TryGetValue(key, c)
            Return c
        End Get
    End Property

    Public Sub RemoveCell(ByVal c As Cell)
        If _cells.ContainsKey(c.Position) Then
            _cells.Remove(c.Position)
        End If
    End Sub

    Private Function GetCellIndex(ByVal key As MTV3D65.TV_3DVECTOR) As Int32
        Dim i As Int32
        For Each k As MTV3D65.TV_3DVECTOR In _cells.Keys
            If k.Equals(key) Then Return i
            i += 1
        Next
        Return -1
    End Function

    Public Sub Read(ByVal reader As System.IO.BinaryReader) Implements IBinaryWriter.Read

        Dim id As Integer = reader.ReadInt32
        _outwardDirection = reader.ReadInt32

        ' create the key which is a combination of AreaID.ToString and OutwardDirection.ToString
        _key = CreateKey(id, _outwardDirection)

        Dim count As Int32 = reader.ReadInt32
        Dim c As Cell

        For i As Int32 = 0 To count - 1
            c = New Cell ' TODO: this is totally wrong.   You cant restore a cell simply by newing it and having it loads its settings from a file

            c.Read(reader)
            _cells.Add(c.Position, c)
        Next

    End Sub

    Public Sub Write(ByVal writer As System.IO.BinaryWriter) Implements IBinaryWriter.Write

        writer.Write(_area.ID)
        writer.Write(_outwardDirection)

        writer.Write(_cells.Count)
        For Each c As Cell In _cells.Values
            c.Write(writer)
        Next

    End Sub
End Class
