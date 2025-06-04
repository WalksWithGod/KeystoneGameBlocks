using System;
using System.Collections.Generic;
using Keystone.Collision;
using Keystone.Entities;
using Keystone.Enum;
using Keystone.IO;
using Keystone.Shaders;
using Keystone.Traversers;
using Keystone.Types;
using MTV3D65;

namespace Keystone.Elements
{
    // note: This doesn't inherit from BoundGroup because this node type has no
    // need to be derived from Transform.cs
    public abstract class Geometry : BoundNode, IPageableTVNode
    {
        protected readonly object mSyncRoot;
        // TODO: why arent these in flags
        protected int _vertexCount;
        protected int _groupCount;
        protected int _triangleCount;
        protected CONST_TV_MESHFORMAT _meshFormat;

        //TODO: the following i wonder if its appropriate to have here and on top of that
        // whether this is how we would want to implement shadows for stencils here anyway.
        // i mean our ShadowMapping controls all of these things for us.  We could conceivably
        // set that up to handle stencil on/off too... or frankly, just dont ever uses stencils.
        // and terrain obviously doesnt need any of this nor do particle systems
        //protected bool _shadowMapping;   // set in Model now as ReceiveShadow and CastShadow
        //protected bool _shadowsEnabled;  // set in Model now as ReceiveShadow and CastShadow
        protected bool _additiveShadows;
        protected bool _selfShadowsEnabled;


        // NOTE: alpha, blending, culling modes are NOT serialized to XML.  That only occurs
        // in the Appearance node.  Here these values only serve as runtime hashcode / caching for comparison against appearance nodes
        //TODO: this next set should be for Actors and Meshes (Billboards are a typeof tvmesh)and MiniMeshes (not used in particles or landscape)
        // but.  Particle systems though should be something different as it does not need these things. 
        protected bool _alphaTestEnable = false;
        protected int _alphaTestRefValue = 0;
        // depth buffer write so that our mesh doesnt get blended out by other objects (e.g terrain)
        protected bool _alphaTestDepthBufferWriteEnable = true;
        protected int _cullMode = (int)CONST_TV_CULLING.TV_BACK_CULL; // TODO: appearance should set this and we cache to compare changes between shared instances of the geometry and when to update


        protected Shader _shader;
        protected int _appearanceHashCode;
        protected int _tvfactoryIndex = -1;

        protected PageableNodeStatus _resourceStatus;
        
        
        protected Geometry(string resourcePath)
            : base(resourcePath)
        {
            SetChangeFlags(Enums.ChangeStates.All, Enums.ChangeSource.Self);
            Shareable = true;
            _cullMode = (int)CONST_TV_CULLING.TV_BACK_CULL;
            mChangeStates = Keystone.Enums.ChangeStates.BoundingBoxDirty; // default starting state for all geometry.. this is really a hack for cases where i generate a primitive and so the IPageable.LoadTVResource is skipped
            mSyncRoot = new object();
        }

        #region ITraversable Members
        public override object Traverse(ITraverser target, object data)
        {
            return target.Apply(this, data);
        }
        #endregion

