using System;
using System.Collections.Generic;
using Keystone.Elements;
using Keystone.Resource;
using Keystone.Appearance;
using Keystone.Entities;
using Keystone.Types;
using KeyCommon.Flags;

namespace KeyEdit.ContentCreation
{
    internal class Helper
    {

        private static Dictionary<string, ModeledEntity> mProxies = new Dictionary<string, ModeledEntity>();

        internal static void SetTranslation(string id, Vector3d translation)
        {
            // TODO: it would be nice if the SensorContact had a reference
            // to the proxy.
            mProxies[id].Translation = translation;
            mProxies[id].LatestStepTranslation = translation;
        }

        private static string GetProxyKey (int viewportTVIndex, string sourceEntityID)
        {
        	return sourceEntityID + "_" + viewportTVIndex.ToString(); // TODO: can we ensure that these TVIndices wont get recycled when changing viewport configuration?
        }
        
        #region load // Move to ProceduralHelper

        internal static ModeledEntity Create3DProxyBare(int viewportTVIndex, string identifier, string sourceEntityID)
        {
        	// NOTE: if we want to have multiple proxies for same entity, then the identifier must be unique
        	//       such as one identifier for proxy for planet, a different identifier for orbit elliptical path mesh.

            // proxy's must be key'd uniquely for each viewport.  That is, proxies are only shareable 
        	// within their own RenderingContext, and not between different RenderingContexts.
        	string proxyKey = GetProxyKey (viewportTVIndex, sourceEntityID) + "_" + identifier;
        	
            ModeledEntity me;
            if (mProxies.TryGetValue(proxyKey, out me))
                return me;

            me = new Proxy3D(proxyKey, sourceEntityID);
            mProxies.Add(proxyKey, me);
            
            return me;
        }
        // note: these proxy entities are cached so we dont have to create them for existing proxy
        // elements
        // TODO: we don't have a system to expire unused proxy elements
        internal static ModeledEntity Create3DProxyBillboard(int viewportTVIndex, string sourceEntityID, string texturePath)
        {

            // proxy's must be key'd uniquely for each viewport.  That is, proxies are only shareable 
        	// within their own RenderingContext, and not between different RenderingContexts.
        	string proxyKey = GetProxyKey (viewportTVIndex, sourceEntityID);
        	
            ModeledEntity me;
            if (mProxies.TryGetValue(proxyKey, out me))
                return me;

            Model m = Load3DBillboardIcon(texturePath, 1f, 1f);

            me = new Proxy3D(proxyKey, sourceEntityID);
            me.AddChild(m);
            me.UseFixedScreenSpaceSize = true;
            me.ScreenSpaceSize = 0.05f;

            mProxies.Add(proxyKey, me);

            return me;
        }


        // TODO: need a way to prune out proxy elements from cache that are not used for x interval
        // TODO: why is this not just a "Create()" method inside of EntityProxy?
        /// <summary>
        /// Creates a Billboard3d geometry based InputAwareProxy entity.  If a cached copy exists since
        /// previous frame, return it instead of creating a new one. 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="texturePath"></param>
        /// <param name="defaultMaterial"></param>
        /// <param name="mouseEnter"></param>
        /// <param name="mouseLeave"></param>
        /// <param name="mouseDown"></param>
        /// <param name="mouseUp"></param>
        /// <param name="mouseClick"></param>
        /// <returns></returns>
        internal static ModeledEntity Create3DProxy(int viewportTVIndex, Entity entity, 
                                                    Material.DefaultMaterials defaultMaterial)
        {
        	// proxy's must be key'd uniquely for each viewport.  That is, proxies are only shareable 
        	// within their own RenderingContext, and not between different RenderingContexts.
        	string proxyKey = GetProxyKey (viewportTVIndex, entity.ID);
        	
            ModeledEntity p;
                        // TODO: what if we want to change the style of the proxy when changing camera zooms?  that value is not reflected in the proxyKey
            //       so when trying to change proxies, it will see a cached copy already exists and use it instead of creating new one with new style
            //       TODO: - we could always check the existing proxy's scale and update if necessary since that is main aspect of what makes up a "style"
            //            but it still doesn't help us with the 2D icon proxy style
            if (mProxies.TryGetValue(proxyKey, out p))
                return p;

            Model model = (Model)Repository.Create ("Model");
            
            Mesh3d mesh = Mesh3d.CreateSphere (1, 15, 15, false);
            model.AddChild (mesh);
            
            Material blue = Material.Create(defaultMaterial);
            model.AddChild ((DefaultAppearance)Repository.Create("DefaultAppearance"));
            model.Appearance.AddChild(blue);

            Proxy3D proxy = new Proxy3D (proxyKey, entity);
            proxy.AddChild (model);
			mProxies.Add(proxyKey, proxy);
            return proxy;
        }
        
