using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace KeyPlugins
{
    public partial class RigidBodyEditCard : NotecardPropGrid
    {


        public RigidBodyEditCard(string rigidBodyID) : base(rigidBodyID)
        {
            mTargetID = rigidBodyID;
            InitializeComponent();
        }

        public RigidBodyEditCard(string rigidBodyID, string entityID) 
            : this(rigidBodyID)
        {
            mParentID = entityID;
        }

       


        private void OnPropGrid_Click(object sender, EventArgs e)
        {

        }

        private void OnPropGrid_PropertyValueChanging(object sender, DevComponents.DotNetBar.PropertyValueChangingEventArgs e)
        {
            
        }

        private void ClearProperties()
        {
            propertyGrid.SelectedObject = null;

        }

        /// <summary>
        /// Called when user changes the property value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void SetPropertyValue(object sender, Settings.PropertySpecEventArgs e)
        {
            base.SetPropertyValue(sender, e);



            // raise event .OnParameterChanged()
            //if (ShaderParameterChanged != null)
            //    ShaderParameterChanged.Invoke(this, e);
        }



        private void buttonClose_Click_1(object sender, EventArgs e)
        {
            base.buttonClose_Click(sender, e);
        }
    }
}
