using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using DevComponents.DotNetBar;
using DevComponents.DotNetBar.Rendering;
using KeyEdit.GUI;

namespace KeyEdit
{
    /// <summary>
    /// Summary description for Form1.
    /// </summary>
    partial class FormMain : FormMainBase 
    {
        private System.ComponentModel.IContainer components;
        private DevComponents.DotNetBar.ButtonItem buttonSave;
        private DevComponents.DotNetBar.ButtonItem buttonNew;
        private DevComponents.DotNetBar.ButtonItem buttonHelp;
        private DevComponents.DotNetBar.SuperTooltip superTooltip1;
        private DevComponents.DotNetBar.ButtonItem buttonItemNew;
        private DevComponents.DotNetBar.ButtonItem buttonItemLoad;
        private DevComponents.DotNetBar.ButtonItem buttonSettings;
        private DevComponents.DotNetBar.ButtonItem buttonCloseScene;
        private DevComponents.DotNetBar.LabelItem labelRecentProjects;
        private DevComponents.DotNetBar.ItemContainer menuFileContainer;
        private DevComponents.DotNetBar.ItemContainer menuFileTwoColumnContainer;
        private DevComponents.DotNetBar.ItemContainer menuFileItems;
        private DevComponents.DotNetBar.ItemContainer menuFileMRU;
        private DevComponents.DotNetBar.ItemContainer menuFileBottomContainer;
        private DevComponents.DotNetBar.ButtonItem buttonOptions;
        private DevComponents.DotNetBar.ButtonItem buttonExit;
        private DevComponents.DotNetBar.ContextMenuBar contextMenuBar1;
        private Command AppCommandTheme;
        private DevComponents.DotNetBar.StyleManager styleManager1;
        private DevComponents.DotNetBar.ButtonItem buttonStyleOffice2010Silver;
        private DevComponents.DotNetBar.ButtonItem buttonCredits;
        private ButtonItem buttonStyleOffice2010Blue;
        private DockSite dockSite2;
        private DockSite dockSite1;
        private DockSite dockSite3;
        private DockSite dockSite4;
        private DockSite dockSite5;
        private DockSite dockSite6;
        private DockSite dockSite7;
        private DockSite dockSite8;
        private DockSite dockSite9;
        private DotNetBarManager dotNetBarManager;
        private Bar barDocumentDockBar;
        private DevComponents.DotNetBar.Office2007StartButton buttonBegin;


        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            DevComponents.DotNetBar.SuperTooltipInfo superTooltipInfo2 = new DevComponents.DotNetBar.SuperTooltipInfo();
            DevComponents.DotNetBar.SuperTooltipInfo superTooltipInfo1 = new DevComponents.DotNetBar.SuperTooltipInfo();
            this.buttonItemResume = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemEdit = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemEditScene = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemEditPrefab = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemEditMission = new DevComponents.DotNetBar.ButtonItem();
            this.ribbon = new DevComponents.DotNetBar.RibbonControl();
            this.contextMenuBar1 = new DevComponents.DotNetBar.ContextMenuBar();
            this.buttonBegin = new DevComponents.DotNetBar.Office2007StartButton();
            this.menuFileContainer = new DevComponents.DotNetBar.ItemContainer();
            this.menuFileTwoColumnContainer = new DevComponents.DotNetBar.ItemContainer();
            this.menuFileItems = new DevComponents.DotNetBar.ItemContainer();
            this.buttonItemNew = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemNewOutdoorScene = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemNewSingleRegion = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemNewCampaign = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemNewMultiRegionScene = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemLoad = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemLoadMission = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemSave = new DevComponents.DotNetBar.ButtonItem();
            this.buttonCloseScene = new DevComponents.DotNetBar.ButtonItem();
            this.buttonSettings = new DevComponents.DotNetBar.ButtonItem();
            this.menuFileMRU = new DevComponents.DotNetBar.ItemContainer();
            this.labelRecentProjects = new DevComponents.DotNetBar.LabelItem();
            this.menuFileBottomContainer = new DevComponents.DotNetBar.ItemContainer();
            this.buttonOptions = new DevComponents.DotNetBar.ButtonItem();
            this.buttonExit = new DevComponents.DotNetBar.ButtonItem();
            this.buttonViews = new DevComponents.DotNetBar.ButtonItem();
            this.buttonFloorplanEditor = new DevComponents.DotNetBar.ButtonItem();
            this.buttonHardpointEditor = new DevComponents.DotNetBar.ButtonItem();
            this.buttonComponentEditor = new DevComponents.DotNetBar.ButtonItem();
            this.buttonTextureEditor = new DevComponents.DotNetBar.ButtonItem();
            this.buttonBehaviorTree = new DevComponents.DotNetBar.ButtonItem();
            this.buttonCodeEditor = new DevComponents.DotNetBar.ButtonItem();
            this.buttonSimulation = new DevComponents.DotNetBar.ButtonItem();
            this.buttonAdministration = new DevComponents.DotNetBar.ButtonItem();
            this.buttonNavigation = new DevComponents.DotNetBar.ButtonItem();
            this.buttonShipSystems = new DevComponents.DotNetBar.ButtonItem();
            this.buttonSecurity = new DevComponents.DotNetBar.ButtonItem();
            this.buttonCommunications = new DevComponents.DotNetBar.ButtonItem();
            this.buttonCnC = new DevComponents.DotNetBar.ButtonItem();
            this.buttonOperations = new DevComponents.DotNetBar.ButtonItem();
            this.buttonFleet = new DevComponents.DotNetBar.ButtonItem();
            this.buttonHelp = new DevComponents.DotNetBar.ButtonItem();
            this.buttonStyleOffice2010Blue = new DevComponents.DotNetBar.ButtonItem();
            this.AppCommandTheme = new DevComponents.DotNetBar.Command(this.components);
            this.buttonStyleOffice2010Silver = new DevComponents.DotNetBar.ButtonItem();
            this.buttonCredits = new DevComponents.DotNetBar.ButtonItem();
            this.buttonCommunityForums = new DevComponents.DotNetBar.ButtonItem();
            this.buttonNew = new DevComponents.DotNetBar.ButtonItem();
            this.buttonSave = new DevComponents.DotNetBar.ButtonItem();
            this.superTooltip1 = new DevComponents.DotNetBar.SuperTooltip();
            this.styleManager1 = new DevComponents.DotNetBar.StyleManager(this.components);
            this.dotNetBarManager = new DevComponents.DotNetBar.DotNetBarManager(this.components);
            this.dockSite4 = new DevComponents.DotNetBar.DockSite();
            this.dockSite9 = new DevComponents.DotNetBar.DockSite();
            this.barDocumentDockBar = new DevComponents.DotNetBar.Bar();
            this.dockSite1 = new DevComponents.DotNetBar.DockSite();
            this.dockSite2 = new DevComponents.DotNetBar.DockSite();
            this.dockSite8 = new DevComponents.DotNetBar.DockSite();
            this.dockSite5 = new DevComponents.DotNetBar.DockSite();
            this.dockSite6 = new DevComponents.DotNetBar.DockSite();
            this.dockSite7 = new DevComponents.DotNetBar.DockSite();
            this.dockSite3 = new DevComponents.DotNetBar.DockSite();
            ((System.ComponentModel.ISupportInitialize)(this.contextMenuBar1)).BeginInit();
            this.dockSite9.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.barDocumentDockBar)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonItemResume
            // 
            this.buttonItemResume.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItemResume.Image = global::KeyEdit.Properties.Resources.target_icon_16;
            this.buttonItemResume.Name = "buttonItemResume";
            this.buttonItemResume.Text = "&Resume";
            this.buttonItemResume.Click += new System.EventHandler(this.buttonItemResume_Click);
            // 
            // buttonItemEdit
            // 
            this.buttonItemEdit.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItemEdit.Image = global::KeyEdit.Properties.Resources.target_icon_16;
            this.buttonItemEdit.Name = "buttonItemEdit";
            this.buttonItemEdit.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonItemEditScene,
            this.buttonItemEditPrefab,
            this.buttonItemEditMission});
            this.buttonItemEdit.SubItemsExpandWidth = 24;
            this.buttonItemEdit.Text = "&Edit";
            // 
            // buttonItemEditScene
            // 
            this.buttonItemEditScene.Name = "buttonItemEditScene";
            this.buttonItemEditScene.Text = "Edit Scene";
            this.buttonItemEditScene.Click += new System.EventHandler(this.buttonItemEditSceneClick);
            // 
            // buttonItemEditPrefab
            // 
            this.buttonItemEditPrefab.Name = "buttonItemEditPrefab";
            this.buttonItemEditPrefab.Text = "Edit Prefab";
            this.buttonItemEditPrefab.Click += new System.EventHandler(this.buttonItemEditPrefabClick);
            // 
            // buttonItemEditMission
            // 
            this.buttonItemEditMission.Name = "buttonItemEditMission";
            this.buttonItemEditMission.Text = "Edit Mission";
            this.buttonItemEditMission.Click += new System.EventHandler(this.buttonItemEditMissionClick);
            // 
            // ribbon
            // 
            this.ribbon.BackColor = System.Drawing.SystemColors.Control;
            // 
            // 
            // 
            this.ribbon.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbon.CaptionVisible = true;
            this.ribbon.Dock = System.Windows.Forms.DockStyle.Top;
            this.ribbon.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ribbon.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ribbon.GlobalContextMenuBar = this.contextMenuBar1;
            this.ribbon.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonBegin,
            this.buttonViews,
            this.buttonHelp});
            this.ribbon.KeyTipsFont = new System.Drawing.Font("Tahoma", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ribbon.Location = new System.Drawing.Point(5, 1);
            this.ribbon.MdiSystemItemVisible = false;
            this.ribbon.Name = "ribbon";
            this.ribbon.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.ribbon.Size = new System.Drawing.Size(1249, 57);
            this.ribbon.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ribbon.SystemText.MaximizeRibbonText = "&Maximize the Ribbon";
            this.ribbon.SystemText.MinimizeRibbonText = "Mi&nimize the Ribbon";
            this.ribbon.SystemText.QatAddItemText = "&Add to Quick Access Toolbar";
            this.ribbon.SystemText.QatCustomizeMenuLabel = "<b>Customize Quick Access Toolbar</b>";
            this.ribbon.SystemText.QatCustomizeText = "&Customize Quick Access Toolbar...";
            this.ribbon.SystemText.QatDialogAddButton = "&Add >>";
            this.ribbon.SystemText.QatDialogCancelButton = "Cancel";
            this.ribbon.SystemText.QatDialogCaption = "Customize Quick Access Toolbar";
            this.ribbon.SystemText.QatDialogCategoriesLabel = "&Choose commands from:";
            this.ribbon.SystemText.QatDialogOkButton = "OK";
            this.ribbon.SystemText.QatDialogPlacementCheckbox = "&Place Quick Access Toolbar below the Ribbon";
            this.ribbon.SystemText.QatDialogRemoveButton = "&Remove";
            this.ribbon.SystemText.QatPlaceAboveRibbonText = "&Place Quick Access Toolbar above the Ribbon";
            this.ribbon.SystemText.QatPlaceBelowRibbonText = "&Place Quick Access Toolbar below the Ribbon";
            this.ribbon.SystemText.QatRemoveItemText = "&Remove from Quick Access Toolbar";
            this.ribbon.TabGroupHeight = 14;
            this.ribbon.TabIndex = 8;
            // 
            // contextMenuBar1
            // 
            this.contextMenuBar1.ColorScheme.PredefinedColorScheme = DevComponents.DotNetBar.ePredefinedColorScheme.Blue2003;
            this.contextMenuBar1.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.contextMenuBar1.IsMaximized = false;
            this.contextMenuBar1.Location = new System.Drawing.Point(352, 309);
            this.contextMenuBar1.Name = "contextMenuBar1";
            this.contextMenuBar1.Size = new System.Drawing.Size(150, 25);
            this.contextMenuBar1.Stretch = true;
            this.contextMenuBar1.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.contextMenuBar1.TabIndex = 13;
            this.contextMenuBar1.TabStop = false;
            this.contextMenuBar1.WrapItemsDock = true;
            // 
            // buttonBegin
            // 
            this.buttonBegin.AutoExpandOnClick = true;
            this.buttonBegin.CanCustomize = false;
            this.buttonBegin.HotTrackingStyle = DevComponents.DotNetBar.eHotTrackingStyle.Image;
            this.buttonBegin.Image = ((System.Drawing.Image)(resources.GetObject("buttonBegin.Image")));
            this.buttonBegin.ImageFixedSize = new System.Drawing.Size(16, 16);
            this.buttonBegin.ImagePaddingHorizontal = 0;
            this.buttonBegin.ImagePaddingVertical = 0;
            this.buttonBegin.Name = "buttonBegin";
            this.buttonBegin.ShowSubItems = false;
            this.buttonBegin.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.menuFileContainer});
            this.buttonBegin.Text = "&Begin";
            // 
            // menuFileContainer
            // 
            // 
            // 
            // 
            this.menuFileContainer.BackgroundStyle.Class = "RibbonFileMenuContainer";
            this.menuFileContainer.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.menuFileContainer.LayoutOrientation = DevComponents.DotNetBar.eOrientation.Vertical;
            this.menuFileContainer.Name = "menuFileContainer";
            this.menuFileContainer.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.menuFileTwoColumnContainer,
            this.menuFileBottomContainer});
            // 
            // 
            // 
            this.menuFileContainer.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // menuFileTwoColumnContainer
            // 
            // 
            // 
            // 
            this.menuFileTwoColumnContainer.BackgroundStyle.Class = "RibbonFileMenuTwoColumnContainer";
            this.menuFileTwoColumnContainer.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.menuFileTwoColumnContainer.BackgroundStyle.PaddingBottom = 2;
            this.menuFileTwoColumnContainer.BackgroundStyle.PaddingLeft = 2;
            this.menuFileTwoColumnContainer.BackgroundStyle.PaddingRight = 2;
            this.menuFileTwoColumnContainer.BackgroundStyle.PaddingTop = 2;
            this.menuFileTwoColumnContainer.ItemSpacing = 0;
            this.menuFileTwoColumnContainer.Name = "menuFileTwoColumnContainer";
            this.menuFileTwoColumnContainer.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.menuFileItems,
            this.menuFileMRU});
            // 
            // 
            // 
            this.menuFileTwoColumnContainer.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // menuFileItems
            // 
            // 
            // 
            // 
            this.menuFileItems.BackgroundStyle.Class = "RibbonFileMenuColumnOneContainer";
            this.menuFileItems.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.menuFileItems.ItemSpacing = 5;
            this.menuFileItems.LayoutOrientation = DevComponents.DotNetBar.eOrientation.Vertical;
            this.menuFileItems.MinimumSize = new System.Drawing.Size(120, 0);
            this.menuFileItems.Name = "menuFileItems";
            this.menuFileItems.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonItemNew,
            this.buttonItemEdit,
            this.buttonItemResume,
            this.buttonItemLoad,
            this.buttonItemSave,
            this.buttonCloseScene,
            this.buttonSettings});
            // 
            // 
            // 
            this.menuFileItems.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // buttonItemNew
            // 
            this.buttonItemNew.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItemNew.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemNew.Image")));
            this.buttonItemNew.ImageSmall = ((System.Drawing.Image)(resources.GetObject("buttonItemNew.ImageSmall")));
            this.buttonItemNew.Name = "buttonItemNew";
            this.buttonItemNew.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonItemNewOutdoorScene,
            this.buttonItemNewSingleRegion,
            this.buttonItemNewCampaign,
            this.buttonItemNewMultiRegionScene});
            this.buttonItemNew.SubItemsExpandWidth = 24;
            this.buttonItemNew.Text = "&New";
            this.buttonItemNew.Click += new System.EventHandler(this.buttonItemNew_Click);
            // 
            // buttonItemNewOutdoorScene
            // 
            this.buttonItemNewOutdoorScene.Name = "buttonItemNewOutdoorScene";
            this.buttonItemNewOutdoorScene.Text = "Outdoor Terrain Scene";
            this.buttonItemNewOutdoorScene.Click += new System.EventHandler(this.buttonItemNewOutdoorSceneClick);
            // 
            // buttonItemNewSingleRegion
            // 
            this.buttonItemNewSingleRegion.Name = "buttonItemNewSingleRegion";
            this.buttonItemNewSingleRegion.Text = "Single Region Scene";
            this.buttonItemNewSingleRegion.Click += new System.EventHandler(this.buttonItemNewSingleRegion_Click);
            // 
            // buttonItemNewCampaign
            // 
            this.buttonItemNewCampaign.Name = "buttonItemNewCampaign";
            this.buttonItemNewCampaign.Text = "Campaign";
            this.buttonItemNewCampaign.Click += new System.EventHandler(this.buttonItemNewCampaignClick);
            // 
            // buttonItemNewMultiRegionScene
            // 
            this.buttonItemNewMultiRegionScene.Name = "buttonItemNewMultiRegionScene";
            this.buttonItemNewMultiRegionScene.Text = "Multi Region Scene";
            this.buttonItemNewMultiRegionScene.Click += new System.EventHandler(this.buttonItemNewMultiRegionScene_Click);
            // 
            // buttonItemLoad
            // 
            this.buttonItemLoad.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItemLoad.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemLoad.Image")));
            this.buttonItemLoad.Name = "buttonItemLoad";
            this.buttonItemLoad.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonItemLoadMission});
            this.buttonItemLoad.SubItemsExpandWidth = 24;
            this.buttonItemLoad.Text = "&Load";
            this.buttonItemLoad.Click += new System.EventHandler(this.buttonLoad_Click);
            // 
            // buttonItemLoadMission
            // 
            this.buttonItemLoadMission.Name = "buttonItemLoadMission";
            this.buttonItemLoadMission.Text = "Load Mission";
            this.buttonItemLoadMission.Click += new System.EventHandler(this.buttonItemLoadMissionClick);
            // 
            // buttonItemSave
            // 
            this.buttonItemSave.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItemSave.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemSave.Image")));
            this.buttonItemSave.ImageSmall = ((System.Drawing.Image)(resources.GetObject("buttonItemSave.ImageSmall")));
            this.buttonItemSave.Name = "buttonItemSave";
            this.buttonItemSave.Text = "&Save";
            this.buttonItemSave.Click += new System.EventHandler(this.buttonItemSave_Click);
            // 
            // buttonCloseScene
            // 
            this.buttonCloseScene.BeginGroup = true;
            this.buttonCloseScene.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonCloseScene.Image = ((System.Drawing.Image)(resources.GetObject("buttonCloseScene.Image")));
            this.buttonCloseScene.Name = "buttonCloseScene";
            this.buttonCloseScene.SubItemsExpandWidth = 24;
            this.buttonCloseScene.Text = "&Close";
            this.buttonCloseScene.Click += new System.EventHandler(this.buttonCloseScene_Click);
            // 
            // buttonSettings
            // 
            this.buttonSettings.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonSettings.Image = global::KeyEdit.Properties.Resources.search;
            this.buttonSettings.Name = "buttonSettings";
            this.buttonSettings.Text = "Settings";
            this.buttonSettings.Click += new System.EventHandler(this.buttonSettings_Click);
            // 
            // menuFileMRU
            // 
            // 
            // 
            // 
            this.menuFileMRU.BackgroundStyle.Class = "RibbonFileMenuColumnTwoContainer";
            this.menuFileMRU.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.menuFileMRU.LayoutOrientation = DevComponents.DotNetBar.eOrientation.Vertical;
            this.menuFileMRU.MinimumSize = new System.Drawing.Size(225, 0);
            this.menuFileMRU.Name = "menuFileMRU";
            this.menuFileMRU.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.labelRecentProjects});
            // 
            // 
            // 
            this.menuFileMRU.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // labelRecentProjects
            // 
            this.labelRecentProjects.BorderSide = DevComponents.DotNetBar.eBorderSide.Bottom;
            this.labelRecentProjects.BorderType = DevComponents.DotNetBar.eBorderType.Etched;
            this.labelRecentProjects.Name = "labelRecentProjects";
            this.labelRecentProjects.PaddingBottom = 2;
            this.labelRecentProjects.PaddingTop = 2;
            this.labelRecentProjects.Stretch = true;
            this.labelRecentProjects.Text = "Recent Games";
            // 
            // menuFileBottomContainer
            // 
            // 
            // 
            // 
            this.menuFileBottomContainer.BackgroundStyle.Class = "RibbonFileMenuBottomContainer";
            this.menuFileBottomContainer.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.menuFileBottomContainer.HorizontalItemAlignment = DevComponents.DotNetBar.eHorizontalItemsAlignment.Right;
            this.menuFileBottomContainer.Name = "menuFileBottomContainer";
            this.menuFileBottomContainer.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonOptions,
            this.buttonExit});
            // 
            // 
            // 
            this.menuFileBottomContainer.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // buttonOptions
            // 
            this.buttonOptions.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonOptions.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.buttonOptions.Image = ((System.Drawing.Image)(resources.GetObject("buttonOptions.Image")));
            this.buttonOptions.Name = "buttonOptions";
            this.buttonOptions.SubItemsExpandWidth = 24;
            this.buttonOptions.Text = "KGB Opt&ions";
            this.buttonOptions.Click += new System.EventHandler(this.ButtonOptionsClick);
            // 
            // buttonExit
            // 
            this.buttonExit.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonExit.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.buttonExit.Image = ((System.Drawing.Image)(resources.GetObject("buttonExit.Image")));
            this.buttonExit.Name = "buttonExit";
            this.buttonExit.SubItemsExpandWidth = 24;
            this.buttonExit.Text = "E&xit KGB";
            this.buttonExit.Click += new System.EventHandler(this.buttonExit_Click);
            // 
            // buttonViews
            // 
            this.buttonViews.Name = "buttonViews";
            this.buttonViews.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonFloorplanEditor,
            this.buttonHardpointEditor,
            this.buttonComponentEditor,
            this.buttonTextureEditor,
            this.buttonBehaviorTree,
            this.buttonCodeEditor,
            this.buttonSimulation});
            this.buttonViews.Text = "Views";
            // 
            // buttonFloorplanEditor
            // 
            this.buttonFloorplanEditor.Name = "buttonFloorplanEditor";
            this.buttonFloorplanEditor.Text = "Floorplan Designer";
            this.buttonFloorplanEditor.Click += new System.EventHandler(this.buttonFloorplanEditor_Click);
            // 
            // buttonHardpointEditor
            // 
            this.buttonHardpointEditor.Name = "buttonHardpointEditor";
            this.buttonHardpointEditor.Text = "Hardpoints Editor";
            this.buttonHardpointEditor.Click += new System.EventHandler(this.buttonHardpointEditor_Click);
            // 
            // buttonComponentEditor
            // 
            this.buttonComponentEditor.Name = "buttonComponentEditor";
            this.buttonComponentEditor.Text = "Component Editor";
            this.buttonComponentEditor.Click += new System.EventHandler(this.buttonComponentEditor_Click);
            // 
            // buttonTextureEditor
            // 
            this.buttonTextureEditor.Name = "buttonTextureEditor";
            this.buttonTextureEditor.Text = "Procedural Texture Editor";
            this.buttonTextureEditor.Click += new System.EventHandler(this.buttonTextureEditor_Click);
            // 
            // buttonBehaviorTree
            // 
            this.buttonBehaviorTree.Name = "buttonBehaviorTree";
            this.buttonBehaviorTree.Text = "Behavior Tree Editor";
            this.buttonBehaviorTree.Click += new System.EventHandler(this.buttonBehaviorTree_Click);
            // 
            // buttonCodeEditor
            // 
            this.buttonCodeEditor.Name = "buttonCodeEditor";
            this.buttonCodeEditor.Text = "Code Editor";
            this.buttonCodeEditor.Click += new System.EventHandler(this.buttonCodeEditor_Click);
            // 
            // buttonSimulation
            // 
            this.buttonSimulation.Name = "buttonSimulation";
            this.buttonSimulation.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonAdministration,
            this.buttonNavigation,
            this.buttonShipSystems,
            this.buttonSecurity,
            this.buttonCommunications,
            this.buttonCnC,
            this.buttonOperations,
            this.buttonFleet});
            this.buttonSimulation.Text = "Simulation";
            // 
            // buttonAdministration
            // 
            this.buttonAdministration.Name = "buttonAdministration";
            this.buttonAdministration.Text = "Administration";
            this.buttonAdministration.Click += new System.EventHandler(this.buttonAdministration_Click);
            // 
            // buttonNavigation
            // 
            this.buttonNavigation.Name = "buttonNavigation";
            this.buttonNavigation.Text = "Navigation";
            this.buttonNavigation.Click += new System.EventHandler(this.buttonNavigation_Click);
            // 
            // buttonShipSystems
            // 
            this.buttonShipSystems.Name = "buttonShipSystems";
            this.buttonShipSystems.Text = "Ship Systems";
            this.buttonShipSystems.Click += new System.EventHandler(this.buttonShipSystems_Click);
            // 
            // buttonSecurity
            // 
            this.buttonSecurity.Name = "buttonSecurity";
            this.buttonSecurity.Text = "Security";
            // 
            // buttonCommunications
            // 
            this.buttonCommunications.Name = "buttonCommunications";
            this.buttonCommunications.Text = "Communications";
            // 
            // buttonCnC
            // 
            this.buttonCnC.Name = "buttonCnC";
            this.buttonCnC.Text = "C&&C";
            this.buttonCnC.Click += new System.EventHandler(this.buttonTactical_Click);
            // 
            // buttonOperations
            // 
            this.buttonOperations.Name = "buttonOperations";
            this.buttonOperations.Text = "Operations";
            // 
            // buttonFleet
            // 
            this.buttonFleet.Name = "buttonFleet";
            this.buttonFleet.Text = "Fleet";
            // 
            // buttonHelp
            // 
            this.buttonHelp.AutoExpandOnClick = true;
            this.buttonHelp.ItemAlignment = DevComponents.DotNetBar.eItemAlignment.Far;
            this.buttonHelp.Name = "buttonHelp";
            this.buttonHelp.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonStyleOffice2010Blue,
            this.buttonStyleOffice2010Silver,
            this.buttonCredits,
            this.buttonCommunityForums});
            superTooltipInfo2.BodyText = "Change the style of all DotNetBar User Interface elements.";
            superTooltipInfo2.HeaderText = "Change the style";
            this.superTooltip1.SetSuperTooltip(this.buttonHelp, superTooltipInfo2);
            this.buttonHelp.Text = "Help";
            // 
            // buttonStyleOffice2010Blue
            // 
            this.buttonStyleOffice2010Blue.Checked = true;
            this.buttonStyleOffice2010Blue.Command = this.AppCommandTheme;
            this.buttonStyleOffice2010Blue.CommandParameter = "Office2010Blue";
            this.buttonStyleOffice2010Blue.Name = "buttonStyleOffice2010Blue";
            this.buttonStyleOffice2010Blue.OptionGroup = "style";
            this.buttonStyleOffice2010Blue.Text = "How To Play";
            this.buttonStyleOffice2010Blue.Click += new System.EventHandler(this.buttonHowToPlay_Click);
            // 
            // AppCommandTheme
            // 
            this.AppCommandTheme.Name = "AppCommandTheme";
            // 
            // buttonStyleOffice2010Silver
            // 
            this.buttonStyleOffice2010Silver.Command = this.AppCommandTheme;
            this.buttonStyleOffice2010Silver.CommandParameter = "Office2010Silver";
            this.buttonStyleOffice2010Silver.Name = "buttonStyleOffice2010Silver";
            this.buttonStyleOffice2010Silver.OptionGroup = "style";
            this.buttonStyleOffice2010Silver.Text = "About";
            this.buttonStyleOffice2010Silver.Click += new System.EventHandler(this.menuItemAbout_Click);
            // 
            // buttonCredits
            // 
            this.buttonCredits.Command = this.AppCommandTheme;
            this.buttonCredits.CommandParameter = "Windows7Blue";
            this.buttonCredits.Name = "buttonCredits";
            this.buttonCredits.OptionGroup = "style";
            this.buttonCredits.Text = "Credits";
            this.buttonCredits.Click += new System.EventHandler(this.buttonCredits_Click);
            // 
            // buttonCommunityForums
            // 
            this.buttonCommunityForums.Name = "buttonCommunityForums";
            this.buttonCommunityForums.Text = "Community Forums";
            this.buttonCommunityForums.Click += new System.EventHandler(this.buttonCommunityForums_Click);
            // 
            // buttonNew
            // 
            this.buttonNew.Image = ((System.Drawing.Image)(resources.GetObject("buttonNew.Image")));
            this.buttonNew.Name = "buttonNew";
            this.buttonNew.Shortcuts.Add(DevComponents.DotNetBar.eShortcut.CtrlN);
            this.buttonNew.Text = "New Document";
            this.buttonNew.Click += new System.EventHandler(this.buttonItemNewSingleRegion_Click);
            // 
            // buttonSave
            // 
            this.buttonSave.Image = ((System.Drawing.Image)(resources.GetObject("buttonSave.Image")));
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Text = "buttonItem2";
            this.buttonSave.Click += new System.EventHandler(this.buttonItemSave_Click);
            // 
            // superTooltip1
            // 
            this.superTooltip1.DefaultFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.superTooltip1.DefaultTooltipSettings = superTooltipInfo1;
            this.superTooltip1.MinimumTooltipSize = new System.Drawing.Size(150, 50);
            // 
            // styleManager1
            // 
            this.styleManager1.ManagerStyle = DevComponents.DotNetBar.eStyle.Office2010Blue;
            this.styleManager1.MetroColorParameters = new DevComponents.DotNetBar.Metro.ColorTables.MetroColorGeneratorParameters(System.Drawing.Color.White, System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(87)))), ((int)(((byte)(154))))));
            // 
            // dotNetBarManager
            // 
            this.dotNetBarManager.AutoDispatchShortcuts.Add(DevComponents.DotNetBar.eShortcut.F1);
            this.dotNetBarManager.AutoDispatchShortcuts.Add(DevComponents.DotNetBar.eShortcut.CtrlC);
            this.dotNetBarManager.AutoDispatchShortcuts.Add(DevComponents.DotNetBar.eShortcut.CtrlA);
            this.dotNetBarManager.AutoDispatchShortcuts.Add(DevComponents.DotNetBar.eShortcut.CtrlV);
            this.dotNetBarManager.AutoDispatchShortcuts.Add(DevComponents.DotNetBar.eShortcut.CtrlX);
            this.dotNetBarManager.AutoDispatchShortcuts.Add(DevComponents.DotNetBar.eShortcut.CtrlZ);
            this.dotNetBarManager.AutoDispatchShortcuts.Add(DevComponents.DotNetBar.eShortcut.CtrlY);
            this.dotNetBarManager.AutoDispatchShortcuts.Add(DevComponents.DotNetBar.eShortcut.Del);
            this.dotNetBarManager.AutoDispatchShortcuts.Add(DevComponents.DotNetBar.eShortcut.Ins);
            this.dotNetBarManager.BottomDockSite = this.dockSite4;
            this.dotNetBarManager.EnableFullSizeDock = false;
            this.dotNetBarManager.FillDockSite = this.dockSite9;
            this.dotNetBarManager.LeftDockSite = this.dockSite1;
            this.dotNetBarManager.ParentForm = this;
            this.dotNetBarManager.RightDockSite = this.dockSite2;
            this.dotNetBarManager.Style = DevComponents.DotNetBar.eDotNetBarStyle.Office2003;
            this.dotNetBarManager.ToolbarBottomDockSite = this.dockSite8;
            this.dotNetBarManager.ToolbarLeftDockSite = this.dockSite5;
            this.dotNetBarManager.ToolbarRightDockSite = this.dockSite6;
            this.dotNetBarManager.ToolbarTopDockSite = this.dockSite7;
            this.dotNetBarManager.TopDockSite = this.dockSite3;
            // 
            // dockSite4
            // 
            this.dockSite4.AccessibleRole = System.Windows.Forms.AccessibleRole.Window;
            this.dockSite4.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.dockSite4.DocumentDockContainer = new DevComponents.DotNetBar.DocumentDockContainer();
            this.dockSite4.Location = new System.Drawing.Point(5, 766);
            this.dockSite4.Name = "dockSite4";
            this.dockSite4.Size = new System.Drawing.Size(1249, 0);
            this.dockSite4.TabIndex = 17;
            this.dockSite4.TabStop = false;
            // 
            // dockSite9
            // 
            this.dockSite9.AccessibleRole = System.Windows.Forms.AccessibleRole.Window;
            this.dockSite9.Controls.Add(this.barDocumentDockBar);
            this.dockSite9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dockSite9.DocumentDockContainer = new DevComponents.DotNetBar.DocumentDockContainer(new DevComponents.DotNetBar.DocumentBaseContainer[] {
            ((DevComponents.DotNetBar.DocumentBaseContainer)(new DevComponents.DotNetBar.DocumentBarContainer(this.barDocumentDockBar, 1249, 708)))}, DevComponents.DotNetBar.eOrientation.Horizontal);
            this.dockSite9.Location = new System.Drawing.Point(5, 58);
            this.dockSite9.Name = "dockSite9";
            this.dockSite9.Size = new System.Drawing.Size(1249, 708);
            this.dockSite9.TabIndex = 22;
            this.dockSite9.TabStop = false;
            // 
            // barDocumentDockBar
            // 
            this.barDocumentDockBar.AccessibleDescription = "DotNetBar Bar (barDocumentDockBar)";
            this.barDocumentDockBar.AccessibleName = "DotNetBar Bar";
            this.barDocumentDockBar.AccessibleRole = System.Windows.Forms.AccessibleRole.Grouping;
            this.barDocumentDockBar.AlwaysDisplayDockTab = true;
            this.barDocumentDockBar.CanAutoHide = false;
            this.barDocumentDockBar.CanDockDocument = true;
            this.barDocumentDockBar.CanHide = true;
            this.barDocumentDockBar.DockTabAlignment = DevComponents.DotNetBar.eTabStripAlignment.Top;
            this.barDocumentDockBar.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.barDocumentDockBar.IsMaximized = false;
            this.barDocumentDockBar.LayoutType = DevComponents.DotNetBar.eLayoutType.DockContainer;
            this.barDocumentDockBar.Location = new System.Drawing.Point(0, 0);
            this.barDocumentDockBar.Name = "barDocumentDockBar";
            this.barDocumentDockBar.Stretch = true;
            this.barDocumentDockBar.Style = DevComponents.DotNetBar.eDotNetBarStyle.Office2003;
            this.barDocumentDockBar.TabIndex = 0;
            this.barDocumentDockBar.TabStop = false;
            // 
            // dockSite1
            // 
            this.dockSite1.AccessibleRole = System.Windows.Forms.AccessibleRole.Window;
            this.dockSite1.Dock = System.Windows.Forms.DockStyle.Left;
            this.dockSite1.DocumentDockContainer = new DevComponents.DotNetBar.DocumentDockContainer();
            this.dockSite1.Location = new System.Drawing.Point(5, 58);
            this.dockSite1.Name = "dockSite1";
            this.dockSite1.Size = new System.Drawing.Size(0, 708);
            this.dockSite1.TabIndex = 14;
            this.dockSite1.TabStop = false;
            // 
            // dockSite2
            // 
            this.dockSite2.AccessibleRole = System.Windows.Forms.AccessibleRole.Window;
            this.dockSite2.Dock = System.Windows.Forms.DockStyle.Right;
            this.dockSite2.Location = new System.Drawing.Point(1254, 58);
            this.dockSite2.Name = "dockSite2";
            this.dockSite2.Size = new System.Drawing.Size(0, 708);
            this.dockSite2.TabIndex = 15;
            this.dockSite2.TabStop = false;
            // 
            // dockSite8
            // 
            this.dockSite8.AccessibleRole = System.Windows.Forms.AccessibleRole.Window;
            this.dockSite8.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.dockSite8.Location = new System.Drawing.Point(5, 766);
            this.dockSite8.Name = "dockSite8";
            this.dockSite8.Size = new System.Drawing.Size(1249, 0);
            this.dockSite8.TabIndex = 21;
            this.dockSite8.TabStop = false;
            // 
            // dockSite5
            // 
            this.dockSite5.AccessibleRole = System.Windows.Forms.AccessibleRole.Window;
            this.dockSite5.Dock = System.Windows.Forms.DockStyle.Left;
            this.dockSite5.Location = new System.Drawing.Point(5, 1);
            this.dockSite5.Name = "dockSite5";
            this.dockSite5.Size = new System.Drawing.Size(0, 765);
            this.dockSite5.TabIndex = 18;
            this.dockSite5.TabStop = false;
            // 
            // dockSite6
            // 
            this.dockSite6.AccessibleRole = System.Windows.Forms.AccessibleRole.Window;
            this.dockSite6.Dock = System.Windows.Forms.DockStyle.Right;
            this.dockSite6.Location = new System.Drawing.Point(1254, 58);
            this.dockSite6.Name = "dockSite6";
            this.dockSite6.Size = new System.Drawing.Size(0, 708);
            this.dockSite6.TabIndex = 19;
            this.dockSite6.TabStop = false;
            // 
            // dockSite7
            // 
            this.dockSite7.AccessibleRole = System.Windows.Forms.AccessibleRole.Window;
            this.dockSite7.Dock = System.Windows.Forms.DockStyle.Top;
            this.dockSite7.Location = new System.Drawing.Point(5, 58);
            this.dockSite7.Name = "dockSite7";
            this.dockSite7.Size = new System.Drawing.Size(1249, 0);
            this.dockSite7.TabIndex = 20;
            this.dockSite7.TabStop = false;
            // 
            // dockSite3
            // 
            this.dockSite3.AccessibleRole = System.Windows.Forms.AccessibleRole.Window;
            this.dockSite3.Dock = System.Windows.Forms.DockStyle.Top;
            this.dockSite3.DocumentDockContainer = new DevComponents.DotNetBar.DocumentDockContainer();
            this.dockSite3.Enabled = false;
            this.dockSite3.Location = new System.Drawing.Point(5, 58);
            this.dockSite3.Name = "dockSite3";
            this.dockSite3.Size = new System.Drawing.Size(1249, 0);
            this.dockSite3.TabIndex = 16;
            this.dockSite3.TabStop = false;
            // 
            // FormMain
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(7, 20);
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.ClientSize = new System.Drawing.Size(1259, 768);
            this.Controls.Add(this.dockSite2);
            this.Controls.Add(this.dockSite1);
            this.Controls.Add(this.dockSite9);
            this.Controls.Add(this.contextMenuBar1);
            this.Controls.Add(this.dockSite3);
            this.Controls.Add(this.dockSite4);
            this.Controls.Add(this.dockSite6);
            this.Controls.Add(this.dockSite7);
            this.Controls.Add(this.dockSite8);
            this.Controls.Add(this.ribbon);
            this.Controls.Add(this.dockSite5);
            this.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Keystone Game Blocks (KGB) - Untitled";
            ((System.ComponentModel.ISupportInitialize)(this.contextMenuBar1)).EndInit();
            this.dockSite9.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.barDocumentDockBar)).EndInit();
            this.ResumeLayout(false);

        }
        private DevComponents.DotNetBar.ButtonItem buttonItemNewOutdoorScene;
        #endregion

        private ButtonItem buttonItemSave;
        private ButtonItem buttonItemNewSingleRegion;
        private ButtonItem buttonItemNewCampaign;
        private ButtonItem buttonItemNewMultiRegionScene;

        private ButtonItem buttonCommunityForums;
        
        private ButtonItem buttonViews;
        private ButtonItem buttonFloorplanEditor;
        private ButtonItem buttonTextureEditor;
        private ButtonItem buttonShipSystems;
        private ButtonItem buttonCodeEditor;
        private ButtonItem buttonSimulation;
        private ButtonItem buttonAdministration;
        private ButtonItem buttonNavigation;
        private ButtonItem buttonSecurity;
        private ButtonItem buttonCommunications;
        private ButtonItem buttonCnC;
        private ButtonItem buttonBehaviorTree;
        private ButtonItem buttonOperations;
        private ButtonItem buttonFleet;
        private ButtonItem buttonComponentEditor;
        private ButtonItem buttonHardpointEditor;
        private ButtonItem buttonItemLoadMission;
        private ButtonItem buttonItemResume;
        private ButtonItem buttonItemEdit;
        private ButtonItem buttonItemEditScene;
        private ButtonItem buttonItemEditPrefab;
        private ButtonItem buttonItemEditMission;
        private RibbonControl ribbon;
    }
}