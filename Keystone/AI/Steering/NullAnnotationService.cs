
using Keystone.Types;

namespace Steering
{
    class NullAnnotationService
        :IAnnotationService
    {
        public bool IsEnabled
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        public void Line(Vector3d startPoint, Vector3d endPoint, Color color, double  opacity = 1)
        {

        }

        public void CircleXZ(double radius, Vector3d center, Color color, int segments)
        {

        }

        public void DiskXZ(double radius, Vector3d center, Color color, int segments)
        {

        }

        public void Circle3D(double  radius, Vector3d center, Vector3d axis, Color color, int segments)
        {

        }

        public void Disk3D(double  radius, Vector3d center, Vector3d axis, Color color, int segments)
        {

        }

        public void CircleOrDiskXZ(double  radius, Vector3d center, Color color, int segments, bool filled)
        {

        }

        public void CircleOrDisk3D(double  radius, Vector3d center, Vector3d axis, Color color, int segments, bool filled)
        {

        }

        public void CircleOrDisk(double  radius, Vector3d axis, Vector3d center, Color color, int segments, bool filled, bool in3D)
        {

        }

        public void AvoidObstacle(double  minDistanceToCollision)
        {

        }

        public void PathFollowing(Vector3d future, Vector3d onPath, Vector3d target, double  outside)
        {

        }

        public void AvoidCloseNeighbor(IVehicle other, double  additionalDistance)
        {

        }

        public void AvoidNeighbor(IVehicle threat, double  steer, Vector3d ourFuture, Vector3d threatFuture)
        {

        }

        public void VelocityAcceleration(IVehicle vehicle)
        {

        }

        public void VelocityAcceleration(IVehicle vehicle, double  maxLength)
        {

        }

        public void VelocityAcceleration(IVehicle vehicle, double  maxLengthAcceleration, double  maxLengthVelocity)
        {

        }
    }
}
