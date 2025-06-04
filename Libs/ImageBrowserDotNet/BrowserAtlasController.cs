using System;
using System.IO;
using System.Drawing;
using Ionic.Zip;
using AtlasTextureTools;


namespace ImageBrowserDotnet
{
    // TODO: maybe KeyEdit derives this class and uses it for BrowserController?
    public class BrowserAtlasController : BrowserController
    {
        private string mAtlasResource;

        public BrowserAtlasController(string atlasResource)
            : base(null, atlasResource)
        {
            if (string.IsNullOrEmpty(atlasResource)) throw new ArgumentNullException();
            mAtlasResource = atlasResource;
            mRecurseSubFolders = false;
        }

        // this "folder" is actually the atlas texture path which can be a resource descriptor
        // to a archived texture, but maybe it should actually contain an TextureAtlas.xml
        // that represents the saved node since we need not the atlas texture file
        // we need an actual node instance or the capacity to create the node and 
        // all it's sub-texture offset settings
        protected override void AddFolderIntern(string atlasRecordsFilePath)
        {
            // this is the only routine we need to override and implement
            // to handle atlas images instead of regular seperate disk file images
            if (m_CancelScanning) mThread.Abort();

            Bitmap atlasImage = null;
            AtlasRecord[] records = null;
            KeyCommon.IO.ResourceDescriptor descriptor = new KeyCommon.IO.ResourceDescriptor(atlasRecordsFilePath);

            if (descriptor.IsArchivedResource)
            {
                // if the atlas file is within a archive mod, extract it and open it
                ZipFile zip = new ZipFile(descriptor.ModName);
                if (zip != null)
                {
                    // select our .tai entry
                    string entryName = KeyCommon.IO.ArchiveIOHelper.TidyZipEntryName (descriptor.EntryName);
                    if (zip.ContainsEntry(entryName))
                    {
                        string file = KeyCommon.IO.ArchiveIOHelper.GetTextFromArchive(entryName, zip);

                        //        System.IO.MemoryStream mem = new System.IO.MemoryStream();
                        //        entry.Extract(mem);
                        //        mem.Seek(0, System.IO.SeekOrigin.Begin);

                        //        catch
                        //        {
                        //            // do nothing
                        //        }
                        //        finally
                        //        {
                        //            if (mem != null)
                        //            {
                        //                mem.Close();
                        //                mem.Dispose();
                        //            }
                        //        }
                    }
                    

                }

                // we should always open/close the zip just when we need it.  Do not
                // keep it occupied or else our Worker threads will never be able to save changes
                // to this zip
                zip.Dispose();
                zip = null;
            }
            else
            {
                // TODO: NOTE: The problem with DevIL is there is no way to load directly
                //       a .DDS from stream.  So we'd have to save to disk first for a temp
                //       hack solution.
                // if the atlas file is on disk, simply open it and create records
                records = AtlasParser.Parse(descriptor.EntryName);
                // note: DevIL required to handle DDS atlases
                if (IsValidImageFormat(records[0].AtlasFileName) == false) return;

                //DevIL.ImageImporter importer = new DevIL.ImageImporter ();
                //DevIL.Image img = importer.LoadImage (DevIL.ImageType.Dds, records[0].AtlasFileName);
                //img.GetImageInfo (); // ensure Bind is called 
                //byte[] data = DevIL.Unmanaged.IL.GetImageData();

                //System.ComponentModel.TypeConverter tc = System.ComponentModel.TypeDescriptor.GetConverter(typeof(Bitmap));

                // http://www.mastropaolo.com/files/DevILDotNetMainSource.htm   <-- might help
                //Bitmap bitmap1 = (Bitmap)tc.ConvertFrom(data);

                //atlasImage = Bitmap.FromHbitmap (DevIL.Unmanaged.IL.GetData ());
                // this atlas was constructed from nvidia atlas maker tool which saves as .dds
                //atlasImage =  DevIL.DevIL.LoadBitmap(records[0].AtlasFileName);
                //atlasImage = (Bitmap)Bitmap.FromFile(records[0].AtlasFileName);
                // TODO: must mBrowser.BindToZipArchive() when trying to select
                // a new category in the browser control toolbar

                // squishlib
                // crunch - http://code.google.com/p/crunch/
                FileStream fs = null;
                try
                {
                    Gibbed.Squish.DDSFile ddsFile = new Gibbed.Squish.DDSFile();
                    fs = new FileStream(records[0].AtlasFileName, FileMode.OpenOrCreate, FileAccess.Read);

                    ddsFile.Deserialize(fs);

                    // TODO: this is slow as hell.. need to try the code used by Open Paint Dot Net 
                    atlasImage = (Bitmap)ddsFile.Image();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
                finally
                {
                    if (fs != null)
                        fs.Close();
                }
                

            }


            // instant quadtree insertion?
            // http://lspiroengine.com/?p=530

            if (records == null || records.Length == 0 || atlasImage == null) return;

            // create the tiles for the image browser
            System.Drawing.Imaging.PixelFormat format = atlasImage.PixelFormat;
            for (int i = 0; i < records.Length; i++)
            {
                 //  get the sub image defined by the bounds
                Image img = atlasImage.Clone(records[i].GetRectangle(atlasImage), format);
                string name = string.Format(records[i].FileName + ".{0}.TA_INDEX", i);
                InvokeAdd(img, null, name);
            }

            // TODO: Below is from ZipBrowserController and just used as reference for writing
            // this Atlas Browser.. but we have yet to work with compressed atlases...within our mod dbs
            //foreach (AtlasRecord entry in records)
            //{
            //    if (m_CancelScanning) mThread.Abort(); // another opportunity to cancel browsing
            //    if (IsFileTypeFiltered(entry.FileName)) continue;

            //    Image img = null;

            //    if (IsValidImageFormat(entry.FileName))
            //    {
            //        System.IO.MemoryStream mem = new System.IO.MemoryStream();
            //        entry.Extract(mem);
            //        mem.Seek(0, System.IO.SeekOrigin.Begin);

            //        try
            //        {
            //            // .dds  <-- use DevIL 
            //            // .tga  <-- use the TargaImage loader
            //            switch (Path.GetExtension(entry.FileName).ToUpper())
            //            {
            //                case ".DDS":
            //                    // use a temporary image so that we can still use them in our editor
            //                    img = new Bitmap(@"E:\dev\c#\KeystoneGameBlocks\Data\editor\Texture.bmp");
            //                    break;
            //                case ".TGA":
            //                    img = Paloma.TargaImage.LoadTargaImage(mem);
            //                    break;
            //                default:
            //                    img = Image.FromStream(mem);
            //                    break;
            //            }
            //        }
            //        catch
            //        {
            //            // do nothing
            //        }
            //        finally
            //        {
            //            if (mem != null)
            //            {
            //                mem.Close();
            //                mem.Dispose();
            //            }
            //        }
            //    }
            //    else
            //    {
            //        InvokeNonImageFound(entry.FileName);
            //    }

            //    if (img != null)
            //    {
            //        InvokeAdd(img, entry.FileName);

            //        //  img.Dispose(); // cannot dispose image while we are using it
            //    }

            //}

        }
    }
}
