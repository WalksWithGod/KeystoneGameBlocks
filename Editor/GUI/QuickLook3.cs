//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Diagnostics;
//using System.Drawing;
//using System.IO;
//using System.Runtime.InteropServices;
//using System.Text;
//using System.Windows.Forms;

//using Antlr4.StringTemplate;
//using Antlr4.StringTemplate.Compiler;
//using Awesomium.Core;
//using Awesomium.Windows.Forms;
//using KeyEdit.Properties;
//using KeyPlugins;

//namespace KeyEdit.GUI
//{
//	/// <summary>
//	/// QuickLook implementation that uses Awesomium and Antlr4 StringTemplate library.
//	/// </summary>
//	/// <remarks>
//	/// TODO: "E:\dev\c#\KeystoneGameBlocks\Design\i like the planet icons that differentiate whats in orbit___neptunes_pride_game_shot1.jpg"
//	/// the above image is a great example I think of how HTML layout can produce a nice, pleasing, efficient design.
//	/// Also the more that I deal with our plugins and WinForms gui, the more I hate the idea of even using Windows Forms based controls
//	/// for any of the GUI.  It's all just too slow and cpu intensive whereas Awesomium just feels responsive and lightweight and clean and
//	/// it almost encourages a simple design layout.  
//	/// </remarks>
//	public partial class QuickLook3 : System.Windows.Forms.Panel, IResourceInterceptor, Awesomium.Core.INavigationInterceptor
//	{
//		protected KeyPlugins.IPluginHost  mHost;
//        protected string mTargetEntityID;
        
//        #region WebControl Fields
//        private WebView mWebView;
//        private ImageSurface mViewSurface;
//        private WebSession mSession;
//        private BindingSource mBindingSource;
//        #endregion
        
//		public QuickLook3() 
//		{
//            InitializeComponent();
           	         
          
//            this.DoubleBuffered = true; // prevents flicker
//    		this.SetStyle(ControlStyles.UserPaint, true);
//    		this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
    		
//    		this.SetStyle(ControlStyles.Selectable, true);
//        	this.TabStop = true;
        
    		
//    		// awesomium does not provide combobox drop down so we create our own
//    		lbSelect = new ListBox();
//    		this.Controls.Add(lbSelect);
//            lbSelect.BringToFront();
//            lbSelect.Visible=false;
//            lbSelect.MouseClick += lbSelect_MouseClick;
//            lbSelect.LostFocus += lbSelect_LostFocus;
               
//		}

        
//        public QuickLook3(KeyPlugins.IPluginHost scriptingHost) : this()
//        {
//            if (scriptingHost == null) throw new ArgumentNullException();
//            mHost = scriptingHost;

//            // Initialize the core and get a WebSession.
//            mSession = InitializeCoreAndSession();

//            // Initialize a new view.
//            mWebView = WebCore.CreateWebView(this.ClientSize.Width, this.ClientSize.Height, mSession);
          
//            InitializeView( mWebView);
            
           
//        }
        
//        public virtual void Notify(string entityID, Settings.PropertySpec[] customProperties)
//        {
//            // often when we Select an Entity, that entity needs to be read from the database because
//            // it is not paged in.  A distant star is a good example.  When this occurs
//            // if this QuickLook is registered to receive notifications of a particular nodeID
//            // they will be sent to it upon arrival.

//            // We not be given an entity instance however, but the 
//            // information that locates that entity and it's properties so that we can
//            // display them in the QuickLook or update the QuickLook with the new data.

//            // generally this quicklook data is read only for data that is not paged in
//            // because that data is usually for entities that are simulated entirely by 
//            // the game and do not have properties that can be changed by user. (although 
//            // potentially changed by an admin)
//            System.Diagnostics.Debug.Assert(mTargetEntityID == entityID);
//            CreateHTML(mLastPick);
//        }

//        Keystone.Collision.PickResults mLastPick;
//        // TODO: find out when select occurs and then after the call is made, have a subscription
//        // made on behalf of this quicklook so that it is received updates for this particular entity
//        // from this scene
//        public virtual void Select(string sceneName, string entityID, string entityTypeName, Keystone.Collision.PickResults pick)
//        {
//            // NOTE: We do not allow instancing of entities here or references to them from
//            // Repository.  This must go through the scripting host.
//            // The scripting host will in turn send the request to the server if the item
//            // is not cached in Repository.  The server will determine the validity of the request
//            // and send back any properties.
//            // HOWEVER: client side version of db should be completely accessible.  Some of the 
//            // client data may be stale, but that is by design and will reflect best known 
//            // intel at the time.
//            mTargetEntityID = entityID;

//            // TODO: surely assignment to mLastPick doesn't need to be done here
//            mLastPick = pick;

