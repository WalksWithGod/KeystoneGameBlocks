using System;
using System.Collections.Generic;
using DevComponents.DotNetBar;
using System.Diagnostics;
using Silver.UI;
using System.Windows.Forms;

namespace KeyEdit.Workspaces
{

    /// <summary>
    /// </summary>
    public class ProceduralTextureWorkspace : Keystone.Workspaces.IWorkspace
    {
        private KeyEdit.Controls.LibNoiseTextureDesigner mDocument;
        private Silver.UI.ToolBox mToolBox;
        private bool mToolBoxInitialized = false;

        protected Keystone.Workspaces.IWorkspaceManager mWorkspaceManager;
        protected string mName;
        protected bool mIsActive;
        protected string mCaption; 

        public ProceduralTextureWorkspace (string name) 
        {
            mName = name;
            InitializeToolbox();
        }



        public virtual void Configure(Keystone.Workspaces.IWorkspaceManager manager, Keystone.Scene.Scene scene)
        {
            if (manager == null) throw new ArgumentNullException("WorkspaceBase.Configre() - No ViewManager set.");
            mWorkspaceManager = manager;

            mDocument = new KeyEdit.Controls.LibNoiseTextureDesigner("Procedural Texture");
            mDocument.AllowDrop = true;
            mDocument.DragDrop += new DragEventHandler(this.dragContainer_DragDrop);
            mDocument.DragOver += new DragEventHandler(this.dragContainer_DragOver);

            mWorkspaceManager.CreateWorkspaceDocumentTab(mDocument, GotFocus);

        }

        public virtual void UnConfigure()
        {
            if (mWorkspaceManager == null) throw new Exception("WorkspaceBase.Unconfigure() - No ViewManager set.");
        }

        #region IWorkspace Members
        public string Name { get { return mName; } }

        public bool IsActive {get {return mIsActive;}}
        
        public Keystone.Workspaces.IWorkspaceManager Manager { get { return mWorkspaceManager; } }

        public Keystone.Controllers.InputControllerBase IOController { get { return null; } set {  } }
        
        public Control Document { get { return mDocument; } }
        
        public Keystone.Collision.PickResults SelectedEntity
        {
            set { throw new Exception(); }
            get { throw new Exception(); }
        }

        public Keystone.Collision.PickResults MouseOverItem
        {
            set { throw new Exception(); }
            get { throw new Exception(); }
        }
        
        public void OnEntityAdded(Keystone.Entities.Entity parent, Keystone.Entities.Entity child)
        {
            throw new NotImplementedException();
        }

        public void OnEntityRemoved(Keystone.Entities.Entity parent, Keystone.Entities.Entity child)
        {
            throw new NotImplementedException();
        }

        public void OnEntitySelected(Keystone.Collision.PickResults pickResults)
        {
            throw new NotImplementedException();
        }

        public void OnEntityDeleteKeyPress ()
        {
        	//throw new NotImplementedException();
        }

        public void OnNodeAdded(string parentID, string childID, string childTypeName)
        {
        }
        public void OnNodeRemoved(string parentID, string childID, string childTypeName)
        {
        }

        public void Resize() { }

        public void Show() 
        {
            if (mToolBoxInitialized == false)
            {
                ((WorkspaceManager)mWorkspaceManager).DockControl(mToolBox, "Procedural Texture Toolbox", "leftDockSiteBar", "Procedural Texture Toolbox", eDockSide.Left);
                mToolBoxInitialized = true;
            }
            mIsActive = true;
        }

        public void Hide() { mIsActive = false; }
        #endregion 


        private void GotFocus(Object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("EditorWorkspace.GotFocus() - ");
        }


