using System;
using System.Collections.Generic;
using KeyCommon.Flags;
using KeyCommon.Traversal;
using keymath.DataStructures;
using Keystone.Cameras;
using Keystone.Collision;
using Keystone.Elements;
using Keystone.Entities;
using Keystone.Lights;
using Keystone.Octree;
using Keystone.Portals;
using Keystone.QuadTree;
using Keystone.Types;

namespace Keystone.Traversers
{
    /// <summary>
    /// Provides Ray Space picking of scene entities.
    /// </summary>
    public class Picker : ITraverser
    {
        private KeyCommon.Traversal.PickParameters _parameters;
        

        private float TV_PICK_SCALING; // since TV's mesh.AdvancedCollide expects start and end points and not just start + direction vector, we compute the end point by scaling the normalized direction and adding that to the start position

        private SingleLinkedList<PickResults> mFoundItems;
        private IPickResultComparer mNearComparer;
        private IPickResultComparer mFarComparer;
        private IPickResultComparer mDeepestNestedComparer;

        private Viewport mViewport;
		private Region mRayOriginRegion;
		
        private Ray mRegionSpaceRay;
        private Ray mModelSpaceRay;


        public Picker()
        {
            mFoundItems = new SingleLinkedList<PickResults>();
            mNearComparer = new PickNearComparer();
            mFarComparer = new PickFarComparer();
            mDeepestNestedComparer = new PickDeepestNestLevelComparer();
        }

        // lookAt vector used only for 3D Billboard picking
        private Vector3d mLookAt; 
        // overloaded Pick() that additionally accepts Viewport and thus allows 2D screenspace pick collision test when applicable
        public PickResults Pick(Viewport vp, Region rayOriginRegion, SceneNode startNode, Ray regionSpaceRay, KeyCommon.Traversal.PickParameters parameters)
        {
        	if (vp == null) throw new ArgumentNullException ();
        	mViewport = vp;
            mLookAt = vp.Context.LookAt;

        	return this.Pick (rayOriginRegion, startNode, regionSpaceRay, parameters);
        }

        /// <summary>
        /// Finds the closest of two PickResults when both results have HasCollided = true
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static PickResults FindClosest(PickResults a, PickResults b)
        {
            if (a.HasCollided && b.HasCollided)
            {
                if (a.DistanceSquared < b.DistanceSquared)
                    return a;
                else
                    return b;
            }
            else if (a.HasCollided)
                return a;
            else return b;

        }

        // overloaded Pick() that only does 3D collisions.
        public PickResults Pick (Region rayOriginRegion, SceneNode startNode, Ray regionSpaceRay, KeyCommon.Traversal.PickParameters parameters)
        {
                	
        	PickResults[] results = this.Collide (rayOriginRegion, startNode, regionSpaceRay, parameters);
        	
        	if (results == null)
            	return PickResults.Empty(); 
            
	        return results[0];
        }

        // a type of "Pick()" for rays that are cast from within the simulation such as lasers, bullets, line of sight, etc instead of Mouse picking.
        public PickResults[] Collide(Region rayOriginRegion, SceneNode startNode, Ray regionSpaceRay, KeyCommon.Traversal.PickParameters parameters)
        {
        	if (startNode == null) throw new ArgumentNullException("Picker.Pick() - Start Node cannot be null.");
            if (rayOriginRegion == null) return null;

            _parameters = parameters;
            TV_PICK_SCALING = (float)(parameters.T1 - parameters.T0);

            // mRayOriginRegion primarily used to compare regin locations of ray and current entity being tested
            // and thus whether to use identy matrix or entity.RegionMatrix prior to applying camera offset
            // and matrix inversion
			mRayOriginRegion = rayOriginRegion ;
			
            mFoundItems.Clear();

            mRegionSpaceRay = regionSpaceRay;
                                    
            mInsideInterior.Push(false);
            #if DEBUG
            if (_parameters.DebugOutput)
            	System.Diagnostics.Debug.WriteLine ("Picker.Collide() - Beginning traversal at '" + startNode.GetType().Name.ToUpper() + "'");
            #endif
            try 
            {
            	startNode.Traverse(this, null);
            }
            catch (Exception ex)
            {
            	System.Diagnostics.Debug.WriteLine ("Picker.Collide() - EXCEPTION: " + ex.Message);
            }
            #if DEBUG
            if (_parameters.DebugOutput)
            	System.Diagnostics.Debug.WriteLine ("Picker.Collide() - Traversal complete.");
            #endif
            mInsideInterior.Pop();

            if (mFoundItems == null || mFoundItems.Count == 0) return null;
            
            PickResults[] results = new PickResults[mFoundItems.Count];
            mFoundItems.CopyTo (results, 0);
            return results;
        }


        private void UpdatePickResults(Vector3d modelSpaceRayOrigin, Matrix regionMatrix, PickResults localResult, Entity entity, Model model, Geometry geometry, uint minimeshElementIndex, int floorLevel)
        {
        	int tileID = ((Keystone.Elements.MinimeshGeometry)geometry).GetElementTag (minimeshElementIndex);
        	localResult.FaceID = tileID;
        	// TODO: is there a way to store the x/y/z tile location in the localResult?
        	//       - also can we perhaps for minimesh also store the obstacle TileID too if applicable?
			//       TODO: also we should store the structuralLevel which hosts the minimesStructures and layer data for that level.
        	// grab the tileID which is in the MinimeshGeometry tag            
        	// TODO: i think that if we are picking minimeshes we should try to avoid needing TileID or TileLocation
        	//       I think this is where our picking HUD MARKERS comes in to show us what we're picking.
        	//
        	//       If we do pick a wall, maybe we do need an EdgeID and maybe we know the "layer" by name since it's "structure"
        	//       and if it's terrain it is "terrain" layout.  So for now, we'll try to get the layer by name to unflatten
        	if (entity as TileMap.Structure != null)
        	{
	        	TileMap.Structure tmp = (TileMap.Structure)entity;
    	    	int x, z;
    	    	tmp.UnflattenIndex ("layout", floorLevel, (uint)tileID, out x, out z);
	        	// TODO: recall that a TileLocation is dependant on the resolution of the specific data layer.  Typically during picking
	        	//       we are choosing a high res obstacle layer, but some obstacle layers can be lower res if they are not populated by
	        	//       units, and instead just have large structures on them. don't know if that even makes sense to do though yet since eventually
	        	//       units would find there way to that zone and need a higher res obstacle map to write to.
	        	// TODO: TileLocation from a TileID is just unflattening, yes but without
	        	//       the specific layer, we dont know the resolution values for unflattening.  Is there a way to store the layer
	        	//       in the pickresult?  then we can easily unflatten and get .TileLocation
        		localResult.TileLocation = new Vector3i (x, floorLevel, z);
        	}
        	localResult.GroupIndex = (int)minimeshElementIndex;
        	localResult.Geometry = geometry; // we pick Mesh3d but then we assign MinimeshGeometry as hit result 
        	UpdatePickResults (modelSpaceRayOrigin, regionMatrix, localResult, entity, model, geometry);
        }
        
