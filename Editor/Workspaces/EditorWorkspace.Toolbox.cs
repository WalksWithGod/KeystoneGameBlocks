using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Keystone.Appearance;
using Keystone.Elements;
using Keystone.Entities;
using Keystone.Resource;
using Keystone.Types;
using MTV3D65;
using Silver.UI;

namespace KeyEdit.Workspaces
{
    public partial class EditorWorkspace
    {
        #region constants
        // Regions / Zones
        private const int TOOLBOX_ID_ZONE = 0;
        	
        // lights
        private const int TOOLBOX_ID_DIRLIGHT = 1;
        private const int TOOLBOX_ID_POINTLIGHT = 2;
        private const int TOOLBOX_ID_SPOTLIGHT = 3;

        // sky
        private const int TOOLBOX_ID_DYNAMIC_DAYNIGHT = 4;
        
        // terrain
        private const int TOOLBOX_ID_TERRAIN = 5; 
        private const int TOOLBOX_ID_TERRAIN_MESH = 6;
        private const int TOOLBOX_ID_TERRAIN_RAISE = 7;
        private const int TOOLBOX_ID_TERRAIN_LOWER = 8;
        private const int TOOLBOX_ID_WATER_MESH = 9;
        
        // structure
        private const int TOOLBOX_ID_WALL_PLACE = 10;
        	
        
        // ai
        private const int TOOLBOX_ID_NAVPOINT = 11;

        // primitives
        private const int TOOLBOX_ID_FLOOR = 12;
        private const int TOOLBOX_ID_BILLBOARD = 13;
        private const int TOOLBOX_ID_SPHERE = 14;
        private const int TOOLBOX_ID_BOX = 15;
        private const int TOOLBOX_ID_CYLINDER = 16;
        private const int TOOLBOX_ID_CONE = 17;
        private const int TOOLBOX_ID_CAPSULE = 18;
        private const int TOOLBOX_ID_TORUS = 19;
        private const int TOOLBOX_ID_PYRAMID_3 = 20;
        private const int TOOLBOX_ID_PYRAMID_4 = 21;
        private const int TOOLBOX_ID_TEAPOT = 22;
        private const int TOOLBOX_ID_CIRCLE = 34;

        // celestial
        private const int TOOLBOX_ID_MOTIONFIELD = 23;
        private const int TOOLBOX_ID_STARFIELD = 24;
        private const int TOOLBOX_ID_NEBULA = 25;
        private const int TOOLBOX_ID_PLANETOIDFIELD = 26;

        private const int TOOLBOX_ID_STAR = 27;
        private const int TOOLBOX_ID_GASGIANT = 28;
        private const int TOOLBOX_ID_TERRESTIALPLANET = 29;
        private const int TOOLBOX_ID_MOON = 30;
        private const int TOOLBOX_ID_SOLSYSTEM = 31;

        private const int TOOLBOX_ID_EXPLOSION = 32;
        private const int TOOLBOX_ID_THRUSTER = 33;
        
        private const int TOOLBOX_ID_LASER = 35;
        private const int TOOL_ID_PARTICLESYSTEM = 36;


        // hardpoints
        private const int TOOLBOX_ID_HARDPOINT = 37;

        // Game Objects and Server Objects - GameObjects are never serialized to the main scene.  They are only loaded from Mission object instances.  And ServerObjects are only added to Scene.ServerObjects not Scene.ActiveEntities.  Also they are only updated by server (eg. Simulation.UpdateLoopback() and not the general Simulation.Update())
        private const int TOOLBOX_ID_SPAWNPOINT = 38;
        private const int TOOLBOX_ID_TRIGGER_VOLUME = 39;
        #endregion

        private void InitializeToolBox()
        {
            mToolBox = new Silver.UI.ToolBox();
            mToolBox.AllowDrop = false; // can drag items off, but can't drop things onto
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
            //            mToolBox.ItemMouseUp += new ItemMouseEventHandler(ToolBox_ItemMouseUp);
            //mToolBox.OnDeSerializeObject += new XmlSerializerHandler(ToolBox_OnDeSerializeObject);
            //mToolBox.OnSerializeObject += new XmlSerializerHandler(ToolBox_OnSerializeObject);
            //mToolBox.ItemKeyPress += new ItemKeyPressEventHandler(ToolBox_ItemKeyPress);

            mToolBox.ItemMouseDown += ToolboxItem_Clicked;
            
            mToolBox.DeleteAllTabs(false);
            bool allowDrag = true;

            // Lights tab
            int lightsTab = mToolBox.AddTab("Lights", -1);
            mToolBox[lightsTab].Deletable = false;
            mToolBox[lightsTab].Renamable = false;
            mToolBox[lightsTab].Movable = false;

            mToolBox[lightsTab].View = Silver.UI.ViewMode.List;
            mToolBox[lightsTab].AddItem("Directional Light", 0, 0, allowDrag, TOOLBOX_ID_DIRLIGHT);
            mToolBox[lightsTab].AddItem("Point Light", 0, 0, allowDrag, TOOLBOX_ID_POINTLIGHT);
            mToolBox[lightsTab].AddItem("Spot Light", 0, 0, allowDrag, TOOLBOX_ID_SPOTLIGHT);

            // Hardpoints tab
            int hardpointsTabs = mToolBox.AddTab("Hardpoints", -1);
            mToolBox[hardpointsTabs].Deletable = false;
            mToolBox[hardpointsTabs].Renamable = false;
            mToolBox[hardpointsTabs].Movable = false;

            mToolBox[hardpointsTabs].View = Silver.UI.ViewMode.List;
            mToolBox[hardpointsTabs].AddItem("hardpoint", 0, 0, true, TOOLBOX_ID_HARDPOINT);

            // Sky tab
            int skyTab = mToolBox.AddTab("Sky", -1);
            mToolBox[skyTab].Deletable = false;
            mToolBox[skyTab].Renamable = false;
            mToolBox[skyTab].Movable = false;

            mToolBox[skyTab].View = Silver.UI.ViewMode.List;
            mToolBox[skyTab].AddItem("Dynamic Day & Night", 0, 0, allowDrag, TOOLBOX_ID_DYNAMIC_DAYNIGHT);
            
            // Landscape & Water tab
            int terrainTab = mToolBox.AddTab("Landscape", -1);
            mToolBox[terrainTab].Deletable = false;
            mToolBox[terrainTab].Renamable = false;
            mToolBox[terrainTab].Movable = false;

            mToolBox[terrainTab].View = Silver.UI.ViewMode.List;
            mToolBox[terrainTab].AddItem("Terrain & Water", 0, 0, allowDrag, TOOLBOX_ID_TERRAIN);
            mToolBox[terrainTab].AddItem("Water Patch", 0, 0, allowDrag, TOOLBOX_ID_WATER_MESH);
            mToolBox[terrainTab].AddItem("Terrain (Mesh)", 0, 0, allowDrag, TOOLBOX_ID_TERRAIN_MESH);
            mToolBox[terrainTab].AddItem("Terrain - Raise", 0, 0, allowDrag, TOOLBOX_ID_TERRAIN_RAISE);
            mToolBox[terrainTab].AddItem("Terrain - Lower", 0, 0, allowDrag, TOOLBOX_ID_TERRAIN_LOWER);
            
            // structure - walls, floors
            mToolBox[terrainTab].View = Silver.UI.ViewMode.List;
            mToolBox[terrainTab].AddItem("Wall Place", 0, 0, allowDrag, TOOLBOX_ID_WALL_PLACE);

            // mission objects - spawnpoints, trigger volumes
            int gameObjectsTab = mToolBox.AddTab("Mission Objects", -1);
            mToolBox[gameObjectsTab].Deletable = false;
            mToolBox[gameObjectsTab].Renamable = false;
            mToolBox[gameObjectsTab].Movable = false;

            mToolBox[gameObjectsTab].View = Silver.UI.ViewMode.List;
            mToolBox[gameObjectsTab].AddItem("Spawn Point", 0, 0, true, TOOLBOX_ID_SPAWNPOINT);


            // AI tab
            int aiTabs = mToolBox.AddTab("AI", -1);
            mToolBox[aiTabs].Deletable = false;
            mToolBox[aiTabs].Renamable = false;
            mToolBox[aiTabs].Movable = false;

            mToolBox[aiTabs].View = Silver.UI.ViewMode.List;
            mToolBox[aiTabs].AddItem("Nav Point", 0, 0, true, TOOLBOX_ID_NAVPOINT);

            // Primitves Tab
            int primitivesTabs = mToolBox.AddTab("Primitives", -1);
            mToolBox[primitivesTabs].Deletable = false;
            mToolBox[primitivesTabs].Renamable = false;
            mToolBox[primitivesTabs].Movable = false;

            mToolBox[primitivesTabs].View = Silver.UI.ViewMode.List;
            mToolBox[primitivesTabs].AddItem("floor", 1, 1, allowDrag, TOOLBOX_ID_FLOOR);
            mToolBox[primitivesTabs].AddItem("billboard", 1, 1, allowDrag, TOOLBOX_ID_BILLBOARD);
            mToolBox[primitivesTabs].AddItem("billboard laser", 1, 1, allowDrag, TOOLBOX_ID_LASER);
            mToolBox[primitivesTabs].AddItem("sphere", 0, 0, allowDrag, TOOLBOX_ID_SPHERE);
            mToolBox[primitivesTabs].AddItem("box", 1, 1, allowDrag, TOOLBOX_ID_BOX);
            mToolBox[primitivesTabs].AddItem("teapot", 1, 1, allowDrag, TOOLBOX_ID_TEAPOT);
            mToolBox[primitivesTabs].AddItem("cylinder", 1, 1, allowDrag, TOOLBOX_ID_CYLINDER);
            mToolBox[primitivesTabs].AddItem("cone", 1, 1, allowDrag, TOOLBOX_ID_CONE);
            mToolBox[primitivesTabs].AddItem("capsule", 1, 1, allowDrag, TOOLBOX_ID_CAPSULE);
            mToolBox[primitivesTabs].AddItem("torus", 1, 1, allowDrag, TOOLBOX_ID_TORUS);
            mToolBox[primitivesTabs].AddItem("pyramid (3)", 1, 1, allowDrag, TOOLBOX_ID_PYRAMID_3);
            mToolBox[primitivesTabs].AddItem("pyramid (4)", 1, 1, allowDrag, TOOLBOX_ID_PYRAMID_4);
			mToolBox[primitivesTabs].AddItem("circle", 1, 1, allowDrag, TOOLBOX_ID_CIRCLE);

            // Celestials
            int celestialsTab = mToolBox.AddTab("Celestials", -1);

            mToolBox[celestialsTab].Deletable = false;
            mToolBox[celestialsTab].Renamable = false;
            mToolBox[celestialsTab].Movable = false;

            mToolBox[celestialsTab].View = Silver.UI.ViewMode.List;
            mToolBox[celestialsTab].AddItem("SkyBox", 0, 0, allowDrag, 13);
            mToolBox[celestialsTab].AddItem("SkySphere", 0, 0, allowDrag, 14);
            mToolBox[celestialsTab].AddItem("Starfield", 0, 0, allowDrag, TOOLBOX_ID_STARFIELD);
            mToolBox[celestialsTab].AddItem("Motion field", 0, 0, allowDrag, TOOLBOX_ID_MOTIONFIELD);
            mToolBox[celestialsTab].AddItem("Nebula", 0, 0, allowDrag, TOOLBOX_ID_NEBULA);
            mToolBox[celestialsTab].AddItem("Asteroid Field", 0, 0, allowDrag, TOOLBOX_ID_PLANETOIDFIELD);
            mToolBox[celestialsTab].AddItem("Comet", 0, 0, allowDrag, 23);
            mToolBox[celestialsTab].AddItem("Star", 0, 0, allowDrag, TOOLBOX_ID_STAR);
            mToolBox[celestialsTab].AddItem("Gas Giant", 0, 0, allowDrag, TOOLBOX_ID_GASGIANT);
            mToolBox[celestialsTab].AddItem("Terrestial Planet", 0, 0, allowDrag, TOOLBOX_ID_TERRESTIALPLANET);
            mToolBox[celestialsTab].AddItem("Moon", 0, 0, allowDrag, TOOLBOX_ID_MOON);
            mToolBox[celestialsTab].AddItem("Sol System", 0, 0, allowDrag, TOOLBOX_ID_SOLSYSTEM);

            mToolBox[celestialsTab].AddItem("Explosion", 0, 0, allowDrag, TOOLBOX_ID_EXPLOSION);
            mToolBox[celestialsTab].AddItem("Particle System", 0, 0, allowDrag, TOOL_ID_PARTICLESYSTEM);
            mToolBox[celestialsTab].AddItem("Thruster", 0, 0, allowDrag, TOOLBOX_ID_THRUSTER);
        }
        
