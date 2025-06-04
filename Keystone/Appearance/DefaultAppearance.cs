using System.Xml;
using Keystone.Elements;
using Keystone.Entities;
using Keystone.IO;
using Keystone.Resource;
using Keystone.Shaders;

namespace Keystone.Appearance
{
    /// <summary>
    /// Overall Appearance for Meshes and Actors.  See SplatAppearance for Terrains.
    /// </summary>
    public class DefaultAppearance : Appearance
    {
        private bool mEnableGroupRenderingOrder;
        private int[] mGroupRenderingOrder;
        private int[] mEnabledGroups; // configured via plugin checkbox in the treeModel 

        /// <summary>
        /// Note: No static Create methods since Appearance nodes cannot be shared.
        /// </summary>
        /// <param name="id"></param>
        internal DefaultAppearance(string id)
            : base(id)
        {
            mEnableGroupRenderingOrder = false;
        }

        public override void MoveChildOrder (string childID, bool down)
        {
            // this must change the mGroupRenderingORder and
            // if the order does not equal the default order
            // must enable mEnableGroupRenderingOrder
            // and set the change flag
            base.MoveChildOrder(childID, down);

            if (_children == null || _children.Count <= 1) 
            {
                mEnableGroupRenderingOrder = false;
                mGroupRenderingOrder = null;
                return; 
            }

            bool outOfOrder = false;
            for (int i = 0; i < _children.Count; i++)
            // is the order by child index no longer 0 - N in order?
                if (((GroupAttribute)_children[i]).GroupID != i)
                {
                    outOfOrder = true;
                    break;
                }

            if (outOfOrder)
            {
                // TODO: are all children guaranteed to be GroupAttributes?! 
                //       i dont think they are!  the following wont work otherwise
                mGroupRenderingOrder = new int[_children.Count];
                for (int i = 0; i < _children.Count; i++)
                    mGroupRenderingOrder[i] = ((GroupAttribute)_children[i]).GroupID;
            }

            mEnableGroupRenderingOrder = outOfOrder;
            SetChangeFlags(Keystone.Enums.ChangeStates.AppearanceParameterChanged, Keystone.Enums.ChangeSource.Self);

        }

        /// <summary>
        /// Applies visual changes to the duplicate ParticleSystemDuplicate.mDuplicateSystem
        /// </summary>
        /// <param name="ps"></param>
        /// <param name="elapsedSeconds"></param>
        /// <returns></returns>
        internal override int Apply(ParticleSystemDuplicate ps, EmitterProperties[] emitters, double elapsedSeconds)
        {

            if (ps.LastAppearance == GetHashCode()) return mHashCode;

            if (Groups == null) return mHashCode;

            // TODO: what about lOds?  
            //       I think the best solution is we must have LOD that matches
            //       the Appearance... afterall, some lod's wont have the same groups
            //       as the top most lod
            // apply color and textures on per group basis
            for (int iGroup = 0; iGroup < Groups.Length; iGroup++)
            {
                // TODO: emitters shouldn't actually use a Material since we just need a Diffuse, but... it works.
                //       Do minimesh particles use full material?
                if (Groups[iGroup].Material != null && Groups[iGroup].Material.PageStatus == PageableNodeStatus.Loaded)
                    ps.SetColor(iGroup, Groups[iGroup].Material.Diffuse);

                // TODO: i need to add support for SetBlendingModeEx() 
                ps.SetBlendingMode(emitters[iGroup].Change, emitters[iGroup].BlendingMode, iGroup);
                if (emitters[iGroup].Minimesh != null)
                    // NOTE: setting the blendingmode directly on the minimesh seems required. 
                    // Fortunately, it does use the RGBA values set in the particle keyframes
                    emitters[iGroup].Minimesh.SetBlendingMode(emitters[iGroup].BlendingMode);


                if (Groups[iGroup].Layers != null)
                {
                    if (Groups[iGroup].Shader != null)
                        emitters[iGroup].ShaderPath = Groups[iGroup].Shader.ResourcePath;
                    else
                        emitters[iGroup].ShaderPath = null;

                    foreach (Layer layer in Groups[iGroup].Layers)
                    {
                        if (layer.Texture == null) continue;

                        // IMPORTANT: for this to work properly, Emitter group indices must align with the Appearance.GroupAttribute indices
                        if (layer.Texture.PageStatus == PageableNodeStatus.Loaded)
                        {
                            if (layer is Keystone.Appearance.CubeMap)
                            {
                                emitters[iGroup].CubeTextureMaskPath = layer.Texture.ResourcePath;
                                ps.SetCubeTextureMask(iGroup, layer.Texture.TVIndex);
                            }
                            else
                                emitters[iGroup].TexturePath = layer.Texture.ResourcePath; //<-- this is not an ideal way to assign the texturepath
                            // NOTE: for Minimesh, the texture needs to be applied directly to the Minimesh, not the particleSystem.SetPointSprite() or particleSystem.SetBillboard()
                            // But that shouldn't affect how we compute the hashCode for determining changes to the GroupAttribute
                            if (emitters[iGroup].type == MTV3D65.CONST_TV_EMITTERTYPE.TV_EMITTER_MINIMESH)
                            {
                                ps.SetMinimeshTexture(emitters[iGroup].Minimesh, layer.TextureIndex);

                            }
                            else if (layer is Diffuse)
                                ps.SetTexture(iGroup, emitters[iGroup].type, layer.TextureIndex);
                        }
                        else if (layer.Texture.PageStatus == PageableNodeStatus.Loading)
                            continue;
                        else
                            emitters[iGroup].TexturePath = null;
                    }
                }
            }

            return mHashCode;
            
        }

