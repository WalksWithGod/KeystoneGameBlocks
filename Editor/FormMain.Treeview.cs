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


namespace KeyEdit
{
 
    // OBSOLETE - See KeyEdit\Workspaces\EditorWorkspace.Treeview.cs
    // instead.
    partial class FormMain : FormMainBase
    {
        //private TreeView treeEntityBrowser;
        //private delegate void AddTreeItem(IResource parent, IResource child);
        //private delegate void RemoveTreeItem(IResource parent, IResource child);

        //private void SceneNodeRemovedHandler(IResource parent, IResource child)
        //{
            // notify all workspaces or perhaps these SceneNodeRemovedHandler
            // and AddedHandler exists in WorkspaceManager

            //RemoveTreeItem item = RemoveTreeItemHandler;
            //if (treeEntityBrowser.InvokeRequired)
            //    treeEntityBrowser.BeginInvoke(item, parent, child);
            //else
            //    RemoveTreeItemHandler(parent, child);
        //}

        // TODO: here this treeEntityBrowser is not known to FormMain as it's created by the
        // EditorWorkspace.  WHat is needed is a way for us to assign these delegate handlers
        // which isnormally done on SceneBase constructor, to do this upon loading of the editorworkspace
        // and just prior to loading the scene... but im not sure that's possible. is it?  Should be?
        // Scene can be instanced before it's .Load() ed.

        //private void SceneNodeAddedHandler(IResource parent, IResource child)
        //{
            //AddTreeItem item = AddTreeItemHandler;
            //if (treeEntityBrowser.InvokeRequired)
            //    treeEntityBrowser.BeginInvoke(item, parent, child);
            //else
            //    AddTreeItemHandler(parent, child);

            //// TODO: there's an issue here with prefabs using textures that are in the archive... trying to add an icon for the prefab
            //// using the resourcepath that is a ImportLib.ResourceDescriptor to an archvied image is not working here yet.
            ////if (child is Keystone.Appearance.Texture)
            ////{
            ////    string filepath = ((Keystone.Appearance.Texture)child).ResourcePath;
            ////    if (!galleryContainerTextures.SubItems.Contains(filepath))
            ////    {
            ////        DevComponents.DotNetBar.ButtonItem buttonItem = CreateGalleryButtonItem(filepath,
            ////            Path.GetFileName(filepath), filepath, "", filepath, 32, 24, true);

            ////        galleryContainerTextures.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            ////        buttonItem});
            ////    }
            ////}
        //}

        //private void AddTreeItemHandler(IResource parent, IResource child)
        //{
        //    try
        //    {
        //        // This sub is essentially called via Repository.Resource.IncrementRef()
        //        // in succession for every node.  If an Entity with a single Geometry child is added
        //        // to the scene, this event will fire for each.  However, we will skip non Entity children.
        //        // 
        //        // we only add Entities (note: viewpoint is an entity)
        //        if ((child is Entity) == false) return;

        //        if (parent != null)
        //        {
        //            // TODO: For actors we special case that node and add children
        //            // for each boneID so that we can drag and drop actors or meshes
        //            // to specific child bones.
        //            //
        //            // search starting at root node collection
        //            TreeNode[] nodes = treeEntityBrowser.Nodes.Find(parent.ID, true);
        //            if (nodes.Length > 0)
        //            {
        //                int imageIndex = -1;
        //                nodes[0].Nodes.Add(child.ID, child.TypeName + " " + child.ID, imageIndex);
        //                // since this only contains entities, there should only ever be just one node with this key found
        //                Trace.Assert(nodes.Length == 1, "More than one entity instance found in treeview.  Entities should only ever have one instance!");
        //            }
        //            // NOTE: following we wont use.  If a parent node does not exist, this child is
        //            // not actually connected to the Scene (eg. widget controls, sceneInfo, floor_overlay hud entities)
        //            //else
        //            //    treeEntityBrowser.Nodes.Add(child.ID, child.TypeName + " " + child.ID); // temp
        //        }
        //        else
        //            treeEntityBrowser.Nodes.Add(child.ID, child.TypeName + " " + child.ID);

