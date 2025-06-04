using System;
using System.Diagnostics;
using System.IO;
using MTV3D65;


namespace Keystone.Loaders
{
    /// <summary>
    /// Author: Hypnotron
    /// Date: October 10, 2009
    /// License: None.  This code is placed in the Public Domain.  (be nice if you kept at least these 3 lines ) 
    /// 
    /// Description:
    /// A preliminary implemention of a TVM file loader.  Some advantages over TV3D's own loader
    /// is better control\tracking of materials and texture resrouces that you load and
    /// ability to handle >32bit worth of user data in the MUSR section. 
    /// 
    /// Issues:
    /// - A significant performance issue in the implementation below (an easy fix i'll leave to you)
    /// is how many seperate BinaryReader.Read() calls there are.  It
    /// would be much faster to read a big chunk of the file all at once, and then do processing 
    /// on that array.
    /// - There are 4 .TVM file versions.  Geometry loading is 100%working for all 4 version.  
    /// Material and Texture loading  is only 95% implemented for the newest .TVM version 4.0.  
    /// The other's ill try to add later or  someone else can.  That's what open src is about.
    /// 
    /// In any case, the TVM format is (mostly) spelled out below so you're now able to 
    /// write your own plugins for importers/exporters with this knowledge.
    /// 
    /// For format BYTE OFFSETS help see.     http://www.makosoft.com/stuff/tvm_format.tx
    /// keep in mind that the sections aren't necessarily
    /// in the order shown but can have some variation.  This loader handles any such variation.
    ///
    /// </summary>
    public class TruevisionMeshLoader
    {
        private const int FILE_MAGIC = 0x18644421;// decimal - 409224225;
        private const string MESH_FORMAT = "MFOR";
        private const string MESH_STRINGTABLE = "MSTR";
        private const string MESH_STATS = "MSTA";
        private const string MESH_VERTICES = "MVER";
        private const string MESH_INDICES16 = "MI16";
        private const string MESH_INDICES32 = "MI32";
        private const string MESH_TRIANGLE_GROUP_IDS = "MATT";
        private const string MESH_GROUPS = "MGRO";
        // MGRP thru MGR4 reflect the revisions tv exporters have gone through.
        // MGRP and MGR2 versions of a group's material/texture data didn't use a string table
        // MGR3 and  the most current version MGR4, does use a string table
        // If you want to write an exporter, you should only implement MGR4
        private const string MESH_MGRP = "MGRP"; // 1.0
        private const string MESH_MGR2 = "MGR2"; 
        private const string MESH_MGR3 = "MGR3"; // started using a string table
        private const string MESH_MGR4 = "MGR4"; // same as MGR3 but added 8 texture layers up from 4
        
        private const string MESH_BOUNDS = "MBOU";
        private const string MESH_USERDATA = "MUSR";
        private const string MESH_END = "MEND";

        private struct GroupAttributes
        {
            public uint TriangleCount;
            public uint Material;
            public uint Texture;
            public uint VertexCount;
        }

        private struct GroupAppearance
        {
            public int GroupNameID; 
            public int[] TextureFactoryNameID;
            public int[] TextureFilenameID;
            public CONST_TV_LAYERMODE[] LayerMode;
            public bool[] layerEnabled;
            public bool[] isShader;
            public int[] iTechnique;

            public TV_COLOR diffuse;
            public TV_COLOR ambient;
            public TV_COLOR specular;
            public TV_COLOR emissive;
            public float power;
        }

        public static TVMesh Load(string fullpath, bool loadTextures, bool loadMaterials)
        {
            return Load(fullpath, loadTextures, loadMaterials, false);
        }

