using Keystone.Types;

namespace Steering.Pathway
{
    public interface IPathway
    {
        /// <summary>
        /// Given an arbitrary point ("A"), returns the nearest point ("P") on
        /// this path.  Also returns, via output arguments, the path tangent at
        /// P and a measure of how far A is outside the Pathway's "tube".  Note
        /// that a negative distance indicates A is inside the Pathway.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="tangent"></param>
        /// <param name="outside"></param>
        /// <returns></returns>
        Vector3d MapPointToPath(Vector3d point, out Vector3d tangent, out double outside);

        /// <summary>
        /// given a distance along the path, convert it to a point on the path
        /// </summary>
        /// <param name="pathDistance"></param>
        /// <returns></returns>
        Vector3d MapPathDistanceToPoint(double pathDistance);

        /// <summary>
        /// Given an arbitrary point, convert it to a distance along the path.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        double MapPointToPathDistance(Vector3d point);
        
        bool HasArrivedAtEndPath (Vector3d point);
        
    }
}