//            // TODO: we need to remove requirment of passing pick arg here.  Instead we should
//            //       find out which properties of PickResults we use in CreateHTML and just pass those
//            //       that way we can call CreateHTML without having a pick event.
//            CreateHTML(pick);     
//// TODO: are multiple QuickLook panels being created with the previous from Editor not hiding
////       when Floorplan workspace becomes active?
//// TODO: i would like this code moved to workspace entitySelected and not left being triggered by plugin
//        }

//        bool QUICKLOOK_ENABLE = false;

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="pick">Can be null.</param>
//        private void CreateHTML(Keystone.Collision.PickResults pick)
//        {
//            // http://www.antlr.org/wiki/display/ST/Examples
//            // http://www.antlr.org/wiki/display/ST/Five+minute+Introduction
//            // TEMP: local test of stringtemplate prior to calling script         
//            if (!QUICKLOOK_ENABLE) return;


//            // call script 
//            string htmlContent = mHost.Entity_GetGUILayout(mTargetEntityID, pick);
//            if (string.IsNullOrEmpty (htmlContent))
//            {
//                System.Diagnostics.Debug.WriteLine ("QuickLook.CreateHTML() - Content is null.");
//                return;
//            }
//            try
//            {
//            	string file = System.IO.Path.Combine (AppMain.MOD_PATH, @"common\layouts\star.html");
//                // TODO: remove hardcoded path
//            	string htmlfile = @"file:///D:/dev/c%23/KeystoneGameBlocks/Data/mods/common/layouts/star.html";            
//            	System.IO.File.WriteAllText (file, htmlContent);
            	

//            	var uri = new Uri(htmlfile, UriKind.Absolute);
            
//            	if (mWebView.Source.OriginalString.Equals (uri.OriginalString))
//            	{
//            		System.Diagnostics.Debug.WriteLine ("QuickLook3.CreateHTML() - Uri address has not changed.  Reload existing page.");         	
//            		mWebView.Reload (true); 
//            	}
//            	else
//            	{
//	            	mWebView.Source = uri;
//	            	//BindMethods(webView);
//            	}

//            	mWebView.FocusView ();
//            }
//            catch (Exception ex) 
//            {
//                System.Diagnostics.Debug.WriteLine("QuickLook3.CreateHTML() - ERROR: " + ex.Message);
//            }
//        }
        
////        private void BindMethods(WebView webView)
////        {
////        	// Create a global js object named 'window'
////    		JSValue window = webView.CreateGlobalJavascriptObject("window");
////   
////    		
////        }
////        
////        using ( JSObject myGlobalObject = webView.CreateGlobalJavascriptObject( "myGlobalObject" ) )
////            {
////                // 'Bind' is the method of the regular API, that needs to be used to create
////                // a custom method on our global object and bind it to a handler.
////                // The handler is of type JavascriptMethodEventHandler. Here we define it
////                // using a lambda expression.
////                myGlobalObject.Bind( "changeHTML", false, ( s, e ) =>
////                {
////                    // Get the new content.
////                    string newContent = e.Arguments[ 0 ];
////                    ChangeHTML( newContent );
////                } );
////                
//        // Bound to app.sayHello() in JavaScript
////	  private void OnSayHello(WebView webView, JSArray args) 
////	  {
////	    	//webView.sho. .ShowMessage("Hello!");
////	  }
        
        
//        #region WebControl Methods
//        private WebSession InitializeCoreAndSession()
//        {
//            if ( !WebCore.IsRunning && !WebCore.IsInitialized)
//                WebCore.Initialize( new WebConfig() { LogLevel = LogLevel.Normal } );

//            // Build a data path string. In this case, a Cache folder under our executing directory.
//            // - If the folder does not exist, it will be created.
//            // - The path should always point to a writeable location.
//            string dataPath = String.Format( "{0}{1}Cache", Path.GetDirectoryName( Application.ExecutablePath ), Path.DirectorySeparatorChar );

//            // Check if a session synchronizing to this data path, is already created;
//            // if not, create a new one.
//            WebSession session = WebCore.Sessions[ dataPath ] ??
//                WebCore.CreateWebSession( dataPath, WebPreferences.Default );

//            // The core must be initialized by now. Print the core version.
//            Debug.Print( "InitializeCoreAndSession() - WebCore Version " + WebCore.Version.ToString() );
//            Debug.Print( "InitializeCoreAndSession() - dataPath " + dataPath);
//            // add a new data src handler for resources specified 
//            // http://wiki.awesomium.com/general-use/using-data-sources.html
//            // Uri dataSrc = new Uri (); 
//            // session.AddDataSource ();
            
//            // Return the session.
//            return session;
//        }

