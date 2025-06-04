using System;
using System.Collections.Generic;
using Keystone.Entities;
using Keystone.Appearance;
using Keystone.Elements;
using Keystone.Types;
using Keystone.Cameras;
using KeyEdit.ContentCreation;


namespace KeyEdit.Workspaces.Huds
{
    public class ClientHUD : Keystone.Hud.Hud 
    {
        #region mouse events
        protected virtual void Proxy_OnMouseEnter(object sender, EventArgs args)
        {
            // set the transform/axis mode

            // TODO: here we might do something like change the material color of the control to it's RollOver state color
            System.Diagnostics.Debug.WriteLine("ClientHUD.Proxy_OnMouseEnter - " + ((ProxyControl2D)sender).ID);

            // change the material to roll over
        }

        protected virtual void Proxy_OnMouseLeave(object sender, EventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("ClientHUD.Proxy_OnMouseLeave - " + ((ProxyControl2D)sender).ID);

            // change the material's back to default
        }


        protected virtual void Proxy_OnMouseDown(object sender, EventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("ClientHUD.Proxy_OnMouseDown - " + ((ProxyControl2D)sender).ID);
        }

        protected virtual void Proxy_OnMouseUp(object sender, EventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("ClientHUD.Proxy_OnMouseUp - " + ((ProxyControl2D)sender).ID);
        }

