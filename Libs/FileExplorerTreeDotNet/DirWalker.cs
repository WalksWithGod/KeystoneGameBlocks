using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace FileExplorerTreeDotnet
{
    public class DirWalker
    {
        protected string mPath;

        public DirWalker(string path)
        {
            mPath = path;
        }
        public virtual void LoadDirectory(DirectoryNode parentNode, string path)
        {
            // TODO: callback here?  
            mPath = path;

            DirectoryInfo info = new DirectoryInfo(path);
            foreach (DirectoryInfo directoryInfo in info.GetDirectories())
            {
                new DirectoryNode(parentNode, directoryInfo.FullName);
            }
        }

        public virtual void LoadFiles(DirectoryNode parentNode, string path)
        {
            mPath = path;

            DirectoryInfo info = new DirectoryInfo(path);
            foreach (FileInfo file in info.GetFiles())
            {
                new FileNode(parentNode, file.FullName);
            }
        }

        internal virtual void Virtualize(DirectoryNode parentNode, bool showFiles)
        {
            int fileCount = 0;

            try
            {
                DirectoryInfo info = new DirectoryInfo(mPath);
                if (showFiles == true)
                    fileCount = info.GetFiles().Length;

                // add a fake child but do not expand.  Thus we get the 
                // expand + sign and can expand further when/if the user clicks
                if ((fileCount + info.GetDirectories().Length) > 0)
                    new FakeChildNode(parentNode);
            }
            catch
            {
            }
        }

    }
}
