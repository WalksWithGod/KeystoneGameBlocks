using System;
using System.Collections.Generic;
using System.Diagnostics;
using KeyCommon.Traversal;
using Keystone.Appearance;
using Keystone.Collision;
using Keystone.Culling;
using Keystone.IO;
using Keystone.Shaders;
using Keystone.Traversers;
using Keystone.Types;
using MTV3D65;

namespace Keystone.Elements
{

    public class Terrain : Geometry
    {
        private TVLandscape _landscape;
        private string mHeightmap;
        
        private bool _splattingEnabled;
        private bool _clampingEnabled; // clamping enable might be required to fix seams between chunks

        // TODO: how is the originOffset impacted by scale?  Well probably not at all since it's local BoundingBox and scale is applied via Model or Entity   
        private TV_3DVECTOR mOriginOffset;
        
        private int _chunksX;
        private int _chunksZ;
        
        private Chunk[] _chunks;
        protected CONST_TV_LANDSCAPE_AFFINE _affine;
        private CONST_TV_DETAILMAP_MODE _detailMode;
        private CONST_TV_LANDSCAPE_PRECISION _precision;
        
        // protected bool _collisionEnabled; //TODO: maybe add PickingEnabled, PickPreview, etc
        protected CONST_TV_BLENDINGMODE _blendingMode;

        public ConvexHull Hull;

        #region LandscapeFAQ

            // TODO: NEED LOD FOR LANDSCAPE TOO.  We'll use a LODSwitch but we need a good automatic way
            // to switch between really far away low LOD landscapes... and frankly, to be able to use low sampled versions of textures and heightmpas
            // that would be better than having to create actual LOD versions... but we might be able to do that on the fly or in th eeditor.

            //TODO: read my notes in Lux terain to remind which of these affine and precision and such and any others MUST be set prior to generating 
            // in order to take effect
            // store the precision, chunk height and width and other read only data

            // create our chunks and assign them to the proper array subscript
            //http://www.truevision3d.com/phpBB2/viewtopic.php?t=7555
            //Land.SetAffineDetail = TV_AFFINE_LOW ' = 1
            //tWidth=2  - Width of the terrain in CHUNKS
            //tLength=3 - Length of the terrain in CHUNKS
            //tPosX=0   - Translation of terrain (useful for multiple terrain or paged terrain systems)
            //tPosZ=0   - Translation of terrain (useful for multiple terrain or paged terrain systems)
            //tAffine=true - Apply smoothing
            //Land.GenerateTerrain “Heightmap.bmp”, TV_Precision_Best, tWidth, tLength, tPosX, tPosZ, tAffine

            //=> This will generate a highly detailed (but smoothed) terrain that is 512 x 768 world units in size.

            //Land.SetAffineDetail - defines the smoothing amount and the options are:

            //TV_AFFINE_HIGH = 2
            //TV_AFFINE_LOW = 1
            //TV_AFFINE_NO = 0

            //The default setting is
            //Land.SetAffineDetail = TV_AFFINE_LOW

            
            //TV_PRECISION_ULTRA= 2
            //TV_PRECISION_BEST = 4
            //TV_PRECISION_HIGH = 8
            //TV_PRECISION_AVERAGE = 16
            //TV_PRECISION_LOW = 32
            //TV_PRECISION_VERY_LOW = 64
            //TV_PRECISION_ULTRA_LOW = 128
            
            //It is important to note that there is a relationship between the vertices of the mesh and the world unit. 
            //This relationship is the Landscape Precision. In the example above the constant TV_Precision_Best = 4.
            //Thus each vertex is 4 units apart.  Thus it should be noted that no matter the precision, the chunk size in
            // world units is always 256 x 256 units unless their is scaling applied.

            //The number of vertices on one side of a CHUNK is: 256 / TV_Precision_Best = 256 / 4 = 64 


			
            //To represent every HeightMap pixel fully in the terrain we would need to map one pixel to one vertex and so make a terrain that is sized like this:
			// (*NOTE: we may actually want to represent each pixel by more than 1 vert (eg 4 verts) so that we get more ability to smooth the results!)
			
            //If the HeightMap is 512 x 768 in size and we are using TV_Precision_Best:
            //chunksX = (512 * TV_Precision_Best) / 256 = (512 * 4) / 256 = 8
            //chunksZ = (768 * TV_Precision_Best) / 256 = (768* 4) / 256 = 12

            // note: MPJ - I think the above is expressed strangly.  But it works out the same as the following
            // chunksX = 512 / 256 / TV_PRECISION_BEST = 512 / 256 / 4 = 512 / 64 = 8
            // chunksY = 768 / 256 / TV_PRECISION_BEST = 768 / 256 / 4 = 768 / 64 = 12

            // NOTE: The best verts per side we can get is 128 using ULTRA.

            // Recall that since each chunk is 256x256 units along x and z, 
            // then with normal 1,1,1 scaling, an TV_PRECISION_ULTRA chunk with 128 vertices or a TV_PRECISION_ULTRA_LOW chunk with 2 vertices
            // will both still be 256x256 world units in dimension.

            // Thus, when using programs like L3DT, make sure you understand how it's own resolutions settings will fit in with
            // tv3d's landscaping.

            // WorldWidth = chunksX * 256 * scaleX;
            // HeightMapMetersPerPixelX = WorldWidth / HeightmapWidth

            // So, if you have a 1024 x 1024 heightmap and you load it with Ultra precision and 8 chunks x 8 chunks to have 1:1 pixel to vertex resolution
            // you'll end up with a 2048 x 2048 world size
            // aka: pixel resolution = 2meters
            // THis is very important to understand because if you intended for your 1024 x 1024 heightmap to use 1:1 pixel to vertex resolution, 
            // AND ALSO have a resolution of 1 meters, than you have to apply a scaling of
            // scaleX = 1 / HeightMapMetersPerPixelX
            // However, the normal idea is probably to not use as many chunks.   
            //
            //Land.GenerateHugeTerrain “Heightmap.bmp”, TV_Precision_Best, chunksX, chunksZ, tPosX, tPosZ, tAffine 

    #endregion

        internal Terrain(string name) : base (name)
        {
        	// number of chunks can never be less than 1 in either dimension	
           	_chunksX = 1;
           	_chunksZ = 1;
           	
           	_precision = CONST_TV_LANDSCAPE_PRECISION.TV_PRECISION_HIGH;
			_affine = CONST_TV_LANDSCAPE_AFFINE.TV_AFFINE_HIGH; // high affine seems to cause exception when loading some heightmap image files

        }

        #region ITraversable
        public override object Traverse(ITraverser target, object data)
        {
            return target.Apply(this, data);
        }
        #endregion
        
        public static Terrain Create (string name)
        {
            Terrain t;
            t = (Terrain)Keystone.Resource.Repository.Create(name, "Terrain");
            // TODO: in the future, maybe terrain is not an entity but a model as are floors in a celledregion.
            //       this way we can model the underlying geometry with mesh3d or tvlandscape 
            if (t == null) throw new Exception("Terrain cannot be null.");
            return t;
        }
        