        public override int Apply(Actor3dDuplicate actor,  double elapsedSeconds)
        {
            // if the shader's current parameters do not match the ones used here
            // we must update them.. problem is ChangeStates.ShaderParameterValuesChanged
            // is not going to cover that case
            // NOTE: We must always re-apply shader parameters because if more than one instance hosts 
            // the same shader, the assignment of parameters of one, will affect all the other instances.
            // So we must apply the unique parameters to all.
            //if ((mChangeStates & Keystone.Enums.ChangeStates.ShaderParameterValuesChanged) != 0)
                ApplyShaderParameterValues();

            // NOTE: call to GetHashCode() tests our _changeStates flag
            if (actor.LastAppearance == GetHashCode()) return mHashCode;

            //NOTE: I once used a common base class "TexturedElement3dGroup" for mesh
            // and actor which implemented SetMaterial and LightingMode and SetTextureEx, etc
            // but this was an error.  These particulars should be done seperately in Mesh and Actor
            // and TexturedElement3dGroup should be deleted.  

            //TODO: i think in retrospect that since .LightingMode and .Shader are internal
            // maybe ok to add argument for entityID.  We can change the interface later without
            // breaking binary compatibility.  Ulimately for the future it's more threadsafe
            // to have the extra argument.

            actor.LightingMode = _lightingMode;
            // TODO: base.Apply() should be used just like for mesh and that should set blendingmode
            //       for default model.  why not here?
            actor.BlendingMode = mBlendingMode;

            //mGroupRenderingOrder = new int[] { 3, 2, 1, 0 };
            //mGroupRenderingOrder = new int[] { 2, 1, 0 };
            actor.SetGroupRenderOrder(mEnableGroupRenderingOrder, mGroupRenderingOrder);


            // note: we don't have to track and swap shaders.  Only FX needs to worry about
            // restoring any previous shader
            actor.Shader = mShader;

            // default material and default layers when groups are not used.
            if (mMaterial != null)
            {
                // Updating the material just prior to it's use is only safe place
                // to do it.  Otherwise we get exceptions in TVMesh.Render() if we
                // are changing TVMaterialFactory material while rendering a mesh.
                // This is unique for Material PageableResource.
                if (mMaterial.PageStatus == PageableNodeStatus.NotLoaded)
                {
                    PagerBase.LoadTVResource(mMaterial);
                }
                mMaterial.Apply();
                actor.SetMaterial(mMaterial.TVIndex, -1);
            }

            if (mLayers != null)
                for (int i = 0; i < mLayers.Length; i++)
            	{
            		if (((IPageableTVNode)mLayers[i]).PageStatus == PageableNodeStatus.Loaded)
            		{
	                    actor.SetTextureEx(i, mLayers[i].TextureIndex, -1);
	            		
	                    // alpha test
	                    if (mLayers[i] is Diffuse)
	                    {
	                    	Diffuse diffuseLayer = (Diffuse)mLayers[i];
							actor.AlphaTestDepthWriteEnable = diffuseLayer.AlphaTestDepthWriteEnable;
							actor.AlphaTestRefValue = diffuseLayer.AlphaTestRefValue ;                        	
							actor.SetAlphaTest ( diffuseLayer.AlphaTest, this.GroupID);
	                    }
            		}
            		else if (mLayers[i].Texture.PageStatus == PageableNodeStatus.Loading)
            			continue;
            		else
            			actor.SetTextureEx(i, -1, -1);
            	}

            if (Groups == null) return mHashCode;

            // TODO: what about lOds?  
            //       I think the best solution is we must have LOD that matches
            //       the Appearance... afterall, some lod's wont have the same groups
            //       as the top most lod
            // apply materials and textures on per group basis
            for (int iGroup = 0; iGroup < Groups.Length; iGroup++)
            {
                if (Groups[iGroup].Material != null && Groups[iGroup].Material.PageStatus == PageableNodeStatus.Loaded)
                    actor.SetMaterial(Groups[iGroup].Material.TVIndex, iGroup);

                if (Groups[iGroup].Layers != null)
                {
                    foreach (Layer layer in Groups[iGroup].Layers)
                    {
                        if (layer.Texture == null) continue;

                        if (layer.Texture.PageStatus == PageableNodeStatus.Loaded)
                            actor.SetTextureEx(layer.LayerID, layer.TextureIndex, iGroup);
                        // NOTE: tvactor does not have TextureMod functions (i.e texture tiling, translation offset or rotation)
                        else if (layer.Texture.PageStatus == PageableNodeStatus.Loading)
            				continue;
                        //else // see notes regarding Mesh3d and SetTextureEx (-1) while PageStatus == NotLoaded.
                        //    actor.SetTextureEx(layer.LayerID, -1, iGroup);// if the Layer is there but underly Texture has been removed or is in process of replace
                    }
                }
            }

            return mHashCode;
        }

