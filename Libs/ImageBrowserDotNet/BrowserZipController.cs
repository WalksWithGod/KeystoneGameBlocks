using System;
using System.Drawing;
using System.Threading;
using System.IO;
using Ionic.Zip;

namespace ImageBrowserDotnet
{
    public class BrowserZipController : BrowserController
    {

        public BrowserZipController(string modsPath, string modName)
            : base(modsPath, modName)
        {
            if (string.IsNullOrEmpty(modsPath)) throw new ArgumentNullException();
            if (string.IsNullOrEmpty(modName)) throw new ArgumentNullException();
            mRecurseSubFolders = true;
        }


        protected override void AddFolderIntern(string folderPath)
        {
            // this is the only routine we need to override and implement
            // to handle zips instead of regular disk files
            if (m_CancelScanning) mThread.Abort();


            // we should always open/close the zip just when we need it.  Do not
            // keep it occupied or else our Worker threads will never be able to save changes
            // to this zip
            string fullPath = System.IO.Path.Combine (mModsPath, mModName);
            ZipFile zip = new ZipFile(fullPath);
            if (zip != null)
            {

                System.Collections.Generic.ICollection<ZipEntry> entries = zip.SelectEntries("*.*", KeyCommon.IO.ArchiveIOHelper.CleanZipDirPath(folderPath));
                foreach (ZipEntry entry in entries)
                {
                    if (m_CancelScanning) mThread.Abort(); // another opportunity to cancel browsing
                    if (IsFileTypeFiltered(entry.FileName)) continue;

                    Image img = null;

                    if (IsValidImageFormat(entry.FileName))
                    {
                        System.IO.MemoryStream mem = new System.IO.MemoryStream();
                        entry.Extract(mem);
                        mem.Seek(0, System.IO.SeekOrigin.Begin);

                        try
                        {
                            // .dds  <-- use DevIL 
                            // .tga  <-- use the TargaImage loader
                            switch (Path.GetExtension(entry.FileName).ToUpper())
                            {
                                case ".DDS":
                                    // use a temporary image so that we can still use them in our editor
                                    img = new Bitmap(@"E:\dev\c#\KeystoneGameBlocks\Data\editor\Texture.bmp");
                                    break;
                                case ".TGA":
                                    img = Paloma.TargaImage.LoadTargaImage(mem);
                                    break;
                                default:
                                    img = Image.FromStream(mem);
                                    break;
                            }
                        }
                        catch
                        {
                            // do nothing
                        }
                        finally
                        {
                            if (mem != null)
                            {
                                mem.Close();
                                mem.Dispose();
                            }
                        }
                    }
                    else
                    {
                        InvokeNonImageFound(entry.FileName, entry.FileName); 
                    }

                    if (img != null)
                    {
                        InvokeAdd(img, entry.FileName, entry.FileName);

                        //  img.Dispose(); // cannot dispose image while we are using it
                    }

                }
                zip.Dispose();
                zip = null;
            }
        }
    }

}