        #region IResource Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="specOnly">True returns the properties without any values assigned</param>
        /// <returns></returns>
        public override Settings.PropertySpec[] GetProperties(bool specOnly)
        {
            Settings.PropertySpec[] tmp = base.GetProperties(specOnly);
            Settings.PropertySpec[] properties = new Settings.PropertySpec[11 + tmp.Length];
            tmp.CopyTo(properties, 11);

            
            // TODO: _id is the relative path to tv data format for terrain
            //       if _id path does not exist, and heightmap path does, the heightmap will be loaded instead.
            
            properties[0] = new Settings.PropertySpec("heightmap", typeof(string).Name);
            properties[1] = new Settings.PropertySpec("chunksx", _chunksX.GetType().Name);
            properties[2] = new Settings.PropertySpec("chunksz", _chunksZ.GetType().Name);
            properties[3] = new Settings.PropertySpec("precision", typeof(int).Name);
			properties[4] = new Settings.PropertySpec("affine", typeof(int).Name);  //affine does not get saved in terrain data.  really not useful accept just to know after the fact.  
			properties[5] = new Settings.PropertySpec("slodenable", mStandardLODEnable.GetType().Name);
			properties[6] = new Settings.PropertySpec("slodinterval", mLODSwitchInterval.GetType().Name);
			properties[7] = new Settings.PropertySpec("slodminprecision", typeof(int).Name);
			properties[8] = new Settings.PropertySpec("slodstart", mLODSwitchStartDistance.GetType().Name);
			properties[9] = new Settings.PropertySpec("sloddiscardalt", mLODDiscardAltitudeInDistanceTests.GetType().Name);
			properties[10] = new Settings.PropertySpec("plodenable", mLODProgressiveEnable.GetType().Name);

            if (!specOnly)
            {
            	properties[0].DefaultValue = mHeightmap;
                properties[1].DefaultValue = _chunksX;
                properties[2].DefaultValue = _chunksZ;
                properties[3].DefaultValue = (int)_precision;
                properties[4].DefaultValue = (int)_affine;
                properties[5].DefaultValue = mStandardLODEnable;
                properties[6].DefaultValue = mLODSwitchInterval;
                properties[7].DefaultValue = (int)mLODMinimumPrecision;
                properties[8].DefaultValue = mLODSwitchStartDistance;
                properties[9].DefaultValue = mLODDiscardAltitudeInDistanceTests;
                properties[10].DefaultValue = mLODProgressiveEnable;
            }
            
            //XmlHelper.CreateAttribute(xmlnode, "detailmode", terrain.DetailMode.ToString());

            // the following do not need to be stored/restored from XML because it's done by the Appearance node
            //_splattingEnabled;  // set by Appearance node's application
            //_clampingEnabled;   // set by Appearance node's application // TODO: i dont think clamping is a var that has been added to Appearance yet
            // 
      
            return properties;
        }

        public override void SetProperties(Settings.PropertySpec[] properties)
        {
            if (properties == null) return;
            base.SetProperties(properties);

            for (int i = 0; i < properties.Length; i++)
            {
            	if (properties[i].DefaultValue == null) continue ;
            	
                // use of a switch allows us to pass in all or a few of the propspecs depending
                // on whether we're loading from xml or changing a single property via server directive
                switch (properties[i].Name)
                {
            		case "heightmap":
                        mHeightmap = (string)properties[i].DefaultValue;
                        break;
                    case "chunksx":
                        _chunksX = (int)properties[i].DefaultValue;
                        break;
                    case "chunksz":
                        _chunksZ = (int)properties[i].DefaultValue;
                        break;
                    case "precision":
                        _precision = (CONST_TV_LANDSCAPE_PRECISION)(int)properties[i].DefaultValue;
                        break;
                    case "affine":
                        _affine = (CONST_TV_LANDSCAPE_AFFINE)(int)properties[i].DefaultValue;
                        break;
                       
                       case "slodenable":
						mStandardLODEnable = (bool)properties[i].DefaultValue;						
                        break;
					case "slodinterval":
                        mLODSwitchInterval = (float)properties[i].DefaultValue;
						break;
                    case "slodminprecision":
                        mLODMinimumPrecision = (CONST_TV_LANDSCAPE_PRECISION)(int)properties[i].DefaultValue;
                        break;
                   case "slodstart":
                        mLODSwitchStartDistance = (float)properties[i].DefaultValue;
						break;
					case "sloddiscardalt":
                        mLODDiscardAltitudeInDistanceTests = (bool)properties[i].DefaultValue;
						break;
					case "plodenable":
						mLODProgressiveEnable = (bool)properties[i].DefaultValue;
						break;
                }
            }
        }
#endregion

        // OBSOLETE - This should never be allowed.  User must create a new TVLandscape by passing 
        //            heightmap, saved terrain data file, or new empty but with a save path 
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="landscape">Instanced TVLandscape object</param>
//        /// <param name="id">Unique tvobject name</param>
//        private Terrain(TVLandscape landscape, string id, CONST_TV_LANDSCAPE_AFFINE affine)
//            : this(id)
//        {
//            if (landscape == null) throw new ArgumentNullException();
//
//            _landscape = landscape;
//            SetChangeFlags(Enums.ChangeStates.All, Enums.ChangeSource.Self);
//            _tvfactoryIndex = landscape.GetIndex();
//            _affine = affine; // affine is not stored in the terrain data
//
//            // the following 6 values are already contained within the generated landscape            
//            // 		Position = _landscape.GetPosition();
//            //      Scale = _landscape.GetScale();
//            //      Rotation = _landscape.GetRotation();
//            _precision = _landscape.GetPrecision();
//            _chunkWidth = _landscape.GetLandWidth();
//            _chunkHeight = _landscape.GetLandHeight();
//
//            //create our chunk elements
//            _chunks = new Chunk[_chunkWidth*_chunkHeight];
//            for (int i = 0; i < _chunkWidth; i++)
//            {
//                for (int j = 0; j < _chunkHeight; j++)
//                {
//                    _chunks[i*_chunkWidth + j] = new Chunk(id + "_x" + i + "z" + j, this, i, j);
//                }
//            }
//        }


        #region IPageableTVNode Members
        public override void UnloadTVResource()
        {
        	DisposeUnmanagedResources();
        }
                	
        public override void LoadTVResource()
        {
        	
            if (_landscape != null) _landscape.Destroy();
            
       
			CONST_TV_LANDSCAPE_HEIGHTMAP heightMapType = CONST_TV_LANDSCAPE_HEIGHTMAP.TV_HEIGHTMAP_RED;
						
            // "_id" is always a terrain data path location whether the file exists (i.e. terrain is saved to disk) or not.
            // if the file does not exist, 
            //  a) if heightmap does NOT exist, an empty terrain is loaded
            //	b) if the heightmap exists, it is loaded
            // The terrain is not saved to disk automatically.  It requires bool "save terrain data" to be set 
            		
            // tv3d native terrain data file exists?
            string dir = System.IO.Path.Combine (Core._Core.ModsPath, "terrain");
            string path = System.IO.Path.Combine (dir, _id);
            bool nativeDataExists = System.IO.File.Exists (path);
            
            // heightmap image file exists?
            if (string.IsNullOrEmpty (mHeightmap) == false)
	            path = System.IO.Path.Combine (dir, mHeightmap);
    
            bool heightMapExists = System.IO.File.Exists (path);
            	
            
			// TODO: following scale flat out does not work because we set the matrix on Terrain prior to render
			// which resets scale to 1, 1, 1!  So we must set scale on the ModeledEntity that hosts Terrain Geometry.
			Vector3d scale = new Vector3d (1, 1,1);
			
            if (nativeDataExists)
            {	            	
            	_landscape = Import (_id, _affine);
            }
            else if (heightMapExists)
            {
            	_landscape = Import (_id, mHeightmap, _precision, 
            	                     				  heightMapType, 
            	                     				  _affine, 
            	                     				  Vector3d.Zero(), scale , true);
            	
            }
            // create an empty terrain
            else
            {
            	// cannot create terrain with 0 x 0 chunks.  Must be at least 1x1
                // NOTE: never use _landscape = new TVLandscape().  Must use Scene.CreateLandscape()
	            _landscape = CoreClient._CoreClient.Scene.CreateLandscape(_id);
	            
	            // do not allow collision or rendering until terrain has been loaded or else AccessViolations may occur in TVEngine
	            _landscape.SetCollisionEnable (false);
                _landscape.Enable (false); 
                
                _landscape.SetAffineLevel(_affine);
                // position refers to bottom left corner of terrain, not center so to ensure it's centered
                // we will use an mOriginOffset during Picking/Ray Collision, Rendering, BoundingBox calculation
            	_landscape.CreateEmptyTerrain (_precision, _chunksX, _chunksZ, 0, 0, 0);
            	
            	_landscape.SetScale((float)scale.x, (float)scale.y, (float)scale.z);
            	           
				// re-enable collision and rendering            	
            	_landscape.SetCollisionEnable (true);
                _landscape.Enable (true); 
                            	
                #if DEBUG // verify empty terrain has size

//            	
//            	// if i move position to 0,0,0 here, does it ruin the -halfwidth -halfHeight offset we set during CreateEmptyTerrain()?
//            	TV_3DMATRIX tmp = Helpers.TVTypeConverter.CreateTVIdentityMatrix ();
//            	CoreClient._CoreClient.Maths.TVMatrixIdentity(ref tmp);
//            	TV_3DVECTOR pos = _landscape.GetPosition();
//            	tmp = _landscape.GetMatrix ();
//            	tmp.m41 = 0;
//            	tmp.m42 = 0;
//            	tmp.m43 = 0;
//            	_landscape.SetMatrix (tmp);
//            	_landscape.GetBoundingBox(ref min, ref max);
//            	
//            	// SetMatrix does not affect the value from .GetPosition!!!  TV3D Bug
//            	pos = _landscape.GetPosition();
//          
//            	pos.x = 0;
//            	pos.y = 0;
//            	pos.z  = 0;
//            	
//            	_landscape.SetPosition (pos.x, pos.y, pos.z);
//            	// now the bounding box changes
//            	_landscape.GetBoundingBox(ref min, ref max);
            	
                #endif
            }

    		// _landscape.GenerateTerrain <-- Generate is for Heightmap images
    		// _landscape.LoadTerrainData <-- LoadTerrainData is for TV terrain data files
    		// _landscape.CreateEmptyTerrain <-- CreateEmptyTerrain is for empty no heightmap or terrain data

            // compute offset to move origin from bottom left corner to center of landscape
            TV_3DVECTOR min = new TV_3DVECTOR(), max = new TV_3DVECTOR();
        	_landscape.GetBoundingBox(ref min, ref max);

        	mOriginOffset = (max / 2f);
        	mOriginOffset.x = -mOriginOffset.x;
        	mOriginOffset.y = -mOriginOffset.y;
        	mOriginOffset.z = -mOriginOffset.z;
    		
            	
    		_landscape.SetClamping(_clampingEnabled);

    		_tvfactoryIndex = _landscape.GetIndex();
    		
    		InitializeLOD (_landscape);
    		
            SetChangeFlags(Keystone.Enums.ChangeStates.GeometryAdded | 
    		               Keystone.Enums.ChangeStates.BoundingBoxDirty | 
    		               Keystone.Enums.ChangeStates.RegionMatrixDirty |
    		               Keystone.Enums.ChangeStates.MatrixDirty, Keystone.Enums.ChangeSource.Self);
        }

