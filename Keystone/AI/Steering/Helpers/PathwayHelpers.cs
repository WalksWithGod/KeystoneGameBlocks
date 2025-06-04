using Keystone.Types;
using Steering.Pathway;

namespace Steering.Helpers
{
    public static class PathwayHelpers
    {
        /// <summary>
        /// is the given point inside the path tube?
        /// </summary>
        /// <param name="pathway"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static bool IsInsidePath(this IPathway pathway, Vector3d point)
		{
			double outside;
			Vector3d tangent;
            pathway.MapPointToPath(point, out tangent, out outside);
			return outside < 0;
		}

        /// <summary>
        /// how far outside path tube is the given point?  (negative is inside)
        /// </summary>
        /// <param name="pathway"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static double HowFarOutsidePath(this IPathway pathway, Vector3d point)
		{
			double outside;
			Vector3d tangent;
            pathway.MapPointToPath(point, out tangent, out outside);
			return outside;
		}
    }
}