        //public override int Apply(Actor3d actor, double elapsedSeconds)
        //{
        //    // no need to aply if the target geometry already has these settings set
        //    if (actor.LastAppearance == GetHashCode()) return _hashCode;

        //    //NOTE: I once used a common base class "TexturedElement3dGroup" for mesh
        //    // and actor which implemented SetMaterial and LightingMode and SetTextureEx, etc
        //    // but this was an error.  These particulars should be done seperately in Mesh and Actor
        //    // and TexturedElement3dGroup should be deleted.  

        //    //TODO: i think in retrospect that since .LightingMode and .Shader are internal
        //    // maybe ok to add argument for entityID.  We can change the interface later without
        //    // breaking binary compatibility.  Ulimately for the future it's more threadsafe
        //    // to have the extra argument.
            
        //    actor.LightingMode = _lightingMode;
        //    actor.BlendingMode = _blendingMode;
        //    actor.AlphaTestRefValue = _alphaTestRefValue;
        //    actor.AlphaTestDepthWriteEnable = _alphaTestDepthBufferWriteEnable;
        //    actor.AlphaTest = _alphaTest;

        //    // only set the appearance shader if the geometry's current is different
        //    // note: we don't have to track and swap shaders.  Only FX needs to worry about
        //    // restoring any previous shader
        //    if (actor.Shader != _shader)
        //    {
        //        actor.Shader = _shader;
        //    }

        //    if (_material != null) actor.SetMaterial(_material.TVIndex, -1);
        //    if (Layers != null)
        //        for (int i = 0; i < Layers.Length; i++)
        //            actor.SetTextureEx(i, _layers[i].TextureIndex, -1);
            
        //    if (Groups == null) return _hashCode;

        //    // TODO: what about lOds?  
        //    //       I think the best solution is we must have LOD that matches
        //    //       the Appearance... afterall, some lod's wont have the same groups
        //    //       as the top most lod
        //    // apply materials and textures on per group basis
        //    for (int iGroup = 0; iGroup < Groups.Length; iGroup++)
        //    {
        //        if (Groups[iGroup].Material != null)
        //            actor.SetMaterial(Groups[iGroup].Material.TVIndex, iGroup);

        //        if (Groups[iGroup].Layers != null)
        //        {
        //            foreach (Layer layer in Groups[iGroup].Layers)
        //            {
        //                if (layer.TVResourceIsLoaded)
        //                    actor.SetTextureEx(layer.LayerID, layer.TextureIndex, iGroup);
        //                // NOTE: tvactor does not have TextureMod functions (i.e texture tiling, translation offset or rotation)
        //                else
        //                    actor.SetTextureEx(layer.LayerID, -1, iGroup);// if the Layer is there but underly Texture has been removed or is in process of replace
        //            }
        //        }
        //    }

