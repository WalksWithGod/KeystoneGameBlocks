using System.Collections.Generic;
using MTV3D65;

namespace Keystone.Scheduling
{
    public interface ISchedular
    {
        // actually i think this list of tasks shoudl correspond to the list of ShadowVolumes (e.g. chunks)
        // So our "ChunkInfo" should be renamed to LandShadowVolumes and they should implement ITask
        // When Terrain is registered, a LandShadowVolume object is created for each chunk.  When a land
        // is unregistered, they are removed.
        List<ITask> Tasks { get; }

        /// <summary>
        /// Frequency expressed in itterations per second.  Eg. 30.  This value must be between 0 and 1000.
        /// </summary>
        int Hertz { set; }

        /// <summary>
        /// NOTE: Ive been contemplating this a bit more and I think maybe a better solution is to think in terms
        /// of CPU utilization.   If the elapsed time per frame = 30ms and we want shadow updates to be responsible
        /// for 10% of utilization, then we would only allow 3ms worth of processing each itteration.  This 
        /// is necessary because shadow rendering to our LightspaceRS and SHadowMapRS must be on the main thread
        /// because our GPU is only single threaded anyway so tryng to multithread the rendering itself wouldnt give
        /// us any advantage and might even be an unstable practice. 
        /// Valid values are between 0 and 100
        /// </summary>
        double CPU { get; set; }

        /// <summary>
        /// Readonly value that is computed based on the Hertz value. 
        /// It is the timeslice in milliseconds to spend processing this itteration.  
        /// Quantum = 1000 / Hertz 
        /// </summary>
        int Quantum { get; }

        /// <summary>
        /// Based on distance to the camera, we update the priorities of all the tasks.
        /// We also increment the waitTime for each task.
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="elapsed"></param>
        void Update(TVCamera camera, int elapsed);

        /// <summary>
        /// Within the Quantum allowed, process as many tasks this itteration as possible.
        /// </summary>
        /// <param name="elapsed"></param>
        void ProcessTasks(int elapsed);

        void Add(ITask item);
        void Remove(ITask item);
    }
}