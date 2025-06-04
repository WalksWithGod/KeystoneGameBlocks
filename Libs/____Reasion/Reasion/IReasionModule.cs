using Gdk;
using System;
using System.Collections.Specialized;

namespace Reasion
{
	public interface IReasionModule
	{
		int Width { get; }
		int Height { get; }
		uint Ticks { get; }
		uint[] Palette { get; set; }
		ModuleParameter[] Parameters { get; }
		
		bool Initialize();
		bool Iterate();
		void Render(Gdk.Image image);
	}
}