using System;
using System.Collections.Generic;
using Keystone.Cameras;
using MTV3D65;

namespace Keystone.PSSM
{
    // E:\dev\c#\KeystoneGameBlocks\Design\cascade_shadow_map.jpg
    // http://msdn.microsoft.com/en-us/library/windows/desktop/ee416324%28v=vs.85%29.aspx
    internal class CParallelSplitShadowMapping : IDisposable
    {
        public delegate void RenderGeometryDelegate(TV_3DMATRIX lightView, TV_3DMATRIX lightProj);
        private RenderGeometryDelegate mRenderGeometryIntoDepthMap = null;

		private RenderingContext mContext;
        private CCamera mSceneCamera;
        private TVCamera mRenderSurfaceCamera;
        private TVRenderSurface mDepthRS;

        private int mDepthTextureIndex;
        private int mShadowMapSize;
                       
        private int mSplitCount = 4;
        
        // Increase the scale of the frustum a bit to avoid artifacts near screen edges.
		private const float FRUSTUM_SCALE = 1.1f; // 1.0f 
        
        private float m_fExtraDistance;
        private float m_fSplitLambda;
        private float[] mSplitDistances;
        private TV_3DMATRIX mShadowMapTextureMatrix;
        private TV_3DMATRIX[] mShadowMapTextureMatrices;
        private TV_3DMATRIX[] mLightViewProjection; 
        private bool[][] mSplitColorChannels;
        
        private double m_dNextRenderTime;
        private float m_fRenderInterval;
        private CBoundingBox m_pSceneBoundingBox;
        private Keystone.Types.Vector3d mLightDirection;
        private TV_3DVECTOR mUpVector;
		private List <Shaders.Shader> mShaders;
		


        public CParallelSplitShadowMapping(RenderingContext context, 
		                                   int numSplits,
		                                   int nShadowMapSize, 
		                                   CONST_TV_RENDERSURFACEFORMAT eRenderSurfaceFormat, 
		                                   float fRenderInterval,
		                                   RenderGeometryDelegate renderDelegate)
        {
        	mContext = context;
            mShaders = new List<Shaders.Shader>();
            
            // TODO: we need  to ensure TVCamera is not null
            if (context.Camera == null || context.Camera.TVCamera == null)
            	throw new ArgumentNullException ("ParallelSplitShadowMapping.ctor() - Null camera.");
            
            if (renderDelegate == null) throw new ArgumentNullException();
            
            if (numSplits < 1 || numSplits > 4) throw new ArgumentOutOfRangeException ();
            mSplitCount = numSplits;
            
            mRenderGeometryIntoDepthMap = renderDelegate;
            
            mSceneCamera = new CCamera(context.Camera.TVCamera);
            mSceneCamera.AspectRatio = (float)context.Viewport.AspectRatio; //  vSceneViewportResolution.x / vSceneViewportResolution.y;
            mSceneCamera.MaxFarPlane = mSceneCamera.FarPlane;
            mSceneCamera.MinFarPlane = 60.0f;
			mSceneCamera.FieldOfView = context.Camera.FOVRadians;
            mSceneCamera.FarPlane = context.Camera.Far;
            mSceneCamera.NearPlane = context.Camera.Near ;
            
            mUpVector = new TV_3DVECTOR(0, 1, 0);
       
            m_fSplitLambda = 0.3f;
            mShadowMapSize = nShadowMapSize;
            mSplitDistances = new float[mSplitCount + 1];
            
            m_pSceneBoundingBox = new CBoundingBox();


            mLightViewProjection = new TV_3DMATRIX[mSplitCount];
            mShadowMapTextureMatrices = new TV_3DMATRIX[mSplitCount];
            m_fRenderInterval = fRenderInterval;

            // Set up the color channels whe want to render the single 
            // depth maps into
            mSplitColorChannels = new bool[mSplitCount][];
            mSplitColorChannels[0] = new bool[4] { true, false, false, false };
            if (mSplitCount > 1)
            	mSplitColorChannels[1] = new bool[4] { false, true, false, false };
            if (mSplitCount > 2)
            	mSplitColorChannels[2] = new bool[4] { false, false, true, false };
            if (mSplitCount > 3)
	            mSplitColorChannels[3] = new bool[4] { false, false, false, true };

            // Compute the texture matrix for the depth maps to
            // sample within the correct UV zones. We need to use
            // "0.5f / m_nShadowMapSize", because D3D samples at
            // the (imaginary) center of each texel
            float fOffset = 0.5f + (0.5f / (float)mShadowMapSize);

            mShadowMapTextureMatrix = new TV_3DMATRIX(
                0.5f, 0.0f, 0.0f, 0.0f,
                0.0f, -0.5f, 0.0f, 0.0f,
                0.0f, 0.0f, 1.0f, 0.0f,
                fOffset, fOffset, 0.0f, 1.0f);

            mRenderSurfaceCamera = CoreClient._CoreClient.CameraFactory.CreateCamera ("pssm_" + context.Viewport.Name);
            // TODO: our HTML QuickLook was creating a hidden viewport I always forget about. we've disabled for now
                      
            CoreClient._CoreClient.TextureFactory.SetTextureMode(CONST_TV_TEXTUREMODE.TV_TEXTUREMODE_32BITS);
            // Create the depth map with a white background and ARGB16F
            // You may want to change that to a different format
            mDepthRS = CoreClient._CoreClient.Scene.CreateRenderSurface(mShadowMapSize, mShadowMapSize, true, eRenderSurfaceFormat);
        	mDepthTextureIndex = mDepthRS.GetTexture();
            mDepthRS.SetBackgroundColor(CoreClient._CoreClient.Globals.RGBA(1.0f, 1.0f, 1.0f, 1.0f));
            mDepthRS.StartRender(); // Render it once to clear the background
            mDepthRS.EndRender();
        }
        
