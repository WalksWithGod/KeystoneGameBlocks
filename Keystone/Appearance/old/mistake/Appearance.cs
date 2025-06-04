using System;
using Keystone.Algorithms;
using Keystone.Elements;
using Keystone.Entities;
using Keystone.IO;
using Keystone.Shaders;
using Keystone.Traversers;
using MTV3D65;

namespace Keystone.Appearance
{
    // note: we wont delete the old appearance until we've got the new one
    // tested and working.  This is because it'll result in too many compile errors
    // to fix in the short term while trying to test the new implementation
    public class DefaultAppearance : Appearance
    {
        public DefaultAppearance(string id)
            : base(id)
        {
        }

        public void AddChild(GroupAttribute child)
        {
            base.AddChild(child);
            SetChangeFlags(Enums.ChangeStates.AppearanceChanged, Enums.ChangeSource.Self);
        }

        public GroupAttribute[] Groups
        {
            get
            {
                if (_children == null) return null;
                GroupAttribute[] tmp = new GroupAttribute[_children.Count];
                int j = 0;
                for (int i = 0; i < _children.Count; i++)
                    if (_children[i] is GroupAttribute)
                    {
                        tmp[j] = (GroupAttribute)_children[i];
                        j++;
                    }

                // need to resize to just the length of the groups without materials
                GroupAttribute[] final = new GroupAttribute[j];
                Array.Copy(tmp, final, j);
                return final;
            }
        }

        #region IGroup members
        protected override void PropogateChangeFlags(Enums.ChangeStates flags, Enums.ChangeSource source)
        {
            switch (flags)
            {
                case Enums.ChangeStates.AppearanceChanged:
                    // if it's a child or self we notify parent models
                    if (source == Enums.ChangeSource.Self || source == Enums.ChangeSource.Child)
                        NotifyParents(flags);
                    // if a parent for some reason such as having added this appearance, then buck stops here.  Do nothing.

                    break;
                case Enums.ChangeStates.ScriptLoaded :
                    AssignParamaterValues();
                    break;
            }
            //note: no need to notify parents because our traverser will compare the HashCode of 
            // this appearance with the appearance currently set on the Geometry in question
            // to determine if it needs to be re-Apply(geometry).   Or is this the incorrect way?  Maybe we should
            // not use a HashCode and instead set the actual flag?  

            // NOTE: if the HashCode == 0 then the Apply will be skipped since clearly no children exist
            // (todo: well, lightingmode will?  could maybe special case lightingMode to always
            // apply and to have the Geometry check its previous lighting modes before changing..
        }

        protected override void NotifyParents(Enums.ChangeStates flags)
        {
            if (_parents == null) return;
            foreach (Entity parent in _parents)
                if (parent != null)
                    parent.SetChangeFlags(flags, Enums.ChangeSource.Child);
        }
        #endregion



        public override int Apply(Actor3d actor, int appearanceFlags)
        {
            //// no need to aply if the target geometry already has these settings set
            //if (actor.LastAppearance == GetHashCode()) return _hashCode;

            //// only set the appearance shader if the geometry's current is different
            //// note: we don't have to track and swap shaders.  Only FX needs to worry about
            //// restoring any previous shader
            //if (actor.Shader != _shader)
            //{
            //    actor.Shader = _shader;
            //}

            //if (_material != null) actor.SetMaterial(_material.TVIndex, -1);
            //if (Layers != null)
            //    for (int i = 0; i < Layers.Length; i++)
            //        actor.SetTextureEx(i, _layers[i].TextureIndex, -1);

            //if (Groups == null) return _hashCode;

            //// apply materials and textures on per group basis
            //for (int iGroup = 0; iGroup < Groups.Length; iGroup++)
            //{
            //    if (Groups[iGroup].Material != null)
            //        actor.SetMaterial(Groups[iGroup].Material.TVIndex, iGroup);

            //    if (Groups[iGroup].Layers != null)
            //    {
            //        foreach (Layer layer in Groups[iGroup].Layers)
            //        {
            //            if (layer.TVResourceIsLoaded)
            //                actor.SetTextureEx(layer.LayerID, layer.TextureIndex, iGroup);
            //            // NOTE: tvactor does not have TextureMod functions (i.e texture tiling, translation offset or rotation)
            //            else
            //                actor.SetTextureEx(layer.LayerID, -1, iGroup);// if the Layer is there but underly Texture has been removed or is in process of replace
            //        }
            //    }
            //}

            //DisableChangeFlags(Keystone.Enums.ChangeStates.AppearanceChanged);
            return _hashCode;
        }

