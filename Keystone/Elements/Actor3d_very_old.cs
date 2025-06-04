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

// TODO: I think perhaps what we should do is we can have an "SharedActor" which can contain an array of
// duplicates and an array of everything for every property that has to specify the duplicate
// to change a material on or whatever...
// And then encapsulate that with our Actor3d
//  
//namespace Keystone.Elements
//{
//    public class Actor3d : Geometry
//    {
//        /// <summary>
//        /// Vertices, UVs, Normals, Animation Data (actually animation data is part of
//        //// Entity and is usually shared but the Animation.cs objects technically dont have to be
//       ///// and new ones can be created from the same tvactor animation.  Down the road
//       //  eventually it could be possible with a custom Actor class to have our animation data
//       // completely seperate), Bones are the constant shared
//        /// data for Duplicates.  
//        /// Textures, Materials, Shaders, can all be different.
//        /// </summary>
//        public class DuplicateActor : IDisposable 
//        {
//            internal DuplicateActor(TVActor original)
//            {
//                TVActor = original.Duplicate ();
//                if (TVActor == null) throw new Exception("DuplicateActor.CTor() -- Could not create duplicate.");
//                int TVIndex = TVActor.GetIndex();
//            }

//            internal string ID;
//            internal int TVIndex;
//            // the following properties do not need to be serialized.  
//            // They are strictly for determining if the appearance
//            // has changed since last time and thus needs to be Applied
//            // to this Duplicate.  
//            // NOTE: Since no duplicate is ever itself shared, the only time
//            // it will have a different appearance is if during real time edit
//            // or during game logic, the appearance is explicitly altered for this
//            // specific instance.  
//            // In other words, it's not a case of the duplicate being shared and 
//            // appearance change intended for one duplicate, but not the current duplicate.
//            internal int AppearanceHashCode;
//            internal TVActor TVActor;
//            private CONST_TV_LIGHTINGMODE _lightingMode;
//            private CONST_TV_BLENDINGMODE _blendingMode; 
//            internal byte AlphaTestRef;  
//            // to disable AlphaTest set the reference value to 0.
//            internal bool AlphaTestEnabled { get{return AlphaTestRef != 0;}}
//            private Shader _shader;
//            internal int AnimationID;

///// <summary>
///// note: SetLightingMode occurs after geometry is loaded because the lightingmode
///// is stored in .tvm, .tva files and so anything you set prior to loading, will be
///// replaced with the lightingmode stored in the file.
///// </summary>
//            internal CONST_TV_LIGHTINGMODE LightingMode
//            {
//                get { return _lightingMode; }
//                set
//                {
//                    _lightingMode = value;
//                    TVActor.SetLightingMode(_lightingMode);

//                }
//            }

//            /// <summary>
//            /// Shader is applied via Appearance only.
//            /// </summary>
//            internal Shader Shader
//            {
//                set
//                {
//                    if (_shader == value) return;

//                    _shader = value;
//                    if (_shader == null)
//                        TVActor.SetShader(null);
//                    else
//                        TVActor.SetShader(_shader.TVShader);
//                }
//            }

//            // TODO: hrm, we moved Overlay out to Entities and basically overlay
//            // forces a mesh/actor/particle system to ignore the depth buffer.  In effect
//            // it just disables the depth buffer during render.
//            // internal used by Appearance
//            internal bool Overlay
//            {
//                set
//                {
//                    TVActor.SetOverlay(value);
//                }
//            }

//            // TODO: should change to SetAlphaTest and also seperate vars for getting the alpha test bool and refvalue?
//            internal int AlphaTest
//            {
//                set
//                {
//                    bool enable = value != 0;
//                    TVActor.SetAlphaTest(enable, value);
//                }
//            }

//            /// <summary>
//            /// For opacity, BlendingMode must be set to .TV_BLEND_ALPHA
//            /// </summary>
//            internal CONST_TV_BLENDINGMODE BlendingMode
//            {
//                set
//                {
//                    _blendingMode = value;
//                    TVActor.SetBlendingMode(value);
//                }
//            }

//            // materials and textures can be different for different duplicates
//            internal int GetMaterial(int groupID)
//            {
//                return TVActor.GetMaterial(groupID);
//            }

//            internal void SetMaterial(int tvMaterialID, int groupID)
//            {
//                if (groupID == -1)
//                    TVActor.SetMaterial(tvMaterialID);
//                else
//                    TVActor.SetMaterial(tvMaterialID, groupID);
//            }

