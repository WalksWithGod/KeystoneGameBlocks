using System;
using System.Windows.Forms;
using Keystone.Cameras;
using KeyEdit.Controls;
using MTV3D65;

namespace KeyEdit
{
    // TODO: switch back to PanelDockContainer after done with user design because (actually need to verify first) I think
    // the PanelDockContainer has automatic focus tracking when mouse enters/leaves?  Or maybe what they mean is
    // clicking on the container will automatically give focus to the tab?
    public partial class ViewportEditorControl :  ViewportControl  // DevComponents.DotNetBar.PanelDockContainer //: DockContent
    {
        public ViewportEditorControl() : base()
        {
            InitializeComponent();
        }

        public ViewportEditorControl(string name)
            : base(name)
        {
            InitializeComponent();
            Name = name; // must be done after InitializeComponent
        }


        #region ViewportControl 
        public override void ReadSettings(Settings.Initialization iniFile)
        {
       //     int sectionCount = iniFile.sectionCount("viewport");
        //    string sectionName = Name;

         //   if (iniFile.GetSection(sectionName) == null) return;

      //      View = (Viewport.ViewType)Settings.EnumHelper.EnumeratedMemberValue(typeof(Viewport.ViewType), iniFile.settingRead(sectionName, "viewtype"));
      //      Projection = (Viewport.ProjectionType)Settings.EnumHelper.EnumeratedMemberValue(typeof(Viewport.ProjectionType), iniFile.settingRead(sectionName, "projectiontype"));
          //  ShowGrid(iniFile.settingReadBool(sectionName, "showgrid"));
         //   ShowFPS(iniFile.settingReadBool(sectionName, "showfps"));
          //  ShowBoundingBoxes(iniFile.settingReadBool(sectionName, "showboundingboxes"));
       //     string strColor = iniFile.settingRead(sectionName, "backcolor");
      //      System.Drawing.ColorConverter colorConv = new System.Drawing.ColorConverter();
            //  string test = colorConv.ConvertToString (System.Drawing.Color.Red);
            //System.Drawing.Color sysColor = (System.Drawing.Color)colorConv.ConvertFromInvariantString(strColor);
            //{
            //    Keystone.Types.Color color = Keystone.Helpers.TVTypeConverter.FromSystemColor(sysColor);
            //    Viewport.BackColor = color;
            //}
            
            
            buttonShowCelestialGrid.Checked = (bool)mContext.GetCustomOptionValue (null, "show celestial grid");
            buttonShowMotionField.Checked = (bool)mContext.GetCustomOptionValue (null, "show motion field");
            buttonShowFPS.Checked = mContext.ShowFPS;
            buttonShowLineProfiler.Checked = mContext.ShowLineProfiler;
            buttonItemTimeScalingx1.Checked = true;
            buttonItem10meterps.Checked = true;
            buttonItemTimeScaling_Click (buttonItemTimeScalingx1, null);
        }

        public override void WriteSettings(Settings.Initialization iniFile)
        {
            iniFile.settingWrite(Name, "viewtype", View.ToString());
            iniFile.settingWrite(Name, "projectiontype", Projection.ToString());
         //   iniFile.settingWrite(Name, "showgrid", buttonItemGrid.Checked.ToString());
            //iniFile.settingWrite(Name, "showfps", "");
         //   iniFile.settingWrite(Name, "showboundingboxes", buttonItemBoundingBoxes.Checked.ToString());
            if (Viewport != null)
                iniFile.settingWrite(Name, "backcolor", Viewport.BackColor.ToString());
        }
#endregion

