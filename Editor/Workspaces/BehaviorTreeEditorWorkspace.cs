using System;
using System.Collections.Generic;
using System.Diagnostics;
using Ionic.Zip;
using Keystone.Resource;
using System.Windows.Forms;

namespace KeyEdit.Workspaces
{
    public class BehaviorTreeEditorWorkspace : Keystone.Workspaces.IWorkspace
    {
        KeyEdit.Controls.BehaviorTreeEditorControl mDocument;
        protected Keystone.Workspaces.IWorkspaceManager mWorkspaceManager;
        protected string mName;
        private const string EXTENSION = ".kgbbehavior";
        protected bool mIsActive;
        protected string mBehaviorTreePath;
        KeyCommon.IO.ResourceDescriptor mDescriptor;
        private Keystone.Behavior.Composites.Composite mRoot;

        public BehaviorTreeEditorWorkspace(string documentPath)
        {
            mDescriptor = new KeyCommon.IO.ResourceDescriptor(documentPath);
            mName = mDescriptor.ToString();
            mBehaviorTreePath = AppMain.MOD_PATH + "\\" + AppMain.ModName + "\\behaviors";

            if (!System.IO.Directory.Exists(mBehaviorTreePath))
                System.IO.Directory.CreateDirectory(mBehaviorTreePath);
        }

        public BehaviorTreeEditorWorkspace(KeyCommon.IO.ResourceDescriptor descriptor)
        {
            mDescriptor = descriptor;
            mName = mDescriptor.ToString();
        }


        public virtual void Configure(Keystone.Workspaces.IWorkspaceManager manager, Keystone.Scene.Scene scene)
        {
            if (manager == null) throw new ArgumentNullException("WorkspaceBase.Configre() - No ViewManager set.");
            mWorkspaceManager = manager;


            mDocument = new KeyEdit.Controls.BehaviorTreeEditorControl(OnNew, OnOpen, OnSave);
            mDocument.treeView.NodeMouseClick += treeView_NodeMouseClick;

            // CreateWorkspaceDocumentTab also tells the dockbar to make this tab the active tab
            //string name = string.Format("Code Editor[{0}]", count);

            mWorkspaceManager.CreateWorkspaceDocumentTab(mDocument, GotFocus);
        }

        public virtual void UnConfigure()
        {
            if (mWorkspaceManager == null) throw new Exception("WorkspaceBase.Unconfigure() - No ViewManager set.");
        }

        #region IWorkspace Members
        public string Name
        {
            get { return mName; }
        }

        public bool IsActive { get { return mIsActive; } }

        public Keystone.Workspaces.IWorkspaceManager Manager { get { return mWorkspaceManager; } }

        public Keystone.Controllers.InputControllerBase IOController { get { return null; } set { } }

        public Control Document { get { return mDocument; } }

        public Keystone.Collision.PickResults SelectedEntity
        {
            set { throw new Exception(); }
            get { throw new Exception(); }
        }

        public Keystone.Collision.PickResults MouseOverItem
        {
            set { throw new Exception(); }
            get { throw new Exception(); }
        }

        // todo: weneed to know when Behavior nodes are created and deleted, not just Entities
        public void OnEntityAdded(Keystone.Entities.Entity parent, Keystone.Entities.Entity child)
        {
            throw new NotImplementedException();
        }

        public void OnEntityRemoved(Keystone.Entities.Entity parent, Keystone.Entities.Entity child)
        {
            throw new NotImplementedException();
        }

        public void OnEntitySelected(Keystone.Collision.PickResults pickResults)
        {
            //throw new NotImplementedException();
        }

        public void OnEntityDeleteKeyPress()
        {
            //throw new NotImplementedException();
        }

        public void OnNodeAdded(string parentID, string childID, string childTypeName)
        {
            if (string.IsNullOrEmpty(parentID))
                this.mDocument.treeView.Nodes.Add(childID, "Sequence, " + childID);
            else
            {
                TreeNode[] parent = mDocument.treeView.Nodes.Find(parentID, true);

                AddNode(parent[0], childTypeName, childID);
            }
        }
        public void OnNodeRemoved(string parentID, string childID, string childTypeName)
        {
            RemoveNode();
        }

