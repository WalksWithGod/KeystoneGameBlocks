using System;
using System.Diagnostics;
using System.Xml;
using Keystone.Appearance;
using Keystone.Collision;
using Keystone.Entities;
using Keystone.IO;
using Keystone.Resource;
using Keystone.Traversers;
using Keystone.Types;
using MTV3D65;

namespace Keystone.Elements
{
    // July.10.2011 - Minimesh.cs is no longer geometry. (see MinimeshGeometry.cs for that)
	// Minimesh.cs is now just an optional internal part of any Mesh3d as a rendering alternative.
    // Then in our ScaleCuller we can do ((Mesh3d)entity.Geometry).Minimesh.AddInstance();
    // 
    // Custom Minimesh shader with per pixel lighting
    // http://www.truevision3d.com/forums/tv3d_sdk_65/minimesh_lighting-t19807.0.html;msg135808#msg135808
    // defferred using minimesh lights
    // http://www.blitzbasic.com/Community/posts.php?topic=89138
    public class Minimesh2 : IPageableTVNode, IDisposable 
    {
        private readonly object mSyncRoot;
        protected PageableNodeStatus _resourceStatus;
        private int _tvResourceIndex = -1;
        private TVMiniMesh _minimesh;
        private bool _isBillboard;
        private Mesh3d _mesh; // this way things like bounding box for any particular is easy to compute
        private Keystone.Appearance.Appearance _appearance; // this is the appearance used by the underlying StaticEntity and so
                                        // does not need to ever be serialized.
        private int _appearanceHashCode;
        private Shaders.Shader _shader;
        protected CONST_TV_BLENDINGMODE _blendingMode = CONST_TV_BLENDINGMODE.TV_BLEND_NO;
        // we store this because we won't independantly save a Billboard or Mesh3d geometry because it's not how we "LoadTVResource"
        // or maybe we should because I think LoadTVResource does expect Mesh3d to already be loaded...
        private string _meshOrTextureResourcePath;

        //private CONST_TV_BILLBOARDTYPE _rotationMode;
        //private int _billboardHeight;
        //private int _billboardWidth;
        //private bool _billboardCenterHeight;

        private uint _instanceCount;
        private uint _maxInstancesCount;
        private TV_3DMATRIX[] _matrices;
        private TV_3DVECTOR[] _positions;
        private TV_3DVECTOR[] _scales;
        private TV_3DVECTOR[] _rotations;
        private int[] _colorArray;
        private byte[] _enableArray;
        private bool _fadeenabled;
        private bool _fadeUseTV3D;
        private double _fadestart;
        private double _fadedistance;
        private double _fadeDistanceReciprocal;
        private double _fadeEnd;
        private bool _useColor;
        private int _cullMode;

        // each minimesh is tied to a specific Mesh3d node.  No exceptions.
        // Thus a Minimesh is not a node or an IResource.  It has no "id".
        // Also, the Appearance of an instanced mesh cannot be changed.  This is because
        // it is based off the appearance of the mesh used to create it...
        // However, perhaps later if in AddInstanceData we include the appearances, we can
        // compare hashcodes and know whether to change... but even then, we must clone
        // appearances so that our cloned ProceduralShader can compile a new shader specifically
        // for TVMinimesh and not the existing TVMesh that existed in the original Appearance
        public Minimesh2 (Mesh3d mesh, Appearance.Appearance appearance, uint maxInstances)
        {
            // if it's create from TVMesh that we must load first, just call that code first to createa  Mesh3d
            // and we'll create teh mini after that

            // TODO: even this create should pass in a Billoard.cs  Geometry mesh
            // and so again minimesh becomes the instancing alternative renderer.
            // And as far as appearance goes, the Apply in DefaultAppearance will simply
            // call SetMaterial/SetTexture/etc and then Mesh3d or Billboard will see if Minimesh 
            // is being used and set to that one too.


            // TODO: all these rotation mode, heigh/width/etc is already in Billboard.cs 
            // the fade options arent
            if (mesh == null) throw new ArgumentNullException();
            if (mesh.GroupCount != 1) throw new ArgumentOutOfRangeException("Minimeshes can't be created from Meshes with more than one group.");
            _mesh = mesh;
            _cullMode = (int)_mesh.CullMode;
            // NOTE: this appearance is for us to use and was already cloned by FXInstanceRenderer
            _appearance = appearance;
            // NOTE: since the appearance is cloned, no ProceduralShader will exist if the original
            // does not use a shader.  
            if (_appearance != null)
            {
                //_appearance.TargetType = 2; // TargetType is strictly for Shaders to compile in the correct DEFINE (eg GEOMETRY=2) 
				//                            //                 
                Resource.Repository.IncrementRef(_appearance);
            }

            MaxInstancesCount = maxInstances; // TODO: this needs heuristic of some sort to compute?
                                       // or at least dynamic method of changing.
            _resourceStatus = PageableNodeStatus.NotLoaded;
            mSyncRoot = new object();
            
            

        }


