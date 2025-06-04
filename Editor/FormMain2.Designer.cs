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
    partial class FormMain2 : DevComponents.DotNetBar.Office2007RibbonForm
    {
        private System.ComponentModel.IContainer components;
        private BalloonSearch m_Search = null;
        private DevComponents.DotNetBar.RibbonTabItem ribbonTabItemDisplay;
        private DevComponents.DotNetBar.RibbonPanel ribbonPanelDisplay;
        private DevComponents.DotNetBar.ButtonItem buttonSave;
        private DevComponents.DotNetBar.RibbonTabItem ribbonTabItemEdit;
        private DevComponents.DotNetBar.RibbonPanel ribbonPanelEdit;
        private DevComponents.DotNetBar.RibbonBar ribbonBarFont;
        private DevComponents.DotNetBar.RibbonBar ribbonBarColors;
        private DevComponents.DotNetBar.ButtonItem buttonUndo;
        private DevComponents.DotNetBar.ComboBoxItem comboFont;
        private DevComponents.DotNetBar.ComboBoxItem comboFontSize;
        private DevComponents.DotNetBar.ItemContainer itemContainer2;
        private DevComponents.DotNetBar.ItemContainer itemContainer3;
        private DevComponents.DotNetBar.ButtonItem buttonFontItalic;
        private DevComponents.DotNetBar.ButtonItem buttonFontUnderline;
        private DevComponents.DotNetBar.ButtonItem buttonFontStrike;
        private DevComponents.DotNetBar.ColorPickerDropDown buttonTextColor;
        private DevComponents.DotNetBar.ItemContainer itemContainer6;
        private DevComponents.DotNetBar.ItemContainer itemContainer4;
        private DevComponents.DotNetBar.ButtonItem buttonItemForeColor;
        private DevComponents.DotNetBar.ButtonItem buttonNew;
        private DevComponents.DotNetBar.ButtonItem buttonFontBold;
        private DevComponents.Editors.ComboItem comboItem1;
        private DevComponents.Editors.ComboItem comboItem2;
        private DevComponents.Editors.ComboItem comboItem3;
        private DevComponents.Editors.ComboItem comboItem4;
        private DevComponents.Editors.ComboItem comboItem5;
        private DevComponents.Editors.ComboItem comboItem6;
        private DevComponents.Editors.ComboItem comboItem7;
        private DevComponents.Editors.ComboItem comboItem8;
        private DevComponents.Editors.ComboItem comboItem9;
        private DevComponents.DotNetBar.RibbonPanel ribbonPanelContext;
        private DevComponents.DotNetBar.RibbonBar ribbonBar6;
        private DevComponents.DotNetBar.ButtonItem buttonItem1;
        private DevComponents.DotNetBar.ButtonItem buttonItem8;
        private DevComponents.DotNetBar.ButtonItem buttonItem12;
        private DevComponents.DotNetBar.RibbonTabItem ribbonTabItemPrefabs;
        private DevComponents.DotNetBar.ButtonItem buttonChangeStyle;
        private DevComponents.DotNetBar.SuperTooltip superTooltip1;
        private DevComponents.DotNetBar.ButtonItem buttonStyleOffice2007Blue;
        private DevComponents.DotNetBar.ButtonItem buttonStyleOffice2007Black;
        private DevComponents.DotNetBar.ButtonItem buttonItemNew;
        private DevComponents.DotNetBar.ButtonItem buttonItemOpen;
        private DevComponents.DotNetBar.ButtonItem buttonImport;
        private DevComponents.DotNetBar.ButtonItem buttonItemPrint;
        private DevComponents.DotNetBar.ButtonItem buttonCloseScene;
        private DevComponents.DotNetBar.LabelItem labelItem8;
        private DevComponents.DotNetBar.ButtonItem buttonItem26;
        private DevComponents.DotNetBar.ButtonItem buttonItem27;
        private DevComponents.DotNetBar.ButtonItem buttonItem28;
        private DevComponents.DotNetBar.ButtonItem buttonItem29;
        private DevComponents.DotNetBar.ButtonItem buttonFileSaveAs;
        private DevComponents.DotNetBar.ItemContainer menuFileContainer;
        private DevComponents.DotNetBar.ItemContainer menuFileTwoColumnContainer;
        private DevComponents.DotNetBar.ItemContainer menuFileItems;
        private DevComponents.DotNetBar.ItemContainer menuFileMRU;
        private DevComponents.DotNetBar.ItemContainer menuFileBottomContainer;
        private DevComponents.DotNetBar.ButtonItem buttonOptions;
        private DevComponents.DotNetBar.ButtonItem buttonExit;
        private DevComponents.DotNetBar.QatCustomizeItem qatCustomizeItem1;
        private DevComponents.DotNetBar.ContextMenuBar contextMenuBar1;
        private DevComponents.DotNetBar.ButtonItem bEditPopup;
        private DevComponents.DotNetBar.ButtonItem bCut;
        private DevComponents.DotNetBar.ButtonItem bCopy;
        private DevComponents.DotNetBar.ButtonItem bPaste;
        private DevComponents.DotNetBar.ButtonItem bSelectAll;
        private DevComponents.DotNetBar.ButtonItem buttonStyleOffice2007Silver;
        private DevComponents.DotNetBar.ColorPickerDropDown buttonStyleCustom;
        private DevComponents.DotNetBar.ItemContainer itemContainer12;
        private DevComponents.DotNetBar.LabelItem labelItemSaveCaption;
        private DevComponents.DotNetBar.ButtonItem buttonItemSaveScene;
        private DevComponents.DotNetBar.ButtonItem buttonItemSavePrefab;
        private DevComponents.DotNetBar.ButtonItem buttonItem58;
        private DevComponents.DotNetBar.ButtonItem buttonItem59;
        private Command AppCommandTheme;
        private DevComponents.DotNetBar.ButtonItem buttonItem60;
        private DevComponents.DotNetBar.StyleManager styleManager1;
        private DevComponents.DotNetBar.ButtonItem buttonStyleOffice2010Silver;
        private DevComponents.DotNetBar.ButtonItem buttonItem62;
        private ButtonItem buttonStyleOffice2010Blue;
        private DockSite dockSite2;
        private DockSite dockSite1;
        private Bar barLeftDockBar;
        private PanelDockContainer panelDockContainerLeft;
        private DockContainerItem dockContainerItemLeft;
        private DockSite dockSite3;
        private DockSite dockSite4;
        private DockSite dockSite5;
        private DockSite dockSite6;
        private DockSite dockSite7;
        private DockSite dockSite8;
        private DotNetBarManager dotNetBarManager;
        private Bar barRightDockBar;
        private PanelDockContainer panelDockContainerRight;
        private DockContainerItem dockContainerItemRight;
        private DockSite dockSite9;
        private Bar barDocumentDockBar;
        //private DockContainerItem dockContainerItem3;
        private Bar barStatusBar;
        //private DockContainerItem dockContainerItem4;
        private ItemContainer itemContainer13;
        internal LabelItem labelPosition;
        private SliderItem zoomSlider;
        private ProgressBarItem progressBarItem1;
        private ItemContainer itemContainer9;
        private ButtonItem buttonItem13;
        private ButtonItem buttonItem14;
        private ButtonItem buttonItem15;
        private ButtonItem buttonItem16;
        private ButtonItem buttonItem17;
        private LabelItem labelStatus;
        protected RibbonTabItemGroup ribbonTabItemGroup1;
        protected RibbonTabItemGroup ribbonTabItemGroup2;
        private DevComponents.DotNetBar.Office2007StartButton buttonFile;


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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain2));
            this.ribbonControl = new DevComponents.DotNetBar.RibbonControl();
            this.ribbonPanel10 = new DevComponents.DotNetBar.RibbonPanel();
            this.ribbonBar11 = new DevComponents.DotNetBar.RibbonBar();
            this.itemContainer23 = new DevComponents.DotNetBar.ItemContainer();
            this.buttonItemCycleNextShip = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemCyclePrevShip = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem19 = new DevComponents.DotNetBar.ButtonItem();
            this.itemContainer24 = new DevComponents.DotNetBar.ItemContainer();
            this.buttonItem25 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem40 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem41 = new DevComponents.DotNetBar.ButtonItem();
            this.itemContainer25 = new DevComponents.DotNetBar.ItemContainer();
            this.buttonIncreaseVelocity = new DevComponents.DotNetBar.ButtonItem();
            this.buttonDecreaseVelocity = new DevComponents.DotNetBar.ButtonItem();
            this.buttonStop = new DevComponents.DotNetBar.ButtonItem();
            this.itemContainer35 = new DevComponents.DotNetBar.ItemContainer();
            this.buttonItem45 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem46 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem55 = new DevComponents.DotNetBar.ButtonItem();
            this.ribbonBar14 = new DevComponents.DotNetBar.RibbonBar();
            this.itemContainer29 = new DevComponents.DotNetBar.ItemContainer();
            this.checkBoxItem13 = new DevComponents.DotNetBar.CheckBoxItem();
            this.checkBoxItem14 = new DevComponents.DotNetBar.CheckBoxItem();
            this.checkBoxItem15 = new DevComponents.DotNetBar.CheckBoxItem();
            this.ribbonPanel5 = new DevComponents.DotNetBar.RibbonPanel();
            this.ribbonBar7 = new DevComponents.DotNetBar.RibbonBar();
            this.buttonItemStar = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemBluePlanet = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemGasPlanet = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemPlanetoidField = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemMoon = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemSolSystem = new DevComponents.DotNetBar.ButtonItem();
            this.ribbonBar4 = new DevComponents.DotNetBar.RibbonBar();
            this.buttonItemStarFieldNoTexture = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemStarfield = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemDustField = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemSkybox = new DevComponents.DotNetBar.ButtonItem();
            this.ribbonPanel1 = new DevComponents.DotNetBar.RibbonPanel();
            this.ribbonBar17 = new DevComponents.DotNetBar.RibbonBar();
            this.itemContainer30 = new DevComponents.DotNetBar.ItemContainer();
            this.buttonSpherePrimitive = new DevComponents.DotNetBar.ButtonItem();
            this.buttonBoxPrimitive = new DevComponents.DotNetBar.ButtonItem();
            this.buttonCylinderPrimitive = new DevComponents.DotNetBar.ButtonItem();
            this.itemContainer31 = new DevComponents.DotNetBar.ItemContainer();
            this.buttonPyramidPrimitive = new DevComponents.DotNetBar.ButtonItem();
            this.buttonTeapotPrimitive = new DevComponents.DotNetBar.ButtonItem();
            this.buttonTorusPrimitive = new DevComponents.DotNetBar.ButtonItem();
            this.itemContainer34 = new DevComponents.DotNetBar.ItemContainer();
            this.buttonNewComet = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem72 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem73 = new DevComponents.DotNetBar.ButtonItem();
            this.itemContainer32 = new DevComponents.DotNetBar.ItemContainer();
            this.buttonNewStar = new DevComponents.DotNetBar.ButtonItem();
            this.buttonNewWorld = new DevComponents.DotNetBar.ButtonItem();
            this.buttonNewAsteroidBelt = new DevComponents.DotNetBar.ButtonItem();
            this.itemContainer33 = new DevComponents.DotNetBar.ItemContainer();
            this.buttonItem6 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem7 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem42 = new DevComponents.DotNetBar.ButtonItem();
            this.ribbonBar2 = new DevComponents.DotNetBar.RibbonBar();
            this.buttonItem2 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem3 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem4 = new DevComponents.DotNetBar.ButtonItem();
            this.ribbonPanelEdit = new DevComponents.DotNetBar.RibbonPanel();
            this.ribbonBar16 = new DevComponents.DotNetBar.RibbonBar();
            this.itemContainer26 = new DevComponents.DotNetBar.ItemContainer();
            this.buttonSelectTool = new DevComponents.DotNetBar.ButtonItem();
            this.buttonMoveTool = new DevComponents.DotNetBar.ButtonItem();
            this.buttonRotateTool = new DevComponents.DotNetBar.ButtonItem();
            this.itemContainer27 = new DevComponents.DotNetBar.ItemContainer();
            this.buttonScaleTool = new DevComponents.DotNetBar.ButtonItem();
            this.buttonLineTool = new DevComponents.DotNetBar.ButtonItem();
            this.buttonRectangleTool = new DevComponents.DotNetBar.ButtonItem();
            this.itemContainer28 = new DevComponents.DotNetBar.ItemContainer();
            this.buttonCircleTool = new DevComponents.DotNetBar.ButtonItem();
            this.buttonPolygonTool = new DevComponents.DotNetBar.ButtonItem();
            this.buttonThrowProjectile = new DevComponents.DotNetBar.ButtonItem();
            this.ribbonBar5 = new DevComponents.DotNetBar.RibbonBar();
            this.buttonMargins = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem9 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem50 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem51 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem52 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem10 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem11 = new DevComponents.DotNetBar.ButtonItem();
            this.labelEditTabNotesTemp = new System.Windows.Forms.Label();
            this.ribbonBar8 = new DevComponents.DotNetBar.RibbonBar();
            this.itemContainer10 = new DevComponents.DotNetBar.ItemContainer();
            this.checkBoxItem1 = new DevComponents.DotNetBar.CheckBoxItem();
            this.checkBoxItem2 = new DevComponents.DotNetBar.CheckBoxItem();
            this.checkBoxItem3 = new DevComponents.DotNetBar.CheckBoxItem();
            this.itemContainer11 = new DevComponents.DotNetBar.ItemContainer();
            this.checkBoxItem4 = new DevComponents.DotNetBar.CheckBoxItem();
            this.checkBoxItem5 = new DevComponents.DotNetBar.CheckBoxItem();
            this.checkBoxItem6 = new DevComponents.DotNetBar.CheckBoxItem();
            this.ribbonBar1 = new DevComponents.DotNetBar.RibbonBar();
            this.buttonPaste = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem53 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem54 = new DevComponents.DotNetBar.ButtonItem();
            this.itemContainer1 = new DevComponents.DotNetBar.ItemContainer();
            this.buttonCut = new DevComponents.DotNetBar.ButtonItem();
            this.buttonCopy = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemPaste = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemUndo = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemRedo = new DevComponents.DotNetBar.ButtonItem();
            this.ribbonBarSearch = new DevComponents.DotNetBar.RibbonBar();
            this.buttonFind = new DevComponents.DotNetBar.ButtonItem();
            this.itemContainer5 = new DevComponents.DotNetBar.ItemContainer();
            this.buttonReplace = new DevComponents.DotNetBar.ButtonItem();
            this.buttonGoto = new DevComponents.DotNetBar.ButtonItem();
            this.ribbonPanelContext = new DevComponents.DotNetBar.RibbonPanel();
            this.ribbonBar6 = new DevComponents.DotNetBar.RibbonBar();
            this.buttonItem1 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem8 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem12 = new DevComponents.DotNetBar.ButtonItem();
            this.ribbonPanel4 = new DevComponents.DotNetBar.RibbonPanel();
            this.ribbonBar3 = new DevComponents.DotNetBar.RibbonBar();
            this.buttonItemDirectionalLight = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemPointLight = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemSpotLight = new DevComponents.DotNetBar.ButtonItem();
            this.ribbonPanel9 = new DevComponents.DotNetBar.RibbonPanel();
            this.ribbonBar15 = new DevComponents.DotNetBar.RibbonBar();
            this.itemContainer17 = new DevComponents.DotNetBar.ItemContainer();
            this.buttonPT_Select = new DevComponents.DotNetBar.ButtonItem();
            this.buttonPT_ScaleOutput = new DevComponents.DotNetBar.ButtonItem();
            this.buttonPT_ScaleBiasOutput = new DevComponents.DotNetBar.ButtonItem();
            this.itemContainer18 = new DevComponents.DotNetBar.ItemContainer();
            this.buttonPT_FastNoise = new DevComponents.DotNetBar.ButtonItem();
            this.buttonPT_FastBillow = new DevComponents.DotNetBar.ButtonItem();
            this.buttonPT_FastTurbulence = new DevComponents.DotNetBar.ButtonItem();
            this.itemContainer22 = new DevComponents.DotNetBar.ItemContainer();
            this.buttonPT_Voronoi = new DevComponents.DotNetBar.ButtonItem();
            this.buttonPT_Billow = new DevComponents.DotNetBar.ButtonItem();
            this.buttonPT_Turbulence = new DevComponents.DotNetBar.ButtonItem();
            this.itemContainer21 = new DevComponents.DotNetBar.ItemContainer();
            this.buttonPT_FastRidgedMF = new DevComponents.DotNetBar.ButtonItem();
            this.buttonPT_RidgedMF = new DevComponents.DotNetBar.ButtonItem();
            this.buttonPT_Perlin = new DevComponents.DotNetBar.ButtonItem();
            this.itemContainer19 = new DevComponents.DotNetBar.ItemContainer();
            this.buttonPT_Checkerboard = new DevComponents.DotNetBar.ButtonItem();
            this.buttonPT_Int = new DevComponents.DotNetBar.ButtonItem();
            this.buttonPT_Float = new DevComponents.DotNetBar.ButtonItem();
            this.itemContainer20 = new DevComponents.DotNetBar.ItemContainer();
            this.buttonPT_SphereMap = new DevComponents.DotNetBar.ButtonItem();
            this.buttonPT_PlaneMap = new DevComponents.DotNetBar.ButtonItem();
            this.buttonPT_CylinderMap = new DevComponents.DotNetBar.ButtonItem();
            this.ribbonBar13 = new DevComponents.DotNetBar.RibbonBar();
            this.buttonItem20 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem21 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem22 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem23 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem30 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem24 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem31 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem32 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem33 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem34 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem35 = new DevComponents.DotNetBar.ButtonItem();
            this.ribbonBar12 = new DevComponents.DotNetBar.RibbonBar();
            this.itemContainer8 = new DevComponents.DotNetBar.ItemContainer();
            this.checkBoxItem7 = new DevComponents.DotNetBar.CheckBoxItem();
            this.checkBoxItem8 = new DevComponents.DotNetBar.CheckBoxItem();
            this.checkBoxItem9 = new DevComponents.DotNetBar.CheckBoxItem();
            this.ribbonBar10 = new DevComponents.DotNetBar.RibbonBar();
            this.buttonItemGenerateTexture = new DevComponents.DotNetBar.ButtonItem();
            this.ribbonPanelDisplay = new DevComponents.DotNetBar.RibbonPanel();
            this.ribbonBarViewLayouts = new DevComponents.DotNetBar.RibbonBar();
            this.galleryViewLayouts = new DevComponents.DotNetBar.GalleryContainer();
            this.buttonItem47 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem48 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem49 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemDisplaySingleViewport = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemDisplayVSplit = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemDisplayHSplit = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemDisplayTripleLeft = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemDisplayTripleRight = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemDisplayQuad = new DevComponents.DotNetBar.ButtonItem();
            this.ribbonBarColors = new DevComponents.DotNetBar.RibbonBar();
            this.itemContainer4 = new DevComponents.DotNetBar.ItemContainer();
            this.buttonItemForeColor = new DevComponents.DotNetBar.ButtonItem();
            this.itemContainer6 = new DevComponents.DotNetBar.ItemContainer();
            this.buttonItem37 = new DevComponents.DotNetBar.ButtonItem();
            this.controlContainerItem3 = new DevComponents.DotNetBar.ControlContainerItem();
            this.itemContainer15 = new DevComponents.DotNetBar.ItemContainer();
            this.ribbonBarFont = new DevComponents.DotNetBar.RibbonBar();
            this.itemContainer2 = new DevComponents.DotNetBar.ItemContainer();
            this.comboFont = new DevComponents.DotNetBar.ComboBoxItem();
            this.comboFontSize = new DevComponents.DotNetBar.ComboBoxItem();
            this.comboItem1 = new DevComponents.Editors.ComboItem();
            this.comboItem2 = new DevComponents.Editors.ComboItem();
            this.comboItem3 = new DevComponents.Editors.ComboItem();
            this.comboItem4 = new DevComponents.Editors.ComboItem();
            this.comboItem5 = new DevComponents.Editors.ComboItem();
            this.comboItem6 = new DevComponents.Editors.ComboItem();
            this.comboItem7 = new DevComponents.Editors.ComboItem();
            this.comboItem8 = new DevComponents.Editors.ComboItem();
            this.comboItem9 = new DevComponents.Editors.ComboItem();
            this.itemContainer3 = new DevComponents.DotNetBar.ItemContainer();
            this.buttonFontBold = new DevComponents.DotNetBar.ButtonItem();
            this.buttonFontItalic = new DevComponents.DotNetBar.ButtonItem();
            this.buttonFontUnderline = new DevComponents.DotNetBar.ButtonItem();
            this.buttonFontStrike = new DevComponents.DotNetBar.ButtonItem();
            this.buttonTextColor = new DevComponents.DotNetBar.ColorPickerDropDown();
            this.ribbonPanel3 = new DevComponents.DotNetBar.RibbonPanel();
            this.ribbonBar9 = new DevComponents.DotNetBar.RibbonBar();
            this.galleryContainerTextures = new DevComponents.DotNetBar.GalleryContainer();
            this.buttonItemOnlineTextureSearch = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemBrowseTextures = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemProceduralTextureGen = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemSaveTexture = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemTexture1 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemTexture2 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemTexture3 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemTexture4 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemTexture5 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemTexture6 = new DevComponents.DotNetBar.ButtonItem();
            this.ribbonPanel2 = new DevComponents.DotNetBar.RibbonPanel();
            this.ribbonPanel8 = new DevComponents.DotNetBar.RibbonPanel();
            this.ribbonPanel7 = new DevComponents.DotNetBar.RibbonPanel();
            this.ribbonPanel11 = new DevComponents.DotNetBar.RibbonPanel();
            this.ribbonPanel6 = new DevComponents.DotNetBar.RibbonPanel();
            this.labelScriptsNotesTemp = new System.Windows.Forms.Label();
            this.contextMenuBar1 = new DevComponents.DotNetBar.ContextMenuBar();
            this.bEditPopup = new DevComponents.DotNetBar.ButtonItem();
            this.bCut = new DevComponents.DotNetBar.ButtonItem();
            this.bCopy = new DevComponents.DotNetBar.ButtonItem();
            this.bPaste = new DevComponents.DotNetBar.ButtonItem();
            this.bSelectAll = new DevComponents.DotNetBar.ButtonItem();
            this.buttonFile = new DevComponents.DotNetBar.Office2007StartButton();
            this.menuFileContainer = new DevComponents.DotNetBar.ItemContainer();
            this.menuFileTwoColumnContainer = new DevComponents.DotNetBar.ItemContainer();
            this.menuFileItems = new DevComponents.DotNetBar.ItemContainer();
            this.buttonItemNew = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemNewSingleRegion = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemNewMultiRegionScene = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemNewFloorPlan = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemOpen = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemSave = new DevComponents.DotNetBar.ButtonItem();
            this.buttonFileSaveAs = new DevComponents.DotNetBar.ButtonItem();
            this.itemContainer12 = new DevComponents.DotNetBar.ItemContainer();
            this.labelItemSaveCaption = new DevComponents.DotNetBar.LabelItem();
            this.buttonItemSaveScene = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemSavePrefab = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem58 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem59 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonImport = new DevComponents.DotNetBar.ButtonItem();
            this.buttonImportMesh = new DevComponents.DotNetBar.ButtonItem();
            this.buttonImportEditableObjMesh = new DevComponents.DotNetBar.ButtonItem();
            this.buttonImportActor = new DevComponents.DotNetBar.ButtonItem();
            this.buttonImportMinimesh = new DevComponents.DotNetBar.ButtonItem();
            this.buttonImportBillboard = new DevComponents.DotNetBar.ButtonItem();
            this.buttonImportVehicle = new DevComponents.DotNetBar.ButtonItem();
            this.buttonPhysicsDemos = new DevComponents.DotNetBar.ButtonItem();
            this.buttonWallDemo = new DevComponents.DotNetBar.ButtonItem();
            this.buttonIncomingDemo = new DevComponents.DotNetBar.ButtonItem();
            this.buttonSpheresDemo = new DevComponents.DotNetBar.ButtonItem();
            this.buttonCatapultDemo = new DevComponents.DotNetBar.ButtonItem();
            this.buttonPendulumDemo = new DevComponents.DotNetBar.ButtonItem();
            this.buttonPlinkoDemo = new DevComponents.DotNetBar.ButtonItem();
            this.buttonVehicleDemo = new DevComponents.DotNetBar.ButtonItem();
            this.buttonSandboxDemo = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItemPrint = new DevComponents.DotNetBar.ButtonItem();
            this.buttonCloseScene = new DevComponents.DotNetBar.ButtonItem();
            this.menuFileMRU = new DevComponents.DotNetBar.ItemContainer();
            this.labelItem8 = new DevComponents.DotNetBar.LabelItem();
            this.buttonItem26 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem27 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem28 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem29 = new DevComponents.DotNetBar.ButtonItem();
            this.menuFileBottomContainer = new DevComponents.DotNetBar.ItemContainer();
            this.buttonOptions = new DevComponents.DotNetBar.ButtonItem();
            this.buttonExit = new DevComponents.DotNetBar.ButtonItem();
            this.ribbonTabItemVehicles = new DevComponents.DotNetBar.RibbonTabItem();
            this.ribbonTabItemDisplay = new DevComponents.DotNetBar.RibbonTabItem();
            this.ribbonTabItemGroup2 = new DevComponents.DotNetBar.RibbonTabItemGroup();
            this.ribbonTabItemEdit = new DevComponents.DotNetBar.RibbonTabItem();
            this.ribbonTabItemFX = new DevComponents.DotNetBar.RibbonTabItem();
            this.ribbonTabItemLights = new DevComponents.DotNetBar.RibbonTabItem();
            this.ribbonTabItemPrimitives = new DevComponents.DotNetBar.RibbonTabItem();
            this.ribbonTabItemPrefabs = new DevComponents.DotNetBar.RibbonTabItem();
            this.ribbonTabItemGroup1 = new DevComponents.DotNetBar.RibbonTabItemGroup();
            this.ribbonTabItemProceduralTexture = new DevComponents.DotNetBar.RibbonTabItem();
            this.ribbonTabItemTextures = new DevComponents.DotNetBar.RibbonTabItem();
            this.ribbonTabItemMaterials = new DevComponents.DotNetBar.RibbonTabItem();
            this.ribbonTabItemShaders = new DevComponents.DotNetBar.RibbonTabItem();
            this.ribbonTabItemBehaviors = new DevComponents.DotNetBar.RibbonTabItem();
            this.ribbonTabItemPhysics = new DevComponents.DotNetBar.RibbonTabItem();
            this.ribbonTabItemScripts = new DevComponents.DotNetBar.RibbonTabItem();
            this.buttonChangeStyle = new DevComponents.DotNetBar.ButtonItem();
            this.buttonStyleOffice2010Blue = new DevComponents.DotNetBar.ButtonItem();
            this.AppCommandTheme = new DevComponents.DotNetBar.Command(this.components);
            this.buttonStyleOffice2010Silver = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem62 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonStyleOffice2007Blue = new DevComponents.DotNetBar.ButtonItem();
            this.buttonStyleOffice2007Black = new DevComponents.DotNetBar.ButtonItem();
            this.buttonStyleOffice2007Silver = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem60 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonStyleCustom = new DevComponents.DotNetBar.ColorPickerDropDown();
            this.buttonNew = new DevComponents.DotNetBar.ButtonItem();
            this.buttonSave = new DevComponents.DotNetBar.ButtonItem();
            this.buttonUndo = new DevComponents.DotNetBar.ButtonItem();
            this.qatCustomizeItem1 = new DevComponents.DotNetBar.QatCustomizeItem();
            this.superTooltip1 = new DevComponents.DotNetBar.SuperTooltip();
            this.buttonItemBackColor = new DevComponents.DotNetBar.ButtonItem();
            this.controlContainerItem1 = new DevComponents.DotNetBar.ControlContainerItem();
            this.itemContainer7 = new DevComponents.DotNetBar.ItemContainer();
            this.buttonItem36 = new DevComponents.DotNetBar.ButtonItem();
            this.controlContainerItem2 = new DevComponents.DotNetBar.ControlContainerItem();
            this.itemContainer14 = new DevComponents.DotNetBar.ItemContainer();
            this.buttonItem38 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem39 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem70 = new DevComponents.DotNetBar.ButtonItem();
            this.styleManager1 = new DevComponents.DotNetBar.StyleManager();
            this.dotNetBarManager = new DevComponents.DotNetBar.DotNetBarManager(this.components);
            this.dockSite4 = new DevComponents.DotNetBar.DockSite();
            this.dockSite9 = new DevComponents.DotNetBar.DockSite();
            this.barDocumentDockBar = new DevComponents.DotNetBar.Bar();
            this.dockSite1 = new DevComponents.DotNetBar.DockSite();
            this.barLeftDockBar = new DevComponents.DotNetBar.Bar();
            this.panelDockContainerLeft = new DevComponents.DotNetBar.PanelDockContainer();
            this.treeEntityBrowser = new System.Windows.Forms.TreeView();
            this.dockContainerItemLeft = new DevComponents.DotNetBar.DockContainerItem();
            this.dockSite2 = new DevComponents.DotNetBar.DockSite();
            this.barRightDockBar = new DevComponents.DotNetBar.Bar();
            this.panelDockContainerRight = new DevComponents.DotNetBar.PanelDockContainer();
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.dockContainerItemRight = new DevComponents.DotNetBar.DockContainerItem();
            this.dockSite8 = new DevComponents.DotNetBar.DockSite();
            this.barStatusBar = new DevComponents.DotNetBar.Bar();
            this.dockSite5 = new DevComponents.DotNetBar.DockSite();
            this.dockSite6 = new DevComponents.DotNetBar.DockSite();
            this.dockSite7 = new DevComponents.DotNetBar.DockSite();
            this.dockSite3 = new DevComponents.DotNetBar.DockSite();
            this.itemContainer13 = new DevComponents.DotNetBar.ItemContainer();
            this.labelPosition = new DevComponents.DotNetBar.LabelItem();
            this.zoomSlider = new DevComponents.DotNetBar.SliderItem();
            this.progressBarItem1 = new DevComponents.DotNetBar.ProgressBarItem();
            this.itemContainer9 = new DevComponents.DotNetBar.ItemContainer();
            this.buttonItem13 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem14 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem15 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem16 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem17 = new DevComponents.DotNetBar.ButtonItem();
            this.labelStatus = new DevComponents.DotNetBar.LabelItem();
            this.buttonItem5 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem18 = new DevComponents.DotNetBar.ButtonItem();
            this.itemContainer16 = new DevComponents.DotNetBar.ItemContainer();
            this.checkBoxItem10 = new DevComponents.DotNetBar.CheckBoxItem();
            this.checkBoxItem11 = new DevComponents.DotNetBar.CheckBoxItem();
            this.checkBoxItem12 = new DevComponents.DotNetBar.CheckBoxItem();
            this.buttonItem43 = new DevComponents.DotNetBar.ButtonItem();
            this.buttonItem44 = new DevComponents.DotNetBar.ButtonItem();
            this.ribbonControl.SuspendLayout();
            this.ribbonPanel10.SuspendLayout();
            this.ribbonPanel5.SuspendLayout();
            this.ribbonPanel1.SuspendLayout();
            this.ribbonPanelEdit.SuspendLayout();
            this.ribbonPanelContext.SuspendLayout();
            this.ribbonPanel4.SuspendLayout();
            this.ribbonPanel9.SuspendLayout();
            this.ribbonPanelDisplay.SuspendLayout();
            this.ribbonPanel3.SuspendLayout();
            this.ribbonPanel6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.contextMenuBar1)).BeginInit();
            this.dockSite9.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.barDocumentDockBar)).BeginInit();
            this.dockSite1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.barLeftDockBar)).BeginInit();
            this.barLeftDockBar.SuspendLayout();
            this.panelDockContainerLeft.SuspendLayout();
            this.dockSite2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.barRightDockBar)).BeginInit();
            this.barRightDockBar.SuspendLayout();
            this.panelDockContainerRight.SuspendLayout();
            this.dockSite8.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.barStatusBar)).BeginInit();
            this.SuspendLayout();
            // 
            // ribbonControl
            // 
            this.ribbonControl.AutoSize = true;
            this.ribbonControl.BackColor = System.Drawing.SystemColors.Control;
            // 
            // 
            // 
            this.ribbonControl.BackgroundStyle.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ribbonControl.BackgroundStyle.BackColor2 = System.Drawing.SystemColors.ControlLight;
            this.ribbonControl.BackgroundStyle.Class = "";
            this.ribbonControl.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonControl.CaptionVisible = true;
            this.ribbonControl.Controls.Add(this.ribbonPanel10);
            this.ribbonControl.Controls.Add(this.ribbonPanel5);
            this.ribbonControl.Controls.Add(this.ribbonPanel1);
            this.ribbonControl.Controls.Add(this.ribbonPanelEdit);
            this.ribbonControl.Controls.Add(this.ribbonPanelContext);
            this.ribbonControl.Controls.Add(this.ribbonPanel4);
            this.ribbonControl.Controls.Add(this.ribbonPanel9);
            this.ribbonControl.Controls.Add(this.ribbonPanelDisplay);
            this.ribbonControl.Controls.Add(this.ribbonPanel3);
            this.ribbonControl.Controls.Add(this.ribbonPanel2);
            this.ribbonControl.Controls.Add(this.ribbonPanel8);
            this.ribbonControl.Controls.Add(this.ribbonPanel7);
            this.ribbonControl.Controls.Add(this.ribbonPanel11);
            this.ribbonControl.Controls.Add(this.ribbonPanel6);
            this.ribbonControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.ribbonControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ribbonControl.GlobalContextMenuBar = this.contextMenuBar1;
            this.ribbonControl.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonFile,
            this.ribbonTabItemVehicles,
            this.ribbonTabItemDisplay,
            this.ribbonTabItemEdit,
            this.ribbonTabItemFX,
            this.ribbonTabItemLights,
            this.ribbonTabItemPrimitives,
            this.ribbonTabItemPrefabs,
            this.ribbonTabItemProceduralTexture,
            this.ribbonTabItemTextures,
            this.ribbonTabItemMaterials,
            this.ribbonTabItemShaders,
            this.ribbonTabItemBehaviors,
            this.ribbonTabItemPhysics,
            this.ribbonTabItemScripts,
            this.buttonChangeStyle});
            this.ribbonControl.KeyTipsFont = new System.Drawing.Font("Tahoma", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ribbonControl.Location = new System.Drawing.Point(5, 1);
            this.ribbonControl.MdiSystemItemVisible = false;
            this.ribbonControl.Name = "ribbonControl";
            this.ribbonControl.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
            this.ribbonControl.Size = new System.Drawing.Size(1014, 144);
            this.ribbonControl.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ribbonControl.TabGroupHeight = 14;
            this.ribbonControl.TabGroups.AddRange(new DevComponents.DotNetBar.RibbonTabItemGroup[] {
            this.ribbonTabItemGroup1,
            this.ribbonTabItemGroup2});
            this.ribbonControl.TabIndex = 8;
            // 
            // ribbonPanel10
            // 
            this.ribbonPanel10.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ribbonPanel10.Controls.Add(this.ribbonBar11);
            this.ribbonPanel10.Controls.Add(this.ribbonBar14);
            this.ribbonPanel10.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ribbonPanel10.Location = new System.Drawing.Point(0, 53);
            this.ribbonPanel10.Name = "ribbonPanel10";
            this.ribbonPanel10.Padding = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.ribbonPanel10.Size = new System.Drawing.Size(1014, 89);
            // 
            // 
            // 
            this.ribbonPanel10.Style.Class = "";
            this.ribbonPanel10.Style.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonPanel10.StyleMouseDown.Class = "";
            this.ribbonPanel10.StyleMouseDown.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonPanel10.StyleMouseOver.Class = "";
            this.ribbonPanel10.StyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonPanel10.TabIndex = 14;
            // 
            // ribbonBar11
            // 
            this.ribbonBar11.AutoOverflowEnabled = true;
            // 
            // 
            // 
            this.ribbonBar11.BackgroundMouseOverStyle.Class = "";
            this.ribbonBar11.BackgroundMouseOverStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBar11.BackgroundStyle.Class = "";
            this.ribbonBar11.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonBar11.ContainerControlProcessDialogKey = true;
            this.ribbonBar11.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.itemContainer23,
            this.itemContainer24,
            this.itemContainer25,
            this.itemContainer35});
            this.ribbonBar11.ItemSpacing = 4;
            this.ribbonBar11.Location = new System.Drawing.Point(130, 4);
            this.ribbonBar11.Name = "ribbonBar11";
            this.ribbonBar11.Size = new System.Drawing.Size(686, 80);
            this.ribbonBar11.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ribbonBar11.TabIndex = 15;
            // 
            // 
            // 
            this.ribbonBar11.TitleStyle.Class = "";
            this.ribbonBar11.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBar11.TitleStyleMouseOver.Class = "";
            this.ribbonBar11.TitleStyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // itemContainer23
            // 
            // 
            // 
            // 
            this.itemContainer23.BackgroundStyle.Class = "";
            this.itemContainer23.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.itemContainer23.LayoutOrientation = DevComponents.DotNetBar.eOrientation.Vertical;
            this.itemContainer23.Name = "itemContainer23";
            this.itemContainer23.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonItemCycleNextShip,
            this.buttonItemCyclePrevShip,
            this.buttonItem19});
            // 
            // buttonItemCycleNextShip
            // 
            this.buttonItemCycleNextShip.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItemCycleNextShip.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemCycleNextShip.Image")));
            this.buttonItemCycleNextShip.Name = "buttonItemCycleNextShip";
            this.superTooltip1.SetSuperTooltip(this.buttonItemCycleNextShip, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonItemCycleNextShip.Text = "Next Ship";
            // 
            // buttonItemCyclePrevShip
            // 
            this.buttonItemCyclePrevShip.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItemCyclePrevShip.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemCyclePrevShip.Image")));
            this.buttonItemCyclePrevShip.Name = "buttonItemCyclePrevShip";
            this.superTooltip1.SetSuperTooltip(this.buttonItemCyclePrevShip, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonItemCyclePrevShip.Text = "Prev Ship";
            // 
            // buttonItem19
            // 
            this.buttonItem19.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItem19.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem19.Image")));
            this.buttonItem19.Name = "buttonItem19";
            this.superTooltip1.SetSuperTooltip(this.buttonItem19, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonItem19.Text = "Scale Bias Output";
            // 
            // itemContainer24
            // 
            // 
            // 
            // 
            this.itemContainer24.BackgroundStyle.Class = "";
            this.itemContainer24.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.itemContainer24.LayoutOrientation = DevComponents.DotNetBar.eOrientation.Vertical;
            this.itemContainer24.Name = "itemContainer24";
            this.itemContainer24.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonItem25,
            this.buttonItem40,
            this.buttonItem41});
            // 
            // buttonItem25
            // 
            this.buttonItem25.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItem25.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem25.Image")));
            this.buttonItem25.Name = "buttonItem25";
            this.superTooltip1.SetSuperTooltip(this.buttonItem25, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonItem25.Text = "Orbit Nearest Body";
            // 
            // buttonItem40
            // 
            this.buttonItem40.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItem40.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem40.Image")));
            this.buttonItem40.Name = "buttonItem40";
            this.superTooltip1.SetSuperTooltip(this.buttonItem40, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonItem40.Text = "Persuit Nearest Ship";
            // 
            // buttonItem41
            // 
            this.buttonItem41.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItem41.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem41.Image")));
            this.buttonItem41.Name = "buttonItem41";
            this.superTooltip1.SetSuperTooltip(this.buttonItem41, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonItem41.Text = "Fast Turbulence";
            // 
            // itemContainer25
            // 
            // 
            // 
            // 
            this.itemContainer25.BackgroundStyle.Class = "";
            this.itemContainer25.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.itemContainer25.LayoutOrientation = DevComponents.DotNetBar.eOrientation.Vertical;
            this.itemContainer25.Name = "itemContainer25";
            this.itemContainer25.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonIncreaseVelocity,
            this.buttonDecreaseVelocity,
            this.buttonStop});
            // 
            // buttonIncreaseVelocity
            // 
            this.buttonIncreaseVelocity.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonIncreaseVelocity.Image = ((System.Drawing.Image)(resources.GetObject("buttonIncreaseVelocity.Image")));
            this.buttonIncreaseVelocity.Name = "buttonIncreaseVelocity";
            this.superTooltip1.SetSuperTooltip(this.buttonIncreaseVelocity, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonIncreaseVelocity.Text = "Increase Velocity";
            this.buttonIncreaseVelocity.Click += new System.EventHandler(this.buttonIncreaseVelocity_Click);
            // 
            // buttonDecreaseVelocity
            // 
            this.buttonDecreaseVelocity.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonDecreaseVelocity.Image = ((System.Drawing.Image)(resources.GetObject("buttonDecreaseVelocity.Image")));
            this.buttonDecreaseVelocity.Name = "buttonDecreaseVelocity";
            this.superTooltip1.SetSuperTooltip(this.buttonDecreaseVelocity, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonDecreaseVelocity.Text = "Decrease Velocity";
            this.buttonDecreaseVelocity.Click += new System.EventHandler(this.buttonDecreaseVelocity_Click);
            // 
            // buttonStop
            // 
            this.buttonStop.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonStop.Image = ((System.Drawing.Image)(resources.GetObject("buttonStop.Image")));
            this.buttonStop.Name = "buttonStop";
            this.superTooltip1.SetSuperTooltip(this.buttonStop, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonStop.Text = "Stop";
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // itemContainer35
            // 
            // 
            // 
            // 
            this.itemContainer35.BackgroundStyle.Class = "";
            this.itemContainer35.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.itemContainer35.LayoutOrientation = DevComponents.DotNetBar.eOrientation.Vertical;
            this.itemContainer35.Name = "itemContainer35";
            this.itemContainer35.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonItem45,
            this.buttonItem46,
            this.buttonItem55});
            // 
            // buttonItem45
            // 
            this.buttonItem45.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItem45.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem45.Image")));
            this.buttonItem45.Name = "buttonItem45";
            this.superTooltip1.SetSuperTooltip(this.buttonItem45, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonItem45.Text = "Place Portal";
            this.buttonItem45.Click += new System.EventHandler(this.buttonItem45_Click);
            // 
            // buttonItem46
            // 
            this.buttonItem46.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItem46.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem46.Image")));
            this.buttonItem46.Name = "buttonItem46";
            this.superTooltip1.SetSuperTooltip(this.buttonItem46, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonItem46.Text = "Place Occluder";
            this.buttonItem46.Click += new System.EventHandler(this.buttonItem46_Click);
            // 
            // buttonItem55
            // 
            this.buttonItem55.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItem55.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem55.Image")));
            this.buttonItem55.Name = "buttonItem55";
            this.superTooltip1.SetSuperTooltip(this.buttonItem55, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonItem55.Text = "TBD";
            // 
            // ribbonBar14
            // 
            this.ribbonBar14.AutoOverflowEnabled = true;
            // 
            // 
            // 
            this.ribbonBar14.BackgroundMouseOverStyle.Class = "";
            this.ribbonBar14.BackgroundMouseOverStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBar14.BackgroundStyle.Class = "";
            this.ribbonBar14.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonBar14.ContainerControlProcessDialogKey = true;
            this.ribbonBar14.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.itemContainer29});
            this.ribbonBar14.ItemSpacing = 4;
            this.ribbonBar14.Location = new System.Drawing.Point(3, 3);
            this.ribbonBar14.Name = "ribbonBar14";
            this.ribbonBar14.Size = new System.Drawing.Size(124, 76);
            this.ribbonBar14.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ribbonBar14.TabIndex = 14;
            // 
            // 
            // 
            this.ribbonBar14.TitleStyle.Class = "";
            this.ribbonBar14.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBar14.TitleStyleMouseOver.Class = "";
            this.ribbonBar14.TitleStyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // itemContainer29
            // 
            // 
            // 
            // 
            this.itemContainer29.BackgroundStyle.Class = "";
            this.itemContainer29.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.itemContainer29.LayoutOrientation = DevComponents.DotNetBar.eOrientation.Vertical;
            this.itemContainer29.Name = "itemContainer29";
            this.itemContainer29.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.checkBoxItem13,
            this.checkBoxItem14,
            this.checkBoxItem15});
            // 
            // checkBoxItem13
            // 
            this.checkBoxItem13.Name = "checkBoxItem13";
            this.checkBoxItem13.Text = "Create Normal Map";
            this.checkBoxItem13.ThreeState = true;
            // 
            // checkBoxItem14
            // 
            this.checkBoxItem14.Name = "checkBoxItem14";
            this.checkBoxItem14.Text = "Use Palette";
            // 
            // checkBoxItem15
            // 
            this.checkBoxItem15.Name = "checkBoxItem15";
            this.checkBoxItem15.Text = "Random Seed";
            // 
            // ribbonPanel5
            // 
            this.ribbonPanel5.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ribbonPanel5.Controls.Add(this.ribbonBar7);
            this.ribbonPanel5.Controls.Add(this.ribbonBar4);
            this.ribbonPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ribbonPanel5.Location = new System.Drawing.Point(0, 53);
            this.ribbonPanel5.Name = "ribbonPanel5";
            this.ribbonPanel5.Padding = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.ribbonPanel5.Size = new System.Drawing.Size(1014, 89);
            // 
            // 
            // 
            this.ribbonPanel5.Style.Class = "";
            this.ribbonPanel5.Style.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonPanel5.StyleMouseDown.Class = "";
            this.ribbonPanel5.StyleMouseDown.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonPanel5.StyleMouseOver.Class = "";
            this.ribbonPanel5.StyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonPanel5.TabIndex = 9;
            this.ribbonPanel5.Visible = false;
            // 
            // ribbonBar7
            // 
            this.ribbonBar7.AutoOverflowEnabled = true;
            // 
            // 
            // 
            this.ribbonBar7.BackgroundMouseOverStyle.Class = "";
            this.ribbonBar7.BackgroundMouseOverStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBar7.BackgroundStyle.Class = "";
            this.ribbonBar7.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonBar7.ContainerControlProcessDialogKey = true;
            this.ribbonBar7.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonItemStar,
            this.buttonItemBluePlanet,
            this.buttonItemGasPlanet,
            this.buttonItemPlanetoidField,
            this.buttonItemMoon,
            this.buttonItemSolSystem});
            this.ribbonBar7.Location = new System.Drawing.Point(347, 4);
            this.ribbonBar7.Name = "ribbonBar7";
            this.ribbonBar7.Size = new System.Drawing.Size(367, 65);
            this.ribbonBar7.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ribbonBar7.TabIndex = 3;
            // 
            // 
            // 
            this.ribbonBar7.TitleStyle.Class = "";
            this.ribbonBar7.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBar7.TitleStyleMouseOver.Class = "";
            this.ribbonBar7.TitleStyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // buttonItemStar
            // 
            this.buttonItemStar.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItemStar.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemStar.Image")));
            this.buttonItemStar.ImagePosition = DevComponents.DotNetBar.eImagePosition.Top;
            this.buttonItemStar.Name = "buttonItemStar";
            this.buttonItemStar.RibbonWordWrap = false;
            this.buttonItemStar.Text = "Star";
            this.buttonItemStar.Click += new System.EventHandler(this.buttonItemStar_Click);
            // 
            // buttonItemBluePlanet
            // 
            this.buttonItemBluePlanet.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItemBluePlanet.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemBluePlanet.Image")));
            this.buttonItemBluePlanet.ImagePosition = DevComponents.DotNetBar.eImagePosition.Top;
            this.buttonItemBluePlanet.Name = "buttonItemBluePlanet";
            this.buttonItemBluePlanet.RibbonWordWrap = false;
            this.buttonItemBluePlanet.Text = "Blue Planet";
            this.buttonItemBluePlanet.Click += new System.EventHandler(this.buttonItemBluePlanet_Click);
            // 
            // buttonItemGasPlanet
            // 
            this.buttonItemGasPlanet.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItemGasPlanet.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemGasPlanet.Image")));
            this.buttonItemGasPlanet.ImagePosition = DevComponents.DotNetBar.eImagePosition.Top;
            this.buttonItemGasPlanet.Name = "buttonItemGasPlanet";
            this.buttonItemGasPlanet.RibbonWordWrap = false;
            this.buttonItemGasPlanet.Text = "Gas Giant";
            this.buttonItemGasPlanet.Click += new System.EventHandler(this.buttonItemGasPlanet_Click);
            // 
            // buttonItemPlanetoidField
            // 
            this.buttonItemPlanetoidField.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItemPlanetoidField.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemPlanetoidField.Image")));
            this.buttonItemPlanetoidField.ImagePosition = DevComponents.DotNetBar.eImagePosition.Top;
            this.buttonItemPlanetoidField.Name = "buttonItemPlanetoidField";
            this.buttonItemPlanetoidField.RibbonWordWrap = false;
            this.buttonItemPlanetoidField.Text = "Planetoid Field";
            this.buttonItemPlanetoidField.Click += new System.EventHandler(this.buttonItemPlanetoidField_Click);
            // 
            // buttonItemMoon
            // 
            this.buttonItemMoon.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItemMoon.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemMoon.Image")));
            this.buttonItemMoon.ImagePosition = DevComponents.DotNetBar.eImagePosition.Top;
            this.buttonItemMoon.Name = "buttonItemMoon";
            this.buttonItemMoon.RibbonWordWrap = false;
            this.buttonItemMoon.Text = "Moon";
            this.buttonItemMoon.Click += new System.EventHandler(this.buttonItemMoon_Click);
            // 
            // buttonItemSolSystem
            // 
            this.buttonItemSolSystem.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItemSolSystem.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemSolSystem.Image")));
            this.buttonItemSolSystem.ImagePosition = DevComponents.DotNetBar.eImagePosition.Top;
            this.buttonItemSolSystem.Name = "buttonItemSolSystem";
            this.buttonItemSolSystem.RibbonWordWrap = false;
            this.buttonItemSolSystem.Text = "Sol System";
            this.buttonItemSolSystem.Click += new System.EventHandler(this.buttonItemSolSystem_Click);
            // 
            // ribbonBar4
            // 
            this.ribbonBar4.AutoOverflowEnabled = true;
            // 
            // 
            // 
            this.ribbonBar4.BackgroundMouseOverStyle.Class = "";
            this.ribbonBar4.BackgroundMouseOverStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBar4.BackgroundStyle.Class = "";
            this.ribbonBar4.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonBar4.ContainerControlProcessDialogKey = true;
            this.ribbonBar4.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonItemStarFieldNoTexture,
            this.buttonItemStarfield,
            this.buttonItemDustField,
            this.buttonItemSkybox});
            this.ribbonBar4.Location = new System.Drawing.Point(3, 3);
            this.ribbonBar4.Name = "ribbonBar4";
            this.ribbonBar4.Size = new System.Drawing.Size(307, 65);
            this.ribbonBar4.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ribbonBar4.TabIndex = 2;
            // 
            // 
            // 
            this.ribbonBar4.TitleStyle.Class = "";
            this.ribbonBar4.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBar4.TitleStyleMouseOver.Class = "";
            this.ribbonBar4.TitleStyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // buttonItemStarFieldNoTexture
            // 
            this.buttonItemStarFieldNoTexture.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItemStarFieldNoTexture.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemStarFieldNoTexture.Image")));
            this.buttonItemStarFieldNoTexture.ImagePosition = DevComponents.DotNetBar.eImagePosition.Top;
            this.buttonItemStarFieldNoTexture.Name = "buttonItemStarFieldNoTexture";
            this.buttonItemStarFieldNoTexture.RibbonWordWrap = false;
            this.buttonItemStarFieldNoTexture.Text = "Star Field (No Texture)";
            this.buttonItemStarFieldNoTexture.Click += new System.EventHandler(this.buttonItemStarFieldNoTexture_Click);
            // 
            // buttonItemStarfield
            // 
            this.buttonItemStarfield.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItemStarfield.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemStarfield.Image")));
            this.buttonItemStarfield.ImagePosition = DevComponents.DotNetBar.eImagePosition.Top;
            this.buttonItemStarfield.Name = "buttonItemStarfield";
            this.buttonItemStarfield.RibbonWordWrap = false;
            this.buttonItemStarfield.Text = "Star Field";
            this.buttonItemStarfield.Click += new System.EventHandler(this.buttonItemStarfield_Click);
            // 
            // buttonItemDustField
            // 
            this.buttonItemDustField.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItemDustField.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemDustField.Image")));
            this.buttonItemDustField.ImagePosition = DevComponents.DotNetBar.eImagePosition.Top;
            this.buttonItemDustField.Name = "buttonItemDustField";
            this.buttonItemDustField.RibbonWordWrap = false;
            this.buttonItemDustField.Text = "Dust Field";
            this.buttonItemDustField.Click += new System.EventHandler(this.buttonItemDustField_Click);
            // 
            // buttonItemSkybox
            // 
            this.buttonItemSkybox.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItemSkybox.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemSkybox.Image")));
            this.buttonItemSkybox.ImagePosition = DevComponents.DotNetBar.eImagePosition.Top;
            this.buttonItemSkybox.Name = "buttonItemSkybox";
            this.buttonItemSkybox.RibbonWordWrap = false;
            this.buttonItemSkybox.Text = "Skybox";
            this.buttonItemSkybox.Click += new System.EventHandler(this.buttonItemSkybox_Click);
            // 
            // ribbonPanel1
            // 
            this.ribbonPanel1.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ribbonPanel1.Controls.Add(this.ribbonBar17);
            this.ribbonPanel1.Controls.Add(this.ribbonBar2);
            this.ribbonPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ribbonPanel1.Location = new System.Drawing.Point(0, 53);
            this.ribbonPanel1.Name = "ribbonPanel1";
            this.ribbonPanel1.Padding = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.ribbonPanel1.Size = new System.Drawing.Size(1014, 72);
            // 
            // 
            // 
            this.ribbonPanel1.Style.Class = "";
            this.ribbonPanel1.Style.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonPanel1.StyleMouseDown.Class = "";
            this.ribbonPanel1.StyleMouseDown.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonPanel1.StyleMouseOver.Class = "";
            this.ribbonPanel1.StyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonPanel1.TabIndex = 5;
            this.ribbonPanel1.Visible = false;
            // 
            // ribbonBar17
            // 
            this.ribbonBar17.AutoOverflowEnabled = true;
            // 
            // 
            // 
            this.ribbonBar17.BackgroundMouseOverStyle.Class = "";
            this.ribbonBar17.BackgroundMouseOverStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBar17.BackgroundStyle.Class = "";
            this.ribbonBar17.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonBar17.ContainerControlProcessDialogKey = true;
            this.ribbonBar17.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.itemContainer30,
            this.itemContainer31,
            this.itemContainer34,
            this.itemContainer32,
            this.itemContainer33});
            this.ribbonBar17.ItemSpacing = 4;
            this.ribbonBar17.Location = new System.Drawing.Point(229, 0);
            this.ribbonBar17.Name = "ribbonBar17";
            this.ribbonBar17.Size = new System.Drawing.Size(394, 83);
            this.ribbonBar17.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ribbonBar17.TabIndex = 14;
            // 
            // 
            // 
            this.ribbonBar17.TitleStyle.Class = "";
            this.ribbonBar17.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBar17.TitleStyleMouseOver.Class = "";
            this.ribbonBar17.TitleStyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // itemContainer30
            // 
            // 
            // 
            // 
            this.itemContainer30.BackgroundStyle.Class = "";
            this.itemContainer30.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.itemContainer30.LayoutOrientation = DevComponents.DotNetBar.eOrientation.Vertical;
            this.itemContainer30.Name = "itemContainer30";
            this.itemContainer30.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonSpherePrimitive,
            this.buttonBoxPrimitive,
            this.buttonCylinderPrimitive});
            // 
            // buttonSpherePrimitive
            // 
            this.buttonSpherePrimitive.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonSpherePrimitive.Image = ((System.Drawing.Image)(resources.GetObject("buttonSpherePrimitive.Image")));
            this.buttonSpherePrimitive.Name = "buttonSpherePrimitive";
            this.superTooltip1.SetSuperTooltip(this.buttonSpherePrimitive, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonSpherePrimitive.Text = "Sphere";
            this.buttonSpherePrimitive.Click += new System.EventHandler(this.buttonSpherePrimitive_Click);
            // 
            // buttonBoxPrimitive
            // 
            this.buttonBoxPrimitive.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonBoxPrimitive.Image = ((System.Drawing.Image)(resources.GetObject("buttonBoxPrimitive.Image")));
            this.buttonBoxPrimitive.Name = "buttonBoxPrimitive";
            this.superTooltip1.SetSuperTooltip(this.buttonBoxPrimitive, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonBoxPrimitive.Text = "Box";
            this.buttonBoxPrimitive.Click += new System.EventHandler(this.buttonBoxPrimitive_Click);
            // 
            // buttonCylinderPrimitive
            // 
            this.buttonCylinderPrimitive.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonCylinderPrimitive.Image = ((System.Drawing.Image)(resources.GetObject("buttonCylinderPrimitive.Image")));
            this.buttonCylinderPrimitive.Name = "buttonCylinderPrimitive";
            this.superTooltip1.SetSuperTooltip(this.buttonCylinderPrimitive, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonCylinderPrimitive.Text = "Cylinder";
            this.buttonCylinderPrimitive.Click += new System.EventHandler(this.buttonCylinderPrimitive_Click);
            // 
            // itemContainer31
            // 
            // 
            // 
            // 
            this.itemContainer31.BackgroundStyle.Class = "";
            this.itemContainer31.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.itemContainer31.LayoutOrientation = DevComponents.DotNetBar.eOrientation.Vertical;
            this.itemContainer31.Name = "itemContainer31";
            this.itemContainer31.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonPyramidPrimitive,
            this.buttonTeapotPrimitive,
            this.buttonTorusPrimitive});
            // 
            // buttonPyramidPrimitive
            // 
            this.buttonPyramidPrimitive.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonPyramidPrimitive.Image = ((System.Drawing.Image)(resources.GetObject("buttonPyramidPrimitive.Image")));
            this.buttonPyramidPrimitive.Name = "buttonPyramidPrimitive";
            this.superTooltip1.SetSuperTooltip(this.buttonPyramidPrimitive, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonPyramidPrimitive.Text = "Cone";
            this.buttonPyramidPrimitive.Click += new System.EventHandler(this.buttonPyramidPrimitive_Click);
            // 
            // buttonTeapotPrimitive
            // 
            this.buttonTeapotPrimitive.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonTeapotPrimitive.Image = ((System.Drawing.Image)(resources.GetObject("buttonTeapotPrimitive.Image")));
            this.buttonTeapotPrimitive.Name = "buttonTeapotPrimitive";
            this.superTooltip1.SetSuperTooltip(this.buttonTeapotPrimitive, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonTeapotPrimitive.Text = "Capsule";
            this.buttonTeapotPrimitive.Click += new System.EventHandler(this.buttonTeapotPrimitive_Click);
            // 
            // buttonTorusPrimitive
            // 
            this.buttonTorusPrimitive.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonTorusPrimitive.Image = ((System.Drawing.Image)(resources.GetObject("buttonTorusPrimitive.Image")));
            this.buttonTorusPrimitive.Name = "buttonTorusPrimitive";
            this.superTooltip1.SetSuperTooltip(this.buttonTorusPrimitive, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonTorusPrimitive.Text = "Torus";
            this.buttonTorusPrimitive.Click += new System.EventHandler(this.buttonTorusPrimitive_Click);
            // 
            // itemContainer34
            // 
            // 
            // 
            // 
            this.itemContainer34.BackgroundStyle.Class = "";
            this.itemContainer34.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.itemContainer34.LayoutOrientation = DevComponents.DotNetBar.eOrientation.Vertical;
            this.itemContainer34.Name = "itemContainer34";
            this.itemContainer34.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonNewComet,
            this.buttonItem72,
            this.buttonItem73});
            // 
            // buttonNewComet
            // 
            this.buttonNewComet.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonNewComet.Image = ((System.Drawing.Image)(resources.GetObject("buttonNewComet.Image")));
            this.buttonNewComet.Name = "buttonNewComet";
            this.superTooltip1.SetSuperTooltip(this.buttonNewComet, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonNewComet.Text = "Pyramid(3)";
            this.buttonNewComet.Click += new System.EventHandler(this.buttonNewComet_Click);
            // 
            // buttonItem72
            // 
            this.buttonItem72.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItem72.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem72.Image")));
            this.buttonItem72.Name = "buttonItem72";
            this.superTooltip1.SetSuperTooltip(this.buttonItem72, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonItem72.Text = "Pyramid(4)";
            // 
            // buttonItem73
            // 
            this.buttonItem73.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItem73.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem73.Image")));
            this.buttonItem73.Name = "buttonItem73";
            this.superTooltip1.SetSuperTooltip(this.buttonItem73, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonItem73.Text = "Teapot";
            // 
            // itemContainer32
            // 
            // 
            // 
            // 
            this.itemContainer32.BackgroundStyle.Class = "";
            this.itemContainer32.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.itemContainer32.LayoutOrientation = DevComponents.DotNetBar.eOrientation.Vertical;
            this.itemContainer32.Name = "itemContainer32";
            this.itemContainer32.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonNewStar,
            this.buttonNewWorld,
            this.buttonNewAsteroidBelt});
            // 
            // buttonNewStar
            // 
            this.buttonNewStar.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonNewStar.Image = ((System.Drawing.Image)(resources.GetObject("buttonNewStar.Image")));
            this.buttonNewStar.Name = "buttonNewStar";
            this.superTooltip1.SetSuperTooltip(this.buttonNewStar, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonNewStar.Text = "Star";
            this.buttonNewStar.Click += new System.EventHandler(this.buttonNewStar_Click);
            // 
            // buttonNewWorld
            // 
            this.buttonNewWorld.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonNewWorld.Image = ((System.Drawing.Image)(resources.GetObject("buttonNewWorld.Image")));
            this.buttonNewWorld.Name = "buttonNewWorld";
            this.superTooltip1.SetSuperTooltip(this.buttonNewWorld, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonNewWorld.Text = "World";
            this.buttonNewWorld.Click += new System.EventHandler(this.buttonNewWorld_Click);
            // 
            // buttonNewAsteroidBelt
            // 
            this.buttonNewAsteroidBelt.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonNewAsteroidBelt.Image = ((System.Drawing.Image)(resources.GetObject("buttonNewAsteroidBelt.Image")));
            this.buttonNewAsteroidBelt.Name = "buttonNewAsteroidBelt";
            this.superTooltip1.SetSuperTooltip(this.buttonNewAsteroidBelt, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonNewAsteroidBelt.Text = "Asteroid Belt";
            this.buttonNewAsteroidBelt.Click += new System.EventHandler(this.buttonNewAsteroidBelt_Click);
            // 
            // itemContainer33
            // 
            // 
            // 
            // 
            this.itemContainer33.BackgroundStyle.Class = "";
            this.itemContainer33.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.itemContainer33.LayoutOrientation = DevComponents.DotNetBar.eOrientation.Vertical;
            this.itemContainer33.Name = "itemContainer33";
            this.itemContainer33.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonItem6,
            this.buttonItem7,
            this.buttonItem42});
            // 
            // buttonItem6
            // 
            this.buttonItem6.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItem6.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem6.Image")));
            this.buttonItem6.Name = "buttonItem6";
            this.superTooltip1.SetSuperTooltip(this.buttonItem6, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonItem6.Text = "Nebula";
            // 
            // buttonItem7
            // 
            this.buttonItem7.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItem7.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem7.Image")));
            this.buttonItem7.Name = "buttonItem7";
            this.superTooltip1.SetSuperTooltip(this.buttonItem7, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonItem7.Text = "Comet";
            // 
            // buttonItem42
            // 
            this.buttonItem42.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItem42.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem42.Image")));
            this.buttonItem42.Name = "buttonItem42";
            this.superTooltip1.SetSuperTooltip(this.buttonItem42, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonItem42.Text = "Asteroid Belt";
            // 
            // ribbonBar2
            // 
            this.ribbonBar2.AutoOverflowEnabled = true;
            // 
            // 
            // 
            this.ribbonBar2.BackgroundMouseOverStyle.Class = "";
            this.ribbonBar2.BackgroundMouseOverStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBar2.BackgroundStyle.Class = "";
            this.ribbonBar2.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonBar2.ContainerControlProcessDialogKey = true;
            this.ribbonBar2.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonItem2,
            this.buttonItem3,
            this.buttonItem4});
            this.ribbonBar2.Location = new System.Drawing.Point(3, 3);
            this.ribbonBar2.Name = "ribbonBar2";
            this.ribbonBar2.Size = new System.Drawing.Size(220, 65);
            this.ribbonBar2.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ribbonBar2.TabIndex = 1;
            // 
            // 
            // 
            this.ribbonBar2.TitleStyle.Class = "";
            this.ribbonBar2.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBar2.TitleStyleMouseOver.Class = "";
            this.ribbonBar2.TitleStyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // buttonItem2
            // 
            this.buttonItem2.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItem2.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem2.Image")));
            this.buttonItem2.ImagePosition = DevComponents.DotNetBar.eImagePosition.Top;
            this.buttonItem2.Name = "buttonItem2";
            this.buttonItem2.RibbonWordWrap = false;
            this.buttonItem2.Text = "Command 1";
            // 
            // buttonItem3
            // 
            this.buttonItem3.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItem3.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem3.Image")));
            this.buttonItem3.ImagePosition = DevComponents.DotNetBar.eImagePosition.Top;
            this.buttonItem3.Name = "buttonItem3";
            this.buttonItem3.RibbonWordWrap = false;
            this.buttonItem3.Text = "Command 2";
            // 
            // buttonItem4
            // 
            this.buttonItem4.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItem4.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem4.Image")));
            this.buttonItem4.ImagePosition = DevComponents.DotNetBar.eImagePosition.Top;
            this.buttonItem4.Name = "buttonItem4";
            this.buttonItem4.RibbonWordWrap = false;
            this.buttonItem4.Text = "Command 3";
            // 
            // ribbonPanelEdit
            // 
            this.ribbonPanelEdit.AutoSize = true;
            this.ribbonPanelEdit.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ribbonPanelEdit.Controls.Add(this.ribbonBar16);
            this.ribbonPanelEdit.Controls.Add(this.ribbonBar5);
            this.ribbonPanelEdit.Controls.Add(this.labelEditTabNotesTemp);
            this.ribbonPanelEdit.Controls.Add(this.ribbonBar8);
            this.ribbonPanelEdit.Controls.Add(this.ribbonBar1);
            this.ribbonPanelEdit.Controls.Add(this.ribbonBarSearch);
            this.ribbonPanelEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ribbonPanelEdit.Location = new System.Drawing.Point(0, 53);
            this.ribbonPanelEdit.Name = "ribbonPanelEdit";
            this.ribbonPanelEdit.Padding = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.ribbonPanelEdit.Size = new System.Drawing.Size(1014, 89);
            // 
            // 
            // 
            this.ribbonPanelEdit.Style.Class = "";
            this.ribbonPanelEdit.Style.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonPanelEdit.StyleMouseDown.Class = "";
            this.ribbonPanelEdit.StyleMouseDown.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonPanelEdit.StyleMouseOver.Class = "";
            this.ribbonPanelEdit.StyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonPanelEdit.TabIndex = 3;
            this.ribbonPanelEdit.Visible = false;
            // 
            // ribbonBar16
            // 
            this.ribbonBar16.AutoOverflowEnabled = true;
            // 
            // 
            // 
            this.ribbonBar16.BackgroundMouseOverStyle.Class = "";
            this.ribbonBar16.BackgroundMouseOverStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBar16.BackgroundStyle.Class = "";
            this.ribbonBar16.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonBar16.ContainerControlProcessDialogKey = true;
            this.ribbonBar16.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.itemContainer26,
            this.itemContainer27,
            this.itemContainer28});
            this.ribbonBar16.ItemSpacing = 4;
            this.ribbonBar16.Location = new System.Drawing.Point(680, 0);
            this.ribbonBar16.Name = "ribbonBar16";
            this.ribbonBar16.Size = new System.Drawing.Size(370, 80);
            this.ribbonBar16.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ribbonBar16.TabIndex = 16;
            // 
            // 
            // 
            this.ribbonBar16.TitleStyle.Class = "";
            this.ribbonBar16.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBar16.TitleStyleMouseOver.Class = "";
            this.ribbonBar16.TitleStyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // itemContainer26
            // 
            // 
            // 
            // 
            this.itemContainer26.BackgroundStyle.Class = "";
            this.itemContainer26.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.itemContainer26.LayoutOrientation = DevComponents.DotNetBar.eOrientation.Vertical;
            this.itemContainer26.Name = "itemContainer26";
            this.itemContainer26.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonSelectTool,
            this.buttonMoveTool,
            this.buttonRotateTool});
            // 
            // buttonSelectTool
            // 
            this.buttonSelectTool.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonSelectTool.Image = ((System.Drawing.Image)(resources.GetObject("buttonSelectTool.Image")));
            this.buttonSelectTool.Name = "buttonSelectTool";
            this.superTooltip1.SetSuperTooltip(this.buttonSelectTool, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonSelectTool.Text = "Select";
            this.buttonSelectTool.Click += new System.EventHandler(this.OnToolboxItemSelected);
            // 
            // buttonMoveTool
            // 
            this.buttonMoveTool.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonMoveTool.Image = ((System.Drawing.Image)(resources.GetObject("buttonMoveTool.Image")));
            this.buttonMoveTool.Name = "buttonMoveTool";
            this.superTooltip1.SetSuperTooltip(this.buttonMoveTool, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonMoveTool.Text = "Move";
            this.buttonMoveTool.Click += new System.EventHandler(this.OnToolboxItemSelected);
            // 
            // buttonRotateTool
            // 
            this.buttonRotateTool.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonRotateTool.Image = ((System.Drawing.Image)(resources.GetObject("buttonRotateTool.Image")));
            this.buttonRotateTool.Name = "buttonRotateTool";
            this.superTooltip1.SetSuperTooltip(this.buttonRotateTool, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonRotateTool.Text = "Rotate";
            this.buttonRotateTool.Click += new System.EventHandler(this.OnToolboxItemSelected);
            // 
            // itemContainer27
            // 
            // 
            // 
            // 
            this.itemContainer27.BackgroundStyle.Class = "";
            this.itemContainer27.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.itemContainer27.LayoutOrientation = DevComponents.DotNetBar.eOrientation.Vertical;
            this.itemContainer27.Name = "itemContainer27";
            this.itemContainer27.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonScaleTool,
            this.buttonLineTool,
            this.buttonRectangleTool});
            // 
            // buttonScaleTool
            // 
            this.buttonScaleTool.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonScaleTool.Image = ((System.Drawing.Image)(resources.GetObject("buttonScaleTool.Image")));
            this.buttonScaleTool.Name = "buttonScaleTool";
            this.superTooltip1.SetSuperTooltip(this.buttonScaleTool, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonScaleTool.Text = "Scale";
            this.buttonScaleTool.Click += new System.EventHandler(this.OnToolboxItemSelected);
            // 
            // buttonLineTool
            // 
            this.buttonLineTool.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonLineTool.Image = ((System.Drawing.Image)(resources.GetObject("buttonLineTool.Image")));
            this.buttonLineTool.Name = "buttonLineTool";
            this.superTooltip1.SetSuperTooltip(this.buttonLineTool, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonLineTool.Text = "Line";
            this.buttonLineTool.Click += new System.EventHandler(this.OnToolboxItemSelected);
            // 
            // buttonRectangleTool
            // 
            this.buttonRectangleTool.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonRectangleTool.Image = ((System.Drawing.Image)(resources.GetObject("buttonRectangleTool.Image")));
            this.buttonRectangleTool.Name = "buttonRectangleTool";
            this.superTooltip1.SetSuperTooltip(this.buttonRectangleTool, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonRectangleTool.Text = "Rectangle";
            this.buttonRectangleTool.Click += new System.EventHandler(this.OnToolboxItemSelected);
            // 
            // itemContainer28
            // 
            // 
            // 
            // 
            this.itemContainer28.BackgroundStyle.Class = "";
            this.itemContainer28.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.itemContainer28.LayoutOrientation = DevComponents.DotNetBar.eOrientation.Vertical;
            this.itemContainer28.Name = "itemContainer28";
            this.itemContainer28.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonCircleTool,
            this.buttonPolygonTool,
            this.buttonThrowProjectile});
            // 
            // buttonCircleTool
            // 
            this.buttonCircleTool.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonCircleTool.Image = ((System.Drawing.Image)(resources.GetObject("buttonCircleTool.Image")));
            this.buttonCircleTool.Name = "buttonCircleTool";
            this.superTooltip1.SetSuperTooltip(this.buttonCircleTool, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonCircleTool.Text = "Cirlce";
            this.buttonCircleTool.Click += new System.EventHandler(this.OnToolboxItemSelected);
            // 
            // buttonPolygonTool
            // 
            this.buttonPolygonTool.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonPolygonTool.Image = ((System.Drawing.Image)(resources.GetObject("buttonPolygonTool.Image")));
            this.buttonPolygonTool.Name = "buttonPolygonTool";
            this.superTooltip1.SetSuperTooltip(this.buttonPolygonTool, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonPolygonTool.Text = "Polygon";
            this.buttonPolygonTool.Click += new System.EventHandler(this.OnToolboxItemSelected);
            // 
            // buttonThrowProjectile
            // 
            this.buttonThrowProjectile.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonThrowProjectile.Image = ((System.Drawing.Image)(resources.GetObject("buttonThrowProjectile.Image")));
            this.buttonThrowProjectile.Name = "buttonThrowProjectile";
            this.superTooltip1.SetSuperTooltip(this.buttonThrowProjectile, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonThrowProjectile.Text = "Projectile";
            this.buttonThrowProjectile.Click += new System.EventHandler(this.OnToolboxItemSelected);
            // 
            // ribbonBar5
            // 
            this.ribbonBar5.AutoOverflowEnabled = true;
            // 
            // 
            // 
            this.ribbonBar5.BackgroundMouseOverStyle.Class = "";
            this.ribbonBar5.BackgroundMouseOverStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBar5.BackgroundStyle.Class = "";
            this.ribbonBar5.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonBar5.ContainerControlProcessDialogKey = true;
            this.ribbonBar5.DialogLauncherVisible = true;
            this.ribbonBar5.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonMargins,
            this.buttonItem9,
            this.buttonItem10,
            this.buttonItem11});
            this.ribbonBar5.Location = new System.Drawing.Point(261, 6);
            this.ribbonBar5.Name = "ribbonBar5";
            this.ribbonBar5.Size = new System.Drawing.Size(222, 68);
            this.ribbonBar5.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ribbonBar5.TabIndex = 11;
            this.ribbonBar5.Text = "Widget Edit Mode";
            // 
            // 
            // 
            this.ribbonBar5.TitleStyle.Class = "";
            this.ribbonBar5.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBar5.TitleStyleMouseOver.Class = "";
            this.ribbonBar5.TitleStyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // buttonMargins
            // 
            this.buttonMargins.Image = ((System.Drawing.Image)(resources.GetObject("buttonMargins.Image")));
            this.buttonMargins.ImagePosition = DevComponents.DotNetBar.eImagePosition.Top;
            this.buttonMargins.Name = "buttonMargins";
            this.buttonMargins.Text = "Margins";
            // 
            // buttonItem9
            // 
            this.buttonItem9.AutoExpandOnClick = true;
            this.buttonItem9.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItem9.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem9.Image")));
            this.buttonItem9.ImagePosition = DevComponents.DotNetBar.eImagePosition.Top;
            this.buttonItem9.Name = "buttonItem9";
            this.buttonItem9.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonItem50,
            this.buttonItem51,
            this.buttonItem52});
            this.buttonItem9.Text = "Orientation <expand/>";
            // 
            // buttonItem50
            // 
            this.buttonItem50.Checked = true;
            this.buttonItem50.Name = "buttonItem50";
            this.buttonItem50.OptionGroup = "orientation";
            this.buttonItem50.Text = "Auto";
            // 
            // buttonItem51
            // 
            this.buttonItem51.Name = "buttonItem51";
            this.buttonItem51.OptionGroup = "orientation";
            this.buttonItem51.Text = "Horizontal";
            // 
            // buttonItem52
            // 
            this.buttonItem52.Name = "buttonItem52";
            this.buttonItem52.OptionGroup = "orientation";
            this.buttonItem52.Text = "Vertical";
            // 
            // buttonItem10
            // 
            this.buttonItem10.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem10.Image")));
            this.buttonItem10.ImagePosition = DevComponents.DotNetBar.eImagePosition.Top;
            this.buttonItem10.Name = "buttonItem10";
            this.buttonItem10.Text = "Size";
            // 
            // buttonItem11
            // 
            this.buttonItem11.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItem11.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem11.Image")));
            this.buttonItem11.ImagePosition = DevComponents.DotNetBar.eImagePosition.Top;
            this.buttonItem11.Name = "buttonItem11";
            this.buttonItem11.Text = "Print Area";
            // 
            // labelEditTabNotesTemp
            // 
            this.labelEditTabNotesTemp.AutoSize = true;
            this.labelEditTabNotesTemp.Location = new System.Drawing.Point(670, 73);
            this.labelEditTabNotesTemp.Name = "labelEditTabNotesTemp";
            this.labelEditTabNotesTemp.Size = new System.Drawing.Size(402, 13);
            this.labelEditTabNotesTemp.TabIndex = 10;
            this.labelEditTabNotesTemp.Text = "When in this Edit Tab, floorplan related options will be here if in floor plan ed" +
                "itor view";
            // 
            // ribbonBar8
            // 
            this.ribbonBar8.AutoOverflowEnabled = true;
            // 
            // 
            // 
            this.ribbonBar8.BackgroundMouseOverStyle.Class = "";
            this.ribbonBar8.BackgroundMouseOverStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBar8.BackgroundStyle.Class = "";
            this.ribbonBar8.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonBar8.ContainerControlProcessDialogKey = true;
            this.ribbonBar8.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.itemContainer10,
            this.itemContainer11});
            this.ribbonBar8.ItemSpacing = 4;
            this.ribbonBar8.Location = new System.Drawing.Point(489, 0);
            this.ribbonBar8.Name = "ribbonBar8";
            this.ribbonBar8.Size = new System.Drawing.Size(185, 78);
            this.ribbonBar8.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ribbonBar8.TabIndex = 9;
            this.ribbonBar8.Text = "Terrain Sculpting";
            // 
            // 
            // 
            this.ribbonBar8.TitleStyle.Class = "";
            this.ribbonBar8.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBar8.TitleStyleMouseOver.Class = "";
            this.ribbonBar8.TitleStyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // itemContainer10
            // 
            // 
            // 
            // 
            this.itemContainer10.BackgroundStyle.Class = "";
            this.itemContainer10.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.itemContainer10.LayoutOrientation = DevComponents.DotNetBar.eOrientation.Vertical;
            this.itemContainer10.Name = "itemContainer10";
            this.itemContainer10.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.checkBoxItem1,
            this.checkBoxItem2,
            this.checkBoxItem3});
            // 
            // checkBoxItem1
            // 
            this.checkBoxItem1.Checked = true;
            this.checkBoxItem1.CheckState = System.Windows.Forms.CheckState.Indeterminate;
            this.checkBoxItem1.Name = "checkBoxItem1";
            this.checkBoxItem1.Text = "Header";
            this.checkBoxItem1.ThreeState = true;
            // 
            // checkBoxItem2
            // 
            this.checkBoxItem2.Name = "checkBoxItem2";
            this.checkBoxItem2.Text = "Footer";
            // 
            // checkBoxItem3
            // 
            this.checkBoxItem3.Checked = true;
            this.checkBoxItem3.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxItem3.Name = "checkBoxItem3";
            this.checkBoxItem3.Text = "Margins";
            // 
            // itemContainer11
            // 
            // 
            // 
            // 
            this.itemContainer11.BackgroundStyle.Class = "";
            this.itemContainer11.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.itemContainer11.LayoutOrientation = DevComponents.DotNetBar.eOrientation.Vertical;
            this.itemContainer11.MultiLine = true;
            this.itemContainer11.Name = "itemContainer11";
            this.itemContainer11.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.checkBoxItem4,
            this.checkBoxItem5,
            this.checkBoxItem6});
            // 
            // checkBoxItem4
            // 
            this.checkBoxItem4.CheckBoxStyle = DevComponents.DotNetBar.eCheckBoxStyle.RadioButton;
            this.checkBoxItem4.Name = "checkBoxItem4";
            this.checkBoxItem4.Text = "Round Brush";
            // 
            // checkBoxItem5
            // 
            this.checkBoxItem5.CheckBoxStyle = DevComponents.DotNetBar.eCheckBoxStyle.RadioButton;
            this.checkBoxItem5.Name = "checkBoxItem5";
            this.checkBoxItem5.Text = "Square Brush";
            // 
            // checkBoxItem6
            // 
            this.checkBoxItem6.CheckBoxStyle = DevComponents.DotNetBar.eCheckBoxStyle.RadioButton;
            this.checkBoxItem6.Checked = true;
            this.checkBoxItem6.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxItem6.Name = "checkBoxItem6";
            this.checkBoxItem6.Text = "Automatic Layout";
            // 
            // ribbonBar1
            // 
            this.ribbonBar1.AutoOverflowEnabled = true;
            // 
            // 
            // 
            this.ribbonBar1.BackgroundMouseOverStyle.Class = "";
            this.ribbonBar1.BackgroundMouseOverStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBar1.BackgroundStyle.Class = "";
            this.ribbonBar1.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonBar1.ContainerControlProcessDialogKey = true;
            this.ribbonBar1.DialogLauncherVisible = true;
            this.ribbonBar1.Dock = System.Windows.Forms.DockStyle.Left;
            this.ribbonBar1.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonPaste,
            this.itemContainer1});
            this.ribbonBar1.Location = new System.Drawing.Point(3, 0);
            this.ribbonBar1.Name = "ribbonBar1";
            this.ribbonBar1.Size = new System.Drawing.Size(135, 86);
            this.ribbonBar1.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.superTooltip1.SetSuperTooltip(this.ribbonBar1, new DevComponents.DotNetBar.SuperTooltipInfo("SuperTooltip for Dialog Launcher", "", "Assigning the Super Tooltip to the Ribbon Bar control will display it when mouse " +
                        "hovers over the Dialog Launcher button.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.ribbonBar1.TabIndex = 5;
            this.ribbonBar1.Text = "&Clipboard";
            // 
            // 
            // 
            this.ribbonBar1.TitleStyle.Class = "";
            this.ribbonBar1.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBar1.TitleStyleMouseOver.Class = "";
            this.ribbonBar1.TitleStyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // buttonPaste
            // 
            this.buttonPaste.Image = ((System.Drawing.Image)(resources.GetObject("buttonPaste.Image")));
            this.buttonPaste.ImagePosition = DevComponents.DotNetBar.eImagePosition.Top;
            this.buttonPaste.Name = "buttonPaste";
            this.buttonPaste.SplitButton = true;
            this.buttonPaste.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonItem53,
            this.buttonItem54});
            this.superTooltip1.SetSuperTooltip(this.buttonPaste, new DevComponents.DotNetBar.SuperTooltipInfo("Paste (Ctrl+V)", "", "Paste text from clipboard.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonPaste.Text = "&Paste";
            // 
            // buttonItem53
            // 
            this.buttonItem53.Enabled = false;
            this.buttonItem53.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem53.Image")));
            this.buttonItem53.Name = "buttonItem53";
            this.buttonItem53.Text = "&Paste";
            // 
            // buttonItem54
            // 
            this.buttonItem54.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem54.Image")));
            this.buttonItem54.Name = "buttonItem54";
            this.buttonItem54.Text = "Paste &Special...";
            // 
            // itemContainer1
            // 
            // 
            // 
            // 
            this.itemContainer1.BackgroundStyle.Class = "";
            this.itemContainer1.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.itemContainer1.ItemSpacing = 0;
            this.itemContainer1.MultiLine = true;
            this.itemContainer1.Name = "itemContainer1";
            this.itemContainer1.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonCut,
            this.buttonCopy,
            this.buttonItemPaste,
            this.buttonItemUndo,
            this.buttonItemRedo});
            // 
            // buttonCut
            // 
            this.buttonCut.Image = ((System.Drawing.Image)(resources.GetObject("buttonCut.Image")));
            this.buttonCut.Name = "buttonCut";
            this.superTooltip1.SetSuperTooltip(this.buttonCut, new DevComponents.DotNetBar.SuperTooltipInfo("Cut (Ctrl+X)", "", "Removes selected text and copies it to clipboard.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonCut.Text = "Cu&t";
            this.buttonCut.Click += new System.EventHandler(this.buttonCut_Click);
            // 
            // buttonCopy
            // 
            this.buttonCopy.Image = ((System.Drawing.Image)(resources.GetObject("buttonCopy.Image")));
            this.buttonCopy.Name = "buttonCopy";
            this.superTooltip1.SetSuperTooltip(this.buttonCopy, new DevComponents.DotNetBar.SuperTooltipInfo("Copy (Ctrl+C)", "", "Copy selected text to clipboard.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonCopy.Text = "&Copy";
            this.buttonCopy.Click += new System.EventHandler(this.buttonCopy_Click);
            // 
            // buttonItemPaste
            // 
            this.buttonItemPaste.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemPaste.Image")));
            this.buttonItemPaste.Name = "buttonItemPaste";
            this.superTooltip1.SetSuperTooltip(this.buttonItemPaste, new DevComponents.DotNetBar.SuperTooltipInfo("Format Painter", "This command is not implemented", "Copy formatting from one place and apply it to another.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonItemPaste.Text = "Format Painter";
            this.buttonItemPaste.Click += new System.EventHandler(this.buttonItemPaste_Click);
            // 
            // buttonItemUndo
            // 
            this.buttonItemUndo.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItemUndo.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemUndo.Image")));
            this.buttonItemUndo.Name = "buttonItemUndo";
            this.superTooltip1.SetSuperTooltip(this.buttonItemUndo, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonItemUndo.Tooltip = "Undo";
            this.buttonItemUndo.Click += new System.EventHandler(this.buttonItemUndo_Click);
            // 
            // buttonItemRedo
            // 
            this.buttonItemRedo.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItemRedo.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemRedo.Image")));
            this.buttonItemRedo.Name = "buttonItemRedo";
            this.superTooltip1.SetSuperTooltip(this.buttonItemRedo, new DevComponents.DotNetBar.SuperTooltipInfo("Replace", "", "Find and replace the text in document.\r\n\r\nThis feature has not been implemented y" +
                        "et.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonItemRedo.Tooltip = "Redo";
            this.buttonItemRedo.Click += new System.EventHandler(this.buttonItemRedo_Click);
            // 
            // ribbonBarSearch
            // 
            this.ribbonBarSearch.AutoOverflowEnabled = true;
            // 
            // 
            // 
            this.ribbonBarSearch.BackgroundMouseOverStyle.Class = "";
            this.ribbonBarSearch.BackgroundMouseOverStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBarSearch.BackgroundStyle.Class = "";
            this.ribbonBarSearch.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonBarSearch.ContainerControlProcessDialogKey = true;
            this.ribbonBarSearch.DialogLauncherVisible = true;
            this.ribbonBarSearch.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonFind,
            this.itemContainer5});
            this.ribbonBarSearch.Location = new System.Drawing.Point(144, 0);
            this.ribbonBarSearch.Name = "ribbonBarSearch";
            this.ribbonBarSearch.Size = new System.Drawing.Size(111, 86);
            this.ribbonBarSearch.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.superTooltip1.SetSuperTooltip(this.ribbonBarSearch, new DevComponents.DotNetBar.SuperTooltipInfo("SuperTooltip for Dialog Launcher Button", "", "Assigning the Super Tooltip to the Ribbon Bar control will display it when mouse " +
                        "hovers over the Dialog Launcher button.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.ribbonBarSearch.TabIndex = 7;
            this.ribbonBarSearch.Text = "Fi&nd";
            // 
            // 
            // 
            this.ribbonBarSearch.TitleStyle.Class = "";
            this.ribbonBarSearch.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBarSearch.TitleStyleMouseOver.Class = "";
            this.ribbonBarSearch.TitleStyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // buttonFind
            // 
            this.buttonFind.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonFind.Enabled = false;
            this.buttonFind.Image = ((System.Drawing.Image)(resources.GetObject("buttonFind.Image")));
            this.buttonFind.ImagePosition = DevComponents.DotNetBar.eImagePosition.Top;
            this.buttonFind.Name = "buttonFind";
            this.buttonFind.Shortcuts.Add(DevComponents.DotNetBar.eShortcut.CtrlF);
            this.superTooltip1.SetSuperTooltip(this.buttonFind, new DevComponents.DotNetBar.SuperTooltipInfo("Find (Ctrl+F)", "", "Find text in document.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonFind.Text = "&Find";
            // 
            // itemContainer5
            // 
            // 
            // 
            // 
            this.itemContainer5.BackgroundStyle.Class = "";
            this.itemContainer5.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.itemContainer5.LayoutOrientation = DevComponents.DotNetBar.eOrientation.Vertical;
            this.itemContainer5.Name = "itemContainer5";
            this.itemContainer5.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonReplace,
            this.buttonGoto});
            // 
            // buttonReplace
            // 
            this.buttonReplace.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonReplace.Enabled = false;
            this.buttonReplace.Image = ((System.Drawing.Image)(resources.GetObject("buttonReplace.Image")));
            this.buttonReplace.Name = "buttonReplace";
            this.superTooltip1.SetSuperTooltip(this.buttonReplace, new DevComponents.DotNetBar.SuperTooltipInfo("Replace", "", "Find and replace the text in document.\r\n\r\nThis feature has not been implemented y" +
                        "et.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonReplace.Text = "&Replace";
            // 
            // buttonGoto
            // 
            this.buttonGoto.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonGoto.Enabled = false;
            this.buttonGoto.Image = ((System.Drawing.Image)(resources.GetObject("buttonGoto.Image")));
            this.buttonGoto.Name = "buttonGoto";
            this.superTooltip1.SetSuperTooltip(this.buttonGoto, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonGoto.Text = "&Goto";
            // 
            // ribbonPanelContext
            // 
            this.ribbonPanelContext.AutoSize = true;
            this.ribbonPanelContext.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ribbonPanelContext.Controls.Add(this.ribbonBar6);
            this.ribbonPanelContext.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ribbonPanelContext.Location = new System.Drawing.Point(0, 53);
            this.ribbonPanelContext.Name = "ribbonPanelContext";
            this.ribbonPanelContext.Padding = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.ribbonPanelContext.Size = new System.Drawing.Size(1014, 89);
            // 
            // 
            // 
            this.ribbonPanelContext.Style.Class = "";
            this.ribbonPanelContext.Style.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonPanelContext.StyleMouseDown.Class = "";
            this.ribbonPanelContext.StyleMouseDown.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonPanelContext.StyleMouseOver.Class = "";
            this.ribbonPanelContext.StyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonPanelContext.TabIndex = 4;
            this.ribbonPanelContext.Visible = false;
            // 
            // ribbonBar6
            // 
            this.ribbonBar6.AutoOverflowEnabled = true;
            // 
            // 
            // 
            this.ribbonBar6.BackgroundMouseOverStyle.Class = "";
            this.ribbonBar6.BackgroundMouseOverStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBar6.BackgroundStyle.Class = "";
            this.ribbonBar6.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonBar6.ContainerControlProcessDialogKey = true;
            this.ribbonBar6.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonItem1,
            this.buttonItem8,
            this.buttonItem12});
            this.ribbonBar6.Location = new System.Drawing.Point(6, 3);
            this.ribbonBar6.Name = "ribbonBar6";
            this.ribbonBar6.Size = new System.Drawing.Size(217, 66);
            this.ribbonBar6.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ribbonBar6.TabIndex = 0;
            // 
            // 
            // 
            this.ribbonBar6.TitleStyle.Class = "";
            this.ribbonBar6.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBar6.TitleStyleMouseOver.Class = "";
            this.ribbonBar6.TitleStyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // buttonItem1
            // 
            this.buttonItem1.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItem1.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem1.Image")));
            this.buttonItem1.ImagePosition = DevComponents.DotNetBar.eImagePosition.Top;
            this.buttonItem1.Name = "buttonItem1";
            this.buttonItem1.RibbonWordWrap = false;
            this.buttonItem1.Text = "Command 1";
            // 
            // buttonItem8
            // 
            this.buttonItem8.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItem8.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem8.Image")));
            this.buttonItem8.ImagePosition = DevComponents.DotNetBar.eImagePosition.Top;
            this.buttonItem8.Name = "buttonItem8";
            this.buttonItem8.RibbonWordWrap = false;
            this.buttonItem8.Text = "Command 2";
            // 
            // buttonItem12
            // 
            this.buttonItem12.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItem12.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem12.Image")));
            this.buttonItem12.ImagePosition = DevComponents.DotNetBar.eImagePosition.Top;
            this.buttonItem12.Name = "buttonItem12";
            this.buttonItem12.RibbonWordWrap = false;
            this.buttonItem12.Text = "Command 3";
            // 
            // ribbonPanel4
            // 
            this.ribbonPanel4.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ribbonPanel4.Controls.Add(this.ribbonBar3);
            this.ribbonPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ribbonPanel4.Location = new System.Drawing.Point(0, 53);
            this.ribbonPanel4.Name = "ribbonPanel4";
            this.ribbonPanel4.Padding = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.ribbonPanel4.Size = new System.Drawing.Size(1014, 89);
            // 
            // 
            // 
            this.ribbonPanel4.Style.Class = "";
            this.ribbonPanel4.Style.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonPanel4.StyleMouseDown.Class = "";
            this.ribbonPanel4.StyleMouseDown.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonPanel4.StyleMouseOver.Class = "";
            this.ribbonPanel4.StyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonPanel4.TabIndex = 8;
            this.ribbonPanel4.Visible = false;
            // 
            // ribbonBar3
            // 
            this.ribbonBar3.AutoOverflowEnabled = true;
            // 
            // 
            // 
            this.ribbonBar3.BackgroundMouseOverStyle.Class = "";
            this.ribbonBar3.BackgroundMouseOverStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBar3.BackgroundStyle.Class = "";
            this.ribbonBar3.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonBar3.ContainerControlProcessDialogKey = true;
            this.ribbonBar3.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonItemDirectionalLight,
            this.buttonItemPointLight,
            this.buttonItemSpotLight});
            this.ribbonBar3.Location = new System.Drawing.Point(3, 3);
            this.ribbonBar3.Name = "ribbonBar3";
            this.ribbonBar3.Size = new System.Drawing.Size(235, 69);
            this.ribbonBar3.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ribbonBar3.TabIndex = 2;
            // 
            // 
            // 
            this.ribbonBar3.TitleStyle.Class = "";
            this.ribbonBar3.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBar3.TitleStyleMouseOver.Class = "";
            this.ribbonBar3.TitleStyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // buttonItemDirectionalLight
            // 
            this.buttonItemDirectionalLight.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItemDirectionalLight.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemDirectionalLight.Image")));
            this.buttonItemDirectionalLight.ImagePosition = DevComponents.DotNetBar.eImagePosition.Top;
            this.buttonItemDirectionalLight.Name = "buttonItemDirectionalLight";
            this.buttonItemDirectionalLight.RibbonWordWrap = false;
            this.buttonItemDirectionalLight.Text = "Directional Light";
            this.buttonItemDirectionalLight.Click += new System.EventHandler(this.buttonItemDirectionalLight_Click);
            // 
            // buttonItemPointLight
            // 
            this.buttonItemPointLight.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItemPointLight.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemPointLight.Image")));
            this.buttonItemPointLight.ImagePosition = DevComponents.DotNetBar.eImagePosition.Top;
            this.buttonItemPointLight.Name = "buttonItemPointLight";
            this.buttonItemPointLight.RibbonWordWrap = false;
            this.buttonItemPointLight.Text = "Point Light";
            this.buttonItemPointLight.Click += new System.EventHandler(this.buttonItemPointLight_Click);
            // 
            // buttonItemSpotLight
            // 
            this.buttonItemSpotLight.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItemSpotLight.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemSpotLight.Image")));
            this.buttonItemSpotLight.ImagePosition = DevComponents.DotNetBar.eImagePosition.Top;
            this.buttonItemSpotLight.Name = "buttonItemSpotLight";
            this.buttonItemSpotLight.RibbonWordWrap = false;
            this.buttonItemSpotLight.Text = "Spotlight";
            // 
            // ribbonPanel9
            // 
            this.ribbonPanel9.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ribbonPanel9.Controls.Add(this.ribbonBar15);
            this.ribbonPanel9.Controls.Add(this.ribbonBar13);
            this.ribbonPanel9.Controls.Add(this.ribbonBar12);
            this.ribbonPanel9.Controls.Add(this.ribbonBar10);
            this.ribbonPanel9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ribbonPanel9.Location = new System.Drawing.Point(0, 53);
            this.ribbonPanel9.Name = "ribbonPanel9";
            this.ribbonPanel9.Padding = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.ribbonPanel9.Size = new System.Drawing.Size(1014, 89);
            // 
            // 
            // 
            this.ribbonPanel9.Style.Class = "";
            this.ribbonPanel9.Style.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonPanel9.StyleMouseDown.Class = "";
            this.ribbonPanel9.StyleMouseDown.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonPanel9.StyleMouseOver.Class = "";
            this.ribbonPanel9.StyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonPanel9.TabIndex = 13;
            this.ribbonPanel9.Visible = false;
            // 
            // ribbonBar15
            // 
            this.ribbonBar15.AutoOverflowEnabled = true;
            // 
            // 
            // 
            this.ribbonBar15.BackgroundMouseOverStyle.Class = "";
            this.ribbonBar15.BackgroundMouseOverStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBar15.BackgroundStyle.Class = "";
            this.ribbonBar15.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonBar15.ContainerControlProcessDialogKey = true;
            this.ribbonBar15.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.itemContainer17,
            this.itemContainer18,
            this.itemContainer22,
            this.itemContainer21,
            this.itemContainer19,
            this.itemContainer20});
            this.ribbonBar15.ItemSpacing = 4;
            this.ribbonBar15.Location = new System.Drawing.Point(325, 3);
            this.ribbonBar15.Name = "ribbonBar15";
            this.ribbonBar15.Size = new System.Drawing.Size(686, 80);
            this.ribbonBar15.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ribbonBar15.TabIndex = 13;
            // 
            // 
            // 
            this.ribbonBar15.TitleStyle.Class = "";
            this.ribbonBar15.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBar15.TitleStyleMouseOver.Class = "";
            this.ribbonBar15.TitleStyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // itemContainer17
            // 
            // 
            // 
            // 
            this.itemContainer17.BackgroundStyle.Class = "";
            this.itemContainer17.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.itemContainer17.LayoutOrientation = DevComponents.DotNetBar.eOrientation.Vertical;
            this.itemContainer17.Name = "itemContainer17";
            this.itemContainer17.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonPT_Select,
            this.buttonPT_ScaleOutput,
            this.buttonPT_ScaleBiasOutput});
            // 
            // buttonPT_Select
            // 
            this.buttonPT_Select.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonPT_Select.Image = ((System.Drawing.Image)(resources.GetObject("buttonPT_Select.Image")));
            this.buttonPT_Select.Name = "buttonPT_Select";
            this.superTooltip1.SetSuperTooltip(this.buttonPT_Select, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonPT_Select.Text = "Select";
            this.buttonPT_Select.Click += new System.EventHandler(this.buttonPT_Select_Click);
            // 
            // buttonPT_ScaleOutput
            // 
            this.buttonPT_ScaleOutput.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonPT_ScaleOutput.Image = ((System.Drawing.Image)(resources.GetObject("buttonPT_ScaleOutput.Image")));
            this.buttonPT_ScaleOutput.Name = "buttonPT_ScaleOutput";
            this.superTooltip1.SetSuperTooltip(this.buttonPT_ScaleOutput, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonPT_ScaleOutput.Text = "Scale Output";
            this.buttonPT_ScaleOutput.Click += new System.EventHandler(this.buttonPT_ScaleOutput_Click);
            // 
            // buttonPT_ScaleBiasOutput
            // 
            this.buttonPT_ScaleBiasOutput.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonPT_ScaleBiasOutput.Image = ((System.Drawing.Image)(resources.GetObject("buttonPT_ScaleBiasOutput.Image")));
            this.buttonPT_ScaleBiasOutput.Name = "buttonPT_ScaleBiasOutput";
            this.superTooltip1.SetSuperTooltip(this.buttonPT_ScaleBiasOutput, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonPT_ScaleBiasOutput.Text = "Scale Bias Output";
            this.buttonPT_ScaleBiasOutput.Click += new System.EventHandler(this.buttonItemScaleBiasOutput_Click);
            // 
            // itemContainer18
            // 
            // 
            // 
            // 
            this.itemContainer18.BackgroundStyle.Class = "";
            this.itemContainer18.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.itemContainer18.LayoutOrientation = DevComponents.DotNetBar.eOrientation.Vertical;
            this.itemContainer18.Name = "itemContainer18";
            this.itemContainer18.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonPT_FastNoise,
            this.buttonPT_FastBillow,
            this.buttonPT_FastTurbulence});
            // 
            // buttonPT_FastNoise
            // 
            this.buttonPT_FastNoise.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonPT_FastNoise.Image = ((System.Drawing.Image)(resources.GetObject("buttonPT_FastNoise.Image")));
            this.buttonPT_FastNoise.Name = "buttonPT_FastNoise";
            this.superTooltip1.SetSuperTooltip(this.buttonPT_FastNoise, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonPT_FastNoise.Text = "Fast Noise";
            this.buttonPT_FastNoise.Click += new System.EventHandler(this.buttonPT_FastNoise_Click);
            // 
            // buttonPT_FastBillow
            // 
            this.buttonPT_FastBillow.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonPT_FastBillow.Image = ((System.Drawing.Image)(resources.GetObject("buttonPT_FastBillow.Image")));
            this.buttonPT_FastBillow.Name = "buttonPT_FastBillow";
            this.superTooltip1.SetSuperTooltip(this.buttonPT_FastBillow, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonPT_FastBillow.Text = "Fast Billow";
            this.buttonPT_FastBillow.Click += new System.EventHandler(this.buttonPT_FastBillow_Click);
            // 
            // buttonPT_FastTurbulence
            // 
            this.buttonPT_FastTurbulence.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonPT_FastTurbulence.Image = ((System.Drawing.Image)(resources.GetObject("buttonPT_FastTurbulence.Image")));
            this.buttonPT_FastTurbulence.Name = "buttonPT_FastTurbulence";
            this.superTooltip1.SetSuperTooltip(this.buttonPT_FastTurbulence, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonPT_FastTurbulence.Text = "Fast Turbulence";
            this.buttonPT_FastTurbulence.Click += new System.EventHandler(this.buttonPT_FastTurbulence_Click);
            // 
            // itemContainer22
            // 
            // 
            // 
            // 
            this.itemContainer22.BackgroundStyle.Class = "";
            this.itemContainer22.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.itemContainer22.LayoutOrientation = DevComponents.DotNetBar.eOrientation.Vertical;
            this.itemContainer22.Name = "itemContainer22";
            this.itemContainer22.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonPT_Voronoi,
            this.buttonPT_Billow,
            this.buttonPT_Turbulence});
            // 
            // buttonPT_Voronoi
            // 
            this.buttonPT_Voronoi.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonPT_Voronoi.Image = ((System.Drawing.Image)(resources.GetObject("buttonPT_Voronoi.Image")));
            this.buttonPT_Voronoi.Name = "buttonPT_Voronoi";
            this.superTooltip1.SetSuperTooltip(this.buttonPT_Voronoi, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonPT_Voronoi.Text = "Voronoi";
            this.buttonPT_Voronoi.Click += new System.EventHandler(this.buttonPT_Voronoi_Click);
            // 
            // buttonPT_Billow
            // 
            this.buttonPT_Billow.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonPT_Billow.Image = ((System.Drawing.Image)(resources.GetObject("buttonPT_Billow.Image")));
            this.buttonPT_Billow.Name = "buttonPT_Billow";
            this.superTooltip1.SetSuperTooltip(this.buttonPT_Billow, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonPT_Billow.Text = "Billow";
            this.buttonPT_Billow.Click += new System.EventHandler(this.buttonPT_Billow_Click);
            // 
            // buttonPT_Turbulence
            // 
            this.buttonPT_Turbulence.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonPT_Turbulence.Image = ((System.Drawing.Image)(resources.GetObject("buttonPT_Turbulence.Image")));
            this.buttonPT_Turbulence.Name = "buttonPT_Turbulence";
            this.superTooltip1.SetSuperTooltip(this.buttonPT_Turbulence, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonPT_Turbulence.Text = "Turbulence";
            this.buttonPT_Turbulence.Click += new System.EventHandler(this.buttonPT_Turbulence_Click);
            // 
            // itemContainer21
            // 
            // 
            // 
            // 
            this.itemContainer21.BackgroundStyle.Class = "";
            this.itemContainer21.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.itemContainer21.LayoutOrientation = DevComponents.DotNetBar.eOrientation.Vertical;
            this.itemContainer21.Name = "itemContainer21";
            this.itemContainer21.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonPT_FastRidgedMF,
            this.buttonPT_RidgedMF,
            this.buttonPT_Perlin});
            // 
            // buttonPT_FastRidgedMF
            // 
            this.buttonPT_FastRidgedMF.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonPT_FastRidgedMF.Image = ((System.Drawing.Image)(resources.GetObject("buttonPT_FastRidgedMF.Image")));
            this.buttonPT_FastRidgedMF.Name = "buttonPT_FastRidgedMF";
            this.superTooltip1.SetSuperTooltip(this.buttonPT_FastRidgedMF, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonPT_FastRidgedMF.Text = "Fast Ridged MultiF";
            this.buttonPT_FastRidgedMF.Click += new System.EventHandler(this.buttonPT_FastRidgedMF_Click);
            // 
            // buttonPT_RidgedMF
            // 
            this.buttonPT_RidgedMF.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonPT_RidgedMF.Image = ((System.Drawing.Image)(resources.GetObject("buttonPT_RidgedMF.Image")));
            this.buttonPT_RidgedMF.Name = "buttonPT_RidgedMF";
            this.superTooltip1.SetSuperTooltip(this.buttonPT_RidgedMF, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonPT_RidgedMF.Text = "Ridged MultiFractal";
            this.buttonPT_RidgedMF.Click += new System.EventHandler(this.buttonPT_RidgedMF_Click);
            // 
            // buttonPT_Perlin
            // 
            this.buttonPT_Perlin.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonPT_Perlin.Image = ((System.Drawing.Image)(resources.GetObject("buttonPT_Perlin.Image")));
            this.buttonPT_Perlin.Name = "buttonPT_Perlin";
            this.superTooltip1.SetSuperTooltip(this.buttonPT_Perlin, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonPT_Perlin.Text = "Perlin";
            this.buttonPT_Perlin.Click += new System.EventHandler(this.buttonPT_Perlin_Click);
            // 
            // itemContainer19
            // 
            // 
            // 
            // 
            this.itemContainer19.BackgroundStyle.Class = "";
            this.itemContainer19.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.itemContainer19.LayoutOrientation = DevComponents.DotNetBar.eOrientation.Vertical;
            this.itemContainer19.Name = "itemContainer19";
            this.itemContainer19.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonPT_Checkerboard,
            this.buttonPT_Int,
            this.buttonPT_Float});
            // 
            // buttonPT_Checkerboard
            // 
            this.buttonPT_Checkerboard.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonPT_Checkerboard.Image = ((System.Drawing.Image)(resources.GetObject("buttonPT_Checkerboard.Image")));
            this.buttonPT_Checkerboard.Name = "buttonPT_Checkerboard";
            this.superTooltip1.SetSuperTooltip(this.buttonPT_Checkerboard, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonPT_Checkerboard.Text = "Checkerboard";
            this.buttonPT_Checkerboard.Click += new System.EventHandler(this.buttonPT_Checkerboard_Click);
            // 
            // buttonPT_Int
            // 
            this.buttonPT_Int.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonPT_Int.Image = ((System.Drawing.Image)(resources.GetObject("buttonPT_Int.Image")));
            this.buttonPT_Int.Name = "buttonPT_Int";
            this.superTooltip1.SetSuperTooltip(this.buttonPT_Int, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonPT_Int.Text = "Integer";
            this.buttonPT_Int.Click += new System.EventHandler(this.buttonItemInt_Click);
            // 
            // buttonPT_Float
            // 
            this.buttonPT_Float.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonPT_Float.Image = ((System.Drawing.Image)(resources.GetObject("buttonPT_Float.Image")));
            this.buttonPT_Float.Name = "buttonPT_Float";
            this.superTooltip1.SetSuperTooltip(this.buttonPT_Float, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonPT_Float.Text = "Float";
            this.buttonPT_Float.Click += new System.EventHandler(this.buttonItemFloat_Click);
            // 
            // itemContainer20
            // 
            // 
            // 
            // 
            this.itemContainer20.BackgroundStyle.Class = "";
            this.itemContainer20.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.itemContainer20.LayoutOrientation = DevComponents.DotNetBar.eOrientation.Vertical;
            this.itemContainer20.Name = "itemContainer20";
            this.itemContainer20.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonPT_SphereMap,
            this.buttonPT_PlaneMap,
            this.buttonPT_CylinderMap});
            // 
            // buttonPT_SphereMap
            // 
            this.buttonPT_SphereMap.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonPT_SphereMap.Image = ((System.Drawing.Image)(resources.GetObject("buttonPT_SphereMap.Image")));
            this.buttonPT_SphereMap.Name = "buttonPT_SphereMap";
            this.superTooltip1.SetSuperTooltip(this.buttonPT_SphereMap, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonPT_SphereMap.Text = "Sphere Map";
            this.buttonPT_SphereMap.Click += new System.EventHandler(this.buttonItemSphereMap_Click);
            // 
            // buttonPT_PlaneMap
            // 
            this.buttonPT_PlaneMap.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonPT_PlaneMap.Image = ((System.Drawing.Image)(resources.GetObject("buttonPT_PlaneMap.Image")));
            this.buttonPT_PlaneMap.Name = "buttonPT_PlaneMap";
            this.superTooltip1.SetSuperTooltip(this.buttonPT_PlaneMap, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonPT_PlaneMap.Text = "Plane Map";
            this.buttonPT_PlaneMap.Click += new System.EventHandler(this.buttonPT_PlaneMap_Click);
            // 
            // buttonPT_CylinderMap
            // 
            this.buttonPT_CylinderMap.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonPT_CylinderMap.Image = ((System.Drawing.Image)(resources.GetObject("buttonPT_CylinderMap.Image")));
            this.buttonPT_CylinderMap.Name = "buttonPT_CylinderMap";
            this.superTooltip1.SetSuperTooltip(this.buttonPT_CylinderMap, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonPT_CylinderMap.Text = "Cylinder Map";
            this.buttonPT_CylinderMap.Click += new System.EventHandler(this.buttonPT_CylinderMap_Click);
            // 
            // ribbonBar13
            // 
            this.ribbonBar13.AutoOverflowEnabled = true;
            // 
            // 
            // 
            this.ribbonBar13.BackgroundMouseOverStyle.Class = "";
            this.ribbonBar13.BackgroundMouseOverStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBar13.BackgroundStyle.Class = "";
            this.ribbonBar13.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonBar13.ContainerControlProcessDialogKey = true;
            this.ribbonBar13.DialogLauncherVisible = true;
            this.ribbonBar13.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonItem20,
            this.buttonItem24});
            this.ribbonBar13.Location = new System.Drawing.Point(133, 3);
            this.ribbonBar13.Name = "ribbonBar13";
            this.ribbonBar13.Size = new System.Drawing.Size(112, 65);
            this.ribbonBar13.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ribbonBar13.TabIndex = 11;
            // 
            // 
            // 
            this.ribbonBar13.TitleStyle.Class = "";
            this.ribbonBar13.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBar13.TitleStyleMouseOver.Class = "";
            this.ribbonBar13.TitleStyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // buttonItem20
            // 
            this.buttonItem20.AutoExpandOnClick = true;
            this.buttonItem20.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItem20.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem20.Image")));
            this.buttonItem20.ImagePosition = DevComponents.DotNetBar.eImagePosition.Top;
            this.buttonItem20.Name = "buttonItem20";
            this.buttonItem20.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonItem21,
            this.buttonItem22,
            this.buttonItem23,
            this.buttonItem30});
            this.buttonItem20.Text = "Width <expand/>";
            // 
            // buttonItem21
            // 
            this.buttonItem21.Checked = true;
            this.buttonItem21.Name = "buttonItem21";
            this.buttonItem21.OptionGroup = "orientation";
            this.buttonItem21.Text = "256";
            // 
            // buttonItem22
            // 
            this.buttonItem22.Name = "buttonItem22";
            this.buttonItem22.OptionGroup = "orientation";
            this.buttonItem22.Text = "512";
            // 
            // buttonItem23
            // 
            this.buttonItem23.Name = "buttonItem23";
            this.buttonItem23.OptionGroup = "orientation";
            this.buttonItem23.Text = "1024";
            // 
            // buttonItem30
            // 
            this.buttonItem30.Name = "buttonItem30";
            this.buttonItem30.Text = "2048";
            // 
            // buttonItem24
            // 
            this.buttonItem24.AutoExpandOnClick = true;
            this.buttonItem24.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItem24.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem24.Image")));
            this.buttonItem24.ImagePosition = DevComponents.DotNetBar.eImagePosition.Top;
            this.buttonItem24.Name = "buttonItem24";
            this.buttonItem24.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonItem31,
            this.buttonItem32,
            this.buttonItem33,
            this.buttonItem34,
            this.buttonItem35});
            this.buttonItem24.Text = "Height <expand/>";
            // 
            // buttonItem31
            // 
            this.buttonItem31.Name = "buttonItem31";
            this.buttonItem31.Text = "128";
            // 
            // buttonItem32
            // 
            this.buttonItem32.Name = "buttonItem32";
            this.buttonItem32.Text = "256";
            // 
            // buttonItem33
            // 
            this.buttonItem33.Name = "buttonItem33";
            this.buttonItem33.Text = "512";
            // 
            // buttonItem34
            // 
            this.buttonItem34.Name = "buttonItem34";
            this.buttonItem34.Text = "1024";
            // 
            // buttonItem35
            // 
            this.buttonItem35.Name = "buttonItem35";
            this.buttonItem35.Text = "2048";
            // 
            // ribbonBar12
            // 
            this.ribbonBar12.AutoOverflowEnabled = true;
            // 
            // 
            // 
            this.ribbonBar12.BackgroundMouseOverStyle.Class = "";
            this.ribbonBar12.BackgroundMouseOverStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBar12.BackgroundStyle.Class = "";
            this.ribbonBar12.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonBar12.ContainerControlProcessDialogKey = true;
            this.ribbonBar12.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.itemContainer8});
            this.ribbonBar12.ItemSpacing = 4;
            this.ribbonBar12.Location = new System.Drawing.Point(3, 0);
            this.ribbonBar12.Name = "ribbonBar12";
            this.ribbonBar12.Size = new System.Drawing.Size(124, 76);
            this.ribbonBar12.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ribbonBar12.TabIndex = 10;
            // 
            // 
            // 
            this.ribbonBar12.TitleStyle.Class = "";
            this.ribbonBar12.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBar12.TitleStyleMouseOver.Class = "";
            this.ribbonBar12.TitleStyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // itemContainer8
            // 
            // 
            // 
            // 
            this.itemContainer8.BackgroundStyle.Class = "";
            this.itemContainer8.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.itemContainer8.LayoutOrientation = DevComponents.DotNetBar.eOrientation.Vertical;
            this.itemContainer8.Name = "itemContainer8";
            this.itemContainer8.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.checkBoxItem7,
            this.checkBoxItem8,
            this.checkBoxItem9});
            // 
            // checkBoxItem7
            // 
            this.checkBoxItem7.Name = "checkBoxItem7";
            this.checkBoxItem7.Text = "Create Normal Map";
            this.checkBoxItem7.ThreeState = true;
            // 
            // checkBoxItem8
            // 
            this.checkBoxItem8.Name = "checkBoxItem8";
            this.checkBoxItem8.Text = "Use Palette";
            // 
            // checkBoxItem9
            // 
            this.checkBoxItem9.Name = "checkBoxItem9";
            this.checkBoxItem9.Text = "Random Seed";
            // 
            // ribbonBar10
            // 
            this.ribbonBar10.AutoOverflowEnabled = true;
            // 
            // 
            // 
            this.ribbonBar10.BackgroundMouseOverStyle.Class = "";
            this.ribbonBar10.BackgroundMouseOverStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBar10.BackgroundStyle.Class = "";
            this.ribbonBar10.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonBar10.ContainerControlProcessDialogKey = true;
            this.ribbonBar10.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonItemGenerateTexture});
            this.ribbonBar10.Location = new System.Drawing.Point(251, 3);
            this.ribbonBar10.Name = "ribbonBar10";
            this.ribbonBar10.Size = new System.Drawing.Size(68, 65);
            this.ribbonBar10.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ribbonBar10.TabIndex = 2;
            // 
            // 
            // 
            this.ribbonBar10.TitleStyle.Class = "";
            this.ribbonBar10.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBar10.TitleStyleMouseOver.Class = "";
            this.ribbonBar10.TitleStyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // buttonItemGenerateTexture
            // 
            this.buttonItemGenerateTexture.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItemGenerateTexture.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemGenerateTexture.Image")));
            this.buttonItemGenerateTexture.ImagePosition = DevComponents.DotNetBar.eImagePosition.Top;
            this.buttonItemGenerateTexture.Name = "buttonItemGenerateTexture";
            this.buttonItemGenerateTexture.RibbonWordWrap = false;
            this.buttonItemGenerateTexture.Text = "Generate";
            this.buttonItemGenerateTexture.Click += new System.EventHandler(this.buttonItemGenerateTexture_Click);
            // 
            // ribbonPanelDisplay
            // 
            this.ribbonPanelDisplay.AutoSize = true;
            this.ribbonPanelDisplay.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ribbonPanelDisplay.Controls.Add(this.ribbonBarViewLayouts);
            this.ribbonPanelDisplay.Controls.Add(this.ribbonBarColors);
            this.ribbonPanelDisplay.Controls.Add(this.ribbonBarFont);
            this.ribbonPanelDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ribbonPanelDisplay.Location = new System.Drawing.Point(0, 53);
            this.ribbonPanelDisplay.Name = "ribbonPanelDisplay";
            this.ribbonPanelDisplay.Padding = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.ribbonPanelDisplay.Size = new System.Drawing.Size(1014, 89);
            // 
            // 
            // 
            this.ribbonPanelDisplay.Style.Class = "";
            this.ribbonPanelDisplay.Style.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonPanelDisplay.StyleMouseDown.Class = "";
            this.ribbonPanelDisplay.StyleMouseDown.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonPanelDisplay.StyleMouseOver.Class = "";
            this.ribbonPanelDisplay.StyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonPanelDisplay.TabIndex = 1;
            this.ribbonPanelDisplay.Visible = false;
            // 
            // ribbonBarViewLayouts
            // 
            this.ribbonBarViewLayouts.AutoOverflowEnabled = true;
            // 
            // 
            // 
            this.ribbonBarViewLayouts.BackgroundMouseOverStyle.Class = "";
            this.ribbonBarViewLayouts.BackgroundMouseOverStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBarViewLayouts.BackgroundStyle.Class = "";
            this.ribbonBarViewLayouts.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonBarViewLayouts.ContainerControlProcessDialogKey = true;
            this.ribbonBarViewLayouts.DialogLauncherVisible = true;
            this.ribbonBarViewLayouts.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.galleryViewLayouts});
            this.ribbonBarViewLayouts.LayoutOrientation = DevComponents.DotNetBar.eOrientation.Vertical;
            this.ribbonBarViewLayouts.Location = new System.Drawing.Point(347, 5);
            this.ribbonBarViewLayouts.Name = "ribbonBarViewLayouts";
            this.ribbonBarViewLayouts.Size = new System.Drawing.Size(469, 70);
            this.ribbonBarViewLayouts.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ribbonBarViewLayouts.TabIndex = 4;
            this.ribbonBarViewLayouts.Text = "View Layouts";
            // 
            // 
            // 
            this.ribbonBarViewLayouts.TitleStyle.Class = "";
            this.ribbonBarViewLayouts.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBarViewLayouts.TitleStyleMouseOver.Class = "";
            this.ribbonBarViewLayouts.TitleStyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonBarViewLayouts.VerticalItemAlignment = DevComponents.DotNetBar.eVerticalItemsAlignment.Middle;
            // 
            // galleryViewLayouts
            // 
            // 
            // 
            // 
            this.galleryViewLayouts.BackgroundStyle.Class = "RibbonGalleryContainer";
            this.galleryViewLayouts.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.galleryViewLayouts.DefaultSize = new System.Drawing.Size(200, 58);
            this.galleryViewLayouts.MinimumSize = new System.Drawing.Size(58, 58);
            this.galleryViewLayouts.Name = "galleryViewLayouts";
            this.galleryViewLayouts.PopupGalleryItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonItem47,
            this.buttonItem48,
            this.buttonItem49});
            this.galleryViewLayouts.StretchGallery = true;
            this.galleryViewLayouts.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonItemDisplaySingleViewport,
            this.buttonItemDisplayVSplit,
            this.buttonItemDisplayHSplit,
            this.buttonItemDisplayTripleLeft,
            this.buttonItemDisplayTripleRight,
            this.buttonItemDisplayQuad});
            // 
            // buttonItem47
            // 
            this.buttonItem47.BeginGroup = true;
            this.buttonItem47.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem47.Image")));
            this.buttonItem47.Name = "buttonItem47";
            this.buttonItem47.Text = "Search for Templates Online...";
            // 
            // buttonItem48
            // 
            this.buttonItem48.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem48.Image")));
            this.buttonItem48.Name = "buttonItem48";
            this.buttonItem48.Text = "Browse for Templates...";
            // 
            // buttonItem49
            // 
            this.buttonItem49.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem49.Image")));
            this.buttonItem49.Name = "buttonItem49";
            this.buttonItem49.Text = "Save Current Template...";
            // 
            // buttonItemDisplaySingleViewport
            // 
            this.buttonItemDisplaySingleViewport.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemDisplaySingleViewport.Image")));
            this.buttonItemDisplaySingleViewport.Name = "buttonItemDisplaySingleViewport";
            this.buttonItemDisplaySingleViewport.Text = "Single Viewport";
            this.buttonItemDisplaySingleViewport.Tooltip = "Single Viewport";
            this.buttonItemDisplaySingleViewport.Click += new System.EventHandler(this.buttonItemDisplayMode);
            // 
            // buttonItemDisplayVSplit
            // 
            this.buttonItemDisplayVSplit.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemDisplayVSplit.Image")));
            this.buttonItemDisplayVSplit.Name = "buttonItemDisplayVSplit";
            this.buttonItemDisplayVSplit.Text = "Vertical Split Viewports";
            this.buttonItemDisplayVSplit.Tooltip = "Vertical Split Viewports";
            this.buttonItemDisplayVSplit.Click += new System.EventHandler(this.buttonItemDisplayMode);
            // 
            // buttonItemDisplayHSplit
            // 
            this.buttonItemDisplayHSplit.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemDisplayHSplit.Image")));
            this.buttonItemDisplayHSplit.Name = "buttonItemDisplayHSplit";
            this.buttonItemDisplayHSplit.Text = "Horizontal Split Viewports";
            this.buttonItemDisplayHSplit.Tooltip = "Horizontal Split Viewports";
            this.buttonItemDisplayHSplit.Click += new System.EventHandler(this.buttonItemDisplayMode);
            // 
            // buttonItemDisplayTripleLeft
            // 
            this.buttonItemDisplayTripleLeft.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemDisplayTripleLeft.Image")));
            this.buttonItemDisplayTripleLeft.Name = "buttonItemDisplayTripleLeft";
            this.buttonItemDisplayTripleLeft.Text = "Left-handed Triple Viewports";
            this.buttonItemDisplayTripleLeft.Tooltip = "Left-handed Triple Viewports";
            this.buttonItemDisplayTripleLeft.Click += new System.EventHandler(this.buttonItemDisplayMode);
            // 
            // buttonItemDisplayTripleRight
            // 
            this.buttonItemDisplayTripleRight.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemDisplayTripleRight.Image")));
            this.buttonItemDisplayTripleRight.Name = "buttonItemDisplayTripleRight";
            this.buttonItemDisplayTripleRight.Text = "Right-handed Triple Viewports";
            this.buttonItemDisplayTripleRight.Tooltip = "Right-handed Triple Viewports";
            this.buttonItemDisplayTripleRight.Click += new System.EventHandler(this.buttonItemDisplayMode);
            // 
            // buttonItemDisplayQuad
            // 
            this.buttonItemDisplayQuad.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemDisplayQuad.Image")));
            this.buttonItemDisplayQuad.Name = "buttonItemDisplayQuad";
            this.buttonItemDisplayQuad.Text = "Quad Viewports";
            this.buttonItemDisplayQuad.Tooltip = "Quad Viewports";
            this.buttonItemDisplayQuad.Click += new System.EventHandler(this.buttonItemDisplayMode);
            // 
            // ribbonBarColors
            // 
            this.ribbonBarColors.AutoOverflowEnabled = true;
            // 
            // 
            // 
            this.ribbonBarColors.BackgroundMouseOverStyle.Class = "";
            this.ribbonBarColors.BackgroundMouseOverStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBarColors.BackgroundStyle.Class = "";
            this.ribbonBarColors.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonBarColors.ContainerControlProcessDialogKey = true;
            this.ribbonBarColors.DialogLauncherVisible = true;
            this.ribbonBarColors.Dock = System.Windows.Forms.DockStyle.Left;
            this.ribbonBarColors.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.itemContainer4,
            this.itemContainer6});
            this.ribbonBarColors.Location = new System.Drawing.Point(158, 0);
            this.ribbonBarColors.Name = "ribbonBarColors";
            this.ribbonBarColors.Size = new System.Drawing.Size(183, 86);
            this.ribbonBarColors.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.superTooltip1.SetSuperTooltip(this.ribbonBarColors, new DevComponents.DotNetBar.SuperTooltipInfo("SuperTooltip for Dialog Launcher Button", "", "Assigning the Super Tooltip to the Ribbon Bar control will display it when mouse " +
                        "hovers over the Dialog Launcher button.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.ribbonBarColors.TabIndex = 2;
            this.ribbonBarColors.Text = "&Colors";
            // 
            // 
            // 
            this.ribbonBarColors.TitleStyle.Class = "";
            this.ribbonBarColors.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBarColors.TitleStyleMouseOver.Class = "";
            this.ribbonBarColors.TitleStyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonBarColors.VerticalItemAlignment = DevComponents.DotNetBar.eVerticalItemsAlignment.Middle;
            // 
            // itemContainer4
            // 
            // 
            // 
            // 
            this.itemContainer4.BackgroundStyle.Class = "";
            this.itemContainer4.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.itemContainer4.ItemSpacing = 3;
            this.itemContainer4.LayoutOrientation = DevComponents.DotNetBar.eOrientation.Vertical;
            this.itemContainer4.MinimumSize = new System.Drawing.Size(50, 50);
            this.itemContainer4.MultiLine = true;
            this.itemContainer4.Name = "itemContainer4";
            this.itemContainer4.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonItemForeColor});
            this.itemContainer4.VerticalItemAlignment = DevComponents.DotNetBar.eVerticalItemsAlignment.Middle;
            // 
            // buttonItemForeColor
            // 
            this.buttonItemForeColor.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItemForeColor.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemForeColor.Image")));
            this.buttonItemForeColor.Name = "buttonItemForeColor";
            this.superTooltip1.SetSuperTooltip(this.buttonItemForeColor, new DevComponents.DotNetBar.SuperTooltipInfo("Add light", "", "Adds a light to scenet.\r\n\r\nCurrently only directional light supported.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonItemForeColor.Text = "&Text Color";
            // 
            // itemContainer6
            // 
            // 
            // 
            // 
            this.itemContainer6.BackgroundStyle.Class = "";
            this.itemContainer6.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.itemContainer6.ItemSpacing = 3;
            this.itemContainer6.LayoutOrientation = DevComponents.DotNetBar.eOrientation.Vertical;
            this.itemContainer6.Name = "itemContainer6";
            this.itemContainer6.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonItem37});
            this.itemContainer6.VerticalItemAlignment = DevComponents.DotNetBar.eVerticalItemsAlignment.Middle;
            // 
            // buttonItem37
            // 
            this.buttonItem37.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItem37.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem37.Image")));
            this.buttonItem37.Name = "buttonItem37";
            this.buttonItem37.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.controlContainerItem3,
            this.itemContainer15});
            this.superTooltip1.SetSuperTooltip(this.buttonItem37, new DevComponents.DotNetBar.SuperTooltipInfo("Shading", "", "Changes shading of selected text.\r\n\r\nThis feature has not been implemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonItem37.Text = "&Back Color";
            // 
            // controlContainerItem3
            // 
            this.controlContainerItem3.AllowItemResize = true;
            this.controlContainerItem3.MenuVisibility = DevComponents.DotNetBar.eMenuVisibility.VisibleAlways;
            this.controlContainerItem3.Name = "controlContainerItem3";
            this.controlContainerItem3.Text = "controlContainerItem1";
            // 
            // itemContainer15
            // 
            // 
            // 
            // 
            this.itemContainer15.BackgroundStyle.Class = "";
            this.itemContainer15.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.itemContainer15.BeginGroup = true;
            this.itemContainer15.Name = "itemContainer15";
            // 
            // ribbonBarFont
            // 
            this.ribbonBarFont.AutoOverflowEnabled = true;
            // 
            // 
            // 
            this.ribbonBarFont.BackgroundMouseOverStyle.Class = "";
            this.ribbonBarFont.BackgroundMouseOverStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBarFont.BackgroundStyle.Class = "";
            this.ribbonBarFont.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonBarFont.ContainerControlProcessDialogKey = true;
            this.ribbonBarFont.DialogLauncherVisible = true;
            this.ribbonBarFont.Dock = System.Windows.Forms.DockStyle.Left;
            this.ribbonBarFont.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.itemContainer2,
            this.itemContainer3});
            this.ribbonBarFont.ItemSpacing = 5;
            this.ribbonBarFont.LayoutOrientation = DevComponents.DotNetBar.eOrientation.Vertical;
            this.ribbonBarFont.Location = new System.Drawing.Point(3, 0);
            this.ribbonBarFont.Name = "ribbonBarFont";
            this.ribbonBarFont.ResizeItemsToFit = false;
            this.ribbonBarFont.Size = new System.Drawing.Size(155, 86);
            this.ribbonBarFont.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.superTooltip1.SetSuperTooltip(this.ribbonBarFont, new DevComponents.DotNetBar.SuperTooltipInfo("SuperTooltip for Dialog Launcher Button", "", "Assigning the Super Tooltip to the Ribbon Bar control will display it when mouse " +
                        "hovers over the Dialog Launcher button.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.ribbonBarFont.TabIndex = 1;
            this.ribbonBarFont.Text = "F&ont";
            // 
            // 
            // 
            this.ribbonBarFont.TitleStyle.Class = "";
            this.ribbonBarFont.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBarFont.TitleStyleMouseOver.Class = "";
            this.ribbonBarFont.TitleStyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonBarFont.VerticalItemAlignment = DevComponents.DotNetBar.eVerticalItemsAlignment.Middle;
            // 
            // itemContainer2
            // 
            // 
            // 
            // 
            this.itemContainer2.BackgroundStyle.Class = "";
            this.itemContainer2.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.itemContainer2.Name = "itemContainer2";
            this.itemContainer2.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.comboFont,
            this.comboFontSize});
            // 
            // comboFont
            // 
            this.comboFont.ComboWidth = 96;
            this.comboFont.DropDownHeight = 106;
            this.comboFont.DropDownWidth = 242;
            this.comboFont.FontCombo = true;
            this.comboFont.ItemHeight = 14;
            this.comboFont.Name = "comboFont";
            this.superTooltip1.SetSuperTooltip(this.comboFont, new DevComponents.DotNetBar.SuperTooltipInfo("Font", "", "Change the font face.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            // 
            // comboFontSize
            // 
            this.comboFontSize.ComboWidth = 40;
            this.comboFontSize.DropDownHeight = 106;
            this.comboFontSize.ItemHeight = 14;
            this.comboFontSize.Items.AddRange(new object[] {
            this.comboItem1,
            this.comboItem2,
            this.comboItem3,
            this.comboItem4,
            this.comboItem5,
            this.comboItem6,
            this.comboItem7,
            this.comboItem8,
            this.comboItem9});
            this.comboFontSize.Name = "comboFontSize";
            // 
            // comboItem1
            // 
            this.comboItem1.Text = "6";
            // 
            // comboItem2
            // 
            this.comboItem2.Text = "7";
            // 
            // comboItem3
            // 
            this.comboItem3.Text = "8";
            // 
            // comboItem4
            // 
            this.comboItem4.Text = "9";
            // 
            // comboItem5
            // 
            this.comboItem5.Text = "10";
            // 
            // comboItem6
            // 
            this.comboItem6.Text = "11";
            // 
            // comboItem7
            // 
            this.comboItem7.Text = "12";
            // 
            // comboItem8
            // 
            this.comboItem8.Text = "13";
            // 
            // comboItem9
            // 
            this.comboItem9.Text = "14";
            // 
            // itemContainer3
            // 
            // 
            // 
            // 
            this.itemContainer3.BackgroundStyle.Class = "";
            this.itemContainer3.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.itemContainer3.BeginGroup = true;
            this.itemContainer3.Name = "itemContainer3";
            this.itemContainer3.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonFontBold,
            this.buttonFontItalic,
            this.buttonFontUnderline,
            this.buttonFontStrike,
            this.buttonTextColor});
            // 
            // buttonFontBold
            // 
            this.buttonFontBold.Enabled = false;
            this.buttonFontBold.Image = ((System.Drawing.Image)(resources.GetObject("buttonFontBold.Image")));
            this.buttonFontBold.Name = "buttonFontBold";
            this.buttonFontBold.Shortcuts.Add(DevComponents.DotNetBar.eShortcut.CtrlB);
            this.superTooltip1.SetSuperTooltip(this.buttonFontBold, new DevComponents.DotNetBar.SuperTooltipInfo("Bold (Ctrl+B)", "", "Make selected text bold", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonFontBold.Text = "&Bold";
            // 
            // buttonFontItalic
            // 
            this.buttonFontItalic.Enabled = false;
            this.buttonFontItalic.Image = ((System.Drawing.Image)(resources.GetObject("buttonFontItalic.Image")));
            this.buttonFontItalic.Name = "buttonFontItalic";
            this.buttonFontItalic.Shortcuts.Add(DevComponents.DotNetBar.eShortcut.CtrlI);
            this.superTooltip1.SetSuperTooltip(this.buttonFontItalic, new DevComponents.DotNetBar.SuperTooltipInfo("Italic (Ctrl+I)", "", "Italicize the selected text.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonFontItalic.Text = "&Italic";
            // 
            // buttonFontUnderline
            // 
            this.buttonFontUnderline.Enabled = false;
            this.buttonFontUnderline.Image = ((System.Drawing.Image)(resources.GetObject("buttonFontUnderline.Image")));
            this.buttonFontUnderline.Name = "buttonFontUnderline";
            this.buttonFontUnderline.Shortcuts.Add(DevComponents.DotNetBar.eShortcut.CtrlU);
            this.superTooltip1.SetSuperTooltip(this.buttonFontUnderline, new DevComponents.DotNetBar.SuperTooltipInfo("Underline (Ctrl+U)", "", "Underline the selected text.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonFontUnderline.Text = "&Underline";
            // 
            // buttonFontStrike
            // 
            this.buttonFontStrike.Enabled = false;
            this.buttonFontStrike.Image = ((System.Drawing.Image)(resources.GetObject("buttonFontStrike.Image")));
            this.buttonFontStrike.Name = "buttonFontStrike";
            this.superTooltip1.SetSuperTooltip(this.buttonFontStrike, new DevComponents.DotNetBar.SuperTooltipInfo("Strikethrough", "", "Draw a line through the middle of the selected text.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonFontStrike.Text = "&Strike";
            // 
            // buttonTextColor
            // 
            this.buttonTextColor.Image = ((System.Drawing.Image)(resources.GetObject("buttonTextColor.Image")));
            this.buttonTextColor.Name = "buttonTextColor";
            this.buttonTextColor.SelectedColorImageRectangle = new System.Drawing.Rectangle(0, 13, 16, 3);
            this.superTooltip1.SetSuperTooltip(this.buttonTextColor, new DevComponents.DotNetBar.SuperTooltipInfo("Text Color", "", "Change the selected text color.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonTextColor.Text = "Text &Color";
            this.buttonTextColor.ColorPreview += new DevComponents.DotNetBar.ColorPreviewEventHandler(this.buttonTextColor_SelectedColorPreview);
            this.buttonTextColor.SelectedColorChanged += new System.EventHandler(this.buttonTextColor_SelectedColorChanged);
            // 
            // ribbonPanel3
            // 
            this.ribbonPanel3.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ribbonPanel3.Controls.Add(this.ribbonBar9);
            this.ribbonPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ribbonPanel3.Location = new System.Drawing.Point(0, 53);
            this.ribbonPanel3.Name = "ribbonPanel3";
            this.ribbonPanel3.Padding = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.ribbonPanel3.Size = new System.Drawing.Size(1014, 89);
            // 
            // 
            // 
            this.ribbonPanel3.Style.Class = "";
            this.ribbonPanel3.Style.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonPanel3.StyleMouseDown.Class = "";
            this.ribbonPanel3.StyleMouseDown.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonPanel3.StyleMouseOver.Class = "";
            this.ribbonPanel3.StyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonPanel3.TabIndex = 7;
            this.ribbonPanel3.Visible = false;
            // 
            // ribbonBar9
            // 
            this.ribbonBar9.AutoOverflowEnabled = true;
            // 
            // 
            // 
            this.ribbonBar9.BackgroundMouseOverStyle.Class = "";
            this.ribbonBar9.BackgroundMouseOverStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBar9.BackgroundStyle.Class = "";
            this.ribbonBar9.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonBar9.ContainerControlProcessDialogKey = true;
            this.ribbonBar9.DialogLauncherVisible = true;
            this.ribbonBar9.Dock = System.Windows.Forms.DockStyle.Left;
            this.ribbonBar9.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.galleryContainerTextures});
            this.ribbonBar9.LayoutOrientation = DevComponents.DotNetBar.eOrientation.Vertical;
            this.ribbonBar9.Location = new System.Drawing.Point(3, 0);
            this.ribbonBar9.Name = "ribbonBar9";
            this.ribbonBar9.Size = new System.Drawing.Size(631, 86);
            this.ribbonBar9.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ribbonBar9.TabIndex = 4;
            this.ribbonBar9.Text = "Active Textures";
            // 
            // 
            // 
            this.ribbonBar9.TitleStyle.Class = "";
            this.ribbonBar9.TitleStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonBar9.TitleStyleMouseOver.Class = "";
            this.ribbonBar9.TitleStyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonBar9.VerticalItemAlignment = DevComponents.DotNetBar.eVerticalItemsAlignment.Middle;
            // 
            // galleryContainerTextures
            // 
            // 
            // 
            // 
            this.galleryContainerTextures.BackgroundStyle.Class = "RibbonGalleryContainer";
            this.galleryContainerTextures.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.galleryContainerTextures.DefaultSize = new System.Drawing.Size(200, 58);
            this.galleryContainerTextures.MinimumSize = new System.Drawing.Size(58, 58);
            this.galleryContainerTextures.Name = "galleryContainerTextures";
            this.galleryContainerTextures.PopupGalleryItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonItemOnlineTextureSearch,
            this.buttonItemBrowseTextures,
            this.buttonItemProceduralTextureGen,
            this.buttonItemSaveTexture});
            this.galleryContainerTextures.StretchGallery = true;
            this.galleryContainerTextures.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonItemTexture1,
            this.buttonItemTexture2,
            this.buttonItemTexture3,
            this.buttonItemTexture4,
            this.buttonItemTexture5,
            this.buttonItemTexture6});
            // 
            // buttonItemOnlineTextureSearch
            // 
            this.buttonItemOnlineTextureSearch.BeginGroup = true;
            this.buttonItemOnlineTextureSearch.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemOnlineTextureSearch.Image")));
            this.buttonItemOnlineTextureSearch.Name = "buttonItemOnlineTextureSearch";
            this.buttonItemOnlineTextureSearch.Text = "Search for Textures Online...";
            // 
            // buttonItemBrowseTextures
            // 
            this.buttonItemBrowseTextures.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemBrowseTextures.Image")));
            this.buttonItemBrowseTextures.Name = "buttonItemBrowseTextures";
            this.buttonItemBrowseTextures.Text = "Browse for Textures...";
            // 
            // buttonItemProceduralTextureGen
            // 
            this.buttonItemProceduralTextureGen.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemProceduralTextureGen.Image")));
            this.buttonItemProceduralTextureGen.Name = "buttonItemProceduralTextureGen";
            this.buttonItemProceduralTextureGen.Text = "Generate Procedural Texture...";
            // 
            // buttonItemSaveTexture
            // 
            this.buttonItemSaveTexture.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemSaveTexture.Image")));
            this.buttonItemSaveTexture.Name = "buttonItemSaveTexture";
            this.buttonItemSaveTexture.Text = "Save Current Texture...";
            // 
            // buttonItemTexture1
            // 
            this.buttonItemTexture1.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemTexture1.Image")));
            this.buttonItemTexture1.Name = "buttonItemTexture1";
            this.buttonItemTexture1.Text = "Texture1";
            this.buttonItemTexture1.Tooltip = "Texture1";
            // 
            // buttonItemTexture2
            // 
            this.buttonItemTexture2.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemTexture2.Image")));
            this.buttonItemTexture2.Name = "buttonItemTexture2";
            this.buttonItemTexture2.Text = "Texture2";
            this.buttonItemTexture2.Tooltip = "Texture2";
            // 
            // buttonItemTexture3
            // 
            this.buttonItemTexture3.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemTexture3.Image")));
            this.buttonItemTexture3.Name = "buttonItemTexture3";
            this.buttonItemTexture3.Text = "Concourse";
            this.buttonItemTexture3.Tooltip = "Concourse";
            // 
            // buttonItemTexture4
            // 
            this.buttonItemTexture4.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemTexture4.Image")));
            this.buttonItemTexture4.Name = "buttonItemTexture4";
            this.buttonItemTexture4.Text = "Currency";
            this.buttonItemTexture4.Tooltip = "Currency";
            // 
            // buttonItemTexture5
            // 
            this.buttonItemTexture5.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemTexture5.Image")));
            this.buttonItemTexture5.Name = "buttonItemTexture5";
            this.buttonItemTexture5.Text = "Deluxe";
            this.buttonItemTexture5.Tooltip = "Deluxe";
            // 
            // buttonItemTexture6
            // 
            this.buttonItemTexture6.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemTexture6.Image")));
            this.buttonItemTexture6.Name = "buttonItemTexture6";
            this.buttonItemTexture6.Text = "Equity";
            this.buttonItemTexture6.Tooltip = "Equity";
            // 
            // ribbonPanel2
            // 
            this.ribbonPanel2.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ribbonPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ribbonPanel2.Location = new System.Drawing.Point(0, 53);
            this.ribbonPanel2.Name = "ribbonPanel2";
            this.ribbonPanel2.Padding = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.ribbonPanel2.Size = new System.Drawing.Size(1014, 89);
            // 
            // 
            // 
            this.ribbonPanel2.Style.Class = "";
            this.ribbonPanel2.Style.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonPanel2.StyleMouseDown.Class = "";
            this.ribbonPanel2.StyleMouseDown.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonPanel2.StyleMouseOver.Class = "";
            this.ribbonPanel2.StyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonPanel2.TabIndex = 6;
            this.ribbonPanel2.Visible = false;
            // 
            // ribbonPanel8
            // 
            this.ribbonPanel8.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ribbonPanel8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ribbonPanel8.Location = new System.Drawing.Point(0, 53);
            this.ribbonPanel8.Name = "ribbonPanel8";
            this.ribbonPanel8.Padding = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.ribbonPanel8.Size = new System.Drawing.Size(1014, 89);
            // 
            // 
            // 
            this.ribbonPanel8.Style.Class = "";
            this.ribbonPanel8.Style.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonPanel8.StyleMouseDown.Class = "";
            this.ribbonPanel8.StyleMouseDown.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonPanel8.StyleMouseOver.Class = "";
            this.ribbonPanel8.StyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonPanel8.TabIndex = 12;
            this.ribbonPanel8.Visible = false;
            // 
            // ribbonPanel7
            // 
            this.ribbonPanel7.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ribbonPanel7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ribbonPanel7.Location = new System.Drawing.Point(0, 53);
            this.ribbonPanel7.Name = "ribbonPanel7";
            this.ribbonPanel7.Padding = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.ribbonPanel7.Size = new System.Drawing.Size(1014, 89);
            // 
            // 
            // 
            this.ribbonPanel7.Style.Class = "";
            this.ribbonPanel7.Style.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonPanel7.StyleMouseDown.Class = "";
            this.ribbonPanel7.StyleMouseDown.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonPanel7.StyleMouseOver.Class = "";
            this.ribbonPanel7.StyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonPanel7.TabIndex = 11;
            this.ribbonPanel7.Visible = false;
            // 
            // ribbonPanel11
            // 
            this.ribbonPanel11.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ribbonPanel11.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ribbonPanel11.Location = new System.Drawing.Point(0, 53);
            this.ribbonPanel11.Name = "ribbonPanel11";
            this.ribbonPanel11.Padding = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.ribbonPanel11.Size = new System.Drawing.Size(1014, 89);
            // 
            // 
            // 
            this.ribbonPanel11.Style.Class = "";
            this.ribbonPanel11.Style.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonPanel11.StyleMouseDown.Class = "";
            this.ribbonPanel11.StyleMouseDown.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonPanel11.StyleMouseOver.Class = "";
            this.ribbonPanel11.StyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonPanel11.TabIndex = 15;
            this.ribbonPanel11.Visible = false;
            // 
            // ribbonPanel6
            // 
            this.ribbonPanel6.ColorSchemeStyle = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.ribbonPanel6.Controls.Add(this.labelScriptsNotesTemp);
            this.ribbonPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ribbonPanel6.Location = new System.Drawing.Point(0, 53);
            this.ribbonPanel6.Name = "ribbonPanel6";
            this.ribbonPanel6.Padding = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.ribbonPanel6.Size = new System.Drawing.Size(1014, 89);
            // 
            // 
            // 
            this.ribbonPanel6.Style.Class = "";
            this.ribbonPanel6.Style.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonPanel6.StyleMouseDown.Class = "";
            this.ribbonPanel6.StyleMouseDown.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            // 
            // 
            // 
            this.ribbonPanel6.StyleMouseOver.Class = "";
            this.ribbonPanel6.StyleMouseOver.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonPanel6.TabIndex = 10;
            this.ribbonPanel6.Visible = false;
            // 
            // labelScriptsNotesTemp
            // 
            this.labelScriptsNotesTemp.AutoSize = true;
            this.labelScriptsNotesTemp.Location = new System.Drawing.Point(58, 26);
            this.labelScriptsNotesTemp.Name = "labelScriptsNotesTemp";
            this.labelScriptsNotesTemp.Size = new System.Drawing.Size(829, 13);
            this.labelScriptsNotesTemp.TabIndex = 11;
            this.labelScriptsNotesTemp.Text = "This will contain both components that are scriptable like the various sensors an" +
                "d triggers that can be dragged into the level as well as code view tab so we can" +
                " edit the scripts";
            // 
            // contextMenuBar1
            // 
            this.contextMenuBar1.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.bEditPopup});
            this.contextMenuBar1.Location = new System.Drawing.Point(352, 309);
            this.contextMenuBar1.Name = "contextMenuBar1";
            this.contextMenuBar1.Size = new System.Drawing.Size(150, 25);
            this.contextMenuBar1.Stretch = true;
            this.contextMenuBar1.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.contextMenuBar1.TabIndex = 13;
            this.contextMenuBar1.TabStop = false;
            // 
            // bEditPopup
            // 
            this.bEditPopup.AutoExpandOnClick = true;
            this.bEditPopup.GlobalName = "bEditPopup";
            this.bEditPopup.Name = "bEditPopup";
            this.bEditPopup.PopupAnimation = DevComponents.DotNetBar.ePopupAnimation.SystemDefault;
            this.bEditPopup.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.bCut,
            this.bCopy,
            this.bPaste,
            this.bSelectAll});
            this.bEditPopup.Text = "bEditPopup";
            // 
            // bCut
            // 
            this.bCut.BeginGroup = true;
            this.bCut.GlobalName = "bCut";
            this.bCut.ImageIndex = 5;
            this.bCut.Name = "bCut";
            this.bCut.PopupAnimation = DevComponents.DotNetBar.ePopupAnimation.SystemDefault;
            this.bCut.Text = "Cu&t";
            // 
            // bCopy
            // 
            this.bCopy.GlobalName = "bCopy";
            this.bCopy.ImageIndex = 4;
            this.bCopy.Name = "bCopy";
            this.bCopy.PopupAnimation = DevComponents.DotNetBar.ePopupAnimation.SystemDefault;
            this.bCopy.Text = "&Copy";
            // 
            // bPaste
            // 
            this.bPaste.GlobalName = "bPaste";
            this.bPaste.ImageIndex = 12;
            this.bPaste.Name = "bPaste";
            this.bPaste.PopupAnimation = DevComponents.DotNetBar.ePopupAnimation.SystemDefault;
            this.bPaste.Text = "&Paste";
            // 
            // bSelectAll
            // 
            this.bSelectAll.BeginGroup = true;
            this.bSelectAll.GlobalName = "bSelectAll";
            this.bSelectAll.Name = "bSelectAll";
            this.bSelectAll.PopupAnimation = DevComponents.DotNetBar.ePopupAnimation.SystemDefault;
            this.bSelectAll.Text = "Select &All";
            // 
            // buttonFile
            // 
            this.buttonFile.AutoExpandOnClick = true;
            this.buttonFile.CanCustomize = false;
            this.buttonFile.HotTrackingStyle = DevComponents.DotNetBar.eHotTrackingStyle.Image;
            this.buttonFile.Image = ((System.Drawing.Image)(resources.GetObject("buttonFile.Image")));
            this.buttonFile.ImageFixedSize = new System.Drawing.Size(16, 16);
            this.buttonFile.ImagePaddingHorizontal = 0;
            this.buttonFile.ImagePaddingVertical = 0;
            this.buttonFile.Name = "buttonFile";
            this.buttonFile.ShowSubItems = false;
            this.buttonFile.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.menuFileContainer});
            this.buttonFile.Text = "F&ile";
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
            this.buttonItemOpen,
            this.buttonItemSave,
            this.buttonFileSaveAs,
            this.buttonImport,
            this.buttonPhysicsDemos,
            this.buttonItemPrint,
            this.buttonCloseScene});
            // 
            // buttonItemNew
            // 
            this.buttonItemNew.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItemNew.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemNew.Image")));
            this.buttonItemNew.ImageSmall = ((System.Drawing.Image)(resources.GetObject("buttonItemNew.ImageSmall")));
            this.buttonItemNew.Name = "buttonItemNew";
            this.buttonItemNew.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonItemNewSingleRegion,
            this.buttonItemNewMultiRegionScene,
            this.buttonItemNewFloorPlan});
            this.buttonItemNew.SubItemsExpandWidth = 24;
            this.buttonItemNew.Text = "&New";
            // 
            // buttonItemNewSingleRegion
            // 
            this.buttonItemNewSingleRegion.Name = "buttonItemNewSingleRegion";
            this.buttonItemNewSingleRegion.Text = "Single Region Scene";
            this.buttonItemNewSingleRegion.Click += new System.EventHandler(this.buttonItemNewSingleRegion_Click);
            // 
            // buttonItemNewMultiRegionScene
            // 
            this.buttonItemNewMultiRegionScene.Name = "buttonItemNewMultiRegionScene";
            this.buttonItemNewMultiRegionScene.Text = "Multi Region Scene";
            this.buttonItemNewMultiRegionScene.Click += new System.EventHandler(this.buttonItemNewMultiRegionScene_Click);
            // 
            // buttonItemNewFloorPlan
            // 
            this.buttonItemNewFloorPlan.Name = "buttonItemNewFloorPlan";
            this.buttonItemNewFloorPlan.Text = "Floor Plan";
            this.buttonItemNewFloorPlan.Click += new System.EventHandler(this.buttonItemNewFloorPlan_Click);
            // 
            // buttonItemOpen
            // 
            this.buttonItemOpen.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItemOpen.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemOpen.Image")));
            this.buttonItemOpen.Name = "buttonItemOpen";
            this.buttonItemOpen.SubItemsExpandWidth = 24;
            this.buttonItemOpen.Text = "&Open...";
            this.buttonItemOpen.Click += new System.EventHandler(this.buttonOpen_Click);
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
            // buttonFileSaveAs
            // 
            this.buttonFileSaveAs.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonFileSaveAs.Image = ((System.Drawing.Image)(resources.GetObject("buttonFileSaveAs.Image")));
            this.buttonFileSaveAs.ImageSmall = ((System.Drawing.Image)(resources.GetObject("buttonFileSaveAs.ImageSmall")));
            this.buttonFileSaveAs.Name = "buttonFileSaveAs";
            this.buttonFileSaveAs.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.itemContainer12});
            this.buttonFileSaveAs.SubItemsExpandWidth = 24;
            this.buttonFileSaveAs.Text = "Save &As...";
            // 
            // itemContainer12
            // 
            // 
            // 
            // 
            this.itemContainer12.BackgroundStyle.Class = "";
            this.itemContainer12.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.itemContainer12.ItemSpacing = 4;
            this.itemContainer12.LayoutOrientation = DevComponents.DotNetBar.eOrientation.Vertical;
            this.itemContainer12.MinimumSize = new System.Drawing.Size(210, 256);
            this.itemContainer12.Name = "itemContainer12";
            this.itemContainer12.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.labelItemSaveCaption,
            this.buttonItemSaveScene,
            this.buttonItemSavePrefab,
            this.buttonItem58,
            this.buttonItem59});
            // 
            // labelItemSaveCaption
            // 
            this.labelItemSaveCaption.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.labelItemSaveCaption.BorderSide = DevComponents.DotNetBar.eBorderSide.Bottom;
            this.labelItemSaveCaption.BorderType = DevComponents.DotNetBar.eBorderType.Etched;
            this.labelItemSaveCaption.Name = "labelItemSaveCaption";
            this.labelItemSaveCaption.PaddingBottom = 5;
            this.labelItemSaveCaption.PaddingLeft = 5;
            this.labelItemSaveCaption.PaddingRight = 5;
            this.labelItemSaveCaption.PaddingTop = 5;
            this.labelItemSaveCaption.Text = "<b>Save a copy of the scene</b>";
            // 
            // buttonItemSaveScene
            // 
            this.buttonItemSaveScene.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItemSaveScene.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemSaveScene.Image")));
            this.buttonItemSaveScene.Name = "buttonItemSaveScene";
            this.buttonItemSaveScene.Text = "<b>&Complete Scene</b>\r\n<div padding=\"0,0,4,0\" width=\"170\">Save the entire scene." +
                "</div>";
            this.buttonItemSaveScene.Click += new System.EventHandler(this.buttonItemSaveAs_Click);
            // 
            // buttonItemSavePrefab
            // 
            this.buttonItemSavePrefab.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItemSavePrefab.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemSavePrefab.Image")));
            this.buttonItemSavePrefab.Name = "buttonItemSavePrefab";
            this.buttonItemSavePrefab.Text = "<b>Prefab</b>\r\n<div padding=\"0,0,4,0\" width=\"170\">Save as a prefab object  that c" +
                "an be loaded into future scenes.</div>";
            // 
            // buttonItem58
            // 
            this.buttonItem58.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItem58.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem58.Image")));
            this.buttonItem58.Name = "buttonItem58";
            this.buttonItem58.Text = "<b>&Template</b>\r\n<div padding=\"0,0,4,0\" width=\"180\">place holder text.</div>";
            // 
            // buttonItem59
            // 
            this.buttonItem59.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItem59.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem59.Image")));
            this.buttonItem59.Name = "buttonItem59";
            this.buttonItem59.Text = "<b>&Wavefront OBJ</b>\r\n<div padding=\"0,0,4,0\" width=\"170\">Save all geometry eleme" +
                "nts as wavefront objects.</div>";
            // 
            // buttonImport
            // 
            this.buttonImport.BeginGroup = true;
            this.buttonImport.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonImport.Image = ((System.Drawing.Image)(resources.GetObject("buttonImport.Image")));
            this.buttonImport.Name = "buttonImport";
            this.buttonImport.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonImportMesh,
            this.buttonImportEditableObjMesh,
            this.buttonImportActor,
            this.buttonImportMinimesh,
            this.buttonImportBillboard,
            this.buttonImportVehicle});
            this.buttonImport.SubItemsExpandWidth = 24;
            this.buttonImport.Text = "I&mport...";
            // 
            // buttonImportMesh
            // 
            this.buttonImportMesh.Name = "buttonImportMesh";
            this.buttonImportMesh.Text = "Static Mesh";
            this.buttonImportMesh.Click += new System.EventHandler(this.menuItemImportGeometry_Click);
            // 
            // buttonImportEditableObjMesh
            // 
            this.buttonImportEditableObjMesh.Name = "buttonImportEditableObjMesh";
            this.buttonImportEditableObjMesh.Text = "Editable Mesh (obj)";
            this.buttonImportEditableObjMesh.Click += new System.EventHandler(this.menuItemImportGeometry_Click);
            // 
            // buttonImportActor
            // 
            this.buttonImportActor.Name = "buttonImportActor";
            this.buttonImportActor.Text = "Actor";
            this.buttonImportActor.Click += new System.EventHandler(this.menuItemImportGeometry_Click);
            // 
            // buttonImportMinimesh
            // 
            this.buttonImportMinimesh.Name = "buttonImportMinimesh";
            this.buttonImportMinimesh.Text = "Minimesh";
            this.buttonImportMinimesh.Click += new System.EventHandler(this.menuItemImportGeometry_Click);
            // 
            // buttonImportBillboard
            // 
            this.buttonImportBillboard.Name = "buttonImportBillboard";
            this.buttonImportBillboard.Text = "Billboard";
            this.buttonImportBillboard.Click += new System.EventHandler(this.menuItemImportGeometry_Click);
            // 
            // buttonImportVehicle
            // 
            this.buttonImportVehicle.Name = "buttonImportVehicle";
            this.buttonImportVehicle.Text = "Vehicle";
            this.buttonImportVehicle.Click += new System.EventHandler(this.menuItemImportGeometry_Click);
            // 
            // buttonPhysicsDemos
            // 
            this.buttonPhysicsDemos.BeginGroup = true;
            this.buttonPhysicsDemos.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonPhysicsDemos.Image = ((System.Drawing.Image)(resources.GetObject("buttonPhysicsDemos.Image")));
            this.buttonPhysicsDemos.Name = "buttonPhysicsDemos";
            this.buttonPhysicsDemos.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonWallDemo,
            this.buttonIncomingDemo,
            this.buttonSpheresDemo,
            this.buttonCatapultDemo,
            this.buttonPendulumDemo,
            this.buttonPlinkoDemo,
            this.buttonVehicleDemo,
            this.buttonSandboxDemo});
            this.buttonPhysicsDemos.SubItemsExpandWidth = 24;
            this.buttonPhysicsDemos.Text = "P&hysics Demos...";
            // 
            // buttonWallDemo
            // 
            this.buttonWallDemo.Name = "buttonWallDemo";
            this.buttonWallDemo.Text = "Wall";
            this.buttonWallDemo.Click += new System.EventHandler(this.OnPhysicsDemoSelected);
            // 
            // buttonIncomingDemo
            // 
            this.buttonIncomingDemo.Name = "buttonIncomingDemo";
            this.buttonIncomingDemo.Text = "Incoming";
            this.buttonIncomingDemo.Click += new System.EventHandler(this.OnPhysicsDemoSelected);
            // 
            // buttonSpheresDemo
            // 
            this.buttonSpheresDemo.Name = "buttonSpheresDemo";
            this.buttonSpheresDemo.Text = "Lots of Spheres";
            this.buttonSpheresDemo.Click += new System.EventHandler(this.OnPhysicsDemoSelected);
            // 
            // buttonCatapultDemo
            // 
            this.buttonCatapultDemo.Name = "buttonCatapultDemo";
            this.buttonCatapultDemo.Text = "Catapult";
            this.buttonCatapultDemo.Click += new System.EventHandler(this.OnPhysicsDemoSelected);
            // 
            // buttonPendulumDemo
            // 
            this.buttonPendulumDemo.Name = "buttonPendulumDemo";
            this.buttonPendulumDemo.Text = "Pendulum";
            this.buttonPendulumDemo.Click += new System.EventHandler(this.OnPhysicsDemoSelected);
            // 
            // buttonPlinkoDemo
            // 
            this.buttonPlinkoDemo.Name = "buttonPlinkoDemo";
            this.buttonPlinkoDemo.Text = "Plinko";
            this.buttonPlinkoDemo.Click += new System.EventHandler(this.OnPhysicsDemoSelected);
            // 
            // buttonVehicleDemo
            // 
            this.buttonVehicleDemo.Name = "buttonVehicleDemo";
            this.buttonVehicleDemo.Text = "Vehicle";
            this.buttonVehicleDemo.Click += new System.EventHandler(this.OnPhysicsDemoSelected);
            // 
            // buttonSandboxDemo
            // 
            this.buttonSandboxDemo.Name = "buttonSandboxDemo";
            this.buttonSandboxDemo.Text = "Sandbox";
            this.buttonSandboxDemo.Click += new System.EventHandler(this.OnPhysicsDemoSelected);
            // 
            // buttonItemPrint
            // 
            this.buttonItemPrint.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItemPrint.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemPrint.Image")));
            this.buttonItemPrint.Name = "buttonItemPrint";
            this.buttonItemPrint.SubItemsExpandWidth = 24;
            this.buttonItemPrint.Text = "&Print...";
            // 
            // buttonCloseScene
            // 
            this.buttonCloseScene.BeginGroup = true;
            this.buttonCloseScene.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonCloseScene.Image = ((System.Drawing.Image)(resources.GetObject("buttonCloseScene.Image")));
            this.buttonCloseScene.Name = "buttonCloseScene";
            this.buttonCloseScene.SubItemsExpandWidth = 24;
            this.buttonCloseScene.Text = "&Close";
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
            this.labelItem8,
            this.buttonItem26,
            this.buttonItem27,
            this.buttonItem28,
            this.buttonItem29});
            // 
            // labelItem8
            // 
            this.labelItem8.BorderSide = DevComponents.DotNetBar.eBorderSide.Bottom;
            this.labelItem8.BorderType = DevComponents.DotNetBar.eBorderType.Etched;
            this.labelItem8.Name = "labelItem8";
            this.labelItem8.PaddingBottom = 2;
            this.labelItem8.PaddingTop = 2;
            this.labelItem8.Stretch = true;
            this.labelItem8.Text = "Recent Projects";
            // 
            // buttonItem26
            // 
            this.buttonItem26.Name = "buttonItem26";
            this.buttonItem26.Text = "&1. Battlespace.kgb";
            // 
            // buttonItem27
            // 
            this.buttonItem27.Name = "buttonItem27";
            this.buttonItem27.Text = "&2. Demo.kgb";
            // 
            // buttonItem28
            // 
            this.buttonItem28.Name = "buttonItem28";
            this.buttonItem28.Text = "&3. Island Test.kgb";
            // 
            // buttonItem29
            // 
            this.buttonItem29.Name = "buttonItem29";
            this.buttonItem29.Text = "&4. Example.kgb";
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
            // buttonOptions
            // 
            this.buttonOptions.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonOptions.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.buttonOptions.Image = ((System.Drawing.Image)(resources.GetObject("buttonOptions.Image")));
            this.buttonOptions.Name = "buttonOptions";
            this.buttonOptions.SubItemsExpandWidth = 24;
            this.buttonOptions.Text = "KGB Opt&ions";
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
            // ribbonTabItemVehicles
            // 
            this.ribbonTabItemVehicles.Checked = true;
            this.ribbonTabItemVehicles.Name = "ribbonTabItemVehicles";
            this.ribbonTabItemVehicles.Panel = this.ribbonPanel10;
            this.ribbonTabItemVehicles.Text = "Vehicles";
            this.ribbonTabItemVehicles.Click += new System.EventHandler(this.ribbonTabItemVehicles_Click);
            // 
            // ribbonTabItemDisplay
            // 
            this.ribbonTabItemDisplay.Group = this.ribbonTabItemGroup2;
            this.ribbonTabItemDisplay.Name = "ribbonTabItemDisplay";
            this.ribbonTabItemDisplay.Panel = this.ribbonPanelDisplay;
            this.ribbonTabItemDisplay.Text = "&Display";
            // 
            // ribbonTabItemGroup2
            // 
            this.ribbonTabItemGroup2.GroupTitle = "New Group";
            this.ribbonTabItemGroup2.Name = "ribbonTabItemGroup2";
            // 
            // 
            // 
            this.ribbonTabItemGroup2.Style.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(174)))), ((int)(((byte)(109)))), ((int)(((byte)(148)))));
            this.ribbonTabItemGroup2.Style.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(144)))), ((int)(((byte)(72)))), ((int)(((byte)(123)))));
            this.ribbonTabItemGroup2.Style.BackColorGradientAngle = 90;
            this.ribbonTabItemGroup2.Style.BorderBottom = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.ribbonTabItemGroup2.Style.BorderBottomWidth = 1;
            this.ribbonTabItemGroup2.Style.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(58)))), ((int)(((byte)(59)))));
            this.ribbonTabItemGroup2.Style.BorderLeft = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.ribbonTabItemGroup2.Style.BorderLeftWidth = 1;
            this.ribbonTabItemGroup2.Style.BorderRight = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.ribbonTabItemGroup2.Style.BorderRightWidth = 1;
            this.ribbonTabItemGroup2.Style.BorderTop = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.ribbonTabItemGroup2.Style.BorderTopWidth = 1;
            this.ribbonTabItemGroup2.Style.Class = "";
            this.ribbonTabItemGroup2.Style.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonTabItemGroup2.Style.TextAlignment = DevComponents.DotNetBar.eStyleTextAlignment.Center;
            this.ribbonTabItemGroup2.Style.TextColor = System.Drawing.Color.White;
            this.ribbonTabItemGroup2.Style.TextLineAlignment = DevComponents.DotNetBar.eStyleTextAlignment.Near;
            this.ribbonTabItemGroup2.Style.TextShadowColor = System.Drawing.Color.Black;
            this.ribbonTabItemGroup2.Style.TextShadowOffset = new System.Drawing.Point(1, 1);
            // 
            // ribbonTabItemEdit
            // 
            this.ribbonTabItemEdit.Group = this.ribbonTabItemGroup2;
            this.ribbonTabItemEdit.Name = "ribbonTabItemEdit";
            this.ribbonTabItemEdit.Panel = this.ribbonPanelEdit;
            this.ribbonTabItemEdit.Text = "&Edit";
            // 
            // ribbonTabItemFX
            // 
            this.ribbonTabItemFX.Name = "ribbonTabItemFX";
            this.ribbonTabItemFX.Panel = this.ribbonPanel5;
            this.ribbonTabItemFX.Text = "FX";
            // 
            // ribbonTabItemLights
            // 
            this.ribbonTabItemLights.Name = "ribbonTabItemLights";
            this.ribbonTabItemLights.Panel = this.ribbonPanel4;
            this.ribbonTabItemLights.Text = "Lights";
            // 
            // ribbonTabItemPrimitives
            // 
            this.ribbonTabItemPrimitives.Name = "ribbonTabItemPrimitives";
            this.ribbonTabItemPrimitives.Panel = this.ribbonPanel1;
            this.ribbonTabItemPrimitives.Text = "Primitives";
            // 
            // ribbonTabItemPrefabs
            // 
            this.ribbonTabItemPrefabs.ColorTable = DevComponents.DotNetBar.eRibbonTabColor.Orange;
            this.ribbonTabItemPrefabs.Group = this.ribbonTabItemGroup1;
            this.ribbonTabItemPrefabs.Name = "ribbonTabItemPrefabs";
            this.ribbonTabItemPrefabs.Panel = this.ribbonPanelContext;
            this.ribbonTabItemPrefabs.Text = "Prefabs";
            // 
            // ribbonTabItemGroup1
            // 
            this.ribbonTabItemGroup1.Color = DevComponents.DotNetBar.eRibbonTabGroupColor.Orange;
            this.ribbonTabItemGroup1.GroupTitle = "Contextual Group";
            this.ribbonTabItemGroup1.Name = "ribbonTabItemGroup1";
            // 
            // 
            // 
            this.ribbonTabItemGroup1.Style.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(158)))), ((int)(((byte)(159)))));
            this.ribbonTabItemGroup1.Style.BackColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(225)))), ((int)(((byte)(226)))));
            this.ribbonTabItemGroup1.Style.BackColorGradientAngle = 90;
            this.ribbonTabItemGroup1.Style.BorderBottom = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.ribbonTabItemGroup1.Style.BorderBottomWidth = 1;
            this.ribbonTabItemGroup1.Style.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(154)))), ((int)(((byte)(58)))), ((int)(((byte)(59)))));
            this.ribbonTabItemGroup1.Style.BorderLeft = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.ribbonTabItemGroup1.Style.BorderLeftWidth = 1;
            this.ribbonTabItemGroup1.Style.BorderRight = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.ribbonTabItemGroup1.Style.BorderRightWidth = 1;
            this.ribbonTabItemGroup1.Style.BorderTop = DevComponents.DotNetBar.eStyleBorderType.Solid;
            this.ribbonTabItemGroup1.Style.BorderTopWidth = 1;
            this.ribbonTabItemGroup1.Style.Class = "";
            this.ribbonTabItemGroup1.Style.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.ribbonTabItemGroup1.Style.TextAlignment = DevComponents.DotNetBar.eStyleTextAlignment.Center;
            this.ribbonTabItemGroup1.Style.TextColor = System.Drawing.Color.Black;
            this.ribbonTabItemGroup1.Style.TextLineAlignment = DevComponents.DotNetBar.eStyleTextAlignment.Near;
            // 
            // ribbonTabItemProceduralTexture
            // 
            this.ribbonTabItemProceduralTexture.Name = "ribbonTabItemProceduralTexture";
            this.ribbonTabItemProceduralTexture.Panel = this.ribbonPanel9;
            this.ribbonTabItemProceduralTexture.Text = "Procedural Texture";
            this.ribbonTabItemProceduralTexture.Click += new System.EventHandler(this.ribbonTabItemProceduralTexture_Click);
            // 
            // ribbonTabItemTextures
            // 
            this.ribbonTabItemTextures.Name = "ribbonTabItemTextures";
            this.ribbonTabItemTextures.Panel = this.ribbonPanel3;
            this.ribbonTabItemTextures.Text = "Textures";
            // 
            // ribbonTabItemMaterials
            // 
            this.ribbonTabItemMaterials.Name = "ribbonTabItemMaterials";
            this.ribbonTabItemMaterials.Panel = this.ribbonPanel2;
            this.ribbonTabItemMaterials.Text = "Materials";
            // 
            // ribbonTabItemShaders
            // 
            this.ribbonTabItemShaders.Name = "ribbonTabItemShaders";
            this.ribbonTabItemShaders.Panel = this.ribbonPanel8;
            this.ribbonTabItemShaders.Text = "Shaders";
            // 
            // ribbonTabItemBehaviors
            // 
            this.ribbonTabItemBehaviors.Name = "ribbonTabItemBehaviors";
            this.ribbonTabItemBehaviors.Panel = this.ribbonPanel7;
            this.ribbonTabItemBehaviors.Text = "Behaviors";
            // 
            // ribbonTabItemPhysics
            // 
            this.ribbonTabItemPhysics.Name = "ribbonTabItemPhysics";
            this.ribbonTabItemPhysics.Panel = this.ribbonPanel11;
            this.ribbonTabItemPhysics.Text = "Physics";
            // 
            // ribbonTabItemScripts
            // 
            this.ribbonTabItemScripts.Name = "ribbonTabItemScripts";
            this.ribbonTabItemScripts.Panel = this.ribbonPanel6;
            this.ribbonTabItemScripts.Text = "Scripts";
            // 
            // buttonChangeStyle
            // 
            this.buttonChangeStyle.AutoExpandOnClick = true;
            this.buttonChangeStyle.ItemAlignment = DevComponents.DotNetBar.eItemAlignment.Far;
            this.buttonChangeStyle.Name = "buttonChangeStyle";
            this.buttonChangeStyle.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonStyleOffice2010Blue,
            this.buttonStyleOffice2010Silver,
            this.buttonItem62,
            this.buttonStyleOffice2007Blue,
            this.buttonStyleOffice2007Black,
            this.buttonStyleOffice2007Silver,
            this.buttonItem60,
            this.buttonStyleCustom});
            this.superTooltip1.SetSuperTooltip(this.buttonChangeStyle, new DevComponents.DotNetBar.SuperTooltipInfo("Change the style", "", "Change the style of all DotNetBar User Interface elements.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonChangeStyle.Text = "Style";
            // 
            // buttonStyleOffice2010Blue
            // 
            this.buttonStyleOffice2010Blue.Checked = true;
            this.buttonStyleOffice2010Blue.Command = this.AppCommandTheme;
            this.buttonStyleOffice2010Blue.CommandParameter = "Office2010Blue";
            this.buttonStyleOffice2010Blue.Name = "buttonStyleOffice2010Blue";
            this.buttonStyleOffice2010Blue.OptionGroup = "style";
            this.buttonStyleOffice2010Blue.Text = "Office 2010 Blue";
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
            this.buttonStyleOffice2010Silver.Text = "Office 2010 <font color=\"Silver\"><b>Silver</b></font>";
            // 
            // buttonItem62
            // 
            this.buttonItem62.Command = this.AppCommandTheme;
            this.buttonItem62.CommandParameter = "Windows7Blue";
            this.buttonItem62.Name = "buttonItem62";
            this.buttonItem62.OptionGroup = "style";
            this.buttonItem62.Text = "Windows 7";
            // 
            // buttonStyleOffice2007Blue
            // 
            this.buttonStyleOffice2007Blue.Command = this.AppCommandTheme;
            this.buttonStyleOffice2007Blue.CommandParameter = "Office2007Blue";
            this.buttonStyleOffice2007Blue.Name = "buttonStyleOffice2007Blue";
            this.buttonStyleOffice2007Blue.OptionGroup = "style";
            this.buttonStyleOffice2007Blue.Text = "Office 2007 <font color=\"Blue\"><b>Blue</b></font>";
            // 
            // buttonStyleOffice2007Black
            // 
            this.buttonStyleOffice2007Black.Command = this.AppCommandTheme;
            this.buttonStyleOffice2007Black.CommandParameter = "Office2007Black";
            this.buttonStyleOffice2007Black.Name = "buttonStyleOffice2007Black";
            this.buttonStyleOffice2007Black.OptionGroup = "style";
            this.buttonStyleOffice2007Black.Text = "Office 2007 <font color=\"black\"><b>Black</b></font>";
            // 
            // buttonStyleOffice2007Silver
            // 
            this.buttonStyleOffice2007Silver.Command = this.AppCommandTheme;
            this.buttonStyleOffice2007Silver.CommandParameter = "Office2007Silver";
            this.buttonStyleOffice2007Silver.Name = "buttonStyleOffice2007Silver";
            this.buttonStyleOffice2007Silver.OptionGroup = "style";
            this.buttonStyleOffice2007Silver.Text = "Office 2007 <font color=\"Silver\"><b>Silver</b></font>";
            // 
            // buttonItem60
            // 
            this.buttonItem60.Command = this.AppCommandTheme;
            this.buttonItem60.CommandParameter = "Office2007VistaGlass";
            this.buttonItem60.Name = "buttonItem60";
            this.buttonItem60.OptionGroup = "style";
            this.buttonItem60.Text = "Vista Glass";
            // 
            // buttonStyleCustom
            // 
            this.buttonStyleCustom.BeginGroup = true;
            this.buttonStyleCustom.Command = this.AppCommandTheme;
            this.buttonStyleCustom.Name = "buttonStyleCustom";
            this.buttonStyleCustom.Text = "Custom scheme";
            this.buttonStyleCustom.Tooltip = "Custom color scheme is created based on currently selected color table. Try selec" +
                "ting Silver or Blue color table and then creating custom color scheme.";
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
            // buttonUndo
            // 
            this.buttonUndo.Enabled = false;
            this.buttonUndo.Image = ((System.Drawing.Image)(resources.GetObject("buttonUndo.Image")));
            this.buttonUndo.Name = "buttonUndo";
            this.buttonUndo.Text = "Undo";
            // 
            // qatCustomizeItem1
            // 
            this.qatCustomizeItem1.Name = "qatCustomizeItem1";
            // 
            // superTooltip1
            // 
            this.superTooltip1.DefaultFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.superTooltip1.MinimumTooltipSize = new System.Drawing.Size(150, 50);
            // 
            // buttonItemBackColor
            // 
            this.buttonItemBackColor.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItemBackColor.Image = ((System.Drawing.Image)(resources.GetObject("buttonItemBackColor.Image")));
            this.buttonItemBackColor.Name = "buttonItemBackColor";
            this.buttonItemBackColor.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.controlContainerItem1,
            this.itemContainer7,
            this.buttonItem36});
            this.superTooltip1.SetSuperTooltip(this.buttonItemBackColor, new DevComponents.DotNetBar.SuperTooltipInfo("Shading", "", "Changes shading of selected text.\r\n\r\nThis feature has not been implemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonItemBackColor.Text = "&Back Color";
            // 
            // controlContainerItem1
            // 
            this.controlContainerItem1.AllowItemResize = true;
            this.controlContainerItem1.MenuVisibility = DevComponents.DotNetBar.eMenuVisibility.VisibleAlways;
            this.controlContainerItem1.Name = "controlContainerItem1";
            this.controlContainerItem1.Text = "controlContainerItem1";
            // 
            // itemContainer7
            // 
            // 
            // 
            // 
            this.itemContainer7.BackgroundStyle.Class = "";
            this.itemContainer7.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.itemContainer7.BeginGroup = true;
            this.itemContainer7.Name = "itemContainer7";
            // 
            // buttonItem36
            // 
            this.buttonItem36.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItem36.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem36.Image")));
            this.buttonItem36.Name = "buttonItem36";
            this.buttonItem36.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.controlContainerItem2,
            this.itemContainer14});
            this.superTooltip1.SetSuperTooltip(this.buttonItem36, new DevComponents.DotNetBar.SuperTooltipInfo("Shading", "", "Changes shading of selected text.\r\n\r\nThis feature has not been implemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonItem36.Text = "&Back Color";
            // 
            // controlContainerItem2
            // 
            this.controlContainerItem2.AllowItemResize = true;
            this.controlContainerItem2.MenuVisibility = DevComponents.DotNetBar.eMenuVisibility.VisibleAlways;
            this.controlContainerItem2.Name = "controlContainerItem2";
            this.controlContainerItem2.Text = "controlContainerItem1";
            // 
            // itemContainer14
            // 
            // 
            // 
            // 
            this.itemContainer14.BackgroundStyle.Class = "";
            this.itemContainer14.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.itemContainer14.BeginGroup = true;
            this.itemContainer14.Name = "itemContainer14";
            // 
            // buttonItem38
            // 
            this.buttonItem38.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItem38.Enabled = false;
            this.buttonItem38.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem38.Image")));
            this.buttonItem38.Name = "buttonItem38";
            this.superTooltip1.SetSuperTooltip(this.buttonItem38, new DevComponents.DotNetBar.SuperTooltipInfo("Replace", "", "Find and replace the text in document.\r\n\r\nThis feature has not been implemented y" +
                        "et.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonItem38.Text = "&Replace";
            // 
            // buttonItem39
            // 
            this.buttonItem39.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItem39.Enabled = false;
            this.buttonItem39.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem39.Image")));
            this.buttonItem39.Name = "buttonItem39";
            this.superTooltip1.SetSuperTooltip(this.buttonItem39, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonItem39.Text = "&Goto";
            // 
            // buttonItem70
            // 
            this.buttonItem70.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItem70.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem70.Image")));
            this.buttonItem70.Name = "buttonItem70";
            this.superTooltip1.SetSuperTooltip(this.buttonItem70, new DevComponents.DotNetBar.SuperTooltipInfo("Go to line", "", "Go to specified line number in current document.\r\n\r\nThis feature has not been imp" +
                        "lemented yet.", null, null, DevComponents.DotNetBar.eTooltipColor.Gray));
            this.buttonItem70.Text = "Perlin";
            // 
            // styleManager1
            // 
            this.styleManager1.ManagerStyle = DevComponents.DotNetBar.eStyle.Office2010Blue;
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
            this.dotNetBarManager.ItemClick += new System.EventHandler(this.dotNetBarManager_ItemClick);
            // 
            // dockSite4
            // 
            this.dockSite4.AccessibleRole = System.Windows.Forms.AccessibleRole.Window;
            this.dockSite4.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.dockSite4.DocumentDockContainer = new DevComponents.DotNetBar.DocumentDockContainer();
            this.dockSite4.Location = new System.Drawing.Point(5, 740);
            this.dockSite4.Name = "dockSite4";
            this.dockSite4.Size = new System.Drawing.Size(1014, 0);
            this.dockSite4.TabIndex = 17;
            this.dockSite4.TabStop = false;
            // 
            // dockSite9
            // 
            this.dockSite9.AccessibleRole = System.Windows.Forms.AccessibleRole.Window;
            this.dockSite9.Controls.Add(this.barDocumentDockBar);
            this.dockSite9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dockSite9.DocumentDockContainer = new DevComponents.DotNetBar.DocumentDockContainer(new DevComponents.DotNetBar.DocumentBaseContainer[] {
            ((DevComponents.DotNetBar.DocumentBaseContainer)(new DevComponents.DotNetBar.DocumentBarContainer(this.barDocumentDockBar, 558, 595)))}, DevComponents.DotNetBar.eOrientation.Horizontal);
            this.dockSite9.Location = new System.Drawing.Point(184, 145);
            this.dockSite9.Name = "dockSite9";
            this.dockSite9.Size = new System.Drawing.Size(558, 595);
            this.dockSite9.TabIndex = 22;
            this.dockSite9.TabStop = false;
            // 
            // barDocumentDockBar
            // 
            this.barDocumentDockBar.AccessibleDescription = "DotNetBar Bar (barDocumentDockBar)";
            this.barDocumentDockBar.AccessibleName = "DotNetBar Bar";
            this.barDocumentDockBar.AccessibleRole = System.Windows.Forms.AccessibleRole.ToolBar;
            this.barDocumentDockBar.CanAutoHide = false;
            this.barDocumentDockBar.CanCustomize = false;
            this.barDocumentDockBar.CanDockBottom = false;
            this.barDocumentDockBar.CanDockDocument = true;
            this.barDocumentDockBar.CanDockLeft = false;
            this.barDocumentDockBar.CanDockRight = false;
            this.barDocumentDockBar.CanDockTop = false;
            this.barDocumentDockBar.CanHide = true;
            this.barDocumentDockBar.CanUndock = false;
            this.barDocumentDockBar.DockTabAlignment = DevComponents.DotNetBar.eTabStripAlignment.Top;
            this.barDocumentDockBar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.barDocumentDockBar.LayoutType = DevComponents.DotNetBar.eLayoutType.DockContainer;
            this.barDocumentDockBar.Location = new System.Drawing.Point(0, 0);
            this.barDocumentDockBar.Name = "barDocumentDockBar";
            this.barDocumentDockBar.Size = new System.Drawing.Size(558, 595);
            this.barDocumentDockBar.Stretch = true;
            this.barDocumentDockBar.Style = DevComponents.DotNetBar.eDotNetBarStyle.Office2003;
            this.barDocumentDockBar.TabIndex = 0;
            this.barDocumentDockBar.TabStop = false;
            // 
            // dockSite1
            // 
            this.dockSite1.AccessibleRole = System.Windows.Forms.AccessibleRole.Window;
            this.dockSite1.Controls.Add(this.barLeftDockBar);
            this.dockSite1.Dock = System.Windows.Forms.DockStyle.Left;
            this.dockSite1.DocumentDockContainer = new DevComponents.DotNetBar.DocumentDockContainer(new DevComponents.DotNetBar.DocumentBaseContainer[] {
            ((DevComponents.DotNetBar.DocumentBaseContainer)(new DevComponents.DotNetBar.DocumentBarContainer(this.barLeftDockBar, 176, 595)))}, DevComponents.DotNetBar.eOrientation.Horizontal);
            this.dockSite1.Location = new System.Drawing.Point(5, 145);
            this.dockSite1.Name = "dockSite1";
            this.dockSite1.Size = new System.Drawing.Size(179, 595);
            this.dockSite1.TabIndex = 14;
            this.dockSite1.TabStop = false;
            // 
            // barLeftDockBar
            // 
            this.barLeftDockBar.AccessibleDescription = "DotNetBar Bar (barLeftDockBar)";
            this.barLeftDockBar.AccessibleName = "DotNetBar Bar";
            this.barLeftDockBar.AccessibleRole = System.Windows.Forms.AccessibleRole.ToolBar;
            this.barLeftDockBar.AutoSyncBarCaption = true;
            this.barLeftDockBar.CloseSingleTab = true;
            this.barLeftDockBar.Controls.Add(this.panelDockContainerLeft);
            this.barLeftDockBar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.barLeftDockBar.GrabHandleStyle = DevComponents.DotNetBar.eGrabHandleStyle.Caption;
            this.barLeftDockBar.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.dockContainerItemLeft});
            this.barLeftDockBar.LayoutType = DevComponents.DotNetBar.eLayoutType.DockContainer;
            this.barLeftDockBar.Location = new System.Drawing.Point(0, 0);
            this.barLeftDockBar.Name = "barLeftDockBar";
            this.barLeftDockBar.Size = new System.Drawing.Size(176, 595);
            this.barLeftDockBar.Stretch = true;
            this.barLeftDockBar.Style = DevComponents.DotNetBar.eDotNetBarStyle.Office2003;
            this.barLeftDockBar.TabIndex = 0;
            this.barLeftDockBar.TabStop = false;
            this.barLeftDockBar.Text = "dockContainerItemLeft";
            // 
            // panelDockContainerLeft
            // 
            this.panelDockContainerLeft.Controls.Add(this.treeEntityBrowser);
            this.panelDockContainerLeft.Location = new System.Drawing.Point(3, 23);
            this.panelDockContainerLeft.Name = "panelDockContainerLeft";
            this.panelDockContainerLeft.Size = new System.Drawing.Size(170, 569);
            this.panelDockContainerLeft.Style.Alignment = System.Drawing.StringAlignment.Center;
            this.panelDockContainerLeft.Style.BackColor1.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.BarBackground;
            this.panelDockContainerLeft.Style.BorderColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.BarDockedBorder;
            this.panelDockContainerLeft.Style.ForeColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.ItemText;
            this.panelDockContainerLeft.Style.GradientAngle = 90;
            this.panelDockContainerLeft.TabIndex = 0;
            // 
            // treeEntityBrowser
            // 
            this.treeEntityBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeEntityBrowser.Location = new System.Drawing.Point(0, 0);
            this.treeEntityBrowser.Name = "treeEntityBrowser";
            this.treeEntityBrowser.Size = new System.Drawing.Size(170, 569);
            this.treeEntityBrowser.TabIndex = 1;
            this.treeEntityBrowser.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeEntityBrowser_NodeMouseDoubleClick);
            this.treeEntityBrowser.MouseUp += new System.Windows.Forms.MouseEventHandler(this.treeEntityBrowser_MouseUp);
            this.treeEntityBrowser.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeEntityBrowser_NodeMouseClick);
            // 
            // dockContainerItemLeft
            // 
            this.dockContainerItemLeft.Control = this.panelDockContainerLeft;
            this.dockContainerItemLeft.Name = "dockContainerItemLeft";
            this.dockContainerItemLeft.Text = "dockContainerItemLeft";
            // 
            // dockSite2
            // 
            this.dockSite2.AccessibleRole = System.Windows.Forms.AccessibleRole.Window;
            this.dockSite2.Controls.Add(this.barRightDockBar);
            this.dockSite2.Dock = System.Windows.Forms.DockStyle.Right;
            this.dockSite2.DocumentDockContainer = new DevComponents.DotNetBar.DocumentDockContainer(new DevComponents.DotNetBar.DocumentBaseContainer[] {
            ((DevComponents.DotNetBar.DocumentBaseContainer)(new DevComponents.DotNetBar.DocumentBarContainer(this.barRightDockBar, 274, 595)))}, DevComponents.DotNetBar.eOrientation.Horizontal);
            this.dockSite2.Location = new System.Drawing.Point(742, 145);
            this.dockSite2.Name = "dockSite2";
            this.dockSite2.Size = new System.Drawing.Size(277, 595);
            this.dockSite2.TabIndex = 15;
            this.dockSite2.TabStop = false;
            // 
            // barRightDockBar
            // 
            this.barRightDockBar.AccessibleDescription = "DotNetBar Bar (barRightDockBar)";
            this.barRightDockBar.AccessibleName = "DotNetBar Bar";
            this.barRightDockBar.AccessibleRole = System.Windows.Forms.AccessibleRole.ToolBar;
            this.barRightDockBar.AutoSyncBarCaption = true;
            this.barRightDockBar.CloseSingleTab = true;
            this.barRightDockBar.Controls.Add(this.panelDockContainerRight);
            this.barRightDockBar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.barRightDockBar.GrabHandleStyle = DevComponents.DotNetBar.eGrabHandleStyle.Caption;
            this.barRightDockBar.Items.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.dockContainerItemRight});
            this.barRightDockBar.LayoutType = DevComponents.DotNetBar.eLayoutType.DockContainer;
            this.barRightDockBar.Location = new System.Drawing.Point(3, 0);
            this.barRightDockBar.Name = "barRightDockBar";
            this.barRightDockBar.Size = new System.Drawing.Size(274, 595);
            this.barRightDockBar.Stretch = true;
            this.barRightDockBar.Style = DevComponents.DotNetBar.eDotNetBarStyle.Office2003;
            this.barRightDockBar.TabIndex = 0;
            this.barRightDockBar.TabStop = false;
            this.barRightDockBar.Text = "dockContainerItemRight";
            // 
            // panelDockContainerRight
            // 
            this.panelDockContainerRight.Controls.Add(this.propertyGrid);
            this.panelDockContainerRight.Location = new System.Drawing.Point(3, 23);
            this.panelDockContainerRight.Name = "panelDockContainerRight";
            this.panelDockContainerRight.Size = new System.Drawing.Size(268, 569);
            this.panelDockContainerRight.Style.Alignment = System.Drawing.StringAlignment.Center;
            this.panelDockContainerRight.Style.BackColor1.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.BarBackground;
            this.panelDockContainerRight.Style.BorderColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.BarDockedBorder;
            this.panelDockContainerRight.Style.ForeColor.ColorSchemePart = DevComponents.DotNetBar.eColorSchemePart.ItemText;
            this.panelDockContainerRight.Style.GradientAngle = 90;
            this.panelDockContainerRight.TabIndex = 0;
            // 
            // propertyGrid
            // 
            this.propertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid.Location = new System.Drawing.Point(0, 0);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.Size = new System.Drawing.Size(268, 569);
            this.propertyGrid.TabIndex = 0;
            // 
            // dockContainerItemRight
            // 
            this.dockContainerItemRight.Control = this.panelDockContainerRight;
            this.dockContainerItemRight.Name = "dockContainerItemRight";
            this.dockContainerItemRight.Text = "dockContainerItemRight";
            // 
            // dockSite8
            // 
            this.dockSite8.AccessibleRole = System.Windows.Forms.AccessibleRole.Window;
            this.dockSite8.Controls.Add(this.barStatusBar);
            this.dockSite8.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.dockSite8.Location = new System.Drawing.Point(5, 740);
            this.dockSite8.Name = "dockSite8";
            this.dockSite8.Size = new System.Drawing.Size(1014, 26);
            this.dockSite8.TabIndex = 21;
            this.dockSite8.TabStop = false;
            // 
            // barStatusBar
            // 
            this.barStatusBar.AccessibleDescription = "DotNetBar Bar (barStatusBar)";
            this.barStatusBar.AccessibleName = "DotNetBar Bar";
            this.barStatusBar.AccessibleRole = System.Windows.Forms.AccessibleRole.StatusBar;
            this.barStatusBar.AutoSyncBarCaption = true;
            this.barStatusBar.BarType = DevComponents.DotNetBar.eBarType.StatusBar;
            this.barStatusBar.CanAutoHide = false;
            this.barStatusBar.CanCustomize = false;
            this.barStatusBar.CanDockLeft = false;
            this.barStatusBar.CanDockRight = false;
            this.barStatusBar.CanDockTab = false;
            this.barStatusBar.CanDockTop = false;
            this.barStatusBar.CanReorderTabs = false;
            this.barStatusBar.CanUndock = false;
            this.barStatusBar.CloseSingleTab = true;
            this.barStatusBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.barStatusBar.DockSide = DevComponents.DotNetBar.eDockSide.Bottom;
            this.barStatusBar.GrabHandleStyle = DevComponents.DotNetBar.eGrabHandleStyle.ResizeHandle;
            this.barStatusBar.Location = new System.Drawing.Point(0, 1);
            this.barStatusBar.Name = "barStatusBar";
            this.barStatusBar.Size = new System.Drawing.Size(1014, 25);
            this.barStatusBar.Stretch = true;
            this.barStatusBar.Style = DevComponents.DotNetBar.eDotNetBarStyle.Office2003;
            this.barStatusBar.TabIndex = 0;
            this.barStatusBar.TabStop = false;
            this.barStatusBar.Text = "barStatusBar";
            // 
            // dockSite5
            // 
            this.dockSite5.AccessibleRole = System.Windows.Forms.AccessibleRole.Window;
            this.dockSite5.Dock = System.Windows.Forms.DockStyle.Left;
            this.dockSite5.Location = new System.Drawing.Point(5, 1);
            this.dockSite5.Name = "dockSite5";
            this.dockSite5.Size = new System.Drawing.Size(0, 739);
            this.dockSite5.TabIndex = 18;
            this.dockSite5.TabStop = false;
            // 
            // dockSite6
            // 
            this.dockSite6.AccessibleRole = System.Windows.Forms.AccessibleRole.Window;
            this.dockSite6.Dock = System.Windows.Forms.DockStyle.Right;
            this.dockSite6.Location = new System.Drawing.Point(1019, 1);
            this.dockSite6.Name = "dockSite6";
            this.dockSite6.Size = new System.Drawing.Size(0, 739);
            this.dockSite6.TabIndex = 19;
            this.dockSite6.TabStop = false;
            // 
            // dockSite7
            // 
            this.dockSite7.AccessibleRole = System.Windows.Forms.AccessibleRole.Window;
            this.dockSite7.Dock = System.Windows.Forms.DockStyle.Top;
            this.dockSite7.Location = new System.Drawing.Point(5, 1);
            this.dockSite7.Name = "dockSite7";
            this.dockSite7.Size = new System.Drawing.Size(1014, 0);
            this.dockSite7.TabIndex = 20;
            this.dockSite7.TabStop = false;
            // 
            // dockSite3
            // 
            this.dockSite3.AccessibleRole = System.Windows.Forms.AccessibleRole.Window;
            this.dockSite3.Dock = System.Windows.Forms.DockStyle.Top;
            this.dockSite3.DocumentDockContainer = new DevComponents.DotNetBar.DocumentDockContainer();
            this.dockSite3.Enabled = false;
            this.dockSite3.Location = new System.Drawing.Point(5, 1);
            this.dockSite3.Name = "dockSite3";
            this.dockSite3.Size = new System.Drawing.Size(1014, 0);
            this.dockSite3.TabIndex = 16;
            this.dockSite3.TabStop = false;
            // 
            // itemContainer13
            // 
            // 
            // 
            // 
            this.itemContainer13.BackgroundStyle.Class = "Office2007StatusBarBackground2";
            this.itemContainer13.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.itemContainer13.Name = "itemContainer13";
            this.itemContainer13.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.labelPosition,
            this.zoomSlider});
            // 
            // labelPosition
            // 
            this.labelPosition.Name = "labelPosition";
            this.labelPosition.PaddingLeft = 2;
            this.labelPosition.PaddingRight = 2;
            this.labelPosition.SingleLineColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(97)))), ((int)(((byte)(156)))));
            this.labelPosition.Width = 100;
            // 
            // zoomSlider
            // 
            this.zoomSlider.Maximum = 200;
            this.zoomSlider.Name = "zoomSlider";
            this.zoomSlider.Step = 5;
            this.zoomSlider.Text = "100%";
            this.zoomSlider.Value = 100;
            // 
            // progressBarItem1
            // 
            // 
            // 
            // 
            this.progressBarItem1.BackStyle.Class = "";
            this.progressBarItem1.BackStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.progressBarItem1.ChunkGradientAngle = 0F;
            this.progressBarItem1.MenuVisibility = DevComponents.DotNetBar.eMenuVisibility.VisibleAlways;
            this.progressBarItem1.Name = "progressBarItem1";
            this.progressBarItem1.RecentlyUsed = false;
            // 
            // itemContainer9
            // 
            // 
            // 
            // 
            this.itemContainer9.BackgroundStyle.Class = "";
            this.itemContainer9.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.itemContainer9.BeginGroup = true;
            this.itemContainer9.Name = "itemContainer9";
            this.itemContainer9.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.buttonItem13,
            this.buttonItem14,
            this.buttonItem15,
            this.buttonItem16,
            this.buttonItem17});
            // 
            // buttonItem13
            // 
            this.buttonItem13.Checked = true;
            this.buttonItem13.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem13.Image")));
            this.buttonItem13.ImagePaddingVertical = 9;
            this.buttonItem13.Name = "buttonItem13";
            this.buttonItem13.OptionGroup = "statusGroup";
            this.buttonItem13.Text = "Print Layout";
            this.buttonItem13.Tooltip = "Print Layout";
            // 
            // buttonItem14
            // 
            this.buttonItem14.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem14.Image")));
            this.buttonItem14.ImagePaddingVertical = 9;
            this.buttonItem14.Name = "buttonItem14";
            this.buttonItem14.OptionGroup = "statusGroup";
            this.buttonItem14.Text = "Web Layout";
            this.buttonItem14.Tooltip = "Web Layout";
            // 
            // buttonItem15
            // 
            this.buttonItem15.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem15.Image")));
            this.buttonItem15.ImagePaddingVertical = 9;
            this.buttonItem15.Name = "buttonItem15";
            this.buttonItem15.OptionGroup = "statusGroup";
            this.buttonItem15.Text = "Full Screen";
            this.buttonItem15.Tooltip = "Full Screen Reading";
            // 
            // buttonItem16
            // 
            this.buttonItem16.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem16.Image")));
            this.buttonItem16.ImagePaddingVertical = 9;
            this.buttonItem16.Name = "buttonItem16";
            this.buttonItem16.OptionGroup = "statusGroup";
            this.buttonItem16.Text = "Outline";
            this.buttonItem16.Tooltip = "Outline";
            // 
            // buttonItem17
            // 
            this.buttonItem17.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem17.Image")));
            this.buttonItem17.ImagePaddingVertical = 9;
            this.buttonItem17.Name = "buttonItem17";
            this.buttonItem17.OptionGroup = "statusGroup";
            this.buttonItem17.Text = "Draft";
            this.buttonItem17.Tooltip = "Draft";
            // 
            // labelStatus
            // 
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.PaddingLeft = 2;
            this.labelStatus.PaddingRight = 2;
            this.labelStatus.SingleLineColor = System.Drawing.Color.FromArgb(((int)(((byte)(59)))), ((int)(((byte)(97)))), ((int)(((byte)(156)))));
            this.labelStatus.Stretch = true;
            // 
            // buttonItem5
            // 
            this.buttonItem5.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItem5.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem5.Image")));
            this.buttonItem5.ImagePosition = DevComponents.DotNetBar.eImagePosition.Top;
            this.buttonItem5.Name = "buttonItem5";
            this.buttonItem5.RibbonWordWrap = false;
            this.buttonItem5.Text = "Command 1";
            // 
            // buttonItem18
            // 
            this.buttonItem18.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItem18.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem18.Image")));
            this.buttonItem18.ImagePosition = DevComponents.DotNetBar.eImagePosition.Top;
            this.buttonItem18.Name = "buttonItem18";
            this.buttonItem18.RibbonWordWrap = false;
            this.buttonItem18.Text = "Command 1";
            // 
            // itemContainer16
            // 
            // 
            // 
            // 
            this.itemContainer16.BackgroundStyle.Class = "";
            this.itemContainer16.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.itemContainer16.LayoutOrientation = DevComponents.DotNetBar.eOrientation.Vertical;
            this.itemContainer16.Name = "itemContainer16";
            this.itemContainer16.SubItems.AddRange(new DevComponents.DotNetBar.BaseItem[] {
            this.checkBoxItem10,
            this.checkBoxItem11,
            this.checkBoxItem12});
            // 
            // checkBoxItem10
            // 
            this.checkBoxItem10.Name = "checkBoxItem10";
            this.checkBoxItem10.Text = "Create Normal Map";
            this.checkBoxItem10.ThreeState = true;
            // 
            // checkBoxItem11
            // 
            this.checkBoxItem11.Name = "checkBoxItem11";
            this.checkBoxItem11.Text = "Use Palette";
            // 
            // checkBoxItem12
            // 
            this.checkBoxItem12.Name = "checkBoxItem12";
            this.checkBoxItem12.Text = "Random Seed";
            // 
            // buttonItem43
            // 
            this.buttonItem43.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItem43.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem43.Image")));
            this.buttonItem43.ImagePosition = DevComponents.DotNetBar.eImagePosition.Top;
            this.buttonItem43.Name = "buttonItem43";
            this.buttonItem43.RibbonWordWrap = false;
            this.buttonItem43.Text = "Blue Planet";
            // 
            // buttonItem44
            // 
            this.buttonItem44.ButtonStyle = DevComponents.DotNetBar.eButtonStyle.ImageAndText;
            this.buttonItem44.Image = ((System.Drawing.Image)(resources.GetObject("buttonItem44.Image")));
            this.buttonItem44.ImagePosition = DevComponents.DotNetBar.eImagePosition.Top;
            this.buttonItem44.Name = "buttonItem44";
            this.buttonItem44.RibbonWordWrap = false;
            this.buttonItem44.Text = "Star";
            // 
            // FormMain2
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.ClientSize = new System.Drawing.Size(1024, 768);
            this.Controls.Add(this.dockSite9);
            this.Controls.Add(this.dockSite2);
            this.Controls.Add(this.dockSite1);
            this.Controls.Add(this.contextMenuBar1);
            this.Controls.Add(this.ribbonControl);
            this.Controls.Add(this.dockSite3);
            this.Controls.Add(this.dockSite4);
            this.Controls.Add(this.dockSite5);
            this.Controls.Add(this.dockSite6);
            this.Controls.Add(this.dockSite7);
            this.Controls.Add(this.dockSite8);
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "FormMain2";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Keystone Game Blocks (KGB) - Untitled";
            this.ribbonControl.ResumeLayout(false);
            this.ribbonControl.PerformLayout();
            this.ribbonPanel10.ResumeLayout(false);
            this.ribbonPanel5.ResumeLayout(false);
            this.ribbonPanel1.ResumeLayout(false);
            this.ribbonPanelEdit.ResumeLayout(false);
            this.ribbonPanelEdit.PerformLayout();
            this.ribbonPanelContext.ResumeLayout(false);
            this.ribbonPanel4.ResumeLayout(false);
            this.ribbonPanel9.ResumeLayout(false);
            this.ribbonPanelDisplay.ResumeLayout(false);
            this.ribbonPanel3.ResumeLayout(false);
            this.ribbonPanel6.ResumeLayout(false);
            this.ribbonPanel6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.contextMenuBar1)).EndInit();
            this.dockSite9.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.barDocumentDockBar)).EndInit();
            this.dockSite1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.barLeftDockBar)).EndInit();
            this.barLeftDockBar.ResumeLayout(false);
            this.panelDockContainerLeft.ResumeLayout(false);
            this.dockSite2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.barRightDockBar)).EndInit();
            this.barRightDockBar.ResumeLayout(false);
            this.panelDockContainerRight.ResumeLayout(false);
            this.dockSite8.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.barStatusBar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private ButtonItem buttonImportMesh;
        private ButtonItem buttonImportActor;
        private ButtonItem buttonImportMinimesh;
        private ButtonItem buttonImportBillboard;
        internal RibbonControl ribbonControl;
        private ButtonItem buttonItemSave;
        private RibbonPanel ribbonPanel1;
        private RibbonTabItem ribbonTabItemPrimitives;
        private RibbonBar ribbonBar1;
        private ButtonItem buttonPaste;
        private ButtonItem buttonItem53;
        private ButtonItem buttonItem54;
        private ItemContainer itemContainer1;
        private ButtonItem buttonCut;
        private ButtonItem buttonCopy;
        private ButtonItem buttonItemPaste;
        private RibbonBar ribbonBar8;
        private ItemContainer itemContainer10;
        private CheckBoxItem checkBoxItem1;
        private CheckBoxItem checkBoxItem2;
        private CheckBoxItem checkBoxItem3;
        private ItemContainer itemContainer11;
        private CheckBoxItem checkBoxItem4;
        private CheckBoxItem checkBoxItem5;
        private RibbonBar ribbonBarSearch;
        private ButtonItem buttonFind;
        private ItemContainer itemContainer5;
        private ButtonItem buttonReplace;
        private ButtonItem buttonGoto;
        private RibbonPanel ribbonPanel2;
        private RibbonPanel ribbonPanel3;
        private RibbonTabItem ribbonTabItemTextures;
        private RibbonTabItem ribbonTabItemMaterials;
        private RibbonBar ribbonBar2;
        private ButtonItem buttonItem2;
        private ButtonItem buttonItem3;
        private ButtonItem buttonItem4;
        private CheckBoxItem checkBoxItem6;
        private RibbonPanel ribbonPanel4;
        private RibbonBar ribbonBar3;
        private ButtonItem buttonItemDirectionalLight;
        private ButtonItem buttonItemPointLight;
        private ButtonItem buttonItemSpotLight;
        private RibbonTabItem ribbonTabItemLights;
        private RibbonPanel ribbonPanel5;
        private RibbonBar ribbonBar4;
        private ButtonItem buttonItemStarfield;
        private ButtonItem buttonItemDustField;
        private ButtonItem buttonItemSkybox;
        private RibbonTabItem ribbonTabItemFX;
        private RibbonBar ribbonBar7;
        private ButtonItem buttonItemBluePlanet;
        private ButtonItem buttonItemPlanetoidField;
        private ButtonItem buttonItemStar;
        private ButtonItem buttonItemNewSingleRegion;
        private ButtonItem buttonItemNewMultiRegionScene;
        private ButtonItem buttonItemNewFloorPlan;
        private TreeView treeEntityBrowser;
        private PropertyGrid propertyGrid;
        private ButtonItem buttonImportEditableObjMesh;
        private ButtonItem buttonItemStarFieldNoTexture;
        private RibbonBar ribbonBar9;
        private GalleryContainer galleryContainerTextures;
        private ButtonItem buttonItemOnlineTextureSearch;
        private ButtonItem buttonItemBrowseTextures;
        private ButtonItem buttonItemProceduralTextureGen;
        private ButtonItem buttonItemSaveTexture;
        private ButtonItem buttonItemTexture1;
        private ButtonItem buttonItemTexture2;
        private ButtonItem buttonItemTexture3;
        private ButtonItem buttonItemTexture4;
        private ButtonItem buttonItemTexture5;
        private ButtonItem buttonItemTexture6;
        private RibbonPanel ribbonPanel8;
        private RibbonPanel ribbonPanel6;
        private RibbonPanel ribbonPanel7;
        private RibbonTabItem ribbonTabItemBehaviors;
        private RibbonTabItem ribbonTabItemScripts;
        private RibbonTabItem ribbonTabItemShaders;
        private Label labelEditTabNotesTemp;
        private Label labelScriptsNotesTemp;
        private RibbonPanel ribbonPanel9;
        private RibbonTabItem ribbonTabItemProceduralTexture;
        private RibbonBar ribbonBar10;
        private ButtonItem buttonItem5;
        private ButtonItem buttonItem18;
        private ButtonItem buttonItemGenerateTexture;
        private RibbonBar ribbonBar12;
        private ItemContainer itemContainer8;
        private CheckBoxItem checkBoxItem7;
        private CheckBoxItem checkBoxItem8;
        private CheckBoxItem checkBoxItem9;
        private RibbonBar ribbonBar13;
        private ButtonItem buttonItem20;
        private ButtonItem buttonItem21;
        private ButtonItem buttonItem22;
        private ButtonItem buttonItem23;
        private ButtonItem buttonItem24;
        private ButtonItem buttonItem30;
        private ButtonItem buttonItem31;
        private ButtonItem buttonItem32;
        private ButtonItem buttonItem33;
        private ButtonItem buttonItem34;
        private ButtonItem buttonItem35;
        private ButtonItem buttonItem37;
        private ControlContainerItem controlContainerItem3;
        private ItemContainer itemContainer15;
        private ButtonItem buttonItemBackColor;
        private ControlContainerItem controlContainerItem1;
        private ItemContainer itemContainer7;
        private ButtonItem buttonItem36;
        private ControlContainerItem controlContainerItem2;
        private ItemContainer itemContainer14;
        private RibbonBar ribbonBarViewLayouts;
        private GalleryContainer galleryViewLayouts;
        private ButtonItem buttonItem47;
        private ButtonItem buttonItem48;
        private ButtonItem buttonItem49;
        private ButtonItem buttonItemDisplaySingleViewport;
        private ButtonItem buttonItemDisplayVSplit;
        private ButtonItem buttonItemDisplayHSplit;
        private ButtonItem buttonItemDisplayTripleLeft;
        private ButtonItem buttonItemDisplayTripleRight;
        private ButtonItem buttonItemDisplayQuad;
        private ButtonItem buttonItem38;
        private ButtonItem buttonItem39;
        private RibbonBar ribbonBar15;
        private ItemContainer itemContainer17;
        private ButtonItem buttonPT_Select;
        private ButtonItem buttonPT_ScaleBiasOutput;
        private ButtonItem buttonPT_ScaleOutput;
        private ItemContainer itemContainer16;
        private CheckBoxItem checkBoxItem10;
        private CheckBoxItem checkBoxItem11;
        private CheckBoxItem checkBoxItem12;
        private ItemContainer itemContainer18;
        private ButtonItem buttonPT_FastNoise;
        private ButtonItem buttonPT_FastBillow;
        private ButtonItem buttonPT_FastTurbulence;
        private ItemContainer itemContainer19;
        private ButtonItem buttonPT_Float;
        private ButtonItem buttonPT_Checkerboard;
        private ButtonItem buttonPT_Int;
        private ItemContainer itemContainer20;
        private ButtonItem buttonPT_SphereMap;
        private ButtonItem buttonPT_PlaneMap;
        private ButtonItem buttonPT_CylinderMap;
        private ItemContainer itemContainer21;
        private ButtonItem buttonPT_FastRidgedMF;
        private ButtonItem buttonPT_Turbulence;
        private ButtonItem buttonPT_RidgedMF;
        private ItemContainer itemContainer22;
        private ButtonItem buttonPT_Voronoi;
        private ButtonItem buttonPT_Perlin;
        private ButtonItem buttonPT_Billow;
        private RibbonPanel ribbonPanel10;
        private RibbonBar ribbonBar11;
        private ItemContainer itemContainer23;
        private ButtonItem buttonItemCycleNextShip;
        private ButtonItem buttonItemCyclePrevShip;
        private ButtonItem buttonItem19;
        private ItemContainer itemContainer24;
        private ButtonItem buttonItem25;
        private ButtonItem buttonItem40;
        private ButtonItem buttonItem41;
        private RibbonBar ribbonBar14;
        private ItemContainer itemContainer29;
        private CheckBoxItem checkBoxItem13;
        private CheckBoxItem checkBoxItem14;
        private CheckBoxItem checkBoxItem15;
        private RibbonTabItem ribbonTabItemVehicles;
        private RibbonPanel ribbonPanel11;
        private RibbonTabItem ribbonTabItemPhysics;
        private ButtonItem buttonItemRedo;
        private ButtonItem buttonItemUndo;
        private RibbonBar ribbonBar5;
        private ButtonItem buttonMargins;
        private ButtonItem buttonItem9;
        private ButtonItem buttonItem50;
        private ButtonItem buttonItem51;
        private ButtonItem buttonItem52;
        private ButtonItem buttonItem10;
        private ButtonItem buttonItem11;
        private ButtonItem buttonImportVehicle;
        private ItemContainer itemContainer25;
        private ButtonItem buttonIncreaseVelocity;
        private ButtonItem buttonDecreaseVelocity;
        private ButtonItem buttonStop;
        private RibbonBar ribbonBar16;
        private ItemContainer itemContainer26;
        private ButtonItem buttonSelectTool;
        private ButtonItem buttonMoveTool;
        private ButtonItem buttonRotateTool;
        private ItemContainer itemContainer27;
        private ButtonItem buttonScaleTool;
        private ButtonItem buttonLineTool;
        private ButtonItem buttonRectangleTool;
        private ItemContainer itemContainer28;
        private ButtonItem buttonCircleTool;
        private ButtonItem buttonPolygonTool;
        private ButtonItem buttonThrowProjectile;
        private RibbonBar ribbonBar17;
        private ItemContainer itemContainer30;
        private ButtonItem buttonSpherePrimitive;
        private ButtonItem buttonBoxPrimitive;
        private ButtonItem buttonCylinderPrimitive;
        private ItemContainer itemContainer31;
        private ButtonItem buttonTeapotPrimitive;
        private ButtonItem buttonTorusPrimitive;
        private ButtonItem buttonPyramidPrimitive;
        private ItemContainer itemContainer32;
        private ButtonItem buttonNewStar;
        private ButtonItem buttonNewWorld;
        private ButtonItem buttonNewAsteroidBelt;
        private ItemContainer itemContainer34;
        private ButtonItem buttonNewComet;
        private ButtonItem buttonItem72;
        private ButtonItem buttonItem73;
        private ButtonItem buttonItem70;
        private ButtonItem buttonPhysicsDemos;
        private ButtonItem buttonWallDemo;
        private ButtonItem buttonIncomingDemo;
        private ButtonItem buttonSpheresDemo;
        private ButtonItem buttonCatapultDemo;
        private ButtonItem buttonPendulumDemo;
        private ButtonItem buttonVehicleDemo;
        private ButtonItem buttonPlinkoDemo;
        private ButtonItem buttonSandboxDemo;
        private ItemContainer itemContainer33;
        private ButtonItem buttonItem6;
        private ButtonItem buttonItem7;
        private ButtonItem buttonItem42;
        private ButtonItem buttonItem43;
        private ButtonItem buttonItemGasPlanet;
        private ButtonItem buttonItemMoon;
        private ButtonItem buttonItem44;
        private ButtonItem buttonItemSolSystem;
        private ItemContainer itemContainer35;
        private ButtonItem buttonItem45;
        private ButtonItem buttonItem46;
        private ButtonItem buttonItem55;
        
    }
}