using System;
using System.Collections.Generic;
using System.Linq;
using Keystone.Types;
using Steering.Obstacles;
using Steering.Pathway;

namespace Steering.Helpers
{
    public static class VehicleHelpers
    {
        public static Vector3d SteerForWander(this IVehicle vehicle, double dt, ref double wanderSide, ref double wanderUp, IAnnotationService annotation = null)
        {
            // random walk WanderSide and WanderUp between -1 and +1
            double speed = 12 * dt; // maybe this (12) should be an argument?
            wanderSide = Utilities.ScalarRandomWalk(wanderSide, speed, -1, +1);
            wanderUp = Utilities.ScalarRandomWalk(wanderUp, speed, -1, +1);

            // return a pure lateral steering vector: (+/-Side) + (+/-Up)
            return (vehicle.Side * wanderSide) + (vehicle.Up * wanderUp);
        }

        public static Vector3d SteerForFlee(this IVehicle vehicle, Vector3d target, double maxSpeed, IAnnotationService annotation = null)
        {
            Vector3d offset = vehicle.Position - target;
            Vector3d desiredVelocity = offset.TruncateLength(maxSpeed); //xxxnew
            return desiredVelocity - vehicle.Velocity;
        }

        public static Vector3d SteerForSeek(this IVehicle vehicle, Vector3d target, double maxSpeed, IAnnotationService annotation = null)
        {
            Vector3d offset = target - vehicle.Position;
            Vector3d desiredVelocity = offset.TruncateLength(maxSpeed); //xxxnew
            return desiredVelocity - vehicle.Velocity;
        }

        public static Vector3d SteerForArrival(this IVehicle vehicle, Vector3d target, double maxSpeed, double slowingDistance, IAnnotationService annotation = null)
        {
            Vector3d offset = target - vehicle.Position;
            double distance = offset.Length;
            double rampedSpeed = maxSpeed * (distance / slowingDistance);
            double clippedSpeed = Math.Min(rampedSpeed, maxSpeed);
            Vector3d desiredVelocity = (clippedSpeed / distance) * offset;
            return desiredVelocity - vehicle.Velocity;
        }

        public static Vector3d SteerToFollowFlowField(this IVehicle vehicle, IFlowField flowField, double maxSpeed, double predictionDistance, IAnnotationService annotation = null)
        {
            var futurePosition = vehicle.PredictFuturePosition(predictionDistance);
            var flow = flowField.Sample(futurePosition);
            return vehicle.Velocity - flow.TruncateLength(maxSpeed);
        }

        public static Vector3d SteerToStayOnPath(this IVehicle vehicle, double predictionTime, IPathway path, double maxSpeed, IAnnotationService annotation = null)
        {
            // predict our future position
            Vector3d futurePosition = vehicle.PredictFuturePosition(predictionTime);

            // find the point on the path nearest the predicted future position
            Vector3d tangent;
            double outside;
            Vector3d onPath = path.MapPointToPath(futurePosition, out tangent, out outside);

            if (outside < 0)
            	return Vector3d.Zero();    // our predicted future position was in the path, return zero steering.

            // our predicted future position was outside the path, need to
            // steer towards it.  Use onPath projection of futurePosition
            // as seek target
            if (annotation != null)
                annotation.PathFollowing(futurePosition, onPath, onPath, outside);

            return vehicle.SteerForSeek(onPath, maxSpeed);
        }

        public static Vector3d SteerToFollowPath(this IVehicle vehicle, bool direction, double predictionTime, IPathway path, double maxSpeed, IAnnotationService annotation = null)
        {
            double pathDistance;
            return SteerToFollowPath(vehicle, direction, predictionTime, path, maxSpeed, out pathDistance, annotation);
        }