        //    }
        //    catch (Exception ex)
        //    {
        //        Trace.WriteLine(string.Format("Error adding node {0} to treeview." + ex.Message, child.TypeName));
        //    }
        //}

        //private void RemoveTreeItemHandler(IResource parent, IResource child)
        //{
        //    try
        //    {
        //        // we know we only add Entities so non entity children cant be removed because
        //        // they were never added.
        //        if ((child is Entity) == false) return;
        //        TreeNode[] nodes = treeEntityBrowser.Nodes.Find(child.ID, true);

        //        if (nodes.Length > 0)
        //        {
        //            if (nodes.Length == 1)
        //                if (nodes[0].Parent != null)
        //                    nodes[0].Parent.Nodes.Remove(nodes[0]);
        //                //if (treeEntityBrowser.Nodes.ContainsKey(nodes[0].Name))
        //                //    treeEntityBrowser.Nodes[nodes[0].Index].Remove();
        //                else
        //                {
        //                    // since our tree is based off of a DAG, there might be some child nodes that have multiple instances in the tree
        //                    // we only want to remove the one who's parent matches parent.ID
        //                    // TODO: when generating a temporary scene such as Universe, we dont want these RemoveTreeItemHandler or
        //                    // AddTreeItemHandlers to fire.
        //                    for (int i = 0; i < nodes.Length; i++)
        //                        if (nodes[i].Parent != null)
        //                            if (nodes[i].Parent.Name == parent.ID)
        //                                if (treeEntityBrowser.Nodes.ContainsKey(nodes[i].Name))
        //                                    treeEntityBrowser.Nodes[nodes[i].Index].Remove();
        //                }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Trace.WriteLine(string.Format("Error removing node {0} to treeview." + ex.Message, child.ID));
        //    }
        //}

        ///// <summary>
        ///// If an EntityBase is being added, this Clears the tree and adds a new Entity resource as root node of the Treeview
        ///// </summary>
        ///// <param name="resourceID"></param>
        //private void AddResourceToTree(IResource resource)
        //{

        //    if (resource != null)
        //    {
        //        // if we're already on this resource, just return
        //        if (mCurrentResource == resource) return;
        //        mCurrentResource = resource;

        //        // save the tree state, push it onto the stack and then 
        //        // clear the treeview and repopulate it with the selected entities
        //        if (resource is Entity)
        //        {
        //            TreeviewState state = new TreeviewState();
        //            string parentID = "";
        //            if (((Entity)resource).Parent != null)
        //                parentID = ((Entity)resource).Parent.ID;

        //            treeEntityBrowser.Tag = parentID;
        //            state.SaveTreeView(treeEntityBrowser, parentID);
        //            mTreeStates.Push(state);

        //            treeEntityBrowser.Nodes.Clear();

        //            // if this is not the root region, add as first tree node "..." for returning to the previous parent
        //            if ((resource is Keystone.Portals.Root) == false)
        //                treeEntityBrowser.Nodes.Add("..", "..");

        //            // add the entity itself
        //            TreeNode node = treeEntityBrowser.Nodes.Add(resource.ID, resource.TypeName + " - " + resource.ID);

        //            // Add all the entity components as well as all recursive child entities.  But only our
        //            // primary selected Entity will have it's sub-components loaded.  Child entities will appear just as if we
        //            // were still in the main Entity browser mode
        //            AddSubNodes((Entity)resource, node, true);

        //            // expand after the child nodes added
        //            if (node.Parent == null)
        //                treeEntityBrowser.ExpandAll();
        //            else
        //                node.Parent.Expand();
        //        }
        //    }
        //}


        //private void AddSubNodes(Keystone.Elements.IGroup group, TreeNode parentNode, bool recurseChildEntities)
        //{
        //    if (group.Children != null)
        //        for (int i = 0; i < group.Children.Length; i++)
        //        {
        //            TreeNode subParent = null;
        //            if (group.Children[i] is Entity)
        //                subParent = parentNode.Nodes.Add(group.Children[i].ID, group.Children[i].TypeName + " - " + group.Children[i].ID);
        //            // now recurse all child Groups except Entities if prohibited
        //            if (group.Children[i] is Keystone.Elements.IGroup)
        //            {
        //                if (group.Children[i] is Entity && !recurseChildEntities)
        //                    continue;

