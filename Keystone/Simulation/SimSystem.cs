using System;
using Keystone.Entities;

namespace Keystone.Simulation
{
	/// <summary>
	/// Systems can be added together just like InputController behavior logic?
	/// e.g Wind, RainStorms
	/// </summary>
	public interface ISimSystem : IDisposable 
	{
		
		ISimulation Simulation {get; set;}
		
		string Name {get;}
		uint HertzInTimesPerSecond {get;set;} // eg: 1 - 100x a second
		bool Paused {get;set;}
		
		void Update();
		void UnRegister(Entity entity);
        void Register(Entity entity);

	}
	
	
}
