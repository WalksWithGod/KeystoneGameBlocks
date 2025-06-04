using System;
using System.Diagnostics;
using Keystone.Cameras;
using Keystone.Elements;
using Keystone.Entities;
using Keystone.RenderSurfaces;
using Keystone.Types;
using MTV3D65;
using Matrix = Microsoft.DirectX.Matrix;

namespace Keystone.FX
{
    // E:\dev\_projects\_TV\Zak_BluePlanet
    // Like the FXImposters.cs, this class manages the billboard rendering of things (e.g. planets, death stars) that are too far to be drawn normally 
    // or scaled (actually im thinking why not handle scaled here too?  It does either or).
    // Also, it's probably best only for spheres.  We'll use our other Imposter System for other types of objects right? 
    // Though its important to remember that, THAT imposter system draws vertices in world space whereas here we draw them in 2d screenspace because
    // taht way we guarantee never to have zbuffer problems for even extremely far things.
    // 
    // Unfortunately drawing in screen space like this makes sorting with other NON planet imposters difficult.  If a ship travels behind the moon or in front
    // if we draw the imposters always first, then the ship will always be behind unless it too uses the imposter.  That could make sense because
    // anything far enough to be BEHIND a moon or planet this also an imposter then too must be an imposter and will get auto sorted.
    // HOWEVER, this means then DrawScaled should be handled outside of here in the regular pipeline...  That way a ship that is further away and will use
    // an imposter will get drawn first here, prior to the rest of the scene which will get drawn on top.
    //
    // And dont forget, i think there was still the issue of drawing the moon billboard in front of the earth, but actually maybe i solved that with the
    // alpha RS?  Maybe that worked fine but it was the earths billboard atmosphere when in front of the moon billboard?  Need to test
    public class FXPlanetAtmosphere : FXBase
    {
        private struct ScreenSpaceImposterData
        {
            public Entity Entity;
            public RenderSurface RS;
			public bool Enable;
			public Vector3d[] TextureCoords;
            //based on distance we will draw the impster.  Note: We will no longer DrawScaled here because it prevents us from properly handling depth sorting
        }

        private double _max_visible_range;
        private bool _screenshot = false;
		private MTV3D65.TVCamera mTVCamera;

        // TODO: perhaps every planet that gets loaded should register itself here and then this FX class can
        //       update planet imposters as needed.
        public FXPlanetAtmosphere(double max_draw_distance)
        {
            _max_visible_range = max_draw_distance;
            mTVCamera = CoreClient._CoreClient.CameraFactory.CreateCamera();
        }

        // http://msdn.microsoft.com/en-us/library/bb197900.aspx
        // http://forums.create.msdn.com/forums/p/44093/262427.aspx  <-- render whole object to fit screen
        // http://forums.create.msdn.com/forums/p/44093/262614.aspx  

