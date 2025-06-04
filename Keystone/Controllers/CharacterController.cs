//using System;
//using System.Diagnostics;
//using MTV3D65;
//using Keystone.Entities;
//using Keystone.Types;

//using System.Drawing;
//using Keystone.Cameras;


//namespace Keystone.Controllers
//{
//    // in previous version of the codebase I had the CharacterController combined with the ThirdPersonCameraController
//    // but that made no sense.  They should be split up and I should be able to keep the character controller tied to 
//    // a character/player regardless of whether I switch to ThirdPerson or FirstPerson cameras and camera controllers.
//    public class CharacterController :InputController 
//    {

//        public CharacterController(Entity target)
//        {
//            Core._Core.Console.Register("+mouselook", ToggleMouseLook);
//            Core._Core.Console.Register("-mouselook", ToggleMouseLook);
//            Core._Core.Console.Register("zoom", ZoomIn);
//            Core._Core.Console.Register("zoomout", ZoomOut);
//            Core._Core.Console.Register("+attack", Attack);
//            Core._Core.Console.Register("-attack", Attack);


//            Core._Core.Console.Register("+forward", Forward);
//            Core._Core.Console.Register("-forward", Forward);
//            Core._Core.Console.Register("+backward", Backward);
//            Core._Core.Console.Register("-backward", Backward);
//            Core._Core.Console.Register("+strafeleft", StrafeLeft);
//            Core._Core.Console.Register("-strafeleft", StrafeLeft);
//            Core._Core.Console.Register("+straferight", StrafeRight);
//            Core._Core.Console.Register("-straferight", StrafeRight);
//            Core._Core.Console.Register("+turnleft", TurnLeft);
//            Core._Core.Console.Register("-turnleft", TurnLeft);
//            Core._Core.Console.Register("+turnright", TurnRight);
//            Core._Core.Console.Register("-turnright", TurnRight);
//            Core._Core.Console.Register("+sprint", Sprint);
//            Core._Core.Console.Register("-sprint", Sprint);

//            //_interpreter.registerFunction("+mouselook", ToggleMouseLook);
//            //_interpreter.registerFunction("-mouselook", ToggleMouseLook);
//            //_interpreter.registerFunction("zoom", ZoomIn);
//            //_interpreter.registerFunction("zoomout", ZoomOut);
//            //_interpreter.registerFunction("+attack", Attack);
//            //_interpreter.registerFunction("-attack", Attack);


//            //_interpreter.registerFunction("+forward", Forward);
//            //_interpreter.registerFunction("-forward", Forward);
//            //_interpreter.registerFunction("+backward", Backward);
//            //_interpreter.registerFunction("-backward", Backward);
//            //_interpreter.registerFunction("+strafeleft", StrafeLeft);
//            //_interpreter.registerFunction("-strafeleft", StrafeLeft);
//            //_interpreter.registerFunction("+straferight", StrafeRight);
//            //_interpreter.registerFunction("-straferight", StrafeRight);
//            //_interpreter.registerFunction("+turnleft", TurnLeft);
//            //_interpreter.registerFunction("-turnleft", TurnLeft);
//            //_interpreter.registerFunction("+turnright", TurnRight);
//            //_interpreter.registerFunction("-turnright", TurnRight);
//            //_interpreter.registerFunction("+sprint", Sprint);
//            //_interpreter.registerFunction("-sprint", Sprint);

//            //
//            _interpreter.scriptLoad(@"Config\binds_thirdperson.config", false, false);
//            mAvatar = target;


//        }

//        private void ToggleMouseLook(object[] args)
//        {
//            mSelectedContext.mViewController.MouseLook = _buttonPressed;
//        }


//        private void ZoomIn(object[] args)
//        {
//            Viewport vp = Cameras.Viewport.GetMouseOverViewport(_mouse.ScreenPosition.X, _mouse.ScreenPosition.Y);
//            if (vp == null)
//            {
//                mSelectedContext = null;
//                return;
//            }
//            mSelectedContext = vp.Context;
//            mSelectedContext.mViewController.ZoomIn(_mouse.scrollAmount);
//        }

//        private void ZoomOut(object[] args)
//        {
//            Viewport vp = Cameras.Viewport.GetMouseOverViewport(_mouse.ScreenPosition.X, _mouse.ScreenPosition.Y);
//            if (vp == null)
//            {
//                mSelectedContext = null;
//                return;
//            }
//            mSelectedContext = vp.Context;
//            mSelectedContext.mViewController.ZoomOut(_mouse.scrollAmount);
//        }

//        private void Attack(object[] args)
//        {
//            Trace.WriteLine("Attacking.");
//        }

//        private void Sprint(object[] args)
//        {
//            ((PlayerCharacter)mAvatar).IsRunning = _buttonPressed;
//        }

//        private void Forward(object[] args)
//        {
//            ((PlayerCharacter)mAvatar).MovingForward = _buttonPressed;
//        }

//        private void Backward(object[] args)
//        {
//            ((PlayerCharacter)mAvatar).MovingBackward = _buttonPressed;
//        }

//        private void StrafeLeft(object[] args)
//        {
//            ((PlayerCharacter)mAvatar).StrafingLeft = _buttonPressed;
//        }

//        private void StrafeRight(object[] args)
//        {
//            ((PlayerCharacter)mAvatar).StrafingRight = _buttonPressed;
//        }

//        private void TurnLeft(object[] args)
//        {
//            ((PlayerCharacter)mAvatar).TurningLeft = _buttonPressed;
//        }

//        private void TurnRight(object[] args)
//        {
//            ((PlayerCharacter)mAvatar).TurningRight = _buttonPressed;
//        }

//        /// <summary>
//        /// Handles the movement of the character when that character's rotation is tied to the mouse rotation
//        /// and also calls to the chase camera controller to update it's move in sychronization.
//        /// </summary>
//        /// <param name="xyDelta"></param>
//        /// <returns></returns>
//        protected override bool HandleMouseMove(Point delta)
//        {

//            // TODO: rather than Player, this should perhaps be an object that implements IControllable which
//            // internally has the reqt interface to allow this object to be controlled by user, by AI, 
//            PlayerCharacter p = (PlayerCharacter)mAvatar;

//            // //the  y rotation of the player has to be tied to the mouse movement, else the controls will feel unresponsive
//            // //the x rotation is for camera elevation. It will be constrained by min/max angles and also in Update by collision
//            //if (mSelectedContext._controller.MouseLook)
//            //{
//            //    mSelectedContext._controller.HandleMouseLook(delta);

//            //    // TODO: the p.Rotation.y always gets reset to 0 because in Simulation.Update() i grab the rotationmatrix 
//            //    // for every physics enabled entity and set it, but what i dont do is ever have this new rotation computed here
//            //    // assigned in the physics engine.
//            //    float rotationY = (float)p.Rotation.y; // sychronize mouselook rotation with keyboard rotation
//            //    rotationY += delta.X / 2.0f; // mouse x axis movement controls horizontal rotation about world Y axis

//            //    //keep the angle values between 0 and 360
//            //    if (rotationY < 0) rotationY += 360;
//            //    if (rotationY > 0) rotationY %= 360;

//            //    // as the player Updates, it's position is handled and any collision and terrain height adjustments are made
//            //    // then the camera can respond appropriately  after the player is done updating
//            //    p.Rotation = new Vector3d(0, rotationY, 0);
//            //}

//            return true;
//        }

//    }
//}
