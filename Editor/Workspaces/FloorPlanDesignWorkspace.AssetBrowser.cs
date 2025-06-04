using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Keystone.Types;


namespace KeyEdit.Workspaces
{
    public partial class FloorPlanDesignWorkspace
    {
        DevComponents.DotNetBar.ButtonItem[] mButtons;
        DevComponents.DotNetBar.ComboBoxItem comboCategories;
        DevComponents.DotNetBar.ButtonItem previewButton;

        protected override void InitializeAssetBrowser()
        {
            mBrowser = new KeyEdit.GUI.AssetBrowserControl();
            
            mBrowser.ShowTreeView = false;
            mBrowser.RecurseSubFolders = false; // we do not want the imagebrowser recursing!
            mBrowser.AllowedExtensions = new string[] {".KGBENTITY", ".KGBSEGMENT"};

            // ModPath needs to be set after recurse = false or it will immediately start
            // searching before we have a chance to set .RecurseSubFolders = false
            mBrowser.BindToMod (AppMain.MOD_PATH,  AppMain.ModName);
            mBrowser.SelectedPath = "\\entities\\structure\\floors"; // default path is interior structure - floors

            mBrowser.OnAssetClicked += AssetSelected;
            mBrowser.OnEntryRenamed += EntryRenamed;
            mBrowser.QueryCustomImageForNonImage += OnNonImageFileFound;


            System.Windows.Forms.ImageList imglist = new System.Windows.Forms.ImageList();
            System.Resources.ResourceManager resourceManager = 
                new System.Resources.ResourceManager("KeyEdit.Properties.Resources", GetType().Assembly);
            imglist.Images.Add ("cert16", (System.Drawing.Bitmap)resourceManager.GetObject("cert_icon&16"));
            imglist.Images.Add ("eyeicon16", (System.Drawing.Bitmap)resourceManager.GetObject("eye_icon_16"));
            
            mBrowser.ImageList = imglist;
            
            comboCategories = new DevComponents.DotNetBar.ComboBoxItem();
            comboCategories.SelectedIndexChanged += OnComboCategories_SelectedIndexChanged;
            mBrowser.AddRibbonControl(comboCategories);

            previewButton = mBrowser.AddRibbonButton("Preview", "Preview", "Preview", "", 1, OnShowPreview_Click);

            PopulateCategories();
        }

        
        private void PopulateCategories()
        {

            comboCategories.Items.AddRange(new string[] 
            {"Structure", "Accommodations", "Crew Stations", "Electronics",
            "Plumbing", "Power", "Propulsion", "Storage", 
            "Vehicles", "Weapons", "Characters"});

            comboCategories.SelectedIndex = 0;
            comboCategories.DropDownWidth = 100;
            //System.Windows.Forms.TreeNode node;
            //node.ImageKey= "test";
            //node.SelectedImageKey = 
        }

        private void OnComboCategories_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (sender is DevComponents.DotNetBar.ComboBoxItem)
            {
                string category = (sender as DevComponents.DotNetBar.ComboBoxItem).SelectedItem.ToString();

                switch (category)
                {
                    case "Structure":
                        ClearRibbonBarButtons(mButtons);
                        mButtons = new DevComponents.DotNetBar.ButtonItem[5];
                        mButtons[0] = mBrowser.AddRibbonButton("Floors", "Floors", "Floors", "\\entities\\structure\\floors", 0, OnCategory_Click);
                        mButtons[1] = mBrowser.AddRibbonButton("Walls", "Walls", "Walls", "\\entities\\structure\\walls", 0, OnCategory_Click);
                        mButtons[2] = mBrowser.AddRibbonButton("Doors", "Doors", "Doors","\\entities\\structure\\doors", 0, OnCategory_Click);
                        mButtons[3] = mBrowser.AddRibbonButton("Access", "Access", "Access", "\\entities\\structure\\stairs & ladders", 0, OnCategory_Click);
                        mButtons[4] = mBrowser.AddRibbonButton("Access", "Access", "Hatches", "\\entities\\structure\\hatches", 0, OnCategory_Click);
                        
                        break;
                    case "Power":
                        ClearRibbonBarButtons(mButtons);
                        mButtons = new DevComponents.DotNetBar.ButtonItem[2];
                        mButtons[0] = mBrowser.AddRibbonButton("Reactors", "Reactors", "Reactors", "\\entities\\power", 0, OnCategory_Click);
                        mButtons[1] = mBrowser.AddRibbonButton("Batteries", "Batteries", "Batteries", "\\entities\\power\\batteries", 0, OnCategory_Click);
                        break;
                    case "Propulsion":
                        ClearRibbonBarButtons(mButtons);
                        mButtons = new DevComponents.DotNetBar.ButtonItem[2];
                        mButtons[0] = mBrowser.AddRibbonButton("Main Engines", "Main Engines", "Main Engines", "\\entities\\propulsion", 0, OnCategory_Click);
                        mButtons[1] = mBrowser.AddRibbonButton("Thrusters", "Thrusters", "Thrusters", "\\entities\\propulsion\\thrusters", 0, OnCategory_Click);
                        break;
                    case "Electronics":
                        ClearRibbonBarButtons(mButtons);
                        mButtons = new DevComponents.DotNetBar.ButtonItem[1];
                        mButtons[0] = mBrowser.AddRibbonButton("Electronics", "Electronics", "Electronics", "\\entities\\electronics", 0, OnCategory_Click);
                        break;
                    case "Accommodations":
                        ClearRibbonBarButtons(mButtons);
                        mButtons = new DevComponents.DotNetBar.ButtonItem[2];
                        mButtons[0] = mBrowser.AddRibbonButton("Beds", "Beds", "Beds", "\\entities\\accommodations", 0, OnCategory_Click);
                        mButtons[1] = mBrowser.AddRibbonButton("Stations", "Stations", "Stations", "\\entities\\stations", 0, OnCategory_Click);
                        break;
                    case "Crew Stations":
                        ClearRibbonBarButtons(mButtons);
                        mButtons = new DevComponents.DotNetBar.ButtonItem[1];
                        mButtons[0] = mBrowser.AddRibbonButton("Crew Stations", "Crew Stations", "Crew Stations", "\\entities\\crew stations", 0, OnCategory_Click);
                        break;
                    case "Storage":
                        ClearRibbonBarButtons(mButtons);
                        mButtons = new DevComponents.DotNetBar.ButtonItem[1];
                        mButtons[0] = mBrowser.AddRibbonButton("Crates", "Crates", "Crates", "\\entities\\storage", 0, OnCategory_Click);
                        break;
                    case "Weapons":
                        ClearRibbonBarButtons(mButtons);
                        mButtons = new DevComponents.DotNetBar.ButtonItem[1];
                        mButtons[0] = mBrowser.AddRibbonButton("Weapons", "Weapons", "Weapons", "\\entities\\weapons", 0, OnCategory_Click);
                        break;
                    case "Characters":
                        ClearRibbonBarButtons(mButtons);
                        mButtons = new DevComponents.DotNetBar.ButtonItem[1];
                        mButtons[0] = mBrowser.AddRibbonButton("Characters", "Characters", "Characters", "\\actors", 0, OnCategory_Click);
                        break;
                    default:
                        break;
                }

                mButtons[0].Checked = true;
            }
        }

