using System;
using Keystone.Cameras;
using Keystone.Elements;
using Keystone.Resource;
using Keystone.Types;
using Keystone.Appearance;
using Keystone.Entities;
using KeyCommon.Traversal;

namespace KeyEdit.Workspaces.Huds
{
	/// <summary>
	/// Description of TacticalHud.
	/// </summary>
	public class TacticalHud : ClientHUD 
	{
        private Vector3d mLastCameraPosition = Vector3d.Zero();
        private bool mShowMotionField = true; // TODO: should be context.CustomOptions["show motion field"];
        private ModeledEntity mMotionField;
        private ModeledEntity mGalacticGrid;
        private Database.AppDatabaseHelper.StarRecord[] mRecords;

        public TacticalHud()
		{
            InitializePickParameters();

            LoadMotionField();
            LoadGalacticGrid();

        }


        private void InitializePickParameters()
        {

            mPickParameters = new PickParameters[1];

            KeyCommon.Flags.EntityAttributes excludedObjectTypes =
                KeyCommon.Flags.EntityAttributes.HUD;


            // recall that "excluded" types will skip without traversing children whereas "ignored" will traverse children
            KeyCommon.Flags.EntityAttributes ignoredObjectTypes =
                KeyCommon.Flags.EntityAttributes.Region;

            PickParameters pickParams = new PickParameters
            {
                T0 = AppMain.MINIMUM_PICK_DISTANCE,
                T1 = AppMain.MAXIMUM_PICK_DISTANCE,
                SearchType = PickCriteria.Closest,
                SkipBackFaces = true,
                Accuracy = PickAccuracy.Face,
                ExcludedTypes = excludedObjectTypes,
                IgnoredTypes = ignoredObjectTypes,
                FloorLevel = int.MinValue
            };

            mPickParameters[0] = pickParams;
        }
        // tactical utilizes icons of Vehicles (everything from satelites to space stations

        // tactical also has planetary overlays for areas where there is production
        // (production is everything from Universities to Starship Factories)

        // tactical QuickLook panel can show sortable contacts list


        // Scene.EntityAttached in TacticalWorkspace we look for Vehicle and player id

        public override Keystone.Collision.PickResults Pick(RenderingContext context, Ray r, KeyCommon.Traversal.PickParameters parameters)
        {
            Keystone.Collision.PickResults results = base.Pick(context, r, parameters);

            if (results.HasCollided)
            {
                if (results.Entity == mStarMap)
                {
                    int recordID = results.VertexID;
                    // TODO: results.SetEntity (mStarDigest.Records[recordID].ID, mStarDigest.Records[recordID].TypeName);
                }
            }
            return results;
        }


        public override void UpdateBeforeCull(RenderingContext context, double elapsedSeconds)
        {
            base.UpdateBeforeCull(context, elapsedSeconds);

            // Celestial Grid - need to render as Background3D and 
            if ((bool)context.GetCustomOptionValue(null, "show celestial grid"))
                AddHUDEntity_Immediate(context.Scene.Root, mGalacticGrid, true);

            LoadWaypointMarkers();

            // note: one rendering context per viewport and one hud per rendering context
            // so there is no need to unload things dynamically.  we only have to unload
            // when the rendering context (and thus viewport) is closed.
            // note: and viewmodes cant be changed.  a viewport must first be closed and then
            // recreated if for instance you wanted to simulate a view mode switch on an MFD
            if ((bool)context.GetCustomOptionValue(null, "show motion field"))
                UpdateMotionField(context.Viewpoint, context.Position, context.LookAt, elapsedSeconds);

            if ((bool)context.GetCustomOptionValue(null, "show star map"))
            {
                // TODO: no more scaling of root... that is causing too many problems.
                //       -instead, the "cam_zoom" is a zoom for a specific branch... say our 3dHUDRoot only
                //       or perhaps a specific parent node for just the starmap?
                //float cameraZoom = context.Viewpoint.CustomData.GetFloat("cam_zoom");
                //float scale = 1f / cameraZoom;

                //LoadStarMap();
                //UpdateStarMap(scale);
            }

            mLastCameraPosition = context.Position;
        }

