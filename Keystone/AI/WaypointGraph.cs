using System;
using Keystone.Types;
using MTV3D65;

namespace Keystone.AI
{

    public class WaypointGraph 
    {
        private Vector3d[] _points;
        private double _epsilon;

        public WaypointGraph(Vector3d A, Vector3d B, Vector3d C, Vector3d D, uint numPoints, double epsilonInMeters)
        {
            if (numPoints == 0)
                _points = new Vector3d[] {A, B, C, D};
            else
                _points = BezierCurve.Get3DCubicBezierCurvePoints(A, B, C, D, numPoints);

            _epsilon = System.Math.Abs(epsilonInMeters);
        }

        public WaypointGraph(Vector3d A, Vector3d B, Vector3d C, uint numPoints, double epsilonInMeters)
        {
            if (numPoints == 0)
                _points = new Vector3d[] {A, B, C};
            else
                _points = BezierCurve.Get3DQuadraticBezierCurvePoints(A, B, C, numPoints);

            _epsilon = System.Math.Abs(epsilonInMeters);
        }

        public WaypointGraph(Vector3d A, Vector3d B, uint numPoints, double epsilonInMeters)
        {
            if (numPoints == 0)
                _points = new Vector3d[] {A, B};
            else
                _points = BezierCurve.Get3DBezierLinePoints(A, B, numPoints);

            _epsilon = System.Math.Abs(epsilonInMeters);
        }

        public int WaypointCount
        {
            get { return _points.Length; }
        }

        public Vector3d WayPointLocation(uint waypoint)
        {
            if (waypoint > _points.Length - 1)
                throw new ArgumentOutOfRangeException("Waypoint exceeds the bounds of total waypoints.");

            return _points[waypoint];
        }

        public uint GetNextWaypoint(Vector3d currentPosition, uint lastWaypoint)
        {
            if (lastWaypoint > _points.Length - 1)
                throw new ArgumentOutOfRangeException("Previous Waypoint exceeds the bounds of total waypoints.");

            // if we're already on the last waypoint, return the same index;
            if (lastWaypoint == _points.Length - 1) return lastWaypoint;

            // if we've reached the last waypoint, we can pass the next, else we still havent reached it so return it again
            if (AtWayPoint(currentPosition, lastWaypoint))
                return ++lastWaypoint;
            else
                return lastWaypoint;
        }

        public uint GetPrevWaypoint(Vector3d currentPosition, uint lastWaypoint)
        {
            if (lastWaypoint > _points.Length - 1)
                throw new ArgumentOutOfRangeException("Previous Waypoint exceeds the bounds of total waypoints.");

            // if we're already on the first waypoint, return the same index;
            if (lastWaypoint == 0) return 0;

            // if we've reached the last waypoint, we can pass the next, else we still havent reached it so return it again
            if (AtWayPoint(currentPosition, lastWaypoint))
                return --lastWaypoint;
            else
                return lastWaypoint;
        }

        public bool AtWayPoint(Vector3d currentPosition, uint waypoint)
        {
            if (waypoint > _points.Length - 1)
                throw new ArgumentOutOfRangeException("Waypoint exceeds the bounds of total waypoints.");

            return (currentPosition - _points[waypoint]).Length < _epsilon;
            return currentPosition.Equals(_points[waypoint]);
        }

        public void ReversePoints()
        {
            if (_points != null && _points.Length > 0)
            {
                Vector3d[] tmp = new Vector3d[_points.Length];
                for (int i = 0; i < _points.Length; i++)
                    tmp[i] = _points[_points.Length - i - 1];

                _points = tmp;
            }
        }

        //public void Draw()
        //{
        //    Line3d[] lines = new Line3d[_points.Length - 1];
        //    for (int i = 0; i < _points.Length - 1; i++)
        //        lines[i] = new Line3d(_points[i], _points[i + 1]);

        //    DebugDraw.DrawLines(lines, CONST_TV_COLORKEY.TV_COLORKEY_RED);
        //}
    }
}