        private void toolStripButtonView_Click(object sender, EventArgs e)
        {
            if (sender as DevComponents.DotNetBar.ButtonItem == null) return;
            DevComponents.DotNetBar.ButtonItem item = (DevComponents.DotNetBar.ButtonItem)sender;

            switch (item.Text)
            {
                case "Top":
                    View = Viewport.ViewType.Top;
                    break;
                case "Bottom":
                    View = Viewport.ViewType.Bottom;
                    break;
                case "Left":
                    View = Viewport.ViewType.Left;
                    break;
                case "Right":
                    View = Viewport.ViewType.Right;
                    break;
                case "Front":
                    View = Viewport.ViewType.Front;
                    break;
                case "Back":
                    View = Viewport.ViewType.Back;
                    break;
                case "Free":
                    View = Viewport.ViewType.Free;
                    break;
            }

            buttonItemTop.Checked = false;
            buttonItemBottom.Checked = false;
            buttonItemLeft.Checked = false;
            buttonItemRight.Checked = false;
            buttonItemFront.Checked = false;
            buttonItemBack.Checked = false;
            buttonItemFree.Checked = false;

            // enable the current one
            item.Checked = true;
        }


        private void toolStripButtonProjection_Click(object sender, EventArgs e)
        {
            if (sender as DevComponents.DotNetBar.ButtonItem == null) return;
            DevComponents.DotNetBar.ButtonItem item = (DevComponents.DotNetBar.ButtonItem)sender;

            switch (item.Text)
            {
                case "Perspective":
                    Projection = Viewport.ProjectionType.Perspective;
                    break;

                case "Orthographic":
                    Projection = Viewport.ProjectionType.Orthographic;
                    break;
                case "Isometric":
                    Projection = Viewport.ProjectionType.Isometric;
                    break;
            }

            buttonItemPerspective.Checked = false;
            buttonItemOrthographic.Checked = false;
            buttonItemIsometric.Checked = false;

            // enable the current one
            item.Checked = true;
            buttonItemProjection.Text = item.Text;
        }

        private void buttonItemSpeed_Click(object sender, EventArgs e)
        {
            if (sender as DevComponents.DotNetBar.ButtonItem == null) return;
            DevComponents.DotNetBar.ButtonItem item = (DevComponents.DotNetBar.ButtonItem)sender;
            float speed = 0;
            switch (item.Text)
            {
                case "1 m/s":
                    speed = 1;
                    break;
                case "10 m/s":
                    speed = 10;
                    break;
                case "100 m/s":
                    speed = 100;
                    break;
                case "1,000 m/s":
                    speed = 1000;
                    break;
                case "10,000 m/s":
                    speed = 10000;
                    break;
                case "100,000 m/s":
                    speed = 100000;
                    break;
                case "1,000,000 m/s":
                    speed = 1000000; // an extra 0 here
                    break;
                case "10,000,000 m/s":
                    speed = 10000000;
                    break;
                case "100,000,000 m/s":
                    speed = 100000000;
                    break;
                case "299,792,458 m/s": // speed of light
                    speed = 299792458;
                    break;
                case "1,000,000,000 m/s":
                    speed = 1000000000;
                    break;
                case "10,000,000,000 m/s":
                    speed = 10000000000;
                    break;
                case "100,000,000,000 m/s":
                    speed = 100000000000;
                    break;
                case "1,000,000,000,000 m/s":
                    speed = 1000000000000;
                    break;
                default:
                    speed = 10;
                    break;
                    
            }

            // disable all of them to ensure we get the right one
            buttonItem1000000000000meterps.Checked = false;
            //buttonItem100000000000meterps.Checked = false; 
            buttonItem10000000000meterps.Checked = false; 
            buttonItem1000000000meterps.Checked = false;
            buttonItem299792458meterps.Checked = false;
            buttonItem100000000meterps.Checked = false; 
            buttonItem10000000meterps.Checked = false; 
            buttonItem1000000meterps.Checked = false; 
            buttonItem100000meterps.Checked = false;
            buttonItem10000meterps.Checked = false;
            buttonItem1000meterps.Checked = false;
            buttonItem100meterps.Checked = false;
            buttonItem10meterps.Checked = false;
            buttonItem1meterps.Checked = false;
            
            // enable the current one
            item.Checked = true;
            buttonItemSpeed.Text = item.Text;
            
            //AppMain._core.CommandProcessor.EnQueue("cam_speed " + pow);
            // TODO: this is nasty. ideally this would send a console command to the engine so that we dont have too much of the core exposed directly to the UI
            //mContext.SetCustomOptionValue (null, "cam_speed", speed);
            mContext.Viewpoint.BlackboardData.SetFloat ("cam_speed", speed);
        }

