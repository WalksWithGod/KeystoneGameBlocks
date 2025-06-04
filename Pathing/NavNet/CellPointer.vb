
''' <summary>
''' A cell that points to another cell (usually one from a neighboring page)
''' If _cell reference is not set, then an attempt is made to obtain that reference.
''' This requires and assumes that the neighboring page is loaded into memory.
''' NOTE: CellPointers as well as regular cells are only used during generation.
''' In game at runtime, we strictly use areas and links although when the areas
''' and links for a specific page are loaded, the link that points to a area
''' in another page, will use a "AreaPointer" instead of the actual reference.
''' This way these neighboring area references can be given a change to be obtained.
''' </summary>
''' <remarks></remarks>
Public Class CellPointer : Inherits Cell

    Delegate Function GetCell(ByVal position As MTV3D65.TV_3DVECTOR) As Cell
    Private _getCell As GetCell
    Private _cell As Cell

    ' used when deserializing 
    Public Sub New(ByVal gc As GetCell)
        _getCell = gc
    End Sub

    ' used when instancing the cell and knowing hte reference in advance
    Public Sub New(ByVal cell As Cell)
        MyBase.New(cell.Position, cell.BBox.Width, cell.BBox.Depth, cell.BBox.Height)
        _cell = cell
        Debug.Assert(_height = cell.BBox.Height AndAlso _width = cell.BBox.Width AndAlso _depth = cell.BBox.Depth)
    End Sub

    Private ReadOnly Property ID() As MTV3D65.TV_3DVECTOR
        Get
            Return _position
        End Get
    End Property

    Private ReadOnly Property Cell() As Cell
        Get
            If _cell Is Nothing Then
                _cell = _getCell(ID)
            End If
            Return _cell
        End Get
    End Property

    Public Overrides ReadOnly Property Position() As MTV3D65.TV_3DVECTOR
        Get
            Return Cell.Position
        End Get
    End Property

    Public Overrides Property Neighbor(ByVal index As CardinalDirection) As Cell
        Get
            Return Cell.Neighbor(index)
        End Get
        Set(ByVal value As Cell)
            Cell.Neighbor(index) = value
        End Set
    End Property

    Public Overrides Property BBox() As BoundingBox
        Get
            Return Cell.BBox
        End Get
        Set(ByVal box As BoundingBox)
            Cell.BBox = box
        End Set
    End Property

    Public Overrides Sub Combine(ByVal c As Cell)
        Cell.BBox = BoundingBox.CombineBoundingBoxes(Cell.BBox, c.BBox)
        ' _position = _boundingBox.Center
    End Sub

    Public Overrides Property AreaID() As Int32
        Get
            Return Cell.AreaID
        End Get
        Set(ByVal value As Int32)
            Cell.AreaID = value
        End Set
    End Property

    Public Overrides Sub Draw()
        Dim aqua As Integer = globals.Globals.RGBA(0, 1, 1, 1)
        ' draw the bounding box for this area 
        Cell.BBox.Draw(aqua)
    End Sub

    Public Overrides Sub Read(ByVal reader As System.IO.BinaryReader)
        Try
            _areaID = reader.Read
            _position = New MTV3D65.TV_3DVECTOR(reader.ReadSingle, reader.ReadSingle, reader.ReadSingle)
        Catch

        End Try
    End Sub

    Public Overrides Sub Write(ByVal writer As System.IO.BinaryWriter)
        Try
            writer.Write(_areaID)
            writer.Write(ID.x)
            writer.Write(ID.y)
            writer.Write(ID.z)
        Catch

        End Try
    End Sub
End Class

