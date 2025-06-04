using System;
using Keystone.Elements;
using Keystone.Resource;
using Keystone.Types;
using Keystone.Utilities;
using MTV3D65;

namespace Keystone.EntitySystems.Emitters
{
internal class InstancedBillboardHPEmitter : BillboardHPEmitter
	{
		public InstancedBillboardHPEmitter (string id, string billboardTexturePath) : base (id, billboardTexturePath)
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
			base.LoadTVResource();
		
			if (mModel == null) 
			{
				CONST_TV_BILLBOARDTYPE rotationType = CONST_TV_BILLBOARDTYPE.TV_BILLBOARD_FREEROTATION;
				if (AxialBillboard)
					rotationType = CONST_TV_BILLBOARDTYPE.TV_BILLBOARD_NOROTATION;

				 LoadInstancedBillboard (mBillboardTextureFullPath);
			}
		}
		
		private void LoadInstancedBillboard (string textureFullPath)
		{
			string shaderPath =   @"caesar\shaders\PSSM\pssm.fx"; //@"caesar\shaders\CustomInstancing.fx";
			
            float billboardUnitWidth = 1f; 
            float billboardUnitHeight = 0.1f;
            
            // do i need shareable billboard id for instancedgeometry lasers?
			// for Mesh3d or Billboard3d it's fine, but for mini's and InstancedGeometry they cannot be shared
			string geometryID = string.Format(Mesh3d.PRIMITIVE_QUAD + "_{0}_{1}", billboardUnitWidth, billboardUnitHeight);	
			// use TV_BILLBOARD_NOROTATION since we'll be handling rotations via shader
			Model tmp  = Keystone.Celestial.ProceduralHelper.CreateInstancedBillboardModel(geometryID,
			                                                                               CONST_TV_BILLBOARDTYPE.TV_BILLBOARD_NOROTATION,
			                                                                               textureFullPath,
			                                                                               shaderPath,
			                                                                               null,
			                                                                               CONST_TV_BLENDINGMODE.TV_BLEND_ADDALPHA, true);
			

        	this.AddChild (tmp);
        	
        	System.Diagnostics.Debug.Assert (this.mModel != null, "InstancedBillboardHPEmitter.ctor() - ERROR: Model is NULL.");
        	
        	// replace geometry with this one, or add call .CreateInstancedGeometryBillboardModel or CreateBillboardParticlesModel
        	InstancedBillboard instancedBillboard = (InstancedBillboard)this.mModel.Geometry;

        	// TODO: make sure we're handling axial rotation in shader
        	//geometry.AxialRotationEnable = AxialBillboard;

        	instancedBillboard.SetProperty ("meshortexresource", typeof(string), instancedBillboard.ID);
        	instancedBillboard.SetProperty ("maxcount", typeof(uint), (uint)MaxParticles);
        	instancedBillboard.SetProperty ("billboardwidth", typeof(float), 1.0f);
        	instancedBillboard.SetProperty ("billboardheight", typeof(float), 1.0f);
        	instancedBillboard.SetProperty ("isbillboard", typeof(bool), true);
        	instancedBillboard.SetProperty ("usecolors", typeof(bool), true);
        	instancedBillboard.SetProperty ("alphasort", typeof(bool), true);
        	// precache is critical since we will allow the HeavyParticlEmitter to manage
			// particleIndices and we want corresponding mini element indices to always match
        	instancedBillboard.SetProperty ("precache", typeof(bool), true);
        	
    		Keystone.IO.PagerBase.LoadTVResource (instancedBillboard, false);
		}
		
		protected override void UpdateParticle (int particleIndex)
		{
			InstancedBillboard instancedBillboard = (InstancedBillboard)Model.Geometry;
			
//			instancedBillboard.ChangeElement ((uint)particleIndex, 
//                    Particles[particleIndex].Position, 
//                    Particles[particleIndex].Scale, 
//                    Particles[particleIndex].Rotation, 
//                    Particles[particleIndex].Color.ToInt32());
			
			// NOTE: regionSpacePosition is supplied
			// TODO: this is not correct.  We render in camera space, but the particular region's offset is not supplied so all
			//       get rendered relative to region 0,0,0
			// TODO: the problem here is, the .Model is in Root region's space and instead, the HeavyParticleSystem
			//       the model is attached to needs to be in the Region's space that the particles are emitted from
	
			this.Model.AddInstance (Particles[particleIndex].Position, Particles[particleIndex].Rotation);
		}

		
		protected override void ActivateParticle(int particleIndex)
		{			
			InstancedBillboard instancedBillboard = (InstancedBillboard)mModel.Geometry;
			
			// NOTE: for axial-billboards, the particle.Rotation property contains the direction vector
			//       which is what we want, and for non axial, it contains normal euler angles
			// Vector3d direction = Vector3d.Normalize (particle.Velocity);
			
//			instancedBillboard.ChangeElement ((uint)particleIndex, 
//			                    Particles[particleIndex].Position, 
//			                    Particles[particleIndex].Scale, 
//			                    Particles[particleIndex].Rotation, 
//			                    Particles[particleIndex].Color.ToInt32());
//			//System.Diagnostics.Debug.WriteLine ("BillboardHPEmitter.Add() - Particle added at index '" + index.ToString() + "'");
//			const byte enable = (byte)1;
//			instancedBillboard.SetElementEnable ((uint)particleIndex, enable);

			// throw new ArgumentOutOfRangeException("BillboardHPEmitter.Add() - MaxParticles exceeded.");
		}

		protected override void RetireParticle(int particleIndex)
		{

//			InstancedBillboard instancedBillboard = (InstancedBillboard)Model.Geometry;
//			
//			const byte disable = (byte)0;
//			instancedBillboard.SetElementEnable ((uint)particleIndex, disable);
			// NOTE: we must not mini.RemoveElement() or else the indices of our 
			// Particles[] will not matchup with their corresponding element within MinimeshGeometry!
			// Instead, we should use a seperate method that runs outside of this loop to RetireParticles()
			// and manage those which are no longer alive, possibly removing excess, but if so, doing it to both
			// MinimeshGeometry and Particles[] at the same time and in the same way.
			// mini.RemoveElement ((uint)i);
			
			System.Diagnostics.Debug.WriteLine ("BillboardHPEmitter.RetireParticle() - Particle disabled at index '" + particleIndex.ToString() + "'");
		}
	}
}