//            internal int GetTextureEx(CONST_TV_LAYER layer, int groupID)
//            {
//                return TVActor.GetTextureEx((int)layer, groupID);
//            }

//            internal void SetTexture(int tvTextureID, int groupID)
//            {
//                if (groupID == -1)
//                    TVActor.SetTexture(tvTextureID);
//                else
//                    TVActor.SetTexture(tvTextureID, groupID);
//            }

//            internal void SetTextureEx(int layer, int tvTextureID, int groupID)
//            {
//                if (groupID == -1)
//                    TVActor.SetTextureEx(layer, tvTextureID);
//                else
//                    TVActor.SetTextureEx(layer, tvTextureID, groupID);
//            }


//            //Matrix
//            //Position
//            //AdvancedCollision
//            //Update
//            //Render
//            private BoundingBox _box;
//            private BoundingSphere _sphere;

//            public bool BoundVolumeIsDirty { get { return true; } }
//            public BoundingBox BoundingBox { get { return _box; } }
//            public BoundingSphere BoundingSphere { get { return _sphere; } }

//            // BoundingVolume is per duplicate because the volume changes as the animation changes
//            // However, maybe the proper thing to do is create a combined volume that takes into
//            // account every animation so that we dont have to compute this effectively every frame
//            public void UpdateBoundVolume()  // TODO: i think all these UpdateBoundVolume's should be Internal only
//            {
//                if (TVActor == null) return;
//                // IMPORTANT: For Actors (not sure yet about meshes) TV re-centers
//                // x and z. NOTE however there is no GetVertex for TV so you can't iterate
//                // through them to see if computing the bounding box that way produces
//                // the re-centered bounding box.  
//                // 
//                float radius = 0f;
//                TV_3DVECTOR min, max, center;
//                min.x = min.y = min.z = 0;
//                max.x = max.y = max.z = 0;
//                center.x = center.y = center.z = 0;

//                // April.18.2011 - Sylvain stated in private IRC chat that
//                // .GetBoundingBox (local = TRUE) is buggy.  I confirmed that.
//                // Sylvain says part of the problem is he keeps bone matrices in world
//                // coords so it's difficult to untransform them all to get real local
//                // However using local=False will give us the world bounding box.
//                // What does this mean?  
                
//                // So considering the bug, all we have to do is first move the actor
//                // to 0,0,0.  
//                TV_3DVECTOR originalPosition = TVActor.GetPosition();
//                TVActor.SetPosition(0, 0, 0);
//                TVActor.GetBoundingBox(ref min, ref max, false); // MUST use false because localspace=true is buggy and wont be fixed
//                _box = new BoundingBox(min.x, min.y, min.z, max.x, max.y, max.z);

//                TVActor.GetBoundingSphere(ref center, ref radius, true);
//                _sphere = new BoundingSphere(_box);

//                TVActor.SetPosition(originalPosition.x, originalPosition.y, originalPosition.z); 
//                //DisableChangeFlags(Keystone.Enums.ChangeStates.BoundingBox_TranslatedOnly | Keystone.Enums.ChangeStates.BoundingBoxDirty);

//                // April.18.2011 - there's still a problem with the Worldspace box too though.
//                // However keep in mind that my far/near rendering is resulting in actors being
//                // rendered twice and the "Actor.ShowBoundingBox(true)" gets drawn completely
//                // wrong when rendered using the Far pass. 
//                TVActor.ShowBoundingBox(true);

//               // TVActor.SetAnimationID(1);
//                TVActor.PlayAnimation(1);
//                // there is no GetVertex()!
//                // http://www.truevision3d.com/forums/tv3d_sdk_65/actors_and_getvertex-t12490.0.html;msg92852#msg92852
//                //for (int i = 0; i < TVActor.GetVertexCount(); i++)
//                //    TVActor.GetVertex()

//                //TVMesh mesh = TVActor.GetDeformedMesh();
//                //mesh.GetBoundingBox(ref min, ref max, true);
//                //_box = new BoundingBox(min.x, min.y, min.z, max.x, max.y, max.z);
//            }


//            #region IDisposable Members
//            public void Dispose()
//            {
//                try
//                {
//                    if (TVActor != null)
//                        TVActor.Destroy();
//                }
//                catch (Exception ex)
//                {
//                    Debug.WriteLine("DuplicateActor.Dispose() - " + ex.Message);
//                }
//            }
//            #endregion
//        }