        public override void UpdateAfterCull(RenderingContext context, double elapsedSeconds)
        {
            base.UpdateAfterCull(context, elapsedSeconds);

            if (mVehicle != null)
            {
                // draw velocity vector
                const double LENGTH = 100;
                Vector3d start = mVehicle.Translation - context.Position;
                Vector3d end = (Vector3d.Normalize(mVehicle.Velocity) * LENGTH) - context.Position;
                Color color = Color.White;
                Line3d[] line = new Line3d[1];
                line[0] = new Line3d(start, end);
                Keystone.Immediate_2D.Renderable3DLines renderableLine = new Keystone.Immediate_2D.Renderable3DLines(line, color);
                // TODO: verify this Viewpoint is the one attached to the Vehicle.

                AddHUDEntity_Immediate(context.Viewpoint.Region, renderableLine);
            }
        }

        Keystone.Vehicles.Vehicle mVehicle;
        ModeledEntity mWaypointMarker;
        Game01.GameObjects.NavPoint[] mNavPoints;
        Entity[] mWaypointProxies;

        private void LoadWaypointMarkers()
        {
            if (mVehicle == null)
            {
                Predicate<Node> match = e => e is Keystone.Vehicles.Vehicle;

                Node[] v = this.Context.Scene.Root.Query(true, match);
                if (v == null) return;

                // TODO: we should be matching against Vehicle.ID in cases where
                // there are more than one vehicle!
                mVehicle = (Keystone.Vehicles.Vehicle)v[0];
            }


            string texturePath = System.IO.Path.Combine(AppMain._core.DataPath, @"editor\navpoint.png");
 
            Game01.GameObjects.NavPoint[] navpoints = mVehicle.GetCustomPropertyValue("navpoints") as Game01.GameObjects.NavPoint[];
            if (haveNavpointsChanged(navpoints))
            {
                // rebuild 2d screenspace waypoint markers
                if (mWaypointProxies != null)
                    for (int i = 0; i < mWaypointProxies.Length; i++)
                        RemoveHUDEntity_Retained(mWaypointProxies[i]);

                if (navpoints != null)
                {
                    mWaypointProxies = new Entity[navpoints.Length];
                    for (int i = 0; i < navpoints.Length; i++)
                    {
                        mWaypointProxies[i] = Load2DProxy(navpoints[i], texturePath);
                        AddHudEntity_Retained(mWaypointProxies[i]);
                    }
                }
            }

            // update 2D screenspace waypoint markers' positions to take into account per frame camera rotation and translation
            // NOTE: since we grab navpoints custom property each frame, these navpoints already take into account any
            //       changes in the translation of those navpoints made in NavMap via mouse dragging.      
            if (mWaypointProxies != null)   
                for (int i = 0; i < mWaypointProxies.Length; i++)
                   UpdateWaypointProxies((ProxyControl2D)mWaypointProxies[i], navpoints[i]);

            mNavPoints = navpoints;
        }



        private bool haveNavpointsChanged(Game01.GameObjects.NavPoint[] navpoints)
        {
            if (navpoints == null && mNavPoints == null) return false;
            if (navpoints == null && mNavPoints != null) return true;
            if (navpoints != null && mNavPoints == null) return true;
            if (navpoints.Length != mNavPoints.Length) return true;

            return false;
        }

        private Entity Load2DProxy(Game01.GameObjects.NavPoint navpoint, string texturePath)
        {

            Model m = ContentCreation.Helper.Load2DScreenspaceIcon(texturePath);

            // Do we create a proxy, or an Entity?  For now we'll create a new Entity
            ProxyControl2D e = new ProxyControl2D("navpoint," + navpoint.RowID.ToString(), mVehicle); // (ProxyControl2D)Repository.Create("ProxyControl2D");

            e.AddChild(m);
            return e;
        }

        private void UpdateWaypointProxies(ProxyControl2D e, Game01.GameObjects.NavPoint navpoint)
        {
            // if this waypoint is in the same region, position is simple
            // TODO: what if the region that the first waypoint lies in isn't paged in?
            // TODO: just as we draw circle textures in 2D screenspace for planets too far away
            //       we should draw our waypoints the same way.

            // TODO: does using global coords instead of relative affect precision in Zones far from origin?
            // the problem is, we place these entities via AddHUDRetained under the root node and not
            // the Zone we're in, so we need galactic coords.
            Vector3d globalTranslation = Keystone.Portals.Zone.GlobalTranslationFromName((Keystone.Portals.ZoneRoot)this.Context.Region.Region, navpoint.RegionID);
            globalTranslation += navpoint.Position;
            Vector3d center = this.Context.Viewport.Project(globalTranslation - this.Context.Viewpoint.GlobalTranslation,
                                        Context.Camera.View,
                                        Context.Camera.Projection,
                                        Matrix.Identity());
            e.CenterX = (int)center.x;
            e.CenterY = (int)center.y;
            e.ZOrder = (int)center.z;
            e.Height = 20;
            e.Width = 20;

            // if (this.Context.Viewpoint.Region.ID == navpoints[i].RegionID)
            // {
            //mWaypointMarker.Translation = navpoints[i].Position;
            // }
        }

