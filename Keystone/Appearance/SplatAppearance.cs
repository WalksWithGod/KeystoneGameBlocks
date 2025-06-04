using System;
using System.Diagnostics;
using Keystone.Elements;
using Keystone.Entities;
using Keystone.IO;

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

        public override int Apply(Terrain land, double elapsedSeconds)
        {
            // if the shader's current parameters do not match the ones used here
            // we must update them.. problem is ChangeStates.ShaderParameterValuesChanged
            // is not going to cover that case
            // NOTE: We must always re-apply shader parameters because if more than one instance hosts 
            // the same shader, the assignment of parameters of one, will affect all the other instances.
            // So we must apply the unique parameters to all.
            //if ((mChangeStates & (Keystone.Enums.ChangeStates.ShaderParameterValuesChanged | Keystone.Enums.ChangeStates.ShaderFXLoaded)) != 0)
            //if ((mChangeStates & (Keystone.Enums.ChangeStates.ShaderParameterValuesChanged | Keystone.Enums.ChangeStates.ShaderFXLoaded)) != 0)
                ApplyShaderParameterValues();

                        
            // no need to aply if the target terrain already has these settings set
            if (land.LastAppearance == GetHashCode())  return mHashCode;


            
            // If splatting, no clamping can be used or the splats wont tile properly
            land.ClampingEnable = false;
            
            
            land.SetSplattingEnable = true;
            land.SetSplattingMode(true, false);
            land.OptimizeSplatting(true, true);
            land.LightingMode = _lightingMode;

            bool baseTextureIsSet = false;

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

            if (mLayers != null)
            {
            	land.ClearSplats();
            	land.RemoveSplats();
                for (int i = 0; i < mLayers.Length; i++)
            	{
            		SplatAlpha alpha = (SplatAlpha)  mLayers[i];
            		// TODO: if the base texture isn't loaded yet
            		if (alpha.Texture != null && alpha.Texture.TVResourceIsLoaded)
                    	land.AddSplattingTexture(alpha.Texture.TVIndex, 0, alpha.TileU, alpha.TileV);
                    if (alpha.AlphaTexture != null && alpha.AlphaTexture.TVResourceIsLoaded)
	                    land.ExpandSplattingTexture(alpha.AlphaTexture.TVIndex, alpha.Texture.TVIndex, alpha.ChunkStartX,
                                                alpha.ChunkStartY, alpha.ChunksX, alpha.ChunksY);
            	}
            }

            // TODO: if a texture is deleted, we need to call SetTexture(-1)
            // TODO: we need to check if texture.TVResourceIsLoaded which is not being done here
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
                        // If there is a diffuse layer, it must be the first and only one
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
                        land.AddSplattingTexture(alpha.Texture.TVIndex, 0, layer.TileU, layer.TileV);
                        land.ExpandSplattingTexture(alpha.AlphaTexture.TVIndex, alpha.Texture.TVIndex, alpha.ChunkStartX,
                                                    alpha.ChunkStartY, alpha.ChunksX, alpha.ChunksY);
                    }
                }
                iGroup++;
            }
            return mHashCode;
        }

        public override int Apply(Actor3dDuplicate actor, double elapsedSeconds)
        {
            throw new Exception("SplatAppearance:Apply() -- " + " Splats cannot be applied to Actor3d.");
        }

        //public override int Apply(Actor3d actor, double elapsedSeconds)
        //{
        //    throw new Exception("SplatAppearance:Apply() -- " + " Splats cannot be applied to Actor3d.");
        //}

        public override int Apply(Mesh3d mesh, double elapsedSeconds)
        {
            throw new Exception("SplatAppearance:Apply() -- " + " Splats cannot be applied to Meshes.");
        }

        public override int Apply(MinimeshGeometry mini, double elapsedSeconds)
        {
            throw new Exception("SplatAppearance:Apply() -- " + " Splats cannot be applied to Minimeshes.");
        }

        public override int Apply(Minimesh2 mini, double elapsedSeconds)
        {
            throw new Exception("SplatAppearance:Apply() -- " + " Splats cannot be applied to Minimeshes.");
        }
    }
}