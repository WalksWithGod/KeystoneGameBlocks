using System;
using System.Diagnostics;
using Keystone.AI;
using Keystone.Animation;
using Keystone.Elements;
using Keystone.Traversers;
using Keystone.Types;


namespace Keystone.Entities
{
    
    public class PlayerCharacter : Character
    {
        // state vars 
        private bool _mouseLook;
        private bool _isRunning;
        private bool _forward;
        private bool _backward;
        private bool _strafeLeft;
        private bool _strafeRight;
        private bool _turnLeft;
        private bool _turnRight;

        // should i make Actor public and just set its position and such directly?
        // actors would not be apart of the scene's normal update process?
        // note; if i do add actors to the scene they'd be under the root node
        // and we'd need to use the Simulation to determine which ones to update ... meh, need to ponder 
        public PlayerCharacter(string id, Actor3d actor)
            : base(id)
        {

            //string file = "scripts\\npc1.css";
            //string assembly = CSScript.CompileCode(file, null, false);
            //AsmHelper helper = new AsmHelper(assembly, "defaultscripts", false);
            // apparentl our shared Script lib can be instanced and given access to various Math and other routines
            // we want our scripts to have access too without having to pass them.
        }

        public override object Traverse(ITraverser target, object data)
        {
            return target.Apply(this, data);
        }

        
        // states related to a IControllable character
        public bool IsRunning
        {
            get { return _isRunning; }
            set { _isRunning = value; }
        }

        public bool MovingForward
        {
            get { return _forward; }
            set { _forward = value; }
        }

        public bool MovingBackward
        {
            get { return _backward; }
            set { _backward = value; }
        }

        public bool StrafingLeft
        {
            get { return _strafeLeft; }
            set { _strafeLeft = value; }
        }

        public bool StrafingRight
        {
            get { return _strafeRight; }
            set { _strafeRight = value; }
        }

        public bool TurningLeft
        {
            get { return _turnLeft; }
            set { _turnLeft = value; }
        }

        public bool TurningRight
        {
            get { return _turnRight; }
            set { _turnRight = value; }
        }

        // TODO: It's tempting to think we should just get rid of Player and make it a regular ModeledEntity that just has
        // different Behavior and Animation nodes added as children, but no.  We're not going to go full component system like that.
        // We'll use our seperate Player, NPC, Vehicle, etc and allow those respective entity types to have pluggable (strategy pattern) 
        // nodes for behavior and such, but these respective entities will have interfaces that are relevant to their type.
        // So we can still have relatively few entity types, but with full flexibility for modifying any specific instance.
        // As far as sharing a single IPLugin, well.. we'll see.  However we can always make it very simple for a user
        // to transparently change an entity from Static to Dynamic or Player or Vehicle, etc so that they dont explicitly
        // need to delete the node, add a new one and add any sub-nodes appearance and such they had before.

        // this should Update logic and Animation... and with physics im just not sure yet how we integrate whether physics must move us
        // or whether we can tell it to only do collision and response but not general movement updates... 


        public override void Update(double elapsedSeconds)
        {
            double perFrameMoveVelocity = 0;
            double perFrameStrafeVelocity = 0;
            double rotationDegrees = 0; // (float)Rotation.y;

            if (MovingForward)
                perFrameMoveVelocity = _isRunning ? RUN_VELOCITY : WALK_VELOCITY;
            if (MovingBackward)
                perFrameMoveVelocity = _isRunning ? -RUN_VELOCITY : -WALK_VELOCITY;
            if (StrafingLeft)
                perFrameStrafeVelocity = _isRunning ? -STRAFE_RUN_VELOCITY : -STRAFE_VELOCITY;
            if (StrafingRight)
                perFrameStrafeVelocity = _isRunning ? STRAFE_RUN_VELOCITY : STRAFE_VELOCITY;

            // rotation is only done when MouseLook = false
            if (!_mouseLook)
            {
                if (TurningLeft)
                    rotationDegrees += -ANGULAR_VELOCITY * elapsedSeconds;
                if (TurningRight)
                    rotationDegrees += ANGULAR_VELOCITY * elapsedSeconds;
            }


            perFrameMoveVelocity *= elapsedSeconds;
            perFrameStrafeVelocity *= elapsedSeconds;

            double rotationRadians = Utilities.MathHelper.DegreesToRadians((double) rotationDegrees);
            Vector3d pos = Translation;
            // TODO: we should be tracking our position independantly?  The actor's position shoulb be strictly for rendering?  Or should they?  I mean, a BonedModel is instanced. But i could see not wanting to have to touch the Scene tree since it might be in seperate thread so local tracking isbette
            pos.z += (double) (Math.Cos(rotationRadians)*perFrameMoveVelocity) +
                     (double) (Math.Cos(rotationRadians + Utilities.MathHelper.PI_OVER_2) * perFrameStrafeVelocity);
            pos.x += (double) (Math.Sin(rotationRadians)*perFrameMoveVelocity) +
                     (double) (Math.Sin(rotationRadians + Utilities.MathHelper.PI_OVER_2) * perFrameStrafeVelocity);

            // TODO: for threaded, here we need to Queue this command
            //  Commands.SetPosition setPosition = new SetPosition(pos.x, pos.y, pos.z);
            //  Simulation.QueueCommand(setPosition);
            // or
            // Simulation.QueueCommand(new SetMove(pos, rotationDegrees, scale));
// TODO: uncomment after conversion to Quats from Eulers            Rotation = new Vector3d(0, rotationDegrees, 0);
            Translation = pos;

            // update the animation and skinning
            base.Update(elapsedSeconds);
            // NotifyChildEntities(); // TODO: i dont think this is necessary if we are updating Translation or Rotation or Matrix here
        }
    }
}