        private void LoadGalacticGrid()
        {

            // http://www.physics.csbsju.edu/astro/sky/sky.10.html
            if (mGalacticGrid == null)
            {
                float height = 40000f;
                float radius = 20000f;
                int slices = 25;


                string gridName = "galacticgrid_" + Repository.GetNewName(typeof(Entity));

                Keystone.Types.Color color = new Keystone.Types.Color(97, 106, 127, 255); // shuttle grey

                // TODO: the celestial sphere is blocking out the Grid lines... not sure why since it's transparent but it does
                //       seem to be doing that.
                // TODO: option to disable the celestial grid in menu along with "show grid"
                // TODO: when camera elevation moves, this grid needs to be projected differently...  
                //		 - since i wont be using right ascension and declination (since those are earth centric)
                //       the grid will always be in camera centric position when it's enabled.  
                //       - it is helpful for basic orientation knowing where the galactic center is, or system center
                //       etc
                mGalacticGrid = Keystone.Celestial.ProceduralHelper.CreateCelestialSphere(gridName,
                    radius, color);

            }



            // http://www.southernstars.com/support/manual/grids_reference.shtml
            // grid options 
            //            Grids & Reference
            //
            //The settings in this view let you show or hide grids which display the major celestial coordinate systems, as well as the reference lines and points that those systems are based on.
            //
            //Please Note: these settings are only found in SkySafari Plus and Pro.
            //Celestial Coordinate Grid
            //
            //Show Grid: Sets whether a celestial coordinate grid is displayed on the sky chart. When turned off, the following two items are disabled:
            //
            //    with Horizon Coordinates: displays an alt-azimuth coordinate grid on the sky chart.
            //
            //    with Equatorial Coordinates: displays a right ascension/declination grid on the sky chart.
            //
            //Reference Lines
            //
            //Celestial Equator: Sets whether the celestial equator is displayed on the sky chart. The celestial equator is the plane of the Earth's equator projected onto the celestial sphere.
            //
            //Galactic Equator: Sets whether the galactic equator is displayed on the sky chart. The galactic equator is the plane of the Milky Way galaxy projected onto the celestial sphere.
            //
            //Ecliptic Path: Sets whether the Ecliptic path is displayed on the sky chart. The Ecliptic is the plane of the Earth's orbit projected onto the sky. It is also the annual path of the Sun around the celestial sphere.
            //
            //Meridian Line: Sets whether the meridian is displayed on the sky chart. The meridian is the projection of your longitude on Earth onto the celestial sphere. It extends from the northern horizon through the zenith to the south cardinal point on the horizon. An object is said to transit when it crosses the meridian.
            //Reference Points
            //
            //Celestial Poles: Sets whether the celestial poles are displayed on the sky chart. The celestial poles are where the Earth's polar axis (i.e. the line perpendicular to the plane of the Earth's equator) intersects the celestial sphere. The north and south celestial poles are currently in the constellations Ursa Minor and Octans, but they move slowly over the centuries due to precession.
            //
            //Galactic Poles: Sets whether the galactic poles are displayed on the sky chart. The north and south galactic poles are where a line perpendicular to the plane of the Milky Way galaxy intersects the celestial sphere. They are currently located in the constellations Coma Berenices and Sculptor, respectively.
            //
            //Ecliptic Poles: Sets whether the ecliptic poles are displayed on the sky chart. The ecliptic poles are where a line perpendicular to plane of the Ecliptic intersects the celestial sphere. The north and south ecliptic poles are in the constellations of Draco and Dorado, respectively.
            //
            //Zenith & Nadir: Sets whether the zenith and nadir are displayed on the sky chart. This marks and labels the points directly overhead and underneath your feet.
        }