        //                // only add entiy children, not non entities like mesh3d, scripts, etc
        //                AddSubNodes((Keystone.Elements.IGroup)group.Children[i], subParent, recurseChildEntities);
        //            }
        //        }
        //}


        //#region treeEntityBrowser event handlers
        //private IResource mCurrentResource;
        //private System.Collections.Generic.Stack<TreeviewState> mTreeStates = new Stack<TreeviewState>();
        ///// <summary>
        ///// Double click causes us to act like we're switching URLs or following a link to the internals of that Entity.
        ///// We can later use the "back or up" commands to go back to the Entity treeview.   Using two seperate treeviews
        ///// will make it easier to maintain our state when going between the Entity and it's Internals.  To simplify things we
        ///// should not worry about a "history" stack or any such thing.  All we need is a single "Forward" button when an 
        ///// Entity is highlighted and a "Back" image to take the place on this toggle button when viewing an Entity's internal
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void treeEntityBrowser_NodeMouseDoubleClick(object sender, System.Windows.Forms.TreeNodeMouseClickEventArgs e)
        //{

        //    //treeEntityBrowser.SelectedNode = treeEntityBrowser.GetNodeAt(e.X, e.Y);

        //    //if (treeEntityBrowser.SelectedNode != null)
        //    //{

        //    //    if (treeEntityBrowser.SelectedNode.Name == "..")
        //    //    {
        //    //        // clear the treeview and move to the previous treeState 
        //    //        treeEntityBrowser.Nodes.Clear();
        //    //        // Display a wait cursor while the TreeNodes are being created.
        //    //        Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

        //    //        // Suppress repainting the TreeView until all the objects have been created.
        //    //        treeEntityBrowser.BeginUpdate();

        //    //        TreeviewState state = mTreeStates.Pop();

        //    //        AddResourceToTree(_core.SceneManager.ActiveSimulation.Scene.GetResource((string)state.Tag));
        //    //        SelectPlugin(_core.SceneManager.ActiveSimulation.Scene.GetResource((string)state.Tag));

        //    //        state.RestoreTreeView(treeEntityBrowser, "");

        //    //        // TODO: now there's a slight issue here... what happens if
        //    //        // we somehow add or delete new nodes to the scene such that when we
        //    //        // revert back, nodes in the cached state do not match those in the actual scene
        //    //        // Well i think for robustness this answers itself, we should not store the actual nodes, but just the parent
        //    //        // since it should be impossible to delete a parent without being on that node in the tree 
        //    //        // further, when trying to restore the expanded state, if a particular nodes in the expanded list does not match
        //    //        // any node in the tree, we skip it
        //    //        // treeEntityBrowser.node

        //    //        // TODO: if we do delete a parent, we should clear the stack and return to the root
        //    //        treeEntityBrowser.EndUpdate();
        //    //        Cursor.Current = System.Windows.Forms.Cursors.Default;
        //    //    }
        //    //    else
        //    //    {
        //    //        // double click is always preceded by NodeMouseClick (single click event) so there is no need to set the Selected
        //    //        // entity in SceneManager.ActiveScene.Selected or to set the property grid again.
        //    //        IResource resource = _core.SceneManager.ActiveSimulation.Scene.GetResource(treeEntityBrowser.SelectedNode.Name);
        //    //        // AddResourceToTree actually clears the tree and adds this resource as a root tree node
        //    //        AddResourceToTree(resource);
        //    //        SelectPlugin(resource);
        //    //    }
        //    //}
        //}

        //// TODO: this particular treeview should be owned by the Editor workspace
        //private void treeEntityBrowser_NodeMouseClick(object sender, System.Windows.Forms.TreeNodeMouseClickEventArgs e)
        //{
        //    // note: this only fires on left mouse click.
        //    EndDrag();

        //    treeEntityBrowser.SelectedNode = treeEntityBrowser.GetNodeAt(e.X, e.Y);

