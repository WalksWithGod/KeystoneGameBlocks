//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Xml;
//using Keystone.Appearance;
//using Keystone.Collision;
//using Keystone.Entities;
//using Keystone.Enum;
//using Keystone.IO;
//using Keystone.Resource;
//using Keystone.Shaders;
//using Keystone.Traversers;
//using Keystone.Types;
//using MTV3D65;


//namespace Keystone.Elements
//{
//    /// <summary>
//    /// This new actor3d class does not attempt to manage duplicates under this single
//    /// class.  Instead, any attempt to create an Actor3d using an existing Actor3d's
//    /// filepath, we'll result in a new Actor3d class being constructed.
//    /// NOTE: Can you call .Duplicate() from another Duplicate?
//    /// Attempts to manage Actor3d has just been too complicated.  This is going to 
//    /// work better for us.
//    /// 
//    /// Vertices, UVs, Normals, Animation Data, Bones are the constant shared
//    /// data for Duplicates.  
//    /// Textures, Materials, Shaders, can all be different.
//    /// </summary>
//    public class Actor3d : Geometry 
//    {
//        internal TVActor _actor;
//        private CONST_TV_ACTORMODE _actorMode;        
//        private CONST_TV_LIGHTINGMODE _lightingMode;


//        /// <summary>
//        /// Initialize an Actor3d with just a path.  The details will be
//        /// read in during deserialization (eg. ReadXML)
//        /// </summary>
//        /// <param name="resourcePath"></param>
//        internal Actor3d(string resourcePath)
//            : base(resourcePath)
//        {
//        }

//        private Actor3d(string id, TVActor actor)
//            : this(id)
//        {
//            if (actor == null) throw new ArgumentNullException("Actor.ctor()");
//            _tvfactoryIndex = actor.GetIndex();
//        }


//        public static Actor3d Create( string relativePath, CONST_TV_ACTORMODE actorMode, bool loadTextures,
//                                     bool loadMaterials, out DefaultAppearance appearance)
//        {

//            Actor3d actor = (Actor3d)Repository.Get(relativePath);

//            appearance = null;
//            if (actor == null)
//            {
//                // Load() immediately - On importing an actor to Assets, we do have to load it first and cannot
//                // add to tree and wait for LoadTVResource()
//                TVActor a = Load(relativePath, actorMode, loadTextures,
//                    loadMaterials, out appearance);
//                actor = new Actor3d(relativePath, a); 
//                actor._actor = a;
//            }
//            else
//            {
//                // TODO: how do you duplicate with a resourcepath that wont conflict?
//                // even if we denote like mIsDuplicate()  what does that buy us?
//                // Well, a duplicate has some limitations I think... and I think what should
//                // happen is that Duplicate's should reference the original and perhaps
//                // so should the original have an array of the duplicates!  This could help us
//                // prevent the original from every falling out of scope in repository if
//                // necessary. We need to keep it in the repository so that future query
//                // for the resourcePath will still use it and not struggle with the Duplicate
//                // which has a different _id now.
//                // TODO: If, every time this Actor were SetParent() we created a duplicate...
//                // and managed the duplicates here....
//                //
//                // the only problematic part is 
//                //
//                Trace.Assert(actor.TVResourceIsLoaded, "Actor3d.Create() - TVResource is not loaded.  Cannot duplicate.");
//                TVActor a = actor._actor.Duplicate(); 
//                if (a == null) throw new Exception("Actor3dNew.Create() - Could not create duplicate.");
//                actor = new Actor3d(relativePath, a);
//                actor._actor = a;
//            }
//            // we can skip loading the actor and go straight to loading the appearance if the actor already exists in the cache.
//            // NOTE: We cannot know the appearance though of a previous one... but the way to avoid creating a virtual dupe 
//            // is to just .Clone the model and not try to re-load the appearance this way

//            return actor;
//        }

//        public static Actor3d Create(string id)
//        {
//            Actor3d actor;
//            actor = (Actor3d)Repository.Get(id);
//            if (actor != null) return actor;
//            actor = new Actor3d(id);
//            return actor;
//        }

