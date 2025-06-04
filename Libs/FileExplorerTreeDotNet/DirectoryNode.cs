using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Ionic.Zip;

namespace FileExplorerTreeDotnet
{
    public class ZipDirectoryNode : DirectoryNode
    {
        public ZipDirectoryNode(string zipFilePath, DirectoryNode parent, string directoryPath)
            : base(Path.GetFileName (directoryPath) == "" ? directoryPath : Path.GetFileName (directoryPath))
        {
            this._directoryPath = directoryPath;
            this.ImageIndex = FileSystemTreeView.Folder;
            this.SelectedImageIndex = this.ImageIndex;

            parent.Nodes.Add(this);

            _walker = new ZipWalker(zipFilePath, _directoryPath);  
            _walker.Virtualize(this, this.TreeView.ShowFiles);
        }

        public ZipDirectoryNode(string zipFilePath, FileSystemTreeView treeView, string directoryPath)
            : base(Path.GetFileName(directoryPath) == "" ? directoryPath : Path.GetFileName(directoryPath))
        {

            
            this._directoryPath = directoryPath;
            this.ImageIndex = FileSystemTreeView.Folder;
            this.SelectedImageIndex = this.ImageIndex;

            treeView.Nodes.Add(this);

            _walker = new ZipWalker(zipFilePath, _directoryPath);  
            _walker.Virtualize(this, this.TreeView.ShowFiles);

        }
    }

    public class DirectoryNode : TreeNode
    {
        protected string _directoryPath;
        protected DirWalker _walker;

        protected DirectoryNode(string directoryPath) : base (directoryPath )
        { 
        }

        public DirectoryNode(DirectoryNode parent, string directoryPath)
            : this(Path.GetFileName (directoryPath) == "" ? directoryPath : Path.GetFileName (directoryPath))
        {
            this._directoryPath = directoryPath;
            this.ImageIndex = FileSystemTreeView.Folder;
            this.SelectedImageIndex = this.ImageIndex;

            parent.Nodes.Add(this);

            _walker =  new DirWalker(directoryPath);
            _walker.Virtualize(this, this.TreeView.ShowFiles);
        }

        public DirectoryNode(FileSystemTreeView treeView, string directoryPath)
            : this(Path.GetFileName(directoryPath) == "" ? directoryPath : Path.GetFileName(directoryPath))
        {

            
            this._directoryPath = directoryPath;
            this.ImageIndex = FileSystemTreeView.Folder;
            this.SelectedImageIndex = this.ImageIndex;

            treeView.Nodes.Add(this);

            _walker = new DirWalker(directoryPath );
            _walker.Virtualize(this, this.TreeView.ShowFiles);

        }

        

        public void LoadDirectory()
        {
            _walker.LoadDirectory(this, _directoryPath);
        }

        public void LoadFiles()
        {
            _walker.LoadFiles(this, _directoryPath);
        }

        public bool Loaded
        {
            get
            {
                if (this.Nodes.Count != 0)
                {
                    if (this.Nodes[0] is FakeChildNode)
                        return false;
                }

                return true;
            }
        }

        public new FileSystemTreeView TreeView
        {
            get { return (FileSystemTreeView)base.TreeView; }
        }
    }
}