//        private void InitializeView( WebView wv, bool isChild = false, Uri targetURL = null )
//        {
//        	if ( wv == null) throw new ArgumentNullException ();
                

//            // We demonstrate the use of a resource interceptor.
//            if ( WebCore.ResourceInterceptor == null )
//                WebCore.ResourceInterceptor = this;

            
//            // Create an image surface to render the
//            // WebView's pixel buffer.
//            mViewSurface = new ImageSurface();
//            mViewSurface.Updated += OnSurfaceUpdated;


//            // Assign our surface.
//            wv.Surface = mViewSurface;
//            // Assign a context menu.
//            webControlContextMenu.View = wv;
 
//            // Handle some important events.
//            wv.CursorChanged += OnCursorChanged;
//            wv.AddressChanged += OnAddressChanged;
//            //wv.InjectKeyboardEvent += OnKeyboardEvent;
//            wv.ShowCreatedWebView += OnShowNewView;
//            wv.ShowPopupMenu += OnShowPopupMenu;
//            wv.ShowContextMenu += OnShowContextMenu;
//            wv.PrintRequest += OnPrintRequest;
//            wv.PrintComplete += OnPrintComplete;
//            wv.PrintFailed += OnPrintFailed;
//            wv.Crashed += OnCrashed;
//            wv.ShowJavascriptDialog += OnJavascriptDialog;
//            wv.WindowClose += OnWindowClose;
//			wv.SelectionChanged += OnSelectionChanged;
//			wv.TargetURLChanged += OnTargetURLChanged ;
//			wv.ProcessCreated += OnProcessCreated;
//			// We demonstrate binding to properties.
//            mBindingSource = new BindingSource() { DataSource = wv };
//            this.DataBindings.Add( new Binding( "Text", mBindingSource, "Title", true ) );
       
        	
//            this.Focus();
                        
//            // Give focus to the view.
//            wv.FocusView();


            
//            Form f = this.FindForm ();
//            if (f != null)
//            {
//	            f.KeyPreview = true;
//	            this.Parent.KeyDown += OnKeyDown;
//	            this.Parent.KeyPress += OnKeyPress ;
//            }
//        }

//        protected override void OnPaint( PaintEventArgs e )
//        {
//            if ( ( mViewSurface != null ) && ( mViewSurface.Image != null ) )
//                e.Graphics.DrawImageUnscaled( mViewSurface.Image, 0, 0 );
//            else
//                base.OnPaint( e );
//        }

////        protected override void OnActivated( EventArgs e )
////        {
////            base.OnActivated( e );
////            this.Opacity = 1.0D;
////
////            if ( ( webView == null ) || !webView.IsLive )
////                return;
////
////            webView.FocusView();
////        }
////
////        protected override void OnDeactivate( EventArgs e )
////        {
////            base.OnDeactivate( e );
////
////            if ( ( webView == null ) || !webView.IsLive )
////                return;
////
////            // Let popup windows be semi-transparent,
////            // when they are not active.
////            if ( webView.ParentView != null )
////                this.Opacity = 0.8D;
////
////            webView.UnfocusView();
////        }
////        
//// 
//// 

//		private void OnProcessCreated (object sender, EventArgs e )
//		{
//			if (mWebView.IsLive)
//			{
//				using (JSObject globalJSObject = mWebView.CreateGlobalJavascriptObject ("app"))
//				{
//					if (globalJSObject != null)
//					{
//						globalJSObject.Bind ("ButtonClick", OnButtonClick);
//					}
//				}
//			}
//		}
		
		
		
//		private JSValue OnButtonClick (object sender, JavascriptMethodEventArgs e)
//		{
			
//			JSValue[] args = e.Arguments;
//			string buttonID = args[0];
		
//			switch (e.MethodName)
//			{
//				case "ButtonClick":
					
//					switch (buttonID)
//					{
//						case "addtask":
//							break;
//						default:
//							break;
//					}
//					break;
//				default:
//					break;
//			}
			
//			mHost.Entity_GUILinkClicked (mTargetEntityID, buttonID, buttonID);
				
//			JSValue value = new JSValue (true);
//			return value;
//		}
        
//		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
//		{
//			System.Diagnostics.Debug.WriteLine ("ProcessCmdKey");
//			return base.ProcessCmdKey(ref msg, keyData);
//		}

//        protected override void OnResize(EventArgs e)
//        {
//            base.OnResize(e);

//            try
//            {
//                if ((mWebView == null) || !mWebView.IsLive)
//                    return;

//                // Never resize the view to a width or height equal to 0;
//                // instead, you can pause internal rendering.
//                mWebView.IsRendering = (this.ClientSize.Width > 0) && (this.ClientSize.Height > 0);