//        #region ITraversable Members
//        public override object Traverse(ITraverser target, object data)
//        {
//            return target.Apply(this, data);
//        }
//        #endregion

//        private static TVActor Load( string relativePath, CONST_TV_ACTORMODE actorMode,
//                                           bool loadTextures, bool loadMaterials, out DefaultAppearance appearance)
//        {
//            appearance = null;
//            TVActor a = CoreClient._CoreClient.Scene.CreateActor(relativePath);

//            // ActorMode set BEFORE the actor is loaded from file because it impacts 
//            //how the actor data is configured during load.
//            a.SetActorMode(actorMode);

//            KeyCommon.IO.ResourceDescriptor descriptor = new KeyCommon.IO.ResourceDescriptor(relativePath);

//            bool result = false;

//#if DEBUG
//            Stopwatch watch = new Stopwatch();
//            watch.Reset();
//            watch.Start();
//#endif


//            if (descriptor.IsArchivedResource)
//            {
//                if (descriptor.Extension == ImportLib.XFILE_EXTENSION || descriptor.Extension == ImportLib.TVACTOR_EXTENSION)
//                {
//                    System.Runtime.InteropServices.GCHandle gchandle;
//                    string memoryFile = Keystone.ImportLib.GetTVDataSource(descriptor.ArchiveEntryName, "", Keystone.Core.FullArchivePath(descriptor.RelativePathToArchive ), out gchandle);
//                    if (string.IsNullOrEmpty(memoryFile)) throw new Exception("Error importing actor file from archive.");

//                    if (descriptor.Extension == ImportLib.XFILE_EXTENSION)
//                        result = a.LoadXFile(memoryFile, loadTextures, loadMaterials);
//                    else if (descriptor.Extension == ImportLib.TVACTOR_EXTENSION)
//                    {
//                        result = a.LoadTVA(memoryFile, loadTextures, loadMaterials);
//                    }
//                    gchandle.Free();
//                }
//            }
//            else
//            {
//                if (descriptor.Extension == ImportLib.XFILE_EXTENSION)
//                    result = a.LoadXFile(relativePath, loadTextures, loadMaterials);
//                else if (descriptor.Extension == ImportLib.TVACTOR_EXTENSION)
//                    result = a.LoadTVA(relativePath, loadTextures, loadMaterials);
//            }

//            if ((a != null) && (loadMaterials) || (loadTextures))
//                ImportLib.GetActorTexturesAndMaterials(a, out appearance);


//#if DEBUG
//            watch.Stop();
//#endif
//            if (!result)
//                throw new Exception(string.Format("Failed to load actor '{0}'.  Check path.", relativePath));

//#if DEBUG
//            Trace.WriteLine(string.Format ("Actor '{0}' loaded with {1} groups and {2} bones in {3} seconds with {4} vertices in {5} triangles.",relativePath, a.GetGroupCount(), a.GetBoneCount (), watch.Elapsed, a.GetVertexCount(), a.GetTriangleCount()));
//            for (int i = 0; i < a.GetBoneCount(); i++)
//                Trace.WriteLine("Bone " + i.ToString() + " " + a.GetBoneName(i));

//#endif

//            return a;
//        }

//        #region ResourceBase members
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="specOnly">True returns the properties without any values assigned</param>
//        /// <returns></returns>
//        public override Settings.PropertySpec[] GetProperties(bool specOnly)
//        {
//            Settings.PropertySpec[] tmp = base.GetProperties(specOnly);
//            Settings.PropertySpec[] properties = new Settings.PropertySpec[1 + tmp.Length];
//            tmp.CopyTo(properties, 1);

//            properties[0] = new Settings.PropertySpec("actormode", typeof(int).Name);

//            if (!specOnly)
//            {
//                properties[0].DefaultValue = (int)_actorMode;
//            }
//            //    Hull = new ConvexHull(_filePath);
//            //    Hull = ConvexHull.ReadXml(xmlNode);
//            return properties;
//        }

//        public override void SetProperties(Settings.PropertySpec[] properties)
//        {
//            if (properties == null) return;
//            base.SetProperties(properties);

