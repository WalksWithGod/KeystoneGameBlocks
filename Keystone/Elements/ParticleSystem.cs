using System;
using KeyCommon.Traversal;
using Keystone.Collision;
using Keystone.IO;
using Keystone.Resource;
using Keystone.Traversers;
using Keystone.Types;
using MTV3D65;
using System.Collections.Generic;
using Keystone.Appearance;
using Keystone.Shaders;

namespace Keystone.Elements
{
    public class ParticleSystemDuplicate
    {
        internal TVParticleSystem mDuplicateSystem;

        // TODO: verify emitters and attactors get auatomaticallly shared even when created AFTER the particleSystem has been duplicated.
        // NOTE: here we only track differences in Appearance.  
        // Duplicates internally shares geometry, bones and animations.  Thus we can have different shaders, textures and materials on the duplicates.
        // Using the _appearanceHashCode, we can avoid having to SetShader, SetTexture, SetMaterial, etc. during AttributeGroup.Apply() or Appearance.Apply()
        // Animations and Keyframes are part of the prefab however.  I'm not sure if tv internally duplicates those when setting up AnimationClips.
        private CONST_TV_LIGHTINGMODE _lightingMode; // lightingmode is cached only so during traversal we can tell if we need to change it after rendering a previous instance.  But it does NOT need to be saved/read from file
        private Shader _shader;
        private bool _alphaTestEnable;
        private int _alphaTestRefValue;
        private bool _alphaTestDepthBufferWriteEnable;
        private CONST_TV_BLENDINGMODE[] mBlendingModes;
        private int _appearanceHashCode;

        public ParticleSystemDuplicate()
        {


        }

        // todo: maybe here we pass in the full ParticleSystem and not just the tvparticlesystem
        public ParticleSystemDuplicate(TVParticleSystem duplicate)
        {
            mDuplicateSystem = duplicate;
        }

        internal int LastAppearance
        {
            get { return _appearanceHashCode; }
            set { _appearanceHashCode = value; }
        }


        /// <summary>
        /// Shader is applied via Appearance only to emitters of type BILLBOARD only.
        /// </summary>
        internal void SetShader(int emitterIndex, Shader value)
        {
            
                if (_shader == value && (value == null || value.PageStatus == PageableNodeStatus.Loaded)) return;

                if (value != null)
                {
                    // also we don't want to assign this shader if it's underlying tvshader is not yet paged in.
                    if (value.PageStatus != PageableNodeStatus.Loaded)
                    {
                        System.Diagnostics.Debug.WriteLine("ParticleSystem.Shader '" + value.ID + "' not loaded.  Cannot assign to emitter billboard '" + mDuplicateSystem.GetName() + "'");
                        return;
                    }
                }

                _shader = value;
                if (_shader == null)
                    mDuplicateSystem.SetBillboardShader(emitterIndex, null);
                else
                    mDuplicateSystem.SetBillboardShader(emitterIndex, _shader.TVShader);
            
        }


        // TODO: should change to SetAlphaTest and also seperate vars for getting the alpha test bool and refvalue?
        internal void SetAlphaTest(int emitterIndex, bool enable, int iGroup)
        {

            _alphaTestEnable = enable;
            mDuplicateSystem.SetEmitterAlphaTest(emitterIndex, _alphaTestEnable, _alphaTestRefValue, _alphaTestDepthBufferWriteEnable);

        }


        internal int AlphaTestRefValue
        {
            get { return _alphaTestRefValue; }
            set
            {
                _alphaTestRefValue = value;
            }
        }

        internal bool AlphaTestDepthWriteEnable
        {
            get { return _alphaTestDepthBufferWriteEnable; }
            set
            {
                _alphaTestDepthBufferWriteEnable = value;
            }
        }


        /// <summary>
        /// For opacity, BlendingMode must be set to .TV_BLEND_ALPHA
        /// </summary>
        internal CONST_TV_BLENDINGMODE GetBlendingMode(int emitterIndex)
        {
     
            return mBlendingModes[emitterIndex]; // mSystem.GetEm.GetBlendingMode(emitterIndex, value);
            
        }

        internal void SetBlendingMode(CONST_TV_PARTICLECHANGE change, CONST_TV_BLENDINGMODE mode, int emitterIndex)
        {
            // NOTE: If the emitter is using a TVMinimesh, this call does not sem to work.
            //       The workaround is for DefaultAppearance.Apply() to check if the emitter is using TVMinimesh
            //       and if so, set the blendingmode directly on the TVMinimesh
            mDuplicateSystem.SetEmitterAlphaBlending(emitterIndex, change, mode);
        }

        internal void SetBlendingMode(CONST_TV_PARTICLECHANGE change, CONST_TV_BLENDEX src, CONST_TV_BLENDEX dest, int emitterIndex)
        {
            // NOTE: If the emitter is using a TVMinimesh, this call does not sem to work.
            //       The workaround is for DefaultAppearance.Apply() to check if the emitter is using TVMinimesh
            //       and if so, set the blendingmode directly on the TVMinimesh
            mDuplicateSystem.SetEmitterAlphaBlendingEx(emitterIndex, change, src, dest);
        }

        internal void SetBlendingMode(CONST_TV_PARTICLECHANGE change, CONST_TV_BLENDEX mode, int emitterIndex)
        {
            // NOTE: If the emitter is using a TVMinimesh, this call does not sem to work.
            //       The workaround is for DefaultAppearance.Apply() to check if the emitter is using TVMinimesh
            //       and if so, set the blendingmode directly on the TVMinimesh
            mDuplicateSystem.SetEmitterAlphaBlendingEx(emitterIndex, change, mode);
        }

        // materials and textures can be different for different duplicates
        internal int GetColor(int groupID)
        {
            throw new NotImplementedException();
            //return mDuplicateSystem.GetPar.GetMaterial(groupID);
        }

        /// <summary>
        /// Takes the diffuse value of a Material and assigns it to the emitter.
        /// Oddly, the change in color is not instanteious.  It changes gradually
        /// </summary>
        /// <param name="emitterIndex"></param>
        /// <param name="color"></param>
        internal void SetColor(int emitterIndex, Color color)
        {
              mDuplicateSystem.SetParticleDefaultColor(emitterIndex, Helpers.TVTypeConverter.ToTVColor(color));
        }

        internal int GetTextureEx(CONST_TV_LAYER layer, int emitterIndex)
        {
            
            throw new NotImplementedException();
            //return mDuplicateSystem.GetTextureEx((int)layer, emitterIndex);
        }

        internal void SetTexture(int emitterIndex, CONST_TV_EMITTERTYPE type, int textureID)
        {
            // TODO: we need to do this based on the TYPE of emitter (pointsprite, billboard, minimesh)
            // VERIFIED - duplicates CAN use seperate textures
            if (type == CONST_TV_EMITTERTYPE.TV_EMITTER_POINTSPRITE)
                mDuplicateSystem.SetPointSprite(emitterIndex, textureID);
            else if (type == CONST_TV_EMITTERTYPE.TV_EMITTER_BILLBOARD)
                mDuplicateSystem.SetBillboard(emitterIndex, textureID);
        }

        internal void SetCubeTextureMask(int emitterIndex, int textureID)
        {

        }
        // todo: i think we need to save the actual mesh relative path and texture relative path to the emitter so we can save and restore it.
        //       and for the relative texture path, this is necessary for pointsprite and billboard emitters too
        internal void SetMinimeshTexture(TVMiniMesh minimesh, int textureID)
        {
            if (minimesh != null)
                minimesh.SetTexture(textureID);
        }
    }

