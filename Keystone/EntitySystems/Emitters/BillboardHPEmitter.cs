using System;
using Keystone.Elements;
using Keystone.Resource;
using Keystone.Types;
using Keystone.Utilities;
using MTV3D65;

namespace Keystone.EntitySystems.Emitters
{
	
	
	// TODO: Rather than inherit HeavyParticleEmitter, the graphical model should employ
	//       a strategy pattern within HeavyParticleEmitter.VisualModel or as a ModelSelector or something
	//       like that
	// TODO: Similarly, Collisions too should employ strategy pattern for the collision hull type
	internal class BillboardHPEmitter : HeavyParticleEmitter  
	{
		protected Keystone.Traversers.Picker mRayCollider;
		protected string mBillboardTextureFullPath;
		
		public BillboardHPEmitter (string id, string billboardTexturePath) : base (id)
		{
			RefCount = 0;
			MaxParticles = 200;
			Particles = null;
			Lifespan = 1.5f;
			
			string dataPath = Core._Core.ModsPath;
			mBillboardTextureFullPath = System.IO.Path.Combine (dataPath, billboardTexturePath);
			
            InitializeRayCollisions();
		}
		
		public override void LoadTVResource()
		{
			// Emitter is an Entity so call baseclass.LoadTVResource() to load any scripts
			base.LoadTVResource();
		
			if (mModel == null && this is InstancedBillboardHPEmitter == false)
			{
				CONST_TV_BILLBOARDTYPE rotationType = CONST_TV_BILLBOARDTYPE.TV_BILLBOARD_FREEROTATION;
				if (AxialBillboard)
					rotationType = CONST_TV_BILLBOARDTYPE.TV_BILLBOARD_NOROTATION;
				
				 LoadMinimeshlBillboard(mBillboardTextureFullPath, rotationType);
				
				// TODO: LoadInstancedGeometryBillboard (mBillboardTextureFullPath);
			}
		}

		
		private void LoadMinimeshlBillboard (string fullPath, CONST_TV_BILLBOARDTYPE rotationType)
		{
			string shaderPath= null; // @"E:\dev\c#\KeystoneGameBlocks\Data\pool\shaders\CustomMinimeshParticles.fx";
			Model tmp  = Keystone.Celestial.ProceduralHelper.CreateBillboardModel(rotationType, 
			                                                                      fullPath, 
			                                                                      shaderPath, 
			                                                                      null,
			                                                                      CONST_TV_BLENDINGMODE.TV_BLEND_ADDALPHA, true);
        	tmp.CastShadow = false;
        	tmp.ReceiveShadow = false;
        	this.AddChild (tmp);
        	
        	System.Diagnostics.Debug.Assert (this.mModel != null, "BillboardHPEmitter.ctor() - ERROR: Model is NULL.");
        	Geometry geometry = this.mModel.Geometry;
        	((Billboard)geometry).AxialRotationEnable = AxialBillboard;
        	
        	MinimeshGeometry mmGeometry = (MinimeshGeometry)Repository.Create ("MinimeshGeometry");
        	mmGeometry.SetProperty ("meshortexresource", typeof(string), geometry.ID);
        	mmGeometry.SetProperty ("maxcount", typeof(uint), (uint)MaxParticles);
        	mmGeometry.SetProperty ("billboardwidth", typeof(float), 1.0f);
        	mmGeometry.SetProperty ("billboardheight", typeof(float), 1.0f);
        	mmGeometry.SetProperty ("isbillboard", typeof(bool), true);
        	mmGeometry.SetProperty ("usecolors", typeof(bool), true);
        	mmGeometry.SetProperty ("alphasort", typeof(bool), true);
        	// precache is critical since we will allow the HeavyParticlEmitter to manage
			// particleIndices and we want corresponding mini element indices to always match
        	mmGeometry.SetProperty ("precache", typeof(bool), true); 
        	
        	// we don't want the Mesh3d falling out of scope before we
    		// .PagerBase.LoadTVResource() on the minimesh which needs to gain a reference to the Mesh3d
        	Repository.IncrementRef (geometry); 
    		this.mModel.RemoveChild (geometry);
    		
    		Keystone.IO.PagerBase.LoadTVResource (mmGeometry, false);
			Resource.Repository.DecrementRef (geometry); 

    		this.mModel.AddChild (mmGeometry);
		}
		
		private void LoadMinimeshBillboard (string fullPath)
		{
             // Minimesh Billboard
             Model model = Celestial.ProceduralHelper.CreateMinimeshBillboard(fullPath, (uint)MaxParticles, CONST_TV_BLENDINGMODE.TV_BLEND_ALPHA);
             this.AddChild (model);
		}
		
		protected void InitializeRayCollisions()
		{
			mRayCollider = new Keystone.Traversers.Picker();
		}
				
		
		protected override void UpdateParticle (int particleIndex)
		{
			MinimeshGeometry mini = (MinimeshGeometry)Model.Geometry;
			
			mini.ChangeElement ((uint)particleIndex, 
                    Particles[particleIndex].Position, 
                    Particles[particleIndex].Scale, 
                    Particles[particleIndex].Rotation, 
                    Particles[particleIndex].Color.ToInt32());
		}
		