        public float RenderInterval
        {
            get { return m_fRenderInterval; }
            set { m_fRenderInterval = value; }
        }

        public float SplitLambda
        {
            get { return m_fSplitLambda; }
            set { m_fSplitLambda = value; }
        }

        public float FarPlane
        {
            get { return mSceneCamera.MaxFarPlane; }
            set { mSceneCamera.MaxFarPlane = value; }
        }

        public float NearPlane
        {
            get { return mSceneCamera.NearPlane; }
            set { mSceneCamera.NearPlane = value; }
        }
        
        private Keystone.Types.Color mLightColor;

        // Use this property to set the lights direction
        // after you map has been loaded or whenever
        // the lights direction has changed
        public Keystone.Types.Vector3d LightDirection
        {
            get { return mLightDirection; }
            set
            {
//            	if (mLightDirection.x != value.x ||
//					mLightDirection.y != value.y ||
//					mLightDirection.z != value.z)
//            	{

					// TODO: it seems that if the dir light is not found every single frame, it completely stops updating
					//       such that when the camera moves, the shadows are not updated with respect to new camera position!
					// TODO: how come dirLightDir if set in the semantic doesn't work?  But even if it could
					//       sometimes for shadows, we want to tweak the direction to be a balance between say moonlight and sunlight
					//       until either has completely set
            		mLightDirection = value;   // direction value passed in should be normalized
                	// invert the lights direction vector to spare that step
                	// within the fragment shaders later on
                	foreach(Shaders.Shader shader in mShaders)
                	{
                		// NOTE: we negate the mLightDirection
                		shader.SetShaderParameterVector("g_DirLightDirection", -mLightDirection);
                		shader.SetShaderParameterVector ("g_DirLightColor", Helpers.TVTypeConverter.ColorToVector3d (mLightColor));
                	}
//            	}
            }
        }

        public float ExtraDistance
        {
            get { return m_fExtraDistance; }
            set { m_fExtraDistance = value; }
        }
        