//            for (int i = 0; i < properties.Length; i++)
//            {
//                // use of a switch allows us to pass in all or a few of the propspecs depending
//                // on whether we're loading from xml or changing a single property via server directive
//                switch (properties[i].Name)
//                {
//                    case "actormode":
//                        _actorMode = (CONST_TV_ACTORMODE)((int)properties[i].DefaultValue);
//                        break;
//                }
//            }
//        }
//        #endregion

//        #region IPageableNode Members
//        public override void LoadTVResource()
//        {
//            // TODO: if the mesh's src file has changed, we should unload the previous mesh first
//            if (_actor != null)
//            {
//                try
//                {
//                    _actor.Destroy();
//                }
//                catch
//                {
//                    Trace.WriteLine("Actor3d.LoadTVResource() - Error on Actor.Destroy() - actor path == " + _id);
//                }
//            }
            
//            try
//            {
//            DefaultAppearance dummy;
//            _actor = Load(_id, _actorMode, false, false, out dummy);
//            _actor.OptimizeAnimations(); // TODO: should this ever NOT be called?
//            //_actor.EnableFrustumCulling(false);
//            _actor.SetCullMode(_cullMode);

//            _tvfactoryIndex = _actor.GetIndex();
//            SetChangeFlags(Keystone.Enums.ChangeStates.GeometryAdded |
//                Keystone.Enums.ChangeStates.AppearanceChanged |
//                Keystone.Enums.ChangeStates.BoundingBoxDirty |
//                Keystone.Enums.ChangeStates.MatrixDirty, Keystone.Enums.ChangeSource.Self);
//            }
//            catch (Exception ex)
//            {
//                Trace.WriteLine("error on LoadTVResource() - actor path == " + _id + ex.Message);
//                throw ex;
//            }
//        }
//        public override void SaveTVResource(string resourcePath)
//        {
//            if (TVResourceIsLoaded)
//                _actor.SaveTVA(resourcePath); // this is why name i think should not be equal to filepath right?
//        }

//        #endregion


//        public void ComputeNormals()
//        {
//            _actor.ComputeNormals();
//        }

//        /// <summary>
//        /// ActorMode should never be changed once the underlying actor geometry has been loaded.
//        /// </summary>
//        internal CONST_TV_ACTORMODE ActorMode
//        {
//            // NOTE: No Setter because we can only set the ActorMode before loading the geometry
//            // and that means either during static Create() or upon deserialization.
//            get { return _actorMode; }
//        }

//        /// <summary>
//        /// note: SetLightingMode occurs after geometry is loaded because the lightingmode
//        /// is stored in .tvm, .tva files and so anything you set prior to loading, will be
//        /// replaced with the lightingmode stored in the file.
//        /// note; this access modifer must be "internal" and is only setable by DefaultAppearance.Apply()
//        /// </summary>
//        internal CONST_TV_LIGHTINGMODE LightingMode
//        {
//            get { return CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED; }
//            set
//            {
//                _lightingMode = value;
//                _actor.SetLightingMode(value);
//            }
//        }

//        /// <summary>
//        /// Shader is applied via Appearance only.
//        /// </summary>
//        internal override Shader Shader
//        {
//            set
//            {
//                if (_shader == value && value.TVShader != null) return;
//                if (value.TVShader == null) return;

//                if (!value.TVResourceIsLoaded)
//                {
//                    System.Diagnostics.Debug.WriteLine("Shader '" + _shader.ID + "' not loaded.  Cannot assign to Actor '" + _id + "'");
//                    return;
//                }
//                _shader = value;
//                if (_shader == null)
//                    _actor.SetShader(null);
//                else
//                    _actor.SetShader(_shader.TVShader);
//            }
//        }

//        // TODO: hrm, we moved Overlay out to Entities and basically overlay
//        // forces a mesh/actor/particle system to ignore the depth buffer.  In effect
//        // it just disables the depth buffer during render.
//        // internal used by Appearance
//        internal override bool Overlay
//        {
//            set
//            {
//                _actor.SetOverlay(value);
//            }
//        }