		protected override void ActivateParticle(int particleIndex)
		{			
			MinimeshGeometry mini = (MinimeshGeometry)mModel.Geometry;
			
			// NOTE: for axial-billboards, the particle.Rotation property contains the direction vector
			//       which is what we want, and for non axial, it contains normal euler angles
			// Vector3d direction = Vector3d.Normalize (particle.Velocity);
			
			mini.ChangeElement ((uint)particleIndex, 
			                    Particles[particleIndex].Position, 
			                    Particles[particleIndex].Scale, 
			                    Particles[particleIndex].Rotation, 
			                    Particles[particleIndex].Color.ToInt32());
			//System.Diagnostics.Debug.WriteLine ("BillboardHPEmitter.Add() - Particle added at index '" + index.ToString() + "'");
			const byte enable = (byte)1;
			mini.SetElementEnable ((uint)particleIndex, enable);

			// throw new ArgumentOutOfRangeException("BillboardHPEmitter.Add() - MaxParticles exceeded.");
		}

		
		protected override void RetireParticle(int particleIndex)
		{

			MinimeshGeometry mini = (MinimeshGeometry)Model.Geometry;
			
			const byte disable = (byte)0;
			mini.SetElementEnable ((uint)particleIndex, disable);
			// NOTE: we must not mini.RemoveElement() or else the indices of our 
			// Particles[] will not matchup with their corresponding element within MinimeshGeometry!
			// Instead, we should use a seperate method that runs outside of this loop to RetireParticles()
			// and manage those which are no longer alive, possibly removing excess, but if so, doing it to both
			// MinimeshGeometry and Particles[] at the same time and in the same way.
			// mini.RemoveElement ((uint)i);
			
			//System.Diagnostics.Debug.WriteLine ("BillboardHPEmitter.Update() - Particle disabled at index '" + i + "'");	
		}
		
