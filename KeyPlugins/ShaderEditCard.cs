using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace KeyPlugins
{
    public partial class ShaderEditCard : NotecardPropGrid 
    {
        public string ShaderID;
        public string AppearanceID;

        public string ResourceDescriptor;


        public ShaderEditCard (string nodeID) : base (nodeID)
        {
            InitializeComponent();
        }

        // note: because shaders can be dynamically compiled with defines to create deferred vs non deferred versions
        // a resourceDescriptor is NOT the same thing as the shaderID
        public ShaderEditCard(string resourceDescriptor, string shaderID, string appearanceID, ResourceEventHandler browseForResourceHandler, ResourceEventHandler editResourceHandler)
            : base(resourceDescriptor)
        {
            InitializeComponent();

            this.mEditResourceHandler = editResourceHandler;
            this.mBrowseForResourceHandler = browseForResourceHandler;

            ShaderID = shaderID;
            ResourceDescriptor = resourceDescriptor;
            AppearanceID = appearanceID;

            if (string.IsNullOrEmpty(ResourceDescriptor) == false)
            {
                mDescriptor = new KeyCommon.IO.ResourceDescriptor(ResourceDescriptor);
                UpdateLabels();
            }
        }


        private void UpdateLabels()
        {

            string filename = System.IO.Path.GetFileName(mDescriptor.EntryName);
            this.Text = "Shader Parameters"; // +filename;


            labelMod.Show();
            labelEntry.Show();
            labelMod.Text = "Mod Name: " + mDescriptor.ModName;
            labelEntry.Text = "Mod Entry Name: " + mDescriptor.EntryName;    
 
        }
         

        // obsolete - shaders must be destroyed and recreated, they cannot be
        // drag and dropped onto a shader card... unless we enforce ProcShaders everywhere
        // with no regular Shaders.  In that case we could change the Shader because it's just
        // would be an underlying resource of the ProcShader
        #region drag & drop
        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            // IMPORTANT: When a texture is dropped from resource gallery to
            // our texture edit unit card, that specific unit card represents several nodes
            // It represents the AttributeGroup and as such has 
            base.OnDragDrop(drgevent);
            System.Diagnostics.Debug.WriteLine("ShaderCard Drag Drop");
            try
            {
                DragDropContext node = (DragDropContext)drgevent.Data.GetData(typeof(DragDropContext));

                // TODO: is the picture being attempted to drop valid?

                throw new Exception("Temp throw cuz ive not implemented this yet...");
                //ResourceEventArgs e = new ResourceEventArgs();
                //e.ParentID = mParentID; // this is the GroupAttribute or Appearance node
                //e.ResourceID = mDescriptor.ToString(); // the current resource
                //string newResourceID = new KeyCommon.IO.ResourceDescriptor(node.RelativeZipFilePath, node.ResourcePath).ToString();
                //e.Value = newResourceID;
                //ShaderChanged.Invoke(this, e);

                // must update this descriptor or next time we try to change the texture
                // it won't work because it'll still be pointing to the original one we deleted
                //
                // TODO: my mBrowseForResourceHandler needs to return a string...
       //         mDescriptor = new KeyCommon.IO.ResourceDescriptor(newResourceID);
                UpdateLabels();

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("TextureAttributeEdit.OnDragoDrop() - " + ex.Message);
            }
        }

        protected override void OnDragEnter(DragEventArgs drgevent)
        {
            base.OnDragEnter(drgevent);

            drgevent.Effect = drgevent.AllowedEffect;
            DragDropContext node = (DragDropContext)drgevent.Data.GetData(typeof(DragDropContext));
            // TODO: if this is not the correct node type, then
            if (node == null)
                drgevent.Effect = DragDropEffects.None;
        }

        protected override void OnDragOver(DragEventArgs drgevent)
        {
            base.OnDragOver(drgevent);
            Point mousePoint = new Point(drgevent.X, drgevent.Y);
        }

        protected override void OnDragLeave(EventArgs e)
        {
            base.OnDragLeave(e);
        }
        #endregion

    }
}