        /// <summary>
        /// Holds normal forward shaders for Actors, Meshes, Landscapes, etc.  Those shaders
        /// however must be adapted to use shadowmap tetxure
        /// </summary>
        public void AddShader (Keystone.Shaders.Shader shader)
        {
        	if (mShaders.Contains (shader)) return;
        	
        	// force it to load
        	Keystone.IO.PagerBase.LoadTVResource (shader, false);
        	if (shader.PageStatus != IO.PageableNodeStatus.Loaded) throw new Exception ("PSSM.AddShader() - shader failed to load." + shader.ID);
        	
        	// TODO: when this shader is added it may not be loaded yet so at some point we need to set the texture and Percentage Close Filtering scale 
        	shader.SetShaderParameterFloat("g_fPCFScale", 1.0f / (float)mShadowMapSize);
        	shader.SetShaderParameterVector("g_DirLightDirection", -mLightDirection);
        	shader.SetShaderParameterTexture ("texShadowMap", mDepthTextureIndex);
        	shader.SetShaderParameterVector2("g_ShadowMapSize", (float)mShadowMapSize,(float)mShadowMapSize);
        	mShaders.Add (shader);
        	
        	Keystone.Resource.Repository.IncrementRef (shader);
        }

        public void RemoveShader(Keystone.Shaders.Shader shader)
        {
        	
        	if (mShaders.Contains (shader) == false) throw new ArgumentOutOfRangeException ();
        	
        	mShaders.Remove (shader);
        	Keystone.Resource.Repository.DecrementRef (shader);
        }

        /// <summary>
        /// Camera space world bounding box.  This way all zones can use same depth texture.
        /// </summary>
        /// <param name="BoundingBox"></param>
        public void AddBoundingBoxToSceneBoundingBox (Keystone.Types.BoundingBox box)
        {
        	// Never add an uninitialized box or it will screw up sizes.
        	// If the BoundingBox becomes artificially large, we will suffers lots of 
        	// Projective Aliasing and Perspective Aliasing.
        	// http://msdn.microsoft.com/en-us/library/windows/desktop/ee416324%28v=vs.85%29.aspx
        	if (box.Min == new Types.Vector3d (float.MaxValue * .5f, float.MaxValue * .5f, float.MaxValue * .5f) && 
        	   box.Max == new Types.Vector3d (float.MinValue * .5f, float.MinValue * .5f, float.MinValue * .5f))
        		return;
        	
        	// this box must be camera space 
        	TV_3DVECTOR min;
        	min.x = (float)box.Min.x;
        	min.y = (float)box.Min.y;
        	min.z = (float)box.Min.z;
        	
        	TV_3DVECTOR max;
        	max.x = (float)box.Max.x;
        	max.y = (float)box.Max.y;
        	max.z = (float)box.Max.z;

        	m_pSceneBoundingBox.Merge (min);
        	m_pSceneBoundingBox.Merge(max);
        }
        
        public void ClearSceneBoundingBox ()
        {
        	m_pSceneBoundingBox.Reset();
        }

         /// <summary>
        /// Re-compute the view frustum based on information.
        /// This occurs prior to creating Light Matrices for each split.
        /// 
        /// Clamp the cameras view frustum to the worlds bounding box.
        /// If camera is near the world bounding  limit, it reduces the
        /// space where no shadow caster can ever be 
        /// </summary>
        private void AdjustCameraPlanes()
        {
            TV_3DMATRIX mCameraView = MathHelpers.MatrixLookAtLH(mSceneCamera.Position, mSceneCamera.LookAt, mUpVector);

            // Find the most distant point of AABB
            float maxZ = 0.0f;
            float vertexZ;

            // TODO: i think this call is supposed to be for the light bounds!
            m_pSceneBoundingBox = RoundBoundsToNearestTexelMultiple (m_pSceneBoundingBox);
        	m_pSceneBoundingBox.ConstructEverything();
                    	
            for (int i = 0; i < 8; i++)
            {
                // Transform z coordinate with view matrix
                vertexZ = m_pSceneBoundingBox.Vertices[i].x * mCameraView.m13
                        + m_pSceneBoundingBox.Vertices[i].y * mCameraView.m23
                        + m_pSceneBoundingBox.Vertices[i].z * mCameraView.m33
                        + mCameraView.m43;

                // Check if its largest
                if (vertexZ > maxZ)
                    maxZ = vertexZ;
            }

            // Use largest Z coordinate as new far plane
            maxZ += mSceneCamera.NearPlane;

            MathHelpers.Clamp(ref maxZ, mSceneCamera.MinFarPlane, mSceneCamera.MaxFarPlane);

            mSceneCamera.FarPlane = maxZ;

            // We need to recompute the split distances since the frustum has changed.
            // Only do this when the frustum size has changed - pheeew, what an uber optimization ;)
            if (mSceneCamera.FarPlane != mSceneCamera.FarPlaneLastFrame)
            {
                mSplitDistances = CalculateSplitDistances();
                
                float[] fDepthTestParameters = null;
                
                if (mSplitCount == 1)
					fDepthTestParameters = new float[] { mSplitDistances[1] };
                else if (mSplitCount == 2)
					fDepthTestParameters = new float[] { mSplitDistances[1], mSplitDistances[2]};
                else if (mSplitCount == 3)
					fDepthTestParameters = new float[] { mSplitDistances[1], mSplitDistances[2], mSplitDistances[3] };
                else if (mSplitCount == 4)
					fDepthTestParameters = new float[] { mSplitDistances[1], mSplitDistances[2], mSplitDistances[3], mSplitDistances[4] };

            	// Add the new values to the shader
				foreach (Shaders.Shader shader in mShaders)
            	    shader.SetShaderParameterFloatArray("g_fSplitDistances", fDepthTestParameters);
            	    
                mSceneCamera.FarPlaneLastFrame = mSceneCamera.FarPlane;
            }
        }
        
