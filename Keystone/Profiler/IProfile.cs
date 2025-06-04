using System;

namespace Keystone.Profiler
{
	/// <summary>
	/// Description of IProfile.
	/// </summary>
    public interface IProfile
    {
        
        void Update (float elapsed);
        void ResetTimer();
    }
}