        private void ToolboxItem_Clicked(object sender, EventArgs e) 
        {
        	if (sender is ToolBoxItem == false) return;
        	if (((ToolBoxItem)sender).Object is int == false) 
            {
            	System.Diagnostics.Debug.WriteLine ("EditorWorkspace.Toolbox.ToolboxItem_Clicked() - ERROR: itemID has unexpected type.");
            	return;
            }
        	
        	int itemID = (int)((ToolBoxItem)sender).Object;
        	switch (itemID)
            {
                //            	// Regions   <-- TODO: maybe better as an Asset prefab so i can place with tool? In meantime i can position with plugin position property panel
                //            	case TOOLBOX_ID_ZONE:
                //	            	AddZone (region, position);
                //            		break;
                //            		
                //                // lights
                //                case TOOLBOX_ID_DIRLIGHT:
                //                    AddDirectionalLight(region);
                //                    break;
                //                case TOOLBOX_ID_POINTLIGHT:
                //                    // TODO: for pointlight, we want a mouse pick location not context.Position
                //                    AddPointLight(region, position);
                //                    break;
                //                case TOOLBOX_ID_SPOTLIGHT:
                //                    AddSpotLight(region, position);
                //                    break;

                // hardpoints
                case TOOLBOX_ID_HARDPOINT:
                    {
                        // todo: how do we save the hardpoints?  Is it part of the vehicle prefab?  I think it should be and same goes for the Entities mounted on the hardpoints.
                        // lets take this in steps.  click the object, and we create a new hardpoint from a hardpoint prefab with script, and have it follow the mouse and show red when invalid placement, green for valid placement.
                        AddHardpoint();
                        break;
                    }

                // Sky
                case TOOLBOX_ID_DYNAMIC_DAYNIGHT:
                {   
                	Keystone.Portals.Root root = AppMain._core.SceneManager.Scenes[0].Root;
        			double worldDiameter = AppMain.REGION_DIAMETER * AppMain.REGIONS_ACROSS;
        			//AddDynamicDayNightCycle(root, worldDiameter);
        			
        			Keystone.Celestial.SkyHelper.AddGradientSky (root, worldDiameter, AppMain.FARPLANE);
                	break;
                }
                // Water
                case TOOLBOX_ID_WATER_MESH:
                	AddWaterPatch2();
                	//System.Diagnostics.Debug.WriteLine ("EditorWorkspace.Toolbox.ToolboxItem_Dropped() - Adding non-reflective water patch to " + region.ID + " at " + position.ToString());
                	break;
                	
                // Auto-Tiling Terrain Blocks 
                case TOOLBOX_ID_TERRAIN_RAISE:
                	TerrainRaise();
                	break;
                case TOOLBOX_ID_TERRAIN_LOWER:
                	TerrainLower();
                	break;    
                case TOOLBOX_ID_WALL_PLACE:
                	AddWallToStructure ();
            		break;
                	
//                // TVLandscape based Terrain
//                // TODO: move to Asset Prefab so i can place with tool, or can we get the Tool to spawn here for Toolbox items as well? Toolbox is kind of redundant, but its easier to navigate
//                case TOOLBOX_ID_TERRAIN :
//                	// for terrain, if region is a Zone, we are going to
//                	// compute a new position that is the center of the region
//                	// at x,z and bottom of region at y
//                	if (region is Keystone.Portals.Zone)
//                	{
//                		position = region.SceneNode.BoundingBox.Center; 
//                		position.y -= region.SceneNode.BoundingBox.Height /2d;
//                		System.Diagnostics.Debug.WriteLine ("EditorWorkspace.Toolbox.ToolboxItem_Dropped() - Adding TVLandscape terrain to " + region.ID + " at " + position.ToString());
//                	}
//                   AddTerrain(region, position);
//                   break;
//                   
//                // TVMesh based Terrain
//                case TOOLBOX_ID_TERRAIN_MESH :
//                   	// for terrain, if region is a Zone, we are going to
//                	// compute a new position that is the center of the region
//                	// at x,z and bottom of region at y
//                	
//                	
//                	// http://www.zephyrosanemos.com/#heading3
//                	// - above is a great site that shows a terrain implementation.
//                	//   he also advises that adding a skirt to hide seams between terrains is the best way to go.
//                	//   - its faster in practice and doesnt require terrain to verts to be modified. You are litterally just
//                	//     inserting a skirt.
//                	// TODO: we need visual aids for mouse picking a terrain placement.. ideally snapping
//                	//       asset placement style.  we can create the asset on the fly and prefab it...
//                	//       as we do for visual aids like wall placement stick.  and picking we need
//                	//       to pick first zone, not deepest?
//                	//       visual aid i should be able to add a colored grid patch even over the mouse zone
//                	//       or even a textured box or transparent box + the grid patch where it would fit in there...etc
//                	
//                	region = context.Region ;
//                	if (region is Keystone.Portals.Zone)
//                	{
//                		// TODO: can we launch a tool here instead? 
//                		//       and then, how do we deal with mesh terrain? and stitching them and LOD and heightmap editing
//                		position = region.SceneNode.BoundingBox.Center; 
//                		position.y -= region.SceneNode.BoundingBox.Height /2d;
//                		System.Diagnostics.Debug.WriteLine ("EditorWorkspace.Toolbox.ToolboxItem_Dropped() - Adding TVMesh terrain to " + region.ID + " at " + position.ToString());
//                	}
//                   //AddTerrainMesh(region, position);
//                   //AddTerrainSegment (region, position);
//                    
//                   AddTileStructureToRegion (region, position.y);
//                   break;
//                   
//                // AI
//                case TOOLBOX_ID_NAVPOINT :
//                	AddWaypoint(region, position);
//                    break;
//
//                // primitives
//                case TOOLBOX_ID_FLOOR:
//        			AddFloor (region, position, 100, 100);
//                	break;
//                case TOOLBOX_ID_BILLBOARD:
//                    AddBillboard(region, position);
//                    break;
                case TOOLBOX_ID_SPHERE:
                    {
                    	bool useInstancing = false;
                    	AddSphere(useInstancing);
                    	break;
                    }
                case TOOLBOX_ID_CYLINDER:
                    {
                        bool useInstancing = false;
                        AddCylinder(useInstancing);
                        break;
                    }
                case TOOLBOX_ID_BOX:
                    {
                    	bool useInstancing = false;
                    	AddBox(useInstancing);
//        	            for (int i = 0; i < 25; i++)
//			            {
//        	            	VariableVector3d itemPosition = new VariableVector3d  ();
//        	            	itemPosition.Value = position;
//        	            	itemPosition.Variation =  50;
//        	            	        	            	
//        	            	AddBox (itemPosition.Sample(), useInstancing);
//			            }
                    }
                    break;
                case TOOLBOX_ID_TEAPOT:
                {
                    	bool useInstancing = false;
                    	AddTeapot(useInstancing);
//        	            for (int i = 0; i < 25; i++)
//			            {
//        	            	VariableVector3d itemPosition = new VariableVector3d  ();
//        	            	itemPosition.Value = position;
//        	            	itemPosition.Variation =  50;
//        	            	AddTeapot (region, itemPosition.Sample(), useInstancing);
//			            }
                    }
                    break;
//                case TOOLBOX_ID_CYLINDER:
//                    AddCylinder(region, position);
//                    break;
//                case TOOLBOX_ID_CONE:
//                    AddCone(region, position);
//                    break;
//                case TOOLBOX_ID_CAPSULE:
//                    AddCapsule(region, position);
//                    break;
//                case TOOLBOX_ID_TORUS:
//                    AddTorus(region, position);
//                    break;
//                case TOOLBOX_ID_PYRAMID_3:
//                    AddPyramid3(region, position);
//                    break;
//                case TOOLBOX_ID_PYRAMID_4:
//                    AddPyramid4(region, position);
//                    break;

//
//                // celestial
//                case TOOLBOX_ID_STARFIELD:
//                    AddStarfield(context.Scene.Root, context.Position);
//                    break;
//                case TOOLBOX_ID_PLANETOIDFIELD:
//                    AddPlanetoidField(region, position);
//                    break;
//                case TOOLBOX_ID_NEBULA :
//                    AddNebula(region, position);
//                    break;
//
//                // celestial - stars & planets
//                case TOOLBOX_ID_SOLSYSTEM:
//                    AddSolSystem(region, position);
//                    break;
//                case TOOLBOX_ID_STAR :
//                    AddStar(region, position);
//                    break;
//                case TOOLBOX_ID_GASGIANT:
//                    AddGasPlanet(region, position);
//                    break;
//                case TOOLBOX_ID_TERRESTIALPLANET:
//                    AddTerrestialPlanet(region, position);
//                    break;
//                case TOOLBOX_ID_MOON:                    
//                    AddMoon(region, position);
//                    break;
//                case TOOLBOX_ID_EXPLOSION :
//                    AddExplosion(region, position);
//                    break;
//                case TOOLBOX_ID_THRUSTER:
//                    AddThruster(region, position);
//                    break;
                default:
                    break;
            }
        }

    #region Drag Drop from Toolbox to Workspace (eg 3D Viewport, Mindfusion Chart, etc)
        private void dragContainer_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
        {
        }
        
        private void dragContainer_DragLeave(object sender, System.EventArgs e)
        {
        }
        
        private void dragContainer_QueryContinueDrag(object sender, System.Windows.Forms.QueryContinueDragEventArgs  e)
        {
        }
        
        	
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
                    ToolboxItem_Dropped((KeyEdit.Controls.ViewportControl)sender, dragItem.Object, e.X, e.Y);

