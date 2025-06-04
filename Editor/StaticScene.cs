//using System;
//using System.Diagnostics;
//using Keystone;
//using Keystone.Appearance;
//using Keystone.Culling;
//using Keystone.Elements;
//using Keystone.Entities;
//using Keystone.FX;
//using Keystone.RenderSurfaces;
//using Keystone.Octree;
//using Keystone.Portals;
//using Keystone.Shaders;
//using Keystone.Sound;
//using Keystone.Traversers;
//using Keystone.Types;
//using MTV3D65;


//namespace KeyEdit
//{
//    public class StaticScene
//    {
//        // particles
//        private static TVParticleSystem ps;
//        private static TVParticleSystem _waterFall;
//
//        // terrain
//        private static Terrain terrain; // needed by input handler until we get a proper Scene.Picker
//        private const int WORLD_WIDTH = 4096;
//        private const int WORLD_HEIGHT = 4096;
//        private const int HALF_WORLD_WIDTH = WORLD_WIDTH / 2;
//        private const int HALF_WORLD_HEIGHT = WORLD_HEIGHT / 2;
//
//        // private static TerrainGridBuilder _terrainGridBuilder;
//        // used for mapping quadtree or chunk boundaries with surface lines for visualization
//
//        //quadtree testing related
//        private const int FEMALE_COUNT = 1;
//        private const int RANCOR_COUNT = 1;
//        private const int PAWN_COUNT = 20;
//        private const int HOUSE_COUNT = 0;
//        private const int IMPOSTERS_X = 32;
//        private const int IMPOSTERS_Y = 32; // 32x32 = 1024
//        private const int IMPOSTER_START_DISTANCE = 256;
//        private const RSResolution imposterResolution = RSResolution.R_1024x1024;
//
//        
//
//        // occlusion testing related
//        // http://www.truevision3d.com/forums/tv3d_sdk_65/prerelease_completion_progress_post-t5988.0.html;msg103388#msg103388
//        //
//        private static TVMesh _occluder;
//        private static int _transparentMat;
//        private static OcclusionCuller _occlusionCuller;
//        private static OcclusionFrustum _occlusionFrustum;
//        private static bool _sphereTest = true;
//        private static bool _planeTest = false;
//        private static bool _occlusionEnabled = true;
//
//        // temp for shadowmap
//        public static Keystone.Lights.PointLight _pointLight;
//        public static float lightRange = 200;
//
//        private static SoundNode sn;
//        
//        public static void Load(float farplane)
//        {
//            // TODO: Were do we handle FIleManager.Read() and the callbacks for paging data in/out?
//            //       Because below what we're doing is loading a hardcoded scene.  Surely I could specify
//            //       for the FileManager to load in a file directly right here... but instead we want to have the pager
//            //       do it.  But ideally if we had a start location, we'd maybe want to load first with a progress screen
//            //       til we get the bulk of stuff in then when that workItem is complete, we can actually start rendering...

//            Trace.WriteLine("Begin loading scene...");
//            // Begin to load Scene Elements.  Nearly all of the below with the exception of Controller related stuff
//            // and certain FX systems like Imposter (though blue planet and others would be dependant on scene design)
//            // can be loaded from file and not hardcoded as below.
//            Trace.Write("Loading lights...");
//            Light sunlight = InitLights();
//            Trace.WriteLine("Done.");
//            Trace.Write("Loading materials...");
//            InitMaterials(); // init materials before trying to load any mesh that references them
//            Trace.WriteLine("Done.");
//            Trace.Write("Loading atmosphere...");
//            InitAtmosphere(sunlight);
//            //InitAtmosphere2(farplane); // <-- tv basic sky box
//            //InitAtmosphere3();
//            Trace.WriteLine("Done.");
//            Trace.Write("Loading flog...");
//            InitFog(); //TODO: why does SkyScattering seem to require fog?
//            Trace.WriteLine("Done.");
//            //Trace.Write("Loading weather...");
//            //InitWeather(); // render after main scene and terrain to avoid clipping
//            //Trace.WriteLine("Done.");
//            Trace.Write("Loading shadowmap...");
//            InitShadowMap(); // must be inited before we can add land to it
//            Trace.WriteLine("Done.");
//            Trace.Write("Loading land shadowmap...");
//            InitLandShadowMap(sunlight);
//            Trace.WriteLine("Done.");
//            // shadowmapping requires splatting?
//            Trace.Write("Loading terrain...");
//            InitLandscape(true); //meh, landscaped used in GetTerrainHeight for player
//            Trace.WriteLine("Done.");
            

//            // TODO:fix this?  Player should not have to exist for us to create for instance an EditController.
//            //      but right now, Player must be init before Controllers
//            Trace.Write("Loading male player...");
//            InitMalePlayer();
//            Trace.WriteLine("Done.");
//            Trace.Write("Loading female player...");
//            InitFemalePlayer();
//            Trace.WriteLine("Done.");
//            Trace.Write("Loading female opponents...");
//            InitFemaleOpponents();
//            Trace.WriteLine("Done.");
//            Trace.Write("Loading Reznok...");
//            InitReznok();
//            Trace.WriteLine("Done.");
//            Trace.Write("Loading rancors...");
//            InitRancors(); // done after player for now because "_simulation.CurrentTarget" is used to track the player.
//            Trace.WriteLine("Done.");
//            Trace.Write("Loading water...");
//            InitWater(false); 
//            //InitZakWater2();  // NOTE: PostFX is screwing up water 
//            Trace.WriteLine("Done.");
//            //Trace.Write("Loading glow...");
//            //InitGlow();
//            //Trace.WriteLine("Done.");
//            //Trace.Write("Loading bloom...");
//            //InitBloom(); //requires sky renders first or huge artifacts ensue
//            //Trace.WriteLine("Done.");
//            // NOTE: ScreenSpaceApprox = ObjectWorldRadius / Distance;

//            //UENGINE - COPY THEIR GUI DESIGN
//            // http://www.neoaxisgroup.com/screenshots/indoorscenemanagement.jpg  <-- nexoaxis portals are good too
//            // http://www.neoaxisgroup.com/screenshots/outdoorportals.jpg
//            //http://unigine.com/screenshots/      <-- LOVE THEIR EDITOR!
//            //////
//        //    Trace.Write("Loading portals...");
//        //    InitPortals(AppMain._core.SceneManager.Root);
//        //    Trace.WriteLine("Done.");
//            Trace.Write("Loading minimeshes...");
//            InitMiniMeshes();
//            Trace.WriteLine("Done.");
//            Trace.Write("Loading imposter system...");
//            InitImposterSystem(IMPOSTER_START_DISTANCE);
//            Trace.WriteLine("Done.");
//            Trace.Write("Loading BlindSides room mesh...");
//            InitBlindsRoom();
//            Trace.WriteLine("Done.");
//            Trace.Write("Loading pawns...");
//            InitPawns();
//            Trace.WriteLine("Done.");
//            Trace.Write("Loading houses...");
//            InitHouses();
//            Trace.WriteLine("Done.");
//            Trace.Write("Loading trees...");
//            InitTrees();
//            Trace.WriteLine("Done.");
//           // InitBluePlanet();
//            //Trace.Write("Done.");
//            Trace.Write("Loading spacecraft...");
//            InitSpacecraft();
//            Trace.WriteLine("Done.");
//            Trace.Write("Loading occluders...");
//            InitOccluder();
//            Trace.WriteLine("Done.");
//            Trace.Write("Loading particle systems...");
//            InitializeParticleSystem();
//            Trace.Write("Done.");
//            //Trace.Write("Loading waterfall...");
//            //InitializeWaterfall();
//            //Trace.WriteLine("Done.");
//            //bool DepthofField = false;
//            //TVGraphicEffect DoF;
//            //if (DepthofField)
//            //{
//            //    DoF = new TVGraphicEffect();
//            //    DoF.SetDepthOfFieldParameters(5, 30, 3);
//            //}
//            Trace.Write("Loading sounds...");
//            InitSounds();
//            Trace.WriteLine("Done.");
//        }
//        private static void InitSounds()
//        {
//            string file = @"E:\dev\vb.net\test\DirectSoundDemo\bin\Debug\AudioMono.wav";
//            AppMain._core.AudioManager.Initialize(AppMain._core.Graphics.Handle);
//            sn = AppMain._core.AudioManager.CreateSoundNode(file);
//            AppMain._core.AudioManager.Listener.Translation = new TV_3DVECTOR(300, 45, 300);
//            sn.Translation = new TV_3DVECTOR(300, 45, 300);

//            // so let's now setup a sound with one of the rancors
//            // and 
//        }

//        private static Light InitLights()
//        {
//            Light.SetGlobalAmbient(0f, 0f, 0f);

//            // NOTE: regarding materials and light and ambient values 
//            //http://www.truevision3d.com/phpBB2/viewtopic.php?t=6802&highlight=setglobalambient

//            //TV_LIGHT dirLight = new TV_LIGHT();
//            Light sunlight = new Light("sunlight", new TV_3DVECTOR(0.5f, -0.5f, 0.8f), 1f, 1f, 1f, 1, 1, false);

//            sunlight.Ambient = new Color(1f, 1f, 1f, 1);
//            sunlight.Enable = true;
//            sunlight.ManagedLight = true; // we want to manage the lights ourself..
//            sunlight.CastShadows = true;
//            sunlight.UseForLightMapping = false;
//            //sunlight.range = 1000;
//            //sunlight.attenuation = new TV_3DVECTOR(0, 0, 0);
//            AppMain._core.Simulation.AddEntity(sunlight, new TV_3DVECTOR (0,0,0));

//            AppMain._core.Scene.SetShadowParameters(AppMain._core.Globals.RGBA(0, 0, 0, 0.5f), true);

//            Light.SetSpecularLighting(true);

//            float x, y, z;
//            x = -01;
//            y = 37;
//            z = 198;

//            //AppMain._camera.SetPosition(x, y, z); // doesnt have to start here, but why not.
//            _pointLight = new PointLight("shadow1", new Vector3d(x, y, z), 1, .7F, .4F, 1, lightRange, 1, true);

//            Diffuse glass = Diffuse.Create(AppMain._core.GetNewName(typeof(Diffuse)), @"Shaders\Shadow\glass.dds");
//            //_pointLight = new ProjectiveTexturePointLight("shadow1", new TV_3DVECTOR(x, y, z), 1, .7F, .4F, 1, lightRange, 1, glass);

//            _pointLight.Enable = true;
//            _pointLight.ManagedLight = true;
//            _pointLight.CastShadows = false;
//            _pointLight.UseForLightMapping = false;
//            AppMain._core.Simulation.AddEntity(_pointLight, new TV_3DVECTOR (x, y, z));

//            return sunlight;
//        }
//        private static void InitMaterials()
//        {
//            int matId;
//            //NOTE: these end up getting added because the AutoLoad materials
//            // during mesh/actor Import references these matterials and
//            // our Material objects get created to wrap those on load.
//            matId = AppMain._core.MaterialFactory.CreateMaterial("Matte"); // TODO: Im fairly certain that tv3d is loading a different copy of this
//            AppMain._core.MaterialFactory.SetAmbient(matId, .65f, .65f, .65f, 1);
//            AppMain._core.MaterialFactory.SetDiffuse(matId, 1, 1, 1, 1);
//            AppMain._core.MaterialFactory.SetSpecular(matId, 0, 0, 0, 1);
//            AppMain._core.MaterialFactory.SetEmissive(matId, 0, 0, 0, 1);
//            AppMain._core.MaterialFactory.SetOpacity(matId, 1);
//            AppMain._core.MaterialFactory.SetPower(matId, 0); // used to magnify the specular

//            for (int i = 22; i < 28; i++)
//            {
//                matId = AppMain._core.MaterialFactory.CreateMaterial("Material" + i); // used by the house
//                AppMain._core.MaterialFactory.SetAmbient(matId, .8f, .8f, .8f, 1);
//                AppMain._core.MaterialFactory.SetDiffuse(matId, 1, 1, 1, 1);
//                AppMain._core.MaterialFactory.SetSpecular(matId, 0, 0, 0, 1);
//                AppMain._core.MaterialFactory.SetEmissive(matId, 0, 0, 0, 1);
//                AppMain._core.MaterialFactory.SetOpacity(matId, 1);
//                AppMain._core.MaterialFactory.SetPower(matId, 0);
//            }
//            matId = AppMain._core.MaterialFactory.CreateMaterial("Shiny");
//            AppMain._core.MaterialFactory.SetAmbient(matId, 0.25f, 0.25f, 0.25f, 1);
//            AppMain._core.MaterialFactory.SetDiffuse(matId, 0.9f, 0.9f, 0.9f, 1);
//            AppMain._core.MaterialFactory.SetSpecular(matId, 1, 1, 1, 1);
//            AppMain._core.MaterialFactory.SetEmissive(matId, 0, 0, 0, 1);
//            AppMain._core.MaterialFactory.SetOpacity(matId, 1);
//            AppMain._core.MaterialFactory.SetPower(matId, 25);
//        }

