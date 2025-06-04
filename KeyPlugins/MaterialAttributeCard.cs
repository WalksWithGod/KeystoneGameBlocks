using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace KeyPlugins
{
    public partial class MaterialAttributeCard : NotecardBase
    {
        Keystone.Types.Color mAmbient, mDiffuse, mEmissive, mSpecular;
        float mPower;
        byte mOpacity;

        public event EventHandler AmbientChanged;
        public event EventHandler DiffuseChanged;
        public event EventHandler EmissiveChanged;
        public event EventHandler SpecularChanged;
        public event EventHandler OpacityChanged;
        public event EventHandler SpecularPowerChanged;

        public MaterialAttributeCard()
        {
            InitializeComponent();
            mAmbient = new Keystone.Types.Color();
            mDiffuse = new Keystone.Types.Color();
            mEmissive = new Keystone.Types.Color();
            mSpecular = new Keystone.Types.Color();
            mPower = 20;
            mOpacity = 255; // if this value is 0, the pic boxes will be invisible
        }

        public MaterialAttributeCard(string resourceDescriptor,  string childID, string parentID, ResourceEventHandler browseForResourceHandler, ResourceEventHandler editResourceHandler)
                                     : base (resourceDescriptor, childID, parentID, browseForResourceHandler, editResourceHandler )
        {
            InitializeComponent(); 
            this.Text = "Material - " + mTargetID;
            mAmbient = new Keystone.Types.Color();
            mDiffuse = new Keystone.Types.Color();
            mEmissive = new Keystone.Types.Color();
            mSpecular = new Keystone.Types.Color();
            mPower = 20;
            mOpacity = 255; // if this value is 0, the pic boxes will be invisible
        }

        private void UpdateGUI()
        {
            numOpacity.Value = mOpacity;
            numSpecularPower.Maximum = 1000;
            numSpecularPower.Value = Convert.ToDecimal(mPower);

            byte alpha = (byte)numOpacity.Value;
            System.Drawing.Color c = System.Drawing.Color.FromArgb(alpha, (byte)(mAmbient.r * 255), (byte)(mAmbient.g * 255), (byte)(mAmbient.b * 255));
            picAmbient.BackColor = c;
            numRedAmbient.Value = c.R;
            numGreenAmbient.Value = c.G;
            numBlueAmbient.Value = c.B;

            c = System.Drawing.Color.FromArgb(alpha, (byte)(mDiffuse.r * 255), (byte)(mDiffuse.g * 255), (byte)(mDiffuse.b * 255));
            picDiffuse.BackColor = c;
            numRedDiffuse.Value = c.R;
            numGreenDiffuse.Value = c.G;
            numBlueDiffuse.Value = c.B;

            c = System.Drawing.Color.FromArgb(alpha, (byte)(mEmissive.r * 255), (byte)(mEmissive.g * 255), (byte)(mEmissive.b * 255));
            picEmissive.BackColor = c;
            numRedEmissive.Value = c.R;
            numGreenEmissive.Value = c.G;
            numBlueEmissive.Value = c.B;

            c = System.Drawing.Color.FromArgb(alpha, (byte)(mSpecular.r * 255), (byte)(mSpecular.g * 255), (byte)(mSpecular.b * 255));
            picSpecular.BackColor = c;
            numRedSpecular.Value = c.R;
            numGreenSpecular.Value = c.G;
            numBlueSpecular.Value = c.B;


        }

        public float SpecularPower 
        { 
            get { return mPower; } 
            set 
            {
                mPower = value;
                UpdateGUI();
            } 
        }

        public byte Opacity 
        {
            get { return mOpacity; } 
            set 
            {
                mOpacity = value;
                UpdateGUI();
            } 
        }

        public Keystone.Types.Color Ambient
        {
            get
            {
                return mAmbient;
            }
            set 
            {
                mAmbient = value;
                UpdateGUI();
            }
        }


        public Keystone.Types.Color Diffuse
        {
            get
            {
                return mDiffuse;
            }
            set
            {
                mDiffuse = value;
                UpdateGUI();
            }
        }

        public Keystone.Types.Color Emissive
        {
            get
            {
                return mEmissive;
            }
            set
            {
                mEmissive = value;
                UpdateGUI();
            }
        }

        public Keystone.Types.Color Specular
        {
            get
            {
                return mSpecular;
            }
            set
            {
                mSpecular = value;
                UpdateGUI();
            }
        }

        public override string TargetID
        {
            get
            {
                return base.TargetID;
            }
        }

        private void numDiffuse_ValueChanged(object sender, EventArgs e)
        {
            Diffuse = new Keystone.Types.Color(
                (byte)numRedDiffuse.Value, 
                (byte)numGreenDiffuse.Value, 
                (byte)numBlueDiffuse.Value, mOpacity);

            if (DiffuseChanged != null)
                DiffuseChanged.Invoke(this, e);
        }

        private void numAmbient_ValueChanged(object sender, EventArgs e)
        {
            Ambient = new Keystone.Types.Color(
                (byte)numRedAmbient.Value,
                (byte)numGreenAmbient.Value,
                (byte)numBlueAmbient.Value, mOpacity);

            if (AmbientChanged != null)
                AmbientChanged.Invoke(this, e);
        }

        private void numEmissive_ValueChanged(object sender, EventArgs e)
        {
            Emissive = new Keystone.Types.Color(
                (byte)numRedEmissive.Value,
                (byte)numGreenEmissive.Value,
                (byte)numBlueEmissive.Value, mOpacity);

            if (EmissiveChanged != null) EmissiveChanged.Invoke(this, e);
        }

        private void numSpecular_ValueChanged(object sender, EventArgs e)
        {
            Specular = new Keystone.Types.Color(
                (byte)numRedSpecular.Value,
                (byte)numGreenSpecular.Value,
                (byte)numBlueSpecular.Value, mOpacity);

            if (SpecularChanged != null) SpecularChanged.Invoke(this, e);
        }

        private void numSpecularPower_ValueChanged(object sender, EventArgs e)
        {
            SpecularPower = Convert.ToSingle(numSpecularPower.Value);

            if (SpecularPowerChanged != null) SpecularPowerChanged.Invoke(this, e);
        }

        private void numOpacity_ValueChanged(object sender, EventArgs e)
        {
            Opacity = (byte)(numOpacity.Value);
            if (OpacityChanged != null) OpacityChanged.Invoke(this, e);
        }

        private void picAmbient_Click(object sender, EventArgs e)
        {


            // todo: cache the original color so if dialogresult = cancel, we can revert
            // todo: we want an event for whenever the selected color changes so we can update the visuals in realtime
        }

        private void picDiffuse_Click(object sender, EventArgs e)
        {

        }

        private void picEmissive_Click(object sender, EventArgs e)
        {

        }

        private void picSpecular_Click(object sender, EventArgs e)
        {

        }



        private void labelAmbient_Click(object sender, EventArgs e)
        {

        }
        

    }
}
