using Keystone.Appearance;
using Keystone.Elements;
using Keystone.Entities;
using Keystone.Resource;
using Keystone.Types;
using MTV3D65;
using System;
using System.Collections.Generic;


namespace Keystone.Celestial
{
    public class SkyHelper
    {
        #region Sky
        /// <summary>
        /// Gradient sky provides a cheap atmosperic scattering effect without
        /// doing any atmospheric scattering calculations.  You can easily fade from day to night and back.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="worldDiameter"></param>
        public static void AddGradientSky(Keystone.Portals.Root root, double worldDiameter, double farplane)
        {
            string id = Repository.GetNewName(typeof(ModeledEntity));
            ModeledEntity atmosphereEntity = new ModeledEntity(id);
            atmosphereEntity.CollisionEnable = false; // pickable, but not collidable


            string cloudsTexturePath = @"caesar\shaders\SkyGradient\Clouds.dds";
            //            Texture tex = Diffuse.Create(AppMain._core.GetNewName(typeof(Diffuse)), @"Shaders\SkyGradient\Clouds.dds");
            //            Material mat = Material.Create("SkyGradientMat", new Color(1, 1, 1, 1), new Color(1, 1, 1, 1), new Color(0, 0, 0, 1), new Color(0, 0, 0, 1));


            //TODO: the Mesh isnt being resource tracked this way.  We need a Mesh3d.Create() static-method
            double meshDiameter = 1;
            Mesh3d atmosphereMesh = Mesh3d.CreateUVTriangleStripSphere(.15f, 64, 16, meshDiameter);

            // load Mesh resource immediately since we want to be able to grab diameter so we can compute custom scale
            //atmosphereMesh.LoadTVResource(); // <-- WARNING: LoadTVResource() results in our atmosphere not rendering at all for some reason

            if (worldDiameter > farplane)
                worldDiameter = farplane;

            // compute proper atmosphere hemisphere scale based on world dimensions
            double atmosphereScale = farplane / meshDiameter - 2;

            System.Diagnostics.Debug.WriteLine("EditorWorkspace.ToolBox.AddGradientSky() - Skyscale = " + atmosphereScale.ToString());

            id = Repository.GetNewName(typeof(Model));
            Model atmosphereModel = new Model(id);
            atmosphereModel.CastShadow = false;
            atmosphereModel.ReceiveShadow = false;
            atmosphereModel.Scale = new Vector3d(atmosphereScale, atmosphereScale, atmosphereScale);

            atmosphereModel.AddChild(atmosphereMesh);

            string shaderPath = @"caesar\shaders\SkyGradient\SkyGradient.fx";
            Keystone.Appearance.Appearance appearance = Keystone.Celestial.ProceduralHelper.CreateAppearance(CONST_TV_LIGHTINGMODE.TV_LIGHTING_NONE, shaderPath, cloudsTexturePath, null, null, null);



            Material material = (Material)Repository.Create("Material");

            // diffuse -> overHorizonColor
            material.Diffuse = new Keystone.Types.Color(0.88235294F, 1, 1, 1);
            // ambient -> horizon color
            material.Ambient = new Keystone.Types.Color(0.9294F, 0.898F, 0.8156862745F, 1);
            // specular -> midSkyColor
            material.Specular = new Keystone.Types.Color(0.67843F, 1, 1, 1);
            // emissive -> zenith color
            material.Emissive = new Keystone.Types.Color(0.2549F, 0.6941F, 1, 1);



            //            shader parameters
            //            ----------------
            //            // fogColor -> belowHorizonColor
            //            BelowHorizonColor = new TV_COLOR(0.345F, 0.51372549F, 0.6392156862745F, 1);
            //            TV_COLOR White = new TV_COLOR(1, 1, 1, 1);
            //            CloudsColor = White;
            //            CloudsTranslation = new TV_2DVECTOR(1, -5);
            //            CloudsTiling = new TV_2DVECTOR(1.5F, 1.5F);

            appearance.AddChild(material);

            atmosphereModel.AddChild(appearance);

            atmosphereEntity.AddChild(atmosphereModel);

            //            FXSkyGradient sky =
            //                new FXSkyGradient(FXSkyGradient.InterpolationModes.Linear, model, tex, mat);

            atmosphereEntity.Name = "Sky";
            root.AddChild(atmosphereEntity);
        }