        //    if (treeEntityBrowser.SelectedNode != null)
        //    {
        //        IResource resource = Repository.Get(treeEntityBrowser.SelectedNode.Name);
        //        if (resource != null) SelectPlugin(resource);

        //        if (resource is Entity)
        //            // TODO: cannot hardcode Scenes[0] !!
        //            ((ClientScene)_core.SceneManager.Scenes[0]).Selected = (Entity)resource;
        //    }
        //}

        ///// <summary>
        ///// Right Mouse Click Context Menu Handler
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void treeEntityBrowser_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        //{
        //    if (e.Button == MouseButtons.Left)
        //    {
        //        EndDrag();
        //        return;
        //    }

        //    if (e.Button != System.Windows.Forms.MouseButtons.Right)
        //        return;

        //    treeEntityBrowser.SelectedNode = treeEntityBrowser.GetNodeAt(e.X, e.Y);

        //    if (treeEntityBrowser.SelectedNode != null)
        //    {
        //        // based on the node type, select the context menu to use
        //        IResource resource = Repository.Get(treeEntityBrowser.SelectedNode.Name);
        //        if (resource != null)
        //            SelectContextMenu(resource, e.Location);
        //    }
        //}

        //private void SelectContextMenu(IResource resource, Point location)
        //{
        //    ContextMenuStrip menu = null;

        //    if (treeEntityBrowser.SelectedNode == null) return;

        //    // TODO: lets see if we can't just grab the right click menu from the plugin
        //    KeyPlugins.AvailablePlugin plugin = SelectPlugin(resource);
        //    if (plugin != null && plugin.Instance != null)
        //    {
        //        string parentID = "";
        //        // if the very first node is ".." it means the parent is one level up 
        //        // and we've stored that parent's ID in the tag
        //        if (treeEntityBrowser.Nodes[0].Name == "..")
        //        {
        //            // for the scene, get the root node ID
        //            // which I suppose is the treeEntityBrowser's tag?
        //            parentID = (string)treeEntityBrowser.Tag;
        //        }
        //        else if (treeEntityBrowser.SelectedNode.Parent == null)
        //        {
        //            parentID = "";
        //        }
        //        else
        //        {
        //            parentID = treeEntityBrowser.SelectedNode.Parent.Name;
        //        }
        //        // TODO: if we have entered a sub-entity, then the "..." might be the parent
        //        // if that's the case, then we look at the treeview tag to know the real parent
        //        //
        //        menu = plugin.Instance.GetContextMenu(resource.ID, parentID, location);
        //    }
        //    // EntityMenu
        //    // LODGeometry Menu
        //    // GroupAttribute Menu (add material, add texture) <-- layers too
        //    //    else if (resource is Keystone.Animation.ActorAnimation) // todo this should be general IAnimation first and then we can check subtypes?
        //    //    {
        //    //    }
        //    //    // GroupAttribute - Added as children of Appearance nodes.  A single GroupAttribute node corresponds to a single Group in a Mesh or Actor, or a single Terrain chunk
        //    //    else if (resource is Keystone.Appearance.GroupAttribute)
        //    //    {
        //    //        // when selecting a specific GroupAttribute, we should highlight in the 3d viewport that selected part of the mesh

        //    //    }

        //    if (menu != null)
        //    {
        //        menu.Show(treeEntityBrowser, location);
        //    }
        //}

        //private void treeEntityBrowser_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        //{
        //    treeEntityBrowser.SelectedNode = treeEntityBrowser.GetNodeAt(e.X, e.Y);
        //    // following three lines involving the KeyPlugins.DragDropContext is handled in QueryDrag
        //    //KeyPlugins.DragDropContext context = new KeyPlugins.DragDropContext();
        //    //context.Data = treeEntityBrowser.SelectedNode;
        //    //treeEntityBrowser.DoDragDrop(context, DragDropEffects.Copy);
        //    EndDrag();