        private void LoadMotionField()
        {
            if (mMotionField == null)
            {
                int particleCount = 2500;
                int color = Keystone.Utilities.RandomHelper.RandomColor().ToInt32();

                float spriteSize = 0.25f;

                string texture = @"caesar\Shaders\Planet\star2.dds";
                // unique field name since each viewport can potentially have an indepedant motion field
                string fieldName = "motionfield_" + Repository.GetNewName(typeof(Entity));

                mMotionField = Keystone.Celestial.ProceduralHelper.CreateMotionField(fieldName,
                    Keystone.Celestial.ProceduralHelper.MOTION_FIELD_TYPE.SPRITE, particleCount,
                    texture, spriteSize, color);

                mMotionField.Translation = Vector3d.Zero();
                mMotionField.Name = null; // don't want label to render

                AddHudEntity_Retained(mMotionField);
                Repository.IncrementRef(mMotionField);
            }
        }

        Entity mScaleableMapOverlayRoot;
        ModeledEntity mStarMap;
        bool mStarMapInitialized = false;
        private void LoadStarMap()
        {
            // star citizen galaxy map
            // http://www.youtube.com/watch?v=9cLeXj4p03k
            // 
            if (mStarMapInitialized) return;


            try
            {
                mRecords = Database.AppDatabaseHelper.GetStarRecords();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("EditorHud.LoadStarMap() - Star Records not found. " + ex.Message);
            }

            // for single region scene, mRecords will be null
            if (mRecords == null)
            {
                mStarMapInitialized = true;
                return;
            }

            string id = Repository.GetNewName(typeof(ModeledEntity));

            // NavMap usees a version of the starMap that is NOT Background3D and
            // so does not follow camera.
            //        		mScaleableMapOverlayRoot = new ModeledEntity(id);
            //        		mScaleableMapOverlayRoot.Serializable = false;
            //        		id =  Repository.GetNewName( typeof(ModeledEntity));
            mStarMap = new ModeledEntity(id);
            mStarMap.Serializable = false;
            mStarMap.UseLargeFrustum = false;
            mStarMap.Dynamic = false;
            mStarMap.Name = null; // don't want the label rendering

            // TODO different Model in sequence for each Region?  For 100x10x100 = 100,000 models.  
            //      ideally, we'd have an octree for the model itself...  this is one of those things where
            //       we're so used to  octrees and spatial partitioning for entire entities and not for geometry
            //      that hurts us...
            //      Do we have any examples of spatially divided models?  Our celledRegion uses quadtree collections dont they?
            //      but it's only for the child entities that are added to the interior...
            //	        	id = Repository.GetNewName( typeof(Keystone.Elements.ModelSequence));
            //	            Keystone.Elements.ModelSequence sequence = new Keystone.Elements.ModelSequence(id);
            //	            sequence.Serializable = false;

            // note: entire sequence is direct models of interior and is NOT added to any quadtree.
            //       this also means that disabling rendering of certain floor meshes requires
            //       we either enable/disable sequences or modify model = Sequence.Select()
            //       to filter the models we dont want

            // TODO: Sept.23.2016 - let's say for now we postpone getting this working for our EditorHud and
            //       focus on what we need for our Navigation HUD.
            //		 - we still are working with StarDigest and we want to scale down the positions
            //       so that the universe dimensions fits entirely in the viewport.
            //			- this means we should focus on NEAR STAR style galaxies and not real galaxies.
            //				- more like Trek galaxies where there's a SANE number of star systems and worlds
            //				and where we can practically generate eb and flow of rise and fall of alien civilizations
            //				in order to generate procedural histories and cultures.
            //		- we also want the ability to mouse pick each star.
            //		- do we want to use an octree
            //			- i think yes, that is where we start and how does this fit with our Digest?
            //			- let's focus on StarDigest and Octree.  We have Octrees already for our Terrain.
            //			TileMap.Structure

            //		- we want to be able to overlay around star systems, icons that indicate owner, ships, stations in orbit, etc
            //		- we need to performance test our universe generation because it's taking WAY TOO LONG.
            //		- should we use an octree for faster mouse picking
            //			- should it use a seperate Scene
            //		- 6DOF camera movement
            //		- nebulae, galaxies in background
            //				

            // radius can be relatively small at 10k because background3d nodes are always rendered first and then depth buffer is cleared
            float RADIUS = 10000f;

            mRecords = Database.AppDatabaseHelper.GetStarRecords();
            Vector3d[] pointList = new Vector3d[mRecords.Length];
            for (int i = 0; i < mRecords.Length; i++)
                pointList[i] = mRecords[i].GlobalTranslation;

            const bool useTexture = true;
            Appearance appearance = null;
            if (useTexture)
            {
                const string texturePath = @"caesar\Shaders\Planet\stardx7.png";
                appearance = Keystone.Celestial.ProceduralHelper.CreateAppearance(MTV3D65.CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED, "tvdefault", texturePath, null, null, null);
                appearance.BlendingMode = MTV3D65.CONST_TV_BLENDINGMODE.TV_BLEND_ADD;
                // we dont want material if using vertex coloring
                appearance.RemoveMaterial();
            }

            // one mesh can only contain 65k vertices so we may need more than 1
            int modelCount = mRecords.Length / ushort.MaxValue;

            // TODO: colors to match star color rather than random
            // colors can be null
            int[] randomColors = new int[mRecords.Length];
            for (int i = 0; i < randomColors.Length; i++)
                randomColors[i] = Keystone.Utilities.RandomHelper.RandomColor().ToInt32();

            // TODO: would be nice when picking a pointsprite mesh, that the Model is aware so that if we want to special case
            //       the handling of the result vertexID to correspond with a Digest record for instance, we can.
            // TODO: this method .CreatePointSprite() does not supply an ID and the assumption is that any pointsprite with the same
            //       args is _the same_ mesh and that is just NOT necessarily the case.  But for now i doubt we'll see any collisions            
            const int spriteSize = 250;

            string relativePath = "caesar\\meshes\\pointsprites\\starmap.tvm";
            Mesh3d mesh = Mesh3d.CreatePointSprite(relativePath, pointList, randomColors, useTexture, spriteSize);
            string fullPath = System.IO.Path.Combine(AppMain.MOD_PATH, relativePath);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fullPath));
            mesh.SaveTVResource(fullPath);

            

