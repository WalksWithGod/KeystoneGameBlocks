using System;
using Keystone.Collision;
using Keystone.Entities;
using Keystone.Enum;
using Keystone.IO;
using Keystone.Shaders;
using Keystone.Traversers;
using Keystone.Types;
using Keystone.KeyFrames;
using MTV3D65;

namespace Keystone.Elements
{
	// what if all of Geometry contains all tvparticle system emitters and attractors
	// and then, from that we can create groups of Models with different Geometry (aka tvparticle systems)
	// to create a complex entity 
    public class Emitter : Geometry
    {
        private TVParticleSystem mTVParticleSystem; // reference to parent system obtained in constructor

        private ParticleKeyframe[] _particleKF;
        private EmitterKeyframe[] _emitterKF;
        private int mEmitterShape;
        private int mEmitterAlphaBlending;

        #region Emitter
        internal Emitter(string id)
            : base(id)
        {
        }

//        public static Emitter Create(string id, ParticleSystem system) // TODO: this is weird, an emitter i think is bound to a particular particle system.
//                                                            // because it's created via particleSystem.CreateEmitter() 
//                                                            // so we need to consider how these things really relate.
//                                                            // and what about how we think of our minimesh as a rendering system...
//                                                            // and not really as geometry per se.  Consider
//                                                            // that particle systems have a .Duplicate() function... so 
//        {
//           Emitter emitter = (Emitter)Keystone.Resource.Repository.Get((id));
//            if (emitter != null) return emitter ;
//
//            emitter = new Emitter(id, system._particle, MTV3D65.CONST_TV_EMITTERTYPE.TV_EMITTER_POINTSPRITE);
//            return emitter;
//        }

        #region ITraversable Members
        public override object Traverse(ITraverser target, object data)
        {
            return target.Apply(this, data);
        }
        #endregion

        #region IPageableTVNode member
        public override void UnloadTVResource()
        {
        	DisposeUnmanagedResources();
        }
                
