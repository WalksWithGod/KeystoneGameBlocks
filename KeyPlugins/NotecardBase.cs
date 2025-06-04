using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace KeyPlugins
{
	// IDE designer does not like for this class to be abstract.
    public partial class NotecardBase : UserControl
    {

        protected  KeyCommon.IO.ResourceDescriptor mDescriptor;
    	        
    	protected int BORDER_WIDTH = 3;
        protected int TOP_BORDER_WIDTH = 5;
        protected string mTargetID = "";
        protected string mParentID = "";

        public event EventHandler ControlClosed;  
        //public event EventHandler ControlCreated;

        public delegate void ResourceEventHandler(string resourceType, string previousDescriptor, string parentNodeID);
        protected ResourceEventHandler mBrowseForResourceHandler;
        protected ResourceEventHandler mEditResourceHandler;
        
        public NotecardBase() : base()
        {
            InitializeComponent();
        }

        public NotecardBase(string resourceDescriptor,  string childID, string parentID, ResourceEventHandler browseForResourceHandler, ResourceEventHandler editResourceHandler)
            : this()
        {
        	
            // note: resourceDescriptor can be null
            if (browseForResourceHandler == null) throw new ArgumentNullException();

            if (string.IsNullOrEmpty (resourceDescriptor) == false)
                mDescriptor = new KeyCommon.IO.ResourceDescriptor (resourceDescriptor);
            
            mTargetID = childID;
            mParentID = parentID;
            
            mBrowseForResourceHandler = browseForResourceHandler;
            mEditResourceHandler = editResourceHandler;
        }

        protected void buttonClose_Click(object sender, EventArgs e)
        {
            if (ControlClosed != null)
                ControlClosed.Invoke(this, e); // this since we want the sender to have ref to this control so it can dispose it
        }

        void ButtonBrowseClick(object sender, System.EventArgs e)
        {
        	switch (this.GetType().Name)
        	{
        		case "MaterialAttributeCard":

        			mBrowseForResourceHandler.Invoke ("Material", mTargetID,  mParentID);

        			break;
        		case "TextureEditCard":
        			                  
                     Control control =  ((Control)sender).Parent;
                     TextureEditCard texCard = (TextureEditCard)control;

                     // NOTE: LayerID must be used for Texture resources
                     // because recall that a texture resource sits under a Layer object instance
                     // and not directly under Appearance node.
		            mBrowseForResourceHandler("Texture", mDescriptor.ToString(), texCard.LayerID);
		            break;
	           case "ShaderEditCard":
		            // TODO: for shaders browse is obsolete?  we know that now changing shaders
		            // means we change ResourceID in Appearance and that Appearance must then
		            // change the shaders.  We no longer allow Shaders to be externally added
		            // or removed as nodes.  These are private nodes managed only by Appearance nodes.
		            mBrowseForResourceHandler("Shader", mDescriptor.ToString(), mParentID);
		            break;
	           default:
		            break;
        	}
        }
       
        public virtual string TargetID
        {
            get { return mTargetID; }
        }

        public virtual string ParentID
        {
            get { return mParentID; }
        }

        public override string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
                groupPanel.Text = value;
            }
        }
    }
}