        // E:\dev\_projects\_TV\Zak_BluePlanet
        // The goal here is to create a new projection matrix that will fit the extents of the model
        // tightly.  We have to create our own projection matrix since tv3d by default uses the height/width 
        // of the viewport and we want to use the height/width of the model when placed at its proper world coords
        // returns the billboard texture coords for the Draw_Texture function
        // TODO: Make sure our ModelBase is calculating its BoundingVolume properly when for instance adding the larger Atmosphere model
        //       as a child to the Earth model which has smaller radius.  
        // TODO: the context we pass in here should be one specifically for rendering imposters and NOT
        //       our general one.  In fact, we should create the context here and use that.
        private Vector3d[] GenerateImposter(RenderingContext context, Entity entity, RenderSurface imposter)
        {
            Vector3d targetPositionWS = entity.Translation;

            // -------------------------------------------------------------------------
            // compute variables for correct camera perspective projection matrix
            Vector3d direction = context.Position  - targetPositionWS;
            double distance;
            direction = Vector3d.Normalize (direction, out distance);
            Vector3d orientation = -direction; // facing the camera
            double distanceSquared = distance * distance;
                        
            double boundingRadius = entity.BoundingSphere.Radius;
			double boundingRadiusSquared = boundingRadius * boundingRadius;
            //E:\dev\_projects\_TV\Zak_BluePlanet
            // The whole point of this is to get the proper billboard width
            // for setting up the perspective projection matrix which will allow us to render the mesh
            // to the RS at maximum size and  fully visible/contained within the frustum bounds.
            
            // fAltitude is distance to the surface of the target's bounding radius
            double fAltitude = distance - boundingRadius; 

            // Distance to horizon (use pythagorean theorem a2 = c2 - b2)
            // TODO: (This bug goes away when i stop tying in the GenerateBillboard with the Scaling.  See notes
            // in the RenderBody() about how to breakt his up. NOTE: Maybe this is the cause of the weird billboard texture coords too.
            // Assert that distance2 is > boundingRadius squared or else fHorizon will = 0 and this wont work!
            // so the distance has to be further out.  But its complicated by the fact that the scale depends
            // on the distance too.  So why is this now causing problems? 
            Trace.Assert(distanceSquared > boundingRadiusSquared);
            // NOTE: Its still possible however to try and render as an imposter something that is too close and that causes this assert to fail. 
            // (but for the most part, final game with large planets if we're properly determining which render path to use 
            // (ie. drawScaled vs drawBillboard) then this shouldnt be a problem.  
            double fHorizon = Math.Sqrt(distanceSquared - boundingRadiusSquared);

            // Cosine of angle between ray to center of sphere and ray to horizon (part of a right triangle) 
            double fCos = fHorizon / distance;

            // Horizon distance cut off at the near clipping plane
            double fTemp = fAltitude / fCos; 

            // Distance from center to sides at the near clipping plane (boundaries of the projection frustum) (part of another right triangle)
            double fBound = Math.Sqrt(fTemp * fTemp - fAltitude * fAltitude); 
            double fTemp2 = distance / fCos;

            // Distance from center to sides at the center of the object (size of the billboard)
            double targetRadiusWorldSpace = Math.Sqrt(fTemp2 * fTemp2 - distanceSquared); 

            double fScreenSpace = targetRadiusWorldSpace / distance;
            // TODO: the screenSpace size should be used to grab an appropriate resolution
            // Imposter from the pool.  The less screenspace the smaller surface area the imposter 
            // needs to be to look acceptable.
            double targetNearPlaneDiameterWorldSpace = fBound * 2;
            
            // NOTE: the near plane is computed as "boundingRadius" distance away from target
            // this will give us real good zbuffer precision when rendering the planet to billboard
            double near = fAltitude;
            double far = distance + boundingRadius;


            // -------------------------------------------------------------------------
            // Compute Billboard Orientation and Texture Coords
        	Vector3d dummy = Vector3d.Zero();
            Vector3d modelUp = Vector3d.Zero();
            context.Camera.GetBasisVectors(ref dummy, ref modelUp, ref dummy); // grab up vector prior to setting the new projection

            // compute the world vectors for our billboard
            Vector3d modelSide = Vector3d.CrossProduct(modelUp, orientation);
            modelSide = Vector3d.Normalize(modelSide);
            modelUp = Vector3d.CrossProduct(orientation, modelSide);
            //TODO: do we need to normalize modelSide and modelUp? i think we do
            modelUp = Vector3d.Normalize(modelUp); 

            // NOTE: we dont have to scale anything by fScalingFactor because we are using the proper distance to compute the billboard radius
            //       unless we are worried that even with close tight frustum, if planet is extremely large, it might have zbuffer precision issues
            //fScalingFactor = GetScalingFactor(cam.GetPosition(), target);
            //modelSide = _maths.VScale(modelSide, fScalingFactor);
            // modelUp = _maths.VScale(modelUp, fScalingFactor);
            //direction = _maths.VScale(direction, fScalingFactor);
            // target = _maths.VScale(target, fScalingFactor);
            
            Vector3d vNeg = modelUp * targetRadiusWorldSpace;
            vNeg += modelSide * -targetRadiusWorldSpace;

            Vector3d[] textureCoords = new Vector3d[2];
            textureCoords[0] = targetPositionWS + vNeg;
            // NOTE: we are using direction vector here instead of position.  Hence the need to adjust the billboardRadius earlier
            textureCoords[1] = targetPositionWS - vNeg;

            //Vector3.Project();
            //Vector3.Unproject();
            //Microsoft.DirectX.UnsafeNativeMethods.Vector3.TransformCoordinateArray();
            // NOTE: http://www.gamasutra.com/features/20060105/listing5.txt
            // the above listing creates a rectangle for the texture but the downside is its not a perfect square
            // as is with the radius method

            // -------------------------------------------------------------------------
            // Get the suggested resolution for this impostor and see if it's changed since last time
            //short nOldResolution = _Resolution;
            //_Resolution = GetImpostorResolution(GetImpostorScreenSpace(distance));
            //if (nOldResolution != _Resolution)   // then Create a new RS.  Ideally send this operation to the pager?
            
            // -------------------------------------------------------------------------
            // setup camera and render
            // TODO: verify that our own Matrix can create PerspectiveLH projection matrices properly
            Matrix projection = Matrix.PerspectiveLH( (float)targetNearPlaneDiameterWorldSpace, (float)targetNearPlaneDiameterWorldSpace, (float)near,(float) far);
            mTVCamera.SetCustomProjection (Helpers.TVTypeConverter.ToTVMatrix(projection));
            mTVCamera.SetCamera(0f, 0f, 0f, (float)targetPositionWS.x, (float)targetPositionWS.y,(float)targetPositionWS.z);
	
            // Render the Model to the RS
            // clear color is Fuchsia, and alpha component must be 0 so we dont see the border and only the planet.  
            imposter.SetBackgroundColor(CoreClient._CoreClient.Globals.RGBA(255, 0, 255, 0)); //Fuchsia 
            imposter.StartRender();
 
            // TODO: we just want to render this Entity to the RS. I forget how this callback works? i think it's old and i never updated.  
            //       i think we want to handle "RenderBeforeClear()" callback ourselves so we can update the imposter beforeclear.
            //       and then our normal LOD mechanism can select the billboard if applicable
            //       We may need a special callback that allows us to specify a single Entity to be rendered 
            // eg mRenderEntityCB.Invoke();   <-- the traverser for that would have to iterate until it found just that entity, and so it could still build proper regionPVS with proper offset
            mRenderCB.Invoke(); 
            Trace.Assert(true, "The above line is commented out but we need to implement method to render this Entity.");
            imposter.EndRender(); // seems like there's still Zbuffer issues rendering the billboard :/
                     
            // -------------------------------------------------------------------------
            // output to texture so we can see if it works
            if (_screenshot)
            {
                imposter.SaveTexture(@"E:\dev\c#\KeystoneGameBlocks\Data\previews\planet_imposter.dds", CONST_TV_IMAGEFORMAT.TV_IMAGE_DDS);
                _screenshot = false;
            }
            
            return textureCoords;
        }