        public override void LoadTVResource()
        {
        	
            mTVParticleSystem = CoreClient._CoreClient.Scene.CreateParticleSystem ();
        	
            //mTVParticleSystem.SetEmitterEnable
            //mTVParticleSystem.GetEmitterBoundingBox;   
            
		 	CONST_TV_EMITTERTYPE emitterType = CONST_TV_EMITTERTYPE.TV_EMITTER_POINTSPRITE; 
		 	int maxParticles = 200;

            // creation - TODO: LoadTVResource should handle this?
            // TODO: like TVActor, shared particle systems should use .Duplicate()
            //       And a particle system with all emitters and attractors, should be viewed as single
            //       Geometry.  I could perhaps derive a special Model.cs for it where emitters and attractors are
            //       seperate child nodes, but i dont see the point.
            //       - if were to create a custom particle system to mimic TV's but be done in our own
            //       "way" of doing things, im not sure what i would do... i would probably have a modelsequence
            //       with seperate billboards under each and then i could mix minimeshes, and have different types of minimeshes
            //       such as spheres, cubes, billboard quads, etc. as well as pointsprites.
            _tvfactoryIndex = mTVParticleSystem.CreateEmitter(emitterType, maxParticles);

            // appearance
            int textureID = CoreClient._CoreClient.TextureFactory.LoadTexture(_id, _id);
            mTVParticleSystem.SetPointSprite(_tvfactoryIndex, textureID);
            mTVParticleSystem.SetParticleDefaultColor(_tvfactoryIndex, new TV_COLOR(1, 1, 1, 0.5F));
            mTVParticleSystem.SetEmitterAlphaBlending(_tvfactoryIndex, 
                                                      CONST_TV_PARTICLECHANGE.TV_CHANGE_ALPHA,
                                                      CONST_TV_BLENDINGMODE.TV_BLEND_ALPHA);
            mTVParticleSystem.SetEmitterAlphaTest(_tvfactoryIndex, _alphaTestEnable);
            if (_alphaTestEnable)
                mTVParticleSystem.SetEmitterAlphaTest(_tvfactoryIndex, 
            	                                      _alphaTestEnable, 
            	                                      _alphaTestRefValue, 
            	                                      _alphaTestDepthBufferWriteEnable);
            
            
            // physical properties
            mTVParticleSystem.SetEmitterDirection(_tvfactoryIndex, true, new TV_3DVECTOR(0F, 0.5F, 0F),
                                   new TV_3DVECTOR(0.05F, 0.5F, 0.05F));
            mTVParticleSystem.SetEmitterGravity(_tvfactoryIndex, true, new TV_3DVECTOR(0, -1, 0));
            mTVParticleSystem.SetEmitterPosition(_tvfactoryIndex, new TV_3DVECTOR(0, 0, 0));
            mTVParticleSystem.SetEmitterPower(_tvfactoryIndex, 25, 4);
            mTVParticleSystem.SetEmitterShape(_tvfactoryIndex, CONST_TV_EMITTERSHAPE.TV_EMITTERSHAPE_SPHEREVOLUME);
            mTVParticleSystem.SetEmitterSphereRadius(_tvfactoryIndex, 8);
            mTVParticleSystem.SetEmitterSpeed(_tvfactoryIndex, 125);
            
  
            //_particle.GetEmitterKeyFrame();
            TV_PARTICLE_KEYFRAME[] KeyFrames = new TV_PARTICLE_KEYFRAME[3];

            KeyFrames[0].fKey = 0;
            KeyFrames[0].cColor = new TV_COLOR(0.7F, 0.7F, 0.7F, 0);
            KeyFrames[0].fSize = new TV_3DVECTOR(8, 8, 8);
            KeyFrames[1].fKey = 1.5F;
            KeyFrames[1].cColor = new TV_COLOR(0.85F, 0.85F, 0.85F, 0.75F);
            KeyFrames[1].fSize = new TV_3DVECTOR(32, 32, 32);
            KeyFrames[2].fKey = 4;
            KeyFrames[2].cColor = new TV_COLOR(1, 1, 1, 0);
            KeyFrames[2].fSize = new TV_3DVECTOR(64, 64, 64);
            mTVParticleSystem.SetParticleKeyFrames(_tvfactoryIndex, 
                                                   (int)(CONST_TV_PARTICLE_KEYUSAGE.TV_PARTICLE_KEYUSAGE_COLOR | 
                                                         CONST_TV_PARTICLE_KEYUSAGE.TV_PARTICLE_KEYUSAGE_SIZE),
                                                         3, KeyFrames);
            


            mTVParticleSystem.SetEmitterEnable(_tvfactoryIndex, true);
            mTVParticleSystem.SetEmitterLooping(_tvfactoryIndex, true);
            mTVParticleSystem.ResetEmitter(_tvfactoryIndex);
            
            SetChangeFlags(Keystone.Enums.ChangeStates.GeometryAdded | 
                Keystone.Enums.ChangeStates.BoundingBoxDirty |
                Keystone.Enums.ChangeStates.MatrixDirty 
                , Keystone.Enums.ChangeSource.Self);
        }

        public override void SaveTVResource(string filepath)
        {
            throw new NotImplementedException();
        }
        #endregion



        internal void ResetEmitter(int iEmitter)
        {
            mTVParticleSystem.ResetEmitter(iEmitter);
        }

        //internal void UpdateEx(float fTimeSeconds)
        //{
        //	// NOTE: actually UpdateEx() is called during Render() 
        //    mTVParticleSystem.UpdateEx(_tvfactoryIndex, fTimeSeconds);
        //}

        internal void PreGenerate(int iNu_particles)
        {
            mTVParticleSystem.PreGenerate(_tvfactoryIndex, iNu_particles);
        }

        internal CONST_TV_EMITTERTYPE GetEmitterType()
        {
            return mTVParticleSystem.GetEmitterType(_tvfactoryIndex);
        }

        internal void Billboard(int iTexture)
        {
            mTVParticleSystem.SetBillboard(_tvfactoryIndex, iTexture);
        }