        /// <summary>
        /// Calculates the split distances for each single split.
        /// This comes directly out of the demo, but you can also
        /// use procentual values for this
        /// </summary>
        private float[] CalculateSplitDistances()
        {
            float fIDM, fLog, fUniform;

            float[] results = new float[mSplitCount + 1];
            
            for (int i = 0; i < mSplitCount; i++)
            {
                fIDM = i / (float)mSplitCount;
                fLog = mSceneCamera.NearPlane * (float)Math.Pow((mSceneCamera.FarPlane / mSceneCamera.NearPlane), fIDM);
                fUniform = mSceneCamera.NearPlane + (mSceneCamera.FarPlane - mSceneCamera.NearPlane) * fIDM;
                results[i] = fLog * m_fSplitLambda + fUniform * (1.0f - m_fSplitLambda);
            }

            results[0] = mSceneCamera.NearPlane;
    
            int lastIndex = mSplitCount;
            results[lastIndex] = mSceneCamera.FarPlane;

            // Das hier ist besser, da im nahen Bereich dann immer mehr Detail anliegt
            /* results[0] = m_fSceneCameraNear;
             results[1] = m_fSceneCameraNear + 50.0f;
             results[2] = results[1] + (m_fSceneCameraFar - results[1]) * 0.3f;
             results[3] = results[1] + (m_fSceneCameraFar - results[1]) * 0.6f;
             results[4] = m_fSceneCameraFar;*/

            return results;
        }

        /// <summary>
        /// Calculates the frustum cornes / vertices / points / whatever
        /// of our scene camera for a single frustum split.
        /// </summary>
        /// <param name="fNear">Near plane of the split.</param>
        /// <param name="fFar">Far plane of the split</param>
        private TV_3DVECTOR[] CalculateFrustumCorners(float fNear, float fFar, float fovRadians, float aspectRatio)
        {
            float nearHeight, nearWidth, farHeight, farWidth;
            TV_3DVECTOR center, farCenter, nearCenter;
            TV_3DVECTOR cameraPos, lookAt, vZ, vX, vY;

            // TVCamera.GetFrustumPoints() is not an option, since we
            // only compute the frustum corners for a single split,
            // not for the whole frustum. Changing the planes of the TVCamera
            // could work, but I haven't tested this.. since this is working fine :)
            cameraPos = mSceneCamera.Position;
            lookAt = mSceneCamera.LookAt;
            vZ = MathHelpers.Vec3Normalize(lookAt - cameraPos);
            vX = MathHelpers.Vec3Normalize(MathHelpers.Vec3CrossProduct(mUpVector, vZ));
            vY = MathHelpers.Vec3CrossProduct(vZ, vX);

            nearHeight = (float)Math.Tan(fovRadians * 0.5f) * fNear;
            nearWidth = nearHeight * aspectRatio;

            farHeight = (float)Math.Tan(fovRadians * 0.5f) * fFar;
            farWidth = farHeight * aspectRatio;

            nearCenter = cameraPos + vZ * fNear;
            farCenter = cameraPos + vZ * fFar;

            TV_3DVECTOR[] corners = new TV_3DVECTOR[8];
            corners[0] = nearCenter - vX * nearWidth - vY * nearHeight;
            corners[1] = nearCenter + vX * nearWidth - vY * nearHeight;
            corners[2] = nearCenter - vX * nearWidth + vY * nearHeight;
            corners[3] = nearCenter + vX * nearWidth + vY * nearHeight;
            corners[4] = farCenter - vX * farWidth - vY * farHeight;
            corners[5] = farCenter + vX * farWidth - vY * farHeight;
            corners[6] = farCenter - vX * farWidth + vY * farHeight;
            corners[7] = farCenter + vX * farWidth + vY * farHeight;

            // Increase the scale of the frustum a bit to avoid artifacts near screen edges.
            // Start by calculating the center of our frustum points
            float scale = FRUSTUM_SCALE;

            center.x = center.y = center.z = 0;
            for (int i = 0; i < 8; i++)
                center += corners[i];

            center /= 8.0f;

            // Finally scale it by adding offset from center
            for (int i = 0; i < 8; i++)
                corners[i] += (corners[i] - center) * (scale - 1.0f);
            
            return corners;
        }

