// Copyright (c) 2002-2003, Sony Computer Entertainment America
// Copyright (c) 2002-2003, Craig Reynolds <craig_reynolds@playstation.sony.com>
// Copyright (C) 2007 Bjoern Graf <bjoern.graf@gmx.net>
// Copyright (C) 2007 Michael Coles <michael@digini.com>
// All rights reserved.
//
// This software is licensed as described in the file license.txt, which
// you should have received as part of this distribution. The terms
// are also available at http://www.codeplex.com/SharpSteer/Project/License.aspx.

using System.Collections.Generic;
using Keystone.Types;
using Steering.Helpers;
using Steering.Obstacles;
using Steering.Pathway;

namespace Steering
{
	public abstract class SteerLibrary : BaseVehicle
	{
	    protected IAnnotationService annotation { get; private set; }

	    // Constructor: initializes state
	    protected SteerLibrary(IAnnotationService annotationService = null)
		{
            annotation = annotationService ?? new NullAnnotationService();

			// set inital state
			Reset();
		}

		// reset state
		public virtual void Reset()
		{
			// initial state of wander behavior
			_wanderSide = 0;
			_wanderUp = 0;
		}

        #region steering behaviours
	    private double  _wanderSide;
	    private double  _wanderUp;
	    protected Vector3d SteerForWander(double  dt)
	    {
	        return this.SteerForWander(dt, ref _wanderSide, ref _wanderUp);
	    }

	    protected Vector3d SteerForFlee(Vector3d target)
	    {
	        return this.SteerForFlee(target, MaxSpeed);
	    }

	    protected Vector3d SteerForSeek(Vector3d target)
	    {
	        return this.SteerForSeek(target, MaxSpeed);
		}

        protected Vector3d SteerForArrival(Vector3d target, double  slowingDistance)
	    {
	        return this.SteerForArrival(target, MaxSpeed, slowingDistance, annotation);
	    }

	    protected Vector3d SteerToFollowFlowField(IFlowField field, double  predictionTime)
	    {
	        return this.SteerToFollowFlowField(field, MaxSpeed, predictionTime, annotation);
	    }

        protected Vector3d SteerToFollowPath(bool direction, double  predictionTime, IPathway path)
	    {
	        return this.SteerToFollowPath(direction, predictionTime, path, MaxSpeed, annotation);
	    }

        protected Vector3d SteerToStayOnPath(double  predictionTime, IPathway path)
	    {
	        return this.SteerToStayOnPath(predictionTime, path, MaxSpeed, annotation);
	    }

        protected Vector3d SteerToAvoidObstacle(double  minTimeToCollision, IObstacle obstacle)
        {
            return this.SteerToAvoidObstacle(minTimeToCollision, obstacle, annotation);
        }

	    protected Vector3d SteerToAvoidObstacles(double  minTimeToCollision, IEnumerable<IObstacle> obstacles)
	    {
	        return this.SteerToAvoidObstacles(minTimeToCollision, obstacles, annotation);
	    }

	    protected Vector3d SteerToAvoidNeighbors(double  minTimeToCollision, IEnumerable<IVehicle> others)
		{
	        return this.SteerToAvoidNeighbors(minTimeToCollision, others, annotation);
	    }

	    protected Vector3d SteerToAvoidCloseNeighbors<TVehicle>(double  minSeparationDistance, IEnumerable<TVehicle> others) where TVehicle : IVehicle
        {
            return this.SteerToAvoidCloseNeighbors<TVehicle>(minSeparationDistance, others, annotation);
        }

	    protected Vector3d SteerForSeparation(double  maxDistance, double  cosMaxAngle, IEnumerable<IVehicle> flock)
	    {
	        return this.SteerForSeparation(maxDistance, cosMaxAngle, flock, annotation);
	    }

	    protected Vector3d SteerForAlignment(double  maxDistance, double  cosMaxAngle, IEnumerable<IVehicle> flock)
	    {
	        return this.SteerForAlignment(maxDistance, cosMaxAngle, flock, annotation);
	    }

	    protected Vector3d SteerForCohesion(double  maxDistance, double  cosMaxAngle, IEnumerable<IVehicle> flock)
	    {
	        return this.SteerForCohesion(maxDistance, cosMaxAngle, flock, annotation);
	    }

	    protected Vector3d SteerForPursuit(IVehicle quarry, double  maxPredictionTime = double .MaxValue)
	    {
	        return this.SteerForPursuit(quarry, maxPredictionTime, MaxSpeed, annotation);
	    }

        protected Vector3d SteerForEvasion(IVehicle menace, double  maxPredictionTime)
        {
            return this.SteerForEvasion(menace, maxPredictionTime, MaxSpeed, annotation);
        }

	    protected Vector3d SteerForTargetSpeed(double  targetSpeed)
	    {
	        return this.SteerForTargetSpeed(targetSpeed, MaxForce, annotation);
	    }
        #endregion
	}
}
