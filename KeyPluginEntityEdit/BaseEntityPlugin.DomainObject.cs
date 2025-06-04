using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using KeyPlugins;
using Keystone.Types;

namespace KeyPlugins
{
    partial class BaseEntityPlugin
    {
        private void buttonAddDomainObject_Click(object sender, EventArgs e)
        {
            // TODO: here the problem is i think in adding a DO this way anymore when we intend
            //       to have user set Entity.ResourcePath  instead.  As it is, we are still creating
            //       a domainobject node and adding it 
            string fileDialogFilter = "Scripts|*.css";
            BrowseNewResource("DomainObject", mTargetNodeID, fileDialogFilter);            
        }

        private void buttonRemoveDomainObject_Click(object sender, EventArgs e)
        {
            string domainObjectID = propGridDomainObject.Tag.ToString();
            mHost.Node_Remove(domainObjectID, mTargetNodeID);
            ClearDomainObjectPanel();
        }

        private void buttonEditScript_Click(object sender, EventArgs e)
        {
            string domainObjectID = propGridDomainObject.Tag.ToString();
            EditResource("DomainObject", domainObjectID, mTargetNodeID);
        }

        private void buttonChangeDomainObject_Click(object sender, EventArgs e)
        {
            string domainObjectID = propGridDomainObject.Tag.ToString();
            
            // if the two are the same, ignore (TODO: should that check be done server side too?
            BrowseReplacementResource("DomainObject", domainObjectID, mTargetNodeID);
        }


        private void OnPropGridDomainObject_Click(object sender, EventArgs e)
        {
            
        }

        private void OnPropGridDomainObject_PropertyValueChanging(object sender, DevComponents.DotNetBar.PropertyValueChangingEventArgs e)
        {

        }

        private void ClearDomainObjectPanel()
        {
            // TODO: i dont think we need seperate AddDomainObject, AddDomainScript
            // because the former only exists because of the latter which serves as
            // the shareable resource.  The DO itself is shareable however our scripts
            // are shared also with BehaviorScripts and so the Script node itself is just
            // a pageable resource.
            labelResourceDatabaseRelativePath.Hide();
            labelResourceDBEntry.Hide();
            propGridDomainObject.Hide();
            buttonRemoveDomainObject.Hide();
            buttonEditScript.Hide();
            buttonAddDomainObject.Show();
            buttonChangeDomainObject.Hide();
        }

        // TODO: this doesn't get called if there is no DomainObject
        // attached to an entity, however it's not clearing the .SelectedObject either 
        // of the previous entity.
        // second, when adding a new domainobject, this doesnt get called to now reflect
        // the DO that now exists. (animationset and behaviortree does same thing)
        private void PopulateDomainObjectPanel(string domainObjectNodeID)
        {
            try
            {
                propGridDomainObject.SuspendLayout();
                labelResourceDatabaseRelativePath.Hide();
                labelResourceDBEntry.Hide();
                propGridDomainObject.Show();
                buttonAddDomainObject.Hide();
                buttonRemoveDomainObject.Show();
                buttonEditScript.Show();
                buttonChangeDomainObject.Show();


                KeyCommon.IO.ResourceDescriptor descriptor = new KeyCommon.IO.ResourceDescriptor(domainObjectNodeID);
                labelResourceDatabaseRelativePath.Show();
                labelResourceDBEntry.Show();
                labelResourceDatabaseRelativePath.Text = "Mod Name: " + descriptor.ModName;
                labelResourceDBEntry.Text = "Mod Entry Name: " + descriptor.EntryName;

                // the parent for any scripts
                propGridDomainObject.Tag = domainObjectNodeID;

                // clear the property grid
                propGridDomainObject.SelectedObject = null;


                // get any custom properties and their values and populate property grid
                // TODO: we're not assigning any UITypeEditor for complex types like Vector3d.  So far we haven't needed them, but nowhere am i creating any  UITypeEditor for a property
                Settings.PropertySpec[] customProperties = mHost.Entity_GetCustomProperties(mSceneName, mTargetNodeID, mTargetTypename);
                if (customProperties == null || customProperties.Length == 0) return;

                _propertyTable = new System.Collections.Hashtable();
                Settings.PropertyBag bag = new Settings.PropertyBag();
                bag.GetValue += GetPropertyValue;
                bag.SetValue += SetPropertyValue;
                
                for (int i = 0; i < customProperties.Length; i++)
                    _propertyTable[customProperties[i].Name] = customProperties[i].DefaultValue;

                bag.Properties.AddRange(customProperties);


                propGridDomainObject.SelectedObject = bag; //.Properties;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("KeyPlugins.BaseEntityPlugin.PopulateDomainObjectPanel() - " + ex.Message); 
            }
            finally
            {
                propGridDomainObject.ResumeLayout();
            }
        }



        System.Collections.Hashtable _propertyTable;
        private void GetPropertyValue(object sender, Settings.PropertySpecEventArgs e)
        {
            e.Value = _propertyTable[e.Property.Name];
        }

        /// <summary>
        /// Called when user changes the property value.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetPropertyValue(object sender, Settings.PropertySpecEventArgs e)
        {
            _propertyTable[e.Property.Name] = e.Value;

            if (mHost != null)
                mHost.Entity_SetCustomPropertyValue(mTargetNodeID, e.Property.Name, e.Property.TypeName, e.Value);
        }
    }
}