//        private Dictionary<string, DuplicateActor> _duplicates;
//        private TVActor _actor;
//        private CONST_TV_ACTORMODE _actorMode;
//        // note: these are not relevant since we use Duplicates however
//        // if we didn't use duplicates and if we ever implemented our own actors
//        // then these would be ok.
//        //private CONST_TV_LIGHTINGMODE _lightingMode;
//        //private int _animationID;

//        // TODO: there are issues with trying to use this _currentActor thing...  
//        // but it allows me to use just one appearance and one ModelBase for all of them.
//        // but it's problematic referencing the proper duplicate within this Actor based
//        // on which Entity is being updated or rendered.
//        public static Actor3d Create( string relativePath, CONST_TV_ACTORMODE actorMode, bool loadTextures,
//                                     bool loadMaterials, out DefaultAppearance appearance)
//        {

//            Actor3d actor = (Actor3d)Repository.Get(relativePath);

//            appearance = null;
//            if (actor == null)
//            {
//                TVActor a = CreateActor( relativePath, actorMode, loadTextures, loadMaterials, out appearance );

//                actor = new Actor3d(relativePath, a); // TODO: needs to be added to cache
//                //Trace.WriteLine("Actor " + relativePath + " loaded with " + a.GetBoneCount() + " bones, " + a.GetAnimationCount() + " animations, " + a.GetVertexCount() + " vertices in " + a.GetGroupCount() + " groups.");
//                //for (int i = 0; i < a.GetBoneCount(); i++ )
//                //    Trace.WriteLine ("Bone " + i.ToString( ) + " " + a.GetBoneName(i));

//                actor._actor = a;
//            }

//            // we can skip loading the actor and go straight to loading the appearance if the actor already exists in the cache.
//            // NOTE: We cannot know the appearance though of a previous one... but the way to avoid creating a virtual dupe 
//            // is to just .Clone the model and not try to re-load the appearance this way

//            return actor;
//        }

//        internal Actor3d(string resourcePath)
//            : base(resourcePath)
//        {
//            if (string.IsNullOrEmpty(resourcePath)) throw new ArgumentNullException();
//        }

//        public static Actor3d Create(string id, XmlNode xmlnode)
//        {
//            Actor3d actor;
//            actor = (Actor3d)Repository.Get(id);
//            if (actor != null) return actor;
//            actor = new Actor3d(id);
//            actor.ReadXml(xmlnode);
//            return actor;
//        }

//        private Actor3d(string id, TVActor actor)
//            : base(id)
//        {
//            if (actor == null) throw new ArgumentNullException("Actor.ctor()");
//            _tvfactoryIndex = actor.GetIndex();
//        }

//        private static TVActor CreateActor( string relativePath, CONST_TV_ACTORMODE actorMode,
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
//                    string memoryFile = Keystone.ImportLib.GetTVDataSource(descriptor.ArchiveEntryName, "", descriptor.FullArchivePath, out gchandle);
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
//            Trace.WriteLine("Actor loaded with " + a.GetGroupCount() + " groups." + watch.Elapsed + "seconds, with = " + a.GetVertexCount() + " vertices in " + a.GetTriangleCount() + " triangles. " + relativePath);
//#endif

//            return a;
//        }

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

//            properties[0] = new Settings.PropertySpec("actormode", _actorMode.GetType());

//            if (!specOnly)
//            {
//                properties[0].DefaultValue = (int)_actorMode;
//            }

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
//            _actor = CreateActor(_id, _actorMode, false, false, out dummy);

//            _actor.SetCullMode(_cullMode);
//                // TODO: temporary set of _blendingMode to use alpha to test transparency
//            _blendingMode = CONST_TV_BLENDINGMODE.TV_BLEND_ALPHA;
            
//            _actor.SetBlendingMode(_blendingMode);

//            // alpha test is for alpha mask transparency
//            if (_alphaTestEnable)
//                _actor.SetAlphaTest(_alphaTestEnable, _alphaTestRefValue, _alphaTestDepthBufferWriteEnable);

//            _tvfactoryIndex = _actor.GetIndex();
//            SetChangeFlags(Keystone.Enums.ChangeStates.GeometryAdded | Keystone.Enums.ChangeStates.BoundingBoxDirty, Keystone.Enums.ChangeSource.Self);
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

