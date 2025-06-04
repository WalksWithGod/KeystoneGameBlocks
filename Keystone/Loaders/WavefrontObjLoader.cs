using System;
using System.Collections.Generic;
using System.IO;
using MTV3D65;
using Keystone.Appearance;

namespace Keystone.Loaders
{
    /// <summary>
    /// Uses the 3.0 .obj specification and is backwards compatible
    /// http://www.martinreddy.net/gfx/3d/OBJ.spec
    /// http://www.robthebloke.org/source/obj.html
    /// http://www.fileformat.info/format/wavefrontobj/
    /// </summary>
    public class WavefrontObjLoader
    {
        #region constants

        private const int GL_TO_DX_OPACITY = 255;

        internal const string ID_COMMENT = "#";

        // vertex data
        private const string ID_VERTEX = "v";
        private const string ID_TEXTURE_COORD = "vt";
        private const string ID_VERTEX_NORMAL = "vn";
        private const string ID_PARAMETER_SPACE_VERTICES = "vp"; // v 3.0

        // elements
        private const string ID_POINT = "p";
        private const string ID_LINE = "l";
        private const string ID_FACE = "f";
        private const string ID_CURVE = "curv"; // v 3.0
        private const string ID_2DCURVE = "curv2"; // v 3.0
        private const string ID_SURFACE = "surf"; // v 3.0


        //Free-form curve/surface attributes:   // v 3.0
        private const string ID_DEGREE = "deg";
        private const string ID_BASIS_MATRIX = "bmat";
        private const string ID_STEP_SIZE = "step";
        private const string ID_CURVE_OR_SURFACE_TYPE = "cstype";

        // Free-form curve/surface body statements:
        private const string ID_PARAMETER_VALUES = "parm";
        private const string ID_OUTER_TRIMMING_LOOP = "trim";
        private const string ID_INNER_TRIMMING_LOOP = "hole";
        private const string ID_SPECIAL_CURVE = "scrv";
        private const string ID_SPECIAL_POINT = "sp";
        private const string ID_END_STATEMENT = "end";


        // Connectivity between free-form surfaces:
        private const string ID_CONNECT = "con"; // v 3.0

        // grouping
        private const string ID_GROUP_NAME = "g";
        private const string ID_OBJECT_NAME = "o";
        private const string ID_SMOOTHING_GROUP = "s";
        private const string ID_MERGING_GROUP = "mg";
        private const string ID_OFF_TAG = "off";

        // Display/render attributes:
        private const string ID_BEVEL_INTERPOLATION = "bevel"; // v 3.0
        private const string ID_COLOR_INTERPOLATION = "c_interp";
        private const string ID_DISSOLVE_INTERPOLATION = "d_interp";
        private const string ID_MATERIAL_NAME = "usemtl";
        private const string ID_MATERIAL_LIBRARY = "mtllib";
        private const string ID_LEVEL_OF_DETAIL = "lod"; // v 3.0
        private const string ID_SHADOW_CASTING = "shadow_obj"; // v 3.0
        private const string ID_RAY_TRACING = "trace_obj"; // v 3.0
        private const string ID_CURVE_APPROX_TECHNIQUE = "ctech"; // v 3.0
        private const string ID_SURFACE_APPROX_TECHNIQUE = "stech"; // v 3.0

        #endregion

