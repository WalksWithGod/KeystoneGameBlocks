using System;
using System.Collections.Generic;
using System.Diagnostics;

using Keystone.Appearance;
using Keystone.Collision;
using Keystone.Entities;
using Keystone.Extensions;
using Keystone.IO;
using Keystone.Resource;
using Keystone.Types;
using MTV3D65;

namespace Keystone.Elements
{

    public class MinimeshGeometry : Geometry
    {
        private TVMiniMesh _minimesh;
        private Mesh3d _mesh; // this way things like bounding box for any particular is easy to compute

        private Appearance.Appearance _appearance;
        
        // we store _meshOrTextureResourcePath because the Mesh3d or Texture (for billboard textured minis) will use the
        // path for it's id.  But maybe this is a bad idea?  I think if this minimesh uses a naming convention
        // based on it's Structure, then we should be ok.  Because we want each zone to manage it's own minimeshes.
        // This gives us reasonably good degree of culling for non visible zones.  So if this is the case where minis are
        // assigned to structure, then surely we need a way to grab the Mesh3d and create a Mini for it within the Structure instead?
        private string _meshOrTextureResourcePath;
        private bool _isBillboard;
        private bool mAxialRotation;
        private CONST_TV_BILLBOARDTYPE _billboardtype;
        private float _billboardHeight;
        private float _billboardWidth;
        private bool _billboardCenterHeight;

        private bool _alphaSort = false;
        private bool _useColors = false;
        private bool mPreCacheMaxElements = false;
        
        // array elemnt data
        private uint mInstanceCount;
        private uint _maxInstancesCount;
        private Vector3d[] _rotationArray;
        private Vector3d[] _positionArray;
        private Vector3d[] _scaleArray;
        private int[] mColorArray;
        private int[] _tagArray;   // can be used to store a structure TileID
        private byte[] mEnableArray;
        private bool[] mDisposedElements;
        
 		// NOTE: SetMatrixArray() prevents SetGlobalPosition() from working.  This is because
    	// SetMatrixArray() sets global matrices and not modelspace matrices.  Thus, 
    	// SetMatrixArray() matrices MUST include any regional & cameraSpace offsets for each element!
    	// The _good aspect of this_ is that we can perform Axial Rotation matrix calculations
    	// in camera space without having to worry about the global matrix of the minimesh.
        private Matrix[] _matrixArray;


                
        private bool mEnableArrayChanged;
        private bool mColorsArrayChanged;
		private bool mPositionArrayChanged;
		private bool mScaleArrayChanged;
		private bool mRotationArrayChanged;
		private bool mMatrixArrayChanged;
		
        private bool _fadeenabled;
        private bool _fadeUseTV3D;
        private double _fadestart;
        private double _fadedistance;
        private double _fadeDistanceReciprocal;
        private double _fadeEnd;



        private object mSyncLock;
        
        // version that doesnt require a TVMiniMesh or filepath so that we can load the xmlnode and set the tvresource
        // load state as unloaded so that a traverser can load it later
        internal MinimeshGeometry(string id)
            : base(id)
        {
        	mSyncLock = new object();
        	_isBillboard = false;
        	

        	_billboardtype = CONST_TV_BILLBOARDTYPE.TV_BILLBOARD_NOROTATION;
        	// March.6.2015 - setting max instance count ONE TIME is ideal.  Having to grow the minimesh
        	// at runtime is not only super expensive and slows loading emmensly, but it seems to cause
        	// random crashes.  We may need to enforce that the max count is set and is never changeable
        	 _maxInstancesCount = 1024 * 1;
        }

        private MinimeshGeometry(string id, Texture texture)
            : this(id)
        {
            _appearance = null; // TODO: not doing anything with texture yet!
        }

        private MinimeshGeometry(string id, string resourcePath, uint maxCount)
            : this(id)
        {
            System.Diagnostics.Debug.Assert(id != resourcePath, "MinimeshGeometry.Ctor() - Mesh3d.ID is resource path so cannot match MinimeshGeometry node's ID which can be a generic id.");

            // NOTE: The actual mesh3d and mini will get created during LoadTVResource()
            _appearance = null;
            _mesh = null;
            _maxInstancesCount = maxCount;
            _meshOrTextureResourcePath = resourcePath;
        }

#region obsolete 
        // OBSOLETE: Cannot pass a Mesh3d to MinimeshGeometry.ctor() because
        // it results in bad habits and mistakes.
        // Must pass in a mesh path, but not actual mesh.  we should always load mesh here
//        private MinimeshGeometry(string id, Mesh3d mesh, Appearance.Appearance app)
//            : this(id, mesh, 5000, app)
//        {
//        }
//
//        private MinimeshGeometry(string id, Mesh3d mesh, uint maxCount, Appearance.Appearance app)
//            : this(id)
//        {
//            System.Diagnostics.Debug.Assert(id != mesh.ID, "MinimeshGeometry.Ctor() - Mesh3d.ID is resource path so cannot match MinimeshGeometry node's ID which can be a generic id.");
//
//            // NOTE: The actual mini will get created during LoadTVResource()
//            // however _mesh's own LoadTVResource must be true by then.
//            _appearance = app;
//            _maxInstancesCount = maxCount;
//            _mesh = mesh;
//            _meshOrTextureResourcePath = mesh.ResourcePath;
//
//            if (_minimesh == null)
//            {
//            	Keystone.IO.PagerBase.LoadTVResourceSynchronously (this, false);
//            }
//                // hack
//            // TODO: texture and material will only get applied properly if a TVMesh is loaded with loadTextures and loadMaterials = true
//            // otherwise no Appearance.Apply() is currently set to occur and im not 100% how to implement that since InstancedModel
//            // ultimately needs to do Model.Minimesh.SetTexture and .SetMaterial i believe
//            //_minimesh.SetTexture( 2);
//            //_minimesh.SetMaterial(Material.Create(Material.DefaultMaterials.red).TVIndex);
//            //    m.SetFadeOptions(True, 600, 150) ', 1200, 256)
//            //m.SetAlphaSort(True)
//            //m.SetCullMode(CONST_TV_CULLING.TV_DOUBLESIDED)
//            //m.SetBlendingMode(CONST_TV_BLENDINGMODE.TV_BLEND_ALPHA)
//            //m.SetAlphaTest(False, 128)
//        }

//        public static MinimeshGeometry Create(string id, Mesh3d mesh3d, uint maxCount, Appearance.Appearance app)
//        {
//            lock (Repository._cache)
//            {
//                System.Diagnostics.Debug.Assert(id != mesh3d.ID, "MinimeshGeometry.Create() - Mesh3d.ID is resource path so cannot match MinimeshGeometry node's ID which can be a generic id.");
//                return new MinimeshGeometry(id, mesh3d, maxCount, app);
//            }
//        }
#endregion



		// TODO: THESE STATIC CREATION METHODS ARE NOT THREAD SAFE WITH REPOSITORY.
        ///// <summary>
        ///// For Billboard minimeshes since they do not require an underlying TVMesh
        ///// </summary>
        ///// <param name="id"></param>
        ///// <param name="billboardTexturePath"></param>
        ///// <param name="width"></param>
        ///// <param name="height"></param>
        ///// <param name="centerHeight"></param>
        ///// <returns></returns>
        //public static MinimeshGeometry Create(string id, string billboardTexturePath, int width, int height, bool centerHeight)
        //{
        //    MinimeshGeometry minimesh = (MinimeshGeometry)Repository.Get(id);
        //    if (minimesh != null) return minimesh;

        //    // if it's create from TVMesh that we must load first, just call that code first to createa  Mesh3d
        //    // and we'll create teh mini after that

        //    Layer texture = Diffuse.Create(billboardTexturePath);

