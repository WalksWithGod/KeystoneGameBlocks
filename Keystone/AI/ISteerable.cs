using Keystone.Types;
using MTV3D65;

namespace Keystone.AI
{
    public interface ISteerable
    {
        double MaxSpeed { get; set; } // max speed
        double MaxForce { get; set; } // max acceleration & turning force
     
        Vector3d Acceleration { get; set; }
        Vector3d Velocity { get; set; }
        
        Vector3d Translation { get; set; }
        double Radius { get; }        // the bounding radius to use 
        
        // TODO: target and WanderTheta should be blackboard data?
        //Vector3d Target { get; set; }

        // the following are really just behavior characteristics and probably dont belong here.  
        // These shoudl be part of Behavior
        // e.g. FlockingBehavior,  WanderBehavior, etc.
        double WanderTheta { get; set; }
        
        // TODO: following are blackboard / userdata (Entity.UserData) vars
        //double visualRange;
        //double audioRange;
        //double olfactoryRange;
        //double sixthSenseRange;
    }
}