        /// <summary>
        /// Creates concentric frustums with the smallest contained in the center of all the others.
        /// 
        /// The Near/Far values are based on our SplitDistances such that inner most frustum has smallest Far - Near value 
        /// which will give us higher precision in the depth buffer than the others.
        /// 
        /// TODO: I believe this can only work with directional lights since it can compute a minimum light distance from light volume using only light.direction
        /// E:\dev\c#\KeystoneGameBlocks\Design\cascade_shadow_map.jpg
        /// </summary>
        /// <param name="mLightView"></param>
        /// <param name="mLightProj"></param>
        /// <param name="nSplit"></param>
        private void CreateLightCentricCameraMatrices(TV_3DVECTOR[] frustumCorners, Keystone.Types.Vector3d lightDirection,  int splitIndex, out TV_3DMATRIX lightView, out TV_3DMATRIX lightProj)
        {
        	// E:\dev\c#\KeystoneGameBlocks\Design\cascade_shadow_map.jpg
            float distance;
            float[] faNear = new float[3];
            float[] faFar = new float[3];
            float[] faDiff = new float[3]; // 0 = xaxis, 1 = yaxis, 2 = zaxis
            TV_3DVECTOR[] vaAxis = new TV_3DVECTOR[3]; // Orthonormal basis for the view matrix
            TV_3DVECTOR centerPos = new TV_3DVECTOR();

            // Reset boundaries
            for (int i = 0; i < 3; i++)
            {
                faNear[i] = float.MaxValue;
                faFar[i] = float.MinValue;
            }

            // Construct orthonormal basis
            vaAxis[2] = Helpers.TVTypeConverter.ToTVVector(lightDirection);
            vaAxis[0] = MathHelpers.Vec3Normalize(MathHelpers.Vec3CrossProduct(mUpVector, vaAxis[2]));
            vaAxis[1] = MathHelpers.Vec3Normalize(MathHelpers.Vec3CrossProduct(vaAxis[2], vaAxis[0]));

            // Get the boundaries and compute the distances for our light volume
            for (int i = 0; i < 8; i++)
            {
                for (int k = 0; k < 3; k++)
                {
                	// for all 3 axis, get distance from that axis to every frustum corner
                    distance = MathHelpers.Vec3DotProduct(vaAxis[k], frustumCorners[i]);

                    // find the nearest near
                    if (distance < faNear[k])
                        faNear[k] = distance;

                    // find the farthest far
                    if (distance > faFar[k])
                        faFar[k] = distance;
                }
            }
            
            // Compute differences between this light volume and our split frustum and get the center point of what will be
			// a new view frustum that only encompasses areas that the light can touch.
			float smallestNear = float.MaxValue;
			float biggestFar = float.MinValue;
            for (int i = 0; i < 3; i++)
            {
                faDiff[i] = faFar[i] - faNear[i];
                centerPos += vaAxis[i] * (faNear[i] + faFar[i]);
                smallestNear = Math.Min (smallestNear, faNear[i]);
                biggestFar = Math.Min (biggestFar, faFar[i]);
            }

            centerPos *= 0.5f;

            // Finally compute the new camera matrices - these will be set in shader during Depth pass 
            // TODO: Compute rather than hardcode the extra distance with the worlds bounding box to be sure that 
            //       you have enclosed everything!
            float fExtraDistance = m_fExtraDistance;
            float ZNear = 10.0f;
            float fLightZFar = faDiff[2] + ZNear + fExtraDistance;
            
            // directional lights have no position, but we will construct an ideal EYE position for this light-centric frustum
            // NOTE: because the lights are always relative to the camera and everything in the world is rendered
            // relative to the camera, there is no need to do seperate renders for each zone.  Each zone is already
            // rendered relative to the camera.
            TV_3DVECTOR lightPos = centerPos - vaAxis[2] * (faDiff[2] * 0.5f + ZNear + fExtraDistance);

            lightView = MathHelpers.MatrixLookAtLH(lightPos, centerPos, mUpVector);
            lightProj = MathHelpers.MatrixOrthoLH(faDiff[0], faDiff[1], -ZNear, fLightZFar);

            // This would linearize the depth values. Its usually a good idea
            // and its required for some filtering purposes, but I don't use it :)
        //    lightProj.m33 /= fLightZFar;
        //    lightProj.m43 /= fLightZFar;
        }

