
' Why do links (portals) in the gamasutra connectivity graph article use 3d bounding boxes to describe link (portal) volume?
' Actually the gamasutra article presumes that the AI will be using the cg to pathfind in a world that is also 
' utilizing conventional collision detection at runtime.  So, the code to determine a path for an AI to navigate 
' via a link (and thus avoiding hitting any obstacles including the edges of doorways) make use of the 3d 
' "doorway" of the link to properly orient and define its path through to the other area.  In fact, since 
' the mouths of both sides of the link (portal) extend out from the actual collidable object, guiding an npc to
' the next area via the link guarantees they can then make it through the door.  

' For lux game server, they arent completely entirely necessary.  All one really needs to know is a players current area 
' location and from that, when a player moves out of a area, you loop through all links in the list and find 
' if the player traversed into a area that is allowed.  If still not found, you can optionally continue to check 
' thru links up to a max distance from previous location and determine if they made it through a series of links 
' inbetween last check. 

' All we're verifying is that a client _can traverse_ from their previous location to the next given the available 
' links they have access to from their previous area location.  Remember,moving within areas is guaranteed
' to be free of collision so if they are in a area,and move to another area next check... we simply need to verify 
' that a link path existed (and alternatively if we want more complex interaction... check whether their path clipped 
' any corners, but i doubt the need for this). 

' NOTE: if there is a tree in a user's path, and the extrapolated position has them collide (no path through that link)
' then you can send them a forced update.  

' So to recap, two methods.  We can verify valid link connectivity from previous to current location.  And we can also
' on top of this, verify the path they took was through a link and didnt clip.  Thats the tricky part.

' For lux npc server, we "could" use this 3d info to help navigate better... to properly funnel creatures through
' links and not just clip through the doorways any which way. So, we will go with 3d and only if we have serious 
' mem issues game server side will we modify it to use a 2d version of links.

Public Class Link : Implements IBinaryWriter

    Private _destination1 As Area
    Private _destination2 As Area
    Private _bounds As BoundingBox
    Private _isTwoWay As Boolean

    Public Sub New(ByVal d1 As Area, ByVal d2 As Area, ByVal MakeTwoWay As Boolean)
        If d1 Is Nothing OrElse d2 Is Nothing Then Throw New ArgumentNullException
        _destination1 = d1
        _destination2 = d2
        _isTwoWay = MakeTwoWay
    End Sub

    Public Property BBox() As BoundingBox
        Get
            Return _bounds
        End Get
        Set(ByVal value As BoundingBox)
            _bounds = value
        End Set
    End Property

    Private ReadOnly Property IsTwoWayPortal() As Boolean
        Get
            Return _isTwoWay
        End Get
    End Property

    ''' <summary>
    ''' Given a departure area, returns the destination area a link will take you too.
    ''' </summary>
    ''' <param name="currentArea"></param>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Destination(ByVal currentArea As Area) As Area
        Get
            If currentArea Is _destination1 Then
                Return _destination2
            ElseIf IsTwoWayPortal Then
                'If currentArea Is _destination2 Then Return _destination1
                Return _destination1
            Else
                Return _destination1
            End If
        End Get
    End Property

    Public Sub Read(ByVal reader As System.IO.BinaryReader) Implements IBinaryWriter.Read

    End Sub

    Public Sub Write(ByVal writer As System.IO.BinaryWriter) Implements IBinaryWriter.Write

        writer.Write(_isTwoWay)

    End Sub
End Class
