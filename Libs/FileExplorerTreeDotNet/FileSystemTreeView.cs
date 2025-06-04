using System;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections;
using System.Drawing;
using Ionic.Zip;

namespace FileExplorerTreeDotnet
{
    /// <summary>
    /// Summary description for DirectoryTreeView.
    /// </summary> 
    public class FileSystemTreeView : TreeView
    {
        // public event DirectorySelectedDelegate DirectoryAdded;
        // public event DirectorySelectedDelegate DirectoryRemoved;
        // public event DirectorySelectedDelegate DirectoryRenamed;
        // TODO: then in 
        public delegate void DirectorySelectedDelegate(object sender, DirectorySelectedEventArgs e);
        public event DirectorySelectedDelegate DirectorySelected;
        private bool _showFiles = true;
        private ImageList _imageList = new ImageList();
        private Hashtable _systemIcons = new Hashtable();
        string mModFullPath;
        public static readonly int Folder = 0;      

        public FileSystemTreeView()
        {
            this.ImageList = _imageList; 
        
            // TODO: some event's now id like for the form to be able to wire up and respond too? Hrm..
            //this.MouseDown += new MouseEventHandler(FileSystemTreeView_MouseDown);         
            //this.BeforeExpand += new TreeViewCancelEventHandler(FileSystemTreeView_BeforeExpand);
            //this.AfterSelect  += new TreeViewEventHandler(FileSystemTreeView_AfterSelect);

        }


        public void Load(string modFullPath, string directoryPath, string iconPath)
        {
            //if( Directory.Exists( directoryPath ) == false )
            //    throw new DirectoryNotFoundException( "Directory Not Found" );
            mModFullPath = modFullPath;
            _systemIcons.Clear();
            _imageList.Images.Clear();
            Nodes.Clear();

            Icon folderIcon = new Icon(iconPath);
            
            _imageList.Images.Add(folderIcon);
            _systemIcons.Add(FileSystemTreeView.Folder, 0);

            // so obviously for the imageviewer, we have callbacks
            // but for here, this then calls these directorynodes which handle
            // the recursion themselves... this cannot be
            // These nodes must be created by callbacks...
			DirectoryNode temporaryRoot;
            if (modFullPath.EndsWith (".zip"))
                temporaryRoot = new ZipDirectoryNode(modFullPath, this, directoryPath);
    		else   
    		{
    			DirectoryInfo rootDirectory = new DirectoryInfo (modFullPath);
    			DirectoryInfo[] directories = rootDirectory.GetDirectories ();
    			
	            // instead of adding useless root node, add and expand starting from first child nodes
    			if (directories != null)
    			{
    				for (int i = 0; i < directories.Length; i++)
    				{
    					DirectoryNode child = new DirectoryNode (this, System.IO.Path.Combine(modFullPath, directories[i].Name)); 
		    			// child.Expand();
    				}
    			}
    		}

        }

        //TODO: i think this is main thing that needs to be handled
        // by caller 
        public void Load( string directoryPath )
        {
            //if( Directory.Exists( directoryPath ) == false )
            //    throw new DirectoryNotFoundException( "Directory Not Found" );

            _systemIcons.Clear();
            _imageList.Images.Clear();
            Nodes.Clear();

            // TODO: remove hardcoded path
            Icon folderIcon = new Icon(@"d:\dev\c#\KeystoneGameBlocks\Editor\Resources\" + "ClosedFolder.ICO"); 

            _imageList.Images.Add( folderIcon );
            _systemIcons.Add( FileSystemTreeView.Folder, 0 );
        
            // so obviously for the imageviewer, we have callbacks
            // but for here, this then calls these directorynodes which handle
            // the recursion themselves... this cannot be
            // These nodes must be created by callbacks...
            
            DirectoryNode root = new DirectoryNode(this, directoryPath);
                  
            root.Expand();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            TreeNode node = this.GetNodeAt(e.X, e.Y);

            if (node == null)
                return;

            this.SelectedNode = node; //select the node under the mouse   
            node.Expand();

        }

        protected override void OnBeforeExpand(TreeViewCancelEventArgs e)
        {

            base.OnBeforeExpand(e);
            if (e.Node is FileNode) return;
            DirectoryNode node = (DirectoryNode)e.Node;

            if (!node.Loaded)
            {
                node.Nodes[0].Remove(); //remove the fake child node used for virtualization

                // TODO: controller must be called here... some OnBeforeExpand
                // we can use the built in walker or a custom

                node.LoadDirectory();
                if (this._showFiles == true)
                    node.LoadFiles();
            }
        }

        protected override void OnAfterSelect(TreeViewEventArgs e)
        {
            base.OnAfterSelect(e);

            // Raise the DirectorySelected event.
            if (DirectorySelected != null)
            {
                DirectorySelected(this,
                                  new DirectorySelectedEventArgs(e.Node.FullPath));
            }
        }

        public int GetIconImageIndex( string path )
        {  
            string extension = Path.GetExtension( path );

            if( _systemIcons.ContainsKey( extension ) == false )
            {
                Icon icon = null;
                try
                {
                    icon = ShellIcon.GetSmallIcon(path);
                    if (icon != null)
                    {
                        _imageList.Images.Add(icon);
                        _systemIcons.Add(extension, _imageList.Images.Count - 1);
                    }
                    else return 0;
                }
                catch
                {
                    return 0;
                }
            }
            
            return (int)_systemIcons[ Path.GetExtension( path )];         
        }

        public bool ShowFiles
        {
            get{ return this._showFiles; }
            set{ this._showFiles = value; }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // FileSystemTreeView
            // 
            this.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.OnBeforeExpand);
            this.ResumeLayout(false);

        }

        private void OnBeforeExpand(object sender, TreeViewCancelEventArgs e)
        {

        }
    }
}