        private PSSM.CBoundingBox RoundBoundsToNearestTexelMultiple(PSSM.CBoundingBox box)
        {
        	// http://msdn.microsoft.com/en-us/library/windows/desktop/ee416324%28v=vs.85%29.aspx
        	// Shadows without shimmering edges 
        	// For directional lights, the solution to this problem is to round the 
        	// minimum/maximum value in X and Y (that make up the orthographic projection bounds)
			// to pixel size increments. This can be done with a divide operation, a floor operation,
			// and a multiply.
			
			// Bounding the maximum size of the view frustum results in a looser fit for the orthographic projection.
			// It is important to note that the texture is 1 pixel larger in width and height when using this technique. 
			// This keeps shadow coordinates from indexing outside of the shadow map.
			
			// Moving the Light Texel-Sized Increments, give better results when the size of the light's
			// projection remains constant in every frame. // TODO: should i be rounding light's xyz position as it moves too?
			TV_3DVECTOR min = box.Minimum;
			TV_3DVECTOR max = box.Maximum;
			
			float diameter = Math.Max (max.x - min.x, max.z - min.z);
			float fWorldUnitsPerTexel = diameter  / mShadowMapSize;
        	
			TV_3DVECTOR vWorldUnitsPerTexel;
        	vWorldUnitsPerTexel.x = fWorldUnitsPerTexel;
        	vWorldUnitsPerTexel.y = fWorldUnitsPerTexel;
        	vWorldUnitsPerTexel.z = fWorldUnitsPerTexel;
			
        	min.x /= vWorldUnitsPerTexel.x;
        	min.y /= vWorldUnitsPerTexel.y;
        	min.z /= vWorldUnitsPerTexel.z;

        	min.x = (float)Math.Floor (min.x);
        	min.y = (float)Math.Floor (min.y);
        	min.z = (float)Math.Floor (min.z);
        	
        	min.x *= vWorldUnitsPerTexel.x;
        	min.y *= vWorldUnitsPerTexel.y;
        	min.z *= vWorldUnitsPerTexel.z;
        	
        	max.x /= vWorldUnitsPerTexel.x;
        	max.y /= vWorldUnitsPerTexel.y;
        	max.z /= vWorldUnitsPerTexel.z;
        	
        	max.x = (float)Math.Floor (max.x);
        	max.y = (float)Math.Floor (max.y);
        	max.z = (float)Math.Floor (max.z);
        	
        	max.x *= vWorldUnitsPerTexel.x;
        	max.y *= vWorldUnitsPerTexel.y;
        	max.z *= vWorldUnitsPerTexel.z;
        	
        	box.Minimum = min;
        	box.Maximum = max;
        	
        	return box;
        }
        
