using System;
using System.Collections.Generic;
using System.Text;
using MTV3D65;

namespace Keystone.Loaders
{
    public class WaveFrontObjMaterial
    {
        public string Name;
        public MTV3D65.TV_COLOR Diffuse = new TV_COLOR( 1,1,1,1);
        public MTV3D65.TV_COLOR Ambient = new TV_COLOR( 0,0,0,1);
        public MTV3D65.TV_COLOR Specular = new TV_COLOR(0, 0, 0, 1);
        public MTV3D65.TV_COLOR Emissive = new TV_COLOR(0, 0, 0, 1);
        /// <summary>
        /// Phong specular component ranges from 0 - 200
        /// </summary>
        public float SpecularPower = 0;
        public float Opacity = 1;    // franges from 0 to 1.  TV's must then convert from 0 to 255
        public string TextureFile;
        public string BumpTextureFile;

        public WaveFrontObjMaterial (string name)
        {
            Name = name;
        }

        /// <summary>
        /// Compares all field values EXCEPT "Name" for equality.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is WaveFrontObjMaterial == false)
                return base.Equals(obj);

            WaveFrontObjMaterial material = (WaveFrontObjMaterial)obj;

            // NOTE: Material.Name is NOT evaluated.
            if (material.Diffuse.Equals(Diffuse) &&
                material.Ambient.Equals(Ambient) &&
                material.Specular.Equals(Specular) &&
                material.Emissive.Equals(Emissive) &&
                material.SpecularPower == SpecularPower &&
                material.Opacity == Opacity &&
                string.Equals(material.TextureFile, TextureFile) &&
                string.Equals(material.BumpTextureFile, BumpTextureFile))
                return true;

            return false;

        }
    }
}