        private void InitializeToolbox()
        {
            mToolBox = new Silver.UI.ToolBox();
            mToolBox.AllowDrop = true;
            mToolBox.BackColor = System.Drawing.SystemColors.Control;
            mToolBox.Dock = System.Windows.Forms.DockStyle.Fill;
            mToolBox.ItemHeight = 20;
            mToolBox.ItemHoverColor = System.Drawing.Color.BurlyWood;
            mToolBox.ItemNormalColor = System.Drawing.SystemColors.Control;
            mToolBox.ItemSelectedColor = System.Drawing.Color.Linen;
            mToolBox.ItemSpacing = 1;
            mToolBox.Location = new System.Drawing.Point(0, 0);
            mToolBox.Name = "_toolBox";
            //mToolBox.Size = new System.Drawing.Size(208, 405);
            mToolBox.TabHeight = 18;
            //mToolBox.SetImageList(GetImage("ToolBox_Small.bmp"), new System.Drawing.Size(16, 16), System.Drawing.Color.Magenta, true);
            //mToolBox.SetImageList(GetImage("ToolBox_Large.bmp"), new System.Drawing.Size(32, 32), System.Drawing.Color.Magenta, false);

            //mToolBox.RenameFinished += new RenameFinishedHandler(ToolBox_RenameFinished);
            //mToolBox.TabSelectionChanged += new TabSelectionChangedHandler(ToolBox_TabSelectionChanged);
            //mToolBox.ItemSelectionChanged += new ItemSelectionChangedHandler(ToolBox_ItemSelectionChanged);
            //mToolBox.TabMouseUp += new TabMouseEventHandler(ToolBox_TabMouseUp);
            //mToolBox.ItemMouseUp += new ItemMouseEventHandler(ToolBox_ItemMouseUp);
            //mToolBox.OnDeSerializeObject += new XmlSerializerHandler(ToolBox_OnDeSerializeObject);
            //mToolBox.OnSerializeObject += new XmlSerializerHandler(ToolBox_OnSerializeObject);
            //mToolBox.ItemKeyPress += new ItemKeyPressEventHandler(ToolBox_ItemKeyPress);


            mToolBox.DeleteAllTabs(false);
           
            // entity nodes tab
            int nodesTab = mToolBox.AddTab("Nodes", -1);
            mToolBox[nodesTab].Deletable = false;
            mToolBox[nodesTab].Renamable = false;
            mToolBox[nodesTab].Movable = false;


            mToolBox[nodesTab].View = Silver.UI.ViewMode.List;
            mToolBox[nodesTab].AddItem("Sphere Mapper", 0, 0, true, 0);
            mToolBox[nodesTab].AddItem("Plane Mapper", 0, 0, true, 1);
            mToolBox[nodesTab].AddItem("Cylinder Mapper", 0, 0, true, 2);

            mToolBox[nodesTab].AddItem("Select", 1, 1, true, 3);
            mToolBox[nodesTab].AddItem("Scale Output", 2, 2, true, 4);
            mToolBox[nodesTab].AddItem("Scale Bias Output", 2, 2, true, 5);
            mToolBox[nodesTab].AddItem("Fast Noise", 2, 2, true, 6);
            mToolBox[nodesTab].AddItem("Fast Billow", 2, 2, true, 7);
            mToolBox[nodesTab].AddItem("Fast Turbulence", 2, 2, true, 8);
            mToolBox[nodesTab].AddItem("Fast Ridged Multifractal", 2, 2, true, 9);

            mToolBox[nodesTab].AddItem("Voronoi", 2, 2, true, 10);
            mToolBox[nodesTab].AddItem("Billow", 2, 2, true, 11);
            mToolBox[nodesTab].AddItem("Turbulence", 2, 2, true, 12);
            mToolBox[nodesTab].AddItem("Ridged Multifractal", 2, 2, true, 13);
            mToolBox[nodesTab].AddItem("Perlin", 2, 2, true, 14);
            mToolBox[nodesTab].AddItem("Checkerboard", 2, 2, true, 15);


            // value parameter tab
            int valueTabs = mToolBox.AddTab("Values", -1);
            mToolBox[valueTabs].Deletable = false;
            mToolBox[valueTabs].Renamable = false;
            mToolBox[valueTabs].Movable = false;

            mToolBox[valueTabs].View = Silver.UI.ViewMode.List;
            mToolBox[valueTabs].AddItem("float", 0, 0, true, 16);
            mToolBox[valueTabs].AddItem("integer", 1, 1, true, 17);


        }