        private void UpdatePickResults(Vector3d modelSpaceRayOrigin, Matrix regionMatrix, PickResults localResult, Entity entity, Model model, Geometry geometry)
        {
            // TODO: is the picking of GUI widgets a result of 
            // scaling them during render?  the visual size we see is not actually the size they are?
            // or is it precision?
            if (localResult.HasCollided == false) return;
            
            #if DEBUG
            if (_parameters.DebugOutput)
            	System.Diagnostics.Debug.WriteLine ("Picker.UpdatePickResults() - '" + entity.TypeName.ToUpper() + "' added.");
            #endif
            
            Region region = entity.Region;
            bool isVisualModel = entity is ModeledEntity;
			
        	// special handling of proxy entity to assign the referenced entity and not the proxy
            if (entity is IEntityProxy)
            {
            	Entity referencedEntity = ((IEntityProxy)entity).ReferencedEntity;
                if (referencedEntity == null) return;
            	region = referencedEntity.Region;
            	isVisualModel = referencedEntity is ModeledEntity;
                localResult.SetEntity(entity); // NOTE: We assign the proxy and not the referencedEntity.  We can determine the referenced Entity from the Proxy when needed.
            }
            else if (entity is Region)
            {
				// if the picked entity is a Region, we use itself as both targetEntity and targetRegion
            	region = (Region)entity;
            	localResult.SetEntity(region);
            }
            else if (entity is ModeledEntity)
            {
                // TODO: we no longer use IEntitySystems for StarMap (and possibly for other things
                //       since a sqlite db makes more sense for such things) so we need to modify this code
                //       to work with new implementation of our StarMap.
            	// IEntitySystem for instance a StarDigest.  This means
            	// the Entity referenced by the picked Record may not be paged in. Where does
            	// the responsibility lie for paging in these "selected" entities? (and paging them back out).
            	// It shouldn't be the picker because the Picker.cs wouldn't know when to page them out.
            	if (entity is Keystone.Simulation.IEntitySystem)
            	{
            		// NOTE: we assign the IEntitySystem to SetEntity but also the digest Record and Typename
            		// This way Workspace3D knows the selected Entity is of type IEntitySystem and can respond appropriately.
            		localResult.SetEntity (entity);
            		int recordID = localResult.FaceID;
					localResult.SetEntity (((Keystone.Simulation.IEntitySystem)entity).Records[recordID].ID, ((Keystone.Simulation.IEntitySystem)entity).Records[recordID].TypeName);
            	}
            	else
                	localResult.SetEntity(entity);
            }
            else 
            {
				localResult.SetEntity(entity);
            	System.Diagnostics.Debug.WriteLine ("Picker.UpdatePickResults() - 'NON VISUAL " + entity.TypeName.ToUpper() + "' added.");
            }
            
        	// ModeledEntities always score higher than NON VISUAL (non rendered) entities such as Regions, Portals, etc
        	// so that the nearest Zone for example, is not picked over a further away rendered entity
        	if (isVisualModel)
        		localResult.Score++;
            	
            localResult.Model = model;
            localResult.Geometry = geometry;
             
            // NOTE: we need ImpactPointRelativeToRayOriginRegion where the coord is relative to the ray's region
			// so we can render a preview easily relative to camera regardless of how many zones we are trying to pick across
			localResult.ImpactPointRelativeToRayOriginRegion = Vector3d.TransformCoord(localResult.ImpactPointLocalSpace, regionMatrix);

            if (mRayOriginRegion != region && mRayOriginRegion.GlobalMatrix != region.GlobalMatrix)
			{
				Matrix toOriginRegionSpace;
                if (model == null)
                    Helpers.Functions.GetToModelSpaceMatrix(mRayOriginRegion, entity, region, out toOriginRegionSpace);
                else
                {
                        Helpers.Functions.GetToModelSpaceMatrix(mRayOriginRegion, model, region, out toOriginRegionSpace);
                }

                localResult.ImpactPointRelativeToRayOriginRegion = Vector3d.TransformCoord(localResult.ImpactPointLocalSpace, toOriginRegionSpace);
			}
			
			// distance from ray origin to impact point (TODO: this should always use ImpactPointRelativeToRayOriginRegion
            // TODO: does distance between Region space origin and regionspace impact point == modelSpay origin and modelspace impact point?
			localResult.DistanceSquared  = Vector3d.GetDistance3dSquared(mRegionSpaceRay.Origin, localResult.ImpactPointRelativeToRayOriginRegion);
            
            //System.Diagnostics.Debug.WriteLine("Collisioin with - " + localResult.EntityTypeName + " distance = " + Math.Sqrt (localResult.DistanceSquared).ToString());
            localResult.Matrix = regionMatrix;              
                

            IPickResultComparer comparer = null;
            if ((_parameters.SearchType & PickCriteria.Closest) != PickCriteria.None)
                comparer = mNearComparer;
            else if ((_parameters.SearchType & PickCriteria.Last) != PickCriteria.None)
                comparer = mFarComparer;
            else if ((_parameters.SearchType & PickCriteria.DeepestNested) != PickCriteria.None)
            {
            	comparer = mDeepestNestedComparer;
            }
            
            if (comparer != null)
            {
                localResult.PickOrigin = mRegionSpaceRay.Origin;
                // NOTE: PickEnd is not the same as impact point, it's just the start/end points of the Ray line segment. This is useful for debug drawing of that line.
                // NOTE: this property "PickEnd" never seems to be used anywhere else in the solution
                localResult.PickEnd = mRegionSpaceRay.Origin + (mRegionSpaceRay.Direction * TV_PICK_SCALING);
                mFoundItems.Add(localResult, comparer);
            }
            else
            {
            	// are we looking to add by finding the deepest nested node?
            	// if a region is a child of another region, it's depth could is this.Region.Depth++;
            	// I could even compute Depth here quickly to...
            	mFoundItems.Add (localResult);
            }
        }

        
        private bool ModelSpaceRayBBTest(Ray modelSpaceRay, BoundingBox box)
        {
        	// TODO: something is wrong here when trying to intersect some worlds.  If i force
        	//       return TRUE, the Mesh3d.AdvancedCollide works fine so it is not the ray's conversion
        	//       to Model space that is the problem.  It's the box.Intersect test itself.
        	//return true;
        	// TODO: shouldn't i have option to return impact points here?  
        	// sometimes box impact points for region and things like portals are needed
            bool result = box.Intersects(modelSpaceRay, _parameters.T0, _parameters.T1);
        
            #if DEBUG
            if (_parameters.DebugOutput && result == false)
            	System.Diagnostics.Debug.WriteLine ("Picker.ModelSpaceRayBBTest() - Ray 2 Box test failed.");
            #endif
            
            return result;
        }
        