        /// <summary>
        /// The ID is used by FXInstanceRenderer to verify only one minimesh is used for instances
        /// of a particular mesh.  So we just return the mesh's id.
        /// </summary>
        public string ID
        {
            get { return _mesh.ID; }
        }
        
        public int LastAppearance { get { return _appearanceHashCode; } }

        public Keystone.Appearance.Appearance Appearance
        {
            set {  _appearance = value; }
        }

    	public virtual void UnloadTVResource()
        {
    		DisposeManagedResources();
    		
    		if (_mesh != null)
	    		Repository.DecrementRef (_mesh);
        }
                
        public void LoadTVResource()
        {
            // it's perfectly ok to try to load the resource for the mesh3d first if it's not
            // since that call as well as this one is taking place on the same worker thread.
            if (_mesh == null) throw new Exception();
            if (_mesh.TVResourceIsLoaded == false) throw new Exception();

            Repository.IncrementRef (_mesh);
            
            if (_minimesh != null)
            {
                _minimesh.Destroy();
            }

            _minimesh = CoreClient._CoreClient.Scene.CreateMiniMesh();

            if (_isBillboard)
            {
                Billboard bb = (Billboard)_mesh;
                _minimesh.CreateBillboard(bb.Width, bb.Height, bb.RotateAtCenter);
            }
            else
            {
                _mesh.LightingMode = CONST_TV_LIGHTINGMODE.TV_LIGHTING_NORMAL;
                _minimesh.CreateFromMesh(CoreClient._CoreClient.Globals.GetMeshFromID(_mesh.TVIndex));
            }
            _minimesh.SetMaxMeshCount((int)_maxInstancesCount);
            _minimesh.EnableFrustumCulling(false);

            // TODO: if the minimesh  blending or alphatest values change, how do we update them?
            // it has to come from the mesh itself and the meshes using instancing should all have same appearance hashcode
            _minimesh.SetCullMode((CONST_TV_CULLING)_cullMode);
            // TODO: do minimesh appearances get changed by appearance node?
     //       _minimesh.SetBlendingMode(_mesh.BlendingMode);
     //       TODO: minimesh alpha test has to come from appearance
     //       _minimesh.SetAlphaTest(_mesh..AlphaTest, _mesh.AlphaTestRefValue, _mesh.AlphaTestDepthWriteEnable);

            _tvResourceIndex = _minimesh.GetIndex();

            // add a color
            _minimesh.SetColorMode(_useColor);
            _minimesh.SetColorArray((int)_maxInstancesCount, _colorArray);
            // bool hardwareInstancing = CoreClient._CoreClient.HardwareInstancing;
            //_minimesh.EnableHardwareInstancing(hardwareInstancing);
            _minimesh.Enable(true);
            
            // TODO: recall that unlike MinimeshGeometry, this Minimesh object was designed to be a Render alternative... still not sure
            //       how useful it is because it requires our RegionPVS add elements during Cull if "UseInstancing" is set and that is rather slow.
            //       We could potentially improve it for static objects if we track the minimesh element index and skip adding if it's already added and
            //       hasn't moved... but that's tricky.  We'd still have to update enable/disable array for elements visible one frame and not visible next
            //       rather than clearing all elements and re-adding 
            // TODO: temp hack to force minimesh shadowmapping shader 
            // TODO: however, i'd prefer is the Model hosting this Minimesh could build the proper string once it knows the shader file path
            // TODO: in other words, when we create the Model, the shader path is assigned to Appearance and the rest of the defines Model can
            //       help with setting such as GEOMETRY_MINIMESH and the NUM_SPLITS_4 can be taken from core.Settings
            string shaderID =  @"caesar\shaders\PSSM\pssm.fx@GEOMETRY_MINIMESH,NUM_SPLITS_4,FORWARD";
            Shader = (Shaders.Shader)Repository.Create (shaderID, "Shader");
        }

