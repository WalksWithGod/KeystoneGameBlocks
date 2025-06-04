using System;
using System.Drawing;
using System.Xml;
using Keystone.Appearance;
using Keystone.Entities;
using Keystone.IO;
using Keystone.Resource;
using Keystone.Traversers;
using Keystone.Types;
using MTV3D65;

namespace Keystone.Elements
{
    // encapsulates the TVMesh but allowing only Billboard
    // rendering to be enabled.  
    // NOTE: for billboards, the texture must be set at creation time
    // and then cannot be changed.
    public class Billboard : Mesh3d
    {
    	
        private CONST_TV_BILLBOARDTYPE _billboardtype;
        private bool _axialRotation = false; // to enable, axialRotation must = true _AND_ _billboardtype must equal CONST_TV_BILLBOARDTYPE.TV_BILLBOARD_NOROTATION
        private bool _rotateAtCenter;
        // NOTE: the automatic texture coordinates of tvbillboard are such that our beam weapon textures
        // need to be pointed up verticaly such that the texture's height is bigger than it's width
        private float _height;
        private float _width;

        // obsolete - we should not allow texture passed in here.  Should assign Appearance with Diffuse layer
        //public static Billboard Create(string id, Diffuse texture, CONST_TV_BILLBOARDTYPE billboardType, Vector3d pos,
        //                               float width, float height, bool rotateAtCenter)
        //{
        //    Billboard mesh = (Billboard) Repository.Get(id);
        //    if (mesh != null) return mesh;

        //    mesh = new Billboard(id,  billboardType, rotateAtCenter, width, height);
        //    return mesh;
        //}

        public static string GetCreationString (CONST_TV_BILLBOARDTYPE billboardType, bool rotateAtCenter, float width, float height)
        {
        	return PRIMITIVE_BILLBOARD + PRIMITIVE_DELIMITER +
				   ((int)billboardType).ToString() + PRIMITIVE_DELIMITER + 
        		   width.ToString() + PRIMITIVE_DELIMITER +
	       		   height.ToString() + PRIMITIVE_DELIMITER +
        			rotateAtCenter.ToString();
        }
        
        internal static Billboard Create(string relativeResourcePath)
        {
        	
            string[] split = relativeResourcePath.Split(PRIMITIVE_DELIMITER);
            
            if (split != null && split.Length > 0)
            {
                {
                    // NOTE: By reaching this method we that the primitive does not already exist in Repository cache
                    // parse the primitive type and attributes and build it
                    switch (split[0])
                    {
                        case PRIMITIVE_BILLBOARD:
                    		CONST_TV_BILLBOARDTYPE type = (CONST_TV_BILLBOARDTYPE)int.Parse(split[1]);
                    		float width = float.Parse(split[2]);
                    		float height = float.Parse(split[3]);
                    		bool rotateAtCenter = bool.Parse(split[4]);
                    		
                    		Billboard result =  new Billboard (relativeResourcePath, type, rotateAtCenter, width, height);
            				result._billboardtype = type;
                    		return result;

                        default:
                            throw new Exception();
                            break;
                    }
                }
            }
            else
                throw new Exception("Mesh3d.LoadTVMesh() - Unknown primitive type '" + split[0].ToString() + "'");
        }



        private Billboard(string id, CONST_TV_BILLBOARDTYPE billboardType, bool rotateAtCenter, float width, float height)
            : this(id)
        {
            // NOTE: texture is not supplied here. That is added to Appearance and applied to Geometry via Appearance
            _billboardtype = billboardType;
            _rotateAtCenter = rotateAtCenter;
            _height = height;
            _width = width;
        }

                // version that doesnt require a TVMesh or filepath so that we can load the xmlnode and set the tvresource
        // load state as unloaded so that a traverser can load it later
        internal Billboard(string id)
            : base(id)
        {
        }

        #region ITraversable Members
        public override object Traverse(ITraverser target, object data)
        {
            return target.Apply(this, data);
        }
        #endregion

