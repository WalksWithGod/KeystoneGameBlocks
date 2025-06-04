//using System.Xml;
//using Keystone.Elements;
//using Keystone.Entities;
//using Keystone.Resource;
//using Keystone.Shaders;

//namespace Keystone.Appearance
//{
//    public class DefaultAppearance : Appearance
//    {

//        /// <summary>
//        /// Note: No static Create methods since Appearance nodes cannot be shared.
//        /// </summary>
//        /// <param name="id"></param>
//        public DefaultAppearance(string id)
//            : base(id)
//        {
//        }


//        public override int Apply(Actor3d actor, int appearanceFlags)
//        {
//            // no need to aply if the target geometry already has these settings set
//            if (actor.LastAppearance == GetHashCode()) return _hashCode;

//            //NOTE: I once used a common base class "TexturedElement3dGroup" for mesh
//            // and actor which implemented SetMaterial and LightingMode and SetTextureEx, etc
//            // but this was an error.  These particulars should be done seperately in Mesh and Actor
//            // and TexturedElement3dGroup should be deleted.  

//            //todo: i think in retrospect that since .LightingMode and .Shader are internal
//            // maybe ok to add argument for entityID.  We can change the interface later without
//            // breaking binary compatibility.  Ulimately for the future it's more threadsafe
//            // to have the extra argument.
//            actor.LightingMode = _lightingMode;

//            // only set the appearance shader if the geometry's current is different
//            // note: we don't have to track and swap shaders.  Only FX needs to worry about
//            // restoring any previous shader
//            if (actor.Shader != _shader)
//            {
//                actor.Shader = _shader;
//            }

//            if (_material != null) actor.SetMaterial(_material.TVIndex, -1);
//            if (Layers != null)
//                for (int i = 0; i < Layers.Length; i++)
//                    actor.SetTextureEx(i, _layers[i].TextureIndex, -1);
            
//            if (Groups == null) return _hashCode;

//            // apply materials and textures on per group basis
//            for (int iGroup = 0; iGroup < Groups.Length; iGroup++)
//            {
//                if (Groups[iGroup].Material != null)
//                    actor.SetMaterial(Groups[iGroup].Material.TVIndex, iGroup);

//                if (Groups[iGroup].Layers != null)
//                {
//                    foreach (Layer layer in Groups[iGroup].Layers)
//                    {
//                        if (layer.TVResourceIsLoaded)
//                            actor.SetTextureEx(layer.LayerID, layer.TextureIndex, iGroup);
//                        // NOTE: tvactor does not have TextureMod functions (i.e texture tiling, translation offset or rotation)
//                        else
//                            actor.SetTextureEx(layer.LayerID, -1, iGroup);// if the Layer is there but underly Texture has been removed or is in process of replace
//                    }
//                }
//            }

//            DisableChangeFlags(Keystone.Enums.ChangeStates.AppearanceChanged);
//            return _hashCode;
//        }

//        /// <summary>
//        /// This handles Billboard meshes as well.
//        /// </summary>
//        /// <param name="mesh"></param>
//        /// <param name="appearanceFlags"></param>
//        /// <param name="hashcode"></param>
//        /// <returns></returns>
//        public override int Apply(Mesh3d mesh, int appearanceFlags)
//        {
//            // todo: note, appearance doesnt swap shaders, only other non appearance related shaders have to do this... so
//            // keep this note here til i make sure i do that right when i enable shadowmapping and such. 
//            // todo: am I properly handling hashcode now that ive added shader to appearance?  maybe hashcode is irrelevant now
//            // that i can just use changestates?

//            // no need to aply if the target geometry already has these settings set
           
//            if (mesh.LastAppearance == GetHashCode()) return _hashCode;