        //    if (e.Button == MouseButtons.Left)
        //    {
        //        // when clicking on a folder, i dont want to autocollapse, but when clicking on an item, perhaps i do
        //        Debug.Assert(treeEntityBrowser == sender);
        //        QueryDrag(sender, e);
        //    }
        //    else
        //    {
        //        // show a popup menu... or currently we're allowing the treeEntityBrowser_MouseUp to do that
        //        //ShowGalleryRightMouseClickMenu((ButtonItem)sender, (Control)((ButtonItem)sender).ContainerControl, e);
        //    }
        //}

        //private void treeEntityBrowser_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        //{
        //    if (e.Button == MouseButtons.Left)
        //    {
        //        QueryDrag(treeEntityBrowser, e);
        //    }
        //}

        //// This event is actually ONLY called on the final DROP action and not
        //// during drag (so disregard the confusing event name).  
        //// This event will trigger when dropping on either a treenode onto another treenode
        //// or a treenode onto the assets (as in for saving an entity to .kgbentity)
        //private void treeEntityBrowser_NodeDrag(object sender, ItemDragEventArgs itemArgs)
        //{
        //    if (itemArgs != null && itemArgs.Item != null && itemArgs.Item is TreeNode)
        //    {
        //        TreeNode node = (TreeNode)itemArgs.Item;
        //        // we are attempting to re-parent an entity
        //        //  DoDragDrop(node, DragDropEffects.Move);
        //        QueryDrag(sender, itemArgs);
        //    }
        //}

        //private void treeEntityBrowser_OnDragEnter(object sender, DragEventArgs drgevent)
        //{
        //    System.Diagnostics.Debug.WriteLine("treeEntityBrowser.OnDragEnter() " + drgevent.ToString());
        //    drgevent.Effect = DragDropEffects.None;

        //    //if (ribbonControl.SelectedRibbonTabItem != null)
        //    //{
        //    //    RibbonTabItem tab = ribbonControl.SelectedRibbonTabItem;
        //    //    if (tab == ribbonTabItemPrimitives)
        //    //    {
        //    //        KeyPlugins.DragDropContext node = (KeyPlugins.DragDropContext)drgevent.Data.GetData(typeof(KeyPlugins.DragDropContext));
        //    //        // TODO: if this is not the correct node type, then
        //    //        if (node == null)
        //    //            return;
        //    //        else
        //    //        {
        //    // test if we are exactly over the tab and not on the ribbonbar
        //    //Rectangle rect = tab.Panel.ClientRectangle;
        //    //rect = tab.Panel.RectangleToScreen(rect);
        //    //if (rect.Contains(Cursor.Position))
        //    // if the data is valid for drop, set the effect to the allowed effect
        //    drgevent.Effect = drgevent.AllowedEffect;
        //    //}
        //    //}
        //    //}
        //}

        //private void treeEntityBrowser_OnDragOver(object sender, DragEventArgs drgevent)
        //{
        //    System.Diagnostics.Debug.WriteLine("treeEntityBrowser.OnDragOver() " + drgevent.ToString());

        //    // Retrieve the client coordinates of the mouse position.
        //    Point targetPoint = treeEntityBrowser.PointToClient(new Point(drgevent.X, drgevent.Y));
        //    //  Select the node at the mouse position.
        //    treeEntityBrowser.SelectedNode = treeEntityBrowser.GetNodeAt(targetPoint);

        //}