        /// <summary>
        /// This handles Billboard meshes as well.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="appearanceFlags"></param>
        /// <param name="hashcode"></param>
        /// <returns></returns>
        public override int Apply(Mesh3d mesh, int appearanceFlags)
        {
            // todo: note, appearance doesnt swap shaders, only other non appearance related shaders have to do this... so
            // keep this note here til i make sure i do that right when i enable shadowmapping and such. 
            // todo: am I properly handling hashcode now that ive added shader to appearance?  maybe hashcode is irrelevant now
            // that i can just use changestates?

            // no need to aply if the target geometry already has these settings set

            if (mesh.LastAppearance == GetHashCode()) return _hashCode;

            // only set the appearance shader if the geometry's current is different
            // note: we don't have to track and swap shaders.  Only FX needs to worry about
            // restoring any previous shader
            if (mesh.Shader != _shader)
            {
                mesh.Shader = _shader;
            }

      ////      if (_material != null) mesh.SetMaterial(_material.TVIndex, -1);

      //      if (_textures != null)
      //          for (int i = 0; i < _textures.Length; i++)
      //              mesh.SetTextureEx(i, _textures[i].TextureIndex, -1);

      //      if (mDeletedLayers != null && mDeletedLayers.Count > 0)
      //      {
      //          for (int i = 0; i < mDeletedLayers.Count; i++)
      //              mesh.SetTexture(mDeletedLayers[i], -1); // global, not per group

      //          mDeletedLayers.Clear();
      //          mDeletedLayers = null;
      //      }

      //      if (Groups == null) return _hashCode;

      //      // apply materials and textures on per group basis
      //      for (int iGroup = 0; iGroup < Groups.Length; iGroup++)
      //      {
      //          //if (Groups[iGroup].Material != null)
      //          //    mesh.SetMaterial(Groups[iGroup].Material.TVIndex, iGroup);

      //          if (Groups[iGroup].mDeletedLayers != null)
      //          {
      //              for (int i = 0; i < Groups[iGroup].mDeletedLayers.Count; i++)
      //                  mesh.SetTextureEx(Groups[iGroup].mDeletedLayers[i], -1, iGroup);

      //              Groups[iGroup].mDeletedLayers.Clear();
      //              Groups[iGroup].mDeletedLayers = null;
      //          }

      //          if (Groups[iGroup]._textures != null)
      //          {
      //              foreach (Layer layer in Groups[iGroup]._textures)
      //              {
      //                  if (layer.TVResourceIsLoaded)
      //                  {
      //                      mesh.SetTextureEx(layer.LayerID, layer.TextureIndex, iGroup);
      //                      if (layer.TextureModEnabled)
      //                      {
      //                          mesh.SetTextureMod(layer.LayerID, iGroup, layer.TranslationU, layer.TranslationV, layer.TileU, layer.TileV);
      //                          mesh.SetTextureRotation(layer.LayerID, iGroup, layer.Rotation);
      //                      }
      //                  }
      //                  else
      //                      mesh.SetTextureEx(layer.LayerID, -1, iGroup);
      //              }
      //          }
      //      }

            DisableChangeFlags(Keystone.Enums.ChangeStates.AppearanceChanged);
            return _hashCode;
        }

        // todo: this is basically obsolete.  The Mesh3d if using the Minimesh rendering option
        // should itself in it's own SetTexture and SetMaterial calls, and Shader setters
        // should apply to minimesh as well.
        public override int Apply(Minimesh2 mini, int appearanceFlags)
        {
            //// no need to aply if the target geometry already has these settings set
            //if (mini.LastAppearance == GetHashCode()) return _hashCode;


            ////  _material = Material.Create(Material.DefaultMaterials.matte);
            ////      if (_material != null) mini.SetMaterial(_material.TVIndex);
            //// todo: only commented out because in transition from Minimesh no longer being Geometry node
            //if (mini.GetShader() != _shader.TVShader)
            //    mini.SetShader(_shader.TVShader);


            //if (Groups != null && Groups.Length > 0)
            //{
            //    if (Groups[0].Layers != null && Groups[0].Layers.Length > 0)
            //    {
            //        if (Groups[0].mDeletedLayers != null)
            //        {
            //            for (int i = 0; i < Groups[0].mDeletedLayers.Count; i++)
            //                mini.SetTexture(Groups[0].mDeletedLayers[i], -1);

            //            Groups[0].mDeletedLayers.Clear();
            //            Groups[0].mDeletedLayers = null;
            //        }

            //        for (int i = 0; i < Groups[0].Layers.Length; i++)
            //            if (Groups[0].Layers[i].TVResourceIsLoaded)
            //                mini.SetTexture(Groups[0].Layers[i].LayerID, Groups[0].Layers[i].TextureIndex);
            //            else
            //                mini.SetTexture(Groups[0].Layers[i].LayerID, -1); // if the Layer is there but underly Texture has been removed or is in process of replace
            //    }
            //    // NOTE: Minimesh cannot use TextureMod's (tiling, translation offset or rotation)
            //}

            //DisableChangeFlags(Keystone.Enums.ChangeStates.AppearanceChanged);
            return _hashCode;
        }