        public static void ObjToIndexedPrimitive (WaveFrontObj wavefrontObj, string id, bool loadTextures, bool loadMaterials, out Keystone.Appearance.DefaultAppearance appearance)
        {
        	appearance = 
                new Keystone.Appearance.DefaultAppearance(Resource.Repository.GetNewName (typeof(Keystone.Appearance.DefaultAppearance)));
        	
        	Dictionary <Tuple<uint, uint, uint>, int> vertices = new Dictionary<Tuple<uint, uint, uint>, int>();
	
			// TODO: assumes each face only has 3 vertices, will not work properly otherwise        	
			int[] indices = new int[wavefrontObj.Faces.Count * 3];
        	
        	
        	foreach (WaveFrontObjGroup group in wavefrontObj.Groups)
            {
                if (group.Faces == null)
                    continue;
                // some obj files will export groups with no faces but which may contain no data or special data like points, lines, & curves

                foreach (WaveFrontObjIndexedFace face in group.Faces)
                {
                	//TV_SVERTEX 
                    TV_3DVECTOR normal = new TV_3DVECTOR(0,0,0);
                    if (face.Normals == null)
                        normal = face.GetFaceNormal();
                    
                    
//                    // Add each vertex one at a time
//                    for (int j = 0; j < face.TriangulatedPoints.Length; j++)
//                    {
//                    	uint pIndex = face.TriangulatedPoints[j];
//                    	uint nIndex = face.Normals[j];
//                    	uint tIndex = face.Textures[j];
//                    	
//                    	Tuple<uint, uint, uint> t = new Tuple<uint, uint, uint>(pIndex, nIndex, tIndex);
//                    	int vertexIndex;
//                    	if (vertices.TryGetValue (t, out vertexIndex))
//                    	{
//                    		indices[] = vertexIndex;
//                    	}
//                    	else 
//                    	{
//                    		vertexIndex = vertices.Count - 1;
//                    		vertices.Add (t, vertexIndex);
//                    	}
//                    }
                }
                
                LoadAppearance (wavefrontObj, group, appearance, loadMaterials);
        	}
        }
        
        public static void ObjToTVM(WaveFrontObj wavefrontObj, string id, bool loadTextures, bool loadMaterials, ref TVMesh m, out Keystone.Appearance.DefaultAppearance appearance)
        {


            System.Diagnostics.Debug.WriteLine("WavefrontObjLoader.ObjToTVM() - beginning.");
            appearance = 
                new Keystone.Appearance.DefaultAppearance(Resource.Repository.GetNewName (typeof(Keystone.Appearance.DefaultAppearance)));



            m.SetPrimitiveType(CONST_TV_PRIMITIVETYPE.TV_TRIANGLELIST);

            int tmpGroupCount = 0;
            int tmpFaceCount = 0;
            try
            {
                // TODO: lets just use AddVertex for now instead of SetGeometry.  eventually need to change because
                // AddVertex results in allot of duplicated vertices whereas with SetGeometry we can properly share vertices 
                // via the index list.  But it's annoying as hell because here we're treating every vertex as unique and with
                // setGeometry we cant assume 1:1 relationship with a coordinate and a normal vector.  So we'd have to
                // totally recompute a new set of indices based on new vertices where a coord that could have two different normals
                // had to have a new vertex computed for each different normal.
                foreach (WaveFrontObjGroup group in wavefrontObj.Groups)
                {
                    if (group.Faces == null)
                        continue;
                    // some obj files will export groups with no faces but which may contain no data or special data like points, lines, & curves
                    int groupID = m.AddFace();

                    foreach (WaveFrontObjIndexedFace face in group.Faces)
                    {
                        TV_3DVECTOR normal = new TV_3DVECTOR(0,0,0);
                        if (face.Normals == null)
                            normal = face.GetFaceNormal();

                        // Add each vertex one at a time
                        //for (int j = 0; j < face.TriangulatedPoints.Length; j++)
                        for (int j = face.TriangulatedPoints.Length - 1; j >= 0; j--)
                        {
                            // see if we need to use a smoothing group normal instead
                            if (face.Normals == null && face.SmoothingGroup != null)
                            {
                                // On smoothing groups.  This is what sybixus had to say on how to properly import and export these:
                                // http://www.truevision3d.com/forums/content_pipeline/smoothing_group_support-t17973.0.html
                                // Game engines don't generally use smoothing groups, they have no correlation in hardware. 
                                // Normally, what happens is the exporter takes care of this when you export. If two adjacent 
                                // triangles belong to the same smoothing group, the shared vertices have their vertex normals 
                                // set as a blended normal between the two surfaces to produce smooth lighting. If the same adjacent 
                                // triangles do *not* belong to the same smoothing group, they are "unwelded", which is to say that no 
                                // vertices are shared between the two triangles and thus they can each have their own vertex normal 
                                // in order to produce a sharp edge between them.
                                normal = face.SmoothingGroup.GetSmoothNormal(face.TriangulatedPoints[j]);
                            }

                            // if we only have coords
                            if (face.Textures == null && face.Normals == null)
                            {
                                m.AddVertex(wavefrontObj.Points[(int)face.TriangulatedPoints[j]].x,
                                            wavefrontObj.Points[(int)face.TriangulatedPoints[j]].y,
                                            wavefrontObj.Points[(int)face.TriangulatedPoints[j]].z,
                                            normal.x, normal.y, normal.z, 
                                            0, 0);
                            }
                                // if we have coords and UV
                            else if (face.Normals == null)
                            {
                                m.AddVertex(wavefrontObj.Points[(int)face.TriangulatedPoints[j]].x,
                                            wavefrontObj.Points[(int)face.TriangulatedPoints[j]].y,
                                            wavefrontObj.Points[(int)face.TriangulatedPoints[j]].z,
                                            normal.x, normal.y, normal.z,
                                            wavefrontObj.UVs[(int)face.Textures[j]].x,
                                            wavefrontObj.UVs[(int)face.Textures[j]].y);
                            }
                                // if we have coords and Normals
                            else if (face.Textures == null)
                            {
                                m.AddVertex(wavefrontObj.Points[(int)face.TriangulatedPoints[j]].x,
                                            wavefrontObj.Points[(int)face.TriangulatedPoints[j]].y,
                                            wavefrontObj.Points[(int)face.TriangulatedPoints[j]].z,
                                            wavefrontObj.Normals[(int)face.Normals[j]].x,
                                            wavefrontObj.Normals[(int)face.Normals[j]].y,
                                            wavefrontObj.Normals[(int)face.Normals[j]].z,
                                            0, 0);
                            }
                                // if we have coords, UV and Normals
                            else
                            {
                                TV_3DVECTOR n;
                                n.x = wavefrontObj.Normals[(int)face.Normals[j]].x;
                                n.y = wavefrontObj.Normals[(int)face.Normals[j]].y;
                                n.z = wavefrontObj.Normals[(int)face.Normals[j]].z;
                                CoreClient._CoreClient.Maths.TVVec3Normalize(ref normal, n);
                                m.AddVertex(wavefrontObj.Points[(int)face.TriangulatedPoints[j]].x,
                                            wavefrontObj.Points[(int)face.TriangulatedPoints[j]].y,
                                            wavefrontObj.Points[(int)face.TriangulatedPoints[j]].z,
                                            normal.x,
                                            normal.y,
                                            normal.z,
                                            wavefrontObj.UVs[(int)face.Textures[j]].x,
                                            wavefrontObj.UVs[(int)face.Textures[j]].y);
                            }
                        }
                        tmpFaceCount++;
                    }

                   
                    LoadAppearance(wavefrontObj, group, appearance, loadMaterials);

                    tmpFaceCount = 0;
                    tmpGroupCount++;
                }

                //m.ComputeNormals(); // TODO: this seems to have zero effect
                //m.InvertNormals(); // TODO: this seems to have zero effect

                // if there is only 1 group, get rid of the GroupAttribute and Add Materials and Textures
                // directly to the defaultappearance
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(string.Format("WaveFrontObjLoader.ObjToTVM() - ERROR: Failed on group count {0} on face number {1}",
                                                                 tmpGroupCount, tmpFaceCount));
                throw(ex);
            }

            System.Diagnostics.Debug.WriteLine("ObjToTVM() - COMPLETE.");
        }

        