//        // TODO: should change to SetAlphaTest and also seperate vars for getting the alpha test bool and refvalue?
//        internal override bool AlphaTest
//        {
//            set
//            {
//                _alphaTestEnable = value;
//                _actor.SetAlphaTest(_alphaTestEnable, _alphaTestRefValue, _alphaTestDepthBufferWriteEnable);
//            }
//        }


//        internal int AlphaTestRefValue
//        {
//            get { return _alphaTestRefValue; }
//            set
//            {
//                _alphaTestRefValue = value;
//            }
//        }

//        internal bool AlphaTestDepthWriteEnable
//        {
//            get { return _alphaTestDepthBufferWriteEnable; }
//            set
//            {
//                _alphaTestDepthBufferWriteEnable = value;
//            }
//        }

//        /// <summary>
//        /// For opacity, BlendingMode must be set to .TV_BLEND_ALPHA
//        /// </summary>
//        internal override CONST_TV_BLENDINGMODE BlendingMode
//        {
//            set
//            {
//                _blendingMode = value;
//                _actor.SetBlendingMode(value);
//            }
//        }

//        // materials and textures can be different for different duplicates
//        internal int GetMaterial(int groupID)
//        {
//            return _actor.GetMaterial(groupID);
//        }

//        internal void SetMaterial(int tvMaterialID, int groupID)
//        {
//            if (groupID == -1)
//                _actor.SetMaterial(tvMaterialID);
//            else
//                _actor.SetMaterial(tvMaterialID, groupID);
//        }

//        internal int GetTextureEx(CONST_TV_LAYER layer, int groupID)
//        {
//            return _actor.GetTextureEx((int) layer, groupID);
//        }

//        internal void SetTexture(int tvTextureID, int groupID)
//        {
//            if (groupID == -1)
//                _actor.SetTexture(tvTextureID);
//            else
//                _actor.SetTexture(tvTextureID, groupID);
//        }

//        internal void SetTextureEx(int layer, int tvTextureID, int groupID)
//        {
//            if (groupID == -1)
//                _actor.SetTextureEx(layer, tvTextureID);
//            else
//                _actor.SetTextureEx(layer, tvTextureID, groupID);
//        }



//        #region Bones
//        //public void SetBoneEnable(int iBoneId, bool bEnable)
//        //{
//        //    mActor.SetBoneEnable(iBoneId, bEnable);
//        //}

//        //public bool IsBoneEnabled(int iBoneId)
//        //{
//        //    return mActor.IsBoneEnabled(iBoneId);
//        //}
//        //public void SetBoneRotation(int iBone, float aRotateX, float aRotateY, float aRotateZ, bool bRelative)
//        //{
//        //    mActor.SetBoneRotation(iBone, aRotateX, aRotateY, aRotateZ, bRelative);
//        //}

//        //public void SetBoneRotationMatrix(int iBone, TV_3DMATRIX mRotateMatrix, bool bRelative)
//        //{
//        //    mActor.SetBoneRotationMatrix(iBone, mRotateMatrix, bRelative);
//        //}

//        //public TV_3DMATRIX GetBoneMatrix(int iBone)
//        //{
//        //    return mActor.GetBoneMatrix(iBone);
//        //}

//        //public TV_3DMATRIX GetBoneMatrix(int iBone, bool bModelSpace)
//        //{
//        //    return mActor.GetBoneMatrix(iBone, bModelSpace);
//        //}
//        //public void SetBoneMatrix(int iBone, TV_3DMATRIX mMatrix)
//        //{
//        //    mActor.SetBoneMatrix(iBone, mMatrix);
//        //}

//        //public void SetBoneMatrix(int iBone, TV_3DMATRIX mMatrix, bool bRelative)
//        //{
//        //    mActor.SetBoneMatrix(iBone, mMatrix, bRelative);
//        //}

//        //public void SetBoneMatrix(int iBone, TV_3DMATRIX mMatrix, bool bRelative, bool bModelSpace)
//        //{
//        //    mActor.SetBoneMatrix(iBone, mMatrix, bRelative, bModelSpace);
//        //}

