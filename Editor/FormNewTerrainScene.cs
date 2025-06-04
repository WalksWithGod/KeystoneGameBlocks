using System;
using System.Drawing;
using System.Windows.Forms;

namespace KeyEdit
{
	/// <summary>
	/// Description of FormNewTerrainScene.
	/// </summary>
	public partial class FormNewTerrainScene : Form
	{
		public FormNewTerrainScene()
		{
			InitializeComponent();
			this.DialogResult = DialogResult.Cancel;
		}
		
		
		public string SceneName { get { return textSceneName.Text; } }
		// TODO: instead of diameter, we should have seperate width, height and depth that the regions will be
		//       because the root zone will encompass all of them.   And very often if we want to have the camera high up
		//       then the ceiling of each region needs to be higher
		//       Some Zones along the perimiter of the scene can be just empty zones for providing room to move the camera around

		
        public uint RegionsAcross { get { return uint.Parse(textRegionsAcross.Text); } }
        public uint RegionsHigh { get { return uint.Parse(textRegionsHigh.Text); } }
        public uint RegionsDeep { get { return uint.Parse(textRegionsDeep.Text); } }
        
        public uint RegionResolution { get { return uint.Parse(cboSectorResolution.Text); } }
        public uint TerrainTileCountY {get {return uint.Parse (cboTerrainTileCountY.Text); } }
        
        public float TileSizeX { get { return float.Parse(textTileSizeX.Text); } }
        public float TileSizeY { get { return float.Parse(textTileSizeY.Text); } }
        public float TileSizeZ { get { return float.Parse(textTileSizeZ.Text); } }
        
        public bool SerializeEmptyZones { get { return cbSerializeEmptyZones.Checked; } }
        
        public bool CreateEmptyTerrain {get{ return cbAddTerrainGeometry.Checked;}}
        
        
		void ButtonOKClick(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			
			double x = float.Epsilon * Math.Truncate((double)(TileSizeX / float.Epsilon));
			double y = float.Epsilon * Math.Truncate((double)(TileSizeY / float.Epsilon));
			double z = float.Epsilon * Math.Truncate((double)(TileSizeZ / float.Epsilon));
		}
		
		void ButtonCancelClick(object sender, EventArgs e)
		{
            this.DialogResult = DialogResult.Cancel;
		}
		
		
		
		void TextFieldChanged(object sender, EventArgs e)
		{
			this.labelSectorWidth.Text = "Zone Width: " + TileSizeX * RegionResolution + " meters";
			this.labelSectorHeight.Text = "Zone Height: " + TileSizeY * RegionResolution + " meters";
			this.labelSectorDepth.Text = "Zone Depth: " + TileSizeZ * RegionResolution + " meters";
			
			double worldWidth = TileSizeX * RegionResolution * RegionsAcross;
			double worldHeight = TileSizeY * RegionResolution * RegionsHigh;
			double worldDepth  = TileSizeZ * RegionResolution * RegionsDeep;
			this.lblWorldWidth.Text = "World Width: " + worldWidth  + " meters [" + MetersToMiles(worldWidth) + " miles]";
			this.lblWorldHeight.Text = "World Height: " + worldHeight  + " meters [" + MetersToMiles(worldHeight) + " miles]";
			this.lblWorldDepth.Text = "World Depth: " + worldDepth  + " meters [" + MetersToMiles(worldDepth) + " miles]";
		}
		
		private double MetersToMiles (double meters)
		{
			return meters / 1300;
		}
		
		public uint OctreeDepth 
		{ 
			get
			{
				int sectorResolution = int.Parse (cboSectorResolution.Text);
				int power = 0;
        		while (((sectorResolution % 2) == 0) && sectorResolution > 1)
        		{
        	    	sectorResolution /= 2;
        	    	power++;
        		}
        		return (uint)power;
			}
		}
		void CboSectorResolutionSelectedIndexChanged(object sender, EventArgs e)
		{
			
           
			
			labelOctreeDepth.Text = "Octree Depth = " + OctreeDepth.ToString();;
		}
	}
}