        /// <summary>
        /// Atmosphere entity is the root elements and holds 
        /// 	- Cloud Entity
        /// 		- cloud layer is rendered last and rendererd with TV_BLEND_ADD so that
        ///           Sun, Moon and nightime starmap shine through it.
        /// 	- Sun Entity
        /// 	- Moon Entity
        /// Atmosphere also holds a night-time starmap texture.
        /// Sun and Moon are always inside of the Atmosphere dome but beyond the Cloud dome.
        /// This is so that our atmosphere at least does not need to do any alphablending.
        /// </summary>
        /// <param name="region"></param>
        public static void AddDynamicDayNightCycle(Keystone.Portals.Root root, double worldDiameter, double farplane)
        {
            // TODO: prevent adding of this DayNightCycle if it has already been added once before

            string id = Repository.GetNewName(typeof(ModeledEntity));
            ModeledEntity atmosphereEntity = new ModeledEntity(id);
            atmosphereEntity.CollisionEnable = false; // pickable, but not collidable

            id = Repository.GetNewName(typeof(ModeledEntity));
            ModeledEntity cloudEntity = new ModeledEntity(id);
            cloudEntity.CollisionEnable = false;      // pickable, but not collidable

            id = Repository.GetNewName(typeof(ModeledEntity));
            ModeledEntity sunEntity = new ModeledEntity(id);
            sunEntity.CollisionEnable = false;        // pickable, but not collidable

            id = Repository.GetNewName(typeof(ModeledEntity));
            ModeledEntity moonEntity = new ModeledEntity(id);
            moonEntity.CollisionEnable = false;       // pickable, but not collidable

            id = Repository.GetNewName(typeof(Model));
            Model atmosphereModel = new Model(id);
            id = Repository.GetNewName(typeof(Model));
            Model cloudModel = new Model(id);


            // compute proper atmosphere hemisphere scale based on world dimensions

            string atmosphereMeshPath = @"caesar\shaders\Sky\Hemisphere.tvm";
            Mesh3d atmosphereMesh = (Mesh3d)Repository.Create(atmosphereMeshPath, "Mesh3d");
            atmosphereMesh.MeshFormat = CONST_TV_MESHFORMAT.TV_MESHFORMAT_TEX1 | CONST_TV_MESHFORMAT.TV_MESHFORMAT_NOLIGHTING;

            // load Mesh resource immediately since we want to be able to grab diameter so we can compute custom scale
            //atmosphereMesh.LoadTVResource(); // <-- WARNING: LoadTVResource() results in our atmosphere not rendering at all for some reason
            double meshDiameter = 150; // atmosphereMesh.BoundingSphere.Radius;

            if (worldDiameter > farplane)
                worldDiameter = farplane;

            double atmosphereScale = farplane / meshDiameter - 2;

            System.Diagnostics.Debug.WriteLine("EditorWorkspace.ToolBox.AddDynamicDayNightCycle() - Skyscale = " + atmosphereScale.ToString());

            atmosphereModel.CastShadow = false;
            atmosphereModel.ReceiveShadow = false;
            atmosphereModel.Scale = new Vector3d(atmosphereScale, atmosphereScale, atmosphereScale);

            string nighttimeStarsTexture = @"caesar\shaders\Sky\Starmap.dds";
            string shaderPath = @"caesar\shaders\Sky\SkyShader.fx";
            Keystone.Appearance.Appearance appearance = Keystone.Celestial.ProceduralHelper.CreateAppearance(CONST_TV_LIGHTINGMODE.TV_LIGHTING_NONE, shaderPath, nighttimeStarsTexture, null, null, null);
            atmosphereModel.AddChild(appearance);
            atmosphereModel.AddChild(atmosphereMesh);
            // NOTE: atmosphere entity should be positioned so that it's bottom is below the horizon slightly
            // TODO: why is it that positioning this model currently has it's y = 0 mesh bottom not flush with horizon?  I have to lower it substantially.  Is it
            //       inheriting the height of Sun?  I think it is!  In fact that's why i have to lower the thing 1000 meters!  And there seems no way to avoid
            //       the sky from inheriting the position (we can only stop inheritance of scale and rotation)
            //       - option1: make atmosphere the parent and sun, moon, clouds, rain, etc, the children
            //       - option2: make a graphic-less parent entity that is parent to all the above
            // TODO: cloudsphere  http://www.riemers.net/eng/Tutorials/XNA/Csharp/Series4/Skydome.php

            atmosphereEntity.AddChild(atmosphereModel);

            string scriptPath = @"caesar\scripts\daynightcycle_atmosphere.css";
            Keystone.Celestial.ProceduralHelper.MakeDomainObject(atmosphereEntity, scriptPath);

            // CLOUDS ENTITY
            Mesh3d cloudMesh;
            bool zakClouds = true;
            if (zakClouds)
            {
                string cloudMeshPath = @"caesar\shaders\Sky\dome.tvm";
                cloudMesh = (Mesh3d)Repository.Create(cloudMeshPath, "Mesh3d");
                cloudMesh.MeshFormat = CONST_TV_MESHFORMAT.TV_MESHFORMAT_TEX1 | CONST_TV_MESHFORMAT.TV_MESHFORMAT_TEX2;

                double cloudScale = atmosphereScale;
                cloudModel.Scale = new Vector3d(cloudScale, cloudScale, cloudScale);

                string cloudTexture1 = @"caesar\shaders\Sky\CloudsLots.dds";
                string cloudTexture2 = @"caesar\shaders\Sky\CloudsLess.dds";

                shaderPath = @"caesar\shaders\Sky\CloudsShader.fx";
                appearance = Keystone.Celestial.ProceduralHelper.CreateAppearance(CONST_TV_LIGHTINGMODE.TV_LIGHTING_NONE, shaderPath, cloudTexture1, cloudTexture2, null, null);

                // cube texture we need to add seperately
                // TODO: Zak's use of this cubeNOrmalizer seems unusual compared to other cloud shader implementation.  I believe the idea is that
                //       this texture holds the normals and that it's designed specifically to deal with the pancake shaped dome...   
                string cloudCubeTexture = @"caesar\shaders\Sky\CubeNormalizer_ULQ.dds";
                Keystone.Appearance.Layer textureLayer = (Layer)Keystone.Resource.Repository.Create("CubeMap");
                Keystone.Appearance.Texture tex = (Texture)Keystone.Resource.Repository.Create(cloudCubeTexture, "Texture");
                tex.TextureType = Texture.TEXTURETYPE.Cube;
                textureLayer.AddChild(tex);
                appearance.AddChild(textureLayer);
            }
            else // permutating clouds
            {
                float width = (float)farplane * 1.8f;
                cloudMesh = Mesh3d.CreateFloor(width, width, 1, 1, 1.0f, 1.0f);
                cloudMesh.CullMode = (int)CONST_TV_CULLING.TV_DOUBLESIDED;  // or invert normals will work so long as we dont fly above cloud layer

                string cloudTexturePath = @"caesar\shaders\Sky\CloudPermutation.png";
                int cloudTextureIndex = CloudsPermutating_GenerateNoise(cloudTexturePath);

                shaderPath = @"caesar\shaders\Sky\CloudsPermutatingShader.fx";
                appearance = Keystone.Celestial.ProceduralHelper.CreateAppearance(CONST_TV_LIGHTINGMODE.TV_LIGHTING_NONE, shaderPath, cloudTexturePath, null, null, null);
            }

            // NOTE: even though the cloud is rendered in shader which ignores this blending mode
            // setting this blending mode will ensure the proper render order in our PVS list.
            appearance.BlendingMode = CONST_TV_BLENDINGMODE.TV_BLEND_ALPHA;

            cloudModel.AddChild(appearance);
            cloudModel.AddChild(cloudMesh);

            cloudModel.CastShadow = false;
            cloudModel.ReceiveShadow = false;

            string cloudScriptPath = @"caesar\scripts\daynightcycle_clouds.css";
            Keystone.Celestial.ProceduralHelper.MakeDomainObject(cloudEntity, cloudScriptPath);
            cloudEntity.AddChild(cloudModel);
            cloudEntity.Translation = new Vector3d(0, 1000, 0);

            //        	// TODO: compute sunScale dynamically based on it's orbital radius which we should also compute
            //        	//       here ahead of time and based on atmosphere diameter and farplane
            //        	// add sun model

            // SUN ENTITY
            // NOTE: for non-alpha rendering, we typically render Front-2-Back so that we can take advantage of early-z
            // but for sun and moon, we set a blending mode so our regionPVS will render them Back-2-Front.
            // where Atmosphere renders first as NON-ALPHA background.  Then sun, moon and clouds are rendered in that order.  
            // To do this, we designate the Clouds, Sun and Moon as having some form of alpha blending which then uses backToFront comparer
            // when inserting into RegionPVS bucket
            string billboardTexturePath = @"caesar\shaders\Sky\Sun.png"; // dds";
                                                                         //billboardTexturePath = @"caesar\shaders\Planet\SunFlare.png";
            billboardTexturePath = @"caesar\shaders\Planet\Sun.tga";
            //billboardTexturePath = @"pool\OdysseyMedia\Textures\Stars\YellowDwarf.png";

            // WARNING: if fog is enabled, Sun billboard may become completely fogged over and look white as if rendering is failing somehow
            // WARNING: in fact, if fog is enabled and if the fog range is still fairly far, we'll see part of the billboard outline and thinks it's an alpha issue
            //          when it is not
            Model sunModel = Keystone.Celestial.ProceduralHelper.CreateBillboardModel(CONST_TV_BILLBOARDTYPE.TV_BILLBOARD_FREEROTATION,
                                                                                      billboardTexturePath,
                                                                                      "tvdefault",
                                                                                      null,
                                                                                      CONST_TV_BLENDINGMODE.TV_BLEND_ADD,
                                                                                      false);
            sunModel.CastShadow = false;
            sunModel.ReceiveShadow = false;

            // TODO: ideally sunScale should be assigned in script? since it computes sunOrbitRadius too right?
            double sunScale = 55.0d;
            sunModel.Scale = new Vector3d(sunScale, sunScale, sunScale);

            string sunScriptPath = @"caesar\scripts\daynightcycle_sun.css";
            Keystone.Celestial.ProceduralHelper.MakeDomainObject(sunEntity, sunScriptPath);

            sunEntity.AddChild(sunModel);

            // MOON ENTITY
            billboardTexturePath = @"caesar\shaders\Sky\Moon.dds";
            // WARNING: if fog is enabled, Moon billboard may become completely fogged over and look white as if rendering is failing somehow
            // WARNING: in fact, if fog is enabled and if the fog range is still fairly far, we'll see part of the billboard outline and thinks it's an alpha issue
            //          when it is not
            Model moonModel = Keystone.Celestial.ProceduralHelper.CreateBillboardModel(CONST_TV_BILLBOARDTYPE.TV_BILLBOARD_FREEROTATION,
                                                                                       billboardTexturePath,
                                                                                       "tvdefault",
                                                                                       null,
                                                                                       CONST_TV_BLENDINGMODE.TV_BLEND_ADD,
                                                                                       false);
            moonModel.CastShadow = false;
            moonModel.ReceiveShadow = false;
            double moonScale = 10.0d;
            moonModel.Scale = new Vector3d(moonScale, moonScale, moonScale);

            string moonScriptPath = @"caesar\scripts\daynightcycle_moon.css";
            Keystone.Celestial.ProceduralHelper.MakeDomainObject(moonEntity, moonScriptPath);

            moonEntity.AddChild(moonModel);

            // assemble hierarchy (sky, clouds, moon, sun, directional light, lightning, rain) then add day night cycle entity hierarchy to region
            atmosphereEntity.Translation = Vector3d.Zero(); // the bottom of the dome is the y origin of the model too so 0,0,0 is good for translation.
            atmosphereEntity.AddChild(cloudEntity);

            Keystone.Lights.DirectionalLight directionLight = (Keystone.Lights.DirectionalLight)Repository.Create("DirectionalLight");
            // NOTE: without proper range, the light bbox will not be visible and if the lightbox is not visible
            // no shadowing will occur.  
            // NOTE: this is might be a good way to one day disable the light for underground areas...
            // by essentially testing for CSG (constructive solid geometry) subtraction of the directionlight box
            // with that by the underground volume.
            directionLight.Range = float.MaxValue;

            // NOTE: the directionLight is added as child to overall atmosphereEntity
            // and NOT just the sunEntity because when the sun is disabled, the directionLight
            // must switch direction of moon and stay active and not itself be disabled by virtue 
            // of being child to disabled SunEntity.
            atmosphereEntity.AddChild(directionLight);// TODO: id prefer if this was child of root
                                                      // sunEntity.AddChild (directionLight);
            atmosphereEntity.AddChild(sunEntity);
            atmosphereEntity.AddChild(moonEntity);

            // since there can only be one of these, it's best to just add directly rather than use brush
            root.AddChild(atmosphereEntity);
            //ActivateBrush (atmosphereEntity, KeyCommon.Flags.EntityFlags.AllEntities_Except_Root, true);
        }