        //    minimesh = new MinimeshGeometry(id, texture);
        //    minimesh._billboardWidth = width;
        //    minimesh._billboardHeight = height;
        //    minimesh._billboardCenterHeight = centerHeight;
        //    minimesh._meshOrTextureResourcePath = billboardTexturePath;
        //    return minimesh;
        //}



        #region ITraversable Members
        public override object Traverse(Keystone.Traversers.ITraverser target, object data)
        {
            return target.Apply(this, data);
        }
        #endregion 

        #region ResourceBase members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="specOnly">True returns the properties without any values assigned</param>
        /// <returns></returns>
        public override Settings.PropertySpec[] GetProperties(bool specOnly)
        {
            Settings.PropertySpec[] tmp = base.GetProperties(specOnly);
            Settings.PropertySpec[] properties = new Settings.PropertySpec[9 + tmp.Length];
            tmp.CopyTo(properties, 9);

            // precache == true will result in all minimesh elements being created ahead of time with each element disabled until needed.
            properties[0] = new Settings.PropertySpec("precache", typeof(bool).Name);
            properties[1] = new Settings.PropertySpec("billboardwidth", _billboardWidth.GetType().Name);
            properties[2] = new Settings.PropertySpec("billboardheight", _billboardHeight.GetType().Name);
            properties[3] = new Settings.PropertySpec("centerheight", _billboardCenterHeight.GetType().Name);
            properties[4] = new Settings.PropertySpec("meshortexresource", typeof(string).Name);
            properties[5] = new Settings.PropertySpec("isbillboard", typeof(bool).Name);
            properties[6] = new Settings.PropertySpec("usecolors", typeof(bool).Name);
            properties[7] = new Settings.PropertySpec ("alphasort", typeof(bool).Name);
            properties[8] = new Settings.PropertySpec("maxcount", _maxInstancesCount.GetType().Name);


            // TODO: hull isnt necessary for a minimesh which ive decided is only used for instance rendereing.
            //    Hull = new ConvexHull(_filePath);
            //    Hull = ConvexHull.ReadXml(xmlNode);
            if (!specOnly)
            {
                properties[0].DefaultValue = mPreCacheMaxElements;
                properties[1].DefaultValue = _billboardWidth;
                properties[2].DefaultValue = _billboardHeight;
                properties[3].DefaultValue = _billboardCenterHeight;
                properties[4].DefaultValue = _meshOrTextureResourcePath;
                properties[5].DefaultValue = _isBillboard;
                properties[6].DefaultValue = _useColors;
                properties[7].DefaultValue = _alphaSort;
                properties[8].DefaultValue = _maxInstancesCount;
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
                    case "precache":
                        mPreCacheMaxElements = (bool)properties[i].DefaultValue;
                        break;
                    case "billboardwidth":
                        _billboardWidth = (float)properties[i].DefaultValue;
                        break;
                    case "billboardheight":
                        _billboardHeight = (float)properties[i].DefaultValue;
                        break;
                    case "centerheight":
                        _billboardCenterHeight = (bool)properties[i].DefaultValue;
                        break;
                    case "meshortexresource":
                        _meshOrTextureResourcePath = (string)properties[i].DefaultValue;
                        break;
                    case "isbillboard":
                        _isBillboard = (bool)properties[i].DefaultValue;
                        break;
                    case "usecolors":
                        _useColors = (bool)properties[i].DefaultValue;
                        break;
                    case "alphasort":
                        _alphaSort = (bool)properties[i].DefaultValue;
                        break;
                    case "maxcount":
                        _maxInstancesCount = (uint)properties[i].DefaultValue;
                        break;
                }
            }
        }
        #endregion

        #region IPageableTVNode Members
        public override void UnloadTVResource()
        {
        	DisposeUnmanagedResources();
        }
                
        public override void LoadTVResource()
        {
            // it's perfectly ok to try to load the resource for the mesh3d first if it's not
            // since that call as well as this one is taking place on the same worker thread.
			// However, we do need to use LoadTVResourceSynchronously() which will lock the resource
			// TODO: making all calls to LoadTVResource() internal can help force us to only use
			// pager.LoadTVResourceSychronously()
          if (_isBillboard)
          {
              System.Diagnostics.Debug.Assert (_mesh == null); 
              _mesh = (Billboard)Resource.Repository.Create (_meshOrTextureResourcePath, "Billboard");
              
              Keystone.IO.PagerBase.LoadTVResource (_mesh, false);
            		if (_mesh.PageStatus != PageableNodeStatus.Loaded) 
            			throw new Exception("MinimeshGeometry.LoadTVResource() - ERROR: loading reference Mesh3d '" + _meshOrTextureResourcePath + "' for minimesh.");

              System.Diagnostics.Debug.Assert(_mesh.GroupCount == 1, "MinimeshGeometry.LoadTVResource() - Mesh must only contain 1 group.");
    		  Resource.Repository.IncrementRef (_mesh);
          
    		  _billboardtype = ((Billboard)_mesh).BillboardType;
    		  mAxialRotation = ((Billboard)_mesh).AxialRotationEnable;
          }
		  else
          { 
            	try 
            	{
                	_mesh = (Mesh3d)Resource.Repository.Create(_meshOrTextureResourcePath, "Mesh3d");
    	        	Keystone.IO.PagerBase.LoadTVResource (_mesh, false);
            		if (_mesh.PageStatus != PageableNodeStatus.Loaded) 
            			throw new Exception("MinimeshGeometry.LoadTVResource() - ERROR: loading reference Mesh3d '" + _meshOrTextureResourcePath + "' for minimesh.");

                    System.Diagnostics.Debug.Assert(_mesh.GroupCount == 1, "MinimeshGeometry.LoadTVResource() - Mesh must only contain 1 group.");
                    Resource.Repository.IncrementRef (_mesh);
            	}
            	catch (Exception ex)
            	{
            		System.Diagnostics.Debug.WriteLine ("MinimeshGeometry.LoadTVResource() - ERROR: loading reference Mesh3d '" + _meshOrTextureResourcePath + "' for minimesh.");
            		throw ex;
            	}
         }
     
            // hack
            // TODO: texture and material will only get applied properly if a TVMesh is loaded with loadTextures and loadMaterials = true
            // otherwise no Appearance.Apply() is currently set to occur and im not 100% how to implement that since InstancedModel
            // ultimately needs to do Model.Minimesh.SetTexture and .SetMaterial i believe
            //_minimesh.SetTexture( 2);
            //_minimesh.SetMaterial(Material.Create(Material.DefaultMaterials.red).TVIndex);
            //    m.SetFadeOptions(True, 600, 150) ', 1200, 256)
            //m.SetAlphaSort(True)
            //m.SetCullMode(CONST_TV_CULLING.TV_DOUBLESIDED)
            //m.SetBlendingMode(CONST_TV_BLENDINGMODE.TV_BLEND_ALPHA)
            //m.SetAlphaTest(False, 128)
            
        	// TODO: we do want to grab the appearance (texture and material from the mesh which i dont think we're fully doing)

        	// We know Minimesh can only have 1 texture and 1 material because only 1 group is allowed in mesh

            if (_minimesh != null)
            {
                _minimesh.Destroy();
            }

            // TODO: why does this line sometimes fail?  When we are creating minis for tilemap structure 
            //       the mini is not even added to scene yet so no picking or update or render is getting called
            //       it's just this one call to .LoadTVResource() with the crash on CreateMiniMesh
            //       - TODO: it seems part of the issue may be in apply of appearance when we're not loaded yet?
            //       
            try 
            {
            	System.Diagnostics.Debug.Assert (_maxInstancesCount > 0, "MinimeshGeometry.LoadTVResource() - ERROR: maxInstancesCount must be > 0.");
            	//System.Diagnostics.Debug.WriteLine ("MinimeshGeometry.LoadTVResource() - Scene.CreateMinimesh() with id = " + _id);
            	// TODO: i wonder if TV had a problem creating minimeshes from multiple threads?  Because forcing 
            	//       our ClientPager to use a single concurrent thread for scenegraph paging in of our Zones seems to solve the issue
            	_minimesh = CoreClient._CoreClient.Scene.CreateMiniMesh((int)_maxInstancesCount, _id);
            	_minimesh.Enable (false);
            	// SetColorMode() should be set after Scene.CreateMiniMesh() but before minimesh.CreateBillboard() or minimesh.CreateFromMesh()
            	// TODO: SetColorMode() doesn't seem to work.  Or does it only work with meshes and not billboards?
	            _minimesh.SetColorMode(_useColors);
                
	            if (_isBillboard)
	                _minimesh.CreateBillboard(_billboardWidth, _billboardHeight, _billboardCenterHeight);
	            else
	                _minimesh.CreateFromMesh(CoreClient._CoreClient.Globals.GetMeshFromID(_mesh.TVIndex));
	            
	          
            }
            catch (Exception ex)
            {
            	System.Diagnostics.Debug.WriteLine ("MinimeshGeometry.LoadTVResource() - Error CreateMiniMesh.");
            	throw ex;
            }
            
           
            _tvfactoryIndex = _minimesh.GetIndex();
            _minimesh.EnableFrustumCulling(false, false, false);
            _minimesh.SetCullMode((CONST_TV_CULLING)_cullMode);
            
            
            if (_isBillboard) 	
	            // NOTE: changing TVMinimesh rotation mode at run-time also changes any Scales you've set
	            // with _minimesh.SetScale (x,y,z, elementIndex);   
            	// so it's best to set rotation mode only during LoadTVResource() and never again.
	            _minimesh.SetRotationMode(_billboardtype);
            

            if (mPreCacheMaxElements)
            {
            	for (int i = 0; i < _maxInstancesCount; i++)
            	{
            		uint index = AddElement (Vector3d.Zero());
            		System.Diagnostics.Debug.Assert ((int)index == i);
            	}
            	for (int i = 0; i < _maxInstancesCount; i++)
            	{
            		mDisposedElements[i] = true;	
            		mEnableArray[i] = 0;
            	}
            	// reset instance count since no elements actually added, only precached
                // Sept.23.3017 - Actually we don't reset mInstanceCount.  The mInstanceCount
                // must be incremented during the call to AddElement and we should not change it.
                // The mDisposedElements array tells us when an element can be used.
            	// mInstanceCount = 0;
            }
            
            
            _minimesh.Enable(true);
            
            // call .Render() to ensure everything is initialized
            // NOTE: not having .Render() here cuts down on the minimesh crashes during loading dramatically.
 //           _minimesh.Render(); 
             
            SetChangeFlags(Keystone.Enums.ChangeStates.GeometryAdded | 
                           Keystone.Enums.ChangeStates.BoundingBoxDirty | 
                           Keystone.Enums.ChangeStates.MatrixDirty, Keystone.Enums.ChangeSource.Self);
        }

