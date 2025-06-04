Public Class NavNetBuilder

    Private Structure SubNet
        Public FileName As String
        Public AllCells As Generic.Dictionary(Of MTV3D65.TV_3DVECTOR, Cell)
        Public TaskList As Generic.Stack(Of Cell)
        Public BoundaryList As Generic.Stack(Of Cell)
        Public Bounds As BoundingBox
        Public CellsGenerated As Boolean
    End Structure

    Private Shared sg As SubNet  'subgraph
    Private _sg() As SubNet               ' all subgraphs

    Private _worldBounds As BoundingBox
    Private _partitionWidth, _partitionDepth, _partitionHeight As Single
    Private _cellWidth, _cellDepth, _cellHeight, _cellMaxStepUpHeight, _cellMaxStepDownHeight As Single


    ' TODO: i believe "worldBounds" also include the partition height/width infromation since i think CGBuilder
    ' can also handle the partititioning of hte entire world and managing seperate _allCells, _taskLists, etc for each.  
    ' or should i have another class do that?  
    Public Sub New(ByVal worldBounds As BoundingBox, ByVal cellWidth As Single, ByVal cellDepth As Single, ByVal cellHeight As Single, ByVal maxStepUpHeight As Single, ByVal maxStepDownHeight As Single)
        If cellWidth <= 0 OrElse cellDepth <= 0 OrElse cellHeight <= 0 OrElse maxStepUpHeight < 0 OrElse maxStepDownHeight < 0 Then Throw New ArgumentOutOfRangeException

        sg = New SubNet()

        ' its easier to specify iPartitionsX , iPartitionsZ for partitions
        ' however, if they do specify actual values then thrown an exception if worldWidth / partitionWidth = fraction.  same for WorldDepth / partitionDepth
        ' heights are irrelevant since partition heights = world heights

        ' Users will typically want to specify the cell dimensions rather than compute an iCellsX, iCellsZ i think?  We will provide both.
        ' 
        ' cellWidht and cellDepth must also be evenly divisible into a partition.
        ' throw exception if partitionWidth / cellWidth = fraction or PartitionDepth / cellDepth = fraction

        ' cell height must be less than= to worldHeight

        ' once we have all this data we will create subgraph instances for
        ' count = iPartitionsX * iPartitionsZ
        Try
            sg.Bounds = worldBounds ' based on world size and partition info, the region needs to be computed
            _cellWidth = cellWidth
            _cellDepth = cellDepth
            _cellHeight = cellHeight
            _cellMaxStepUpHeight = maxStepUpHeight
            _cellMaxStepDownHeight = maxStepDownHeight

            sg.AllCells = New Generic.Dictionary(Of MTV3D65.TV_3DVECTOR, Cell)
            sg.TaskList = New Generic.Stack(Of Cell)
            sg.BoundaryList = New Generic.Stack(Of Cell)
        Catch

        End Try
    End Sub

    ' TODO: need index into subgraph array or change it to subgraph dictionary and use a key
    ' but i think index is ok.  then we pass that index to our _areaBuilder and its corresponding
    ' region will use the same id
    Public ReadOnly Property Cells() As Generic.Dictionary(Of MTV3D65.TV_3DVECTOR, Cell)
        Get
            Return sg.AllCells
        End Get
    End Property
    Public Sub Clear()
        sg.AllCells.Clear()
        sg.TaskList.Clear()
    End Sub

    Public Function CellsGenerated() As Boolean
        Return sg.CellsGenerated
    End Function
    'Generates list of traversable cells.  Here the neighbors for each cell are also assigned so that
    ' connectivity is established.  Cells that are processed are added to the _allCells list
    ' unless .Clear has been called on this object.  This way we can 
    Public Sub GenerateNet(Optional ByVal List() As Cell = Nothing)

        Debug.Print("Generating Cells.")
        If List IsNot Nothing Then
            ' push all the items onto the tasklist

        End If


        If Not CellsGenerated() Then
            Dim x, y As Single
            x = sg.Bounds.Min.x : y = sg.Bounds.Min.z

            'create start cell at top left of region area 
            ' TODO: the start cell technically can start anywhere in the region provided
            'a) there's no mesh besides terrain to collide with
            'b) the position of the cell fits within any grid cell.  This guarantees that all cells within
            '   the region are the exact same size and that the rows and columns will always match up with
            '   any neighboring pages that might be used.
            Dim startCell As Cell = New Cell(New MTV3D65.TV_3DVECTOR(x, AreaHelper.GetHeight(x, y), y), _cellWidth, _cellDepth, _cellHeight) 'TODO: figure starting coords at top/left of page

            sg.AllCells.Add(startCell.Position, startCell)
            sg.TaskList.Push(startCell)

            Do While sg.TaskList.Count > 0
                Dim c As Cell = sg.TaskList.Pop
                For i As Int32 = 0 To 3
                    ' if a neighbor isnt already assigned for that side, try to traverse in that direction
                    If c.Neighbor(DirectCast(i, CardinalDirection)) Is Nothing Then
                        Dim tryPosition As MTV3D65.TV_3DVECTOR = GetNewPosition(c.Position, DirectCast(i, CardinalDirection))
                        If Not BoundingBox.PointInBounds(sg.Bounds, tryPosition) Then
                            ' it must be in a neighboring partition and so the current cell is a border cell.
                            ' find the neighbor partition to that CardinalDirection and verify its correct and contains the tryPosition
                            ' and then call the CanTraverse using that partition instead.
                            ' and set the CrossingPartition=True.
                            ' then when we create the PlaceHolderCell 
                        End If
                        If AreaHelper.CanTraverse(c.Position, tryPosition, sg.Bounds, _cellHeight, _cellMaxStepUpHeight) Then
                            ' if we're able to traverse to this position, then add a new cell for that neighbor
                            ' and add this cell to the task list
                            ' TODO: if we can traverse here and the newPosition is over a page border, then we
                            ' need to add that cell to the tasklist of the subgraph for that page also.  
                            Dim newCell As Cell = GetCell(tryPosition)
                            If newCell Is Nothing Then
                                newCell = New Cell(tryPosition, _cellWidth, _cellDepth, _cellHeight)
                                sg.AllCells.Add(newCell.Position, newCell)
                                sg.TaskList.Push(newCell)
                            Else
                                ' NOTE: By definition, if the call to GetCell(newPosition) returns an existing cell
                                ' then that cell is _exactly_ in the same position and of the same dimensions.  This is because
                                ' the Y height is part of the TV_3DVector key used to store the cell in the collection.  This allows us to have
                                ' stacked cells that have same x,z but different Y heights.
                            End If
                            c.Neighbor(DirectCast(i, CardinalDirection)) = newCell
                            ' save us some time next itteration and make the assignmnet mutual. NOTE at the top of this
                            ' for loop we check first to only process when the neighbor in that direction is unassigned.
                            newCell.Neighbor(GetOppositeDirection(DirectCast(i, CardinalDirection))) = c
                        Else
                            ' if cant walk there, then add a nullCell type there instead
                            c.Neighbor(DirectCast(i, CardinalDirection)) = New NullCell
                        End If
                    End If
                Next
            Loop
        End If
        sg.CellsGenerated = True

        Debug.WriteLine("Cell Generation Complete.  Cell Count = " & sg.AllCells.Count)
        Debug.Write("Saving temporary cell data to file... ")
        Save("celllist.dat")
        Debug.WriteLine("complete.")
        sg.AllCells.Clear() '<-- to verify it works, start fresh

        ' NOTE: our cells must always be loadable as long as the areas
        ' being generated for this page are not complete.  That means
        Debug.Write("Loading temporary cell data from file... ")
        Load("celllist.dat")
        Debug.WriteLine("complete.")
    End Sub

    ' NOTE: Here we need to be able to find a cell from any of the regions.  So first
    ' we'd need to find the proper region based on the bounds.  Simply itterating through
    ' all until we get a "PointInBounds = true" and then lookup the cell.
    Public Function GetCell(ByVal position As MTV3D65.TV_3DVECTOR) As Cell
        Dim c As Cell = Nothing
        If sg.AllCells.TryGetValue(position, c) Then
            Return c
        Else
            Return Nothing
        End If

    End Function


    Public Shared Function GetOppositeDirection(ByVal direction As CardinalDirection) As CardinalDirection
        Select Case direction
            Case CardinalDirection.West
                Return CardinalDirection.East
            Case CardinalDirection.East
                Return CardinalDirection.West
            Case CardinalDirection.North
                Return CardinalDirection.South
            Case CardinalDirection.South
                Return CardinalDirection.North
        End Select
    End Function

    Private Function GetNewPosition(ByVal position As MTV3D65.TV_3DVECTOR, ByVal direction As CardinalDirection) As MTV3D65.TV_3DVECTOR
        Dim tryPosition As MTV3D65.TV_3DVECTOR
        Select Case direction
            Case CardinalDirection.West
                tryPosition = New MTV3D65.TV_3DVECTOR(position.x - _cellWidth, 0, position.z)
            Case CardinalDirection.East
                tryPosition = New MTV3D65.TV_3DVECTOR(position.x + _cellWidth, 0, position.z)
            Case CardinalDirection.North
                tryPosition = New MTV3D65.TV_3DVECTOR(position.x, 0, position.z + _cellDepth)
            Case CardinalDirection.South
                tryPosition = New MTV3D65.TV_3DVECTOR(position.x, 0, position.z - _cellDepth)
        End Select

        tryPosition.y = AreaHelper.UNKNOWNHEIGHT
        Return tryPosition
    End Function

    Public Sub Save(ByVal path As String)
        ' save the cells 
        Dim fs As IO.FileStream = Nothing
        Dim writer As IO.BinaryWriter = Nothing
        Try
            fs = IO.File.Create(path)
            writer = New IO.BinaryWriter(fs)

            writer.Write(sg.AllCells.Count)
            For Each c As Cell In sg.AllCells.Values
                c.Write(writer)
            Next

        Catch ex As Exception
        Finally
            If writer IsNot Nothing Then writer.Close()
            If fs IsNot Nothing Then fs.Close()
        End Try
    End Sub

    Public Sub Load(ByVal path As String)
        Dim fs As IO.FileStream = Nothing
        Dim reader As IO.BinaryReader = Nothing

        Try
            fs = New IO.FileStream(path, IO.FileMode.Open)
            reader = New IO.BinaryReader(fs)

            Dim count As Int32 = reader.ReadInt32

            For i As Int32 = 0 To count - 1
                Dim c As New Cell
                c.Read(reader)
                sg.AllCells.Add(c.Position, c)
            Next
        Catch ex As Exception
            Debug.WriteLine("CGBuilder.GenerateNet() -- " & ex.Message)
        Finally
            If reader IsNot Nothing Then reader.Close()
            If fs IsNot Nothing Then fs.Close()
        End Try

        ' restore the neighbor references for all cells EXCEPT cell pointers.
        ' NOTE: cell's neighbor references must be restored prior to trying to generate the areas
        For Each c As Cell In sg.AllCells.Values
            For i As Int32 = 0 To 3
                If Not NullCell.IsNullCell(c.NeighborID(i)) Then
                    c.Neighbor(i) = sg.AllCells(c.NeighborID(i))
                Else
                    c.Neighbor(i) = New NullCell
                End If
            Next
        Next
    End Sub
End Class