        internal void Billboard(int iTexture, float fDefaultWidth)
        {
            mTVParticleSystem.SetBillboard(_tvfactoryIndex, iTexture, fDefaultWidth);
        }

        internal void Billboard(int iTexture, float fDefaultWidth, float fDefaultHeight)
        {
            mTVParticleSystem.SetBillboard(_tvfactoryIndex, iTexture, fDefaultWidth, fDefaultHeight);
        }


        internal void MiniMesh(TVMiniMesh pMiniMesh)
        {
            mTVParticleSystem.SetMiniMesh(_tvfactoryIndex, pMiniMesh);
        }

        internal void PointSprite(int iTexture)
        {
            mTVParticleSystem.SetPointSprite(_tvfactoryIndex, iTexture);
        }

        internal void PointSprite(int iTexture, float fDefaultSize)
        {
            mTVParticleSystem.SetPointSprite(_tvfactoryIndex, iTexture, fDefaultSize);
        }

        // TODO: texture should be a Diffuse object right?  Hooked into an Appearance object?
        internal int Texture()
        {
            return mTVParticleSystem.GetEmitterTexture(_tvfactoryIndex);
        }

        public Vector3d Direction()
        {
            return Helpers.TVTypeConverter.FromTVVector(mTVParticleSystem.GetEmitterDirection(_tvfactoryIndex));
        }

        public void Direction(bool bEnableMainDirection, Vector3d vMainDirection, Vector3d vRandomDirectionFactor)
        {
            mTVParticleSystem.SetEmitterDirection(_tvfactoryIndex, bEnableMainDirection, Helpers.TVTypeConverter.ToTVVector(vMainDirection),
                                          Helpers.TVTypeConverter.ToTVVector(vRandomDirectionFactor));
        }

        public void DirectionCubeMask(bool bEnableCubeTextureMask, int iCubeTextureMask)
        {
            mTVParticleSystem.SetEmitterDirectionCubeMask(_tvfactoryIndex, bEnableCubeTextureMask, iCubeTextureMask);
        }

        public void DirectionRandomFactor(Vector3d vRandomDirectionFactor)
        {
            mTVParticleSystem.SetEmitterDirectionRandomFactor(_tvfactoryIndex, Helpers.TVTypeConverter.ToTVVector(vRandomDirectionFactor));
        }

        public TV_2DVECTOR DefaultBillboardSize()
        {
            return mTVParticleSystem.GetEmitterDefaultBillboardSize(_tvfactoryIndex);
        }

        public TV_COLOR EmitterDefaultColor()
        {
            return mTVParticleSystem.GetEmitterDefaultColor(_tvfactoryIndex);
        }

        public void ParticleDefaultColor(TV_COLOR cColor)
        {
            mTVParticleSystem.SetParticleDefaultColor(_tvfactoryIndex, cColor);
        }

        public int ParticleCount()
        {
            return mTVParticleSystem.GetEmitterParticleCount(_tvfactoryIndex);
        }

        // NOTE: According to Sylvain, moveEmitter moves the emitter but the already emitted particles will not move in relation
        //       for that you must use SetGlobalPosition to move the emitter AND all currently emitted particles. So for exhaust plumes
        //       we want SetGlobalPosition.
        public void Move(Vector3d vEmitterPosition)
        {
            mTVParticleSystem.MoveEmitter(_tvfactoryIndex, Helpers.TVTypeConverter.ToTVVector(vEmitterPosition));
        }

        public void Shape()
        {
            mTVParticleSystem.SetEmitterShape(_tvfactoryIndex); // point is default
        }

        public void Shape(CONST_TV_EMITTERSHAPE eShape)
        {
            mTVParticleSystem.SetEmitterShape(_tvfactoryIndex, eShape);
        }

        public void BoxSize(Vector3d bSize)
        {
            mTVParticleSystem.SetEmitterBoxSize(_tvfactoryIndex, Helpers.TVTypeConverter.ToTVVector(bSize));
        }

