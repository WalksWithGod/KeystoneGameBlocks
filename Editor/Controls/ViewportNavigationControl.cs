using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevComponents.Editors;

namespace KeyEdit.Controls
{
    public partial class ViewportNavigationControl : ViewportControl
    {
        Keystone.Scene.Scene mScene;

        public ViewportNavigationControl() : base ()
        {
            InitializeComponent();
        }

        public ViewportNavigationControl(string name, Keystone.Scene.Scene scene )
            : base(name)
        {

            InitializeComponent();

            this.Name = name;
            this.ShowToolbar = true;


            mScene = scene;


            // find and add vehicles to the cboVehicleSelect combobox
            // NOTE: By using ComboItem, we can add images 

            // TODO: should the FloorplanEditorDesigner use a timer when active
            // to update the combo box?
            Keystone.Entities.Entity[] entities = mScene.GetEntities();
            if (entities == null) return;

            for (int i = 0; i < entities.Length; i++)
            {
                if (entities[i] is Keystone.Entities.Container == false)
                    continue;

                ComboItem item = new ComboItem(entities[i].Name);
                item.Tag = entities[i].ID;
                
                cboVehicleSelect.Items.Add(item);
            }
        }

        // select vehicle to navigate
        private void cboVehicleSelect_Click(object sender, EventArgs e)
        {

//            Keystone.Entities.Entity entity = GetSelectedEntity();
//            if (entity == null) return;
//            // TODO: if entity is null or if no interior we must remove the viewpoint
//
//            this.Viewport.Context.Workspace.Selected = 
//                (Keystone.Vehicles.Vehicle) entity; 
        }


        private Keystone.Entities.Entity GetSelectedEntity()
        {
            int selectedItemIndex = cboVehicleSelect.SelectedIndex;
            if (selectedItemIndex == -1)
            {
                MessageBox.Show("No entity selected.");
                return null;
            }


            DevComponents.Editors.ComboItem item = (DevComponents.Editors.ComboItem)cboVehicleSelect.Items[selectedItemIndex];

            string targetEntityID = (string)item.Tag;
            Keystone.Entities.Entity result = mScene.GetEntity(targetEntityID);

            if (result is Keystone.Entities.Container == false)
            {
                // must be a container entity
                MessageBox.Show("This selected entity has only an exterior representation." +
                        "It must be imported to the Asset Browser as a 'Container' entity if you " +
                        "wish for it have an editable 'interior.'", "Entity is not a Container Entity.", MessageBoxButtons.OK);

                return null;
            }

            return result;
        }

        private void buttonBack_Click(object sender, EventArgs e)
        {
            // go to last navigation view 

            // TODO: temp hack for now it always takes us back to
            // the starmap view
            //this.Workspace;
            ((KeyEdit.Workspaces.Huds.NavigationHud)this.Viewport.Context.Hud).Navigate_Previous();
        }

        private void buttonForward_Click(object sender, EventArgs e)
        {
            // go to previous navigation view
            ((KeyEdit.Workspaces.Huds.NavigationHud)this.Viewport.Context.Hud).Navigate_Forward();
        }
    }
}
