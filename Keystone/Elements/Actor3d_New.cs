using System;
using System.Collections.Generic;
using System.Diagnostics;
using KeyCommon.Traversal;
using Keystone.Appearance;
using Keystone.Collision;
using Keystone.IO;
using Keystone.Resource;
using Keystone.Shaders;
using Keystone.Traversers;
using Keystone.Types;
using MTV3D65;

namespace Keystone.Elements
{
    public class Actor3dDuplicate // NOTE: public accessible for AnimationTrack and Appearance traverser
    {
        // how would you specify this node when using GetProperties/SetProperties?
        // well... you shouldn't have to because the idea is that this Actor is treated
        // as shared just like a shared mesh... 

        // for a mesh if you wanted different behavior, you MUST load a different Mesh
        // but if you just want different appearance, you can easily do that because
        // appearance is tied to the model, not the mesh.

        // so there's no reason this should not work... the absolutely only thing
        // different we have to worry about is actor animations... and for that
        // we need to persist across each "shared use."

        internal TVActor mDuplicateActor;

        // NOTE: here we only track differences in Appearance.  
        // Duplicates internally shares geometry, bones and animations.  Thus we can have different shaders, textures and materials on the duplicates.
        // Using the _appearanceHashCode, we can avoid having to SetShader, SetTexture, SetMaterial, etc. during AttributeGroup.Apply() or Appearance.Apply()
        // Animations and Keyframes are part of the prefab however.  I'm not sure if tv internally duplicates those when setting up AnimationClips.
        private CONST_TV_LIGHTINGMODE _lightingMode; // lightingmode of the actor is cached only so during traversal we can tell if we need to change it after rendering a previous instance.  But it does NOT need to be saved/read from file
        private Shader _shader;
        private bool _alphaTestEnable;
        private int _alphaTestRefValue;
        private bool _alphaTestDepthBufferWriteEnable;
        private int _appearanceHashCode;

        public Actor3dDuplicate()
        {
            
            
        }

        public Actor3dDuplicate(TVActor actor)
        {
            mDuplicateActor = actor;
        }

        internal int LastAppearance
        {
            get { return _appearanceHashCode; }
            set { _appearanceHashCode = value; }
        }

        /// <summary>
        /// note: SetLightingMode occurs after geometry is loaded because the lightingmode
        /// is stored in .tvm, .tva files and so anything you set prior to loading, will be
        /// replaced with the lightingmode stored in the file.
        /// note; this access modifer must be "internal" and is only setable by DefaultAppearance.Apply()
        /// </summary>
        internal CONST_TV_LIGHTINGMODE LightingMode
        {
            get { return _lightingMode; }
            set
            {
                _lightingMode = value;
                mDuplicateActor.SetLightingMode(value);
            }
        }

        internal void SetGroupRenderOrder(bool enableRenderOrder, int[] order)
        {
            if (order == null)
                return;

            mDuplicateActor.SetGroupRenderOrder(enableRenderOrder, order.Length, order);
        }

        /// <summary>
        /// Shader is applied via Appearance only.
        /// </summary>
        internal Shader Shader
        {
            get { return _shader; }
            set
            {                
                if (_shader == value && ( value == null || value.PageStatus == PageableNodeStatus.Loaded)) return;

                if (value != null)
                {
                    // also we don't want to assign this shader if it's underlying tvshader is not yet paged in.
                    if (value.PageStatus != PageableNodeStatus.Loaded)
                    {
                        System.Diagnostics.Debug.WriteLine("Actor3d.Shader '" + value.ID + "' not loaded.  Cannot assign to Actor '" + mDuplicateActor.GetName() + "'");
                        return;
                    }
                }
                
                _shader = value;
                if (_shader == null)
                    mDuplicateActor.SetShader(null);
                else
                    mDuplicateActor.SetShader(_shader.TVShader);
            }
        }


        // TODO: should change to SetAlphaTest and also seperate vars for getting the alpha test bool and refvalue?
        internal void SetAlphaTest (bool enable, int iGroup)
        {
           
            _alphaTestEnable = enable;
            mDuplicateActor.SetAlphaTest(_alphaTestEnable, _alphaTestRefValue, _alphaTestDepthBufferWriteEnable);
            
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
        internal CONST_TV_BLENDINGMODE BlendingMode
        {
            set
            {
                mDuplicateActor.SetBlendingMode(value);
            }
        }

        internal void SetBlendingMode(CONST_TV_BLENDINGMODE blendingMode, int group)
        {
            throw new Exception ("Actor doesn't seem to support unique per group blending modes");
            //if (group <= -1)
            //    _actor.SetBlendingMode(blendingMode);
            //else
            //    _actor.SetBlendingMode (blendingMode, group);  // this method doesnt exist in tv3d for actors
        }

        // materials and textures can be different for different duplicates
        internal int GetMaterial(int groupID)
        {
            return mDuplicateActor.GetMaterial(groupID);
        }

        internal void SetMaterial(int tvMaterialID, int groupID)
        {
            if (groupID == -1)
                mDuplicateActor.SetMaterial(tvMaterialID);
            else
                mDuplicateActor.SetMaterial(tvMaterialID, groupID);
        }

        internal int GetTextureEx(CONST_TV_LAYER layer, int groupID)
        {
            return mDuplicateActor.GetTextureEx((int)layer, groupID);
        }

        internal void SetTexture(int tvTextureID, int groupID)
        {
            if (groupID == -1)
                mDuplicateActor.SetTexture(tvTextureID);
            else
                mDuplicateActor.SetTexture(tvTextureID, groupID);
        }

        internal void SetTextureEx(int layer, int tvTextureID, int groupID)
        {
            if (groupID == -1)
                mDuplicateActor.SetTextureEx(layer, tvTextureID);
            else
                mDuplicateActor.SetTextureEx(layer, tvTextureID, groupID);
        }

    }