        public uint MaxInstancesCount
        {
            get { return _maxInstancesCount; }
            set
            {
                _maxInstancesCount = value;
                _matrices = new TV_3DMATRIX[_maxInstancesCount];
                _positions = new TV_3DVECTOR[_maxInstancesCount];
                _scales = new TV_3DVECTOR[_maxInstancesCount];
                _rotations = new TV_3DVECTOR[_maxInstancesCount];
                _colorArray = new int[_maxInstancesCount];
                Keystone.Types.Color white = new Color (255, 255, 255, 255);
                
                int color = white.ToInt32();
                for (int i = 0; i < _maxInstancesCount; i++)
                    _colorArray[i] = color; // init all colors to white

                _enableArray = new byte[_maxInstancesCount];
            }
        }

        public uint InstancesCount
        {
            get { return _instanceCount; }
        }

        public bool UseColorMode
        {
            get { return _useColor; }
            set 
            {
                _useColor = value;
                if (_minimesh != null)
                    _minimesh.SetColorMode(_useColor);
            }
        }

        public int CullMode
        {
            get { return _cullMode; }
            set
            {
                _cullMode = value;
                if (_minimesh != null)
                    _minimesh.SetCullMode((CONST_TV_CULLING)_cullMode);
            }
        }

        public void Clear()
        {
            _instanceCount = 0;
            // not necessary to clear since we aren't changing size of array
            //Array.Clear(_enableArray, 0, _enableArray.Length);
        }