        public override int Apply(Terrain land, int appearanceFlags)
        {
            //// no need to aply if the target geometry already has these settings set
            //if (land.LastAppearance == GetHashCode()) return _hashCode;

            //land.SetSplattingEnable = false;
            //// in our Chunk.cs get rid of direct access to TVLand and everything goes thru the chunk which will abstract the handling of the chunk index stuff
            //land.SetClampingEnable = true;

            //int iGroup = -1; // chunks in Terrain must match the number of Groups. 
            //// and the index position in the Group[] must match the chunkID's so its up to user to add them in proper order
            //// for cases where we're using the same exact settings for each group, use -1
            //// TODO; the alternate approach if they are all the same for multiple groups is to use
            //// expandTexture... but this ruins the GroupAttribute 1:1 relationship with chunks... hrm. Which way to go?
            //// does expand texture stretch or tile?  because that would give us no choice.
            ////
            //if (_material != null) land.SetMaterial(_material.TVIndex, -1);
            //if (land.Shader != _shader)
            //    land.Shader = _shader;

            //if (_layers != null)
            //    for (int i = 0; i < Layers.Length; i++)
            //        land.SetTexture(_layers[i].TextureIndex, -1);


            //if (Groups == null) return _hashCode;
            //// apply appearance settings on per group basis
            //foreach (GroupAttribute g in Groups)
            //{
            //    land.LightingMode = _lightingMode;
            //    if (g.Material != null) land.SetMaterial(g.Material.TVIndex, iGroup);
            //    if (g.Layers != null)
            //    {
            //        foreach (Layer layer in g.Layers)
            //        {
            //            if (layer is Diffuse)
            //            {
            //                land.SetTextureScale(layer.TileU, layer.TileV, iGroup);
            //                land.SetTexture(layer.TextureIndex, iGroup);
            //                land.ExpandTexture(layer.TextureIndex, 0, 0, land.ChunkHeight, land.ChunkWidth);
            //            }
            //            else if (layer is NormalMap)
            //            {
            //                land.SetDetailTexture(layer.TextureIndex, iGroup); // note: land doesnt have SetTextureEx()
            //                land.SetDetailTextureScale(15, 15, iGroup);
            //                // why dont detail texture and lightmap need expanding?
            //            }
            //            // since shadowmapping uses this texture layer, this should ONLY get set here via Appearance if shadowmapping is disabled.
            //            else if (layer is Specular)
            //            // this kinda sucks.  I mean they are loaded "normally"  but we simply want them treated differently.  
            //            // I suppose what I should do is just set the types available to
            //            // so like layer.Type and we can enforce the types that are settable :/
            //            {
            //                // todo: make this accessible so we can verify
            //                //Trace.Assert (Scene.FXProviders[(int)FX.FX_SEMANTICS.FX_DIRECTIONAL_LAND_SHADOW] == null);
            //                land.SetLightMapTexture(layer.TextureIndex, iGroup);
            //            }
            //            //else if (layer is SplatAlpha )
            //            //{
            //            //    land.SetSplattingTexture() //<-- goes in our SplatAppearance
            //            //}
            //        }
            //    }
            //    iGroup++;
            //}

            //DisableChangeFlags(Keystone.Enums.ChangeStates.AppearanceChanged);
            return _hashCode;
        }
    }

    public abstract class Appearance : GroupAttribute // you can switch between multiple appearances if they're available
    {
        //protected CONST_TV_LIGHTINGMODE _lightingMode;

        // _material for an overall Appearance is always equals to the one and only
        // child of type Material IF ANY in the _children array. 
        // for meshes or landscapes with many groups/chunks, rather than set a material in each
        // groupAttribute object, we can use the default.  If a group/chunk's group attribute
        // already has a material, that will be used instead.
        // a default Material is not required.  If you dont want to have this type of
        // cascading behavior, do not set the defaultMaterial by not adding a Material to an Appearance node.
        protected Appearance(string id) : base(id)
        {
            //LightingMode = CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED;
        }

        public abstract int Apply(Terrain land, int appearanceFlags); // chunk?  I might make it so terrains are always handled per chunk
        public abstract int Apply(Mesh3d mesh, int appearanceFlags);
        public abstract int Apply(Actor3d actor, int appearanceFlags);
        public abstract int Apply(Minimesh2 mini, int appearanceFlags);

        #region ITraversable Members
        public override void Traverse(ITraverser target)
        {
            target.Apply(this);
        }
        #endregion 