        //    DisableChangeFlags(Keystone.Enums.ChangeStates.AppearanceChanged);
        //    return _hashCode;
        //}

        public override int Apply (InstancedGeometry geometry, double elapsedSeconds)
        {
        	int lastAppearance = geometry.LastAppearance; // cache value before base.Apply() can change it
            if (lastAppearance == base.Apply(geometry, elapsedSeconds)) return mHashCode;
            
            return mHashCode;
        }
                
        /// <summary>
        /// Does texture animation updates, shader parameter updates and appearance changes
        /// lazily only on visible geometry.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="appearanceFlags"></param>
        /// <param name="hashcode"></param>
        /// <remarks>
        /// This handles Billboard meshes as well.
        /// </remarks>
        /// <returns></returns>
        public override int Apply(Mesh3d mesh, double elapsedSeconds)
        {
            // TODO: note, appearance doesnt swap shaders, only other non appearance related shaders have to do this... so
            // keep this note here til i make sure i do that right when i enable shadowmapping and such. 
            #region Dynamic Appearance updating // update any layers and parameters.  This must be done for all Groups
            Update(elapsedSeconds);

            // TODO: The following is expensive for meshes with lots of groups... 
            // really texture animation and shader parameters by group should NOT be done.
            // we should focus on keeping groups to 0 or VERY LOW and not doing shaders or animations
            // by group at all.
            // now we do what we just did for the default group, for all sub-groups
            bool updateGroups = false; // temp to test performance without these group looping
            if (updateGroups)
                if (Groups != null)
                {
                    // apply materials and textures for any individual groups
                    for (int i = 0; i < Groups.Length; i++)
                    {
                        // update any texture animations and shader parameters by group
                        Groups[i].Update(elapsedSeconds);
                    }
                }
            #endregion

            //if (this.mShaderResourceDescriptor.Contains("atlas.fx"))
            //{
            //    System.Diagnostics.Debug.WriteLine("atlas texture");
            //}

            // shader parameters do not constitute normal appearance change.  In other words, 
            // it does not result in a change of the lastAppearance hashcode.  Shader parameters
            // are often needed to be changed prior to rendering each specific Geometry instance.
            // if the shader's current parameters do not match the ones used here
            // we must update them.. problem is ChangeStates.ShaderParameterValuesChanged
            // is not going to cover that case
            // NOTE: We must always re-apply shader parameters because if more than one instance hosts 
            // the same shader, the assignment of parameters of one, will affect all the other instances.
            // So we must apply the unique parameters to all.
            //if ((mChangeStates & Keystone.Enums.ChangeStates.ShaderParameterValuesChanged) != 0)
            ApplyShaderParameterValues();

            // in our Sol System, some gas giants seem to be trying to apply before shader is loaded
            // NOTE: base.Apply() calls GetHashCode() and tests our _changeStates flag
            int lastAppearance = mesh.LastAppearance; // cache value before base.Apply() can change it
            if (lastAppearance == base.Apply(mesh, elapsedSeconds)) return mHashCode;


            // TODO: if it's a TextureCycle how do we automate the texture updates for that?
            // I mean without a script and without a call to the Layer to manually update it...
            // A TextureCycle should update just prior to Render because it is shared...
            // or is it?  If it's NOT shared, we can cache the values in it, clone it only allowed
            // and then register it as an IUpdateable node that if visible gets updated 
            // This could be same mechanism we use to update shader parameters
            // Perhaps here in Apply instead... we don't just compare hashcode... we first
            // have to see if a particular Layer requires parameter changes or updating texture keyframe


            // TODO: should lightingmode be removed from Appearance and enforced
            // only one geometry tvloadresource? and changing lightingmode must reload the geometry?
            mesh.LightingMode = _lightingMode;


            // TODO: Ideally it'd be nice if the group render order could be done by
            //       raising/lowering the child order.  
            //       Similarly, blending modes directly in the tree would be nice too.
            //       This would make it very easy to edit and see.
            //mGroupRenderingOrder = new int[] { 3, 2, 1, 0 };
            //mGroupRenderingOrder = new int[] { 2, 1, 0 };
            mesh.SetGroupRenderOrder(mEnableGroupRenderingOrder, mGroupRenderingOrder);


            if (Groups != null)
            {
                // apply materials and textures for any individual groups
                for (int i = 0; i < Groups.Length; i++)
                {
                    // NOTE: In our ImportLib and WavefrontObjLoader, if there is only 1 group in the model
                    // we add materials, shaders, and textures directly to the defaultappearance and add
                    // NO GroupAttribute children.
                    Groups[i].Apply(mesh, elapsedSeconds);
                }
            }

            return mHashCode;
        }