        /// <summary>
        /// Ignore if this entity has flags set on it that the parameters var specifies ignorable
        /// AND if the pickpass is COLLIDE (as opposed to mousepick) and entity.CollisonEnable == false 
        /// we must ignore as well.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private bool Ignore (Entity entity)
        {
        	// NOTE: entities like Viewpoints, Triggers are ignored because Pickable == false and Visible == false.
        	//       TODO: if portals are ignored, we need to ensure their destination is not
        	bool result  = entity.Visible == false ||
        			((entity.Attributes & _parameters.IgnoredTypes) != EntityAttributes.None ||
            	    (_parameters.PickPassType == PickPassType.Collide &&  entity.GetEntityAttributesValue ((uint)EntityAttributes.CollisionEnabled) == false) || 
            	    (_parameters.PickPassType == PickPassType.Mouse &&  entity.GetEntityAttributesValue ((uint)EntityAttributes.PickableInGame) == false));
        
        	#if DEBUG
        	if (_parameters.DebugOutput && result)
	        	System.Diagnostics.Debug.WriteLine ("Picker.Ignore() - Ignoring '" + entity.TypeName.ToUpper() + "'.");
        	#endif
        	
        	return result;
        }
        
        private bool Exclude (Entity entity)
        {
            bool result = (entity == null ||
                entity.Enable == false ||
                entity.PageStatus != Keystone.IO.PageableNodeStatus.Loaded ||
                (entity.Attributes & _parameters.ExcludedTypes) != EntityAttributes.None);        	
        
        	#if DEBUG
        	if (_parameters.DebugOutput && result)
	        	System.Diagnostics.Debug.WriteLine ("Picker.Exclude() - Excluding '" +  entity.TypeName.ToUpper() + "'.");
        	#endif
        	return result;
        }
        
        #region ITraverser Members
        #region SceneNodes
        public object Apply(SceneNode node, object data)
        {
            throw new Exception(
                "Picker.Apply (SceneNode) - ERROR: Missing Apply() overload or Traverse() override in derived node type.");
        }

        private Stack<bool> mInsideInterior = new Stack<bool>();
        
        public object Apply(RegionNode node, object data)
        {
            #if DEBUG
            if (_parameters.DebugOutput)
            	System.Diagnostics.Debug.WriteLine ("Picker.Apply(RegionNode) - PRE EXCLUDE");
            #endif
            
            if (Exclude (node.Region)) return null;
     
            #if DEBUG
            if (_parameters.DebugOutput)
            	System.Diagnostics.Debug.WriteLine ("Picker.Apply(RegionNode) - PRE MODELSPACE BOX CREATION");
            #endif
            
            BoundingBox modelSpaceBox = Helpers.Functions.GetModelSpaceBox(node.Region);
            mModelSpaceRay = Helpers.Functions.GetModelSpaceRay(mRegionSpaceRay, mRayOriginRegion, node.Region, node.Region);

            #if DEBUG
            if (_parameters.DebugOutput)
            	System.Diagnostics.Debug.WriteLine ("Picker.Apply(RegionNode) - PRE RAY BOX TEST");
            #endif
            
            // IMPORTANT: Region's are locked at the their own local coordinate system origin and thus
            // their boundingBox's are always in modelspace
            if (ModelSpaceRayBBTest(mModelSpaceRay, modelSpaceBox))
            {
            	if (Ignore(node.Region) == false)
            		node.Region.Traverse (this, null);
            	
            	SceneNode[] children = node.Children;
            	if (children != null)
            		for (int i = 0; i < children.Length; i++)
            			children[i].Traverse (this, null);
            }

            return null;
        }
        
        public object Apply(CelledRegionNode node, object data)
        {
        	if (Exclude(node.Region)) return null;
        	         
            BoundingBox modelSpaceBox = Helpers.Functions.GetModelSpaceBox(node.Region);
            mModelSpaceRay = Helpers.Functions.GetModelSpaceRay(mRegionSpaceRay, mRayOriginRegion, node.Region, node.Region);


            // IMPORTANT: Region's are locked at the their own local coordinate system origin and thus
            // their boundingBox's are always in modelspace
            if (ModelSpaceRayBBTest(mModelSpaceRay, modelSpaceBox))
            {
                mInsideInterior.Push(true);

                // PickSearchType.Interior means we do want to traverse child nodes, but we will ignore including 
                // the parent in any returned HIT result if it is not descended from an Interior region. 
                bool searchInterior = (_parameters.SearchType & PickCriteria.Interior) != PickCriteria.None;
                bool searchInteriorDescendants = searchInterior && mInsideInterior.Peek(); 

                // if ignored, we skip test on the region's internal structure but continue to traverse child entities
 //               if (Ignore(node.Region) == false)
                    // does the pick parameters specify we should search floors and not just overall interior bounding volume?
                        if ((_parameters.Accuracy & PickAccuracy.Tile) == PickAccuracy.Tile)
                            DoCelledRegionInteriorTest(mModelSpaceRay, (Interior)node.Region);

                if (searchInteriorDescendants)
                {
                	SceneNode[] children = node.Children;
	                if (children != null)
                		// celledRegionNode may have interior child entities, if searchInteriorDescendants we pick them
	                	for (int i = 0; i < children.Length; i++)
	                		children[i].Traverse(this, null);
                }

                mInsideInterior.Pop();
            }