    public class ParticleSystem : Geometry
    {
        // TODO: 1) how do we manage particles across zones 
        //			a) Keep in mind some "particles" are "heavy" and must be tracked and are not purely decorative particles
        //			
        // TODO: 2) How do we manage particles that need to use a starting velocity /or inherited relative position such as for high speed rocket plume


        // TODO: I think a particle system might actually end up being better implemented as a Model.cs or even Geometry.cs derived type.
        //       TVParticleSystem especially is very much rendering based... there's no accessible records for each particle
        //       So in cases where we do want to use TVParticleSystem, I think they should be treated as DUMB PARTICLES
        //       that are "fire & forget" and where the individual particles are decorative only and do not matter nor
        //       need to be tracked.  The good news here is, they wind up being very fast and lightweight.

        //       For lasers and bullets that can impact the world, there we can make our own system for tracking those particles
        //       and rendering them with minimesh.

        //       So this being the case, 
        //       - is TVParticleSystem a geometry, model, entity?  TVParticleSystem.Duplicate() exists.
        //       - is TVParticleSystem shareable as a resource node?
        //       - does TVParticleSystem need to bother with "Appearance" nodes "Textures" "Materials" etc?
        //       - should Emitter and Attractor be seperate Scene Nodes or just internal aspects of the Geometry node
        //         that is a TVParticleSystem


        //TODO: I think it might be ok that somewhere in the system, a reference to any
        // used Minimesh is maintained.  I think ultimiately, ill only be using minimeshes for particles
        // and maybe for some dynamic grass simulation where an array of grasses are "planted" in the proper spots
        // based on the chunk and neighbor chunk's vegetation maps.  In other words, im not as much a fan anymore
        // of having to instance these minimeshes when new chunks are loaded.  They should be shared... only when
        // the type of grass changes then should a new one be loaded into the pool for use.  FXDecoration or something
        // which can handle not jsut grass, but other non interactive things like trash, small rocks, small shrubs, etc.

        // always keep reference to the original TVParticleSystem that was used make the duplicates from.
        // if we try to just store it only in the mDuplicates, it could get removed when it's deleted
        // from the scene for some reason
        internal TVParticleSystem mSystem; // the duplicated TVParticleSystem.  This is not the same reference to the original mSystem in ParticleSystem.cs

        
        // todo: when you duplicate, do the emitter and attractor indices rmeain the same?
        // todo: Emitters and Attractors have properties so we should probably store them as a struct
        EmitterProperties[] mEmitters; // these can be thought of as mesh Groups and not as child objects.  Just like groups in Mesh3d or Actor3d, emitters can have names.

        AttractorProperties[] mAttractors;

        // todo: i could save the particleSystem using tv's build in serializer and deserializer and then store the resourcePath 
        //       This would treat the particleSystem as a Geometry
        //CONST_TV_PARTICLEEMITTER_KEYUSAGE[] mEmitterFramesUsage;
        //CONST_TV_PARTICLEEMITTER_KEYUSAGE[] mParticleUsage;
        //Dictionary<int, TV_PARTICLEEMITTER_KEYFRAME[]> mEmittenKeyFrames;
        //Dictionary<int, TV_PARTICLE_KEYFRAME[]> mParticleColorFrames;

        // attributes are seperated by a space character eg: attrib1="value1" attrib2="value2"
        // attributes with an array or struct internally seperated by comma   eg: attrib1="value1.x, valuey1.y, value1.z" attrib2="value2.x, value2.y, value2.z"
        // attributes with an array of arrays are seperated by {} and comma   eg: attrib1="{array1.x, array1.y, array1.z}{array2.x, array2.y, array2.z}"
        //                     I could use spaces within the attributes and then commas to seperate the array elements


        // todo: i need in Keystone.Helpers.TypeConverter to convert these arrays to types that our ExtensionMethods can serialize and deserialize


        // keyed by the parent model's id
        internal Dictionary<string, ParticleSystemDuplicate> mDuplicates;



        // indexer
        public ParticleSystemDuplicate this[string parentModelID]
        {
            get
            {
                if (mDuplicates == null) throw new ArgumentOutOfRangeException();

                ParticleSystemDuplicate dupe;
                if (mDuplicates.TryGetValue(parentModelID, out dupe))
                    return dupe;

                return null;
            }
        }

        // NOTE: id really should be a resource path, however, when first creating a new ParticleSystem, we load a blank one
        // and then have the opportunity to save it
        internal ParticleSystem(string id)
            : base(id)
        {
          
        }

        public static ParticleSystem Create(string id)
        {

            // This Geometry class is shareable, but we internally create duplicates like we do with Actor3d

            ParticleSystem system = (ParticleSystem)Repository.Get((id));
            if (system != null) return system;

            system = new ParticleSystem(id);

            return system;
        }

        public override object Traverse(ITraverser target, object data)
        {
            return target.Apply(this, data);
        }

        private int GetEmitterIndex(int groupIndex)
        {
            if (mEmitters == null || mEmitters.Length == 0) return -1;
            for (int i = 0; i < mEmitters.Length; i++)
                if (mEmitters[i].Index == groupIndex) return i;

            return - 1;
        }

        private int GetAttractorIndex(int groupIndex)
        {
            if (mAttractors == null || mAttractors.Length == 0) return -1;
            for (int i = 0; i < mAttractors.Length; i++)
                if (mAttractors[i].Index == groupIndex) return i;

            return -1;
        }

