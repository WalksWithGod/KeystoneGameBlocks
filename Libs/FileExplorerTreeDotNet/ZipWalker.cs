using System;
using System.Collections.Generic;
using System.Text;
using Ionic.Zip;
using System.IO;

namespace FileExplorerTreeDotnet
{
    public class ZipWalker : DirWalker
    {
        string mZipFilePath;
        public ZipWalker(string zipFilePath, string path) : base (path)
        {
            // should i just add the zip path here and keep it?
            // then this entire TreeViewFileExplorer doesnt need
            // to know the ResourceDescriptor struct
            // actually i dont think it matters because the caller
            // is the one that should implement this ZipWalker
            // derived from DirWalker
            mZipFilePath = zipFilePath;
        }

        /// <summary>
        /// This is called in the constructor of every DirectoryNode and thus we 
        /// recurse through it's children.
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="showFiles"></param>
        internal override void Virtualize(DirectoryNode parentNode, bool showFiles)
        {

            int fileCount = 0;

            try
            {
                // TODO: this is the key here, we need to add a fakechild node 
                // in order to bootstrap this process

                if (showFiles == true)
                {
                    string[] files = KeyCommon.IO.ArchiveIOHelper.SelectFiles(mZipFilePath, KeyCommon.IO.ArchiveIOHelper.CleanZipDirPath(parentNode.FullPath));
                    if (files != null)
                        fileCount = files.Length;
                }

                // add a fake child but do not expand.  Thus we get the 
                // expand + sign and can expand further when/if the user clicks
                // NOTE: Here the parentNode.FullPath is in the form of \\dirName
                // but what we want is dirName\\subDirName
                // with no preceeding \\
                 string[] dirs = KeyCommon.IO.ArchiveIOHelper.SelectDirectories(mZipFilePath, parentNode.FullPath);
                if (dirs != null)
                    fileCount += dirs.Length;

                if (fileCount > 0)
                    new FakeChildNode(parentNode);
            }
            catch
            {
            }
        }

        public override void LoadDirectory(DirectoryNode parentNode, string directoryPathInArchive)
        {
            string[] results = KeyCommon.IO.ArchiveIOHelper.SelectDirectories(mZipFilePath, directoryPathInArchive);
            if (results != null)
                for (int i = 0; i < results.Length ; i++)
                    new ZipDirectoryNode(mZipFilePath, parentNode, results[i]);
        }

        public override void LoadFiles(DirectoryNode parentNode, string path)
        {
            mPath = path;
            // TODO: here we only show files in the thumbnail window
            // not in our treeview 
            string[] results = KeyCommon.IO.ArchiveIOHelper.SelectFiles(mZipFilePath, path);

            if (results != null)
                for (int i = 0; i < results.Length; i++)
                {
                    string filename = Path.GetFileName(results[i]);
                    new FileNode(parentNode, filename);
                }

            
        }
    }
}
