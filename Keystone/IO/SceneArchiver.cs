using System;
using System.Diagnostics;
using System.IO;
using ComponentAce.Compression.Archiver;
using ComponentAce.Compression.ZipForge;
using Ionic.Zip;
using Ionic.Zlib;

namespace Keystone.IO
{

    /// <summary>
    /// SceneArchiver maintains the tmp files used during scene management.  
    /// In this way the program doesnt need to constantly re-extract files from the 
    /// scene archive whenever it needs to do a read.  However, if the disk space on the tmp
    /// drive becomes too large, it can delete those files.
    /// 
    /// Further, it only does so on demand.  This way if the file doesnt exist in the Archive
    /// then it'll just create a new tmp file for it and on either SceneArchiver.Save() or .SaveAs()
    /// it will add these new files to the archive
    /// 
    /// </summary>
    public class SceneArchiver : IDisposable
    {
    	// TODO: why still ComponentAce.Compression.ZipForge
    	// TODO: why did i never switch this to DotNetZipLib? // E:\dev\c#\KeystoneGameBlocks\Libs\DotNetZipLib-DevKit-v1.9\DotNetZip-v1.9\Release\Ionic.Zip.dll
    	//       using "STORE" compression option would give us fast speeds
    	//       and we wouldn't have to use this closed source ZipForge library
    	//       WARNING: seems still bugs in DotNetZipLib.   .Net 4.5 has built in Zip functions
    	//       but recall that limits my users from running WindowsXP since 4.5 requires Win7+
        private ZipForge _archive;
        private bool _isOpen;
        private Stream mStream;
        private bool mUsingStream = false;

        public void Open(Stream stream)
        {
            try
            {
                mStream = stream;
                mUsingStream = true;
                _isOpen = true;
            } 
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("SceneArchiver.Open() - ERROR: " + ex.Message);
                throw ex; // we want the caller to know the exception occurred
            }
        }

        private string mFileName;
        public void Open(string filename,  bool append, FileMode fileMode, FileAccess fileAccess)
        {
            // NOTE: Since I want all these to be threadsafe methods, we will allow multiple "Open" and "Reads" to be done
            // on any given archive.  The rule though is that the archiveName must be unique since it's used as a key.
            // open the archive
            try
            {
                bool create = true;
                if (File.Exists(filename))
                {
                    if (!append)
                    {
                        File.Delete(filename);
                        create = true;
                    }
                    else create = false;
                }

                mFileName = filename;

                if (create)
                {
                    ZipFile zip = new ZipFile(mFileName);
                    zip.Save();
                    zip.Dispose();
                }

                
                // obsolete - switched to Ionic.Zip
                //_archive = new ZipForge();
                //_archive.CompressionLevel = ComponentAce.Compression.Archiver.CompressionLevel.None;
                //_archive.FileName = filename;
                
                // note: ZipArchiver will throw an exception if the filename is not a valid Zip archive OR a non-existant archive which it will create.
                // It will in otherwords, throw an exception on a 0 byte file or corrupt zip
                //_archive.OpenArchive(fileMode, fileAccess);
                _isOpen = true;
            }
            catch (Exception ex)
            {
                // NOTE: If the filename is that of a temp filename created via Keystone.IO.XMLDatabase.GetTempFileName() then you must first delete that file
                // if you want a new Archive created because otherwise that 0 byte file will be reported here as an invalid archive type.
                System.Diagnostics.Debug.WriteLine("SceneArchiver.Open() - ERROR: " + ex.Message);
                throw ex; // we want the caller to know the exception occurred
            }
        }

        public string Comment
        {
            get { return _archive.Comment; }
            set { return; } // _archive.Comment = value; }
        }

        // TODO: i could maybe make most of the methods and properties in this class internal so only the FileManager can directly access them
        public string FileName
        {
            get { return _archive.FileName; }
        }

        /// <summary>
        /// Stores the individual XMLDocument files to the Archive.  If the files already exist they are replaced in the archive.
        /// </summary>
        public void Save(XMLDatabase xmldb)
        {
            using (ZipFile zip = new ZipFile(mFileName))
            {

                System.Collections.Generic.List<FileStream> filestreams = new System.Collections.Generic.List<FileStream>();
                foreach (XMLDatabase.FileMap map in xmldb.FileMaps.Values)
                {
                    FileStream fs = new FileStream(map.FilePath, FileMode.Open);
                    string newname = map.StoredName; // GetTableName(map.TypeName);
                    zip.AddEntry(newname, fs);
                    filestreams.Add(fs);
                    // _archive.AddFromStream(newname, fs);
                    //fs.Close();
                }

                zip.Save();

                // can only close the fs objects after saving the zip
                foreach (FileStream fs in filestreams)
                {
                    fs.Close();
                    fs.Dispose();
                }
                filestreams = null;
            }
        }