//        #region ResourceBase members
//        protected override void DisposeManagedResources()
//        {
//            base.DisposeManagedResources();
//            try
//            {
//                if (_duplicates != null)
//                    foreach (DuplicateActor a in _duplicates.Values)
//                        a.Dispose();
//            }
//            catch
//            {
//            }
//        }
//        #endregion

//        #region ITraversable Members
//        public override object Traverse(ITraverser target, object data)
//        {
//            return target.Apply(this, data);
//        }
//        #endregion

//        public void UnRegisterDuplicate(string id)
//        {
//            if (_duplicates == null) throw new ArgumentOutOfRangeException();
//            if (!_duplicates.ContainsKey(id)) throw new ArgumentOutOfRangeException();

//            _duplicates[id].Dispose();
//            _duplicates.Remove(id);
//        }

//        // IMPORTANT: RegisterDuplicates() is called by the Entity because it uses the Entity.Name and not the Actor3d.Name
//        // TODO: this must apply actormode, shaders, materials, textures, etc to all duplicates whenever they are created...
//        // TODO:  we must also re-apply to _all_ when ever we change any of the above...
//        public void RegisterDuplicate(string id)
//        {
//            // if the actor != null, then we must use a small hack to use a duplicate actor which we'll store and track in this single actor object
//            // in order to work around the actor.Update() issue that prevents us from being able to "update" the actor's animation and get it's
//            // new bone matrices when we are just sharing a single actor.  Using the .duplicates and tracking which duplicate belongs to 
//            // which shared reference allows us to do that without breaking our interface model for sharing ModelBase objects.
//            if (_duplicates == null) _duplicates = new Dictionary<string, DuplicateActor>();
//            if (_duplicates.ContainsKey(id))
//            {
//                // if the actual TVActor has not been instantiated 
//                // such as is the case when the underlying geometry has not
//                // yet been paged in, then we have an opportunity to create it here
//                // which is best done by creating a new instance
//                if (_duplicates[id] == null)
//                {
//                    _duplicates[id] = new DuplicateActor(_actor);
//                    return;
//                }
//                else
//                    throw new Exception();
//            }
//            if (TVResourceIsLoaded)
//            {
//                DuplicateActor duplicate = new DuplicateActor (_actor.Duplicate (id));
//                _duplicates.Add(id, duplicate);

//            }   
//            else
//                _duplicates.Add(id, null);

//            // note: we do not return and we allow the load textures/materials to occur below. Recall that Appearance objects
//            // are per instance but the Material and Texture objects are shared.
//        }

//        internal DuplicateActor GetDuplicateInstance(string id)
//        {
//            return _duplicates[id];
//        }

//        // obsolete: we only use AddChild and we manage our own hierarchies
//        // TODO: we dont ever plan on using AttachTo as we use our own hierarchical entities and
//        //       we handle the hierarchicla matrix mults ourselves.  So the note below about
//        //       tvactor.attatchto prohibiting sharing of our actor although true is irrelevant 
//        //       because we wont use that function.
//        // note: Use of AttachTo prohibits future use of one to many relationship of this node in the graph via Matrix updates
//        // and render because it is totally attached to the underlying actor.  Granted when using tvactor nothing short of
//        // implementing X3D Humanoid would prevent this but, just an fyi
//        // actually i suppose we could update this manually using the boneID and getting the matrix and doing the multiply ourselves
//        // then we dont need to use the AttachTo at all.  I should test this, but for now will use this and see..
//        // i mean if we get it working, then its same code for our planet hierarchie too
//        //internal override void AttachTo(CONST_TV_NODETYPE type, int objIndex, int subIndex, bool keepMatrix,
//        //                                bool removeScale)
//        //{
//        //    _actor.AttachTo(type, objIndex, subIndex, keepMatrix, removeScale);
//        //}

//        // what bout adding Meshes via "AttachTo" as a child?  It would have to gain the parent matrix?
//        // crap, Group doesnt inherit from Element3d... so id need to add a new type of group that did that first Group3d or something.
//        // not such a big deal though really.  Actually is fine design wise. Hrm...  The main problem
//        // is that without a special "transform" node, we'd just be hardcoding the way child element3d nodes are hirarchal treated
//        // to move with their parents.
//        // The main advantage though is in the simulation, a pointer to the root Parent is all thats needed to find child
//        // element3d's including meshes and child actor3d elements.  So from simulation you'd get Character.Equip(item, enum_location)
//        // and from that, the simulation would load the proper mesh and add it to the character Actor3d in the scene using
//        // the proper coord offset info about the item. e.g. equip(hat, enum_head) and we'd be able to lookup where that goes
//        // given the current actor model.  And we could update the actor's bounding box to include its child elements
//        // in our Move, MoveRelative etc code, we could update the children at the same time...  In fact, merging
//        // bounding volumes is code we already have.  So its just a question of do we just assume any child mesh is hierarchally 
//        // bound?
//        // Though im still not clear on how we'll track changes in the previous switch?  I suppose the switch itself can do this.
//        // although that would preclude sharing of the switch within the graph. hrm... otherwise we need to track texture/material id's.


