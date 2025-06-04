using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace FileExplorerTreeDotnet
{
    public class FileNode : TreeNode
    {
        private string _filePath;
        private DirectoryNode _directoryNode;

        public FileNode(DirectoryNode directoryNode, string filePath)
            : base(filePath)
        {
            this._directoryNode = directoryNode;
            this._filePath = filePath;

            this.ImageIndex = ((FileSystemTreeView)_directoryNode.TreeView).GetIconImageIndex(filePath);
            this.SelectedImageIndex = this.ImageIndex;

            _directoryNode.Nodes.Add(this);
        }
    }

    public class FakeChildNode : TreeNode
    {
        public FakeChildNode(TreeNode parent)
            : base()
        {
            parent.Nodes.Add(this);
        }
    }
}