        // TODO: need a way to prune out proxy elements from cache that are not used for x interval
        // TODO: why is this not just a "Create()" method inside of EntityProxy?
        /// <summary>
        /// Creates a Billboard3d geometry based InputAwareProxy entity.  If a cached copy exists since
        /// previous frame, return it instead of creating a new one. 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="texturePath"></param>
        /// <param name="defaultMaterial"></param>
        /// <param name="mouseEnter"></param>
        /// <param name="mouseLeave"></param>
        /// <param name="mouseDown"></param>
        /// <param name="mouseUp"></param>
        /// <param name="mouseClick"></param>
        /// <returns></returns>
        internal static ModeledEntity Create2DProxy(int viewportTVIndex, Entity entity, string texturePath,
                                                    Material material,
                                                    EventHandler mouseEnter, EventHandler mouseLeave,
                                                    EventHandler mouseDown, EventHandler mouseUp,
                                                    EventHandler mouseClick)
        {
        	   	
        	// proxy's must be key'd uniquely for each viewport.  That is, proxies are only shareable 
        	// within their own RenderingContext, and not between different RenderingContexts.
        	string proxyKey = GetProxyKey (viewportTVIndex, entity.ID);
        	
            ModeledEntity p;
            // TODO: what if we want to change the style of the proxy when changing camera zooms?  that value is not reflected in the proxyKey
            //       so when trying to change proxies, it will see a cached copy already exists and use it instead of creating new one with new style
            //       TODO: - we could always check the existing proxy's scale and update if necessary since that is main aspect of what makes up a "style"
            //            but it still doesn't help us with the 2D icon proxy style
            if (mProxies.TryGetValue(proxyKey, out p))
                return p;

            Model model = Load2DScreenspaceIcon (texturePath);
            
            model.Appearance.RemoveMaterial();
            model.Appearance.AddChild(material);
            
            ProxyControl2D proxy = ProxyControl2D.Create(entity, model, mouseEnter, mouseLeave, mouseDown, mouseUp, mouseClick);

            mProxies.Add(proxyKey, proxy);
            return proxy;
        }

        internal static Model Load2DScreenspaceIcon(string screenspaceTexturePath)
        {
            string newID = Repository.GetNewName(typeof(Model));
            Model model = new Model(newID);

            newID = Repository.GetNewName(typeof(Keystone.Elements.TexturedQuad2D));
            TexturedQuad2D quad = (Keystone.Elements.TexturedQuad2D)Keystone.Resource.Repository.Create(newID, "TexturedQuad2D");
            
            Keystone.Appearance.Appearance appearance = Keystone.Celestial.ProceduralHelper.CreateAppearance(MTV3D65.CONST_TV_LIGHTINGMODE.TV_LIGHTING_NORMAL,
                                                                                                            "", screenspaceTexturePath, "", "", "");
           // appearance.BlendingMode = MTV3D65.CONST_TV_BLENDINGMODE.TV_BLEND_ADD;

            model.AddChild(appearance);
            model.AddChild(quad);

            return model;
        }


        internal static Model Load3DBillboardIcon(string billboardTexturePath, float width, float height, MTV3D65.CONST_TV_BILLBOARDTYPE rotationType = MTV3D65.CONST_TV_BILLBOARDTYPE.TV_BILLBOARD_FREEROTATION)
        {
            string id = Repository.GetNewName(typeof(Billboard));

        // TODO: move to extension methods to keystone objects but in meantime
        //       perhaps move all these to static methods in KeyEdit.Helpers.ContentCreation.cs

            // note: because a new Hud.cs instance is created for every RenderingContext
            // this is one of the few times when we'd actually want to share an Entity
            // I think.  Only tricky part is with multiple viewports open and potentially
            // viewing different scenes, id have to make sure the current parent is correct
            // (eg current celledRegion) used by marker is correct and if not, set it.
            // One thing we could potentially do is make mMarker a private static var, but
            // grabbing the entity from the repository using a known name should be just as good.
            // Since these markers are shared across all RenderingContext->Hud instances
            // then they should never be unloaded until the app shuts down.
            // Actually, there is no harm in sharing the entities since the underlying
            // textures and meshes do get shared!  Let's just be consistant and never
            // share entities.  We don't have to.
            System.Diagnostics.Debug.Assert(id != billboardTexturePath, "Texture.cs instance id and Billboard.cs id cannot be same or Repository will retreive wrong type.");
            Model model = new Model(Repository.GetNewName(typeof(Model)));

            // create the mesh
            string shareableBillboardID = Billboard.GetCreationString (rotationType,
               								true, width, height);

            Billboard markerMesh = (Billboard)Repository.Create (shareableBillboardID, "Billboard");
         
            //markerMesh.SetTextureClamping (true); // TODO: should textureClamping be part of Appearance?

            Material emissiveWhite = Material.Create(Material.DefaultMaterials.white_fullbright);
            Keystone.Appearance.Appearance appearance = Keystone.Celestial.ProceduralHelper.CreateAppearance(MTV3D65.CONST_TV_LIGHTINGMODE.TV_LIGHTING_NORMAL, null, billboardTexturePath, null, null, null);
            appearance.RemoveMaterial();
            appearance.AddChild(emissiveWhite);

            appearance.BlendingMode = MTV3D65.CONST_TV_BLENDINGMODE.TV_BLEND_ADD;  // TV_BLEND_ADD is correct so that starfield shines through

            model.AddChild(appearance);
            model.AddChild(markerMesh);

            return model;
            // TODO: im incrementRef to keep cached, but im never decrementRef when unloading/disposing hud and i should be
            //Repository.IncrementRef(null, mLightProxyIcon);

        }

        #endregion

    }
}
