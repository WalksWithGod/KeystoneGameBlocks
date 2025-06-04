using System;
using Amib.Threading;
using Keystone.Commands;
using Keystone.Scene;
using Keystone.Simulation;

namespace Keystone
{
    public interface ICore
    {
        //ICore CoreInstance { get; }
        SmartThreadPool ThreadPool { get; }
        CommandProcessor CommandProcessor { get; }
        SceneManagerBase SceneManager { get; set; }
        //ISimulation Simulation { get; set; }
        string DataPath { get; }
        string AppPath { get; }
        bool IsInitialized { get; }

        string GetNewName(Type type);
        void Shutdown();
    }
}