        private static void LoadAppearance (WaveFrontObj wavefrontObj, WaveFrontObjGroup group, DefaultAppearance appearance, bool loadMaterials)
        {
        	// TODO: Below rather than do the following, we should create a proper DefaultAppearance with groupAttributes
            // and then allow those materials and textures to get set normally when apperance nodes are added to models
            // (or when appearance nodes are modified)
            // add any material to the face
            if (loadMaterials == true) // must NOT loadMaterials when loading from xml or this will add the material with same "id" to cache and end up using that instead of deserializing hte saved one!
            {

                GroupAttribute attribute;
                if (wavefrontObj.Groups.Count == 1)
                    attribute = appearance;
                else 
                    attribute = new GroupAttribute(Keystone.Resource.Repository.GetNewName(typeof(GroupAttribute)));

                if (group.Material != null)
                {
                    string materialName = group.Material.Name;
                    Material mat = Material.Create(materialName, Helpers.TVTypeConverter.FromTVColor(group.Material.Diffuse),
                                        Helpers.TVTypeConverter.FromTVColor(group.Material.Ambient),
                                        Helpers.TVTypeConverter.FromTVColor(group.Material.Specular),
                                        Helpers.TVTypeConverter.FromTVColor(group.Material.Emissive));

                    mat.SpecularPower = group.Material.SpecularPower;
                    mat.Opacity = group.Material.Opacity;
                    attribute.AddChild(mat);

                    //// create a new tv material if necessary and add it to the mesh
                    //int newMaterial = CoreClient._CoreClient.MaterialFactory.CreateMaterial(group.Material.Name);
                    //CoreClient._CoreClient.MaterialFactory.SetDiffuse(newMaterial, group.Material.Diffuse.r,
                    //                                      group.Material.Diffuse.g, group.Material.Diffuse.b, group.Material.Diffuse.a);
                    //CoreClient._CoreClient.MaterialFactory.SetAmbient(newMaterial, group.Material.Ambient.r,
                    //                                      group.Material.Ambient.g, group.Material.Ambient.b, group.Material.Ambient.a);
                    //CoreClient._CoreClient.MaterialFactory.SetSpecular(newMaterial, group.Material.Specular.r,
                    //                                      group.Material.Specular.g, group.Material.Specular.b, group.Material.Specular.a);
                    //CoreClient._CoreClient.MaterialFactory.SetEmissive(newMaterial, group.Material.Emissive.r,
                    //                                      group.Material.Emissive.g, group.Material.Emissive.b, group.Material.Emissive.a);
                    //CoreClient._CoreClient.MaterialFactory.SetPower(newMaterial, group.Material.SpecularPower);
                    //CoreClient._CoreClient.MaterialFactory.SetOpacity(newMaterial, group.Material.Opacity * GL_TO_DX_OPACITY);
                    ////same for any textures
                    //// then in our Mesh3d.cs, we already have code to map the loaded materials and textures properly in an Appearance node
                    //m.SetMaterial(newMaterial, groupID);

                    if (!string.IsNullOrEmpty(group.Material.TextureFile))
                    {
                        string textureFile = group.Material.TextureFile;
                        string ext = System.IO.Path.GetExtension(textureFile);
                        if (string.IsNullOrEmpty(ext))
                            textureFile += ".png";  // default extension of .png we add if it's not included on the texture filename

                        //int textureIndex = CoreClient._CoreClient.TextureFactory.LoadTexture(System.IO.Path.GetDirectoryName(id) + "\\" + textureFile);
                        //// TODO: eventually need more layer support.  just diffuse for now
                        //m.SetTextureEx(0, textureIndex, groupID);
                        // NOTE: Here is where we create the Appearance Attributes so here
                        // is where we would rename mtl textures to use proper ResourceDescriptor path

                        Keystone.Appearance.Diffuse diffuse = (Diffuse)Keystone.Resource.Repository.Create("Diffuse");
                        Keystone.Appearance.Texture tex = (Texture)Keystone.Resource.Repository.Create(textureFile, "Texture");
                        diffuse.AddChild(tex);
                        attribute.AddChild(diffuse);
                    }
                }
                if (wavefrontObj.Groups.Count > 1)
                    appearance.AddChild(attribute);
                // else 
                //     do nothing.  a model with just 1 group will not need any GroupAttribute childs
                //     as all materials, shaders, textures are added directly to the defaultappearance node.
                    
            }
        }
        