//                if (mWebView.IsRendering)
//                    // Request a resize.
//                    mWebView.Resize(this.ClientSize.Width, this.ClientSize.Height);
//            }
            
//            catch (Exception ex)
//            {
//                // windows like our preview renderer throws here
//                System.Diagnostics.Debug.WriteLine(ex.Message);
//            }
//        }
        
//		protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
//		{
//			System.Diagnostics.Debug.WriteLine ("OnPreviewKeyDown");
//			base.OnPreviewKeyDown(e);
//		} 
        
//		// http://www.daniweb.com/software-development/csharp/threads/189361/custom-user-control-key-events-problem
//        protected override void OnKeyPress( KeyPressEventArgs e )
//        {
//        	System.Diagnostics.Debug.WriteLine ("OnKeyPress");
//            base.OnKeyPress( e );

//            if ( mWebView == null || !mWebView.IsLive )
//                return;

//            mWebView.InjectKeyboardEvent( e.GetKeyboardEvent() );
//        }

//        void OnKeyPress (object sender, KeyPressEventArgs e)
//        {
//        	System.Diagnostics.Debug.WriteLine ("OnKeyPress");
//        	if ( mWebView == null || !mWebView.IsLive )
//                return;

//            mWebView.InjectKeyboardEvent( e.GetKeyboardEvent() );
//        }
        
//        void OnKeyDown (object sender, KeyEventArgs e)
//        {
//        	System.Diagnostics.Debug.WriteLine ("OnKeyDown");
//        	if ( mWebView == null || !mWebView.IsLive )
//                return;

//             mWebView.InjectKeyboardEvent( e.GetKeyboardEvent(WebKeyboardEventType.KeyDown) );
//        }
//        protected override void OnKeyDown(KeyEventArgs e )
//        {
//        	System.Diagnostics.Debug.WriteLine ("OnKeyDown");
//            base.OnKeyDown( e );

//            if ( mWebView == null || !mWebView.IsLive )
//                return;

//            mWebView.InjectKeyboardEvent( e.GetKeyboardEvent( WebKeyboardEventType.KeyDown ) );
//        }

//        protected override void OnKeyUp(KeyEventArgs e )
//        {
//        	System.Diagnostics.Debug.WriteLine ("OnKeyUp");
//            base.OnKeyUp( e );

//            if ( mWebView == null || !mWebView.IsLive )
//                return;

//            mWebView.InjectKeyboardEvent( e.GetKeyboardEvent( WebKeyboardEventType.KeyUp ) );
//        }

//		protected override void OnClick(EventArgs e)
//		{
//			// UserControl and Panel do not like to take focus.  Force it
//        	// or else we will not be able to receive keyboard events
//			this.Focus();
//			base.OnClick(e);
//		} 
		
//        protected override void OnMouseDown( MouseEventArgs e )
//        {
//        	// UserControl and Panel do not like to take focus.  Force it
//        	// or else we will not be able to receive keyboard events
//        	this.Focus();
        	
//        	System.Diagnostics.Debug.WriteLine ("OnMouseDown");
//            base.OnMouseDown( e );

//            if ( mWebView == null || !mWebView.IsLive )
//                return;
            
//            mWebView.InjectMouseDown( e.Button.GetMouseButton() );
//        }

//        protected override void OnMouseUp( MouseEventArgs e )
//        {
//        	System.Diagnostics.Debug.WriteLine ("OnMouseUp");
//            base.OnMouseUp( e );

//            if ( mWebView == null || !mWebView.IsLive )
//                return;

//            mWebView.InjectMouseUp( e.Button.GetMouseButton() );
//        }

//        protected override void OnMouseMove( MouseEventArgs e )
//        {
//            base.OnMouseMove( e );

//            if ( mWebView == null || !mWebView.IsLive )
//                return;

//            mWebView.InjectMouseMove( e.X, e.Y );
//        }

//        protected override void OnMouseWheel( MouseEventArgs e )
//        {
//            base.OnMouseWheel( e );

//            if ( mWebView == null || !mWebView.IsLive )
//                return;

//            mWebView.InjectMouseWheel( e.Delta, 0 );
//        }
//        #endregion
        
//        #region WebControl Event Handlers
//        private string mCurrentUri;
//	    /// <summary>
//        /// Event fires when mouse hovers over a new link in the page.
//        /// </summary>
//        /// <param name="sender"></param>
//        /// <param name="e"></param>
//        private void OnTargetURLChanged (object sender, UrlEventArgs e)
//        {
//        	string address = "";
//          	if (e.Url != null)
//          		address = e.Url.ToString ();
          
//          	System.Diagnostics.Debug.WriteLine( "QuickLook.OnTargetURLChanged() - " + address );
//        }
        
