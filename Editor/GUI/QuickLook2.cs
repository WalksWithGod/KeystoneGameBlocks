/*
 * Created by SharpDevelop.
 * User: Hypnotron
 * Date: 11/13/2013
 * Time: 5:05 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Antlr4.StringTemplate;
using Antlr4.StringTemplate.Compiler;
using KeyEdit.Properties;
using KeyPlugins;
using WebKit;

namespace KeyEdit.GUI
{
        
	/// <summary>
	/// Description of Form1.
	/// </summary>
	public partial class QuickLook2 : WebKit.WebKitBrowser
	{
		
		Timer mTestTimer;
		Timer mTestTimer2;

    	protected KeyEdit.PluginHost.EditorHost mHost;
    	protected string mTargetEntityID;
    
		
    	public QuickLook2()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}
    	
    	public QuickLook2 (KeyEdit.PluginHost.EditorHost scriptingHost) : this()
        {
            if (scriptingHost == null) throw new ArgumentNullException();
            mHost = scriptingHost;

          
			
			
            // disable automatic double buffering since we'll be implementing our own
            this.DoubleBuffered = true;
    		//this.SetStyle(ControlStyles.UserPaint, false);
    		this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
    		//this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			//this.SetStyle(ControlStyles.Opaque, true); // eliminates any background drawing. Depending on your application you may want a background.
    		
			// create a buffered graphics context to draw to
    		// http://stackoverflow.com/questions/76651/dirty-rectangles

    		
    		// TIMERS - HTTP Is a request driven protocol so client must request
    		//          user interface data update.
    		
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
            
            mTestTimer2 = new Timer ();
            mTestTimer2.Interval = 500;
            mTestTimer2.Tick += Timer2_OnTick;
            mTestTimer2.Start ();
            
            
            // wire up events
   			this.DocumentCompleted += OnDocumentCompleted;    
            // this.Click +=
            // this.ChangeUICues +=
            // this.CursorChanged+= 
  
      //      this.ImageLoad += OnImageLoad;
      //      this.LinkClicked += OnLinkClicked;
      //      this.LinkMouseEnter += OnLinkMouseEnter;
      //      this.LinkMouseLeave += OnLinkMouseLeave;
      //      this.RenderError += OnRenderError;
      //      this.StylesheetLoad += OnStyleSheetLoad;
			// this.FontChanged += 
			
			

        }

        private int mCounter = 0;
         private int mCounter2 = 228;
         private object mTimerSynch = new object();
         
        private void Timer_OnTick (object sender, EventArgs e)
        {
        	// todo: do we have a previousHTMLPanel to compare for dirty rects?
        	//       because we need to be able to find 
        	//       a difference between first hierarchy of CssBox and the second
        	//       and if the essential boxes are same but with different text for instance
        	//       then we should clip just the modified boxes and then redraw
        	//
        	//this.Text = html;
        	mCounter ++;
    //    	CreateHTML();
        }
        
        
        
        private void Timer2_OnTick (object sender, EventArgs e)
        {
        	// todo: do we have a previousHTMLPanel to compare for dirty rects?
        	//       because we need to be able to find 
        	//       a difference between first hierarchy of CssBox and the second
        	//       and if the essential boxes are same but with different text for instance
        	//       then we should clip just the modified boxes and then redraw
        	//
        	//this.Text = html;
        	mCounter2 ++;
    //    	CreateHTML();
        	
        	// todo: the way to do this with webkit may be to directly modify the DOM
        	//       for those elements we want to change.  So maybe find an element with a particular
        	//       id and change it's value using value of mCounter2
        		
        	this.Url = null;
        	
        	WebKit.DOM.NodeList nlist = this.Document.GetElementsByTagName("li");
     	
        	foreach (WebKit.DOM.Node node in nlist) 
        	{
				//node.NodeValue = mCounter2.ToString();
 				
 					//node.InvokeMember("click");
 				
 			}
        	// http://stackoverflow.com/questions/5185266/reading-links-in-header-using-webkit-net
        	// http://anychart.com/products/stock/online-demos/html-js-samples-center/data-streaming/index.html

        	WebKit.DOM.Element element = this.Document.GetElementById ("test123");
        	if (element != null)
        	{
        		// todo: attempting to modify this value is not working.  
        		
        		element.NodeValue = mCounter2.ToString ();
        	  
        		// https://rniwa.com/2013-02-10/live-nodelist-and-htmlcollection-in-webkit/
        		WebKit.DOM.Node parent = element.ParentNode ;
        		WebKit.DOM.Node modified = this.Document.CreateElement ("li");
        		WebKit.DOM.Attr idAttribute = this.Document.CreateAttribute ("id");
        		idAttribute.NodeValue = "test123";
     //   		modified.AppendChild (idAttribute );
        		
        		//modified = this.Document.CreateTextNode (mCounter2.ToString());;
        		modified.NodeValue = mCounter2.ToString();
   //     		this.Stop ();
        		//modified.NodeValue = mCounter.ToString();
        		//this.Document.ReplaceChild (modified, element);
        		//this.Document.InsertBefore (modified, element);
    //    		parent.RemoveChild (element);
     //   		parent.AppendChild (modified);
        	}
        	string value = element.GetAttribute ("data-target");
        	if (!string.IsNullOrEmpty (value))
    	    {
        		
    	    }
        	
        	
        	
        	// recursively assign events to all all nodes that are clickable
    //    	WebKit.DOM.NodeList nodes = this.Document.ChildNodes;
    //        foreach (WebKit.DOM.Node node in nodes)
    //        {
    //            node.MouseDown += new EventHandler(browser_DomMouseClick);
    //        }
        

        }
        
        private void OnDocumentCompleted (object sender, WebBrowserDocumentCompletedEventArgs args)
        {
        	
        }

		
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
            CreateHTML();
        }

        // todo: find out when select occurs and then after the call is made, have a subscription
        // made on behalf of this quicklook so that it is received updates for this particular entity
        // from this scene
        public virtual void Select(string sceneName, string entityID, string entityTypeName)
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

            // obsolete - grid replaced by html layout?
            //grid.Tag = mTargetEntityID;
            //PopulateGrid(customProperties); // obsolete - HTML Panel used in place of grid
            CreateHTML();
            
            
        }

        private void CreateHTML()
        {
        	        	
            // http://www.antlr.org/wiki/display/ST/Examples
            // http://www.antlr.org/wiki/display/ST/Five+minute+Introduction
   			// TEMP: comment out and test locally         string html = mHost.Entity_GetGUILayout(entityID);

            string html = TemplateTest(null, mTargetEntityID, null);
            try
            {
            	// in the original panel code, assigning new text will always change the VerticalScroll value back to the minimum value.
            	// todo: i suspect that we should not allow painting to be done while we are reconstructing the HTML 
            	lock(mTimerSynch)
            	{
            		DrawingControl.SuspendDrawing (this);
            		//this.SetHtml (html, false);
            		this.DocumentText = html;
            		// todo: how can i change the Url without then replacing the .DocumentText?  id have to 
            		// write out the html that we generated with StringTemplate and then assign that as the Url
            		this.Url = new Uri ("file:///c:/");
            		DrawingControl.ResumeDrawing (this);
            	}
            }
            catch (Exception ex) 
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            

        }
        
        struct generator
        {
            public string Name; // friendly name for title
            public string ID;
            public double Output; // watts?  which we can convert to kilowatts during display // a car battery will produce about 500 Watts before draining completely
            public double Load;
        }

        struct consumer
		{
			public string id;
			public string name; // we will fill with typename for testing
		}

        // http://www.antlr.org/wiki/display/ST4/StringTemplate+4+Wiki+Home
        // http://www.cs.usfca.edu/~parrt/course/601/lectures/stringtemplate.html

        // these scripts are run from \\E:\dev\c#\KeystoneGameBlocks\KeyScript\bin\x86\Debug_x86
        // but why?  in our shipped app surely the only \bin folder will be the main where client exe sits

        // so i think that is the default search directory
        // but forgetting about that for now, how do we handle localization?
        // we might want templates that are localized... 
        // 
        // todo: do we support dynamic updates to the templates?  good for debugging but not good
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
        // todo: if these templates are inside of a zip, i should be trying to load these templates via an API call
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
        private string TemplateTest(string sceneName, string entityID, string entityTypename)
        {
             // todo: remember this is normally occuring in script only here we'd use EntityAPI to retrieve properties
            //Settings.PropertySpec[] customProperties = mHost.Entity_GetCustomProperties(sceneName, entityID, entityTypename);


            string name= "Titan" ; // "2.5 MW Fission Reactor"; // (string)mHost.Entity_GetCustomPropertyValue(entityID, "fuel type") + " " +
                            // (string)mHost.Entity_GetCustomPropertyValue(entityID, "description");
                            
            return PowerHTMLGen(entityID, name); 
            return StarHTMLGen (entityID);
			return WorldHTMLGen (entityID);
        }
        
        private string PowerHTMLGen(string entityID, string name)
        {

            // todo: if the entity we've loaded is not of this type, 
            // we will not be able to find these custom properties
            // but in practice this is never a problem because all of this
            // HTML generation is within the specific entity's script that uses it!
            // so excluding typeos/bugs, its impossible to query a custom property's value that
            // does not exist.
            generator g;
            g.ID = entityID;
            g.Output = 150000; // (double)mHost.Entity_GetCustomPropertyValue(entityID, "thrust");
            g.Load = 0.0; //
            g.Name = name;
            
            //g.CoreTemp

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
            
		    consumer[] array = new consumer[2];
            array[0].id = "0";
            array[0].name = "Counter = " + mCounter.ToString();
            array[1].id = "1";
            array[1].name = "Counter = " + mCounter2.ToString();

            string prefix = null;
    			
            // todo: seems most of the problems im having result from not loading these damn
            // .st files from disk.  Trying to load them from string seems to prevent from assigning a name
            // to the template.  it's quite bizarre
            
            // collapsable lists in html?
            // http://stackoverflow.com/questions/15095933/pure-css-collapse-expand-div

            //NOTE: must use RawGroupDirectory if we just want straight template text with no type definition info required
            TemplateRawGroupDirectory group = new TemplateRawGroupDirectory(@"E:\dev\c#\KeystoneGameBlocks\Data\mods\common\layouts\", Encoding.ASCII, '$', '$');
        //    group.LoadGroupFile(prefix, @"E:\dev\c#\KeystoneGameBlocks\Data\mods\common\layouts\group_components.stg"); 
        //    group.LoadTemplateFile(prefix, "component_page.st");
        //    group.LoadTemplateFile(prefix, "power_gen_body.st");
        //    group.LoadTemplateFile(prefix, "power_gen_consumers.st");
        //    group.LoadTemplateFile(prefix, "power_gen_consumers_row.st");

            // note: the problem with TemplateGroup (as opposed to GroupDirectory) is you cannot supply
            // names to the templates you load.  Names are always based on a filename.
		    //TemplateGroup group = new TemplateGroup('$', '$');
            //group.LoadGroupFile ("testgroup", @"E:\dev\c#\KeystoneGameBlocks\Data\mods\common\layouts"); 
            
            // these effectively re-read the file everytime and allows runtime modification
            Template powerGeneratorST = group.GetInstanceOf("component_page");
            Template bodyST = group.GetInstanceOf("world_body"); //power_gen_body");
            Template consumerListST = group.GetInstanceOf("power_gen_consumers");
            

            // .Add cannot be done on compiled templates.  CompiledTemplates are effectively frozen so it makes sense!
            bodyST.Add("componenttypename", g.Name);
            powerGeneratorST.Add("title", "Component Page");
            powerGeneratorST.Add("body", bodyST);
            
            
            consumerListST.Add("consumers", array);
            bodyST.Add("consumertable", consumerListST);
            
            return powerGeneratorST.Render();
        }

        private string StarHTMLGen(string entityID)
        {
            return null;
        }

        private string WorldHTMLGen(string entityID)
        {
            // we mostly want to see habital zone and mineral info
            // and if applicable, culture and life.
            // http://www.youtube.com/watch?v=JZHW5wbys3M

            // we will only show orbital info, mass, eccentricity and all this other
            // stuff as "more" popup

            return null;
        }

        #region HTMLRenderer
//        /// <summary>
//        /// Handle stylesheet resolve.
//        /// </summary>
//        private void OnStyleSheetLoad(object sender, HtmlStylesheetLoadEventArgs e)
//        {
//            var stylesheet = GetStyleSheet(e.Src);
//            if (stylesheet != null)
//                e.SetStyleSheet = stylesheet;
//        }
//
//        private void OnImageLoad(object sender, HtmlImageLoadEventArgs e)
//        {
//            // load the image from the embedded resource if it is an embedded resource...
//            // it might not be!  it might want to convert it to a localized path if available.
//            Image img = TryLoadResourceImage(e.Src);
//
//            Point p = new Point (0, 0);
//            Rectangle r = new Rectangle(p, img.Size);
//            if (img != null)
//                e.Callback(img, r);
//        }

        private string GetStyleSheet (string src)
        {
            if (src.ToLower() == "styles/style0.css")
            {
                string path = @"E:\dev\c#\KeystoneGameBlocks\Data\mods\common\layouts\styles\style3.css";
                return System.IO.File.ReadAllText(path);
            }

            return "";
        }

        /// <summary>
        /// Get image by resource key.
        /// </summary>
        private Image TryLoadResourceImage(string src)
        {
        	// todo: any cache should be done here?
        	// todo: why aren't fonts loaded this way as well?
            switch (src.ToLower())
            {
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
//
//        /// <summary>
//        /// Show error raised from html renderer.
//        /// </summary>
//        private void OnRenderError(object sender, HtmlRenderErrorEventArgs e)
//        {
//            MessageBox.Show(e.Message + (e.Exception != null ? "\r\n" + e.Exception : null), "Error in Html Renderer", MessageBoxButtons.OK);
//        }
//
//        private void OnLinkMouseEnter(object sender, HtmlLinkClickedEventArgs e)
//        {
//        	System.Diagnostics.Debug.WriteLine ("QuickLook.OnLinkMouseEnter() - ");
//        }
//                
//        private void OnLinkMouseLeave(object sender, HtmlLinkClickedEventArgs e)
//        {
//        	System.Diagnostics.Debug.WriteLine ("QuickLook.OnLinkMouseLeave() - ");
//        }
                

//        /// <summary>
//        /// On specific link click handle it here. NOTE: links can contain images
//        /// </summary>
//        private void OnLinkClicked(object sender, HtmlLinkClickedEventArgs e)
//        {
//            // link with image is easy
//            // http://www.w3schools.com/html/tryit.asp?filename=tryhtml_imglink
//
//            // NOTE: We do not allow instancing of entities here or references to them from
//            // Repository.  This must go through the scripting host.
//            // The scripting host will in turn send the request to the server if the item
//            // is not cached in Repository.  The server will determine the validity of the request
//            // and send back any properties.
//            
//
//            if (e.Link == "#")
//            {
//            	string handler = null;
//            	bool hasHandler = e.Attributes.TryGetValue("data-handler", out handler);
//            
//	            if (hasHandler)
//	            {
//	            	string id = null;
//	            	bool hasID = e.Attributes.TryGetValue ("id", out id);
//	                string scriptedMethodName = handler;
//	                switch (handler)
//	                {
//	                	case "GUILayout_LinkClick":
//	                		break;
//	                	case "GUILayout_ButtonClick":
//	                		break;
//	                	default:
//	                		break;
//	                }
//	                // todo: verify i can a custom attribute "handler" to a link ref so i can
//	                // use it to store the method name to call in the entity's script
//	                mHost.Entity_GUILinkClicked(mTargetEntityID, scriptedMethodName, id);
//	                e.Handled = true;
//	                
//	            }
//           }
//           else // default
//           {
//           		// we should typically always have e.Handled = true; else HtmlContainer will try to handle it with a processInfo run attempt
//           		e.Handled = true;
//           }
//        }
        #endregion
        
        
        // todo: ideally this belongs in our property_converter.cs and can be called by
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
