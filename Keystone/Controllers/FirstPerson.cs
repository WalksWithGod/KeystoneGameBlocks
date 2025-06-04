//using System;
//using Keystone.Entities;

//namespace Keystone.Controllers
//{
//    public class FirstPerson : InputController
//    {

//        protected override bool HandleMouseMove(global::System.Drawing.Point xyDelta)
//        {
//            throw new NotImplementedException();
//        }

////' When the user presses a key or mouse button, ctrlrFirstPerson checks its keyMap object
////' to see if that key is bound.
////'
////' If the key is not bound, its ignored.
////' If the key is bound, our ctrlFirstPerson invokes the virtual machine to 
////' run the command (commands are either intrinsic or user scripts)

////' our interpreter parses the scripts at run-time and eventually calls function pointers that execute
////' our entity state modifying action commands in this controller class.  Very elegant. 
////' Adding different control implementation to an entity or maybe adding an AI controller instead
////' is as simple as implementing a new controller object and then referencing the Entity to be
////' controlled by it.  So there is no need to mess with the Entity class, just the controller.
////'

//        //'=====================================
//        //' Command keywords for arcade context intrinsic function.  'TODO: Conver to Enums?
//        //' Functions are registered with the interpreter using these values as dictionary keys
//        //' These string representations of these commands are what user config files will use
//        //' to refer to these functions
//        //'=====================================
//        //' attack
//        //'=====================================
//        //Const KW_ATTACK As String = "attack"
//        //Const KW_ATTACK2 As String = "attack2"
//        //Const KW_RELOAD As String = "reload"

//        //' We can't have +/- to differentiate between key down/key release binds for a key
//        //' but we can check for "+" or "-" during parsing and deduce the correct
//        //' scriptlet to run.  Afterall, we can add "+" & GAME_COMMANDS.attack.toString
//        //' to our scriptlets collection

//        //' movement
//        //'=====================================
//        //Const KW_MOVEBACK As String = "moveback"
//        //Const KW_MOVEFORWARD As String = "moveforward"
//        //Const KW_MOVELEFT As String = "moveleft"
//        //Const KW_MOVERIGHT As String = "moveright"
//        //Const KW_MOVEUP As String = "moveup"
//        //Const KW_MOVEDOWN As String = "movedown"
//        //Const KW_STRAFELEFT As String = "strafeleft"
//        //Const KW_STRAFERIGHT As String = "straferight"
//        //Const KW_JUMP As String = "jump"
//        //Const KW_DUCK As String = "duck"
//        //Const KW_LOOKKEYBOARD As String = "lookkeyboard"
//        //Const KW_LOOKMOUSE As String = "lookmouse"
//        //Const KW_LOOKUP As String = "lookup"
//        //Const KW_LOOKDOWN As String = "lookdown"
//        //Const KW_LOOKLEFT As String = "lookleft"
//        //Const KW_LOOKRIGHT As String = "lookright"
//        //Const KW_STRAFE As String = "strafe"
//        //Const KW_SPEED As String = "speed"
//        //Const KW_USEITEM As String = "use"

//        //' misc
//        //'=====================================
//        //Const KW_SHOWSCORES As String = "scores"

//        //'=====================================
//        //' Command keywords for console context  'TODO: Conver to Enums?
//        //'=====================================

//        //Const KW_QUIT As String = "quit"
//        //Const KW_SAVE As String = "save"
//        //Const KW_PAUSE As String = "pause"
//        //Const KW_WAIT As String = "wait"


//        public FirstPerson(Entity player) : base(player)
//        {
//            //    Trace.Assert(player IsNot Nothing, "controllerFirstPerson.New() player argument not set.")
//            //    _player = player


//            //    // avatar functions used by a first person controller
//            //    _interpreter.registerFunction("+attack", AddressOf AttackBegin)
//            //    _interpreter.registerFunction("-attack", AddressOf AttackStop)
//            //    _interpreter.registerFunction("+forward", AddressOf moveForward)
//            //    _interpreter.registerFunction("-forward", AddressOf moveForwardStop)

//            //    // simulation functions 
//            //    _interpreter.registerFunction(KW_QUIT, AddressOf QuitSimulation)
//            //    _interpreter.registerFunction(KW_SAVE, AddressOf SaveSimulation)
//            //    _interpreter.registerFunction(KW_PAUSE, AddressOf PauseSimulation)
//            //    _interpreter.registerFunction(KW_WAIT, AddressOf WaitSimulation)

//            //    // load our binds _after_ intrinsic functions are registered
//            //    _interpreter.scriptLoad("C:\Documents and Settings\Hypnotron\My Documents\dev\vb.net\projects\x3d\bin\iraqitown\keyboard.config", False, False)
//        }


//        //' =======================================================================
//        //' Delegates for generic support functions 
//        //' PURPOSE: 
//        //' =======================================================================
//        //Private Function QuitSimulation(ByRef args() As Object) As Object
//        //    'TODO: we could use events for these types of functions?  Otherwise need to consider how we elegantly fit these into the Engine class.
//        //    ' _bIsRunning = False

//        //End Function

