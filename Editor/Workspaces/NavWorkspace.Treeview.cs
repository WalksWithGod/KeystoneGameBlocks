using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using Keystone.Elements;
using Keystone.Entities;
using Keystone.Resource;
using Keystone.Types;


namespace KeyEdit.Workspaces
{
    public partial class NavWorkspace
    {

        // private void SystemViewGeneration_Recurse(TreeNode parentTreeNode, string parentID, string parentTypeName, Keystone.Types.Vector3d parentTranslation)
        // {
        //     // Load children from db but do not recurse them on the DB call itself.
        //     // NOTE: if we clone entities the entity names will be replaced with new GUIDs
        //     // that do not match the actual Entity ids and we dont want that.  We want to be able to know
        //     // the real Entity id and to be able to update it's DB entry.
        ////     Keystone.Elements.Node[] children = mScene.XMLDB.ReadSynchronous(parentTypeName, parentID,
        ////                            false, false, true);

        //     TreeNodeCollection children = parentTreeNode.Nodes;

        //     if (children == null) return;
        //     for (int i = 0; i < children.Count; i++)
        //     {

        //         if (children[i] is Keystone.Entities.Entity == false) continue;
        //         if (children[i] is Keystone.Lights.Light) continue;

        //         TreeNode childTreeNode = parentTreeNode.Nodes.Add(children[i].Name);
        //         childTreeNode.Tag = children[i].ID;

        //         // accumulate the translation
        //         Keystone.Types.Vector3d translation = ((Keystone.Entities.Entity)children[i]).Translation;
        //         translation += parentTranslation;

        //         if (children[i] is Keystone.Celestial.Star)
        //         {
        //             // compute the translation in AU
        //             // TODO: id like to get the star temperature here and use that
        //             // to color = Celestial.ProceduralHelper.ColorFromTemperature();
        //             mOrbitInfoCollection.Add(CreateOrbitInfo((Keystone.Celestial.Star)children[i], translation);
        //         }

        //         SystemViewGeneration_Recurse(children[i], children[i].ID, children[i].TypeName, translation);
        //     }

        //     for (int i = 0; i < children.Length; i++)
        //     {
        //         // loading the children from the db stores them in the Repository with a ref count == 0.
        //         // therefore if they are not actually in use by the scene, we can increment their ref count 
        //         // to 1 and then back to 0 and that will unload it with no accidental worry of unloading
        //         // a required entity.
        //         Keystone.Resource.Repository.IncrementRef(null, children[i]);
        //         Keystone.Resource.Repository.DecrementRef(null, children[i]);
        //     }
        // }


    }
}
