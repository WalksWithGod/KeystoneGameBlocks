using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using KeyPlugins;
using HtmlRenderer.Entities;
using KeyEdit.Properties;
using Antlr4.StringTemplate;
using Antlr4.StringTemplate.Compiler;
using System.Runtime.InteropServices; // for SuspendDrawing and ResumeDrawing

namespace KeyEdit.GUI
{
	public class DrawingControl
	{
	    [DllImport("user32.dll")]
	    public static extern int SendMessage(IntPtr hWnd, Int32 wMsg, bool wParam, Int32 lParam);
	
	    private const int WM_SETREDRAW = 11; 
	
	    public static void SuspendDrawing( Control parent )
	    {
	        SendMessage(parent.Handle, WM_SETREDRAW, false, 0);
	    }
	
	    public static void ResumeDrawing( Control parent )
	    {
	        SendMessage(parent.Handle, WM_SETREDRAW, true, 0);
	        parent.Refresh();
	    }
	}
	
	/// <summary>
	/// QuickLook implementation that uses the unfinished HtmlRenderer control.
	/// </summary>
    public partial class QuickLook : HtmlRenderer.HtmlPanel
    {
    	Timer mTestTimer;
		Timer mTestTimer2;

        protected KeyEdit.PluginHost.EditorHost mHost;
        protected string mTargetEntityID;
        SourceGrid.Cells.Editors.TextBox mEditorString; // editor is required along with a controller
        SourceGrid.Cells.Editors.TextBoxNumeric mEditorNumeric;
        SourceGrid.Cells.Views.Cell categoryView;
        SourceGrid.Cells.Views.ColumnHeader headerView;

        
            //Awesomium.Windows.Forms.WebControl mWebControl = new Awesomium.Windows.Forms.WebControl ();

        public QuickLook() { }

        // TODO: re-selecting exact same entity should not re-init a new QuickLook panel...
        // TODO: can i set a timer to manually update a cssbox and verify we get flicker free updating
        //       to just that box?
        
        public QuickLook(KeyEdit.PluginHost.EditorHost scriptingHost)
        {
            if (scriptingHost == null) throw new ArgumentNullException();
            InitializeComponent();
            mHost = scriptingHost;

			
            // disable automatic double buffering since we'll be implementing our own
            this.DoubleBuffered = true;
    		this.SetStyle(ControlStyles.UserPaint, true);
    		this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
    		this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			//this.SetStyle(ControlStyles.Opaque, true); // eliminates any background drawing. Depending on your application you may want a background.
    		
			// create a buffered graphics context to draw to
    		// http://stackoverflow.com/questions/76651/dirty-rectangles

    		
		    // in order to render just the dirty rectangles, we need
		    // to not only know which rectangles are dirty
		    // we need to know what is to be drawn there.  
		    // - but can we update a dirty rectangle that is a child 
		    // to a sequence of ancestors without drawing all of the ancestors?
		    // to do that, we need to clip all ancester drawing to the dirty area?
		    mTestTimer = new Timer ();
            mTestTimer.Interval = 1000;
            mTestTimer.Tick += Timer_OnTick;
            mTestTimer.Start ();
            

            //mWebControl.
            	
            // wire up events
            this.ImageLoad += OnImageLoad;
            this.LinkClicked += OnLinkClicked;
            this.LinkMouseEnter += OnLinkMouseEnter;
            this.LinkMouseLeave += OnLinkMouseLeave;
            this.RenderError += OnRenderError;
            this.StylesheetLoad += OnStyleSheetLoad;
			// this.FontChanged += 
			
            this.htmlPanel1.Visible = false; 

            // obsolete - grid replaced by html layout?
            //InitializeGrid();
            grid.Visible = false;

        }


        private object mTimerSynch = new object();
         
        // called once a second to refresh the quicklook panel
        private void Timer_OnTick (object sender, EventArgs e)
        {
        	// TODO: or perhaps we add "watches" to property values and have changes
        	//       to them set flag for mGuiDirty = true; and use that to control
        	//       if html refresh is done
        	CreateHTML(mLastPick);
        }
        

        
        #region Paint
		protected override void OnInvalidated(InvalidateEventArgs e)
		{
			base.OnInvalidated(e);
		}
		