        // results in a new archive being saved with the contents of the previous copied to it at the new path/filename.
        public void SaveAs(string newArchiveFilename, XMLDatabase xmldb)
        {
            _archive.CloseArchive();

            if (_archive.FileName == newArchiveFilename)
            {       
                Save(xmldb);
                return; 
            }

            if (File.Exists(newArchiveFilename))
                File.Delete(newArchiveFilename);

            File.Copy(_archive.FileName, newArchiveFilename);

            _archive.Dispose();

            // create a new archive
            _archive = new ZipForge();
            _archive.FileName = newArchiveFilename;
            _archive.OpenArchive();
        }

        public bool FindEntry(string tableName, ref Stream result)
        {
            
            System.IO.MemoryStream mem = new System.IO.MemoryStream();
            ZipFile zip;

            if (mUsingStream)
                zip = ZipFile.Read(mStream);
            else
                zip = new ZipFile(mFileName);
   
            ZipEntry entry = zip[tableName];
            if (entry != null)
            {
                entry.Extract(mem);
                mem.Seek(0, System.IO.SeekOrigin.Begin);
                result = mem;
                //System.Collections.Generic.ICollection<ZipEntry> entries = zip.SelectEntries(tableName);

                //foreach (ZipEntry zipentry in entries)
                //{
                //    result = zipentry;
                //    break;
                //}
            }

            // reset the stream so that on next FindEntry call, we start search from beginning
            if (mUsingStream)
                mStream.Seek(0, SeekOrigin.Begin);

            zip.Dispose();
            if (result == null) return false;
            return true;

            // obsolete - using Ionic.Zip instead of ZipForge
            //ArchiveItem item = new ArchiveItem();
           // bool found = _archive.FindFirst(tableName, ref item);
           // result = item;
           // return found;  
        }

        public Stream ExtractToFileStream(string tablename)
        {
            try
            {



                Stream stream = null;
                if (FindEntry(tablename, ref stream))
                {
                    return stream;
                }

                return null;

                // obsolete - switched from ZipForge to Ionic.Zip
                //ArchiveItem archiveItem = new ArchiveItem();
                //// get memory stream of extracted archive item.  
                //if (_archive.FindFirst(tablename, ref archiveItem))
                //{
                //    MemoryStream ms = new MemoryStream ();
                //    _archive.ExtractToStream(tablename, ms);
                  
                //    ms.Flush();
                //    // must seek to start since the cursor is at the end of the document after extracting into the stream
                //    ms.Seek (0, SeekOrigin.Begin);
                //    return ms;
                //}
                //else return null;
            }
            catch (IOException)
            {
                Trace.WriteLine("SceneArchiver.Extract() -- IO Error.");
            }
            catch (Exception ex)
            {
                Trace.WriteLine("SceneArchiver.Extract() - ERROR: " + ex.Message);
            }
            return null;
        }

        #region IDisposable Members
        private bool _disposed = false;

        ~SceneArchiver()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // pass true if managed resources should be disposed. Otherwise false.
        private void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    DisposeManagedResources();
                    _disposed = true;
                }
                DisposeUnmanagedResources();
            }
        }

        protected virtual void DisposeManagedResources()
        {
            if (_isOpen)
            {
                if (mUsingStream)
                {
                    mStream.Dispose();
                    mStream = null;
                }
                // normally disposes (which closes) when it is GC'd.

                // obsolete - switched from ZipForge to Ionic.Zip
                //_archive.CloseArchive();
                //_archive.Dispose();
                _isOpen = false;
            }
        }

        protected virtual void DisposeUnmanagedResources()
        {
            //if (FileMaps != null && FileMaps.Count > 0)
            //    foreach (FileMap map in FileMaps.Values)
            //    {
            //        File.Delete(map.TempFile);
            //    }
        }

        protected void CheckDisposed()
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().Name + " is disposed.");
        }

        protected bool IsDisposed
        {
            get { return _disposed; }
        }
        #endregion
    }
}