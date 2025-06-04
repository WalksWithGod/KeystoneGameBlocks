using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Keystone.Simulation.Missions
{
    public class MissionData
    {
        Settings.Initialization mData;

        public void Load(string path)
        {
            mData = Settings.Initialization.Load(path);
        }

        public void Save(string path)
        {
            Settings.Initialization.Save(path, mData);
        }
    }
}