        #region ResourceBase members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="specOnly">True returns the properties without any values assigned</param>
        /// <returns></returns>
        public override Settings.PropertySpec[] GetProperties(bool specOnly)
        {
            Settings.PropertySpec[] tmp = base.GetProperties(specOnly);
            Settings.PropertySpec[] properties = new Settings.PropertySpec[5 + tmp.Length];
            tmp.CopyTo(properties, 5);

            properties[0] = new Settings.PropertySpec("billboardtype", typeof(int).Name);
            properties[1] = new Settings.PropertySpec("axialrotation", _axialRotation.GetType().Name);
            properties[2] = new Settings.PropertySpec("rotateAtCenter", _rotateAtCenter.GetType().Name);
            properties[3] = new Settings.PropertySpec("height", _height.GetType().Name);
            properties[4] = new Settings.PropertySpec("width", _width.GetType().Name);

            if (!specOnly)
            {
                properties[0].DefaultValue = (int)_billboardtype;
                properties[1].DefaultValue = _axialRotation;
                properties[2].DefaultValue = _rotateAtCenter;
                properties[3].DefaultValue = _height;
                properties[4].DefaultValue = _width;
            }

            return properties;
        }

        public override void SetProperties(Settings.PropertySpec[] properties)
        {
            if (properties == null) return;
            base.SetProperties(properties);

            for (int i = 0; i < properties.Length; i++)
            {
                // use of a switch allows us to pass in all or a few of the propspecs depending
                // on whether we're loading from xml or changing a single property via server directive
                switch (properties[i].Name)
                {
                    case "billboardtype":
                        _billboardtype = (CONST_TV_BILLBOARDTYPE)(int)properties[i].DefaultValue;
                        break;
                    case "axialrotation":
                        _axialRotation = (bool)properties[i].DefaultValue;
                        break;
                    case "rotateAtCenter":
                        _rotateAtCenter = (bool)properties[i].DefaultValue;
                        break;
                    case "height":
                        _height = (float)properties[i].DefaultValue;
                        break;
                    case "width":
                        _width = (float)properties[i].DefaultValue;
                        break;
                }
            }
        }
        #endregion 

        #region IPageableNode Members
        public override void LoadTVResource()
        {
            // TODO: if the mesh's src file has changed, we should unload the previous mesh first
            // TODO: actually we should not support changing of the source, just replace the Mesh3d/Billboard.
            if (_tvmesh != null) _tvmesh.Destroy();
            // TODO: these _height, _width, _rotateAtCenter and other vars arent read properly? Why not? 
            // TODO: also the billboard seems to be positioned at the bottom whereas CreateSphere is in the center.
            _tvmesh = CoreClient._CoreClient.Scene.CreateBillboard(-1, 0, 0, 0, _width, _height, _id, _rotateAtCenter);

            // todo: allow for modifying the vertices via SetVertex() so we can use origin and endpoint. To modify the endpoint, we perhaps only need a distance to modify the z component of the end vertices
            int vertexCount = _tvmesh.GetVertexCount();
            System.Diagnostics.Debug.Assert(vertexCount == 4);

            // endPoint.z = length * dir; // length could be the "width" parameter
            //_tvmesh.SetVertex (0)
            //_tvmesh.SetVertex(1)
            //_tvmesh.SetVertex(2)
            //_tvmesh.SetVertex(3)


            // since we use our custom rotation code for axial and y axis and free rotation
            // we must set tv's type to NOROTATION or else it will override us.  
            _tvmesh.SetBillboardType (_billboardtype);
            _tvmesh.EnableFrustumCulling(false);

           // _tvmesh.SetMeshFormat((int)_meshformat);
           // _tvmesh.SetCullMode(_cullMode);
            //_tvmesh.SetOverlay(_overlay);

            _tvfactoryIndex = _tvmesh.GetIndex();
            System.Diagnostics.Debug.WriteLine ("Billboard.LoadTVResource() - SUCCESS '" + _id + "'");
            SetChangeFlags(Keystone.Enums.ChangeStates.GeometryAdded | 
                			Keystone.Enums.ChangeStates.AppearanceParameterChanged |
                			Keystone.Enums.ChangeStates.BoundingBoxDirty |
                			Keystone.Enums.ChangeStates.RegionMatrixDirty |
                			Keystone.Enums.ChangeStates.MatrixDirty, Keystone.Enums.ChangeSource.Self);
        }
        #endregion

