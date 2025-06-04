using System;
using System.Collections.Generic;
using Keystone.AI;
using Keystone.Elements;
using Keystone.Types;

namespace Keystone.Entities
{
    // TODO: Character should inherit EntityBase or ModeledEntity rihgt?  Shouldnt really inherit BonedEntity exactly or if so
    // not named BonedEntity.  Instead I think a bonedEntity should just inherit an IActorEntity or IAnimatedActorEntity
    // since there are other types of animated entities that dont use bones for animation, but just linear interpolation about fixed axis
    // TODO: Agent should not need to inherit BonedEntity...  any ModeledEntity should do
    // what if instead i made SteeringData a composite object that any Entity could hold and be used by Steering API?
    // because with scripting, seems to me having "Character" and Steering vars is a waste
    public class Character : BonedEntity, ISteerable
    {
    	// TODO: path and waypoint and Steering vars will be in our UserData blackboard and available to scripts
        protected WaypointGraph mWaypointGraph;
        protected uint _currentWaypoint = 0;

        // ISteerable // I think we should also have an IPhysicsBody however, many steering variables max limits are body performance related
        protected Vector3d _target;
        protected double _wanderTheta;
        protected ISteerable _targetAgent;
        
        
        //private List<CharacterAttributes> mAttributes;

        // values are in meters per second
        protected const double METERS_PER_MILLISECOND = .001f;
        
        // these really are attributes.  this type of character data could exist perhaps as Script user data
        protected const double _speedModifier = 0; // for powerups
        protected const double RUN_VELOCITY = 12f;
        protected const double STRAFE_RUN_VELOCITY = 10f;
        protected double WALK_VELOCITY = 6f;
        protected const double STRAFE_VELOCITY = 6f;
        protected const double ANGULAR_VELOCITY = 180f; // degrees per second 50% of a full circle per second

        protected double _maxForce = .1f; // 
        protected double _maxSpeed = RUN_VELOCITY;

        //private double _maxDeceleration; 
        // private double _maxAcceleration;


        public Character(string id) : base(id) 
        {
            _maxForce = RUN_VELOCITY;
            _maxSpeed = RUN_VELOCITY;
        }

        public Vector3d Center
        {
            get { return BoundingBox.Center; }
        }


        // TODO: velocity, maxspeed,accel and such should be apart of Physics and with ISteerable setting those values so that
        // ISteerable actually works in conjunction with Physics and not in place of it completely.  Afterall, we still need physics to
        // govern the interaction of these bodies in the world after steering attempts to move them

        public double MaxSpeed
        {
            get { return _maxSpeed; }
            set { _maxSpeed = value; }
        }

        public double MaxForce
        {
            get { return _maxForce; }
            set { _maxForce = value; }
        }

              
        public Vector3d Target
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public double WanderTheta
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        #region ISteerable Members
       
        public double Radius
        {
            get
            {
                throw new Exception(); //return _models.BoundingSphere.Radius; 
            }
        }
        #endregion
        
        public override void Update(double elapsedSeconds)
        {
//        	// simple if/else if/else  behavior selector
//            if (_targetAgent != null)
//            {
//                // subtract a minimum radius from the target's position so we dont collide with it
//                Vector3d destination = _targetAgent.Translation +
//                                       (Vector3d.Normalize(Translation - _targetAgent.Translation)*Radius);
//
//                // determine if we should wander or follow a target
//                double detectRangeSquared = 256f*256f;
//                    // radiusSq at which the agent will start to follow the target, beyond which he'll break off
//                if (Vector3d.GetDistance3dSquared(destination, Translation) > detectRangeSquared)
//                    Acceleration += Steering.Wander(this);
//                else
//                    Acceleration += Steering.Steer(this, destination, 20);
//            }
//            else if (mWaypointGraph != null)
//            {
//                _currentWaypoint = mWaypointGraph.GetNextWaypoint(Translation, _currentWaypoint);
//                Vector3d destination = mWaypointGraph.WayPointLocation(_currentWaypoint);
//                Acceleration += Steering.Steer(this, destination, 0);
//                //TODO: _path.Draw();
//            }
//            else
//                Acceleration += Steering.Wander(this); // wander by default
//
//
//            UpdatePosition(elapsedSeconds);
//
//            // TODO: itterative approach to solve the overshoot problem that occurs when frame rate is very low
//            // recurse
//            // if ( _maxForce > 0 && (!(_currentWaypoint  != _path.Waypoints.Length - 1  || )
//            //   Update(elapsed);
//
//            // also further up we need to _maxForce = System.Math.Min (_maxForce, distanceRemaining);
//            //
//            // finally reset _maxForce to full
//            //_maxForce = RUN_VELOCITY;
//            base.Update(elapsedSeconds);
//            // NotifyChildEntities(); // TODO: i dont think this is necessary if we are updating Translation or Rotation or Matrix here
        }


        private void UpdatePosition(double elapsedSeconds)
        {
            // here unlike a user controlled player that updates based on the input 
            // technically we could update the same way, but our steering code needs to produce
            // the movement data we need

            //TODO: I think after steering, i should check the current rotation of the actor
            // compare that with the new rotation and if the difference is > ANGULAR_VELOCITY / seconds
            // then limit the rotation by that amount.  
            // AND if the monster cant be expected to run in that direction, to ignore the acceleration and just
            // have it rotate.  It can start moving again when its closer to facing its target. 


            // our target destination may not always be the Translation though.  If we have a good weapon, we may want to close "safely" if we can
            // to within our best weapon's max range.  Here we'll compute a new target location that stops us
            // when we get within the bounding sphere radius of the character.  For hand combat,we'd probably want to use boundingBox.Width  


            //NOTE: "velocity" represents the direction and speed (magnitude) of travel in that direction.
            //      Velocity is not the same as "facing" and as such, it would be up to us to constrain velocity
            //      by facing.  For instance, a monster cant start increasing velocity opposite its facing
            //      until it's oriented itself in that direction.  But actually as I consider my code
            //      the monster is rotating every frame as it heads towards its target.  The only time
            //      it really needs to turn more than in the Steer routine is if its distance to target
            //      is near zero yet its not facing the target.  
            //
            //      Velocity is constrained by MaxSpeed which is a double representing max magnitude (length).

            //      "acceleration" is the change in velocity and is analogous to our "steering" vector.
            //      If the velocity and acceleration are in the same direction (both have the same sign - both positive or both negative) 
            // the object is speeding up. If the velocity and acceleration are in opposite directions (they have opposite signs), 
            // the object is slowing down.


            //acceleration is cumulative.
            mVelocity += mAcceleration; 
            mVelocity = Vector3d.Limit(mVelocity, MaxSpeed);

            // velocity,  acceleration and MaxSpeed are in units of Meters Per Second 
            // so we must scale them down to "per frame" values
            Vector3d tmpPerFrameVelocity = mVelocity * elapsedSeconds;
            Translation += tmpPerFrameVelocity;
            mAcceleration = new Vector3d();

            // velocity direction vector will be used to compute a heading rotation
            Vector3d direction = Vector3d.Normalize(tmpPerFrameVelocity);
            Rotation =  AI.Steering.RotateTo (direction);
        }
    }
}