//        //public TV_3DVECTOR GetBonePosition(int iBone)
//        //{
//        //    return mActor.GetBonePosition(iBone);
//        //}

//        //public TV_3DVECTOR GetBonePosition(int iBone, bool bLocal)
//        //{
//        //    return mActor.GetBonePosition(iBone, bLocal);
//        //}
//        //public void SetBoneTranslation(int iBone, float fTranslationX, float fTranslationY, float fTranslationZ)
//        //{
//        //    mActor.SetBoneTranslation(iBone, fTranslationX, fTranslationY, fTranslationZ);
//        //}

//        //public void SetBoneTranslation(int iBone, float fTranslationX, float fTranslationY, float fTranslationZ, bool bRelative)
//        //{
//        //    mActor.SetBoneTranslation(iBone, fTranslationX, fTranslationY, fTranslationZ, bRelative);
//        //}

//        //public void SetBoneScale(int iBone, float fScaleX, float fScaleY, float fScaleZ)
//        //{
//        //    mActor.SetBoneScale(iBone, fScaleX, fScaleY, fScaleZ);
//        //}

//        //public void SetBoneGlobalMatrix(int iBone, TV_3DMATRIX mMatrix)
//        //{
//        //    mActor.SetBoneGlobalMatrix(iBone, mMatrix);
//        //}

//        //public int GetBoneParent(int iBone)
//        //{
//        //    return mActor.GetBoneParent(iBone);
//        //}
//        public int GetBoneCount()
//        {
//            return _actor.GetBoneCount();
//        }

//        public string GetBoneName(int index)
//        {
//            return _actor.GetBoneName(index);
//        }

//        // TODO: i think the initial bone array should be done right off the bat.
//        // well... maybe not, how often do we really need to call GetBones? Although
//        // since we do internally use duplicates we could share this bone info but
//        // the matrices would be different for each duplicate instance
//        public Bone[] GetBones()
//        {
//            Bone[] results = new Bone[GetBoneCount()];
//            if (results == null) return null;

//            for (int i = 0; i < results.Length; i++)
//            {
//                results[i].ID = i;
//                results[i].Name = GetBoneName(i);
//                results[i].ParentID = _actor.GetBoneParent(i);
//                //results[i].Enable = _actor.SetBoneEnable (i, true); // ugh
//                results[i].Position = Helpers.TVTypeConverter.FromTVVector (_actor.GetBonePosition(i));
//                //results[i].Matrix = Helpers.TVTypeConverter.FromTVMatrix (_actor.GetBoneMatrix(i, true));
//                //results[i].Rotation = 

//            }
//            return results;
//        }

//        public struct Bone
//        {
//            public int ID;
//            public int ParentID;
//            public string Name;
//            public Vector3d Position;
//            public Matrix Matrix;
//        }

//        public Matrix GetBoneMatrix(int boneID, bool modelSpace)
//        {
//            // i think we should always return in model space 
//            // TODO: delete this bool
//            return Helpers.TVTypeConverter.FromTVMatrix(_actor.GetBoneMatrix(boneID, modelSpace));
//            // return _actor.GetBoneMatrix (boneID, modelSpace);
//            throw new NotImplementedException();
//        }

//        public void SetBoneMatrix(int boneID, TV_3DMATRIX matrix, bool modelSpace)
//        {
//            //_actor.SetBoneMatrix(boneID, matrix, false, modelSpace);
//            //_actor.SetBoneGlobalMatrix(boneID, matrix);

//            //_actor.SetBoneMatrix(boneID, Matrix, relative, modelSpace);
//            //_actor.SetBoneRotationMatrix(boneID, Matrix, relative);

//        }
//        #endregion