        public static Vector3d SteerToFollowPath(this IVehicle vehicle, bool direction, double predictionTime, IPathway path, double maxSpeed, out double currentPathDistance, IAnnotationService annotation = null)
        {
            // if we've arrived at final destination don't need to do anything
            if (path.HasArrivedAtEndPath (vehicle.Position))
            {
            	currentPathDistance = 0;
            	return Vector3d.Zero();
            }
            
            // our goal will be offset from our path distance by this amount
            double pathDistanceOffset = (direction ? 1 : -1) * predictionTime * vehicle.Speed;

            // predict our future position
            Vector3d futurePosition = vehicle.PredictFuturePosition(predictionTime);

            // measure distance along path of our current and predicted positions
            currentPathDistance = path.MapPointToPathDistance(vehicle.Position);
            double futurePathDistance = path.MapPointToPathDistance(futurePosition);

            // are we facing in the correction direction?
            bool rightway = ((pathDistanceOffset > 0) ?
                            (currentPathDistance < futurePathDistance) :
                            (currentPathDistance > futurePathDistance));

            // find the point on the path nearest the predicted future position
            Vector3d tangent;
            double outside;
            Vector3d onPath = path.MapPointToPath(futurePosition, out tangent, out outside);

            // no steering is required if (a) our future position is inside
            // the path tube and (b) we are facing in the correct direction
            if ((outside <= 0) && rightway)
            {
                //We're going at max speed, in the right direction. don't need to do anything
                if (vehicle.Speed >= maxSpeed)
                    return Vector3d.Zero();

                //Predict vehicle position and sample multiple times, incresingly far along the path
                var seek = path.MapPointToPath(vehicle.PredictFuturePosition(predictionTime / 3), out tangent, out outside);
                for (int i = 0; i < 3; i++)
                {
                    var s = path.MapPointToPath(seek + tangent * vehicle.Speed / (i + 1), out tangent, out outside);

                    //terminate search if we wander outside the path
                    if (outside > 0)
                        break;
                    seek = s;

                    if (annotation != null)
                    	annotation.Circle3D(0.3f, seek, Vector3d.Right(), Colors.Green, 6);
                }
                
                //Steer towards future path point
                return vehicle.SteerForSeek(seek, maxSpeed, annotation);
            }

            // otherwise we need to steer towards a target point obtained
            // by adding pathDistanceOffset to our current path position
            double targetPathDistance = currentPathDistance + pathDistanceOffset;
            Vector3d target = path.MapPathDistanceToPoint(targetPathDistance);

            if (annotation != null)
                annotation.PathFollowing(futurePosition, onPath, target, outside);

            // return steering to seek target on path
            return SteerForSeek(vehicle, target, maxSpeed);
        }

        /// <summary>
        /// Returns a steering force to avoid a given obstacle.  The purely
        /// lateral steering force will turn our this towards a silhouette edge
        /// of the obstacle.  Avoidance is required when (1) the obstacle
        /// intersects the this's current path, (2) it is in front of the
        /// this, and (3) is within minTimeToCollision seconds of travel at the
        /// this's current velocity.  Returns a zero vector value (Vector3::zero)
        /// when no avoidance is required.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="minTimeToCollision"></param>
        /// <param name="obstacle"></param>
        /// <param name="annotation"></param>
        /// <returns></returns>
        public static Vector3d SteerToAvoidObstacle(this IVehicle vehicle, double minTimeToCollision, IObstacle obstacle, IAnnotationService annotation = null)
        {
            Vector3d avoidance = obstacle.SteerToAvoid(vehicle, minTimeToCollision);

            // XXX more annotation modularity problems (assumes spherical obstacle)
            if (avoidance != Vector3d.Zero() && annotation != null)
                annotation.AvoidObstacle(minTimeToCollision * vehicle.Speed);

            return avoidance;
        }