//        //private static void InitLandscapeSQL (bool useSplatting)
//        //{
//        //    try
//        //    {
//        //        // this version will test us loading a complete Landscape from our DB
//        //        string connstring;
//        //        connstring = @"Data Source=C:\Documents and Settings\Hypnotron\My Documents\dev\vb.net\test\AGT3rdPersonTV3DDemo\Data\world\world.s3db";
//        //        //connstring = @"Data Source=\Data\world\world.s3db"; // not sure why this relative form of the path isn't working..

//        //        SQLiteConnection cnn = new SQLiteConnection(connstring);
//        //        cnn.Open();
//        //        // get a record for a terrain object.
//        //        string text = "select * from Terrains where name ='test3'";
//        //        using (SQLiteCommand cmd = SQLiteHelper.CreateCommand(cnn, text, null))
//        //        {

//        //            //text =
//        //            //    "insert into Terrains VALUES (-1700, 0, -1700, 1, 1, 1, 0, 0, 0, 10, 10, 'test0', 2, 'TRUE', 50, 0, 'TRUE', 2)";
//        //            IDataReader results = SQLiteHelper.ExecuteReader(cmd, text, null);
//        //            Debug.WriteLine(String.Format("PositionX = {0}, PositionY = {1}, PositionZ = {2}", results[0], results[1], results[2]));
//        //            //TODO: should the terrain object have a constructor where you can simply pass in the IDataReader 
//        //            // and have it instance properly?

//        //        }
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        Debug.WriteLine("InitLandscapeSQL() Error " + ex.Message);
//        //    }
//        //}


//        // TODO: UNTIL WE GET PROPER RS SHARING where only the closest chunks will have any shadowing, we must be careful with 
//        //       how many chunks we create
//        private static void InitLandscape(bool useSplatting)
//        {
//            string heightmap = @"Terrain\cry\cryterrain.bmp";  //@"Terrain\Isle2\isle2_hm.bmp"; //@"Terrain\Isle1\Heightmap.dds" ; note: we change to 16x16 chunks instead of 10x10 for isle
//            TV_3DVECTOR position = new TV_3DVECTOR(-HALF_WORLD_WIDTH, 0, -HALF_WORLD_WIDTH);
//            TV_3DVECTOR scale = new TV_3DVECTOR(1, 1, 1);
//            terrain =
//                    Terrain.Create("t1", heightmap, CONST_TV_LANDSCAPE_PRECISION.TV_PRECISION_HIGH, CONST_TV_LANDSCAPE_HEIGHTMAP.TV_HEIGHTMAP_ALLRGB,
//                                            CONST_TV_LANDSCAPE_AFFINE.TV_AFFINE_HIGH, 16, 16, position, scale, true);

//            // this version of the Create function auto computes the chunks needed to achieve a 1:1 pixel to vertex ratio
//            // note: careful because this can result in lots of chunks.
//            //terrain =
//            //        Terrain.Create("t1", heightmap, CONST_TV_LANDSCAPE_PRECISION.TV_PRECISION_HIGH , CONST_TV_LANDSCAPE_HEIGHTMAP.TV_HEIGHTMAP_ALLRGB,
//            //                                CONST_TV_LANDSCAPE_AFFINE.TV_AFFINE_HIGH , position, scale, true);

//            // TODO: This really should be read only and forced to be set at init?  Because
//            // once your LandShadowmap makes mesh assignments and use of the chunk dimensions it'd be a PITA to notify and change things. 
//            // terrain.Translation = new TV_3DVECTOR(- HALF_WORLD_WIDTH, 0, - HALF_WORLD_HEIGHT);
//            terrain.EnableLOD(true, terrain.ChunkWorldWidth, CONST_TV_LANDSCAPE_PRECISION.TV_PRECISION_ULTRA_LOW, 512, false);
//            //  terrain.SetProgressiveLOD (true);

//            Debug.WriteLine(string.Format("Landscape World Dimensions x={0}, z={1}", terrain.WorldWidth, terrain.WorldHeight));

//            Appearance app;
//            GroupAttribute group = new GroupAttribute(AppMain._core.GetNewName(typeof(GroupAttribute)));

//            if (useSplatting)
//            {
//                app = new SplatAppearance("SplatAppearance1");
//                app.LightingMode = CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED;

//                //TODO: although we can have a single diffuse added as the first and only base texture
//                // which doesnt require an alpha texture, for this test code we dont intend that
//                // so we dont add it to the group.  Need a better way to handle these
//                Texture diffuse =
//                    ImportLib.Import(@"Terrain\common\riverdalegrass.jpg", CONST_TV_LAYER.TV_LAYER_0,
//                                     CONST_TV_TEXTURETYPE.TV_TEXTURE_DIFFUSEMAP);
//                //group.AddChild(diffuse);

//                SplatAlpha alpha = SplatAlpha.Create(AppMain._core.GetNewName(typeof(SplatAlpha)), @"Terrain\Isle2\isle2_alphas_Alpha1.png");
//                alpha.TileU = 20; // note: this always tiles the diffuse.  note the tiling is per chunk
//                alpha.TileV = 20;
//                alpha.baseTextureIndex = diffuse.TextureIndex;
//                alpha.ChunksX = terrain.ChunkWidth;
//                alpha.ChunksY = terrain.ChunkHeight;
//                group.AddChild(alpha);

//                diffuse =
//                    ImportLib.Import(@"Terrain\common\riverdalegrassdead.jpg", CONST_TV_LAYER.TV_LAYER_0,
//                                     CONST_TV_TEXTURETYPE.TV_TEXTURE_DIFFUSEMAP);
//                //group.AddChild(diffuse);

//                alpha = SplatAlpha.Create(AppMain._core.GetNewName(typeof(SplatAlpha)), @"Terrain\Isle2\isle2_alphas_Alpha2.png");
//                alpha.TileU = 5;
//                alpha.TileV = 5;
//                alpha.baseTextureIndex = diffuse.TextureIndex;
//                alpha.ChunksX = terrain.ChunkWidth;
//                alpha.ChunksY = terrain.ChunkHeight;
//                group.AddChild(alpha);
//            }
//            else
//            {
//                // here we should create our texture sets and such?  expand texture determines which chunks the texture is expanded to cover
//                // i think our code will have to know in advance because its only a single call that effects all landscapes...
//                // however it can be called singularly for a given chunk which is i think how we'll most use it since
//                // we'll typically have seperate textures per chunk, or in the case of splatting, its just tiled anyway..
//                // unfortunately, i dont think detailTexture can be used with splatting...
//                app = new DefaultAppearance("DefaultAppearanceLandScape");
//                app.LightingMode = CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED;


//                Texture diffuse =
//                    ImportLib.Import(@"Terrain\Isle1\isle1_Tex.dds", CONST_TV_LAYER.TV_LAYER_0,
//                                     CONST_TV_TEXTURETYPE.TV_TEXTURE_DIFFUSEMAP);
//                group.AddChild(diffuse);

//                Texture detail =
//                    ImportLib.Import(@"Terrain\Isle1\isle1_Detail.dds", CONST_TV_LAYER.TV_LAYER_1,
//                                     CONST_TV_TEXTURETYPE.TV_TEXTURE_DIFFUSEMAP);
//                detail.TileU = 15;
//                detail.TileV = 15;
//                group.AddChild(detail);
//                Texture lightmap =
//                    ImportLib.Import(@"Terrain\Isle1\isle1_Lmap.dds", CONST_TV_LAYER.TV_LAYER_2,
//                                     CONST_TV_TEXTURETYPE.TV_TEXTURE_DIFFUSEMAP);
//                group.AddChild(lightmap);
//            }

//            // i think when you're using a lightmap you wouldnt set a material?
//            Material mat = Material.Create(AppMain._core.Globals.GetMat("Matte"));
//            app.AddChild(mat); // since we want to share the same material for all groups, we add it to the Appearance and not the group!
//            app.AddChild(group);
//            terrain.AddChild(app);

//            // we want to place the terrain such that the terrain's center is also at the octree's center
//            // TODO: but how then do we handle adding more landscapes?
//            // well the key is that the terrains MUST be contained fully within the bounds of the octree
//            // previously when we had the culling issues its because our octree was too small 
//            // so actually i dont think below is an issue.  Instead we should add a check
//            // in the root.Inject(node) and make sure the bounds of the item to be added is contained in the
//            // octree.  
//            // however, how does this impact roaming origin?  consider vast space games?  No, this is only something done during render.  
//            StaticEntity terrainEntity = new StaticEntity(terrain.Name, terrain);
//            AppMain._core.Simulation.AddEntity(terrainEntity, terrain.Translation );

//            if (_landShadowMap != null) terrain.Subscribe((_landShadowMap));
//            //TODO: temp hack code to create our quadtree terrain following grid
//            // if we knew it would never change we could generate a mesh
//            // only then we'd have to draw it wireframe and you'd get those nasty triangles instead of squares
//            // other option then would be to texture the top as Steve was saying.  That'd work well for chunks but
//            // its more complicated since then you have to manage another texture and picking gets complicated trying to click
//            // through this mesh

//            // so i guess we need a grid generation traverser?  and how do we store
//            // all these so that we only render the ones visible each frame yet dont end up
//            // regenerating when we dont need to?  (and what about only gen'ing for quadtree segments that get changed
//            // instead of the entire landscape or chunk?
//            //_terrainGridBuilder = new TerrainGridBuilder(terrain);
//            //_terrainGridBuilder.Apply( AppMain._core.SceneManager.Root);

//        }



//        //private static void InitPortals(Region root)
//        //{
//        //    DefaultAppearance app;
//        //    GroupAttribute ga;
//        //    Mesh3d tunnel = Mesh3d.Create("Tunnel", @"Meshes\Misc\tunnel.TVM", true, false, out app);

//        //    Material mat = Material.Create(AppMain._core.Globals.GetMat("Matte"));
//        //    ModelBase model = new SimpleModel("tunnel1");
//        //    model.Scale = new TV_3DVECTOR(1, 1, 1);
//        //    model.Translation = new TV_3DVECTOR(300, 35f, 300);
//        //    model.Rotation = new TV_3DVECTOR(0, 0, 0);

//        //    app.AddChild(mat);
//        //    model.AddChild(tunnel);
//        //    model.AddChild(app);
//        //    if (_shadowmap != null) model.Subscribe(_shadowmap);

//        //    Region sector2 = Region.Create(AppMain._core.GetNewName(typeof(Region)), model.BoundingBox);
//        //    AppMain._core.SceneManager.Add(sector2);
//        //    sector2.AddChild(model);


//        //    model = new SimpleModel("tunnel2");
//        //    model.Scale = new TV_3DVECTOR(1, 1, 1);
//        //    model.Rotation = new TV_3DVECTOR(0, -180, 0);
//        //    model.Translation = new TV_3DVECTOR(1180, 35f, 390);

//        //    model.AddChild(tunnel);
//        //    model.AddChild(app);
//        //    if (_shadowmap != null) model.Subscribe(_shadowmap);

//        //    Sector sector1 = Sector.Create(AppMain._core.GetNewName(typeof(Sector)), model.BoundingBox);
//        //    AppMain._core.SceneManager.Add(sector1);
//        //    sector1.AddChild(model);

//        //    // now we need to add two 1-way portals connecting our root sector to the entrance to sector1  
//        //    TV_3DVECTOR[] coords2 = new TV_3DVECTOR[4];
//        //    coords2[0] = new TV_3DVECTOR(830, 100, 745);
//        //    coords2[1] = new TV_3DVECTOR(645, 100, 745);
//        //    coords2[2] = new TV_3DVECTOR(645, -35, 745);
//        //    coords2[3] = new TV_3DVECTOR(830, -35, 745);

//        //    // find the deepest OctreeNode that contains all coords of this portal and use that node as the "location" for portal in the Octree
//        //    Locator locator = new Locator();
//        //    ISector deepestOctreeNode = locator.Search((OctreeNode)root, coords2);
//        //    Portal p = new Portal(AppMain._core.GetNewName(typeof(Portal)), deepestOctreeNode, sector1, coords2);
//        //    deepestOctreeNode.AddPortal(p);
//        //    // for the two way effect from inside to looking out, we create a new portal
//        //    // only we always use the Root OctreeNode as the destination so our culling/traversals can always start at root
//        //    TV_3DVECTOR[] coords4 = new TV_3DVECTOR[4];
//        //    coords4[0] = new TV_3DVECTOR(830, 100, 745); // 0 the reverse order from previous
//        //    coords4[1] = new TV_3DVECTOR(830, -35, 745); // 3
//        //    coords4[2] = new TV_3DVECTOR(645, -35, 745); // 2
//        //    coords4[3] = new TV_3DVECTOR(645, 100, 745); // 1
//        //    p = new Portal(AppMain._core.GetNewName(typeof(Portal)), sector1, root, coords4);
//        //    sector1.AddPortal(p);