//        /// <summary>
//        /// Returns an array of the animations that are intrinsic (built in) to the
//        /// actor.
//        /// </summary>
//        /// <returns></returns>
//        public Animation.BonedAnimation[] GetAnimations()
//        {
//            // NOTE: This call should occur on Import of an Actor
//            // to the Assets and when creating the KGBEntity
//            // Also, later when adding animation ranges, user should save.
//            // Also  if ImportAnimations and adding more, new Animation[] should be created
//            // for adding as children to the Entity
//            //
//            // But also it should be callable after the fact.... how do we name these?
//            // 
//            // TODO: or should i just pass in Actor3d here
//            // also, when adding new  animation ranges should I
//            // 
//            if (!TVResourceIsLoaded) return null;

//            // TODO: verify the animation count change if you Add/Delete animation ranges?
//            int count = _actor.GetAnimationCount();
//            if (count == 0) return null;

//            Animation.BonedAnimation[] results = new Animation.BonedAnimation[count];

//            for (int i = 0; i < results.Length; i++)
//            {
//                int src = 0;
//                float start = 0;
//                float end = 0;
             
//                // GetAnimationRangeInfo is only for animations made with SetAnimationRangeInfo()
//                //bool success = _actor.GetAnimationRangeInfo(i, ref src, ref start, ref end);
//                float length = _actor.GetAnimationLength(i); // length in milliseconds it seems
//                start = 0;
//                end = length;
//                string name = _actor.GetAnimationName(i);
//                results[i] = new Keystone.Animation.BonedAnimation(Resource.Repository.GetNewName(typeof(Animation.BonedAnimation)), name, i, start, end);
//                results[i]._isIntrinsicAnimation = true;
//            }
//            return results;

//        }

//        #region Geometry Members
//        public override CONST_TV_CULLING CullMode
//        {
//            set
//            {
//                _cullMode = value;
//                _actor.SetCullMode(value);
//            }
//        }

//        public override int GetVertexCount(uint groupIndex)
//        {
//            return -1; // method does not exist for TVActor.  Don't know why
//        }

//        public override int VertexCount
//        {
//            get { return _actor.GetVertexCount(); }
//        }

//        public override int GroupCount
//        {
//            get { return _actor.GetGroupCount(); }
//        }

//        public override int TriangleCount
//        {
//            get { return _actor.GetTriangleCount(); }
//        }

//        public override PickResults AdvancedCollide(Matrix regionMatrix, Vector3d start, Vector3d end, PickParameters parameters)
//        {
//            TV_COLLISIONRESULT result = Helpers.TVTypeConverter.CreateTVCollisionResult();
//            TV_3DMATRIX identity = Helpers.TVTypeConverter.CreateTVIdentityMatrix();
//            // note: picking is done in model space so we use Identity always.  
//            _actor.SetMatrix(identity);
//            // this .Update() call is sadly necessary whenever changing the matrix 
//            // and since we pick in model space, this seems unavoidable.
//            _actor.UpdateEx(0); // TODO: i think we need to pass the keyframe time

//            _actor.AdvancedCollision(Helpers.TVTypeConverter.ToTVVector(start), Helpers.TVTypeConverter.ToTVVector(end), ref result, parameters.ToTVTestType());
//            PickResults pr = new PickResults();
//            pr.Result = result;
//            pr.HasCollided = result.bHasCollided;
            
//            // region matrix is useful for determining ImpactPoint and Distance and for debug rendering info
//            Vector3d impactPoint = Vector3d.TransformCoord(Helpers.TVTypeConverter.FromTVVector(result.vCollisionImpact), regionMatrix);
//            pr.DistanceSquared = Vector3d.GetDistance3dSquared(Vector3d.TransformCoord(start, regionMatrix), impactPoint); // Math.Abs(result.fDistance);
//            pr.Matrix = regionMatrix;
//            pr.CollidedObjectType = CollidedObjectType.TVGeometry;
//            return pr;
//        }

//        // TODO: for Geometry, AdvancedCollide and Update i think can all be Internal
//        public override void Update(ModeledEntity entityInstance)
//        {
//            // TODO: actually i dont think i no longer need this in any Geometry node- MPJ April.27.2012

