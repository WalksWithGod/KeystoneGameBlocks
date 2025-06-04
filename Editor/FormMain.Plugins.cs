using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Keystone.Cameras;
using Keystone.Commands;
using Keystone.Controllers;
using Keystone.Devices;
using Keystone.Elements;
using Keystone.Entities;
using Keystone.Types;
using Keystone.Appearance;
using Keystone.IO;
using Keystone.Resource;
using Keystone.Scene;
using Keystone.Types;
using Lidgren.Network;
using MTV3D65;
using System.Collections.Generic;
using DevComponents.DotNetBar;
using Keystone.Celestial;
using Ionic.Zip;
namespace KeyEdit
{
    // OBSOLETE - See EditorWorkspace.Treeview.OnEntitySelected  and
    //                EditorWorkspace.Treeview.CurrentPlugin
    partial class FormMain : FormMainBase
    {
        //private KeyPlugins.AvailablePlugin mCurrentPlugin;


        //internal KeyPlugins.AvailablePlugin SelectPlugin(IResource resource)
        //{
        //    KeyPlugins.AvailablePlugin plugin = null;

            //if (resource != null)
            //{
            //    #region obsolete Now we'll allow the plugin host to determine which plugin to select based on the available plugins
            //    //if (resource is Keystone.Entities.EntityBase)
            //    //{
            //    //    plugin = SelectPlugin("Entity");
            //    //}
            //    //// display the plugin for the appropriate type
            //    ////else if (resource is JigLibX.Physics.Body)
            //    ////{
            //    ////}
            //    //else if (resource is Keystone.AI.Behavior.Behavior)
            //    //{
            //    //}
            //    //else if (resource is Keystone.Elements.Geometry)
            //    //{
            //    //    if (resource is Keystone.Elements.Mesh3d)
            //    //    {
            //    //        plugin = SelectPlugin(resource.GetType().Name);
            //    //    }
            //    //    else if (resource is Keystone.Elements.Actor3d)
            //    //    {
            //    //        plugin = SelectPlugin(resource.GetType().Name);
            //    //    }
            //    //    else if (resource is Keystone.Entities.Terrain)
            //    //    {
            //    //        plugin = SelectPlugin(resource.GetType().Name);
            //    //    }
            //    //    else if (resource is Keystone.Elements.ParticleSystem)
            //    //    {
            //    //        plugin = SelectPlugin(resource.GetType().Name);
            //    //    }
            //    //    else if (resource is Keystone.Elements.Emitter)
            //    //    {
            //    //        plugin = SelectPlugin(resource.GetType().Name);
            //    //    }
            //    //    else if (resource is Keystone.Elements.Attractor)
            //    //    {
            //    //        plugin = SelectPlugin(resource.GetType().Name);
            //    //    }


            //    //}
            //    //else if (resource is Keystone.Animation.ActorAnimation) // todo this should be general IAnimation first and then we can check subtypes?
            //    //{
            //    //}
            //    //else if (resource is Appearance)
            //    //{

            //    //}
            //    //// GroupAttribute - Added as children of Appearance nodes.  A single GroupAttribute node corresponds to a single Mesh or Actor group or Terrain chunk
            //    //else if (resource is Keystone.Appearance.GroupAttribute)
            //    //{
            //    //    // when selecting a specific GroupAttribute, we should highlight in the 3d viewport that selected part of the mesh
            //    //    plugin = SelectPlugin("GroupAttribute");
            //    //}
            //    //else if (resource is Keystone.Shaders.Shader)
            //    //{
            //    //    plugin = SelectPlugin("Shader");
            //    //}
            //    //else if (resource is Keystone.Appearance.Texture)
            //    //{
            //    //    plugin = SelectPlugin("Texture");
            //    //}
            //    //else if (resource is Keystone.Appearance.Material)
            //    //{
            //    //    // add the Material editor to the smart context sensitive plugin dock pane
            //    //    plugin = SelectPlugin(resource.GetType().Name);

            //    //}
            //    //else if (resource is Keystone.Elements.ScriptNode)
            //    //{
            //    //    plugin = SelectPlugin("ScriptNode");
            //    //}
            //    #endregion

            //    //plugin = AppMain.PluginService.SelectPlugin(resource.TypeName);
            //    plugin = AppMain.PluginService.SelectPlugin("Editor", "Entity");

            //    // when the IPlugin activates, it'll request updated state information
            //    if (plugin != null)
            //    {
            //        this.panelPlugins.Parent.Text = resource.TypeName + " - " + resource.ID;

            //        if (plugin != mCurrentPlugin)
            //        {
            //            PanelDockContainer dockContainer = this.panelPlugins;
            //            //Remove the previous plugin from the plugin dock panel
            //            //Note: this only affects visuals.. doesn't close the instance of the plugin     

            //            if (mCurrentPlugin != null)
            //            {
            //                dockContainer.Controls.Remove(mCurrentPlugin.Instance.MainInterface);
            //            }
            //            mCurrentPlugin = plugin;
            //            dockContainer.Controls.Add(mCurrentPlugin.Instance.MainInterface);
            //            mCurrentPlugin.Instance.MainInterface.Dock = DockStyle.Fill;

            //            // do not use the Fill dockstyle on MainInterface as it does not
            //            // work as you'd imagine
            //            // selectedPlugin.Instance.MainInterface.Dock = DockStyle.Fill;

            //            // assign the node type this plugin will modify and also so that changes to this node
            //            // will result in the plugin being notified (but only when the changes are made outside of hte plugin)
            //            // I think one way to accomplish this without having to require every node type to have some xtra glue
            //            // is to simply monitor the commandprocessor since we will restrict modification to any node
            //            // via Commands.  No direct editing of properties via the property grid.

            //            // so it can update it's display with any new values from the Node it's representing
            //            // selectedPlugin.Register(resource);
            //            mCurrentPlugin.Instance.EditResourceRequest += Plugin_OnEditResource;
            //        }
            //        // if it's the same plugin as last time, see if it's also the same
            //        // resource being used for the data.  If so then no need to plugin.SelectTarget
            //        else if (plugin.Instance.TargetID == resource.ID)
            //            return plugin;

            //        ((PluginHost.EditorHost)plugin.Instance.Host).Selected = resource;
            //        //((PluginHost.EditorHost)AppMain.PluginService).Selected = resource;

            //        try
            //        {
            //            plugin.Instance.Host.PluginChangesSuspended = true;
            //            plugin.Instance.SelectTarget(resource.ID, resource.TypeName);

            //            // never rely on the plugin to un-suspend changes.  One plugin writer may forget
            //            // and then they will be able to break everyone elses plugin by never unsuspending
            //            plugin.Instance.Host.PluginChangesSuspended = false;
            //        }
            //        catch (NotImplementedException ex)
            //        {
            //            // typical if user is developing a plugin and hasnt debugged it, just ignore.
            //            Debug.WriteLine("FormMain.SelectPlugin() -- " + ex.Message);
            //        }
            //        catch (Exception ex)
            //        {
            //            Debug.WriteLine("FormMain.SelectPlugin() -- " + ex.Message);
            //        }
            //    }
            //}

        //    return plugin;
        //}

    }
}