//        //    // and then add 2 portals connecting sector1 to sector2 
//        //    TV_3DVECTOR[] coords = new TV_3DVECTOR[4];
//        //    coords[0] = new TV_3DVECTOR(830, 100, 345);
//        //    coords[1] = new TV_3DVECTOR(645, 100, 345);
//        //    coords[2] = new TV_3DVECTOR(645, -35, 345);
//        //    coords[3] = new TV_3DVECTOR(830, -35, 345);
//        //    p = new Portal(AppMain._core.GetNewName(typeof(Portal)), sector1, sector2, coords);
//        //    sector1.AddPortal(p);
//        //    // since these are one way portals, we need to use two to get a two-way effect between rooms
//        //    // however its critical that the coords are reversed properly else we will recursively try to render between the two rooms
//        //    // since the backface test will fail
//        //    TV_3DVECTOR[] coords3 = new TV_3DVECTOR[4];
//        //    coords3[0] = new TV_3DVECTOR(830, 100, 345); // 0 the reverse order from previous
//        //    coords3[1] = new TV_3DVECTOR(830, -35, 345); // 3
//        //    coords3[2] = new TV_3DVECTOR(645, -35, 345); // 2
//        //    coords3[3] = new TV_3DVECTOR(645, 100, 345); // 1

//        //    p = new Portal(AppMain._core.GetNewName(typeof(Portal)), sector2, sector1, coords3);
//        //    sector2.AddPortal(p);



//        //    // now we need to start the camera in one of these sectors
//        //    // NOTE: from within and no portals to the main sector, we should not be rendering any terrain or anything outside

//        //    // verify toggle key to disable traversal of portals, that the opposite sectors turns on/off 

//        //    // that's the gyst, add some stats for elements rendered and portals found and sectors rendered.

//        //    // and the camera's current sector

//        //}

//        private static void InitMiniMeshes()
//        {



//        }

//        static FXImposters _imposterSystem;
//        private static void InitImposterSystem(float defaultStartDistance)
//        {
//            _imposterSystem = new FXImposters(imposterResolution, IMPOSTERS_X, IMPOSTERS_Y, defaultStartDistance);
//            AppMain._core.SceneManager.Add(_imposterSystem);
//            AppMain._core.RegisterForDeviceResetNotification(_imposterSystem);
//        }

//        private static void InitTrees()
//        {
//            //string file = @"Meshes\flora\tropical\palmt5.tvm";
//        }

//        private static void InitHouses()
//        {
//            float x, y, z;
//            DefaultAppearance app;
//            GroupAttribute ga;
//            Material mat;

//            string file = @"Meshes\Buildings\Human_building_2st.TVM";
//            Mesh3d house = Mesh3d.Create("Mesh3d_House", file, true, false, out app);
//            float scale = 7f / house.BoundingSphere.Radius; // 7 meter tall houses

//            if (app != null && app.Groups != null)
//            {
//                for (int i = 0; i < app.Groups.Length; i++)
//                {
//                    if (app.Groups[i].Material != null)
//                    {
//                        int matId = app.Groups[i].Material.TVIndex;
//                        AppMain._core.MaterialFactory.SetAmbient(matId, 0f, 0f, 0f, 1);
//                        AppMain._core.MaterialFactory.SetDiffuse(matId, .7f, .7f, .7f, 1);
//                        AppMain._core.MaterialFactory.SetSpecular(matId, 0, 0, 0, 1);
//                        AppMain._core.MaterialFactory.SetEmissive(matId, 0, 0, 0, 1);
//                        AppMain._core.MaterialFactory.SetOpacity(matId, 1);
//                        AppMain._core.MaterialFactory.SetPower(matId, 0);
//                    }
//                }
//            }

//            // house.ComputeNormals();
//            //LoadLayers(house);
//            //house.SetShadowCast(false, false);
//            //house.SetTextureEx(2, _normalMap);

//            // NOTE: when reading/paing the XML stuff, when we get to a Mesh3d 
//            // we're calling ensure we instant the mesh from the path first and then call the proper
//            // constructor mesh = new Mesh3d(tvmesh)
//            SimpleModel model = new SimpleModel("SimpleModel_House");
//            model.Scale = new TV_3DVECTOR(scale, scale, scale);
//            model.Translation = new TV_3DVECTOR(100, 35f, 200);

//            mat = Material.Create(AppMain._core.Globals.GetMat("Matte"));
//            if (app == null) app = new DefaultAppearance("DefaultAppearance_House");
//            app.LightingMode = CONST_TV_LIGHTINGMODE.TV_LIGHTING_BUMPMAPPING_TANGENTSPACE;
//            //NOTE: if we've loaded this mesh with its Textures, then this particular mesh has lots of groups already.
//            //If they each have a material we also had load, then we have to itterate through all to replace.
//            //However, we can set a "DefaultMaterial" in the Appearance which can be applied to each group that doesnt
//            // already have its own set.
//            app.AddChild(mat);
//            model.AddChild(house);
//            model.AddChild(app);
//            StaticEntity sn = new StaticEntity(model.Name, model);
//            AppMain._core.Simulation.AddEntity(sn, model.Translation);
//            if (_landShadowMap != null) model.Subscribe(_landShadowMap);
//            if (_shadowmap != null) model.Subscribe(_shadowmap);


//            SimpleModel simplemodel;
//            for (int i = 0; i < HOUSE_COUNT; i++)
//            {
//                // duplicate is automatically used whenever the name of the mesh already exists
//                // NOTE: we go through Create because its the only way to get the automatic Resource reference tracking
//                // TODO: this resource reference tracking is thus very fragile and really is flawed design as its prone to user error.
//                DefaultAppearance dummy;
//                house = Mesh3d.Create("Mesh3d_House", file, true, true, out dummy);

//                x = AppMain._core.Random.Next(-1500, -1100);
//                z = AppMain._core.Random.Next(-1400, -500);
//                simplemodel = new SimpleModel("SimpleModel_House" + (i + 1));
//                simplemodel.Translation = new TV_3DVECTOR(x, terrain.GetHeight(x, z), z);
//                simplemodel.Scale = new TV_3DVECTOR(scale, scale, scale);
//                simplemodel.Rotation = new TV_3DVECTOR();
//                simplemodel.AddChild(house);
//                simplemodel.AddChild(app); //note we share the original appearance
//                sn = new StaticEntity(simplemodel.Name, simplemodel);
//                AppMain._core.Simulation.AddEntity(sn, model.Translation );
//                if (_landShadowMap != null) simplemodel.Subscribe(_landShadowMap);
//                if (_imposterSystem != null) simplemodel.Subscribe(_imposterSystem);
//            }
//        }

//        private static void InitBlindsRoom()
//        {

//            float x, y, z;
//            DefaultAppearance app;
//            GroupAttribute ga;
//            Material mat;

//            Mesh3d room = Mesh3d.Create("BlindsRoom_Mesh", @"Meshes\Buildings\Room4\room4.X", true, false, out app);




//            float minimumDemension = Math.Min(room.BoundingBox.Height, room.BoundingBox.Width);
//            minimumDemension = Math.Min(room.BoundingBox.Depth, minimumDemension);
//            float scale = 6f / minimumDemension; // 6 meter tall room

//            if (app != null && app.Groups != null)
//            {
//                for (int i = 0; i < app.Groups.Length; i++)
//                {
//                    if (app.Groups[i].Material != null)
//                    {
//                        int matId = app.Groups[i].Material.TVIndex;
//                        AppMain._core.MaterialFactory.SetAmbient(matId, 0f, 0f, 0f, 1);
//                        AppMain._core.MaterialFactory.SetDiffuse(matId, .7f, .7f, .7f, 1);
//                        AppMain._core.MaterialFactory.SetSpecular(matId, 0, 0, 0, 1);
//                        AppMain._core.MaterialFactory.SetEmissive(matId, 0, 0, 0, 1);
//                        AppMain._core.MaterialFactory.SetOpacity(matId, 1);
//                        AppMain._core.MaterialFactory.SetPower(matId, 0);
//                    }
//                }
//            }


//            // NOTE: when reading/paing the XML stuff, when we get to a Mesh3d 
//            // we're calling ensure we instant the mesh from the path first and then call the proper
//            // constructor mesh = new Mesh3d(tvmesh)
//            SimpleModel model = new SimpleModel("BlindsRoom_SimpleModel");
//            model.Scale = new TV_3DVECTOR(scale, scale, scale); // for blind's indoor room.x
//            model.Translation = new TV_3DVECTOR(0, 35f, 200);
//            model.Rotation = new TV_3DVECTOR(-90, 0, 0);

//            mat = Material.Create(AppMain._core.Globals.GetMat("Matte"));
//            if (app == null) app = new DefaultAppearance("DefaultAppearance_BlindsRoom");
//            app.LightingMode = CONST_TV_LIGHTINGMODE.TV_LIGHTING_BUMPMAPPING_TANGENTSPACE;
//            //NOTE: if we've loaded this mesh with its Textures, then this particular mesh has lots of groups already.
//            //If they each have a material we also had load, then we have to itterate through all to replace.
//            //However, we can set a "DefaultMaterial" in the Appearance which can be applied to each group that doesnt
//            // already have its own set.
//            app.AddChild(mat);
//            model.AddChild(room);
//            model.AddChild(app);
//            StaticEntity sn = new StaticEntity(model.Name, model);
//            AppMain._core.Simulation.AddEntity(sn, model.Translation );
//            if (_landShadowMap != null) model.Subscribe(_landShadowMap);
//            if (_shadowmap != null) model.Subscribe(_shadowmap);


//        }

//        private static void ImportMesh(string file, string key)
//        {
//            DefaultAppearance app;
//            Mesh3d mesh = Mesh3d.Create(key, file, true, true, out app);

//            if (app == null) app = new DefaultAppearance(AppMain._core.GetNewName(typeof(DefaultAppearance)));

//            SimpleModel model = new SimpleModel(AppMain._core.GetNewName (typeof(SimpleModel)));
//            model.AddChild(mesh);
//            model.AddChild(app);
//            StaticEntity sn = new StaticEntity(model.Name, model);
//            AppMain._core.Simulation.AddEntity(sn, model.Translation );

//            if (_landShadowMap != null) model.Subscribe(_landShadowMap);
//            if (_shadowmap != null) model.Subscribe(_shadowmap);
//            if (_imposterSystem != null) model.Subscribe(_imposterSystem);
//        }

//        private static void InitPawns()
//        {
//            float x, y, z;
//            DefaultAppearance app;

//            string file = @"Meshes\Misc\pawn.tvm";
//            file = "Meshes\\Proteus-MKII\\Proteus-MKII.TVM";
//            Mesh3d pawn = Mesh3d.Create("Pawn", file, true, true, out app);
//            float scale = 2; // 3f / pawn.BoundingSphere.Radius; // three meter tall pawns!

//            //pawn.SetMaterial(Core.Globals.GetMat("Shiny"), 0);
//            x = 125;
//            z = 220;
//            if (app == null) app = new DefaultAppearance("DefaultAppearance_Pawn");
//          //  app.LightingMode = CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED;
//            SimpleModel model = new SimpleModel("SimpleModelPawn");
//            model.AddChild(pawn);
//            model.AddChild(app);
//            model.Translation = new TV_3DVECTOR(x, terrain.GetHeight(x, z), z);
//            model.Scale = new TV_3DVECTOR(scale, scale, scale);
//            model.Rotation = new TV_3DVECTOR(0, 0, 0);
//            AppMain._core.Simulation.AddEntity(new StaticEntity(model.Name,model),model.Translation );

//            if (_landShadowMap != null) model.Subscribe(_landShadowMap);
//            if (_shadowmap != null) model.Subscribe(_shadowmap);
//            if (_imposterSystem != null) model.Subscribe(_imposterSystem);

            
//            SimpleModel simplemodel;
//            for (int i = 0; i < PAWN_COUNT; i++)
//            {
//                // duplicate is automatically used whenever the name of the mesh already exists
//                // NOTE: we go through Create because its the only way to get the automatic Resource reference tracking
//                // TODO: this resource reference tracking is thus very fragile and really is flawed design as its prone to user error.
//                DefaultAppearance dummy;
//                pawn = Mesh3d.Create("Pawn", file, true, true, out dummy);

//                x = AppMain._core.Random.Next(-1500, -1100);
//                z = AppMain._core.Random.Next(-1400, -500);
//                simplemodel = new SimpleModel("SimpleModel_Pawn" + (i + 1));
//                simplemodel.Translation = new TV_3DVECTOR(x, terrain.GetHeight(x, z), z);
//                simplemodel.Scale = new TV_3DVECTOR(scale, scale, scale);
//                simplemodel.Rotation = new TV_3DVECTOR();
//                simplemodel.AddChild(pawn);
//                simplemodel.AddChild(app); //note we share the original appearance
//                AppMain._core.Simulation.AddEntity(new StaticEntity(simplemodel.Name, simplemodel), simplemodel.Translation );
//                if (_landShadowMap != null) simplemodel.Subscribe(_landShadowMap);
//                if (_imposterSystem != null) simplemodel.Subscribe(_imposterSystem);
//            }
//        }