//            // //then we can do during Update (EntityBase entityInstance) we can do ultimately in the
//            // //Actor.Render(EntityBase entity)is if the Entity for which this actor is 
//            // //being rendered is a child entity of another boned entity, then we will 
//            // //grab the entity's _correspondingBones 
//            //if (entityInstance.Parent is BonedEntity)
//            //{

//            //    for (int i = 0; i < ((BonedEntity)entityInstance).BonedModel.Bones.Length; i++)
//            //    {
//            //        // the entity.Name and entity.Parent.Name are used because GetBoneMatrix must reference the proper underlying actor.Duplicate when grabbing the matrix
//            //        _actor.SetBoneGlobalMatrix(((BonedEntity )entityInstance).BonedModel.Bones[i],
//            //                                                             ((BonedEntity )entityInstance.Parent).BonedModel.GetBoneMatrix(
//            //                                                                 entityInstance.Parent.Name,
//            //                                                                 ((BonedEntity )entityInstance).CorrespondingParentBones[i],
//            //                                                                 false));
//            //    }
//            //}
//            // _actor.SetAnimationID(1);
//           // _actor.PlayAnimation(1);
//           // _actor.StopAnimation();
//           // _actor.PlayAnimation(0.005f);
////           _actor.Update();

//            // NOTE: I believe that SetChangeFlags should only be done to the particular parent of this Entity and not ALL entities that share this Actor3d.
//            //       TODO: for now i think we'll only do SetChangeFlags in the Entity that references this instead.  
//            //       TODO: Perhaps just utilizing the max bounding volume over any set of animations will make needing SetChangeFlag here irrelevant.
////             SetChangeFlags( Enums.ChangeStates.KeyFrameUpdated, Keystone.Enums.ChangeSource.Self); 
//        }


//        public override void Render(ModeledEntity entityInstance, Model model, Vector3d cameraSpacePosition, float elapsedMilliseconds)
//        {
//            if (_actor == null || !TVResourceIsLoaded) return;

//            if (model.Appearance != null)
//            {
//               _appearanceHashCode = model.Appearance.Apply(this, model.AppearanceFlags,  elapsedMilliseconds);
//            }
//            else
//                _appearanceHashCode = NullAppearance.Apply(this, model.AppearanceFlags, _appearanceHashCode);


//            // TODO: Here is the crux of our multiple Actor Update problem.  If we "Update" 
//            // our actors keyframes in Simulation.Update() then TV still invariably needs 
//            // to Update() the Actor whenever the keyframe changes and since we MUST set 
//            // the underlying actor3d's keyframe for every BonedMondel instance
//            // on Render() because we are sharing just a single underlying Actor3d then
//            // it results in one extra unnecessary actor.Update FOR EACH BonedMondel instance!
//            // This is why the best solution is to modify Actor3d to underthehood use 
//            // Duplicates() but still only use one Actor3d node.
//            //
//            // even though we've already updated the animation itself in
//            // our Simulation.Update()  we now just need to restore the resulting animationID and keyframe
//            // so this instance renders appropriately. 
//            // (note: only non keyframe animations (e.g. ragdoll physics) need to restore BoneMatrices.
//            //     AnimationID = ((BonedModel) parent).AnimationID;
//            //     KeyFrame = ((BonedModel) parent).KeyFrame;

//            // ((BonedModel) parent).SetBones();
//            //    this.Update(entityInstance ); // update the TVActor

//            // set camera space centric region matrix
//            TV_3DMATRIX tvmatrix = Helpers.TVTypeConverter.ToTVMatrix(model.RegionMatrix); 

//            tvmatrix.m41 = (float)cameraSpacePosition.x;
//            tvmatrix.m42 = (float)cameraSpacePosition.y;
//            tvmatrix.m43 = (float)cameraSpacePosition.z;

//            _actor.SetMatrix(tvmatrix);                        
            
//            try
//            {
//                // MUST pass (true) whenever we change the Matrix!  This is the unfortunate thing
//                // because camera space rendering requires it.
//                _actor.UpdateEx(0);
//                _actor.Render(); 
//                // TODO: while we render(true) notify parent Model of change
//                //SetChangeFlags(Enums.ChangeStates.MatrixDirty | Enums.ChangeStates.BoundingBoxDirty| Enums.ChangeStates.KeyFrameUpdated, Keystone.Enums.ChangeSource.Self); 
            
