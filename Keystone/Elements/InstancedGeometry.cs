using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using Keystone.Appearance;
using Keystone.Collision;
using Keystone.Culling;
using Keystone.Entities;
using Keystone.Enum;
using Keystone.IO;
using Keystone.Resource;
using Keystone.Shaders;
using Keystone.Traversers;
using Keystone.Types;
using KeyCommon.Traversal;
using MTV3D65;
using Direct3D = Microsoft.DirectX.Direct3D;
using DirectX = Microsoft.DirectX;

namespace Keystone.Elements
{
	/// <summary>
	/// Description of InstancedGeometry.
	/// </summary>
	public class InstancedGeometry : Geometry
	{
		public const int InstancesPerBatch = 64; //224; // when using more complex shader with shadowmapping, 248;
        protected const int StartRegister = 256 - InstancesPerBatch;
        
        protected Direct3D.Device Device;
        protected Direct3D.VertexBuffer mVertexBuffer;
        protected Direct3D.IndexBuffer mIndexBuffer;
        protected Direct3D.PrimitiveType mPrimitiveType;
        protected Direct3D.VertexDeclaration mVertexDeclaration;

        protected DXObjects.PositionNormalTextureInstance[] mVertices;
        protected short[] mIndices;
        
        protected int mVertexCount;
        protected int mIndexCount;
        protected int mPrimitiveCount;

        protected string _meshOrTextureResourcePath;
        public DirectX.Vector4[] mPositions;
                
        public int ViewedInstances { get; protected set; }
        public uint MaxInstancesCount { get; internal set; }
        
        
        
		internal InstancedGeometry(string id) : base (id)
		{
			mPrimitiveType = Direct3D.PrimitiveType.TriangleList;
		}
		
		#region ITraversable Members
        public override object Traverse(ITraverser target, object data)
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
            Settings.PropertySpec[] properties = new Settings.PropertySpec[3 + tmp.Length];
            tmp.CopyTo(properties, 3);

            // precache == true will result in all minimesh elements being created ahead of time with each element disabled until needed.
//            properties[1] = new Settings.PropertySpec("billboardwidth", _billboardWidth.GetType().Name);
//            properties[2] = new Settings.PropertySpec("billboardheight", _billboardHeight.GetType().Name);
//            properties[3] = new Settings.PropertySpec("centerheight", _billboardCenterHeight.GetType().Name);
            properties[0] = new Settings.PropertySpec("meshortexresource", typeof(string).Name);
//            properties[6] = new Settings.PropertySpec("usecolors", typeof(bool).Name);
//            properties[7] = new Settings.PropertySpec ("alphasort", typeof(bool).Name);
            properties[1] = new Settings.PropertySpec("maxcount", MaxInstancesCount.GetType().Name);
            properties[2] = new Settings.PropertySpec("primitivetype", typeof(int).Name);