        #region IPageableTVNode Members
        public override void LoadTVResource()
        {
            

            // todo: verify if we duplicate, all emitters, attractors and keyframes are duplicated as well.
            try
            {
                System.Diagnostics.Debug.Assert(mSystem == null, "ParticleSystem.LoadTVResource() - system '" + _id + "' is NOT NULL");

                //// TODO: here we should load from binary file that particleSystem and create alll EmitterProperties it's keyframes and AttractorProperties and then Apply() them to the overall system.
                mSystem = CoreClient._CoreClient.Scene.CreateParticleSystem(_id); //_id here should be a resource path
                _tvfactoryIndex = mSystem.GetIndex();

                string fullPath = Core.FullNodePath(_id);

                if (System.IO.File.Exists (fullPath))
                    LoadTVResource(fullPath);

                //// todo: with multiple Emitters we should use AtttributeGroup.cs and not DefaultAppearance.cs
                //// todo: need to get these values from the deserialized properties 
                //// todo: plugin needs ability to add/remove emitters and attractros.  We may need to reconstruct the mSystem after such changes and then update the mSystem of duplicates
                //// todo: DisposeManagedResources() needs to occur on destroy
                //// todo: i think when adding emitters after LoadTVResource() we need to verify the duplicates receive those updates otherwise we should unload and reload the entire tvparticleSystem and assign duplicates the new system
                //mEmitters = new EmitterProperties[1];

                //this.AddEmitter(CONST_TV_EMITTERTYPE.TV_EMITTER_BILLBOARD, 32);
                //for (int i = 0; i < mEmitters.Length; i++)
                //{
                //    mEmitters[i].Apply(this);
                //}
                // TODO: // then test serialization/deserialization, then keyframes, and plugin updates last


                // we could treat each Emitter as belonging to a Model.Geometry.Group() and then we can use AttributeGroup() for each emitter to host textures

                // we should be able to make prefabs of the ParticleSystem Entity. 

                // for being able to use LOD, _particleSystem should be in a Model? Probaly not, just in a public class EmittersGeometry : Geometry {}
                //        TV_PARTICLEEMITTER_KEYFRAME emitterKeyFrame;
                //        TV_PARTICLE_KEYFRAME particleKeyFrame;
                //        mSystem.SetEmitterKeyFrames(emitterID, cemitterKeyFrame.Length, emitterKeyframes);
                //        mSystem.SetParticleKeyFrames(emitterID, 0, particleFrames.Length, particleFrames);
                //        mSystem.SetBillboardShader(emitterIndex, shader); // this would get set on the emitter by an AttributeGroup

                //_particle.
                //_particle.SetGlobalMatrix();
                //_particle.SetGlobalPosition();
                //_particle.SetGlobalRotation();
                //_particle.SetGlobalScale();


                // _particle.SetEmitterKeyFrames(0, 0, TV_PARTICLE_KEYFRAME, keyframes);
                // todo: does this duplicate emitters too?

                //mSystem.EnableFrustumCulling(false);



                //// NOTE: emitter types can be different in the system... but i dont think the emitters can be changed 
                //// so we should have different emitter classes for the different kinds
                //// emitter move constants
                //CONST_TV_EMITTERMOVEMODE.TV_EMITTER_LERP;
                //CONST_TV_EMITTERMOVEMODE.TV_EMITTER_NOLERP;
                //// emitter shape constants
                //CONST_TV_EMITTERSHAPE.TV_EMITTERSHAPE_BOXSURFACE;
                //CONST_TV_EMITTERSHAPE.TV_EMITTERSHAPE_BOXVOLUME;
                //CONST_TV_EMITTERSHAPE.TV_EMITTERSHAPE_POINT;
                //CONST_TV_EMITTERSHAPE.TV_EMITTERSHAPE_SPHERESURFACE;
                //CONST_TV_EMITTERSHAPE.TV_EMITTERSHAPE_SPHEREVOLUME;
                //// emitter type constants
                //CONST_TV_EMITTERTYPE.TV_EMITTER_BILLBOARD;       // utilizes a single texture via system.SetBillboard (iEmitterIndex,...)
                //                                                                              // note: for help on minimesh sprite issues, see the remarks on the stardust field)
                //CONST_TV_EMITTERTYPE.TV_EMITTER_MINIMESH;        // utilizes a minimesh via system.SetMinimesh (iEmitterIndex, )
                //CONST_TV_EMITTERTYPE.TV_EMITTER_POINTSPRITE;   // utilizes a single texture via system.SetPointSprite (iEmitterIndex,...)

                //        Emit = pSystem.CreateEmitter(TV_EMITTER_POINTSPRITE, 8000)
                //pSystem.SetEmitterPosition Emit, Vector3(0, 250, 0)
                //pSystem.SetEmitterPower Emit, 40, 5
                //pSystem.SetEmitterSpeed Emit, 1
                //pSystem.SetEmitterDirection Emit, True, Vector3(0, -1, 0), Vector3(-1, -1, -1)
                //pSystem.SetEmitterSphereRadius Emit, 80
                //pSystem.SetEmitterGravity Emit, True, Vector3(0, 10, 0)
                //pSystem.SetEmitterLooping Emit, True

                //TVParticleSystem system;

                //system.SetEmitterMatrix 
                //system.SetEmitterPosition 
                //system.SetEmitterSpawnInterpolation 
                // system.GetEmitterBoundingBox     // for culling specific emitters ourselves
                // system.UpdateEx(iEmitter, fTimeSeconds);  // update a specific emitter
                // system.SetEmitterEnable    // for enabling / disabling emitters 

                // emitter appearance 

                //pSystem.SetEmitterShape Emit, TV_EMITTERSHAPE_SPHEREVOLUME
                //pSystem.SetParticleDefaultColor Emit, Col
                //pSystem.SetPointSprite Emit, pTex, 10



                // assign duplicates immediately after resource is loaded
                // todo: doesn't this assume all Model's hosting all the duplicates have been loaded?
                // todo: shouldn't we destroy duplicate systems first before we assign new duplicates based off of this mSystem?

                //if (mDuplicates != null)
                //    foreach (ParticleSystemDuplicate dupe in mDuplicates.Values)
                //        dupe.mDuplicateSystem = mSystem.Duplicate();

                SetChangeFlags(Keystone.Enums.ChangeStates.GeometryAdded |
                    Keystone.Enums.ChangeStates.AppearanceParameterChanged |
                    Keystone.Enums.ChangeStates.BoundingBoxDirty |
                    Keystone.Enums.ChangeStates.RegionMatrixDirty |
                    Keystone.Enums.ChangeStates.MatrixDirty, Keystone.Enums.ChangeSource.Self);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ParticleSystem.LoadTVResource() ERROR: - particle system path == " + _id + ex.Message);
                throw ex;
            }

            _resourceStatus = PageableNodeStatus.Loaded;
            SetChangeFlags(Keystone.Enums.ChangeStates.GeometryAdded | Keystone.Enums.ChangeStates.BoundingBoxDirty, Keystone.Enums.ChangeSource.Self);
        }

