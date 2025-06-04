using System;
using System.Diagnostics;

namespace Keystone.Profiler
{
	/// <summary>
	/// Description of ProfileHook.
	/// </summary>
	internal class ProfileHook : IProfileHook
    {

        private IProfile HookedProfile;

		private long mStartCounter;
		
        public ProfileHook(IProfile Hook)
        {
        	if (Hook == null) throw new ArgumentNullException ("ProfileHook.ctor() - Hook cannot be null.");
            this.HookedProfile = Hook;

            mStartCounter = Keystone.Profiler.Time.Counter; 
          
        }

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
            	HookedProfile.Update ((float)Time.ElapsedSeconds (mStartCounter));
                HookedProfile = null;
            }
            this.disposedValue = true;
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