//        /// <summary>
//        /// Event fires when mouse clicks on a link on the page.
//        /// </summary>
//        /// <param name="sender"></param>
//        /// <param name="e"></param>
//        private void OnAddressChanged( object sender, UrlEventArgs e )
//        {
          
//	   		mCurrentUri = null;
//          	string address = "";
//          	string fragment = "";
//          	  	 	if (e.Url != null)
//          	{
//          		Debug.WriteLine ("QuickLook.OnAddressChanged() - " + e.EventName);
//          		             // mHost.Entity_GUILinkClicked(mTargetEntityID, scriptedMethodName, id);
//          		address = e.Url.ToString ();
//          		fragment = e.Url.Fragment;
//          		mCurrentUri  = e.Url.AbsoluteUri;
//          		string s = e.Url.OriginalString;
          		
//	          	switch (fragment)
//	          	{
//	          		case "#tabs-1":
//	          			// TODO: but this doesn't tell us if we've hit cancel or create task buttons
//	          			//       all it tells us is the address is changing to the Url specified
	          			
//	          			// TODO: how would we get the form data? need to google this.. perhaps have to 
//	          			// post it in the url?
//	          			mHost.Task_Create (null);
//	          			break;
//	          		default:
//	          			break;
//	          	}
//          	}
//          	System.Diagnostics.Debug.WriteLine( "QuickLook.OnAddressChanged() - " + address );
          
//        }

                
//        /// <summary>
//        /// Event fires when the highlighted text on our web page changes
//        /// </summary>
//        /// <param name="sender"></param>
//        /// <param name="e"></param>
//        private void OnSelectionChanged (object sender, WebSelectionEventArgs e)
//        {
//        	string selection = "";
        	
//        	if (e.Selection != null)
//        		selection = e.Selection.Text;
        	
        	
//	        System.Diagnostics.Debug.WriteLine( "QuickLook.OnSelectionChanged() - Selection: " + selection);
//        }
       
        
        
//        private void OnJavascriptDialog( object sender, JavascriptDialogEventArgs e )
//        {
//        	Debug.Print( String.Format( "QuickLook.OnJavascriptDialog() - {0} .", e.ToString() ) );
        	
//            if ( !e.DialogFlags.HasFlag( JSDialogFlags.HasPromptField ) &&
//                !e.DialogFlags.HasFlag( JSDialogFlags.HasCancelButton ) )
//            {
//                // It's a 'window.alert'
//                MessageBox.Show( this, e.Message );
//                e.Handled = true;
//            }
//        }
        
        
//        private void OnCursorChanged( object sender, CursorChangedEventArgs e )
//        {
//            // Update the cursor.
//           // this.Cursor = Awesomium.Windows.Forms.Utilities.GetCursor( e.CursorType );
//        }

//        private void OnSurfaceUpdated( object sender, SurfaceUpdatedEventArgs e )
//        {
//            // When the surface is updated, invalidate the 'dirty' region.
//            // This will force the form to repaint that region.
//            Invalidate( e.DirtyRegion.ToRectangle(), false );
//        }

//        private void OnShowContextMenu( object sender, ContextMenuEventArgs e )
//        {
//            // A context menu is requested, typically as a result of the user
//            // right-clicking in the view. Open our extended WebControlContextMenu.
//            webControlContextMenu.Show( this );
//        }

//        private ListBox lbSelect;
//        private PopupMenuEventArgs _MenuArgs;
        
//        private void OnShowPopupMenu(object sender, PopupMenuEventArgs e)
//        {
//    		// NOTE: awesomium does not yet support drop down list in winforms, only wpf
//			// but it can be implemented manually
//			// http://answers.awesomium.com/questions/498/index.html
//        	// http://answers.awesomium.com/questions/498/how-do-you-handle-drop-down-menus-on-web-pages.html
        	
//        	System.Diagnostics.Debug.WriteLine( "QuickLook.OnShowPopupMenu() - " + e.Info.SelectedItem.ToString() );
//             lbSelect.Items.Clear();
//             uint count = e.Info.Count;
//             for (uint i = 0; i < count; i++)
//                 lbSelect.Items.Add(e.Info[i].Label);
 
//             Rectangle lbb = new Rectangle
//             {
//                 X = e.Info.Bounds.X,
//                 Y = e.Info.Bounds.Y + e.Info.Bounds.Height,
//                 Height = (count <= 10 ? (int)(e.Info.ItemHeight * count) : e.Info.ItemHeight * 10),
//                 Width = e.Info.Bounds.Width
//             };
 
//             lbSelect.Bounds = lbb;
//             lbSelect.Visible = true;
//             lbSelect.SelectedIndex = e.Info.SelectedItem;
//             lbSelect.Focus();
 
//             _MenuArgs = e;
//        }
        