        protected virtual void Proxy_OnMouseClick(object sender, EventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("ClientHUD.Proxy_OnMouseClick - " + ((ProxyControl2D)sender).ID);
        }
        #endregion
        /// <summary>
        /// Takes a list of Entities and creates Proxies that are rendered in 2D and can trigger mouse event handlers
        /// </summary>
        /// <param name="context"></param>
        /// <param name="entities"></param>
        /// <param name="match"></param>
        /// <param name="iconTexturePath"></param>
        /// <param name="material"></param>
        /// <param name="recurseChildrenAndSkipParentIfCloseEnough"></param>
        /// <param name="mouseEnter"></param>
        /// <param name="mouseLeave"></param>
        /// <param name="mouseDown"></param>
        /// <param name="mouseUp"></param>
        /// <param name="mouseClick"></param>
        protected void IconizeNonRecursive(RenderingContext context, Entity[] entities, Predicate<Entity> match, 
            string iconTexturePath, Material material, bool recurseChildrenAndSkipParentIfCloseEnough,
            EventHandler mouseEnter, EventHandler mouseLeave,
            EventHandler mouseDown, EventHandler mouseUp,
            EventHandler mouseClick)
        {

            if (entities == null) return;

            
            for (int i = 0; i < entities.Length; i++)
            {
                Entity entity = entities[i];
                // create the icon for this entity but do not recurse it's children
                // note: we pass the events to the CreateProxy call
                //       because if the proxy was cached we do not want to wire another
                //       instance of the event 
                ModeledEntity proxy = Helper.Create2DProxy(context.Viewport.TVIndex, entity, iconTexturePath, material,
                                                            mouseEnter, mouseLeave, mouseDown, mouseUp, mouseClick);

//                    ModeledEntity proxy = Helper.Create3DProxy(context.Viewport.TVIndex, entities[i], defaultMaterial);

                // add as pickable RETAINED MODE 
                if (proxy.Parent != this.mHud3DRoot && proxy.Parent != this.mGUI2DRoot)
                    AddHudEntity_Retained(proxy);

//                    if (entity is Keystone.Vehicles.Vehicle && 
//                        entity.Parent != null &&))
//                    {
//                    	// vehicle orbits cant really use eliptical because it's using forces to compute
//                    	// TODO: can we find a way to compute the path faster than trying to calc for every x tick in the future?
//                    	
//                    }
                    
                // The original proxy is added as RETAINED MODE but below we 
                // add IMMEDIATE MODE (non pickable) orbit and altitude lines
                if (entity as Keystone.Celestial.World != null && 
                    entity.Parent != null &&
                    (bool)context.GetCustomOptionValue (null, "show orbit lines"))
                {
                    float cameraZoom = context.Viewpoint.BlackboardData.GetFloat("cam_zoom");
                        	
			        if (cameraZoom != -1.0f)
			        {
			            	
			            Keystone.Celestial.Body body = (Keystone.Celestial.Body)entity;
		                    
							// NOTE: since our orbit uses actual TVMesh, the orbit will scale automatically when we scale the scene during zooms
						//Keystone.Entities.ModeledEntity orbitRoot = Helper.Create3DProxyBare (context.Viewport.TVIndex, "orbit_root", body.ID);							 
						Keystone.Entities.ModeledEntity orbitProxy = Helper.Create3DProxyBare (context.Viewport.TVIndex, "orbit", body.ID);
						Keystone.Entities.ModeledEntity circleEntity =  Helper.Create3DProxyBare (context.Viewport.TVIndex, "world_smooth_circle", body.ID);
						Keystone.Entities.ModeledEntity altitudeEntity =  Helper.Create3DProxyBare (context.Viewport.TVIndex, "world_altitude_line", body.ID);
						  	
						if (orbitProxy.Model == null)
						{
						  			
						  	orbitProxy.Name = "#ORBIT"; // null; // we don't want this rendering a label
  			                int segmentCount = 100;
  			                                
				            Keystone.Types.Color color = new Keystone.Types.Color(100, 149, 237, 255);
				           	color = new Keystone.Types.Color(176, 196, 222, 255); // light steel blue
			            	color = new Keystone.Types.Color(70, 130, 180, 255); // steel blue
            				color = new Keystone.Types.Color(102, 102, 102, 255); // dove gray
            				color = new Keystone.Types.Color(224, 233, 233, 255); // baby blue
            				color = new Keystone.Types.Color(97, 106, 127, 255); // shuttle grey
            				//color = new Keystone.Types.Color(255, 26, 0, 255); // scarlet  <-- nice color for enemy ship markers
            					
            				bool quadLines = false;
						  	Keystone.Celestial.ProceduralHelper.InitOrbitLineStrip(orbitProxy, segmentCount, body.OrbitalRadius, body.OrbitalEccentricity,
	                    	                                                       	body.OrbitalInclination, body.OrbitalProcession, color, quadLines);
  			                    
  			                     
  			                // a circular endpoint to the line against the grid to make it easier to visualize where it touches the grid in relation to any other
  			                // object such as ships from mesh makes scaling it automatic with camera zoom
  			                circleEntity.InheritScale = true; // if we scale the entire solar system, why shouldn't the orbit's inherit that scale?  
            				circleEntity.InheritRotation = false;
                            bool dashedSegments = false;
  			                Model smoothCircleModel =  Keystone.Celestial.ProceduralHelper.CreateSmoothCircle ( segmentCount, color, dashedSegments, quadLines);
  			                double mag = 1; //1000d;
  			                smoothCircleModel.Scale = new Vector3d (body.Radius * mag, body.Radius * mag, body.Radius * mag);
  			                circleEntity.AddChild (smoothCircleModel);
  			                circleEntity.Name = null; // we don't want this rendering a label
  			                    
  			                // an altidue line from body to grid created from mesh (as opposed to 2d line) makes scaling it automatic with camera zoom 	
  			                altitudeEntity.Name = null; // we don't want this rendering a label  			                    
  			                altitudeEntity.InheritScale = true;
  			                altitudeEntity.InheritRotation = false;
  			                    
  			                Vector3d start = body.GlobalTranslation * cameraZoom;
  			                // we only want the height
  			                start.x = 0;
  			                start.z = 0;
  			                   
  			                Vector3d end = start;
  			                end.y = 0;
  			                    
  			                // TODO: actually, we may end up going with immediate lines because trying to reposition the verts as world orbits may be too annoying
  			                Model altitudeLineModel =  Keystone.Celestial.ProceduralHelper.CreateLine (start, end, color, quadLines);
  			                altitudeEntity.AddChild (altitudeLineModel);
  			                    
  			                    
  			                //orbitRoot.AddChild (orbitProxy);
  			                //orbitRoot.AddChild (circleEntity);
  			                //orbitRoot.AddChild (altitudeEntity);
						}
						  	
						// TODO: if these lines are not scaled, and if not rendered to large frustum, then i dont think we'll be able to see them.
						//       before when we were scaling the Root node of the scene, adding these orbit lines would also be scaled, but now they are not
						//       and we are not scaling them manually.
						// TODO: is the max_visible_Range of these orbit lines too far?  and preventing them from being scaled to fit within small far plane?
						//		- and is there no way to fix that maxvisiblerange?
						  	
						// TODO: does the following AddHudEntity_Retained fail since we're unable to set retained Parents? Why on earth am I not
						//       allowing nested?  What happens if i add only the root Entity? 						  	
						// TODO: what if these orbit's were not added to mHud3DRoot but to a scaled parent
						//       that is at origin such as mScaledOrbitsRoot and then i can scale that
						double scale = 1 / cameraZoom;							
						//orbitRoot.Scale = new Vector3d (scale, scale, scale);
						//if (orbitProxy.Parent != this.mHud3DRoot)
						//	AddHudEntity_Retained (orbitProxy);
							AddHUDEntity_Immediate (context.Scene.Root, orbitProxy, false);
							
						//if (circleEntity.Parent != this.mHud3DRoot)
						//	AddHudEntity_Retained (circleEntity);
							AddHUDEntity_Immediate (context.Scene.Root, circleEntity, false);
								
						//if (altitudeEntity.Parent != this.mHud3DRoot)
						//	AddHudEntity_Retained (altitudeEntity);
							AddHUDEntity_Immediate (context.Scene.Root, altitudeEntity, false);
						// TODO: when adding to Immediate, Scene.Root must be used since all RegionPVS must use a Region node
						//       
							
//							if (orbitRoot.Parent != this.mHud3DRoot)
//								AddHudEntity_Retained (orbitRoot);
							
						// orbit's are centered at the PARENT body's translation (eg. Earth's orbit mesh is centered about Sol's position.  Ganymede's orbit is centered around Jupiter)
						// TODO: perhaps if we scale the orbit lines to near plane, always scale 2d lines to near plane, then render them using a PVS that gets them in relative correct spot
						//       - same with mesh grid, but for grid losing our 3d ztesting is bad... you cant tell when ships are above or below
						// TODO: why is it that jupiter's moon's orbitProxies seem to be centered about Sol and not Jupiter?
						//       it is because we are grabbing the scaled .GlobalTranslation and then scaling it again via mRoot's scale. 
						// TODO: similarly when placing Vehicle during scaled view, is it screwing up the placement coordinate? not sure what is going on there yet		
						//       indeed, this is why my viewpoint also takes on the scale... so everything fits in the frustum.  If i was only scaling down models and
						//       and not ALSO the viewpoint position, then the viewpoint would still be far away and nothing would fit	
							
						// scale the distance from camera down along with the size
						Vector3d translation = body.Parent.GlobalTranslation; // * cameraZoom ;  
						// TODO: I hate having to interpret the .GlobalTranslation based on the camerazoom! i have to do that
						// EVERYWHERE!!!  Even in our simulation?  UGH!  Seems very error prone.  It's fine if we only need local translation, but even regional translation
						// will have the scaling in it!  We'll worry about it after we finish up HUD, Camera/Viewpoint and our various Views
						// Tactical x10, x100, x???
						// Planetary x10, x100, etc
						// System x10, 100, x1000
						// Galactic x10, x100, x1000
						//
						// c) scaling the entire scene via it's root means thatother open viewports will use that scale when i intend to have independantscaling ability for any open viewport
			            // d) whenever switching to interview, we'd have to check the scale was 1,1,1 on root.
			            //    - actually i dont have to check, i believe i do it automatically at start of cull traversal
						orbitProxy.Translation = translation;
						translation = body.GlobalTranslation; // * cameraZoom;  
						translation.y = 0;
						circleEntity.Translation = translation;
							
						// TODO: we adjust translation but not the altitude as worlds orbit their altitudes may change (eg pluto)
						//       it may end up being easier to use immediate 3d lines here since otherwise we have to tweak vertex lines
						//       or perhaps if we create a unit line, then we can scale it to scale = halfAltidude = altidude * 0.5; and position it at position.y = halfAltitude
						altitudeEntity.Translation = translation;
			        }
                }
                    
                // distance to actual entity, not proxy
                double distanceToCameraSq = context.Viewpoint.DistanceToSquared (entity); 

                // disable this proxy if underlying entity is now close enough to be rendered
                //   if (distanceToCameraSq <= entities[i].MaxVisibleDistanceSquared)
                //       proxy.Enable = false;
                //   else 
	                proxy.Enable = true;
                // if distance to is close enough, we can disable parent proxy
                // and render proxy icons for children instead (i.e. Jupiter is now visible but it's tiny moons can
                // now use proxies)
                // TODO: if we get too far away from our Star, the distanceToCameraSq <= will fail and we will stop attempts at traversing children!
                //       and the proxy will never get disabled and so the actual body of say Uranus or Neptune (both farther than Jupiter from star) will never render.
                //       However the idea here is that our culler will have found the real body, but if a proxy exists for it, it will not add it to the PVS.
                //       But our culler will not find the real body if too far and that is why we do a seperate query for types of "Body" to see see 
                //       if we should render an icon for it.   I think part of what we're doing here is a bit convoluted.  We should be building up a catalog
                //       of all bodies rather than doing the Query each time.  
                if (recurseChildrenAndSkipParentIfCloseEnough )
                {
                    // we don't automatically recurse children.  We want to do that
                    // manually after we've verified distance check 
                    List<Entity> childResults = entity.SceneNode.Query(false, match);
                    IconizeNonRecursive(context, childResults.ToArray(), match,
                        iconTexturePath, material, recurseChildrenAndSkipParentIfCloseEnough,
                        mouseEnter, mouseLeave,
                        mouseDown, mouseUp,
                        mouseClick);
                }
                

                // TODO: depending on the size of the computed iconScale, does the proxy get rendered to 
                //       either the big or small frustum?  if not should it?
                if ( proxy.Enable == true)
                {
                    // // if this is using Billboard.cs geometry
                    // double iconScale = context.GetConstantScreenSpaceScalePerspective( Math.Sqrt (distanceToCameraSq), 0.025f); ;
                    // Vector3d vecScale;
                    // vecScale.x = iconScale;
                    // vecScale.y = iconScale;
                    // vecScale.z = iconScale;
                    // proxy.Scale = vecScale;

                    // proxy's are placed always on Root and so have to be positioned in global coordinates 
                    proxy.Translation = entity.GlobalTranslation; 
                    if (proxy is Proxy3D)
                    {

            	        // question: how do we add a orbital path if a planet is not visible in the frustum?
				        // how does the hud know to render a orbit path "icon" for it?
				        // I suppose the simple answer is, orbit paths are associated with planets yes
				        // but they are part of the "StellarSystem" perhaps... or maybe the Region it sits.
				
				        // TODO: HOwever, can we add OrbitalPath models to StellarSystem when a child star/world
				        // is added anywhere below it's hierarchy.  This would be something that I think we wont
				        // end up doing on a HUD way because it could be too slow to recreate these meshes everytime
				        // or to dig them from caches and determine when they fall out of scope.  There's nothing wrong
				        // with making those orbit paths a model and one which can be disabled as a "LAYER"
				
				        // these 3d elements are rendered at a fixed position relative to our camera.
				        // we use an Identity camera when rendering these elements so that there is never any need
				        // to worry about unprojecting a 2d point and computing the forward vector, etc.  Our forward
				        // vector will always simply be 0,0,1.
				
				        // these 3d elements are placed in the scene and depending on whether the element
				        // has EntityFlags.HUD or EntityFlags.HUD_ANCHORED then it will be added to appropriate RegionPVS bucket
            
				    	// TODO: rather than zoom by raising scale of 3d proxies, let's try for now to see if our draw traverser
				    	//       can do this...or what if i just added a scale to the root?  well that would scale both distance and size which we dont want
				    	// TODO: by scaling world size and not distance, we can end up with scenarios where the proxies for moons are enveloped by the proxy for the 
				    	//       pair world (eg jupiter).
				    	//		 - scaling the planet is fine when in solar system view, but in planetary view, the planet's scale should probably not be scaled at all
				    	// TODO: one of the ADVANTAGES to scaling the distances down though is it makes using a GRID that encompasses entire system so much easier.
				    	//          including ability to have large 3d objects above and beneath it and not having to use multiple frustums (i.e. near & far).  
				    	//       - so question then perhaps is, can we transition smoothly from one view to the other?
				    	//       - where we scale down distances, scale up worlds for solar system view and render an icon to indicate moons or moon count, 
				    	//       - Fix isometric horizontal angle, but allow vertical angle to orbit, and allow to zoom somewhat closer to a specific target that is selected
				    	//       in that view such as ship, satellite, moon, etc. 
						//       - also, for nav, it'd be better to scale distance down by x0.5 for worlds orbiting at distances beyond 10AU so that they all fit on screen
						//			- perhaps even to scale based on logarithmic distance to keep all worlds in the system within the small frustum so grid use is easy.
						//		- tactical is always limited to planetary or closer where we do not need to use any non uniform distance scaling
						// - the zoom views must always use orbit viewpoint behavior and so much always have a target even if it's center of system.  This also
						//   provides a reference for which to zoom especially when we want to animate camera to proper zoom position 
						// - can the initiator for these different scale settings be our Zoom multiplier menu setting?
						//		- so from 1:1 scale, we want to move to 1:10 scale (aka zoom x10), distances now all scale by 1/10
						//		  but the camera distance cannot scale by 1/10 or the zoom will be negated.  But perhaps the camera speed will be
						//        scaled to account for the zoom setting   <-- I think this is a good starting point
						//        - though still not sure yet how this affects normal cull traverser?  if we do add this to the root
						//			then here we only ever need to scale sizes differently potentially and/or distances for those beyond 10AU.
						//			- certainly a 1/10 scale of the root node could help?  Or even what if we did that to the a celestial proxy root node?
						// TODO: Google View Scaling DirectX9 or D3D- i thought i could apply to the View or InverseView matrix but seems to have no affect

						// when scaling scene Root via Camera Zoom modes, it will scale both translation and size of the child entities.  To make our proxies
						// larger than the downscaled entities, we can use the original celestial body radius as scale
						if (entity is Keystone.Celestial.Body)
						{
							double radius = ((Keystone.Celestial.Body)entity).Radius;
							proxy.Scale =  new Vector3d (radius, radius, radius );
						}
                        // proxy.Scale = entities[i].Radius * zoom;
                    }
                        
                    System.Diagnostics.Debug.Assert(proxy.Dynamic == false);
                    System.Diagnostics.Debug.Assert(proxy.Serializable == false);
                        
                    // if we're using TexturedQuad2D geometry we need to compute screenspace center coordinates
                    // WARNING: if we've added 3D geometry to an ProxyControl2D which is derived from Control2D, then it will have
                    //          bad z axis scaling because of how it handles RegionMatrix (2D controls ignore parent's Matrix and treat it's own position and size as being in absolute coords)
                    if (proxy.Model.Geometry is TexturedQuad2D)
                    {
	                    ProxyControl2D inputProxy = (ProxyControl2D)proxy;
                        // TODO: does using global coords instead of relative affect precision in Zones far from origin?
	                    Vector3d center = this.Context.Viewport.Project (proxy.Translation - this.Context.Viewpoint.GlobalTranslation,
                                                    Context.Camera.View,
                                                    Context.Camera.Projection,
                                                    Matrix.Identity());
	                    inputProxy.CenterX = (int)center.x;
	                    inputProxy.CenterY = (int)center.y;
	                    inputProxy.ZOrder = (int)center.z;
	                    inputProxy.Height = 20;
	                    inputProxy.Width = 20;
                    }
                }
            }
        }