        public static Vector3d SteerToAvoidObstacles(this IVehicle vehicle, double minTimeToCollision, IEnumerable<IObstacle> obstacles, IAnnotationService annotation = null)
        {
            PathIntersection? nearest = null;
            double minDistanceToCollision = minTimeToCollision * vehicle.Speed;

            // test all obstacles for intersection with my forward axis,
            // select the one whose point of intersection is nearest
            foreach (var o in obstacles)
            {
                var next = o.NextIntersection(vehicle);
                if (!next.HasValue)
                    continue;

                if (!nearest.HasValue || (next.Value < nearest.Value.Distance))
                    nearest = new PathIntersection { Distance = next.Value, Obstacle = o };
            }

            if (nearest.HasValue)
            {
                if (annotation != null)
                    annotation.AvoidObstacle(minDistanceToCollision);

                return nearest.Value.Obstacle.SteerToAvoid(vehicle, minTimeToCollision);
            }
            return Vector3d.Zero();
        }

        private struct PathIntersection
        {
            public double Distance;
            public IObstacle Obstacle;
        }

        public static Vector3d SteerForSeparation(this IVehicle vehicle, double maxDistance, double cosMaxAngle, IEnumerable<IVehicle> others, IAnnotationService annotation = null)
        {
            // steering accumulator and count of neighbors, both initially zero
            Vector3d steering = Vector3d.Zero();
            int neighbors = 0;

            // for each of the other vehicles...
            foreach (var other in others)
            {
                if (!IsInBoidNeighborhood(vehicle, other, vehicle.Radius * 3, maxDistance, cosMaxAngle))
                    continue;

                // add in steering contribution
                // (opposite of the offset direction, divided once by distance
                // to normalize, divided another time to get 1/d falloff)
                Vector3d offset = other.Position - vehicle.Position;
                double distanceSquared = Vector3d.DotProduct(offset, offset);
                steering += (offset / -distanceSquared);

                // count neighbors
                neighbors++;
            }

            // divide by neighbors, then normalize to pure direction
            if (neighbors > 0)
            {
                steering = Vector3d.Normalize(steering / neighbors);
            }

            return steering;
        }

        /// <summary>
        /// avoidance of "close neighbors"
        /// </summary>
        /// <remarks>
        /// Does a hard steer away from any other agent who comes withing a
        /// critical distance.  Ideally this should be replaced with a call
        /// to steerForSeparation.
        /// </remarks>
        /// <typeparam name="TVehicle"></typeparam>
        /// <param name="vehicle"></param>
        /// <param name="minSeparationDistance"></param>
        /// <param name="others"></param>
        /// <param name="annotation"></param>
        /// <returns></returns>
        public static Vector3d SteerToAvoidCloseNeighbors<TVehicle>(this IVehicle vehicle, double minSeparationDistance, IEnumerable<TVehicle> others, IAnnotationService annotation = null)
            where TVehicle : IVehicle
        {
            // for each of the other vehicles...
            foreach (IVehicle other in others)
            {
                if (other != vehicle)
                {
                    double sumOfRadii = vehicle.Radius + other.Radius;
                    double minCenterToCenter = minSeparationDistance + sumOfRadii;
                    Vector3d offset = other.Position - vehicle.Position;
                    double currentDistance = offset.Length;

                    if (currentDistance < minCenterToCenter)
                    {
                        if (annotation != null)
                            annotation.AvoidCloseNeighbor(other, minSeparationDistance);

                        return Vector3Helpers.PerpendicularComponent(-offset, vehicle.Forward);
                    }
                }
            }

            // otherwise return zero
            return Vector3d.Zero();
        }

        public static Vector3d SteerForAlignment(this IVehicle vehicle, double maxDistance, double cosMaxAngle, IEnumerable<IVehicle> flock, IAnnotationService annotation = null)
        {
            // steering accumulator and count of neighbors, both initially zero
            Vector3d steering = Vector3d.Zero();
            int neighbors = 0;

            // for each of the other vehicles...
            foreach (IVehicle other in flock.Where(other => vehicle.IsInBoidNeighborhood(other, vehicle.Radius * 3, maxDistance, cosMaxAngle)))
            {
                // accumulate sum of neighbor's heading
                steering += other.Forward;

                // count neighbors
                neighbors++;
            }

            // divide by neighbors, subtract off current heading to get error-
            // correcting direction, then normalize to pure direction
            if (neighbors > 0)
            {
                steering = ((steering / neighbors) - vehicle.Forward);

                var length = steering.Length;
                if (length > 0.025f)
                    steering /= length;
            }

            return steering;
        }

