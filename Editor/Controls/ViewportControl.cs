using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Keystone.Cameras;

namespace KeyEdit.Controls
{
    public partial class ViewportControl : UserControl
    {
        // TODO: i dont think i need this.
        public enum FormUnloadMode : int
        {
            None,
            FormControlMenu,
            Code,
            OperatingSystemShutDown
        }

        // UserControl already has a Name field
        //protected string _name;
        //protected Viewport _viewport;
        protected Viewport.ProjectionType m_projectionType;
        protected Viewport.ViewType m_viewType;
        protected bool mEnabled = false;


        public ViewportControl()
            : base()
        {
            InitializeComponent();
            AllowDrop = true; // this property not exposed in the design time property grid but can be set here.

            m_viewType = Viewport.ViewType.Free;
            m_projectionType = Viewport.ProjectionType.Perspective;
        }

        public ViewportControl(string name)
            : this()
        {

            this.Name = name;
        }
        
       public IntPtr RenderHandle 
       {
           get { return this.pictureBox.Handle; }
       }

        // UserControl already has a Name field
       //public string Name { get { return _name; } set { _name = value; } }

       public virtual void ReadSettings(Settings.Initialization iniFile)
       {
           // color
           // cursor
       }

       public virtual void WriteSettings(Settings.Initialization iniFile)
       {

       }

       public virtual void ShowDocument()
       { 
       }

       public virtual void HideDocument()
       { 
       }
       
       DevComponents.DotNetBar.RibbonBar mContextualizedToolbar;

       public void SetToolbar (DevComponents.DotNetBar.RibbonBar toolbar)
       {
	       	// if the toolbar is not the default, we hide default
	       	if (toolbar != null)
	       	{	       		
	       		ribbonBar.Hide();
		       	
		       	// if this is not same contextualized toolbar as last time, remove the previous
		       	if (mContextualizedToolbar != null && toolbar != mContextualizedToolbar)
		       	{
		       		this.Controls.Remove (mContextualizedToolbar);
		       		mContextualizedToolbar = null;
		       	}
		       	
   				// and we add the new one to the form as a ContextualizedToolbar;
				mContextualizedToolbar = toolbar;
				this.Controls.Add(mContextualizedToolbar); 
	       	}
       		else
       		{
 //      			ShowToolbar = true;
       				// if this is not same contextualized toolbar as last time, remove the previous
		       	if (mContextualizedToolbar != null)
		       	{
		       		this.Controls.Remove (mContextualizedToolbar);
		       		mContextualizedToolbar = null;
		       	}
       		}
		

       }
       
       public bool ShowToolbar 
       {
           get { return this.ribbonBar.Visible; }
           set 
           {
                this.ribbonBar.Top = 0;
               this.ribbonBar.Visible = value;
               this.pictureBox.BringToFront();
               this.pictureBox.Dock = DockStyle.Fill;
               OnResize(null);
           }
       }

       public new bool Enabled
       {
           get { return mEnabled; }
           set
           {
               mEnabled = value;
               if (mContext != null)
               {
                   mContext.Enabled = mEnabled;
               }
           }
       }
       
       protected RenderingContext mContext;
       
       public RenderingContext Context 
       {
       		get { return mContext;}
       		set
            {
                mContext = value;
                if (mContext != null)
                {
                    mContext.Enabled = mEnabled;

                    mContext.ProjectionType = m_projectionType;
                    mContext.ViewType = m_viewType;
                }
            }
       }
       
        public Keystone.Cameras.Viewport Viewport 
        {
        	get { if (mContext == null) return null;  return mContext.Viewport  ;}
        }

        public Viewport.ProjectionType Projection
        {
            get { return m_projectionType; }
            set
            {
                m_projectionType = value;
                if (mContext != null)
                {
                    mContext.ProjectionType = value;
                }

                Text = m_viewType.ToString() + " - "+  m_projectionType.ToString();
             //   buttonItemProjection.Text = m_projectionType.ToString();
            }
        }

        public Viewport.ViewType View
        {
            get { return m_viewType; }
            set
            {
                m_viewType = value;
                if (mContext != null)
                {
                    mContext.ViewType = value;
                }
                Text = m_viewType.ToString() + " - " + m_projectionType.ToString();
         //       buttonItemView.Text = m_viewType.ToString();
            }
        }

        // prevent arrow keys from acting like Tab index cycling of the viewport "documents" which is what Up/Down seem to do
        // TODO: does this work with localization?  ugh.  need to test
        public override bool PreProcessMessage(ref Message msg)
        {
            if (msg.Msg == (int)Win32Messages.WM_KEYDOWN && 
                (msg.WParam.ToInt32() == (int)VirtualKeyCodes.VK_UP 
                 || msg.WParam.ToInt32() == (int)VirtualKeyCodes.VK_DOWN 
                 || msg.WParam.ToInt32() == (int)VirtualKeyCodes.VK_LEFT 
                 || msg.WParam.ToInt32() == (int)VirtualKeyCodes.VK_RIGHT))
                return false;
            
            base.PreProcessMessage(ref msg);
            return true;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            this.Focus();
        }

        protected void OnPictureBoxMouseEnter(object sender, EventArgs e)
        {
            this.Focus();
        }

        public void ResizeViewport()
        {
        	bool designMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);
        	if (designMode) return;
            	
        	if (Viewport != null)
            {           	
                System.Drawing.Point p = PointToScreen(new System.Drawing.Point(pictureBox.Left, pictureBox.Top));
                Viewport.Resize(p.X, p.Y);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            // NOTE: As long as the ViewportControl's .Dock property is NOT set to FILL and instead
            // uses the anchors to automatically control resize, no manual size calc code is needed.
            ResizeViewport();
        }

        protected override void OnDragEnter(DragEventArgs drgevent)
        {
            base.OnDragEnter(drgevent);
            //System.Diagnostics.Debug.WriteLine("FormViewport.OnDragEnter() " + drgevent.ToString());

            // if the data is valid for drop, set the effect to copy
            drgevent.Effect = DragDropEffects.Copy;
        }

        protected override void OnDragOver(DragEventArgs drgevent)
        {
            base.OnDragOver(drgevent);
            //System.Diagnostics.Debug.WriteLine("FormViewport.OnDragOver() " + drgevent.ToString());
        }

        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            base.OnDragDrop(drgevent);
            //System.Diagnostics.Debug.WriteLine("FormViewport.OnDragDrop() " + drgevent.ToString());
            //drgevent.Data.GetData 
        }

        protected override void OnDragLeave(EventArgs e)
        {
            base.OnDragLeave(e);
            //System.Diagnostics.Debug.WriteLine("FormViewport.OnDragLeave() " + e.ToString());
           
        }

    }
}