        public override void SaveTVResource(string resourcePath)
        {
            //_relativeResourcePath = resourcePath;
            if (TVResourceIsLoaded)
            {
                // for minimesh if it's a billboard texture then we can just save the texture via it's texture index
                if (_isBillboard)
                {
                    // TODO: need a Diffuse object ref and not just some int _billboardTextureTVIndex
                    //System.Diagnostics.Trace.Assert(_billboardTextureTVIndex > 0);
                    //CoreClient._CoreClient.TextureFactory.SaveTexture(_billboardTextureTVIndex, resourcePath);
                }
                // for a TVMesh then I suppose this resource should be permanently loaded with a MiniMesh right? It doesnt have to 
                // be serialized though because it gets loaded from the resourcePath file.
                else
                {
                    System.Diagnostics.Trace.Assert(_mesh.TVResourceIsLoaded);
                    _mesh.SaveTVResource(resourcePath); // this is why name i think should not be equal to filepath right?
                }
            }
        }
        #endregion

        public Mesh3d Mesh3d { get { return _mesh; } }

        public uint MaxInstancesCount
        {
            get { return _maxInstancesCount; }
            set
            {
                _maxInstancesCount = value;
                if (TVResourceIsLoaded)
                {
	                _minimesh.SetMaxMeshCount((int)value);
	                mMaxMeshCountChanged = false;
                }
                //_matrices = new TV_3DMATRIX[_maxInstancesCount];
                //_positions = new TV_3DVECTOR[_maxInstancesCount];
                //_scales = new TV_3DVECTOR[_maxInstancesCount];
                //_rotations = new TV_3DVECTOR[_maxInstancesCount];
                //_enableArray = new byte[_maxInstancesCount];
            }
        }

        public uint InstancesCount // actual instances count.  
        {
            get 
            {
            	lock (mSyncLock)
	            	return mInstanceCount;
            }
        }

        public bool FadeEnabled
        {
            get { return _fadeenabled; }
            set { _fadeenabled = value; }
        }

        public bool FadeUseTV3D
        {
            get { return _fadeUseTV3D; }
            set { _fadeUseTV3D = value; }
        }

        public double FadeStart
        {
            get { return _fadestart; }
            set { _fadestart = value; }
        }

        public double FadeDistance
        {
            get { return _fadedistance; }
            set { _fadedistance = value; }
        }

        internal bool AlphaSort
        {
            get { return _alphaSort;}
            set { _alphaSort = value;}
        }
        
