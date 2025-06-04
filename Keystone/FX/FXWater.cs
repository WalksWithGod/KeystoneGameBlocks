using System;
using Keystone.Cameras;
using Keystone.Elements;
using Keystone.Entities;
using Keystone.RenderSurfaces;
using Keystone.Types;
using MTV3D65;

namespace Keystone.FX
{
    public class FXWater : FXBase
    {
    	// names for the rendersurfaces. Used to dereference the proper one in methods like SetRSResolution(resolution, name)
        private const string REFLECTION = "reflection";
        private const string REFRACTION = "refraction";

        private RenderSurface mReflectRS;
        private RenderSurface mRefractRS;
        private int mStyle;
        private bool mUseDirection;
        private bool mUseAnimation;
        private float mDirSpeedX;
        private float mDirSpeedZ;
        private bool mReflectionEnabled;
        private bool mRefractionEnabled;
        private TVGraphicEffect mEffect;
        private TV_PLANE mWaterPlane;
        // NOTE: we cache the water plane distance since our camera space rendering screws up
        //       TV's own updating of the distance in it's internal water shader parameter
        private float mWaterPlaneDistance;
        
        private TV_COLOR mTintReflection = new TV_COLOR(1, 1, 1F, .3F); //same as tv's defaults for water
        private TV_COLOR mTintRefraction = new TV_COLOR(1, 1, 1F, .3F); //same as tv's defaults for water
        private float mFresnal = 1.0f; // fresnal to 1.0 if you want to disable refraction

        TVCamera mCamera;

        
        /// <summary>
        /// A very expensive TV water rendering system.  It uses seperate rendering passes for reflection and refraction.
        /// It is possible to go without reflection however (i think)
        /// </summary>
        /// <param name="style">1 is default</param>
        /// <param name="waterPlaneDistance"></param>
        /// <param name="waterPlaneNormal"></param>
        /// <param name="useReflection"></param>
        /// <param name="useRefraction"></param>
        /// <param name="reflectionColor"></param>
        /// <param name="refractionColor"></param>
        /// <param name="customFresnel"></param>
        /// <param name="cb"></param>
        public FXWater(int style, float waterPlaneDistance, Vector3d waterPlaneNormal, bool useReflection,
                       bool useRefraction, TV_COLOR reflectionColor, TV_COLOR refractionColor, double customFresnel,
                       RenderCallback cb)
        {
            mRenderCB = cb;
        	_semantic = FX_SEMANTICS.FX_WATER_LAKE;
            
            // TODO: should we be using a seperate TVGraphicEffect for each water patch?
            //       - it would mean seperate RS draws for reflection and refraction
            //       - but it's the only way if we want seperate settings (colors, fresnals, water plane heights, etc) to do this.
            //       HOWEVER, we can also use LOD where when a patch is far enough away, we switch to a different, non reflecting water shader
            mEffect = new TVGraphicEffect();
            mCamera = CoreClient._CoreClient.CameraFactory.CreateCamera ();
            
			// TODO: first test is to just do a single water patch at plane height = -0.5  and vector 0,1,0
			//       and set mesh size to one zone width/depth and see if it renders below the floor
			//       

            // TODO: global reflection and refraction enabled override local custom properties for each water entity patch
            // WaterReflectionEnabled = CoreClient._CoreClient.Settings.settingReadBool("graphics", "water_reflection_enabled");
            // WaterRefractionEnabled = CoreClient._CoreClient.Settings.settingReadBool("graphics", "water_refraction_enabled");
            
            // TODO: we may want seperate Water plane heights, reflection colors, animation speeds, for each patch!  So i'm thinking these settings are
            //       actually custom properties from the Water patch entity itself.
//         	mStyle;
         	mUseDirection = true;
        	mUseAnimation = true;
        	mDirSpeedX = 0.1f;
        	mDirSpeedZ = 0.1f;
            mFresnal = 0.85f;
            mStyle = style;
            mReflectionEnabled = useReflection;
            mRefractionEnabled = useRefraction;
            mTintReflection = reflectionColor;
            mTintRefraction = refractionColor;
            
            // NOTE: for camera space rendering, the "waterPlaneDistance" has to be continuously updated since the mesh
            //       moves and not the camera!
            mWaterPlane = new TV_PLANE(Helpers.TVTypeConverter.ToTVVector (waterPlaneNormal), waterPlaneDistance);
			mWaterPlaneDistance = waterPlaneDistance;
			
            mReflectRS =
                RenderSurface.CreateRS(RSResolution.R_512x512, CONST_TV_RENDERSURFACEFORMAT.TV_TEXTUREFORMAT_DEFAULT,
                                       true);
            mRefractRS =
                RenderSurface.CreateRS(RSResolution.R_512x512, CONST_TV_RENDERSURFACEFORMAT.TV_TEXTUREFORMAT_DEFAULT,
                                       false);
            
            mReflectRS.SetNewCamera (mCamera);
            mRefractRS.SetNewCamera (mCamera);
        }