        public override VisibleItem? Iconize(LightInfo lightInfo, double iconScale)
        {
            base.Iconize(lightInfo, iconScale);

            //// TODO: ideally, i can enable/disable the drawing of these various icons
            ////       but really, that is a HUD swapping thing.  So i should make it so i can
            ////       when in Editor, switch from Editor hud to a game hud.
            ////
            //// DISABLED - We now use proxy icons during Update for stars.
            ////            Although this is for testing nav hud icons.  For edit mode
            ////            ultimately we do want to have light icons showing for lights 
            ////            so that we can click on the icon to modify it's properties
            ////            But that too suggests those icons should be added in a similar way
            ////            during update() so they are in the scene and pickable.
            ////            NOTE: I think now it's a matter of EDITOR, vs GAME vs NAV
            ////            We are tempoarily adding GAME hud overlays and I think its ok
            ////            to add all ship contacts in EDITOR without SENSORs 
            ////            
            //// item.entity, item.model, item.cameraSpacePosition, item.DistanceToCameraSq
            //if (item.Light != null)
            //{
            //    // create a light proxy icon entity 

            //    // do we load light proxy icon elements on creation of this
            //    // EditorHud instance and render instances of them as needed?

            //    // but for other resources, that can't be pre-loaded and instead
            //    // we must load dynamically and then rely on caching to avoid recreating
            //    // how do we know when to unload them when no longer needed 
            //    // (eg traveled to a new stellar system)
            //    // and how can we determine if the item already exists for a given entity?
            //    // is dictionary the best option?


            //        string texturePath = @"E:\dev\c#\KeystoneGameBlocks\Data\editor\sun.png";

            //        Proxy proxy = CreateProxy(item.Light, texturePath);
            //        //string texturepath = @"E:\dev\c#\KeystoneGameBlocks\Data\editor\sun.png";
            //        //lightProxy.AddChild(Load2DScreenspaceIcon(texturepath, texturepath));

            //        // TODO: NOTE: interesting thing here is the proxy is actually added to the scene
            //        //       which will make it pickable even if it's just added to an overlay scene
            //        //       but it also requires we clone this instance or somehow recylce it frame
            //        //       to frame.  Ideally, i think we'd have to associate the proxy
            //        //       with the actual entity we are iconifying
            //        // TODO: I think this is incorrect.  Here the icon should be added to the
            //        // PVS as a IRenderable2d element and NOT to the scene!
            //        // In fact, Iconize should return the Proxy and then the caller can add
            //        // that to the RegionPVS.  But otherwise, no, this should not be added.
            //        // Then the question becomes, how do we recycle the Entity and Model
            //        // so we dont have to recreate it with only ability to share 
            //        //AddHudEntityToScene(lightProxy, mContext.Region, true);
            //        //lightProxy.Translation = item.CameraSpacePosition;



            //    //else
            //    //{
            //    //    // 2d screenspace textured quad version
            //    //    Vector3d toItem = Vector3d.Normalize(item.CameraSpacePosition);
            //    //    Vector3d result = mContext.Viewport.Project(toItem);
            //    //    Vector2f starPosition2D;
            //    //    starPosition2D.x = (float)result.x;
            //    //    starPosition2D.y = (float)result.y;

            //    //    lightProxy.Translation = result;
            //    //}


            //    Vector3d vecScale;
            //    vecScale.x = iconScale;
            //    vecScale.y = iconScale;
            //    vecScale.z = iconScale;
            //    proxy.Scale = vecScale;

            //    VisibleItem vi = new VisibleItem(proxy, proxy.Model, item.CameraSpacePosition);
            //    return vi;
            //}

            return null;
        }