//         void lbSelect_MouseClick(object sender, MouseEventArgs e)
//         {
//             _MenuArgs.Info.Select(lbSelect.SelectedIndex);
//             lbSelect.Visible = false;
//         }
 
//         void lbSelect_LostFocus(object sender, EventArgs e)
//         {
//             lbSelect.Visible = false;
//         }
        
//        private void OnShowNewView( object sender, ShowCreatedWebViewEventArgs e )
//        {
////            if ( ( webView == null ) || !webView.IsLive )
////                return;
////
//            if ( e.IsPopup )
//            {
////                // Create a WebView wrapping the view created by Awesomium.
////                WebView view = new WebView( e.NewViewInstance );
////                // ShowCreatedWebViewEventArgs.InitialPos indicates screen coordinates.
////                Rectangle screenRect = e.Specs.InitialPosition.ToRectangle();
////                // Create a new WebForm to render the new view and size it.
////                QuickLook3 childForm = new QuickLook3( view, screenRect.Width, screenRect.Height )
////                {
////                    ShowInTaskbar = false,
////                    FormBorderStyle = FormBorderStyle.FixedToolWindow,
////                    ClientSize = screenRect.Size != Size.Empty ? screenRect.Size : new Size( 640, 480 )
////                };
////
////                // Show the form.
////                childForm.Show( this );
////
////                if ( screenRect.Location != Point.Empty )
////                    // Move it to the specified coordinates.
////                    childForm.DesktopLocation = screenRect.Location;
//            }
//            else if ( e.IsWindowOpen || e.IsPost )
//            {
////                // Create a WebView wrapping the view created by Awesomium.
////                WebView view = new WebView( e.NewViewInstance );
////                // Create a new WebForm to render the new view and size it.
////                QuickLook3 childForm = new QuickLook3( view, 640, 480 );
////                // Show the form.
////                childForm.Show( this );
//            }
//            else
//            {
//                // Let the new view be destroyed. It is important to set Cancel to true 
//                // if you are not wrapping the new view, to avoid keeping it alive along
//                // with a reference to its parent.
//                e.Cancel = true;

//                // Load the url to the existing view.
//                mWebView.Source = e.TargetURL;
//            }
//        }

//        private void OnCrashed( object sender, CrashedEventArgs e )
//        {
//            System.Diagnostics.Debug.WriteLine( "QuickLook.OnCrasheded() - Status: " + e.Status );
//        }

//        // Called in response to JavaScript: 'window.close'.
//        private void OnWindowClose( object sender, WindowCloseEventArgs e )
//        {
//            // If this is a child form, respect the request and close it.
//            //if ( ( webView != null ) && ( webView.ParentView != null ) )
//            //    this.Close();
//            System.Diagnostics.Debug.WriteLine( "QuickLook.OnWindowClose() - Status: " + e.EventName );
//        }

//        // This is called when the page asks to be printed, usually as result of
//        // a window.print().
//        private void OnPrintRequest( object sender, PrintRequestEventArgs e )
//        {
//            if ( !mWebView.IsLive )
//                return;

//            // You can actually call PrintToFile anytime after the ProcessCreated
//            // event is fired (or the DocumentReady or LoadingFrameComplete in 
//            // subsequent navigations), but you usually call it in response to
//            // a print request. You should possibly display a dialog to the user
//            // such as a FolderBrowserDialog, to allow them select the output directory
//            // and verify printing.
//            int requestId = mWebView.PrintToFile( @".\Prints", PrintConfig.Default );

//            Debug.Print( String.Format( "QuickLook.OnPrintRequest() - {0} is being printed to {1}.", requestId, @".\Prints" ) );
//        }

//        private void OnPrintComplete( object sender, PrintCompleteEventArgs e )
//        {
//        	Debug.Print( String.Format( "QuickLook.OnPrintComplete() - {0} completed. The following files were created:", e.RequestId ) );

//            foreach ( string file in e.Files )
//                Debug.Print( String.Format( "\t {0}", file ) );
//        }

//        private void OnPrintFailed( object sender, PrintOperationEventArgs e )
//        {
//            Debug.Print( String.Format( "QuickLook.OnPrintFailed() - {0} failed! Make sure the provided outputDirectory is writable.", e.RequestId ) );
//        }


//        private void webControlContextMenu_Opening( object sender, ContextMenuOpeningEventArgs e )
//        {
//            // Update the visibility of our menu items based on the
//            // latest context data.
//            openSeparator.Visible =
//                openMenuItem.Visible = !e.Info.IsEditable && ( mWebView.Source != null );
//        }

