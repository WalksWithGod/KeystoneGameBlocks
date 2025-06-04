using System;
using System.Diagnostics;
using Keystone.Types;
using MTV3D65;
using Keystone.Appearance;
using Keystone.IO;
using Direct3D = Microsoft.DirectX.Direct3D;
using DirectX = Microsoft.DirectX;

namespace Keystone.Elements
{
	/// <summary>
	/// Description of InstancedBillboard.
	/// </summary>
	internal class InstancedBillboard : InstancedGeometry 
	{

        private bool mAxialRotations;
        
		public InstancedBillboard(string id) : base (id)
		{
			
			mPrimitiveType = Direct3D.PrimitiveType.TriangleList;
			mAxialRotations = false;
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

            // precache == true will result in all minimesh elements being created ahead of time with each element disabled until needed.
//            properties[1] = new Settings.PropertySpec("billboardwidth", _billboardWidth.GetType().Name);
//            properties[2] = new Settings.PropertySpec("billboardheight", _billboardHeight.GetType().Name);
//            properties[3] = new Settings.PropertySpec("centerheight", _billboardCenterHeight.GetType().Name);

//            properties[6] = new Settings.PropertySpec("usecolors", typeof(bool).Name);
//            properties[7] = new Settings.PropertySpec ("alphasort", typeof(bool).Name);
			properties[0] = new Settings.PropertySpec("axialrotations", typeof(bool).Name);

            // TODO: hull isnt necessary for a minimesh which ive decided is only used for instance rendereing.
            //    Hull = new ConvexHull(_filePath);
            //    Hull = ConvexHull.ReadXml(xmlNode);
            if (!specOnly)
            {
//                properties[0].DefaultValue = mPreCacheMaxElements;
//                properties[1].DefaultValue = _billboardWidth;
//                properties[2].DefaultValue = _billboardHeight;
//                properties[3].DefaultValue = _billboardCenterHeight;
//                properties[6].DefaultValue = _useColors;
//                properties[7].DefaultValue = _alphaSort;
                properties[0].DefaultValue = (bool)mAxialRotations;
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
//                    case "usecolors":
//                        _useColors = (bool)properties[i].DefaultValue;
//                        break;
//                    case "alphasort":
//                        _alphaSort = (bool)properties[i].DefaultValue;
//                        break;
                    case "axialrotations":
                        mAxialRotations = (bool)properties[i].DefaultValue;
                        break;
                }
            }
        }
        #endregion
        
        #region IPageableNode Members
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

            	// TODO: these dimensions should be passed in or read from settings.  For cube, the
            	// dimensions should match those of structure's cube/tile dimensions.
                float halfWidth = 3.0f / 2f;
    			float halfHeight = 1.0f / 2f;
    			float halfDepth = 2.5f / 2f;
    
            	// NOTE: By reaching this method we know that the primitive does not already exist in Repository cache
                // parse the primitive type and attributes and build it
                InitializeQuadVertices(halfWidth, halfHeight);
	                                    
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
        }

        
        private void InitializeQuadVertices(float halfWidth, float halfHeight)
        {
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
            
            mVertices = new DXObjects.PositionNormalTextureInstance[] 
            {
            	new DXObjects.PositionNormalTextureInstance(new Microsoft.DirectX.Vector3(-halfWidth, -halfHeight, 0.0f), up, new Microsoft.DirectX.Vector2 (0.0f, 1.0f)),
                new DXObjects.PositionNormalTextureInstance(new Microsoft.DirectX.Vector3(halfWidth, -halfHeight, 0.0f), up, new Microsoft.DirectX.Vector2 (1.0f, 1.0f)),
                new DXObjects.PositionNormalTextureInstance(new Microsoft.DirectX.Vector3(halfWidth, halfHeight, 0.0f), up, new Microsoft.DirectX.Vector2 (1.0f, 0.0f)),
                new DXObjects.PositionNormalTextureInstance(new Microsoft.DirectX.Vector3(-halfWidth, halfHeight, 0.0f), up, new Microsoft.DirectX.Vector2 (0.0f, 0.0f)),
            };
            
            // DX is CW order by default
            mIndices = new short[]
            {
                0, 3, 2,  0, 2, 1
            };
        }
		#endregion

	
        internal bool AxialRotationEnable { get {return mAxialRotations;} }
        
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
        internal override void AddInstances (Vector3d[] positions, Vector3d[] rotations, int count)
        {
        	#if DEBUG
        	if (positions == null || rotations == null) throw new ArgumentNullException();
        	if (positions.Length >= rotations.Length) throw new ArgumentOutOfRangeException ();
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

	        	// pack the rotation axis vector into a single float - axial billboarding
				vec.W = Utilities.MathHelper.PackToFloat ((float)rotations[i].x, (float)rotations[i].y, (float)rotations[i].z);

    
	        	mPositions[ViewedInstances + i] = vec;
        	}
        	ViewedInstances += count;
        	SetChangeFlags (Keystone.Enums.ChangeStates.BoundingBoxDirty, Keystone.Enums.ChangeSource.Self);
        }
        
        #region Geometry Member
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
                Trace.WriteLine(string.Format("InstancedBillboard.Render() - Failed to render '{0}' w/ refount '{1}'.", _id, RefCount));
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
	                // http://stackoverflow.com/questions/2866778/hlsl-index-to-unaligned-packed-floats
	                //mAxialRotations = true;
//	                if (mAxialRotations)
//	                {
//	                	// TODO: set var in shader for axial rotation and assign shader param direction vector/axis
//						// TODO: each billboard needs to have a unique axis vector so where do we assign the "rotation"
//	                } 
//	                else
//	                {
	                	// x, y, z, w = rotation
		        		DirectX.Vector4[] batchPositions = new DirectX.Vector4[InstancesPerBatch];
	                	Array.Copy(mPositions, batchOffset, batchPositions, 0, actualBatchSize);
	                	batchOffset += InstancesPerBatch;
	            	    // positions are passed as shader constant
	            	    Device.SetVertexShaderConstant(StartRegister, batchPositions);
//	                }
	                Device.DrawIndexedPrimitives(mPrimitiveType, 0, 0, mVertexCount * actualBatchSize, 0, mPrimitiveCount * actualBatchSize);
	            }

	            CoreClient._CoreClient.Internals.Shader_End(model.Appearance.Shader.TVShader);

	            // since InstanceGeometry.cs is shared geomtry type, we must reset after each Model.Render()
                // since multiple models may be using it.
	            ClearVisibleInstances();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("InstancedBillboard.Render() - '{0}' ID: '{1}' w/ refount '{2}'.", ex.Message, _id, RefCount));
            }
    		}
        }
        #endregion
	}}