        /// <summary>
        /// The alpha test is a last chance to reject a pixel from being written to the screen.
        /// After the final output color has been calculated, the color can optionally have its alpha value 
        /// compared to a fixed value. If the test fails, the pixel is not written to the display.
        /// 
        /// When rendering plants and trees, many games have the hard edges typical of alpha testing. 
        /// A way around that is to render the object twice. In the first pass, we use alpha testing to 
        /// only render pixels that are more than 50% opaque. In the second pass, we alpha-blend the graphic 
        /// in the parts that were cut away, without recording the depth of the pixel. We might get a bit of 
        /// confusion as further away branches overwrite the nearby ones, but in practice, that is hard to 
        /// see as leaves have a lot of visual detail in them.
        /// </summary>
        /// <param name="enable"></param>
        /// <param name="iGroup"></param>
        internal override void SetAlphaTest (bool enable, int iGroup)
        {
            _alphaTestEnable = enable;
            // caution: DepthBufferWrite  can be enabled or disabled
            // even if the param _alphaTestEnable is set to false.   
            // but normally, you are only disabling it so that unsorted
            // alphatested groups in the mesh just render ontop of each other
            // with minimal artifacts.
            if (_minimesh != null)
            {
    	        // TODO: alpha test can be set different PER GROUP, but for now
        		// let's not do that unless we have to.  it saves space in xml
        		// to not have every group store that information
	            _minimesh.SetAlphaTest(_alphaTestEnable, _alphaTestRefValue, _alphaTestDepthBufferWriteEnable);
	            _minimesh.SetAlphaSort (_alphaSort); 
    	        //if (_alphaTestEnable)
    	        //	_minimesh.SetCullMode (CONST_TV_CULLING.TV_DOUBLESIDED); // HACK until we add this option 
            }
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
        /// Note: CONST_TV_BLENDINGMODE.TV_BLEND_ALPHA is required when using alpha 
        /// component in the diffuse of material to enable Alpha Transparency.
        /// Note: When alpha blending is enabled in the hardware, some optimizations 
        /// such as zbuffer pixel rejections in the pixel shader are disabled.
        /// </summary>
        internal override CONST_TV_BLENDINGMODE BlendingMode
        {
            set
            {
                _minimesh.SetBlendingMode(value);
            }
        }

        /// <remarks>
        /// must be overridden by derived types because Appearance nodes 
        /// use this to Apply BlendingMode to Geometry.
        /// </remarks>
        internal override void SetBlendingMode(CONST_TV_BLENDINGMODE blendingMode, int group)
        {
            // minimeshes don't use groups so group should be either -1 or 0
            System.Diagnostics.Debug.Assert(group == 0 || group == -1);
            SetBlendingMode(blendingMode);
        }

        internal void SetBlendingMode(CONST_TV_BLENDINGMODE blendingMode)
        {
            _minimesh.SetBlendingMode(blendingMode);
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
        
        //public int GetTextureEx(CONST_TV_LAYER layer)
        //{
        //    return _minimesh.GetTexture((int)layer );
        //}

        internal void SetTexture(int tvTextureID)
        {
            if (tvTextureID < 0) return;
            _minimesh.SetTexture(tvTextureID);
        }

        internal void SetTextureEx(int layer, int tvTextureID)
        {
            if (tvTextureID < 0) return;
            _minimesh.SetTextureEx(layer, tvTextureID);
        }

        internal TVMiniMesh TVMiniMesh
        {
            get { return _minimesh; }
            set { _minimesh = value; }
        }

        public bool AxialRotationEnable
        {
            get { return mAxialRotation; }
            set 
            { 
                mAxialRotation = value;
                
                // rotation must be NOROTATION for our custom axial rotation to work
                if (mAxialRotation == true)
                    _billboardtype = CONST_TV_BILLBOARDTYPE.TV_BILLBOARD_NOROTATION;
            }
        }
                
        public CONST_TV_BILLBOARDTYPE BillboardType
        {
            get { return _billboardtype; }
        }

        public bool IsBillboard
        {
            get { return _isBillboard; }
        }
        
        /// <summary>
        /// Returns model space bounding sphere for minimesh at specified elementIndex
        /// </summary>
        /// <param name="elementIndex"></param>
        /// <returns></returns>
        internal BoundingSphere GetBoundingSphere (uint elementIndex)
        {
        	Keystone.Types.BoundingSphere sphere = new Keystone.Types.BoundingSphere(this._mesh.BoundingSphere);
        	//System.Diagnostics.Debug.Assert (sphere.Center == Vector3d.Zero());
        	
        	Vector3d vScale = _scaleArray[elementIndex];
        	double scale =  Math.Max(vScale.z, Math.Max (vScale.x, vScale.y));
        	sphere.Scale(scale);
			
			return sphere;
        }
        
        internal Vector3d GetElementPosition(uint elementIndex)
        {
            return _positionArray[elementIndex];
        }

        internal Vector3d GetElementScale(uint elementIndex)
        {
            return _scaleArray[elementIndex];
        }
            
        internal Vector3d GetElementRotation(uint elementIndex)
        {
            Vector3d rotation = _rotationArray[elementIndex];
        	return rotation;
        }

        internal int GetElementColor(uint elementIndex)
        {
            return mColorArray[elementIndex];
        }
        
        internal int GetElementTag(uint elementIndex)
        {
            return _tagArray[elementIndex];
        }

        // Sept.29.2016 - I believe these array assignment properties
        // are obsolete. I switched to InstancedGeometry instead.
        internal Vector3d[] PositionArray
        {
            get { return _positionArray; }
            set
            {
                _positionArray = value;
                mPositionArrayChanged = true;
            }
        }

        internal Vector3d[] RotationArray
        {
            get { return _rotationArray; }
            set
            {
                _rotationArray = value;
                mRotationArrayChanged = true;
            }
        }

        internal Vector3d[] ScaleArray
        {
            get { return _scaleArray; }
            set
            {
                _scaleArray = value;
                mScaleArrayChanged = true;
            }
        }

        internal int[] ColorArray
        {
            get
            {
                if (TVResourceIsLoaded)
                {
                    return mColorArray;
                }
                return null;
            }
        }

        internal byte[] EnableArray
        {
            get
            {
                if (TVResourceIsLoaded)
                {
                    return mEnableArray;
                }
                return null;
            }
            set
            {
                mEnableArrayChanged = true;
                mEnableArray = value;
            }
        }
        
        internal bool GetElementEnable (uint elementIndex)
        {
        	return mEnableArray[elementIndex] != 0;
        }
        
        public void SetElementEnable(uint elementIndex, byte value)
        {
            //byte val = 0;
            //if (value) val = 1;
            if (elementIndex >= mEnableArray.Length) return;

            // March.30.2015 - when precaching and using enable/disable to control active instances
            // we must update the mInstanceCount here since FindFreeIndex() will never get called 
            // since AddElement() never gets called, only ChangeElement()
            // NOTE: if the element is already enabled or disabled, we shouldn't modify the mInstanceCount in either case
            // NOTE: wait a minute, this is enable/disable in mEnableArray not mDisposedElements so we shouldn't be
            // modifying the mInstanceCount at all!  RemoveElement on the other hand does modify dispose.
            // NOTE: as far as precaching goes, I don't think the top most comment is actually relevant.  The
            // mInstanceCount should match the pre-cached count.
            //if (value == 1)
            //{
            //    if (mEnableArray[elementIndex] != 1)
            //        mInstanceCount++;
            //}
            //else
            //{
            //    if (mEnableArray[elementIndex] != 0)
            //        mInstanceCount--;
            //} 

            mEnableArray[elementIndex] = value;
            mEnableArrayChanged = true;
            SetChangeFlags (Keystone.Enums.ChangeStates.BoundingBoxDirty, Keystone.Enums.ChangeSource.Self);
        }
              
        internal void SetElementPosition (uint elementIndex, double x, double y, double z)
        {
            Vector3d position;
            position.x = x;
            position.y = y;
            position.z = z;
            SetElementPosition (elementIndex, position);
        }
        
        internal void SetElementPosition (uint elementIndex, Vector3d value)
        {       
        	_positionArray[elementIndex] = value;
            mPositionArrayChanged = true;
        }
        
        internal void SetElementScale (uint elementIndex, double x, double y, double z)
        {
            Vector3d scale;
            scale.x = x;
            scale.y = y;
            scale.z = z;
            SetElementScale (elementIndex, scale);
        }
        
        internal void SetElementScale (uint elementIndex, Vector3d value)
        {       
        	_scaleArray[elementIndex] = value;
            mScaleArrayChanged = true;
        }
        
    	internal void SetElementRotation (uint elementIndex, double x, double y, double z)
        {
            Vector3d rotation;
            rotation.x = x;
            rotation.y = y;
            rotation.z = z;
            SetElementRotation (elementIndex, rotation);
        }
        
        internal void SetElementRotation (uint elementIndex, Vector3d value)
        {       
        	_rotationArray[elementIndex] = value;
            mRotationArrayChanged = true;
        }
        
        internal void SetElementMatrix (uint elementIndex, Matrix matrix)
        {
        	_matrixArray[elementIndex] = matrix;
      		mMatrixArrayChanged = true;
        }
        
        internal Matrix GetElementMatrix (uint elementIndex)
        {        	
        	// TODO: what if this particular matrix has not been computed?
        	// seems we are not actually using _matrices as far as setting those
        	// to the minimesh prior to render.  Instead, we should just compute the matrix
        	// when we want the value since it seems the only time we need a matrix of a 
        	// specific minimesh element is when we want to do Picking using the mesh3d and
        	// parent matrix and element matrix.
        	
        	 Matrix tmat = Matrix.CreateTranslation(_positionArray[elementIndex]);
        	 // TODO: hack, if no scaling inheritance, we do not 
        	 Matrix smat = Matrix.CreateScaling(_scaleArray[elementIndex]); // Matrix.Identity(); // TODO: scaling is screwing up ability to create modelspace ray 
             Vector3d rotation = _rotationArray[elementIndex];
             Matrix Rx = Matrix.CreateRotationX(rotation.x * Utilities.MathHelper.DEGREES_TO_RADIANS);
             Matrix Ry = Matrix.CreateRotationY(rotation.y * Utilities.MathHelper.DEGREES_TO_RADIANS);
             Matrix Rz = Matrix.CreateRotationZ(rotation.z * Utilities.MathHelper.DEGREES_TO_RADIANS);
             // The order these rotations are performed to match TV3D is: Yaw(y), Pitch(x), then Roll (z). 
             Matrix rmat = Ry * Rx * Rz;
                    
             Matrix result  = smat * rmat * tmat;
            
             return result;
        }

        
        public uint AddElement(Vector3d modelSpacePosition)
        {
        	Vector3d scale;
        	scale.x = scale.y = scale.z = 1.0d;
            return AddElement((float)modelSpacePosition.x, (float)modelSpacePosition.y, (float)modelSpacePosition.z,
                              (float)scale.x, (float)scale.y, (float)scale.z);
        }
                
        // rotation is in degrees
        internal uint AddElement(Vector3d modelSpacePosition, Vector3d scale)
        {
            return AddElement((float)modelSpacePosition.x, (float)modelSpacePosition.y, (float)modelSpacePosition.z,
                              (float)scale.x, (float)scale.y, (float)scale.z);
        }

        // rotation is in degrees
        internal uint AddElement(Vector3d modelSpacePosition, Vector3d scale, Vector3d rotation)
        {
            return AddElement((float)modelSpacePosition.x, (float)modelSpacePosition.y, (float)modelSpacePosition.z,
			                  (float)scale.x, (float)scale.y, (float)scale.z,
			                  (float)rotation.x, (float)rotation.y, (float)rotation.z);
        }

        internal uint AddElement(Vector3d modelSpacePosition, Vector3d scale, Vector3d rotation, int color, int tag)
        {
            return AddElement((float)modelSpacePosition.x, (float)modelSpacePosition.y, (float)modelSpacePosition.z,
			                  (float)scale.x, (float)scale.y, (float)scale.z,
			                  (float)rotation.x, (float)rotation.y, (float)rotation.z,
			                  color, tag);
        }
                
        // rotation is in degrees
        internal uint AddElement(float modelSpacePositionX, float modelSpacePositionY, float modelSpacePositionZ,
                                 float scaleX, float scaleY, float scaleZ)
        {
            return AddElement(modelSpacePositionX, modelSpacePositionY, modelSpacePositionZ,
                			  scaleX, scaleY, scaleZ, 
                			  0f, 0f, 0f);
        }

        // rotation is in degrees
        internal uint AddElement(float modelSpacePositionX, float modelSpacePositionY, float modelSpacePositionZ,
                                float scaleX, float scaleY, float scaleZ,
                                float rotationX, float rotationY, float rotationZ)
        {

        	int tag = -1;
        	int color = 0;
            return AddElement (modelSpacePositionX, modelSpacePositionY, modelSpacePositionZ,
                			   scaleX, scaleY, scaleZ,
                			   rotationX, rotationY, rotationZ,
                			   color,
                			   tag);
        }

        // rotation is in degrees
        internal uint AddElement(float modelSpacePositionX, float modelSpacePositionY, float modelSpacePositionZ,
                        float scaleX, float scaleY, float scaleZ,
                        float rotationX, float rotationY, float rotationZ,
                        int color,
                        int tag)
        {

            mInstanceCount++;
           // System.Diagnostics.Debug.WriteLine("Instance Count = " + mInstanceCount);
            if (mInstanceCount > _maxInstancesCount) throw new Exception("MinimeshGeometry.FindFreeIndex() - Max Instances reached.");


            // if there is a free spot at a mDisposedElements index, use it
            uint freeIndex = FindFreeIndex();

        //System.Diagnostics.Debug.WriteLine("MinimeshGeometry.AddElement() - Free Index = " + freeIndex.ToString() + " MinimeshID = " + _id);

        ChangeElement(freeIndex, 
                      modelSpacePositionX, modelSpacePositionY, modelSpacePositionZ,
                      scaleX, scaleY, scaleZ,
                      rotationX, rotationY, rotationZ,
                      color,
                      tag);


            return freeIndex;
        }
        
//        // overload adds a "tag" we can associate with each element such as an edgeID for Tiled Structure
//        internal uint AddElement(float modelSpacePositionX, float modelSpacePositionY, float modelSpacePositionZ,
//                                float scaleX, float scaleY, float scaleZ,
//                                float rotationX, float rotationY, float rotationZ,
//                                object tag)
//        {
//        	
//        }
        
        
        // TODO: there is a problem where remove element is not working
        //       properly.  somehow it seems to me we are successfully causing the element's
        //       index to be flaged disposed so it can be recycled, but it's not causing
        //        the minimesh element to not be rendered.
        internal void RemoveElement(uint elementIndex)
        {
        	lock (mSyncLock)
        	{
        		#if DEBUG
        		if (mEnableArray == null && mPreCacheMaxElements == false)
        			System.Diagnostics.Debug.WriteLine ("MinimeshGeometry.RemoveElement() - ERROR");
        		#endif
                // Feb.10.2015 - we now only apply changes to actual TVMinimesh during Update() or Render(), so do not EnableMinimesh\Disable element here      		
                //_minimesh.EnableMiniMesh((int)elementIndex, false);
            	mDisposedElements[elementIndex] = true;
            	mEnableArrayChanged = true;
            	mEnableArray[elementIndex] = (byte)0;
                System.Diagnostics.Debug.Assert(mInstanceCount > 0);
        	    mInstanceCount--;
        	    
        	    // if all disposed elements == true, delete entire mDisposedElements and support arrays
        	    for (int i = 0; i < mDisposedElements.Length; i++)
        	    	if (mDisposedElements[i] == false) 
        	    		return;
        	    			
        	 	mDisposedElements = null;
        	 	_matrixArray = null;
        	 	_positionArray = null;
	            _rotationArray = null;
	            _scaleArray = null;
	            mColorArray = null;
	            _tagArray = null;
	            mEnableArray = null;
	                	
        	    System.Diagnostics.Debug.Assert (mInstanceCount == 0);
        	    
        	    // NOTE: removing an individual element is conceptually same as removing Geometry 
	            SetChangeFlags (Enums.ChangeStates.GeometryRemoved | Enums.ChangeStates.BoundingBoxDirty, Enums.ChangeSource.Self);
        	}
        }
        
		internal void ChangeElement(uint elementIndex, Vector3d position, Vector3d rotation)
		{
			// use existing scale
			Vector3d scale = GetElementScale (elementIndex);
			
			ChangeElement (elementIndex, 
			               (float)position.x, (float)position.y, (float)position.z,
			               (float)scale.x, (float)scale.y, (float)scale.z,
			               (float)rotation.x, (float)rotation.y, (float)rotation.z);
		}
        
        // rotation is in degrees
		internal void ChangeElement(uint elementIndex, Vector3d position, Vector3d scale, Vector3d rotation)
		{
			ChangeElement (elementIndex, 
			               (float)position.x, (float)position.y, (float)position.z,
			               (float)scale.x, (float)scale.y, (float)scale.z,
			               (float)rotation.x, (float)rotation.y, (float)rotation.z);
		}
		
		internal void ChangeElement(uint elementIndex, Vector3d position, Vector3d scale, Vector3d rotation, int color)
		{
			ChangeElement (elementIndex, 
			               (float)position.x, (float)position.y, (float)position.z,
			               (float)scale.x, (float)scale.y, (float)scale.z,
			               (float)rotation.x, (float)rotation.y, (float)rotation.z,
			               color,
			               -1);
		}
				
		internal void ChangeElement(uint elementIndex, 
                                    float modelSpacePositionX, float modelSpacePositionY, float modelSpacePositionZ,
                                    float rotationX, float rotationY, float rotationZ)
        {
			// use existing scale
			Vector3d scale = GetElementScale (elementIndex);
        	int color = 0;
        	int tag = -1;
            ChangeElement(elementIndex, 
        	              modelSpacePositionX, modelSpacePositionY, modelSpacePositionZ,
        	              (float)scale.x, (float)scale.y, (float)scale.z,
                          rotationX, rotationY, rotationZ,
                          color,
                          tag);
		}
        // rotation is in degrees
        internal void ChangeElement(uint elementIndex, 
                                    float modelSpacePositionX, float modelSpacePositionY, float modelSpacePositionZ,
                                    float scaleX, float scaleY, float scaleZ,
                                    float rotationX, float rotationY, float rotationZ)
        {

        	int color = 0;
        	int tag = -1;
            ChangeElement(elementIndex, 
        	              modelSpacePositionX, modelSpacePositionY, modelSpacePositionZ,
                          scaleX, scaleY, scaleZ, 
                          rotationX, rotationY, rotationZ,
                          color,
                          tag);
        }

        // rotation is in degrees
        public void ChangeElement(uint elementIndex, 
                                    float modelSpacePositionX, float modelSpacePositionY, float modelSpacePositionZ,
                                    float scaleX, float scaleY, float scaleZ,
                                    float rotationX, float rotationY, float rotationZ,
                                    int color,
									int tag)
        {
        	lock (mSyncLock)
        	{
	            // TODO: what if we prevent modification here and only ever update actual minimesh
	            //       in Update() and Render()  one problem is still axial rotations are annoying... i really do 
	            //       need to do those in shader
	           
// Feb.10.2015 - we now only apply changes to actual TVMinimesh during Update() or Render()	            
//	            _minimesh.SetPosition(modelSpacePositionX, modelSpacePositionY, modelSpacePositionZ, (int)elementIndex);
//	            _minimesh.SetScale(scaleX, scaleY, scaleZ, (int)elementIndex);
//	            _minimesh.SetRotation(rotationX, rotationY, rotationZ, (int)elementIndex);

	            SetElementPosition (elementIndex, modelSpacePositionX, modelSpacePositionY, modelSpacePositionZ);	            
	            SetElementRotation (elementIndex, rotationX, rotationY, rotationZ);
	            SetElementScale (elementIndex, scaleX, scaleY, scaleZ);
	            
	            // perhaps we can use just 8 bits of the color to store any stencil ref value?
	            // _minimesh.SetColor (color, (int)elementIndex); <-- Pretty sure this is wrong.  Color we add here is per element
	            mColorsArrayChanged = true;
	            mColorArray[elementIndex] = color;
	            _tagArray[elementIndex] = tag;
	            
	            // NOTE: adding an individual element is conceptually same as adding Geometry 
	            SetChangeFlags (Enums.ChangeStates.GeometryAdded | Enums.ChangeStates.BoundingBoxDirty, Enums.ChangeSource.Self);
        	}
        }

        bool mMaxMeshCountChanged = true;

        private uint FindFreeIndex()
        {
            
            lock (mSyncLock)
            {
                // NOTE: we do not increment mInstanceCount automatically every time.  We only increment if the arrays need to be appended.
	            // mInstanceCount++;
                	            
                uint freeIndex = 0;
                bool found = false;
                
                // is there an available free slot of disposed element we can recyle?
            	if (mDisposedElements != null)
	                for (uint i = 0; i < mDisposedElements.Length; i++)
    	                if (mDisposedElements[i] == true) 
    	            	{
    	            		freeIndex = i;
    	            		found = true;
    	            		break;
    	            	}
                
                // expand all arrays by 1 since no free existing slots
                if (found == false)
	            {
                    if (mAxialRotation)
                		_matrixArray = _matrixArray.ArrayAppend (Matrix.Identity());

                	// for mAxialRotation, _rotationArray still holds the Billboard orientation (eg bullet fire direction)
                	// and _positionArray holds current position towards goal and scale still holds indpendant scale
                	_rotationArray = _rotationArray.ArrayAppend(Vector3d.Zero());
                	_positionArray = _positionArray.ArrayAppend(Vector3d.Zero());
                	_scaleArray = _scaleArray.ArrayAppend(Vector3d.Zero());
                	mColorArray = mColorArray.ArrayAppend (new Color((byte)255, (byte)255, (byte)255, (byte)255).ToInt32());
	                _tagArray = _tagArray.ArrayAppend ((int)-1);
	                mEnableArray = mEnableArray.ArrayAppend((byte)1);
	                
	                bool disposed = false;
	                // disposed elements holds boolean for each element and tracks if that element is active or disposed
	                // and is same size as other arrays (eg _positionArray)
	                mDisposedElements = mDisposedElements.ArrayAppend(disposed);
	                freeIndex = (uint)mDisposedElements.Length - 1;
	                 
					// set flag to expand the minimesh max count if necessary
					if (freeIndex > _maxInstancesCount - 1) 
					{
						_maxInstancesCount += (uint)(_maxInstancesCount * 2);
						// March.6.2015 - we no longer call _minimesh.SetMaxMeshCount ((int)_maxInstancesCount) here.
						// if we need to grow the number of elements, we set a flag and then must do it in Update() or Render()
						// however setting the minimesh to a number of elements you will not exceed seems to have solved
						// the random crashes with minimeshes paging in.  
						mMaxMeshCountChanged = true;
	            	}
                }

        		mDisposedElements[freeIndex] = false; // we'll be using this index now so set disposed == false!
        		mEnableArray[freeIndex] = (byte)1;
                mEnableArrayChanged = true;
                
            	System.Diagnostics.Debug.Assert(freeIndex <= mInstanceCount); // can never be higher
// Feb.10.2015 - we now only apply changes to actual TVMinimesh during Update() or Render()
//            	_minimesh.EnableMiniMesh((int)freeIndex, true);
            	
            	// NOTE: it's possible for the enable array length to be larger than instance count
            	// when elements are disabled but the array is not compacted, but the instance count should never
            	// be greater than the mEnableArray.Length
//				System.Diagnostics.Debug.Assert (mEnableArray.Length >= mInstanceCount, "MinimeshGeometry.FindFreeIndex() - ERROR: freeIndex must always be less than instance count.");
                return freeIndex;
            }
        }

    #region Geometry Members
        internal override Keystone.Shaders.Shader Shader
        {
            get { return _shader; }
            set
            {
                if (_shader == value && (value == null || value.PageStatus == PageableNodeStatus.Loaded)) return;

                if (value != null)
                {
                    // also we don't want to assign this shader if it's underlying tvshader 
                    // is not yet paged in.
                    if (value.PageStatus != PageableNodeStatus.Loaded)
                    {
                        System.Diagnostics.Debug.WriteLine("MinimeshGeometry.Shader '" + value.ID + "' not loaded.  Cannot assign to Mesh '" + _id + "'");
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
        

        // AdvancedCollide can be internal scope
        internal override PickResults AdvancedCollide(Vector3d start, Vector3d end, KeyCommon.Traversal.PickParameters parameters)
        {
        	// see picker.cs.PickMinimeshGeometry()
        	throw new NotImplementedException ("MinimeshGeometry.AdvancedCollide() - ERROR: Picking collision must be implemented as Apply() traverser method");
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
        	lock (mSyncLock) // TODO: until we have command processor orders so add/remove minis during autotile occurs ONLY through command processor
        		             // we need to prevent rendering during runtime user editing autotiling
        	{
            try
            {
            	// TODO: it should be impossible for _resourceStatus to be "Loaded" before it's actually loaded
            	//       HOWEVER if paging out, we could start to render here first since it's not synclocked and then
            	//       while minimesh.Render() we page out and set _resourceStatus to Unloading but we're already in .Render()!
            	
                // NOTE: we check PageableNodeStatus.Loaded and NOT TVResourceIsLoaded because that 
            	// TVIndex is set after Scene.CreateMinimeshCall() and thus before we've even finished 
            	// adding MinimeshElements!
                if (_resourceStatus != PageableNodeStatus.Loaded) return; 
                
                if (model.Appearance != null) // TODO: i could potentially set it to use the parent model's appearance if applicable
                {
                    // TODO: why is this check for geometry added only now setting appearancechanged flag?
                    //if ((_changeStates & Enums.ChangeStates.GeometryAdded) != 0)
                    //    _changeStates |= Keystone.Enums.ChangeStates.AppearanceChanged;

                    // TODO: shouldn't the above call SetChangeStates
                    // TODO: actually, shouldn't this have already been done once this geometry node was added? including after the resource is loaded
                    //SetChangeFlags(Keystone.Enums.ChangeStates.AppearanceChanged, Keystone.Enums.ChangeSource.Self);

                    // TODO: can we verify all textures, shaders, are loaded in? and materials?
                    _appearanceHashCode = model.Appearance.Apply(this, elapsedSeconds);
                }
                else
                    _appearanceHashCode = NullAppearance.Apply(this, _appearanceHashCode);

            }
            catch
            {
                // note: TODO:  very very rarely ill get an access violation on render after a mesh has been paged in.  The mesh shows as loading fine
                // in my code and in tv debug log.  My only real guess is that when the materials and textures get loaded, there's a race condition
                // where either of those will attempt to set while the mesh is rendering.  we need some kind of guard against that.
                Trace.WriteLine(string.Format("MinimeshGeometry.Render() - ERROR: Failed to apply appearance. '{0}'.", _id));
            }

            try
            {
				if (mMaxMeshCountChanged)
				{
					mMaxMeshCountChanged = false;
					// March.6.2015 - Having to call SetMaxMeshCount() to grow the minimesh SEEMS TO BE VERY BAD.
					// 1) It is very expensive and slows the paging in of our terrains in each zone considerably!
					// 2) Setting the max count to some higher value so we never need to expand when paging in terrain
					// or when painting seems to have solved the random crashes!
					// we only grow the max value inside of Render() and never during calls to AddElement 
					// since that would modify this mesh potentially DURING a render from Render thread.
					MaxInstancesCount = _maxInstancesCount;
            	}

            	// NOTE: It is important that we allow Appearance.Apply() to occur above before we exit if mEnableArray null or empty
                //       otherwise AppearanceParameterChanged flag will never get disabled.
            	if (mEnableArray == null || mEnableArray.Length == 0) return;
            	if (mEnableArrayChanged)
                {
            		// TODO: April.21.2015 - why is this crashing?  is it out of memory error? seems to happen when our ClientPager.Zone_Visible_Range >= 5
            		// TODO: we create 11 minimeshes currently for each model type within the segment.  This is a huge waste and why Zak's version might be better
            		//       especially if we can simply re-use a single instanced mesh and change the positions for each region as we iterate?  THus 11 minimeshes for
            		//       entire world no matter how many zones are currently visible.
            		//       -first step will be loading a custom tvmesh into internal object mesh
            		using (CoreClient._CoreClient.Profiler.HookUp("MinimeshGeometry.Assign Arrays"))
	                    _minimesh.SetEnableArray(mEnableArray.Length, mEnableArray);
                    mEnableArrayChanged = false;
                } 
                     
                if (mColorsArrayChanged && _useColors)
                {
                	// WARNING: TV3D's built in minimesh shader does not use element color.a for per element alpha blending
                	using (CoreClient._CoreClient.Profiler.HookUp("MinimeshGeometry.Assign Arrays"))
	                    _minimesh.SetColorArray (mColorArray.Length, mColorArray);
                	mColorsArrayChanged = false;
                }
            	// NOTE: TV3d's design is such that you can't SetRotationMatrixArray() or SetMatrixArray() (and probably not individual rotations either)
                // if you want the billboardtype rotation mode to work. In other words, if a billboard type is set such that TV computes the rotation
                // each frame, then attempting to set the rotation manually will effectively change the rotation mode to NOROTATION.  
                // I think it's a bug, Sylvain says it's by design.  But I say if you have to load
                // a minimesh from a file, then you can't just store/restore the entire matrix and use a single SetMatrixArray() call to apply saved state. 
                // Instead you have to have seperate calls to apply Position and Scale.  
                if (_isBillboard && mAxialRotation) 
                {       
	             	// if we are computing axial rotations in CPU (see Model.Render()) then we use Matrix to update
	             	// each element, not individual arrays.  The reason is because axial billboarding results
	             	// in a rotation matrix per element, and not a euler rotation angles stored in a vector. 
					// NOTE: I suspect in order to do axial rotations in shader, we will need to store the "heading"
					// vector in Color vertex element perhaps... because with scaling allowed, we cannot easily get 
					// it out of world matrix.				
             		if (mMatrixArrayChanged)
             		{
	             		TV_3DMATRIX[] tvMatrices = Helpers.TVTypeConverter.ToTVMatrix (_matrixArray);
	             		// NOTE: SetMatrixArray() prevents SetGlobalPosition() from working.  This is because
	                	// SetMatrixArray() sets global matrices and not modelspace matrices.  Thus, 
	                	// SetMatrixArray() matrices MUST include any regional & cameraSpace offsets for each element!
	                	// The good aspect of this is that we can perform Axial Rotation matrix calculations
	                	// in camera space without having to worry about the global matrix of the minimesh.
	                	// TODO: element scaling seems to be ignored here...
	                	using (CoreClient._CoreClient.Profiler.HookUp("MinimeshGeometry.Assign Arrays"))
		             		_minimesh.SetMatrixArray (tvMatrices.Length, tvMatrices);
	             		mMatrixArrayChanged = false;
             		}
	             	
                }
             	else // non-axial type billboard or no billboard at all
             	{             

	                if (mPositionArrayChanged && _positionArray != null)
	                {
	                	using (CoreClient._CoreClient.Profiler.HookUp("MinimeshGeometry.Assign Arrays"))
	                	{
	                		TV_3DVECTOR[] tvvectors = Helpers.TVTypeConverter.ToTVVector (_positionArray);
	                		_minimesh.SetPositionArray (tvvectors.Length, tvvectors);
	                	}
	                	mPositionArrayChanged = false;
	                }
	             	if (mScaleArrayChanged && _scaleArray != null)
	                {
	             		using (CoreClient._CoreClient.Profiler.HookUp("MinimeshGeometry.Assign Arrays"))
	             		{
		                	TV_3DVECTOR[] tvvectors = Helpers.TVTypeConverter.ToTVVector (_scaleArray);
		                	_minimesh.SetScaleArray (tvvectors.Length, tvvectors);
	             		}
	                	mScaleArrayChanged = false;
	                }
	             	if (_isBillboard == false)
	             	{
	             		// if this is a billboard, never allow SetRotationArray to be called 
	             		// or even setting individual element rotations because it will break the rotation mode
		             	if (mRotationArrayChanged && _rotationArray != null)
		             	{
		             		// remember, not all minimesh elements are billboards.  Non billboard elements rely on 
		             		// user rotations and not tv computed billboard rotations.
		                    using (CoreClient._CoreClient.Profiler.HookUp("MinimeshGeometry.Assign Arrays"))
		                    {
		             			TV_3DVECTOR[] tvrotations = Helpers.TVTypeConverter.ToTVVector(_rotationArray);
		                    	_minimesh.SetRotationArray((int)tvrotations.Length, tvrotations);
		                    }
		                    mRotationArrayChanged = false;
		             	}
	             	}
 	                // NOTE: This branch can use SetGlobalPosition to apply global camera space position because we are using independant
 	                // position, scale and rotation arrays rather than matrixArray.  See notes in above
 	                // branch for more details.
	                _minimesh.SetGlobalPosition((float)matrix.M41, (float)matrix.M42, (float)matrix.M43);
             	}
             	             	
                _minimesh.Render();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("MinimeshGeometry.Render() - ERROR: Failed to render '{0}'. {1}", _id, ex.Message));
            }
    	} // end lock(mSyncLock)
        }
        #endregion

       

        //public MinimeshGeometry(int count)
        //{
        //    if (count <= 0) throw new ArgumentOutOfRangeException();
        //    _count = count;

        //    ReDim _color(0 To count - 1)
        //    ReDim _mat(0 To count - 1)
        //    ReDim _enable(0 To count - 1)
        //}


        //// billboard
        //    public MinimeshGeometry (string nodekey, BoundingBOx box, MTV3D65.TV_3DMATRIX[] mats, MINIMESH_INFO info, int[] colors)
        //    {
        //        if (mats== null) throw new ArgumentNullException();
        //        if (String.IsNullOrEmpty(nodekey)) throw new ArgumentNullException();

        //     _mini.EnableFrustumCulling(false);
        //                                                            _nodekey = nodekey;
        //        _id = nodekey;
        //        //_lodKey = info.LODGroupKey
        //        //_switchDistance = info.LODSwitchDistance

        //        _box = box;
        //        _mat = mats;

        //        _count = _mat.Length;

        //        ReDim _scaleArray(0 To _count - 1)

        //        ReDim _tmpScale(0 To _count - 1)
        //        ;


        //        _material = info.Material;
        //        _texture = info.TextureFile;
        //        _meshpath = info.MeshFile;

        //        _cullmode = info.cullMode;
        //        _isBillboard = info.isBillboard;
        //        _rotationMode = info.rotationMode;

        //        _alphaSort = info.AlphaSort;
        //        _alphatest = info.AlphaTest;
        //        _alphatestvalue = info.AlphaTestValue;

        //        _fadeuseTV3D = info.fadeUseTV3D;
        //        _fadeenabled = info.FadeEnabled;
        //        _fadestart = info.FadeStart;
        //        _fadedistance = info.FadeDistance;

        //        _fadestart = 64;
        //        _fadedistance = 128;

        //        _fadeDistanceReciprocal = 1 / _fadedistance // using this in a multiply is faster than a double divide
        //        _fadeEnd = _fadestart + _fadedistance

        //        if (colors == null) 
        //            _hasColor = false;
        //        else{
        //            if (colors.Length != _count) throw new ArgumentException("Array lengths of both arguments must match.");
        //        _hasColor = true;
        //        _color = colors;
        //    }

        //    ReDim _enable(0 To _count - 1)
        //        ;

        //    _isLoaded = true;
        //}


        //public Apply (MTV3D65.TVMiniMesh mini)
        //{
        //    If _isLoaded Then
        //        ' TODO: load any textures

        //        If _count > 0 Then
        //            mini.SetMaxMeshCount(_count)
        //            //If _isBillboard Then
        //            // for billboards we must _not_ use SetMatrixArray because it cant be used with SetRotationMode. 
        //            // I consider it a bug but Sylvain says SetMatrixArray overrides it by design. Whatever Sylvain.
        //            For i As Int32 = 0 To _count - 1
        //                mini.SetPosition(PositionArray(i).x, PositionArray(i).y, PositionArray(i).z, i)
        //                mini.SetScale(ScaleArray(i).x, ScaleArray(i).y, ScaleArray(i).z, i)
        //            Next
        //            'Else
        //            'mini.SetMatrixArray(_count, _mat)
        //            'End If
        //            mini.SetColorMode(_hasColor)
        //            If _hasColor Then
        //                mini.SetColorArray(_count, _color)
        //            End If

        //            If _hasEnable Then
        //                mini.SetEnableArray(_count, _enable)
        //            End If
        //            For i As Int32 = 0 To _count - 1
        //                mini.EnableMiniMesh(i, True)
        //            Next

        //            If _isBillboard Then
        //                mini.SetRotationMode(_rotationMode)
        //            End If

        //            mini.SetCullMode(_cullmode)


        //            'mini.SetPosition(BoundingBox.GetCenter(_box).x, _box.Min.y, BoundingBox.GetCenter(_box).z)
        //            mini.SetPosition(BoundingBox.GetCenter(_box).x, BoundingBox.GetCenter(_box).y, BoundingBox.GetCenter(_box).z)

        //            mini.SetAlphaTest(_alphatest, _alphatestvalue)
        //            mini.SetAlphaSort(_alphaSort)

        //            If _fadeenabled Then
        //                If _fadeusetv3d Then
        //                    mini.SetFadeOptions(True, _fadedistance, _fadestart)
        //                Else
        //                    mini.SetFadeOptions(False, 0, 0)
        //                    For i As Int32 = 0 To _count - 1
        //                        _scaleArray(i) = ScaleArray(i)
        //                    Next
        //                End If
        //            End If

        //            _materialID = TV3DUtility.CreateMaterial(_material)
        //            mini.SetMaterial(_materialID)

        //            mini.Enable(True)
        //        Else
        //            mini.Enable(False)
        //        End If

        //        _minimesh = mini
        //    End If
        //}


        #region IBoundVolume Members
        protected override void UpdateBoundVolume()
        {

        	if (_resourceStatus != PageableNodeStatus.Loaded) return;

            lock (mSyncLock)
            {
            	_box.Reset ();
	            
	//            float radius = 0f;
	//            TV_3DVECTOR min, max, center;
	//            min = new TV_3DVECTOR(0, 0, 0);
	//            max = new TV_3DVECTOR(0, 0, 0);
	//            center = new TV_3DVECTOR(0, 0, 0);
	//            // model space bounding box
	//            
	//            _minimesh.GetBoundingBox(ref min, ref max);
	//            _box = new BoundingBox(min.x, min.y, min.z, max.x, max.y, max.z);
	//
	//            _minimesh.GetBoundingSphere(ref center, ref radius);
	            
	            
	            // NOTE: TVMiniMesh.GetBoundingBox() call above seems to fail.  Iterating thru each element
	            // and combining the scaled and positioned default mesh3d.BoundingBox works
	            System.Diagnostics.Debug.Assert (_mesh.TVResourceIsLoaded);
	            BoundingBox meshBase = _mesh.BoundingBox;
	            
	            if (mEnableArray != null)
		            for (int i = 0; i < mEnableArray.Length; i++)
		            {
	            		//System.Diagnostics.Debug.Assert (mInstanceCount <= mEnableArray.Length, "MinimeshGeometry.UpdateBoundVolume()");
		            	if (mEnableArray[i] == 0) continue;
		            	// TODO: when using SetMatrixArray, the position, scale and rotation are not updated
		            	//       so there's no way to get a correct bounding box 
		            	// TODO: rotation is not being taken into account
		            	BoundingBox tmp = Keystone.Types.BoundingBox.Scale (meshBase, _scaleArray[i]);
		            	tmp.Translate (_positionArray[i]);
		            	_box.Combine (tmp);
		            }
	            
	            _sphere = new BoundingSphere(_box);
	            DisableChangeFlags(Keystone.Enums.ChangeStates.BoundingBox_TranslatedOnly | Keystone.Enums.ChangeStates.BoundingBoxDirty);
            }
        }
        #endregion



        #region IDisposable Members
        protected override void DisposeUnmanagedResources()
        {
            try
            {
            	//System.Diagnostics.Debug.WriteLine ("MinimeshGeometry.DisposeUnmanagedResources() - RefCount == " + this.RefCount.ToString());
                if (_appearance != null)
                    Resource.Repository.DecrementRef(_appearance);

                if (_mesh != null)
                	Resource.Repository.DecrementRef (_mesh);
                
                // TODO: we must ensure if this is unloaing that we are not currently rendering and this means we need to synclock our
                //       loading state variable
                if (TVResourceIsLoaded)
	                if (_minimesh != null)
                {
    	                _minimesh.Destroy();
                }
                //Debug.WriteLine("Minimesh.DisposeUnmanagedResources() - Minimesh Disposed Successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Minimesh.DisposeUnmanagedResources() - " + ex.Message);
            }
            finally 
            {
                base.DisposeUnmanagedResources();
            }
        }
        #endregion
    }
}