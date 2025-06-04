using System;
using System.Collections.Generic;

namespace Keystone
{
    public class CoreServer : ICore
    {

        internal static Core _Core;

        #region ICore Members
        public ICore CoreInstance { get { throw new NotImplementedException(); } }

        public Amib.Threading.SmartThreadPool ThreadPool
        {
            get { throw new NotImplementedException(); }
        }

        public Keystone.Commands.CommandProcessor CommandProcessor
        {
            get { throw new NotImplementedException(); }
        }

        public Keystone.Scene.SceneManagerBase SceneManager
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public Keystone.Simulation.ISimulation Simulation
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string DataPath
        {
            get { throw new NotImplementedException(); }
        }

        public string AppPath
        {
            get { throw new NotImplementedException(); }
        }

        public string ConfigPath
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsInitialized
        {
            get { throw new NotImplementedException(); }
        }


        public string GetNewName(Type type)
        {
            throw new NotImplementedException();
        }

        public void Shutdown()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