            return null;
        }

        private void DoCelledRegionInteriorTest(Ray modelSpaceRay, Interior interior)
        {
            // TODO: is our CelledRegion pick being performed in modelspace?  must verify it is.  i think so though since it
            // seems to work regardless of exterior ship rotation or position
            Keystone.Types.Vector3d start = modelSpaceRay.Origin;
            Keystone.Types.Vector3d end = modelSpaceRay.Origin + (modelSpaceRay.Direction * TV_PICK_SCALING);
            
            KeyCommon.Traversal.PickParameters parameters = _parameters;

            
            // iterate through floors
            for (uint i = 0; i < interior.CellCountY; i++) 
            {

            	// TODO: i do think that LevelIndex should reflect the actual 0 based array index
            	//       I also think that all Levels should be placed in order with lowest floorID at index 0.
            	//       This does mean that level IDs across adjacent zones will sometimes be at different indices.
            	// TODO: I think if we want to pick starting at a certain floor, we should find the LevelIndex of that
            	//       FloorID.
                // -1 parameters.Floor indicate we search all floors
                if (parameters.FloorLevel != int.MinValue)
                {
                    // always skip floors greater than the specified .Floor
                    if (i > parameters.FloorLevel) continue;
                }

                //// TODO: i think problem here is for placing components, we dont want to require floor here
                ////       because it prevents us from rendering the invalid location because the pickResult is not
                ////       filled properly.  This means we really should be returning the "pick" and then using IsChildPlaceable
                ////       code to determine if the picked location is ok.
                //// there is a difference between excluding/ignoring non floored cells, and prioritizing them lower
                //bool cellFloorRequired = (parameters.Accuracy &  PickAccuracy.CellWithFloor) != PickAccuracy.None;
                //// if we're picking any cell (not just the cell's with floors), then we will skip any floor
                //// that is not the current because we're guaranteed to always find the closest cell on the current floor
                //// since we dont care if that cell is collapsed or not.
                //if (parameters.Floor != i && cellFloorRequired == false)
                //    continue;

                bool cellFloorPrioritized = (parameters.SearchType & PickCriteria.FlooredCell) != PickCriteria.None;

                int prevFloor = _parameters.FloorLevel;
                _parameters.FloorLevel = (int)i;
                PickResults localResult = interior.Collide(start, end, _parameters);

                if (localResult.CellLocation.X == -1 || localResult.CellLocation.Z == -1)
                    continue;

              
                // restore floor
                _parameters.FloorLevel = prevFloor;

                // if this cell is collapsed, depending on game\build mode, we may want to NOT report a hit
                // really this is all about pick parameters because during build mode to uncollapse a cell 
                // to place it in bounds or place a floor, collapsed must be pickable but when in game
                // mode, we may want to test the floor beneath next
                bool currentCellHasFloor = interior.CellHasFloor((uint)localResult.CellLocation.X, (uint)i, (uint)localResult.CellLocation.Z);
                
                // - set the correspondng bit in the localResult.Score property.  then we can sort by score
                //   and distance.  The more important the criteria, the more significant the bitflag we enable
                if (cellFloorPrioritized && currentCellHasFloor)
                {
                    localResult.Score += 1;
                }
                
                UpdatePickResults(start, interior.RegionMatrix, localResult, interior, null, null);   
                
                // Search and Test Interior wall models
                // TODO: structure should be picked here? because it is a Model collision
                //       and each wall model or floor model is in the "model space" of the celledRegion
				//       so they do not need to be transformed right?  But we do need distance calcs
				//       that get done here... so how do we get the walls (especially in 3d pick pass) 
				//		 pick results factored into the current?
				// wall models are stored in _selector 
				// and currently i forget how/if they are sorted by different wall geometry and appearance...
                
			    // get the Models under this celledRegion that correspond to walls
                // and are within x distance of where the pick ray intersects the floor height
                Predicate<Keystone.Elements.Node> match = e =>
	            {
	                return
	                    e is Keystone.Elements.Model &&
	                    e.ID.Contains(Interior.GetInteriorElementPrefix(interior.ID, Interior.PREFIX_WALL, typeof(Keystone.Elements.Model)));
	            };
	
	            // Perform the actual query passing in our match 
	            // NOTE: we are guaranteed because of the Predicate delegate we are using, that all results
	            // are of type Keystone.Elements.Model
	            Keystone.Elements.Node[] foundModels = interior.Query(true, match);
				
	            if (foundModels == null) continue;
	            
				for (int n = 0; n < foundModels.Length; n++)
					PickModel (interior, (Model)foundModels[n]);
            } 
        }