        #region IResource Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="specOnly">True returns the properties without any values assigned</param>
        /// <returns></returns>
        public override Settings.PropertySpec[] GetProperties(bool specOnly)
        {
            Settings.PropertySpec[] tmp = base.GetProperties(specOnly);
            Settings.PropertySpec[] properties = new Settings.PropertySpec[2 + tmp.Length];
            tmp.CopyTo(properties, 2);

            properties[0] = new Settings.PropertySpec("meshformat", typeof(int).Name);
            properties[1] = new Settings.PropertySpec("cullmode", typeof(int).Name);
            // obsolete - moved to appearance
            //properties[1] = new Settings.PropertySpec("blendingmode", typeof(int).Name);

            if (!specOnly)
            {
                properties[0].DefaultValue = (int)_meshFormat;
                properties[1].DefaultValue = _cullMode;
            }

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
            		case "meshformat":
                        _meshFormat = (CONST_TV_MESHFORMAT)(int)properties[i].DefaultValue;
                        break;
                    case "cullmode":
                        // NOTE: the Entity plugin editor only supports double sided or backface culling for now.
                        CullMode = (int)properties[i].DefaultValue;
                        break;
                    // obsolete - moved to appearance
                    //case "blendingmode":
                    //    _blendingMode = (CONST_TV_BLENDINGMODE)properties[i].DefaultValue;
                    //    break;
                }
            }
        }
        #endregion

        #region IPageableTVNode Members
        public object SyncRoot { get { return mSyncRoot; } }

        public int TVIndex
        {
            get { return _tvfactoryIndex; }
        }

        public bool TVResourceIsLoaded
        {
            get { return _tvfactoryIndex > -1; }
        }

        public PageableNodeStatus PageStatus { get { return _resourceStatus; } set { _resourceStatus = value; } }
        public abstract void LoadTVResource();
        public abstract void UnloadTVResource();
        public abstract void SaveTVResource(string filepath);

        public string ResourcePath
        {
            get { return _id; }
            set { _id = value; }
        }

        #endregion

        protected override void PropogateChangeFlags(global::Keystone.Enums.ChangeStates flags, Enums.ChangeSource source)
        {
        	Keystone.Enums.ChangeStates filter = Keystone.Enums.ChangeStates.Translated |
                Keystone.Enums.ChangeStates.BoundingBox_TranslatedOnly |
                Keystone.Enums.ChangeStates.BoundingBoxDirty |
                Keystone.Enums.ChangeStates.GeometryAdded |
                Keystone.Enums.ChangeStates.GeometryRemoved |
                Keystone.Enums.ChangeStates.KeyFrameUpdated |
                Keystone.Enums.ChangeStates.MatrixDirty |
                Keystone.Enums.ChangeStates.RegionMatrixDirty;
        	
        	Keystone.Enums.ChangeStates filteredFlags = flags & filter;
        	
            if (filteredFlags != 0)
            {
                
        		// if the source of the flag is a child or self, notify parents
                if (mParents != null && (source == Enums.ChangeSource.Child || source == Enums.ChangeSource.Self))
                    NotifyParents(filteredFlags);
            }

            // OBSOLETE - never use switch for testing for certain flags.
            //switch (flags)
            //{
                
            //    case Enums.ChangeStates.Translated:
            //    case Keystone.Enums.ChangeStates.BoundingBox_TranslatedOnly:
            //    case Enums.ChangeStates.BoundingBoxDirty :
               

            //    case Enums.ChangeStates.GeometryAdded:
            //    case Enums.ChangeStates.GeometryRemoved:
            //    case Enums.ChangeStates.KeyFrameUpdated:
            //    case Enums.ChangeStates.MatrixDirty:
            //    case Enums.ChangeStates.RegionMatrixDirty:
            //    default:
            //        // if the source of the flag is a child or self, notify parents
            //        if (_parents != null && source == Enums.ChangeSource.Child || source == Enums.ChangeSource.Self)
            //            NotifyParents(flags);

            //        break;
            //}
        }


        internal override ChildSetter GetChildSetter()
        {
            throw new NotImplementedException();
        }

        internal int LastAppearance
        {
            get { return _appearanceHashCode; }
            set { _appearanceHashCode = value; }
        }

        //TODO: the following should all be Appearance last used cached values for dirty updates only?
        //      and so all ideally should reside in the Appearance classes yes?
        // Also the bools should be in bitflags and the AlphaTest should be a function
        // that is based on byte AlphaTestRef if that = 0 then return false.
        internal virtual Shader Shader
        {
            get { return _shader; }
            set { _shader = value; }
        }

        // TODO: obsolete i think because now we don't render Overlay items by setting this property on geometry
        // instead we disable z compares and write overlay items last.  We only need check if
        // Overlay is enabled on the Model.  We also check Overlay for picking sorting.
        //internal virtual bool Overlay 
        //{
        //    set { throw new NotImplementedException(); }
        //}


        internal virtual void SetAlphaTest (bool enable, int iGroup)
        {    
			        	
        }

        /// <summary>
        /// Note: CONST_TV_BLENDINGMODE.TV_BLEND_ALPHA is required when using alpha 
        /// component in the diffuse of material to enable Alpha Transparency.
        /// Note: When alpha blending is enabled in the hardware, some optimizations 
        /// such as zbuffer pixel rejections in the pixel shader are disabled.
        /// </summary>
        /// <remarks>
        /// must be overridden by derived types because Appearance nodes 
        /// use this to Apply BlendingMode to Geometry.
        /// </remarks>
        internal virtual CONST_TV_BLENDINGMODE BlendingMode
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        /// <remarks>
        /// must be overridden by derived types because Appearance nodes 
        /// use this to Apply BlendingMode to Geometry.
        /// </remarks>
        internal virtual void SetBlendingMode (CONST_TV_BLENDINGMODE blendingMode, int group)
        {
            throw new NotImplementedException ();
        }

        // TODO: once this is added to Appearance, then TextureClampingy can be made
        // "internal" access where only Appearance/GroupAppearance can modify
        public virtual bool TextureClamping  
        { 
            set 
            {
                throw new NotImplementedException();
            } 
        }
        // Recall what Sylvain said about the "keepMatrix."  It only applies one time on the initial AttachTo
        // call.  keepMatrix=true causes the existing matrix to be subtracted from the parent's to create the new relative matrix
        // otherwise the relativeMatrix just starts at identity and so is at origin of parent
        // valid nodetypes = actor, camera, light, mesh, node, none, particleemitter
        // OBSOLETE - We only use AddChild() never AttachTo.
        // For BonedEntities, we have extra AddChild() that allow for boneID to be specified
        //internal abstract void AttachTo(CONST_TV_NODETYPE type, int objIndex, int subIndex, bool keepMatrix,
        //                                bool removeScale);


        // TODO: once this is added to Appearance, then TextureClampingy can be made
        // "internal" access where only Appearance/GroupAppearance can modify
        public virtual int CullMode
        {
            get { return _cullMode; }
            set { throw new NotImplementedException(); }
        }

        public virtual int GetVertexCount(uint groupIndex)
        {
            throw new NotImplementedException();
        }

        public virtual object GetStatistic(string name)
        {
            switch (name.ToUpper())
            {
                case "VERTEXCOUNT":
                case "VERTICES":
                    return VertexCount;
                
                case "TRIANGLES":
                case "TRIANGLECOUNT":
                    return TriangleCount;
                    
                case "GROUPS":
                case "GROUPCOUNT":
                    return GroupCount;

                case "DEPTH":
                    if (_box == BoundingBox.Initialized()) return 0;
                    return _box.Depth;
                case "WIDTH":
                    if (_box == BoundingBox.Initialized()) return 0;
                    return _box.Width;
                    
                case "HEIGHT":
                    if (_box == BoundingBox.Initialized()) return 0;
                    return _box.Height;

                case "CENTEROFFSET":
                    if (_box == BoundingBox.Initialized()) return Vector3d.Zero();
                    return _box.Center;

                default :
                    return null;
            }
        }

        public virtual int VertexCount
        {
            get { throw new NotImplementedException(); }
        }

        public virtual int GroupCount
        {
            get { throw new NotImplementedException(); }
        }

        public virtual int TriangleCount
        {
            get { throw new NotImplementedException(); }
        }

        internal abstract PickResults AdvancedCollide(Vector3d start, Vector3d end, KeyCommon.Traversal.PickParameters parameters);
        // No longer need Update() in any Geometry node- MPJ April.27.2012
        // internal abstract void Update(ModeledEntity entityInstance);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="worldMatrix"></param>
        /// <param name="scene">Usually Context.Scene rather than Entity.Scene since AssetPlacementTool preview Entity's are not connected to Scene.</param>
        /// <param name="model"></param>
        /// <param name="elapsedSeconds"></param>
        internal abstract void Render(Matrix worldMatrix, Keystone.Scene.Scene scene, Model model, double elapsedSeconds);
        
    }
}