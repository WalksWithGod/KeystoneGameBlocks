using System;

namespace KeyEdit.Workspaces.Huds
{
	/// <summary>
	/// Description of HealthBar. A 2D GUI element that gets sorted such that they don't
	/// overlap with the healthbars of other units.
	/// </summary>
	public class HealthBar
	{
		public HealthBar()
		{
			// implemented using 2 textured quads
			
			
			//AppMain.mScriptingHost.VisualFXAPI.DrawTexturedQuad (contextID, textureID, left, top, right, bottom, 0);
			
			// Screen2D.DRAW_FilledRectangle(BarXPos, BarYPos, BarXPos + (health/100 * 200), BarYPos + 30, rgba(1,1,1,0.5))

			// That will draw a white half-transparent filled bar at BarXPos, BarYPos that is 30 pixels high and will be proportional to your health at 200 pixels = 100%.
	
		}
	}
}
