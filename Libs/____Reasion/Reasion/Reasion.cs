using Gtk;
using Gdk;
using GLib;
using Glade;
using System;
using System.Timers;
using System.Reflection;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Reasion
{
	public class Reasion
	{
		IReasionModule runningModule, workingModule;
		Dictionary<string, string> moduleList;
		Dictionary<string, uint[]> paletteList;
		
		Gdk.Image drawingImage;
		uint renderTick;
		uint iterationDelay;
		System.Timers.Timer iterationTimer;
		System.Object threadLock = new System.Object();
		
		[Widget] Gtk.Window mainWindow = null;
		[Widget] DrawingArea drawingArea = null;
		[Widget] Label ticksLabel = null;
		[Widget] VBox optionsBox = null;
		[Widget] VBox moduleOptionsTableHolder = null;
		[Widget] Button moduleGenerateButton = null;
		[Widget] Table displayOptionsTable = null;
		[Widget] HScale iterationDelayScale = null;
		[Widget] HScale renderTickScale = null;
		ComboBox modulesComboBox = null;
		ComboBox palettesComboBox = null;
		Table moduleOptionsTable = null;
		
		public static void Main (string[] args)
		{
			Application.Init();
			new Reasion();
			Application.Run();
		}
		
		public Reasion()
		{
			// Load and connect Glade
			Glade.XML gxml = new Glade.XML("Reasion.glade", "mainWindow");
			gxml.Autoconnect(this);
			
			// Populate the palettes list with Title:uint[] pairs
			this.paletteList = new Dictionary<string, uint[]>();
			this.paletteList.Add("Adrift in Dreams",	new uint[] {11,72,107, 59,134,134, 121,189,154, 168,219,168, 207,240,158});
			this.paletteList.Add("War",					new uint[] {35,15,43, 242,29,85, 235,235,188, 188,227,197, 130,179,174});
			this.paletteList.Add("Hanger Management",	new uint[] {184,42,102, 184,195,178, 241,227,193, 221,206,189, 76,90,95});
			this.paletteList.Add("RGBW",				new uint[] {0,0,255, 0,255,0, 255,0,0, 255,255,255});
			this.paletteList.Add("White-Black",			new uint[] {0,0,0, 255,255,255});
			this.paletteList.Add("Barf",				new uint[] {0,255,255, 255,0,255, 0,255,255, 255,0,255, 0,255,255, 255,0,255, 0,255,255, 255,0,255, 0,255,255, 255,0,255});
			
			// Find IReasionModule modules and shove the Title:Type pair in _moduleList
			this.moduleList = new Dictionary<string, string>();
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (Type type in assembly.GetTypes())
				{	
					if (type.GetInterface(typeof(IReasionModule).FullName) != null)
					{
						MemberInfo membInfo = type;
						object[] attributes = membInfo.GetCustomAttributes(typeof(DisplayNameAttribute), true);
						if (attributes != null) { this.moduleList.Add(((DisplayNameAttribute)attributes[0]).DisplayName, type.ToString()); } 
							else { this.moduleList.Add(type.ToString(), type.ToString()); }
					}
				}
			}
			
			// Create comboboxes and settings table
			this.palettesComboBox = new ComboBox(new string[] {});
			foreach (string title in this.paletteList.Keys) { this.palettesComboBox.AppendText(title); }
			this.palettesComboBox.Active = 0;
			this.palettesComboBox.Changed += this.OnPalettesComboBoxChange;
			this.displayOptionsTable.Attach(this.palettesComboBox, 1, 2, 2, 3);
			
			this.modulesComboBox = new ComboBox(new string[] {});			
			foreach (string title in this.moduleList.Keys) { this.modulesComboBox.AppendText(title); }
			this.modulesComboBox.Active = 0;
			this.modulesComboBox.Changed += this.OnModulesComboBoxChange;
			this.optionsBox.PackStart(this.modulesComboBox, false, false, 0);
			this.optionsBox.ReorderChild(this.modulesComboBox, 4);
			
			// Create Module
			this.SetWorkingModule(this.moduleList[this.modulesComboBox.ActiveText]);
			this.SetRunningModule();
			this.renderTick = 1;
			this.iterationDelay = 50;
			this.iterationDelayScale.Value = this.iterationDelay;
			this.renderTickScale.Value = this.renderTick;
			
			// Create image and drawing area for the module
			this.drawingArea.ExposeEvent += RenderModule;

			// Resize window and show everything
			this.mainWindow.DeleteEvent += OnMainWindowDelete;
			this.mainWindow.ShowAll();
			
			// Hooks and shit
			this.moduleGenerateButton.Clicked += OnModuleGenerateButtonPress;
			this.renderTickScale.ChangeValue += OnRenderTickScaleChange;
			this.iterationDelayScale.ChangeValue += OnIterationDelayScaleChange;
			
			// Process the module and create a timer for it
			this.iterationTimer = new Timer();
			this.iterationTimer.Elapsed += new ElapsedEventHandler(this.ProcessModule);
			this.iterationTimer.Interval = this.iterationDelay;
			this.iterationTimer.Start();
		}
		
		private void SetRunningModule()
		{
			for (int i = 0; i < this.moduleOptionsTable.Children.Length; i += 2)
			{
				ModuleParameter p = this.workingModule.Parameters[(this.workingModule.Parameters.Length - (i / 2)) - 1];
				if (p.DisplayType == ParameterDisplayType.Slider)
				{
					HScale w = (HScale)this.moduleOptionsTable.Children[i];
					p.SetValue(w.Value);
					w.Value = p.Value;
				}
				else if (p.DisplayType == ParameterDisplayType.Text)
				{
					Entry w = (Entry)this.moduleOptionsTable.Children[i];
					p.SetValue(w.Text);
					w.Text = p.Value.ToString();
				}
			}
			
			this.runningModule = (IReasionModule)(Type.GetType(this.workingModule.ToString()).GetConstructor(Type.EmptyTypes).Invoke(null));
			for (int i = 0; i < this.runningModule.Parameters.Length; i++)
			{
				this.runningModule.Parameters[i].SetValue(this.workingModule.Parameters[i].Value);
			}
			this.runningModule.Initialize();
			SetPalette(this.paletteList[this.palettesComboBox.ActiveText]);
			this.drawingImage = new Gdk.Image(Gdk.ImageType.Fastest, this.mainWindow.Visual, this.runningModule.Width, this.runningModule.Height);
			this.drawingArea.SetSizeRequest(this.runningModule.Width, this.runningModule.Height);
		}
		
		private void SetWorkingModule(string type)
		{
			this.workingModule = (IReasionModule)(Type.GetType(type).GetConstructor(Type.EmptyTypes).Invoke(null));
			
			// Recreate module options table
			if (this.moduleOptionsTable != null) {
				this.moduleOptionsTableHolder.Remove(this.moduleOptionsTable);
				this.moduleOptionsTable.Destroy();
			}
			this.moduleOptionsTable = new Table((uint)this.workingModule.Parameters.Length, 2, true);
			for (uint i = 0; i < (uint)this.workingModule.Parameters.Length; i++)
			{
				ModuleParameter param = this.workingModule.Parameters[i];
				
				Label paramLabel = new Label(param.Name);
				this.moduleOptionsTable.Attach(paramLabel, 0, 1, i, i + 1);
				if (param.DisplayType == ParameterDisplayType.Slider)
				{
					HScale paramInput = new HScale(param.MinValue, param.MaxValue, param.Stepping);
					paramInput.Value = param.Value;
					this.moduleOptionsTable.Attach(paramInput, 1, 2, i, i + 1);
				}
				else if (param.DisplayType == ParameterDisplayType.Text)
				{
					Entry paramInput = new Entry(param.Value.ToString());
					paramInput.Alignment = 1;
					this.moduleOptionsTable.Attach(paramInput, 1, 2, i, i + 1);
				}
			}

			this.moduleOptionsTable.BorderWidth = 4;
			this.moduleOptionsTable.WidthRequest = 150;
			this.moduleOptionsTableHolder.PackStart(this.moduleOptionsTable, false, true, 0);
			this.moduleOptionsTableHolder.ShowAll();
		}
		
		private void SetPalette(uint[] palette)
		{
			this.runningModule.Palette = palette;
		}
		
		private void ProcessModule(object sender, ElapsedEventArgs e)
		{
			lock (this)
			{
				this.runningModule.Iterate();
				if (this.runningModule.Ticks % this.renderTick == 0)
				{
					this.drawingArea.QueueDraw();
					this.ticksLabel.Text = "<span size='small'>Ticks: " + this.runningModule.Ticks.ToString() + "</span>";
					this.ticksLabel.UseMarkup = true;
				}
			}
		}
	
		private void RenderModule(object obj, ExposeEventArgs args)
		{
			lock (this)
			{
				this.runningModule.Render(this.drawingImage);
				this.drawingArea.GdkWindow.DrawImage(drawingArea.Style.BaseGC(StateType.Normal), this.drawingImage, 0, 0, 0, 0, this.runningModule.Width, this.runningModule.Height);		
			}
		}
		
		private void OnModuleGenerateButtonPress(object o, EventArgs args)
		{
			this.SetRunningModule();
		}
		
		private void OnIterationDelayScaleChange(object o, EventArgs args)
		{
			this.iterationDelay = (uint)this.iterationDelayScale.Value;
			this.iterationTimer.Interval = this.iterationDelay;
		}
		
		private void OnRenderTickScaleChange(object o, EventArgs args)
		{
			this.renderTick = (uint)this.renderTickScale.Value;
		}
		
		private void OnModulesComboBoxChange(object o, EventArgs args)
		{
			this.SetWorkingModule(this.moduleList[this.modulesComboBox.ActiveText]);
		}
		
		private void OnPalettesComboBoxChange(object o, EventArgs args)
		{
			this.SetPalette(this.paletteList[this.palettesComboBox.ActiveText]);
		}
		
		static void OnMainWindowDelete(object o, DeleteEventArgs args)
		{
			Application.Quit();
		}
	}
}
