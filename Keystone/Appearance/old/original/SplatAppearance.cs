using System;
using System.Diagnostics;
using Keystone.Elements;
using Keystone.Entities;

namespace Keystone.Appearance
{
    public class SplatAppearance : Appearance
    {

        /// <summary>
        /// Note: No static Create methods since Appearance nodes cannot be shared.
        /// </summary>
        /// <param name="id"></param>
        public SplatAppearance(string id)
            : base(id)
        {
        }

        public override int Apply(Terrain land, int appearanceFlags)
        {
            // no need to aply if the target geometry already has these settings set
            if (land.LastAppearance == GetHashCode()) return _hashCode;

            // If splatting, no clamping can be used or the splats wont tile properly
            land.SetClampingEnable = false;
            land.SetSplattingEnable = true;
            land.SetSplattingMode(true, true);
            land.OptimizeSplatting(true, true);
            land.LightingMode = _lightingMode;

            bool baseTextureIsSet = false;

            if (_material != null) land.SetMaterial(_material.TVIndex, -1);

            // todo: if a texture is deleted, we need to call SetTexture(-1)
            // todo: we need to check if texture.TVResourceIsLoaded which is not being done here
            //  prior to attempting to SetTexture
            int iGroup = 0;
            foreach (GroupAttribute  g in Groups)
            {
                if (g.Material != null)
                    land.SetMaterial(g.Material.TVIndex, iGroup);

                foreach (Layer layer in g.Layers)
                {
                    if (layer is Diffuse)
                    {
                        //  if there is a diffuse layer, it must be the first and only one
                        // This acts as a base texture without an alphamap, otherwise, you can have
                        // just SplatAlpha texture's which always include both a Base diffuse and an alpha texture
                        if (!baseTextureIsSet)
                        {
                            land.SetTextureScale(layer.TileU, layer.TileV, iGroup);
                            land.SetTexture(layer.TextureIndex, iGroup);
                            baseTextureIsSet = true;
                        }
                        else
                        {
                            Trace.WriteLine(("SplatSet:Apply() -- " + " some problem..."));
                        }
                    }
                    else if (layer is SplatAlpha)
                    {
                        SplatAlpha alpha = (SplatAlpha) layer;
                        land.AddSplattingTexture(alpha.baseTextureIndex, 0, layer.TileU, layer.TileV);
                        land.ExpandSplattingTexture(alpha.TextureIndex, alpha.baseTextureIndex, alpha.ChunkStartX,
                                                    alpha.ChunkStartY, alpha.ChunksX, alpha.ChunksY);
                    }
                }
                iGroup++;
            }

            DisableChangeFlags(Keystone.Enums.ChangeStates.AppearanceChanged);
            return _hashCode;
        }

        public override int Apply(Actor3d actor, int appearanceFlags)
        {
            if (actor.LastAppearance == GetHashCode()) return _hashCode;

            DisableChangeFlags(Keystone.Enums.ChangeStates.AppearanceChanged);

            throw new Exception("SplatAppearance:Apply() -- " + " Splats cannot be applied to Actor3d.");
        }

        public override int Apply(Mesh3d mesh, int appearanceFlags)
        {
            if (mesh.LastAppearance == GetHashCode()) return _hashCode;
            
            DisableChangeFlags(Keystone.Enums.ChangeStates.AppearanceChanged);

            throw new Exception("SplatAppearance:Apply() -- " + " Splats cannot be applied to Meshes.");
        }

        public override int Apply(Minimesh2 mini, int appearanceFlags)
        {
            if (mini.LastAppearance == GetHashCode()) return _hashCode;
            if ((_changeStates & Enums.ChangeStates.AppearanceChanged) == 0) return 0;

            DisableChangeFlags(Keystone.Enums.ChangeStates.AppearanceChanged);
            throw new Exception("SplatAppearance:Apply() -- " + " Splats cannot be applied to Minimeshes.");
        }
    }
}