        // TODO: this is basically obsolete.  The Mesh3d if using the Minimesh rendering option
        // should itself in it's own SetTexture and SetMaterial calls, and Shader setters
        // should apply to minimesh as well.
        public override int Apply(Minimesh2 mini, double elapsedSeconds)
        {
            // NOTE: call to GetHashCode() tests our _changeStates flag
            if (mini.LastAppearance == GetHashCode()) return mHashCode;

            mini.BlendingMode = mBlendingMode;

            // NOTE: the _material referenced here is shared with the underlying Mesh3d used
            //       by this mini.  
            // TODO: however we should update our planet shader to work with minimesh
            if (mMaterial != null)
            {
                // Updating the material just prior to it's use is only safe place
                // to do it.  Otherwise we get exceptions in TVMesh.Render() if we
                // are changing TVMaterialFactory material while rendering a mesh.
                // This is unique for Material PageableResource.
                if (mMaterial.PageStatus == PageableNodeStatus.NotLoaded)
                {
                    PagerBase.LoadTVResource(mMaterial);
                }
                mMaterial.Apply();
                mini.SetMaterial(mMaterial.TVIndex);
            }
            mini.Shader = mShader;

            // assign textures
            if ((Groups == null || Groups.Length == 0) && (Layers != null && Layers.Length >= 1))
            {
                for (int i = 0; i < Layers.Length; i++)
                	if (Layers[i].Texture.PageStatus == PageableNodeStatus.Loaded)
                    	mini.SetTexture(Layers[i].LayerID, Layers[i].TextureIndex);
                //  else // see notes regarding Mesh3d and SetTextureEx (-1) while PageStatus == NotLoaded.
               	//		mini.SetTexture (Layers[i].LayerID, -1);
            }
            else if (Groups != null && Groups.Length > 0) // we'll ignore all but first group  since a TVMinimesh wont actually have more than one group even if we have accidentally added more than one GroupAttribute node somehow
            {
                if (Groups[0].Layers != null && Groups[0].Layers.Length > 0)
                {
                    if (Groups[0].mDeletedLayers != null)
                    {
                        for (int i = 0; i < Groups[0].mDeletedLayers.Count; i++)
                            mini.SetTexture(Groups[0].mDeletedLayers[i], -1);

                        Groups[0].mDeletedLayers.Clear();
                        Groups[0].mDeletedLayers = null;
                    }

                    for (int i = 0; i < Groups[0].Layers.Length; i++)
                    	if (Groups[0].Layers[i].Texture.PageStatus == PageableNodeStatus.Loaded)
                            mini.SetTexture(Groups[0].Layers[i].LayerID, Groups[0].Layers[i].TextureIndex);
                        //else // see notes regarding Mesh3d and SetTextureEx (-1) while PageStatus == NotLoaded.
                        //    mini.SetTexture(Groups[0].Layers[i].LayerID, -1); // if the Layer is there but underly Texture has been removed or is in process of replace
                }
                // NOTE: Minimesh cannot use TextureMod's (tiling, translation offset or rotation)
            }

            return mHashCode;
        }

        public override int Apply(TexturedQuad2D quad, double elapsedSeconds)
        {
            //if (this.Material
            // quad.BlendingMode = MTV3D65.CONST_TV_BLENDINGMODE.TV_BLEND_ALPHA ;
            // NOTE: call to GetHashCode() tests our _changeStates flag
            if (quad.LastAppearance == GetHashCode()) return mHashCode;
            
            quad.Shader = mShader;
            
            // a TexturedQuad2D has no Groups.  It just has one default Layer
            if (mLayers != null)
                if (mLayers.Length >= 0)
            		if (mLayers[0].Texture.PageStatus == PageableNodeStatus.Loaded)
                    	quad.SetTexture (mLayers[0].TextureIndex);
            	//	else // see notes regarding Mesh3d and SetTextureEx (-1) while PageStatus == NotLoaded.
            	//		quad.SetTexture (-1);
            
            //    // TODO: "Apply()" call should also set the quad's color properties?
            //    // TODO: these should result in hashcode changes too so need for re-apply can be tested
            //    mColor1 = mColor2 = mColor3 = mColor4 = model.Appearance.Material.Diffuse.ToInt32();

            return mHashCode;
        }