//        private void webControlContextMenu_ItemClicked( object sender, ToolStripItemClickedEventArgs e )
//        {
////            if ( ( webView == null ) || !webView.IsLive )
////                return;
////
////            // We only process the menu item added by us. The WebControlContextMenu
////            // will handle the predefined items.
////            if ( (string)e.ClickedItem.Tag != "open" )
////                return;
////
////            QuickLook3 webForm = new QuickLook3( webView.Source );
////            webForm.Show( this );
//        }
//        #endregion

//        #region WebControl IResourceInterceptor Members
//        private const string LOGO_RESOURCE = "WinFormsSample.osm_logo_550.png";

        
//        // Note that this is called on the I/O thread.
//        ResourceResponse IResourceInterceptor.OnRequest( ResourceRequest request )
//        {
//            // We are only interested in GET requests.
//            if ( String.Compare( request.Method, "GET", false ) != 0 )
//                return null;

//            string oringalString = request.Url.OriginalString;
//            bool isGoogleHost = oringalString.EndsWith( "google.com" ) ||
//				                oringalString.EndsWith( "ggpht.com" ) ||
//				                oringalString.EndsWith( "gstatic.com" ) ||
//				                oringalString.EndsWith( "googleapis.com" ) ||
//				                oringalString.EndsWith( "googleusercontent.com" );
            

//            if ( isGoogleHost && ( request.Url.Segments.Length > 1 ) )
//            {
//                // Get the last segment of the Uri. This is usually the file name.
//                string fileName = request.Url.Segments[ request.Url.Segments.Length - 1 ];

//                //Debug.Print( "Possible file-name: " + fileName );

//                // Check if this is a request for 'logo4w.png' (this is currently 
//                // the name of the 'Google' logo file).
//                if ( String.Compare( fileName, "logo11w.png", true ) == 0 )
//                {
//                    // Get the embedded resource of the Awesomium logo.
//                    var assembly = System.Reflection.Assembly.GetExecutingAssembly();
//                    var info = assembly.GetManifestResourceInfo( LOGO_RESOURCE );

//                    // The resource does not exist.
//                    if ( info == null )
//                        return null;

//                    using ( var stream = assembly.GetManifestResourceStream( LOGO_RESOURCE ) )
//                    {
//                        // Get a byte array of the resource.
//                        byte[] buffer = new byte[ stream.Length ];
//                        stream.Read( buffer, 0, buffer.Length );

//                        // Initialize unmanaged memory to hold the array.
//                        int size = Marshal.SizeOf( buffer[ 0 ] ) * buffer.Length;
//                        IntPtr pnt = Marshal.AllocHGlobal( size );

//                        try
//                        {
//                            // Copy the array to unmanaged memory.
//                            Marshal.Copy( buffer, 0, pnt, buffer.Length );

//                            // Alternatively, you can pin the array in the managed heap.
//                            // Note however that pinning objects seriously disrupts GC operation. 
//                            // Being able to move objects around in the heap is one of the reasons 
//                            // why modern GCs can (somewhat) keep up with manual memory management. 
//                            // By pinning objects in the managed heap, the GC looses it's one 
//                            // performance advantage over manual memory management: 
//                            // a relatively un-fragmented heap.
//                            //GCHandle handle = GCHandle.Alloc( buffer, GCHandleType.Pinned );
//                            //IntPtr pnt = handle.AddrOfPinnedObject();

//                            // Create a ResourceResponse. A copy is made of the supplied buffer.
//                            return ResourceResponse.Create( (uint)buffer.Length, pnt, "image/png" );
//                        }
//                        finally
//                        {
//                            // Data is not owned by the ResourceResponse. A copy is made 
//                            // of the supplied buffer. We can safely free the unmanaged memory.
//                            Marshal.FreeHGlobal( pnt );
//                        }
//                    }
//                }
//            }
//            else 
//            {
//            	//Debug.Print( String.Format( "QuickLook.OnRequest() - {0}.", oringalString) );
//            }

//            // Return NULL to allow normal behavior.
//            return null;
//        }

//        // Note that this is called on the I/O thread.
//        bool IResourceInterceptor.OnFilterNavigation( NavigationRequest request )
//        {
//        	string oringalString = request.Url.OriginalString;
//            bool isGoogleHost = oringalString.EndsWith( "google.com" ) ||
//				                oringalString.EndsWith( "ggpht.com" ) ||
//				                oringalString.EndsWith( "gstatic.com" ) ||
//				                oringalString.EndsWith( "googleapis.com" ) ||
//				                oringalString.EndsWith( "googleusercontent.com" );

//            // Uncomment the following lines, to block (almost) everything
//            // outside Google. This will cancel any attempt to navigate away 
//            // from Google sites.
//            // return !isGoogleHost;

//            return false; // false == do not filter, allow continue
//        }
//        #endregion
	