        private void buttonItemZoom_Click(object sender, EventArgs e)
        {
            if (sender as DevComponents.DotNetBar.ButtonItem == null) return;
            DevComponents.DotNetBar.ButtonItem item = (DevComponents.DotNetBar.ButtonItem)sender;
            float pow = 0;
            switch (item.Text)
            {
                case "x1":
                    pow = 1;
                    break;
                case "x10":
                    pow = 10;
                    break;
                case "x100":
                    pow = 100;
                    break;
                case "x1,000":
                    pow = 1000;
                    break;
                case "x10,000":
                    pow = 10000;
                    break;
                case "x100,000":
                    pow = 100000;
                    break;
                case "x1,000,000":
                    pow = 1000000;
                    break;
                case "x10,000,000":
                    pow = 10000000;
                    break;
                case "x100,000,000":
                    pow = 100000000;
                    break;
                case "x1,000,000,000":
                    pow = 1000000000;
                    break;
                case "x10,000,000,000":
                    pow = 10000000000;
                    break;
                case "x100,000,000,000":
                    pow = 100000000000;
                    break;
                case "x1,000,000,000,000":
                    pow = 1000000000000;
                    break;
                default:
                    pow = 10;
                    break;
            }


            // disable all of them to ensure we get the right one
            buttonItemX1000000000000.Checked = false;
            buttonItemX100000000000.Checked = false;
            buttonItemX10000000000.Checked = false;
            buttonItemX1000000000.Checked = false;
            buttonItemX100000000.Checked = false;
            buttonItemX10000000.Checked = false;
            buttonItemX1000000.Checked = false;
            buttonItemX100000.Checked = false;
            buttonItemX10000.Checked = false;
            buttonItemX1000.Checked = false;
            buttonItemX100.Checked = false;
            buttonItemX10.Checked = false;
            buttonItemX1.Checked = false;

            // enable the current one
            item.Checked = true;
            buttonItemZoom.Text = item.Text;
            
            //AppMain._core.CommandProcessor.EnQueue("cam_zoom " + pow);
            // TODO: this is nasty. ideally this would send a console command to the engine so that we dont have too much of the core exposed directly to the UI
            //_viewport.Context.SetCustomOptionValue (null, "cam_zoom", pow);
            mContext.Viewpoint.BlackboardData.SetFloat("cam_zoom", pow);
        }

        private void buttonItemTimeScaling_Click(object sender, EventArgs e)
        {
            if (sender as DevComponents.DotNetBar.ButtonItem == null) return;
            DevComponents.DotNetBar.ButtonItem item = (DevComponents.DotNetBar.ButtonItem)sender;
            float pow = 1;
            switch (item.Text)
            {
                case "Time Scaling x1":
                    pow = 1;
                    break;
                case "Time Scaling x2":
                    pow = 2;
                    break;
                case "Time Scaling x10":
                    pow = 10;
                    break;
                case "Time Scaling x100":
                    pow = 100;
                    break;
                case "Time Scaling x1000":
                    pow = 1000;
                    break;
                default:
                    pow = 1;
                    break;
            }

            // disable all of them to ensure we get the right one
            buttonItemTimeScalingx1.Checked = false;
            buttonItemTimeScalingx2.Checked = false;
            buttonItemTimeScalingx10.Checked = false;
            buttonItemTimeScalingx100.Checked = false;
            buttonItemTimeScalingx1000.Checked = false;
            
            // enable the current one
            item.Checked = true;
            buttonItemTimeScaling.Text = item.Text;
            
            AppMain.SetTimeScale (pow);
        }
        
