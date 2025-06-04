Option Strict Off
Option Explicit On

Imports MTV3D65

' TODO:
' A) Consider what would be needed for use as a client side area/link data structure for assisting in indoor
'   visibility culling and camera constraining.
'   -for instance, for camera constraining with the tiny areas sometimes clumping together, it could impact
'   the camera path.  i suppose as long as the camera was both constrained to any area, it would only need to close
'   in on the player when it loses direct line of sight of the whole player.  then it would need to raise/lower and/or close in.
'   - also, there would definetly need to be ceiling info in the areas... you couldnt have continguous areas that
' had different height ceilings. they would need to be broken up here
' also for indoors if there's an arched ceiling, some kind of check such that a single contingous area could still be made
' but constrained at the highest point that all could commonly share.

' B) Better more comprehensive collision checks for the generation algo (things like ceiling clearance checks, etc.
'   - for instance the down ray cast wont prevent walking through walls that are 2d.  the ray would miss the check
'   - drop test so that players cant walk off things too high (optional setting <=0 = no height drop test )

' C) double check im not storing too much data in the classes directly instead of computing at runtime (to keep memory lower)
'   - e.g. storing some neighbors who you can easily compute in the Get method rather than store directly
' double check the TwoWayPortal thing.  I believe the gamasutra article either had them all as two way, or made them all one way.
' also not sure mine works 100% in both directions.
' D) add code to enable/disable use of cg  or tv based collision in the test app to better verify things
' E) save/load of the data set 
' F) add toggle for enabling/disabling wireframes.
' G) fix the multiple area crossing lookahead stuff (see AreaHelper.CanTraverse())
' H) page to page stiching so links on one page boundary match up to links in the neighboring page
'       - first divide up the map with a quadtree.  Then each leaf node gets it CG generated seperately.  But then we need
'         a second pass to connect the cells between them (merge into links, etc). 
' I) make into DLL and intergrate with ag3t demo for testing
' J) add more options and tweaks to the generation settings
'   - max up height
'  - cell height/width settings
' K) add A* algorithm for pathfinding
' 
Friend Class Form1
    Inherits System.Windows.Forms.Form

    Dim TV As New TVEngine
    Public Scene As New TVScene
    Dim Bridge As TVMesh
    Dim Actor As TVActor
    Dim Land As TVLandscape
    Dim Tex As New TVTextureFactory
    Dim Inp As New TVInputEngine


    Dim Globals As New TVGlobals
    Dim Math As New TVMathLibrary

    Dim Finished As Boolean

    ' Information about the movement of the actor.
    Dim ActorPosition As TV_3DVECTOR
    Dim ActorDestination As TV_3DVECTOR
    Dim ActorDir As TV_3DVECTOR
    Dim Moving As Boolean

    Private _actorHeight, _actorWidth, _actorDepth, _actorMaxStepUpHeight As Single
    Private _mode As Int32 = 0 ' 0 = user camera, 1 = fixed camera
    Private _navnet As NavNet
    Private _netBuilder As NavNetBuilder
    Private _areaBuilder As AreaBuilder


    Private Sub GenerateNavNet()
        ' *.2 is to scale the height/width/depth since the actor bounding box is incorrectly return without taking scaling into account

        _actorHeight *= 0.2 ' NOTE: set this adjusted since actor bounding box incorrectly returns and also this is used to test collision of actor as it moves

        Dim min, max As MTV3D65.TV_3DVECTOR
        Land.GetBoundingBox(min, max)
        AreaHelper.CurrentLandscape = Land

        Dim box As BoundingBox = New BoundingBox(min, max)

        ' NOTE: You must merge the bounding volumes (in world coords) of all meshes in the land)
        Bridge.GetBoundingBox(min, max, False)
        Dim box2 As BoundingBox = New BoundingBox(min, max)
        box = BoundingBox.CombineBoundingBoxes(box, box2)

        '_cgBuilder = New NavNetBuilder(box, _actorWidth * 0.2 / 4, _actorDepth * 0.2 / 4, _actorHeight, _actorMaxStepUpHeight,_actorMaxStepUpHeight)
        _netBuilder = New NavNetBuilder(box, 4, 4, 40, 12, 12)
        _netBuilder.Clear()

        _netBuilder.GenerateNet()

        _areaBuilder = New AreaBuilder

        ' TODO: here actually we need to use the FixSeems2 type of code for managing loading/unloading 
        ' well, we could let .GenerateAreas manage all that too, and if so
        ' then we need to just pass in .GenerateAreas(_cgBuilder.SubNets) 
        ' all of them.  Because the AreaBuilder needs to be able to get references
        ' the actual loaded Cells.  It cannot "new" its own cells because those wont
        ' be the real ones.  The neighbor references wont be recreateable.

        ' also i think because the area builder needs to be able to manage the cgBuilder's loading/unloading
        ' of cell data, then maybe it needs a reference to the entire cgBuidler so it can call the save/load functions?
        ' Either that or we have something else dedicated to managing the entire process...

        ' if so, then it was probably a mistake to make "subgraphs" and "regions" in both cgBuilder and AreaBuilder
        ' since the overall manager could manage all of those seperately
        ' as arrays of cgBuilders and areaBuilders

        ' for i as int32 = 0 to _cgBuilder.SubNets.Length - 1
        '     _areaBuilder.GenerateAreas(i, _cgBuilder(i).Cells)
        _areaBuilder.GenerateAreas(box.Center, _netBuilder.Cells)

        ' Next
        _navnet = New NavNet(_areaBuilder.Areas(box.Center))

        ActorPosition.x = 150
        ActorPosition.z = 300
        ActorPosition.y = Land.GetHeight(ActorPosition.x, ActorPosition.z)

        _navnet.SetCurrentArea(ActorPosition)

    End Sub

    Public Sub Initialize()

        'Engine initialization.
        Me.Show()
        System.Windows.Forms.Application.DoEvents()

        TV.Init3DWindowed(Me.Picture1.Handle.ToInt32)
        TV.SetAngleSystem(CONST_TV_ANGLE.TV_ANGLE_DEGREE)
        Inp.Initialize(True, True)

        'Scene.SetRenderMode(CONST_TV_RENDERMODE.TV_LINE)

        'Objects loading.
        Actor = Scene.CreateActor
        Actor.Load("character.x")
        Actor.SetMaterial(0)
        Actor.PlayAnimation(50)
        Actor.SetScale(0.2, 0.2, 0.2)

        Dim min, max As TV_3DVECTOR
        Actor.GetBoundingBox(min, max, False)

        _actorHeight = max.y - min.y
        _actorWidth = max.x - min.x
        _actorDepth = max.z - min.z
        _actorMaxStepUpHeight = _actorHeight * 0.2 / 2


        Scene.SetBackgroundColor(0.7, 0.8, 1)

        'Load the bridge mesh, that the character can go upon.
        Bridge = Scene.CreateMeshBuilder
        Bridge.LoadXFile("brige.x")
        Bridge.SetMaterial(0)
        Bridge.SetCullMode(CONST_TV_CULLING.TV_BACK_CULL)

        'Landscape creation.
        Land = Scene.CreateLandscape("Landscape")
        Land.SetScale(1, 0.3, 1)
        Land.GenerateTerrain("track.jpg", CONST_TV_LANDSCAPE_PRECISION.TV_PRECISION_AVERAGE, 2, 2, 0, 0, True)
        Tex.LoadTexture("sand.jpg", "sand")
        Land.SetTexture(Globals.GetTex("sand"))

        Bridge.SetPosition(100, Land.GetHeight(100, 100), 100)

        'Camera settings
        Scene.SetCamera(400, 150, 350, 0, 150, 0)

        GenerateNavNet()
    End Sub

    Public Sub CheckMouse()

        ' do input mousepicking on the scene.
        Dim mousex, mousey As Integer
        Dim button As Short
        Inp.GetAbsMouseState(mousex, mousey, button)

        Dim Pick As TVCollisionResult
        If button Then
            Pick = Scene.MousePick(mousex, mousey, CONST_TV_OBJECT_TYPE.TV_OBJECT_LANDSCAPE Or CONST_TV_OBJECT_TYPE.TV_OBJECT_MESH, CONST_TV_TESTTYPE.TV_TESTTYPE_ACCURATETESTING)
            If Pick.IsCollision Then
                'the impact point is the new destination for the actor
                ActorDestination = Pick.GetCollisionImpact

                'compute his new direction (in 2d so remove the y argument)
                ActorDestination.y = ActorPosition.y
                ActorDir = Math.VNormalize(Math.VSubtract(ActorDestination, ActorPosition))
                Actor.LookAtPoint(ActorDestination)
                Actor.RotateY(180)
                Actor.SetAnimationID(1)
                Actor.PlayAnimation(50)
                Moving = True
            End If
        End If

        ' now do the actor moving.
        Dim LandHeight As Single
        Dim Coll As TV_COLLISIONRESULT
        Dim oldPosition As TV_3DVECTOR

        If Moving = True Then
            oldPosition = ActorPosition
            ActorPosition = Math.VAdd(ActorPosition, Math.VScale(ActorDir, TV.AccurateTimeElapsed * 0.08))

            ' The Important Part : Accurate Advanced Collision detection.

            ' Get the landscape point below the actor
            LandHeight = Land.GetHeight(ActorPosition.x, ActorPosition.z)

            ' GRAVITY TEST - Used to determine Actor's position.Y value. (not used to determine or restrict x,z at all)
            '                EDIT: Actually, depending on implementatin, to traverse to some areas might require the players user controls
            '                have them "crouched" or "jumping" to the neighboring area.  Not sure exactly how one would know\determine and test
            '                the reqts. at runtime.
            ' Do a vertical check from the player position + _actorHeight to the landscape.
            If Not Scene.AdvancedCollision(New TV_3DVECTOR(ActorPosition.x, ActorPosition.y + _actorHeight, ActorPosition.z), _
                New TV_3DVECTOR(ActorPosition.x, LandHeight, ActorPosition.z), Coll, CONST_TV_OBJECT_TYPE.TV_OBJECT_MESH, _
                CONST_TV_TESTTYPE.TV_TESTTYPE_ACCURATETESTING, 1) Then

                ' there is nothing else than landscape below the actor, so the actor must fall
                ActorPosition.y = ActorPosition.y - TV.AccurateTimeElapsed * 0.1
                If ActorPosition.y < LandHeight Then ActorPosition.y = LandHeight

            ElseIf Coll.vCollisionImpact.y < ActorPosition.y Then
                ' if impact is below actor, it must fall,
                ActorPosition.y = ActorPosition.y - TV.AccurateTimeElapsed * 0.1
                If ActorPosition.y < Coll.vCollisionImpact.y Then ActorPosition.y = Coll.vCollisionImpact.y
            Else
                'else it must go up if its not too high to step up on
                If (Coll.vCollisionImpact.y - ActorPosition.y) > _actorMaxStepUpHeight Then
                    ActorPosition = oldPosition
                    ' stop them from moving (note: if you wanted more sophistication, you could also test if the actor
                    ' crouched and retest the collision to see if they could move forward into a low tunnel beneath whatever is 
                    ' blocking their upper body
                    ActorDestination = ActorPosition
                Else
                    ActorPosition.y = Coll.vCollisionImpact.y
                End If
            End If


            '=========
            ' verify that the actor's position (which always includes the height) is valid based on its area location and the available exit links
            ' we might add 1 to height just to make sure we're not testing on the actual floor of a cell which may or may not cause the PointInBounds function some problems)

            ' NOTE: to test some aspects, we can a) set the climb height HIGHER after having generated the cg and then verifying that 
            ' attempting to climb higher at runtime is detected by the CG since no link should exist allowing the area change
            ' b) turning off collision detetion while walking entirely and then verifying we are stopped before walking through the walls/floors of the 
            ' platform
            ' c) if an optional "fall" heihg was set in the CG during creation, then we can also check jumping off from high points to see if they are not
            ' allowed.
            If _navnet.CurrentArea Is Nothing Then
                Debug.Print("Lost actor area.  Re-acquring.")
                _navnet.SetCurrentArea(oldPosition)
            End If

            If Not _navnet.Traverse(_navnet.CurrentArea, New MTV3D65.TV_3DVECTOR(oldPosition.x, oldPosition.y + 1, oldPosition.z), New MTV3D65.TV_3DVECTOR(ActorPosition.x, ActorPosition.y + 1, ActorPosition.z)) Then

                Debug.Print("player cant go there.  moving back")
                ActorPosition = oldPosition
                ActorDestination = ActorPosition
            End If
            '========

            ' if we are near the dest point, just stop it
            If Math.GetDistance2D(ActorPosition.x, ActorPosition.z, ActorDestination.x, ActorDestination.z) < 0.5 Then
                Actor.SetAnimationID(0)
                Actor.PlayAnimation(20)
                Moving = False
            End If
        End If
    End Sub

    Public Sub RenderAll()

        If _mode = 1 Then
            'put the camera above player
            Scene.SetCamera(ActorPosition.x, ActorPosition.y + 200, ActorPosition.z + 200, ActorPosition.x, ActorPosition.y, ActorPosition.z)
        End If

        'render
        TV.Clear()
        Land.Render()
        Bridge.Render()

        Actor.SetPosition(ActorPosition.x, ActorPosition.y, ActorPosition.z)
        Actor.Render()

        _navnet.draw()

        TV.RenderToScreen()
    End Sub

    Public Sub MainLoop()
        Do
            ' Render everything to the screen.
            System.Windows.Forms.Application.DoEvents()
            RenderAll()

            ' Do Input and physics check
            CheckMouse()
            CheckKeyboard()

        Loop Until Finished = True Or Inp.IsKeyPressed(CONST_TV_KEY.TV_KEY_ESCAPE) = True
        End
    End Sub

    Private Sub CheckKeyboard()
        If Inp.IsKeyPressed(CONST_TV_KEY.TV_KEY_1) = True Then
            Scene.GetCamera.RotateY(-TV.AccurateTimeElapsed * 0.02)
        End If

        If Inp.IsKeyPressed(CONST_TV_KEY.TV_KEY_2) = True Then
            Scene.GetCamera.RotateY(TV.AccurateTimeElapsed * 0.02)
        End If


        ' manage the camera
        If _mode = 0 Then
            If Inp.IsKeyPressed(CONST_TV_KEY.TV_KEY_LEFT) = True Then
                Scene.GetCamera.RotateY(-TV.AccurateTimeElapsed * 0.02)
            End If

            If Inp.IsKeyPressed(CONST_TV_KEY.TV_KEY_RIGHT) = True Then
                Scene.GetCamera.RotateY(TV.AccurateTimeElapsed * 0.02)
            End If

            If Inp.IsKeyPressed(CONST_TV_KEY.TV_KEY_UP) = True Then
                Scene.GetCamera.MoveRelative(TV.AccurateTimeElapsed * 0.2, 0, 0)
            End If

            If Inp.IsKeyPressed(CONST_TV_KEY.TV_KEY_DOWN) = True Then
                Scene.GetCamera.MoveRelative(-TV.AccurateTimeElapsed * 0.2, 0, 0)
            End If

            If Inp.IsKeyPressed(CONST_TV_KEY.TV_KEY_PAGEUP) = True Then
                Scene.GetCamera.MoveRelative(0, TV.AccurateTimeElapsed * 0.2, 0)
            End If

            If Inp.IsKeyPressed(CONST_TV_KEY.TV_KEY_PAGEDOWN) = True Then
                Scene.GetCamera.MoveRelative(0, -TV.AccurateTimeElapsed * 0.2, 0)
            End If
        End If
    End Sub
    Private Sub cmdQuit_Click(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles cmdQuit.Click
        Finished = True
    End Sub

    Private Sub Form1_Load(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles MyBase.Load
        Initialize()
        MainLoop()
    End Sub

    Private Sub Form1_FormClosing(ByVal eventSender As System.Object, ByVal eventArgs As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        Dim Cancel As Boolean = eventArgs.Cancel
        Dim UnloadMode As System.Windows.Forms.CloseReason = eventArgs.CloseReason
        Finished = True
        eventArgs.Cancel = Cancel
    End Sub
End Class