//        //Private Function SaveSimulation(ByRef args() As Object) As Object
//        //End Function

//        //Private Function PauseSimulation(ByRef args() As Object) As Object
//        //End Function

//        //Private Function WaitSimulation(ByRef args() As Object) As Object
//        //    Debug.Print("controllerFirstPerson.WaitSimulation() -- Waiting.")
//        //End Function

//        //Private Property player() As EntityFirstPerson
//        //    Get
//        //        Return DirectCast(_player, EntityFirstPerson)
//        //    End Get
//        //    Set(ByVal value As EntityFirstPerson)
//        //        _player = value
//        //    End Set
//        //End Property
//        //' =======================================================================
//        //' Delegates for user actions
//        //' PURPOSE: Having actions implemented in the controller allows us to 
//        //'          easily implement and "attach" new controllers to Entities.
//        //'          For example, an AI controller.  This can even be done at run-time.
//        //'          something that would require a kludge of class variable copying
//        //'          if you had to instance a new entity class for that
//        //' =======================================================================
//        //Private Sub moveLeft()
//        //    '_player.IsStrafingLeft()
//        //    '_sngStrafe = 0.1
//        //End Sub

//        //Private Sub moveRight()
//        //    ' _player.IsStrafingRight
//        //    ' _sngStrafe = -0.1
//        //End Sub

//        //Private Function moveForward(ByRef args() As Object) As Object
//        //    player.IsMovingForward = True
//        //End Function
//        //Private Function moveForwardStop(ByRef args() As Object) As Object
//        //    player.IsMovingForward = False
//        //End Function
//        //Private Sub moveBackward()
//        //    ' _player.IsMovingReverse()
//        //    ' _sngWalk = -0.05
//        //End Sub

//        //Private Function AttackBegin(ByRef args() As Object) As Object
//        //    System.Diagnostics.Trace.WriteLine("controllerFirstPerson:AttackRun()")
//        //    'TODO: note: we'd also update our entity context with things like
//        //    ' check current _player.State.Blah
//        //    ' and see if attack is allowed (maybe they are out of ammo, are currently reloading, etc
//        //    ' very complicated stuff.  
//        //    ' the "what can be done next" can be modelled as a finite state graph
//        //End Function
//        //Private Function AttackStop(ByRef args() As Object) As Object
//        //    System.Diagnostics.Trace.WriteLine("controllerFirstPerson:AttackStop()")

//        //End Function

//        //Private Sub UseItem()

//        //End Sub

//        //Private Sub reload()
//        //    'If Not _player.IsReloading Then
//        //    '    ' start reload animation
//        //    '    ' _Weapon.Ready = False
//        //    '    ' reload()
//        //    '    ' decrement ammo
//        //    '    ' how do you know when the reload animation is done so that you can finally
//        //    '    ' allow the player to fire?
//        //    '    ' _Weapon.Ready = True
//        //    'End If
//        //End Sub


//        //'' Non entity specific commands \ debug \ dev commands
//        //'' now this stuff would have to follow under a different controller (e.g. controllerRenderOptions) or something
//        //'
//        //'   If CBool(iKeysCurrent(MTV3D65.CONST_TV_KEY.TV_KEY_ESCAPE)) Then
//        //'       _bRunning = False
//        //'       Exit Sub
//        //'   End If
//        //'   cycle thru the various cull modes
//        //'    If CBool(iKeysCurrent(MTV3D65.CONST_TV_KEY.TV_KEY_F3)) Then
//        //'        If _cullMode = CULL_MODE.NONE Then
//        //'            _cullMode = CULL_MODE.SPHERE_TO_FRUSTUM_CONE
//        //'        ElseIf _cullMode = CULL_MODE.SPHERE_TO_FRUSTUM_CONE Then
//        //'            _cullMode = CULL_MODE.SPHERE_TO_FRUSTUM_SPHERE
//        //'        ElseIf _cullMode = CULL_MODE.SPHERE_TO_FRUSTUM_SPHERE Then
//        //'            _cullMode = CULL_MODE.TV3D_CULLING
//        //'        ElseIf _cullMode = CULL_MODE.TV3D_CULLING Then
//        //'            _cullMode = CULL_MODE.SPHERE_TO_FRUSTUM_BOX
//        //'        ElseIf _cullMode = CULL_MODE.SPHERE_TO_FRUSTUM_BOX Then
//        //'            _cullMode = CULL_MODE.NONE
//        //'        End If
//        //'        _renderState.CullMode = _cullMode
//        //'    End If

//        //'    If CBool(iKeysCurrent(MTV3D65.CONST_TV_KEY.TV_KEY_F2)) Then
//        //'        _renderState.MultitextureEnabled = Not _renderState.MultitextureEnabled
//        //'    End If

//        //'    If CBool(iKeysCurrent(MTV3D65.CONST_TV_KEY.TV_KEY_F12)) Then
//        //'        _bIsFullScreen = Not _bIsFullScreen
//        //'        If _bIsFullScreen Then
//        //'            _Engine.SwitchWindowed()
//        //'        Else
//        //'            _Engine.SwitchFullscreen(800, 600)
//        //'        End If
//        //'    End If

//    }
//}