        const float VERSION = 1.0f;
        private void LoadTVResource(string resourcePath)
        {
            using (var stream = System.IO.File.Open(resourcePath, System.IO.FileMode.Open))
            {
                using (var reader = new System.IO.BinaryReader(stream, System.Text.Encoding.UTF8))
                {
                    float version = reader.ReadSingle();

                    int count = reader.ReadInt32();
                    // emitters
                    if (count > 0)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            // emitter properties
                            EmitterProperties emitter = ReadEmitterProperties(reader);
                                                       
                            // emitter keyframes
                            int frameCount = reader.ReadInt32();
                            if (frameCount > 0)
                            {
                                emitter.mEmitterFrames = new TV_PARTICLEEMITTER_KEYFRAME[frameCount];
                                for (int j = 0; j < frameCount; j++)
                                    emitter.mEmitterFrames[j] = ReadEmitterKeyframe(reader);
                            }

                            // particle keyframes
                            frameCount = reader.ReadInt32();
                            if (frameCount > 0)
                            {
                                emitter.mParticleFrames = new TV_PARTICLE_KEYFRAME[frameCount];
                                for (int j = 0; j < frameCount; j++)
                                    emitter.mParticleFrames[j] = ReadParticleKeyframe(reader);
                            }

                            if (emitter.type == CONST_TV_EMITTERTYPE.TV_EMITTER_MINIMESH)
                            {
                                emitter.mMesh = CoreClient._CoreClient.Scene.CreateMeshBuilder(emitter.Name);
                                string fullPath = System.IO.Path.Combine(CoreClient._CoreClient.ModsPath, emitter.MeshPath);
                                emitter.mMesh.LoadTVM(fullPath);
                                // mesh.LoadXFile();
                                //Keystone.Loaders.WavefrontObjLoader.ObjToTVM(obj)= new Keystone.Loaders.WaveFrontObj(path);
                                System.Diagnostics.Debug.Assert(emitter.mMesh.GetGroupCount() == 1, "ParticleSystem.LoadTVResource() - Minimesh must have exactly 1 group.");
                                emitter.Minimesh = CoreClient._CoreClient.Scene.CreateMiniMesh(emitter.maxParticles); // IMPORTANT: TV3D requires that the minimesh is created with at least as many meshes as will be used by max particlecount
                                emitter.Minimesh.CreateFromMesh(emitter.mMesh);
                            }

                            AddEmitter(emitter);
                        }
                    }

                    count = reader.ReadInt32();
                    // attractors
                    if (count > 0)
                    {
                        mAttractors = new AttractorProperties[count];
                        for (int i = 0; i < count; i++)
                        {
                            mAttractors[i] = ReadAttractors(reader);
                            mAttractors[i].Apply(this);
                        }
                    }
                }
            }
        }

        #region File I/O

        private EmitterProperties ReadEmitterProperties(System.IO.BinaryReader reader)
        {
            EmitterProperties result = new EmitterProperties();
            result.Index = -1;

            result.type = (CONST_TV_EMITTERTYPE)reader.ReadInt32();
            result.maxParticles = reader.ReadInt32(); ;
            result.shape = (CONST_TV_EMITTERSHAPE)reader.ReadInt32();

            result.Name = reader.ReadString();
            result.Enable = reader.ReadBoolean();
                        
            result.systemRadius = reader.ReadInt32(); // i believe this is radius when shape is spherical

            TV_3DVECTOR size;
            size.x = reader.ReadSingle();
            size.y = reader.ReadSingle();
            size.z = reader.ReadSingle();
            result.systemBoxSize = size;


            result.BillboardWidth = reader.ReadInt32();
            result.BillboardHeight = reader.ReadInt32();

            bool hasTexture = reader.ReadBoolean();
            if (hasTexture)
                result.TexturePath = reader.ReadString();
            bool hasCubeMapTexture = reader.ReadBoolean();
            if (hasCubeMapTexture)
                result.CubeTextureMaskPath = reader.ReadString();
            bool hasShader = reader.ReadBoolean();
            if (hasShader)
                result.ShaderPath = reader.ReadString();
            bool hasMinimesh = reader.ReadBoolean();
            if (hasMinimesh)
                result.MeshPath = reader.ReadString();

            result.Color.r = reader.ReadSingle();
            result.Color.g = reader.ReadSingle();
            result.Color.b = reader.ReadSingle();
            result.Color.a = reader.ReadSingle();

            result.BlendEx = reader.ReadBoolean();
            result.BlendExSrc = (CONST_TV_BLENDEX)reader.ReadInt32();
            result.BlendExDest = (CONST_TV_BLENDEX)reader.ReadInt32(); 

            result.BlendingMode = (CONST_TV_BLENDINGMODE)reader.ReadInt32();
            result.Change = (CONST_TV_PARTICLECHANGE)reader.ReadInt32();
            result.AlphaTest = reader.ReadBoolean();
            result.alphaRefValue = reader.ReadInt32();
            result.depthWrite = reader.ReadBoolean();

            result.gravityVector.x = reader.ReadSingle();
            result.gravityVector.y = reader.ReadSingle();
            result.gravityVector.z = reader.ReadSingle();

            result.useGravity = reader.ReadBoolean();
            result.EnableMainDirection = reader.ReadBoolean();

            result.direction.x = reader.ReadSingle();
            result.direction.y = reader.ReadSingle();
            result.direction.z = reader.ReadSingle();

            result.randomDirectionFactor.x = reader.ReadSingle();
            result.randomDirectionFactor.y = reader.ReadSingle();
            result.randomDirectionFactor.z = reader.ReadSingle();

            result.lifeTime = reader.ReadSingle();
            result.randomLifeTime = reader.ReadSingle();
            result.power = reader.ReadSingle();
            result.powerRandom = reader.ReadSingle();
            result.Speed = reader.ReadSingle();
            result.Loop = reader.ReadBoolean();

            result.EnableSpawnInterpolation = reader.ReadBoolean ();
            result.BillboardRotationEnable = reader.ReadBoolean();
            result.AngularSpeed = reader.ReadSingle();
            result.EnableMinimeshRotationScale = reader.ReadBoolean();


            result.EmitterKeyUsage = (CONST_TV_PARTICLEEMITTER_KEYUSAGE)reader.ReadInt32();
            result.ParticleKeyUseage = (CONST_TV_PARTICLE_KEYUSAGE) reader.ReadInt32();


            return result;
        }

        private TV_PARTICLEEMITTER_KEYFRAME ReadEmitterKeyframe(System.IO.BinaryReader reader)
        {
            TV_PARTICLEEMITTER_KEYFRAME keyframe;
            keyframe.fKey = reader.ReadSingle();

            keyframe.vMainDirection.x = reader.ReadSingle();
            keyframe.vMainDirection.y = reader.ReadSingle();
            keyframe.vMainDirection.z = reader.ReadSingle();

            keyframe.vLocalPosition.x = reader.ReadSingle();
            keyframe.vLocalPosition.y = reader.ReadSingle();
            keyframe.vLocalPosition.z = reader.ReadSingle();

            keyframe.vGeneratorBoxSize.x = reader.ReadSingle();
            keyframe.vGeneratorBoxSize.y = reader.ReadSingle();
            keyframe.vGeneratorBoxSize.z = reader.ReadSingle();


            keyframe.fGeneratorSphereRadius = reader.ReadSingle();

            keyframe.vDefaultColor.r = reader.ReadSingle();
            keyframe.vDefaultColor.g = reader.ReadSingle();
            keyframe.vDefaultColor.b = reader.ReadSingle();
            keyframe.vDefaultColor.a = reader.ReadSingle();

            keyframe.fParticleLifeTime = reader.ReadSingle();
            keyframe.fPower = reader.ReadSingle();
            keyframe.fSpeed = reader.ReadSingle();

            return keyframe;
        }

        private TV_PARTICLE_KEYFRAME ReadParticleKeyframe(System.IO.BinaryReader reader)
        {
            TV_PARTICLE_KEYFRAME keyframe;
            keyframe.fKey = reader.ReadSingle();

            keyframe.cColor.r = reader.ReadSingle();
            keyframe.cColor.g = reader.ReadSingle();
            keyframe.cColor.b = reader.ReadSingle();
            keyframe.cColor.a = reader.ReadSingle();

            keyframe.fSize.x = reader.ReadSingle();
            keyframe.fSize.y = reader.ReadSingle();
            keyframe.fSize.z = reader.ReadSingle();

            keyframe.vRotation.x = reader.ReadSingle();
            keyframe.vRotation.y = reader.ReadSingle();
            keyframe.vRotation.z = reader.ReadSingle();

            return keyframe;
        }

        private AttractorProperties ReadAttractors(System.IO.BinaryReader reader)
        {
            AttractorProperties attractor;
            attractor.DirectionField = reader.ReadBoolean();

            int index = this.mSystem.CreateAttractor(attractor.DirectionField);            
            attractor.Index = index;
            
            attractor.Name = reader.ReadString();
            attractor.Enable = reader.ReadBoolean();

            attractor.FieldDirection.x = reader.ReadSingle();
            attractor.FieldDirection.y = reader.ReadSingle();
            attractor.FieldDirection.z = reader.ReadSingle();

            attractor.Position.x = reader.ReadSingle();
            attractor.Position.y = reader.ReadSingle();
            attractor.Position.z = reader.ReadSingle();

            attractor.Radius = reader.ReadSingle();

            attractor.Attenuation.x = reader.ReadSingle();
            attractor.Attenuation.y = reader.ReadSingle();
            attractor.Attenuation.z = reader.ReadSingle();

            attractor.AttractionRepulsionConstant = reader.ReadSingle();
            attractor.VelocityDependancy = (CONST_TV_ATTRACTORVELOCITYPOWER)reader.ReadInt32();

            return attractor;
        }

        public override void SaveTVResource(string filepath)
        {
            if (_resourceStatus == PageableNodeStatus.Loaded && TVResourceIsLoaded)
            {
                // store the .tvp file
                using (var stream = System.IO.File.Open(filepath, System.IO.FileMode.Create))
                {
                    using (var writer = new System.IO.BinaryWriter(stream, System.Text.Encoding.UTF8))
                    {
                        writer.Write(VERSION);

                        int emitterCount = 0;
                        if (mEmitters != null && mEmitters.Length > 0)
                            emitterCount = mEmitters.Length;

                        writer.Write(emitterCount);
                        // Emitters
                        for (int i = 0; i < emitterCount; i++)
                        {
                            
                            // todo: how do i restore the lighting mode, textures, minimesh, corresponding groupAttributes?
                            WriteEmitter(mEmitters[i], writer) ;
                            
                            // emitter keyframes
                            int frameCount = 0;
                            if (mEmitters[i].mEmitterFrames != null && mEmitters[i].mEmitterFrames.Length > 0)
                                frameCount = mEmitters[i].mEmitterFrames.Length;

                            writer.Write(frameCount);
                            for (int j = 0; j < frameCount; j++)
                            {
                                WriteEmitterKeyframes(mEmitters[i].mEmitterFrames[j], writer);
                            }

                            // particle keyframes
                            frameCount = 0;
                            if (mEmitters[i].mParticleFrames != null && mEmitters[i].mParticleFrames.Length > 0)
                                frameCount = mEmitters[i].mParticleFrames.Length;

                            writer.Write(frameCount);
                            for (int j = 0; j < frameCount; j++)
                            {
                                WriteParticleKeframes(mEmitters[i].mParticleFrames[j], writer);
                            }
                        }

                        int attractorCount = 0;
                        if (mAttractors != null && mAttractors.Length > 0)
                            attractorCount = mAttractors.Length;

                        writer.Write(attractorCount);
                        // Attractors
                        for (int i = 0; i < attractorCount; i++)
                        {
                            WriteAttractor(mAttractors[i], writer);
                        }

                        writer.Flush();
                    }
                }
            }
            else throw new Exception("ParticleSystem cannot be NULL");
        }

        private void WriteEmitter(EmitterProperties emitter, System.IO.BinaryWriter writer)
        {
            writer.Write((int)emitter.type);
            writer.Write(emitter.maxParticles);
            writer.Write((int)emitter.shape);

            writer.Write(emitter.Name);
            writer.Write(emitter.Enable);
            
                        
            writer.Write(emitter.systemRadius); // i believe this is radius when shape is spherical
            writer.Write(emitter.systemBoxSize.x);
            writer.Write(emitter.systemBoxSize.y);
            writer.Write(emitter.systemBoxSize.z);

            writer.Write(emitter.BillboardWidth);
            writer.Write(emitter.BillboardHeight);

            // appearance
            bool hasTexture = !string.IsNullOrEmpty(emitter.TexturePath);
            writer.Write(hasTexture);
            if (hasTexture)
                writer.Write(emitter.TexturePath);
            bool hasCubeMask = !string.IsNullOrEmpty(emitter.CubeTextureMaskPath);
            writer.Write(hasCubeMask);
            if (hasCubeMask)
                writer.Write(emitter.CubeTextureMaskPath);
            bool hasShader = !string.IsNullOrEmpty(emitter.ShaderPath);
            writer.Write(hasShader);
            if (hasShader)
                writer.Write(emitter.ShaderPath);
            bool hasMinimesh = !string.IsNullOrEmpty(emitter.MeshPath);
            writer.Write(hasMinimesh);
            if (hasMinimesh)
                writer.Write(emitter.MeshPath);

            writer.Write(emitter.Color.r);
            writer.Write(emitter.Color.g);
            writer.Write(emitter.Color.b);
            writer.Write(emitter.Color.a);

            writer.Write (emitter.BlendEx);
            writer.Write ((int)emitter.BlendExSrc);
            writer.Write((int) emitter.BlendExDest); 

            writer.Write((int)emitter.BlendingMode);
            writer.Write((int)emitter.Change);
            writer.Write(emitter.AlphaTest);
            writer.Write(emitter.alphaRefValue);
            writer.Write(emitter.depthWrite);

            // behavior
            writer.Write(emitter.gravityVector.x);
            writer.Write(emitter.gravityVector.y);
            writer.Write(emitter.gravityVector.z);

            writer.Write(emitter.useGravity);
            writer.Write(emitter.EnableMainDirection);

            writer.Write(emitter.direction.x);
            writer.Write(emitter.direction.y);
            writer.Write(emitter.direction.z);

            writer.Write(emitter.randomDirectionFactor.x);
            writer.Write(emitter.randomDirectionFactor.y);
            writer.Write(emitter.randomDirectionFactor.z);

            writer.Write(emitter.lifeTime);
            writer.Write(emitter.randomLifeTime);
            writer.Write(emitter.power);
            writer.Write(emitter.powerRandom);
            writer.Write(emitter.Speed);
            writer.Write(emitter.Loop);

            writer.Write (emitter.EnableSpawnInterpolation);
            writer.Write (emitter.BillboardRotationEnable);
            writer.Write(emitter.AngularSpeed);
            writer.Write(emitter.EnableMinimeshRotationScale);


            writer.Write((int)emitter.EmitterKeyUsage);
            writer.Write((int)emitter.ParticleKeyUseage);

        }

        private void WriteEmitterKeyframes(TV_PARTICLEEMITTER_KEYFRAME keyframe, System.IO.BinaryWriter writer)
        {
            writer.Write (keyframe.fKey);

            writer.Write(keyframe.vMainDirection.x);
            writer.Write(keyframe.vMainDirection.y);
            writer.Write(keyframe.vMainDirection.z);

            writer.Write(keyframe.vLocalPosition.x);
            writer.Write(keyframe.vLocalPosition.y);
            writer.Write(keyframe.vLocalPosition.z);

            writer.Write(keyframe.vGeneratorBoxSize.x);
            writer.Write(keyframe.vGeneratorBoxSize.y);
            writer.Write(keyframe.vGeneratorBoxSize.z);


            writer.Write(keyframe.fGeneratorSphereRadius);

            writer.Write(keyframe.vDefaultColor.r);
            writer.Write(keyframe.vDefaultColor.g);
            writer.Write(keyframe.vDefaultColor.b);
            writer.Write(keyframe.vDefaultColor.a);

            writer.Write(keyframe.fParticleLifeTime);
            writer.Write(keyframe.fPower);
            writer.Write(keyframe.fSpeed);
        }

        private void WriteParticleKeframes(TV_PARTICLE_KEYFRAME keyframes, System.IO.BinaryWriter writer)
        {
            writer.Write (keyframes.fKey);

            writer.Write(keyframes.cColor.r);
            writer.Write(keyframes.cColor.g);
            writer.Write(keyframes.cColor.b);
            writer.Write(keyframes.cColor.a);

            writer.Write(keyframes.fSize.x);
            writer.Write(keyframes.fSize.y);
            writer.Write(keyframes.fSize.z);

            writer.Write(keyframes.vRotation.x);
            writer.Write(keyframes.vRotation.y);
            writer.Write(keyframes.vRotation.z);
        }

        private void WriteAttractor(AttractorProperties attractor, System.IO.BinaryWriter writer)
        {
            writer.Write(attractor.DirectionField);
            writer.Write(attractor.Name);
            writer.Write(attractor.Enable);

            writer.Write(attractor.FieldDirection.x);
            writer.Write(attractor.FieldDirection.y);
            writer.Write(attractor.FieldDirection.z);

            writer.Write(attractor.Position.x);
            writer.Write(attractor.Position.y);
            writer.Write(attractor.Position.z);

            writer.Write(attractor.Radius);

            writer.Write(attractor.Attenuation.x);
            writer.Write(attractor.Attenuation.y);
            writer.Write(attractor.Attenuation.z);

            writer.Write(attractor.AttractionRepulsionConstant);
            writer.Write((int)attractor.VelocityDependancy);
        }
        #endregion

        //public string ResourcePath
        //{
        //    get { return _id; }
        //    set { _id = value; }
        //}

        public bool LoadFromScript(string sDataSource)
        {

            return mSystem.Load(sDataSource);
        }

        public bool Load(string sBinaryDataSource)
        {
            return mSystem.Load(sBinaryDataSource);
        }

        public bool Save(string sBinaryDataPlace)
        {
            return mSystem.Save(sBinaryDataPlace);
        }

        public bool Save(string sBinaryDataPlace, bool bSaveTextures)
        {
            return mSystem.Save(sBinaryDataPlace, bSaveTextures);
        }
        #endregion

        #region ResourceBase members
        public void SetProperty(int groupIndex, string propertyName, object newValue, int geomtryParams = 0)
        {
            if (geomtryParams == 0) // emitter
            {
                int index = GetEmitterIndex(groupIndex);
                if (index == -1) return;

                mEmitters[index].SetProperty(propertyName, newValue);
                mEmitters[index].Apply(this);
            }
            else if (geomtryParams == 1) //attractor
            {
                int i = GetAttractorIndex(groupIndex);
                if (i == -1) return;

                mAttractors[i].SetProperty(propertyName, newValue);
                mAttractors[i].Apply(this);
            }
        }

        public Settings.PropertySpec GetProperty(int groupIndex, string propertyname, int geometryParams)
        {
            if (geometryParams == 0)
            {
                // emitters
                int index = GetEmitterIndex(groupIndex);
                if (index == -1) return null;

                Settings.PropertySpec[] properties = mEmitters[index].GetProperties();
                for (int i = 0; i < properties.Length; i++)
                    if (properties[i].Name == propertyname)
                    {
                        return properties[i];
                    }

                return null;

            }
            else if (geometryParams == 1)
            {
                // attractors
                int index = GetAttractorIndex(groupIndex);
                if (index == -1) return null;

                Settings.PropertySpec[] properties = mAttractors[index].GetProperties();
                for (int i = 0; i < properties.Length; i++)
                    if (properties[i].Name == propertyname)
                    {
                        return properties[i];
                    }

                return null;

            }
            else throw new ArgumentOutOfRangeException();

            
            return null;
        }

        public Settings.PropertySpec[] GetProperties(int groupIndex, int geometryParams, bool specOnly)
        {
            if (geometryParams == 0)
            {
                int index = GetEmitterIndex(groupIndex);
                if (index == -1 ) return null;

                return mEmitters[index].GetProperties();
            }
            else if (geometryParams == 1)
            {
                int i = GetAttractorIndex(groupIndex);
                if (i == -1 ) return null;

                return mAttractors[i].GetProperties();
            }

            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="specOnly">True returns the properties without any values assigned</param>
        /// <returns></returns>
        public override Settings.PropertySpec[] GetProperties(bool specOnly)
        {

            Settings.PropertySpec[] tmp = base.GetProperties(specOnly);
            Settings.PropertySpec[] properties = new Settings.PropertySpec[4 + tmp.Length];
            tmp.CopyTo(properties, 4);

            string category = "particle system  properties";
            properties[0] = new Settings.PropertySpec("emittercount", typeof(int).Name, category, GetEmittersCount());
            properties[1] = new Settings.PropertySpec("attractorcount", typeof(int).Name, category, GetAttractorsCount());
            properties[2] = new Settings.PropertySpec("emitterindices", typeof(int[]).Name, category, GetEmitterIndices());
            properties[3] = new Settings.PropertySpec("attractorindices", typeof(int[]).Name, category, GetAttractorIndices());

            // todo: since we're going to treat ParticleSystem's as a special case, we should add a GetEmitterProperties(int index) and skip this call to GetProperties() altgoether
            // NOTE: we don't serialize emitter properties.  Instead these are loaded from a custom binary file just as if we loaded a TVMesh with multiple Groups
 //           properties[4] = new Settings.PropertySpec("emitters", typeof (Settings.PropertySpec[,]), category, null);

            if (!specOnly)
            {
                properties[0].DefaultValue = GetEmittersCount();
                properties[1].DefaultValue = GetAttractorsCount();
                properties[2].DefaultValue = GetEmitterIndices();
                properties[3].DefaultValue = GetAttractorIndices();
 //               properties[4].DefaultValue = GetEmitterProperties();
            }

            return properties;
        }

        public override void SetProperties(Settings.PropertySpec[] properties)
        {
            if (properties == null) return;
            base.SetProperties(properties);
                        
            for (int i = 0; i < properties.Length; i++)
            {
                switch (properties[i].Name)
                {
                    case "emitters":
                        Settings.PropertySpec[,] emitterProperties = (Settings.PropertySpec[,])properties[i].DefaultValue;
                        break;
            //                    case "emittercount":
            //                        bleh = ()properties[i].DefaultValue;
            //                        break;
            //                    //case "attractorcount":
            //                    //    mPointSprite = (bool)properties[i].DefaultValue;
            //                    //    break;
                    default:
                        break;
                }
            }
        }

        #endregion

        //const int EMITTER_PROPERTY_COUNT = 31;
        //private Settings.PropertySpec[,] GetEmitterProperties ()
        //{
        //    if (mEmitters == null) return null;

        //    Settings.PropertySpec[,] result = new Settings.PropertySpec[mEmitters.Length, EMITTER_PROPERTY_COUNT];

        //    for (int i = 0; i < mEmitters.Length; i++)
        //    {
        //        Settings.PropertySpec[] properties = mEmitters[i].GetProperties();
        //        for (int j = 0; j < properties.Length; j++)
        //            result[i, j] = properties[j]; // TODO: problem here is the emitter and particle keyframe arrays.  Well its not too different than an array of any type of struct{}  The only thing is, it must be a struct and not simply ore PropertySpecs.  This means, we should place the structs in MeyCommon or Keymath.Types.  Then add a TVTypeConvertor for them
        //    }

        //    return result;
        //}

        #region Group Members
        public override void AddParent(IGroup parent)
        {
            base.AddParent(parent);

            // TODO: every time a duplicate is added, we have to initialize it's shared settings
            // such as cullmode, shadermode, lightingmode
            if (mDuplicates == null)
            {
                // NOTE: first instance, uses the original TVParticleSystem.  
                // NOTE: Its imperative the mSystem is loaded at this point. I'm not sure why it's not loading on call from ImportLib.Load() in VisualFXAPI during "RegisterParticleSystem()"
                mDuplicates = new Dictionary<string, ParticleSystemDuplicate>();
                if (!TVResourceIsLoaded)
                    LoadTVResource();

                mDuplicates.Add(((Node)parent).ID, new ParticleSystemDuplicate(mSystem));
            }
            else
            {
                mDuplicates.Add(((Node)parent).ID, new ParticleSystemDuplicate());
                // NOTE: mSystem MUST be loaded already.  
                System.Diagnostics.Debug.Assert(TVResourceIsLoaded && mSystem != null);
                mDuplicates[((Node)parent).ID].mDuplicateSystem = mSystem.Duplicate();
            }
        }

        public override void RemoveParent(IGroup parent)
        {
            base.RemoveParent(parent);
            mDuplicates.Remove(((Node)parent).ID);
        }
        #endregion

        #region Geometry Members
        internal void AddEmitter(EmitterProperties emitter)
        {
            if (mSystem == null) throw new NullReferenceException();

            int index = mSystem.CreateEmitter(emitter.type, emitter.maxParticles);
            emitter.Index = index;

            if (emitter.type == CONST_TV_EMITTERTYPE.TV_EMITTER_MINIMESH)
            {
                // TODO: on ParticleSystem.Destroy we need to destroy TVMinimesh and TVMesh
                System.Diagnostics.Debug.Assert(emitter.Minimesh != null);
                mSystem.SetMiniMesh(index, emitter.Minimesh);
                // mSystem.SetMiniMeshRotationScale(index, bEnableRotationScale); // TODO:
            }

            mEmitters = Keystone.Extensions.ArrayExtensions.ArrayAppend(mEmitters, emitter);
            emitter.Apply(this);
        }

        public void AddEmitter(string name, CONST_TV_EMITTERTYPE type, int maxParticles, TVMiniMesh tvmini = null, TVMesh tvmesh = null, string meshPath = null)
        {
            if (mSystem == null) LoadTVResource();

            // index should be treated as transient.  If we destroy an emitter and create a new one, it may not necessarily use the lowest index available
            int index = mSystem.CreateEmitter(type, maxParticles);
            EmitterProperties emitter = new EmitterProperties(index);
            emitter.maxParticles = maxParticles;
            emitter.Name = name;
            emitter.type = type;
            emitter.mMesh = tvmesh;
            emitter.Minimesh = tvmini;

            if (type == CONST_TV_EMITTERTYPE.TV_EMITTER_MINIMESH)
            {
                // TODO: on ParticleSystem.Destroy we need to destroy TVMinimesh and TVMesh
                System.Diagnostics.Debug.Assert(tvmini != null);
                mSystem.SetMiniMesh(index, tvmini);
               // mSystem.SetMiniMeshRotationScale(index, bEnableRotationScale); // TODO:
            }


            // set defaults
            emitter.MeshPath = meshPath;
            mEmitters = Keystone.Extensions.ArrayExtensions.ArrayAppend(mEmitters, emitter);

            emitter.Apply(this);

           // UpdateDuplicates();
        }


        //private void UpdateDuplicates()
        //{
        //    if (mDuplicates == null || mDuplicates.Count == 0) return;

        //    foreach (ParticleSystemDuplicate duplicate in mDuplicates.Values)
        //    {
        //        duplicate.mDuplicateSystem.Destroy();
        //        duplicate.mDuplicateSystem = null;
        //        duplicate.mDuplicateSystem = this.mSystem.Duplicate("test");
        //    }
        //}

        public void RemoveEmitter(int groupIndex)
        {
            int i = GetEmitterIndex(groupIndex);

            EmitterProperties emitter = mEmitters[i];
            mEmitters = Extensions.ArrayExtensions.ArrayRemoveAt(mEmitters, i);
            System.Diagnostics.Debug.Assert(emitter.Index == groupIndex);
            mSystem.DestroyEmitter(groupIndex);
        }

        public void AddAttractor(string name)
        {
            if (mSystem == null) LoadTVResource();

            // index should be treated as transient.  If we destroy an emitter and create a new one, it may not necessarily use the lowest index available
            int index = mSystem.CreateAttractor();

            AttractorProperties attractor = new AttractorProperties(index);
            attractor.Name = name;

            // set defaults
            mAttractors = Keystone.Extensions.ArrayExtensions.ArrayAppend(mAttractors, attractor);
            attractor.Apply(this);
        }

        public void RemoveAttractor(int groupIndex)
        {
            int i = GetAttractorIndex(groupIndex);

            AttractorProperties attractors = mAttractors[i];
            mAttractors = Extensions.ArrayExtensions.ArrayRemoveAt(mAttractors, i);
            System.Diagnostics.Debug.Assert(attractors.Index == groupIndex);
            mSystem.DestroyAttractor(groupIndex);
        }

        public override int GroupCount
        {
            get { return GetEmittersCount(); }
        }


            private int GetEmittersCount()
        {
            if (mEmitters == null) return 0;
            return mEmitters.Length;
        }

        private int GetAttractorsCount()
        {
            if (mAttractors == null) return 0;
            return mAttractors.Length;
        }

        public double Duration
        {
            get
            {
                if (EmitterCount == 0) return 0;
                double result = 0;
                for (int i = 0; i < EmitterCount; i++)
                    result = Math.Max(result, mEmitters[i].lifeTime);

                return result;
            }
        }

        private int[] GetEmitterIndices()
        {
            if (mEmitters == null || mEmitters.Length == 0) return null;

            int[] results = new int[mEmitters.Length];
            for (int i = 0; i < results.Length; i++)
                results[i] = mEmitters[i].Index;

            return results;
        }
        private int[] GetAttractorIndices()
        {
            if (mAttractors == null || mAttractors.Length == 0) return null;

            int[] results = new int[mAttractors.Length];
            for (int i = 0; i < results.Length; i++)
                results[i] = mAttractors[i].Index;

            return results;
        }

        public override int CullMode
        {
            get
            {
                return 0; // there is no front/backface/both cullmode for ParticleSystems
            }

            set
            {
                return; // There is no cullMode for billboards. Billboards always face the camera with no need for backface culling
            }
        }
        public int EmitterCount
        {
            get { return mSystem.GetEmitterCount();}
        }

        public int AttractorCount
        {
            get { return mSystem.GetAttractorCount(); }
        }

        public int GlobalParticleCount
        {
            get { return mSystem.GetGlobalParticleCount(); }
        }

        public Keystone.Appearance.DefaultAppearance CreateAppearance()
        {
            int attributeCount = EmitterCount;
            if (attributeCount > 0)
            {
                // TODO: what if we just had ps.GetAppearance() ?
                DefaultAppearance appearance = (DefaultAppearance)Repository.Create("DefaultAppearance");
                for (int i = 0; i < attributeCount; i++)
                {
                    GroupAttribute ga = (GroupAttribute)Repository.Create("GroupAttribute");

                    //diffuse + gloss/specular map split in two chunks, resulting in a 4096×2048 resolution - shader requires store in diffuse layer 
                    if (!string.IsNullOrEmpty(mEmitters[i].TexturePath))
                    {
                        Keystone.Appearance.Layer textureLayer = (Layer)Keystone.Resource.Repository.Create("Diffuse");
                        Keystone.Appearance.Texture tex = (Texture)Keystone.Resource.Repository.Create(mEmitters[i].TexturePath, "Texture");
                        tex.TextureType = Texture.TEXTURETYPE.Default;
                        textureLayer.AddChild(tex);
                        ga.AddChild(textureLayer);
                    }

                    if (!string.IsNullOrEmpty(mEmitters[i].ShaderPath))
                    {
                        string id = Repository.GetNewName(typeof(Shader));
                        ga.ResourcePath = mEmitters[i].ShaderPath;

                        // TODO: our Plugin needs to take this into account when we want to add a shader path to an Appearance.  
                        // obsolete - July.14.2013 - Shader nodes are no longer directly added as child nodes. 
                        // We only assign Apearance.Resource path and allow pager to load it
                        //Shader genericShader = Shader.Create(id, shaderPath);
                        //appearance.AddChild(genericShader);                
                    }

                    // todo: emitters are treated as internal to ps so i cant grab the stored texturePath, shaderPath and minimeshPath
                    //if (ps.GetProperty(i, ))
                    // TODO: add the texture, shader, material. I think Minimesh already gets loaded during ps.LoadTVResource()
                    // todo: when changing the texture, the GroupAttribute.Apply() is not correctly finding a change resulting in re-apply of appearance details
                    appearance.AddChild(ga);
                }
                return appearance;
            }
            else return null;
        }

        ////internal override void AttachTo(CONST_TV_NODETYPE type, int objIndex, int subIndex, bool keepMatrix, bool removeScale)
        //{
        //    throw new NotImplementedException();
        //}

    
        /// <summary>
        /// This does NOT delete your emitters, attractors and keyframes.  It simply resets the starting conditions of the system.
        /// </summary>
        public void ResetAll()
        {
            // todo: verify the above summary is true
            mSystem.ResetAll();
        }
        



        #endregion

        #region Element Members

        //// for particle systems this is the global matrix
        //public override TV_3DMATRIX Matrix
        //{
        //    get { return _particle.GetGlobalMatrix(); }
        //    set
        //    {
        //        _matrix = value;
        //        _particle.SetGlobalMatrix(value);
        //        BoundVolumeIsDirty = true;
        //    }
        //}

        //public override TV_3DMATRIX RotationMatrix
        //{
        //    set
        //    {
        //        _rotationMatrix = value;
        //        // _particle.SetGlobalRotationMatrix(_rotationMatrix);
        //        BoundVolumeIsDirty = true;
        //    }
        //}

        //public override Vector3d Position
        //{
        //    get { return _particle.GetGlobalPosition(); }
        //    set
        //    {
        //        _position = value;
        //        _particle.SetGlobalPosition(_position.x, _position.y, _position.z);
        //        BoundVolumeIsDirty = true;
        //    }
        //}

        //public override Vector3d Scale
        //{
        //    set
        //    {
        //        _scale = value;
        //        // _particle.SetGlobalScale(_scale.x, _scale.y, _scale.z);
        //        BoundVolumeIsDirty = true;
        //    }
        //}

        //public override Vector3d Rotation
        //{
        //    set
        //    {
        //        _rotation = value;
        //        _particle.SetGlobalRotation(_rotation.x, _rotation.y, _rotation.z);
        //        BoundVolumeIsDirty = true;
        //    }
        //}

        #endregion
            

        public override void UnloadTVResource()
        {
            throw new NotImplementedException();
        }

        internal override PickResults AdvancedCollide(Vector3d start, Vector3d end, PickParameters parameters)
        {
            return new PickResults();
            throw new NotImplementedException();
        }

        internal override void Render(Matrix worldMatrix, Scene.Scene scene, Model model, double elapsedSeconds)
        {
            lock (mSyncRoot)
            {
                
                ParticleSystemDuplicate duplicate = this[model.ID];

                // TODO: it should be impossible for _resourceStatus to be "Loaded" before it's actually loaded
                if (_resourceStatus != PageableNodeStatus.Loaded) return;
                
                if (model.Appearance != null)
                {
                    //_appearanceHashCode = model.Appearance.Apply(duplicate, model.AppearanceFlags, elapsedMilliseconds);
                    duplicate.LastAppearance = model.Appearance.Apply(duplicate, mEmitters, elapsedSeconds);
                }
                else
                    //_appearanceHashCode = NullAppearance.Apply(duplicate, model.AppearanceFlags, _appearanceHashCode);
                    mDuplicates[model.ID].LastAppearance = NullAppearance.Apply(duplicate, _appearanceHashCode);


                // NOTE: if the emitter index is invalid, SetEmitterMatrix() and SetEmitterPosition() will throw AccessViolations
                TV_3DMATRIX tvmatrix = Helpers.TVTypeConverter.ToTVMatrix(worldMatrix);
                // mSystem.SetEmitterMatrix(mEmitters[0], tvmatrix);
                TV_3DVECTOR translation = Helpers.TVTypeConverter.ToTVVector(worldMatrix.GetTranslation());
                //mSystem.SetEmitterPosition(mEmitters[0], translation); 


                duplicate.mDuplicateSystem.SetGlobalMatrix(tvmatrix);

               //duplicate.mDuplicateSyste.SetGlobalPosition(fX, fY, fZ);
                
                /// Used to set rotation of entire ParticleSystem.  This should be called rather than trying to change the direction of Emitters.
                /// Angles passed in should be in degrees.

                //duplicate.mDuplicateSyste.SetGlobalRotation(x, y, z);


                duplicate.mDuplicateSystem.Update();
                duplicate.mDuplicateSystem.Render();
            }
        }

        #region IBoundVolume Members
        // TODO: Do we allow ParticleSystem to be exception to the rule that an Entity only
        // worries about it's own bounding volume and NOT it's children.  Istead we rely on
        // SceneNode's to worry about hierarchical bounding volumes.
        // Region's are the only exception to this rule and maybe ParticleSystem should be
        // viewed as a type of Region
        protected override void UpdateBoundVolume()
        {
            if (_resourceStatus != PageableNodeStatus.Loaded) return;

            if (_box == null)
                _box = new BoundingBox();
            else
                _box.Reset();

            

            // todo: whenever emitter shape or size changes, we need to set BoundingBoxDirty  
            if (mEmitters == null || mEmitters.Length == 0)
            {
                _box.Reset();
            }
            else
            { 
                TV_3DVECTOR min, max;
                min.x = min.y = min.z = 0f;
                max.x = max.y = max.z = 0f;

                BoundingBox emitterBox;
                for (int i = 0; i < mEmitters.Length; i++)
                {
                    
                    //if (mEmitters[i].shape == CONST_TV_EMITTERSHAPE.TV_EMITTERSHAPE_BOXVOLUME)
                    //{
                    //}
                    //else if (mEmitters[i].shape == CONST_TV_EMITTERSHAPE.TV_EMITTERSHAPE_SPHEREVOLUME)
                    //{
                    //}
                    //else if (mEmitters[i].shape == CONST_TV_EMITTERSHAPE.TV_EMITTERSHAPE_POINT)
                    //{
                    //    // extrude a box that is 1x1 along emitter.Power and lifetime
                    //    emitterBox = new BoundingBox(-0.5, 0, -0.5, mEmitters[i].direction.x * mEmitters[i].power, mEmitters[i].direction.y * mEmitters[i].power, mEmitters[i].direction.z * mEmitters[i].power);
                    //}
                    //else if (mEmitters[i].shape == CONST_TV_EMITTERSHAPE.TV_EMITTERSHAPE_BOXSURFACE)
                    //{
                    //}
                    //else if (mEmitters[i].shape == CONST_TV_EMITTERSHAPE.TV_EMITTERSHAPE_SPHERESURFACE)
                    //{
                    //}
                    emitterBox = new BoundingBox(-mEmitters[i].systemBoxSize.x / 2f, -mEmitters[i].systemBoxSize.y / 2f, -mEmitters[i].systemBoxSize.z / 2f, mEmitters[i].systemBoxSize.x / 2f, mEmitters[i].systemBoxSize.y / 2f, mEmitters[i].systemBoxSize.z / 2f);
                    _box = BoundingBox.Combine(_box, emitterBox);
                }
            }
            DisableChangeFlags(Keystone.Enums.ChangeStates.BoundingBox_TranslatedOnly | Keystone.Enums.ChangeStates.BoundingBoxDirty);
        }


        #endregion

        #region IDisposable Members

        protected override void DisposeUnmanagedResources()
        {
           // mSystem.Destroy(); // TODO: verify the below method is executing which will destory the particle system. also, do we have to destroy duplicates there?
            base.DisposeUnmanagedResources();
        }

        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            try
            {
                // TODO: I must sychronize dispose of resource after they are removed from repository
                //       in the same way that I do with sychronizing their Create() in factory.
                if (mSystem != null && mEmitters != null)
                    for (int i = 0; i < mEmitters.Length; i++)
                        // TODO: we need to destroy any TVMesh or TVMinimesh connected to the emitter as well as any Shader or Texture to avoid leaks i think
                        mSystem.DestroyEmitter(mEmitters[i].Index);

                mEmitters = null;

                if (mSystem != null && mAttractors != null)
                    for (int i = 0; i < mAttractors.Length; i++)
                        mSystem.DestroyAttractor(mAttractors[i].Index);

                mAttractors = null;
                

                if (mSystem != null)
                    mSystem.Destroy();

                mSystem = null;
                System.Diagnostics.Debug.Assert(RefCount == 0, "ParticleSystem.DisposeManagedResources() - RefCount not at 0.");
                //System.Diagnostics.Debug.WriteLine(string.Format("Mesh3d.DisposeManagedResources() - Disposed '{0}'.", _id));
                //if (_minimesh != null)
                //    _minimesh = null; // dont dispose mini, fxinstancerenderer will do that? // TODO: i think that's changd? we have other ways of using minimesh now right? such as MinimeshGeometry
            }
            catch
            {
            }
        }
        #endregion
    }
}