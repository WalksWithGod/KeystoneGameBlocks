using System;


namespace FileExplorerTreeDotnet
{
    public class DirectorySelectedEventArgs : EventArgs
    {
        public string DirectoryName;

        public DirectorySelectedEventArgs(string directoryName)
        {
            this.DirectoryName = directoryName;
        }
    }
}