        private void GotFocus(Object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("BehaviorTreeEditorWorkspace.GotFocus() - ");
        }

        public void Resize() { }
        public void Show() { mIsActive = true; }
        public void Hide() { mIsActive = false; }
        #endregion

        // todo: allow drag and drop from asset browser to the treeview
        // todo: allow drag and drop of behavior tree nodes within the tree
        private void OnNodeClick(object sender, TreeNodeMouseClickEventArgs e)
        {
           
            
        }

        private void OnOpen(object sender, EventArgs e)
        {
            // todo: do we already have an open behavior tree?
            if (mRoot != null)
            {
                if (MessageBox.Show("Save existing Behavior Tree?") == DialogResult.Yes)
                {
                    // todo:
                }
            }
            // todo: add ability to drag and drop from asset browser with extension .kgbbehavior
            // todo: add ability to right mouse click or double click on a .kgbbehavior and open it in the BehaviorTreeWorkspace
            // todo: browse for behavior tree.  make sure it has .kgbbehavior extension

            string path = AppMain.MOD_PATH + "\\caesar\\behaviors\\";


            OpenFileDialog openFile = new OpenFileDialog();
            openFile.InitialDirectory = path;

            openFile.Filter = "behavior trees (*.kgbbehavior)|*.kgbbehavior"; 
            //openFile.FilterIndex = DEFAULT_FILTER_INDEX;

            openFile.RestoreDirectory = true;

            if (openFile.ShowDialog() == DialogResult.OK)
            {
                string filename = openFile.FileName;
                // todo: i think we need a drag and drop version of "open" when the extension is .kgbbehavior. 
                //        Otherwise when dragging and dropping on nodes from \\scripts_behavior\\ should add to the dropped on node.
                Keystone.Elements.Node node = Keystone.ImportLib.Load(filename, false, true, true, null);

                if (node is Keystone.Behavior.Composites.Composite)
                {
                    mRoot = (Keystone.Behavior.Composites.Composite)node;


                    string parentID = null;
                    TreeNode rootNode = KeyPlugins.StaticControls.GenerateTreeBranch(AppMain.PluginService, mDocument.treeView, null, parentID, node.ID, node.TypeName, null, true, null);
 
                }
                
            }

            // todo: after closing the behavior tree editor, IncrementRef and DecremetRef the mRoot



        }

        private void OnNew(object sender, EventArgs e)
        {

            if (mRoot != null)
            {
                // do we want to remove the current behavior tree if it exists?

                mDocument.Clear();
                Repository.IncrementRef(mRoot);
                Repository.DecrementRef(mRoot);
            }

            string text = null;
            DialogResult result = KeyPlugins.StaticControls.InputBox ("Enter New Name", "Name", ref text);

            if (result == DialogResult.OK)
            {
                // check if file with this name already exists
                string fullPath = System.IO.Path.Combine(mBehaviorTreePath, text + EXTENSION);
                
                // todo: how do we ensure that Behavior tree node IDs are unique?  Perhaps we still use guid
                //       but as we do in our main treeview, show typename - name. IpageableResource however will still use the resource path as "id".  todo:  Actually
                //       the root node ID should be the filepath to the tree.  We should verify no existing tree of this name exists and if it does, prompt for overwrite.

                // todo: don't EXTENSION if the user has already typed it in
                // if ok, create new xmldatabase with EXTENSION and a default root node. 
                // todo: is hould use mHost.Node_Create() ?? 
                string id =  AppMain.ModName + "\\behaviors\\" + text + EXTENSION;
                mRoot = new Keystone.Behavior.Composites.Sequence(id);
                AppMain.PluginService.Node_Create("Sequence", null);

                
            }
        }


        private void OnSave(object sender, EventArgs e)
        {

            if (mRoot == null) return;

            string path = System.IO.Path.Combine(AppMain.MOD_PATH, mRoot.ID);
            Keystone.IO.XMLDatabaseCompressed treeDB = new Keystone.IO.XMLDatabaseCompressed();

            // NOTE: The save filename is derived from the root Behavior node's ID
            treeDB.Create(Keystone.Scene.SceneType.Prefab, path, mRoot.TypeName);
            treeDB.Write(mRoot, true, false, null);
            treeDB.SaveAllChanges();

        }


