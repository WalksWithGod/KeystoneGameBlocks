Public Class AreaBuilder

    Private Structure Region
        Public Filename As String
        Public Areas As Generic.List(Of Area)
        Public Edges As Generic.Dictionary(Of String, Edge)
        Public EdgesCreated As Boolean
        Public PortalsCreated As Boolean
    End Structure

    Private _regions As New Generic.Dictionary(Of MTV3D65.TV_3DVECTOR, Region)
    Private _currentRegion As Region

    Private _RandomGenerator As New Random(Environment.TickCount)


    Public ReadOnly Property Areas(ByVal key As MTV3D65.TV_3DVECTOR) As Area()
        Get
            Return _regions(key).Areas.ToArray
        End Get
    End Property

    Public ReadOnly Property Edges(ByVal key As MTV3D65.TV_3DVECTOR) As Generic.Dictionary(Of String, Edge)
        Get
            Return _regions(key).Edges
        End Get
    End Property

    Public Sub Draw(ByVal key As MTV3D65.TV_3DVECTOR)

        If _regions(key).Areas.Count > 0 Then
            For Each sec As Area In _regions(key).Areas
                sec.Draw()
            Next
        End If
    End Sub

    'Public Sub Generate()(ByVal pageCount As Int32)
    '    Dim rowstart, futureNode, nextNode As WorldArea
    '    Dim height, width As Int32
    '    height = CInt(Math.Sqrt(pageCount))
    '    width = height

    '    Debug.WriteLine("Fixing seems... this takes ~eight minutes on 64x64 page scene w/ p4 2.5GHz")

    '    Dim tvlands(0 To height - 1, 0 To width - 1) As MTV3D65.TVLandscape
    '    Dim name As String = ""

    '    ' pick the top left and go from left to right til we reach the edge
    '    ' then recursively call starting with the south sibling of our very first in this row.  repeat
    '    ' this until there is no south sibling (not including wrapped siblings)
    '    Dim pick As PointF
    '    pick = New PointF(_LuxScene.World.Bounds.Left, _LuxScene.World.Bounds.Top)
    '    _pickedNode = Nothing ' reset any previous one
    '    Dim picker As New QTreePick(New QTreePick.NodePicked(AddressOf NodePicked), New IQTreeTraverser.ProgressUpdate(AddressOf ProgressHandler))
    '    picker.Pick = pick
    '    picker.Apply(DirectCast(_LuxScene.World, QTreeNodeBranch))

    '    If _pickedNode IsNot Nothing Then
    '        ' starting with top left corner leaf node in the world quadtree
    '        rowstart = _pickedNode
    '        nextNode = rowstart

    '        ' start off loading the first row
    '        For x As Int32 = 0 To width - 1
    '            name = Configuration.Settings.AppPath & "\terrain\data\page" & nextNode.Index & ".terrain"
    '            tvlands(0, x) = TV3DUtility._Scene.CreateLandscape(name)
    '            tvlands(0, x).LoadTerrainData(name)
    '            nextNode = DirectCast(nextNode.Neighbors(QT_NODE_NEIGHBOR.EAST), WorldArea)
    '        Next

    '        For i As Int32 = 0 To height - 1

    '            nextNode = rowstart
    '            If i < height - 1 Then
    '                futureNode = DirectCast(rowstart.Neighbors(QT_NODE_NEIGHBOR.SOUTH), WorldArea)
    '            End If

    '            For j As Int32 = 0 To width - 1

    '                ' load an east element for future row
    '                If i < height - 1 Then
    '                    name = Configuration.Settings.AppPath & "\terrain\data\page" & futureNode.Index & ".terrain"
    '                    tvlands(i + 1, j) = TV3DUtility._Scene.CreateLandscape(name)
    '                    tvlands(i + 1, j).LoadTerrainData(name)
    '                End If

    '                '  unload an east element of a old row
    '                If i > 1 Then
    '                    tvlands(i - 2, j).DeleteAll()
    '                    tvlands(i - 2, j) = Nothing
    '                End If

    '                ' fix seems
    '                If i > 0 Then
    '                    tvlands(i, j).FixSeams(tvlands(i - 1, j))
    '                End If
    '                If i < height - 1 Then
    '                    tvlands(i, j).FixSeams(tvlands(i + 1, j))
    '                End If

    '                If j < width - 1 Then
    '                    tvlands(i, j).FixSeams(tvlands(i, j + 1))
    '                End If
    '                If j > 0 Then
    '                    tvlands(i, j).FixSeams(tvlands(i, j - 1))
    '                End If

    '                ' save the current node 
    '                If Not CBool(tvlands(i, j).SaveTerrainData(tvlands(i, j).GetName, MTV3D65.CONST_TV_LANDSAVE.TV_LANDSAVE_ALL)) Then

    '                    Debug.Assert(False, "Could not save terrain @ index= " & nextNode.Index & " data to disk.")
    '                Else
    '                    Static seemsFixed As Int32
    '                    seemsFixed += 1
    '                    Debug.Print("Saved at  i = " & i & "  j = " & j & " Total landscapes saved = " & seemsFixed)


    '                End If

    '                If j < width - 1 Then
    '                    nextNode = DirectCast(nextNode.Neighbors(QT_NODE_NEIGHBOR.EAST), WorldArea)
    '                    futureNode = DirectCast(futureNode.Neighbors(QT_NODE_NEIGHBOR.EAST), WorldArea)
    '                End If
    '            Next

    '            ' set the first element in the next row
    '            If i < height - 1 Then
    '                rowstart = DirectCast(rowstart.Neighbors(QT_NODE_NEIGHBOR.SOUTH), WorldArea)
    '            End If
    '        Next
    '    End If
    'End Sub

    Public Sub GenerateAreas(ByVal key As MTV3D65.TV_3DVECTOR, ByVal cells As Generic.Dictionary(Of MTV3D65.TV_3DVECTOR, Cell))
        Dim current As Cell
        Dim currentArea As Area
        Dim bDone As Boolean = False
        Dim level As Int32 = 0

        _currentRegion = New Region
        _currentRegion.Areas = New Generic.List(Of Area)
        _currentRegion.Edges = New Generic.Dictionary(Of String, Edge)()
        _regions.Add(key, _currentRegion)

        Debug.Print("Generating Areas.")
        Debug.Print("     Area Phase 1 Starting.  Merging Inner cells.")
        Do While cells.Count > 0
            current = GetRandomCell(cells)
            currentArea = New Area(_currentRegion.Areas.Count + 1) ' no area at 0 or less.  first = 1
            currentArea.addCell(current)

            ' keep adding concentric layers to form an increasingly larger square until we cant anymore
            bDone = False
            level = 1
            ' try in 4 directions (each direction starts at a particular corner. )
            ' these specific corners are important since the Down2Right, Down2Left, etc functions
            ' depend on this.  Take the first of the 4 that gives us a complete layer 

            Do While Not bDone
                If DownAndRight(currentArea) Then ' use OrElse 
                    'Debug.Print("Test = DownAndRight")
                ElseIf RightAndUp(currentArea) Then
                    'Debug.Print("Test = RightAndUp")
                ElseIf UpAndLeft(currentArea) Then
                    'Debug.Print("Test = UpAndLeft")
                ElseIf LeftAndDown(currentArea) Then
                    'Debug.Print("Test = LeftAndDown")
                Else
                    'Debug.Print("Test = DONE")
                    bDone = True
                End If

                level += 1
            Loop

            ' if < 1, add the current to an orphan list and remove from  _allcells
            If currentArea.CellCount > 1 Then

                Debug.Assert(currentArea.AreaHeight = currentArea.AreaWidth)
                Debug.Assert(currentArea.CellCount = currentArea.AreaHeight * currentArea.AreaWidth)
                ' remove all the cells that were added from the Cells so that GetRandomCell wont repick one we've already processed
                For i As Int32 = 0 To currentArea.CellCount - 1
                    cells.Remove(currentArea.Cells(i).Position)
                Next

                ' NOTE: merge occurs _after_ the above removal since the merge alters the position which we use as a key value
                ' if we do the merge first,then the keys will be wrong for once cell in every area and will not be remoable from the _Cells
                ' list and we'll have an infinite loop
                currentArea.MergeInnerCells()
                _currentRegion.Areas.Add(currentArea)

            Else ' this area only has its original cell and no more.  if we want we can store them in a seperate list
                ' and later try to determine if we can find neighboring orphans and merge them into a single area.
                '_orphanCells.Add(currentArea.Cells(0))
                _currentRegion.Areas.Add(currentArea)
                Debug.Assert(current Is currentArea.Cells(0))
                cells.Remove(currentArea.Cells(0).Position)
            End If
        Loop

        Debug.Print("     Area Phase 1 Complete.  Inner cells merged.")
        _currentRegion.Edges = GenerateEdges(_currentRegion.Areas)
        _currentRegion.EdgesCreated = True
        'VerifyAreas()
        GeneratePortals(_currentRegion)

        Debug.Print("Area Generation Complete.  Area Count = " & _currentRegion.Areas.Count)
    End Sub


    ''' <summary>
    ''' Used to verify that every area's height and width matches.
    ''' That the cellcount in the area = height * width
    ''' All cells share the same areaID
    ''' That one can itterate around the outer edge cells in a complete revolution.
    ''' All outer edge cells neighbors OUTSIDE the area, have cell.areaID's that do NOT match the current areaID.
    ''' That one can traverse thru all the inner cells in a snake pattern
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub VerifyAreas(ByVal areas() As Area)
        Debug.Print("     Verifying Areas.")
        For Each sec As Area In areas
            Dim width As Int32 = sec.AreaWidth
            Dim height As Int32 = sec.AreaHeight
            Dim areaID As Int32 = sec.ID

            Debug.Assert(width = height)
            Debug.Assert(sec.CellCount = width * height)

            ' all cells share same areaid?
            For i As Int32 = 0 To sec.CellCount - 1
                Debug.Assert(sec.Cells(i).AreaID = areaID)
            Next

            ' that all edge cells can be traversed in a complete revolution
            Dim c As Cell = sec.NWCell
            For i As Int32 = 1 To height - 1
                c = c.Neighbor(CardinalDirection.South)
                Debug.Assert(c.AreaID = areaID)
            Next

            c = sec.NECell
            For i As Int32 = 1 To height - 1
                c = c.Neighbor(CardinalDirection.South)
                Debug.Assert(c.AreaID = areaID)
            Next

            c = sec.NWCell
            For i As Int32 = 1 To width - 1
                c = c.Neighbor(CardinalDirection.East)
                Debug.Assert(c.AreaID = areaID)
            Next

            c = sec.SWCell
            For i As Int32 = 1 To width - 1
                c = c.Neighbor(CardinalDirection.East)
                Debug.Assert(c.AreaID = areaID)
            Next
        Next

        Debug.Print("     Area Verification Complete.")
    End Sub

    ' find areas that share the exact same border on any side and merge them into a single area
    Private Sub JoinAreas()
        ' TODO
    End Sub

    ' Create 4 "edges" for every newly created area.  An edge simply stores information
    ' about what area it belongs too, which of the 4 sides of the area it is on,
    ' and from that it can determines on its own which cells within that area it contains.
    ' Edges can then be used to generate the links between areas.
    ' TODO: when generating edges, I should determine which ones are along page boundaries
    ' and then set a boolean.  These will need to find their opposite 
    Private Function GenerateEdges(ByVal areas As Generic.List(Of Area)) As Generic.Dictionary(Of String, Edge)
        Dim newEdge As Edge
        Dim edges As New Generic.Dictionary(Of String, Edge)

        Debug.Print("     Generating Edges.")

        For i As Int32 = 0 To areas.Count - 1

            For j As Int32 = 0 To 3
                ' the meat and potatoes is in the Edge constructor.  During New() it find and assigns the proper exterior cells in a area to a given edge.
                newEdge = New Edge(areas(i), DirectCast(j, CardinalDirection))
                edges.Add(newEdge.Key, newEdge)
                newEdge = Nothing
            Next
        Next
        Debug.Print("     Edge Generation Complete.  Edge Count = " & edges.Count)
        Return edges

    End Function

    Public Function FindEdge(ByVal edges As Generic.Dictionary(Of String, Edge), ByVal areaID As Int32, ByVal direction As CardinalDirection) As Edge
        Dim key As String = areaID.ToString & "_" & direction.ToString
        Dim e As Edge = Nothing

        Dim found As Boolean = edges.TryGetValue(key, e)
        Debug.Assert(found)
        Return e
    End Function

    Private Sub GeneratePortals(ByVal region As Region)
        Const NOT_SET As Int32 = -1
        Dim p As Link = Nothing
        Dim destID As Int32 = NOT_SET
        Dim prevDestID As Int32 = NOT_SET
        Dim oppositeCell As Cell
        Dim isTwoWay As Boolean = False
        Dim prevTwoWay As Boolean = False
        Dim linkDone As Boolean = True
        Dim opposite As CardinalDirection
        Dim destArea As Area = Nothing
        Dim oppositeEdge As Edge = Nothing

        Debug.Print("     Generating Portals.")

        For Each e As Edge In region.Edges.Values

            For Each c As Cell In e.Cells.Values
                If Not e.IsCellAssignedToPortal(c.Position) AndAlso Not TypeOf c Is NullCell Then
                    opposite = e.InnerDirection
                    ' this following call is what is going to complicate "stitching" of links between boundaries.
                    ' because here it assumes the cell can in fact reference its neighbor...
                    oppositeCell = c.Neighbor(e.OutwardDirection)

                    If oppositeCell IsNot Nothing AndAlso Not TypeOf oppositeCell Is NullCell Then
                        Debug.Assert(c.AreaID <> oppositeCell.AreaID)
                        oppositeEdge = FindEdge(region.Edges, oppositeCell.AreaID, opposite)
                        destID = oppositeCell.AreaID
                        destArea = oppositeEdge.Area


                        If oppositeCell.Neighbor(opposite) Is c Then
                            ' assert that there is no way this cell is already assigned since its mirror cell is not
                            Debug.Assert(oppositeEdge.IsCellAssignedToPortal(oppositeCell.Position) = False)
                            isTwoWay = True
                        Else
                            isTwoWay = False
                        End If

                        ' here we determine whether we will add onto the existing link to include more cells from this edge
                        ' or if we have to create a new link.
                        If (prevDestID = NOT_SET) OrElse (destID <> prevDestID) OrElse (isTwoWay <> prevTwoWay) Then
                            p = New Link(e.Area, destArea, isTwoWay)
                            p.BBox = c.BBox ' init to first cell. very important.
                            e.Area.addLink(p)
                            If isTwoWay Then destArea.addLink(p)
                        End If

                        p.BBox = BoundingBox.CombineBoundingBoxes(p.BBox, oppositeCell.BBox)
                        p.BBox = BoundingBox.CombineBoundingBoxes(p.BBox, c.BBox)

                    Else
                        ' there is no destination.  this edge cell will not be used as part of a link
                        destID = NOT_SET
                        isTwoWay = False
                        ' TODO: Here is where we would handle Page Boundaries.  If the oppociteCell is CellPointer 
                        ' then we will instance a PortalPointer and this when user tries to reference the destination
                        ' will (if not already set) try to find the proper reference by picking the proper page and calling
                        ' FindEdge and then resuming here.

                        ' we can still make a call t FindEdge, but first we must get a reference to the opposite page
                        ' _boundaryPage = GetPage(key) ' key is deduced based on direction and page dimensions
                        ' then we can find the OppositeEdge
                    End If

                    ' update the status of this cell in the Edge and if twoWay,then also its counterpart cell in the opposite edge
                    e.IsCellAssignedToPortal(c.Position) = True
                    If isTwoWay Then oppositeEdge.IsCellAssignedToPortal(oppositeCell.Position) = True

                    prevDestID = destID
                    prevTwoWay = isTwoWay
                    destID = NOT_SET
                    destArea = Nothing
                End If
            Next
        Next

        _currentRegion.PortalsCreated = True
        Debug.Print("     Portal Generation Complete.")

    End Sub

    ''' <summary>
    ''' Returns a random picked item from the list of all cells
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function GetRandomCell(ByVal cells As Generic.Dictionary(Of MTV3D65.TV_3DVECTOR, Cell)) As Cell
        Dim i, j As Int32
        i = _RandomGenerator.Next(0, cells.Count - 1)

        For Each c As Cell In cells.Values
            If i = j Then Return c
            j += 1
        Next
        Return Nothing
    End Function

    ' For the purposes of Area generation, determine if two cells can traverse between each other. PageBorder cells
    ' are treated as NullCell's since Areas cannot straddle page boundaries.
    Private Function IsTwoWayConnection(ByVal c1 As Cell, ByVal direction As CardinalDirection) As Boolean
        Dim c2 As Cell

        If c1 Is Nothing Then Return False
        If TypeOf c1 Is NullCell Then Return False ' or Typeof c1 is PageBorder ' for the purposes of area generation we return False.

        Select Case direction

            Case CardinalDirection.East
                If c1.Neighbor(CardinalDirection.East) IsNot Nothing Then
                    c2 = c1.Neighbor(CardinalDirection.East)
                    If TypeOf c2 Is NullCell Then Return False
                    If c2.Neighbor(CardinalDirection.West) IsNot Nothing AndAlso c2.Neighbor(CardinalDirection.West) Is c1 Then Return True
                End If
            Case CardinalDirection.West
                If c1.Neighbor(CardinalDirection.West) IsNot Nothing Then
                    c2 = c1.Neighbor(CardinalDirection.West)
                    If TypeOf c2 Is NullCell Then Return False
                    If c2.Neighbor(CardinalDirection.East) IsNot Nothing AndAlso c2.Neighbor(CardinalDirection.East) Is c1 Then Return True
                End If
            Case CardinalDirection.South
                If c1.Neighbor(CardinalDirection.South) IsNot Nothing Then
                    c2 = c1.Neighbor(CardinalDirection.South)
                    If TypeOf c2 Is NullCell Then Return False
                    If c2.Neighbor(CardinalDirection.North) IsNot Nothing AndAlso c2.Neighbor(CardinalDirection.North) Is c1 Then Return True
                End If
            Case CardinalDirection.North
                If c1.Neighbor(CardinalDirection.North) IsNot Nothing Then
                    c2 = c1.Neighbor(CardinalDirection.North)
                    If TypeOf c2 Is NullCell Then Return False
                    If c2.Neighbor(CardinalDirection.South) IsNot Nothing AndAlso c2.Neighbor(CardinalDirection.South) Is c1 Then Return True
                End If
        End Select

    End Function

    Private Function IsTwoWayConnection(ByVal c1 As Cell, ByVal c2 As Cell) As Boolean
        If c1 Is Nothing OrElse c2 Is Nothing Then Return False
        If TypeOf c1 Is NullCell OrElse TypeOf c2 Is NullCell Then Return False
        If c2.AreaID > 0 Then Return False '<-- for determining the initial connectivity, this is critical.. however. 
        ' this means we cant use this function for checking TwoWay _across_ area boundaries
        If c1.Neighbor(CardinalDirection.East) Is c2 AndAlso c2.Neighbor(CardinalDirection.West) Is c1 Then Return True
        If c1.Neighbor(CardinalDirection.West) Is c2 AndAlso c2.Neighbor(CardinalDirection.East) Is c1 Then Return True
        If c1.Neighbor(CardinalDirection.South) Is c2 AndAlso c2.Neighbor(CardinalDirection.North) Is c1 Then Return True
        If c1.Neighbor(CardinalDirection.North) Is c2 AndAlso c2.Neighbor(CardinalDirection.South) Is c1 Then Return True
    End Function


    ' This series of functions winds its way back and forth
    ' across a group of cells in order to determine the overall bounds 
    ' for what will be a new area.
    Private Function DownAndRight(ByRef sec As Area) As Boolean
        Dim width As Int32 = sec.AreaWidth
        Dim height As Int32 = sec.AreaHeight

        Debug.Assert(width = height)

        Dim cells As New Generic.List(Of Cell)
        Dim prevCell As Cell = Nothing : Dim newcell As Cell = Nothing
        Dim c As Cell = sec.NWCell

        Debug.Assert(c IsNot Nothing)
        Debug.Assert(Not TypeOf (c) Is NullCell)


        For i As Int32 = 1 To height
            newcell = c.Neighbor(CardinalDirection.West)
            If Not IsTwoWayConnection(c, newcell) Then Return False

            If i > 1 Then
                If Not IsTwoWayConnection(newcell, prevCell) Then Return False
            End If
            cells.Add(newcell)
            prevCell = newcell
            If i < height Then c = c.Neighbor(CardinalDirection.South)
        Next

        ' still here
        If Not IsTwoWayConnection(c, c.Neighbor(CardinalDirection.South)) Then Return False

        c = c.Neighbor(CardinalDirection.South)
        prevCell = newcell
        newcell = c.Neighbor(CardinalDirection.West)
        If Not IsTwoWayConnection(newcell, prevCell) Then Return False
        If Not IsTwoWayConnection(c, newcell) Then Return False

        cells.Add(c)
        cells.Add(newcell)
        prevCell = c

        c = sec.SWCell.Neighbor(CardinalDirection.East)

        For i As Int32 = 2 To width
            newcell = c.Neighbor(CardinalDirection.South)
            If Not IsTwoWayConnection(c, newcell) Then Return False
            If Not IsTwoWayConnection(newcell, prevCell) Then Return False

            cells.Add(newcell)
            prevCell = newcell
            c = c.Neighbor(CardinalDirection.East)
        Next

        sec.addCell(cells.ToArray)
        sec.NWCell = cells(0) 'first sell is new NWCell
        Return True
    End Function


    Private Function RightAndUp(ByRef sec As Area) As Boolean
        Dim width As Int32 = sec.AreaWidth
        Dim height As Int32 = sec.AreaHeight

        Debug.Assert(width = height)

        Dim cells As New Generic.List(Of Cell)
        Dim prevCell As Cell = Nothing : Dim newcell As Cell = Nothing
        Dim c As Cell = sec.SWCell

        If c Is Nothing Then Return False
        If TypeOf c Is NullCell Then Return False

        For i As Int32 = 1 To height
            newcell = c.Neighbor(CardinalDirection.South)
            If Not IsTwoWayConnection(c, newcell) Then Return False

            If i > 1 Then
                If Not IsTwoWayConnection(newcell, prevCell) Then Return False
            End If
            cells.Add(newcell)
            prevCell = newcell
            If i < height Then c = c.Neighbor(CardinalDirection.East)
        Next

        ' still here
        If Not IsTwoWayConnection(c, c.Neighbor(CardinalDirection.East)) Then Return False

        c = c.Neighbor(CardinalDirection.East)
        prevCell = newcell
        newcell = c.Neighbor(CardinalDirection.South)
        If Not IsTwoWayConnection(newcell, prevCell) Then Return False
        If Not IsTwoWayConnection(c, newcell) Then Return False

        cells.Add(c)
        cells.Add(newcell)
        prevCell = c

        c = sec.SECell.Neighbor(CardinalDirection.North)
        For i As Int32 = 2 To width
            newcell = c.Neighbor(CardinalDirection.East)
            If Not IsTwoWayConnection(c, newcell) Then Return False
            If Not IsTwoWayConnection(newcell, prevCell) Then Return False

            cells.Add(newcell)
            prevCell = newcell
            c = c.Neighbor(CardinalDirection.North)
        Next

        sec.addCell(cells.ToArray)
        ' NWCell stays the same
        Return True
    End Function


    Private Function UpAndLeft(ByRef sec As Area) As Boolean
        Dim width As Int32 = sec.AreaWidth
        Dim height As Int32 = sec.AreaHeight

        Debug.Assert(width = height)

        Dim cells As New Generic.List(Of Cell)
        Dim prevCell As Cell = Nothing : Dim newcell As Cell = Nothing
        Dim c As Cell = sec.SECell

        If c Is Nothing Then Return False
        If TypeOf c Is NullCell Then Return False

        For i As Int32 = 1 To height
            newcell = c.Neighbor(CardinalDirection.East)
            If Not IsTwoWayConnection(c, newcell) Then Return False

            If i > 1 Then
                If Not IsTwoWayConnection(newcell, prevCell) Then Return False
            End If
            cells.Add(newcell)
            prevCell = newcell
            If i < height Then c = c.Neighbor(CardinalDirection.North)
        Next

        ' still here
        If Not IsTwoWayConnection(c, c.Neighbor(CardinalDirection.North)) Then Return False

        c = c.Neighbor(CardinalDirection.North)
        prevCell = newcell
        newcell = c.Neighbor(CardinalDirection.East)
        If Not IsTwoWayConnection(newcell, prevCell) Then Return False
        If Not IsTwoWayConnection(c, newcell) Then Return False


        cells.Add(newcell)
        cells.Add(c) '<-- in the case of 1x1 (single cell) area, its important that this cell is added last since it will be last in cells collection and last = new NW cell 
        prevCell = c

        c = sec.NECell.Neighbor(CardinalDirection.West)
        For i As Int32 = 2 To width
            newcell = c.Neighbor(CardinalDirection.North)
            If Not IsTwoWayConnection(c, newcell) Then Return False
            If Not IsTwoWayConnection(newcell, prevCell) Then Return False

            cells.Add(newcell)
            prevCell = newcell
            c = c.Neighbor(CardinalDirection.West)
        Next

        sec.addCell(cells.ToArray)
        sec.NWCell = cells(cells.Count - 1) ' last cell is new NWCell
        Return True
    End Function


    Private Function LeftAndDown(ByRef sec As Area) As Boolean
        Dim width As Int32 = sec.AreaWidth
        Dim height As Int32 = sec.AreaHeight

        Debug.Assert(width = height)

        Dim cells As New Generic.List(Of Cell)
        Dim prevCell As Cell = Nothing : Dim newcell As Cell = Nothing
        Dim c As Cell = sec.NECell

        If c Is Nothing Then Return False
        If TypeOf c Is NullCell Then Return False

        For i As Int32 = 1 To height
            newcell = c.Neighbor(CardinalDirection.North)
            If Not IsTwoWayConnection(c, newcell) Then Return False

            If i > 1 Then
                If Not IsTwoWayConnection(newcell, prevCell) Then Return False
            End If
            cells.Add(newcell)
            prevCell = newcell
            If i < height Then c = c.Neighbor(CardinalDirection.West)
        Next

        ' still here
        If Not IsTwoWayConnection(c, c.Neighbor(CardinalDirection.West)) Then Return False

        c = c.Neighbor(CardinalDirection.West)
        prevCell = newcell
        newcell = c.Neighbor(CardinalDirection.North)
        If Not IsTwoWayConnection(newcell, prevCell) Then Return False
        If Not IsTwoWayConnection(c, newcell) Then Return False


        cells.Add(newcell) '<-- imperative this gets added ahead in this case since we compute the new NW cell as cells.count \ 2 
        cells.Add(c)
        prevCell = c

        c = sec.NWCell.Neighbor(CardinalDirection.South)
        For i As Int32 = 2 To width
            newcell = c.Neighbor(CardinalDirection.West)
            If Not IsTwoWayConnection(c, newcell) Then Return False
            If Not IsTwoWayConnection(newcell, prevCell) Then Return False

            cells.Add(newcell)
            prevCell = newcell
            c = c.Neighbor(CardinalDirection.South)
        Next

        sec.addCell(cells.ToArray)
        sec.NWCell = cells(cells.Count \ 2) ' middle index cell is new cell
        Return True
    End Function

    ''' <summary>
    ''' First, recall that area generation only occurs AFTER all cells for all
    ''' pages have been created.  In other words, after the initial NavNet
    ''' generation is complete.
    ''' 
    ''' However, for large worlds with lots of pages, it's still impractical to try
    ''' and keep all pages loaded to generate all areas for all pages.  Because we
    ''' have to dynamically load/unload pages during overall Area generation
    ''' we need to specify in the file for whether a given page has had its
    ''' edges and/or links already generated.
    ''' 
    ''' If only the edges are generated and not the links, then prior to unloading
    ''' an unfinished page, we must save the areas (which are always fully complete)
    ''' and the edges (including the cells that make up that edge).  
    ''' 
    ''' The links are stored with the areas and is the same in either case. 
    ''' </summary>
    ''' <param name="path"></param>
    ''' <remarks></remarks>
    Private Sub Save(ByVal path As String, ByVal region As Region)
        ' save the areas 
        Dim fs As IO.FileStream = Nothing
        Dim writer As IO.BinaryWriter = Nothing
        Try
            fs = IO.File.Create(path)
            writer = New IO.BinaryWriter(fs)

            writer.Write(region.EdgesCreated)
            If region.EdgesCreated Then
                writer.Write(region.PortalsCreated)
            End If

            If region.EdgesCreated AndAlso Not region.PortalsCreated Then
                'write the edges, however since the links arent created
                ' then we must also be able to store the cells and reload them
                ' for those edges and restore the references to neighbors...

                ' so i think we need to have a reference to the _cg.SubNet which 
                ' would maintaint the list of cells.
                writer.Write(region.Edges.Count)
                For Each e As Edge In region.Edges.Values
                    e.Write(writer)
                Next
            End If

            writer.Write(region.Areas.Count)
            For Each sec As Area In region.Areas
                sec.Write(writer)
            Next

        Catch ex As Exception
        Finally
            If writer IsNot Nothing Then writer.Close()
            If fs IsNot Nothing Then fs.Close()
        End Try
    End Sub

    Public Sub Load(ByVal path As String)
        'Dim fs As IO.FileStream = Nothing
        'Dim reader As IO.BinaryReader = Nothing

        'Try
        '    fs = New IO.FileStream(path, IO.FileMode.Open)
        '    reader = New IO.BinaryReader(fs)

        '    Dim count As Int32 = reader.ReadInt32

        '    For i As Int32 = 0 To count - 1
        '        Dim c As New Cell
        '        c.Read(reader)
        '        sg.allCells.Add(c.Position, c)
        '    Next
        'Catch ex As Exception
        '    Debug.WriteLine("CGBuilder.GenerateNet() -- " & ex.Message)
        'Finally
        '    If reader IsNot Nothing Then reader.Close()
        '    If fs IsNot Nothing Then fs.Close()
        'End Try

        '' restore the neighbor references for all cells EXCEPT cell pointers.
        'For Each c As Cell In sg.allCells.Values
        '    For i As Int32 = 0 To 3
        '        If Not NullCell.IsNullCell(c.NeighborID(i)) Then
        '            c.Neighbor(i) = sg.allCells(c.NeighborID(i))
        '        Else
        '            c.Neighbor(i) = New NullCell
        '        End If
        '    Next
        'Next
    End Sub
End Class
