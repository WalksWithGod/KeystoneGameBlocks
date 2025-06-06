using Keystone.Types;
using System;

namespace Keystone.Physics
{

	public class SimulationSettings
	{
		// Constructors
		public SimulationSettings ()
		{
			this.gravity = new Vector3d(0.00d, 0.00d, 0.00d);
			this.iterations = 15;
			this.linearVelocityClamping = 0.01F;
			this.linearVelocityClampingTime = 0.17F;
			this.angularVelocityClamping = 0.01F;
			this.angularVelocityClampingTime = 0.17F;
			this.numEntitiesToTryToDeactivatePerFrame = 0x64;
			this.iterationsBeforeEarlyOut = 1;
			this.minimumImpulse = 0.00f;
			this.defaultMargin = 0.04F;
			this.defaultAllowedPenetration = 0.005F;
			this.maximumPositionCorrectionSpeed = 2.00F;
			this.penetrationRecoveryStiffness = 0.20F;
			this.staticFrictionVelocityThreshold = 0.20F;
			this.bouncinessVelocityThreshold = 1.00F;
			this.bouncinessCombineMethod = PropertyCombineMethod.max;
			this.collisionDetectionType = CollisionDetectionType.fullyContinuous;
			this.timeStep = 0.02F;
			this.timeScale = 1.00F;
			this.timeStepCountPerFrameMaximum = 10;
			this.useSpecialCaseBoxBox = true;
			this.useSpecialCaseBoxSphere = true;
			this.useSpecialCaseSphereTriangle = true;
			this.useSpecialCaseSphereSphere = true;
			this.contactInvalidationLengthSquared = 0.01F;
		}
		
		static SimulationSettings ()
		{
			SimulationSettings.padInertiaTensors = false;
			SimulationSettings.inertiaTensorScale = 2.50F;
		}
		
		
		// Statics
		public static bool padInertiaTensors;
		public static float inertiaTensorScale;
		
		// Instance Fields
		public  Vector3d gravity;
		public  int iterations;
		public  float linearVelocityClamping;
		public  float linearVelocityClampingTime;
		public  float angularVelocityClamping;
		public  float angularVelocityClampingTime;
		public  int numEntitiesToTryToDeactivatePerFrame;
		public  int iterationsBeforeEarlyOut;
		public  float minimumImpulse;
		public  float defaultMargin;
		public  float defaultAllowedPenetration;
		public  bool useSplitImpulsePositionCorrection;
		public  float maximumPositionCorrectionSpeed;
		public  float penetrationRecoveryStiffness;
		public  float staticFrictionVelocityThreshold;
		public  float bouncinessVelocityThreshold;
		public  PropertyCombineMethod bouncinessCombineMethod;
		public  PropertyCombineMethod frictionCombineMethod;
		public  bool useRK4AngularIntegration;
		public  bool useOneShotManifolds;
		public  CollisionDetectionType collisionDetectionType;
		public  bool useContinuousDetectionAgainstDetectors;
		public  bool useContinuousDetectionAgainstMovingKinematics;
		public  bool conserveAngularMomentum;
		public  float timeStep;
		public  float timeScale;
		public  bool useInternalTimeStepping;
		public  int timeStepCountPerFrameMinimum;
		public  int timeStepCountPerFrameMaximum;
		public  bool useSpecialCaseBoxBox;
		public  bool useSpecialCaseBoxSphere;
		public  bool useSpecialCaseSphereTriangle;
		public  bool useSpecialCaseSphereSphere;
		public  float contactInvalidationLengthSquared;
	}
}