        #region Drag Drop from Toolbox to Mindfusion Chart 
        //private void itemAllowDrag_CheckedChanged(object sender, System.EventArgs e)
        //{
        //    ToolBoxTab tab = mToolBox[(int)itemTabIndex.Value];

        //    try
        //    {
        //        if (null != tab)
        //        {
        //            tab[(int)itemIndex.Value].AllowDrag = itemAllowDrag.Checked;
        //        }
        //    }
        //    catch (Exception)
        //    {
        //    }
        //}

        private void dragContainer_DragOver(object sender, System.Windows.Forms.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Silver.UI.ToolBoxItem)))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void dragContainer_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
        {
            ToolBoxItem dragItem = null;
            string strItem = "";

            if (e.Data.GetDataPresent(typeof(Silver.UI.ToolBoxItem)))
            {
                dragItem = e.Data.GetData(typeof(Silver.UI.ToolBoxItem)) as ToolBoxItem;

                if (null != dragItem && null != dragItem.Object)
                {
                    strItem = dragItem.Object.ToString();
                    MessageBox.Show(strItem, "Drag Drop");

                    //toolWindow.ToolBox.Focus();
                }
            }
        }
        #endregion

        #region ProceduralTexture Buttons
        internal void toolboxButton_Click(object sender, EventArgs e)
        {
            if (sender is System.Windows.Forms.Button)
            {
                System.Windows.Forms.Button button = (System.Windows.Forms.Button)sender;
                switch (button.Text)
                {
                    case "Sphere Mapper":
                        break;
                }
            }
        }

        
        private void buttonItemGenerateTexture_Click(object sender, EventArgs e)
        {


            if (mDocument != null)
                mDocument.Generate();
        }

        // note:  blind's screenshot
        // http://www.smithbower.com/old/pics/sc_08.png
        private void buttonItemSphereMap_Click(object sender, EventArgs e)
        {
            mDocument.CreateEntityNode("Sphere Mapper", "Spherical texture mapping algorithm.",
            new string[] { "Module", "Palette", "MinX", "MinY", "MaxX", "MaxY", "Width", "Height" },
            new string[] { "module", "palette", "int", "int", "int", "int", "int", "int" },
            new bool[] { true, false, false, false, false, false, false, false },
            new string[] { "out" },
            new string[] { "texture" });
        }

        private void buttonPT_PlaneMap_Click(object sender, EventArgs e)
        {

        }

        private void buttonPT_CylinderMap_Click(object sender, EventArgs e)
        {

        }

        private void buttonPT_Select_Click(object sender, EventArgs e)
        {
            mDocument.CreateEntityNode("Select", "Select.",
            new string[] { "ControlModule", "SourceModule1", "SourceModule2", "LowerBound", "UpperBound", "EdgeFalloff" },
            new string[] { "module", "module", "module", "float", "float", "float" },
            new bool[] { true, true, true, false, false, false },
            new string[] { "out" },
            new string[] { "module" });
        }

        private void buttonPT_ScaleOutput_Click(object sender, EventArgs e)
        {
            mDocument.CreateEntityNode("Scale Output", "Scale output.",
            new string[] { "Module", "Scale" },
            new string[] { "module", "float" },
            new bool[] { true, false },
            new string[] { "out" },
            new string[] { "module" });
        }

        private void buttonItemScaleBiasOutput_Click(object sender, EventArgs e)
        {
            mDocument.CreateEntityNode("Scale Bias Output", "Scale bias output.",
            new string[] { "Module", "Scale", "Bias" },
            new string[] { "module", "float", "float" },
            new bool[] { true, false, false },
            new string[] { "out" },
            new string[] { "module" });
        }


        // Integer
        private void buttonItemInt_Click(object sender, EventArgs e)
        {
            mDocument.CreateValueTypeNode("Integer", "Positive whole numbers starting at 0.",


            new string[] { "value" },
            new string[] { "2" },
            new string[] { "int" });

            KeyEdit.Controls.LibNoiseTextureDesigner.Parameter p = new KeyEdit.Controls.LibNoiseTextureDesigner.Parameter("value", "int", false);
            p.Value = "1";
            p.EditInPlace = true;
            p.AssignmentRequired = false;

            mDocument.CreateValueTypeNode("Integer", "Positive whole numbers starting at 0.",
                 new KeyEdit.Controls.LibNoiseTextureDesigner.Parameter[] { p });
        }

        // Float
        private void buttonItemFloat_Click(object sender, EventArgs e)
        {
            KeyEdit.Controls.LibNoiseTextureDesigner.Parameter p = new KeyEdit.Controls.LibNoiseTextureDesigner.Parameter("value", "float", false);
            p.Value = "1.0";
            p.EditInPlace = true;
            p.AssignmentRequired = false;

            mDocument.CreateValueTypeNode("Float", "Floating point numbers greater or equal to 0.0",
                 new KeyEdit.Controls.LibNoiseTextureDesigner.Parameter[] { p });
        }

        private void buttonPT_Perlin_Click(object sender, EventArgs e)
        {
            CreateIPersistenceNoiseEntity("Perlin", "Noise using the original perlin noise algorithm.");
        }

        private void buttonPT_FastBillow_Click(object sender, EventArgs e)
        {
            CreateIPersistenceNoiseEntity("Fast Billow", "Billow using the fast billow algorithm.");
        }

        private void buttonPT_Billow_Click(object sender, EventArgs e)
        {
            CreateIPersistenceNoiseEntity("Billow", "Billow using the original billow algorithm.");
        }

        private void buttonPT_FastNoise_Click(object sender, EventArgs e)
        {
            CreateIPersistenceNoiseEntity("Fast Noise", "Noise using the fast perlin noise algorithm.");
        }

        private void CreateIPersistenceNoiseEntity(string name, string description)
        {
            mDocument.CreateEntityNode(name, description,
                       new string[] { "Seed", "Frequency", "Lacunarity", "OctaveCount", "Persistence", "NoiseQuality" },
                       new string[] { "int", "float", "float", "int", "float", "noisequality" },
                       new bool[] { false, false, false, false, false, false },
                       new string[] { "out" },
                       new string[] { "module" });
        }

        private void buttonPT_FastRidgedMF_Click(object sender, EventArgs e)
        {
            CreateINoiseEntity("Fast Ridged Multifractal", "Ridged multifractal using the fast algorithm.");
        }

        private void buttonPT_RidgedMF_Click(object sender, EventArgs e)
        {
            CreateINoiseEntity("Ridged Multifractal", "Ridged multifractal using the original algorithm.");
        }

        private void CreateINoiseEntity(string name, string description)
        {
            mDocument.CreateEntityNode(name, description,
                       new string[] { "Seed", "Frequency", "Lacunarity", "OctaveCount", "NoiseQuality" },
                       new string[] { "int", "float", "float", "int", "noisequality" },
                       new bool[] { false, false, false, false, false },
                       new string[] { "out" },
                       new string[] { "module" });
        }


        private void buttonPT_FastTurbulence_Click(object sender, EventArgs e)
        {
            CreateITurublence("Fast Turbulence", "Fast turbulence algorithm.");
        }

        private void buttonPT_Turbulence_Click(object sender, EventArgs e)
        {
            CreateITurublence("Turbulence", "Original turbulence algorithm");
        }

        private void CreateITurublence(string name, string description)
        {
            mDocument.CreateEntityNode(name, description,
                       new string[] { "Module", "Seed", "Frequency", "Power", "Roughness" },
                       new string[] { "module", "int", "float", "float", "int" },
                       new bool[] { true, false, false, false, false },
                       new string[] { "out" },
                       new string[] { "module" });
        }


        private void buttonPT_Voronoi_Click(object sender, EventArgs e)
        {

        }


        private void buttonPT_Checkerboard_Click(object sender, EventArgs e)
        {

        }
        #endregion


    }
}