		protected override void OnPaint(PaintEventArgs e)
		{
//			this.SuspendLayout();

//			// setting new HTML to the control will cause the AutoScrollPosition to reset, i'm not sure why or where			
//    //	    this.AutoScrollPosition = new Point(Math.Abs(mPreviousVerticalScrollPosition.X), Math.Abs(mPreviousVerticalScrollPosition.Y));
//			mBuffer.Graphics.Clear (this.BackColor);
//	
//			// draw the HTML visuals to our buffer
//			this.PerformPaint (mBuffer.Graphics);
//			
//			// draw the buffer to the screen
//    	    mBuffer.Render(e.Graphics);
    	    
			e.Graphics.Clear (this.BackColor);
    	    this.PerformPaint (e.Graphics);
//    	    this.ResumeLayout();
		}
		
		protected override void OnPaintBackground(PaintEventArgs e)
		{
			// NOTE: as we are drawing our own background, do not allow the base implementation to be called
			// base.OnPaintBackground(e);
		} 
		
		protected override void OnScroll(ScrollEventArgs se)
		{
			base.OnScroll(se);
		    mPreviousVerticalScrollPosition = this.AutoScrollPosition ;
		}
		
		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
		
		} 
		
		BufferedGraphics mBuffer;
		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			
			BufferedGraphicsContext context = BufferedGraphicsManager.Current;
			 			
			if (mBuffer != null)
			{
				mBuffer.Graphics.Dispose();
				mBuffer.Dispose();
				mBuffer = null;
			}	
							
