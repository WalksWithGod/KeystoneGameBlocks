using System;
using Ionic.Zip;
using Ionic.Zlib;
using System.Collections.Generic;
using System.IO;

namespace KeyCommon.IO
{
    public static class ArchiveIOHelper
    {
        public static bool IsFolder(string entryPath)
        {
            string ext = Path.GetExtension(entryPath);
            return string.IsNullOrEmpty(ext);

            // TODO: the above is hackish
            // i could attempt to select the entry and query if it's a folder or file
        }

        public static string GetTextFromArchive(string entryPath, string password, string zipFilePath)
        {
            ZipFile zip = null;
            try
            {
                zip = new ZipFile(zipFilePath);
                string formattedEntryName = CleanZipDirPath(entryPath);
                if (zip != null)
                    return GetTextFromArchive(formattedEntryName, password, zip);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ArchiveIOHelper.GetTextFromArchive() - ERROR: " + ex.Message);
            }
            finally
            {
                if (zip != null)
                    zip.Dispose();
            }
            return null;
        }

        public static string GetTextFromArchive(string entryPath, ZipFile zip)
        {
            return GetTextFromArchive(entryPath, null, zip);
        }

        // For shaders, used for effect.CreateEffectFromString method
        // will return the text of the shader
        // bool loaded = Asteroid.Shader.CreateFromEffectString(ImportLib.GetTextFromArchive("Effects/MiniMesh.fx"));
        public static string GetTextFromArchive(string entryPath, string password, ZipFile zip)
        {
            foreach (ZipEntry entry in zip.Entries)
            {
                if (entry.FileName == entryPath)
                {
                    byte[] bBuffer = new byte[(int)(entry.UncompressedSize)];
                    CrcCalculatorStream stream;
                    if (string.IsNullOrEmpty(password))
                        stream = entry.OpenReader();
                    else
                        stream = entry.OpenReader(password);

                    stream.Read(bBuffer, 0, (int)stream.Length);
                    stream.Close();
                    string source = new string(System.Text.Encoding.UTF8.GetChars(bBuffer));
                    return source;
                }
            }
            return null;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="entryGUID">128 bit hash id to an entry.  Better than sending string paths over network</param>
        /// <param name="password"></param>
        /// <param name="dbIndex">our various MOD DBs will be indexed and specifying which to by id instead of string path means less network traffic
        /// we may still potentially use a GUID id for the mod DB too since we can associate these DBs by a GUID on first creation of the DB</param>
        /// <returns></returns>
        public static System.IO.Stream GetStreamFromArchive(System.Guid entryGUID, string password, int dbIndex)
        {
            // TODO: im not sure how feasible this is... 
            // the idea is we'd maintain a filemapping ourselves but
            // that mapping would have to be identicle on server and client
            // which seems problematic given imports/deletes over the network
            // by multiple users would need sychronization to make sure
            // both sides were in sycn.
            // Perhaps a fix would be to not store by int index but by a 128 bit 

            // TODO: 
            //System.Guid.NewGuid().
            throw new NotImplementedException();
            return null;
        }

        public static System.IO.Stream GetStreamFromMod(string entryPath, string password, string modPath)
        {
            ZipFile zip = null;
            try
            {
                System.Diagnostics.Debug.WriteLine("ArchiveIOHelper.GetSTreamFromMod...");
                bool isArchivedMod = IsFolder(modPath) == false;
            	
            	// if modFullPath is a folder in the Mods directory...
            	if (isArchivedMod == false)
            	{
            		string cleanedEntryPath = entryPath.Replace ('/', '\\');
            		string fullFilePath = Path.Combine (modPath, cleanedEntryPath);
                    // TODO: this fails if kgbentity is currently opened in say 7zip
            		FileStream fs = File.Open (fullFilePath, FileMode.Open, FileAccess.Read);
            		return fs;
            	}
            	// else this is a zip archive mod
                else if (System.IO.File.Exists(modPath))
                {
                    zip = new ZipFile(modPath);
                    if (zip != null)
                    {
                        //foreach (ZipEntry entry in zip.Entries)
                        //{
                        //    if (entry.FileName == entryPath)
                        //    {
                        //        byte[] bBuffer = new byte[(int)(entry.UncompressedSize)];
                        //        CrcCalculatorStream stream = entry.OpenReader(password);
                        //        //stream.Read(bBuffer, 0, (int)stream.Length);
                        //        //stream.Close();
                        //        return stream;

                        // TODO: if a resource is depending on anoterh resource that is 
                        // stored in a seperate mod zip file, and that mod zip file does not
                        // exist, then this will fail obviously.  
                        // TODO: one thing that "could" be done is if the archive itself does not exist
                        // to instead load some default items... but.. that is problematic.
                        // if you cant load everything it really should fail, but it should fail in
                        // a way that the app is notified and can shut down.
                        // Notification is required because our loading is threaded and pages in
                        string formattedEntryName = CleanZipDirPath(entryPath);
                        System.IO.MemoryStream mem = new System.IO.MemoryStream();

                        if (zip.ContainsEntry (formattedEntryName) == false)
                        {
                        	System.Diagnostics.Debug.WriteLine ("ArchiveIOHelper.GetStreamFromMod() - ERROR: Entry '" + formattedEntryName + "' not found.");
                        	return null;
                        }
                        zip[formattedEntryName].Extract(mem);
                        mem.Seek(0, System.IO.SeekOrigin.Begin);
                        return mem;

                        //}
                        //}
                    }

                }
                else
                {
                    // TODO: if a resource is depending on anoterh resource that is 
                    // stored in a seperate mod zip file, and that mod zip file does not
                    // exist, then this will fail obviously.  
                    // TODO: one thing that "could" be done is if the archive itself does not exist
                    // to instead load some default items... but.. that is problematic.
                    // if you cant load everything it really should fail, but it should fail in
                    // a way that the app is notified and can shut down.
                    // Notification is required because our loading is threaded and pages in
                    throw new System.IO.FileNotFoundException("ArchiveIOHelper.GetStreamFromArchive() - ERROR: '" + modPath + "' could not be found.");
                }
            }
            catch (Exception ex)
            {
            	System.Diagnostics.Debug.WriteLine ("ArchiveIOHelper.GetStreamFromArchive() - ERROR: " + ex.Message);
            }
            finally
            {
                if (zip != null)
                    zip.Dispose();

                System.Diagnostics.Debug.WriteLine("completed.");
            }
            return null;
        }

        public static byte[] GetBytesFromArchive(string entryPath, string password, string zipFilePath)
        {
            ZipFile zip = null;
            try
            {
                zip = new ZipFile(zipFilePath);
                if (zip != null)
                    return GetBytesFromArchive(entryPath, password, zip);
            }
            catch (Exception ex)
            {
				System.Diagnostics.Debug.WriteLine ("ArchiveIOHelper.GetBytesFromArchive() - " + ex.Message);
            }
            finally
            {
                if (zip != null)
                    zip.Dispose();
            }
            return null;
        }

        public static byte[] GetBytesFromArchive(string entryPath, string password, ZipFile zip)
        {
            foreach (ZipEntry entry in zip.Entries)
            {
                if (entry.FileName == entryPath)
                {
                    byte[] bBuffer = new byte[(int)(entry.UncompressedSize)];
                    CrcCalculatorStream stream = entry.OpenReader(password);
                    stream.Read(bBuffer, 0, (int)stream.Length);
                    stream.Close();

                    return bBuffer;
                }
            }
            return null;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="zipFileFullPath">Name to existing zip OR if file does not exist, it will be created.</param>
        /// <param name="sourceFiles"></param>
        /// <param name="targetPaths"></param>
        /// <param name="newEntryNames"></param>
        public static void AddFilesToMod(string modsPath, string modName, string[] sourceFiles, string[] targetPaths, string[] newEntryNames)
        {
        	if (string.IsNullOrEmpty (modName) || modName.EndsWith (".zip") == false)
        	{
        		AddFilesToModFolder (modsPath, sourceFiles, targetPaths, newEntryNames);
        	}
        	else
        	{
	            // add files to the  xmldb specified in the path
	            Ionic.Zip.ZipFile zip = null;
	        	try 
	        	{
		            string modFullPath = System.IO.Path.Combine(modsPath, modName);
	        		zip = new Ionic.Zip.ZipFile(modFullPath);
	                zip.TempFileFolder = System.IO.Path.GetTempPath();
	
	                zip.CompressionLevel = Ionic.Zlib.CompressionLevel.None;
	                // TODO: I can rename this maybe IInputOutputAPI
	                // TODO: i should iterate through all sourceFiles[] and verify "file exists" and that 
	                // they are accessible for read rather than blindly try to AddFilesToArchive.
	                // That way i could abort the entire action and perhaps return False and modify
	                // this method to return bool
	                AddFilesToArchive(zip, sourceFiles, targetPaths, newEntryNames);
	
	                zip.Save();
	                if (sourceFiles != null)
	                    System.Diagnostics.Debug.WriteLine(string.Format("AddFile() -- Successfully added {0} to archive '{1}'.", sourceFiles[0], modFullPath));
	                else
	                    System.Diagnostics.Debug.WriteLine(string.Format("AddFile() -- Successfully created {0} to archive '{1}'.", targetPaths[0] + "\\" + newEntryNames[0], modFullPath));
	
	        	}
	            catch (Exception ex)
	            {
	                // note: sometimes on a out of disk space related error, it'll say something about an error with a tmp file.
	                // This is because the original file is not wiped out and replaced until the temp file is successfully created.
	                System.Diagnostics.Debug.WriteLine(string.Format("AddFile() -- Error adding geometry to archive '{0}'.", ex.Message));
	//                System.Windows.Forms.MessageBox.Show("AddFile() - There was an error updating the mod database.  Ensure you have enough disk space on the drive where the mod database is located.", "Error updating mod database.");
	                // TODO: need to trap the exception, unroll any changes and then throw a new exception for the IWorkItemResult.Exception()
	            }
	            finally
	            {
	                if (zip != null) zip.Dispose();
	            }
        	}
        }
                
        private static void AddFilesToModFolder (string modsPath, string[] sourceFiles, string[] targetPaths, string[] newFileNames)
        {
            if (targetPaths == null) throw new ArgumentException();
            // both sourceFiles and newFileNames cannot be null. 
            if (sourceFiles == null && newFileNames == null) throw new ArgumentException();
            // newFileNames can be null but not if sourceFiles is null! thats because with blank newFileNames
            // we by default use the filenames of the sourcefile after stripping the path
            if (newFileNames != null && newFileNames.Length > 0)
                if (targetPaths.Length != newFileNames.Length) throw new ArgumentException();
            // sourceFiles can be null and then all targetpaths/files will be created as new 0 byte entries
            // however if sourceFiles is NOT null, then it must be same length as targetpaths and newFileNames
            if (sourceFiles != null)
                if (sourceFiles.Length > 0)
                    if (sourceFiles.Length != targetPaths.Length) throw new ArgumentException();
        
            if (sourceFiles == null || sourceFiles.Length == 0)
            {
                for (int i = 0; i < targetPaths.Length; i++)
                {
                	string filename = System.IO.Path.Combine (targetPaths[i], newFileNames[i]);
                	
                	// Replace/Overwrite existing entry with new file
                	if (System.IO.File.Exists (filename))
                		System.IO.File.Delete (filename);
                	
                	System.IO.File.Copy (sourceFiles[i], filename);
                }
            }
            else
            {
                for (int i = 0; i < sourceFiles.Length; i++)
                {
                    // TODO: hack to skip files that dont exist on drive since usually these are missing textures
                    // when importing mesh
                    if (File.Exists(sourceFiles[i]) == false) continue;

                    string fileName = System.IO.Path.Combine(targetPaths[i], System.IO.Path.GetFileName(sourceFiles[i]));
                    if (newFileNames != null && newFileNames.Length == targetPaths.Length)
                        fileName = System.IO.Path.Combine(targetPaths[i], newFileNames[i]);

                    fileName = System.IO.Path.Combine(modsPath, fileName);
                    string sourceFileCompare = sourceFiles[i].ToLower();
                    string targetFileCompare = fileName.ToLower();
                    if (File.Exists(fileName)) // Replace/Overwrite existing entry with new file
                    {
                        if (sourceFileCompare.Equals(targetFileCompare) == false)
                            File.Delete(fileName);
                    }

                    // if the source file and filename are same, then the file is already where we want it  
                    // and no need to copy (access violation would occur anyways)
                    if (sourceFileCompare != targetFileCompare)
                    {
                        if (Directory.Exists(fileName) == false)
                            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                        File.Copy(sourceFiles[i], fileName);
                    }
                }
            }
            
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="zip"></param>
        /// <param name="sourceFiles">Full path to the files on disk to add</param>
        /// <param name="targetPaths">Destination path in zip without the filename (the existing filename will be used</param>
        /// <param name="newFileNames">New destination file name to be used in place of original existing filename</param>
        private static void AddFilesToArchive(ZipFile zip, string[] sourceFiles, string[] targetPaths, string[] newFileNames)
        {
            if (targetPaths == null) throw new ArgumentException();
            // both sourceFiles and newFileNames cannot be null. 
            if (sourceFiles == null && newFileNames == null) throw new ArgumentException();
            // newFileNames can be null but not if sourceFiles is null! thats because with blank newFileNames
            // we by default use the filenames of the sourcefile after stripping the path
            if (newFileNames != null && newFileNames.Length > 0)
                if (targetPaths.Length != newFileNames.Length) throw new ArgumentException();
            // sourceFiles can be null and then all targetpaths/files will be created as new 0 byte entries
            // however if sourceFiles is NOT null, then it must be same length as targetpaths and newFileNames
            if (sourceFiles != null)
                if (sourceFiles.Length > 0)
                    if (sourceFiles.Length != targetPaths.Length) throw new ArgumentException();


            // NOTE: No try catch here.  The caller will handle any exceptions
            DateTime timeStamp = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 0, 0);

            if (sourceFiles == null || sourceFiles.Length == 0)
            {
                for (int i = 0; i < targetPaths.Length; i++)
                {
                	string entryName = System.IO.Path.Combine (targetPaths[i], newFileNames[i]);

                    entryName = TidyZipEntryName(entryName);

                    ZipEntry e;
                    if (zip.ContainsEntry(entryName)) // Replace/Overwrite existing entry with new file
                    {
                        e = zip.UpdateEntry(entryName, new byte[]{}); // 0 byte entry is equiv to erasing existing contents
                    }
                    else
                    {
                        e = zip.AddEntry(entryName, new byte[]{});
                    }
                    e.LastModified = timeStamp;
                }
            }
            else
            {
                for (int i = 0; i < sourceFiles.Length; i++)
                {
                	// TODO: hack to skip files that dont exist on drive since usually these are missing textures
                	// when importing mesh
                	if (File.Exists (sourceFiles[i]) == false) continue;
                	
                	string entryName = System.IO.Path.Combine (targetPaths[i], System.IO.Path.GetFileName(sourceFiles[i]));
                    if (newFileNames != null && newFileNames.Length == targetPaths.Length)
                    	entryName = System.IO.Path.Combine (targetPaths[i], newFileNames[i]);

                    // must remove starting slash especially
                    entryName = TidyZipEntryName(entryName);
                    ZipEntry e;
                    if (zip.ContainsEntry(entryName)) // Replace/Overwrite existing entry with new file
                    {

                        //string content = System.IO.File.ReadAllText(sourceFiles[i]);
                        byte[] contents = System.IO.File.ReadAllBytes(sourceFiles[i]);
                        e = zip.UpdateEntry(entryName, contents);
                    }
                    else
                    {

                        e = zip.AddFile(sourceFiles[i], targetPaths[i]);
                        if (newFileNames != null && newFileNames.Length == targetPaths.Length)
                        {
                            // yes, changing the filename here before zip.Save() works fine for changing the name and still
                            // being able to find the underlying source file to add to the zip
                            e.FileName = entryName;
                        }
                    }
                    e.LastModified = timeStamp;
                }
            }
                
        }

        public static void RenameZipEntry(ZipFile zip, string entryPath, string newEntryName)
        {
            // if the entryName is a directory, we must recurse and rename
            // all child directories and entries as well 

            ZipEntry e;
            if (zip.ContainsEntry(entryPath)) // Replace/Overwrite existing entry with new file
            {
                ICollection<ZipEntry> entries = KeyCommon.IO.ArchiveIOHelper.SelectEntriesRecursively(zip, entryPath);

               // NOTE: Doesn't matter if entries returns a single file or a folder and all recursed entries
                System.Diagnostics.Debug.Assert(entries.Count >= 1);
                foreach (ZipEntry entry in entries)
                {
                    System.Diagnostics.Debug.WriteLine("old entry name = " + entry.FileName);
                    entry.FileName.Replace(entryPath, newEntryName);
                    System.Diagnostics.Debug.WriteLine("new entry name = " + entry.FileName);
                }

                // NOTE: SelectEntriesRecursively DOES include the starting folder so no need to rename it seperately
            }
        }

        public static ICollection<ZipEntry> SelectEntriesRecursively(ZipFile zip, string directoryPathInArchive)
        {
            Ionic.FileSelector selector = new Ionic.FileSelector("", false);

            string startPath = (directoryPathInArchive == null) ? null : directoryPathInArchive.Replace("/", @"\");
            if (startPath != null)
            {
                while (startPath.EndsWith(@"\"))
                {
                    startPath = startPath.Substring(0, startPath.Length - 1);
                }
            }

            return zip.SelectEntries(startPath + "\\*");
        }

        /// <summary>
        /// Useful for finding all entires off a particular branch for 
        /// deleting entire "folders"
        /// </summary>
        /// <param name="zip"></param>
        /// <param name="directoryPathInArchive"></param>
        /// <returns></returns>
        public static string[] SelectEntryNamesRecursively(ZipFile zip, string directoryPathInArchive)
        {

            ICollection<ZipEntry> entries = SelectEntriesRecursively(zip, directoryPathInArchive);
            
            string[] results = new string[entries.Count];
            int i = 0;
            foreach (ZipEntry entry in entries)
            {
                results[i] = entry.FileName;
                i++;
            }

            return results;

            //foreach (ZipEntry entry in zip.EntriesSorted)
            //{
            //    entries.Add(entry.FileName);
            //}
        }

        public static string[] SelectDirectories(ZipFile zip, string directoryPathInArchive)
        {
            Ionic.FileSelector selector = new Ionic.FileSelector("type = D", false);
            List<string> results = new List<string>();

            // here we only want to trim the leading and trailing slashes, but we do NOT
            // want to replace backslashes with forward slashes.
            string startPath = directoryPathInArchive;
            char[] trimChars = new char[] { '\\', '/' };
            startPath = startPath.Trim(trimChars);
            

            // note: i never could get DotNetZip.Selector to work so here
            // is my own directory selector implementation where I iterate 
            // thru all and compare dir names.
            foreach (ZipEntry entry in zip.EntriesSorted)
            {
                string entryDir = Path.GetDirectoryName(entry.FileName);
                if (string.IsNullOrEmpty(entryDir)) continue;
                // we only want to add first level child sub directories, and not the starting path dir itself either
                //if (entryDir != startPath && startPath == Path.GetDirectoryName(entryDir))
                if (startPath == Path.GetDirectoryName(entryDir))
                {
                    bool alreadyExits = false;
                    for (int i = 0; i < results.Count; i++)
                    {
                        if (results[i] == entryDir)
                        {
                            alreadyExits = true;
                            break;
                        }
                    }
                    if (!alreadyExits)
                        results.Add(entryDir);
                }
            }

            return results.ToArray();
        }

        public static string[] SelectDirectories(string zipFilePath, string directoryPathInArchive)
        {
            if (string.IsNullOrEmpty(zipFilePath)) return null;
            if (string.IsNullOrEmpty(directoryPathInArchive)) directoryPathInArchive = "";
            ZipFile zip = null;
            try
            {
                zip = new ZipFile(zipFilePath);
                if (zip == null) throw new Exception("Failed to open zip.");
                List<string> results = new List<string>();

                return SelectDirectories(zip, directoryPathInArchive);
            }
            catch 
            {
                return null;
            }
            finally
            {
                if (zip != null) zip.Dispose();
            }
        }


        public static string[] SelectFiles(ZipFile zip, string directoryPathInArchive)
        {
            string[] results;
            ICollection<ZipEntry> entries = zip.SelectEntries("*.*", directoryPathInArchive);

            results = new string[entries.Count];
            int i = 0;
            foreach (ZipEntry entry in entries)
            {
                results[i] = entry.FileName;
                i++;
            }

            return results;
        }

        public static string[] SelectFiles(string zipFilePath, string directoryPathInArchive)
        {
            ZipFile zip = null;
            try 
            {
                zip = new ZipFile(zipFilePath);
                if (zip == null) throw new Exception("Failed to open zip.");

                return SelectFiles(zip, directoryPathInArchive);
            }
            catch 
            {
                return null;
            }
            finally
            {
                if (zip != null) zip.Dispose();
            }
        }

        // // must remove starting slash especially
        public static string TidyZipEntryName(string entryPath)
        {
            char[] charsToTrim = { '\'', '/' };
            string result = entryPath.Replace("\\", "/");
            return result.Trim(charsToTrim);
        }

        public static bool EntryExists(string zipFilePath, string entryPath)
        {
            ZipFile zip = new ZipFile(zipFilePath);
            if (zip == null) throw new Exception("Failed to open zip.");
            string tempName = entryPath.Replace("\\", "/");
            char[] charsToTrim = { '\'', '/' };
            tempName = tempName.Trim(charsToTrim);
            return zip.ContainsEntry(tempName);
        }

        public static string CleanZipDirPath(string path)
        {
            string result = (path == null) ? null : path.Replace("\\", "/");

            if (string.IsNullOrEmpty(result) == false)
            {
                char[] trimChars = new char[] {'\\', '/' };
                result = result.TrimStart(trimChars);
            }

            return result;
        }
    }
}