    /// <summary>
    /// This new actor3d class does not attempt to manage duplicates under this single
    /// class.  Instead, any attempt to create an Actor3d using an existing Actor3d's
    /// filepath, we'll result in a new Actor3d class being constructed.
    /// NOTE: Can you call .Duplicate() from another Duplicate?
    /// Attempts to manage Actor3d has just been too complicated.  This is going to 
    /// work better for us.
    /// 
    /// Vertices, UVs, Normals, Animation Data, Bones are the constant shared
    /// data for Duplicates.  
    /// Textures, Materials, Shaders, can all be different.
    /// </summary>
    public class Actor3d : Geometry 
    {
        
        // keyed by the parent model's id
        internal Dictionary<string, Actor3dDuplicate> mDuplicates;

        // always keep reference to the original actor that was used make the duplicates from.
        // if we try to just store it only in the mDuplicates, it could get removed when it's deleted
        // from the scene for some reason.  
        internal TVActor _actor;   
        private CONST_TV_ACTORMODE _actorMode;        
        private CONST_TV_LIGHTINGMODE _lightingMode;

        // indexer
        public Actor3dDuplicate this[string parentModelID]
        {
            get
            {
                if (mDuplicates == null) throw new ArgumentOutOfRangeException();

                Actor3dDuplicate dupe;
                if (mDuplicates.TryGetValue(parentModelID, out dupe))
                    return dupe;

                return null;
            }
        }

        /// <summary>
        /// Initialize an Actor3d with just a path.  The details will be
        /// read in during deserialization (eg. ReadXML)
        /// </summary>
        /// <param name="resourcePath"></param>
        internal Actor3d(string resourcePath)
            : base(resourcePath)
        {
        }

        private Actor3d(string id, TVActor actor)
            : this(id)
        {
            if (actor == null) throw new ArgumentNullException("Actor.ctor()");
            _tvfactoryIndex = actor.GetIndex();
        }


        public static Actor3d Create( string relativePath, CONST_TV_ACTORMODE actorMode, bool loadTextures,
                                     bool loadMaterials, out DefaultAppearance appearance)
        {

            Actor3d actor = (Actor3d)Repository.Get(relativePath);

            appearance = null;
            if (actor == null)
            {
                // Load() immediately - On importing an actor to Assets, we do have to load it first and cannot
                // add to tree and wait for LoadTVResource()
                // TODO: but why dont we just have the caller call .LoadTVResource()?
                TVActor a = Load(relativePath, actorMode, loadTextures,
                    loadMaterials, out appearance);
                actor = new Actor3d(relativePath, a); 
                actor._actor = a;
            }
            else
            {
                if (actor.Parents != null)
                {
                    for (int i = 0; i < actor.Parents.Length; i++)
                    {
                        if (actor.Parents[i] is Model && ((Model)actor.Parents[i]).Appearance != null)
                        {
                            // TODO: i should probably verify i have checks 4 any non share-able node such as
                            // appearance, Models, Entities, etc can never have more than one parent.
                            // perhaps we can find an alternative way to do this...  Node.Clone()
                            // method 
                            string clonedAppearanceID = Repository.GetNewName(((Model)actor.Parents[i]).Appearance.TypeName);
                            appearance = (DefaultAppearance)((Model)actor.Parents[i]).Appearance.Clone(clonedAppearanceID, true, false);
                            break;
                        }
                    }
                }
                Trace.WriteLine("Actor " + relativePath + " already found in cache... sharing mesh instance.");
            }
            // we can skip loading the actor and go straight to loading the appearance if the actor already exists in the cache.
            // NOTE: We cannot know the appearance though of a previous one... but the way to avoid creating a virtual dupe 
            // is to just .Clone the model and not try to re-load the appearance this way

            return actor;
        }

        public static Actor3d Create(string id)
        {
            Actor3d actor = (Actor3d)Keystone.Resource.Repository.Create (id, typeof (Actor3d).Name);
            return actor;
        }

        #region ITraversable Members
        public override object Traverse(ITraverser target, object data)
        {
            return target.Apply(this, data);
        }
        #endregion