        /// <summary>
        /// Reads a .obj file and any .mtl files stored on disk
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="reverseWinding"></param>
        /// <returns></returns>
        public static WaveFrontObj ParseWaveFrontObj(string filePath, bool reverseWinding, bool mergeIdenticleMaterials, out string[] mtls, out string[] textures)
        {
            System.Diagnostics.Trace.Assert(File.Exists(filePath), "WaveFrontObjLoader.ParseWaveFrontObj() - File '" + filePath + "' not found.");
            StreamReader reader = File.OpenText(filePath);
            Dictionary<string, WaveFrontObjMaterial> materials = null;
            string dirPath = System.IO.Path.GetDirectoryName(filePath);

            // get the material file names from the main .obj file
            string[] materialFiles = ParseWaveFrontMaterialLibrary(reader);

            if (materialFiles != null)
            {
                // load the material files from disk into a dictionary of actual WaveFrontObjMaterial
                for (int i = 0; i < materialFiles.Length; i++)
                {
                    string materialPath = System.IO.Path.Combine(dirPath, materialFiles[i]);
                    materials = WaveFrontObjMaterialLibraryLoader.Load(materialPath);
                }
            }

            mtls = materialFiles;
            List<string> textureList = new List<string>();
            if (materials != null)
                foreach (WaveFrontObjMaterial mat in materials.Values)
                    if (!string.IsNullOrEmpty(mat.TextureFile))
                        textureList.Add(mat.TextureFile);

            textures = textureList.ToArray();

            // reset reader position
            reader.BaseStream.Seek(0, SeekOrigin.Begin);

            System.Diagnostics.Debug.WriteLine("WaveFrontObjLoader.ParseWaveFrontObj() - parse COMPLETE.");
            return ParseWaveFrontObj(reader, materials, reverseWinding, mergeIdenticleMaterials);
        }