        /// <summary>
        /// NOTE: the automatic texture coordinates of tvbillboard are such that our beam weapon textures
        /// need to be pointed up verticaly such that the texture's height is bigger than it's width
        /// </summary>
        public float Height { get { return _height; } }
        
        public float Width { get { return _width; } }
        
        public bool RotateAtCenter { get { return _rotateAtCenter; } }

        public bool AxialRotationEnable
        {
            get { return _axialRotation; }
            set 
            { 
                _axialRotation = value;
                
                // rotation must be NOROTATION for custom rotations to be applied
                // eg. axial lasers or pinwheeling star coronas.
                if (_axialRotation == true)
                    _billboardtype = CONST_TV_BILLBOARDTYPE.TV_BILLBOARD_NOROTATION;
            }
        }
        
        public CONST_TV_BILLBOARDTYPE BillboardType
        {
            get { return _billboardtype; }
        }



        internal override Keystone.Collision.PickResults AdvancedCollide(Vector3d start, Vector3d end, KeyCommon.Traversal.PickParameters parameters)
        {
            return base.AdvancedCollide(start, end, parameters);
        }

        // TODO: this is not working at all camera angles against our waypoint tabs.  When i move the camera rotation
        //       it seems to work.  Should i be modifying the model space start/end vectors to take into account the
        //       billboard rotation instead of doing that here?
        //       Let's think... the model space start/end vectors do take into account entity and model rotation right?
        //       What if for mouse picking purposes, we orient the billboard to face the Ray?
        internal Keystone.Collision.PickResults AdvancedCollide(Entity entity, Model model, Vector3d lookAt, Vector3d start, Vector3d end, KeyCommon.Traversal.PickParameters parameters)
        {
            Collision.PickResults pr = new Collision.PickResults();
            Ray ray = new Ray(start, end - start);

            bool customRotation = BillboardType == CONST_TV_BILLBOARDTYPE.TV_BILLBOARD_NOROTATION;
            // April.18.2017 - This seems to partially fix our billboard picking -> orient lookAt such that the billboard faces the ray
            //                 but it mostly seems to work when clicking in the center of the billboard, not the edges. Arg!
            //                 Is it a billboard scaling issue?
            lookAt = Vector3d.Normalize(end - start);

            // NOTE: the ray "start" and "end" positions are already passed in, in Model Space so there is no need to compute a billboard rotation
            // Matrix m = Model.GetRotationOverrideMatrix(entity.DerivedRotation, model.Rotation, Vector3d.Zero(), modelSpaceCameraPosition, Vector3d.Up(), lookAt, this._axialRotation, customRotation);
            
            Matrix m = Matrix.Identity();
            _tvmesh.SetMatrix(Helpers.TVTypeConverter.ToTVMatrix (m));
            _tvmesh.Update();

            TV_COLLISIONRESULT tvResult = Helpers.TVTypeConverter.CreateTVCollisionResult();
            _tvmesh.AdvancedCollision(Helpers.TVTypeConverter.ToTVVector(start),
                                          Helpers.TVTypeConverter.ToTVVector(end),
                                          ref tvResult,
                                          Collision.PickResults.ToTVTestType(parameters.Accuracy));

            pr.HasCollided = tvResult.bHasCollided;
            if (pr.HasCollided)
            {
                // Debug.WriteLine("Face index = " + tvResult.iFaceindex.ToString());
                // Debug.WriteLine("Distance = " + tvResult.fDistance.ToString());
                // Debug.WriteLine("Tu = " + tvResult.fTexU.ToString());
                // Debug.WriteLine("Tv = " + tvResult.fTexV.ToString());
                // Debug.WriteLine("Fu = " + tvResult.fU.ToString());
                // Debug.WriteLine("Fv = " + tvResult.fV.ToString());

                pr.FaceID = tvResult.iFaceindex;

                pr.ImpactPointLocalSpace = Helpers.TVTypeConverter.FromTVVector(tvResult.vCollisionImpact);
                pr.ImpactNormal = -ray.Direction; // Helpers.TVTypeConverter.FromTVVector(tvResult.vCollisionNormal); // 
                pr.CollidedObjectType = KeyCommon.Traversal.PickAccuracy.Geometry;
            }

            return pr;
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
           
        	// TODO: it should be impossible for _resourceStatus to be "Loaded" before it's actually loaded
        	//       HOWEVER if paging out, we could start to render here first since it's not synclocked and then
        	//       while minimesh.Render() we page out and set _resourceStatus to Unloading but we're already in .Render()!
        	
        	// NOTE: we check PageableNodeStatus.Loaded and NOT TVResourceIsLoaded because that 
        	// TVIndex is set after Scene.CreateMesh() and thus before we've finished adding vertices
        	// via .AddVertex()  or .SetGeometry() or even loaded the billboard from file.
            if (_resourceStatus != PageableNodeStatus.Loaded ) return;
                
            if (BoundVolumeIsDirty) UpdateBoundVolume();

            
            if (model.Appearance != null) // TODO: i could potentially set it to use the parent model's appearance if applicable
            {
                _appearanceHashCode = model.Appearance.Apply(this, elapsedSeconds);
            }
            else
                _appearanceHashCode = NullAppearance.Apply(this, _appearanceHashCode);


            _tvmesh.SetMatrix(Helpers.TVTypeConverter.ToTVMatrix(matrix));
           
            _tvmesh.Render();
        }