		protected override void DoCollisions(int particleIndex, Vector3d previousPosition, Vector3d direction, double distanceTraveled, double elapsedSeconds)
		{
			// https://software.intel.com/en-us/blogs/2011/02/18/building-a-highly-scalable-3d-particle-system
			// 1) Our first pick pass is strictly for Region boundaries including zone boundaries and portals returned in sorted order by impact distance
			//    The origin region is always the first.
			KeyCommon.Traversal.PickParameters parameters  = new KeyCommon.Traversal.PickParameters
			{
				// set T0 and T1 to limit search to between distance traveled between frames
				T0 = 0.0f,
				T1 = (Particles[particleIndex].Position - previousPosition).Length,
				// IMPORTANT: Use PickPassType.Boundaries, not Mouse pick or Collision
				// since otherwise Region's would be Ignored and not added to PickResults.
				PickPassType = KeyCommon.Traversal.PickPassType.Boundaries, 
				Accuracy = KeyCommon.Traversal.PickAccuracy.Any,
				FloorLevel = int.MinValue,
				SearchType = KeyCommon.Traversal.PickCriteria.Closest,
				SkipBackFaces = true, // TODO: maybe this should be false
			    ExcludedTypes = KeyCommon.Flags.EntityAttributes.Background, // NOTE: viewpoints are skipped by virtue of them having Visible=false
			    // collide only with Regions and Portals with exception of Root Region.
				IgnoredTypes = KeyCommon.Flags.EntityAttributes.AllEntityAttributes_Except_Boundaries | KeyCommon.Flags.EntityAttributes.Root 
			};

			// if Root is _NOT_ a ZoneRoot, then we actually CANNOT include Root as IgnoredType or else we'll have
			// no boundaryResults
			if (this.Scene.Root is Portals.ZoneRoot == false)
				parameters.IgnoredTypes = KeyCommon.Flags.EntityAttributes.AllEntityAttributes_Except_Boundaries & ~KeyCommon.Flags.EntityAttributes.Root ;
			
			Ray regionSpaceRay = new Ray(previousPosition, direction); 
			Keystone.Collision.PickResults[] boundaryPickResults = mRayCollider.Collide (Particles[particleIndex].Region, this.Scene.Root.SceneNode, regionSpaceRay, parameters);
					
			// 2) entity collisions between start/end positions within each Region
			parameters  = new KeyCommon.Traversal.PickParameters
			{
				// collision pass, not mouse pick.
				PickPassType = KeyCommon.Traversal.PickPassType.Collide, 
				Accuracy = KeyCommon.Traversal.PickAccuracy.Any,
				SearchType = KeyCommon.Traversal.PickCriteria.Closest,
				SkipBackFaces = true,
				FloorLevel = int.MinValue,
				// exclude Portals since we are using Boundary pass results 
				ExcludedTypes = KeyCommon.Flags.EntityAttributes.Portal | KeyCommon.Flags.EntityAttributes.Background, // NOTE: viewpoints are skipped by virtue of them having Visible=false
				
			    // collide only with ModeledEntities
				IgnoredTypes = KeyCommon.Flags.EntityAttributes.AllEntityAttributes_Except_VisualEntities  
			};

			double remainingLength = distanceTraveled;
			Ray previousRegionSpaceRay = regionSpaceRay;
			Ray currentRegionSpaceRay = regionSpaceRay;
			Keystone.Portals.Region previousRegion = Particles[particleIndex].Region;
			
			if (boundaryPickResults == null) return;
			
			bool hasCollided = false;
			
			for (int j = 0; j < boundaryPickResults.Length; j++)
			{
				// set T0 and T1 to limit search to between start/end positions within the current region
				// the end position is the lesser of remainingLength or impactPoint distance.
				parameters.T0 = 0;
				System.Diagnostics.Debug.Assert (remainingLength > 0);
				parameters.T1 = remainingLength;
				
				System.Diagnostics.Debug.Assert (boundaryPickResults[j].Entity != null && boundaryPickResults[j].Entity is Keystone.Portals.Region);
				Keystone.Portals.Region currentRegion = (Keystone.Portals.Region)boundaryPickResults[j].Entity; 
			
				// convert ray from previous region to current region's space
				currentRegionSpaceRay = Helpers.Functions.GetRegionSpaceRay(previousRegionSpaceRay, previousRegion, currentRegion);
				
				// NOTE: passing in currentRegion.SceneNode as startNode guarantees we don't traverse any other region during collision test
				Keystone.Collision.PickResults[] entityPickResults = mRayCollider.Collide (currentRegion, currentRegion.SceneNode, currentRegionSpaceRay, parameters);
				
				previousRegionSpaceRay = currentRegionSpaceRay;
				// advance the origin so that in next iteration when we convert it to region space of next zone, it will be at start of that zone's boundary
				previousRegionSpaceRay.Origin = boundaryPickResults[j].ImpactPointLocalSpace;
				previousRegion = currentRegion;


				// TODO: will this search not test for collision with other projectiles (eg bullets shooting down rockets)?
            	// TODO: who says we ever have to make lasers change Region?  we can still do model space ray tests against entities in other zones
            	//       without changing the origin zone...hrm... 
            	remainingLength -= Math.Sqrt (boundaryPickResults[j].DistanceSquared);
            	if (entityPickResults == null) continue;
            	
            	for (int k = 0; k < entityPickResults.Length; k++)
            	{
            		if (entityPickResults[k].HasCollided) // TODO; is .HasCollided redundant? well. if no collision we do return .HasCollided = false
					{
		                hasCollided = true;
		                try 
		                {
		                	// laser Production[] only processed on collision.
		                	DoProduction(particleIndex, entityPickResults[k].Entity, entityPickResults[k].ImpactPointRelativeToRayOriginRegion, elapsedSeconds);
		                }
		                catch (Exception ex)
		                {
		                	System.Diagnostics.Debug.WriteLine ("BillboardHPEmitter.DoCollisions() - ERROR: " + ex.Message);
		                }
            		}
            	}
			}
			
			if (hasCollided)
			{
				System.Diagnostics.Debug.WriteLine ("BillboardHPEmitter.DoCollisions() - Retiring collided particle at index " + particleIndex.ToString());
                // retire laser particle that has collided
                Particles[particleIndex].Alive = false;
                RetireParticle(particleIndex);
				mActiveParticleCount--;
			}
		}
		
		protected void DoProduction(int particleIndex, Entities.Entity consumer, Vector3d ImpactPointRelativeToRayOriginRegion, double elapsedSeconds)
		{
			// production of this emitter
        	KeyCommon.Simulation.Production[] production =  this.Production;
		 	if (production == null) return;
		 	
			System.Diagnostics.Debug.Assert (consumer != null, "BillboardHPEmitter.DoProduction() - ERROR: consumer is NULL.");
			if (consumer.Script == null || consumer.Script.Consumers == null) return; 
			
		 	// collided object is potentially a consumer
		 	// TODO: consumer can potentially be the emitting entity right?  why not? 
		 	// TODO: unless it represents a picking epsilon margin of error where the start position
		 	// returns as intersecting the muzzle\barrel.
	
			if (consumer == Particles[particleIndex].Owner) return; // TODO: for now we don't allow self collission
										
            // allow collided object to consume all products this particle produces
            for (int l = 0; l < production.Length; l++)
            {
            	production[l].Location = ImpactPointRelativeToRayOriginRegion;
            	// does this consumer have a receptron for this proudctID?
            	KeyCommon.Simulation.Consumption_Delegate handler = consumer.Script.Consumers[production[l].ProductID];
            	
            	if (handler != null)
            	{
            		// TODO: consumption that is returned should be propogated to nearby multiplayer clients?
	                KeyCommon.Simulation.Consumption[] consumptionResult = handler (consumer.ID, production[l], elapsedSeconds);
            	}
            	// consumption of this Product may in turn result in production of X new products next frame?
            	// eg. production of concussion blast, heat, etc?
            }
            
            // TODO: determine if this particle dies on collision or else if we do nothing, 
            // it will automatically continue to move and potentially hit other entities
		}
	}
}