        /// <summary>
        /// </summary>
        /// <param name="fullpath"></param>
        /// <param name="loadTextures"></param>
        /// <param name="loadMaterials"></param>
        /// <param name="optimize"></param>
        /// <returns></returns>
        public static TVMesh Load(string fullpath, bool loadTextures, bool loadMaterials, bool optimize)
        {
            FileStream fs = null;
            BinaryReader reader = null;
            TVMesh mesh = null;
            string[] stringTable = null;
            int strideInBytes = 0;
            TV_VERTEXELEMENT[] velements = null;
            uint triangleCount = 0;
            uint groupCount = 0;
            uint vertexCount = 0;
            float[] vertices = null;
            int[] indices = null;
            int[] faceGroupIDs = null;
            byte[] userData;
            string path = Path.GetDirectoryName(fullpath);
            GroupAppearance[] groupAppearance = null;
            GroupAttributes[] groupAttribs = null;
            bool isMGR4 = false;

            int[] materials;
            int[] textures;

            bool done = false;
            string currentSection = "";


            try
            {
                fs = new FileStream(fullpath, FileMode.Open);
                reader = new BinaryReader(fs);

                // header
                int magic = reader.ReadInt32();
                if (magic != FILE_MAGIC)
                {
                    Trace.WriteLine("Invalid TVMesh file");
                    return null;
                }
                uint size = reader.ReadUInt32();  // remaining size of the file after this point
                reader.ReadUInt32(); // ?
                reader.ReadUInt32(); // ?

                
                // main body parsing
                while (!done)
                {
                    // mesh format section
                    char[] fourcc = reader.ReadChars(4);  
                    uint sectionLength = reader.ReadUInt32();
                    long sectionStartPosition = fs.Position;
                    currentSection = new string(fourcc);
                    switch (currentSection)
                    {
                        case MESH_FORMAT:
                            reader.ReadInt16(); // anyone figure out what this is?
                            uint elementCount = (sectionLength - 8)/8;
                            velements = ReadVertexElements(reader, elementCount);
                            reader.ReadInt32(); // anyone figure out what this is?
                            reader.ReadInt16(); // anyone figure out what this is?
                            break;
                        case MESH_STRINGTABLE: 
                            uint stringTableOffset = reader.ReadUInt32(); // offset in file where string table starts.
                            // seek to the location of the string table
                            fs.Seek(stringTableOffset, SeekOrigin.Begin);
                            stringTable = ReadStrings(reader);
                             // must seek back to reader position prior to jumping over to the string table
                            fs.Seek(sectionStartPosition + 4, SeekOrigin.Begin);
                            break;
                        case MESH_STATS :
                            triangleCount = reader.ReadUInt32();
                            groupCount = reader.ReadUInt32();
                            vertexCount = reader.ReadUInt32();
                            reader.ReadUInt32(); // anyone figure out what this is?
                            reader.ReadUInt32(); // anyone figure out what this is?
                            reader.ReadUInt32(); // anyone figure out what this is?
                            strideInBytes = reader.ReadInt32();
                            break;
                        case MESH_VERTICES :
                            Trace.Assert(sectionLength / strideInBytes == vertexCount);
                            // the verts various elements are packed as described in mesh vertex elements
                            vertices = ReadVertices(reader, velements, vertexCount, strideInBytes);
                            break;
                        case MESH_INDICES16 :
                            uint indicesCount = sectionLength / 2; 
                            indices = ReadIndices16(reader, indicesCount);
                            break;
                        case MESH_INDICES32:
                            indicesCount = sectionLength / 4; 
                            indices = ReadIndices32(reader, indicesCount);
                            break;
                        case MESH_TRIANGLE_GROUP_IDS :
                            Trace.Assert(sectionLength/4 == triangleCount);
                            faceGroupIDs = ReadFaceGroups(reader, sectionLength/4);
                            break;
                        case MESH_GROUPS :
                            groupAttribs = new GroupAttributes[groupCount];
                            for (int i = 0; i < groupCount; i++)
                            {
                                groupAttribs[i].VertexCount = reader.ReadUInt32();
                                groupAttribs[i].Material= reader.ReadUInt32(); // ? guessing for now. perhaps a material index
                                groupAttribs[i].TriangleCount = reader.ReadUInt32();
                                groupAttribs[i].Texture = reader.ReadUInt32(); // ? guessing for now. perhaps a texture index
                            }
                            break;
                        case MESH_MGRP: // each entry is 416 bytes long
                            //int matcount = (int)sectionLength/416; 
                            //string[] name = new string[matcount]; //64 bytes for each name
                            //string[] path = new string[matcount]; // 64 bytes x4 texture layers
                            
                            //materials = new TV_COLOR[matcount,4];

                            //// 68 bytes for each material
                            //for (int i =0; i < matcount; i++)
                            //{
                            //    name[i] = new string(reader.ReadChars(64));
                            //    path[i] = new string((reader.ReadChars(284)));
                            //    // diffuse
                            //    materials[i, 0].r = reader.ReadSingle();
                            //    materials[i, 0].g = reader.ReadSingle();
                            //    materials[i, 0].b = reader.ReadSingle();
                            //    materials[i, 0].a = reader.ReadSingle();
                            //    // ambient
                            //    materials[i, 1].r = reader.ReadSingle();
                            //    materials[i, 1].g = reader.ReadSingle();
                            //    materials[i, 1].b = reader.ReadSingle();
                            //    materials[i, 1].a = reader.ReadSingle();
                            //    // specular
                            //    materials[i, 2].r = reader.ReadSingle();
                            //    materials[i, 2].g = reader.ReadSingle();
                            //    materials[i, 2].b = reader.ReadSingle();
                            //    materials[i, 2].a = reader.ReadSingle();
                            //    // emissive
                            //    materials[i, 3].r = reader.ReadSingle();
                            //    materials[i, 3].g = reader.ReadSingle();
                            //    materials[i, 3].b = reader.ReadSingle();
                            //    materials[i, 3].a = reader.ReadSingle();
                            //}
                            break;
                        case MESH_MGR2: // TODO: no textures and materials in 2.0 TVM's without this
                            break;
                        case MESH_MGR3:// 136 fixed bytes per group TODO: no textures and materials in 3.0 TVM's without this
                            break;
                        case MESH_MGR4: // 172 fixed bytes per group
                             groupAppearance = ReadGroupAppearanceData4(reader, groupCount);
                            isMGR4 = true;
                            break;
                        case MESH_BOUNDS : 
                            TV_3DVECTOR min, max, center;
                            min.x = reader.ReadSingle();
                            min.y = reader.ReadSingle();
                            min.z = reader.ReadSingle();
                            max.x = reader.ReadSingle();
                            max.y = reader.ReadSingle();
                            max.z = reader.ReadSingle();
                            center.x = reader.ReadSingle();
                            center.y = reader.ReadSingle();
                            center.z = reader.ReadSingle();
                            break;
                        case MESH_USERDATA: // just user data?  
                            //   mesh.SetUserData() only stores a 32bit value (e.g. a pointer)
                            // but potentially .tvm could host any kind of data in this block up to the size specified in the sectionLength
                            userData = reader.ReadBytes((int)sectionLength);
                            break;
                        case MESH_END :
                            done = true;
                            break;
                        default:
                            Trace.WriteLine("Unsupported section name '" + currentSection + '"');
                            //ignore tags we dont recognize and resume
                            break;
                    }

                    // if we havent reached the end of the section for whatever reason, seek to start of next fourcc (next section)
                    int bytesRead = (int)(fs.Position - sectionStartPosition);
                    if (bytesRead < sectionLength)
                    {
                        reader.ReadBytes((int) sectionLength - bytesRead);
                        // log the fact that we unexpectedly had more data left over in this section 
                        Trace.WriteLine("Unexpected '" + ((int)sectionLength - bytesRead) + "' bytes of left over data in section '" + currentSection + "'");
                    }
                }
                
                // all done
                reader.Close();
                fs.Close();

                // finally create the mesh
                mesh = CoreClient._CoreClient.Scene.CreateMeshBuilder(fullpath);
                mesh.SetMeshFormatEx(velements, velements.Length);
                int faceCount = indices.Length/3;
                // TODO: I'm almost positive the vertices.Length below should be (uint)vertexCount 
                mesh.SetGeometryEx(vertices, strideInBytes, (int)vertexCount, indices, faceCount, (int)groupCount, faceGroupIDs, optimize);
                
                // load materials and textures.  This currently only works for MGR4 and not MGRP - MGR3.  
                if (isMGR4)
                {
                    for (int i = 0; i < groupCount; i++)
                    {
                        // assign group name
                        string groupName = stringTable[groupAppearance[i].GroupNameID];
                        mesh.SetGroupName(i, groupName);

                        if (groupAttribs[i].Material >= 0)
                        {
                            int matid = CoreClient._CoreClient.MaterialFactory.CreateMaterial();
                            TV_COLOR color = groupAppearance[i].diffuse;
                            CoreClient._CoreClient.MaterialFactory.SetDiffuse(matid, color.r, color.g, color.b, color.a);
                            color = groupAppearance[i].ambient;
                            CoreClient._CoreClient.MaterialFactory.SetAmbient(matid, color.r, color.g, color.b, color.a);
                            color = groupAppearance[i].specular;
                            CoreClient._CoreClient.MaterialFactory.SetSpecular(matid, color.r, color.g, color.b, color.a);
                            color = groupAppearance[i].emissive;
                            CoreClient._CoreClient.MaterialFactory.SetEmissive(matid, color.r, color.g, color.b, color.a);
                            mesh.SetMaterial(matid, i);
                        }
                        if (groupAttribs[i].Texture >= 0)
                        {
                            int texid;
                            for (int j = 0; j < 8; j++)
                            {
                                if (groupAppearance[i].TextureFilenameID[j] >= 0)
                                {
                                    string filename = Path.Combine(path,
                                                                   stringTable[groupAppearance[i].TextureFilenameID[j]]);
                                    texid =
                                        CoreClient._CoreClient.TextureFactory.LoadTexture(filename);

                                    mesh.SetTextureEx(j, texid, i);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // perhaps unexpected end of file before done = true
                // however if the error occurred after CreateMeshBuilder, loading of materials, textures and SetGeometryEx()
                // we should clean up after ourselves and unload everything
                // TODO:
                Trace.WriteLine(ex.Message);
                return null;
            }
            finally
            {
                if (reader != null) reader.Close();
                if (fs != null) fs.Close();
            }

            return mesh;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static string[] ReadStrings (BinaryReader reader)
        {
            int numStrings = reader.ReadInt32();
            string[] results = new string[numStrings];
            int totalSizeOfAllStrings = reader.ReadInt32(); // includes all null terminators
            int mystery = reader.ReadInt32(); // ? this value is usually 12.  not sure what it is, but i can get strings without it.  

            // next is an alternating series of "offsets" and "lengths" 
            int[] offsets = new int[numStrings];
            int[] lengths = new int[numStrings];
            for (int i = 0; i < numStrings; i++)
            {
                offsets[i] = reader.ReadInt32();
                lengths[i] = reader.ReadInt32();
            }

            // it's odd that we need offsets and lengths because we know how many and the strings are packed end to end
            // seperated with null terminators
            for (int i = 0; i < numStrings; i++ )
            {
                char[] tmp = reader.ReadChars(lengths[i]);
                results[i] = new string(tmp);
                byte terminator = reader.ReadByte();
                Trace.Assert(terminator == 0); 
            }

            return results;
        }

        /// <summary>
        /// Assumes that the reader is already in position to read this section
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="elementCount"></param>
        /// <returns></returns>
        private static TV_VERTEXELEMENT[] ReadVertexElements (BinaryReader reader, uint elementCount)
        {
            TV_VERTEXELEMENT[] results = new TV_VERTEXELEMENT[elementCount];
            for (int i = 0 ; i < elementCount; i++)
            {
                int offset = reader.ReadInt16();
                results[i].element = reader.ReadInt16();
                results[i].usage = reader.ReadInt16();
                results[i].stream = reader.ReadInt16();
            }

            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="velements"></param>
        /// <param name="vertexCount"></param>
        /// <param name="strideInBytes"></param>
        /// <returns></returns>
        private static float[] ReadVertices(BinaryReader reader, TV_VERTEXELEMENT[] velements, uint vertexCount, int strideInBytes)
        {
            float[] results = new float[vertexCount * strideInBytes / 4];
            byte[] data = new byte[vertexCount * strideInBytes];
            int bytesToRead = 0;
            int position = 0;
            for (int i = 0; i < vertexCount; i++ )
            {
                for (int j = 0; j < velements.Length ; j++)
                {
                    switch (velements[j].element)
                    {
                        case (int)CONST_TV_ELEMENTTYPE.TV_ELEMENT_longCOLOR:
                        case (int)CONST_TV_ELEMENTTYPE.TV_ELEMENT_SHORT2:
                        case (int)CONST_TV_ELEMENTTYPE.TV_ELEMENT_BYTE4:
                        case (int)CONST_TV_ELEMENTTYPE.TV_ELEMENT_FLOAT1:
                        case (int)CONST_TV_ELEMENTTYPE.TV_ELEMENT_FLOAT2_HALF:
                            bytesToRead = 4;
                            break;
                        case (int)CONST_TV_ELEMENTTYPE.TV_ELEMENT_SHORT4:
                        case (int)CONST_TV_ELEMENTTYPE.TV_ELEMENT_FLOAT2:
                        case (int)CONST_TV_ELEMENTTYPE.TV_ELEMENT_FLOAT4_HALF:
                            bytesToRead = 8;
                            break;
                        case (int)CONST_TV_ELEMENTTYPE.TV_ELEMENT_FLOAT3:
                            bytesToRead = 12;
                            break;
                        case (int)CONST_TV_ELEMENTTYPE.TV_ELEMENT_FLOAT4:
                            bytesToRead = 16;
                            break;
                      
                    }
                    reader.Read(data, position, bytesToRead);
                    position += bytesToRead;
                }
            }

            Buffer.BlockCopy(data,0, results,0, data.Length);
            return results;
        }

        private static int[] ReadIndices16(BinaryReader reader, uint indicesCount)
        {
            int[] results = new int[indicesCount];
            for (int i = 0; i < indicesCount; i++)
                results[i] = reader.ReadInt16();

            return results;
        }

        private static int[] ReadIndices32(BinaryReader reader, uint indicesCount)
        {
            int[] results = new int[indicesCount];
            for (int i = 0; i < indicesCount; i++)
                results[i] = reader.ReadInt32();

            return results;
        }

        private static int[] ReadFaceGroups(BinaryReader reader, uint faceCount)
        {
            int[] results = new int[faceCount];
            for (int i = 0; i < faceCount; i++)
                results[i] = reader.ReadInt32();

            return results;
        }

        private static GroupAppearance[] ReadGroupAppearanceData4(BinaryReader reader, uint groupCount)
        {
            GroupAppearance[] results = new GroupAppearance[ groupCount];
            const int maxLayers =8;
            const int TO_BASE_ZERO = 1; // the string table lookups in TVM are 1 base.  We subtract 1 to rebase to 0
            for (int i = 0; i < groupCount; i ++ )
            {
                results[i].GroupNameID = reader.ReadInt32() - TO_BASE_ZERO;
                results[i].TextureFilenameID = new int[maxLayers];
                for (int j = 0; j < maxLayers; j++)
                {
                    results[i].TextureFilenameID[j] = reader.ReadInt32() - TO_BASE_ZERO; // this might be just 16 bit followed by another 16bit for textureNameID
                }

                // there's 48 bytes of other stuff here i dont know exactly to deal with yet.  
                // it has to do with layermode, layerenable, isShader, technique... but  we'll just skip it
                reader.ReadBytes(48); //TODO: fix this

                results[i].diffuse.r = reader.ReadSingle();
                results[i].diffuse.g = reader.ReadSingle();
                results[i].diffuse.b = reader.ReadSingle();
                results[i].diffuse.a = reader.ReadSingle();
                results[i].ambient.r = reader.ReadSingle();
                results[i].ambient.g = reader.ReadSingle();
                results[i].ambient.b = reader.ReadSingle();
                results[i].ambient.a = reader.ReadSingle();
                results[i].specular.r = reader.ReadSingle();
                results[i].specular.g = reader.ReadSingle();
                results[i].specular.b = reader.ReadSingle();
                results[i].specular.a = reader.ReadSingle();
                results[i].emissive.r = reader.ReadSingle();
                results[i].emissive.g = reader.ReadSingle();
                results[i].emissive.b = reader.ReadSingle();
                results[i].emissive.a = reader.ReadSingle();

                results[i].power = reader.ReadSingle();
            }

                return results;
        }

        /// <summary>
        /// TODO: export tvm 
        /// note: caller must verify existing mesh at path is safe to overrite if applicable 
        /// </summary>
        /// <param name="fullpath"></param>
        public static void Save(string fullpath)
        {
            FileStream fs = null;
            BinaryWriter writer = null;

            try
            {
                fs = File.Create(fullpath);
                writer = new BinaryWriter(fs);

                writer.Write(FILE_MAGIC);

                // TODO: implement rest!

                writer.Flush();
            }
            catch (Exception ex)
            {
                // TODO: need proper error handling
                Trace.WriteLine(ex.Message);
            }
            finally
            {
                if (writer != null) writer.Close();
                if (fs != null) fs.Close();
            }
        }

        private void WriteVertices()
        {
        }

        private void WriteIndices()
        {
        }

        private void WriteStringTable()
        {
        }

        private void WriteMaterials()
        {
        }
    }
}