        /// <summary>
        /// Add a deferred rendering minimesh pointlight.
        /// </summary>
        /// <remarks>
        /// The injection of scale for light range makes this method only good for deferred pointlights.
        /// </remarks>
        /// <param name="light"></param>
        /// <param name="cameraSpacePosition"></param>
        public void AddInstanceData(Lights.PointLight light, Vector3d cameraSpacePosition)
        {
            _appearance = null;  // lights have no appearance
            Vector3d scale = new Vector3d(light.Range, light.Range, light.Range); // light.Scale;

            //Types.Matrix temp = Matrix.Translation(cameraSpacePosition);
            Types.Matrix temp = Matrix.Identity();


            temp.M41 = cameraSpacePosition.x;
            temp.M42 = cameraSpacePosition.y;
            temp.M43 = cameraSpacePosition.z;

            temp.M11 = light.Range;
            temp.M22 = light.Range;
            temp.M33 = light.Range; // shader hack.  We will read light range from the worldmatrix[2][2] for that minimesh element  

            lock (_matrices)
            {
                _matrices[_instanceCount] = Helpers.TVTypeConverter.ToTVMatrix(temp);

                _enableArray[_instanceCount] = 1;
                if (_useColor)
                    _colorArray[_instanceCount] = light.Diffuse.ToInt32();

                _instanceCount++;
            }
        }
        // TODO: AddInstanceData perhaps should sort into seperate buckets based on
        // appearance hashcode?
        // And what about buckets for CSG Sources vs Targets?
        // Actually, we can add them all to same minimesh, we just need a way to
        // store appearances, and know which enableArrays to to use... 
        // the rest of the arrays dont matter
        //public void AddInstanceData(Matrix mat, Vector3d cameraSpacePosition, Vector3d scale, Quaternion rotation)
        public void AddInstanceData (ModeledEntity entity, Model model, Vector3d  cameraSpacePosition)
        {
        	// TODO: WARNING WARNING WARNING WARNING
        	// if attempting to use this in multiple pass rendering (eg depth + N split shadowmaps, deferred etc) then currently
        	// AddInstanceData gets called again during "ScaleDrawer.Render()" as it iterates pvs regions and draws buckets.
        	// but obviously we do not want that.  
        	        	
        	// TODO: before a normal TVMesh is drawn, Model.Render() is called and is used to compute a proper override matrix
        	//       if necessary.  Minimesh2 needs thta too if it's to serve as a general purpose batch renderer.  
        	//       
 //       	Matrix matrixOverride = Model.GetCameraSpaceWorldMatrixOverride (context, entity, cameraSpacePosition);
        	
            // TODO: asteroid minimeshes as an example will be unlit using our solar system star pointlight
            //       unless we use a custom shader on the minimesh i suspect.. or did we forget
            // to set the lighting mode?
            //this._mesh.LightingMode = CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED;
            _appearance = model.Appearance;

            Matrix mat = model.RegionMatrix;// entity.RegionMatrix;

            lock (_matrices)
            {
#if DEBUG
                if (_instanceCount == _maxInstancesCount) return; // throw new ArgumentOutOfRangeException("Max instances reached.  You can increase this by setting _maxInstancesCount to a higher value");

                //if (scale == Vector3d.Zero()) throw new ArgumentNullException("Minimesh.AddInstanceData() - scale should not be null");
#else
            	if (_instanceCount == _maxInstancesCount) return;
            	//if (scale == Vector3d.Zero()) return;
#endif
                TV_3DMATRIX  temp = Helpers.TVTypeConverter.ToTVMatrix(mat);

                temp.m41 = (float)cameraSpacePosition.x;
                temp.m42 = (float)cameraSpacePosition.y;
                temp.m43 = (float)cameraSpacePosition.z;
                _matrices[_instanceCount] = temp;


                //    _positions[_instanceCount] = Helpers.TVTypeConverter.ToTVVector(position);
                //     _scales[_instanceCount] = Helpers.TVTypeConverter.ToTVVector(scale);
                // because of tv3d design, you can't set rotationMatrixArray or SetMatrixArray and probalby not individual rotations
                // if you want your rotation mode to work. I think it's a bug, Sylvain says it's by design but I say if you have to load 
                // a minimesh from a file, then you can't just store/restore the entire matrix you have to have seperate code to only fill
                // apply the rotation is the RotationMode is none
                if (!_isBillboard || ((Billboard)_mesh).BillboardType == CONST_TV_BILLBOARDTYPE.TV_BILLBOARD_NOROTATION)
                {
                    //_rotations[_instanceCount] = Helpers.TVTypeConverter.ToTVVector(rotation);
                }
                _enableArray[_instanceCount] = 1;
                //_matrices[_instanceCount - 1].m11 = (float)position.x;

                _instanceCount++;
            }
        }

        //     MTV3D65.Vector3d _lastCamPosition ; // these may be bad considering our multiple culling stages for reflection and regular?

        //            //TODO: like our LODSwitch, this can maybe use an ScaleInterpolator node.
        //    public void UpdateScales(MTV3D65.Vector3d camPosition)
        //    {
        //        if (!camPosition.Equals(_lastCamPosition)) {
        //            _lastCamPosition = camPosition;
        //            //  TODO: i could also use the _tmpEnable and then set the ones which scale to 0,0,0 to disabled... see if that speeds things up;
        //            _scaleArray.CopyTo(_tmpScale, 0);
        //            for (Int32 i = 0; i<= (_count - 1); i++)
        //            {
        //                double distanceToCamera = TV3DUtility.TVMath.GetDistance2D(PositionArray(i).x, PositionArray(i).z, camPosition.x, camPosition.z);
        //                if ((distanceToCamera <= _fadestart)) {
        //                    //  _tmpScale(i) = _scale(i) * 1
        //                }
        //                else if ((distanceToCamera <= _fadeEnd)) {
        //                    _tmpScale(i) = (_scaleArray(i) 
        //                                    * (((_fadedistance - distanceToCamera) 
        //                                        + FadeStart) 
        //                                       * _fadeDistanceReciprocal));
        //                }
        //                else {
        //                    _tmpScale(i) = new Vector3d(0, 0, 0);
        //                }
        //            }
        //            _minimesh.SetScaleArray(_count, _tmpScale);
        //        }
        //    }

