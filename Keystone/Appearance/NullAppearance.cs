using Keystone.Elements;
using Keystone.Entities;
using MTV3D65;
using System;

namespace Keystone.Appearance
{
    /// <summary>
    /// Null material removes all material from geometry.  This is because when\if an Appearance node is removed from a model
    /// Potentially the mesh will maintain it's previous settings.  Applying a null material ensures that all textures and materials and shaders
    /// are removed.
    /// </summary>
    public static class NullAppearance
    {
        private static CONST_TV_LIGHTINGMODE _lightingMode;
        private static int _defaultMaterial;
        private static int _defaultTexture;
        static NullAppearance()
        {
            _lightingMode = CONST_TV_LIGHTINGMODE.TV_LIGHTING_NONE;
            _defaultMaterial = 0;  // NOTE: we use material index 0 which is the default tv3d material.  We cannot use -1 for materialIndex or else we'll get an access violation on tvmesh.Render()
            _defaultTexture = -1;
        }

        public static int Apply(ParticleSystemDuplicate ps, float elapsedSeconds)
        {
            return 0; // using -1 on pointsprite results in an accessviolation
            ps.SetTexture(0, 0, -1);
        }
        public static int Apply(Actor3dDuplicate actor, float elapsedMilliseconds)
        {
            throw new NotImplementedException();
            //if (hashcode == 0) return 0;
            //actor.LightingMode = _lightingMode;
            //actor.Shader = null;
            //actor.SetMaterial(_defaultMaterial, -1);
            //actor.SetTexture(_defaultTexture, -1);
            //return 0;
        }

        //public static int Apply(Actor3d actor, int hashcode)
        //{
        //    if (hashcode == 0) return 0;
        //    actor.LightingMode = _lightingMode;
        //    actor.Shader = null;
        //    actor.SetMaterial(_defaultMaterial, -1);
        //    actor.SetTexture(_defaultTexture, -1);
        //    return 0;
        //}

        public static int Apply (InstancedGeometry geometry, int hashcode)
        {
            if (geometry.Shader == null) return 0;
            geometry.Shader.SetShaderParameterTexture("Texture", -1);
        	return 0;
        }
        
        public static int Apply(Mesh3d mesh, int hashcode)
        {
            if (hashcode == 0) return 0;
            mesh.LightingMode = _lightingMode;
            mesh.Shader = null;
            // NOTE: _defaultMaterial must be >= 0.  Trying to completely errase using -1 will result in a tvmesh.Render() access violation.
            mesh.SetMaterial(_defaultMaterial, -1);
            mesh.SetTexture(_defaultTexture, -1);
            return 0;
        }

        public static int Apply(MinimeshGeometry mini, double elapsedSeconds)
        {
            return 0;
        }

        public static int Apply(Minimesh2 mini, int hashcode)
        {
            if (hashcode == 0) return 0;
            return 0;
            // throw new NotImplementedException();
        }

        public static int Apply(TexturedQuad2D quad, int hashcode)
        {
            if (hashcode == 0) return 0;
            return 0;
            // throw new NotImplementedException();
        }

        public static int Apply(Terrain land, int hashcode)
        {
            if (hashcode == 0) return 0;
            land.LightingMode = _lightingMode;
            land.Shader = null;
            land.SetSplattingEnable = false;
            // in our Chunk.cs get rid of direct access to TVLand and everything goes thru the chunk which will abstract the handling of the chunk index stuff
            land.ClampingEnable = true;
            land.SetMaterial(_defaultMaterial, -1);
            land.SetTextureScale(1, 1, -1);
            land.SetTexture(_defaultTexture, -1);
            land.SetDetailTexture(_defaultTexture, -1); // note: land doesnt have SetTextureEx()
            land.SetLightMapTexture(_defaultTexture, -1);
            return 0;
        }
    }
}