        public void SphereRadius(float fRadius)
        {
            mTVParticleSystem.SetEmitterSphereRadius(_tvfactoryIndex, fRadius);
        }

        public void Gravity(bool bUseGravity, Vector3d vGravityVector)
        {
            mTVParticleSystem.SetEmitterGravity(_tvfactoryIndex, bUseGravity, Helpers.TVTypeConverter.ToTVVector(vGravityVector));
        }

        public void Power(float fPower, float fParticleLifeTime)
        {
            mTVParticleSystem.SetEmitterPower(_tvfactoryIndex, fPower, fParticleLifeTime);
        }

        public void Speed(float fGenerationSpeedMS)
        {
            mTVParticleSystem.SetEmitterSpeed(_tvfactoryIndex, fGenerationSpeedMS);
        }

        // ideally this should be apart of the interpolator object
        public void Looping(bool bLooping)
        {
            mTVParticleSystem.SetEmitterLooping(_tvfactoryIndex, bLooping);
        }

        // these are used to interpolate the direction, location, speed, shape  as well as size and color of the particle emitter
        public void SetEmitterKeyFrames(int eEmitterKeyUsage, int iNumKeyFrames,
                                        TV_PARTICLEEMITTER_KEYFRAME[] pFirstKeyFrameArray)
        {
            mTVParticleSystem.SetEmitterKeyFrames(_tvfactoryIndex, eEmitterKeyUsage, iNumKeyFrames, pFirstKeyFrameArray);
        }

        // this is used to interpolate the color and size of the particles
        public void SetParticleKeyFrames(int eParticleKeyUsage, int iNumKeyFrames,
                                         TV_PARTICLE_KEYFRAME[] pFirstKeyFrameArray)
        {
            mTVParticleSystem.SetParticleKeyFrames(_tvfactoryIndex, eParticleKeyUsage, iNumKeyFrames, pFirstKeyFrameArray);
        }

        public int EmitterShape
        {
            get { return mEmitterShape; }
            set
            {
                mEmitterShape = value;
                mTVParticleSystem.SetEmitterShape(value);
            }
        }

        public int EmitterAlphaBlending
        {
            get { return mEmitterAlphaBlending; }
            set
            {
                mEmitterAlphaBlending = value;
                mTVParticleSystem.SetEmitterAlphaBlending(value);
            }
        }
        #endregion



        public bool GetParent(ref int retNodeType, ref int retObjectIndex, ref int retSubIndex)
        {
            return mTVParticleSystem.GetParent(_tvfactoryIndex, ref retNodeType, ref retObjectIndex, ref retSubIndex);
        }

        public void AttachTo(CONST_TV_NODETYPE eObjectType, int iObjectIndex, int iSubIndex)
        {
            mTVParticleSystem.AttachTo(_tvfactoryIndex, eObjectType, iObjectIndex, iSubIndex);
        }

        public void AttachTo(CONST_TV_NODETYPE eObjectType, int iObjectIndex, int iSubIndex, bool bKeepMatrix)
        {
            mTVParticleSystem.AttachTo(_tvfactoryIndex, eObjectType, iObjectIndex, iSubIndex, bKeepMatrix);
        }

        public void SetParent(CONST_TV_NODETYPE eObjectType, int iObjectIndex, int iSubIndex)
        {
            mTVParticleSystem.SetParent(_tvfactoryIndex, eObjectType, iObjectIndex, iSubIndex);
        }

        public void SetParentEx(CONST_TV_NODETYPE eObjectType, int iObjectIndex, int iSubIndex)
        {
            mTVParticleSystem.SetParentEx(_tvfactoryIndex, eObjectType, iObjectIndex, iSubIndex);
        }

        public void SetParentEx(CONST_TV_NODETYPE eObjectType, int iObjectIndex, int iSubIndex,
                                float fTranslationOffsetX)
        {
            mTVParticleSystem.SetParentEx(_tvfactoryIndex, eObjectType, iObjectIndex, iSubIndex, fTranslationOffsetX);
        }