        public override int Apply(Terrain land, double elapsedSeconds)
        {
            // no need to aply if the target terrain already has these settings set
			// NOTE: call to GetHashCode() tests our _changeStates flag
            if (land.LastAppearance == GetHashCode()) return mHashCode;

            
            land.LightingMode = _lightingMode;
            // land.BlendingMode = _blendingMode; // BlendingMode not used by Landscape! It's never transparent and has no method for changing it's Blend state

            land.SetSplattingEnable = false;
            // in our Chunk.cs get rid of direct access to TVLand and everything goes thru the chunk which will abstract the handling of the chunk index stuff
            land.ClampingEnable = true;

            int iGroup = -1; // chunks in Terrain must match the number of Groups. 
            // and the index position in the Group[] must match the chunkID's so its up to user to add them in proper order
            // for cases where we're using the same exact settings for each group, use -1
            // TODO; the alternate approach if they are all the same for multiple groups is to use
            // expandTexture... but this ruins the GroupAttribute 1:1 relationship with chunks... hrm. Which way to go?
            // does expand texture stretch or tile?  because that would give us no choice.
            //
            if (mMaterial != null)
            {
                // Updating the material just prior to it's use is only safe place
                // to do it.  Otherwise we get exceptions in TVMesh.Render() if we
                // are changing TVMaterialFactory material while rendering a mesh.
                // This is unique for Material PageableResource.
                if (mMaterial.PageStatus == PageableNodeStatus.NotLoaded)
                {
                    PagerBase.LoadTVResource(mMaterial);
                }
                mMaterial.Apply();
                land.SetMaterial(mMaterial.TVIndex, -1);
            }
            land.Shader = mShader;

            if (mLayers != null)
                for (int i = 0; i < Layers.Length; i++)
            		if (mLayers[0].Texture.PageStatus == PageableNodeStatus.Loaded)
	                    land.SetTexture( mLayers[i].LayerID, mLayers[i].TextureIndex);
					//else  // see notes regarding Mesh3d and SetTextureEx (-1) while PageStatus == NotLoaded.
            		//	land.SetTexture (_layers[i].LayerID, -1);

            if (Groups == null) return mHashCode;
            // apply appearance settings on per group basis
            foreach (GroupAttribute  g in Groups)
            {
                land.LightingMode = _lightingMode;
                if (g.Material != null && g.Material.PageStatus == PageableNodeStatus.Loaded) 
                	land.SetMaterial(g.Material.TVIndex, iGroup);
                
                if (g.Layers != null)
                {
                    foreach (Layer layer in g.Layers)
                    {
                    	if (layer is Diffuse && layer.Texture.PageStatus == PageableNodeStatus.Loaded)
                    	{
	                        if (layer is Diffuse)
	                        {
	                            land.SetTextureScale(layer.TileU, layer.TileV, iGroup);
	                            land.SetTexture(layer.TextureIndex, iGroup);
	                            land.ExpandTexture(layer.TextureIndex, 0, 0, land.ChunksZ, land.ChunksX);
	                        }
	                        else if (layer is NormalMap)
	                        {
	                            land.SetDetailTexture(layer.TextureIndex, iGroup); // note: land doesnt have SetTextureEx()
	                            land.SetDetailTextureScale(15, 15, iGroup);
	                            // why dont detail texture and lightmap need expanding?
	                        }
	                            // since shadowmapping uses this texture layer, this should ONLY get set here via Appearance if shadowmapping is disabled.
	                        else if (layer is Specular)
	                            // this kinda sucks.  I mean they are loaded "normally"  but we simply want them treated differently.  
	                            // I suppose what I should do is just set the types available to
	                            // so like layer.Type and we can enforce the types that are settable :/
	                        {
	                            // TODO: make this accessible so we can verify
	                            //Trace.Assert (Scene.FXProviders[(int)FX.FX_SEMANTICS.FX_DIRECTIONAL_LAND_SHADOW] == null);
	                            land.SetLightMapTexture(layer.TextureIndex, iGroup);
	                        }
	                        //else if (layer is SplatAlpha )
	                        //{
	                        //    land.SetSplattingTexture() //<-- goes in our SplatAppearance
	                        //}
                    	}
                    }
                }
                iGroup++;
            }

            return mHashCode;
        }
    }
}