        // TODO: shouldn't all of these go through our InputController (aka EditorController) 
        // function interpreter
        private void buttonPlay_Click (object sender, EventArgs e)
        {
        	if (sender as DevComponents.DotNetBar.ButtonItem == null) return;
            DevComponents.DotNetBar.ButtonItem item = (DevComponents.DotNetBar.ButtonItem)sender;
          
            item.Checked = !item.Checked;
            // checked == paused 
            Viewport.Context.Scene.Simulation.Running = !item.Checked;
        }

        
        private void buttonPlayMission_Click(object sender, EventArgs e)
        {
            if (sender as DevComponents.DotNetBar.ButtonItem == null) return;
            DevComponents.DotNetBar.ButtonItem item = (DevComponents.DotNetBar.ButtonItem)sender;

            item.Checked = !item.Checked;


            AppMain._core.SceneManager.Scenes[0].Simulation.EnableMission(item.Checked);
            Workspaces.WorkspaceBase3D workspace3d = AppMain.Form.mWorkspaceManager.CurrentWorkspace as Workspaces.WorkspaceBase3D;
            if (workspace3d == null) return;

            workspace3d.NotifyPlugin();

        }

        private void buttonSelect_Click(object sender, EventArgs e)
        {
            Viewport.Context.Workspace.SelectTool(Keystone.EditTools.ToolType.Select);
        }

        private void buttonTranslate_Click(object sender, EventArgs e)
        {
            Viewport.Context.Workspace.SelectTool(Keystone.EditTools.ToolType.Position);
        }

        private void buttonRotate_Click(object sender, EventArgs e)
        {
            Viewport.Context.Workspace.SelectTool(Keystone.EditTools.ToolType.Rotate);
        }

        private void buttonScale_Click(object sender, EventArgs e)
        {
            Viewport.Context.Workspace.SelectTool(Keystone.EditTools.ToolType.Scale);
        }

        private void buttonItemZoomExtents_Click(object sender, EventArgs e)
        {
            // Zooming and Move To have same overall effect
            Keystone.Entities.Entity target = null;
            if ( this.Viewport.Context.Workspace.SelectedEntity != null)
            	target =  this.Viewport.Context.Workspace.SelectedEntity.Entity;
// TODO: show a relative velocity indicator of the current selected entity in the HUD            	
//            if (target is Keystone.Vehicles.Vehicle)
//				this.Viewport.Context.Viewpoint_Chase(target, 0.9f);
//	            this.Viewport.Context.Viewpoint.CustomData.SetBool ("mode_chase", true);
//    		else 
//				this.Viewport.Context.Viewpoint.CustomData.SetBool ("mode_free", true);
    			
    		// this.Viewport.Context.Viewpoint.CustomData.SetBool ("mode_orbit", true);    
//				this.Viewport.Context.Viewpoint_Orbit(target, 0.9f);
    		
            if (target != null)
                this.Viewport.Context.Viewpoint_MoveTo(target, 0.9f, false);
            else
                MessageBox.Show ("No target entity is selected.");
        }


        		
		void ButtonItemOrbitClick(object sender, EventArgs e)
		{
			Keystone.Entities.Entity target = null;
            if ( this.Viewport.Context.Workspace.SelectedEntity != null)
            	target =  this.Viewport.Context.Workspace.SelectedEntity.Entity;
            
            if (target != null)
            {
	            Keystone.Entities.Entity viewpoint = this.Viewport.Context.Viewpoint;  
	            viewpoint.BlackboardData.SetString ("focus_entity_id", target.ID);
	            viewpoint.BlackboardData.SetString ("control", "user");   
				viewpoint.BlackboardData.SetString ("behavior", "behavior_orbit");             
			}
            else
                MessageBox.Show ("No target entity is selected.");

		}
		
		
        private void buttonTextColor_SelectedColorPreview(object sender, DevComponents.DotNetBar.ColorPreviewEventArgs e)
        {
            DevComponents.DotNetBar.BaseItem item = sender as DevComponents.DotNetBar.BaseItem;
            Keystone.Types.Color color = new Keystone.Types.Color(e.Color.R, e.Color.G, e.Color.B, e.Color.A);
            Viewport.BackColor = color;
        }