//            }
//            catch
//            {
//                Trace.WriteLine(string.Format("Actor3d.Render() - Failed to render '{0}'.", _id));
//            }

//            // TODO: shoudl this script be called against the ENtity from within the Actor?!
//            // that seems wrong.  
//            // What is even the purpose of the "OnRender" script?  We shouldn't need an 
//            // OnRender script right?  I wish i wrote notes on what i was even thinking here...
//            // Actually see Mesh3d where i did note that shader parameters could be passed here.
// //           entityInstance.ExecuteScript("OnRender", null);

//        }
//        #endregion


//        #region IBoundVolume Members        
//        // From Geometry always return the Local box/sphere
//        // and then have the model's return World box/sphere based on their instance
//        // BoundingVolume is per duplicate because the volume changes as the animation changes
//        // However, maybe the proper thing to do is create a combined volume that takes into
//        // account every animation so that we dont have to compute this effectively every frame
//        protected override void UpdateBoundVolume()  // TODO: i think all these UpdateBoundVolume's should be Internal only
//        {
//            if (_actor == null) return;
//            // IMPORTANT: For Actors (not sure yet about meshes) TV re-centers
//            // x and z. NOTE however there is no GetVertex for TV so you can't iterate
//            // through them to see if computing the bounding box that way produces
//            // the re-centered bounding box.  
//            // 
//            float radius = 0f;
//            TV_3DVECTOR min, max, center;
//            min.x = min.y = min.z = 0;
//            max.x = max.y = max.z = 0;
//            center.x = center.y = center.z = 0;

//            // April.18.2011 - Sylvain stated in private IRC chat that
//            // .GetBoundingBox (local = TRUE) is buggy.  I confirmed that.
//            // Sylvain says part of the problem is he keeps bone matrices in world
//            // coords so it's difficult to untransform them all to get real local
//            // However using local=False will give us the world bounding box that takes
//            // accurate bone positions into account.

//            // So considering the bug, all we have to do is first move the actor
//            // to 0,0,0, with 1,1,1 scale and 0,0,0 rotation (ie Identity)
//            // and compute the world bounding box and it will be equivalent to
//            // the local bounding box.  The only problem is... you MUST call .Update()
//            // after changing the matrix (this applies to collision detection tests too)

//            TV_3DMATRIX identity = Helpers.TVTypeConverter.CreateTVIdentityMatrix();
//            _actor.SetMatrix(identity);
////            _actor.UpdateEx(0); // this update is sadly necessary when changing the matrix 

//            // MUST use false (ie WorldSpace box) because localspace=true is buggy and wont be fixed
//            _actor.GetBoundingBox(ref min, ref max, false); 
//            _box = new BoundingBox(min.x, min.y, min.z, max.x, max.y, max.z);

//            _actor.GetBoundingSphere(ref center, ref radius, true);
//            _sphere = new BoundingSphere(_box);
       
//            DisableChangeFlags(Keystone.Enums.ChangeStates.BoundingBox_TranslatedOnly | Keystone.Enums.ChangeStates.BoundingBoxDirty);

             
//            // there is no GetVertex()!
//            // http://www.truevision3d.com/forums/tv3d_sdk_65/actors_and_getvertex-t12490.0.html;msg92852#msg92852
//            //for (int i = 0; i < TVActor.GetVertexCount(); i++)
//            //    _actor.GetVertex()

//            //TVMesh mesh = _actor.GetDeformedMesh();
//            //mesh.GetBoundingBox(ref min, ref max, true);
//            //_box = new BoundingBox(min.x, min.y, min.z, max.x, max.y, max.z);
//        }
//        #endregion

//        #region IDisoposable members
//        protected override void DisposeManagedResources()
//        {
//            base.DisposeManagedResources();
//            try
//            {
//                if (_actor != null)
//                    _actor.Destroy();
//            }
//            catch
//            {
//            }
//        }
//        #endregion
//    }
//}