            // TODO: hull isnt necessary for a minimesh which ive decided is only used for instance rendereing.
            //    Hull = new ConvexHull(_filePath);
            //    Hull = ConvexHull.ReadXml(xmlNode);
            if (!specOnly)
            {
//                properties[0].DefaultValue = mPreCacheMaxElements;
//                properties[1].DefaultValue = _billboardWidth;
//                properties[2].DefaultValue = _billboardHeight;
//                properties[3].DefaultValue = _billboardCenterHeight;
                properties[0].DefaultValue = _meshOrTextureResourcePath;
//                properties[6].DefaultValue = _useColors;
//                properties[7].DefaultValue = _alphaSort;
                properties[1].DefaultValue = MaxInstancesCount;
                properties[2].DefaultValue = (int)mPrimitiveType;
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
//                    case "billboardwidth":
//                        _billboardWidth = (float)properties[i].DefaultValue;
//                        break;
//                    case "billboardheight":
//                        _billboardHeight = (float)properties[i].DefaultValue;
//                        break;
//                    case "centerheight":
//                        _billboardCenterHeight = (bool)properties[i].DefaultValue;
//                        break;
                    case "meshortexresource":
                        _meshOrTextureResourcePath = (string)properties[i].DefaultValue;
                        break;
//                    case "usecolors":
//                        _useColors = (bool)properties[i].DefaultValue;
//                        break;
//                    case "alphasort":
//                        _alphaSort = (bool)properties[i].DefaultValue;
//                        break;
					case "maxcount":
                        MaxInstancesCount = (uint)properties[i].DefaultValue;
                        break;
                    case "primitivetype":
                        mPrimitiveType = (Direct3D.PrimitiveType)(int)properties[i].DefaultValue;
                        break;
                }
            }
        }
        #endregion
        
        #region IPageableNode Members
        public override void UnloadTVResource()
        {
        	DisposeManagedResources();
        }
        
       
        public override void LoadTVResource()
        {
            // TODO: if the mesh's src file has changed such that it's same name, but different geometry
            // then how do we detect that so we can unload the previous mesh and load in the new mesh?
            if (mVertexBuffer != null)
            {
                try
                {
                	DisposeManagedResources();
                }
                catch
                {
                    Trace.WriteLine("InstancedGeometry.LoadTVResource() - Error on DisposeManagedResources() - InstancedGeometry path == " + _id);
                }
            }
            
            try
            {
                if (Mesh3d.IsPrimitive(_meshOrTextureResourcePath))
                {
	            	
                	// TODO: these dimensions should be passed in or read from settings.  For cube, the
                	// dimensions should match those of structure's cube/tile dimensions.
                    float halfWidth = 3.0f / 2f;
        			float halfHeight = 1.0f / 2f;
        			float halfDepth = 2.5f / 2f;
        
                	// NOTE: By reaching this method we know that the primitive does not already exist in Repository cache
                    // parse the primitive type and attributes and build it
                    InitializeCubeVertices(halfWidth, halfHeight, halfDepth);
	                    
	                
                }
               	else 
               	{
               		KeyCommon.IO.ResourceDescriptor descriptor = new KeyCommon.IO.ResourceDescriptor(_meshOrTextureResourcePath);
                	InitializeMeshVertices(descriptor);
               	}
               	
               	
               	InitializeVertexBuffer();
               	
	            mPositions = new DirectX.Vector4[MaxInstancesCount];

	            _tvfactoryIndex = mVertexBuffer.GetHashCode();

                
                // assign values to Properties so they get assigned to _tvMesh                
//				PointSpriteSize = mPointSpriteScale;
//                TextureClamping = mTextureClamping;

                // TODO: .ComputeNormals() must be called if you want binormal and tangents to be
                //       computed and your vertex format must support those so they can be
                //       passed in semantic to shader
                // TODO: however in general, we might not want to recompute normals and lose the existing
                //       normals of the model!  This could result in  smoothing group style normals being lost
                //       and replaced with faceted normals.
//                ComputeNormals();
                CullMode = _cullMode;
                  //_tvmesh.ComputeNormalsEx(fixSeems, threshold); 
				//_tvmesh.ComputeTangents(); // in future build i havent upgraded yet?
                // TODO: use this.CullMode, this.InvertNormals or this.ComputeNormals should all
            	// be set as flags that LoadTVResource() will apply.  Search for all instances in Mesh3d where
            	// we try to tvmesh.SetCullmode or tvmesh.InvertNormals or tvmesh.ComputeNormals directly and 
            	// remove them and switch to flags
            
                //_tvmesh.GenerateUV(); 

                SetChangeFlags(Keystone.Enums.ChangeStates.GeometryAdded | 
                    Keystone.Enums.ChangeStates.AppearanceParameterChanged |
                    Keystone.Enums.ChangeStates.BoundingBoxDirty, Keystone.Enums.ChangeSource.Self);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("error on LoadTVResource() - InstancedGeometry path == " + _id + ex.Message);
                throw ex;
            }
            // TODO: i think convexHull needs to be apart of a Model, not Mesh3d.  Right?  Model.Geometry,  Model.Hull
            // TODO: how do we load the convexhull if this LoadTVResource() doesnt get called from the server? because
            // either mesh3d itself isnt loaded or because we skip the method call on the server
            // and if mesh3d is not even laoded, then this ConvexHull property doesnt exist so where does it sit?
            // TODO: also i need to go back and look at how jiglibx handles convex hulls and whether they do in fact 
            // re-compute world hull every itteration..  In fact
            // our jiglibx PhysicsBody property could be something we use instead of convexhull property here..?
            // UPDATE: jiglibx uses Primitive object which
            //  and inside of CollisionSkin object it maintains lists of local space , world space and the previous frame's world space collision skins
            // it uses lists because you can have multiple collision skins to make up a single object such as three spheres to represent a football
        }

        
         // TODO: if we could move this entire initialize into each specific type of vertex declaration perhaps...
        //       then tie in loading from .obj and tvmesh... essentially what we're talking here is a loader
        //       anyway
        private void InitializeCubeVertices(float halfWidth, float halfHeight,float halfDepth)
        {
            float dec = 0.002f;
            mPrimitiveType = Direct3D.PrimitiveType.TriangleList;
            
            Microsoft.DirectX.Vector3 up, forward, right;
            up.X = 0;
            up.Y = 1;
            up.Z = 0;
            right.X = 1;
            right.Y = 0;
            right.Z = 0;
            forward.X = 0;
            forward.Y = 0;
            forward.Z = 1;
            
            Microsoft.DirectX.Vector2 uv;
            uv.X = 0;
            uv.Y = 0;

            // TODO: use option for both a cube texture and our normal texture in shader? problem with cube textureis
			//       the shader uses positionMS for x,y,z for u,v,w params and if not unit cube, the w param (position.z) will
			//       be no good.  so it may not be useful for our purposes here.			
            mVertices = new DXObjects.PositionNormalTextureInstance[] 
            {
                new DXObjects.PositionNormalTextureInstance(new Microsoft.DirectX.Vector3(-halfWidth, -halfHeight, -halfDepth), up, uv),
                new DXObjects.PositionNormalTextureInstance(new Microsoft.DirectX.Vector3(halfWidth, -halfHeight, -halfDepth), up, uv),
                new DXObjects.PositionNormalTextureInstance(new Microsoft.DirectX.Vector3(halfWidth, halfHeight, -halfDepth), up, uv),
                new DXObjects.PositionNormalTextureInstance(new Microsoft.DirectX.Vector3(-halfWidth, halfHeight, -halfDepth), up, uv),
                new DXObjects.PositionNormalTextureInstance(new Microsoft.DirectX.Vector3(-halfWidth, -halfHeight, halfDepth), up, uv),
                new DXObjects.PositionNormalTextureInstance(new Microsoft.DirectX.Vector3(halfWidth, -halfHeight, halfDepth), up, uv),
                new DXObjects.PositionNormalTextureInstance(new Microsoft.DirectX.Vector3(halfWidth, halfHeight, halfDepth), up, uv),
                new DXObjects.PositionNormalTextureInstance(new Microsoft.DirectX.Vector3(-halfWidth, halfHeight, halfDepth), up, uv),
                	

                new DXObjects.PositionNormalTextureInstance(new Microsoft.DirectX.Vector3(-halfWidth - dec, -halfHeight - dec, -halfDepth - dec), up, uv),
                new DXObjects.PositionNormalTextureInstance(new Microsoft.DirectX.Vector3(halfWidth - dec, -halfHeight - dec, -halfDepth - dec), up, uv),
                new DXObjects.PositionNormalTextureInstance(new Microsoft.DirectX.Vector3(halfWidth - dec, halfHeight - dec, -halfDepth - dec), up, uv),
                new DXObjects.PositionNormalTextureInstance(new Microsoft.DirectX.Vector3(-halfWidth - dec, halfHeight - dec, -halfDepth - dec), up, uv),
                new DXObjects.PositionNormalTextureInstance(new Microsoft.DirectX.Vector3(-halfWidth - dec, -halfHeight - dec, halfDepth - dec), up, uv),
                new DXObjects.PositionNormalTextureInstance(new Microsoft.DirectX.Vector3(halfWidth - dec, -halfHeight - dec, halfDepth - dec), up, uv),
                new DXObjects.PositionNormalTextureInstance(new Microsoft.DirectX.Vector3(halfWidth - dec, halfHeight - dec, halfDepth - dec), up, uv),
                new DXObjects.PositionNormalTextureInstance(new Microsoft.DirectX.Vector3(-halfWidth - dec, halfHeight - dec, halfDepth - dec), up, uv)
            };
            
            // DX is CW order by default
            mIndices = new short[]
            {
                0, 2, 1,  0, 3, 2,
                1, 6, 5,  1, 2, 6,
                0, 4, 7,  0, 7, 3,
                3, 6, 2,  3, 7, 6,
                4, 5, 6,  4, 6, 7,
                0, 1, 5,  0, 5, 4,
            
                8, 10, 9,  8, 11, 10,
                8, 14, 13,  9, 10, 14,
                8, 12, 15,  8, 15, 11,
                11, 14, 10,  11, 15, 14,
                12, 13, 14,  12, 14, 15,
                8, 9, 13,  8, 13, 12
            };
        }
        
        private void InitializeMeshVertices(KeyCommon.IO.ResourceDescriptor descriptor)
        {
        	// obj or x or tvm?
        	bool loadTextures = true;
        	bool loadMaterials = true;
        	DefaultAppearance appearance;
        	TVMesh tvm = null;
        	
        	Loaders.WaveFrontObj obj;

        	// if obj, we still haven't yet got indexed triangle list generation implemented yet
        	string[] dummy;
        	if (descriptor.IsArchivedResource)
        	{
            	obj = Loaders.WavefrontObjLoader.ParseWaveFrontObj(descriptor.ModName, descriptor.EntryName, true, true, out dummy, out dummy);
        	}
        	else
        	{
        		obj = Loaders.WavefrontObjLoader.ParseWaveFrontObj(Core.FullNodePath(descriptor.EntryName), true, true, out dummy, out dummy);
        	}

        	if (obj.IndexedVertices == null || obj.IndexedVertices.Length == 0) return;
        	mIndices = obj.Indices;
        	
            mVertices = new DXObjects.PositionNormalTextureInstance[obj.IndexedVertices.Length];

        	Tuple<uint, uint, uint>[] tuples = obj.IndexedVertices;

        	for (int i = 0; i < mVertices.Length; i++)
        	{
        		// construct dx vertex 
        		Tuple<uint, uint, uint> t = tuples[i]; 
        		
        		Microsoft.DirectX.Vector3 pos, normal;
        		Microsoft.DirectX.Vector2 uv;
        		
        		pos.X = obj.Points[(int)t.Item1].x;
        		pos.Y = obj.Points[(int)t.Item1].y;
        		pos.Z = obj.Points[(int)t.Item1].z;
        		
        		normal.X = obj.Normals[(int)t.Item2].x;
        		normal.Y = obj.Normals[(int)t.Item2].y;
        		normal.Z = obj.Normals[(int)t.Item2].z;
        		
        		// if wavefront obj does contain UVs 
        		if (obj.UVs != null)
        		{
        			uv.X = obj.UVs[(int)t.Item3].x;
        			uv.Y = obj.UVs[(int)t.Item3].y;
        		}
        		else // no uv's supplied in wavefront obj file (this is not recommended!) but for atlas textures, we can supply an int index instead)
        		{
        			uv.X = 0;
        			uv.Y = 0;
        		}
        		
        		mVertices[i] = new Keystone.DXObjects.PositionNormalTextureInstance(pos, normal, uv);
        	}         
        }
        
        protected void InitializeVertexBuffer()
        {
        	Device = new Direct3D.Device(CoreClient._CoreClient.Internals.GetDevice3D());
            mVertexDeclaration = new Direct3D.VertexDeclaration(Device, DXObjects.VertexHelper.GetElements<DXObjects.PositionNormalTextureInstance>());

            mVertexCount = mVertices.Length;
            mIndexCount = mIndices.Length;

            var totalVertices = mVertexCount * InstancesPerBatch;
            var totalIndices = mIndexCount * InstancesPerBatch;

            mPrimitiveCount = mIndexCount;
            switch (mPrimitiveType)
            {
                case Direct3D.PrimitiveType.LineList:       mPrimitiveCount /= 2; break;
                case Direct3D.PrimitiveType.LineStrip:      mPrimitiveCount -= 1; break;
                case Direct3D.PrimitiveType.TriangleFan:    mPrimitiveCount -= 2; break;
                case Direct3D.PrimitiveType.TriangleList:   mPrimitiveCount /= 3; break;
                case Direct3D.PrimitiveType.TriangleStrip:  mPrimitiveCount -= 2; break;
            }

            Type vertexType = typeof(DXObjects.PositionNormalTextureInstance);
            mVertexBuffer = new Direct3D.VertexBuffer(vertexType, totalVertices, Device, Direct3D.Usage.WriteOnly, Direct3D.VertexFormats.None, Direct3D.Pool.Managed);
            mIndexBuffer = new Direct3D.IndexBuffer(typeof(int), totalIndices, Device, Direct3D.Usage.WriteOnly, Direct3D.Pool.Managed);

            var vertexArray = (DXObjects.PositionNormalTextureInstance[]) mVertexBuffer.Lock(0, vertexType, Direct3D.LockFlags.None, totalVertices);
            var indexArray = (int[]) mIndexBuffer.Lock(0, typeof(int), Direct3D.LockFlags.None, totalIndices);
            
            // NOTE: here this uses shader instancing where verts and indices for each instance are added to buffers.
            //       we're not doing hardware instancng where we only need one set of verts and indices and then
            //       just array of transforms
            for (int i = 0; i < InstancesPerBatch; i++)
            {
                for (int j = 0; j < mVertexCount; j++)
                {
                	int index = i * mVertexCount + j;
                    vertexArray[index] = mVertices[j];
                    vertexArray[index].InstanceIndex = i;
                }
                for (int j = 0; j < mIndexCount; j++)
                    indexArray[i * mIndexCount + j] = mIndices[j] + i * mVertexCount;
            }
            mIndexBuffer.Unlock();
            mVertexBuffer.Unlock();
        }
        
        // TODO: why is a resourcePath passed in?  currently we do use it to save to a temp file on disk
        // which we can then import into archive and overwrite existing or add as a variation copy with
        // different name
        // TODO: however should we support a path that is resourcedescriptor to archive and directly
        // save to the archive from here?
        // TODO: will SaveTVM work to a memory stream? similar to the mem data path when loading tvm?
        public override void SaveTVResource(string resourcePath)
        {
            // _relativeResourcePath = resourcePath;
            if (_resourceStatus == PageableNodeStatus.Loaded && TVResourceIsLoaded)
                // TODO: need to create a full path from the relative path
                // string fullPath = Path.Combine (Core._Core.Mod_Path, _relativeResourcePath);
                throw new NotImplementedException("IndexedGeometry.GetTriable()");
        }
        #endregion
        
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="positions">World positions in camera space</param>
        /// <param name="rotations"></param>
        /// <param name="count">Number of array elements to use.</param>
        /// <remarks>
        /// Positions must be passed in camera space world coords.  No world matrix will be applied to the shader.  
        /// This does mean that no scaling will be applied either and for efficiency, this is best.
        /// </remarks>
        internal virtual void AddInstances (Vector3d[] positions, Vector3d[] rotations, int count)
        {
        	#if DEBUG
        	if (positions == null || rotations == null) throw new ArgumentNullException();
        	if (positions.Length > rotations.Length) throw new ArgumentOutOfRangeException ();
        	if (count < 0 || count > positions.Length) throw new ArgumentOutOfRangeException();
        	#endif
        	
        	if (ViewedInstances + count > MaxInstancesCount)
        		count = (int)MaxInstancesCount - ViewedInstances;
        	
        	
        	for (int i = 0; i < count; i++)
        	{
	        	DirectX.Vector4 vec;
	        	vec.X = (float)positions[i].x;
	        	vec.Y = (float)positions[i].y;
	        	vec.Z = (float)positions[i].z;
	        	vec.W = (float)rotations[i].y;
    
	        	mPositions[ViewedInstances + i] = vec;
        	}
        	ViewedInstances += count;
        	SetChangeFlags (Keystone.Enums.ChangeStates.BoundingBoxDirty, Keystone.Enums.ChangeSource.Self);
        }
        
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="position">World position in camera space</param>
        /// <param name="phiRadians"></param>
        /// <remarks>
        /// Position must be passed in camera space world coords.  No world matrix will be applied to the shader.  
        /// This does mean that no scaling will be applied either and for efficiency, this is best.
        /// </remarks>
        internal void AddInstance(Vector3d position, float phiRadians)
        {
        	DirectX.Vector4 vec;
        	vec.X = (float)position.x;
        	vec.Y = (float)position.y;
        	vec.Z = (float)position.z;
        	vec.W = phiRadians; 
        	        	
            // phi is a y axis rotation that is one of 4 different rotations. {0, 90, 180, 270}
        	mPositions[ViewedInstances] = vec;
            
        	// TODO: add atlas texture index in UV coord.  We might
        	// be able to sychronize the UV lookup index with the auto-tile rule that is applied? but probably not easily
        	// since auto-tiling causes adjacents to change
        	// -initially, just get the atlas texture to render
        	        	
        	ViewedInstances++;
        	SetChangeFlags (Keystone.Enums.ChangeStates.BoundingBoxDirty, Keystone.Enums.ChangeSource.Self);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="position">World position in camera space</param>
        /// <param name="phi"></param>
        /// <remarks>
        /// Position must be passed in camera space world coords.  No world matrix will be applied to the shader.  
        /// This does mean that no scaling will be applied either and for efficiency, this is best.
        /// </remarks>
        internal void AddInstance(DirectX.Vector3 position, float phi)
        {
            // phi is a y axis rotation that is one of 4 different rotations. {0, 90, 180, 270}
        	mPositions[ViewedInstances] = new DirectX.Vector4(position.X, position.Y, position.Z, phi);
            ViewedInstances++;
        	
            SetChangeFlags (Keystone.Enums.ChangeStates.BoundingBoxDirty, Keystone.Enums.ChangeSource.Self);
        }
               
        internal void ClearVisibleInstances()
        {
        	ViewedInstances = 0;
        }
        
        #region Geometry Member
        public override int CullMode // TODO: cullMode like  TextureClamping should be set from Appearance
        {
            set
            {
                //throw new NotImplementedException("IndexedGeometry.GetTriable()");
            }
        }

        public override int GetVertexCount(uint groupIndex)
        {
            throw new NotImplementedException("IndexedGeometry.GetTriable()");
        }

        public override int VertexCount
        {
            get { throw new NotImplementedException("IndexedGeometry.GetTriable()"); }
        }

        public override int GroupCount
        {
            get { throw new NotImplementedException("IndexedGeometry.GetTriable()"); }
        }

        public override int TriangleCount
        {
            get { throw new NotImplementedException("IndexedGeometry.GetTriable()"); }
        }

        /// <summary>
        /// Very expensive call.  
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int[] GetTriangleIndices(int index)
        {
			throw new NotImplementedException("IndexedGeometry.GetTriable()");
        }

        public Triangle[] GetTriangles()
        {
		throw new NotImplementedException("IndexedGeometry.GetTriable()");
        }

        public Triangle GetTriangle(int index)
        {
        	throw new NotImplementedException("IndexedGeometry.GetTriable()");
        }

        public void SetVertexUV(int index, float u1, float v1, float u2, float v2)
        {
            throw new NotImplementedException("IndexedGeometry.GetTriable()");
        }

        /// <summary>
        /// Modifies U1 and V1 and keeps the existing U2 and V2
        /// </summary>
        /// <param name="index"></param>
        /// <param name="u"></param>
        /// <param name="v"></param>
        public void SetVertexUV (int index, float u, float v)
        {
            throw new NotImplementedException("IndexedGeometry.GetTriable()");
        }

        // NOTE: SetVertex does not update TVMesh bounding box so TV will cull this mesh
        // even if our own culling determined the mesh to be visible.  So tvmesh.EnableFrustumCulling(false)
        // or force a BoundingVolumeIsDirty = true
        public void SetVertexUVs(int[] index, float[] u, float[] v)
        {
            if (index == null || u == null || v == null) throw new ArgumentNullException ();
            if (index.Length != u.Length || u.Length != v.Length) throw new ArgumentOutOfRangeException("All three array paramters must have same length.");

            for (int i = 0; i < index.Length; i++)
            {
                SetVertexUV (index[i], u[i], v[i]);
            }
        }

        public Vector3d[] GetVertexPositions()
        {
            throw new NotImplementedException("IndexedGeometry.GetTriable()");
        }


        public Vector3d GetVertex(int index)
        {
            throw new NotImplementedException("IndexedGeometry.GetTriable()");
        }

        public void SetVertex(int index, Vector3d position) 
        {
             SetVertex(index, (float)position.x, (float) position.y, (float)position.z);
        }

        public void SetVertex(int index, float x, float y, float z)
        {
            throw new NotImplementedException("IndexedGeometry.GetTriable()");
            SetChangeFlags (Enums.ChangeStates.BoundingBoxDirty , Enums.ChangeSource.Self );
        }

        public void SetVertex (int index, float x, float y, float z, float normalx, float normaly, float normalz, float u, float v)
        {
           throw new NotImplementedException("IndexedGeometry.GetTriable()");
        }
        public void SetVertex (int index, float x, float y, float z, float normalx, float normaly, float normalz, float u, float v, float u2, float v2, int color)
        {
           throw new NotImplementedException("IndexedGeometry.GetTriable()");
        }

        public void SetVertices(Vector3d[] position, Vector3d[] normal, Vector2f[] uv)
        {
            throw new NotImplementedException("IndexedGeometry.GetTriable()");

            SetChangeFlags (Enums.ChangeStates.BoundingBoxDirty , Enums.ChangeSource.Self );
        }


        // The start/end should alredy be in ModelSpace if the Picker has done it's job.  
        // However for billboards it wont have taken into account the last minute rotate to face camera
        // transform (see Billboard.cs AdvancedCollide())
        internal override PickResults AdvancedCollide(Vector3d start, Vector3d end, KeyCommon.Traversal.PickParameters parameters)
        {
            // note: picking is done in model space so we use Identity always. 
			// InstancedGeometry.cs is always maintained at Identity.            
            PickResults pr = new PickResults();
            return pr; // TEMP HACK disable AdvancedCollide until we implement InstancedGeometry rendering
            
//			Ray ray = new Ray (start,  end - start);
//
//            if (mPrimitiveType == Direct3D.PrimitiveType.LineList)
//            {
//                const double pickHalo = 180.0d;	// minimum range to consider a point
//                int foundIndex = -1; double foundIntersectionDistance = double.MaxValue;
//                BoundingSphere sphere = new BoundingSphere (Vector3d.Zero(), pickHalo);
//                
//                int vertextCount = templateVertexCount * ViewedInstances;
//                
//                // TODO: the entity itself that holds these pointsprites if each pointsprite
//                // represents a seperate start IS a digest.  So this pointsprite implementation ends up
//                // being just another way to pick them.
//                
//                for (int i = 0; i < vertextCount ; i++)
//                {
//                    Vector3d point = GetVertex(i);
//                    
//                    _sphere.Center = point; 
//                    
//                    double distanceFrontIntersection = 0;
//					double distanceBackSideIntersection = 0;
//                    if (sphere.Intersects (ray, ref distanceFrontIntersection))
//                    {
//                    	
//                    	// TODO: ray sphere intersection seems broken
//                    	// TODO: however we could also have a problem with our start and end vectors not being in model space which means they should be 
//                    	// about origin because our starfield is moving with the camera
//	                    if (distanceFrontIntersection < foundIntersectionDistance)
//	                    {
//	                        foundIntersectionDistance = distanceFrontIntersection;
//	                        foundIndex = i;
//	                        pr.HasCollided = true;
//	                        // TODO: we can't early exit because another vertex might be closer 
//	                    }
//                    }   
//                }
//   
//                
//                if (pr.HasCollided)
//                {
//                	pr.FaceID = foundIndex;  // for pointsprites, faceID and vertexID are the same thing
//                    pr.VertexID = foundIndex;
//                    pr.VertexCoord = GetVertex (foundIndex); // for pointsprites, impactPoint and vertexcoord are same thing
//                    pr.ImpactPointLocalSpace =  pr.VertexCoord ;  
//                    pr.ImpactNormal = -ray.Direction; // Helpers.TVTypeConverter.FromTVVector(tvResult.vCollisionNormal());
//                    pr.CollidedObjectType = PickAccuracy.Geometry; // TODO: can we say more specifically POINTSPRITE? so traveser can handle this result differently if necessary? perhaps associating it with a Digest?
//                    
//                    System.Diagnostics.Debug.WriteLine ("InstancedGeometry.AdvancedCollided() - PointSprite index found = " + foundIndex);
//                }
//            }
//            else
//            {
//   	            TV_COLLISIONRESULT tvResult = Helpers.TVTypeConverter.CreateTVCollisionResult();
//            	// Dec.11.2013 - after a tvmesh is loaded, the first tvmesh.Render() must be called before AdvancedCollision will work.  This is a known TV3D bug.
//            	_tvmesh.AdvancedCollision(Helpers.TVTypeConverter.ToTVVector(start), 
//            	                          Helpers.TVTypeConverter.ToTVVector(end), 
//            	                          ref tvResult, 
//            	                          PickResults.ToTVTestType(parameters.Accuracy));
//
//                pr.HasCollided = tvResult.bHasCollided;
//                if (pr.HasCollided)
//                {
//	                #if DEBUG
//                	if (this.ID == @"common.zip|actors/infantry_bot_mk4.obj")
//                	{
//                		System.Diagnostics.Debug.WriteLine (System.DateTime.Now.ToString() + " colliding successful");
//                	}
//                	#endif
//                    // Debug.WriteLine("Face index = " + tvResult.iFaceindex.ToString());
//                    // Debug.WriteLine("Distance = " + tvResult.fDistance.ToString());
//                    // Debug.WriteLine("Tu = " + tvResult.fTexU.ToString());
//                    // Debug.WriteLine("Tv = " + tvResult.fTexV.ToString());
//                    // Debug.WriteLine("Fu = " + tvResult.fU.ToString());
//                    // Debug.WriteLine("Fv = " + tvResult.fV.ToString());
//                    
//                    pr.FaceID = tvResult.iFaceindex;
//                   
//                    pr.ImpactPointLocalSpace = Helpers.TVTypeConverter.FromTVVector(tvResult.vCollisionImpact);
//                    pr.ImpactNormal = -ray.Direction; // Helpers.TVTypeConverter.FromTVVector(tvResult.vCollisionNormal); // 
//                    pr.CollidedObjectType = PickAccuracy.Geometry;
//                }
//                #if DEBUG
//                else 
//                {
//                	if (this.ID == @"common.zip|actors/infantry_bot_mk4.obj")
//                	{
//                		//System.Diagnostics.Debug.WriteLine (System.DateTime.Now.ToString() + " colliding failed.");
//                	}
//                }
//                #endif
//            }
//            return pr;
        }
        
        // TODO: can this be internal scope for all Geometry versions of Render(), AdvancedCollide and Update
        internal override void Render(Matrix matrix, Scene.Scene scene, Model model, double elapsedSeconds)
        {
    		lock (mSyncRoot)
            {
            
			try
            {
              
            	// TODO: it should be impossible for _resourceStatus to be "Loaded" before it's actually loaded
            	//       HOWEVER if paging out, we could start to render here first since it's not synclocked and then
            	//       while minimesh.Render() we page out and set _resourceStatus to Unloading but we're already in .Render()!
            	
            	// NOTE: we check PageableNodeStatus.Loaded and NOT TVResourceIsLoaded because that 
            	// TVIndex is set after Scene.CreateMesh() and thus before we've finished adding vertices
            	// via .AddVertex()  or .SetGeometry() or even loaded the mesh from file.
                if (_resourceStatus != PageableNodeStatus.Loaded ) return;
                if (ViewedInstances == 0 || model.Appearance == null || model.Appearance.Shader == null || model.Appearance.Shader.PageStatus != PageableNodeStatus.Loaded) return;

                // TODO: we should i think provide an overloaded Render() where we can
                // override the Appearance by passing one in?  Typically we can clone the 
                // appearance so all groups match up, then we can do things like alter the
                // material alpha or ambience, etc, however ambience should be done with
                // a shader parameter.  
                // i think supplying an override appearance is the easiest way...
                // one of the downsides is determining which ones will use the alternate
                // and how to create that appearance.  it would be so much better if we
                // could override this in some other fashion...but i just cant really think of a way
                // to do that. 
                // I mean lets say we have a selected entity in the world, again here is an entity
                // where i may want to alter the default rendering appearance... 
                // the thing is, if i modify the existing, i lose the original settings 
                // it wouldnt be bad if there was a way to easily restore original settings.
                // Because we could run functions across the entire graph to modify all appearances
                // in some simple way, but then how do we restore them?
                // is there some way to "remember" the original state or a way to
                // pass overrides to the default state in such a way that we can still
                // take advantage of hashcode test of changes?
                // - because perhaps the .Appearance can store overrides and if the overrides
                //   havent changed then hashcode will be same and no need to modify anything...
                // - also, we could conceivably apply the overrides during cull so that we can
                //   still add them to proper buckets?  bucket ordering is necessary for alpha blending for

                if (model.Appearance != null) // TODO: i could potentially set it to use the parent model's appearance if applicable
                {
                    // TODO: why is this check for geometry added only now setting appearancechanged flag?
                    //if ((_changeStates & Enums.ChangeStates.GeometryAdded) != 0)
                    //    _changeStates |= Keystone.Enums.ChangeStates.AppearanceChanged;

                            // TODO: shouldn't the above call SetChangeStates
                            // TODO: actually, shouldn't this have already been done once this geometry node was added? including after the resource is loaded
                            //SetChangeFlags(Keystone.Enums.ChangeStates.AppearanceChanged, Keystone.Enums.ChangeSource.Self);


                    // TODO: Note: a TextureCycle should be viewed as a visual EFFECT, not an Animation
                    // 
                    _appearanceHashCode = model.Appearance.Apply(this, elapsedSeconds);

                    // TODO: for a depth pass, we might not want to update shader params
                    //       should we have another parameter for this  method (bool updateShaderParameters)

                    // in OnRender, shaderParameters are looped thru and set
                    // but the bonus of a script is this can easily be different for other models.
 
                    // // or wait, actually... the script doesnt need the shader's ID
                    // all it needs is to do...
                    // SomeAPI.SetShaderParameter (modelID, "parameterName", parameterValue);
                    // "PlanetRadiusSquared"  // // radius of planet squared, is needed because the rings.fx doesnt inherently know the size of the planet casting a shadow on the ring
                    //"BeginCameraFade "


                    // iterate thru each shader is probably best
                    //void setParameters(){

                    //    sarMaterial.SetColor("_BaseColor", baseColor );
                    //    sarMaterial.SetFloat("_NormalContribution", normalContribution );
                    //    sarMaterial.SetFloat("_DepthContribution", depthContribution );
                    //    sarMaterial.SetFloat("_NormalPower", normalPower );
                    //    sarMaterial.SetFloat("_DepthPower", depthPower );       
                    //}
                    

                    // shader.UpdateParameters (entityInstance)
                    //    
                    // void UpdateParameters (EntityBase entity)
                    // {
                    //      // the short term fix is to simply derive a new type of shader "RingShader" that then 
                    //      // knows all of it's constants and would know that it needs to get the entity's parent entity World radius
                    //      if (delegate != null)
                    //          delegate.Invoke(entity);
                    // }
                    
                    // must assign materials to shader manually
	                if (model.Appearance.Material != null && model.Appearance.Shader != null)
	                {
		                model.Appearance.Shader.TVShader.SetEffectParamVector4 ("materialDiffuse", Keystone.Helpers.TVTypeConverter.ToTVVector4(model.Appearance.Material.Diffuse));
		                model.Appearance.Shader.TVShader.SetEffectParamVector4 ("materialAmbient", Keystone.Helpers.TVTypeConverter.ToTVVector4(model.Appearance.Material.Ambient));
		                model.Appearance.Shader.TVShader.SetEffectParamVector4 ("materialEmissive", Keystone.Helpers.TVTypeConverter.ToTVVector4(model.Appearance.Material.Emissive));
		                model.Appearance.Shader.TVShader.SetEffectParamVector4 ("materialSpecular", Keystone.Helpers.TVTypeConverter.ToTVVector4(model.Appearance.Material.Specular));
		                model.Appearance.Shader.TVShader.SetEffectParamFloat ("materialPower", model.Appearance.Material.SpecularPower);
	                }
	                
	                // HACK: this only supports one group and one diffuse texture for now... 
	                // we only use this for fast single group instanced rendering anyway so may never change
	                if (model.Appearance.Layers != null)
	                {
	                	model.Appearance.Shader.TVShader.SetEffectParamTexture ("textureDiffuse", model.Appearance.Layers[0].TextureIndex);
	                }

                }
                else
                    _appearanceHashCode = NullAppearance.Apply(this, _appearanceHashCode);

            }
            catch
            {
                // note: TODO:  very very rarely ill get an access violation on render after a mesh has been paged in.  The mesh shows as loading fine
                // in my code and in tv debug log.  My only real guess is that when the materials and textures get loaded, there's a race condition
                // where either of those will attempt to set while the mesh is rendering.  we need some kind of guard against that.
                Trace.WriteLine(string.Format("InstancedGeometry.Render() - Failed to render '{0}' w/ refount '{1}'.", _id, RefCount));
            }

            try
            {                 
                // note: TODO:  very very rarely i will get an access violation on render after a mesh has been paged in.  The mesh shows as loading fine
                // in my code and in tv debug log.  My only real guess is that when the materials and textures get loaded, there's a race condition
                // where either of those will attempt to set while the mesh is rendering.  we need some kind of guard against that. 
                // Shaders as well
                // one bug i fixed (which may have fixed this entirely(?) is if the shader was applied while it was loading, this would
                // result in an access violation.
                // TODO: also can it be the shader applied to it is not fully ready?
                //System.Diagnostics.Debug.WriteLine(model.ID.ToString());
                // TODO: If we are unloading a mesh/scene and we are still calling render,
                //       then problem!  resourcestatus var value should be switch to unloading yes?
                //       - this needs to be fixed for all pageable nodes. 
                //       Because zone paging In/Out occur in another thread, we definetly need to
                //       lock the pageable for these nodes and update their resource status
                //       - clearly there's also a race condition since we check resourcestatus and never prevent it from unloading 
                //       between then and here! does adding lock (mSyncRoot) in this Render() method solve it?

	            Device.VertexDeclaration = mVertexDeclaration;
	            Device.SetStreamSource(0, mVertexBuffer, 0);
	            Device.Indices = mIndexBuffer;

	            // TODO: what can/should be in/outside of the Shader_Begin block?
	            CoreClient._CoreClient.Internals.Shader_Begin(model.Appearance.Shader.TVShader);

                
	            int batchOffset = 0;
	            while (batchOffset < ViewedInstances)
	            {
	                var actualBatchSize = Math.Min(InstancesPerBatch, ViewedInstances - batchOffset);

	            	// x, y, z, w = rotation
	        		DirectX.Vector4[] batchPositions = new DirectX.Vector4[InstancesPerBatch];
	            	Array.Copy(mPositions, batchOffset, batchPositions, 0, actualBatchSize);
	            	batchOffset += InstancesPerBatch;
	        	    // positions are passed as shader constant
	        	    Device.SetVertexShaderConstant(StartRegister, batchPositions);
	                
	                Device.DrawIndexedPrimitives(mPrimitiveType, 0, 0, mVertexCount * actualBatchSize, 0, mPrimitiveCount * actualBatchSize);
	            }

	            CoreClient._CoreClient.Internals.Shader_End(model.Appearance.Shader.TVShader);

	            // since InstanceGeometry.cs is shared geomtry type, we must reset after each Model.Render()
                // since multiple models may be using it.
	            ClearVisibleInstances();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("InstancedGeometry.Render() - '{0}' ID: '{1}' w/ refount '{2}'.", ex.Message, _id, RefCount));
            }
    		}
        }
        #endregion

        #region IBoundVolume Members
        // From Geometry always return the Local box/sphere
        // and then have the model's return World box/sphere based on their instance
        protected override void UpdateBoundVolume()
        {
            if (_resourceStatus != PageableNodeStatus.Loaded) return;

            float radius = 0f;
            Vector3d min, max;
            min.x = min.y = min.z = double.MaxValue;
            max.x = max.y = max.z = double.MinValue ;
            
    		_box.Reset ();
	            
            for (int i = 0; i < mVertices.Length; i++)
            {
            	min.x = Math.Min (min.x, mVertices[i].Position.X);
            	min.y = Math.Min (min.y, mVertices[i].Position.Y);
            	min.z = Math.Min (min.z, mVertices[i].Position.Z);
            	
            	max.x = Math.Max (max.x, mVertices[i].Position.X);
            	max.y = Math.Max (max.y, mVertices[i].Position.Y);
            	max.z = Math.Max (max.z, mVertices[i].Position.Z);
            }
			_box.Min = min;
			_box.Max = max;
			
            DisableChangeFlags(Keystone.Enums.ChangeStates.BoundingBox_TranslatedOnly | Keystone.Enums.ChangeStates.BoundingBoxDirty);
        }
        #endregion

        #region IDisposable Members
        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            try
            {
            	// TODO: I must sychronize dispose of resource after they are removed from repository
            	//       in the same way that I do with sychronizing their Create() in factory.
	            if (mVertexDeclaration != null)
	            {
	                Device.VertexDeclaration = null;
	                mVertexDeclaration.Dispose();
	            }
	            if (mVertexBuffer != null)
	            {
	                Device.SetStreamSource(0, null, 0);
	                mVertexBuffer.Dispose();
	            }
	            if (mIndexBuffer != null)
	            {
	                Device.Indices = null;
	                mIndexBuffer.Dispose();
	            }
	        
    
            	System.Diagnostics.Debug.Assert (RefCount == 0, "InstancedGeometry.DisposeManagedResources() - RefCount not at 0.");

            }
            catch
            {
            }
        }
        #endregion
	}
}
