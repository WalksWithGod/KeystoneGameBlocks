using System;
using Keystone.Types;
using Steering.Helpers;

namespace Keystone.Entities
{
	/// <summary>
	/// Description of SteerableEntity.  BonedEntity inherits SteerableEntity
	/// </summary>
	public class SteerableEntity : ModeledEntity, Steering.IVehicle, Steering.ILocalSpaceBasis
	{
		
		Vector3d _lastForward;
        Vector3d _lastPosition;
		double _smoothedCurvature;
		// The acceleration is smoothed
        Vector3d _acceleration;
        
        
        internal SteerableEntity(string id)
            : base(id)
        {
			ResetLocalSpace();
        }

//      public SteerableEntity(Vector3d up, Vector3d forward, Vector3d position)
//		{
//			Up = up;
//			Forward = forward;
//            Position = position;
//			SetUnitSideFromForwardAndUp();
//		}
//
//        public SteerableEntity(Matrix transformation)
//        {
//            LocalSpaceBasisHelpers.FromMatrix(transformation, out ForwardField, out SideField, out UpField, out PositionField);
//        }
        
        
        protected Vector3d SideField;

        /// <summary>
        /// side-pointing unit basis vector
        /// </summary>
        public Vector3d Side
        {
            get { return SideField; }
            set { SideField = value; }
        }

        protected Vector3d UpField;

        /// <summary>
        /// upward-pointing unit basis vector
        /// </summary>
        public Vector3d Up
        {
            get { return UpField; }
            set { UpField = value; }
        }

        protected Vector3d ForwardField;

        /// <summary>
        /// forward-pointing unit basis vector
        /// </summary>
        public Vector3d Forward
        {
            get { return ForwardField; }
            set { ForwardField = value; }
        }

        protected Vector3d PositionField;

        /// <summary>
        /// origin of local space
        /// </summary>
        public Vector3d Position
        {
            get { return PositionField; }
            set { PositionField = value; }
        }
        


		// ------------------------------------------------------------------------
		// reset transform: set local space to its identity state, equivalent to a
		// 4x4 homogeneous transform like this:
		//
		//     [ X 0 0 0 ]
		//     [ 0 1 0 0 ]
		//     [ 0 0 1 0 ]
		//     [ 0 0 0 1 ]
		//
		// where X is 1 for a left-handed system and -1 for a right-handed system.
		public void ResetLocalSpace()
		{
		    LocalSpaceBasisHelpers.ResetLocalSpace(out ForwardField, out SideField, out UpField, out PositionField);
		}

		// ------------------------------------------------------------------------
		// set "side" basis vector to normalized cross product of forward and up
		public void SetUnitSideFromForwardAndUp()
		{
		    LocalSpaceBasisHelpers.SetUnitSideFromForwardAndUp(ref ForwardField, out SideField, ref UpField);
		}

	    // ------------------------------------------------------------------------
		// regenerate the orthonormal basis vectors given a new forward
		//(which is expected to have unit length)
        public void RegenerateOrthonormalBasisUF(Vector3d newUnitForward)
        {
            LocalSpaceBasisHelpers.RegenerateOrthonormalBasisUF(newUnitForward, out ForwardField, out SideField, ref UpField);
        }

		// for when the new forward is NOT know to have unit length
        public void RegenerateOrthonormalBasis(Vector3d newForward)
        {
            LocalSpaceBasisHelpers.RegenerateOrthonormalBasis(newForward, out ForwardField, out SideField, ref UpField);
        }

		// for supplying both a new forward and and new up
        public void RegenerateOrthonormalBasis(Vector3d newForward, Vector3d newUp)
        {
            LocalSpaceBasisHelpers.RegenerateOrthonormalBasis(newForward, newUp, out ForwardField, out SideField, out UpField);
        }
        
        // get/set Mass
        // Mass (defaults to unity so acceleration=force)
	    public double Mass { get; set; }

	 //   // get velocity of entity
  //      public override Vector3d Velocity
		//{
		//	get
  //          {
  //              mVelocity = Forward * Speed;
  //              return mVelocity; // Forward * Speed;
  //          }
		//}

