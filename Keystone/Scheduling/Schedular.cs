using System;
using System.Collections.Generic;
using MTV3D65;

namespace Keystone.Scheduling
{
    public class Schedular : ISchedular
    {
        #region ISchedular Members

        public List<ITask> Tasks
        {
            get { throw new NotImplementedException(); }
        }

        public int Hertz
        {
            set { throw new NotImplementedException(); }
        }

        public double CPU
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public int Quantum
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Update the priorities of all tasks based on distance to the camera.
        /// Then start to loop and callback the highest priority shadowVolume.
        /// (For now we will limit the max loops per frame to two)
        /// NOTE: If a given LandShadowVolume has 0 subscribers, there is no need to udpate it
        /// unless it is Dirty (e.g. previously had some casters and now it doesnt)
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="elapsed"></param>
        public void Update(TVCamera camera, int elapsed)
        {
        }

        public void ProcessTasks(int elapsed)
        {
            throw new NotImplementedException();
        }

        public void Add(ITask item)
        {
            throw new NotImplementedException();
        }

        public void Remove(ITask item)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}