//        //public void Attach (Mesh3d m, int boneID)
//        //{
//        //    m.AttachTo(CONST_TV_NODETYPE.TV_NODETYPE_ACTOR, TVIndex, boneID, false,false);
//        //}

//        //public void Attach (Mesh3d m, string boneName)
//        //{
//        //    AddChild(m, _actor.GetBoneID(boneName));
//        //}

//        // obsolete: the following method is incompatible with my Entity system
//        //public void MoveRelative(float front, float up, float right)
//        //{
//        //    _actor.MoveRelative(front, up, right);
//        //}


//        /// <summary>
//        /// ActorMode should never be changed once the underlying actor geometry has been loaded.
//        /// </summary>
//        internal CONST_TV_ACTORMODE ActorMode  
//        {
//            // NOTE: No Setter because we can only set the ActorMode before loading the geometry
//            // and that means either during static Create() or upon deserialization.
//            get { return _actorMode; }
//        }

//        public void ComputeNormals()
//        {
//            _actor.ComputeNormals();
//        }



//        #region OBSOLETE NOW THAT WE USE DUPLICATES
///// <summary>
///// note: SetLightingMode occurs after geometry is loaded because the lightingmode
///// is stored in .tvm, .tva files and so anything you set prior to loading, will be
///// replaced with the lightingmode stored in the file.
///// </summary>
//        internal CONST_TV_LIGHTINGMODE LightingMode
//        {
//            get { return CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED; }
//            set
//            {
//                //_lightingMode = value;
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
//                if (_shader == value) return;

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
//                _actor.SetAlphaTest(value);
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

//#endregion


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

//        public TV_3DMATRIX GetBoneMatrix(int boneID, bool modelSpace)
//        {
//            // i think we should always return in model space 
//            // TODO: delete this bool
//            //  return _duplicates[name].GetBoneMatrix(boneID, modelSpace);
//            // return _actor.GetBoneMatrix (boneID, modelSpace);
//            throw new NotImplementedException();
//        }

//        public void SetBoneMatrix(int boneID, TV_3DMATRIX matrix, bool modelSpace)
//        {
//            //_duplicates[name].TVActor.SetBoneMatrix 
//            //_actor.SetBoneMatrix(boneID, matrix, false, modelSpace);
//            //_actor.SetBoneGlobalMatrix(boneID, matrix);

//            //_actor.SetBoneMatrix(boneID, Matrix, relative, modelSpace);
//            //_actor.SetBoneRotationMatrix(boneID, Matrix, relative);

//        }

//        // obsolete: lookat is entity specific
//        //public Vector3d LookAt
//        //{
//        //    set { _actor.LookAtPoint(Helpers.TVTypeConverter.ToTVVector(value), true); }
//        //}

//        // obsolete: we dont use stencil shadows. Furthermore
//        // shadows are entity specific not actor geometry specific
//        //public override void SetShadowCast(bool enable, bool shadowMapping, bool selfshadows, bool additive)
//        //{
//        //    _actor.SetShadowCast(enable, additive);
//        //}

//        // obsolete: this should be entity specific.  
//        //public override bool IsVisible
//        //{
//        //    get { return _actor.IsVisible(); }
//        //}

//        #region Geometry Members
//        public override CONST_TV_CULLING CullMode
//        {
//            set
//            {
//                _cullMode = value;
//                _actor.SetCullMode(value);
//            }
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