//        private static void InitRancors()
//        {
//            DefaultAppearance app;
//            float x, y, z;
//            ////////////////////////////////////////////
//            // NPC's
//            ////////////////////////////////////////////
//            if (RANCOR_COUNT > 0)
//            {
//                //file = @"Actors\LuzArius\high-runidle.tva";
//                string file = @"Actors\rancor\rancor_final.tva";
//                //file = @"Actors\fps_female\fps_female_multitrack.tva";
//                TVActor tvactor;
//                Actor3d rancor = Actor3d.Create("Actor3d_Rancor", file, CONST_TV_ACTORMODE.TV_ACTORMODE_SHADER, true, true, out app, out tvactor);
//                float scale = 5.0f / rancor.BoundingSphere.Radius; // rancor scale to be 5 meters tall

//                //LODSwitch rancorSwitch = new LODSwitch();
//                //id = rancor.AddAnimationRange(0, "running", 0, 46); // only for LuzARius warped actor
//                //id = rancor.AddAnimationRange(0, "idle", 47, 47); // only for LuzARius warped actor
//                rancor.ComputeNormals(); // should always be done before lighting mode
//                //rancor.Appearance.LightingMode = CONST_TV_LIGHTINGMODE.TV_LIGHTING_BUMPMAPPING_TANGENTSPACE;
//                //rancor.Appearance.Apply(rancor); // TODO: Isdirtyflag needs to propogate and trigger Apply on update loop
//                // NOTE: when using Stencils and in Shader MODE the rancor looks just like in Shadowmapping, but in cpu mode it looks like crap
//                rancor.PlayAnimation(.3f);
//                rancor.Update();
//                //app.AddChild(matte);
//                app.LightingMode = CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED;
//                BonedModel model = new BonedModel("BonedModel_Rancor", tvactor);
//                model.AnimationID = 1;
//                model.AddChild(app);
//                //rancorSwitch.AddChild(rancor);

//                // LOW LOD BLOCK
//                // low lod file = @"Actors\LuzArius\high-runidle.tva"; //@"Actors\rancor\rancor_final.tva"; 
//                // rancor = Actor3d.Create(@"Actors\LuzArius\low-runidle.tva", CONST_TV_ACTORMODE.TV_ACTORMODE_SHADER, true,true, out app);
//                //id = rancor.AddAnimationRange(0, "running", 0, 46); // only for LuzARius warped actor
//                //id = rancor.AddAnimationRange(0, "idle", 47, 47); // only for LuzARius warped actor
//                // END LOW LOD BLOCK
//                rancor.ComputeNormals(); // should always be done before lighting mode
//                //rancor.Appearance.LightingMode = CONST_TV_LIGHTINGMODE.TV_LIGHTING_BUMPMAPPING_TANGENTSPACE;
//                //rancor.Appearance.Apply(rancor); // TODO: Isdirtyflag needs to propogate and trigger Apply on update loop
//                // NOTE: when using Stencils and in Shader MODE the rancor looks just like in Shadowmapping, but in cpu mode it looks like crap
//                rancor.PlayAnimation(0);
//                // LOW LOD BLOCK
//                //rancorSwitch.AddChild(rancor);
//                //model.AddChild(rancorSwitch);
//                // END LOW LOD BLOCK
//                model.AddChild(rancor);
//                x = 500; // 1072;
//                z = 570;
//                model.Translation = new TV_3DVECTOR(x, 25, z); //terrain.GetHeight(x, z)

//                model.Scale = new TV_3DVECTOR(scale, scale, scale);
//                // model.Rotation = new TV_3DVECTOR(0, 180, 0);
//       //         AppMain._core.SceneManager.Add(model);
//                AppMain._core.Simulation.AddEntity(new NPC(model, AppMain._core.Simulation.CurrentTarget), model.Translation );


//                for (int i = 1; i < RANCOR_COUNT; i++)
//                {
//                    // duplicate is automatically used whenever the name of the mesh already exists
//                    // NOTE: we go through Create because its the only way to get the automatic Resource reference tracking
//                    // TODO: this resource reference tracking is thus very fragile and really is flawed design as its prone to user error.
//                    DefaultAppearance dummy;
//                    rancor = Actor3d.Create("Actor3d_Rancor", file, CONST_TV_ACTORMODE.TV_ACTORMODE_SHADER, true,
//                                            true, out dummy, out tvactor);

//                    x = AppMain._core.Random.Next(-1500, -1100);
//                    z = AppMain._core.Random.Next(-1400, -500);
//                    int animation = AppMain._core.Random.Next(1, 2);
//                    model = new BonedModel("BonedModel_Rancor" + (i + 1), tvactor);
//                    model.AnimationID = animation;
//                    model.Translation = new TV_3DVECTOR(x, terrain.GetHeight(x, z), z);
//                    model.Scale = new TV_3DVECTOR(scale, scale, scale);
//                    model.Rotation = new TV_3DVECTOR();

//                    model.AddChild(rancor);
//                    model.AddChild(app); //note we share the original appearance
//     //               AppMain._core.SceneManager.Add(model, true);
//                    AppMain._core.Simulation.AddEntity(new NPC(model, AppMain._core.Simulation.CurrentTarget), model.Translation );
//                }
//            }
//        }

//        private static void InitFemaleOpponents()
//        {
//            float x, y, z;
//            DefaultAppearance app;
//            string file = @"Actors\fps_female\fps_female_multitrack.tva";

//            TVActor tvactor;
//            Actor3d fpsfemale = Actor3d.Create("Actor3d_FPS_Female", file, CONST_TV_ACTORMODE.TV_ACTORMODE_SHADER, true, true, out app, out tvactor);
//            fpsfemale.ComputeNormals(); // should always be done before lighting mode
//            //fpsfemale.Appearance.LightingMode = CONST_TV_LIGHTINGMODE.TV_LIGHTING_BUMPMAPPING_TANGENTSPACE;
//            //fpsfemale.Appearance.Apply(fpsfemale); // TODO: Isdirtyflag needs to propogate and trigger Apply on update loop
//            // NOTE: when using Stencils and in Shader MODE the fpsfemale looks just like in Shadowmapping, but in cpu mode it looks like crap
//            fpsfemale.PlayAnimation(.3f);
//            fpsfemale.Update();
//            //app.AddChild(matte);
//            app.LightingMode = CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED;
//            float scale = 1.75f / fpsfemale.BoundingSphere.Radius; // compute her scale to the desired height of 5'9"

//            for (int i = 0; i < FEMALE_COUNT; i++)
//            {
//                // duplicate is automatically used whenever the name of the mesh already exists
//                // NOTE: we go through Create because its the only way to get the automatic Resource reference tracking
//                // TODO: this resource reference tracking is thus very fragile and really is flawed design as its prone to user error.
//                //       note that for Actor3d it's imperative we call Create() instead of just passing an existing loaded Actor3d because
//                //       through the Create() we use the HACK Duplicate method which is required because of a inflexibility issue with TVActor and animation updates.
//                DefaultAppearance dummy;
//                Actor3d opponent = Actor3d.Create("Actor3d_FPS_Female", file, CONST_TV_ACTORMODE.TV_ACTORMODE_SHADER, true,
//                                        true, out dummy, out tvactor);

//                x = AppMain._core.Random.Next(-HALF_WORLD_WIDTH, HALF_WORLD_WIDTH);
//                z = AppMain._core.Random.Next(-HALF_WORLD_HEIGHT, HALF_WORLD_HEIGHT);
//                int animation = AppMain._core.Random.Next(1, 2);
//                BonedModel model = new BonedModel("BonedModel_FPSFemale" + (i + 1), tvactor);
//                model.AnimationID = animation;
//                model.Translation = new TV_3DVECTOR(x, terrain.GetHeight(x, z), z);
//                model.Scale = new TV_3DVECTOR(scale, scale, scale);
//                model.Rotation = new TV_3DVECTOR();

//                model.AddChild(opponent);
//                model.AddChild(app); //note we share the original appearance
//                AppMain._core.Simulation.AddEntity(new NPC(model, AppMain._core.Simulation.CurrentTarget), model.Translation );
//            }
//        }

//        private static void InitFemalePlayer()
//        {
//            float x, y, z;
//            DefaultAppearance app;
//            string file = @"Actors\fps_female\fps_female_multitrack.tva";

//            TVActor tvactor;
//            Actor3d fpsfemale = Actor3d.Create("Actor3d_FPS_Female", file, CONST_TV_ACTORMODE.TV_ACTORMODE_SHADER, true, true, out app, out tvactor);
//            float scale = 1.75f / fpsfemale.BoundingSphere.Radius; // compute her scale to the desired height of 5'9"

//            fpsfemale.ComputeNormals(); // should always be done before lighting mode
//            //fpsfemale.Appearance.LightingMode = CONST_TV_LIGHTINGMODE.TV_LIGHTING_BUMPMAPPING_TANGENTSPACE;
//            // NOTE: when using Stencils and in Shader MODE the fpsfemale looks just like in Shadowmapping, but in cpu mode it looks like crap
//            fpsfemale.PlayAnimation(.3f);
//            fpsfemale.Update();
//            //app.AddChild(matte);
//            app.LightingMode = CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED;
//            BonedModel fpsplayer = new BonedModel("BonedModel_FPS_Female", tvactor );
//            fpsplayer.AnimationID = 1;
//            fpsplayer.AddChild(app);


//            fpsplayer.AddChild(fpsfemale);
//            x = -800; // 1072;
//            z = 1620;
//            y = 40;

//            fpsplayer.Translation = new TV_3DVECTOR(x, y, z);
//            fpsplayer.Scale = new TV_3DVECTOR(scale, scale, scale);
//            // model.Rotation = new TV_3DVECTOR(0, 180, 0);

            
//            TV_3DVECTOR A, B, C, D;

//            x = -850; z = -1600; y = 100; // terrain.GetHeight(x, z);
//            A = new TV_3DVECTOR(x, y, z);

//            x = -950; z = -1860; y = 100;// terrain.GetHeight(x, z);
//            B = new TV_3DVECTOR(x, y, z);

//            x = -1180; z = -1600; y = 100; // terrain.GetHeight(x, z);
//            C = new TV_3DVECTOR(x, y, z);

//            x = -1180; z = -1860; y = 100; // terrain.GetHeight(x, z);
//            D = new TV_3DVECTOR(x, y, z);

//            Core.AI.Path path = new Core.AI.Path(A, B, C, D, 200, 0.1f); // TODO: there's a bug i think in steering where if we overshoot our target, we then turn around to reach it rather than 
//            // a) stopping if its our final destination  b) itteratively using remaining movement to get to steer to the next target (re-constraint MaxSpeed to remaining movement )
//  //          AppMain._core.SceneManager.Add(fpsplayer, true);
//            AppMain._core.Simulation.AddEntity(new NPC(fpsplayer, path), fpsplayer.Translation );  // with path

//            //Core.AI.ISteerable dummy = null;
//            //AppMain._simulation.AddPlayer(new NPC(fpsplayer, dummy), true);  // with wander

//            //  AppMain._simulation.AddPlayer(new Player(fpsplayer), true);  // with human controlled (make sure maleplayer is not human controlled)

//            // load up the MP5
//            file = @"Meshes\weapons\mp5.tvm";
//            Mesh3d weapon = Mesh3d.Create("Mesh3D_MP5", file, true, true, out app);
//            scale = .70f / weapon.BoundingSphere.Radius; // about .7 of a meter long

//            //Material shiny = Material.Create(Core.Globals.GetMat("Shiny"));
//            //app.AddChild(mat);
//            app.LightingMode = CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED;
//            SimpleModel weaponModel = new SimpleModel("Model_MP5");
//            weaponModel.AddChild(app);
//            weaponModel.AddChild(weapon);
//            weaponModel.Rotation = new TV_3DVECTOR(0, 180, 0);
//            weaponModel.Translation = new TV_3DVECTOR(0, 0, 0f);
//            weaponModel.Scale = new TV_3DVECTOR(scale, scale, scale);

//            // now this weapon entity should have details on how various bonedmodels can "hold" this weapon...
//            fpsplayer.AddChild(weaponModel, 9);
//        }

//        private static void InitMalePlayer()
//        {
//            float x, y, z;
//            DefaultAppearance app;

//            ////////////////////////////////////////////
//            // HUMAN PLAYER
//            ////////////////////////////////////////////
//            string file = @"Actors\Hmale\hmale.tva";