        private void AddNode(TreeNode parentNode, string typename, string id)
        {

            string text = typename + ", " + id;
            TreeNode newNode = new TreeNode(text);
            parentNode.Nodes.Add(newNode);

        }

        private void RemoveNode()
        {
            TreeNode selectedNode = mDocument.treeView.SelectedNode;
            mDocument.treeView.Nodes.Remove(selectedNode);
        }

        private void GetNodeTypeAndID(TreeNode treeNode, out string ID, out string parentID, out string typeName)
        {
            ID = null;
            parentID = null;
            typeName = null;

            if (treeNode == null) return;

            ID = treeNode.Name;
            if (treeNode.Parent != null)
                parentID = treeNode.Parent.Name;

            string[] split = treeNode.Text.Split(',');
            typeName = split[0];

        }

        private void treeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeView tree = (TreeView)sender;
            if (tree == null) return;

            string nodeID ;
            string parentID;
            string typeName;

            GetNodeTypeAndID(e.Node, out nodeID, out parentID, out typeName);

            if (e.Button == MouseButtons.Right)  // right mouse click menu
            {
                ContextMenuStrip menu = new ContextMenuStrip();
                ToolStripMenuItem menuItem = null;
                ToolStripMenuItem childItem = null;



                switch (typeName)
                {
                    case "Condition":
                    case "Event":
                    case "Guard":
                    case "Trigger": // Stimuli
                        break;

                    case "Decorator":
                    case "Loop":
                    case "InfiniteLoop":
                        break;

                    case "Script":
                        menuItem = new ToolStripMenuItem("Edit");
                        menuItem.Click += menuEdit_Behavior_Script_Click;
                        menu.Items.Add(menuItem);

                        menu.Items.Add(new ToolStripSeparator());

                        menuItem = new ToolStripMenuItem("Delete");
                        menuItem.Click += menuDelete_Click;
                        menu.Items.Add(menuItem);
                        break;
                    case "Action":
                    case "Move":
                    case "Animate":
                        break;

                    case "Composite":
                    case "Switch":
                    case "Sequence":
                    case "Parallel":
                    case "Random":
                        // ACTIONS
                        menuItem = new ToolStripMenuItem("Action");
                        menu.Items.Add(menuItem);

                        childItem = new ToolStripMenuItem("Script");
                        childItem.Click += menuAddBehavior_Click;
                        menuItem.DropDownItems.Add(childItem);

                        childItem = new ToolStripMenuItem("Move");
                        menuItem.DropDownItems.Add(childItem);
                        childItem = new ToolStripMenuItem("Animate");
                        menuItem.DropDownItems.Add(childItem);
                        // play sound
                        // 

                        // DECORATORS
                        menuItem = new ToolStripMenuItem("Decorator");
                        menu.Items.Add(menuItem);

                        childItem = new ToolStripMenuItem("Loop");
                        menuItem.DropDownItems.Add(childItem);

                        childItem = new ToolStripMenuItem("InfiniteLoop");
                        menuItem.DropDownItems.Add(childItem);

                        // CONDITIONS
                        menuItem = new ToolStripMenuItem("Condition");
                        menu.Items.Add(menuItem);

                        childItem = new ToolStripMenuItem("Event");
                        menuItem.DropDownItems.Add(childItem);

                        childItem = new ToolStripMenuItem("Guard");
                        menuItem.DropDownItems.Add(childItem);

                        childItem = new ToolStripMenuItem("Trigger");
                        menuItem.DropDownItems.Add(childItem);

                        // COMPOSITES
                        menuItem = new ToolStripMenuItem("Composite");
                        menu.Items.Add(menuItem);

                        childItem = new ToolStripMenuItem("Switch");
                        menuItem.DropDownItems.Add(childItem);

                        childItem = new ToolStripMenuItem("Sequence");
                        childItem.Click += menuAddBehavior_Click;
                        menuItem.DropDownItems.Add(childItem);

                        childItem = new ToolStripMenuItem("Parallel");
                        menuItem.DropDownItems.Add(childItem);
                        childItem = new ToolStripMenuItem("Random");
                        menuItem.DropDownItems.Add(childItem);


                        // COPY\CUT\PASTE\DELETE SEPERATOR
                        menu.Items.Add(new ToolStripSeparator());

                        menuItem = new ToolStripMenuItem("Copy");
                        menu.Items.Add(menuItem);

                        menuItem = new ToolStripMenuItem("Cut");
                        menu.Items.Add(menuItem);

                        menuItem = new ToolStripMenuItem("Paste");
                        menu.Items.Add(menuItem);

                        menuItem = new ToolStripMenuItem("Delete");
                        menuItem.Click += menuDelete_Click;
                        menu.Items.Add(menuItem);
                        break;
                    default:
                        throw new Exception("Unexpected type '" + typeName + "'");
                }

                menu.Show(tree, e.X, e.Y);
            }
        }


        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }


        private void OnBehaviorTree_DragDrop(object sender, EventArgs e)
        {
            // the only time we do not create a new node is if we've drag and dropped
            // or copy and pasted a sub-tree.

            // 
        }

        #region MenuClick Handlers
        private void menuEdit_Behavior_Script_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menu;
            if (!(sender is ToolStripMenuItem)) return;

            menu = (ToolStripMenuItem)sender;

            string scriptNodeID = menu.Name;
            string parent = (string)menu.Tag;

            // todo: show the edit panel 
            // todo: the edit panel can be closed without closing the entire document
            // EditResource("Script", scriptNodeID, mTargetNodeID);
        }


        private void menuAddBehavior_Click(object sender, EventArgs e)
        {
            // todo: i really should pass in a callback to Node_Create() so we can modify the treeView when we know a node has been added or deleted
            // todo: ability to rename nodes
            ToolStripMenuItem menu;
            if (!(sender is ToolStripMenuItem)) return;

            menu = (ToolStripMenuItem)sender;
            TreeNode node = mDocument.treeView.SelectedNode;
            if (node == null) return;

            string parentNodeID = node.Name;

            switch (menu.Text)
            {
                case "Sequence":
                    // todo: can a composite node have multiple parents within the same behavior tree?
                    // todo: i should be able to name the sequence or composite behavior so long as it is unique
                    AppMain.PluginService.Node_Create(menu.Text, parentNodeID);
                    break;

                case "Script":
                    // if resourcePath argument is null, a browse dialog will open before sending the node create request to the server
                    // todo: the problem is, we don't know what the name of that script is so we can't add the node.  There's also
                    // no callback handler to let us know when the node has been created after the request to the server
                    string filter = "Behaviors|*.kgbbehavior"; ;
                    AppMain.PluginService.Node_Create (menu.Text, parentNodeID, null, filter );
                    break;
                default:
                    break;
            }

            //AddNode(node, menu.Text, id);
        }

        private void menuDelete_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            if (!(sender is ToolStripMenuItem)) return;

            TreeNode selectedNode = mDocument.treeView.SelectedNode;

            string nodeID;
            string parentID;
            string typeName;

            GetNodeTypeAndID(selectedNode, out nodeID, out parentID, out typeName);


            AppMain.PluginService.Node_Remove(nodeID, parentID);

            

            System.Diagnostics.Debug.WriteLine("Node_Remove command sent...");
        }

        private void DeleteNode()
        {
            // todo: enable keyboard 'delete'
            TreeNode node = mDocument.treeView.SelectedNode;
            if (node == null) return;


        }
        ////treeBehaviors.ContextMenuStrip.Enabled 
        ////treeBehaviors.ContextMenu = 
        //// treeBehaviors.ContextMenuStrip = 


        ////treeBehaviors.DragDrop += 
        ////treeBehaviors.DragEnter +=
        ////treeBehaviors.DragLeave +=
        ////treeBehaviors.DragOver +=
        ////treeBehaviors.ItemDrag += 

        ////treeBehaviors.MouseDown += 
        ////treeBehaviors.ItemSelectionChanged+=
        ////treeBehaviors.ParentChanged+=
        ////    treeBehaviors.
        #endregion
    }
}