//            mesh.LightingMode = _lightingMode;
//            //mesh.LightingMode = MTV3D65.CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED; // MTV3D65.CONST_TV_LIGHTINGMODE.TV_LIGHTING_BUMPMAPPING_TANGENTSPACE;
//            // only set the appearance shader if the geometry's current is different
//            // note: we don't have to track and swap shaders.  Only FX needs to worry about
//            // restoring any previous shader
//            if (mesh.Shader != _shader)
//            {
//                mesh.Shader = _shader;
//            }
            
//            if (_material != null) mesh.SetMaterial(_material.TVIndex, -1);
            
//            if (Layers != null)
//                for (int i = 0; i < Layers.Length; i++)
//                    mesh.SetTextureEx(i, _layers[i].TextureIndex, -1);

//            if (mDeletedLayers != null && mDeletedLayers.Count > 0)
//            {
//                for (int i = 0; i < mDeletedLayers.Count; i++)
//                    mesh.SetTexture(mDeletedLayers[i], -1); // global, not per group

//                mDeletedLayers.Clear();
//                mDeletedLayers = null;
//            }

//            if (Groups == null) return _hashCode;

//            // apply materials and textures on per group basis
//            for (int iGroup = 0; iGroup < Groups.Length; iGroup++)
//            {
//                if (Groups[iGroup].Material != null)
//                    mesh.SetMaterial(Groups[iGroup].Material.TVIndex, iGroup);

//                if (Groups[iGroup].mDeletedLayers != null)
//                {
//                    for (int i = 0; i < Groups[iGroup].mDeletedLayers.Count; i++)
//                        mesh.SetTextureEx(Groups[iGroup].mDeletedLayers[i], -1, iGroup);

//                    Groups[iGroup].mDeletedLayers.Clear();
//                    Groups[iGroup].mDeletedLayers = null;
//                }

//                if (Groups[iGroup].Layers != null)
//                {
//                    foreach (Layer layer in Groups[iGroup].Layers)
//                    {
//                        if (layer.TVResourceIsLoaded)
//                        {
//                            mesh.SetTextureEx(layer.LayerID, layer.TextureIndex, iGroup);
//                            if (layer.TextureModEnabled)
//                            {
//                                mesh.SetTextureMod(layer.LayerID, iGroup, layer.TranslationU, layer.TranslationV, layer.TileU, layer.TileV);
//                                mesh.SetTextureRotation(layer.LayerID, iGroup, layer.Rotation);
//                            }
//                        }
//                        else
//                            mesh.SetTextureEx(layer.LayerID, -1, iGroup);
//                    }
//                }
//            }

//            DisableChangeFlags(Keystone.Enums.ChangeStates.AppearanceChanged);
//            return _hashCode;
//        }

//        // todo: this is basically obsolete.  The Mesh3d if using the Minimesh rendering option
//        // should itself in it's own SetTexture and SetMaterial calls, and Shader setters
//        // should apply to minimesh as well.
//        public override int Apply(Minimesh2 mini, int appearanceFlags)
//        {
//            // no need to aply if the target geometry already has these settings set
//            if (mini.LastAppearance == GetHashCode()) return _hashCode;


//          //  _material = Material.Create(Material.DefaultMaterials.matte);
//      //      if (_material != null) mini.SetMaterial(_material.TVIndex);
//            // todo: only commented out because in transition from Minimesh no longer being Geometry node
//            if (mini.GetShader() != _shader.TVShader)
//                mini.SetShader (_shader.TVShader);


//            if (Groups != null && Groups.Length > 0)
//            {
//                if (Groups[0].Layers != null && Groups[0].Layers.Length > 0)
//                {
//                    if (Groups[0].mDeletedLayers != null)
//                    {
//                        for (int i = 0; i < Groups[0].mDeletedLayers.Count; i++)
//                            mini.SetTexture(Groups[0].mDeletedLayers[i], -1);

//                        Groups[0].mDeletedLayers.Clear();
//                        Groups[0].mDeletedLayers = null;
//                    }