        private void DoStructureTest (Ray modelSpaceRay, TileMap.Structure structure)
        {
            // TODO: is our CelledRegion pick being performed in modelspace?  must verify it is.  i think so though since it
            // seems to work regardless of exterior ship rotation or position
            Keystone.Types.Vector3d start = modelSpaceRay.Origin;
            Keystone.Types.Vector3d end = modelSpaceRay.Origin + (modelSpaceRay.Direction * TV_PICK_SCALING);
            KeyCommon.Traversal.PickParameters parameters = _parameters;


            int previousFloorLevelParameter = _parameters.FloorLevel;
                
            // iterate through floors in reverse order so higher (i.e. closer to camera) levels are visited first
            uint levelCount = structure.LevelCount;
            int[] floorLevels = structure.GetActiveFloorLevels();
            
            if (floorLevels == null) return;
            
            for (int i = floorLevels.Length - 1; i  >= 0; i--)
            {
                // int.MinValue for parameters.FloorLevel indicate we search all floors, otherwise we skip floors greater than the index specified
                if (parameters.FloorLevel != int.MinValue)
                {
                    // skip just those floors greater than the specified .FloorLevel
                    if (floorLevels[i] > parameters.FloorLevel)
                    	continue;
                }

                //// TODO: i think problem here is for placing components, we dont want to require floor here
                ////       because it prevents us from rendering the invalid location because the pickResult is not
                ////       filled properly.  This means we really should be returning the "pick" and then using IsChildPlaceable
                ////       code to determine if the picked location is ok.
                //// there is a difference between excluding/ignoring non floored cells, and prioritizing them lower
                //bool cellFloorRequired = (parameters.Accuracy &  PickAccuracy.CellWithFloor) != PickAccuracy.None;
                //// if we're picking any cell (not just the cell's with floors), then we will skip any floor
                //// that is not the current because we're guaranteed to always find the closest cell on the current floor
                //// since we dont care if that cell is collapsed or not.
                //if (parameters.Floor != i && cellFloorRequired == false)
                //    continue;

                bool flooredTilesPrioritized = (parameters.SearchType & PickCriteria.FlooredCell) != PickCriteria.None;

                _parameters.FloorLevel = floorLevels[i];
                
//                // IF WE ARE TESTING STRUCTURE MINIMESH MODELS 
//                // {
//                 		if (structure.mLevels[levelIndex].mStructureMinimeshes != null)
//                 		{
//                 			// TODO: we need a way to query just those structural models that are on level 'i' 
//				         	Keystone.Elements.Node[] foundMinimeshes = structure.mLevels[levelIndex].mStructureMinimeshes;
//				//				
//				          	if (foundMinimeshes == null) continue;
//				//          
//				          	for (int n = 0; n < foundMinimeshes.Length; n++)
//				          	{
//					          	Model model = (Model)foundMinimeshes[levelIndex].Parents[0];
//					          	// NOTE: PickModel() will see model.Geometry is type MinimeshGeometry and call PickMinimeshGeometry();
//					          	// TODO: so we've picked a structure model, this should include in the pickresults the specific
//					          	//       tile and such this model sits in.
//					          	// TODO: well, normally we put tileID in .FaceID, but if we've picked a Minimesh element, we actually end up
//					          	//       returning the model's FaceID which is 0 or 1 for our current test since picking a floor quad will pick
//					          	//       triangle 0 or triangle 1.  Yet when trying to then tile ontop of a layer with existing minimesh elements
//					          	//       our AssetPlacementTool expects the FaceID to be that of the structure layer tileID. 
//					          	
//					          	// TODO: we need a way to associate this particular floor with the local result so AssetPlacementTool knows
//					          	//       which level we are attempting to place an item.
//					          	// I think an overload of PickModel()
//					          	// TODO: what if we include StructureLevel in the pickresult?
//					          	PickModel (structure, model, floorLevels[i]);
//				    		}
//                		}
//            	//			
//                // }
                // ELSE WE ARE TESTING 2D GRID METHOD - eg. drawing paths, links, 
                // {
                // NOTE: Most entities do not implement .Collide() but TileMap.Structure does since it allows us to Pick
                //       in a shortcut way since we know it's a grid layout so we don't have to pick each tile mesh or minimesh.
                try
                {
	                PickResults localResult = structure.Collide(start, end, _parameters);
	                // structure.Collide() is a 2-D call only.  We must fill in the proper floorLevel as we iterate through floors.
	            	localResult.TileLocation = new Vector3i (localResult.TileLocation.X, 
	                                                         floorLevels[i], 
	                                                         localResult.TileLocation.Z);
	                // }
	                
	                
	//				  CellHasFloor TEMP disregard this code for now until we get Structure picking working
	//                // if this cell is collapsed, depending on game\build mode, we may want to NOT report a hit
	//                // really this is all about pick parameters because during build mode to uncollapse a cell 
	//                // to place it in bounds or place a floor, collapsed must be pickable but when in game
	//                // mode, we may want to test the floor beneath next
	//                bool currentTileHasFloor = structure.CellHasFloor((uint)localResult.CellLocation.X, floorLevels[i], (uint)localResult.CellLocation.Z);
	//                
	//                // - set the correspondng bit in the localResult.Score property.  then we can sort by score
	//                //   and distance.  The more important the criteria, the more significant the bitflag we enable
	//                if (flooredTilesPrioritized && currentTileHasFloor)
	//                {
	//                    localResult.Score += 1;
	//                }
	                
	                // TODO: should structure.RegionMatrix actually be StructureLevel matrix that includes difference in Y altitude of each floor level 
	                //       since our picking is 2D here for all floor levels.
	                UpdatePickResults(start, structure.RegionMatrix, localResult, structure, null, null);   

                }
                catch (Exception ex)
                {
                	System.Diagnostics.Debug.WriteLine ("Picker.DoStructureTest() - EXCEPTION: " + ex.Message);
                }
//                // NOTE: Below is attempt to not use Tile/Edge picking and instead do MinimeshGeometry picking
//                //       of EACH element.  It's insanely slow.  

//                // Search and Test Interior wall models
//			    // get the Models under this celledRegion that correspond to walls
//                // and are within x distance of where the pick ray intersects the floor height
//                Predicate<Keystone.Elements.Node> match = e =>
//	            {
//	                return
//	                    e is Keystone.Elements.Model &&
//	                    e.ID.Contains(CelledRegion.GetInteriorElementPrefix(structure.ID, CelledRegion.PREFIX_WALL, typeof(Keystone.Elements.Model)));
//	            };
//	
//	            // Perform the actual query passing in our match 
//	            // NOTE: we are guaranteed because of the Predicate delegate we are using, that all results
//	            // are of type Keystone.Elements.Model
//	            Keystone.Elements.Node[] foundModels = structure.Query(true, match);
//				
//	            if (foundModels == null) continue;
//	            
//				for (int n = 0; n < foundModels.Length; n++)
//					PickModel (structure, (Model)foundModels[n]);
            } 
            
	        // restore floor
            _parameters.FloorLevel = previousFloorLevelParameter;
        }
        
        public object Apply(OctreeOctant octant, object data)
        {
            throw new NotImplementedException();
        }

        public object Apply(QuadtreeQuadrant quadrant, object data)
        {
            throw new NotImplementedException();
        }

        public object Apply(GUINode node, object data)
        {
        	if (Exclude(node.Entity)) return null;
        	
            // ignore this Entity if it's flagged to be ignored, but continue to traverse the children
            if (Ignore(node.Entity) == false)
                node.Entity.Traverse(this, data);

            // TODO: enforce only other GUINodes can exist off of a GUINode
            SceneNode[] children = node.Children;
            	if (children != null)
            		for (int i = 0; i < children.Length; i++)
            			children[i].Traverse (this, data);
            	
            return null;
        }

