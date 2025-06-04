using System;
using System.Diagnostics;
using Keystone.Entities;
using Keystone.IO;
using Keystone.Resource;
using Keystone.Traversers;
using Keystone.Types;

namespace Keystone.Elements
{
    /// <summary>
    /// Unlike a Billboard, a TexturedQuad does not exist in 3D space.  It is drawn
    /// in 2D Screenspace exclusviely.  This makes it good for GUI items such as menus and buttons
    /// but also useful for lens flares and some other fx that are best drawn as 2D elements on
    /// top of the rest of the scene.
    /// </summary>
    public class TexturedQuad2D : Geometry 
    {
        //internal float mWidth;
        //internal float mHeight;
        internal float mAngle; // rotated quad angle
        internal int mColor1;
        internal int mColor2;
        internal int mColor3;
        internal int mColor4;
        internal float mTU1;  // for animated textures, these UVs can be got from the AnimatedTexture?
        internal float mTU2;
        internal float mTV1;
        internal float mTV2;

        internal int mTextureID;

        // TODO: rename resourcePath to just id
        // and have TexturedQuad receive it's texture from Appearance to maintain conventions
        // and our IPageableNoe loadtvresource does not need to do anything.  
        internal TexturedQuad2D(string resourcePath)
            : base(resourcePath)
        {
            SetChangeFlags(Enums.ChangeStates.All, Enums.ChangeSource.Self);
            Shareable = true;

            mColor1 = Keystone.Types.Color.White.ToInt32();
            mColor2 = mColor3 = mColor4 = mColor1; // fullbright opaque white is default color
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
            Settings.PropertySpec[] properties = new Settings.PropertySpec[3 + tmp.Length];
            tmp.CopyTo(properties, 3);

            properties[0] = new Settings.PropertySpec("width", typeof(int).Name);
            properties[1] = new Settings.PropertySpec("height", typeof(int).Name);
            properties[2] = new Settings.PropertySpec("angle", typeof(int).Name); // rotated quad angle

            if (!specOnly)
            {
                // TODO: wait, i want to move width and height to Entity or Model.  I think it's ok
                //       because when drawing a textured quad dynamically from script, it directly
                //       creates Renderable2DTexturedQuad  and there is no long term scene node used.
                //     
                //properties[0].DefaultValue = mWidth;
                //properties[1].DefaultValue = mHeight;
                properties[2].DefaultValue = mAngle;  // angle should be in Entity too. or model
                                                      
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
                    case "width":
                        //mWidth = (int)properties[i].DefaultValue;
                        break;
                    case "height":
                        //mHeight = (int)properties[i].DefaultValue;
                        break;
                    case "angle":
                        mAngle = (float)properties[i].DefaultValue;
                        break;
                }
            }
        }
        #endregion

        public float Angle { get { return mAngle; } set { mAngle = value; } }

        //public float Width { get { return mWidth; } set { mWidth = value; } }

        //public float Height { get { return mHeight; } set { mHeight = value; } }

        #region IPageableNode Members
        public override void UnloadTVResource()
        {
        	DisposeManagedResources();
        }
                
        public override void LoadTVResource()
        {
            try
            {
                // TODO: we must wait for the SetTexture() call here to set the _tvfactoryIndex
                _tvfactoryIndex = int.MaxValue;

                SetChangeFlags(Keystone.Enums.ChangeStates.GeometryAdded |
                               Keystone.Enums.ChangeStates.RegionMatrixDirty |
 			                   Keystone.Enums.ChangeStates.MatrixDirty, Keystone.Enums.ChangeSource.Self);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("error on LoadTVResource() - actor path == " + _id + ex.Message);
                throw ex;
            }
        }
        public override void SaveTVResource(string resourcePath)
        {
            // do nothing.  texturedquad2d has no geometry resource to save.
        }

        #endregion


        internal void SetTexture(int textureID)
        {
            mTextureID = textureID;
            _tvfactoryIndex = mTextureID;
        }