        //#region ResourceBase members
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="specOnly">True returns the properties without any values assigned</param>
        ///// <returns></returns>
        //public override Settings.PropertySpec[] GetProperties(bool specOnly)
        //{
        //    Settings.PropertySpec[] tmp = base.GetProperties(specOnly);
        //    Settings.PropertySpec[] properties = new Settings.PropertySpec[1 + tmp.Length];
        //    tmp.CopyTo(properties, 1);

        //    properties[0] = new Settings.PropertySpec("lightingmode", _lightingMode.GetType().Name);

        //    if (!specOnly)
        //    {
        //        properties[0].DefaultValue = (int)_lightingMode;
        //    }

        //    return properties;
        //}

        //public override void SetProperties(Settings.PropertySpec[] properties)
        //{
        //    if (properties == null) return;
        //    base.SetProperties(properties);

        //    for (int i = 0; i < properties.Length; i++)
        //    {
        //        // use of a switch allows us to pass in all or a few of the propspecs depending
        //        // on whether we're loading from xml or changing a single property via server directive
        //        switch (properties[i].Name)
        //        {
        //            case "lightingmode":
        //                _lightingMode = (CONST_TV_LIGHTINGMODE)((int)properties[i].DefaultValue);
        //                break;
        //        }
        //    }
        //}
        //#endregion

        public override ChildSetter GetChildSetter()
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        // todo: I think i need to take into account AlphaBlending and AlphaTestRef value now
        protected override void ComputeHashCode()
        {
            byte[] baseData = base.GetHashData();

            if ((baseData == null || baseData.Length == 0) ) //&& _lightingMode == CONST_TV_LIGHTINGMODE.TV_LIGHTING_NONE)
            {
                _hashCode = 0;
                return;
            }

            // combine the data with lighting mode bytes and then compute crc32 for hash
            int length;
            if (baseData == null) 
                length = 0;
            else 
                length = baseData.Length ;

            //length += 4; // 4 bytes for the lighting data
            //byte[] tmp;
            //tmp = BitConverter.GetBytes((int)_lightingMode);

            byte[] data = new byte[length];
            //Array.Copy(tmp, 0, data, 0, 4);
            Array.Copy(baseData, 0, data, 0, length);

            _hashCode = BitConverter.ToInt32(CRC32.Crc32(data), 0);
        }


        //public virtual CONST_TV_LIGHTINGMODE LightingMode
        //{
        //    get { return _lightingMode; }
        //    set
        //    {
        //        _lightingMode = value;
        //        SetChangeFlags(Enums.ChangeStates.AppearanceChanged, Enums.ChangeSource.Self);
        //    }
        //}


        public GroupAttribute[] Groups
        {
            get
            {
                if (_children == null) return null;
                GroupAttribute[] tmp = new GroupAttribute[_children.Count];
                int j = 0;
                for (int i = 0; i < _children.Count; i++)
                    if (_children[i] is GroupAttribute)
                    {
                        tmp[j] = (GroupAttribute) _children[i];
                        j++;
                    }

                // need to resize to just the length of the groups without materials
                GroupAttribute[] final = new GroupAttribute[j];
                Array.Copy(tmp, final, j);
                return final;
            }
        }

        #region IGroup members
        protected override void PropogateChangeFlags(Enums.ChangeStates flags, Enums.ChangeSource source)
        {
            switch (flags)
            {
                case Enums.ChangeStates.AppearanceChanged :
                    // if it's a child or self we notify parent models
                    if (source == Enums.ChangeSource.Self || source == Enums.ChangeSource.Child)
                        NotifyParents(flags);
                    // if a parent for some reason such as having added this appearance, then buck stops here.  Do nothing.
                    
                    break;
            }
            //note: no need to notify parents because our traverser will compare the HashCode of 
            // this appearance with the appearance currently set on the Geometry in question
            // to determine if it needs to be re-Apply(geometry).   Or is this the incorrect way?  Maybe we should
            // not use a HashCode and instead set the actual flag?  

            // NOTE: if the HashCode == 0 then the Apply will be skipped since clearly no children exist
            // (todo: well, lightingmode will?  could maybe special case lightingMode to always
            // apply and to have the Geometry check its previous lighting modes before changing..
        }

        protected override void NotifyParents(Enums.ChangeStates flags)
        {
            if (_parents == null) return;
            foreach (Entity  parent in _parents)
                if (parent != null)
                    parent.SetChangeFlags(flags, Enums.ChangeSource.Child);
        }
        #endregion


        public void AddChild(GroupAttribute child)
        {
            base.AddChild(child);
            SetChangeFlags(Enums.ChangeStates.AppearanceChanged, Enums.ChangeSource.Self);
        }
    }
}