using System;
using Keystone.Entities;
using Keystone.Types;
using System.Diagnostics;
using Keystone.Traversers;
using Keystone.Resource;

namespace Keystone.Elements
{
    /// <summary>
    /// </summary>
    public class BillboardText : Geometry 
    {
        internal string mText;
        internal int mColor;


        internal BillboardText(string id)
            : base(id)
        {
            SetChangeFlags(Enums.ChangeStates.All, Enums.ChangeSource.Self);
            Shareable = true;

            mColor = Keystone.Types.Color.White.ToInt32();
            
            _tvfactoryIndex = 0;
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
            Settings.PropertySpec[] properties = new Settings.PropertySpec[2 + tmp.Length];
            tmp.CopyTo(properties, 2);

            properties[0] = new Settings.PropertySpec("text", typeof(string).Name);
            properties[1] = new Settings.PropertySpec("color", typeof(int).Name);

            if (!specOnly)
            {
                // TODO: wait, i want to move width and height to Entity or Model.  I think it's ok
                //       because when drawing a textured quad dynamically from script, it directly
                //       creates Renderable2DTexturedQuad  and there is no long term scene node used.
                //     
                //properties[0].DefaultValue = text;
                //properties[1].DefaultValue = mColor;                                                      
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
                    case "color":
                        mColor = (int)properties[i].DefaultValue;
                        break;
                    case "text":
                        mText = (string)properties[i].DefaultValue;
                        break;
                }
            }
        }
        #endregion

        public string Text { get { return mText; } set { mText = value; } }

        public int Color { get { return mColor; } set { mColor = value; } }

        #region IPageableNode Members
        public override void UnloadTVResource()
        {
        	DisposeManagedResources ();
        }
                
        public override void LoadTVResource()
        {
            try
            {
                // TODO: we must wait for the SetTexture() call here to set the _tvfactoryIndex
                _tvfactoryIndex = int.MaxValue;

                SetChangeFlags(Keystone.Enums.ChangeStates.GeometryAdded |
                    Keystone.Enums.ChangeStates.MatrixDirty, Keystone.Enums.ChangeSource.Self);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("BillboardText.LoadTVResource() - " + _id + " " + ex.Message);
                throw ex;
            }
        }
        public override void SaveTVResource(string resourcePath)
        {
            // do nothing.  texturedquad2d has no geometry resource to save.
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="matrix">RegionMatrix which will account for scaling of translation and width/height
        /// of 2D GUI Controls under parent 2D GUI Controls that may or may not be scaled themselves.</param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        internal override Keystone.Collision.PickResults AdvancedCollide(Vector3d start, Vector3d end, KeyCommon.Traversal.PickParameters parameters)
        {
        	throw new NotImplementedException("BillboardText.AdvancedCollide()");
//        	// TODO: billboard text coord picking to avoid need for passing in a Matrix... ermm
//        	
//            Keystone.Collision.PickResults results = new Keystone.Collision.PickResults();
//
//            // z is z-order and x, y are 2D screenspace coordinates that already take into account
//            // parent 2D control scaling if applicable.  
//            Vector3d center = matrix.GetTranslation();
//            // m11, m22, m33 contain the width, height and rotation
//            // width and height will already include hierarchical scaling of parents scaling
//            Vector3d scale = matrix.GetScale();
//
//            Keystone.Primitives.BoundingRect rect;
//            // half the width of this textured quad
//            float halfWidth = (float)scale.x / 2f;
//            // half the height of this textured quad
//            float halfHeight = (float)scale.y / 2f;
//
//            Vector2f min, max;
//            min.x = (float)center.x - halfWidth;
//            min.y = (float)center.y - halfHeight;
//            max.x = (float)center.x + halfWidth;
//            max.y = (float)center.y + halfHeight;
//
//            rect.Min = min;
//            rect.Max = max;
//
//            if (rect.Contains(parameters.MouseX, parameters.MouseY))
//            {
//                results.HasCollided = true;
//                results.CollidedObjectType = KeyCommon.Traversal.PickAccuracy.Geometry;
//                results.DistanceSquared = matrix.M41; // TODO: this should maybe be a zorder
//            }
//
//            return results;
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
 
            if (model.Appearance != null)
            {
            	if (model.Appearance.Material != null)
            		mColor = model.Appearance.Material.Emissive.ToInt32();
            }


            Vector3d center = matrix.GetTranslation();
            Vector3d scale = matrix.GetScale();        

            float x = (float)center.x;
            float y = (float)center.y;
            float z = (float)center.z;
            
            // TODO: when used as a "Background3D" node, the text must still be rendered earlier than other HUD text.
            // TODO: for text, x,y,z should represent the top left corner of the billboard right?
            CoreClient._CoreClient.Text.TextureFont_DrawBillboardText(mText, x, y, z, mColor);                                                  
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


        #region IDisposeable members
        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();
            try
            {
                // nothing to release here except set fake tvindex
            	_tvfactoryIndex = -1;
            }
            catch
            {
            }
        }
        #endregion
    }
}