        private static TVActor Load( string relativePath, CONST_TV_ACTORMODE actorMode,
                                           bool loadTextures, bool loadMaterials, out DefaultAppearance appearance)
        {
            appearance = null;
            TVActor a = CoreClient._CoreClient.Scene.CreateActor(relativePath);
            
            // ActorMode set BEFORE the actor is loaded from file because it impacts 
            //how the actor data is configured during load.
            a.SetActorMode(actorMode);

            KeyCommon.IO.ResourceDescriptor descriptor = new KeyCommon.IO.ResourceDescriptor(relativePath);

            bool result = false;

#if DEBUG
            Stopwatch watch = new Stopwatch();
            watch.Reset();
            watch.Start();
#endif


            if (descriptor.IsArchivedResource)
            {
                if (descriptor.Extension == ImportLib.XFILE_EXTENSION || descriptor.Extension == ImportLib.TVACTOR_EXTENSION)
                {
                    System.Runtime.InteropServices.GCHandle gchandle;
                    string memoryFile = Keystone.ImportLib.GetTVDataSource(descriptor.EntryName, "", Keystone.Core.FullNodePath(descriptor.ModName ), out gchandle);
                    if (string.IsNullOrEmpty(memoryFile)) throw new Exception("Error importing actor file from archive.");

                    if (descriptor.Extension == ImportLib.XFILE_EXTENSION)
                        result = a.LoadXFile(memoryFile, loadTextures, loadMaterials);
                    else if (descriptor.Extension == ImportLib.TVACTOR_EXTENSION)
                    {
                        result = a.LoadTVA(memoryFile, loadTextures, loadMaterials);
                    }
                    gchandle.Free();
                }
            }
            else
            {
                if (descriptor.Extension == ImportLib.XFILE_EXTENSION)
                    result = a.LoadXFile(Core.FullNodePath(descriptor.EntryName), loadTextures, loadMaterials);
                else if (descriptor.Extension == ImportLib.TVACTOR_EXTENSION)
                    result = a.LoadTVA(Core.FullNodePath(descriptor.EntryName), loadTextures, loadMaterials);
            }

            if ((a != null) && (loadMaterials) || (loadTextures))
                ImportLib.GetActorTexturesAndMaterials(a, out appearance);


#if DEBUG
            watch.Stop();
#endif
            if (!result)
                throw new Exception(string.Format("Actor3d.Load() - ERROR: '{0}' failed to load.  Check path.", relativePath));

#if DEBUG
            Trace.WriteLine(string.Format ("Actor3d.Load() - SUCCESS: '{0}' loaded with {1} groups and {2} bones in {3} seconds with {4} vertices in {5} triangles.",relativePath, a.GetGroupCount(), a.GetBoneCount (), watch.Elapsed, a.GetVertexCount(), a.GetTriangleCount()));
            for (int i = 0; i < a.GetBoneCount(); i++)
                Trace.WriteLine("Bone " + i.ToString() + " " + a.GetBoneName(i));

#endif

            

            return a;
        }

        #region ResourceBase members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="specOnly">True returns the properties without any values assigned</param>
        /// <returns></returns>
        public override Settings.PropertySpec[] GetProperties(bool specOnly)
        {
            Settings.PropertySpec[] tmp = base.GetProperties(specOnly);
            Settings.PropertySpec[] properties = new Settings.PropertySpec[1 + tmp.Length];
            tmp.CopyTo(properties, 1);

            properties[0] = new Settings.PropertySpec("actormode", typeof(int).Name);

            if (!specOnly)
            {
                properties[0].DefaultValue = (int)_actorMode;
            }
            //    Hull = new ConvexHull(_filePath);
            //    Hull = ConvexHull.ReadXml(xmlNode);
            return properties;
        }

        public override void SetProperties(Settings.PropertySpec[] properties)
        {
            if (properties == null) return;
            base.SetProperties(properties);

            for (int i = 0; i < properties.Length; i++)
            {
                // use of a switch allows us to pass in all or a few of the propspecs depending
                // on whether we're loading from xml or changing a single property via server directive
                switch (properties[i].Name)
                {
                    case "actormode":
                        _actorMode = (CONST_TV_ACTORMODE)((int)properties[i].DefaultValue);
                        break;
                }
            }
        }
        #endregion

        #region IPageableNode Members
        public override void UnloadTVResource()
        {
        	DisposeManagedResources ();
        }
                