//            TVActor tvactor;
//            Actor3d player = Actor3d.Create("HumanMale", file, CONST_TV_ACTORMODE.TV_ACTORMODE_SHADER, true, true, out app, out tvactor);
//            float scale = 2f / player.BoundingSphere.Radius; // 2 meter tall 

//            player.ComputeNormals();
//            app.LightingMode = CONST_TV_LIGHTINGMODE.TV_LIGHTING_BUMPMAPPING_TANGENTSPACE;
//            //player.SetCustomAnimation(181, 215);
//            int id = player.AddAnimationRange(0, "idle", 0, 100);
//            id = player.AddAnimationRange(0, "walking", 110, 149);
//            id = player.AddAnimationRange(0, "running", 181, 215);
//            // NOTE: THe human male.tvm does have materials defined within it
//            // Material matte = Material.Create(Core.Globals.GetMat("Matte"));
//            //app.AddChild(matte);
//            BonedModel malePlayer = new BonedModel("human", tvactor);
//            NormalMap normalmap = NormalMap.Create(AppMain._core.GetNewName(typeof(NormalMap)), @"Actors\Hmale\Body_Bmap.dds"); // must store in normal map layer1
//            ((GroupAttribute)app.Children[0]).AddChild(normalmap);
//            normalmap = NormalMap.Create(AppMain._core.GetNewName(typeof(NormalMap)), @"Actors\Hmale\Pants_Bmap.dds"); // must store in normal map layer1
//            ((GroupAttribute)app.Children[1]).AddChild(normalmap);
//            normalmap = NormalMap.Create(AppMain._core.GetNewName(typeof(NormalMap)), @"Actors\Hmale\Boots_Bmap.dds"); // must store in normal map layer1
//            ((GroupAttribute)app.Children[2]).AddChild(normalmap);
//            normalmap = NormalMap.Create(AppMain._core.GetNewName(typeof(NormalMap)), @"Actors\Hmale\Gloves_Bmap.dds"); // must store in normal map layer1
//            ((GroupAttribute)app.Children[3]).AddChild(normalmap);
//            normalmap = NormalMap.Create(AppMain._core.GetNewName(typeof(NormalMap)), @"Actors\Hmale\Hair_Bmap.dds"); // must store in normal map layer1
//            ((GroupAttribute)app.Children[4]).AddChild(normalmap);
//            normalmap = NormalMap.Create(AppMain._core.GetNewName(typeof(NormalMap)), @"Actors\Hmale\Head_Bmap.dds"); // must store in normal map layer1
//            ((GroupAttribute)app.Children[5]).AddChild(normalmap);
//            malePlayer.AddChild(app);
//            malePlayer.AddChild(player);
//            malePlayer.Scale = new TV_3DVECTOR(scale, scale, scale);
//            x = -1500;
//            z = -800;
//            malePlayer.Translation = new TV_3DVECTOR(x, terrain.GetHeight(x, z), z);



//            Mesh3d weapon = Mesh3d.Create("Claymore", @"Meshes\weapons_armor\Guardians_Claymore.TVM", true, true, out app);
//            //Material shiny = Material.Create(Core.Globals.GetMat("Shiny"));
//            //app.AddChild(mat);
//            app.LightingMode = CONST_TV_LIGHTINGMODE.TV_LIGHTING_BUMPMAPPING_TANGENTSPACE;
//            SimpleModel swordModel = new SimpleModel("swordModel");
//            //normalmap = NormalMap.Create(Core.GetNewName(typeof(NormalMap)), @"Meshes\weapons_armor\Guardians_Sword_N.bmp"); // must store in normal map layer1
//            //((GroupAttribute)app.Children[0]).AddChild(normalmap);
//            swordModel.Rotation = new TV_3DVECTOR(0, -100, 0);
//            swordModel.Translation = new TV_3DVECTOR(1, -0.65f, 0);
//            swordModel.AddChild(app);
//            swordModel.AddChild(weapon);

//            Mesh3d cuirass = Mesh3d.Create("Cuirass", @"Meshes\weapons_armor\Iron_Cuirass.TVM", true, true, out app);
//            //app.AddChild(shiny); 
//            app.LightingMode = CONST_TV_LIGHTINGMODE.TV_LIGHTING_BUMPMAPPING_TANGENTSPACE;
//            SimpleModel cuirassModel = new SimpleModel("cuirassdModel");
//            //normalmap = NormalMap.Create(Core.GetNewName(typeof(NormalMap)), @"Meshes\weapons_armor\Iron_Armor_N.bmp"); // must store in normal map layer1
//            //((GroupAttribute)app.Children[0]).AddChild(normalmap);
//            cuirassModel.Rotation = new TV_3DVECTOR(-90, 0, -90);
//            cuirassModel.Translation = new TV_3DVECTOR(-1.5f, 0.25f, 0);
//            cuirassModel.AddChild(app);
//            cuirassModel.AddChild(cuirass);

//            Mesh3d shield = Mesh3d.Create("Shield", @"Meshes\weapons_armor\Guardians_Shield.TVM", true, true, out app);
//            //app.AddChild(shiny);
//            app.LightingMode = CONST_TV_LIGHTINGMODE.TV_LIGHTING_BUMPMAPPING_TANGENTSPACE;
//            SimpleModel shieldModel = new SimpleModel("shieldModel");
//            //normalmap = NormalMap.Create(Core.GetNewName(typeof(NormalMap)), @"Meshes\weapons_armor\Guardians_Shield_N.bmp"); // must store in normal map layer1
//            //((GroupAttribute)app.Children[0]).AddChild(normalmap);
//            shieldModel.Rotation = new TV_3DVECTOR(0, 0, 0);
//            shieldModel.Translation = new TV_3DVECTOR(0.5f, 0, 0.2f);
//            shieldModel.AddChild(app);
//            shieldModel.AddChild(shield);

//            // TODO: in our new Entity model, each below will be an entity and is added to the parent entity
//            //       but when these are added to the scene, they are added to the spatial graph WITHOUT attachment.
//            //       but part of the problem is, the way animation skinning is done, i think i cant grab proper positions
//            //       for child entities easily?  double check how i handle these now...
//            malePlayer.AddChild(swordModel, 11); // "upper_torso"
//            malePlayer.AddChild(cuirassModel, 11);
//            malePlayer.AddChild(shieldModel, 14); // "left_elbow"

//   //         AppMain._core.SceneManager.Add(malePlayer, true);
//            AppMain._core.Simulation.AddEntity(new Player(malePlayer), malePlayer.Translation );
//        }

//        private static void InitReznok()
//        {
//            float x, y, z;
//            DefaultAppearance app;

//            ////////////////////////////////////////////
//            // HUMAN PLAYER
//            ////////////////////////////////////////////
//            string file = @"Actors\dominus\dominum_reznok.tva";

//            TVActor tvactor;
//            Actor3d player = Actor3d.Create("ReznokActor3d", file, CONST_TV_ACTORMODE.TV_ACTORMODE_SHADER, false, false, out app, out tvactor); // NOTE: false false because this particular Reznok model has 3 groups and the 1 and last are useless and the texture must be added to 2nd so we do it manually
//            float scale = 2f / player.BoundingSphere.Radius; // 2 meter tall 

//            player.ComputeNormals();
//            app.LightingMode = CONST_TV_LIGHTINGMODE.TV_LIGHTING_NORMAL;
//            //player.SetCustomAnimation(181, 215);
//            //int id = player.AddAnimationRange(0, "idle", 0, 100);
//            //id = player.AddAnimationRange(0, "walking", 110, 149);
//            //id = player.AddAnimationRange(0, "running", 181, 215);

//            // Material matte = Material.Create(Core.Globals.GetMat("Matte"));
//            //app.AddChild(matte);
//            BonedModel reznok = new BonedModel("ReznokModel", tvactor);
//            //NormalMap normalmap = NormalMap.Create(Core.GetNewName(typeof(NormalMap)), @"Actors\Hmale\Body_Bmap.dds"); // must store in normal map layer1
//            //((GroupAttribute)app.Children[0]).AddChild(normalmap);

//            reznok.AddChild(app);
//            reznok.AddChild(player);
//            reznok.Scale = new TV_3DVECTOR(scale, scale, scale);
//            x = -1500;
//            z = -800;
//            reznok.Translation = new TV_3DVECTOR(x, terrain.GetHeight(x, z), z);


            

//            //Mesh3d weapon = Mesh3d.Create("Claymore", @"Meshes\weapons_armor\Guardians_Claymore.TVM", true, true, out app);
//            ////Material shiny = Material.Create(Core.Globals.GetMat("Shiny"));
//            ////app.AddChild(mat);
//            //app.LightingMode = CONST_TV_LIGHTINGMODE.TV_LIGHTING_BUMPMAPPING_TANGENTSPACE;
//            //SimpleModel swordModel = new SimpleModel("swordModel");
//            ////normalmap = NormalMap.Create(Core.GetNewName(typeof(NormalMap)), @"Meshes\weapons_armor\Guardians_Sword_N.bmp"); // must store in normal map layer1
//            ////((GroupAttribute)app.Children[0]).AddChild(normalmap);
//            //swordModel.Rotation = new TV_3DVECTOR(0, -100, 0);
//            //swordModel.Translation = new TV_3DVECTOR(1, -0.65f, 0);
//            //swordModel.AddChild(app);
//            //swordModel.AddChild(weapon);


//            //Mesh3d shield = Mesh3d.Create("Shield", @"Meshes\weapons_armor\Guardians_Shield.TVM", true, true, out app);
//            ////app.AddChild(shiny);
//            //app.LightingMode = CONST_TV_LIGHTINGMODE.TV_LIGHTING_BUMPMAPPING_TANGENTSPACE;
//            //SimpleModel shieldModel = new SimpleModel("shieldModel");
//            ////normalmap = NormalMap.Create(Core.GetNewName(typeof(NormalMap)), @"Meshes\weapons_armor\Guardians_Shield_N.bmp"); // must store in normal map layer1
//            ////((GroupAttribute)app.Children[0]).AddChild(normalmap);
//            //shieldModel.Rotation = new TV_3DVECTOR(0, 0, 0);
//            //shieldModel.Translation = new TV_3DVECTOR(0.5f, 0, 0.2f);
//            //shieldModel.AddChild(app);
//            //shieldModel.AddChild(shield);

//            //reznok.AddChild(swordModel, 11); // "upper_torso"
//            //reznok.AddChild(shieldModel, 14); // "left_elbow"

//     //       AppMain._core.SceneManager.Add(reznok, true);
//            AppMain._core.Simulation.AddEntity(new Player(reznok), reznok.Translation);
//        }

//        private static void InitOccluder()
//        {
//            // ok this box type occluder sucks.  
//            // 1) it requires you to use an OOBB.  AABB wont work properly.
//            // 2) even with an OOBB, a box or other concave hull type requires
//            // more sophisticated code to determine the edges to use when creating the planes everytime the camera moves.
//            _occluder = AppMain._core.Scene.CreateMeshBuilder();
//            _occluder.CreateBox(35f, 35f, 10);
//            _transparentMat = AppMain._core.MaterialFactory.CreateMaterialQuick(.8f, .8f, .8f, .3f, "occluder");
//            _occluder.SetAlphaTest(false, 0);
//            _occluder.SetMaterial(_transparentMat);
//            _occluder.SetLightingMode(CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED);
//            _occluder.SetBlendingMode(CONST_TV_BLENDINGMODE.TV_BLEND_ALPHA);

//            MoveOccluder(new TV_3DVECTOR(0, 0, 0));
//            _occlusionCuller = new OcclusionCuller(_occlusionFrustum, null);
//        }

//        // TODO: this is a test function and should eventually be deleted.
//        private static void MoveOccluder(TV_3DVECTOR pos)
//        {
//            TV_3DVECTOR rot = AppMain._core.Simulation.CurrentTarget.Rotation;
//            _occluder.SetPosition(pos.x, pos.y, pos.z);
//            _occluder.SetRotation(rot.x, rot.y, rot.z);

//            TV_3DVECTOR min, max;
//            min = new TV_3DVECTOR(0, 0, 0);
//            max = new TV_3DVECTOR(0, 0, 0);
//            _occluder.GetBoundingBox(ref min, ref max, false);
//            ConvexHull hull = new ConvexHull(new BoundingBox(min, max));

//            _occlusionFrustum = new OcclusionFrustum(hull, _sphereTest, _planeTest);
//        }


//        private static void InitializeWaterfall()
//        {
//            _waterFall = AppMain._core.Scene.CreateParticleSystem();
//            int WaterfallEmitor;
//            //TVMiniMesh WaterfallParticles;

//            //no way to duplicate?
//            // WaterfallParticles = Core.Scene.CreateMiniMesh(50, "");
//            //WaterfallParticles.CreateBillboard(1, 1, true); // TODO: this could technically work from actual meshes too... but 
//            // typically you probably wouldnt want to use these?  
//            //WaterfallParticles.SetTexture(Core.Globals.GetTex("Smoke"));