        public void SetParentEx(CONST_TV_NODETYPE eObjectType, int iObjectIndex, int iSubIndex,
                                float fTranslationOffsetX, float fTranslationOffsetY)
        {
            mTVParticleSystem.SetParentEx(_tvfactoryIndex, eObjectType, iObjectIndex, iSubIndex, fTranslationOffsetX,
                                  fTranslationOffsetY);
        }

        public void SetParentEx(CONST_TV_NODETYPE eObjectType, int iObjectIndex, int iSubIndex,
                                float fTranslationOffsetX, float fTranslationOffsetY, float fTranslationOffsetZ)
        {
            mTVParticleSystem.SetParentEx(_tvfactoryIndex, eObjectType, iObjectIndex, iSubIndex, fTranslationOffsetX,
                                  fTranslationOffsetY, fTranslationOffsetZ);
        }

        public void SetParentEx(CONST_TV_NODETYPE eObjectType, int iObjectIndex, int iSubIndex,
                                float fTranslationOffsetX, float fTranslationOffsetY, float fTranslationOffsetZ,
                                float fRotationOffsetX)
        {
            mTVParticleSystem.SetParentEx(_tvfactoryIndex, eObjectType, iObjectIndex, iSubIndex, fTranslationOffsetX,
                                  fTranslationOffsetY, fTranslationOffsetZ, fRotationOffsetX);
        }

        public void SetParentEx(CONST_TV_NODETYPE eObjectType, int iObjectIndex, int iSubIndex,
                                float fTranslationOffsetX, float fTranslationOffsetY, float fTranslationOffsetZ,
                                float fRotationOffsetX, float fRotationOffsetY)
        {
            mTVParticleSystem.SetParentEx(_tvfactoryIndex, eObjectType, iObjectIndex, iSubIndex, fTranslationOffsetX,
                                  fTranslationOffsetY, fTranslationOffsetZ, fRotationOffsetX, fRotationOffsetY);
        }

        public void SetParentEx(CONST_TV_NODETYPE eObjectType, int iObjectIndex, int iSubIndex,
                                float fTranslationOffsetX, float fTranslationOffsetY, float fTranslationOffsetZ,
                                float fRotationOffsetX, float fRotationOffsetY, float fRotationOffsetZ)
        {
            mTVParticleSystem.SetParentEx(_tvfactoryIndex, eObjectType, iObjectIndex, iSubIndex, fTranslationOffsetX,
                                  fTranslationOffsetY, fTranslationOffsetZ, fRotationOffsetX, fRotationOffsetY,
                                  fRotationOffsetZ);
        }

        #region Geometry Members

        internal override Shader Shader
        {
            set
            {
                // TODO: if the type is not billboard throw exceptiion
                _shader = value;
                mTVParticleSystem.SetBillboardShader(_tvfactoryIndex, _shader.TVShader);
            }
        }


        // TODO: should change to SetAlphaTest and also seperate vars for getting the alpha test bool and refvalue?
        internal override void SetAlphaTest (bool enable, int iGroup)
        {            
            _alphaTestEnable = enable;
            mTVParticleSystem.SetEmitterAlphaTest(_tvfactoryIndex, _alphaTestEnable);
        }

        //public void AlphaTest(bool bAlphaTest, int iAlphaRef)
        //{
        //    _particle.SetEmitterAlphaTest(_tvfactoryIndex, bAlphaTest, iAlphaRef);
        //}

        //public void AlphaTest(bool bAlphaTest, int iAlphaRef, bool bDepthWrite)
        //{
        //    _particle.SetEmitterAlphaTest(_tvfactoryIndex, bAlphaTest, iAlphaRef, bDepthWrite);
        //}

        internal override CONST_TV_BLENDINGMODE BlendingMode
        {
            set
            {
                mTVParticleSystem.SetEmitterAlphaBlending(_tvfactoryIndex, CONST_TV_PARTICLECHANGE.TV_CHANGE_ALPHA, value);
            }
        }

