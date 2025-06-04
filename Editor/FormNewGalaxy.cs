using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace KeyEdit
{
    public partial class FormNewGalaxy : Form
    {
        // TODO: im contemplating just using the Control Panel for this instead.
        //       meh.  
        // TODO: or how about the lobby style view from Evo
        public FormNewGalaxy()
        {
            InitializeComponent();
            this.DialogResult = DialogResult.Cancel;
            textRandomSeed.Text = "3";
            textRegionsAcross.Text  = AppMain.REGIONS_ACROSS.ToString ();
            textRegionsHigh.Text = AppMain.REGIONS_HIGH.ToString ();
            textRegionsDeep.Text = AppMain.REGIONS_DEEP.ToString ();
			textDensity.Text = "100";
            textRegionDiameter.Text = AppMain.REGION_DIAMETER.ToString ();
            
            if (AppMain.EMPTY_UNIVERSE)
            {
				cbEmpty.Checked = true;
				cbStars.Checked = false;
				cbPlanets.Checked = false;
            }
			else 
			{
				cbEmpty.Checked = false;
				cbStars.Checked = true;
				cbPlanets.Checked = true;
			}
            
            ComputeConversion(double.Parse (textRegionDiameter.Text));
        }
        
        // http://www.knowdotnet.com/articles/numerictextboxes.html
     	
        public string SceneName { get { return textUniverseName.Text; } }
		public float RegionDiameter { get { return float.Parse(textRegionDiameter.Text ); } }
        public uint RegionsAcross { get { return uint.Parse(textRegionsAcross.Text); } }
        public uint RegionsHigh { get { return uint.Parse(textRegionsHigh.Text); } }
        public uint RegionsDeep { get { return uint.Parse(textRegionsDeep.Text); } }
        public bool SerializeEmptyZones { get { return cbSerializeEmptyZones.Checked; } }
        public bool CreateStarDigest { get { return cbCreateStarDigest.Checked; } }
        
        public bool Stars { get { return cbStars.Checked && cbStars.Enabled; } }
        public bool Planets { get { return cbPlanets.Checked && cbPlanets.Enabled; } }
        public bool Moons { get { return cbMoons.Checked && cbMoons.Enabled; } }
        public bool PlanetoidBelts { get { return cbPlanetoidBelts.Checked && cbPlanetoidBelts.Enabled; } }
        public float Density { get { return float.Parse(textDensity.Text) / 100f;  } }
        public int RandomSeed { get { return int.Parse (textRandomSeed.Text);} }

        public KeyCommon.Messages.Scene_NewUniverse.CreationMode CreationMode 
        { 
            get 
            {
                KeyCommon.Messages.Scene_NewUniverse.CreationMode mode = KeyCommon.Messages.Scene_NewUniverse.CreationMode.Empty;

                mode = KeyCommon.Messages.Scene_NewUniverse.CreationMode.Empty;

                if (cbEmpty.Checked == false)
                {
                    if (cbStars.Checked)
                        mode |= KeyCommon.Messages.Scene_NewUniverse.CreationMode.Stars;
                    if (cbPlanets.Checked)
                        mode |= KeyCommon.Messages.Scene_NewUniverse.CreationMode.Planets;
                    if (cbMoons.Checked)
                        mode |= KeyCommon.Messages.Scene_NewUniverse.CreationMode.Moons;
                    if (cbPlanetoidBelts.Checked)
                        mode |= KeyCommon.Messages.Scene_NewUniverse.CreationMode.PlanetoidBelts;

                }
                return mode;
            } 
        }

        private void ComputeConversion (double meters)
        {
            double KM = meters / 1000;
            double AU = meters / 149597870691 ; 
            double LS = meters / 299792458;
            double LM = meters / 17987547480;
            double LY = meters / 9460730472580800 ;
            double Parsecs = meters / 3.24077928964E-17;

            labelDiameterConversion.Text = 
                string.Format("{1} kilometers{0}{2} AU{0}{3} parsecs{0}{4} lightsecond{0}{5} lightminute{0}{6} lightyears ", System.Environment.NewLine, KM, AU, Parsecs, LS, LM, LY);
        
             
            labelGalaxyDimensions.Text =
                string.Format("{1} kilometers{0}{2} AU{0}{3} parsecs{0}{4} lightsecond{0}{5} lightminute{0}{6} lightyears ", System.Environment.NewLine, KM * RegionsAcross, AU * RegionsAcross, Parsecs * RegionsAcross, LS * RegionsAcross, LM * RegionsAcross, LY * RegionsAcross);
        }


        private void textRegionDiameter_TextChanged(object sender, EventArgs e)
        {
            ComputeConversion(double.Parse (textRegionDiameter.Text) );
        }

        private void textRegionsAcross_TextChanged(object sender, EventArgs e)
        {
            ComputeConversion(double.Parse(textRegionDiameter.Text));
        }

        private void textRegionsHigh_TextChanged(object sender, EventArgs e)
        {
            ComputeConversion(double.Parse(textRegionDiameter.Text));
        }

        private void textRegionsDeep_TextChanged(object sender, EventArgs e)
        {
            ComputeConversion(double.Parse(textRegionDiameter.Text));
        }

        private void cbEmpty_CheckedChanged(object sender, EventArgs e)
        {
            if (cbEmpty.Checked)
            {
                cbStars.Enabled = false;
                cbPlanets.Enabled = false;
                cbMoons.Enabled = false;
                cbPlanetoidBelts.Enabled = false;
            }
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void cbStars_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void cbPlanets_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void cbMoons_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void cbPlanetoidBelts_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        
    }
}