		// get/set speed of vehicle  (may be faster than taking mag of velocity)
        // speed along Forward direction. Because local space is
        // velocity-aligned, velocity = Forward * Speed
	    public double Speed { get; set; }

	    // size of bounding sphere, for obstacle avoidance, etc.
	    public double Radius { get; set; }

	    // get/set maxForce
        // the maximum steering force this vehicle can apply
        // (steering force is clipped to this magnitude)
        public double MaxForce
        {
            get { return 5.0f; }
        }

	    // get/set maxSpeed
        // the maximum speed this vehicle is allowed to move
        // (velocity is clipped to this magnitude)
	    public double MaxSpeed
	    {
	        get { return 5; }
	    }

	    // apply a given steering force to our momentum,
		// adjusting our orientation to maintain velocity-alignment.
	    public void ApplySteeringForce(Vector3d force, double elapsedTime)
		{
			Vector3d adjustedForce = AdjustRawSteeringForce(force, elapsedTime);

			// enforce limit on magnitude of steering force
            Vector3d clippedForce = adjustedForce.TruncateLength(MaxForce);

			// compute acceleration and velocity
			Vector3d newAcceleration = (clippedForce / Mass);
			Vector3d newVelocity = Velocity;

			// damp out abrupt changes and oscillations in steering acceleration
			// (rate is proportional to time step, then clipped into useful range)
			if (elapsedTime > 0)
			{
                double smoothRate = Steering.Helpers.Utilities.Clamp(9 * elapsedTime, 0.15f, 0.4f);
				Steering.Helpers.Utilities.BlendIntoAccumulator(smoothRate, newAcceleration, ref _acceleration);
			}

			// Euler integrate (per frame) acceleration into velocity
			newVelocity += _acceleration * elapsedTime;

			// enforce speed limit
            newVelocity = newVelocity.TruncateLength(MaxSpeed);

			// update Speed
			Speed = (newVelocity.Length);

			// Euler integrate (per frame) velocity into position
			Position = (Position + (newVelocity * elapsedTime));

			// regenerate local space (by default: align vehicle's forward axis with
			// new velocity, but this behavior may be overridden by derived classes.)
			RegenerateLocalSpace(newVelocity, elapsedTime);

			// maintain path curvature information
			MeasurePathCurvature(elapsedTime);

			// running average of recent positions
			Steering.Helpers.Utilities.BlendIntoAccumulator(elapsedTime * 0.06f, // QQQ
								  Position,
								  ref _smoothedPosition);
		}

		// the default version: keep FORWARD parallel to velocity, change
		// UP as little as possible.
	    protected virtual void RegenerateLocalSpace(Vector3d newVelocity, double elapsedTime)
		{
			// adjust orthonormal basis vectors to be aligned with new velocity
			if (Speed > 0)
			{
				RegenerateOrthonormalBasisUF(newVelocity / Speed);
			}
		}

		// alternate version: keep FORWARD parallel to velocity, adjust UP
		// according to a no-basis-in-reality "banking" behavior, something
		// like what birds and airplanes do.  (XXX experimental cwr 6-5-03)
	    protected void RegenerateLocalSpaceForBanking(Vector3d newVelocity, double elapsedTime)
		{
			// the length of this global-upward-pointing vector controls the vehicle's
			// tendency to right itself as it is rolled over from turning acceleration
			Vector3d globalUp = new Vector3d(0, 0.2f, 0);

			// acceleration points toward the center of local path curvature, the
			// length determines how much the vehicle will roll while turning
			Vector3d accelUp = _acceleration * 0.05f;

			// combined banking, sum of UP due to turning and global UP
			Vector3d bankUp = accelUp + globalUp;

			// blend bankUp into vehicle's UP basis vector
			double smoothRate = elapsedTime * 3;
			Vector3d tempUp = Up;
			Steering.Helpers.Utilities.BlendIntoAccumulator(smoothRate, bankUp, ref tempUp);
			Up = Vector3d.Normalize(tempUp);

			//annotation.Line(Position, Position + (globalUp * 4), Colors.White);
	        //annotation.Line(Position, Position + (bankUp * 4), Colors.Orange);
			//annotation.Line(Position, Position + (accelUp * 4), Colors.Red);
	        //annotation.Line(Position, Position + (Up * 1), Colors.Gold);

			// adjust orthonormal basis vectors to be aligned with new velocity
			if (Speed > 0) RegenerateOrthonormalBasisUF(newVelocity / Speed);
		}

