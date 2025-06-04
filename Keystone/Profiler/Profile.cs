using System;
using System.Diagnostics;

namespace Keystone.Profiler
{
	/// <summary>
	/// Description of Profile.
	/// </summary>
	internal class Profile : IProfile
    {

        private string ProfileName;
        private string ProfileCategory;

     //   private long StartTime;
        private float TotalTime;
        private float LastTotal;
       // private bool HookedUp; // TODO: HookedUp var is never used. might be useful for re-entrant sync, but so far it's not used
        //private Stopwatch _watch;

        public Profile(string Name)
        {
            this.ProfileName = Name;
        }
        public Profile(string Name, string Category)
        {
            this.ProfileName = Name;
            this.ProfileCategory = Category;
        }

        
        public void Update (float elapsed)
        {
        	TotalTime += elapsed;
        }

        public void ResetTimer()
        {
            LastTotal = TotalTime;
            TotalTime = 0;
        }

        // we cache the last elapsed since we will only update the display every x interval.
        // thus, our display isn't erratic
        public float ElapsedSeconds
        {
            get { return LastTotal; }
        }
        
        public float ElapsedMilliseconds { get {return LastTotal * 1000f;}}

        public bool Categorized
        {
            get { return ProfileCategory != null; }
        }

        public string Category
        {
            get { return ProfileCategory; }
        }

        public string Name
        {
            get { return ProfileName; }
        }
    }
}