        // 
        /// <summary>
        /// Renders the depth map for every split
        /// </summary>
        public void RenderBeforeClear(double dGameTime)
        {
        	// TODO: 
        	if (dGameTime >= 0) //m_dNextRenderTime)
            {
                TV_3DMATRIX lightView, proj;

                mSceneCamera.LookAt = Helpers.TVTypeConverter.ToTVVector ( mContext.LookAt);
                // Snap the cameras view frustum to the worlds bounding box.
                // If you're near the world boundings, it reduces the
                // space where no shadow caster can ever be 
                AdjustCameraPlanes();


                foreach (Shaders.Shader shader in mShaders)
                {
            	    shader.SetTechnique("depth");
            	    // LightDirection = mLightDirection;
                	//shader.SetShaderParameterTexture ("texShadowMap", mDepthTextureIndex);
                }

                mDepthRS.SetNewCamera(mRenderSurfaceCamera);
                      
                	            // TODO: !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
	            // TODO: !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! SHADOWMAPPING BUG WITH INDEXEDPRIMITIVES?
	            // TODO: Ask Sylvain if when using Internals to render to RS, I need a way to Device.SetRenderTarget to the surface
//	            Device.SetRenderTarget (renderTargetIndex, new Microsoft.DirectX.Direct3D.Surface(renderSurfaceCorePtr));
			//	Microsoft.DirectX.Direct3D.Device Device = new Microsoft.DirectX.Direct3D.Device(CoreClient._CoreClient.Internals.GetDevice3D());
			//	CoreClient._CoreClient.Internals.
			//	Device.SetRenderTarget (mDepthRS.GetTexture(), new Microsoft.DirectX.Direct3D.Surface(mDepthRS.GetCorePointer()));
				
				//CoreClient._CoreClient.Internals.GetTexture(mDepthRS.GetIndex());
				
				//		IntPtr corePtr = mDepthRS.GetCorePointer ();
				//      int rsIndex = mDepthRS.GetIndex(); // TODO: i think this is just TVIndex and not RSIndex... but maybe they are the same


					
                // Compute and then Render every split one by one (Splits are not cached)
                for (int nSplit = 0; nSplit < mSplitCount; nSplit++)
                {
                    // Calculate the cameras frustum corners for the current split
                    TV_3DVECTOR[] frustumCorners = CalculateFrustumCorners(mSplitDistances[nSplit], 
                                                                           mSplitDistances[nSplit + 1], 
                                                                           mSceneCamera.FieldOfView, 
                                                                           mSceneCamera.AspectRatio);

                    // Calculate view and projection matrices for a split frustum that is constrained also
					// by the light volume
					// NOTE: each split has a different camera frustum and thus different frustumCorners
					//       The Witness SHadowmapping I believe uses a single ShadowMatrix and then uses CascadeOffsets[] and CascadeScales[] 
					//       to modify each different split					
                    CreateLightCentricCameraMatrices(frustumCorners, mLightDirection, nSplit, out lightView, out proj);

                    // For each split... Guess what this method does... ;)
                    // TODO: can I pass in the view & proj and then cull the render()
                    //       for each of the 4 different frustums of our splits? since the frustums are concentric
                    //       the idea would be to start with inner, flag all visible items that are fully inside the frustum
                    //       and include but dont flag the ones that merely "intersect" so that they'll be rendered by subsequent
                    //       next outer frustum as well and get flagged there.  
					//       - the main problem with this approach when it concerns our Structure minimeshes is we don't cull per item
					//         so how do we prevent all geometry of all floors in each zone from all still being rendered for every frustum?		

					
                    RenderDepthMap(nSplit, lightView, proj);
                    
                }

                // Set the proper parameters
                foreach (Shaders.Shader shader in mShaders)
                {
                	// assign the texture matrices and lightviewproj matrices for the 4 splits
                	shader.SetTechnique("shadowed");	
                	LightDirection = mLightDirection;
                	shader.SetShaderParameterMatrixArray("g_mLightViewProjTextureMatrix", mShadowMapTextureMatrices);
                    shader.SetShaderParameterMatrixArray("g_mLightViewProj", mLightViewProjection);
                    shader.SetShaderParameterVector ("g_CameraOffset", mContext.Position);
                    //shader.SetShaderParameterTexture ("texShadowMap", mDepthTextureIndex); // temp, test set again for instancing?
                }
                
                // Re-enable the color writing to proceed "normal" rendering
                CoreClient._CoreClient.Scene.SetColorWriteEnable(true, true, true, true);

                // Rendering the shadows in realtime can be costly, so we
                // only render the depth maps in a certain render interval, which
                // is small enough to hide flickering
                m_dNextRenderTime = dGameTime + m_fRenderInterval;
            }
        }

        
        /// <summary>
        /// Render the depth map for each split
        /// </summary>
        /// <param name="nSplit">split index</param>
        /// <param name="mView">light View Matrix</param>
        /// <param name="mProjection">light Projection Matrix</param>
        private void RenderDepthMap(int nSplit, TV_3DMATRIX lightView, TV_3DMATRIX lightProjection)
        {
        	if (mRenderGeometryIntoDepthMap == null) return;
        	
        	// http://http.developer.nvidia.com/GPUGems/gpugems_ch14.html
        	// http://www.zemris.fer.hr/~zeljkam/radovi/12_CSSM.pdf
        	

        	// compute matrix to transform worldspace into light-centric camera's clipspace
            mLightViewProjection[nSplit] = lightView * lightProjection;
            // compute matrix to transform worldspace into light-centric camera's UV coordinates
            mShadowMapTextureMatrices[nSplit] = mLightViewProjection[nSplit] * mShadowMapTextureMatrix;

            // Only inverse the view matrix for the TVCamera, since it needs
            // an inversed one
            lightView = MathHelpers.MatrixInverse(lightView);
            
            // Set the proper matrices we have computed before into the camera used by the RS
            mRenderSurfaceCamera.SetMatrix (lightView);
            mRenderSurfaceCamera.SetCustomProjection (lightProjection);
            
            
            // Only enable the color writing for the channel represented by the current split.  The depth
            // information gets written to split[0] = R = closest, split[3] = A = furthest.  
            // NOTE: this is done per-pixel so that geometry that is on both sides of a split boundary
            // will get written partially to both.  So when culling, we need overlap of the light frustum
            // to ensure the boundary meshes get rendered on both splits it falls on.
            CoreClient._CoreClient.Scene.SetColorWriteEnable(mSplitColorChannels[nSplit][0],
                                                   mSplitColorChannels[nSplit][1],
                                                   mSplitColorChannels[nSplit][2],
                                                   mSplitColorChannels[nSplit][3]);

// obsolete - i think we control the specific RGBA channel we render to via SetColorWriteEnable
//            and so don't need to tell the shader which nSplit index we're on
//   		"g_nCurrentSplit" does not exist in shader.  split index is computed in shader by depth
//           foreach (Shaders.Shader shader in mShaders)
//           	shader.SetShaderParameterInteger ("g_nCurrentSplit", nSplit);
            
            // Start rendering the depth maps.
            // Only clear the render surfaces buffer when
            // we render the first split. In other words:
            // Keep the render surfaces content for one frame,
            // but all four render passes
            // NOTE: TVEngine.ClearDepthBuffer() is also used to clear depth buffer in RS according to Sylvain June.25.2014
            // TODO: The Witness uses a seperate camera project/view for each cascade it seems...? No i dont think it does
            //       As i look at the code, it seems to just calculate them somewhat differently...
    		mDepthRS.StartRender(nSplit > 0);
    		
            // TODO: above essentially sets up camera and render surface, below we would still
            //       render this pass using camera space rendering....
            //       - we'd skip billboards and certain LODs and meshes that were too far...

    	    mRenderGeometryIntoDepthMap.Invoke(lightView, lightProjection);

        	mDepthRS.EndRender();
        	
       
 
        }

        public void RenderShadowMapDebug()
        {
        #if DEBUG
            CoreClient._CoreClient.Screen2D.Action_Begin2D();
            CoreClient._CoreClient.Screen2D.Draw_Texture(mDepthTextureIndex, 16, 16, 16 + 255, 16 + 255);
            CoreClient._CoreClient.Screen2D.Action_End2D();
        #endif
        }
        
        #region IDisoposable members
        protected bool _disposed;
        ~CParallelSplitShadowMapping()
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

        protected void DisposeManagedResources()
        {
        	while (mShaders != null && mShaders.Count > 0)
        		RemoveShader (mShaders[0]);
        }

        protected void DisposeUnmanagedResources()
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
