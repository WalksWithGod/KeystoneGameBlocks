using System;
using System.Collections.Generic;
using System.Text;

namespace Celestial
{
    
    // following is same as "move around point" function that tv3d does.
//sngCameraPosX = sngPlanetPos.X + dblCameraDistanceToPlanet * sin(dblCameraPitchAngle) * cos(dblCameraYawAngle)
//sngCameraPosY = sngPlanetPos.Y + dblCameraDistanceToPlanet * cos(dblCameraPitchAngle)   
//sngCameraPosZ = sngPlanetPos.z + dblCameraDistanceToPlanet * sin(dblCameraPitchAngle) * sin(dblCameraYawAngle)   
//Scene.SetCamera sngCameraPosX, sngCameraPosY, sngCameraPosZ, sngPlanetPos.X, sngPlanetPos.Y, sngPlanetPos.z 
    public class Orbit
    {
        float Radius;
        float Period;
        float Eccentricity; 
    float MinSeperation;
        float MaxSeperation;
    }
}