        public static Vector3d SteerForCohesion(this IVehicle vehicle, double maxDistance, double cosMaxAngle, IEnumerable<IVehicle> flock, IAnnotationService annotation = null)
        {
            // steering accumulator and count of neighbors, both initially zero
            Vector3d steering = Vector3d.Zero();
            int neighbors = 0;

            // for each of the other vehicles...
            foreach (IVehicle other in flock.Where(other => vehicle.IsInBoidNeighborhood(other, vehicle.Radius * 3, maxDistance, cosMaxAngle)))
            {
                // accumulate sum of neighbor's positions
                steering += other.Position;

                // count neighbors
                neighbors++;
            }

            // divide by neighbors, subtract off current position to get error-
            // correcting direction, then normalize to pure direction
            if (neighbors > 0)
            {
                steering = Vector3d.Normalize((steering / neighbors) - vehicle.Position);
            }

            return steering;
        }

        private readonly static float[,] _pursuitFactors = new float[3, 3]
        {
            { 2, 2, 0.5f },         //Behind
            { 4, 0.8f, 1 },         //Aside
            { 0.85f, 1.8f, 4 },     //Ahead
        };

        public static Vector3d SteerForPursuit(this IVehicle vehicle, IVehicle quarry, double maxPredictionTime, double maxSpeed, IAnnotationService annotation = null)
        {
            // offset from this to quarry, that distance, unit vector toward quarry
            Vector3d offset = quarry.Position - vehicle.Position;
            double distance = offset.Length;
            Vector3d unitOffset = offset / distance;

            // how parallel are the paths of "this" and the quarry
            // (1 means parallel, 0 is pependicular, -1 is anti-parallel)
            double parallelness = Vector3d.DotProduct(vehicle.Forward, quarry.Forward);

            // how "forward" is the direction to the quarry
            // (1 means dead ahead, 0 is directly to the side, -1 is straight back)
            double forwardness = Vector3d.DotProduct(vehicle.Forward, unitOffset);

            double directTravelTime = distance / Math.Max(0.001f, vehicle.Speed);
            int f = Utilities.IntervalComparison(forwardness, -0.707f, 0.707f);
            int p = Utilities.IntervalComparison(parallelness, -0.707f, 0.707f);

            // Break the pursuit into nine cases, the cross product of the
            // quarry being [ahead, aside, or behind] us and heading
            // [parallel, perpendicular, or anti-parallel] to us.
            double timeFactor = _pursuitFactors[f + 1, p + 1];

            // estimated time until intercept of quarry
            double et = directTravelTime * timeFactor;

            // xxx experiment, if kept, this limit should be an argument
            double etl = (et > maxPredictionTime) ? maxPredictionTime : et;

            // estimated position of quarry at intercept
            Vector3d target = quarry.PredictFuturePosition(etl);

            // annotation
            if (annotation != null)
                annotation.Line(vehicle.Position, target, Colors.DarkGray);

            return SteerForSeek(vehicle, target, maxSpeed, annotation);
        }

        public static Vector3d SteerForEvasion(this IVehicle vehicle, IVehicle menace, double maxPredictionTime, double maxSpeed, IAnnotationService annotation = null)
        {
            // offset from this to menace, that distance, unit vector toward menace
            Vector3d offset = menace.Position - vehicle.Position;
            double distance = offset.Length;

            double roughTime = distance / menace.Speed;
            double predictionTime = ((roughTime > maxPredictionTime) ? maxPredictionTime : roughTime);

            Vector3d target = menace.PredictFuturePosition(predictionTime);

            return SteerForFlee(vehicle, target, maxSpeed, annotation);
        }