		/// <summary>
        /// adjust the steering force passed to applySteeringForce.
        /// allows a specific vehicle class to redefine this adjustment.
        /// default is to disallow backward-facing steering at low speed.
		/// </summary>
		/// <param name="force"></param>
		/// <param name="deltaTime"></param>
		/// <returns></returns>
		protected virtual Vector3d AdjustRawSteeringForce(Vector3d force, double deltaTime)
		{
			double maxAdjustedSpeed = 0.2f * MaxSpeed;

			if ((Speed > maxAdjustedSpeed) || (force == Vector3d.Zero()))
				return force;

            double range = Speed / maxAdjustedSpeed;
            double cosine = Steering.Helpers.Utilities.Lerp(1.0f, -1.0f, Math.Pow(range, 20));
            return force.LimitMaxDeviationAngle(cosine, Forward);
		}

		/// <summary>
        /// apply a given braking force (for a given dt) to our momentum.
		/// </summary>
		/// <param name="rate"></param>
		/// <param name="deltaTime"></param>
	    public void ApplyBrakingForce(double rate, double deltaTime)
		{
			double rawBraking = Speed * rate;
			double clipBraking = ((rawBraking < MaxForce) ? rawBraking : MaxForce);
			Speed = (Speed - (clipBraking * deltaTime));
		}

		/// <summary>
        /// predict position of this vehicle at some time in the future (assumes velocity remains constant)
		/// </summary>
		/// <param name="predictionTime"></param>
		/// <returns></returns>
        public Vector3d PredictFuturePosition(double predictionTime)
		{
			return Position + (Velocity * predictionTime);
		}

		// get instantaneous curvature (since last update)
	    protected double Curvature { get; private set; }

	    // get/reset smoothedCurvature, smoothedAcceleration and smoothedPosition
		public double SmoothedCurvature
		{
			get { return _smoothedCurvature; }
		}

	    private void ResetSmoothedCurvature(double value = 0)
		{
	    	_lastForward = Vector3d.Zero();
	    	_lastPosition = Vector3d.Zero();
	        _smoothedCurvature = value;
            Curvature = value;
		}

		public override Vector3d Acceleration
		{
			get { return _acceleration; }
		}

	    protected void ResetAcceleration()
	    {
	    	ResetAcceleration(Vector3d.Zero());
	    }

	    private void ResetAcceleration(Vector3d value)
	    {
	        _acceleration = value;
	    }

        Vector3d _smoothedPosition;
	    public Vector3d SmoothedPosition
		{
			get { return _smoothedPosition; }
		}

	    private void ResetSmoothedPosition()
	    {
	    	ResetSmoothedPosition(Vector3d.Zero());
	    }

	    protected void ResetSmoothedPosition(Vector3d value)
	    {
	        _smoothedPosition = value;
	    }

	    // set a random "2D" heading: set local Up to global Y, then effectively
		// rotate about it by a random angle (pick random forward, derive side).
	    protected void RandomizeHeadingOnXZPlane()
		{
	    	Up = Vector3d.Up();
            Forward = Vector3Helpers.RandomUnitVectorOnXZPlane();
	        Side = Vector3d.CrossProduct(Forward, Up);
		}

		// measure path curvature (1/turning-radius), maintain smoothed version
		void MeasurePathCurvature(double elapsedTime)
		{
			if (elapsedTime > 0)
			{
				Vector3d dP = _lastPosition - Position;
				Vector3d dF = (_lastForward - Forward) / dP.Length;
                Vector3d lateral = Vector3Helpers.PerpendicularComponent(dF, Forward);
                double sign = (Vector3d.DotProduct (lateral, Side) < 0) ? 1.0d : -1.0d;
				Curvature = lateral.Length * sign;
				Steering.Helpers.Utilities.BlendIntoAccumulator(elapsedTime * 4.0d, Curvature, ref _smoothedCurvature);
				_lastForward = Forward;
				_lastPosition = Position;
			}
		}
	}
}