        private static short GetImpostorResolution(double fScreenSpace)
        {
            short nResolution;
            if (fScreenSpace > 1.0f)
                nResolution = 0;
            else if (fScreenSpace > 0.5f)
                nResolution = 512;
            else if (fScreenSpace > 0.2f)
                nResolution = 256;
            else if (fScreenSpace > 0.08f)
                nResolution = 128;
            else if (fScreenSpace > 0.032f)
                nResolution = 64;
            else if (fScreenSpace > 0.0128f)
                nResolution = 32;
            else if (fScreenSpace > 0.00512f)
                nResolution = 16;
            else if (fScreenSpace > 0.002048f)
                nResolution = 8;
            else
                nResolution = 4;
            return nResolution;
        }

        #region IFXProvider Members
        public override void Notify(IFXSubscriber subscriber)
        {
            throw new NotImplementedException();
        }

        public override void Register(IFXSubscriber subscriber)
        {
            base.Register(subscriber);

            subscriber.FXData = new FXSubscriberData[0];

            ScreenSpaceImposterData data = new ScreenSpaceImposterData();
            data.Entity = (Entity)subscriber;

            //          NOTE: 512 x 512 is about the largest we'll want to use.  If its very very far away (and or very far and small like a 
            //          moon of jupitor then we could go as low as 8x8 (ditto for asteroids)
            //          earthImposter = _scene.CreateAlphaRenderSurface(512, 512, true);

            // TODO: dont we want to be able to dynamically change the RS size based on distance?  Our GenerateImposter class can do this by
            //       simply grabbing RS's as needed from the RS Cache?
            data.RS =
                RenderSurface.CreateRS(RSResolution.R_512x512, CONST_TV_RENDERSURFACEFORMAT.TV_TEXTUREFORMAT_A8R8G8B8,
                                       true);

            mTVCamera = CoreClient._CoreClient.CameraFactory.CreateCamera ();
            data.RS.SetNewCamera (mTVCamera);
            
            subscriber.FXData[0].Data = new object[1] { data };
        }