//            //WaterfallEmitor = _waterFall.CreateEmitter(CONST_TV_EMITTERTYPE.TV_EMITTER_MINIMESH, 150);
//            WaterfallEmitor = _waterFall.CreateEmitter(CONST_TV_EMITTERTYPE.TV_EMITTER_BILLBOARD, 150);
//            _waterFall.SetBillboard(WaterfallEmitor, AppMain._core.Globals.GetTex("Smoke"), 64, 64);
//            // _waterFall.SetMiniMesh(WaterfallEmitor, WaterfallParticles); // should be added as an "Appearance" node
//            _waterFall.SetEmitterGravity(WaterfallEmitor, true, new TV_3DVECTOR(0, -1, 0));
//            _waterFall.SetEmitterShape(WaterfallEmitor, CONST_TV_EMITTERSHAPE.TV_EMITTERSHAPE_SPHERESURFACE);
//            _waterFall.SetEmitterSphereRadius(WaterfallEmitor, 3.5F);
//            _waterFall.SetEmitterPower(WaterfallEmitor, 15, 16);
//            _waterFall.SetEmitterSpeed(WaterfallEmitor, 240);

//            _waterFall.SetEmitterDirection(WaterfallEmitor, true, new TV_3DVECTOR(1, -.3F, 0), new TV_3DVECTOR(0, 0, 0));
//            _waterFall.SetParticleDefaultColor(WaterfallEmitor, new TV_COLOR(.7F, .5F, 1, 0.7F));
//            // _waterFall.MoveEmitter(WaterfallEmitor, new TV_3DVECTOR(-46, 23 - 0.1F, -10)); // is this relative?

//            _waterFall.SetGlobalPosition(615, terrain.GetHeight(615, 1430), 1430); //); y=1640;


//            TV_PARTICLE_KEYFRAME[] PartKeyFrames = new TV_PARTICLE_KEYFRAME[3];

//            PartKeyFrames[0].fKey = 0;
//            PartKeyFrames[0].fSize = new TV_3DVECTOR(0, 0, 0);
//            PartKeyFrames[0].cColor = new TV_COLOR(1, 1, 1, 0);
//            PartKeyFrames[1].fKey = 0.2F;
//            PartKeyFrames[1].fSize = new TV_3DVECTOR(8, 8, 8);
//            PartKeyFrames[1].cColor = new TV_COLOR(1, 1, 1, 1);
//            PartKeyFrames[2].fKey = 2;
//            PartKeyFrames[2].fSize = new TV_3DVECTOR(10, 10, 10);
//            PartKeyFrames[2].cColor = new TV_COLOR(1, 1, 1, 1);

//            _waterFall.SetParticleKeyFrames(WaterfallEmitor,
//                                            (int)(CONST_TV_PARTICLE_KEYUSAGE.TV_PARTICLE_KEYUSAGE_SIZE |
//                                                   CONST_TV_PARTICLE_KEYUSAGE.TV_PARTICLE_KEYUSAGE_COLOR), 3,
//                                            PartKeyFrames);

//            // the way i see it, there's no way to use tv3d's particle system in a shared way as we do mesh
//            // best we can do is use duplicate. And emitters are definetly tied to the particular system
//            // so dont even think about trying to use those.. of course i could test this theory by creating a second
//            // waterfall2 and seeing if i can set the same emitters to both and still get them both behaviing properly.
//            // 
//            //_waterFall.Duplicate();
//        }

//        ///<summary>
//        ///Creates a smoke particle system
//        ///</summary>
//        private static void InitializeParticleSystem()
//        {
//            AppMain._core.TextureFactory.LoadTexture(@"Particles\Smoke\Smoke.dds", "Smoke");

//            ps = AppMain._core.Scene.CreateParticleSystem();

//            int EmitterIndex = ps.CreateEmitter(CONST_TV_EMITTERTYPE.TV_EMITTER_BILLBOARD, 1000);

//            ps.SetEmitterAlphaBlending(EmitterIndex, CONST_TV_PARTICLECHANGE.TV_CHANGE_ALPHA,
//                                       CONST_TV_BLENDINGMODE.TV_BLEND_ALPHA);
//            ps.SetEmitterDirection(EmitterIndex, true, new TV_3DVECTOR(0F, 0.5F, 0F),
//                                   new TV_3DVECTOR(0.05F, 0.5F, 0.05F));
//            ps.SetEmitterGravity(EmitterIndex, true, new TV_3DVECTOR(0, -1, 0));
//            ps.SetEmitterPosition(EmitterIndex, new TV_3DVECTOR(0, 0, 0));
//            ps.SetEmitterPower(EmitterIndex, 25, 4);
//            ps.SetEmitterShape(EmitterIndex, CONST_TV_EMITTERSHAPE.TV_EMITTERSHAPE_SPHEREVOLUME);
//            ps.SetEmitterSphereRadius(EmitterIndex, 8);
//            ps.SetEmitterSpeed(EmitterIndex, 125);
//            ps.SetEmitterEnable(EmitterIndex, true);

//            ps.SetParticleDefaultColor(EmitterIndex, new TV_COLOR(1, 1, 1, 0.5F));
//            ps.SetGlobalPosition(1200, terrain.GetHeight(1200, 1600), 1600); //);

//            // Keyframes!
//            TV_PARTICLE_KEYFRAME[] KeyFrames = new TV_PARTICLE_KEYFRAME[3];

//            KeyFrames[0].fKey = 0;
//            KeyFrames[0].cColor = new TV_COLOR(0.7F, 0.7F, 0.7F, 0);
//            KeyFrames[0].fSize = new TV_3DVECTOR(8, 8, 8);
//            KeyFrames[1].fKey = 1.5F;
//            KeyFrames[1].cColor = new TV_COLOR(0.85F, 0.85F, 0.85F, 0.75F);
//            KeyFrames[1].fSize = new TV_3DVECTOR(32, 32, 32);
//            KeyFrames[2].fKey = 4;
//            KeyFrames[2].cColor = new TV_COLOR(1, 1, 1, 0);
//            KeyFrames[2].fSize = new TV_3DVECTOR(64, 64, 64);
//            ps.SetParticleKeyFrames(EmitterIndex,
//                                    (int)
//                                    (CONST_TV_PARTICLE_KEYUSAGE.TV_PARTICLE_KEYUSAGE_COLOR |
//                                     CONST_TV_PARTICLE_KEYUSAGE.TV_PARTICLE_KEYUSAGE_SIZE),
//                                    3, KeyFrames);

//            ps.SetBillboard(EmitterIndex, AppMain._core.Globals.GetTex("Smoke"), 64, 64);
//            // test to see if it matters when you set this
//        }

//        static ShadowMap.ShadowMap _shadowmap;
//        static FXLandShadow _landShadowMap;
//        private static void InitShadowMap()
//        {

//            _shadowmap = new ShadowMap.ShadowMap(AppMain._core, true, true, true);
//            _shadowmap.AddLight(_pointLight);
//            AppMain._core.SceneManager.Add(_shadowmap);

//        }

//        private static void InitLandShadowMap(Light sunlight)
//        {
//            _landShadowMap = new FXLandShadow(AppMain._core, sunlight, FXLandShadow.FILTERMODE.None,
//                                                new TV_3DVECTOR(0.4f, 0.4f, 0.4f),
//                                                1, CONST_TV_RENDERSURFACEFORMAT.TV_TEXTUREFORMAT_X8R8G8B8,
//                                                new RSResolution[1] { RSResolution.R_2048x2048 },
//                                                FXLandShadow.SHADOWMAPMODE.ShadowProjection, AppMain._core.SceneManager.CullRequest);
//            AppMain._core.SceneManager.Add(_landShadowMap);

//        }
//        private static void InitBloom()
//        {
//            //FXBloom bloom = new FXBloom(AppMain._core.Engine.GetViewport());
//           // AppMain._core.SceneManager.Add(bloom);

//            FXFullscreenPost post = new FXFullscreenPost(AppMain._core.Engine.GetViewport(), AppMain._core.SceneManager.RenderScene);

//            AppMain._core.SceneManager.Add(post);
//        }

//        private static void InitGlow()
//        {
//            FXGlow glow = new FXGlow(RSResolution.R_256x256, new Color(1, 1, 1, 1), 0.75f, 1, AppMain._core.SceneManager.RenderScene);
//            AppMain._core.SceneManager.Add(glow);
//        }

//        private static void InitHDR()
//        {

//            //glowEffect.InitHDR( CONST_TV_RENDERSURFACEFORMAT.TV_TEXTUREFORMAT_HDR_FLOAT32 )
//            //glowEffect.StartRenderHDR() 
//            ////TODO: render here
//            //glowEffect.EndRenderHDR() 
//        }

//        private static FXFog _fog;
//        private static void InitFog()
//        {
//            // NOTE: NonReflectiveWater seems to have problems with VERTEX fog type.  Not sure why.
//            _fog = new FXFog(CONST_TV_FOG.TV_FOG_EXP2, CONST_TV_FOGTYPE.TV_FOGTYPE_PIXEL);
//            _fog.Enable = true;
//            _fog.Start = 0;
//            _fog.End = 5000;
//            _fog.Density = .0004f; // its the density + distance you look through at that density which determines visibility range
//            _fog.Color = new Color(0.97f, 0.97f, 0.97f, 1);
//            //_skyAverageColor = new TV_COLOR(0.5f, 0.62f, 0.77f, 1);            
//        }

//        private static void InitWeather()
//        {
//            AppMain._core.SceneManager.Add(new FXRain());
//        }

//        public static void InitZakWater2()
//        {
//            const float TV_WATER_HEIGHT = 90;
//            SimpleModel model = new SimpleModel("ZakWater");
//            GroupAttribute ga = new GroupAttribute(AppMain._core.GetNewName(typeof(GroupAttribute)));
//            DefaultAppearance app = new DefaultAppearance("DefaultAppearance_ZakWater");

//            Mesh3d water = Mesh3d.CreateFloor("Water", -1250, 1024, -2048, -2048, 32, 32, TV_WATER_HEIGHT, 25, 22);

//            FXOceanWater.NormalMapMode normalMapMode = FXOceanWater.NormalMapMode.VolumeMap;
//            bool TroubledWaterEnhancements = true;

//            Texture normalMap;


//            if (normalMapMode == FXOceanWater.NormalMapMode.Dual2DMapLookup)
//                normalMap = VolumeTexture.Create(AppMain._core.GetNewName(typeof(VolumeTexture)), @"Shaders\Water\Maps\DualLookupNormalMap.dds", CONST_TV_COLORKEY.TV_COLORKEY_USE_ALPHA_CHANNEL, true);

//            else
//                normalMap = NormalMap.Create(AppMain._core.GetNewName(typeof(NormalMap)), @"Shaders\Water\Maps\VolumeNormalMap.dds");

//            ga.AddChild(normalMap);
//            app.LightingMode = CONST_TV_LIGHTINGMODE.TV_LIGHTING_NORMAL;
//            app.AddChild(ga);
//            water.CullMode = CONST_TV_CULLING.TV_DOUBLESIDED; // if you plan to see underwater
//            model.AddChild(app);
//            model.AddChild(water);

//            // instance the water shader
//            FXOceanWater fx = new FXOceanWater(RSResolution.R_512x512,
//                                              "WaterShader",
//                                              @"Shaders\Water\WaterShader3.fx",
//                                              TV_WATER_HEIGHT, true, true, true, TroubledWaterEnhancements, true,
//                                              normalMapMode, AppMain._core.SceneManager.RenderScene);
//            AppMain._core.SceneManager.Add(fx);

//            model.Matrix = water.Matrix; ////// because the AddFloor computes all the position and crap for us
//            // assign the shader.
//            model.Subscribe(fx);
//            // TODO: should be like
//            // model.Providers = FX_OCEAN ;  // with | for others like  FX_LANDSHADOW | FX_INDOORSHADOW | FX_IMPOSTER
//            // i think our ENUM code can help to just store these and convert them directly.
//            //
//            //AppMain._core.SceneManager.Add(model);  // TODO: Adding this to the scene causes it to get rendered twice.  Must fix
//            // because that ruins our "free" alphablending with glow.
            
//        }

//        private static void InitWater(bool tvwater)
//        {
//            Mesh3d tmp;
//            SimpleModel model = new SimpleModel("water0");
//            GroupAttribute ga = new GroupAttribute(AppMain._core.GetNewName(typeof(GroupAttribute)));
//            DefaultAppearance app = new DefaultAppearance("DefaultAppearance_TVWater");
//            Texture normalMap;

//            if (tvwater)
//            {
//                //TV WATER START
//                //============================
//                const float TV_WATER_HEIGHT = 30;
//                normalMap = DUDVTexture.Create(AppMain._core.GetNewName(typeof(DUDVTexture)), @"Shaders\Water\Water_DUV.dds", -1, -1, 50, false);