        /// <summary>
        /// Reads a .obj file and any .mtl files stored in ZIP archive
        /// </summary>
        /// <param name="relativeArchivePath"></param>
        /// <param name="archiveEntryName"></param>
        /// <param name="reverseWinding"></param>
        /// <returns></returns>
        public static WaveFrontObj ParseWaveFrontObj(string relativeArchivePath, string archiveEntryName, bool reverseWinding, bool mergeIdenticleMaterials, out string[] mtls, out string[] textures)
        {
            KeyCommon.IO.ResourceDescriptor descriptor = new KeyCommon.IO.ResourceDescriptor(relativeArchivePath, archiveEntryName);
            string relativeMaterialPath = System.IO.Path.GetDirectoryName(archiveEntryName);

            Stream mem = KeyCommon.IO.ArchiveIOHelper.GetStreamFromMod(archiveEntryName, "", Keystone.Core.FullNodePath(descriptor.ModName ));
            StreamReader reader = new StreamReader(mem);

            string[] materialFiles = ParseWaveFrontMaterialLibrary(reader);
            Dictionary<string, WaveFrontObjMaterial> materials = null;

            mtls = materialFiles;
            textures = null;
            if (materialFiles != null)
            {
                for (int i = 0; i < materialFiles.Length; i++)
                {
                    // here we know that relative path is the path in a zip archive
                    // so we must load it from memory
                    string materialEntryName = Path.Combine(relativeMaterialPath, materialFiles[i]);
                    descriptor = new KeyCommon.IO.ResourceDescriptor(relativeArchivePath, materialEntryName);
                    Stream matStream = KeyCommon.IO.ArchiveIOHelper.GetStreamFromMod(descriptor.EntryName, "", Keystone.Core.FullNodePath(descriptor.ModName ));
                    if (matStream != null)
                    {
                        StreamReader matReader = new StreamReader(matStream);
                        materials = WaveFrontObjMaterialLibraryLoader.Load(matReader);
                        matStream.Close();
                    }
                }
            

                List<string> textureList = new List<string>();
                if (materials != null)
                {
	                foreach (WaveFrontObjMaterial mat in materials.Values)
    	                if (!string.IsNullOrEmpty(mat.TextureFile))
    	                    textureList.Add(mat.TextureFile);

                	textures = textureList.ToArray();
                }
            }
            
            // reset the reader
            reader.BaseStream.Seek(0, SeekOrigin.Begin);

            // now we have all the materials parsed, let's parse the actual geometry file
            WaveFrontObj result = ParseWaveFrontObj(reader, materials, reverseWinding, mergeIdenticleMaterials);
            reader.Dispose();
            return result;
            
        }

        
        /// <summary>
        /// Parses a valid .obj file and returns an array of all found groups
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="reverseWinding"></param>
        /// <returns></returns>
        private static WaveFrontObj ParseWaveFrontObj(StreamReader reader, Dictionary<string, WaveFrontObjMaterial> materialLib, bool reverseWinding, bool mergeIdenticleMaterials)
        {
            WaveFrontObj wavefrontObj = new WaveFrontObj("");
            wavefrontObj.Materials = materialLib;
            string input = null;
            uint lineNumber = 0;
            try
            {
                // TODO: does this ends on blank line when it should merely continue til EOF?  not sure how .ReadLine treats an empty line
                while ((input = reader.ReadLine()) != null)
                {
                    lineNumber++;
                    // if the first char is a # it is a comment and we can skip
                    if (input.StartsWith(ID_COMMENT)) continue;
                    
                    // the following Trim() and continue's can be removed for a little more speed if you're sure source .obj are good and well formed.
                    input = input.Trim();
                    
                    // in case there's a poorly formed line in the file that only contains white space (tabs, spaces, etc)
                    if (input.Length == 0) continue;

                    // some exporters like Polytrans will split long lines with a "\" character so we must continue on the next line
                    // and grab the rest first
                    if (input.Contains(@"\"))
                    {
                        input = ReadLine(input, reader);
                    }
                    
                    string[] tokens = Converters.Tokenizer.Tokenize(input);
                    if (tokens.Length == 0) continue;

                    // first token determines what the rest of the field contains
                    switch (tokens[0])
                    {
                        case ID_GROUP_NAME:
                        case ID_OBJECT_NAME:
                            // some groups wont have no name and some will be tokenized due to spaces in the name so handle both
                            int start = input.IndexOf(ID_GROUP_NAME) + 1;
                            string name = input.Substring(start).Trim();
                            if (string.IsNullOrEmpty(name)) name = "";
                            wavefrontObj.AddGroup(name);
                            break;
                        case ID_SMOOTHING_GROUP:
                            // if the next token is "off" then disable
                            if (tokens[1] == ID_OFF_TAG)
                                wavefrontObj.DisableCurrentSmoothingGroup();
                            else
                            {
                                //System.Diagnostics.Trace.Assert(uint.Parse(tokens[1]) > 0);
                                wavefrontObj.EnableSmoothingGroup(uint.Parse(tokens[1]));
                            }
                            break;
                        case ID_MERGING_GROUP:
                            // if the next token is "off" then disable
                            if (tokens[1] == ID_OFF_TAG)
                                wavefrontObj.DisableCurrentMergingGroup();
                            else
                            {
                                System.Diagnostics.Trace.Assert(uint.Parse(tokens[1]) > 0);
                                System.Diagnostics.Trace.Assert(uint.Parse(tokens[2]) > 0);
                                wavefrontObj.EnableMergingGroup(uint.Parse(tokens[1]), float.Parse( tokens[2]));
                            }
                            break;

                        case ID_VERTEX:
                            wavefrontObj.AddPoint(float.Parse(tokens[1]), float.Parse(tokens[2]),
                                                       float.Parse(tokens[3]));
                            break;
                        case ID_TEXTURE_COORD:
                            wavefrontObj.AddUV(float.Parse(tokens[1]), float.Parse(tokens[2]));
                            break;
                        case ID_VERTEX_NORMAL:
                            wavefrontObj.AddNormal(float.Parse(tokens[1]), float.Parse(tokens[2]),
                                                        float.Parse(tokens[3]));
                            break;
                        case ID_FACE:
                            WaveFrontObjIndexedFace face = new WaveFrontObjIndexedFace(wavefrontObj, tokens);
                            wavefrontObj.AddFace(face);
                            break;

                        case ID_MATERIAL_NAME:
                            // use this material on the current group.
                            // if the previous material does not match this, and the previous group has not changed
                            // then we should create another group.  Or assert that this is not occuring!
                            // a properly formatted .obj file must have a new group specified before any change in material 
                            // because subsequent faces must be in a new group to have a different material
                            WaveFrontObjMaterial mat = null;
                            if (wavefrontObj.Materials != null)
                                wavefrontObj.Materials.TryGetValue(tokens[1], out mat);
                            else
                                wavefrontObj.Materials = new Dictionary<string, WaveFrontObjMaterial>();

                            if (mat == null)
                            {
                                // add a material of this name to the library using default material settings
                                mat = new WaveFrontObjMaterial(tokens[1]);
                                wavefrontObj.Materials.Add(tokens[1], mat);
                            }
                            wavefrontObj.CurrentMaterial = mat;
                            break;
                        default:
                            {
                                //System.Diagnostics.Trace.WriteLine(string.Format("Unsupported element {0} on line {1}", tokens[0], lineNumber));
                                break;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(string.Format("WaveFrontObjLoader.ParseWaveFrontObj() - ERROR: line {0} src = {1}", lineNumber, input) + ex.Message);
            }

            finally
            {
                System.Diagnostics.Trace.Assert(reader.EndOfStream == true);
                System.Diagnostics.Trace.WriteLine(string.Format("WaveFrontObjLoader.ParseWaveFrontObj() - SUCCESS. {0} Lines parsed.", lineNumber));
                reader.Close();
            }


            if (mergeIdenticleMaterials)
            {
                wavefrontObj.MergeMaterials();
            }

            return wavefrontObj;
        }

        /// <summary>
        /// Parses the paths of material mtl files from the .obj file in to a collection. 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static string[] ParseWaveFrontMaterialLibrary(StreamReader reader)
        {
            string[] fileNames = null;
            int lineNumber = 0;
            string input = null;
            try
            {
                // TODO: does this ends on blank line when it should merely continue til EOF?  not sure how .ReadLine treats an empty line
                while ((input = reader.ReadLine()) != null)
                {
                    lineNumber++;
                    // if the first char is a # it is a comment and we can skip
                    if (input.StartsWith(ID_COMMENT)) continue;

                    // the following Trim() and continue's can be removed for a little more speed if you're sure source .obj are good and well formed.
                    input = input.Trim();

                    // in case there's a poorly formed line in the file that only contains white space (tabs, spaces, etc)
                    if (input.Length == 0) continue;

                    // some exporters like Polytrans will split long lines with a "\" character so we must continue on the next line
                    // and grab the rest first
                    if (input.Contains(@"\"))
                    {
                        input = ReadLine(input, reader);
                    }

                    string[] tokens = Converters.Tokenizer.Tokenize(input);
                    if (tokens.Length == 0) continue;

                    // first token determines what the rest of the field contains
                    switch (tokens[0])
                    {
                        case ID_MATERIAL_LIBRARY: // TODO: i should store these filenames and then load the materials after the main wavefrontobj is laoded
                            // TODO: the only way to do this i think is to retreive the .mtl names in a seperate function
                            //          and then the caller can load those first and then pass the full instanced material library to this routine

                            // get the filenames
                            fileNames = new string[tokens.Length - 1];
                            for (int i = 0; i < tokens.Length - 1; i++)
                            {
                                fileNames[i] = tokens[i + 1];
                                // open and parse the file
                                // issue with loading these from archive
                                //wavefrontObj.Materials = WaveFrontObjMaterialLibraryLoader.Load(System.IO.Path.Combine(relativePath, fileNames[i]));
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return fileNames;
        }

        private static string ReadLine(string lastLine, StreamReader reader)
        {

            string s  = reader.ReadLine();

            // if the last character is a line continuation character "\" then remove it
            // and append the following line and repeat

            if (s.Contains(@"\"))
            {
                // it should be the last character so we'll cut that last char
                //s = s.Substring(0, s.Length - 1);
                return ReadLine(lastLine + s, reader);
            }
            else
                return (lastLine  + s).Replace (@"\", " "); // replace with space " " because sometimes there is no extra space and the previous face will merge with the next and will be unsplitable
       
        }

        //// This glu32 triangulator isn't used... for now.  We triangulate using fan method in 
        ////WaveFrontObjIndexedFace.Triangulate
        //private static void TriangulatePolygon(ref string[] tokens, WaveFrontObj wavefrontObj)
        //{
        //    Tessellator tess = new Tessellator();
        //    double[] coordinates;
        //    int[] indices;
        //    double[] newVertices;
        //    int[] resultIndices;

        //    // fill the coordinates and indices
        //    indices = new int[tokens.Length - 1];
        //    coordinates = new double[indices.Length*3];
        //    int j = 0;
        //    for (int i = 1; i < tokens.Length; i++)
        //    {
        //        indices[i - 1] = int.Parse(tokens[i]);
        //        coordinates[j++] = wavefrontObj.Points[indices[i - 1]].x;
        //        coordinates[j++] = wavefrontObj.Points[indices[i - 1]].y;
        //        coordinates[j++] = wavefrontObj.Points[indices[i - 1]].z;
        //    }

        //    tess.TessellatePolygon(coordinates, indices, false, out resultIndices, out newVertices);

        //    // total hack method, but here we're going to remap the tokens so the caller can resume as if this never happened
        //    string[] s = new string[resultIndices.Length];

        //    for (int i = 0; i < resultIndices.Length; i ++)
        //    {
        //        s[i] = resultIndices[i].ToString();
        //    }
        //    tokens = s;
        //}
    }
}