        internal void AlphaBlending()
        {
            mTVParticleSystem.SetEmitterAlphaBlending(_tvfactoryIndex);
        }

        internal void AlphaBlending(CONST_TV_PARTICLECHANGE eChange)
        {
            mTVParticleSystem.SetEmitterAlphaBlending(_tvfactoryIndex, eChange);
        }

        internal void AlphaBlending(CONST_TV_PARTICLECHANGE eChange, CONST_TV_BLENDINGMODE eBlending)
        {
            mTVParticleSystem.SetEmitterAlphaBlending(_tvfactoryIndex, eChange, eBlending);
        }


        public override int CullMode
        {
            set { throw new ArgumentException("Invalid property for this node type."); }
        }


        public override int VertexCount
            // these could be gotten from the minimesh or just return 2 for billboards times visible
        {
            get { throw new ArgumentException("Invalid property for this node type."); }
        }

        public override int GroupCount
        {
            get { throw new ArgumentException("Invalid property for this node type."); }
        }

        public override int TriangleCount
        {
            get { throw new ArgumentException("Invalid property for this node type."); }
        }

        internal override PickResults AdvancedCollide(Vector3d start, Vector3d end, KeyCommon.Traversal.PickParameters parameters)
        {
        	// picking done with model space ray in model space
            throw new NotImplementedException();
            
           // TV_3DMATRIX m = Helpers.TVTypeConverter.ToTVMatrix(worldMatrix);
           // mTVParticleSystem.SetEmitterMatrix(_tvfactoryIndex, m); 

           // return new PickResults(); //  throw new NotImplementedException();
        }

            //mTVParticleSystem.Update();

            // TODO: according to Sylvain, if i have 50 fighter craft and want to share an emitter, i cant do anything to restore
            //       the previous instance data (e.g. a keyframe) for each position i need it in so that i can then "update" them properly
            //       from the 
            //_particle.UpdateEx (    entity.Elapsed();)


