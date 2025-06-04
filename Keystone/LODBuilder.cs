using System;
using System.IO;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace Keystone
{
    public class LODBuilder
    {
        // Sample specific variables
        private ProgressiveMesh[] meshes = null;
        private ProgressiveMesh fullMesh = null;
        private int currentMeshIndex = 0;
        private Material[] meshMaterials = null;
        private Texture[] meshTextures = null;
        private Vector3 objectCenter; // Center of bounding sphere of object
        private float objectRadius; // Radius of bounding sphere of object
        private Matrix worldCenter; // World matrix to center the mesh
        private Device _device;


        public LODBuilder()
        {
        }


        public void Open(string path)
        {
            // Load the mesh
            GraphicsStream adjacencyBuffer = null;
            ExtendedMaterial[] materials = null;
            _device = new Device(CoreClient._CoreClient.Internals.GetDevice3D());

            // Change the current directory to the mesh's directory so we can
            // find the textures.
            string currentFolder = Directory.GetCurrentDirectory();
            FileInfo info = new FileInfo(path);
            Directory.SetCurrentDirectory(info.Directory.FullName);

            using (Mesh originalMesh = Mesh.FromFile(path, MeshFlags.Managed, _device,
                                                     out adjacencyBuffer, out materials))
            {
                int use32Bit = (int) (originalMesh.Options.Value & MeshFlags.Use32Bit);


                // Perform simple cleansing operations on mesh
                using (Mesh mesh = Mesh.Clean(CleanType.Simplification, originalMesh, adjacencyBuffer, adjacencyBuffer))
                {
                    // Perform a weld to try and remove excess vertices.
                    // Weld the mesh using all epsilons of 0.0f.  A small epsilon like 1e-6 works well too
                    WeldEpsilons epsilons = new WeldEpsilons();
                    mesh.WeldVertices(0, epsilons, adjacencyBuffer, adjacencyBuffer);

                    // Verify validity of mesh for simplification
                    mesh.Validate(adjacencyBuffer);

                    // Allocate a material/texture arrays
                    meshMaterials = new Material[materials.Length];
                    meshTextures = new Texture[materials.Length];

                    // Copy the materials and load the textures
                    for (int i = 0; i < meshMaterials.Length; i++)
                    {
                        meshMaterials[i] = materials[i].Material3D;
                        meshMaterials[i].AmbientColor = meshMaterials[i].DiffuseColor;

                        if ((materials[i].TextureFilename != null) && (materials[i].TextureFilename.Length > 0))
                        {
                            // Create the texture
                            //meshTextures[i] =
                            //    ResourceCache.GetGlobalInstance().CreateTextureFromFile(e.Device,
                            //                                                            materials[i].TextureFilename);
                        }
                    }

                    // Find the mesh's center, then generate a centering matrix
                    using (VertexBuffer vb = mesh.VertexBuffer)
                    {
                        using (GraphicsStream stm = vb.Lock(0, 0, LockFlags.NoSystemLock))
                        {
                            try
                            {
                                objectRadius = Geometry.ComputeBoundingSphere(stm,
                                                                              mesh.NumberVertices, mesh.VertexFormat,
                                                                              out objectCenter);

                                worldCenter = Matrix.Translation(-objectCenter);
                                float scaleFactor = 2.0f/objectRadius;
                                worldCenter *= Matrix.Scaling(scaleFactor, scaleFactor, scaleFactor);
                            }
                            finally
                            {
                                vb.Unlock();
                            }
                        }
                    }

                    // If the mesh is missing normals, generate them.
                    Mesh currentMesh = mesh;
                    if ((mesh.VertexFormat & VertexFormats.Normal) == 0)
                    {
                        currentMesh = mesh.Clone(MeshFlags.Managed | (MeshFlags) use32Bit,
                                                 mesh.VertexFormat | VertexFormats.Normal, _device);
                        // TODO: wait, if we are using Normal Map we dont want to do this either
                        // Compute normals now
                        currentMesh.ComputeNormals();
                    }

                    using (currentMesh)
                    {
                        // Generate progressive meshes
                        using (
                            ProgressiveMesh pMesh =
                                new ProgressiveMesh(currentMesh, adjacencyBuffer, null, 1, MeshFlags.SimplifyVertex))
                        {
                            int minVerts = pMesh.MinVertices;
                            int maxVerts = pMesh.MaxVertices;
                            int vertsPerMesh = (maxVerts - minVerts + 10)/10;

                            // How many meshes should be in the array
                            int numMeshes =
                                Math.Max(1, (int) Math.Ceiling((maxVerts - minVerts + 1)/(double) vertsPerMesh));
                            meshes = new ProgressiveMesh[numMeshes];

                            // Clone full sized pmesh
                            fullMesh = pMesh.Clone(MeshFlags.Managed | MeshFlags.VbShare, pMesh.VertexFormat, _device);

                            // Clone all the separate pmeshes
                            for (int iMesh = 0; iMesh < numMeshes; iMesh++)
                            {
                                meshes[iMesh] =
                                    pMesh.Clone(MeshFlags.Managed | MeshFlags.VbShare, pMesh.VertexFormat, _device);

                                // Trim to appropriate space
                                meshes[iMesh].TrimByVertices(minVerts + vertsPerMesh*iMesh,
                                                             minVerts + vertsPerMesh*(iMesh + 1));
                                meshes[iMesh].OptimizeBaseLevelOfDetail(MeshFlags.OptimizeVertexCache);
                            }

                            // Set the current to be max vertices
                            currentMeshIndex = numMeshes - 1;
                            meshes[currentMeshIndex].NumberVertices = maxVerts;
                            fullMesh.NumberVertices = maxVerts;

                            // Set up the slider to reflect the vertices range the mesh has
                            //sampleUi.GetSlider(Detail).SetRange(meshes[0].MinVertices,
                            //                                    meshes[meshes.Length - 1].MaxVertices);
                            //sampleUi.GetSlider(Detail).Value = (meshes[currentMeshIndex] as BaseMesh).NumberVertices;
                        }
                    }
                }
            }
        }

        /// <summary>Sets the number of vertices for the progressive meshes</summary>
        private void SetNumberVertices(int numberVerts)
        {
            // Update the full mesh first
            fullMesh.NumberVertices = numberVerts;

            // If current pm valid for desired value, then set the number of vertices directly
            if ((numberVerts >= meshes[currentMeshIndex].MinVertices) &&
                (numberVerts <= meshes[currentMeshIndex].MaxVertices))
            {
                meshes[currentMeshIndex].NumberVertices = numberVerts;
            }
            else
            {
                // Search for the right one
                currentMeshIndex = meshes.Length - 1;

                // Look for the correct "bin"
                while (currentMeshIndex > 0)
                {
                    // If number of vertices is less than current max then we found one to fit
                    if (numberVerts >= meshes[currentMeshIndex].MinVertices)
                        break;

                    currentMeshIndex--;
                }

                // Set the vertices on the newly selected mesh
                meshes[currentMeshIndex].NumberVertices = numberVerts;
            }
        }

        //public void OnFrameRender(Device device, double appTime, double elapsedTime)
        //{
        //    bool beginSceneCalled = false;

        //    // Clear the render target and the zbuffer 
        //    device.Clear(ClearFlags.ZBuffer | ClearFlags.Target, 0x00424B79, 1.0f, 0);
        //    try
        //    {
        //        device.BeginScene();
        //        beginSceneCalled = true;

        //        // Get the world matrix
        //        Matrix worldMatrix = worldCenter*camera.WorldMatrix;

        //        if ((meshes != null) && (meshes.Length > 0))
        //        {
        //            // Update the effect's variables.  Instead of using strings, it would 
        //            // be more efficient to cache a handle to the parameter by calling 
        //            // Effect.GetParameter
        //            effect.SetValue("g_mWorldViewProjection", worldMatrix*camera.ViewMatrix*camera.ProjectionMatrix);
        //            effect.SetValue("g_mWorld", worldMatrix);

        //            // Set and draw each of the materials in the mesh
        //            for (int i = 0; i < meshMaterials.Length; i++)
        //            {
        //                effect.SetValue("g_vDiffuse", meshMaterials[i].DiffuseColor);
        //                effect.SetValue("g_txScene", meshTextures[i]);
        //                int passes = effect.Begin(0);
        //                for (int pass = 0; pass < passes; pass++)
        //                {
        //                    effect.BeginPass(pass);
        //                    if (isShowingOptimized)
        //                    {
        //                        meshes[currentMeshIndex].DrawSubset(i);
        //                    }
        //                    else
        //                    {
        //                        fullMesh.DrawSubset(i);
        //                    }
        //                    effect.EndPass();
        //                }
        //                effect.End();
        //            }
        //        }
        //        // Show frame rate
        //        RenderText();

        //        // Show UI
        //        hud.OnRender(elapsedTime);
        //        sampleUi.OnRender(elapsedTime);
        //    }
        //    finally
        //    {
        //        if (beginSceneCalled)
        //            device.EndScene();
        //    }
        //}
    }
}