        /// <summary>
        /// tries to maintain a given speed, returns a maxForce-clipped steering
        /// force along the forward/backward axis
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="targetSpeed"></param>
        /// <param name="maxForce"></param>
        /// <param name="annotation"></param>
        /// <returns></returns>
        public static Vector3d SteerForTargetSpeed(this IVehicle vehicle, double targetSpeed, double maxForce, IAnnotationService annotation = null)
        {
            double mf = maxForce;
            double speedError = targetSpeed - vehicle.Speed;
            return vehicle.Forward * Utilities.Clamp(speedError, -mf, +mf);
        }

        /// <summary>
        /// Unaligned collision avoidance behavior: avoid colliding with other
        /// nearby vehicles moving in unconstrained directions.  Determine which
        /// (if any) other other this we would collide with first, then steers
        /// to avoid the site of that potential collision.  Returns a steering
        /// force vector, which is zero length if there is no impending collision.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="minTimeToCollision"></param>
        /// <param name="others"></param>
        /// <param name="annotation"></param>
        /// <returns></returns>
        public static Vector3d SteerToAvoidNeighbors(this IVehicle vehicle, double minTimeToCollision, IEnumerable<IVehicle> others, IAnnotationService annotation = null)
        {
            // first priority is to prevent immediate interpenetration
            Vector3d separation = SteerToAvoidCloseNeighbors(vehicle, 0, others, annotation);
            if (separation != Vector3d.Zero())
                return separation;

            // otherwise, go on to consider potential future collisions
            double steer = 0;
            IVehicle threat = null;

            // Time (in seconds) until the most immediate collision threat found
            // so far.  Initial value is a threshold: don't look more than this
            // many frames into the future.
            double minTime = minTimeToCollision;

            // xxx solely for annotation
            Vector3d xxxThreatPositionAtNearestApproach = Vector3d.Zero();
            Vector3d xxxOurPositionAtNearestApproach = Vector3d.Zero();

            // for each of the other vehicles, determine which (if any)
            // pose the most immediate threat of collision.
            foreach (IVehicle other in others)
            {
                if (other != vehicle)
                {
                    // avoid when future positions are this close (or less)
                    double collisionDangerThreshold = vehicle.Radius * 2;

                    // predicted time until nearest approach of "this" and "other"
                    double time = PredictNearestApproachTime(vehicle, other);

                    // If the time is in the future, sooner than any other
                    // threatened collision...
                    if ((time >= 0) && (time < minTime))
                    {
                        // if the two will be close enough to collide,
                        // make a note of it
                        if (ComputeNearestApproachPositions(vehicle, other, time) < collisionDangerThreshold)
                        {
                            minTime = time;
                            threat = other;
                        }
                    }
                }
            }

            // if a potential collision was found, compute steering to avoid
            if (threat != null)
            {
                // parallel: +1, perpendicular: 0, anti-parallel: -1
                double parallelness = Vector3d.DotProduct(vehicle.Forward, threat.Forward);
                const double ANGLE = 0.707f;

                if (parallelness < -ANGLE)
                {
                    // anti-parallel "head on" paths:
                    // steer away from future threat position
                    Vector3d offset = xxxThreatPositionAtNearestApproach - vehicle.Position;
                    double sideDot = Vector3d.DotProduct(offset, vehicle.Side);
                    steer = (sideDot > 0) ? -1.0f : 1.0f;
                }
                else
                {
                    if (parallelness > ANGLE)
                    {
                        // parallel paths: steer away from threat
                        Vector3d offset = threat.Position - vehicle.Position;
                        double sideDot = Vector3d.DotProduct(offset, vehicle.Side);
                        steer = (sideDot > 0) ? -1.0f : 1.0f;
                    }
                    else
                    {
                        // perpendicular paths: steer behind threat
                        // (only the slower of the two does this)
                        if (threat.Speed <= vehicle.Speed)
                        {
                            double sideDot = Vector3d.DotProduct(vehicle.Side, threat.Velocity);
                            steer = (sideDot > 0) ? -1.0f : 1.0f;
                        }
                    }
                }

                if (annotation != null)
                    annotation.AvoidNeighbor(threat, steer, xxxOurPositionAtNearestApproach, xxxThreatPositionAtNearestApproach);
            }

            return vehicle.Side * steer;
        }

