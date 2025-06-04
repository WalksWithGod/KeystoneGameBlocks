namespace Keystone.Scheduling
{
    public interface ITask
    {
        int Priority { get; set; }
        int WaitTime { get; set; }

        //isCurrent  - if the camera is in the bounds of this object or if the camera is high overhead, if the camera is closest to this item compared to all the rest.
        //isScheduled
        //reschedule
        //scheduleAt
    }
}