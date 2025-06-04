using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Keystone.Loaders
{
    // http://local.wasp.uwa.edu.au/~pbourke/dataformats/mtl/
    // http://www.fileformat.info/format/material/
    public class WaveFrontObjMaterialLibraryLoader
    {
        private const string ID_NEW_MATERIAL = "newmtl";

        // for color tags, there are 3 mutually exclusive (only the use of one form of these tags should exist in a properly formated material)
        // formats for each.  Kx r g b,  Kx spectral.filter , Kx xyz x y z
        private const string ID_AMBIENT = "Ka";
        private const string ID_DIFFUSE = "Kd";
        private const string ID_SPECULAR = "Ks";
        private const string ID_EMISSIVE = "Tf"; // ID_TRANSMISSION_FILTER, is this basically emissive?

        //Ns exponent
        //Specifies the specular exponent for the current material. This defines the focus of the specular highlight.
        //"exponent" is the value for the specular exponent. A high exponent results in a tight, concentrated highlight.
        // Ns values normally range from 0 to 1000.
        private const string ID_SPECULAR_POWER = "Ns";

        //        Specifies the dissolve for the current material.
        //"factor" is the amount this material dissolves into the background. A factor of 1.0 is fully opaque. This is the 
        // default when a new material is created. A factor of 0.0 is fully dissolved (completely transparent).
        //Unlike a real transparent material, the dissolve does not depend upon material thickness nor does it have any 
        // spectral character. Dissolve works on all illumination models.
        private const string ID_OPACITY = "d"; // d stands for "disolve"
        
        //d -halo factor
        //Specifies that a dissolve is dependent on the surface orientation relative to the viewer. 
        // For example, a sphere with the following dissolve, d -halo 0.0, will be fully dissolved at its center and will appear 
        // gradually more opaque toward its edge.

        //"factor" is the minimum amount of dissolve applied to the material. The amount of dissolve will vary between 1.0 (fully opaque) 
        // and the specified "factor". The formula is:

        //dissolve = 1.0 - (N*v)(1.0-factor)

        // texture maps
        private const string ID_ALPHA_MAP = "map_Ka";
        private const string ID_DIFFUSE_MAP = "map_Kd";
        private const string ID_SPECULAR_MAP = "map_Ks";
        private const string ID_NORMAL_MAP = "map_Ns"; // material specular exponent
        private const string ID_DISOLVE_MAP = "map_d";
        private const string ID_DISPLACEMENT_MAP = "disp";
        private const string ID_BUMP_MAP = "bump";
        private const string ID_DECAL_MAP = "decal";
        //llum illum_#
        private const string ID_ILLUMINIATION = "illum";
        //The "illum" statement specifies the illumination model to use in the material. Illumination models are mathematical equations that represent various material lighting and shading effects.

        //"illum_#"can be a number from 0 to 10. The illumination models are summarized below; for complete descriptions see "Illumination models" on page 5-30.

        //Illumination Properties that are turned on in the
        //model Property Editor

        //0 Color on and Ambient off
        //1 Color on and Ambient on
        //2 Highlight on
        //3 Reflection on and Ray trace on
        //4 Transparency: Glass on
        //Reflection: Ray trace on
        //5 Reflection: Fresnel on and Ray trace on
        //6 Transparency: Refraction on
        //Reflection: Fresnel off and Ray trace on
        //7 Transparency: Refraction on
        //Reflection: Fresnel on and Ray trace on
        //8 Reflection on and Ray trace off
        //9 Transparency: Glass on
        //Reflection: Ray trace off
        //10 Casts shadows onto invisible surfaces 

        private const string ID_REFLECTIONS_SHARPNESS = "sharpness";

        private const string ID_OPTICAL_DENSITY = "Ni"; // also known as index of refraction


        public static Dictionary<string, WaveFrontObjMaterial> Load(string filePath)
        {
            if (!File.Exists(filePath))
            {
            	System.Diagnostics.Trace.WriteLine(string.Format ("WaveFrontObjMaterialLibraryLoader.Load() - WARNING: '{0}' file not found.", filePath));
                return null;
            }

            StreamReader reader = null;

            try
            {
                reader = File.OpenText(filePath);
                return Load(reader);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("WaveFrontObjMaterialLibraryLoader.Load() - ERROR: '" + filePath + "' loading failed. " + ex.Message);
            }
            finally
            {
                reader.Close();
            }
            return null;
        }

        /// <summary>
        /// Parses a valid .mtl file and returns an array of all found materials
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static Dictionary<string, WaveFrontObjMaterial> Load(StreamReader reader)
        {
            Dictionary<string, WaveFrontObjMaterial> materials = new Dictionary<string, WaveFrontObjMaterial>();
            WaveFrontObjMaterial currentMaterial = null;
            string input = null;
            uint lineNumber = 0;
            try
            {
                // TODO: does this ends on blank line when it should merely continue til EOF?  not sure how .ReadLine treats an empty line
                while ((input = reader.ReadLine()) != null)
                {
                    lineNumber++;
                    // if the first char is a # it is a comment and we can skip
                    if (input.StartsWith(WavefrontObjLoader.ID_COMMENT)) continue;

                    // the following Trim() and continue's can be removed for a little more speed if you're sure source .obj are good and well formed.
                    input = input.Trim();
                    // in case there's a poorly formed line in the file that only contains white space (tabs, spaces, etc)
                    if (input.Length == 0) continue;
                    string[] tokens = Converters.Tokenizer.Tokenize(input);
                    if (tokens.Length == 0) continue;

                    // first token determines what the rest of the field contains
                    switch (tokens[0])
                    {
                        case ID_NEW_MATERIAL:
                            // verify name doesn't already exist
                            string name = tokens[1];

                            if (materials.ContainsKey(name))
                                continue; 

                            //System.Diagnostics.Trace.WriteLine(string.Format("WaveFrontObjMaterialLibraryLoader.Load() - New Wavefront Material '{0}' found.", name));
                            // create new material and add it to the WaveFrontObj.Materials array
                            currentMaterial = new WaveFrontObjMaterial(name);
                            materials.Add(name, currentMaterial);
                            break;

                        case ID_DIFFUSE:
                            currentMaterial.Diffuse = ReadColor(tokens);
                            break;
                        case ID_AMBIENT:
                            currentMaterial.Ambient = ReadColor(tokens);
                            break;
                        case ID_SPECULAR:
                            currentMaterial.Specular = ReadColor(tokens);
                            break;
                        case ID_EMISSIVE:
                            currentMaterial.Emissive = ReadColor(tokens);
                            break;
                        case ID_OPACITY:
                            currentMaterial.Opacity = float.Parse(tokens[1]);
                            break;
                        case ID_SPECULAR_POWER:
                            currentMaterial.SpecularPower = float.Parse(tokens[1]);
                            break;
                        case ID_DIFFUSE_MAP:
                            currentMaterial.TextureFile = tokens[1];
                            break;
                        default:
                            {
                                //System.Diagnostics.Trace.WriteLine(string.Format("WaveFrontObjMaterialLibraryLoader.Load() - WARNING: Currently unsupported Wavefront Material element {0} on line {1}", tokens[0], lineNumber));
                                break;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(string.Format("WaveFrontObjMaterialLibraryLoader.Load() - ERROR: line {0} src = {1}", lineNumber, input) + ex.Message);
            }

            finally
            {
                System.Diagnostics.Trace.Assert(reader.EndOfStream == true);
                System.Diagnostics.Trace.WriteLine(string.Format("WaveFrontObjMaterialLibraryLoader.Load() - SUCCESS: Parse complete. {0} Lines parsed.", lineNumber));

            }
            return materials;
        }

        private static MTV3D65.TV_COLOR ReadColor(string[] tokens)
        {
            return new MTV3D65.TV_COLOR(float.Parse(tokens[1]), float.Parse(tokens[2]), float.Parse(tokens[3]), 1);
        }
    }
}