        public override void LoadTVResource()
        {
            if (_actor != null)
            {
                try
                {
                    _actor.Destroy();
                }
                catch
                {
                    Trace.WriteLine("Actor3d.LoadTVResource() - Error on Actor.Destroy() - actor path == " + _id);
                }
            }
            
            try
            {
	            DefaultAppearance dummy;
	            _actor = Actor3d.Load(_id, _actorMode, false, false, out dummy);
	            _actor.OptimizeAnimations(); // TODO: should this ever NOT be called?
	            //_actor.EnableFrustumCulling(false); // why tv doesnt have EnableFrustumCulling method for actor?
	            //_actor.SetCullMode((CONST_TV_CULLING)_cullMode); // backface culling usually
                CullMode = _cullMode; // HACK: Make 
                _tvfactoryIndex = _actor.GetIndex();
               


	            // assign duplicates immediately after resource is loaded
	            if (mDuplicates != null)
	                foreach (Actor3dDuplicate dupe in mDuplicates.Values)
	                    dupe.mDuplicateActor = _actor.Duplicate();
	
	            SetChangeFlags(Keystone.Enums.ChangeStates.GeometryAdded |
	                Keystone.Enums.ChangeStates.AppearanceParameterChanged |
	                Keystone.Enums.ChangeStates.BoundingBoxDirty |
	                Keystone.Enums.ChangeStates.RegionMatrixDirty |
	                Keystone.Enums.ChangeStates.MatrixDirty, Keystone.Enums.ChangeSource.Self);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("error on LoadTVResource() - actor path == " + _id + ex.Message);
                throw ex;
            }
        }
        public override void SaveTVResource(string resourcePath)
        {
            if (TVResourceIsLoaded)
                _actor.SaveTVA(resourcePath); // this is why name i think should not be equal to filepath right?
        }

        #endregion

        #region Group Members
        public override void AddParent(IGroup parent)
        {
            base.AddParent(parent);

            // TODO: every time a duplicate is added, we have to initialize it's shared settings
            // such as cullmode, shadermode, lightingmode
            if (mDuplicates == null)
            {
                mDuplicates = new Dictionary<string, Actor3dDuplicate>();
                mDuplicates.Add(((Node)parent).ID, new Actor3dDuplicate(_actor));
            }
            else
            {
                mDuplicates.Add(((Node)parent).ID, new Actor3dDuplicate());
                if (TVResourceIsLoaded) mDuplicates[((Node)parent).ID].mDuplicateActor = _actor.Duplicate();
            }
        }

        public override void RemoveParent(IGroup parent)
        {
            base.RemoveParent(parent);
            mDuplicates.Remove(((Node)parent).ID);
        }
        #endregion
        
        //public void ComputeNormals()
        //{
        //    _actor.ComputeNormals();
        //}

        ///// <summary>
        ///// ActorMode should never be changed once the underlying actor geometry has been loaded.
        ///// </summary>
        //internal CONST_TV_ACTORMODE ActorMode
        //{
        //    // NOTE: No Setter because we can only set the ActorMode before loading the geometry
        //    // and that means either during static Create() or upon deserialization.
        //    get { return _actorMode; }
        //}

        ///// <summary>
        ///// note: SetLightingMode occurs after geometry is loaded because the lightingmode
        ///// is stored in .tvm, .tva files and so anything you set prior to loading, will be
        ///// replaced with the lightingmode stored in the file.
        ///// note; this access modifer must be "internal" and is only setable by DefaultAppearance.Apply()
        ///// </summary>
        //internal CONST_TV_LIGHTINGMODE LightingMode
        //{
        //    get { return CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED; }
        //    set
        //    {
        //        _lightingMode = value;
        //        _actor.SetLightingMode(value);
        //    }
        //}

        ///// <summary>
        ///// Shader is applied via Appearance only.
        ///// </summary>
        //internal override Shader Shader
        //{
        //    set
        //    {
        //        if (_shader == value && value.TVShader != null) return;
        //        if (value.TVShader == null) return;

        //        if (!value.TVResourceIsLoaded)
        //        {
        //            System.Diagnostics.Debug.WriteLine("Shader '" + _shader.ID + "' not loaded.  Cannot assign to Actor '" + _id + "'");
        //            return;
        //        }
        //        _shader = value;
        //        if (_shader == null)
        //            _actor.SetShader(null);
        //        else
        //            _actor.SetShader(_shader.TVShader);
        //    }
        //}

        //// TODO: hrm, we moved Overlay out to Entities and basically overlay
        //// forces a mesh/actor/particle system to ignore the depth buffer.  In effect
        //// it just disables the depth buffer during render.
        //// internal used by Appearance
        //internal override bool Overlay
        //{
        //    set
        //    {
        //        _actor.SetOverlay(value);
        //    }
        //}

        //// TODO: should change to SetAlphaTest and also seperate vars for getting the alpha test bool and refvalue?
        //internal override bool AlphaTest
        //{
        //    set
        //    {
        //        _alphaTestEnable = value;
        //        _actor.SetAlphaTest(_alphaTestEnable, _alphaTestRefValue, _alphaTestDepthBufferWriteEnable);
        //    }
        //}


        //internal int AlphaTestRefValue
        //{
        //    get { return _alphaTestRefValue; }
        //    set
        //    {
        //        _alphaTestRefValue = value;
        //    }
        //}