//                // NOTE: the water plane distance 0 of the y axis.  Im not sure why it's a negative but i do know the +1 added
//                // is to eliminate the artifact you get along the shore edge where water meets land
//                FXWater fx =
//                    new FXWater(0, -(TV_WATER_HEIGHT + 1), new TV_3DVECTOR(0, 1, 0), true, true, new TV_COLOR(1, 1, 1, .3f),
//                                new TV_COLOR(1, 1, 1, .3f), 0, AppMain._core.SceneManager.RenderScene);
//                AppMain._core.SceneManager.Add(fx);
                

//                tmp = Mesh3d.CreateFloor("waterpatch1", -1024, -1024, 5120, 5120, 32, 32, TV_WATER_HEIGHT, 10, 10);
//                // TODO: note with tvwater you dont have to set DoubleSided culling.  Strange?
//                ga.AddChild(normalMap);
//                app.LightingMode = CONST_TV_LIGHTINGMODE.TV_LIGHTING_NONE;
//                app.AddChild(ga);
//                model.AddChild(app);
//                model.AddChild(tmp);
//                model.Matrix = tmp.Matrix;  // because the AddFloor computes all the position and crap for us
//                // and we dont get a chance to set these directly which causes the matrix
//                // to be properly calculated.  Thus we must do it this way :(
//                // Doing so does force the proper position, scale, rotation to be computed by the Model
//                model.Subscribe(fx);

//                tmp = Mesh3d.CreateFloor("waterpatch2", 2000, 0, 2500, 2500, 32, 32, TV_WATER_HEIGHT, 10, 10);
//                // TODO: note with tvwater you dont have to set DoubleSided culling.  Strange?
//                model = new SimpleModel("water1");
//                model.AddChild(app);
//                model.AddChild(tmp);
//                model.Matrix = tmp.Matrix; //// because the AddFloor computes all the position and crap for us
//                model.Subscribe(fx);
//            }
//            else
//            {
//                // NON REFLECTIVE WATER START
//                // ==========================
//                const float WATER_HEIGHT = 30;
//                AppMain._core.TextureFactory.SetTextureMode(CONST_TV_TEXTUREMODE.TV_TEXTUREMODE_COMPRESSED);

//                FXWaterNoReflection fx2 = new FXWaterNoReflection(WATER_HEIGHT + 1);
//                AppMain._core.SceneManager.Add(fx2);
                
//                // Water init - this mesh should be passed in
//                tmp = Mesh3d.CreateFloor("waterpatch3", -1024, -1024, 5120, 5120, 32, 32, WATER_HEIGHT, 1, 1);
//                // In this shader, the tiling is (1, 1) and multiplied by shader parameters
//                // You need to put a fair amount of polygons to get good-looking vertex fog
//                // The placement of the water plane in relation to the world is very important as well
//                // because of the new alpha map!!
//                tmp.CullMode = CONST_TV_CULLING.TV_DOUBLESIDED; // if you want to see underwater

//                normalMap = NormalMap.Create(AppMain._core.GetNewName(typeof(Diffuse)), "Shaders\\Water2\\NormalMap_DXT5NM.dds");
//                ga.AddChild(normalMap);
//                app.LightingMode = CONST_TV_LIGHTINGMODE.TV_LIGHTING_NORMAL;
//                app.AddChild(ga);
//                model.AddChild(app);
//                model.AddChild(tmp);
//                model.Matrix = tmp.Matrix; //// because the AddFloor computes all the position and crap for us
//                model.Subscribe(fx2);
//            }
//        }

//        private static void InitSpacecraft()
//        {
//            SimpleModel fighter;
//            Mesh3d mesh;
//            DefaultAppearance app;

//            mesh = Mesh3d.Create("Mesh3d_Fighter", "Meshes\\freefighter.x", true, true, out app);
//            app.LightingMode = CONST_TV_LIGHTINGMODE.TV_LIGHTING_NORMAL;
//            fighter = new SimpleModel("fighter");
//            fighter.AddChild(app);
//            fighter.AddChild(mesh);

//            TV_3DVECTOR position = new TV_3DVECTOR(1140, 400, 1590);

//            fighter.Translation = position;
//            AppMain._core.Simulation.AddEntity(new StaticEntity(fighter.Name,fighter), position);

//        }
        
        


//        private static void InitAtmosphere3()
//        {
//            Texture tex = Diffuse.Create(AppMain._core.GetNewName(typeof(Diffuse)), @"Shaders\SkyGradient\Clouds.dds");
//            Material mat = Material.Create("SkyGradientMat", new Color(1, 1, 1, 1), new Color(1, 1, 1, 1), new Color(0, 0, 0, 1), new Color(0, 0, 0, 1));
//            //TODO: the Mesh isnt being resource tracked this way.  We need a Mesh3d.Create() static-method
//            Mesh3d m = Mesh3d.CreateUVTriangleStripSphere("sky", .15f, 64, 16, 1);
//            SimpleModel model = new SimpleModel("skydome");
//            model.Scale = new TV_3DVECTOR(100, 100, 100);
//            model.AddChild(m);
//            FXSkyGradient sky =
//                new FXSkyGradient(FXSkyGradient.InterpolationModes.Linear, model, tex, mat);

//            AppMain._core.SceneManager.Add(sky);
//        }

//        private static void InitAtmosphere2(float farplane)
//        {
//            Texture tex = Diffuse.Create(AppMain._core.GetNewName(typeof(Diffuse)), @"Sky\Sky1.dds");
//            FXSkySimple sky = new FXSkySimple(tex, farplane);
//            sky.Enable = true;
//            AppMain._core.SceneManager.Add(sky);
//        }

//        private static void InitAtmosphere(Light light)
//        {
//            SimpleModel modelSkyHem, modelCloudDome, modelSunBB, modelMoonBB;
//            DefaultAppearance app;
//            GroupAttribute ga;
//            Diffuse diffuse;
//            NormalMap normal;
//            const float SKY_RADIUS = 180;
//            // Init the sun light
//            Light sunlight = light;

//            //http://www.truevision3d.com/phpBB2/viewtopic.php?t=14983
//            // NOTE you don't really need alpha-testing to do transparency, and indeed if you want full 8-bit transparency you should 
//            //disable alpha-testing as it's enabled by default. Though most people prefer it enabled because it solves the alpha-sorting problem. 
//            //Alphablending is needed yes, and if you want "real" 8-bit transparency then you need a correct render order, yep.
//            //Also you need to specify an opacity factor in your material (which is the same as the alpha of diffuse) 
//            //and/or a transparent texture using TGA/DDS/PNG.


//            // Moon Material
//            int moonMat = AppMain._core.MaterialFactory.CreateMaterialQuick(1f, 1f, 1f, 1f, "MoonMat");
//            Material moonMmat = Material.Create(moonMat);


//            // Sky hemisphere
//            Mesh3d skyhem = Mesh3d.Create("Mesh3d_skyhem", "Shaders\\Sky\\Hemisphere.tvm", false, false, out app);
//            //Mesh3d skyhem = new Mesh3d(CreateUVTriangleStripSphere(.15f, 64, 16, 100));
//            skyhem.MeshFormat = CONST_TV_MESHFORMAT.TV_MESHFORMAT_TEX1 | CONST_TV_MESHFORMAT.TV_MESHFORMAT_NOLIGHTING;
//            skyhem.CullMode = CONST_TV_CULLING.TV_BACK_CULL; // for the trianlge strip sphere this should be FRONT_CULL 
//            skyhem.Overlay = true;
//            skyhem.CollisionEnable = false;
//            modelSkyHem = new SimpleModel("skyhemisphere");
//            modelSkyHem.Scale = new TV_3DVECTOR(SKY_RADIUS, SKY_RADIUS, SKY_RADIUS);
//            ga = new GroupAttribute("gaskysphere");
//            diffuse = Diffuse.Create("StarMap", "Shaders\\Sky\\Starmap.dds");
//            ga.AddChild(diffuse);
//            app.AddChild(ga);
//            app.LightingMode = CONST_TV_LIGHTINGMODE.TV_LIGHTING_NONE; // must be set to NONE since appearance inits it to Managed TODO: maybe we should change Appearance to default with NONE?
//            modelSkyHem.AddChild(app);
//            modelSkyHem.AddChild(skyhem);
//            //  AppMain._core.SceneManager.Add(modelSkyHem, true);

//            // Clouds dome
//            Mesh3d cd = Mesh3d.Create("Mesh3d_clouddome", "Shaders\\Sky\\Dome.tvm", false, false, out app);
//            cd.CullMode = CONST_TV_CULLING.TV_BACK_CULL;
//            cd.CollisionEnable = false;
//            cd.MeshFormat = (CONST_TV_MESHFORMAT.TV_MESHFORMAT_TEX1 | CONST_TV_MESHFORMAT.TV_MESHFORMAT_TEX2);
//            modelCloudDome = new SimpleModel("cloudlayer");
//            modelCloudDome.Scale = new TV_3DVECTOR(SKY_RADIUS * 0.99f, SKY_RADIUS * 0.99f, SKY_RADIUS * 0.99f);
//            //cd.Overlay = true;
//            diffuse = Diffuse.Create("CloudLots", "Shaders\\Sky\\Clouds.dds"); // CloudsLots.dds
//            normal = NormalMap.Create("CloudLess", "Shaders\\Sky\\CloudsLess.dds");
//            ga = new GroupAttribute("gacloudlayer");
//            ga.AddChild(diffuse);
//            ga.AddChild(normal);
//            app.AddChild(ga);
//            modelCloudDome.AddChild(cd);
//            modelCloudDome.AddChild(app);
//            // AppMain._core.SceneManager.Add(modelCloudDome, true);

//            Billboard sun =
//                Billboard.Create("sunbillboard", CONST_TV_BILLBOARDTYPE.TV_BILLBOARD_FREEROTATION,
//                                          new TV_3DVECTOR(0f, 0f, 0f), 1200f, 1200f, true);
//            sun.BlendingMode = CONST_TV_BLENDINGMODE.TV_BLEND_ADD;
//            sun.AlphaTest = false; //(false, 0, false);
//            sun.CollisionEnable = false;
//            sun.Overlay = true;
//            Texture tex =
//                ImportLib.Import("Shaders\\Sky\\Sun.dds", CONST_TV_LAYER.TV_LAYER_0,
//                                 CONST_TV_TEXTURETYPE.TV_TEXTURE_DIFFUSEMAP);
//            app = new DefaultAppearance("DefaultAppearance_Sun");
//            ga = new GroupAttribute("gasunbillboard");
//            ga.AddChild(tex);
//            app.AddChild(ga);
//            app.LightingMode = CONST_TV_LIGHTINGMODE.TV_LIGHTING_NONE;
//            modelSunBB = new SimpleModel("sunbillboardModel");
//            modelSunBB.AddChild(sun);
//            modelSunBB.AddChild(app);
//            //     AppMain._core.SceneManager.Add(modelSunBB, true);

//            //NOTE: moon is duplicate but with different texture and different scale)
//            Billboard moon =
//                Billboard.Create("moonbillboard", CONST_TV_BILLBOARDTYPE.TV_BILLBOARD_FREEROTATION,
//                                          new TV_3DVECTOR(0f, 0f, 0f), 1200f, 1200f, true);
//            moon.BlendingMode = CONST_TV_BLENDINGMODE.TV_BLEND_ADD;
//            moon.AlphaTest = false; //(false, 0, false);
//            moon.CollisionEnable = false;
//            moon.Overlay = true;
//            tex =
//                ImportLib.Import("Shaders\\Sky\\Moon.dds", CONST_TV_LAYER.TV_LAYER_0,
//                                 CONST_TV_TEXTURETYPE.TV_TEXTURE_DIFFUSEMAP);
//            modelMoonBB = new SimpleModel("moonbillboardModel");
//            modelMoonBB.Scale = new TV_3DVECTOR(0.5f, 0.5f, 0.5f);
//            ga = new GroupAttribute("gamoonbillboard");
//            ga.AddChild(tex);
//            ga.AddChild(moonMmat);
//            app = new DefaultAppearance("DefaultAppearance_BillBoardMoon");
//            app.LightingMode = CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED;
//            app.AddChild(ga);
//            modelMoonBB.AddChild(app);
//            modelMoonBB.AddChild(moon);
//            //      AppMain._core.SceneManager.Add(modelMoonBB, true);


//            // TODO: These should be loaded into Texture objects and then passed into the SkyScattering constructor?
//            //       But how do we handle Repository reference counting then since it doesnt use the normal AddChild / RemoveChild mechanism 
//            //       that normally handles this?
//            // 
//            AppMain._core.TextureFactory.LoadCubeTexture("Shaders\\Sky\\CubeNormalizer_ULQ.dds", "CubeNormalizationMap", 256,
//                                                CONST_TV_COLORKEY.TV_COLORKEY_NO, false);
//            AppMain._core.TextureFactory.LoadTexture("Shaders\\Sky\\Glow.dds", "Glow");
//            AppMain._core.TextureFactory.LoadTexture("Shaders\\Sky\\Rays.dds", "Rays");