        public object Apply(EntityNode node, object data)
        {
            // NOTE: IgnoredTypes are not immediately tested, but ExcludedTypes are.
            //       IgnoredTypes still allows us to test their children, excludedTypes we
            //       return immediately.
            if (Exclude(node.Entity)) return null;

            //System.Diagnostics.Debug.WriteLine(node.Entity.ID);
            //if (node.Entity.SRC != null && node.Entity.SRC.Contains("chair"))
            //    System.Diagnostics.Debug.WriteLine("chair");

            //if (node.Entity is InputAwareProxy )
            //	System.Diagnostics.Debug.WriteLine(node.Entity.ID);
            
            BoundingBox modelSpaceBox = Helpers.Functions.GetModelSpaceBox(node.Entity);		
            mModelSpaceRay = Helpers.Functions.GetModelSpaceRay(mRegionSpaceRay, mRayOriginRegion, node.Entity, node.Entity.Region);

            if (ModelSpaceRayBBTest(mModelSpaceRay, modelSpaceBox))
            {
            	if (node.Entity is TileMap.Structure)
            	{
	                mInsideInterior.Push(true);

	                // PickSearchType.Interior means we do want to traverse child nodes of TileMap.Structure, but we will ignore including 
	                // the parent in any returned HIT result if it is not descended from an Interior Structure. 
	                bool searchInterior = true; // HACK - force enable search interior (_parameters.SearchType & PickCriteria.Interior) != PickCriteria.None;
	                bool searchInteriorDescendants = searchInterior && mInsideInterior.Peek(); 
	
	                // if ignored, we skip test on the region's internal structure but continue to traverse child entities
	                if (Ignore(node.Entity) == false)
	                    // does the pick parameters specify we should search floors and not just overall interior bounding volume?
	                        if ((_parameters.Accuracy & PickAccuracy.Face) == PickAccuracy.Face || ((_parameters.Accuracy &  PickAccuracy.Tile) == PickAccuracy.Tile))
	                            DoStructureTest(mModelSpaceRay, (TileMap.Structure)node.Entity);
	
	                if (searchInteriorDescendants)
	                {
	                	SceneNode[] children = node.Children;
		                if (children != null)
	                		// TileMap.Structure may have interior child entities, if searchInteriorDescendants we pick them
		                	for (int i = 0; i < children.Length; i++)
		                		children[i].Traverse(this, null);
	                }
	
	                mInsideInterior.Pop();
            	}
            	else 
            	{
	                // ignore this Entity and any of it's Models/ModelSelectors if it's flagged to be ignored, 
	                // but we do not return from the method.  We continue to allow node.Children to be traversed further down
	                // so child Entities can be traversed.         
	                if (Ignore(node.Entity) == false)
	                    node.Entity.Traverse(this, data);
	
	                // hierarchical child entities are searched via this EntityNode's child EntityNode's
	                // however generally, an EntityNode wont have child EntityNodes. I think only RegionNodes do.
	                SceneNode[] children = node.Children;
	            	if (children != null)
	            		for (int i = 0; i < children.Length; i++)
	            			children[i].Traverse (this, data);
            	}
            }

            return null;
        }

        public object Apply(PortalNode node, object data)
        {
        	if (Exclude(node.Entity)) return null;

            BoundingBox modelSpaceBox = Helpers.Functions.GetModelSpaceBox(node.Entity);
            mModelSpaceRay = Helpers.Functions.GetModelSpaceRay(mRegionSpaceRay, mRayOriginRegion, node.Entity, node.Entity.Region);

            if (ModelSpaceRayBBTest(mModelSpaceRay, modelSpaceBox))
            {
	            if (Ignore(node.Entity) == false)
		            if (((Portal)node.Entity).Destination != null)
	    	        {
		                node.Entity.Traverse(this, data);
	    	        }
            }
            return null;
        }
        
        public object Apply(CellSceneNode node, object data)
        {
        	if (Exclude(node.Entity)) return null;
        	
            BoundingBox modelSpaceBox = Helpers.Functions.GetModelSpaceBox(node.Entity);
            mModelSpaceRay = Helpers.Functions.GetModelSpaceRay(mRegionSpaceRay, mRayOriginRegion, node.Entity, node.Entity.Region);

            if (ModelSpaceRayBBTest(mModelSpaceRay, modelSpaceBox))
            {
                // ignore this Entity if it's flagged to be ignored, but continue to traverse the children
                if (Ignore(node.Entity) == false)
                    node.Entity.Traverse(this, data);

                // child entities are searched via the child EntityNode's
                SceneNode[] children = node.Children;
            	if (children != null)
            		for (int i = 0; i < children.Length; i++)
            			children[i].Traverse (this, data);
            }
            return null;
        }
#endregion SceneNodes

		#region Nodes
        public object Apply(Node node, object data)
        {
            throw new Exception(
                "The method or operation is not valid.  Missing Apply() overload or Traverse() override in derived node type.");
        }

        
        public object Apply (TileMap.Structure structure, object data)
        {
        	
        	return null;
        }

        public object Apply(Interior interior, object data)
        {
            return null;
        }

        public object Apply(Portals.Region region, object data)
        {
        	// If this Region's Apply() is called, it means the RegionNode Collision == true
        	// so we don't have to test the region itself, but we do need to have it UpdateCurrentResults() 
        	// with itself.
        	
            // NOTE: we don't iterate child entities of Region here.  That is done through child EntityNode traversal
            
            // Also we add this Region as a pick result in case we are picking Region's to discover which
            // is deepest region that intersects mouse ray
            
    		PickResults localResult = new PickResults ();
    		localResult.HasCollided = true;
    		// by default if we do not collide with a wall of this region's bounding box
    		// the "impactpoint" will be the PickEnd location
			// NOTE: PickEnd is not the same as impact point, it's just the start/end points of the Ray line segment. This is useful for debug drawing of that line.
            localResult.ImpactPointLocalSpace = mModelSpaceRay.Origin + (mModelSpaceRay.Direction * TV_PICK_SCALING);
			localResult.ImpactNormal = -mModelSpaceRay.Direction;
                				
    		// find intersection points of ray against the modelspace box
    		//  - that means iterating each face, and doing polygon collision 
    		Vector3d intersectionPoint;
    		Polygon[] polys = BoundingBox.GetPolyFaces(region.BoundingBox);
    