        public override void UnRegister(IFXSubscriber subscriber)
        {
            _subscribers.Remove(subscriber);
        }

        public override void SetRSResolution(RSResolution res)
        {
            throw new NotImplementedException();
        }

		public override void RenderBeforeClear(RenderingContext context)
		{
			base.RenderBeforeClear(context);

            foreach (IFXSubscriber subscriber in _subscribers)
            {
                ScreenSpaceImposterData data = (ScreenSpaceImposterData)subscriber.FXData[0].Data[0];
                if (!data.Enable) continue;

                // note the imposters are generated before clear
                Vector3d[] coords = GenerateImposter(context, data.Entity, data.RS);
        //        data.TextureCoords = GenerateImposter(camera, data.Entity, data.RS);
                // we no longer need to send the subscript
                //                                   because we can now simply use the overal bounding volume of structData.Entity.BoundingVolume();
            }
        }


        public override void Render(RenderingContext context)
        {
//            foreach (IFXSubscriber subscriber in _subscribers)
//            {
//                ScreenSpaceImposterData data = (ScreenSpaceImposterData)subscriber.FXData[0].Data[0];
//                if (!data.Enable) continue;
//                // E:\dev\_projects\_TV\Zak_BluePlanet
//                //if (!_drawBillboard)
//                //{
//                //    if (_drawScaled)
//                //    {
//                //        double scale = GetScalingFactor(camPos, earthPos);
//                //        DrawScaled(new TVMesh[] { earth, clouds, atmosphereFront }, scale, camPos);
//                //        scale = GetScalingFactor(camPos, moonPos);
//                //        DrawScaled(new TVMesh[] { moon }, scale, camPos);
//                //    }
//                //    else
//                //    {
//                //        // maybe its ok to have these each as seperate Mesh3d's
//                //        // its just a question of how to enforce the rendering order.
//                //        // although having a Composite mesh of sorts that singularly controls
//                //        // position, rotation, scale for the entire hierarchy allows us to do things like 
//                //        // GetScalingFactor only once and to render the children appropriately with those pre-cacled values
//                //        earth.Render();
//                //        clouds.Render();
//                //        atmosphereFront.Render();
//                //        moon.Render();
//                //    }
//                //}
//                //else
//                //{
//                CoreClient._CoreClient.Maths.Project3DPointTo2D(Helpers.TVTypeConverter.ToTVVector(data.TextureCoords[0]), ref data.TextureCoords[0].x,
//                                                    ref data.TextureCoords[0].y, true);
//                CoreClient._CoreClient.Maths.Project3DPointTo2D(Helpers.TVTypeConverter.ToTVVector(data.TextureCoords[1]), ref data.TextureCoords[1].x,
//                                                    ref data.TextureCoords[1].y, true);
//                CoreClient._CoreClient.Screen2D.Action_Begin2D();
//                // NOTE: Renderall Billboards.  
//                // TODO: Billboards need to be depth sorted before rendering.
//                //Core._CoreClient.Screen2D.Settings_SetAlphaBlendingEx(true, CONST_TV_BLENDEX.TV_BLENDEX_SRCALPHA  , CONST_TV_BLENDEX.TV_BLENDEX_INVSRCALPHA);
//                //Core._CoreClient.Screen2D.Settings_SetAlphaTest( false);
//                //Core._CoreClient.Screen2D.Settings_SetAlphaBlending(true,CONST_TV_BLENDINGMODE.TV_BLEND_ALPHA);
//                CoreClient._CoreClient.Screen2D.Draw_Texture(data.Imposter.GetTexture(), data.TextureCoords[0].x,
//                                                 data.TextureCoords[0].y, data.TextureCoords[1].x,
//                                                 data.TextureCoords[1].y, -1, -1, -1, -1);
//
//                CoreClient._CoreClient.Screen2D.Action_End2D();
//                // }
//            }
        }

        #endregion
    }
}