        //internal bool AlphaTestDepthWriteEnable
        //{
        //    get { return _alphaTestDepthBufferWriteEnable; }
        //    set
        //    {
        //        _alphaTestDepthBufferWriteEnable = value;
        //    }
        //}

        ///// <summary>
        ///// For opacity, BlendingMode must be set to .TV_BLEND_ALPHA
        ///// </summary>
        //internal override CONST_TV_BLENDINGMODE BlendingMode
        //{
        //    set
        //    {
        //        _blendingMode = value;
        //        _actor.SetBlendingMode(value);
        //    }
        //}

        //// materials and textures can be different for different duplicates
        //internal int GetMaterial(int groupID)
        //{
        //    return _actor.GetMaterial(groupID);
        //}

        //internal void SetMaterial(int tvMaterialID, int groupID)
        //{
        //    if (groupID == -1)
        //        _actor.SetMaterial(tvMaterialID);
        //    else
        //        _actor.SetMaterial(tvMaterialID, groupID);
        //}

        //internal int GetTextureEx(CONST_TV_LAYER layer, int groupID)
        //{
        //    return _actor.GetTextureEx((int) layer, groupID);
        //}

        //internal void SetTexture(int tvTextureID, int groupID)
        //{
        //    if (groupID == -1)
        //        _actor.SetTexture(tvTextureID);
        //    else
        //        _actor.SetTexture(tvTextureID, groupID);
        //}

        //internal void SetTextureEx(int layer, int tvTextureID, int groupID)
        //{
        //    if (groupID == -1)
        //        _actor.SetTextureEx(layer, tvTextureID);
        //    else
        //        _actor.SetTextureEx(layer, tvTextureID, groupID);
        //}


        #region Bones
        //public void SetBoneEnable(int iBoneId, bool bEnable)
        //{
        //    mActor.SetBoneEnable(iBoneId, bEnable);
        //}

        //public bool IsBoneEnabled(int iBoneId)
        //{
        //    return mActor.IsBoneEnabled(iBoneId);
        //}
        //public void SetBoneRotation(int iBone, float aRotateX, float aRotateY, float aRotateZ, bool bRelative)
        //{
        //    mActor.SetBoneRotation(iBone, aRotateX, aRotateY, aRotateZ, bRelative);
        //}

        //public void SetBoneRotationMatrix(int iBone, TV_3DMATRIX mRotateMatrix, bool bRelative)
        //{
        //    mActor.SetBoneRotationMatrix(iBone, mRotateMatrix, bRelative);
        //}

        //public TV_3DMATRIX GetBoneMatrix(int iBone)
        //{
        //    return mActor.GetBoneMatrix(iBone);
        //}

        //public TV_3DMATRIX GetBoneMatrix(int iBone, bool bModelSpace)
        //{
        //    return mActor.GetBoneMatrix(iBone, bModelSpace);
        //}
        //public void SetBoneMatrix(int iBone, TV_3DMATRIX mMatrix)
        //{
        //    mActor.SetBoneMatrix(iBone, mMatrix);
        //}

        //public void SetBoneMatrix(int iBone, TV_3DMATRIX mMatrix, bool bRelative)
        //{
        //    mActor.SetBoneMatrix(iBone, mMatrix, bRelative);
        //}

        //public void SetBoneMatrix(int iBone, TV_3DMATRIX mMatrix, bool bRelative, bool bModelSpace)
        //{
        //    mActor.SetBoneMatrix(iBone, mMatrix, bRelative, bModelSpace);
        //}

        //public TV_3DVECTOR GetBonePosition(int iBone)
        //{
        //    return mActor.GetBonePosition(iBone);
        //}

        //public TV_3DVECTOR GetBonePosition(int iBone, bool bLocal)
        //{
        //    return mActor.GetBonePosition(iBone, bLocal);
        //}
        //public void SetBoneTranslation(int iBone, float fTranslationX, float fTranslationY, float fTranslationZ)
        //{
        //    mActor.SetBoneTranslation(iBone, fTranslationX, fTranslationY, fTranslationZ);
        //}

        //public void SetBoneTranslation(int iBone, float fTranslationX, float fTranslationY, float fTranslationZ, bool bRelative)
        //{
        //    mActor.SetBoneTranslation(iBone, fTranslationX, fTranslationY, fTranslationZ, bRelative);
        //}

        //public void SetBoneScale(int iBone, float fScaleX, float fScaleY, float fScaleZ)
        //{
        //    mActor.SetBoneScale(iBone, fScaleX, fScaleY, fScaleZ);
        //}

        //public void SetBoneGlobalMatrix(int iBone, TV_3DMATRIX mMatrix)
        //{
        //    mActor.SetBoneGlobalMatrix(iBone, mMatrix);
        //}

        //public int GetBoneParent(int iBone)
        //{
        //    return mActor.GetBoneParent(iBone);
        //}
        public int GetBoneCount()
        {
            return _actor.GetBoneCount();
        }

        public string GetBoneName(int index)
        {
            return _actor.GetBoneName(index);
        }