        // some FX classes such as this one have multiple RS and you can specify by name like "reflect", "refract"
        public void SetRSResolution(RSResolution resolution, string rsName)
        {
            if (rsName == REFLECTION)
            {
                if (mReflectRS != null) mReflectRS.Release();
                mReflectRS =
                    RenderSurface.CreateRS(resolution, CONST_TV_RENDERSURFACEFORMAT.TV_TEXTUREFORMAT_DEFAULT, true);
            }
            else if (rsName == REFRACTION)
            {
                if (mRefractRS != null) mRefractRS.Release();
                mRefractRS =
                    RenderSurface.CreateRS(resolution, CONST_TV_RENDERSURFACEFORMAT.TV_TEXTUREFORMAT_DEFAULT, false);
            }
            //TODO: Now we must re-call SetWaterReflection to update the new RS's for each mesh that is registered.
            //TODO: need IDispose to handle .Release of the rendersurfaces
        }

        public bool ReflectionEnabled
        {
            get { return mReflectionEnabled; }
            set { mReflectionEnabled = value; }
        }

        public bool RefractionEnabled
        {
            get { return mRefractionEnabled; }
            set { mRefractionEnabled = value; }
        }

        public override void Register(IFXSubscriber subscriber)
        {
            if (subscriber is ModeledEntity == false)
            	throw new ArgumentOutOfRangeException("FXWater.Register() - ERROR: Only elements of type Model may subscribe to this FXProvider.");
                       
            if (subscriber is ModeledEntity == false) throw new ArgumentOutOfRangeException();
                         
            ModeledEntity entity = (ModeledEntity)subscriber;
            Geometry mesh = entity.Model.Geometry;
            
            if (mesh.TVResourceIsLoaded == false) throw new Exception ("FXWater.Register() - TVMesh is null.  LoadTVResource() has not been called yet? Or paging in progress?");
            
            TVMesh tvmesh = CoreClient._CoreClient.Globals.GetMeshFromID(mesh.TVIndex);

            if (tvmesh == null) throw new Exception ("FXWater.Register() - TVMesh is null.  Has LoadTVResource() been called yet?");
            
            base.Register(subscriber);
                        
            // i could pass this effect and do m.Apply(this) 
            // and then an overloaded handler acccepts it as IFXWaterReflection
            // and can access public method SetWaterReflection(tvMesh)  
            // this is same thing i do with shaders.  All this just to keep the underlying tv objects hidden (not public) :/
            // but is this really worth it or even any good?  i mean still we end up having to pass the tvmesh
            // to a method that MUST be known to exist and accepting a specific parameter type.
            // i could use GetIndex and then use globals to grab the mesh...
            // unfortunately, this still assumes that the 3rd party library has access to whatever
            // collection or cache is storing the meshes.  But there is a tvglobals one for GetShader(stringName) ... 
            // However, our ULTIMATE OBJECTIVE is to keep access to _tvMesh or _tvShader hidden... not just from
            // 3rd party users running the compiled Keystone.dll but also for good OOP design for me (or src code licensors)
            // of this framework.
            // TODO: I believe SetWaterReflection() can be called multiple times in order to add multiple meshes to a single effect? Or do i need a seperate effect
            //       for each water mesh?  I think it's why each of the SetWater**** calls below all take a TVMesh as argument
			//       and why we dont just set the mesh one time on the TVEffect.
			//       Still, a would seperate effect would give us some more flexibility when it comes to
            //       complex inland water ways... such as would be necessary in a game like Minecraft where we want to model water
            //       flow?
            if (mReflectionEnabled)
            {
            	// TODO: for camera space rendering, the "waterPlaneDistance" has to be continuously updated since the mesh
           		//       moves and not the camera!  But im not sure if that works
	            mEffect.SetWaterReflection(tvmesh, mReflectRS.RS, mRefractRS.RS, mStyle, mWaterPlane);
	            // BumpAnimation requires TVMesh.ComputeNormals()
            	mEffect.SetWaterReflectionBumpAnimation(tvmesh, mUseDirection, mDirSpeedX, mDirSpeedZ);
	            //NOTE: the reason why a Mesh must be passed in for each call, each time
	            // is because the water TVEffect object uses the mesh as a "key" for handling multiple
	            // water effects with a single TVEffect object.  Thus its possible to have seperate colors and animations
	            // for the different water patches, in fact you can use the same reflect and refract RS's as long as 
	            // the water planes are the same.  However, having away to disassocate meshes would be nice because in my 
	            // model, i think i will use seperate FX classes for when i need distinct looking water... because otherwise
	            // i need to keep seperate arrays for the colors, directions, etc. differences.  
	            mEffect.SetWaterReflectionColor(tvmesh, mTintReflection, mTintRefraction, mFresnal); // set fresnal to 1.0 if you want to disable refraction and then skip the refraction render?
            }
        }