////        #region WebControl Custom Javascript Method Handlers
////        // Custom Javascript methods' handler.
//        private void OnCustomJavascriptMethod( object sender, JavascriptMethodEventArgs e )
//        {
////            // We can have the same handler handling many remote methods.
////            // Check here the method that is calling the handler.
////            switch ( e.MethodName )
////            {
////                case "myMethod":
////                    // Print the text passed.
////                    Debug.Print( e.Arguments[ 0 ] );
////
////                    if ( e.MustReturnValue )
////                        // Provide a response.
////                        e.Result = "Message Received!";
////
////                    break;
////
////                case "myAsyncMethod":
////                    // Print the text passed.
////                    Debug.Print( e.Arguments[ 0 ] );
////
////                    break;
////
////                case "onMyInterval":
////                    if ( MessageBox.Show( "3 seconds passed.\n\nCancel this timer?", "Window Interval", MessageBoxButtons.YesNo ) != System.Windows.Forms.DialogResult.Yes )
////                        return;
////
////                    // For this example, the 'onMyInterval' is called asynchronously. However, if
////                    // 'onMyInterval' was created as synchronous, we would have to cancel out interval 
////                    // using only asynchronous calls, since synchronous calls from inside a synchronous
////                    // custom method handler, are not allowed.
////                    if ( e.MustReturnValue )
////                    {
////                        // Just for demonstration, in case 'onMyInterval' is synchronous.
////                        webView.ExecuteJavascript( "window.clearInterval( window.myTimer );" );
////                    }
////                    else
////                    {
////                        dynamic window = (JSObject)webView.ExecuteJavascriptWithResult( "window" );
////
////                        // Make sure we have the object.
////                        if ( window == null )
////                            return;
////
////                        using ( window )
////                            window.clearInterval( window.myTimer );
////                    }
////
////                    break;
////            }
//        }
////        #endregion
		
//		#region WebControl INavigationInterceptor Members
//		event BeginLoadingFrameEventHandler INavigationInterceptor.BeginLoadingFrame 
//		{
//			add {
//				throw new NotImplementedException();
//			}
//			remove {
//				throw new NotImplementedException();
//			}
//		}
		
//		event BeginNavigationEventHandler INavigationInterceptor.BeginNavigation 
//		{
//			add {
//				throw new NotImplementedException();
//			}
//			remove {
//				throw new NotImplementedException();
//			}
//		}
		
//		event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged 
//		{
//			add {
//				throw new NotImplementedException();
//			}
//			remove {
//				throw new NotImplementedException();
//			}
//		}
		
//		event System.Collections.Specialized.NotifyCollectionChangedEventHandler System.Collections.Specialized.INotifyCollectionChanged.CollectionChanged 
//		{
//			add {
//				throw new NotImplementedException();
//			}
//			remove {
//				throw new NotImplementedException();
//			}
//		}
		
//		string[] INavigationInterceptor.Whitelist 
//		{
//			get {
//				throw new NotImplementedException();
//			}
//		}
		
//		string[] INavigationInterceptor.Blacklist 
//		{
//			get {
//				throw new NotImplementedException();
//			}
//		}
		
//		NavigationRule INavigationInterceptor.ImplicitRule 
//		{
//			get {
//				throw new NotImplementedException();
//			}
//			set {
//				throw new NotImplementedException();
//			}
//		}
		
//		CollectionChangeAction INavigationInterceptor.AddRule(string pattern, NavigationRule rule)
//		{
//			throw new NotImplementedException();
//		}
		
//		CollectionChangeAction INavigationInterceptor.AddRule(NavigationFilterRule filterRule)
//		{
//			throw new NotImplementedException();
//		}
		
//		int INavigationInterceptor.AddRules(params NavigationFilterRule[] rules)
//		{
//			throw new NotImplementedException();
//		}
		
//		int INavigationInterceptor.RemoveRules(string pattern)
//		{
//			throw new NotImplementedException();
//		}
		
//		int INavigationInterceptor.RemoveRules(string pattern, NavigationRule rule)
//		{
//			throw new NotImplementedException();
//		}
		
//		bool INavigationInterceptor.Contains(string pattern)
//		{
//			throw new NotImplementedException();
//		}
		
//		void INavigationInterceptor.Clear()
//		{
//			throw new NotImplementedException();
//		}
		
//		NavigationRule INavigationInterceptor.GetRule(string url)
//		{
//			throw new NotImplementedException();
//		}
		
//		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
//		{
//			throw new NotImplementedException();
//		}
		
//		IEnumerator<NavigationFilterRule> IEnumerable<NavigationFilterRule>.GetEnumerator()
//		{
//			throw new NotImplementedException();
//		}
//		#endregion
//	}
//}
