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
            _defaultMaterial = -1;
            _defaultTexture = -1;
        }

        public static int Apply(Actor3d actor,int appearanceFlags, int hashcode)
        {
            if (hashcode == 0) return 0;
            actor.LightingMode = _lightingMode;
            actor.Shader = null;
            actor.SetMaterial(_defaultMaterial, -1);
            actor.SetTexture(_defaultTexture, -1);
            return 0;
        }

        public static int Apply(Mesh3d mesh, int appearanceFlags, int hashcode)
        {
            if (hashcode == 0) return 0;
            mesh.LightingMode = _lightingMode;
            mesh.Shader = null;
            mesh.SetMaterial(_defaultMaterial, -1);
            mesh.SetTexture(_defaultTexture, -1);
            return 0;
        }

        public static int Apply(Minimesh2 mini, int appearanceFlags, int hashcode)
        {
            throw new NotImplementedException();
        }

        public static int Apply(Terrain land, int appearanceFlags, int hashcode)
        {
            if (hashcode == 0) return 0;
            land.LightingMode = _lightingMode;
            land.Shader = null;
            land.SetSplattingEnable = false;
            // in our Chunk.cs get rid of direct access to TVLand and everything goes thru the chunk which will abstract the handling of the chunk index stuff
            land.SetClampingEnable = true;
            land.SetMaterial(_defaultMaterial, -1);
            land.SetTextureScale(1, 1, -1);
            land.SetTexture(_defaultTexture, -1);
            land.SetDetailTexture(_defaultTexture, -1); // note: land doesnt have SetTextureEx()
            land.SetLightMapTexture(_defaultTexture, -1);
            return 0;
        }
    }
}