        public override void SaveTVResource(string filepath)
        {
            throw new NotImplementedException();
        }


        // importing from TV Terrain data file
        private static TVLandscape Import (string id, CONST_TV_LANDSCAPE_AFFINE affine)
        {
            try
            {
                TVLandscape land;

                land = CoreClient._CoreClient.Scene.CreateLandscape(id);
                // do not allow collision or enable until terrain has been loaded or else AccessViolations may occur in TVEngine
                land.SetCollisionEnable(false);
                land.Enable(false);

                //land.SetClamping (_clampingEnable)
                // TODO: LoadTerrainData should not occur here.  It should occur during LoadTVResource()
                string filepath = System.IO.Path.Combine (Core._Core.ModsPath, id);
                
                if (System.IO.File.Exists (filepath) == false)
                	throw new System.IO.FileNotFoundException ("Terrain.Import() - '" + filepath + "' not found.");
                
                land.LoadTerrainData(filepath);
                land.ComputeNormals(true);

                land.SetCollisionEnable(true);
                land.Enable(true);
                
                return land;
            }
            catch
            {
                Trace.WriteLine("Terrain.LoadTerrainData() -- FAILED TO LOAD TERRAIN PATCH!!!!");
                return null;
            } // throw exception or just write to debug?
        }

    	private const int VERTICES_PER_CHUNK_AT_MAX_PRECISION = 256; 
                	
        // importing from a HEIGHTMAP file using ideal 1:1 src heightmap pixel to width and height in meters of the 3d landscape
        private static TVLandscape Import(string id, string heightmap,
                                     CONST_TV_LANDSCAPE_PRECISION ePrecision,
                                     CONST_TV_LANDSCAPE_HEIGHTMAP heightmaptype,
                                     CONST_TV_LANDSCAPE_AFFINE affine,
                                     Vector3d position, Vector3d scale,
                                     bool bSmoothTerrain)
        {
            //The following is designed to compute the X and Z spacing between vertices in the terrain
            //This is necessary for when we want to compute a brush size to manipulate those vertices in our Editor
            string dir = System.IO.Path.Combine (Core._Core.ModsPath, "terrain");
            string path = System.IO.Path.Combine (dir, heightmap);
            
            int index = CoreClient._CoreClient.TextureFactory.LoadTexture(path);
            TV_TEXTURE info = CoreClient._CoreClient.TextureFactory.GetTextureInfo(index);

            int verticesPerChunkSide, idealChunkCountX, idealChunkCountZ;
            verticesPerChunkSide = VERTICES_PER_CHUNK_AT_MAX_PRECISION / (int)ePrecision;

            // ideal dimensions have the effect of ensuring that each pixel in the bitmap/jpg heightmap file is 1:1 mapped to a unique vertex
            idealChunkCountX = info.Height / verticesPerChunkSide;
            idealChunkCountZ = info.Width / verticesPerChunkSide;

            // taking into account scale, get the real world distance between adjacent vertices in both X and Z directions
            double seperationWorldSpaceX = scale.x * VERTICES_PER_CHUNK_AT_MAX_PRECISION / verticesPerChunkSide;
            double seperationWorldSpaceZ = scale.z * VERTICES_PER_CHUNK_AT_MAX_PRECISION / verticesPerChunkSide;
            CoreClient._CoreClient.TextureFactory.DeleteTexture(index);

            return
                Import(id, heightmap, ePrecision, heightmaptype, affine, idealChunkCountX, idealChunkCountZ, position, scale,
                       bSmoothTerrain);
        }

        // importing from a heightmap file
        private static TVLandscape Import (string id, string heightmap,
                                     CONST_TV_LANDSCAPE_PRECISION ePrecision,
                                     CONST_TV_LANDSCAPE_HEIGHTMAP heightmaptype,
                                     CONST_TV_LANDSCAPE_AFFINE affine, int chunksX, int chunksZ,
                                     Vector3d position, Vector3d scale,
                                     bool bSmoothTerrain)
        {
        	if (chunksX == 0 || chunksZ == 0) throw new ArgumentOutOfRangeException ("Terrain.Import() - Cannot create terrain with 0 chunk width or height");
        	
        	try
            {
                TVLandscape land = CoreClient._CoreClient.Scene.CreateLandscape(id);
                // do not allow collision or enable until terrain has been loaded or else AccessViolations may occur in TVEngine
                land.SetCollisionEnable (false);
                land.Enable (false); 
                
                // affine must be set before generation because it effects how terrain vertices are generated
                try
                {
                    land.SetAffineLevel(affine); // allways called prior to generation
                }
                catch
                {
                    land.SetAffineLevel(CONST_TV_LANDSCAPE_AFFINE.TV_AFFINE_NO); // called prior to generation
                }

                // TODO: add optional parameter and savepath here to Save the terrain data after its been generated from the heightmap

                // NOTE: we will enforce just a few rules
                // a) all tvlandscapes must be made from the same size heightmap images so the vertices and chunks are the same for all
                // b) the resulting real width/depth of each chunk should fit wholy within a quadtree sector. i.e. no straddling sector boundaries.
                //    NOTE: you are allowed to have say 2x2 chunks or more in a single sector, but all chunks must be wholly contained.

                string dir = System.IO.Path.Combine (Core._Core.ModsPath, "terrain");
            	string path = System.IO.Path.Combine (dir, heightmap);
                if (!land.GenerateTerrain(path, ePrecision, chunksX, chunksZ, 
                                          (float) position.x, (float) position.y, (float) position.z,
                                          bSmoothTerrain))
                    throw new Exception("Terrain.Import() - Failed to load heightmap");
                
 
                //land.SetClamping(_clampingEnabled);
                // land.ComputeNormals(true); <-- causes access violation here
                
                land.SetScale((float) scale.x, (float) scale.y, (float) scale.z);
                
                //land.SaveTerrainData(Core._CoreClient.DataPath() + "\\t1.terrain", CONST_TV_LANDSAVE.TV_LANDSAVE_ALL );

                // BEGIN - GENERATE CONVEX HULL
                //// create the verices array for the entire landscape by itterating from fPosX and fPosY to fPosX + (verticesPerChunkSide * numChunksX * scaleX) and fPosZ + (verticesPerChunkSide * numChunkY * scaleZ)
                //Vector3d[,] verts = new Vector3d[verticesPerChunkSide * iWidth , verticesPerChunkSide * iHeight ];
                //for (int i = 0; i < verticesPerChunkSide * iWidth; i++)
                //{
                //    for (int j = 0; j < verticesPerChunkSide * iHeight; j++)
                //    {
                //        verts[i,j]=(new Vector3d(i * spacingX, land.GetHeight(i * spacingX, j * spacingZ), j * spacingZ));
                //    }
                //}

                ////TODO: meh this is bad.  because the way the lines are drawn, the cross and dont create just triangles
                //List<Vector3d> tmp = new List<Vector3d>();
                //for (int i = 0; i < verticesPerChunkSide * iWidth - 1; i++)
                //{
                //    for (int j = 0; j < verticesPerChunkSide * iHeight - 1; j++)
                //    {
                //        tmp.Add( verts[i, j]);
                //        tmp.Add(verts[i, j+1]);
                //        tmp.Add(verts[i+1, j]);
                //        tmp.Add(verts[i, j]); // meh to complete the pairing

                //        tmp.Add(verts[i+1, j]);
                //        tmp.Add(verts[i, j + 1]);
                //        tmp.Add(verts[i + 1, j+1]);
                //        tmp.Add(verts[i + 1, j]);
                //    }
                //}

                //ConvexHull hull = new ConvexHull(tmp.ToArray() );
                //t.Hull = hull;
                // END - GENERATE CONVEX HULL
                
                land.SetCollisionEnable(true);
                land.Enable(true);

                return land;
            }
            catch
            {
                return null;
            } // throw exception or just write to debug?
        }