                    //mToolBox.Focus();
                }
            }
        }
        #endregion

        #region hardpoints
        private void AddHardpoint()
        {
            // todo: in the toolbox add "single" enging emount and "dual" engine mount
            // this assigns the hardpoint to a transform tool.  
            ModeledEntity entity = new ModeledEntity(Repository.GetNewName(typeof(ModeledEntity)));
            Model model = new Model(Repository.GetNewName(typeof(Model)));

            Mesh3d mesh = Mesh3d.CreateSphere(2.5f, 25, 25, false);

            Keystone.Appearance.Appearance appearance = Keystone.Celestial.ProceduralHelper.CreateAppearance(MTV3D65.CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED, null, null, null, null, null);
            Material material = Material.Create(Material.DefaultMaterials.green);
            appearance.RemoveMaterial();
            appearance.AddChild(material);


            model.AddChild(appearance);
            model.AddChild(mesh);

            entity.AddChild(model);
            entity.Name = "hardpoint";
            entity.SetEntityAttributesValue((uint)KeyCommon.Flags.EntityAttributes.Hardpoint, true);

            ActivateBrush(entity,
                Keystone.EditTools.PlacementTool.BRUSH_SINGLE_DROP,
               KeyCommon.Flags.EntityAttributes.None,
               KeyCommon.Flags.EntityAttributes.Background | KeyCommon.Flags.EntityAttributes.Light,
               false);
        }
        #endregion

        private void ToolboxItem_Dropped(KeyEdit.Controls.ViewportControl ctl, object itemID, int mouseX, int mouseY)
        {
            if (itemID is int == false) 
            {
            	System.Diagnostics.Debug.WriteLine ("EditorWorkspace.Toolbox.TooboxItem_Dropped() - ERROR: itemID has unexpected type.");
            	return;
            }

            // get the viewport the mouse is over
            Keystone.Cameras.RenderingContext context = ctl.Viewport.Context;
            if (context == null) 
            {
            	System.Diagnostics.Debug.WriteLine ("EditorWorkspace.Toolbox.TooboxItem_Dropped() - ERROR: context is NULL.");
            	return;
            }
            Keystone.Events.MouseEventArgs args = context.Viewport.GetMouseHitInfo (mouseX, mouseY);
                        
            KeyCommon.Traversal.PickParameters pickParameters = context.PickParameters;
            pickParameters.FloorLevel = int.MinValue; // search all floors 
            pickParameters.Accuracy |= KeyCommon.Traversal.PickAccuracy.Tile;  // Tile accuracy will ensure we hit a Structure
            // SearchType = KeyCommon.Traversal.PickCriteria.DeepestNested,
            pickParameters.IgnoredTypes = KeyCommon.Flags.EntityAttributes.None;
            pickParameters.MouseX = args.ViewportRelativePosition.X;
            pickParameters.MouseY = args.ViewportRelativePosition.Y;
            
            #if DEBUG
            pickParameters.DebugOutput = false;
            #endif
            
            System.Diagnostics.Debug.WriteLine (""); // empty spacer to make it easier to read debug log
            System.Diagnostics.Debug.WriteLine ("EditorWorkspace.Toolbox.TooboxItem_Dropped() - MouseX = " + args.ViewportRelativePosition.X + " MouseY = " + args.ViewportRelativePosition.Y );

            // TODO: i wonder if perhaps there is a re-entrancy issue here where our normal Tool mouse picking handler
            //       is interfering with this one. An easy test is to just synclock() the pick traverser itself
            Keystone.Collision.PickResults result =  context.Pick (args.ViewportRelativePosition.X, args.ViewportRelativePosition.Y, pickParameters);
            

            if (result.Entity == null) 
            {
            	System.Diagnostics.Debug.Assert (result.HasCollided == false, "EditorWorkspace.Toolbox.ToolboxItem_Dropped() - No ENTITY picked");
            	System.Diagnostics.Debug.WriteLine ("EditorWorkspace.Toolbox.ToolboxItem_Dropped() - No ENTITY picked. Aborting ToolboxItem_Dropped()");
            	return;
            }

            Keystone.Portals.Region region = null;
            if (result.Entity as Keystone.Portals.Region != null) 
            	region = (Keystone.Portals.Region)result.Entity;
            else 
            	region = result.Entity.Region;
            
          	System.Diagnostics.Debug.WriteLine ("EditorWorkspace.Toolbox.ToolboxItem_Dropped() - '" + result.Entity.TypeName.ToUpper() + "' picked in Region '" + region.ID + "'.");
            

            if (region == null)
            {
            	// should be impossible as it would mean the node was disconnected from the scene
            	System.Diagnostics.Debug.WriteLine ("EditorWorkspace.Toolbox.ToolboxItem_Dropped() - NO REGION?!. Aborting ToolboxItem_Dropped()");
            	return;
            }
            
            // TODO: position should depend on impactPoint in most cases... and in cases like DirLight, should have none
            Vector3d position = context.Position;
            
            int id = (int)itemID;

            switch (id)
            {
            	// Regions   <-- TODO: maybe better as an Asset prefab so i can place with tool? In meantime i can position with plugin position property panel
            	case TOOLBOX_ID_ZONE:
	            	AddZone (region, position);
            		break;
            		
                // lights
                case TOOLBOX_ID_DIRLIGHT:
                    Keystone.Lights.Light light = Keystone.Celestial.LightsHelper.LoadDirectionalLight(AppMain.REGION_DIAMETER * 0.5f);
                    region.AddChild(light);
                    break;
                case TOOLBOX_ID_POINTLIGHT:
                    // TODO: for pointlight, we want a mouse pick location not context.Position
                    Keystone.Lights.PointLight pointLight = Keystone.Celestial.LightsHelper.LoadPointLight(region, position);
                    region.AddChild(pointLight);
                    break;
                case TOOLBOX_ID_SPOTLIGHT:
                    Keystone.Lights.SpotLight spotLight = Keystone.Celestial.LightsHelper.LoadSpotLight(region, position);
                    region.AddChild(spotLight);
                    break;

                // Water
                case TOOLBOX_ID_WATER_MESH:
                	AddWaterPatch2();
                	System.Diagnostics.Debug.WriteLine ("EditorWorkspace.Toolbox.ToolboxItem_Dropped() - Adding non-reflective water patch to " + region.ID + " at " + position.ToString());
                	break;
                	  
                	
                // TVLandscape based Terrain
                // TODO: move to Asset Prefab so i can place with tool, or can we get the Tool to spawn here for Toolbox items as well? Toolbox is kind of redundant, but its easier to navigate
                case TOOLBOX_ID_TERRAIN :
                	// for terrain, if region is a Zone, we are going to
                	// compute a new position that is the center of the region
                	// at x,z and bottom of region at y
                	if (region is Keystone.Portals.Zone)
                	{
                		position = region.SceneNode.BoundingBox.Center; 
                		position.y -= region.SceneNode.BoundingBox.Height /2d;
                		System.Diagnostics.Debug.WriteLine ("EditorWorkspace.Toolbox.ToolboxItem_Dropped() - Adding TVLandscape terrain to " + region.ID + " at " + position.ToString());
                	}
                   AddTerrain( position);
                   break;
                   
                // TVMesh based Terrain
                case TOOLBOX_ID_TERRAIN_MESH :
                   	// for terrain, if region is a Zone, we are going to
                	// compute a new position that is the center of the region
                	// at x,z and bottom of region at y
                	
                	
                	// http://www.zephyrosanemos.com/#heading3
                	// - above is a great site that shows a terrain implementation.
                	//   he also advises that adding a skirt to hide seams between terrains is the best way to go.
                	//   - its faster in practice and doesnt require terrain to verts to be modified. You are litterally just
                	//     inserting a skirt.
                	// TODO: we need visual aids for mouse picking a terrain placement.. ideally snapping
                	//       asset placement style.  we can create the asset on the fly and prefab it...
                	//       as we do for visual aids like wall placement stick.  and picking we need
                	//       to pick first zone, not deepest?
                	//       visual aid i should be able to add a colored grid patch even over the mouse zone
                	//       or even a textured box or transparent box + the grid patch where it would fit in there...etc
                	
                	region = context.Region ;
                	if (region is Keystone.Portals.Zone)
                	{
                		// TODO: can we launch a tool here instead? 
                		//       and then, how do we deal with mesh terrain? and stitching them and LOD and heightmap editing
                		position = region.SceneNode.BoundingBox.Center; 
                		position.y -= region.SceneNode.BoundingBox.Height /2d;
                		System.Diagnostics.Debug.WriteLine ("EditorWorkspace.Toolbox.ToolboxItem_Dropped() - Adding TVMesh terrain to " + region.ID + " at " + position.ToString());
                	}
                   //AddTerrainMesh(region, position);
                   AddTerrainSegment (position);
                    
                   //AddTileStructureToRegion ( position.y);
                   break;

                // Game Objects
                case TOOLBOX_ID_SPAWNPOINT:
                    Entity spawnPoint = AddSpawnPoint(position);
                    region.AddChild(spawnPoint);
                    break;
                case TOOLBOX_ID_TRIGGER_VOLUME:
                    Entity triggerVolume = AddTriggerVolume(position);
                    region.AddChild(triggerVolume);
                    break;

                // AI
                case TOOLBOX_ID_NAVPOINT :
                	Entity waypoint = AddWaypoint(position);
                    region.AddChild(waypoint);
                    break;

                // primitives
                case TOOLBOX_ID_FLOOR:
        			AddFloor ( position, 100, 100);
                	break;
                case TOOLBOX_ID_BILLBOARD:
                    AddBillboard( position);
                    break;
                case TOOLBOX_ID_CIRCLE:
                    AddCircle(region, position);
                    break;
                case TOOLBOX_ID_LASER:
                    AddBillboardLaser(position);
                    break;
                // celestial
                case TOOLBOX_ID_MOTIONFIELD:

                    break;
                case TOOLBOX_ID_STARFIELD:
                    AddStarfield(context.Scene.Root, context.Position);
                    break;
                case TOOLBOX_ID_PLANETOIDFIELD:
                    AddPlanetoidField(region, position);
                    break;
                case TOOLBOX_ID_NEBULA :
                    AddNebula(region, position);
                    break;

                // celestial - stars & planets
                case TOOLBOX_ID_SOLSYSTEM:
                    Keystone.Celestial.StellarSystem system = AddSolSystem(position);
                    region.AddChild(system);
                    ((FormMain)AppMain.Form).SaveNode (region.ID);
                    break;
                case TOOLBOX_ID_STAR :
                    AddStar(region, position);
                    break;
                case TOOLBOX_ID_GASGIANT:
                    AddGasPlanet(region, position);
                    break;
                case TOOLBOX_ID_TERRESTIALPLANET:
                    AddTerrestialPlanet(region, position);
                    break;
                case TOOLBOX_ID_MOON:                    
                    AddMoon(region, position);
                    break;

                case TOOLBOX_ID_EXPLOSION :
                    AddExplosion(region, position);
                    break;
                case TOOL_ID_PARTICLESYSTEM:
                    AddParticleSystem(region, position);
                    break;
                case TOOLBOX_ID_THRUSTER:
                    AddThruster(position);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Add's Entities to Scene at runtime using networked message so that
		/// scene modifications only occur during update thread and never from
		/// Client.exe GUI thread.		Attempting to add entities directly to scene
		/// via parent.AddChild(entity) from Client.exe GUI thread will result in random
		/// crashes.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="parentTypeNameFilter"></param>
        /// <param name="overridePickedPosition"</param> 
        private void ActivateBrush(Entity entity, uint brushStyle, KeyCommon.Flags.EntityAttributes ignoredTypesFilter, KeyCommon.Flags.EntityAttributes excludedParentTypesFilter, bool overridePickedPosition)
        {
        	// TODO: since we are using TEMP files to hold these prefabs, the problem is they wont be available
        	// when trying to re-load a saved scene!  Therefore, the options are to 
        	// a) save these to a prefab cache instead that gets associated with that particular Scene so that they are
        	//    per-scene specific.
        	// b) store all resources (eg meshes, textures, etc) required by these prefabs, in the common.zip or in some common
        	// prefab_cache.zip 
        	
            string prefabFilePath = Keystone.IO.XMLDatabase.GetTempFileName();
            prefabFilePath += ".kgbentity"; // MUST have .kgbentity or .kgbsegment extension to be recognized as non-nested-archive prefab database
            
            Keystone.IO.XMLDatabaseCompressed xmldb = new Keystone.IO.XMLDatabaseCompressed();
            xmldb.Create(Keystone.Scene.SceneType.Prefab, prefabFilePath, entity.TypeName);
            xmldb.WriteSychronous(entity, true, true, false);
            xmldb.SaveAllChanges();
            xmldb.Dispose();

                 
        	string path = prefabFilePath;
          
        	// TODO: some elements we want added at fixed 0,0,0 location on mouse click.  Water at center of a zone perhaps
        	//       day & night cycle as a viewpoint fixed (but y == 0 always)
        	// TODO: perhaps easier to just add option to use passed in translation and to always ignore mouse over location
        	// TODO: can we pass a filter of parent types that are legal?  
        	// TODO: can we pre-cache all toolbox items in a \prefabcache\ folder so they are ready to use quickly and dont have to be
        	//       written then read back in
        	// TODO: still need to prevent preview of some items (auto-tile terrain) and to show preview of others... like water.  
        	//       -maybe a placementtool option to "EnablePlacementPreviewRendering = true/false"
        	KeyEdit.Workspaces.Tools.AssetPlacementTool placementTool = new KeyEdit.Workspaces.Tools.AssetPlacementTool 
        		(
        			AppMain.mNetClient,
        			path,
                    brushStyle,
        			null
        		);
        	
        	placementTool.IgnoredParentTypes = ignoredTypesFilter;
        	placementTool.ExcludedParentTypes = excludedParentTypesFilter;
        	//if (overridePickedPosition)
        	//	placementTool.PlacementMode = Keystone.EditTools.PlacementMode.UseExistingPreviewEntityPosition;
        	
        	
        	// TODO: how come this isn't resulting in a PREVIEW water entity being shown?
        	//       - it could be because shader that is assigned, is not resulting in nothing being rendered...
        	//     perhaps there is no light assigned to it, so we should apply temp fullbright material to it?
			this.CurrentTool = placementTool;

			
//            KeyCommon.Messages.MessageBase msg;
//            
//            if (parent is Keystone.TileMap.Structure)
//            {
//            	msg= new KeyCommon.Messages.Insert_Prefab_Into_Structure(prefabFilePath, null);
//	
//	            // note: for placing entities, for now we only support one at a time but the overload exists for multiple
//	            ((KeyCommon.Messages.Insert_Prefab_Into_Structure)msg).Positions = new Keystone.Types.Vector3d[] { entity.Translation };
//	            ((KeyCommon.Messages.Insert_Prefab_Into_Structure)msg).Rotations = new Quaternion[] { entity.Rotation };
//	            ((KeyCommon.Messages.Insert_Prefab_Into_Structure)msg).ParentStructureID = parent.ID;
//            }
//            else
//            {
//            	msg = new KeyCommon.Messages.Prefab_Load(prefabFilePath, null);
//            	((KeyCommon.Messages.Prefab_Load)msg).ParentID = parent.ID;
//            	((KeyCommon.Messages.Prefab_Load)msg).Position = entity.Translation;
//            }
//			  AppMain.mNetClient.SendMessage(msg);

////            KeyCommon.Messages.node_ msg = new KeyCommon.Messages.Node_Create_Request("PointLight", region.ID);
////            msg.Add("range", typeof(float).Name, range);
////            msg.Add("color", color.GetType().Name, color);
////            msg.Add("specular", specular.GetType().Name, specular);
////            msg.Add("position", pos.GetType().Name, pos);
////            msg.Add("attenuation", typeof(float[]).GetType().Name, new float[] {attenuation0, attenuation1, attenuation2});
        }


        #region Quick Test Geometry, Lights, And FX Functions

        #region Regions / Zones
        private void AddZone(Keystone.Portals.Region region, Vector3d position)
        {
            throw new NotImplementedException("Using New/Create New Terrain Level instead.");
            //            if (AppMain._core.SceneManager == null ||
            //                AppMain._core.SceneManager.Scenes == null ||
            //                AppMain._core.SceneManager.Scenes.Length == 0) return;
            //
            //            // the primary problem here is that the viewport rendering is not
            //            // threaded.
            //            FormPleaseWait waitDialog = new FormPleaseWait();
            //            waitDialog.Owner = this.mWorkspaceManager.Form;
            //            waitDialog.Show(this.mWorkspaceManager.Form);
            //
            //            Keystone.Portals.Zone region = new Keystone.Portals.Zone  ();
            //            
            //            // TODO: here, the selected asset when loaded should be placed
            //            //       into our Repository and somehow flagged as a prefab that exists
            //            //       and manages links to entities that are using it but otherwise
            //            //       is not used in the scene.  The prefab is also used by the AssetPlacementTool
            //            // TODO: when saving an entity that references a Prefab, that entity then should be loaded
            //            //       when reading the 'DEF' attribute 
            //            Keystone.EditTools.PlacementTool placer = new KeyEdit.Workspaces.Tools.AssetPlacementTool
            //            (
            //                    AppMain.mNetClient,
            //                    region, 
            //                    null
            //            );
            //            waitDialog.Close();
            //
            //            this.CurrentTool = placer;
        }
        #endregion
                

        #region Terrain
        private void GenerateProceduralTerrainStructures()
        {

        }

        private void TerrainRaise()
        {
            string path = System.IO.Path.Combine(AppMain.ModName, @"meshes\terrain\dirt.kgbsegment");
            uint brushStyle = Keystone.EditTools.PlacementTool.BRUSH_TERRAIN_SEGMENT;
            KeyEdit.Workspaces.Tools.AssetPlacementTool raiseTerrainTool = new KeyEdit.Workspaces.Tools.AssetPlacementTool
                (
                    AppMain.mNetClient,
                    path,
                    brushStyle,
                    null
                );

            raiseTerrainTool.SetValue("layout", path);
            raiseTerrainTool.paintOperation = KeyCommon.Messages.PaintCellOperation.PAINT_OPERATION.ADD;
            raiseTerrainTool.ShowPreview = false;
            raiseTerrainTool._brushStyle = 1; // 1 = PlacementTool.BRUSH_FLOOR
            this.CurrentTool = raiseTerrainTool;
        }

        private void TerrainLower()
        {
            string path = System.IO.Path.Combine(AppMain.ModName, @"meshes\terrain\dirt.kgbsegment");
            uint brushStyle = Keystone.EditTools.PlacementTool.BRUSH_TERRAIN_SEGMENT;

            KeyEdit.Workspaces.Tools.AssetPlacementTool lowerTerrainTool = new KeyEdit.Workspaces.Tools.AssetPlacementTool
                (
                    AppMain.mNetClient,
                    path,
                    brushStyle,
                    null
                );

            lowerTerrainTool.SetValue("layout", path);
            lowerTerrainTool.paintOperation = KeyCommon.Messages.PaintCellOperation.PAINT_OPERATION.REMOVE;
            lowerTerrainTool.ShowPreview = false;
            lowerTerrainTool._brushStyle = 1; // 1 = PlacementTool.BRUSH_FLOOR
            this.CurrentTool = lowerTerrainTool;
        }

        private void AddWallToStructure()
        {
            string path = System.IO.Path.Combine(AppMain.ModName, @"entities\structure\walls\wall00.kgbsegment");
            uint brushStyle = Keystone.EditTools.PlacementTool.BRUSH_EDGE_SEGMENT;

            KeyEdit.Workspaces.Tools.AssetPlacementTool addWallTool = new KeyEdit.Workspaces.Tools.AssetPlacementTool
                (
                    AppMain.mNetClient,
                    path,
                    brushStyle,
                    null
                );

            addWallTool.SetValue("layout", path);
            addWallTool.paintOperation = KeyCommon.Messages.PaintCellOperation.PAINT_OPERATION.ADD;
            addWallTool.ShowPreview = false;
            addWallTool._brushStyle = 3; // 3 = PlacementTool.BRUSH_EDGE_SEGMENT (aka WALL)
            this.CurrentTool = addWallTool;
        }

        private void AddTileStructureToRegion(double height)
        {
            // this is a Region that has a visible tile based structure such as floors, walls and ceilings.
            // The structure is regarded as part of the Region and not as seperate entities.
            // This is for performance (rendering and memory consumption) primarily.
            // Since having seperate entities for every Tile in the region is costly.
            string id = Repository.GetNewName(typeof(Keystone.TileMap.Structure));
            Keystone.TileMap.Structure structure = (Keystone.TileMap.Structure)Repository.Create(id, "Structure"); //new Keystone.TileMap.Structure (id);


            // DEBUG TEMP
            // assign one map layer for footprints and one for terrain 
            string[] layerPaths = new string[2];
            // TODO: ewww, hard coded paths
            layerPaths[0] = string.Format(@"D:\dev\c#\KeystoneGameBlocks\Data\pool\terrain\{0}{1}{2}footprint.bmp", "$", structure.ID, ':');
            layerPaths[1] = string.Format(@"D:\dev\c#\KeystoneGameBlocks\Data\pool\terrain\{0}{1}{2}terrain_layout.bmp", "$", structure.ID, ':');

            layerPaths = new string[] { @"D:\dev\c#\KeystoneGameBlocks\Data\pool\terrain\terrain_layout.bmp" };

            // terrain_layout.bmp  // index into array of terrain models

            // structure_layout.bmp // 32bit flags indicating the layout of each specific cell (which walls, floors exist and in what configuration)
            // structure_style.bmp  // index into array of segment entities that defines the visual appearance of cell and correspond to a layout in _layout.bmp


            // begin temp: create these bitmaps and save to disk.... seed with random values from 0 - N for now
            if (System.IO.File.Exists(layerPaths[0]) == false)
            {
                int N = 4;
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(8, 8, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                for (int x = 0; x < bmp.Height; ++x)
                    for (int y = 0; y < bmp.Width; ++y)
                    {
                        // use the R to set value
                        // NOTE: for MAX +1 because MAX is exclusive, only MIN is inclusive
                        // NOTE: for Min, we start at 1 because a value 0 means no lookup index is provided and so no geometry goes there.

                        int r = Keystone.Utilities.RandomHelper.RandomNumber(1, N + 1);
                        System.Drawing.Color color = System.Drawing.Color.FromArgb(0, r, 0, 0);
                        // cannot use .SetPixel with Format8bppIndexed so we will use Format24bppRgb format for this test lookup image
                        bmp.SetPixel(x, y, color);
                    }

                bmp.Save(layerPaths[0]);
            }
            // end temp - test bitmap creation

            // store the property
            structure.SetProperty("maplayerpaths", typeof(string[]), layerPaths);

            // domain objects (aka entity scripts) are assigned via entity.ResourcePath
            //        	string scriptPath = @"E:\dev\c#\KeystoneGameBlocks\Data\pool\scripts\tile_structure.css";
            //        	structure.ResourcePath = scriptPath;


            ActivateBrush(structure,
                            Keystone.EditTools.PlacementTool.BRUSH_EXTERIOR_STRUCTURE,
                           KeyCommon.Flags.EntityAttributes.AllEntityAttributes_Except_Region,
                           KeyCommon.Flags.EntityAttributes.AllEntityAttributes_Except_Region,
                           true);
        }


        private void AddTerrainMesh(Vector3d position)
        {
            // NOTE: The lack of built in QuadTree really makes Mesh3d based Terrain MUCH slower
            // than TVLandscape when using high vertex counts.  That is why the trick is to heavily
            // LOD and keep terrain patches smal
            string id = Repository.GetNewName(typeof(ModeledEntity));
            Keystone.Entities.ModeledEntity terrainEntity = new ModeledEntity(id);
            terrainEntity.SetEntityFlagValue("terrain", true);
            terrainEntity.Translation = position;
            terrainEntity.Dynamic = false;
            // field.LatestStepTranslation = position; // terrain is not dynamic. it is fixed object.

            // create mesh
            id = Repository.GetNewName(typeof(Geometry));

            float width = AppMain.REGION_DIAMETER; // 125; // 1.25 cell * 1000
            float depth = AppMain.REGION_DIAMETER; // 125;
            // for now we'll cap width & depth since working with single huge default sized region will break picking of Floor mesh
            width = Math.Min(width, 65000);
            depth = Math.Min(depth, 65000);

            // numCellsX and numCellsZ must be low enough so that total verts created is less than 65k!
            uint cellsX = 10; // 100x100 = 10201 vertices in 20000 triangles
            uint cellsZ = 10;

            float tileU = 1.0f;
            float tileV = 1.0f;

            Mesh3d terrainGeometry = Mesh3d.CreateFloor(width, depth, cellsX, cellsZ, tileU, tileV);

            Model model = (Model)Repository.Create("Model");


            Material material = Material.Create(Material.DefaultMaterials.matte);

            Keystone.Appearance.Appearance appearance = null;

            bool useSplatting = false;
            if (useSplatting == false)
            {
                // non splatting appearance
                appearance = (DefaultAppearance)Repository.Create("DefaultAppearance");
            }
            else
            {
                appearance = (SplatAppearance)Repository.Create("SplatAppearance");
            }

            appearance.AddChild(material);

            model.AddChild(appearance);
            model.AddChild(terrainGeometry);
            terrainEntity.AddChild(model);

            ActivateBrush(terrainEntity,
                           Keystone.EditTools.PlacementTool.BRUSH_FLOOR_MESH,
                           KeyCommon.Flags.EntityAttributes.AllEntityAttributes_Except_Region,
                           KeyCommon.Flags.EntityAttributes.AllEntityAttributes_Except_Region,
                           true);
        }


        // tv3d built in water effectdirt.kgbentity
        private void AddWaterPatch()
        {
            string id = Repository.GetNewName(typeof(ModeledEntity));
            Keystone.Entities.ModeledEntity waterEntity = new ModeledEntity(id);
            waterEntity.SetEntityFlagValue("laterender", true);
            waterEntity.Dynamic = false;
            // waterEntity.LatestStepTranslation = position; // water is not dynamic (i.e. not physics object). it is static object with animation.
            //waterEntity.ResourcePath = "water.css";

            Model model = new Model(Repository.GetNewName(typeof(Model)));

            // create geometry
            id = Repository.GetNewName(typeof(Geometry));
            Mesh3d waterGeometry = Mesh3d.CreateFloor(80, 80, 1, 1, 1.0f, 1.0f);
            waterGeometry.CullMode = (int)CONST_TV_CULLING.TV_DOUBLESIDED;
            waterGeometry.LightingMode = CONST_TV_LIGHTINGMODE.TV_LIGHTING_BUMPMAPPING_TANGENTSPACE;
            waterGeometry.LoadTVResource();
            waterGeometry.ComputeNormals();
            waterGeometry.PageStatus = Keystone.IO.PageableNodeStatus.Loaded;

            string waveTexturePath = @"C:\Users\Hypnotron\Documents\dev\c#\KeystoneGameBlocks\Data\pool\textures\waves.dds";
            //              waveTexturePath = DUDVTexture.Create(Repository.GetNewName(typeof(DUDVTexture)), @"Shaders\Water\Water_DUV.dds", -1, -1, 50, false);
            Keystone.Appearance.Appearance appearance = Keystone.Celestial.ProceduralHelper.CreateAppearance(CONST_TV_LIGHTINGMODE.TV_LIGHTING_NORMAL,
                                                                                                              null, null, waveTexturePath, null, null, false);
            model.AddChild(waterGeometry);
            model.AddChild(appearance);

            waterEntity.AddChild(model);

            Vector3d pos = new Vector3d(0, 1.0, 0);
            waterEntity.Translation = pos;


            // water script should be used to register to FXWater
            ((Keystone.Scene.ClientSceneManager)AppMain._core.SceneManager).FXProviders[(int)Keystone.FX.FX_SEMANTICS.FX_WATER_LAKE].Register(waterEntity);

            ActivateBrush(waterEntity,
                           Keystone.EditTools.PlacementTool.BRUSH_WATER,
                           KeyCommon.Flags.EntityAttributes.AllEntityAttributes_Except_Region,
                           KeyCommon.Flags.EntityAttributes.AllEntityAttributes_Except_Region,
                           true);
        }

        // non-reflective water shader
        // NOTE: unlike reflective water, this non reflective does not need to register to a  FXSystem.  
        private void AddWaterPatch2()
        {
            string id = Repository.GetNewName(typeof(ModeledEntity));
            Keystone.Entities.ModeledEntity waterEntity = new ModeledEntity(id);
            waterEntity.SetEntityFlagValue("laterender", true);
            waterEntity.Dynamic = false;
            // waterEntity.ResourcePath = "water.css";

            Model model = new Model(Repository.GetNewName(typeof(Model)));

            // create geometry
            id = Repository.GetNewName(typeof(Geometry));
            Mesh3d waterGeometry = Mesh3d.CreateFloor(80, 80, 1, 1, 1.0f, 1.0f);
            waterGeometry.CullMode = (int)CONST_TV_CULLING.TV_DOUBLESIDED;  // doublesided if you want to see water surface from underwater
            waterGeometry.LightingMode = CONST_TV_LIGHTINGMODE.TV_LIGHTING_NORMAL;

            // TEXTURE0 : DIFFUSE MAP        NULL
            // TEXTURE1 : NORMAL MAP         "NormalMap_DXT5NM.dds
            // TEXTURE2 : SPECULAR MASK      "SpecularMask.dds"
            // TEXTURE3 : ALPHA MAP          "AlphaMap.dds")
            string shaderPath = System.IO.Path.Combine(AppMain.MOD_PATH, @"caesar\shaders\Water\NonReflectiveWaterShader.fx");
            string normalMapPath = System.IO.Path.Combine(AppMain.MOD_PATH, @"caesar\shaders\Water\Maps\NormalMap_DXT5NM.dds");
            string specularMask = System.IO.Path.Combine(AppMain.MOD_PATH, @"caesar\shaders\Water\Lookup\SpecularMask.dds");
            // TODO: this alphaMap should be generated based on our terrain... then to avoid rendering beneath visible terrain, it looks at the alphamap
            //       and clip() when pixel.alpha < 0.001.
            //       if wide open sea, then the entire map should be alpha or perhaps none at all.
            // the specular mask "could" be kept for all patches, but not the alphamap.  Since these textures
            // are passed in using TV's semantics, we can have different alphamaps automatically by assigning that alphamap to the appearance in the emissive map layer
            string alphaMapPath = System.IO.Path.Combine(AppMain.MOD_PATH, @"caesar\shaders\Water\Lookup\AlphaMap.dds");

            // NOTE: make sure you don't mix the order of arguments for emissive and specular texture layers.  
            Keystone.Appearance.Appearance appearance = Keystone.Celestial.ProceduralHelper.CreateAppearance(CONST_TV_LIGHTINGMODE.TV_LIGHTING_NORMAL,
                                                                                                              shaderPath, null, normalMapPath, alphaMapPath, specularMask, false);
            // TODO: in general, these texturemode settings are placed throughout app very haphazardly and if memory serves, in this case
            // we were experimenting a bit.. i dont even know when/if these are necessary in this case
            AppMain._core.TextureFactory.SetTextureMode(CONST_TV_TEXTUREMODE.TV_TEXTUREMODE_COMPRESSED);
            AppMain._core.TextureFactory.SetTextureMode(CONST_TV_TEXTUREMODE.TV_TEXTUREMODE_32BITS);

            model.AddChild(waterGeometry);
            model.AddChild(appearance);

            waterEntity.AddChild(model);

            double seaLevel = -0.25d;
            Vector3d pos = new Vector3d(0, seaLevel, 0);
            waterEntity.Translation = pos;

            // HACK: these parameters require the appearance node is paged in so that the Shader is created
            // TODO: are the following parameters even being saved at all?
            Keystone.IO.PagerBase.LoadTVResource(appearance);
            Keystone.Types.Color _waterDarkColor = new Keystone.Types.Color(0.08f, 0.3f, 0.4f, 1f);
            Keystone.Types.Color _waterLightColor = new Keystone.Types.Color(0.21f, 0.45f, 0.6f, 1f);
            appearance.DefineShaderParameter("waterDarkColor", typeof(Keystone.Types.Color).Name, _waterDarkColor);
            appearance.DefineShaderParameter("waterLightColor", typeof(Keystone.Types.Color).Name, _waterLightColor);
            // The wave movement speed, based on texture coordinates scrolling/translation
            appearance.DefineShaderParameter("waveMovementSpeed", typeof(float).Name, 3f);
            // NOTE: seems a minimum tiling is needed for both.  Using 1,1 seems to break it.
            // The specular mask tiling, defines the size and spacing of the
            // specular "sparkles"
            appearance.DefineShaderParameter("sparkleSize", typeof(Keystone.Types.Vector2f).Name, new Vector2f(2f, 2f));
            // normalmap tiling -- using more tiles on Y makes the waves appear oval, looks a little better IMHO
            appearance.DefineShaderParameter("waveSize", typeof(Keystone.Types.Vector2f).Name, new Vector2f(2f, 4f));



            // TODO: is water a child of structure or is it added to Zone?  I believe at one point i did decide
            //       that any entity added to a zone should be to the zone and that the structure is static 
            // 		 structural entities like castle walls, terrain, etc.


            // filter structures to be only allowed placement target
            // force position to be 0, -0.25, 0?  but if this were a real prefab in asset browser
            // how would we enforce the position when adding it to the scene?
            ActivateBrush(waterEntity,
                           Keystone.EditTools.PlacementTool.BRUSH_WATER,
                           KeyCommon.Flags.EntityAttributes.AllEntityAttributes_Except_Structure,
                           KeyCommon.Flags.EntityAttributes.Background | KeyCommon.Flags.EntityAttributes.Light | KeyCommon.Flags.EntityAttributes.Occluder,
                           true);
        }

        private void AddTerrainSegment(Vector3d position)
        {
            bool textured = false;
            ModeledEntity entity = Keystone.Celestial.ProceduralHelper.CreateTerrainTileSegment(textured);

            entity.Translation = position;
            entity.LatestStepTranslation = position;

            ActivateBrush(entity,
                           Keystone.EditTools.PlacementTool.BRUSH_TERRAIN_SEGMENT,
                           KeyCommon.Flags.EntityAttributes.AllEntityAttributes_Except_Region,
                           KeyCommon.Flags.EntityAttributes.AllEntityAttributes_Except_Region,
                           false);
        }

        private void AddTerrain(Vector3d position)
        {
            //            KeyCommon.Messages.Node_Create_Request msg = new KeyCommon.Messages.Node_Create_Request("Terrain", region.ID);
            //            
            //             // position is always in current region coords           
            //            //msg.Add("width", 1000);
            //            //msg.Add("depth", 1000);
            //            msg.Add("chunksx", typeof (int).Name, 10);
            //            msg.Add("chunksz", typeof (int).Name, 10);
            //            msg.Add("position", pos.GetType().Name, position);
            //
            //            AppMain.mNetClient.SendMessage(msg);

            string id = Repository.GetNewName(typeof(ModeledEntity));
            Keystone.Entities.ModeledEntity terrainEntity = new ModeledEntity(id);
            terrainEntity.SetEntityFlagValue("terrain", true);
            terrainEntity.Translation = position;
            terrainEntity.Dynamic = false;
            // terrainEntity.LatestStepTranslation = position; // terrain is not dynamic. it is fixed object.

            // create geometry
            id = Repository.GetNewName(typeof(Geometry));
            Terrain terrainGeometry = (Terrain)Repository.Create(id, "Terrain");
            // world width is 256 meters * chunks * scale
            terrainGeometry.ChunksX = 3;
            terrainGeometry.ChunksZ = 3;
            // precision has to do with the reading of heightmap values
            terrainGeometry.SetProperty("precision", typeof(int), CONST_TV_LANDSCAPE_PRECISION.TV_PRECISION_BEST);
            terrainGeometry.EnableLOD(true, 256, CONST_TV_LANDSCAPE_PRECISION.TV_PRECISION_LOW, 512, true);
            terrainGeometry.EnableProgressiveLOD(false);

            // if "heightmap" property is empty, an empty terrain is created instead
            // terrainGeometry.SetProperty ("heightmap", typeof(string), "HieghtMap.png");// "Long_6972102.jpg"); // "Dragon-Valley.png"); // "ireland-heightmap.jpg"); // "test.jpg"); //"jersey.jpg");
            Vector3d modelScale = new Vector3d(3, 4, 3); // Toaster's "HieghtMap.png" looks good at this scale

            Model model = new Model(Repository.GetNewName(typeof(Model)));
            model.Scale = modelScale;


            string terrainDataPath = System.IO.Path.Combine(AppMain._core.ModsPath, "terrain");
            bool useSplatting = false;
            Keystone.Appearance.Appearance appearance = null;


            if (useSplatting == false)
            {
                // non splatting appearance
                appearance = (DefaultAppearance)Repository.Create("DefaultAppearance");

                string texturePath0 = System.IO.Path.Combine(terrainDataPath, "texture.dds");
                texturePath0 = @"d:\dev\c#\KeystoneGameBlocks\Data\editor\default_terrain.bmp"; // todo: remove hardcoded path

                Keystone.Appearance.Diffuse diffuseLayer = (Keystone.Appearance.Diffuse)Keystone.Resource.Repository.Create("Diffuse");
                Keystone.Appearance.Texture tex = (Texture)Keystone.Resource.Repository.Create(texturePath0, "Texture");
                tex.TextureType = Texture.TEXTURETYPE.Default;
                diffuseLayer.AddChild(tex);
                appearance.AddChild(diffuseLayer);
            }
            else
            {
                // splatting appearance
                appearance = (SplatAppearance)Repository.Create("SplatAppearance");
                // we'll use single group with same splat layers all chunks          


                string texturePath1 = System.IO.Path.Combine(terrainDataPath, "grass1.png");
                string texturePath2 = System.IO.Path.Combine(terrainDataPath, "rock 6.png");
                string texturePath3 = System.IO.Path.Combine(terrainDataPath, "dirt 1.png");
                string texturePath4 = System.IO.Path.Combine(terrainDataPath, "snow 1.png");


                Keystone.Appearance.SplatAlpha splatLayer = (SplatAlpha)Keystone.Resource.Repository.Create("SplatAlpha");
                Keystone.Appearance.Texture baseTexture = (Texture)Keystone.Resource.Repository.Create(texturePath1, "Texture");
                baseTexture.TextureType = Texture.TEXTURETYPE.Default;
                // TODO: alpha textures generally do not want to be shared.  They can be, but for terrains it'd be unlikely
                //       unless the same exact terrain is desired to be used in more than one place.  
                //       So as a rule, we want to name alpha maps differently for each terrain.  Remember, alphas can be edited
                //       in real-time and then re-saved.  So we need to load the alphamap, or have a blank one generated
                //		 and then allow it to save to \mods\terrains  path?		
                string alphaPath1 = System.IO.Path.Combine(terrainDataPath, "texture.dds");
                Keystone.Appearance.Texture alphaMap = (Texture)Keystone.Resource.Repository.Create(alphaPath1, "Texture");
                alphaMap.TextureType = Texture.TEXTURETYPE.Alpha;

                splatLayer.AddChild(baseTexture);
                appearance.AddChild(splatLayer);
                //appearance.AddDefine("DIFFUSEMAP", null);

                splatLayer = (SplatAlpha)Keystone.Resource.Repository.Create("SplatAlpha");
                splatLayer.TileU = 64;
                splatLayer.TileV = 64;
                baseTexture = (Texture)Keystone.Resource.Repository.Create(texturePath2, "Texture");
                baseTexture.TextureType = Texture.TEXTURETYPE.Default;
                splatLayer.AddChild(baseTexture);
                appearance.AddChild(splatLayer);

                splatLayer = (SplatAlpha)Keystone.Resource.Repository.Create("SplatAlpha");
                splatLayer.TileU = 64;
                splatLayer.TileV = 64;
                baseTexture = (Texture)Keystone.Resource.Repository.Create(texturePath3, "Texture");
                baseTexture.TextureType = Texture.TEXTURETYPE.Default;
                splatLayer.AddChild(baseTexture);
                appearance.AddChild(splatLayer);

                splatLayer = (SplatAlpha)Keystone.Resource.Repository.Create("SplatAlpha");
                splatLayer.TileU = 64;
                splatLayer.TileV = 64;
                baseTexture = (Texture)Keystone.Resource.Repository.Create(texturePath4, "Texture");
                baseTexture.TextureType = Texture.TEXTURETYPE.Default;
                splatLayer.AddChild(baseTexture);
                appearance.AddChild(splatLayer);
            }

            Material material = Material.Create(Material.DefaultMaterials.matte);
            appearance.AddChild(material);

            model.AddChild(appearance);
            model.AddChild(terrainGeometry);
            terrainEntity.AddChild(model);


            ActivateBrush(terrainEntity,
                           Keystone.EditTools.PlacementTool.BRUSH_TERRAIN,
                           KeyCommon.Flags.EntityAttributes.AllEntityAttributes_Except_Region,
                           KeyCommon.Flags.EntityAttributes.AllEntityAttributes_Except_Region,
                           true);
        }
        #endregion // Terrain

        #region Game Objects
        private Entity AddSpawnPoint(Vector3d position)
        {
            Entity spawnPoint = new Keystone.Entities.DefaultEntity(Repository.GetNewName(typeof(Keystone.Entities.DefaultEntity)));
            spawnPoint.SetEntityAttributesValue((uint)KeyCommon.Flags.EntityAttributes.MissionObject, true);
            spawnPoint.ResourcePath = "caesar\\scripts_entities\\spawnpoint.css";
            spawnPoint.Name = "spawn point 0";
            spawnPoint.Translation = position;
            spawnPoint.LoadTVResource();

            spawnPoint.SetEntityAttributesValue((uint)KeyCommon.Flags.EntityAttributes.ServerObject, true);
            return spawnPoint;
        }

        private Entity AddTriggerVolume(Vector3d position)
        {

            throw new NotImplementedException();
            //string id = Repository.GetNewName("BoxCollider");
            //Entity triggerVolume = new Keystone.Physics.Colliders.BoxCollider(id);

            //return triggerVolume;
        }

        #endregion

        #region AI
        private Entity AddWaypoint(Vector3d position)
        {
            // so i'm looking a various vids of waypoints.

            // you have the kind that are part of the scene or as "missions" (a mission
            // can be thought of as an overlay that gets applied to a static scene)
            // 
            // but the waypoints i'm thinking about I feel aren't really like that.
            // The waypoints I envision are for vehicles.
            // Perhaps the easiest way i can edit these is to right mouse click on the ship
            // have its waypoints appear when it's selected, then select from edit menu
            // to append a waypoint, change cursor and then allow user to click to place.

            // what if, I must "bind" the workspace to a vehicle before the
            // waypoint nav orders appear?

            // waypoints also double as a series of orders that we can schedule in advance
            //

            return null;
        }
        #endregion

        #region primitives
        // TODO: many of these should be prefabs that we design first and then just wire up to
        //       send network command to load that particular prefab.
        private void AddThruster(Vector3d position)
        {
            string id = Repository.GetNewName(typeof(ModeledEntity));
            ModeledEntity thruster = new ModeledEntity(id);
            thruster.Translation = position;

            string meshpath = System.IO.Path.Combine(AppMain.DATA_PATH, @"pool\shaders\Thruster\ThrustCylinderB.x");
            meshpath = @"common.zip|meshes\effects\plumes\Thrust2.obj";
            meshpath = @"common.zip|meshes\effects\plumes\Thruster3.obj";
            meshpath = @"common.zip|meshes\effects\plumes\Thruster1_interior_assembly.obj";
            string shaderPath = System.IO.Path.Combine(AppMain.DATA_PATH, @"pool\shaders\Thruster\Thrust.fx");
            string noiseVolumeTexture = System.IO.Path.Combine(AppMain.DATA_PATH, @"pool\shaders\Thruster\NoiseVolume.dds");


            Keystone.Appearance.DefaultAppearance appearance = (Keystone.Appearance.DefaultAppearance)Repository.Create("DefaultAppearance");

            Mesh3d geometry = (Mesh3d)Repository.Create(meshpath, "Mesh3d");

            // TODO: since change to thread sync Factory.Create, i think now to load noise volume texture correctly, i need to make sure the texture type is set properly
            Keystone.Appearance.VolumeTexture volumeLayer = (Keystone.Appearance.VolumeTexture)Keystone.Resource.Repository.Create(noiseVolumeTexture, "VolumeTexture");
            Keystone.Appearance.Texture texture = (Keystone.Appearance.Texture)Keystone.Resource.Repository.Create(noiseVolumeTexture, "Texture");
            volumeLayer.AddChild(texture);

            // appearance.AddChild(volumeTex);
            appearance.Groups[0].AddChild(volumeLayer);
            appearance.Groups[1].AddChild(volumeLayer);
            appearance.Groups[2].AddChild(volumeLayer);
            appearance.Groups[3].AddChild(volumeLayer);
            appearance.Groups[4].AddChild(volumeLayer);

            // NOTE: Shaders are now loaded by appearance or GroupAttribute for us if we assign path
            appearance.ResourcePath = shaderPath;
            appearance.Groups[0].ResourcePath = shaderPath;
            appearance.Groups[1].ResourcePath = shaderPath;
            appearance.Groups[2].ResourcePath = shaderPath;
            appearance.Groups[3].ResourcePath = shaderPath;
            appearance.Groups[4].ResourcePath = shaderPath;

            appearance.LightingMode = CONST_TV_LIGHTINGMODE.TV_LIGHTING_NORMAL;
            appearance.BlendingMode = CONST_TV_BLENDINGMODE.TV_BLEND_ALPHA;
            //geometry.CullMode = CONST_TV_CULLING.TV_DOUBLESIDED;

            id = Repository.GetNewName(typeof(Model));
            Model model = new Model(id);
            model.AddChild(geometry);
            model.AddChild(appearance);

            thruster.AddChild(model);

            ActivateBrush(thruster,
                Keystone.EditTools.PlacementTool.BRUSH_SINGLE_DROP,
                           KeyCommon.Flags.EntityAttributes.None,
                           KeyCommon.Flags.EntityAttributes.Background | KeyCommon.Flags.EntityAttributes.Light,
                           false);

        }

        private void AddFloor(Vector3d position, int width, int depth)
        {
            string id = Repository.GetNewName(typeof(ModeledEntity));
            ModeledEntity floor = new ModeledEntity(id);
            floor.Translation = position;
            Model model = new Model(Repository.GetNewName(typeof(Model)));


            uint numCellsX = 1;
            uint numCellsZ = 1;
            float tileU = 1f;
            float tileV = 1f;

            Mesh3d mesh = Mesh3d.CreateFloor(width, depth, numCellsX, numCellsZ, tileU, tileV);


            Keystone.Appearance.Appearance appearance = Keystone.Celestial.ProceduralHelper.CreateAppearance(CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED, "", "", "", "", "");

            // Types.Color diffuseColor, ambient, specular, emissive;
            // diffuseColor = new Types.Color(1.0f, 1.0f, 0.0f, 1.0f);
            // ambient = new Types.Color(1.0f, 1.0f, 0.0f, 1.0f);
            // specular = new Types.Color(1.0f, 1.0f, 0.0f, 1.0f);
            // emissive = new Types.Color(1.0f, 1.0f, 0.0f, 1.0f);


            Material diffuseMaterial = Material.Create(Material.DefaultMaterials.silver);
            appearance.RemoveMaterial();
            appearance.AddChild(diffuseMaterial);


            // NOTE: it's never necessary to add defines here since Model.UpdateShader() will handle that

            model.AddChild(appearance);
            model.AddChild(mesh);

            floor.AddChild(model);
            floor.Translation = position;
            floor.Dynamic = false;

            ActivateBrush(floor,
                Keystone.EditTools.PlacementTool.BRUSH_FLOOR_MESH,
               KeyCommon.Flags.EntityAttributes.None,
               KeyCommon.Flags.EntityAttributes.Background | KeyCommon.Flags.EntityAttributes.Light,
               false);
        }



        // Thruster FX System includes several sub-systems
        // - the overall system should have a bounding volume
        // - and the system should be attached as an Entity so that if the main ship is just barely offscreen
        //     the exhaust thruster plume fx will still be visible.
        //      - The model will hold data relevant to drawing the appropriate sized billboards and cones.
        //        
        //   - HOWEVER, the actual FX should be passed off to a batch renderer
        //     where they can be sorted and such.
        //   - the idea behind the batching of these thruster billboards is that
        //     it's a more procedural way to render an FX.  
        private void AddBillboard(Vector3d position)
        {
            string id = Repository.GetNewName(typeof(ModeledEntity));
            ModeledEntity billboardEntity = new ModeledEntity(id);
            billboardEntity.Translation = position;

            // NOTE: unit billboard.  We can scale up the Entity or Model that hosts this Geometry if we want a bigger billboard.
            // Otherwise, we should prompt with an Input dialog for user to select the height and width before creating the billboard geometry.
            float width = 1; // radius 1 is unit billboard width.. actually not quite (think hypotenuse for radius)
            float height = 1; // radius 1 is unit billboard height

            string shareableBillboardID = Billboard.GetCreationString(CONST_TV_BILLBOARDTYPE.TV_BILLBOARD_NOROTATION,
                                               true, width, height);
            Billboard geometry = (Billboard)Repository.Create(shareableBillboardID, "Billboard");
            geometry.AxialRotationEnable = true;

            //string meshpath = @"E:\dev\c#\KeystoneGameBlocks\Data\pool\shaders\Thruster\ThrustCylinderB.x";
            Keystone.Appearance.DefaultAppearance dummy;
            //Mesh3d geometry = Mesh3d.Create(meshpath, false, false, out dummy);

            // create a TextureCycle
            id = Repository.GetNewName(typeof(Keystone.Appearance.TextureCycle));
            Keystone.Appearance.TextureCycle cycle = Keystone.Appearance.TextureCycle.Create(id);


            // add all our textures
            bool useAnimatedTextures = true;
            string textureFolder = "";
            string texturePath = "";

            if (useAnimatedTextures)
            {
                int count = 30;
                // todo: dont we want to pass Repository.Create() a relative path and not include DATA_PATH?
                textureFolder = AppMain.DATA_PATH + @"\pool\textures\fs_darkest_dawn\"; // todo: remove hardcoded path
                for (int i = 0; i < count; i++)
                {
                    texturePath = System.IO.Path.Combine(textureFolder, "thruster01_" + i.ToString("D4") + ".bmp");
                    Keystone.Appearance.Texture tex = (Keystone.Appearance.Texture)Keystone.Resource.Repository.Create(texturePath, "Texture");
                    cycle.AddChild(tex);
                }
            }
            else
            {
                // todo: dont we want to pass Repository.Create() a relative path and not include DATA_PATH?
                textureFolder = AppMain.DATA_PATH + @"\pool\textures\";
                texturePath = System.IO.Path.Combine(textureFolder, "thruster02-01.dds");
                Keystone.Appearance.Texture tex = (Keystone.Appearance.Texture)Keystone.Resource.Repository.Create(texturePath, "Texture");
                cycle.AddChild(tex);
            }

            // create alpha blended appearance and add the texture cycle
            Keystone.Appearance.Appearance app = (Keystone.Appearance.DefaultAppearance)Repository.Create("DefaultAppearance");
            #region appearance for transparent billboards
            app.BlendingMode = CONST_TV_BLENDINGMODE.TV_BLEND_ADD;
            #endregion


            Keystone.Types.Color diffuseColor, ambient, specular, emissive;
            diffuseColor = new Keystone.Types.Color(1.0f, 1.0f, 0.5f, 1.0f);
            ambient = new Keystone.Types.Color(1.0f, 1.0f, 0.0f, 1.0f);
            specular = new Keystone.Types.Color(1.0f, 1.0f, 0.0f, 1.0f);
            emissive = new Keystone.Types.Color(1.0f, 1.0f, 0.0f, 1.0f);

            // TODO: create a shared material here using a name based on Material.GetDefaultName ();
            Keystone.Appearance.Material emissiveMaterial = Keystone.Appearance.Material.Create(Repository.GetNewName(typeof(Keystone.Appearance.Material)), diffuseColor, ambient, specular, emissive);
            emissiveMaterial.Opacity = 0.5f;


            app.AddChild(emissiveMaterial);
            app.AddChild(cycle); // NOTE: Add cycle AFTER the textures have been added to it for reqt Paging order

            id = Repository.GetNewName(typeof(Model));
            Model model = new Model(id);
            model.AddChild(geometry);
            model.AddChild(app);

            billboardEntity.AddChild(model);

            ActivateBrush(billboardEntity,
                Keystone.EditTools.PlacementTool.BRUSH_SINGLE_DROP,
               KeyCommon.Flags.EntityAttributes.None,
               KeyCommon.Flags.EntityAttributes.Background | KeyCommon.Flags.EntityAttributes.Light,
               false);
        }

        private void AddBillboardLaser(Vector3d position)
        {
            string id = Repository.GetNewName(typeof(ModeledEntity));
            ModeledEntity billboardEntity = new ModeledEntity(id);
            billboardEntity.Translation = position;


            // NOTE: unit billboard.  We can scale up the Entity or Model that hosts this Geometry if we want a bigger billboard.
            float width = 1; // radius 1 is unit billboard width.. actually not quite (think hypotenuse for radius)
            float height = 1; // radius 1 is unit billboard height. NOTE: we can scale the y axis to increase the length/range of the laser billboard.


            // NOTE: the .y component of the billboard is 0 and not 0.5.  So this means to make the billboard laser longer, we just need to scale it
            // and only the upper vertices will move. Perfect.
            string shareableBillboardID = Billboard.GetCreationString(CONST_TV_BILLBOARDTYPE.TV_BILLBOARD_NOROTATION,
                                               false, width, height);
            Billboard geometry = (Billboard)Repository.Create(shareableBillboardID, "Billboard");
            geometry.AxialRotationEnable = true; 


            id = Repository.GetNewName(typeof(Keystone.Appearance.Diffuse));
            Keystone.Appearance.Diffuse diffuse = new Keystone.Appearance.Diffuse(id);

            // NOTE: the texture needs to be rotated properly in a bitmap/png editor or via windows explorer right mouse click menu option
            string texturePath = @"caesar\textures\laser.png";
            Keystone.Appearance.Texture tex = (Keystone.Appearance.Texture)Keystone.Resource.Repository.Create(texturePath, "Texture");
            diffuse.AddChild(tex);
            

            // create alpha blended appearance 
            Keystone.Appearance.Appearance app = (Keystone.Appearance.DefaultAppearance)Repository.Create("DefaultAppearance");
            #region appearance for transparent billboards
            app.BlendingMode = CONST_TV_BLENDINGMODE.TV_BLEND_ADD;
            #endregion


            Keystone.Types.Color diffuseColor, ambient, specular, emissive;
            diffuseColor = new Keystone.Types.Color(1.0f, 1.0f, 0.5f, 1.0f);
            ambient = new Keystone.Types.Color(1.0f, 1.0f, 0.0f, 1.0f);
            specular = new Keystone.Types.Color(1.0f, 1.0f, 0.0f, 1.0f);
            emissive = new Keystone.Types.Color(1.0f, 1.0f, 0.0f, 1.0f);

            // TODO: create a shared material here using a name based on Material.GetDefaultName ();
            Keystone.Appearance.Material emissiveMaterial = Keystone.Appearance.Material.Create(Repository.GetNewName(typeof(Keystone.Appearance.Material)), diffuseColor, ambient, specular, emissive);
            emissiveMaterial.Opacity = 0.5f;


            app.AddChild(emissiveMaterial);
            app.AddChild(diffuse); // NOTE: Add diffuse AFTER the textures have been added to it for reqt Paging order

            id = Repository.GetNewName(typeof(Model));
            Model model = new Model(id);
            model.AddChild(geometry);
            model.AddChild(app);
  //          model.Rotation = new Quaternion(0d, 270d, 0); // NOTE: This rotation is used to create the override matrix that are needed for axial rotation Rendering and boundingbox calculation. 

            
            billboardEntity.AddChild(model);

            // temp hack - not really needed because we can elect to save prefab via the plugin
            // todo: this should use Save_Prefab command
            //Keystone.Scene.Scene.WriteEntity(billboardEntity, false);
            //KeyCommon.Messages.Prefab_Save save = new KeyCommon.Messages.Prefab_Save();

            // 0 ) Need an icon for lights that are pickable.  The pickable flag is only relevant for "arcade" mode

            // 0) interior.DirectionalLight should not be active unless in Floorplan View mode

            // 0) plugin needs editor for light colors, specular and attenuation for pontlights

            // 1) spawn the ai controlled vehicle for purposes of being a target

            // 2) update all test vehicle prefabs to use a barrel entity with a pointlight and beam weapon and muzzle flash

            // 3) get user controlled vehicle tactical to target and track and fire on enemy ship on command

            // 4) beam itself should be child of barrel because the ship is moving while firing and we need to keep the beam existing from barrrel muzzle

            // 5) damage and damage FX


            // todo: the actual laser billboard should be a sub-model of the barrel Entity which is itself a sub-model of the turret.
            //       the turret script should control the barrel orientation.
            //       The muzzle flash billboard should be a submodel of the barrel as well.
            //       But what about kinetic projectiles?  Why should they be any different?  
            //       Also, if laser is an Entity, then it can be spawned via a command from the server.  It doesn't need to be scripted.  The weapon (aka barrel) will determine stats and such.
            //       Also as an Entity, we can parent a pointlight to it.
            //       Muzzel flash can be a sub model.


            // todo: add a vibration animation here?  if would be better if Toolbox items were all prefabs i think and just loaded from disk rather than constructing the hierarchy here
            // todo: add a muzzle flash
            // todo: add a pointlight at muzzle position
            // todo: i dont know if we need a script.  we want things like "to hit" to be dice-rolled ahead of time, and "damage" and "heat" consumption to be done by the target.
            // todo: weapons statistics based on type of weapon and barrel length.  
            //       This is part of the game logic and rules and simulation. Seems it should exist server side exe though... I think the scripting API should handle most of this but not sure on specifics of implementation.
            //       Even logic like RoF should be verified server side or else the user can just edit the script to give themselves a higher To Hit, RoF and Damage and everything.  Server perhaps can send the user
            //       properties they can interact with where the client doesn't really use scripting at all.  It's like how we implement the Plugin.
            //       Server doesn't need to send HTML gui (assuming we continue down this path), only the data that client can plug into the HTML. Well, i can postpone to v2.0

            ActivateBrush(billboardEntity,
                Keystone.EditTools.PlacementTool.BRUSH_SINGLE_DROP,
               KeyCommon.Flags.EntityAttributes.None,
               KeyCommon.Flags.EntityAttributes.Background | KeyCommon.Flags.EntityAttributes.Light,
               false);
        }

        private void AddSphere(bool useInstancing)
        {
            ModeledEntity entity = new ModeledEntity(Repository.GetNewName(typeof(ModeledEntity)));
            Model model = new Model(Repository.GetNewName(typeof(Model)));
            model.UseInstancing = useInstancing;

            // todo: should display an input dialog to allow setting of the sphere primitive mesh's radius, slices and stacks
            Mesh3d mesh = Mesh3d.CreateSphere(10f, 15, 15, false);

            Keystone.Appearance.Appearance appearance = Keystone.Celestial.ProceduralHelper.CreateAppearance(CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED, null, null, null, null, null);

            //            Keystone.Types.Color diffuseColor, ambient, specular, emissive;
            //            diffuseColor = new Keystone.Types.Color(1.0f, 1.0f, 0.0f, 1.0f);
            //            ambient = new Keystone.Types.Color(1.0f, 1.0f, 0.0f, 1.0f);
            //            specular = new Keystone.Types.Color(1.0f, 1.0f, 0.0f, 1.0f);
            //            emissive = new Keystone.Types.Color(1.0f, 1.0f, 0.0f, 1.0f);
            //            Material material = Material.Create(Repository.GetNewName(typeof(Material)), diffuseColor, ambient, specular, emissive);

            Material material = Material.Create(Material.DefaultMaterials.gold);
            appearance.RemoveMaterial();
            appearance.AddChild(material);

            // NOTE: it's never necessary to add defines here since Model.UpdateShader() will handle that

            model.AddChild(appearance);
            model.AddChild(mesh);

            entity.AddChild(model);

            ActivateBrush(entity,
                Keystone.EditTools.PlacementTool.BRUSH_SINGLE_DROP,
               KeyCommon.Flags.EntityAttributes.None,
               KeyCommon.Flags.EntityAttributes.Background | KeyCommon.Flags.EntityAttributes.Light,
               false);
        }

        private void AddBox(bool useInstancing)
        {
            ModeledEntity entity = new ModeledEntity(Repository.GetNewName(typeof(ModeledEntity)));

            Model model = new Model(Repository.GetNewName(typeof(Model)));
            model.UseInstancing = useInstancing;
            Mesh3d mesh = Mesh3d.CreateBox(2.5f, 3f, 2.5f);

            Keystone.Appearance.Appearance appearance = Keystone.Celestial.ProceduralHelper.CreateAppearance(CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED, null, null, null, null, null);

            //Keystone.Types.Color diffuseColor, ambient, specular, emissive;
            //diffuseColor = new Keystone.Types.Color(1.0f, 1.0f, 0.0f, 1.0f);
            //ambient = new Keystone.Types.Color(1.0f, 1.0f, 0.0f, 1.0f);
            //specular = new Keystone.Types.Color(1.0f, 1.0f, 0.0f, 1.0f);
            //emissive = new Keystone.Types.Color(1.0f, 1.0f, 0.0f, 1.0f);
            //Material material = Material.Create(Repository.GetNewName(typeof(Material)), diffuseColor, ambient, specular, emissive);

            Material material = Material.Create(Material.DefaultMaterials.gold);
            appearance.RemoveMaterial();
            appearance.AddChild(material);

            // NOTE: it's never necessary to add defines here since Model.UpdateShader() will handle that

            model.AddChild(appearance);
            model.AddChild(mesh);

            entity.AddChild(model);
            entity.Dynamic = false;

            ActivateBrush(entity,
                Keystone.EditTools.PlacementTool.BRUSH_SINGLE_DROP,
               KeyCommon.Flags.EntityAttributes.None,
               KeyCommon.Flags.EntityAttributes.Background | KeyCommon.Flags.EntityAttributes.Light,
               false);
        }

        private void AddTeapot(bool useInstancing)
        {
            ModeledEntity entity = new ModeledEntity(Repository.GetNewName(typeof(ModeledEntity)));

            Model model = new Model(Repository.GetNewName(typeof(Model)));
            model.UseInstancing = useInstancing;
            Mesh3d mesh = Mesh3d.CreateTeapot();

            Keystone.Appearance.Appearance appearance = Keystone.Celestial.ProceduralHelper.CreateAppearance(CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED, null, null, null, null, null);

            Material diffuseMaterial = Material.Create(Material.DefaultMaterials.copper);
            appearance.RemoveMaterial();
            appearance.AddChild(diffuseMaterial);

            // NOTE: it's never necessary to add defines here since Model.UpdateShader() will handle that

            model.AddChild(appearance);
            model.AddChild(mesh);

            entity.AddChild(model);
            entity.Dynamic = false;

            ActivateBrush(entity,
               Keystone.EditTools.PlacementTool.BRUSH_SINGLE_DROP,
               KeyCommon.Flags.EntityAttributes.None,
               KeyCommon.Flags.EntityAttributes.Background | KeyCommon.Flags.EntityAttributes.Light,
               false);
        }

        private void AddCylinder(bool useInstancing)
        {
            ModeledEntity entity = new ModeledEntity(Repository.GetNewName(typeof(ModeledEntity)));

            Model model = new Model(Repository.GetNewName(typeof(Model)));
            model.UseInstancing = useInstancing;
            Mesh3d mesh = Mesh3d.CreateCylinder(10f, 20f, 60);

            Keystone.Appearance.Appearance appearance = Keystone.Celestial.ProceduralHelper.CreateAppearance(CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED, null, null, null, null, null);

            Material diffuseMaterial = Material.Create(Material.DefaultMaterials.iron);
            appearance.RemoveMaterial();
            appearance.AddChild(diffuseMaterial);

            // NOTE: it's never necessary to add defines here since Model.UpdateShader() will handle that

            model.AddChild(appearance);
            model.AddChild(mesh);

            entity.AddChild(model);
            entity.Dynamic = false;

            ActivateBrush(entity,
               Keystone.EditTools.PlacementTool.BRUSH_SINGLE_DROP,
               KeyCommon.Flags.EntityAttributes.None,
               KeyCommon.Flags.EntityAttributes.Background | KeyCommon.Flags.EntityAttributes.Light,
               false);
        }

        private void AddCone(Entity parent, Vector3d position)
        {

        }

        private void AddCapsule(Entity parent, Vector3d position)
        {

        }

        private void AddTorus(Entity parent, Vector3d position)
        {

        }

        private void AddPyramid3(Entity parent, Vector3d position)
        {

        }

        private void AddPyramid4(Entity parent, Vector3d position)
        {

        }

        private void AddCircle(Entity parent, Vector3d position)
        {
            const bool THICK_LINES = true;
            Keystone.Entities.ModeledEntity circleEntity = (ModeledEntity)Keystone.Resource.Repository.Create("ModeledEntity"); 
            circleEntity.Name = "circle";
            circleEntity.Dynamic = false;
            circleEntity.Pickable = false;
            circleEntity.Enable = true;
            //circleEntity.Visible = true; // TODO: why is this defaulting to false if i dont set this here?
            circleEntity.Overlay = true; // i think this should be false, overlay is for things that should be rendered over everything else and in this case, we still want items placed on the floorplan to be ontop of these markers
            //circleEntity.SetEntityAttributesValue((uint)KeyCommon.Flags.EntityAttributes.HUD, true);
            circleEntity.Translation = position;

            int numPoints = 10; // 360
            Model smoothCircleModel = Keystone.Celestial.ProceduralHelper.CreateSmoothCircle(numPoints, Keystone.Types.Color.Yellow, false, THICK_LINES);
            //smoothCircleModel.Scale = new Vector3d(100, 1, 100);
            circleEntity.Scale = new Vector3d(100, 1, 100);
            circleEntity.AddChild(smoothCircleModel);
            parent.AddChild(circleEntity);
            return;

            ActivateBrush(circleEntity,
                Keystone.EditTools.PlacementTool.BRUSH_SINGLE_DROP,
               KeyCommon.Flags.EntityAttributes.None,
               KeyCommon.Flags.EntityAttributes.Background | KeyCommon.Flags.EntityAttributes.Light,
               false);
        }

        #endregion // primitives 

        #region celestial

        private void AddStar(Keystone.Portals.Region region, Vector3d pos)
        {
            Keystone.Celestial.Star star = Keystone.Celestial.ProceduralHelper.CreateStar(pos);
            region.AddChild(star);
        }



        // TODO: all of these "primitives" and such can just be prefabs
        private Keystone.Celestial.StellarSystem AddSolSystem(Vector3d pos)
        {
            int seed = 1;
            Keystone.Celestial.StellarSystem system = Keystone.Celestial.ProceduralHelper.GenerateSolSystem(pos, seed);
            return system;
        }

        private void AddTerrestialPlanet(Keystone.Portals.Region region, Vector3d position)
        {

            const int J2000 = 2451545;

            //TODO: fix FileManager.SuspendWrite = true;
            const float EARTH_ORBITAL_RADIUS = 149598261000f;
            string name = Repository.GetNewName(typeof(Keystone.Celestial.World));
            Keystone.Celestial.World world = new Keystone.Celestial.World(name);
            world.Translation = position;
            //world.LatestStepTranslation = position; // not necessary if entity flag "dyanmic" == false
            world.Diameter = 12756300;
            world.MassKg = 5.9736E24; // required for gravity
            world.OrbitalRadius = EARTH_ORBITAL_RADIUS;
            world.OrbitalEccentricity = 0.01671123f; // Earth orbital period seconds (365.256363004 days)
            world.OrbitalPeriod = 365.256363004f * Keystone.Celestial.ProceduralHelper.DAYS_TO_SECONDS;
            world.OrbitalEpoch = (long)(DateTime.Today - Keystone.Utilities.TimeDateHelper.FromJulian(J2000)).TotalSeconds;
            world.AxialRotationRate = 30;// 30 seconds
            world.AxialTilt = 23.5f;
            world.Name = "Earth";
            Keystone.Celestial.ProceduralHelper.InitWorldVisuals(region, world, true, false, true, true);

            //CreateVehicle (region, world);
        }


        private void AddGasPlanet(Keystone.Portals.Region region, Vector3d position)
        {
            const double JUPITER_ORBITAL_RADIUS = 778547200000;
            //TODO: fix FileManager.SuspendWrite = true;
            string name = Repository.GetNewName(typeof(Keystone.Celestial.World));
            Keystone.Celestial.World world = new Keystone.Celestial.World(name);
            world.Translation = position;
            //world.LatestStepTranslation = position; // not necessary if entity flag "dyanmic" == false
            world.Diameter = 142984000;//jupiter equatorial diameter
            world.MassKg = 1.8986E27; // required for gravity
            world.OrbitalRadius = JUPITER_ORBITAL_RADIUS;
            world.OrbitalEccentricity = 0.048775f; // Jupiter orbital period seconds ( xx days)
            world.OrbitalPeriod = 433259f * Keystone.Celestial.ProceduralHelper.DAYS_TO_SECONDS;
            world.OrbitalInclination = (float)(1.35 * Keystone.Utilities.MathHelper.DEGREES_TO_RADIANS);
            //world.OrbitalEpoch = (long)(DateTime.Today - Keystone.Utilities.MathHelper.FromJulian(J2000)).TotalSeconds;
            world.AxialRotationRate = 30;// 30 seconds
            world.AxialTilt = 23.5f;

            Keystone.Celestial.ProceduralHelper.InitWorldVisuals(region, world, false, true, false, false);
        }

        private void AddMoon(Keystone.Portals.Region region, Vector3d position)
        {
            //TODO: fix FileManager.SuspendWrite = true;
            string id = Repository.GetNewName(typeof(Keystone.Celestial.World));
            Keystone.Celestial.World world = new Keystone.Celestial.World(id);
            world.Name = "Moon";
            world.Diameter = 3476000; //moon's diameter
            world.OrbitalRadius = 384399000; // meters semi-major axis
            world.Translation = position;
            //world.LatestStepTranslation = position; // not necessary if entity flag "dyanmic" == false
            world.MassKg = 7.3477E22; // reqt for gravity

            Keystone.Celestial.ProceduralHelper.InitWorldVisuals(region, world, true, false, false, false);

            //TODO: fix FileManager.WriteNewNode(_core.PrimaryCamera.Region, true);
            //TODO: fix FileManager.SuspendWrite = false;
        }

        private void AddPlanetoidField(Keystone.Portals.Region region, Vector3d position)
        {
            // TODO: perhaps rather than StaticEntity field should be an OctreeRegion
            //Keystone.Celestial.Planet field = new Keystone.Celestial.Planet(Repository.GetNewName(typeof(Keystone.Celestial.Planet)));
            // TODO: its really not reliable to pass in a box and then to try and create
            // an asteroid field that will be contained by that box... its best to have the 
            // Region's proper bounds computed after the field is generated if we want to ensure
            // all asteroids will be contained using the current InitAsteroidField field gen algorithm
            float fieldRadius = 10000;
            int asteroidCount = 1000;
            BoundingBox box = new BoundingBox(new Vector3d(), fieldRadius);
            // IMPORTANT:  Region's cannot be translated/scaled/rotated.  It's our fixed rule!
            // If we really want to add asteroids to a Region, then we must attach that region to 
            // a Container (just like with Vehicle).
            // NOTE: The above fact that region's are fixed also makes perfect sense for things like Octrees
            // because Octrees must be fixed too.  You would not want to have that moving each frame.
            //
            //Keystone.Entities.ModeledEntity field = new Keystone.Entities.ModeledEntity(Repository.GetNewName(typeof(Keystone.Portals.Region)), box, 6);

            Keystone.Entities.ModeledEntity field = new Keystone.Entities.ModeledEntity(Repository.GetNewName(typeof(Keystone.Entities.ModeledEntity)));

            //field.Diameter = fieldRadius;
            field.Dynamic = false;
            field.Translation = position;
            // field.LatestStepTranslation = position; // field is not dynamic
            Keystone.Celestial.ProceduralHelper.InitAsteroidField(field, fieldRadius, asteroidCount);
            region.AddChild(field);
        }

        // be nice if i could just select the FX and modify them after they've been created.  That way when i'm
        // hitting the various Generate buttons i dont have to deal with a box asking what radius, how many elements, etc
        // it can just start with a default and tehn you can modify it easily.
        private void AddStarfield(Keystone.Portals.Region region, Vector3d pos)
        {
            // for the starfield effect, we simply track which region the user is in, where any 
            // planetoid belts in that region are, and then determine if the player is in that field
            // by testing a min/max radius, and their height above and below the plane of that belt.
            // this is something easily done by frame. While player is in the field, we track
            // an array of these "particles" and maintain them in a bubble of x radius around the player

            int[] starCount = new int[] { 5000, 1000, 100 };
            int[] colors = new int[]{Keystone.Utilities.RandomHelper.RandomColor().ToInt32(),
                                     Keystone.Utilities.RandomHelper.RandomColor().ToInt32(),
                                     Keystone.Utilities.RandomHelper.RandomColor().ToInt32()};

            float variance = 1.0f;
            //float[] spriteSize = new float[] {500 + (500 * (float)rand.NextDouble()),
            //            500 + (500 * (float)rand.NextDouble()),
            //            500 + (500 * (float)rand.NextDouble())};
            float[] spriteSize = new float[] { 250, 500, 1000 };

            float radius = 90000;

            string[] texture = new string[] { @"caesar\Shaders\Planet\stardx7.png", @"caesar\Shaders\Planet\stardx7.png", @"caesar\Shaders\Planet\stardx7.png" }; // star2.dds";
            string fieldName = "starfield_" + Repository.GetNewName(typeof(Entity)); // "starfield1"  // random name means we should be able to produce more than one

            Entity field = Keystone.Celestial.ProceduralHelper.CreateRandomStarField(fieldName, texture, radius, starCount, spriteSize, colors);
            field.Name = "starfield";
            field.Translation = pos; // TODO: isn't this pos irrelevant as it follows camera?

            //TODO: cannot attach to viewpoint because our traverser doesn't go beyond viewpoint bounds
            // because a viewpoint has no bounds and fails visiblity test (so obviously children automatically get skipped)
            // I think attaching to viewpoint is bad idea in any case.
            //
            // - disable zbuffer writes when rendering skybox
            //
            // OPTIONS
            // - treat as FX that is renderingcontext/viewport window specific
            // - treat as an Entity that gets rendered first, but has render script to draw it
            //   at the origin always (since camera is at origin).  Perhaps a flag can be read
            //   which would say this items' x position, y position, z position are to equal their
            // respective counterparts.  You can disable the y position flag so only x and z are matched
            // and this would help with a skyhemisphere about a landscaped area
            // - WE CANNOT simply move it to origin because y position might not be desireable as mentioned
            //   above and because that y position can vary from rendering context to rendering context if
            //   the user has multiple viewports open.
            // - we're looking for elegance/flexibility/extensibility
            //
            // - an environment box is a hack really... since nothing visually rendered on it is represented
            //   by it's own entity.  So in a sense that kind of make's it a HUD with it's own special
            //   behavior.
            //
            // - how to serialize an FXEnvironmentalArea
            //
            //_core.PrimaryContext.Viewpoint.AddChild(field);
            region.AddChild(field);
        }

        private void AddStarfieldNoTexture(Keystone.Portals.Region region, Vector3d pos)
        {
            int[] colors = new int[]{Keystone.Utilities.RandomHelper.RandomColor().ToInt32(),
                                     Keystone.Utilities.RandomHelper.RandomColor().ToInt32(),
                                     Keystone.Utilities.RandomHelper.RandomColor().ToInt32()}; // CoreClient._CoreClient.Globals.RGBA(1f, 1f, 1f, 1);
            float variance = 1.0f;

            string[] texture = new string[] { "", "", "" };
            float[] spriteSize = new float[] { 0, 0, 0 };
            int[] starCount = new int[] { 1500, 1500, 1500 };
            float radius = 500;

            Entity field = Keystone.Celestial.ProceduralHelper.CreateRandomStarField("starfield2", texture, radius, starCount, spriteSize, colors);
            // NOTE: rather than set the translation, the field is added to the Viewpoint.
            //           field.Translation = _core.PrimaryContext.Position;

        }

        // new vehicle temp debug procedure to create one with interior (but no exterior)
        private void buttonNewComet_Click(object sender, EventArgs e)
        {

        }

        // particle system spawn test
        private void AddNebula(Entity parent, Vector3d position)
        {


        }

        //        private void mnuItemTestSmokeTrail_Click(object sender, EventArgs e)
        //        {
        //            // we need to load in a ship, and then be able to set a speed on it and have it move
        //            // right now the fields we require only exist in Player and NPC which implement ISTeerable
        //            // So maybe start by ImportPlayer 
        //            // allow us to give some basic movement commands to the ship to change it's course


        //            // load a smoke trail object and tie it to the selected ship such that new smoke segments in the circular queue
        //            // will follow the ship's position

        //            // the smoke should be able to turn on/off with engines (acceleration) and not just velocity

        //            // smoke trail length is a parameter along with fade rate
        //            // 

        private void AddMotionfield(Keystone.Portals.Region region, Vector3d pos)
        {
            // for the starfield effect, we simply track which region the user is in, where any 
            // planetoid belts in that region are, and then determine if the player is in that field
            // by testing a min/max radius, and their height above and below the plane of that belt.
            // this is something easily done by frame. While player is in the field, we track
            // an array of these "particles" and maintain them in a bubble of x radius around the player

            int particleCount = 2500;
            int color = Keystone.Utilities.RandomHelper.RandomColor().ToInt32();
            float spriteSize = .25f;

            string texture = @"caesar\Shaders\Planet\star2.dds";
            string fieldName = "motionfield_" + Repository.GetNewName(typeof(Entity)); // "motion field"  // random name means we should be able to produce more than one

            Entity field = Keystone.Celestial.ProceduralHelper.CreateMotionField(fieldName, Keystone.Celestial.ProceduralHelper.MOTION_FIELD_TYPE.SPRITE, particleCount, texture, spriteSize, color);
            field.Translation = Vector3d.Zero();

            region.AddChild(field);
        }

        private void buttonItemSkybox_Click(object sender, EventArgs e)
        {

        }
        #endregion // celestial

        #region FX
        private void AddExplosion(Entity parent, Vector3d position)
        {
            // AddExplosion() Creates a NON PARTICLE SYSTEM explosiong using HLSL SPRITE SHEET ANIMATION
            Keystone.Entities.ModeledEntity explosionEntity = new Keystone.Entities.ModeledEntity(Repository.GetNewName(typeof(Keystone.Entities.ModeledEntity)));
            explosionEntity.Name = "explosion";
            explosionEntity.Translation = position;

            // NOTE: we pass in model to InitExplosion() so that if we want to call multiple times
            // with different  models, we can fill up a ModelSelector with them
            Model model = new Model(Repository.GetNewName(typeof(Model)));

            Keystone.Celestial.ProceduralHelper.InitExplosion(explosionEntity, model, "explosion_animation0", "explosion_appearance0", 2f);
            explosionEntity.AddChild(model);


            // TODO: AddEntityToScene (explosion, parent); <-- how do we get .Play() to call afterwards?
            //       - i think obvious answer is to create a modified AddEntityToScene() that will expect an animation
            //       to "play" upon completion of the command
            parent.AddChild(explosionEntity);

            explosionEntity.Animations.Play(0);
        }

        private void AddParticleSystem(Entity parent, Vector3d position)
        {
            Keystone.Entities.ModeledEntity explosionEntity = new Keystone.Entities.ModeledEntity(Repository.GetNewName(typeof(Keystone.Entities.ModeledEntity)));
            explosionEntity.Name = "tvparticle";
            explosionEntity.Translation = position;

            // NOTE: we pass in model so that if we want to call multiple times
            // with different  models, we can fill up a ModelSelector with them
            Model model = new Model(Repository.GetNewName(typeof(Model)));

            // Creates a NON PARTICLE SYSTEM explosiong using HLSL SPRITE SHEET ANIMATION
            Keystone.Celestial.ProceduralHelper.InitiTVParticleSystem(model, "tvparticle_animation0", "tvparticle_appearance0", 2f);
            explosionEntity.AddChild(model);

            // TODO: AddEntityToScene (explosion, parent);
            parent.AddChild(explosionEntity);

            bool loop = true;
            if (explosionEntity.Animations != null)
                explosionEntity.Animations.Play(0, loop);
        }
        #endregion

        private void buttonCreateSpriteSheet_Click(object sender, EventArgs e)
        {
            if (AppMain._core == null || AppMain._core.Graphics.IsInitialized == false)
            {
                MessageBox.Show("3D Viewport must be initialized first...");
                return;
            }

            FormSpriteSheet form = new FormSpriteSheet();
            form.Show();
        }

        // TODO: i think our current system for importing is 
        // temp button to place a portal of hardcoded points in a Vehicle's region (so that it moves relative with the vehicle)
        // and which connects back to the root region
        private void AddPortal(Entity parent, Vector3d position, Vector3d direction)
        {
        }

        #endregion
    }
}