        private void ClearRibbonBarButtons(DevComponents.DotNetBar.ButtonItem[] buttons)
        {
            if (buttons != null && buttons.Length > 0)
            {
                for (int i = 0; i < buttons.Length; i++)
                {
                    mBrowser.RemoveRibbonButton(buttons[i], OnCategory_Click);
                }
            }
        }

        private void UncheckAllButtonsExcept(DevComponents.DotNetBar.ButtonItem button)
        {
            for (int i = 0; i < mButtons.Length; i++)
                mButtons[i].Checked = false;

            button.Checked = true;
        }

        private void OnCategory_Click(object sender, EventArgs e)
        {
            string path = (sender as DevComponents.DotNetBar.ButtonItem).Tag.ToString();
            CategoryChanged(path);
            UncheckAllButtonsExcept((DevComponents.DotNetBar.ButtonItem)sender);
        }


        // TODO: how do we get our ViewportFloorplanControl toolbar selections to
        //       change our assetbrowser's bind?
        //       mViewportControls[0] 
        // from within the viewportFloorplanControl i can
        // this.Viewport.Context.Workspace.<-- let's say i cast for now
        //                                 and call a function that exists in this workspace
        //                                 and which will cause the bind of the browser
        //                                 furthermore, when in just say boundary painting
        //                                 i can make the asset browser gone so that panel is 
        //                                 just empty gray form.
        //                                 Indeed, we may even prefer that the toolbar 
        //                                 acts as our component selector instead of the one
        //                                 here... hrm... or... what if selection of categories
        //                                 can be notified here and then we can send commands to
        //                                 the image browser... yes lets try that

        // TODO: when we select the floor painting tool
        //       we want to re-bind the AssetBrowser to use
        //       the atlas.  for now we'll hardcode single atlas that is
        //       .tai file on disk
        // string atlasResourceDescriptor;
        // BrowserAtlasController controller = new BrowserAtlasController(atlasResourceDescriptor);
        // mBrowser.BindToCustom (controller);
        // - these atlas resources then when selected while the floor paint brush is active
        //   will cause that sub-texture index to be current.
        //   - note: imagebrowser "ShowFileTree" must be false when browsing atlas
        //   - note: in future, atlas .tai file may show up in filetree and be excluded from imagebrowser
        //      - TODO: we can use a custom folder icon for atlases in the filetree
        public delegate void CategoryChangedHandler(string category);

        // this function can also be called by the ViewportFloorplanControl 
        private void CategoryChanged(string categoryPath)
        {
            if (categoryPath == "\\entities\\structure\\floors")
            {
                KeyCommon.IO.ResourceDescriptor atlasResourceDescriptor =
                    new KeyCommon.IO.ResourceDescriptor(System.IO.Path.Combine (AppMain.MOD_PATH,  @"caesar\textures\floors\flooratlas.tai"));
                
                ImageBrowserDotnet.BrowserAtlasController controller =
                    new ImageBrowserDotnet.BrowserAtlasController(atlasResourceDescriptor.ToString());

                // TODO: let's see if we can trigger our BrowserAtlasController to 
                // try to handle the adding of images contained in the atlas
                mBrowser.BindToCustom(controller);

                mBrowser.SelectedPath = atlasResourceDescriptor.ToString();
            }
            else
            {
                mBrowser.BindToMod(AppMain.MOD_PATH, AppMain.ModName);
                mBrowser.SelectedPath = categoryPath;
            }
        }

        private void ShutdownPreview()
        {
            // TODO: do not allow this preview to stay alive
        }
    }
}