        private float mLODSwitchStartDistance = 256; // The distance to start the LOD transform at. 
        private float mLODSwitchInterval = 512;      //  The distance that the LOD changes to the next "level'.  If you set this to 512 with a 512 start distance then the LOD will switch at 1024, 1536, 2048, etc. 
        private CONST_TV_LANDSCAPE_PRECISION mLODMinimumPrecision = CONST_TV_LANDSCAPE_PRECISION.TV_PRECISION_LOW; // minimum LOD precision to use when going to next lowest precision at interval
        private bool mLODDiscardAltitudeInDistanceTests = true; // Since a distance check is used for LOD, big altitude changes can mess things up,  so you can discard using the altitude in the distance check, if you so please.
        private bool mStandardLODEnable = true;
        private bool mLODProgressiveEnable = false; 
        
        private void InitializeLOD(TVLandscape land)
        {
            //Regular Instant POP LOD
            //fStartDistance - The distance to start the LOD transform at. 
            // fLODSwitchDistance - The distance that the LOD changes to the next "level'. 
            //   If you set this to 512 with a 512 start distance then the LOD will switch at 1024, 1536, 2048, etc. 
            // iMinPrecision - Is the enum like you expected, it will be setup as the proper enum 
            //   when we're closer to release. (There are a number of different enums not setup properly quite yet.) 
            // bDiscardAltitude - Since a distance check is used for LOD, big altitude changes can mess things up, 
            //   so you can discard using the altitude in the distance check, if you so please. 
            // -darqShadow

            land.EnableLOD(mStandardLODEnable, 
                           mLODSwitchInterval, 
                           mLODMinimumPrecision, 
                           mLODSwitchStartDistance, 
                           mLODDiscardAltitudeInDistanceTests);
            
            
            // NOTE: PLOD seams are unavoidable at close distances, but _seams_ between chunks with regular LOD can occur too
            //       if the mLODSwitchInterval is too small.   
            // Sylvain says to prevent this, the switch interval should be further apart.
            http://www.truevision3d.com/forums/bugs/landscape_seams_within_a_single_landscape_object-t17502.0.html
            
            
            // PLOD - gradual smooth transition LOD
            // MPJ - PLOD has visible seams beteen chunks of the same TVLandscape. 
            // These seams are easily noticeable, so it's best to just stick with regular lod.
            // Zaknafein - I've also seen this kind of issue a loooong while ago with
			// progressive LOD, so make sure you're not using SetProgressiveLOD(true).
			// If you do want to use it, make sure it's not possible to go "lower" than the 
			// LOWEST precision level by walking LOD levels. I mean by that, if your terrain 
			// is 1 million world units and your LOD levels are seperated by 256, then you'll 
			// hit the minimum level much before the terrain stops rendering. Seams and height 
			// field distortion will start to appear at that point.
	
            land.SetProgressiveLOD(mLODProgressiveEnable);
        }
        
        #endregion


        public Chunk[] Chunks()
        {
            return _chunks;
        }

        public int ChunksZ
        {
            get { return _chunksZ; }
            set { _chunksZ = value;}
        }

        public int ChunksX
        {	
            get { return _chunksX; }
            set { _chunksX = value;}
        }

        /// <summary>
        /// Regardless of precision (vertsX & vertsZ in each chunk) or affine or any such thing, all chunks are 256 x 256 units unless the landscape is scaled
        /// </summary>
        public double ChunkWorldHeight
        {
            get { return 256; } //*_scale.z; }
        }

        /// <summary>
        /// Regardless of precision (vertsX & vertsZ in each chunk) or affine or any such thing, all chunks are 256 x 256 units unless the landscape is scaled
        /// </summary>
        public double ChunkWorldWidth
        {
            get { return 256; } // *_scale.x; }
        }

        /// <summary>
        /// More accurately, it's the width of the entire landscape in meters
        /// </summary>
        public double WorldWidth
        {
            get
            {
                Trace.Assert(_landscape.GetLandRealWidth() == ChunkWorldWidth * ChunksX);
                return _landscape.GetLandRealWidth();
            }
        }

        /// <summary>
        /// More accurately, it's the length of the entire landscape in meters
        /// </summary>
        public double WorldHeight
        {
            get
            {
                Trace.Assert(_landscape.GetLandRealHeight() == ChunkWorldHeight * ChunksZ);
                return _landscape.GetLandRealHeight();
            }
        }

        public void EnableLOD(bool bEnable, float fLODSwitchDistance, CONST_TV_LANDSCAPE_PRECISION iMinPrecision,
                              float fStartDistance, bool bDiscardAltitude)
        {
        	mStandardLODEnable = bEnable;
        	mLODDiscardAltitudeInDistanceTests = bDiscardAltitude;
        	mLODSwitchInterval = fLODSwitchDistance;
        	mLODSwitchStartDistance = fStartDistance;
        	mLODMinimumPrecision = iMinPrecision;
    
        	if (_landscape != null && TVResourceIsLoaded)
	            _landscape.EnableLOD(mStandardLODEnable, 
        		                     mLODSwitchInterval, 
        		                     mLODMinimumPrecision, 
        		                     mLODSwitchStartDistance, 
        		                     mLODDiscardAltitudeInDistanceTests);
        }

        public void EnableProgressiveLOD(bool value)
        {
        	mLODProgressiveEnable = value;
        	if (_landscape != null && TVResourceIsLoaded)
	            _landscape.SetProgressiveLOD(mLODProgressiveEnable);
        }

        public void EnableAllChunks()
        {
            foreach (Chunk c in _chunks)
                c.Enable(true);
        }

        public void EnableOneChunk(int x, int y)
        {
            int targetIndex = x*_chunksX + y;

            for (int i = 0; i < _chunks.Length; i++)
            {
                if (i == targetIndex)
                    _chunks[i].Enable(true);
                else
                    _chunks[i].Enable(false);
            }
        }

        public void EnableOneChunk(Chunk chunk)
        {
            foreach (Chunk c in _chunks)
            {
                if (c == chunk)
                    c.Enable(true);
                else
                    c.Enable(false);
            }
        }

        internal bool SetSplattingEnable
        {
            get { return _splattingEnabled; }
            set
            {
                _splattingEnabled = value;
                _landscape.SetSplattingEnable(value);
            }
        }

        /// <summary>
        /// (int)Precision equates to the unscaled distance between verts in a chunk. 
        /// </summary>
        internal CONST_TV_LANDSCAPE_PRECISION Precision
        {
            get { return _precision; }
        }

