using System;
using System.Windows.Forms;
using DevComponents.DotNetBar;

namespace KeyEdit.Workspaces
{
	/// <summary>
	/// partial class of EditorWorkspace for ToolbarTerrain.
	/// We dynamically swap the RibbonBar on the ViewportControl for a
	/// special one for editing Terrain when a Terrain is selected
	/// </summary>
	public partial class EditorWorkspace
	{
		RibbonBar mTerrainToolbar;
		private DevComponents.DotNetBar.ButtonItem buttonRaise;
		private DevComponents.DotNetBar.ButtonItem buttonLower;
		
		

		

		// if you click on a terrain, we select this toolbar
		// if engaged in operation, we do not. 
		// it has to be when entity has changed
		private void InitializeTerrainToolbar()
		{


			if (ViewportControls[0].InvokeRequired)
			{
				
				if (mTerrainToolbar == null)
				{
								
					ViewportControls[0].BeginInvoke (
					(MethodInvoker)delegate {
			    	
					InitializeTerrainToolbarButtons();
					
					mTerrainToolbar = new DevComponents.DotNetBar.RibbonBar();
		            mTerrainToolbar.Name = "mTerrainToolbar";
					mTerrainToolbar.Dock = System.Windows.Forms.DockStyle.Top;
					mTerrainToolbar.Location = new System.Drawing.Point(0, 4);
					mTerrainToolbar.Size = new System.Drawing.Size(615, 39);
					mTerrainToolbar.ItemSpacing = 5;
					mTerrainToolbar.TitleVisible = false;
					mTerrainToolbar.TabIndex = 1;
		            mTerrainToolbar.AutoOverflowEnabled = true;
		            mTerrainToolbar.ContainerControlProcessDialogKey = true;
		            
		                        
		            mTerrainToolbar.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
					mTerrainToolbar.BackgroundMouseOverStyle.Class = "";
					mTerrainToolbar.BackgroundMouseOverStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
					mTerrainToolbar.BackgroundStyle.Class = "";
					mTerrainToolbar.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
				
					
					mTerrainToolbar.TitleStyle.Class = "";
					mTerrainToolbar.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
					
					mTerrainToolbar.TitleStyleMouseOver.Class = "";
					mTerrainToolbar.TitleStyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
					
					mTerrainToolbar.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
											this.buttonRaise,
											this.buttonLower});
					});
				}
				
				ViewportControls[0].BeginInvoke (
					(MethodInvoker)delegate {
						mViewportControls[0].SetToolbar(mTerrainToolbar);
					});
			}
			

		}
		
		private void InitializeTerrainToolbarButtons()
		{
			
			buttonRaise = new DevComponents.DotNetBar.ButtonItem();
			buttonLower = new DevComponents.DotNetBar.ButtonItem();
			
			// 
			// buttonRaise
			// 
			buttonRaise.Image = global::KeyEdit.Properties.Resources.raiseImage;
			buttonRaise.ImageFixedSize = new System.Drawing.Size(32, 32);
			buttonRaise.ImagePaddingHorizontal = 0;
			buttonRaise.ImagePaddingVertical = 0;
			buttonRaise.ItemAlignment = DevComponents.DotNetBar.eItemAlignment.Center;
			buttonRaise.Name = "buttonRaise";
			buttonRaise.RibbonWordWrap = false;
			buttonRaise.SubItemsExpandWidth = 14;
			buttonRaise.Text = "buttonRaise";
			buttonRaise.Tooltip = "Raise Terrain";
			buttonRaise.Click += new System.EventHandler(buttonRaise_Click);
			// 
			// buttonLower
			// 
			buttonLower.Image = global::KeyEdit.Properties.Resources.lowerImage;
			buttonLower.ImageFixedSize = new System.Drawing.Size(32, 32);
			buttonLower.ImagePaddingHorizontal = 0;
			buttonLower.ImagePaddingVertical = 0;
			buttonLower.ItemAlignment = DevComponents.DotNetBar.eItemAlignment.Center;
			buttonLower.Name = "buttonLower";
			buttonLower.RibbonWordWrap = false;
			buttonLower.SubItemsExpandWidth = 14;
			buttonLower.Text = "buttonLower";
			buttonLower.Tooltip = "Lower Terrain";
			buttonLower.Click += new System.EventHandler(buttonLower_Click);
			
		}
		
		
		private void buttonRaise_Click(object sender, EventArgs e)
		{
			Tools.HeightmapTool hmTool = new KeyEdit.Workspaces.Tools.HeightmapTool (AppMain.mNetClient);
			//this.ViewportControls[0].Viewport.Context.Workspace.CurrentTool = hmTool;
			this.CurrentTool = hmTool;
		}
		
		private void buttonLower_Click(object sender, EventArgs e)
		{
			//this.ViewportControls[0].Viewport.Context.Workspace.SelectTool(Keystone.EditTools.ToolType.Position);
			this.SelectTool (Keystone.EditTools.ToolType.Position);
		}
		
		// smooth
		// noise
		
		// buttonConvertLandscapeToMesh  (float bias) {  // allows us to often decrease vertex count by a ton since terrain often has lots of flat areas}
	}
}
