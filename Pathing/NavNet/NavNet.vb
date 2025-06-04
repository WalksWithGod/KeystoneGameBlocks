Public Enum CardinalDirection As Integer
    West = 0
    East
    North
    South
End Enum


''' <summary>
''' This is the primary class used during runtime (as opposed to initial generation of the cells, areas, edges and links.
''' The user controlled player has no use for the NavNet at all.   The client can rely on local physics and collision code.
''' Client Side AI controlled players however, can use the CG to pathfind in the world.  The Areas and Portals take the place of
''' predefined "paths".
''' Server Side AI players as well as Server Side Human Player boundary (cheating) enforcement can also make use of the CG.
''' Thus, the CG sits outside of any other Spatial hierarchy such as an Octree or Quadtree.  Those things are only useful when generating our CG
''' in our Editor.
''' 
''' At runtime, our only additional concern is paging in/out CG data in Single Player games (for network server controlled AI, the client 
''' has no use for the CG at all) as needed.  Or even when generating the preliminary temporary Cell data... we'd like to be able to page some of it out
''' or to even be able to generate CG data for entire regions and then link them because clearly for a huge World of Warcraft like game, you want to design regions
''' one at a time and then to merge them because otherwise it's too much data thrashing.
''' 
''' But the biggest problem with this approach is that the CG is generated based on a single Entity's demensions.  A huge rancor should not be using
''' a CG for a tiny human, although you could probably get away with allowing a tinier creature to use a larger creature's CG however realizing that 
''' these tiny creatures will be limited from going into tiny areas that it should visibly be able to pass, but because the larger creature who's CG it's using
''' cannot.  The scary solution (only because of possible memory constraints) is to create multiple CG's for different sized creatures.  That said, one should 
''' remember that Game Programmming isn't about creating "perfect" systems.   It's about fullfilling a design requirement and that's all.
''' 
''' Todo: i believe i should implement a similar "subgraph" structure for each page.  The thing is
''' sure a single page can reference its own subgraph info, but there needs to be a common point
''' where at runtime, neighboring pages can reconstruct the links between them.
''' </summary>
''' <remarks></remarks>
Public Class NavNet


    Private _areas() As Area
    Private _currentArea As Area
    'Private _bbox As BoundingBox

    Public Sub New(ByVal areas() As Area)
        _areas = areas
    End Sub


    Public Property Areas() As Area()
        Get
            Return _areas
        End Get
        Set(ByVal value() As Area)
            _areas = value
        End Set
    End Property

    Public Sub SetCurrentArea(ByVal actorPosition As MTV3D65.TV_3DVECTOR)
        _currentArea = AreaPicker.FindArea(_areas, actorPosition)
        Debug.Assert(_currentArea IsNot Nothing)
    End Sub

    Public ReadOnly Property CurrentArea() As Area
        Get
            Return _currentArea
        End Get
    End Property
    'The bounding volume of the entire world
    'Public ReadOnly Property BoundVolume() As BoundingBox
    '    Get
    '       
    '        Return _bbox
    '    End Get
    'End Property

    Public Sub Draw()
        If _currentArea IsNot Nothing Then
            _currentArea.Draw()
        End If

        'For Each s As Area In _cg.Areas
        '    s.Draw()
        'Next
    End Sub


    ' TODO: There might be problems here when needing to traverse across a page boundary.  May need to move this function
    ' out to the CGRuntimeManager
    ' this determines that given a players current area and position,they can traverse to a new position without colliding
    ' this should be done per frame on the server as it updates player's positions.  Note: This is a very cheap computation
    ' since unlike "real" collision detection where you have to check against every mesh in the area, here we only
    ' have to verify that the destination is still within their current area _or_ that a link to another area exists.

    ' NOTE: The only problem is when a player manages to skip multiple areas inbetween calls to CanTraverse in which case
    ' we require some recursive calls to perhaps an X depth, however even so... we can know the heading and can extrapolate
    ' positions and if we lose synch, force the player back to last known good position.

    ' NOTE: Actually one thing we could do is to solve ahead X distance through the combination of links and in this way
    ' always have a list of areas the user could reach given a worst case time lag allowance.  we can run this solver
    ' constantly for all players in a seperate thread (no need to do this for npc's).  Remember, these are extremely cheap tests
    ' since its all just PointInBox type tests and if you consider how frustum culling works, its the same thing and we know
    ' that frustum culling can handle thousands of items per frame at many many frames per second.  this will be even less work 
    ' and plus there's no rendering of geometry and textures, etc.  
    Public Function Traverse(ByVal currentArea As Area, ByVal Position As MTV3D65.TV_3DVECTOR, ByVal Destination As MTV3D65.TV_3DVECTOR) As Boolean
        'Const max_distance As Single = 10

        Debug.Assert(currentArea.ContainsPoint(Position), "CurrentArea and Position not sychronized.")
        Dim distance As Single = AreaHelper.GetDistance3d(Position, Destination)

        ' TODO: discovered the bug that has been screwing up traversal WITHIN a area when
        ' within that area there are for instance meshes with different heights. 
        ' so when clicking, you could end up with a "Destination" vector that 
        ' may include a Y value that is outside of the bounds of the current area.
        If currentArea.ContainsPoint(Destination) Then
            _currentArea = currentArea
            Return True
        Else
            ' based on a future destination we determine if they can mvoe there
            ' can really only loop through them since for any given area, a players
            ' heading  can only help us narrow down the choices minimally so its probably jsut faster to brute force it
            'TODO: eventually also test for proper pathing thru the area, but for now simply just check connectivity.
            For Each p As Link In currentArea.Links
                If p.Destination(currentArea).ContainsPoint(Destination) Then
                    _currentArea = p.Destination(currentArea)
                    Return True
                End If
            Next
            ' still here... thats not good.  we will need to recursively verify links within links
            ' up to the max distance the player could possibly have traveled inbetween checks.

        End If

        Return False
    End Function

    Public Sub Save(ByVal path As String)

        ' write the graph count

        ' for each graph, write the areas


    End Sub

    Public Sub Load(ByVal path As String)

    End Sub
End Class