        private void treeEntityBrowser_OnDragDrop(object sender, DragEventArgs drgevent)
        {
            System.Diagnostics.Debug.WriteLine("treeEntityBrowser.OnDragDrop() " + drgevent.ToString());
            KeyPlugins.DragDropContext node = (KeyPlugins.DragDropContext)drgevent.Data.GetData(typeof(KeyPlugins.DragDropContext));

            if (node != null && node.EntityID != null)
            {
                // send command to move this node, NOTE: the move may be rejected
                // if the type is not allowed
                //KeyCommon.Messages.Node_Move(node.EntityID, newParentID);

                // TODO: Since all "commands" go through our single network .SendMessage()
                // all we have to do to suspend other user commands is to tell the network mNetClient
                // to suspend until response is received.  We can also time-out and then simply notify the user
                // that a certain command timed out, please try again in a few moments, if problem persists check your
                // network connection, etc.
                // We can also signal the mouse to suspend at this point too.  This is an easier easier way to handle
                // asychronous "transactional" type messages as opposed to fire and forget messages like status updates.
                //
                //KeyCommon.Messages.Prefab_Save save = new KeyCommon.Messages.Prefab_Save();
                //save.RelativeZipFilePath = AppMain.ModName;
                //save.NodeID = node.EntityID;
                //save.EntryPath = "meshes\\vehicles\\";
                //save.EntryName = prefabName;

                //AppMain.mNetClient.SendMessage(save);

           }
        

        //    // Retrieve the client coordinates of the drop location.
        //    Point targetPoint = treeEntityBrowser.PointToClient(new Point(drgevent.X, drgevent.Y));

        //    // Retrieve the node at the drop location.
        //    TreeNode targetNode = treeEntityBrowser.GetNodeAt(targetPoint);
        //    if (targetNode == null) return;

        //    // Retrieve the node that was dragged.
        //    //TreeNode draggedNode =  (TreeNode)drgevent.Data.GetData(typeof(TreeNode));
        //    //if (draggedNode == null) return;

        //    // Retreive the node that was dragged (not the following two lines replaces above two lines when using DragDropContext as opposed to built in TreeNode DragObject)
        //    KeyPlugins.DragDropContext context = (KeyPlugins.DragDropContext)drgevent.Data.GetData(typeof(KeyPlugins.DragDropContext));
        //    TreeNode[] foundNodes = treeEntityBrowser.Nodes.Find(context.EntityID, true);
        //    if (foundNodes == null) return;
        //    if (foundNodes.Length == 0) return;
        //    if (foundNodes.Length != 1) throw new Exception("FormMain.Treeview() - Entity with same ID should not exist twice in the treeview.");
        //    TreeNode draggedNode = foundNodes[0];
        //    if (draggedNode == null) return;

        //    // Confirm that the node at the drop location is not 
        //    // the dragged node or a descendant of the dragged node.
        //    if (!draggedNode.Equals(targetNode) && !ContainsNode(draggedNode, targetNode))
        //    {
        //        // If it is a move operation, remove the node from its current 
        //        // location and add it to the node at the drop location.
        //        if (drgevent.Effect == DragDropEffects.Move)
        //        {
        //            // IMPORTANT: TODO: if double clicked on the sub node, the Root (or whatever other
        //            // parent will not be in the treeview, it'll be "..." so this will fail.
        //            // For now, make sure the entire tree of entities is visible.
        //            KeyCommon.Messages.Node_ChangeParent changeParent = new KeyCommon.Messages.Node_ChangeParent(draggedNode.Name, targetNode.Name, draggedNode.Parent.Name);
        //            AppMain.mNetClient.SendMessage(changeParent);
        //        }

        //        // If it is a copy operation, clone the dragged node 
        //        // and add it to the node at the drop location.
        //        else if (drgevent.Effect == DragDropEffects.Copy)
        //        {
        //            // TODO: not implemented.  I should only follow thru with copy
        //            // if server authorizes
        //            // targetNode.Nodes.Add((TreeNode)draggedNode.Clone());
        //        }

        //        // Expand the node at the location 
        //        // to show the dropped node.
        //        targetNode.Expand();
        //    }

        }

        //// Determine whether one node is a parent 
        //// or ancestor of a second node.
        //private bool ContainsNode(TreeNode node1, TreeNode node2)
        //{
        //    // Check the parent node of the second node.
        //    if (node2.Parent == null) return false;
        //    if (node2.Parent.Equals(node1)) return true;

        //    // If the parent node is not null or equal to the first node, 
        //    // call the ContainsNode method recursively using the parent of 
        //    // the second node.
        //    return ContainsNode(node1, node2.Parent);
        //}


        //private void treeEntityBrowser_OnDragLeave(object sender, EventArgs e)
        //{
        //    System.Diagnostics.Debug.WriteLine("treeEntityBrowser.OnDragLeave() ");
        //}
        //#endregion

    }
}