        private void buttonTextColor_SelectedColorChanged(object sender, EventArgs e)
        {
            DevComponents.DotNetBar.BaseItem item = sender as DevComponents.DotNetBar.BaseItem;

            Keystone.Types.Color _backColor = new Keystone.Types.Color(((DevComponents.DotNetBar.ColorPickerDropDown)item).SelectedColor.R,
                    ((DevComponents.DotNetBar.ColorPickerDropDown)item).SelectedColor.G,
                    ((DevComponents.DotNetBar.ColorPickerDropDown)item).SelectedColor.B,
                    ((DevComponents.DotNetBar.ColorPickerDropDown)item).SelectedColor.A);

            Viewport.BackColor = _backColor;

            //Workspaces.WorkspaceBase ws = (Workspaces.WorkspaceBase)mWorkspaceManager.CurrentWorkspace;

            //for (int i = 0; i < ws.ViewportControls.Length; i++)
            //    if (ws.ViewportControls[i] != null)
            //    {
            //        string sectionName = ws.ViewportControls[i].Name;
            //        _core.Settings.settingWrite(sectionName, "backcolor", ws.ViewportControls[i].Viewport.BackColor.ToString());
            //    }
        }


        private void buttonItemDisplayMode(object sender, EventArgs e)
        {
            Workspaces.EditorWorkspace ws = (Workspaces.EditorWorkspace)mContext.Workspace;
 
            switch (((DevComponents.DotNetBar.BaseItem)sender).Name)
            {
                case "buttonItemDisplaySingleViewport":
                    ws.SetDisplayMode(KeyEdit.Controls.ViewportsDocument.DisplayMode.Single);
                    break;

                case "buttonItemDisplayVSplit":
                    ws.SetDisplayMode(KeyEdit.Controls.ViewportsDocument.DisplayMode.VSplit);
                    break;

                case "buttonItemDisplayHSplit":
                    ws.SetDisplayMode(KeyEdit.Controls.ViewportsDocument.DisplayMode.HSplit);
                    break;

                case "buttonItemDisplayTripleLeft":
                    ws.SetDisplayMode(KeyEdit.Controls.ViewportsDocument.DisplayMode.TripleLeft);
                    break;

                case "buttonItemDisplayTripleRight":
                    ws.SetDisplayMode(KeyEdit.Controls.ViewportsDocument.DisplayMode.TripleRight);
                    break;

                case "buttonItemDisplayQuad":
                    ws.SetDisplayMode(KeyEdit.Controls.ViewportsDocument.DisplayMode.Quad);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void mnuItemShowLightPositions_Click(object sender, EventArgs e)
        {
            //// ideally this would send a console command to the engine so that we dont have too much of the core exposed directly to the UI
            //// this way we can update the engine from console, network or whatever all the same way
            //mnuItemShowLightPositions.Checked = !mnuItemShowLightPositions.Checked;
            //Viewport.Context.ShowLightLocation = mnuItemShowLightPositions.Checked;
            ////_core.CommandProcessor.EnQueue();
        }

        private void mnuItemShowLightBounds_Click(object sender, EventArgs e)
        {
            //// ideally this would send a console command to the engine so that we dont have too much of the core exposed directly to the UI
            //mnuItemShowLightBounds.Checked = !mnuItemShowLightBounds.Checked;
            //Viewport.Context.ShowLightBoundingBoxes = mnuItemShowLightBounds.Checked;
        }




        private void buttonShowGrid_Click(object sender, EventArgs e)
        {
            buttonShowGrid.Checked = !buttonShowGrid.Checked;
            Viewport.Context.ShowLineGrid = buttonShowGrid.Checked;
        }

        private void buttonItemCelestialGrid_Click(object sender, EventArgs e)
		{
            buttonShowCelestialGrid.Checked = !buttonShowCelestialGrid.Checked;
            mContext.SetCustomOptionValue (null, "show celestial grid", buttonShowCelestialGrid.Checked);
		}
        
		private void ButtonItemShowAxisIndicatorClick(object sender, EventArgs e)
		{
            buttonShowAxisIndicator.Checked = !buttonShowAxisIndicator.Checked;
            mContext.SetCustomOptionValue (null, "show axis indicator", buttonShowAxisIndicator.Checked);
		}
        
		private void ButtonItemShowPathingDebugInfoClick(object sender, EventArgs e)
		{
            buttonShowPathingDebugInfo.Checked = !buttonShowPathingDebugInfo.Checked;
            mContext.SetCustomOptionValue (null, "show pathing debug information", buttonShowPathingDebugInfo.Checked);
		}
				
		void ButtonItemOrbitLinesClick(object sender, EventArgs e)
		{
            buttonShowOrbitLines.Checked = !buttonShowOrbitLines.Checked;
            mContext.SetCustomOptionValue (null, "show orbit lines", buttonShowOrbitLines.Checked);
		}
				
        private void ButtonItemMotionField_Click(object sender, EventArgs e)
		{
            buttonShowMotionField.Checked = !buttonShowMotionField.Checked;
            mContext.SetCustomOptionValue (null, "show motion field", buttonShowMotionField.Checked);
		}
		        
        private void buttonShowFPS_Click(object sender, EventArgs e)
        {
            buttonShowFPS.Checked = !buttonShowFPS.Checked;
            Viewport.Context.ShowFPS = buttonShowFPS.Checked;
        }

        private void buttonShowCullingStats_Click(object sender, EventArgs e)
        {
            buttonShowCullingStats.Checked = !buttonShowCullingStats.Checked;
            Viewport.Context.ShowCullingStats = buttonShowCullingStats.Checked;
        }        
        	
		private void buttonShowLineProfiler_Click(object sender, EventArgs e)
		{
            buttonShowLineProfiler.Checked = !buttonShowLineProfiler.Checked;
            Viewport.Context.ShowLineProfiler = buttonShowLineProfiler.Checked;
            AppMain._core.Profiler.ProfilerEnabled = buttonShowLineProfiler.Checked;
		}
		
        private void buttonShowTVProfiler_Click(object sender, EventArgs e)
        {
            buttonShowTVProfiler.Checked = !buttonShowTVProfiler.Checked;
            Viewport.Context.ShowTVProfiler = buttonShowTVProfiler.Checked;
        }

        private void buttonLabels_Click(object sender, EventArgs e)
        {
            buttonShowLabels.Checked = !buttonShowLabels.Checked;
            Viewport.Context.ShowEntityLabels = buttonShowLabels.Checked;
        }

        private void buttonShowBoundingBoxes_Click(object sender, EventArgs e)
        {
            buttonShowBoundingBoxes.Checked = !buttonShowBoundingBoxes.Checked;
            Viewport.Context.ShowEntityBoundingBoxes = buttonShowBoundingBoxes.Checked;
        }





               //case "buttonLineTool":
               //     ((EditController)AppMain._core.CurrentIOController).CurrentEditTool = new Keystone.EditTools.LineTool();
               //     break;
               // case "buttonRectangleTool":
               //     ((EditController)AppMain._core.CurrentIOController).CurrentEditTool = new Keystone.EditTools.RectangleTool();
               //     break;
               // //case "buttonCircleTool":
               // //    ((EditController)AppMain._core.CurrentIOController).CurrentEditTool = new Keystone.EditTools.CircleTool();
               // //    break;
               // //case "buttonPolygonTool":
               // //    ((EditController)AppMain._core.CurrentIOController).CurrentEditTool = new Keystone.EditTools.PolygonTool();
               // //    break;
               // case "buttonThrowProjectile":
               //     ((EditController)AppMain._core.CurrentIOController).CurrentEditTool = new Keystone.EditTools.ThrowObjectTool();
               //     break;
		
		void ButtonItemZoomClick(object sender, EventArgs e)
		{
			
		}
		

    }
}