        public override void UnRegister(IFXSubscriber subscriber)
        {
        	base.UnRegister (subscriber);
            //TODO: no way to unset the reflection effect?
        }


        public override void RenderBeforeClear(RenderingContext context)
        {
        	if (_subscribers == null) return;
        	
            bool anyInFrustum = false;

            foreach (IFXSubscriber sub in _subscribers)
            {
                // NOTE: visibility is set during Cull() which occurs before RenderBeforeClear()
                if (sub is ModeledEntity)
                {
                	ModeledEntity entity = (ModeledEntity)sub;
                	if (entity.InFrustum)
                    {
                        anyInFrustum = true;
                    }
                }
            }

            if (anyInFrustum)
            {            	
            	// TODO: the below implies that we need to cull for reflection camera in seperate pass.  Otherwise
            	//       objects casting reflection don't get rendered into the water if we cannot see them!
            	//       TV when rendering every actor, mesh, etc for us will do this reflection cull pass for us.
            	//       Here we have to do it ourselves before calling mRenderCB.Invoke()
            	//
            	//       Furthermore, we have to update the waterplane distance for each patch!
            	//       by -context.Position.y since that value does not get automatically updated by the mEffect!
            	//       Note: we only need -y and not x,z because the mesh itself is rendered like all others properly
            	//       with respect to fixed origin camera, but the shader value does not.  I suspect the best thing is to
            	//       simply use custom water shader if we cannot get Sylvain to add a parameter allowing us to modify the
            	//       water plane distance.  The only thing we would have to do is ensure that all water using this mEffect
            	//       instance is using the same water plane distance as well as other water stats.  So 4 adjacent water meshes
            	//       can all use same effect and be efficient to render since only one cull pass between them.
            	//mWaterPlane.Dist = mWaterPlane
            	//mEffect.SetWaterReflection(tvmesh, mReflectRS.RS, mRefractRS.RS, mStyle, mWaterPlane);
                mCamera.SetCustomProjection (Keystone.Helpers.TVTypeConverter.ToTVMatrix(context.Camera.Projection));
                mCamera.SetCamera (0, 0, 0, (float)context.LookAt.x, (float)context.LookAt.y, (float)context.LookAt.z);
                
                // note: reflectionRS via TVEffect, automatically handles creating reflection camera matrix
                mReflectRS.StartRender(false);
                mRenderCB.Invoke();
                mReflectRS.EndRender();

                if (mRefractionEnabled)
                {
                    mRefractRS.StartRender(false);
                    mRenderCB.Invoke();
                    mRefractRS.EndRender();
                }
            }
        }

        /// <summary>
        /// Registered water Meshes are rendered after all other geometry as long as the 
        /// entityflag "laterender" is set on the Entity.  
        /// In other words, there's no need to override this Render() to implement
        /// special handling for rendering registered water Entity patches.
        /// </summary>
        /// <param name="context"></param>
//        public override void Render(RenderingContext context)
//        {
//        }
    }
}