        // iconizeVisible uses visible items found during cull traversal
        // iconizeNear uses a query
        // NON PICKABLE icons that are added directly to the render buckets 
        // and never inserted as node into scene.
        // However, there's no reason I can't modify this code here to insert and cache
        // 
        public override VisibleItem? Iconize(VisibleItem visibleItem, double iconScale)
        {
            if (visibleItem.Entity is Proxy3D || visibleItem.Entity is ProxyControl2D || visibleItem.Entity is Proxy2D) return null;

            bool iconize = false;

            if (visibleItem.Entity is DefaultEntity && AppMain._core.Scene_Mode == Keystone.Core.SceneMode.EditScene)
                iconize = true;
            // visibleItem.entity, visibleItem.model, visibleItem.cameraSpacePosition, visibleItem.DistanceToCameraSq

            // NOTE: The problem with the below is, these icons are NOT pickable!  HOWEVER
            //       There is no reason i can't just add them right here when in Edit Mode
            //       the same was as Update().  No reason at all.  
            //if (visibleItem.Entity is Keystone.Vehicles.Vehicle)
            //{
            //    // only show an icon if the item is too far to be seen otherwise
            //    double max_distance_squared = 50000;
            //    if (visibleItem.Entity is Keystone.Celestial.World)
            //        max_distance_squared = item.Entity.MaxVisibleDistanceSquared;
            //    else
            //        max_distance_squared *= max_distance_squared;

            //    if (visibleItem.DistanceToCameraSq <= max_distance_squared) return null;

            if (iconize)
            {
                string texturepath = System.IO.Path.Combine(AppMain.DATA_PATH, "editor\\hud_uknown_vessel_type.png");
                texturepath = System.IO.Path.Combine(AppMain.DATA_PATH, "editor\\icons\\repair.jpg");

                Material mat = Material.Create(Material.DefaultMaterials.white);
                IconizeNonRecursive(this.Context, new Entity[] { visibleItem.Entity }, null,
                    texturepath, mat, false,
                    null, null,
                    null, null,
                    Proxy_OnMouseClick);

                return new VisibleItem(); // 
                // NOTE: this path returns a dummy VisibleItem because the above call to IconizeNonRecursive() generates
                //       proxies that are added to the HUD and NOT Immediate Mode items that need to be added to the PVS.
                //       Thus the caller of this function
            }
            else
            { 
            //    EntityProxy proxy = CreateProxy(visibleItem.Entity, texturepath);  
            //    // if we are adding this proxy directly to the scene (which we should not, i believe
            //    // we already have the ship in the scene, so we should add temporary hud entry
            //    // and then do picking independantly within the HUD?) then we the mShipProxy.ScreenSpaceSize
            //    // will automatically be used.  Otherwise, below the argument passed in iconScale
            //    // is used.

            //AddHudEntityToScene(mShipProxy, mContext.Region, true);


            //    // The passed in iconScale is used IF this proxy is not added to the scene but is instead
            //    // using immediate mode rendering by adding as a VisibleItem to the PVS directly.

            //    Vector3d vecScale;
            //    vecScale.x = iconScale;
            //    vecScale.y = iconScale;
            //    vecScale.z = iconScale;
            //    proxy.Scale = vecScale;


            //    // TODO: This item needs to be added to FAR frustum, but i think it's not because
            //    //       the entity itself is culled. So this Iconify is never called. So
            //    //    the bug is, when the distance from ship to camera is beyond farplane, the
            //    //    hud icon of it will not get drawn.
            //    return new VisibleItem(proxy, proxy.Model, visibleItem.CameraSpacePosition);
            }
            return null;
        }
    }
}
