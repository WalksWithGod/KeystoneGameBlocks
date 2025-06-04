using System;
using System.Collections.Generic;


namespace KeyEdit.Workspaces
{
    public partial class CrewManagementWorkspace
    {

    	//GUI.QuickLook3 mQuickLookPanel;
        // what if the plugin called this on the current quick look bar?
        public override void QuickLookPanelSelect(PluginHost.EditorHost host, string entityID, string entityTypeName, Keystone.Collision.PickResults pick)
        {

         //   // TODO: this should be in Show as done by EditorWorkspace, TacticalWorkspace and NavigationWorkspace
         //   if (mQuickLookPanel == null)
        	//{
         //   	mQuickLookPanel = new GUI.QuickLook3();
         //       this.mQuickLookHostPanel.Controls.Add(mQuickLookPanel);
         //   	mQuickLookPanel.Dock = System.Windows.Forms.DockStyle.Fill;
         //   }


         //   //string[] childIDs;
         //   //string[] childNodeTypes;
         //   //scriptingHost.Node_GetChildrenInfo(entityID, null, out childIDs, out childNodeTypes);

         //   //string domainObjID = "";

         //   //if (childNodeTypes != null)
         //   //{
         //   //    for (int i = 0; i < childNodeTypes.Length; i++)
         //   //        if (childNodeTypes[i] == "DomainObject")
         //   //        {
         //   //            domainObjID = childIDs[i];
         //   //            break;
         //   //        }
         //   //}
         //   mQuickLookPanel.Select(mScene.Name, entityID, entityTypeName, pick);
        }
    }
}