//        public override PickResults AdvancedCollide(Matrix worldMatrix, Vector3d start, Vector3d end, PickParameters parameters)
//        {
//            TV_COLLISIONRESULT result = new TV_COLLISIONRESULT();
//            _duplicates[parameters.ActorDuplicateInstanceID].TVActor.SetMatrix(Helpers.TVTypeConverter.ToTVMatrix(worldMatrix));
//            //_actor.AdvancedCollision(Helpers.TVTypeConverter.ToTVVector(start), Helpers.TVTypeConverter.ToTVVector(end), ref result, parameters.ToTVTestType());
//            _duplicates[parameters.ActorDuplicateInstanceID].TVActor.AdvancedCollision(Helpers.TVTypeConverter.ToTVVector(start), Helpers.TVTypeConverter.ToTVVector(end), ref result, parameters.ToTVTestType());
//            PickResults pr = new PickResults();
//            pr.Result = result;
//            pr.HasCollided = result.bHasCollided;
//            pr.CollidedObjectType = CollidedObjectType.TVGeometry;
//            return pr;
//        }

//        // TODO: for Geometry, AdvancedCollide and Update i think can all be Internal
//        public override void Update(ModeledEntity entityInstance)
//        {
//            // //then we can do during Update (EntityBase entityInstance) we can do ultimately in the Actor.Render(EntityBase entity)
//            // //is if the Entity for which this actor is being rendered is a child entity of another boned entity, then we will grab 
//            // //the entity's _correspondingBones 
//            //if (entityInstance.Parent is BonedEntity)
//            //{

//            //    for (int i = 0; i < ((BonedEntity)entityInstance).BonedModel.Bones.Length; i++)
//            //    {
//            //        // the entity.Name and entity.Parent.Name are used because GetBoneMatrix must reference the proper underlying actor.Duplicate when grabbing the matrix
//            //        _duplicates[entityInstance.Name].SetBoneGlobalMatrix(((BonedEntity )entityInstance).BonedModel.Bones[i],
//            //                                                             ((BonedEntity )entityInstance.Parent).BonedModel.GetBoneMatrix(
//            //                                                                 entityInstance.Parent.Name,
//            //                                                                 ((BonedEntity )entityInstance).CorrespondingParentBones[i],
//            //                                                                 false));
//            //    }
//            //}
//            //_actor.Update();
//            _duplicates[entityInstance.ID].TVActor.Update();
//            // NOTE: I believe that SetChangeFlags should only be done to the particular parent of this Entity and not ALL entities that share this Actor3d.
//            //       TODO: for now i think we'll only do SetChangeFlags in the Entity that references this instead.  
//            //       TODO: Perhaps just utilizing the max bounding volume over any set of animations will make needing SetChangeFlag here irrelevant.
//            // SetChangeFlags(Enums.ChangeStates.KeyFrameUpdated); 
//        }


//        public override void Render(ModeledEntity entityInstance, ModelBase model, Vector3d cameraSpacePosition)
//        {
//            if (_duplicates[entityInstance.ID] == null && TVResourceIsLoaded)
//                RegisterDuplicate(entityInstance.ID);

//            if (_duplicates[entityInstance.ID] == null) return;

//            if (model.Appearance != null)
//            {
//                // do we need array of appearanceHashCode?  Duplicates only
//                // duplicates geometry not the appearances right?  So we would need
//                // seperate appearance hash codes...
//                _duplicates[entityInstance.ID].AppearanceHashCode = entityInstance.Model.Appearance.Apply(_duplicates[entityInstance.ID], entityInstance.AppearanceFlags, _duplicates[entityInstance.ID].AppearanceHashCode);
//            }
//            else
//                _duplicates[entityInstance.ID].AppearanceHashCode = NullAppearance.Apply(_duplicates[entityInstance.ID], entityInstance.AppearanceFlags, _duplicates[entityInstance.ID].AppearanceHashCode);


//            // TODO: Here is the crux of our multiple Actor Update problem.  If we "Update" our actors keyframes in Simulation.Update()
//            //       then TV still invariably needs to Update() the Actor whenever the keyframe changes
//            //       and since we MUST set the underlying actor3d's keyframe for every BonedMondel instance
//            //       on Render() because we are sharing just a single underlying Actor3d then
//            //       it results in one extra unnecessary actor.Update FOR EACH BonedMondel instance!
//            //       This is why the best solution is to modify Actor3d to underthehood use Duplicates() but still
//            //       only use one Actor3d node.
//            // even though we've already updated the animation itself in
//            // our Simulation.Update()  we now just need to restore the resulting animationID and keyframe
//            // so this instance renders appropriately. 
//            // (note: only non keyframe animations (e.g. ragdoll physics) need to restore BoneMatrices.
//            //     AnimationID = ((BonedModel) parent).AnimationID;
//            //     KeyFrame = ((BonedModel) parent).KeyFrame;

