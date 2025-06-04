Public Class AreaHelper
    Public Shared UNKNOWNHEIGHT As Single = -9999999.0F
    Public Shared CurrentLandscape As MTV3D65.TVLandscape ' temp til we get function to search for proper lands


    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="currentPosition"></param>
    ''' <param name="tryPosition"></param>
    ''' <param name="partition"></param>
    ''' <param name="cellHeight"></param>
    ''' <param name="maxStepHeight"></param>
    ''' <returns></returns>
    ''' <remarks>NOTE: tryPosition is passed byRef since we can modify its Y value.</remarks>
    Public Shared Function CanTraverse(ByVal currentPosition As MTV3D65.TV_3DVECTOR, ByRef tryPosition As MTV3D65.TV_3DVECTOR, ByVal partition As BoundingBox, ByVal cellHeight As Single, ByVal maxStepHeight As Single) As Boolean
        Dim CResult As MTV3D65.TV_COLLISIONRESULT
        Dim Scene As New MTV3D65.TVScene

        Dim land As MTV3D65.TVLandscape = CurrentLandscape ' temp hack.  just reference currentlandscape

        'TODO: first get the landscapein question

        tryPosition.y = currentPosition.y '  ' I think its been a big mistake to use the landscape Height for tryPosition.Y 

        ' if the tryPosition is within the worldbounds then continue , else its clearly going to be a falase
        ' VERY IMPORTANT!!!  It's very important that the partition bound take into account all meshes and NOT just landscapes
        ' or meshes that result in picks with a higher Y value than the highest land triangle, will be treated as out of bounds
        ' and ignored!!!! 
        If BoundingBox.PointInBounds(partition, tryPosition) Then
            Dim landHeight As Single = land.GetHeight(tryPosition.x, tryPosition.z)

            ' Do a vertical check from the player's current position + _actorHeight to the landscape.
            If Not Scene.AdvancedCollision(New MTV3D65.TV_3DVECTOR(tryPosition.x, currentPosition.y + cellHeight, tryPosition.z), _
                New MTV3D65.TV_3DVECTOR(tryPosition.x, landHeight, tryPosition.z), CResult, MTV3D65.CONST_TV_OBJECT_TYPE.TV_OBJECT_MESH, _
                MTV3D65.CONST_TV_TESTTYPE.TV_TESTTYPE_ACCURATETESTING, 1) Then
                ' there is nothing else than landscape below so we keep TryPosition.Y which already has that position
                ' No mesh collision.  Nothing to do here, please move along.
                Debug.Assert(currentPosition.y + cellHeight > landHeight)
                tryPosition.y = landHeight
                Return True

            ElseIf CResult.vCollisionImpact.y <= tryPosition.y Then
                ' if impact is below this cell, then we place the cell ontop of whatever mesh it impacted
                ' TODO: we might want to add another check here to see if the distance below is > death fall height and then
                ' dont allow users to move into areas where if they did, they'd fall to their death... or perhaps even fall
                ' onto an area of the map they will not be able to get out of. ##### Important Consideration ######
                tryPosition.y = CResult.vCollisionImpact.y
                Return True
            ElseIf (CResult.vCollisionImpact.y - currentPosition.y) <= maxStepHeight Then
                'Here the impact point is higher than the terrain, but as long as its not too high no problem
                tryPosition.y = CResult.vCollisionImpact.y
                Return True
            Else
                ' too high, cannot create a cell here. 
                '  (note: if you wanted more sophistication, you could also test if the actor
                ' crouched and retest the collision to see if they could move forward into a low tunnel beneath whatever is 
                ' blocking their upper body
                Return False
            End If
        End If
        Return False
    End Function

    Public Shared Function GetHeight(ByVal x As Single, ByVal z As Single) As Single
        Return CurrentLandscape.GetHeight(x, z)
    End Function

    Public Shared Function GetDistance3d(ByVal v1 As MTV3D65.TV_3DVECTOR, ByVal v2 As MTV3D65.TV_3DVECTOR) As Single
        Dim dx, dy, dz As Single
        With v1
            dx = .x - v2.x
            dy = .y - v2.y
            dz = .z - v2.z
        End With
        Return CSng(System.Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz)))
    End Function
End Class
