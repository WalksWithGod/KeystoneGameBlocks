using System;

namespace KeyCommon.Simulation
{
	// invoked in scripted method and returns a Transmitter that contains the discrete
    // unit of transmittable data for that tick.
    public delegate Transmitter Transmission_Delegate (string entityID, double elapsedSeconds);
    
	/// <summary>
	/// Description of Transmission.
	/// </summary>
	public struct Transmission
	{
        // struct is populated by our script and returned to the caller
        // which is charged with distributing transmissions
	}
}