    		double RAY_SCALE = _parameters.T1 - _parameters.T0;
    		for (int i = 0; i < polys.Length; i++)
    		{
	    		bool hit = polys[i].Intersects(mModelSpaceRay, RAY_SCALE, false, out intersectionPoint);
            
	    		if (hit)
	    		{
	    			// we only need to update the ImpactPointLocalSpace if we hit a wall
	    			// as opposed to just hitting nothing but open space within the Region.
					// UpdatePickResuls() fills in the rest
    				localResult.ImpactPointLocalSpace = intersectionPoint;
	    			break;
	    		}
    		}
    		
    		// if Regions are NOT ignored and NOT excluded, then we are specifically picking Regions (volumes without geometry)
    		// and so need to UpdateResults but likely continue picking to find deepest Region 
    		// that contains pick ray in case of nested regions
    		
    		UpdatePickResults (mModelSpaceRay.Origin, Matrix.Identity(), localResult, region, null, null);


            return null;
        }

        // TODO: is this working?  
        public object Apply(Portal p, object data)
        {
            if ((p.Destination != null) && (p.Destination.SceneNode != null))
                return p.Destination.SceneNode.Traverse(this, data);

            return null;
        }

        /// <summary>
        /// 2D Controls are screenspace positioned and drawn with 2D immediate such as DrawTexturedQuad
        /// 3D Controls exist in 3D space, not screenspace and are drawn as billboards3d.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public object Apply(Controls.Control2D control, object data)
        {
            try
            {
                Model[] models = control.SelectModel(SelectionMode.Collision, -1);

                if (models != null)
                    for (int i = 0; i < models.Length; i++)
                		PickModel(control, models[i]);
                
            }
            catch (Exception ex)
            {
                // fails on star systems and stars.  Why do i have StarSystem as a modeled entity?  
                System.Diagnostics.Trace.WriteLine(string.Format("Advanced collide failed for entity '{0}'", control.ID));
                System.Diagnostics.Trace.WriteLine(ex.Message);

            }
            finally
            {
            }
            return null;
        }

        public object Apply(DefaultEntity entity, object state)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// At this stage a boundingbox collision with this entity has occured
        /// </summary>
        /// <param name="entity"></param>
        public object Apply(ModeledEntity entity, object data)
        {
            try
            {
                Model[] models = entity.SelectModel(SelectionMode.Collision, -1);
                // TODO: apply _parameters.PickSearchType to limit scope if applicable
                if (models != null)
                    for (int i = 0; i < models.Length; i++)
                		PickModel (entity, models[i]);
            }
            catch (Exception ex)
            {
                // fails on star systems and stars.  
                System.Diagnostics.Debug.WriteLine(string.Format("Picker.Apply (ModeledEntity) - ERROR: Advanced collide failed for entity '{0}' {1}", entity.ID, ex.Message));
            }
            finally
            {
            }
            return null;
        }


        public object Apply(ModelLODSwitch lod, object data)
        {
            return null;
        }

        public void PickModel (Entity entity, Model model, int floorLevel = int.MinValue)
        {
	        System.Diagnostics.Debug.Assert (entity is ModeledEntity || entity is Interior, "Currently these are only types of entity that can have pickable objects attached.");
	        
	        // NOTE: here we perform "pickable" test against a SUB-MODEL and not the entity. Ignore() therefore wont work.
	        // NOTE: models in a sequence especially is useful if they can have independant "pickable" setting
	        if (model.Pickable == false) return;                  			
        	if (model.Geometry == null || model.Geometry.PageStatus != Keystone.IO.PageableNodeStatus.Loaded) return;


            if (model.Geometry is TexturedQuad2D)
            {
                // this is our problem for now... normally we would use the model matrix
                // but that would require us to use a special Model2D so that we can
                // implement the RegionMatrix calc differently.  In short, shoe horning
                // our 2D stuff into our 3D stuff is a bit annoying
                Matrix worldMatrix = model.RegionMatrix; // July.1.2014 - switched from entity.RegionMatrix

                PickResults pickresult =
                    ((TexturedQuad2D)model.Geometry).AdvancedCollide((Controls.Control2D)entity, Vector3d.Zero(), Vector3d.Zero(), _parameters);

                if (pickresult.HasCollided == false) return;
                UpdatePickResults(mRegionSpaceRay.Origin, worldMatrix, pickresult, entity, model, model.Geometry);

            }
            else if (model.Geometry is MinimeshGeometry)
            {
                Ray r = Helpers.Functions.GetModelSpaceRay(mRegionSpaceRay, mRayOriginRegion, model, entity.Region);
                PickMinimeshGeometry(entity, model, (MinimeshGeometry)model.Geometry, r, floorLevel);
            }
            else if (model.Geometry is Billboard)
            {

                Billboard bb = (Billboard)model.Geometry;
                Vector3d up = mViewport.Context.Up;

                Vector3d cameraSpacePosition = entity.Translation - this.mViewport.Context.Position;
                Ray msRay = Helpers.Functions.GetBillboardModelSpaceRay(mRegionSpaceRay, mRayOriginRegion, model, entity, entity.Region, Vector3d.Zero(), cameraSpacePosition, up, mLookAt);
                PickGeometry(entity, model, model.Geometry, msRay);
            }
            else
            {
                // IF 3D GEOMETRY or Proxy3D
                // we  compute modelspace ray for each unique model to take into account any different scaling or rotations
                // that exist independantly on each model.
                //if (entity.Name != null && (entity.Name == "Sky" || entity.Name.Contains("starfield"))) // TODO: maybe the issue here is that the ray orgin region and entity.Region are both the same and i need to special case handling of 0,0,0 positioned sky spheres or starfields
                //    System.Diagnostics.Debug.WriteLine("Picker.PickModel() - sky or starfield");
                Ray msRay = Helpers.Functions.GetModelSpaceRay(mRegionSpaceRay, mRayOriginRegion, model, entity.Region);

                PickGeometry(entity, model, model.Geometry, msRay);
            }
        }
                