        internal override Keystone.Collision.PickResults AdvancedCollide(Vector3d start, Vector3d end, KeyCommon.Traversal.PickParameters parameters)
        {
            throw new Exception("TexturedQuad2D.AdvancedCollide() - This method is not valid for this Geometry type. Use below method instead.");
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="matrix">RegionMatrix which will account for scaling of translation and width/height
        /// of 2D GUI Controls under parent 2D GUI Controls that may or may not be scaled themselves.</param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        internal Keystone.Collision.PickResults AdvancedCollide(Controls.Control2D guiControl, Vector3d start, Vector3d end, KeyCommon.Traversal.PickParameters parameters)
        {
            // TODO: I forget why I'm not picking a textured quad in Model space.  I didn't write any notes. argh.

        	//throw new NotImplementedException("TexturedQuad2D.AdvancedCollide()");
            Keystone.Collision.PickResults results = new Keystone.Collision.PickResults();

          
            // z is z-order and x, y are 2D screenspace coordinates that already take into account
            // parent 2D control scaling if applicable.  
                        
            // half the width of this textured quad
            float halfWidth = (float)guiControl.Width / 2f;
            // half the height of this textured quad
            float halfHeight = (float)guiControl.Height / 2f;

            Vector2f min, max;
            min.x = guiControl.CenterX - halfWidth;
            min.y = guiControl.CenterY - halfHeight;
            max.x = guiControl.CenterX + halfWidth;
            max.y = guiControl.CenterY + halfHeight;

            Keystone.Primitives.BoundingRect rect;
            rect.Min = min;
            rect.Max = max;

            if (rect.Contains(parameters.MouseX, parameters.MouseY))
            {
                results.HasCollided = true;
                results.CollidedObjectType = KeyCommon.Traversal.PickAccuracy.Geometry;
        //        results.Matrix = matrix;
        //        results.DistanceSquared = matrix.M41; // TODO: this should maybe be a zorder
            }

            return results;
        }



        // TODO: WARNING: This Render() is never getting called.  Instead  a Renderable2DTexturedQuad is created in RegionPVS when
        //       encountering a TexturedQuad2D and then Renderable2DTexturedQuad.Draw() is called from RegionPVS.Draw()
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
        	// TVIndex is set after Scene.CreateMesh() and thus before we've finished configuring TexturedQuad2D
        	lock (mSyncRoot)
        	{
            if (_resourceStatus != PageableNodeStatus.Loaded ) return;
                
            if (model.Appearance != null)
            {
                _appearanceHashCode = model.Appearance.Apply(this, elapsedSeconds);
            }
            else
            {
                _appearanceHashCode = Keystone.Appearance.NullAppearance.Apply(this, _appearanceHashCode);
                mTextureID = -1;
               
            }


            // x, y are 2D screenspace coordinates.  z is z-order
            Vector3d center = matrix.GetTranslation();

            Vector3d scale = matrix.GetScale();

            Keystone.Primitives.BoundingRect rect;
            // half the width of this textured quad
            float halfWidth = (float)scale.x / 2f;
            // half the height of this textured quad
            float halfHeight = (float)scale.y / 2f;


                // TODO: WARNING: This Render() is never getting called.  Instead  a Renderable2DTexturedQuad is created in RegionPVS when
                //       encountering a TexturedQuad2D and then Renderable2DTexturedQuad.Draw() is called from RegionPVS.Draw()
                if (mAngle == 0)
            {
                // TODO: caller can wrap TexturedQuad2D renders in begin/end block?
                CoreClient._CoreClient.Screen2D.Draw_Texture(mTextureID,
                                                     (float)center.x - halfWidth, (float)center.y - halfHeight,
                                                     (float)center.x + halfWidth, (float)center.y + halfHeight,
                                                     mColor1, mColor2, mColor3, mColor4);
            }
            else
            {
                CoreClient._CoreClient.Screen2D.Draw_TextureRotated(mTextureID,
                                                            (float)center.x,
                                                            (float)center.y,
                                                            (float)scale.x,
                                                            (float)scale.y,
                                                            mAngle, mColor1, mColor2, mColor3, mColor4);
            }
        	}
        }

        #region IBoundVolume Members
        // From Geometry always return the Local box/sphere
        // and then have the model's return World box/sphere based on their instance
        protected override void UpdateBoundVolume()
        {
            if (!TVResourceIsLoaded) return;

            double halfWidth = 0.5;
            double halfHeight = 0.5;

            _box = new BoundingBox(-halfWidth, -halfHeight, 0.0, halfWidth, halfHeight, 0.0);

            _sphere = new BoundingSphere(_box);
            DisableChangeFlags(Keystone.Enums.ChangeStates.BoundingBox_TranslatedOnly | Keystone.Enums.ChangeStates.BoundingBoxDirty);
        }
        #endregion


        #region ResourceBase members
        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            try
            {
                // nothing to release here just reset fake tvindex
                _tvfactoryIndex = -1;
            }
            catch
            {
            }
        }
        #endregion
    }
}