        internal CONST_TV_DETAILMAP_MODE SetDetailMode
        {
            get { return _detailMode; }
            set
            {
                _detailMode = value;
                _landscape.SetDetailMode(value);
            }
        }

        // _clampingEnabled
        // clamping (true) seems to fix problems with texture seams between chunks
        internal bool ClampingEnable
        {
            get { return _clampingEnabled; }
            set
            {
                _clampingEnabled = value;
                _landscape.SetClamping(value);
            }
        }

        internal void OptimizeSplatting(bool useShader, bool combineAlphaMap)
        {
            _landscape.OptimizeSplatting(useShader, combineAlphaMap);
        }

        internal void ClearSplats()
        {
        	_landscape.ClearAllSplattingLayers();
        }
        
        internal void RemoveSplats()
        {
        	//_landscape.ClearAllSplattingLayers();
        	_landscape.RemoveAllSplattingTextures ();
        }
        
        internal void SetSplattingMode(bool bUsePS2Shader, bool bUseTextureAlphaAsSpecularMap)
        {
            _landscape.SetSplattingMode(bUsePS2Shader, bUseTextureAlphaAsSpecularMap);
        }

        internal void AddSplattingTexture(int baseTextureIndex, float priority, float tileU, float tileV)
        {
            _landscape.AddSplattingTexture(baseTextureIndex, priority, tileU, tileV);
        }

        internal void ExpandTexture(int textureIndex, int iStartX, int iStartY, int iWidth, int iHeight)
        {
            _landscape.ExpandTexture(textureIndex, iStartX, iStartY, iWidth, iHeight);
        }

        internal void ExpandSplattingTexture(int alphaMapIndex, int textureIndex, int iStartX, int iStartY, int iWidth,
                                           int iHeight)
        {
            _landscape.ExpandSplattingTexture(alphaMapIndex, textureIndex, iStartX, iStartY, iWidth, iHeight);
        }

        internal void SetMaterial(int tvMaterialID, int groupID)
        {
            if (groupID == -1)
                _landscape.SetMaterial(tvMaterialID);
            else
                _landscape.SetMaterial(tvMaterialID, groupID);
        }

        internal void SetTexture(int tvTextureID, int groupID)
        {
            if (groupID == -1)
                _landscape.SetTexture(tvTextureID);
            else _landscape.SetTexture(tvTextureID, groupID);
        }

        internal void SetTextureScale(float fScaleU, float fScaleV, int groupID)
        {
            if (groupID == -1)
                _landscape.SetTextureScale(fScaleU, fScaleV);
            else _landscape.SetTextureScale(fScaleU, fScaleV, groupID);
        }

        internal void SetDetailTexture(int tvTextureID, int groupID)
        {
            if (groupID == -1)
                _landscape.SetDetailTexture(tvTextureID);
            else _landscape.SetDetailTexture(tvTextureID, groupID);
        }

        internal void SetDetailTextureScale(float fScaleU, float fScaleV, int groupID)
        {
            if (groupID == -1)
                _landscape.SetDetailTextureScale(fScaleU, fScaleV);
            else _landscape.SetDetailTextureScale(fScaleU, fScaleV, groupID);
        }

        public void SetLightMapTexture(int tvTextureID, int groupID)
        {
            if (groupID == -1)
                _landscape.SetCustomLightmap(tvTextureID);
            else
                _landscape.SetCustomLightmap(tvTextureID, groupID);
        }

        public TVLandscape[] CreateWaterPatch(TV_2DVECTOR start, double altitude)
        {
            double x = 0, z = 0;
            List<Vector3d> verts = new List<Vector3d>();
            Stack<TV_2DVECTOR> taskList = new Stack<TV_2DVECTOR>();
            TV_2DVECTOR current;

            bool[,] visited = new bool[ChunksX,ChunksZ];

            // add our starting point
            taskList.Push(start);
            verts.Add(new Vector3d(start.x, altitude, start.y));
            //not sure this is necessary because we're building a grid of verts...

            while (taskList.Count > 0)
            {
                current = taskList.Pop();
                // test all 4 directions to see if that neighbor's height <= altitude
                // our stride should be at the resolution of the terrain chunk.
                for (int i = 0; i < 4; i++)
                {
                    // get the west neighbor (and repeat for all neighbors except those of its neighbors already visisted)
                    // if its been visited, exit and dont add to taaklist or verts
                    //if (visited [current.x, current.y])
                    //{
                    //}
                    // if its not visited, set its visited[x,y] = true;

                    // if its out of bounds exit and dont add to tasklist or verts (it's x position is less than chunks x position for west)

                    // if its height >= altitude, then we add it to our list of verts, but not the tasklist

                    // if height < altitude, we add it to the tasklist only
                }
            }

            // triangulate the verts and then create a tvmesh from them
            // to triangulate, first sort the points from lowest x to highest.  If there are two ...hrm.. need to think
            return null;
        }

        public float GetHeight(float x, float z)
        {
            return _landscape.GetHeight(x, z);
            // TODO: setHeight should update BoundVolumeIsDirty=true.  Same with position and scaling changes.
        }
        
        /// <summary>
        /// Retreives a height array for the chunks specified by starting offsets and number of chunks across and high
        /// </summary>
        /// <param name="xOffset">Starting chunk x index.  Chunks first index is 0</param>
        /// <param name="zOffset">Starting chunk y index.  Chunks first index is 0</param>
        /// <param name="width">Number of chunks across</param>
        /// <param name="height">Number of chunks high</param>
        /// <returns>The number of vertices returned is based on the total number of chunks data you'll be retreiving and the precision value
        /// of the terrain</returns>
        public float[] GetHeightArray(int xOffset, int zOffset, int width, int height)
        {
            int numChunks = width*height;
            float[] results = new float[256 / (int) Precision * numChunks]; //vertices per chunk
            _landscape.GetHeightArray(xOffset, zOffset, width, height, results);
            return results;
        }
        
        bool mHeightChanged = false;
        public void SetHeight (float x, float z, float height)
        {
        	
        	TV_3DMATRIX identity = Helpers.TVTypeConverter.CreateTVIdentityMatrix();
            // note: height assignments are done in model space so we use Identity always.  But we have to apply mOriginOffset
            identity.m41 += mOriginOffset.x;
            identity.m42 += mOriginOffset.y;
            identity.m43 += mOriginOffset.z;
                
            _landscape.SetMatrix(identity);
            
             // NOTE: SetMatrix() does not seem to properly update landscapes internal position and scale so we handle it seperately
            _landscape.SetPosition ((float)identity.m41, (float)identity.m42, (float)identity.m43);
            _landscape.SetScale ((float)identity.m11, (float)identity.m22, (float)identity.m33);
            
        	bool updateQuadtree = true;
        	bool delayUpdate = true;
        	bool relative = false; // true == increase height by this amount. false == set height as absolute height
        	_landscape.SetHeight (x, z, height, updateQuadtree, delayUpdate, relative);
        	//mHeightChanged = true;

        }

        //Sylvain added SetHeightArrayEx last autumn because there was a problem using SetHeightArray from
        //certain .net languages (for example APL). Later on, he fixed the source of the problem, but -Ex stayed 
        //for possible future convenience. The only difference between SetHeightArray and -Ex is that -Ex takes 
        //the data array in character (byte) format, while SetHeightArray takes it as 4-byte floats.
        //The format of DataSource is a long sequence of bytes
        //http://www.truevision3d.com/forums/tv3d_sdk_65/setheightarrayex-t12582.0.html;msg87871#msg87871
        public void SetHeightArray (int xOffset, int zOffset, int width, int height, float[] data)
        {
        	_landscape.SetHeightArray (xOffset, zOffset, width, height, data);
        }


        public void FlushHeightChanges ()
        {
        	_landscape.FlushHeightChanges(false, true);
        }
        