            /// <summary>
            /// 
            /// </summary>
            /// <param name="matrix"></param>
            /// <param name="scene">Usually Context.Scene rather than Entity.Scene since AssetPlacementTool preview Entity's are not connected to Scene.</param>
            /// <param name="model"></param>
            /// <param name="elapsedSeconds"></param>
        internal override void Render(Matrix matrix, Scene.Scene scene, Model model, double elapsedSeconds)
        {
        	// TODO: it should be impossible for _resourceStatus to be "Loaded" before it's actually loaded
	    	//       HOWEVER if paging out, we could start to render here first since it's not synclocked and then
	    	//       while minimesh.Render() we page out and set _resourceStatus to Unloading but we're already in .Render()!
	    	
	    	// NOTE: we check PageableNodeStatus.Loaded and NOT TVResourceIsLoaded because that 
	    	// TVIndex is set after Scene.CreateEmitter() and thus before we've finished configuring the emitter
	        if (_resourceStatus != PageableNodeStatus.Loaded ) return;
        		
            // render to camera space - note for fast moving systems we need for the emitter to be at Identity and instead
            // rely on the global matrix to move both with the ship/engine and with respect to camera.
           // mTVParticleSystem.SetEmitterMatrix(_tvfactoryIndex, Helpers.TVTypeConverter.ToTVMatrix(model.Matrix * entityInstance.RegionMatrix));
           // mTVParticleSystem.SetEmitterPosition(_tvfactoryIndex, Helpers.TVTypeConverter.ToTVVector (cameraSpacePosition));
            // TODO: I think this need to use globalmatrix is going to stop us from having emitter's seperate
            // TODO: Further, now i understand why particles are not exactly keyframeable... they get released and the individual
            //           particles have their own seperate matrices based on where they were emitted.
            //           FURTHER, tests have confirmed that increasing the shared emitter count only speeds up the emitter
            //           as the updates are cumulative elapsed, and not key frame markers
            //           TODO: this cumulative elapsed is critical to remember and why sharing TVParticleSystem is just not good.
            //                 best to do as Actor and duplicate but with internal wrapper for duplicating.
            
           // mTVParticleSystem.SetGlobalMatrix(Helpers.TVTypeConverter.ToTVMatrix(model.Matrix * entityInstance.RegionMatrix));
           // TV_3DVECTOR newPos = Helpers.TVTypeConverter.ToTVVector(cameraSpacePosition);
           // mTVParticleSystem.SetGlobalPosition(newPos.x , newPos.y , newPos.z );
            mTVParticleSystem.SetGlobalMatrix(Helpers.TVTypeConverter.ToTVMatrix(matrix));
//            Random rand = new Random();
            // TODO: im passing in elapsedSeconds, i should not be using the below Engine.AccurateTimeElapsed
//            throw new NotImplementedException("Following keyframe line has been temp commented out since changing Render() parameters for Geometry.Render() to no longer contain entity.Instance.  Instead the Model should set any properties needed by Render()?");
//            float keyframe = 0; // entityInstance.TempFrame += CoreClient._CoreClient.Engine.AccurateTimeElapsed();
//            if (entityInstance.TempFrame > 4) entityInstance.TempFrame =0;
            // we must do UpdateEx in render since we are sharing emitters and we would lose our update if we did in this Geometry.Update()
            // instead of Geometry.Render() since the last shared version's Update would overwrite all the other's Updates.
//            keyframe /= 1000;
            
            
            try
            {
            	// unfortunatley UpdateEx() does not accept a keyframe, it's just elapsed and so you can slow down or speed up
				// the particle system only, you cannot pre-position it (not without some hacking involving cycling updates til
				// you loop around to where u want to resume, but that's not at all reliable with random emitters so its really a no go)
	//			mTVParticleSystem.UpdateEx(_tvfactoryIndex, keyframe);

	            mTVParticleSystem.Update();
	            // TODO: since particlesystem always renders all particle emitters,
	            // until sylvain provides an overloaded
	            // render where we can specify a particular emitter, we're just going to manually call 
	            // coreclient.ParticleSystem.Render()
	            // in our scene.
	
	            // note; no need to ever change the global position because it's always at origin.  This is a factory class effectively
	            //mTVParticleSystem.SetGlobalPosition(position.x, position.y, position.z);

                mTVParticleSystem.Render();
            }
            catch
            {
            }
        }
        #endregion

        #region IBoundVolume Members
        protected override void UpdateBoundVolume()
        {
        	if (_resourceStatus != PageableNodeStatus.Loaded) return;
        	        	
            TV_3DVECTOR min, max, center;
            min.x = min.y = min.z = 0f;
            max.x = max.y = max.z = 0f;
            center.x = center.y = center.z = 0f;
            
            bool worldSpace = false;

            if (_box == null)
            	_box = new BoundingBox ();
            else
            	_box.Reset ();
            
            // TODO: ideally, i should run emitter full cycle and get biggest resutling box and cache it
            //mTVParticleSystem.GetEmitterBoundingSphere(_tvfactoryIndex, ref center, ref radius, worldSpace);
            mTVParticleSystem.GetEmitterBoundingBox(_tvfactoryIndex, ref max, ref min, worldSpace);
            _box.Resize(min.x, min.y, min.z, max.x, max.y, max.z);
            _box.Resize(-10f, -10f, -10f, 10f, 10f, 10f);
            // float radius = 0;
            _sphere = new BoundingSphere(_box);

            DisableChangeFlags(Keystone.Enums.ChangeStates.BoundingBox_TranslatedOnly | Keystone.Enums.ChangeStates.BoundingBoxDirty);
        }
        #endregion

        #region IDisposeable members
        protected override void DisposeUnmanagedResources()
        {
            base.DisposeUnmanagedResources();
            try
            {
                // TODO: destroy the keyframes?
                _emitterKF = null;

                // TODO: destroy all emitters and attractors
                // destroy the emitter
                mTVParticleSystem.DestroyEmitter(_tvfactoryIndex);
            }
            catch
            {
            }
        }
        #endregion
    }
}