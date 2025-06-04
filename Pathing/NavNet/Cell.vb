

' cell edges (we call them edges because our cells are 2-d and thus have a 2-d "edge" border
' with neighboring cells and not a 3-d face border because these cells are not 3-d)
' are the building blocks of both our areas and links
' they are only used to generate these structures pre-process and not during game runtime.
' cells merged = areas.   cell edges merged or alone = 2d links.
Public Class Cell : Implements IBinaryWriter

    Protected _neighborID(0 To 3) As MTV3D65.TV_3DVECTOR
    Protected _neighbors(0 To 3) As Cell
    Protected _areaID As Int32 ' area ID's are always local to the page.  so you can have two areas with the same id in different pages of the map.
    Protected _boundingBox As BoundingBox
    Protected _position As MTV3D65.TV_3DVECTOR
    Protected _height, _width, _depth As Single

    Public Sub New()

    End Sub
    Public Sub New(ByVal position As MTV3D65.TV_3DVECTOR, ByVal width As Single, ByVal depth As Single, ByVal height As Single)

        _boundingBox = CreateBoundingBox(position, width, depth, height)
        _height = height : _width = width : _depth = depth
        _position = position

        ' NOTE: because of single precision rounding errors, you CANNOT / MUST NOT use
        ' _boundingBox.Center() in place of a call to Cell.Position!  Always use Cell.Position 
        ' and save it/load it independantly rather than trying to rely on _boundingBox.Center
        ' Indeed, its better to save the _position, _height, _width and _depth and use that to recreate the bounding volume
        'Debug.Assert(_height = _boundingBox.Height) '<-- will occassionally fail
        'Debug.Assert(_width = _boundingBox.Width)   '<-- will occassionally fail
        'Debug.Assert(_depth = _boundingBox.Depth)   '<-- will occassionally fail
    End Sub

    Protected Function CreateBoundingBox(ByVal position As MTV3D65.TV_3DVECTOR, ByVal width As Single, ByVal depth As Single, ByVal height As Single) As BoundingBox
        '_boundingBox.Min.x = _position.x - width / 2d
        '_boundingBox.Max.x = _position.x + width / 2d
        '_boundingBox.Min.y = _position.y
        '_boundingBox.Max.y = _position.y + height
        '_boundingBox.Min.z = _position.z - depth / 2d
        '_boundingBox.Max.z = _position.z + depth / 2d
        Return New BoundingBox(New MTV3D65.TV_3DVECTOR(position.x - width / 2d, position.y, position.z - depth / 2d), New MTV3D65.TV_3DVECTOR(position.x + width / 2d, position.y + height, position.z + depth / 2d))
    End Function

    Public Overridable ReadOnly Property Position() As MTV3D65.TV_3DVECTOR
        Get
            Return _position
        End Get
    End Property

    Public Overridable ReadOnly Property NeighborID(ByVal index As Int32) As MTV3D65.TV_3DVECTOR
        Get
            Return _neighborID(index)
        End Get

    End Property


    Public Overridable Property Neighbor(ByVal index As CardinalDirection) As Cell
        Get
            Return _neighbors(index)
        End Get
        Set(ByVal value As Cell)
            _neighbors(index) = value
        End Set
    End Property

    Public Overridable Property BBox() As BoundingBox
        Get
            Return _boundingBox
        End Get
        Set(ByVal box As BoundingBox)
            _boundingBox = box
        End Set
    End Property

    Public Overridable Sub Combine(ByVal c As Cell)
        _boundingBox = BoundingBox.CombineBoundingBoxes(_boundingBox, c.BBox)
        ' _position = _boundingBox.Center
    End Sub

    Public Overridable Property AreaID() As Int32
        Get
            Return _areaID
        End Get
        Set(ByVal value As Int32)
            _areaID = value
        End Set
    End Property

    Public Overridable Sub Draw()
        Dim aqua As Integer = globals.Globals.RGBA(0, 1, 1, 1)
        ' draw the bounding box for this area 
        _boundingBox.Draw(aqua)
    End Sub

    Public Overridable Sub Read(ByVal reader As System.IO.BinaryReader) Implements IBinaryWriter.Read
        Try
            _areaID = reader.ReadInt32
            _position.x = reader.ReadSingle
            _position.y = reader.ReadSingle
            _position.z = reader.ReadSingle

            '_position = New MTV3D65.TV_3DVECTOR(reader.ReadSingle, reader.ReadSingle, reader.ReadSingle)
            _height = reader.ReadSingle
            _width = reader.ReadSingle
            _depth = reader.ReadSingle
            _boundingBox = CreateBoundingBox(_position, _width, _depth, _height)

            ' read in the id's for the neighbors?
            ' NOTE: we shouldnt try to use any callback or anything to get the reference
            ' immediately because we simply cant be sure the neighbor is already loaded.
            ' and trying some recursive mess of loading the ones that need loading (requiring
            ' the loaded one to have all its neighbors fully loaded first, etc) is probably
            ' not desireable.  
            ' So right now, we're looking at loading the id's and then having to loop through
            ' and assign the references after they're all loaded...
            ' this does seem to call into question the need for CellPointer?

            ' Recall that in GVD I used to make sure that any "ref" in a file (same with X3D) 
            ' could only reference nodes that were already "def'd" (defined).  

            For i As Int32 = 0 To 3
                Dim dir As CardinalDirection = DirectCast(reader.ReadInt32, CardinalDirection)
                _neighborID(dir) = New MTV3D65.TV_3DVECTOR(reader.ReadSingle, reader.ReadSingle, reader.ReadSingle)
            Next
        Catch

        End Try
    End Sub

    Public Overridable Sub Write(ByVal writer As System.IO.BinaryWriter) Implements IBinaryWriter.Write
        Try
            writer.Write(_areaID)
            writer.Write(_position.x)
            writer.Write(_position.y)
            writer.Write(_position.z)
            writer.Write(_height)
            writer.Write(_width)
            writer.Write(_depth)

            ' for each neighbor, store the cardinal id and the position vector
            For i As Int32 = 0 To 3
                writer.Write(i)
                writer.Write(_neighbors(i).Position.x)
                writer.Write(_neighbors(i).Position.y)
                writer.Write(_neighbors(i).Position.z)
            Next

        Catch

        End Try
    End Sub
End Class