        public Vector3d [] GetVertices(Vector3d positionModelSpace, int[,] mask)
        {
        	TV_3DMATRIX identity = Helpers.TVTypeConverter.CreateTVIdentityMatrix();
            // note: picking is done in model space so we use Identity always.  But we have to apply mOriginOffset
            identity.m41 += mOriginOffset.x;
            identity.m42 += mOriginOffset.y;
            identity.m43 += mOriginOffset.z;
                
            _landscape.SetMatrix(identity);
            
             // NOTE: SetMatrix() does not seem to properly update landscapes internal position and scale so we handle it seperately
            _landscape.SetPosition ((float)identity.m41, (float)identity.m42, (float)identity.m43);
            _landscape.SetScale ((float)identity.m11, (float)identity.m22, (float)identity.m33);
                
            TV_3DVECTOR boxMin = new TV_3DVECTOR();
            TV_3DVECTOR boxMax = new TV_3DVECTOR();
        	
        	_landscape.GetBoundingBox (ref boxMin, ref boxMax);
        	
        	
        	
        	TV_3DVECTOR[] vertices = new TV_3DVECTOR[mask.Length];
        	
        	if (mask == null) throw new ArgumentNullException ();
        	
        	int maskWidth =  mask.GetLength(0);
        	int maskHeight =  mask.GetLength (1);
        		

        	TV_3DVECTOR position = Helpers.TVTypeConverter.ToTVVector(positionModelSpace); // mOriginOffset is already factored in by having moved the terrain already
            
        	
        	int vertex = 0;
        	for (int i = 0; i < maskWidth; i++)
        	{
        		for (int j = 0; j < maskHeight; j++)
        		{
	        		// find x and y offsets of mask given maskWidth and maskHeight
	        		if (mask[i,j] > 0)
	        		{
		        		// compute start and stop positions 
		        		TV_3DVECTOR start = position + new TV_3DVECTOR (i, 0, j);
		                TV_3DVECTOR end = start;
		                start.y = boxMax.y + 1;
		                end.y = boxMin.y - 1;
		
		                // Check if collision with landscape - collision is in model space so "position" argument must be passed in
		                // as model space
		                // NOTE: we call TV's collision because we don't want to call our own x times because it constantly resets landscape matrix
		                TVCollisionResult res = _landscape.AdvancedCollide (start, end);
		                if(res.IsCollision())
		                {
		                    // Collided, hten its over a landscape
		                    vertices[vertex++] = res.GetCollisionImpact();
		                }
		                else
		                {
		                    //  No collision, Pointer is floating in the nothingness, dont render
		                    // TODO: can we make some of the vertice elements nullable?
		                }
	        		}
        		}
        	}
        	
        	return Helpers.TVTypeConverter.FromTVVector(vertices) ;
        }
        
        internal void AttachTo(CONST_TV_NODETYPE type, int objIndex, int subIndex, bool keepMatrix,
                               bool removeScale)
        {
            throw new Exception("AttachTo not allowed for Terrain or Chunks.");
        }


        internal void ComputeNormals()
        {
            _landscape.ComputeNormals();
        }

        /// <summary>
        /// NOTE: SetLightingMode must be called after geometry is loaded because the lightingmode
        /// is stored in .tvm, .tva files and so anything you set prior to loading, will be
        /// replaced with the lightingmode stored in the file.
        /// </summary>
        internal CONST_TV_LIGHTINGMODE LightingMode
        {
            set { _landscape.SetLightingMode(value); }
        }

       
        internal override Shader Shader
        {
            set
            {
                if (_shader == value && ( value == null || value.TVResourceIsLoaded)) return;

                if (value != null)
                {
                    // also we don't want to assign this shader if it's underlying tvshader is not yet paged in.
                    if (value.TVResourceIsLoaded == false)
                    {
                        //System.Diagnostics.Debug.WriteLine("Terrain.Shader '" + value.ID + "' not loaded.  Cannot assign to Mesh '" + _id + "'");
                        return;
                    }
                }
        
                _shader = value;
                if (_shader == null)
                    _landscape.SetShader(null);
                else
                    _landscape.SetShader(_shader.TVShader);
            }
        }

        public bool IsVisible
        {
            get { return _landscape.IsVisible(); }
        }

        public void SetShadowCast(bool enable, bool shadowMapping, bool selfshadows, bool additive)
        {
            throw new Exception("TVLandscape cannot cast shadows.");
        }
        
        // OBSOLETE - we always use  _landscape.AdvancedCollide() as we iterate through all scene geometry 
        // and we never use tvScene.AdvancedCollide() to allow TV to test all scene geometry for us.  We have more
        // control iterating ourselves.
//        public bool CollisionEnable
//        {
//            set
//            {
//            	// TODO: normally, this is done in Entity._entityFlags as Pickable / Collideable
//            	//       Im not sure if SetCollisionEnable is specifically for Newton physics collisions or for ray test collisions
//            	//       but i assume just raytests during TVScene.GetCollision.
//            	//       But we do not use that so this may be irrelevant.
//                _collisionEnabled = value;
//                _landscape.SetCollisionEnable(value);
//                // _landscape.AdvancedCollide ();
//            }
//        }

        internal override  void SetAlphaTest (bool enable, int iGroup)
        {            
        	_landscape.SetAlphaTest (enable, _alphaTestRefValue, _alphaTestDepthBufferWriteEnable);
        }


        internal override CONST_TV_BLENDINGMODE BlendingMode
        {
            set { throw new NotImplementedException(); }
        }

        public override int CullMode
        {
            set
            {
                _cullMode = value;
                // TODO: i can't think of a time we'd ever want this anything other than back cull
                _landscape.SetCullMode((CONST_TV_CULLING)value);
            }
        }


        public override int VertexCount
        {
            get { throw new NotImplementedException(); }
        }

        public override int GroupCount
        {
            get { return _landscape.GetChunkCount(); }
        }

        public override int TriangleCount
        {
            get { throw new NotImplementedException(); }
            // TODO: this needs to be computed from the formula
        }


        
        internal override PickResults AdvancedCollide(Vector3d start, Vector3d end, KeyCommon.Traversal.PickParameters parameters)
        {
            TV_3DMATRIX identity = Helpers.TVTypeConverter.CreateTVIdentityMatrix();
            // note: picking is done in model space so we use Identity always.  
                
            // TODO: on Render() we have to also .SetPosition() and .SetScale because SetMatrix does not update the position.
            //       Why is it so far working without doing that here?
            _landscape.SetMatrix(identity); 

            PickResults pr = new PickResults();
            
            TVCollisionResult tvResult = _landscape.AdvancedCollide(Helpers.TVTypeConverter.ToTVVector(start), 
        	                          Helpers.TVTypeConverter.ToTVVector(end));
            
            // TODO: is the tvmesh.render() requirement problem relevant to tvlandscape as well?
        	// Dec.11.2013 - after a tvmesh is loaded, the first tvmesh.Render() must be called before AdvancedCollision will work.  This is a known TV3D bug.

        	pr.HasCollided = tvResult.IsCollision();
            if (pr.HasCollided)
            {
                // Debug.WriteLine("Face index = " + tvResult.iFaceindex.ToString());
                // Debug.WriteLine("Distance = " + tvResult.fDistance.ToString());
                // Debug.WriteLine("Tu = " + tvResult.fTexU.ToString());
                // Debug.WriteLine("Tv = " + tvResult.fTexV.ToString());
                // Debug.WriteLine("Fu = " + tvResult.fU.ToString());
                // Debug.WriteLine("Fv = " + tvResult.fV.ToString());
                
                pr.FaceID = tvResult.GetCollisionFaceId();
                //tvResult.GetCollisionLandscapeChunk();
                
                // region matrix is useful for determining ImpactPoint and Distance and for debug rendering info
                // TODO: is tvResult.vCollisionImpact in local space? Since ive done the collision
                //       in local space i think it should be.
                pr.ImpactPointLocalSpace = Helpers.TVTypeConverter.FromTVVector(tvResult.GetCollisionImpact());
                pr.ImpactNormal = Helpers.TVTypeConverter.FromTVVector(tvResult.GetCollisionNormal());
                // pr.ImpactNormal = -ray.Direction;

                pr.CollidedObjectType = PickAccuracy.Geometry;
            }
            
            return pr;
        }
         