        // TODO: i think the initial bone array should be done right off the bat.
        // well... maybe not, how often do we really need to call GetBones? Although
        // since we do internally use duplicates we could share this bone info but
        // the matrices would be different for each duplicate instance
        public Bone[] GetBones()
        {
            Bone[] results = new Bone[GetBoneCount()];
            if (results == null) return null;

            for (int i = 0; i < results.Length; i++)
            {
                results[i].ID = i;
                results[i].Name = GetBoneName(i);
                results[i].ParentID = _actor.GetBoneParent(i);
                //results[i].Enable = _actor.SetBoneEnable (i, true); // ugh
                results[i].Position = Helpers.TVTypeConverter.FromTVVector (_actor.GetBonePosition(i));
                //results[i].Matrix = Helpers.TVTypeConverter.FromTVMatrix (_actor.GetBoneMatrix(i, true));
                //results[i].Rotation = 

            }
            return results;
        }

        public struct Bone
        {
            public int ID;
            public int ParentID;
            public string Name;
            public Vector3d Position;
            public Matrix Matrix;
        }

        public Matrix GetBoneMatrix(int boneID, bool modelSpace)
        {
            // i think we should always return in model space 
            // TODO: delete this bool
            return Helpers.TVTypeConverter.FromTVMatrix(_actor.GetBoneMatrix(boneID, modelSpace));
            // return _actor.GetBoneMatrix (boneID, modelSpace);
            throw new NotImplementedException();
        }

        public void SetBoneMatrix(int boneID, TV_3DMATRIX matrix, bool modelSpace)
        {
            //_actor.SetBoneMatrix(boneID, matrix, false, modelSpace);
            //_actor.SetBoneGlobalMatrix(boneID, matrix);

            //_actor.SetBoneMatrix(boneID, Matrix, relative, modelSpace);
            //_actor.SetBoneRotationMatrix(boneID, Matrix, relative);

        }
        #endregion

        #region Animations
        /// <summary>
        /// Returns an array of the animations that are intrinsic (built in) to the
        /// actor.
        /// </summary>
        /// <returns></returns>
        public Animation.BonedAnimation[] GetAnimations()
        {
            // NOTE: This call should occur on Import of an Actor
            // to the Assets and when creating the KGBEntity
            // Also, later when adding animation ranges, user should save.
            // Also  if ImportAnimations and adding more, new Animation[] should be created
            // for adding as children to the Entity
            //
            // But also it should be callable after the fact.... how do we name these?
            // 
            // TODO: or should i just pass in Actor3d here
            // also, when adding new  animation ranges should I
            // 
            if (!TVResourceIsLoaded) return null;

            // TODO: verify the animation count change if you Add/Delete animation ranges?
            int count = _actor.GetAnimationCount();
            if (count == 0) return null;

            Animation.BonedAnimation[] results = new Animation.BonedAnimation[count];

            for (int i = 0; i < results.Length; i++)
            {
                int src = 0;
                float start = 0;
                float end = 0;
             
                // GetAnimationRangeInfo is only for animations made with SetAnimationRangeInfo()
                //bool success = _actor.GetAnimationRangeInfo(i, ref src, ref start, ref end);
                float length = _actor.GetAnimationLength(i); // length in milliseconds it seems
                start = 0;
                end = length;
                string name = _actor.GetAnimationName(i);
                //results[i] = new Keystone.Animation.BonedAnimation(Resource.Repository.GetNewName(typeof(Animation.BonedAnimation)), name, i, start, end);
                results[i] = Keystone.Animation.BonedAnimation.Create(Resource.Repository.GetNewName(typeof(Animation.BonedAnimation)), name, i, start, end);
                results[i]._isIntrinsicAnimation = true;
            }
            
            return results;

        }
		#endregion

        #region Geometry Members
        public override int CullMode
        {
            set
            {
                _cullMode = value;
                if (TVResourceIsLoaded)
                    _actor.SetCullMode((CONST_TV_CULLING)value);
            }
        }

        public override int GetVertexCount(uint groupIndex)
        {
            return -1; // method does not exist for TVActor.  Don't know why
        }

        public int BonesCount
        {
            get { return _actor.GetBoneCount(); }
        }
        public override int VertexCount
        {
            get { return _actor.GetVertexCount(); }
        }

        public override int GroupCount
        {
            get { return _actor.GetGroupCount(); }
        }

        public override int TriangleCount
        {
            get { return _actor.GetTriangleCount(); }
        }

        public override object GetStatistic(string name)
        {
            switch (name.ToUpper())
            {
                case "BONES":
                case "BONESCOUNT":
                    return BonesCount;

                default:
                    return base.GetStatistic(name);
            }
        }