        /// <summary>
        /// Note: CONST_TV_BLENDINGMODE.TV_BLEND_ALPHA is required when using alpha 
        /// component in the diffuse of material to enable Alpha Transparency.
        /// Note: When alpha blending is enabled in the hardware, some optimizations 
        /// such as zbuffer pixel rejections in the pixel shader are disabled.
        /// </summary>
        internal CONST_TV_BLENDINGMODE BlendingMode
        {
            //get { throw new NotImplementedException(); }
            set
            {
                _blendingMode = value;
                _minimesh.SetBlendingMode(value);
            }
        }

        internal int GetMaterial()
        {
            return _minimesh.GetMaterial();
        }

        internal void SetMaterial(int tvMaterialID)
        {
            _minimesh.SetMaterial(tvMaterialID);
        }

        internal int GetTexture()
        {
            return _minimesh.GetTexture();
        }
        internal void SetTexture(int tvTextureID)
        {
            _minimesh.SetTexture(tvTextureID);
        }

        internal void SetTexture(int layer, int tvTextureID)
        {
            _minimesh.SetTextureEx(layer, tvTextureID);
        }

        internal Shaders.Shader Shader
        {
            get { return _shader;}
            set
            {                
                if (_shader == value && ( value == null || value.PageStatus == PageableNodeStatus.Loaded)) return;

                if (value != null)
                {
                    // also we don't want to assign this shader if it's underlying tvshader is not yet paged in.
                    if (value.PageStatus != PageableNodeStatus.Loaded)
                    {
                        //System.Diagnostics.Debug.WriteLine("Minimesh2.Shader '" + value.ID + "' not loaded.  Cannot assign to Mesh '" + _id + "'");
                        return;
                    }
                }
        
                // NOTE: In our ImportLib and WavefrontObjLoader, if there is only 1 group in the model
                // we add materials, shaders, and textures directly to the defaultappearance and add
                // NO GroupAttribute children.
                _shader = value;
                if (_shader == null)
                    _minimesh.SetShader(null);
                else
                    _minimesh.SetShader(_shader.TVShader);
            }
        }
        //internal void SetShader (TVShader shader )
        //{
        //    _minimesh.SetShader (shader);
        //}
        //internal TVShader GetShader()
        //{
        //    return _minimesh.GetShader();
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityInstance"></param>
        /// <param name="model"></param>
        /// <param name="cameraSpacePosition">Not used.  InstanceData is added with the cameraSpacePosition passed in there.</param>
        internal void Render()
        {
			lock (mSyncRoot)
            {
            // http://www.truevision3d.com/forums/tv3d_sdk_65/minimeshes_missing_functions-t18182.0.html;msg125019#msg125019

            // TODO: how is our FXInstanceRenderer using the proper large rendering/near rendering and
            // region infos?  it doesnt seem to be at all...
            if (_instanceCount == 0 || _resourceStatus != PageableNodeStatus.Loaded) return;

            //System.Diagnostics.Debug.WriteLine("Minmesh.Render() - Instances count = " + _instanceCount.ToString());

            // since the same geometry can be rendered with different textures
            // we will need to change the textures + materials + shaders if applicable.
            // However what we will try to do is ensure that all of the entities which use a 
            // specific combination all get rendered as a single batch.
            if (_appearance != null) 
            {
                _appearanceHashCode = _appearance.Apply(this, 0);
            }
            else
                _appearanceHashCode = NullAppearance.Apply(this, _appearanceHashCode);


			// TODO: if no change, we should not SetEnableArray again
            _minimesh.SetEnableArray((int)_maxInstancesCount, _enableArray);

            //_minimesh.SetGlobalPosition();
            // because of tv3d design, you can't set rotationMatrixArray or SetMatrixArray and probalby not individual rotations
            // if you want your rotation mode to work. I think it's a bug, Sylvain says it's by design but I say if you have to load 
            // a minimesh from a file, then you aren't able to just store/restore the entire matrix you have to have seperate code to only fill
            // apply the rotation if the RotationMode is none

            //if (_rotationMode ==  CONST_TV_BILLBOARDTYPE.TV_BILLBOARD_NOROTATION ) 
            // Our matrices in the matrix array for each element are camera space whereas the
            // minimesh itself is always at origin.
            // TODO: if no change, we should not SetMatrixArray() again
            _minimesh.SetMatrixArray((int)_instanceCount, _matrices);
            // NOTE: _minimesh.SetMatrix() <-- if setting the general matrix
            //       any per element scales (and possibly rotations and positions too) get wiped out!
            //       and so must use _minimesh.SetGlobalPosition() 

            //      _minimesh.SetPositionArray((int)_instanceCount, _positions);
            // TODO: If the ModelBase is scaled, we must also scale each matrix array element or the scale array scales
            //          No, thats not true.  The Entity itself should have a RegionMatrix that contains the model's scaling as well.
            //          Recall the logic is that our Model is actually an instance container for our Mesh3d which is just a TVMesh wrapper
            //          and our Entity is host of model, behavior, physics, script logic, etc.
            //       _minimesh.SetScaleArray((int)_instanceCount, _scales);


            // because of tv3d design, you can't set rotationMatrixArray or SetMatrixArray and probalby not individual rotations
            // if you want your rotation mode to work. I think it's a bug, Sylvain says it's by design but I say if you have to load 
            // a minimesh from a file, then you can't just store/restore the entire matrix you have to have seperate code to only fill
            // apply the rotation is the RotationMode is none
            if (_isBillboard && ((Billboard)_mesh).BillboardType != CONST_TV_BILLBOARDTYPE.TV_BILLBOARD_NOROTATION)
            {
                _minimesh.SetRotationMode(((Billboard)_mesh).BillboardType); // TODO: should only change when dirty
                _minimesh.SetRotationArray((int)_instanceCount, _rotations );
            }


            // TODO: temp hack set the shader point light stuff

          //  _minimesh.GetShader().SetEffectParamFloat("Radius", 100);
          //  _minimesh.GetShader().SetEffectParamVector3("LightPos", new TV_3DVECTOR(0, 25, 0));

            _minimesh.Render();
			}
        }


