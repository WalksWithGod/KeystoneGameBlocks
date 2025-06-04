using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace KeyPlugins
{
    public partial class TextureEditCard : NotecardBase 
    {

        public string LayerID;  // layer node which hosts scaling, texture mod settings and to which the texture is child of
        public int GroupID;          // tv geometry group index
        public string TextureType;   // diffuse, normalmap, emissive, specular, etc
        private string mModsPath;


        public event EventHandler AlphaTestChanged;
		public event EventHandler AlphaTestRefValueChanged;
		public event EventHandler AlphaTestDepthWriteChanged;
        public event EventHandler TextureChanged;
        public event EventHandler TextureDropped;
        public event EventHandler TextureScaled;
        public event EventHandler TextureRotated;
        public event EventHandler TextureTranslated;
        
        // NOTE: It is the texture node type (diffuse vs normalmap vs emissive vs specular) which
        // determines the d3d/tv3d texture layer.  The order of these texture nodes as children under
        // a groupAttribute does NOT have anything to do with the layer.  It is always the specific type.
        // Thus, a user can elect to set a texture type to use the diffuse layer to create a diffuse
        // texture node, but if there's an existing one it may be refused to be created.  Or it may be
        // accepted but which of the two diffuses gets used may be undefined.
        public TextureEditCard()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentID">For a TextureEditCard, the parentID is either the AttributeGroup or Appearance Node</param>
        /// <param name="resourceDescriptor"></param>
        public TextureEditCard(string resourceDescriptor, string childID, string parentID, string modsPath, ResourceEventHandler browseForResourceHandler, ResourceEventHandler editResourceHandler)
            : base(resourceDescriptor, childID, parentID, browseForResourceHandler, editResourceHandler)
        {
            InitializeComponent();
            if (string.IsNullOrEmpty(parentID)) throw new ArgumentNullException();
            mModsPath = modsPath;

            LayerID = childID;
            
            labelMod.Text = mDescriptor.ModName;
            labelEntry.Text = mDescriptor.EntryName;

            UpdateLabels();

            DrawTexture(mDescriptor);
            
            numScaleU.Value = 1;
            numScaleV.Value = 1;
        }

        private void UpdateLabels()
        {
            labelMod.Text = mDescriptor.ModName;
            labelEntry.Text = mDescriptor.EntryName;

            string filename = System.IO.Path.GetFileName(mDescriptor.EntryName);
            this.Text = "Texture - " + filename;
        }

        public Keystone.Types.Vector2f Scale
        {
            get { return new Keystone.Types.Vector2f((float)numScaleU.Value, (float)numScaleV.Value); }
            set 
            {
                numScaleU.Value = (decimal)value.x;
                numScaleV.Value = (decimal)value.y;
            }
        }

        public Keystone.Types.Vector2f Translation
        {
            get { return new Keystone.Types.Vector2f((float)numOffsetU.Value, (float)numOffsetV.Value); }
            set 
            {
                numOffsetU.Value = (decimal)value.x;
                numOffsetV.Value = (decimal)value.y;
            }
        }

        public float Rotation
        {
            get { return (float)numRotation.Value; }
            set 
            {
                numRotation.Value = (decimal)value;
            }
        }

        private void DrawTexture(KeyCommon.IO.ResourceDescriptor descriptor)
        {
            if (string.IsNullOrEmpty(descriptor.EntryName)) return;
            
            
            string ext = System.IO.Path.GetExtension(descriptor.EntryName).ToUpper();
            System.Drawing.Image img = null;

            string path;
            
                        
            if (descriptor.IsArchivedResource)
            {
                path = System.IO.Path.Combine(mModsPath, descriptor.ModName);
                System.IO.Stream stream = KeyCommon.IO.ArchiveIOHelper.GetStreamFromMod(descriptor.EntryName, "", path);

                if (stream != null)
                    switch (ext)
                    {
                        case ".BMP":
                        case ".PNG":
                        case ".JPG":
                        case ".GIF":

                            img = Image.FromStream(stream);
                            stream.Dispose();
                            break;
                        case ".TGA":
                            //img = TargaFromStream(stream); // obsolete.  TargaImage is superior
                            img = Paloma.TargaImage.LoadTargaImage(stream);
                            break;
                        case ".DDS":
                            return; // we know this doesn't work.
                        default:
                            // try it anyway and wait for exception
                            img = Image.FromStream(stream);
                            break;
                    }
            }
            else // load from file path (not archive stream)
            {
                path = System.IO.Path.Combine(mModsPath, descriptor.EntryName); 
                switch (ext)
                {
                    case ".TGA":
                        try
                        {
                            //img = TargaFromStream(stream); // obsolete.  TargaImage is superior
                            img = Paloma.TargaImage.LoadTargaImage(path);
                        }
                        catch (Exception ex)
                        {
                            img = null;
                        }
                        break;
                    case ".DDS":
                        return; // we know this doesn't work.
                    case ".PNG":
                        return; // ditto, honestly not sure why png is broken

                    case ".BMP":
                    case ".JPG":
                    case ".GIF":
                    default:   // for default, try it anyway and wait for exception
                        try
                        {
                            img = Image.FromFile(path);
                        }
                        catch (Exception ex)
                        {
                            img = null;
                        }
                        break;
                }
            }

            try
            {

                    
            }
            catch (Exception ex)
            { 
                // .dds can't be loaded
                // ugh, and i dont want to use TV3D... hrm...
                // xna can do it and so can devil.net but
                // the devil.net wrapper doesnt have a stream overload only disk files
                // argh.. 
                System.Diagnostics.Debug.WriteLine("TexureEditCard.DrawTexture() - .DDS not supported.");
            }

            if (img != null)
            {
            	if (picTexture.Image != null)
            		picTexture.Image.Dispose ();
            	
                picTexture.Image = img.GetThumbnailImage(picTexture.Width, picTexture.Height, null, IntPtr.Zero);
                img.Dispose();            
            }
        }

        #region obsolete - now using TargaImage.csproj
        ///// <summary>
        ///// Loads a Targa image from a stream.
        ///// </summary>
        ///// <param name="stream">The stream to load the Targa image from.</param>
        ///// <returns>An instance of the <see cref="Image"/> class.</returns>
        //private Image TargaFromStream(System.IO.Stream stream)
        //{
        //    if (stream == null)
        //        throw new ArgumentNullException("stream");

        //    var header = new byte[18];

        //    stream.Read(header, 0, header.Length);

        //    int width = header[12] + (header[13] << 8);
        //    int height = header[14] + (header[15] << 8);
        //    byte colorDepth = header[16];
        //    byte imageDescriptor = header[17];

        //    if (colorDepth != 32)
        //    {
        //        System.Diagnostics.Trace.WriteLine("Color depth should be 32");
        //        return null;
        //    }

        //    // handling color map
        //    if (header[1] != 0)
        //    {
        //        System.Diagnostics.Trace.WriteLine("Color maps are not supported");
        //        return null;
        //    }

        //    // image type
        //    if (header[2] > 3)
        //    {
        //        System.Diagnostics.Trace.WriteLine("Encoded or compressed images are not supported");
        //        return null;
        //    }

        //    // bypass image ID
        //    var imageID = new byte[header[0]];
        //    stream.Read(imageID, 0, imageID.Length);

        //    bool origoTop = ((imageDescriptor & 0x20) != 0);
        //    bool origoLeft = ((imageDescriptor & 0x10) == 0);

        //    // we don't support color maps so there is nothing to bypass

        //    var bitmap = new Bitmap(width, height);
        //    var argb = new byte[4];

        //    for (int y = 0; y < height; y++)
        //    {
        //        for (int x = 0; x < width; x++)
        //        {
        //            stream.Read(argb, 0, argb.Length);

        //            var color = Color.FromArgb(argb[3], argb[2], argb[1], argb[0]);

        //            bitmap.SetPixel(origoLeft ? x : width - 1 - x,
        //                            origoTop ? y : height - 1 - y,
        //                            color);
        //        }
        //    }

        //    return bitmap;
        //}
        #endregion

        protected override void  OnDragDrop(DragEventArgs e)
        {         
            // IMPORTANT: When a texture is dropped from the Asset Browser to
            // our texture edit unit card, that specific unit card represents several nodes
            // It represents the Layer (texture scaling,translation) and the Texture
 	         base.OnDragDrop(e);

            // Can only drop files, so check
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                KeyCommon.IO.ResourceDescriptor descriptor = new KeyCommon.IO.ResourceDescriptor(file);

                if (descriptor.IsArchivedResource)
                {
                    // the file was dragged from within this mod database (zip)
                    ResourceEventArgs args = new ResourceEventArgs();
                    args.ParentID = LayerID; // this is the Layer (eg Diffuse, NormalMap, etc)
                    args.ChildID = mDescriptor.ToString(); // the previous resource
                    args.Value = descriptor.ToString(); // the new resource
                    TextureChanged.Invoke(this, args);

                    mDescriptor = descriptor; 
                    UpdateLabels();
                    // update the drawn texture on the unit card
                    DrawTexture(descriptor);
                }
            }

             try
             {
                 //DragDropContext node = (DragDropContext)drgevent.Data.GetData(typeof(DragDropContext));

                 //// TODO: is the picture being attempted to drop valid?
                 
                 //ResourceEventArgs e = new ResourceEventArgs();
                 //e.ParentID = mParentID; // this is the GroupAttribute or Appearance node
                 //e.ChildID = LayerID; // mDescriptor.ToString(); // the current resource
                 //string newResourceID = new KeyCommon.IO.ResourceDescriptor(node.RelativeZipFilePath, node.ResourcePath).ToString();
                 //e.Value = newResourceID;
                 //TextureChanged.Invoke(this, e);
                 
                 //// must update this descriptor or next time we try to change the texture
                 //// it won't work because it'll still be pointing to the original one we deleted
               //  mDescriptor = descriptor; // new KeyCommon.IO.ResourceDescriptor(newResourceID);
                 //UpdateLabels();
                 //// update the drawn texture on the unit card
                 //DrawTexture(node.RelativeZipFilePath, node.ResourcePath);
             }
             catch (Exception ex)
             {
                 System.Diagnostics.Debug.WriteLine("TextureAttributeEdit.OnDragoDrop() - " + ex.Message);
             }
        }

         protected override void  OnDragEnter(DragEventArgs e)
        {
 	         base.OnDragEnter(e);

             if (!e.Data.GetDataPresent(DataFormats.FileDrop))
             {
                 e.Effect = DragDropEffects.None;
                 return;
             }

             // still here, Copy will do.
             e.Effect = DragDropEffects.Copy;

             //drgevent.Effect = drgevent.AllowedEffect;
             //DragDropContext node = (DragDropContext)drgevent.Data.GetData(typeof(DragDropContext));
             //// TODO: if this is not the correct node type, then
             //if (node == null)
             //    drgevent.Effect = DragDropEffects.None;
        }
                
        protected override void  OnDragOver(DragEventArgs drgevent)
        {
 	         base.OnDragOver(drgevent);
             Point mousePoint = new Point(drgevent.X, drgevent.Y);
        }

        protected override void  OnDragLeave(EventArgs e)
        {
 	         base.OnDragLeave(e);
        }

        private void numScale_ValueChanged(object sender, EventArgs e)
        {
            if (TextureScaled != null)
            {
                ResourceEventArgs targs = new ResourceEventArgs();
                targs.ParentID = LayerID;
                targs.Value = new Keystone.Types.Vector2f((float)numScaleU.Value, (float)numScaleV.Value);
                TextureScaled.Invoke(sender, targs);
            }
        }

        private void numOffset_ValueChanged(object sender, EventArgs e)
        {
            if (TextureTranslated != null)
            {
                ResourceEventArgs targs = new ResourceEventArgs();
                targs.ParentID = LayerID;
                targs.Value = new Keystone.Types.Vector2f((float)numOffsetU.Value, (float)numOffsetV.Value);
                TextureTranslated.Invoke(sender, targs);
            }
        }

        private void numRotation_ValueChanged(object sender, EventArgs e)
        {
            if (TextureRotated != null)
            {
                ResourceEventArgs targs = new ResourceEventArgs();
                targs.ParentID = LayerID;
                targs.Value = (float)numRotation.Value;
                TextureRotated.Invoke(sender, targs);
            }
        }

        private void buttonConvert_Click(object sender, EventArgs e)
        {

        }

        private void buttonExport_Click(object sender, EventArgs e)
        {

        }

        
        void CbAlphaTestCheckedChanged(object sender, EventArgs e)
        {
        	if (AlphaTestChanged != null)
        	{
        		ResourceEventArgs targs = new ResourceEventArgs();
                targs.ParentID = LayerID;
                targs.Value = cbAlphaTest.Checked;
                AlphaTestChanged.Invoke(sender, targs);
        	}
        }
        
        void GroupPanelClick(object sender, EventArgs e)
        {
        	
        }
        
        void ButtonBrowseClick(object sender, EventArgs e)
        {
        	
        }
    }

}