			mBuffer = context.Allocate(this.CreateGraphics(), this.DisplayRectangle );    

		} 
		#endregion
		
		protected override Point ScrollToControl(Control activeControl)
		{
			//return base.ScrollToControl(activeControl);
			// Returning the current location prevents the panel from
	        // scrolling to the active control when the panel loses and regains focus
	        return this.DisplayRectangle.Location;
		}
		
		private Point mPreviousVerticalScrollPosition; 
		
        public virtual void Notify(string entityID, Settings.PropertySpec[] customProperties)
        {
            // often when we Select an Entity, that entity needs to be read from the database because
            // it is not paged in.  A distant star is a good example.  When this occurs
            // if this QuickLook is registered to receive notifications of a particular nodeID
            // they will be sent to it upon arrival.

            // We not be given an entity instance however, but the 
            // information that locates that entity and it's properties so that we can
            // display them in the QuickLook or update the QuickLook with the new data.

            // generally this quicklook data is read only for data that is not paged in
            // because that data is usually for entities that are simulated entirely by 
            // the game and do not have properties that can be changed by user. (although 
            // potentially changed by an admin)
            System.Diagnostics.Debug.Assert(mTargetEntityID == entityID);
            // PopulateGrid(customProperties); // obsolete - HTML Panel used in place of grid
            CreateHTML(mLastPick);
        }

        Keystone.Collision.PickResults mLastPick;
        // TODO: find out when select occurs and then after the call is made, have a subscription
        // made on behalf of this quicklook so that it is received updates for this particular entity
        // from this scene
        public virtual void Select(string sceneName, string entityID, string entityTypeName, Keystone.Collision.PickResults pick)
        {
            // NOTE: We do not allow instancing of entities here or references to them from
            // Repository.  This must go through the scripting host.
            // The scripting host will in turn send the request to the server if the item
            // is not cached in Repository.  The server will determine the validity of the request
            // and send back any properties.
            // HOWEVER: client side version of db should be completely accessible.  Some of the 
            // client data may be stale, but that is by design and will reflect best known 
            // intel at the time.
            mTargetEntityID = entityID;

            mLastPick = pick;
            // obsolete - grid replaced by html layout?
            //grid.Tag = mTargetEntityID;
            //PopulateGrid(customProperties); // obsolete - HTML Panel used in place of grid
            CreateHTML(pick);     
// TODO: are multiple QuickLook panels being created with the previous from Editor not hiding
//       when Floorplan workspace becomes active?
// TODO: i would like this code moved to workspace entitySelected and not left being triggered by plugin
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pick">Can be null.</param>
        private void CreateHTML(Keystone.Collision.PickResults pick)
        {
            // http://www.antlr.org/wiki/display/ST/Examples
            // http://www.antlr.org/wiki/display/ST/Five+minute+Introduction
            // TEMP: local test of stringtemplate prior to calling script         

            // TODO: remember this is normally occuring in script only here we'd use EntityAPI to retrieve properties
            //Settings.PropertySpec[] customProperties = mHost.Entity_GetCustomProperties(sceneName, mTargetEntityID, entityTypename);


            //  string html = PowerHTMLGen(mTargetEntityID, name); 
            //  html = StarHTMLGen (mTargetEntityID);
            //	html = WorldHTMLGen (mTargetEntityID);

            // call script 
            string html = mHost.Entity_GetGUILayout(mTargetEntityID, pick);
            
            
        	//html = AcidTest();
  
            try
            {
            	lock(mTimerSynch)
            	{
            		DrawingControl.SuspendDrawing (this);
            		this.SetHtml (html, false);
            		
            		DrawingControl.ResumeDrawing (this);
            	}
            }
            catch (Exception ex) 
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }


        private string AcidTest()
        {
        	string path = @"E:\dev\c#\KeystoneGameBlocks\Data\pool\layouts\tests\acidtest.html";
            return System.IO.File.ReadAllText(path);
        }
        
        // http://www.antlr.org/wiki/display/ST4/StringTemplate+4+Wiki+Home
        // http://www.cs.usfca.edu/~parrt/course/601/lectures/stringtemplate.html

        // these scripts are run from \\E:\dev\c#\KeystoneGameBlocks\KeyScript\bin\x86\Debug_x86
        // but why?  in our shipped app surely the only \bin folder will be the main where client exe sits

        // so i think that is the default search directory
        // but forgetting about that for now, how do we handle localization?
        // we might want templates that are localized... 
        // 
        // TODO: do we support dynamic updates to the templates?  good for debugging but not good
        // for anything else so im saying low priority.
        // 
        // so next order of business?
        // - 

        //string path;
        //TemplateGroupDirectory dir = new TemplateGroupDirectory(path);
        //string st = "";
        //TemplateGroup tGroup = new TemplateGroup('$', '$');
        //Template t = new Template(tGroup, st);
        //Template t = new Template("", '$', '$');

        // http://www.antlr.org/wiki/display/ST4/Introduction
        // TODO: if these templates are inside of a zip, i should be trying to load these templates via an API call
        //       and not directly through file io commands in here
        // string dir = System.IO.Path.Combine (mHost.DataPath, "\\templates");

        // http://www.htmldog.com/
        // http://gurps.wikia.com/wiki/Main_Page
        // http://fudge.ouvaton.org/fths.html
        // http://fudge.ouvaton.org/fths-veh.html
        // http://brentnewhall.com/games/gurps_4e.php
        // http://www.w3schools.com/html/html_css.asp
        // http://www.w3schools.com/html/tryit.asp?filename=tryhtml_styles
        // http://www.w3schools.com/html/html_lists.asp   <-- like the list
        // http://www.boeing.com/boeing/defense-space/support/maintenance/b1-lancer/index.page?
        // http://www.canon.com/bctv/products/
        // http://www.novaresource.org/engine.htm

        // reactors
        // http://www.world-nuclear.org/info/Nuclear-Fuel-Cycle/Power-Reactors/Nuclear-Power-Reactors/
        // http://www.energy.siemens.com/hq/en/power-transmission/transformers/power-transformers/large-power-transformers.htm
        // - reactor -> steam -> turbines (varying number) -> transformers -> transmissions wires -> 
        //   power distribution junctions -> potentially more transformers and distribution junctions -> end devices
        // eg US Ronald Reagan has 2x electric turbines connected to each of 2x nuclear reactors each spinning 2x shaft propellers
        // _thermoelectric generators can convert heat direclty into electricity so instead of steam and turbines
        // http://en.wikipedia.org/wiki/Radioisotope_thermoelectric_generator
        // http://en.wikipedia.org/wiki/Optoelectric_nuclear_battery

        
       
            // we mostly want to see habital zone and mineral info
            // and if applicable, culture and life.
            // http://www.youtube.com/watch?v=JZHW5wbys3M

            // we will only show orbital info, mass, eccentricity and all this other
            // stuff as "more" popup
        
        private void PowerHTMLGen(string entityID, string name)
        {

            // following is for fission reactor - these generate electro plasma
            // http://en.wikipedia.org/wiki/Plasma_%28physics%29
            // http://en.wikipedia.org/wiki/Advanced_gas-cooled_reactor  <-- uses carbon dioxide
            // http://en.wikipedia.org/wiki/Gas-cooled_fast_reactor      <-- use helium
            //http://en.wikipedia.org/wiki/Generation_IV_reactor   <-- predicted first appearance in year 2030

            // name      Load: 0 W / 150,000 W
            // image     Health: 100% 
            //           more... (pops up a tooltip showing detailed craftsmanship, materials, volume, weight, cost, etc)

            // Coolant Pressure - 75%
            // //           Core Temperature: 1200F  // even with control rods fully in place, temperature is will still be hot
            //           - 95C (200F) at shutdown - http://en.wikipedia.org/wiki/Shutdown_%28nuclear_reactor%29
            //           - pressurized water reactors steam reaches ~325C
            //           - gas coled reactors steam reaches ~640C (1,184F)
            //           - gas cooled fast reactors steam reaches 850C 
            //
            //
            // Fuel - (internal) Uranium-238 
            // --------------------------
            // 66 / 100 (2.1 year duration) - [Replenish] // tip: requires shutdown of reactor and 1 hour time to replace
            // //   - or remove [ requires shutdown which can take 1 hour]
            // //   - and add [ requires x interval to add]
            //
            // 
            // Power Channels [4] -  Add / Remove (is for construction only)
            // --------------------------
            // Channel[1] 60,000 W (engine[0] - 75,000 W rated) <-- do you increase input levels here?  I mean to a max point i suppose you can right?  since it's a transformer you can configure it up to reactor output / #channels
            // Channel[2] 60,000 W (engine[1] - 75,000 W rated)
            // Channel[3] 4,500 W (APC [0] - 9,000 W rated) 
            //  APC is like a transformer in that it can channel out input at a required amount
            //  per channel.
            //   - in this sense, APC's share some interface commonality with power generators
            //   when it comes to distribution of power.  However one main difference is that
            //   APC power links can be rewired without going to starbase.
            // Channel[3] 4,500 W (APC [1] - 9,000 W rated)
            //
            //
           
            // CSS row highlighting
            // http://datatables.net/examples/advanced_init/highlight.html
            // http://datatables.net/forums/discussion/comment/16117
            // http://www.permadi.com/tutorial/cssHighlightTableRow/
            

            // collapsable lists in html?
            // http://stackoverflow.com/questions/15095933/pure-css-collapse-expand-div

            
        }



        #region HTMLRenderer Events
        /// <summary>
        /// Handle stylesheet resolve.
        /// </summary>
        private void OnStyleSheetLoad(object sender, HtmlStylesheetLoadEventArgs e)
        {
            var stylesheet = GetStyleSheet(e.Src);
            if (stylesheet != null)
                e.SetStyleSheet = stylesheet;
        }

        private void OnImageLoad(object sender, HtmlImageLoadEventArgs e)
        {
            // load the image from the embedded resource if it is an embedded resource...
            // it might not be!  it might want to convert it to a localized path if available.
            Image img = TryLoadResourceImage(e.Src);

            Point p = new Point (0, 0);
            Rectangle r = new Rectangle(p, img.Size);
            if (img != null)
                e.Callback(img, r);
        }

        private string GetStyleSheet (string src)
        {
            if (src.ToLower() == "styles/style0.css")
            {
                string path = @"E:\dev\c#\KeystoneGameBlocks\Data\pool\layouts\styles\style3.css";
                return System.IO.File.ReadAllText(path);
            }
            return "";
        }

        /// <summary>
        /// Get image by resource key.
        /// </summary>
        private Image TryLoadResourceImage(string src)
        {
        	// TODO: any cache should be done here?
        	// TODO: why aren't fonts loaded this way as well?
            switch (src.ToLower())
            {
            	case "planet1_quicklook.png":
            		return Image.FromFile (@"E:\dev\c#\KeystoneGameBlocks\Data\scientificdatabase\planet1_quicklook.png");

				case "reactor1_quicklook.png":
            		return Image.FromFile (@"E:\dev\c#\KeystoneGameBlocks\Data\scientificdatabase\reactor1_quicklook.png");
            	case "corona_yellow.jpg":
            		return Image.FromFile (@"E:\dev\c#\KeystoneGameBlocks\Data\scientificdatabase\corona_yellow.jpg"); 
            			
        		//case "htmlicon":
                //    return Resources.html32;
                //case "staricon":
                //    return Resources.favorites32;
                //case "fonticon":
                //    return Resources.font32;
                //case "commenticon":
                //    return Resources.comment16;
                case "imageicon":
                    return Resources.image32;
                case "methodicon":
                    return Resources.method16;
                case "propertyicon":
                    return Resources.property16;
                case "eventicon":
                    return Resources.Event16;
                case "keyicon":
                    return Resources.key16;
                case "lockicon":
                    return Resources.lock16;
                case "lockopenicon":
                    return Resources.lock_open16;
                case "repairicon":
                    return Resources.repair;
                case "repairicon2":
                    return Resources.repair2;
                case "wrenchicon":
                    return Resources.wrench1;
                case "wrenchicon2":
                    return Resources.wrench2;
                case "radiationicon":
                    return Resources.caution_radiation;
                case "temperatureicon":
                    return Resources.temperature_5;
                case "sort_asc":
                    return Resources.sort_asc ;
                case "sort_desc":
                    return Resources.sort_desc ;
                case "sort_both":
                    return Resources.sort_both ;
                case "sort_asc_disabled":
                    return Resources.sort_asc_disabled;
                case "sort_desc_disabled":
                    return Resources.sort_desc_disabled;
                default:
                    System.Diagnostics.Debug.WriteLine ("QuickLook.TryLoadResourceImage() - " + src);
                    return Resources.image32;

            }
            return null;
        }

        /// <summary>
        /// Show error raised from html renderer.
        /// </summary>
        private void OnRenderError(object sender, HtmlRenderErrorEventArgs e)
        {
            MessageBox.Show(e.Message + (e.Exception != null ? "\r\n" + e.Exception : null), "Error in Html Renderer", MessageBoxButtons.OK);
        }

        private void OnLinkMouseEnter(object sender, HtmlLinkClickedEventArgs e)
        {
        	System.Diagnostics.Debug.WriteLine ("QuickLook.OnLinkMouseEnter() - ");
        }
                
        private void OnLinkMouseLeave(object sender, HtmlLinkClickedEventArgs e)
        {
        	System.Diagnostics.Debug.WriteLine ("QuickLook.OnLinkMouseLeave() - ");
        }
                

        /// <summary>
        /// On specific link click handle it here. NOTE: links can contain images
        /// </summary>
        private void OnLinkClicked(object sender, HtmlLinkClickedEventArgs e)
        {
            // link with image is easy
            // http://www.w3schools.com/html/tryit.asp?filename=tryhtml_imglink

            // NOTE: We do not allow instancing of entities here or references to them from
            // Repository.  This must go through the scripting host.
            // The scripting host will in turn send the request to the server if the item
            // is not cached in Repository.  The server will determine the validity of the request
            // and send back any properties.
            

            if (e.Link == "#")
            {
            	string handler = null;
            	bool hasHandler = e.Attributes.TryGetValue("data-handler", out handler);
            
	            if (hasHandler)
	            {
	            	string id = null;
	            	bool hasID = e.Attributes.TryGetValue ("id", out id);
	                string scriptedMethodName = handler;
	                switch (handler)
	                {
	                	case "GUILayout_LinkClick":
	                		break;
	                	case "GUILayout_ButtonClick":
	                		break;
	                	default:
	                		break;
	                }
	                // TODO: verify i can a custom attribute "handler" to a link ref so i can
	                // use it to store the method name to call in the entity's script
	                mHost.Entity_GUILinkClicked(mTargetEntityID, scriptedMethodName, id);
	                e.Handled = true;
	                
	            }
           }
           else // default
           {
           		// we should typically always have e.Handled = true; else HtmlContainer will try to handle it with a processInfo run attempt
           		e.Handled = true;
           }
        }
        #endregion

        #region Grid
        private void InitializeGrid()
        {
            // init editors
            mEditorString = new SourceGrid.Cells.Editors.TextBox(typeof(string));
            mEditorNumeric = new SourceGrid.Cells.Editors.TextBoxNumeric(typeof(float));

            grid.ColumnsCount = 2; // TODO: maybe make this 3 columns
            //Stretch only the last column
            grid.Columns[0].AutoSizeMode = SourceGrid.AutoSizeMode.EnableAutoSize;
            grid.Columns[1].AutoSizeMode = SourceGrid.AutoSizeMode.EnableAutoSize | SourceGrid.AutoSizeMode.EnableStretch;
            grid.AutoStretchColumnsToFitWidth = true;

            //Category header
            categoryView = new SourceGrid.Cells.Views.Cell();
            categoryView.Background = new DevAge.Drawing.VisualElements.BackgroundLinearGradient(Color.FromKnownColor(KnownColor.GradientActiveCaption), Color.FromKnownColor(KnownColor.ActiveCaption), 0);
            categoryView.ForeColor = Color.FromKnownColor(KnownColor.ActiveCaptionText);
            categoryView.TextAlignment = DevAge.Drawing.ContentAlignment.MiddleCenter;
            categoryView.Border = DevAge.Drawing.RectangleBorder.NoBorder;
            categoryView.Font = new Font(Font, FontStyle.Bold);

            //Title header
            headerView = new SourceGrid.Cells.Views.ColumnHeader();
        }

        /// <summary>
        /// Initialize the headers, and special look and field of our grid
        /// </summary>
        private void PopulateGrid(Settings.PropertySpec[] properties)
        {
            int row = -1;

            grid.Rows.Clear();
           
            // sort by group.  we will skip "private" var groups for instance
            // but we also want the grouped elements together.  for now we can just
            // rely on the fact that we add them in order.
            string lastCategory = "";
            for (int i = 0; i < properties.Length; i++)
            {
                // create new category if applicable
                // TODO: this relies on each property of same
                // category being grouped together so should be considered temp solution
                if (properties[i].Category != lastCategory)
                {
                    row = CreateCategoryRow(grid, properties[i].Category, categoryView);
                    lastCategory = properties[i].Category;
                }

                row = CreateRow(grid, new SourceGrid.Cells.Views.Cell(), properties[i].Name,
                    properties[i].TypeName, properties[i].ConverterTypeName, properties[i].DefaultValue);

                int linkedControlColumn = 1;

                // link any controls depending on the editor type and converter type
                switch (properties[i].EditorTypeName)
                {
                    case "progressbar":
                        Control linkedControl = new ProgressBar();
                        linkedControl.Name = "progressbar" + row.ToString();

                        grid.LinkedControls.Add(new SourceGrid.LinkedControlValue(linkedControl, new SourceGrid.Position(row, linkedControlColumn)));
                        break;

                    case null:
                    case "":
                    default:
                        break;
                }
            }

            grid.AutoSizeCells();
            grid.Columns.StretchToFit();

         }

        protected int CreateRow(SourceGrid.Grid grid, SourceGrid.Cells.Views.Cell view, 
            string caption, string typename, string converterTypeName, object value)
        {
            int row = grid.RowsCount;

            grid.Rows.Insert(row);
            grid[row, 0] = new SourceGrid.Cells.Cell(caption);
            grid[row, 0].View = view;

            if (value != null)
            {
                switch (typename)
                {
                    case KeyCommon.Helpers.ExtensionMethods.mFullyQualifiedBool:
                    case "bool":
                    case "boolean":
                        //grid[row, 1] = new SourceGrid.Cells.CheckBox(null, bool.Parse((string)value));
                        grid[row, 1] = new SourceGrid.Cells.CheckBox(null, (bool)value);
                        grid[row, 1].AddController(new BooleanPropertyController(mHost, caption));
                        break;
                    case KeyCommon.Helpers.ExtensionMethods.mFullyQualifiedString:
                    case "string":
                        // if it does NOT have standard values, just use normal text field editor
                        // else use combobox
                        string[] standardValues = GetStandardValues(converterTypeName);

                        if (standardValues == null || standardValues.Length == 0)
                        {
                            grid[row, 1] = new SourceGrid.Cells.Cell(value.ToString());
                            grid[row, 1].Editor = mEditorString;
                        }
                        else
                        {
                            SourceGrid.Cells.Editors.ComboBox cbEditor = new SourceGrid.Cells.Editors.ComboBox(typeof(string));
                            cbEditor.StandardValues = standardValues;
                            cbEditor.EditableMode = SourceGrid.EditableMode.Focus | SourceGrid.EditableMode.SingleClick | SourceGrid.EditableMode.AnyKey;
                            grid[row, 1] = new SourceGrid.Cells.Cell((string)value, cbEditor);
                            grid[row, 1].View = SourceGrid.Cells.Views.ComboBox.Default;
                        }

                        grid[row, 1].AddController(new TextPropertyController(mHost, caption));
                        break;
                    case KeyCommon.Helpers.ExtensionMethods.mFullyQualifiedSingle:
                    case "float":
                    case "single":
                        grid[row, 1] = new SourceGrid.Cells.Cell((float)value);
                        grid[row, 1].AddController(new FloatPropertyController(mHost, caption));
                        grid[row, 1].Editor = mEditorNumeric;
                        break;
                    case KeyCommon.Helpers.ExtensionMethods.mFullyQualifiedDouble:
                    case "double":
                    case "Double":
                        double result = 0.0d;
                        if (value is int)
                            result = (double)(int)value;
                        else
                            result = (double)value;

                        grid[row, 1] = new SourceGrid.Cells.Cell(result);
                        grid[row, 1].AddController(new DoublePropertyController(mHost, caption));
                        grid[row, 1].Editor = mEditorNumeric;
                        break;
                    default:
                        grid[row, 1] = new SourceGrid.Cells.Cell(value.ToString());
                        break;
                }
            }
            else
                grid[row, 1] = new SourceGrid.Cells.Cell(null);

            grid[row, 1].View = view;
            return row;
        }

        protected int CreateCategoryRow(SourceGrid.Grid grid, string category, SourceGrid.Cells.Views.Cell categoryView)
        {
            //Create Category Row
            int row = grid.RowsCount;
            grid.Rows.Insert(row);
            grid[row, 0] = new SourceGrid.Cells.Cell(category);
            grid[row, 0].View = categoryView;
            grid[row, 0].ColumnSpan = grid.ColumnsCount;

            return row;
        }
        #endregion

        #region Custom Cell Edit Controllers
        private abstract class PropertyController : SourceGrid.Cells.Controllers.ControllerBase
        {
            protected IPluginHost mHost;
            protected string mPropertyName;

            public PropertyController(IPluginHost host, string propertyName)
            {
                if (string.IsNullOrEmpty(propertyName)) throw new ArgumentNullException();
                mHost = host;
                mPropertyName = propertyName;
            }

        }

        private class BooleanPropertyController : PropertyController
        {
            public BooleanPropertyController(IPluginHost host, string propertyName) 
                : base (host, propertyName)
            {}

            public override void OnValueChanged(SourceGrid.CellContext sender, EventArgs e)
            {
                base.OnValueChanged(sender, e);

                int row = sender.Position.Row;
                string id = (string)((SourceGrid.Grid)sender.Grid).Tag;

                if (mHost != null && (!(string.IsNullOrEmpty(id))))
                    mHost.Entity_SetCustomPropertyValue(id, mPropertyName, typeof(bool).Name, (bool)sender.Value);
            }
        }


        private class TextPropertyController : PropertyController
        {
            public TextPropertyController(IPluginHost host, string propertyName) 
                : base (host, propertyName)
            {}

            public override void OnValueChanged(SourceGrid.CellContext sender, EventArgs e)
            {
                base.OnValueChanged(sender, e);

                int row = sender.Position.Row;
                string id = (string)((SourceGrid.Grid)sender.Grid).Tag;

                if (mHost != null && (!(string.IsNullOrEmpty(id))))
                    mHost.Entity_SetCustomPropertyValue(id, mPropertyName, typeof(string).Name, (string)sender.Value);
            }
        }

        private class FloatPropertyController : PropertyController
        {
            public FloatPropertyController(IPluginHost host, string propertyName)
                : base(host, propertyName)
            {}

            public override void OnValueChanged(SourceGrid.CellContext sender, EventArgs e)
            {
                int row = sender.Position.Row;
                string id = (string)((SourceGrid.Grid)sender.Grid).Tag;
                
                if (mHost != null && (!(string.IsNullOrEmpty(id))))
                    mHost.Entity_SetCustomPropertyValue(id, mPropertyName, typeof(float).Name, (float)sender.Value);
            }
        }

        private class DoublePropertyController : PropertyController
        {
            public DoublePropertyController(IPluginHost host, string propertyName)
                : base(host, propertyName)
            { }

            public override void OnValueChanged(SourceGrid.CellContext sender, EventArgs e)
            {
                int row = sender.Position.Row;
                string id = (string)((SourceGrid.Grid)sender.Grid).Tag;
                
                if (mHost != null && (!(string.IsNullOrEmpty(id))))
                    mHost.Entity_SetCustomPropertyValue(id, mPropertyName, typeof(double).Name, (double)(float)sender.Value);
            }
        }

        private class VectorPropertyController : PropertyController
        {
            public VectorPropertyController(IPluginHost host, string propertyName)
                : base(host, propertyName)
            {}

            public override void OnValueChanged(SourceGrid.CellContext sender, EventArgs e)
            {
                int row = sender.Position.Row;
                string id = (string)((SourceGrid.Grid)sender.Grid).Tag;
                
                // TODO: when changing the start frame if it's higher than the end, i should increase
                // the end by same amount, and hten change the min value
                // on the up/down for the end frame to always be >= to start up down.value
                if (mHost != null && (!(string.IsNullOrEmpty(id))))
                    mHost.Entity_SetCustomPropertyValue(id, mPropertyName, typeof(Keystone.Types.Vector3d).Name, (Keystone.Types.Vector3d)sender.Value);
            }
        }
        #endregion


        // TODO: ideally this belongs in our property_converter.cs and can be called by
        //       the script generator that creates the HTML for the interface. This generated HTML
        //       does not even need to go over the wire, its just done locally.  The code that generates
        //       was already downloaded by user by definition if they  have the component prefab!
        private string[] GetStandardValues(string converterTypeName)
        {
            if (string.IsNullOrEmpty(converterTypeName)) return null;

            string scriptPath = System.IO.Path.Combine(AppMain._core.ModsPath, @"common\scripts\property_converters.css");

            // http://en.wikipedia.org/wiki/Quantitative_easing
            
            object temp =
                AppMain._core.ScriptLoader.CreateObjectInstance(scriptPath, converterTypeName); // "TestConverter");

            // with the type converter loaded, get the standard values so we can populate a list
            if (temp == null) return null;

            try
            {
                System.ComponentModel.TypeConverter converter = (System.ComponentModel.TypeConverter)temp;

                System.Collections.ICollection o = converter.GetStandardValues();
                // we can use a drop down if the standard values exists
                if (o != null && o.Count > 0)
                {
                    string[] results = new string[o.Count];
                    IEnumerable<string> list = o.Cast<string>();
                    int i = 0;
                    foreach (string s in list)
                    {
                        //System.Diagnostics.Debug.WriteLine("QuickLook.GetStandardValues() - " + s);
                        results[i] = s; i++;
                    }

                    return results;
                }
            }
            catch { }

            return null;
        }
    }
}