        /// <summary>
        /// Given two vehicles, based on their current positions and velocities,
        /// determine the time until nearest approach
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        private static double PredictNearestApproachTime(IVehicle vehicle, IVehicle other)
        {
            // imagine we are at the origin with no velocity,
            // compute the relative velocity of the other this
            Vector3d myVelocity = vehicle.Velocity;
            Vector3d otherVelocity = other.Velocity;
            Vector3d relVelocity = otherVelocity - myVelocity;
            double relSpeed = relVelocity.Length;

            // for parallel paths, the vehicles will always be at the same distance,
            // so return 0 (aka "now") since "there is no time like the present"
            if (Math.Abs(relSpeed - 0) < float.Epsilon)
                return 0;

            // Now consider the path of the other this in this relative
            // space, a line defined by the relative position and velocity.
            // The distance from the origin (our this) to that line is
            // the nearest approach.

            // Take the unit tangent along the other this's path
            Vector3d relTangent = relVelocity / relSpeed;

            // find distance from its path to origin (compute offset from
            // other to us, find length of projection onto path)
            Vector3d relPosition = vehicle.Position - other.Position;
            double projection = Vector3d.DotProduct(relTangent, relPosition);

            return projection / relSpeed;
        }

        /// <summary>
        /// Given the time until nearest approach (predictNearestApproachTime)
        /// determine position of each this at that time, and the distance
        /// between them
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="other"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        private static double ComputeNearestApproachPositions(IVehicle vehicle, IVehicle other, double time)
        {
            Vector3d myTravel = vehicle.Forward * vehicle.Speed * time;
            Vector3d otherTravel = other.Forward * other.Speed * time;

            Vector3d myFinal = vehicle.Position + myTravel;
            Vector3d otherFinal = other.Position + otherTravel;

            return Vector3d.GetDistance3d(myFinal, otherFinal);
        }

        public static bool IsAhead(this IVehicle vehicle, Vector3d target, double cosThreshold = 0.707f)
        {
            Vector3d targetDirection = Vector3d.Normalize(target - vehicle.Position);
            return Vector3d.DotProduct(vehicle.Forward, targetDirection) > cosThreshold;
        }

        public static bool IsAside(this IVehicle vehicle, Vector3d target, double cosThreshold = 0.707f)
        {
            Vector3d targetDirection = Vector3d.Normalize(target - vehicle.Position);
            double dp = Vector3d.DotProduct(vehicle.Forward, targetDirection);
            return (dp < cosThreshold) && (dp > -cosThreshold);
        }

        public static bool IsBehind(this IVehicle vehicle, Vector3d target, double cosThreshold = -0.707f)
        {
            Vector3d targetDirection = Vector3d.Normalize(target - vehicle.Position);
            return Vector3d.DotProduct(vehicle.Forward, targetDirection) < cosThreshold;
        }

        private static bool IsInBoidNeighborhood(this ILocalSpaceBasis vehicle, ILocalSpaceBasis other, double minDistance, double maxDistance, double cosMaxAngle)
        {
            if (other == vehicle)
                return false;

            Vector3d offset = other.Position - vehicle.Position;
            double distanceSquared = offset.LengthSquared();

            // definitely in neighborhood if inside minDistance sphere
            if (distanceSquared < (minDistance * minDistance))
                return true;

            // definitely not in neighborhood if outside maxDistance sphere
            if (distanceSquared > (maxDistance * maxDistance))
                return false;

            // otherwise, test angular offset from forward axis
            Vector3d unitOffset = offset / Math.Sqrt(distanceSquared);
            double forwardness = Vector3d.DotProduct(vehicle.Forward, unitOffset);
            return forwardness > cosMaxAngle;
        }
    }
}