        // shaderOverride used by landshadow.dll to temporarily draw with depth shader
        public void Render(Matrix matrix, Scene.Scene scene, Model model, double elapsedSeconds, Shader shaderOverride)
        {
        	Shader previousShader = this.Shader;
        	Shader = shaderOverride;
        	Render (matrix,scene, model, elapsedSeconds);
        	Shader = previousShader;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="scene">Usually Context.Scene rather than Entity.Scene since AssetPlacementTool preview Entity's are not connected to Scene.</param>
        /// <param name="model"></param>
        /// <param name="elapsedSeconds"></param>
        internal override void Render(Matrix matrix, Scene.Scene scene, Model model, double elapsedSeconds)
        {
    		lock (mSyncRoot)
            {
            
			try
            {
            	// TODO: it should be impossible for _resourceStatus to be "Loaded" before it's actually loaded
            	//       HOWEVER if paging out, we could start to render here first since it's not synclocked and then
            	//       while minimesh.Render() we page out and set _resourceStatus to Unloading but we're already in .Render()!
            	
            	// NOTE: we check PageableNodeStatus.Loaded and NOT TVResourceIsLoaded because that 
            	// TVIndex is set after Scene.CreateTerrain() and thus before we've finished setting 
            	// normals, or initialized terrain LOD
                if (_resourceStatus != PageableNodeStatus.Loaded ) return;
                
                // TODO: we should i think provide an overloaded Render() where we can
                // override the Appearance by passing one in?  Typically we can clone the 
                // appearance so all groups match up, then we can do things like alter the
                // material alpha or ambience, etc, however ambience should be done with
                // a shader parameter.  
                // i think supplying an override appearance is the easiest way...
                // one of the downsides is determining which ones will use the alternate
                // and how to create that appearance.  it would be so much better if we
                // could override this in some other fashion...but i just cant really think of a way
                // to do that. 
                // I mean lets say we have a selected entity in the world, again here is an entity
                // where i may want to alter the default rendering appearance... 
                // the thing is, if i modify the existing, i lose the original settings 
                // it wouldnt be bad if there was a way to easily restore original settings.
                // Because we could run functions across the entire graph to modify all appearances
                // in some simple way, but then how do we restore them?
                // is there some way to "remember" the original state or a way to
                // pass overrides to the default state in such a way that we can still
                // take advantage of hashcode test of changes?
                // - because perhaps the .Appearance can store overrides and if the overrides
                //   havent changed then hashcode will be same and no need to modify anything...
                // - also, we could conceivably apply the overrides during cull so that we can
                //   still add them to proper buckets?  bucket ordering is necessary for alpha blending for

                if (model.Appearance != null) // TODO: i could potentially set it to use the parent model's appearance if applicable
                {
                    // TODO: why is this check for geometry added only now setting appearancechanged flag?
                    //if ((_changeStates & Enums.ChangeStates.GeometryAdded) != 0)
                    //    _changeStates |= Keystone.Enums.ChangeStates.AppearanceChanged;

                            // TODO: shouldn't the above call SetChangeStates
                            // TODO: actually, shouldn't this have already been done once this geometry node was added? including after the resource is loaded
                            //SetChangeFlags(Keystone.Enums.ChangeStates.AppearanceChanged, Keystone.Enums.ChangeSource.Self);


                            // AutoUpdateOpacityMap();
                            
                    // TODO: how do we apply any TextureCycle animation?
                    // Ahh.... a TextureCycle should be viewed as an EFFECT, not an Animation
                    // 
                    _appearanceHashCode = model.Appearance.Apply(this, elapsedSeconds);

                    // TODO: for a depth pass, we might not want to update shader params
                    //       should we have another parameter for this  method (bool updateShaderParameters)
                    // 

                    // in OnRender, shaderParameters are looped thru and set
                    // but the bonus of a script is this can easily be different for other models.
 
                    // // or wait, actually... the script doesnt need the shader's ID
                    // all it needs is to do...
                    // SomeAPI.SetShaderParameter (modelID, "parameterName", parameterValue);
                    // "PlanetRadiusSquared"  // // radius of planet squared, is needed because the rings.fx doesnt inherently know the size of the planet casting a shadow on the ring
                    //"BeginCameraFade "


                    // iterate thru each shader is probably best
                    //void setParameters(){

                    //    sarMaterial.SetColor("_BaseColor", baseColor );
                    //    sarMaterial.SetFloat("_NormalContribution", normalContribution );
                    //    sarMaterial.SetFloat("_DepthContribution", depthContribution );
                    //    sarMaterial.SetFloat("_NormalPower", normalPower );
                    //    sarMaterial.SetFloat("_DepthPower", depthPower );       
                    //}
                    

                    // shader.UpdateParameters (entityInstance)
                    //    
                    // void UpdateParameters (EntityBase entity)
                    // {
                    //      // the short term fix is to simply derive a new type of shader "RingShader" that then 
                    //      // knows all of it's constants and would know that it needs to get the entity's parent entity World radius
                    //      if (delegate != null)
                    //          delegate.Invoke(entity);
                    // }

                }
                else
                    _appearanceHashCode = NullAppearance.Apply(this, _appearanceHashCode);

            }
            catch
            {
                // note: TODO:  very very rarely ill get an access violation on render after a mesh has been paged in.  The mesh shows as loading fine
                // in my code and in tv debug log.  My only real guess is that when the materials and textures get loaded, there's a race condition
                // where either of those will attempt to set while the mesh is rendering.  we need some kind of guard against that.
                Trace.WriteLine(string.Format("TerrainGeometry.Render() - Failed to render '{0}' w/ refount '{1}'.", _id, RefCount));
            }

            try
            {                 
               
            	if (mHeightChanged)
            	{
            		_landscape.SetCollisionEnable (false);
            		_landscape.Enable (false);
        			_landscape.FlushHeightChanges();
		        	_landscape.ComputeNormals();
		        	_landscape.SetCollisionEnable (true);
		        	_landscape.Enable( true);
        			mHeightChanged = false;
            	}
            	// note: TODO:  very very rarely i will get an access violation on render after a mesh has been paged in.  The mesh shows as loading fine
                // in my code and in tv debug log.  My only real guess is that when the materials and textures get loaded, there's a race condition
                // where either of those will attempt to set while the mesh is rendering.  we need some kind of guard against that. 
                // Shaders as well
                // one bug i fixed (which may have fixed this entirely(?) is if the shader was applied while it was loading, this would
                // result in an access violation.
                // TODO: also can it be the shader applied to it is not fully ready?
                //System.Diagnostics.Debug.WriteLine(model.ID.ToString());
                                // TODO: If we are unloading a mesh/scene and we are still calling render,
                //       then problem!  resourcestatus var value should be switch to unloading yes?
                //       - this needs to be fixed for all pageable nodes. 
                //       Because zone paging In/Out occur in another thread, we definetly need to
                //       lock the pageable for these nodes and update their resource status
                //       - clearly there's also a race condition since we check resourcestatus and never prevent it from unloading 
                //       between then and here! does adding lock (mSyncRoot) in this Render() method solve it?
                // TODO: or is the access violation related to the scale set in matrix? this current access violation has scale = 16961330.3042836,16961330.3042836,16961330.3042836
                // but surely i have stars larger than that.  
                //      - is it something in shader?
                matrix.M41 += mOriginOffset.x * matrix.M11; // originoffset is scaled
                matrix.M42 += mOriginOffset.y * matrix.M22;
                matrix.M43 += mOriginOffset.z * matrix.M33;
                
                _landscape.SetMatrix(Helpers.TVTypeConverter.ToTVMatrix (matrix));
//                TV_3DVECTOR pos;
//                pos.x = (float)matrix.M41;
//                pos.y = (float)matrix.M42;
//                pos.z = (float)matrix.M43;
//                
//                TV_3DVECTOR scale;
//                scale = _landscape.GetScale ();
                // NOTE: SetMatrix() does not seem to properly update landscapes internal position and scale so we handle it seperately
                _landscape.SetPosition ((float)matrix.M41, (float)matrix.M42, (float)matrix.M43);
                _landscape.SetScale ((float)matrix.M11, (float)matrix.M22, (float)matrix.M33);
                
                // TODO: this will throw accessviolation if we try to modify group materials textures while rendering.
                //       and im not sure how to best prevent render until modifications are done.  Best actually is to
                //       treat modifications to mesh as we do all attempts to change scene... force through Command processor
                //       and then through SetProperties()
                // TODO: seems our plugin is causing problems here too when the plugin for the terrain object is populating
                //       and then calling .Render() here.  no idea what the problem is.
                _landscape.Render();

  //              System.Diagnostics.Debug.WriteLine ("TerrainGeometry.Render() " + System.Environment.TickCount.ToString());

	            // http://hollowfear.com/development/the-engine/#!prettyPhoto/1/
	            // 3D grass is not done using billboarding, but using simple low poly mesh with an 
	            // alpha blended texture(or alpha tested at lower quality settings). The trick however,
	            // to get the decent looks, is in the design of the mesh (see the image), in even, 
	            // but slightly randomised distribution across the terrain with randomised mesh rotation 
	            // and scaling, and in an appropriate texture. After some playing around, I realised 
	            // that the texture works best when it has a gradient overlay from dark gray at the
	            // bottom, to bright gray on top. Then in my level editor I set the color overlay of 
	            // grass meshes so that the color of bottom part of the mesh at least roughly matches
	            // with the terrain texture used as 3d grass base. For the sake of performance, all the 
	            // grass meshes are merged to a single vertexbuffer per terrain quadtree. Grass is animated
	            // in a very simple manner, ofsetting top vertices in vertex shader based on randomised 
	            // vertex colors and three sinusoids of different magnitude.
	            
            }
            catch
            {
                // note: TODO:  very very rarely ill get an access violation on render after a mesh has been paged in.  The mesh shows as loading fine
                // in my code and in tv debug log.  My only real guess is that when the materials and textures get loaded, there's a race condition
                // where either of those will attempt to set while the mesh is rendering.  we need some kind of guard against that.
                Trace.WriteLine(string.Format("TerrainGeometry.Render() - Failed to render '{0}' w/ refount '{1}'.", _id, RefCount));
            }
		}
        }
        

        //public override void Render(Vector3d cameraPosition, FX_SEMANTICS source)
        //{
        //    // select either the LOD or Geometry child and traverse it.
        //    if (_landscape != null)
        //    {
        //        if (_children != null)
        //        {
        //            foreach (Node obj in _children)
        //            {
        //                //nothing really to do with chunks right?
        //                // TODO: maybe for debugging add visibility count of chunks visible
        //            }
        //        }

        //        // NOTE: Landscape has no seperate Geometry node so there is never a case of shader swapping.
        //        // that is why we override the Shader set property here to set the shader directly to the TVLandscape.
        //        Render();
        //    }
        //}



        private void AutoUpdateOpacityMap(int alphaMapTextureIndex, int alphaMapTextureWidth, int alphaMapTextureHeight, int splatLayerCount)
        {

        	throw new NotImplementedException();
        	
//	        // Splatmap data is stored internally as a 3d array of floats, so declare a new empty array ready for your custom splatmap data:
//	        float[, ,] splatmapData = new float[alphaMapTextureWidth, alphaMapTextureHeight, splatLayerCount];
//	         
//	        for (int y = 0; y < alphaMapTextureHeight; y++)
//	        {
//	            for (int x = 0; x < alphaMapTextureWidth; x++)
//	        	{
//	                // Normalise x/y coordinates to range 0-1
//	                float y_01 = (float)y / (float)alphaMapTextureHeight;
//	                float x_01 = (float)x / (float)alphaMapTextureWidth;
//	                 
//	                // Sample the height at this location (note GetHeight expects int coordinates corresponding to locations in the heightmap array)
//	                //float height = terrainData.GetHeight(Mathf.RoundToInt(y_01 * terrainData.heightmapHeight), Mathf.RoundToInt(x_01 * terrainData.heightmapWidth) );
//	           //     System.Diagnostics.Debug.Assert (_landscape.GetLandHeight() == verticesPerChunkSide * _landscape.GetLandHeight());
//	                float height = _landscape.GetHeight (y_01 * _landscape.GetLandHeight(), x_01 * _landscape.GetLandWidth());
//	                                
//	                // Calculate the normal of the terrain (note this is in normalised coordinates relative to the overall terrain dimensions)
//	                //Vector3d normal = terrainData.GetInterpolatedNormal(y_01,x_01);
//	                TV_3DVECTOR normal = _landscape.GetInterpolatedNormal(y_01, x_01); // _landscape.GetNormal (y_01, x_01);
//	      			
//	                // Calculate the steepness of the terrain
//	                // float steepness = terrainData.GetSteepness(y_01,x_01);
//	                // .GetSlope is altidude difference / distance between the two points.  .GetSlopeAngle is atan (altidude difference / distance between the two points)
//	                // get the distance between terrain vertices (not including scale) based on precision and chunks
//	                int verticesPerChunkSide = VERTICES_PER_CHUNK_AT_MAX_PRECISION / (int)_precision;
//		
//		            // taking into account scale, get the real world distance between adjacent vertices in both X and Z directions
//		            double seperationWorldSpaceX = scale.x * VERTICES_PER_CHUNK_AT_MAX_PRECISION / verticesPerChunkSide;
//		            double seperationWorldSpaceZ = scale.z * VERTICES_PER_CHUNK_AT_MAX_PRECISION / verticesPerChunkSide;
//            
//	                float steepness = _landscape.GetSlope (y_01, x_01);
//					
//	                
//	                // Setup an array to record the mix of texture weights at this point
//	                float[] splatWeights = new float[splatLayerCount];
//	                 
//	                // CHANGE THE RULES BELOW TO SET THE WEIGHTS OF EACH TEXTURE ON WHATEVER RULES YOU WANT
//	     
//	                // Texture[0] has constant influence
//	                splatWeights[0] = 0.5f;
//	                 
//	                // Texture[1] is stronger at lower altitudes
//	                splatWeights[1] = Utilities.MathHelper.Clamp((terrainData.heightmapHeight - height));
//	                 
//	                // Texture[2] stronger on flatter terrain
//	                // Note "steepness" is unbounded, so we "normalise" it by dividing by the extent of heightmap height and scale factor
//	                // Subtract result from 1.0 to give greater weighting to flat surfaces
//	                splatWeights[2] = 1.0f - Utilities.MathHelper.Clamp(steepness * steepness / (terrainData.heightmapHeight / 5.0f));
//	                 
//	                // Texture[3] increases with height but only on surfaces facing positive Z axis
//	                splatWeights[3] = height * (float)Utilities.MathHelper.Clamp(normal.z);
//	                 
//	                // Sum of all textures weights must add to 1, so calculate normalization factor from sum of weights
//	                float z = splatWeights[0] + splatWeights[1] + splatWeights[2] + splatWeights[3] ;
//	                 
//	                // Loop through each terrain texture
//	                // TODO: for tv3d, we have a single alphamap that is 32bpp, but here we're using 4 floats
//	                for(int i = 0; i < splatLayerCount; i++)
//	                {     
//	                    // Normalize so that sum of all texture weights = 1
//	                    splatWeights[i] /= z;
//	                     
//	                    // Assign this point to the splatmap array
//	                    splatmapData[x, y, i] = splatWeights[i];
//	                }
//	            }
//        	}
//      
//	        // update the alpamap texture with the new data
//	        CoreClient._CoreClient.TextureFactory.LockTexture (alphaMapTextureIndex);
//	        CoreClient._CoreClient.TextureFactory.SetPixelArrayFloatEx (alphaMapTextureIndex, , , , splatmapData);
//	        CoreClient._CoreClient.TextureFactory.UnlockTexture (alphaMapTextureIndex);
//	        
//        	// Finally assign the new splatmap to the TVLandscape:
//        	//_landscape.SetTextureEx .SetAlphamaps(0, 0, splatmapData);
        }
        
        #region IBoundVolume Members
        protected override void UpdateBoundVolume()
        {
        	if (TVResourceIsLoaded == false) return;
        	
            TV_3DVECTOR min = new TV_3DVECTOR(), max = new TV_3DVECTOR();
            _landscape.GetBoundingBox(ref min, ref max);
            
            min += mOriginOffset;
            max += mOriginOffset;
            
            _box = new BoundingBox(min.x, min.y, min.z, max.x, max.y, max.z);
            DisableChangeFlags(Keystone.Enums.ChangeStates.BoundingBox_TranslatedOnly | Keystone.Enums.ChangeStates.BoundingBoxDirty);
        }
        #endregion

        
        #region IDisposable members
        protected override void DisposeUnmanagedResources()
        {
        	if (_landscape != null)
	            _landscape.Destroy();
    
        	base.DisposeUnmanagedResources();
        }

        #endregion

    }
}