        // CloudsPermutating script custom properties
        float inverseCloudVelocity = 16.0f;
        float CloudCover = -0.1f;
        float CloudSharpness = 0.5f;
        float numTiles = 16.0f;

        private static void CloudsPermutating_Update()
        {
            //        noiseEffect.Parameters["time"].SetValue((float)gameTime.TotalGameTime.TotalSeconds / inverseCloudVelocity);
            //        noiseEffect.Parameters["SunColor"].SetValue(this.sunColor);
            //        
            //        noiseEffect.Parameters["numTiles"].SetValue(numTiles);
            //        noiseEffect.Parameters["CloudCover"].SetValue(cloudCover);
            //        noiseEffect.Parameters["CloudSharpness"].SetValue(cloudSharpness);
        }

        private static int CloudsPermutating_GenerateNoise(string path)
        {
            // http://thegoldenmule.com/blog/2013/01/real-time-clouds-pt-1-a-study-of-noise-functions/
            // TODO: what would using some other random noise texture look like?
            // 256 x 32bit permutation values
            int[] perm = { 151,160,137,91,90,15,
            131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
            190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
            88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
            77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
            102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
            135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
            5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
            223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
            129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
            251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
            49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
            138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
            };

            // 48 gradient values
            int[] gradValues = { 1,1,0,
                -1,1,0, 1,-1,0,
                -1,-1,0, 1,0,1,
                -1,0,1, 1,0,-1,
                -1,0,-1, 0,1,1,
                0,-1,1, 0,1,-1,
                0,-1,-1, 1,1,0,
                0,-1,1, -1,1,0,
                0,-1,-1
            };

            // the subsequent call to .LockTexture() requires 16bit or 32bit texture format
            CoreClient._CoreClient.TextureFactory.SetTextureMode(CONST_TV_TEXTUREMODE.TV_TEXTUREMODE_16BITS);
            int iTexture = CoreClient._CoreClient.TextureFactory.CreateTexture(256, 256);

            bool locked = CoreClient._CoreClient.TextureFactory.LockTexture(iTexture);

            Keystone.TileMap.Pixel[] pix = new Keystone.TileMap.Pixel[256 * 256];

            try
            {
                // generate texture using permuation values and gradient values
                for (int i = 0; i < 256; i++)
                {
                    for (int j = 0; j < 256; j++)
                    {

                        int offset = i * 256 + j;
                        byte value = (byte)perm[(j + perm[i]) & 0xFF];

                        pix[offset].A = (byte)(gradValues[value & 0x0F] * 64 + 64);
                        pix[offset].R = (byte)(gradValues[value & 0x0F + 1] * 64 + 64);
                        pix[offset].G = (byte)(gradValues[value & 0x0F + 2] * 64 + 64);
                        pix[offset].B = value;

                        CoreClient._CoreClient.TextureFactory.SetPixel(iTexture, i, j, pix[offset].ARGB);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            finally
            {
                CoreClient._CoreClient.TextureFactory.UnlockTexture(iTexture);

                CoreClient._CoreClient.TextureFactory.SaveTexture(iTexture, path);
            }

            return iTexture;
        }
        #endregion
    }
}