//            //sunflares
//            AppMain._core.TextureFactory.LoadTexture("Shaders\\Sky\\RainbowCircleFlare.jpg", "RainbowCircleFlare");
//            AppMain._core.TextureFactory.LoadTexture("Shaders\\Sky\\HexaFlare.jpg", "HexaFlare");
//            AppMain._core.TextureFactory.LoadTexture("Shaders\\Sky\\CircleFlare.jpg", "CircleFlare");
//            SkyScattering sky = new SkyScattering(0, 
//                                                  AppMain._core.Engine.GetViewport().GetWidth(), 
//                                                  ((Simulation)AppMain._core.Simulation).Time,
//                                                  sunlight, modelSkyHem, modelCloudDome, modelSunBB, modelMoonBB);
            
//            AppMain._core.SceneManager.Add(sky); // sky should be first because right now we just add all FX to a single array
            
//        }




//        //private static void ProcessKeys(TV_KEYDATA currentkey)
//        //{
//        //    switch (currentkey.Key)
//        //    {
//        //        case (int)(CONST_TV_KEY.TV_KEY_0):

//        //            break;

//        //        case (int)(CONST_TV_KEY.TV_KEY_F1):

//        //            break;

//        //        case (int)(CONST_TV_KEY.TV_KEY_F5):
//        //            _sphereTest = !_sphereTest;
//        //            _occlusionFrustum.EnableSphereTest = _sphereTest;
//        //            _planeTest = !_planeTest;
//        //            _occlusionFrustum.EnablePlaneTest = _planeTest;
//        //            break;

//        //        case (int)(CONST_TV_KEY.TV_KEY_F2):
//        //            _occlusionEnabled = !_occlusionEnabled;
//        //            break;

//        //        case (int)(CONST_TV_KEY.TV_KEY_F4):
//        //            _customFrustumEnabled = !_customFrustumEnabled;
//        //            _treeRenderer._enableUserFrustum = _customFrustumEnabled;
//        //            break;

//        //        case (int)(CONST_TV_KEY.TV_KEY_5):
//        //            _manager.UseStencils = !_manager.UseStencils;
//        //            if (_manager.UseStencils)
//        //                Core.Light.SetLightProperties(_pointLight.TVIndex, true, true, false);
//        //            else
//        //                Core.Light.SetLightProperties(_pointLight.TVIndex, true, false, false);
//        //            break;

//        //        case (int)(CONST_TV_KEY.TV_KEY_SPACE):
//        //            // move the oclluder
//        //            TV_3DVECTOR pos;
//        //            pos = AppMain._simulation.CurrentTarget.Translation;
//        //            pos.y = terrain.GetHeight(pos.x, pos.z) + 20;
//        //            MoveOccluder(pos);
//        //            break;
//        //        case (int)(CONST_TV_KEY.TV_KEY_ESCAPE):
//        //            AppMain._simulation.IsRunning = false;
//        //            break;

//        //        default:
//        //            break;
//        //    }
//        //}


//        //private static void RenderHouses(bool useTree, bool useExistingMeshList)
//        //{
//        //_meshesDrawn = 0;
//        //if (!useTree)
//        //{
//        //    //house.Render();
//        //    //foreach (TVMesh m in houses)
//        //    //{
//        //    //    m.Render();
//        //    //}

//        //    // Core.Scene.RenderAllMeshes();
//        //}
//        //else
//        //{
//        //    // TODO: must update the quadtree traversal to skip checking bounding volumes and Camera.IsBoxVisible if the parent
//        //    // was fully visible.                

//        //    if (!useExistingMeshList)
//        //    {
//        //        _treeRenderer.Clear();
//        //        _treeRoot.Traverse(_treeRenderer);
//        //    }

//        //    int[] mlist = _treeRenderer.Object3dListIndices;
//        //    Geometry[] oList = _treeRenderer.Object3DList.ToArray();

//        //    TV_3DVECTOR min = new TV_3DVECTOR(0, 0, 0), max = new TV_3DVECTOR(0, 0, 0);

//        //    // if the occluder is visible, run the checks
//        //    _occluder.GetBoundingBox(ref min, ref max, false);
//        //    if (AppMain._camera.IsBoxVisible(min, max))
//        //    {
//        //        if (_occlusionEnabled)
//        //        {
//        //            // taking the meshlist from the initial traversal, cull this set against the occluder.
//        //            // for now just simply list and no "proper" hierarchal reverse traversing from the player's position node (required to minimize 
//        //            // the number of occluders you potentially have to test against since you can occlude other occluders too!

//        //            if (_occlusionFrustum.IsDirty ) // just a hack to be able to turn off updates so we can walk around it and look at the drawn frustum planes and normals it without it constantly updating.
//        //            {
//        //                // update the occlusion frustum
//        //                // note: edges are in world coords and must be updated anytime an occluder is moved.  since occluders shouldnt be moving 
//        //                // during actual gameplay, best to just create new occlusion frustums when they do move.
//        //                AppMain._cameraPosition = AppMain._camera.GetPosition();
//        //                _occlusionFrustum.Update(AppMain._cameraPosition,   true , false);
//        //                _occlusionCuller.Frustum = _occlusionFrustum;
//        //                // must update with new one for now.  Ideally, there'd be a list of occluders
//        //                // and you'd start with occluders nearest the camera and first do tests to occlude other occluders.  Merge any occluder
//        //                // volumes if possible, then throw out any occluders that are far/wouldnt occlude much anyway
//        //                // and then just test against remaining occluders.
//        //            }
//        //            mlist = _occlusionCuller.Culler(oList);
//        //        }
//        //    }

//        //    _meshesDrawn = mlist.Length;
//        //    // USING RenderMeshList with the culled list
//        //    if (mlist.Length > 0)
//        //    {
//        //        Core.Scene.RenderMeshList(mlist.Length, mlist);
//        //    }

//        //    // ITTERATING manually calling render using GetMeshFromID against the culled list
//        //    //for (int i = 0; i < mlist.Length; i++)
//        //    //{
//        //    //    Core.Globals.GetMeshFromID(mlist[i]).Render();
//        //    //}

//        //    // ITTERATING all the houses ignoring the culling
//        //    //foreach (TVMesh m in houses)
//        //    //{
//        //    //    m.Render();
//        //    //}

//        //    // USING RenderAllMeshes ignoring the culling
//        //    //Core.Scene.RenderAllMeshes();

//        //    // rendered last so it alpha blends properly when not occluding
//        //    //if (_occlusionEnabled) _occluder.Render();
//        //    if (_customFrustumEnabled) _customViewFrustum.Render();
//        //    if (_occlusionFrustum.Planes != null)
//        //    {
//        //        // _occlusionFrustum.Render(); // debug wireframe of the occluders planes
//        //    }
//        //    //TV_3DVECTOR offset = new TV_3DVECTOR(-5, 5, 5);
//        //    //DebugDraw.DrawAxisIndicator(offset, AppMain._camera, 5);               
//        //}
//        //}

//        //void ManageInput()
//        //{
//        //        // Initialize the lines
//        //        _turbLine = new LineFactory.Line("Turbidity [t/T to change]");
//        //        _windLine = new LineFactory.Line("Wind Power [p/P to change]");
//        //        _sizeLine = new LineFactory.Line("Clouds size [c/C to change]");
//        //        _innerOpacLine = new LineFactory.Line("Inner Cloud Layer Opacity [i/I to change]");
//        //        _outerOpacLine = new LineFactory.Line("Outer Cloud Layer Opacity [o/O to change]");
//        //        _timeFactorLine = new LineFactory.Line("Game-Time Factor [f/F to change]");
//        //        LineFactory.Instance.AddLine(ref _turbLine);
//        //        LineFactory.Instance.AddLine(ref _windLine);
//        //        LineFactory.Instance.AddLine(ref _sizeLine);
//        //        LineFactory.Instance.AddLine(ref _innerOpacLine);
//        //        LineFactory.Instance.AddLine(ref _outerOpacLine);
//        //        LineFactory.Instance.AddLine(ref _timeFactorLine);


//        //    int shiftPressed =
//        //        (_inputs.IsKeyPressed(CONST_TV_KEY.TV_KEY_LEFTSHIFT) ||
//        //        _inputs.IsKeyPressed(CONST_TV_KEY.TV_KEY_RIGHTSHIFT))
//        //        ? -1 : 1;

//        //    float speed = shiftPressed * _elapsedTime * 0.001f;

//        //    if (_inputs.IsKeyPressed(CONST_TV_KEY.TV_KEY_T))
//        //    {
//        //        _turbidity += speed;
//        //        if (_turbidity < 2f) _turbidity = 2f;
//        //        if (_turbidity > 6f) _turbidity = 6f;
//        //    }
//        //    if (_inputs.IsKeyPressed(CONST_TV_KEY.TV_KEY_P))
//        //    {
//        //        _windPower.x = _windPower.y += speed * 0.00005f;
//        //    }
//        //    if (_inputs.IsKeyPressed(CONST_TV_KEY.TV_KEY_C))
//        //    {
//        //        _cloudsSize.x = _cloudsSize.y += speed * 0.00005f;
//        //        if (_cloudsSize.x < 0f) _cloudsSize.x = _cloudsSize.y = 0f;
//        //    }
//        //    if (_inputs.IsKeyPressed(CONST_TV_KEY.TV_KEY_I))
//        //    {
//        //        _layersOpacity.x += speed;
//        //        _layersOpacity.x = SkyMaths.Saturate(_layersOpacity.x);
//        //    }
//        //    if (_inputs.IsKeyPressed(CONST_TV_KEY.TV_KEY_O))
//        //    {
//        //        _layersOpacity.y += speed;
//        //        _layersOpacity.y = SkyMaths.Saturate(_layersOpacity.y);
//        //    }
//        //    if (_inputs.IsKeyPressed(CONST_TV_KEY.TV_KEY_F))
//        //    {
//        //        Core.timeFactor += speed * 100f;
//        //        if (Core.timeFactor < 0f) Core.timeFactor = 0f;
//        //    }
//        //}

//        //static void UpdateLines()
//        //{
//        //_turbLine.value = _turbidity;
//        //_windLine.value = _windPower.x;
//        //_sizeLine.value = _cloudsSize.x / 0.6f;
//        //_innerOpacLine.value = _layersOpacity.x;
//        //_outerOpacLine.value = _layersOpacity.y;
//        //_timeFactorLine.value = Core.timeFactor;

//        //}
//        //private static void LoadLayers(TVMesh mMesh)
//        //{
//        //    TV_TEXTURE tTex;
//        //    string sName;
//        //    string[] sName2;
//        //    int i;

//        //    i = mMesh.GetGroupCount() - 1;

//        //    while (i > -1)
//        //    {
//        //        tTex = Core.TextureFactory.GetTextureInfo(mMesh.GetTexture(i));

//        //        if (tTex.Filename != "blank")
//        //        {
//        //            sName = tTex.Filename;
//        //            sName2 = sName.Split('\\');
//        //            sName = sName2[sName2.GetUpperBound(0)];
//        //            sName2 = sName.Split('.');
//        //            sName = sName2[0];

//        //            if (Core.Globals.GetTex("tex" + sName + "norm") < 1)
//        //                Core.TextureFactory.LoadTexture(
//        //                    Application.StartupPath + "\\..\\..\\media\\models\\Blind\\" + sName + "_norm.bmp",
//        //                    "tex" + sName + "norm");

//        //            if (Core.Globals.GetTex("tex" + sName + "gloss") < 1)
//        //                Core.TextureFactory.LoadTexture(
//        //                    Application.StartupPath + "\\..\\..\\media\\models\\Blind\\" + sName + "_gloss.bmp",
//        //                    "tex" + sName + "gloss");

//        //            if (Core.Globals.GetTex("tex" + sName + "height") < 1)
//        //                Core.TextureFactory.LoadTexture(
//        //                    Application.StartupPath + "\\..\\..\\media\\models\\Blind\\" + sName + "_height.bmp",
//        //                    "tex" + sName + "height");

//        //            mMesh.SetTextureEx((int) CONST_TV_LAYER.TV_LAYER_1, Core.Globals.GetTex("tex" + sName + "norm"), i);
//        //            mMesh.SetTextureEx((int) CONST_TV_LAYER.TV_LAYER_2, Core.Globals.GetTex("tex" + sName + "height"), i);
//        //            mMesh.SetTextureEx((int) CONST_TV_LAYER.TV_LAYER_3, Core.Globals.GetTex("tex" + sName + "gloss"));
//        //        }

//        //        i--;
//        //    }
//        //}
//    }

//}