        #region IPageableTVNode Members
        public object SyncRoot { get { return mSyncRoot; } }

        public bool TVResourceIsLoaded { get { return _tvResourceIndex >= 0; } }
        public int TVIndex
        {
            get
            {
                if (_minimesh == null) return -1;
                else return _minimesh.GetIndex(); 
            }
        }

        public string ResourcePath
        {
            get
            {
                return ID;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public PageableNodeStatus PageStatus
        {
            get
            {
                return _resourceStatus; ;
            }
            set
            {
                _resourceStatus = value;
            }
        }

        public void SaveTVResource(string filepath)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IDisposable Members
        private bool _disposed;
        ~Minimesh2()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) here is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // pass true if managed resources should be disposed. Otherwise false.
        private void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    DisposeManagedResources();
                    _disposed = true;
                }
                DisposeUnmanagedResources();
            }
        }

        protected virtual void DisposeManagedResources()
        {
            //note: when a node is disposed, it's reference count should be down to 0 at this point
            // because when the ref count reaches 0, there should be no more references to the node
            // unless they are held by the original caller and they shouldnt be.  
            //
            // IMPORTANT: Thus the Repository will call .Dispose() on the object and the 
            //  overriden DisposeManaged/UnmangedResoruces should call an appropriate tv....Destroy() on any TV3D resource
            // thus if there are any lingering references, they will be invalid so it is the Creator's responsibility to manually
            // increment the reference count of this object if they dont want it to be disposed.

            //But there's no way to enforce
            // that.  In normal file save/load this can't happen, but when the user manually imports/creates from a static load procedure
            // then there is no way to guarantee that so we leave those resources loaded.  There's no harm in that when we unload a scene
            // only when we want to shut down the engine completely.


            // Note: Minimesh is not a Node and thus no Repository.Increment/Decrement ref applies
            try
            {
                if (_appearance != null)
                    Resource.Repository.DecrementRef(_appearance);

                if (_minimesh != null)
                    _minimesh.Destroy();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Minimesh.DisposeUnmanagedResources() - " + ex.Message);
            }
        }

        protected virtual void DisposeUnmanagedResources()
        {

        }

        protected void CheckDisposed()
        {
            if (IsDisposed) throw new ObjectDisposedException(this.GetType().Name + " is disposed.");
        }

        protected bool IsDisposed
        {
            get { return _disposed; }
        }
        #endregion
    }

}