//                    for (int i = 0; i < Groups[0].Layers.Length; i++)
//                        if (Groups[0].Layers[i].TVResourceIsLoaded)
//                            mini.SetTexture(Groups[0].Layers[i].LayerID, Groups[0].Layers[i].TextureIndex);
//                        else
//                            mini.SetTexture(Groups[0].Layers[i].LayerID, -1); // if the Layer is there but underly Texture has been removed or is in process of replace
//                }
//                // NOTE: Minimesh cannot use TextureMod's (tiling, translation offset or rotation)
//            }

//            DisableChangeFlags(Keystone.Enums.ChangeStates.AppearanceChanged);
//            return _hashCode;
//        }

//        public override int Apply(Terrain land, int appearanceFlags)
//        {
//            // no need to aply if the target geometry already has these settings set
//            if (land.LastAppearance  == GetHashCode()) return _hashCode;

//            land.SetSplattingEnable = false;
//            // in our Chunk.cs get rid of direct access to TVLand and everything goes thru the chunk which will abstract the handling of the chunk index stuff
//            land.SetClampingEnable = true;

//            int iGroup = -1; // chunks in Terrain must match the number of Groups. 
//            // and the index position in the Group[] must match the chunkID's so its up to user to add them in proper order
//            // for cases where we're using the same exact settings for each group, use -1
//            // TODO; the alternate approach if they are all the same for multiple groups is to use
//            // expandTexture... but this ruins the GroupAttribute 1:1 relationship with chunks... hrm. Which way to go?
//            // does expand texture stretch or tile?  because that would give us no choice.
//            //
//            if (_material != null) land.SetMaterial(_material.TVIndex, -1);
//            if (land.Shader != _shader)
//                land.Shader = _shader;

//            if (_layers != null)
//                for (int i = 0; i < Layers.Length; i++)
//                    land.SetTexture( _layers[i].TextureIndex, -1);


//            if (Groups == null) return _hashCode;
//            // apply appearance settings on per group basis
//            foreach (GroupAttribute  g in Groups)
//            {
//                land.LightingMode = _lightingMode;
//                if (g.Material != null) land.SetMaterial(g.Material.TVIndex, iGroup);
//                if (g.Layers != null)
//                {
//                    foreach (Layer layer in g.Layers)
//                    {
//                        if (layer is Diffuse)
//                        {
//                            land.SetTextureScale(layer.TileU, layer.TileV, iGroup);
//                            land.SetTexture(layer.TextureIndex, iGroup);
//                            land.ExpandTexture(layer.TextureIndex, 0, 0, land.ChunkHeight, land.ChunkWidth);
//                        }
//                        else if (layer is NormalMap)
//                        {
//                            land.SetDetailTexture(layer.TextureIndex, iGroup); // note: land doesnt have SetTextureEx()
//                            land.SetDetailTextureScale(15, 15, iGroup);
//                            // why dont detail texture and lightmap need expanding?
//                        }
//                            // since shadowmapping uses this texture layer, this should ONLY get set here via Appearance if shadowmapping is disabled.
//                        else if (layer is Specular)
//                            // this kinda sucks.  I mean they are loaded "normally"  but we simply want them treated differently.  
//                            // I suppose what I should do is just set the types available to
//                            // so like layer.Type and we can enforce the types that are settable :/
//                        {
//                            // todo: make this accessible so we can verify
//                            //Trace.Assert (Scene.FXProviders[(int)FX.FX_SEMANTICS.FX_DIRECTIONAL_LAND_SHADOW] == null);
//                            land.SetLightMapTexture(layer.TextureIndex, iGroup);
//                        }
//                        //else if (layer is SplatAlpha )
//                        //{
//                        //    land.SetSplattingTexture() //<-- goes in our SplatAppearance
//                        //}
//                    }
//                }
//                iGroup++;
//            }

//            DisableChangeFlags(Keystone.Enums.ChangeStates.AppearanceChanged);
//            return _hashCode;
//        }
//    }
//}