        #region IBoundVolume Members
        // From Geometry always return the Local box/sphere
        // and then have the model's return World box/sphere based on their instance
        protected override void UpdateBoundVolume()
        {
            if (_resourceStatus != PageableNodeStatus.Loaded) return;

            float radius = 0f;
            TV_3DVECTOR min, max, center;
            min.x = min.y = min.z = 0f;
            max.x = max.y = max.z = 0f;
            center.x = center.y = center.z = 0f;
            
            // model space bounding box
			const bool modelSpace = true;
            _tvmesh.GetBoundingBox(ref min, ref max, modelSpace);

            // for culling and picking purposes our billboards are 3d.  This way
            // when we pick, it doesnt matter if we havent applied the billboard
            // rotation matrix since we generally only do that on the render call to
            // save time from constant billboard updates every time they rotate when camera moves.
            // NOTE: for axial billboards often used for lasers, we tend to not want the min, max 
            min.z = Math.Min (min.x, min.y);
            max.z = Math.Max (max.x, max.y);
 //           min.z -= 5;
 //           max.z += 5;
            if (_box == null)
            	_box = new BoundingBox ();
            else
            	_box.Reset ();
            
            _box.Resize(min.x, min.y, min.z, max.x, max.y, max.z);

            _tvmesh.GetBoundingSphere(ref center, ref radius, true);
            _sphere = new BoundingSphere(_box);
            DisableChangeFlags(Keystone.Enums.ChangeStates.BoundingBox_TranslatedOnly | Keystone.Enums.ChangeStates.BoundingBoxDirty);
        }
        #endregion