        internal override PickResults AdvancedCollide(Vector3d start, Vector3d end, KeyCommon.Traversal.PickParameters parameters)
        {
            Actor3dDuplicate duplicate = this[parameters.ActorDuplicateInstanceID];
             
            // TODO: i need to do this through our Actor3dDuplicates()
            // because different actors will have different animated poses which simply
            // setting to identity is not going to address. (although we will still do model space
            // picking and so set to identity)
            TV_COLLISIONRESULT result = Helpers.TVTypeConverter.CreateTVCollisionResult();
            TV_3DMATRIX identity = Helpers.TVTypeConverter.CreateTVIdentityMatrix();
            //// note: picking is done in model space so we use Identity always.  
            //_actor.SetMatrix(identity);
            //// this .Update() call is sadly necessary whenever changing the matrix 
            //// and since we pick in model space, this seems unavoidable.
            //_actor.UpdateEx(0); 
            //_actor.AdvancedCollision(Helpers.TVTypeConverter.ToTVVector(start), Helpers.TVTypeConverter.ToTVVector(end), ref result, parameters.ToTVTestType());
            duplicate.mDuplicateActor.SetMatrix(identity);
            duplicate.mDuplicateActor.UpdateEx(0); // dont update animations, but update bone positions to model space

            // NOTE: using Accurate TVTest type seems to produce bad results here.  So we don't pass that type.
            duplicate.mDuplicateActor.AdvancedCollision(Helpers.TVTypeConverter.ToTVVector(start), 
                                            Helpers.TVTypeConverter.ToTVVector(end), 
                                            ref result, 
                                            PickResults.ToTVTestType(parameters.Accuracy));
            
            
            PickResults pr = new PickResults();
            //pr.Result = result; // obsolete - we do not place MTV3D65 dependancy in our PickResult class
            pr.HasCollided = result.bHasCollided;
            if (pr.HasCollided)
            {
                // region matrix is useful for determining ImpactPoint and Distance and for debug rendering info
                // TODO: is result.vCollisionImpact in local space? Since ive done the collision
                //       in local space i think it should be.
                pr.ImpactPointLocalSpace = Helpers.TVTypeConverter.FromTVVector(result.vCollisionImpact);
                pr.CollidedObjectType = PickAccuracy.Geometry;

                //pr.ActorDuplicateInstanceID = parameters.ActorDuplicateInstanceID; // TODO ?
            }
            return pr;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="scene">Usually Context.Scene rather than Entity.Scene since AssetPlacementTool preview Entity's are not connected to Scene.</param>
        /// <param name="model"></param>
        /// <param name="elapsedSeconds"></param>
        internal override void Render(Matrix matrix, Scene.Scene scene, Model model, double elapsedSeconds)
        {
        	lock (mSyncRoot)
            {
            Actor3dDuplicate duplicate = this[model.ID];

            // TODO: it should be impossible for _resourceStatus to be "Loaded" before it's actually loaded
        	//       HOWEVER if paging out, we could start to render here first since it's not synclocked and then
        	//       while minimesh.Render() we page out and set _resourceStatus to Unloading but we're already in .Render()!
        	
        	// NOTE: we check PageableNodeStatus.Loaded and NOT TVResourceIsLoaded because that 
        	// TVIndex is set after Scene.CreateMesh() and thus before we've finished adding vertices
        	// via .AddVertex()  or .SetGeometry() or even loaded the mesh from file.
            if (_resourceStatus != PageableNodeStatus.Loaded ) return;
                
            if (model.Appearance != null)
            {
                //_appearanceHashCode = model.Appearance.Apply(duplicate, model.AppearanceFlags, elapsedMilliseconds);
                duplicate.LastAppearance = model.Appearance.Apply(duplicate, elapsedSeconds);
            }
            else
                //_appearanceHashCode = NullAppearance.Apply(duplicate, model.AppearanceFlags, _appearanceHashCode);
                duplicate.LastAppearance = NullAppearance.Apply(duplicate, _appearanceHashCode);

            // TODO: Here is the crux of our multiple Actor Update problem.  If we "Update" 
            // our actors keyframes in Simulation.Update() then TV still invariably needs 
            // to Update() the Actor whenever the keyframe changes and since we MUST set 
            // the underlying actor3d's keyframe for every BonedMondel instance
            // on Render() because we are sharing just a single underlying Actor3d then
            // it results in one extra unnecessary actor.Update FOR EACH BonedMondel instance!
            // This is why the best solution is to modify Actor3d to underthehood use 
            // Duplicates() but still only use one Actor3d node.
            //
            // even though we've already updated the animation itself in
            // our Simulation.Update()  we now just need to restore the resulting animationID and keyframe
            // so this instance renders appropriately. 
            // (note: only non keyframe animations (e.g. ragdoll physics) need to restore BoneMatrices.
            //     AnimationID = ((BonedModel) parent).AnimationID;
            //     KeyFrame = ((BonedModel) parent).KeyFrame;

            // ((BonedModel) parent).SetBones();
            //    this.Update(entityInstance ); // update the TVActor


            // set camera space centric region matrix
            TV_3DMATRIX tvmatrix = Helpers.TVTypeConverter.ToTVMatrix(matrix); 

            //TV_3DMATRIX tvmatrix = Helpers.TVTypeConverter.ToTVMatrix(model.RegionMatrix); 
            //tvmatrix.m41 = (float)cameraSpacePosition.x;
            //tvmatrix.m42 = (float)cameraSpacePosition.y;
            //tvmatrix.m43 = (float)cameraSpacePosition.z;

            duplicate.mDuplicateActor.SetMatrix(tvmatrix);
            //_actor.SetMatrix(tvmatrix);                        
            
            try
            {
                // MUST pass (true) whenever we change the Matrix!  This is the unfortunate thing
                // because camera space rendering requires it.
                // TODO: Scene will be null for assets being previewed in AssetPlacementTool since they aren't scene connected
                duplicate.mDuplicateActor.Render(scene.Simulation.Running);
          
                //_actor.UpdateEx(0);
                //_actor.Render(); 
                // TODO: while we render(true) notify parent Model of change
                //SetChangeFlags(Enums.ChangeStates.MatrixDirty | Enums.ChangeStates.BoundingBoxDirty| Enums.ChangeStates.KeyFrameUpdated, Keystone.Enums.ChangeSource.Self); 
            }
            catch
            {
                Trace.WriteLine(string.Format("Actor3d.Render() - Failed to render '{0}'.", _id));
            }

            // TODO: shoudl this script be called against the ENtity from within the Actor?!
            // that seems wrong.  
            // What is even the purpose of the "OnRender" script?  We shouldn't need an 
            // OnRender script right?  I wish i wrote notes on what i was even thinking here...
            // Actually see Mesh3d where i did note that shader parameters could be passed here.
 //           entityInstance.ExecuteScript("OnRender", null);

        }
        }
        #endregion


        #region IBoundVolume Members        
        // From Geometry always return the Local box/sphere
        // and then have the model's return World box/sphere based on their instance
        // BoundingVolume is per duplicate because the volume changes as the animation changes
        // However, maybe the proper thing to do is create a combined volume that takes into
        // account every animation so that we dont have to compute this effectively every frame
        protected override void UpdateBoundVolume()  // TODO: i think all these UpdateBoundVolume's should be Internal only
        {
        	if (_resourceStatus != PageableNodeStatus.Loaded) return;
        	
            // IMPORTANT: For Actors (not sure yet about meshes) TV re-centers
            // x and z. NOTE however there is no GetVertex for TV so you can't iterate
            // through them to see if computing the bounding box that way produces
            // the re-centered bounding box.  
            // 
            float radius = 0f;
            TV_3DVECTOR min, max, center;
            min.x = min.y = min.z = 0;
            max.x = max.y = max.z = 0;
            center.x = center.y = center.z = 0;

            // April.18.2011 - Sylvain stated in private IRC chat that
            // .GetBoundingBox (local = TRUE) is buggy.  I confirmed that.
            // Sylvain says part of the problem is he keeps bone matrices in world
            // coords so it's difficult to untransform them all to get real local
            // However using local=False will give us the world bounding box that takes
            // accurate bone positions into account.

            // So considering the bug, all we have to do is first move the actor
            // to 0,0,0, with 1,1,1 scale and 0,0,0 rotation (ie Identity)
            // and compute the world bounding box and it will be equivalent to
            // the local bounding box.  The only problem is... you MUST call .Update()
            // after changing the matrix (this applies to collision detection tests too)

            TV_3DMATRIX identity = Helpers.TVTypeConverter.CreateTVIdentityMatrix();
            _actor.SetMatrix(identity);
//            _actor.UpdateEx(0); // this update is sadly necessary when changing the matrix 

            // MUST use false (ie WorldSpace box) because localspace=true is buggy and wont be fixed.
            // So we will translate the _box by the actor's position
            _actor.GetBoundingBox(ref min, ref max, false); 
            _box = new BoundingBox(min.x, min.y, min.z, max.x, max.y, max.z);

            _actor.GetBoundingSphere(ref center, ref radius, true);
            _sphere = new BoundingSphere(_box);
       
            DisableChangeFlags(Keystone.Enums.ChangeStates.BoundingBox_TranslatedOnly | 
                               Keystone.Enums.ChangeStates.BoundingBoxDirty);

             
            // there is no GetVertex()!
            // http://www.truevision3d.com/forums/tv3d_sdk_65/actors_and_getvertex-t12490.0.html;msg92852#msg92852
            //for (int i = 0; i < TVActor.GetVertexCount(); i++)
            //    _actor.GetVertex()

            //TVMesh mesh = _actor.GetDeformedMesh();
            //mesh.GetBoundingBox(ref min, ref max, true);
            //_box = new BoundingBox(min.x, min.y, min.z, max.x, max.y, max.z);
        }
        #endregion

        #region IDisoposable members
        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            try
            {
                if (_actor != null)
                    _actor.Destroy();
            }
            catch
            {
            }
        }
        #endregion
    }
}