        public void PickGeometry(Entity entity, Model model, Geometry geometry, Ray modelSpaceRay)
        {
        	// take the Model RegionMatrix here not the Entity.  THis matrix is only
            // used to convert model space impact point to region space
            // TODO: for a model that hosts as TexturedQuad2D and where it's host ModeledEntity
            //       is using it's own matrix to only store the 2d space value, all we care about is
            //       the entity's matrix and we also dont ever need to do any modelspace conversion of
            //       view matrix during culling or of ray during picking.  
            Matrix worldMatrix = model.RegionMatrix;

            // update parameter to include ID of model if we're dealing with Actor                
            if (geometry is Actor3d)
            	_parameters.ActorDuplicateInstanceID = model.ID;
                
            PickResults pickresult;


            if (geometry is Billboard)
            {
                pickresult = ((Billboard)geometry).AdvancedCollide(entity, model, mLookAt, modelSpaceRay.Origin, modelSpaceRay.Origin + (modelSpaceRay.Direction * TV_PICK_SCALING), _parameters);
            }
            else if (geometry is Mesh3d && ((Mesh3d)geometry).IsPointList)
            	// NOTE: some Entities such as Background3D may have multiple pointsprite meshes under a ModelSequence.
            	// Each Model in the sequence may have their own T/F value for .Pickable  Keep this in mind if you notice multiple pointsprite picks
            	// on what seems to be a single Model.
            	pickresult = ((Mesh3d)geometry).Collide2D (mViewport, worldMatrix, _parameters);
            else
				pickresult =
            	    geometry.AdvancedCollide(modelSpaceRay.Origin, modelSpaceRay.Origin + (modelSpaceRay.Direction * TV_PICK_SCALING),
            	                               _parameters);

            UpdatePickResults(modelSpaceRay.Origin, worldMatrix, pickresult, entity, model, geometry);
        }

        
        public void PickMinimeshGeometry (Entity entity, Model model, MinimeshGeometry mini, Ray modelSpaceRay, int floorLevel)
        {        	
        	byte[] enableArray = mini.EnableArray;        	
        	if (enableArray == null) return;
        	
	       	Keystone.Collision.PickResults result = new Keystone.Collision.PickResults();
        	BoundingBox box = mini.Mesh3d.BoundingBox ;
        	
        	// here we have to itterate through all minimesh elements computing a new regionMatrix for each item and assigning it to
        	// the mesh.  
        	// NOTE: Structure tiles are picked as virtual tile quads and is fast (see Structure.Collide()) whereas this
        	// PickMinimeshGeometry() is very slow.  

        	for (uint i = 0; i < enableArray.Length; i++)
        	{
        		if (enableArray [i] != 0)
        		{
        			Matrix m = mini.GetElementMatrix (i);
        			// NOTE: this element matrix is already in model space!  we do NOT want to multiply by RegionMatrix
        			// m = model.RegionMatrix * m;
					// NOTE: we do still need a ModelSpace Ray.
					// 		 in the following case, our ray is needs to be modelspace with respect to the specific Minimesh Element.
        			// NOTE: for interior models, the target region is not entity.Region, it's the entity itself
        			Region targetRegion = entity.Region;
        			if (entity is Portals.Interior) // NOT TO BE CONFUSED WITH TileMap.Structure.  Indeed, the Portals.Structure is probably obsolete now and this if() is not needed anymore
        				targetRegion = (Portals.Interior)entity; 
        			
        			// NOTE: modelSpaceRay has to be computed uniquely for each element.  That makes things slower.  
        			// TODO: perhaps a way to improve this is to start with the modelSpaceRay passed in and translate it to work with the specific minimesh element
        			//       rather than compute unique modelspaceray for each element?
        			Ray r = Helpers.Functions.GetMiniMeshElementModelSpaceRay(mRegionSpaceRay, mRayOriginRegion, model, targetRegion, m);
        			// somewhat seems when i render this at identity, the pick is just not in proper space in relation to that mesh... its way behind it and it should be
        			// +z from it.
        			
		        	// NOTE: we first do a sphere intersection test since all we have to do is scale the sphere
		        	//       since all collision is modelspace.
		        	// TODO: all of this sphere collision code is untested
		        	// BEGIN UNTESTED
		        	double i1 = 0;
					double	i2 = 0;
					PickResults pickresult = new PickResults();
					if (mini.GetBoundingSphere(i).Intersects (r, ref i1))
					{
						// TODO: if applicable, i should compare i1 and i2 to see if the collision
						//       occurred within a specified interval.  For mousepicking this is generally not
						//       useful, but for collision testing of lasers and bullets between frames, it is.
						//       see BoundingBox.Intersects() for contrast in how it implements t0 and t1
						// AdvancedCollide against each element is slow.  Might be faster if we tested a spherebounding test first
        				pickresult = mini.Mesh3d.AdvancedCollide (r.Origin, 
						                                          r.Origin + (r.Direction * TV_PICK_SCALING),
						                                          _parameters);
					} // END UNTESTED
					
					if (pickresult.HasCollided)
        			{
	        			UpdatePickResults (r.Origin, m * model.RegionMatrix, pickresult, entity, model, mini, i, floorLevel);
        			}       
        		}
        	}
        }
		        
        public object Apply(Light light, object data)
        {
            // or rather how SHOULD we pick a light?  With a placeholder 2d billboard graphic that can scale up and be more easily clicked
            // and visible, or with just a 1unit radius sphere that we render in an immediate mode style if in edit mode?
            // With a spotlight having some different type of graphic?
            // What about a directional light? which in hte case of our Stars 
            // for now we just contruct a 1 meter diameter pick box since we render debug light boxes with 1 meter radius boxes
        	            
			// for picking the light when in Editor mode, a tiny box is constructed with 1 meter diameter
			// TODO: problem here is, if we ever want to do a ray collide against general light volume instead of 
			// editor pickbox, then this tiny editor pickbox is what we'll always get tested.
            BoundingBox pickBox = new BoundingBox(new Vector3d(0, 0, 0), 0.5f);
            pickBox = BoundingBox.Transform(pickBox, light.RegionMatrix);

            PickResults result = new PickResults();
            result.HasCollided = pickBox.Intersects(mModelSpaceRay, _parameters.T0,_parameters.T1);;

            UpdatePickResults (mModelSpaceRay.Origin, light.RegionMatrix, result, light, null, null);
                       
            return null;
        }

        public object Apply(Geometry geometry, object data)
        {
        	// TODO: create new implementation to properly handle pointsprite primitive type
        	return null;
        }
          
        
        public object Apply(Appearance.Appearance app, object data)
        {
            return null;
        }
		#endregion //sceneNodes
    #endregion //ITraverser Members
    }
}