        // TODO: this doesnt belong here glLineWidth using quads.  We should have a seperate class for drawing gl style lines using screenspace quads
    //Here's a way to solve a recurrent problem : emulating glLineWidth in Direct3D. 
    //         * Better : you can use this code to draw textured lines, which are often needed 
    //         * to create a lot of special effects (lightning bolts, toon-like silhouettes, etc).
    // 
    //The code computes a world quad from a segment in world space. Simply render the quad
    //         * instead of the segment! If you have a line-strip, compute one quad for each segment, 
    //         * then render everything as a single non-indexed tristrip of 4 * NbSegments vertices.
    // 
    //You should be able to use it with any flexible vertex format (FVF).
    // 
    //         All matrices are 4x4 D3D-compliant matrices. mRenderWidth is the width of the render window (ex: 640)
    //
    //I hope it will be useful to some of you. I know I would have loved to have this right from the start (read: included in D3D...). 
    // By the way, it's not supposed to be optimized so don't bother telling me it's not :)
    // 
    //Pierre
    // http://www.flipcode.com/archives/Textured_Lines_In_D3D.shtml
    
        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        /**
         * Computes a screen quad aligned with a world segment.
         * \param  inverseview  [in] inverse view matrix
         * \param  view   [in] view matrix
         * \param  proj   [in] projection matrix
         * \param  verts   [out] 4 quad vertices in world space (forming a tri-strip)
         * \param  uvs    [out] 4 quad uvs
         * \param  stride   [in] size of vertex (FVF stride)
         * \param  p0    [in] segment's first point in world space
         * \param  p1    [in] segment's second point in world space
         * \param  size   [in] size of segment/quad
         * \param  constantsize [in] true to keep the quad's screen size constant (e.g. needed to emulate glLineWidth)
         */
        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        //void ComputeScreenQuad(Matrix inverseview, Matrix view, Matrix proj, byte[] verts, byte[] uvs, uint stride,  Point p0, Point p1, float size, bool constantsize)
        //{
        //     // Compute delta in camera space
        //     Point Delta; TransformPoint3x3(Delta, p1-p0, view);
             
        //     // Compute size factors
        //     float SizeP0 = size;
        //     float SizeP1 = size;
             
        //     if(constantsize)
        //     {
        //          // Compute scales so that screen-size is constant
        //          SizeP0 *= camera.ComputeConstantScale(p0, view, proj);
        //          SizeP1 *= camera.ComputeConstantScale(p1, view, proj);
        //     }
             
        //     // Compute quad vertices
        //     double Theta0 = Math.Atan2(-Delta.x, -Delta.y);
        //     double c0 = SizeP0 * Math.Cos(Theta0);
        //     double s0 = SizeP0 * Math.Sin(Theta0);
        //     ComputePoint(*((Point*)verts),  c0, -s0, inverseview, p0); verts+=stride;
        //     ComputePoint(*((Point*)verts),  -c0, s0, inverseview, p0); verts+=stride;
             
        //     double Theta1 = Math.Atan2(Delta.x, Delta.y);
        //     double c1 = SizeP1 * Math.Cos(Theta1);
        //     double s1 = SizeP1 * Math.Sin(Theta1);
        //     ComputePoint(*((Point*)verts),  -c1, s1, inverseview, p1); verts+=stride;
        //     ComputePoint(*((Point*)verts),  c1, -s1, inverseview, p1); verts+=stride;
             
        //     // Output uvs if needed
        //     if(uvs)
        //     {
        //          *((float*)uvs) = 0.0f; *((float*)(uvs+4)) = 1.0f; uvs+=stride;
        //          *((float*)uvs) = 0.0f; *((float*)(uvs+4)) = 0.0f; uvs+=stride;
        //          *((float*)uvs) = 1.0f; *((float*)(uvs+4)) = 1.0f; uvs+=stride;
        //          *((float*)uvs) = 1.0f; *((float*)(uvs+4)) = 0.0f; uvs+=stride;
        //     }
        //}

        //private void ComputePoint(Point& dest, float x, float y, const Matrix rot, const Point& trans)
        //{
        //     dest.x = trans.x + x * rot.m[0][0] + y * rot.m[1][0];
        //     dest.y = trans.y + x * rot.m[0][1] + y * rot.m[1][1];
        //     dest.z = trans.z + x * rot.m[0][2] + y * rot.m[1][2];
        //}

         
        //// Quickly rotates a vector, using the 3x3 part of a 4x4 matrix
        // inline void TransformPoint3x3(Point& dest, const Point& source, const Matrix rot)
        // {
        //      dest.x = source.x * rot.m[0][0] + source.y * rot.m[1][0] + source.z * rot.m[2][0];
        //      dest.y = source.x * rot.m[0][1] + source.y * rot.m[1][1] + source.z * rot.m[2][1];
        //      dest.z = source.x * rot.m[0][2] + source.y * rot.m[1][2] + source.z * rot.m[2][2];
        // }

       
    
    }
}