            id = Repository.GetNewName(typeof(Model));
            Model starModel = (Model)Repository.Create(id, "Model");


            //	            sequence.AddChild(starModel);
            starModel.AddChild(mesh);

            if (useTexture)
                starModel.AddChild(appearance);

            //	            mStarMap.AddChild (sequence);
            mStarMap.AddChild(starModel);
            //	            mScaleableMapOverlayRoot.AddChild (mStarMap);

            Keystone.IO.PagerBase.LoadTVResource(mStarMap, true);

            // Add starmap to mHud3DRoot which means it's for this Viewport/RenderingContext only. <- TODO: is this true? If it's attached to the Root how do we not render it on all viewports?
            AddHudEntity_Retained(mStarMap); // NOTE: recall that StarDigest inherits Background3D so that it will automatically be bound to viewpoint and move with camera.

            mStarMapInitialized = true;
        }


        #region HUD GUI Elements Updates
        // TODO: update of this entity should occur via script
        private void UpdateMotionField(Entity parent, Vector3d cameraPosition, Vector3d cameraLookAt, double elapsedSeconds)
        {
            if (mShowMotionField && mMotionField != null && mMotionField.Model != null && mMotionField.Model.Geometry != null)
            {
                Keystone.Celestial.ProceduralHelper.MoveMotionField(mMotionField,
                    (Mesh3d)mMotionField.Model.Geometry,
                    cameraPosition, mLastCameraPosition, cameraLookAt, elapsedSeconds);
            }
        }


        private void UpdateStarMap(float scale)
        {
            if (mStarMap != null)
            {
                bool showLabels = this.Context.ShowEntityLabels;

                //        		mScaleableMapOverlayRoot.Scale = new Vector3d (scale, scale, scale);

                //	so there's a conflict here in design where we have real world global coords used that will autoscale
                //    for us as we zoom out..
                // TODO: if we are not modifying the local translation of each record to be in cameraspace position
                //       - then why are we inheriting from Background3D?

                if (scale == 1.0f)
                {
                    mStarMap.GlobalTranslation = this.Context.Viewpoint.GlobalTranslation;
                }
                else
                    mStarMap.GlobalTranslation = Vector3d.Zero();

                // if source of the flag is a child or self (and not a parent), notify parents

                Mesh3d mesh = (Mesh3d)mStarMap.Model.Geometry;
                int recordCount = 0;
                if (mRecords != null)
                    recordCount = mRecords.Length;

                // every record needs to have it's GlobalTranslation recomputed
                // typically this is something that rarely has to be done for a Digest as it's attached as
                // direct child of a Region where scales or other transforms rarely occur
                for (int i = 0; i < recordCount; i++)
                {
                    // TODO: what if the HUD used the digest to create HUD proxy representations of the zoomed galaxy?
                    //		- when the zoom was != 1.0d
                    //		- TODO: can we seperate our the Model of StarDigest from the StarDigest itself?  Like why can't we create the background _OR_ zoomed visual
                    //        seperately on the fly as part of the HUD itself as proxies?  The models need not be inherit to the Digest at all....
                    //        - this way we can easily use seperate visuals for zoom.

                    // TODO: is there a way for the Digest to maintain links to requisite Regions or manage a 'record' for that region as well?
                    //       - this is a digest issue is it not?   Thought it's true not all Digests need to span multiple regions, they still might.
                    //       e.g. we may have one Digest manage economies or civilian npcs from multiple worlds on systems across all zones.
                    //       - we wouldnt want some hierarchy of Transform node proxies would we? 
                    //         we dont really want that sort of simulation do we?
                    //       - maybe we require .Translation always be unZoomed GlobalTranslation?
                    //			- even though oribt location may still be off

                    // TODO: .Translation here is already a GlobalTranslation really so its kind of tricky... it's the unscaled GlobalTranslation and
                    //       not even DerivedTranslation...
                    //       where as now .GlobalTranslaion means scaled GlobalTranslation.  If i had the hierarchy for each record it'd be easier
                    //      - indeed. what we would need is regional data... and from that we could get global 


                    // radius can be relatively small at 10k because background3d nodes are always rendered first and then depth buffer is cleared
                    const double RADIUS = 10000;
                    Vector3d direction = mRecords[i].Translation - this.Context.Position; // this.Context.Viewpoint.GlobalTranslation;
                    Vector3d position;
                    double distanceSquared = 0;

                    if (scale == 1.0f)
                    {
                        // we're inside the sphere and can use projected vertex coordinates
                        //direction -= this.Context.Viewpoint.GlobalTranslation;
                        distanceSquared = direction.LengthSquared();
                        position = Vector3d.Normalize(direction);
                        position *= RADIUS;
                    }
                    else
                    {
                        // TODO:  HOWEVER, once we move out of the sphere, we CANNOT use projection on a sphere!  As this means all stars will be visually
                        //       plastered around a sphere!  We must use real positions and NOT any projection.
                        //	     this also means however, that these distances become incredible huge!  
                        position = direction;
                        distanceSquared = direction.LengthSquared();

                        direction *= scale;
                        direction -= this.Context.Viewpoint.GlobalTranslation;
                        // TODO: picking seems to fail here... is it the scaling applied to the root hud3d entity
                        //       or is it the lack of scaling being applied to the start ray coordinate?
                        //       this.Context.Viewpoint.GlobalTranslation and instead .Translation is used which
                        //       is clearly wrong since the starmap is placed on mHUD3Delement which is on ZoneRoot
                    }


                    // TODO: why aren't the pointsprites rendering. Is it getting culled?	
                    //		- it appears to render then disappear when alternating between Nav View and Main Viewer (Tactical) view. 
                    //			what would cause that?	we know the mesh is loading. 
                    //			we know the coords are correct.
                    //			i think we know that it's not being culled unexpectedly.
                    //			could it be material? don't think so.
                    //			don't think it's shader either, using tvdefault shader.	
                    //			is there a rendering state that is getting set that shouldn't be? A state that
                    //			is not set in the Nav view but is in the Editor view.
                    //								
                    //			Converted ModeledEntity to Background3d and now the stars dont show even
                    //			when switching between Nav and Editor workspaces.
                    //			Or is it just flat out not rendering?					
                    // we don't necessarily need to move the vertices if camera hasn't moved 		
                    if (mLastCameraPosition != this.Context.Position)
                        mesh.SetVertex(i, position);

                    if (showLabels)
                    {
                        // draw immediate mode labels				
                        string distanceLabel = GetDistanceLabel(distanceSquared);
                        CreateLabels(position, mRecords[i].Name, distanceLabel, DistanceLabelColor.ToInt32());
                    }
                }
            }
        }
        #endregion

        #region IDisposeable
        public override void Dispose()
        {
            base.Dispose();

            RemoveHUDEntity_Retained(mMotionField);
            Repository.DecrementRef(mMotionField);

 // pasted from Editor.Hud.Dispose() 
 //           if (mVehicleMenu != null)
 //               Repository.DecrementRef(mVehicleMenu);
        }
        #endregion

    }

    // for now, let's place a 360 xz and 360 xy texture planes to show the ship's heading and bearing
    // and get our thrusters to move precisely to desired heading and bearing

    // - get our single zone scene running where we can load prefabs and position them
    //      - this probably means fixing our placement and rotation and scaling tools
    // - get the thruster and engine plumes positions, scales and rotations.  
    //      - do we add these to the prefab and disable them until needed by script?


}
