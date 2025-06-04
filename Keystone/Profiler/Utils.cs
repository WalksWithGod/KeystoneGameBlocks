using System;
using System.Collections.Generic;
    
    using System.Runtime.InteropServices;

namespace Keystone.Profiler
{
    class Utils
    {
        static uint frame_count = 0;
        static long last_fps_time = -1;

        static long last_frame_time = -1;

        public int GetFrequency()
        {
            if (last_frame_time < 0)
            {
                last_frame_time = DateTime.Now.Ticks;
                last_fps_time = last_frame_time;
            }
            long now = DateTime.Now.Ticks;
            long dt = now - last_frame_time;
            last_frame_time = now;

            int dt_fps = (int)(now - last_fps_time);
            if (dt_fps > 1)
            {
                System.Diagnostics.Debug.WriteLine (string.Format("{0} fps", frame_count / dt_fps));
                frame_count = 0;
                last_fps_time = DateTime.Now.Ticks;
            }
            ++frame_count;
            return dt_fps;
        }
    }
    


	/// <summary>
	/// Time management
	/// </summary>
	/// <remarks>The Profiler module uses this module to calculate the elapsed times</remarks>
	public class Time
    {
	    // The performance counter API has the best precision
	    [DllImport("Kernel32.dll")]
		private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);
		
		[DllImport("Kernel32.dll")]
		private static extern bool QueryPerformanceFrequency(out long lpFrequency);
	
	    public static long Counter
	    {
	        get
	        {
	            long R;
	            QueryPerformanceCounter(out R);
	            return R;
	    	}
	    }
	
	    private static long mFrequency = 0L;
	    public static long Frequency
	    {
	    	get 
	    	{
	            // Caches the frequency since it doesn't change
            	if (mFrequency == 0L)
	            	QueryPerformanceFrequency(out mFrequency);
	            return mFrequency;
	    	}
	    }
	    
	    public static double ElapsedSeconds (long startCounter)
	    {
	    	return (Counter - startCounter) *  (1D / (double)Time.Frequency);
	    	
	    }
    }
}