//            // ((BonedModel) parent).SetBones();
//            //    this.Update(entityInstance ); // update the TVActor


                
//            // set the matrix and then translate to camera space 
//            // TODO: eventually see if i can't create the matrix and then apply the translation
//            // to the M41, M42, M43 after the fact.  I dont think we should ever have to 
           
//            _duplicates[entityInstance.ID].TVActor.SetMatrix(Helpers.TVTypeConverter.ToTVMatrix(model.Matrix * entityInstance.RegionMatrix));
//            _duplicates[entityInstance.ID].TVActor.SetPosition((float)cameraSpacePosition.x,
//                                                       (float)cameraSpacePosition.y, (float)cameraSpacePosition.z); // apply translation
                        
            
//            try
//            {
//                // for an actor, the bound volume is based on animation and so
//                // is unique per duplicate.  This poses a problem with our current implementation
//                // of hosting _duplicates here.  We would need an array of bounding boxes too
//                // as well as array of IsDirty flags
//                // TODO: i think its pointless to update bounding volume here
//                //if (_duplicates[entityInstance.ID].BoundVolumeIsDirty)
//                //    _duplicates[entityInstance.ID].UpdateBoundVolume();
//                //_actor.Render(false);
//                _duplicates[entityInstance.ID].TVActor.Render( true);
//            }
//            catch
//            {
//                Trace.WriteLine(string.Format("Failed to render '{0}'.", _id));
//            }

//            // TODO: shoudl this script be called against the ENtity from within the Actor?!
//            // that seems wrong.
//            entityInstance.ExecuteScript("OnRender", null);

//        }
//        #endregion


//        #region IBoundVolume Members
//        public override bool BoundVolumeIsDirty
//        {
//            get
//            {
//                throw new Exception("Actor3d.BoundVolumeIsDirty() - Use .BoundVolumeIsDirty(entityID)");
//            }
//        }

//        public override BoundingBox BoundingBox
//        {
//            get
//            {
//                throw new Exception("Actor3d.BoundVolumeIsDirty() - Use .BoundVolumeIsDirty(entityID)");
//            }
//        }

//        public override BoundingSphere BoundingSphere
//        {
//            get
//            {
//                throw new Exception("Actor3d.BoundVolumeIsDirty() - Use .BoundVolumeIsDirty(entityID)");
//            }
//        }
        
//        // From Geometry always return the Local box/sphere
//        // and then have the model's return World box/sphere based on their instance
//        protected override void UpdateBoundVolume()  // TODO: i think all these UpdateBoundVolume's should be protected only
//        {
//            throw new Exception("Actor3d.BoundVolumeIsDirty() - Use .BoundVolumeIsDirty(entityID)");
//        }
//        #endregion


//        #region Actor Specific Bounding Volume functions that accomodate duplicates
//        internal  bool DuplicateBoundVolumeIsDirty(string entityID)
//        {
//            // TODO: shit, last problem i think is that since Actor3d is only attached
//            // to a single Model, and that model can be attached to multiple entities
//            // then it's difficult to notify the entity that bounding volume is dirty
//            // so that it will recalculate it for each seperate actor3d instance.
//            // Ideally... hrm... the problem is that actor duplicates contains so much
//            // per instance information such as antimation state.
//            //
//            // if the underlying resource has not loaded yet, this will be a problem
//            // because we'll have added the key to the _duplicates dictionary
//            // but assigned it to null
//            return _duplicates[entityID].BoundVolumeIsDirty;
//        }

//        public BoundingBox DuplicateBoundingBox(string entityID)
//        {
//            if (_duplicates == null || !_duplicates.ContainsKey(entityID) || _duplicates[entityID] == null)
//                return BoundingBox.Initialized();

//            if (DuplicateBoundVolumeIsDirty(entityID))
//                _duplicates[entityID].UpdateBoundVolume();

//            return _duplicates[entityID].BoundingBox;
//        }

//        public BoundingSphere DuplicateBoundingSphere(string entityID)
//        {
//            if (_duplicates == null || !_duplicates.ContainsKey(entityID) || _duplicates[entityID] == null)
//                return new BoundingSphere(0,0,0,0);

//            if (DuplicateBoundVolumeIsDirty(entityID))
//                _duplicates[entityID].UpdateBoundVolume();

//            return _duplicates[entityID].BoundingSphere;
//        }
//        #endregion
//    }
//}