using System;
using Keystone.Celestial;
using Keystone.Elements;
using Keystone.IO;
using Keystone.Portals;
using Keystone.Resource;
using Keystone.Scene;
using Keystone.Traversers;
using Keystone.Types;

namespace Keystone.Entities
{
    /// <summary>
    /// An anchor for Cameras that can be scripted as well as trigger 
    /// Simulator/Listener events.
    /// It can also allow us to trigger collisions with sound zones, or any trigger zones
    /// Also, the Viewpoint is crucial for allowing ClientPager.cs to know which neighboring regions
    /// must be paged in.
    /// </summary>
    public class Viewpoint : Entities.Entity 
    {
        private string mStartingRegionID; // 
        private Vector3d mStartingTranslation; // different than the runtime one and is used to reset the position
        private string mFocusEntityID;

        /// <summary>
        /// Creates a default viewpoint for a new scene.
        /// </summary>
        /// <remarks>
        /// Because a scene's Viewpoints must be loaded before any Region it might be added to
        /// in order to fascilitate paging (pager determines what to page next by where the viewpoint
        /// is located) we must store the Region that the viewpoint starts in.  Then these Viewpoints
        /// will be serialized as part of SceneInfo.
        /// </remarks>
        /// <param name="info"></param>
        /// <returns></returns>
        public static Viewpoint Create (string id, string startingRegionID) 
        {
            Viewpoint result = new Viewpoint (id);
            result.mStartingRegionID = startingRegionID;
            return result;
        }

        internal Viewpoint(string id)
            : base(id)
        {
            // a viewpoint can be added to scene and serialized and then when encountered added
            // to a stack.  But at runtime, most viewpoints are not serializable as it's now
            // the user's viewpoint that is being added/removed to different region Entities as it
            // navigates the scene.
            mStartingTranslation.x = 0;
            mStartingTranslation.y = 0;
            mStartingTranslation.z = 0;

            // this viewpoint IS saveable however
            // the "cloned viewpoint" constructor makes those viewpoints
            // that are created from existing viewpoints as NOT saveable
            // since those are cloned from either SceneInfo or from within the scene
            // when traversed.
            // TODO: i still need a drop down to switch between available viewpoints
            //cloned viewpoints are runtime created based off those in the scene
            this.Serializable = false; // only NON CLONED viewpoints can  be serializable
            this.Pickable = false;
            this.Visible = false;     // viewpoints cannot be culled. Visible= false prevents attempts at culling.
            this.InheritRotation = false;
            this.InheritScale = false;

            InitializeCustomData();
        }

        private void InitializeCustomData()
        {
            KeyCommon.Data.UserData data = new KeyCommon.Data.UserData(); // NOTE: 0 for ID because it doesn't matter for behavior trees that don't use Random values.
            
            // TODO: when cloning, entity's UserData needs to be copied too? Or is this data
            //       considered runtime scratch data that we just initialize externally (eg script) and use
            //       externally via AI and such?

            // TODO: all of the below should be set via a script and the ViewpointBehavvior state machine should be apart of that script
            // behavior state
            data.SetString("control", "user"); // {"user, "animated"}
            data.SetString("goal", "goal_none");
            data.SetBool("chase_enabled", true);

            // TODO: what if we want to focus on a point not an Entity?
            // such as a single star in the Nav Map?
            data.SetString("focus_entity_id", FocusEntityID);
            data.SetString("prev_focus_entity_id", null);
            data.SetBool("smooth_zoom_enabled", false);

            // initialize some axis state vars - "cam_speed" and "cam_zoom" must always exist
            data.SetFloat("camera_fov", 60f);
            data.SetVector("camera_up", Vector3d.Up());
            data.SetFloat("cam_speed", 100.0f);
            data.SetFloat("cam_zoom", 1.0f);
            data.SetDouble("mouselookspeed", 0.5);
            data.SetBool("mousepan", false);   // ortho pan toggle
            data.SetBool("mouselook", false);          // look toggle
            data.SetPoint("mouse_deltas", new System.Drawing.Point());
            // initialize the viewpoint's camera angle
            // TODO: can I init mouseAngles in the NavWorkspace in such a way that they dont get overwritten here?
            Vector3d mouseAngles;
            mouseAngles.x = 90; // x (pitch) correlates to mouse Y axis movement
            mouseAngles.y = 0;
            mouseAngles.z = 0;
            data.SetVector("cam_mouse_angles", mouseAngles);

            data.SetQuaternion("cam_dest_rotation", new Quaternion()); // destination look angles
            data.SetQuaternion("cam_dest_smooth", new Quaternion()); // current look angles
            data.SetVector("cam_move_dir", Vector3d.Zero());  // movement direction

            data.SetVector("delta_translation", Vector3d.Zero());
            data.SetDouble("orbit_radius", -80d);
            data.SetDouble("orbit_radius_min", double.MinValue);
            data.SetDouble("orbit_radius_max", double.MaxValue);
            data.SetDouble("orbit_view_angle", -15d);

            data.SetBool("use_quaternion_rotation", false);
            data.SetVector("yawpitchroll", Vector3d.Zero());
            data.SetVector("dest_yawpitchroll", Vector3d.Zero());
            // remember our target here is a Viewpoint so we do want this velocity initialized  
            // TODO: remove this hardcode.  I believe the velocity should already be set before we reach this function      
            Velocity = new Vector3d(0, 0, 10); // TODO: for Viewpoint for now we treat velocity as speed and we just look at "z" value

            BlackboardData = data;
            
        }

//        /// <summary>
//        /// Clones an existing viewpoint.  RenderingContext will always clone
//        /// a viewpoint that it is switching to.  Viewpoints that exist 
//        /// in the scene xml should be thought of as spawnpoints or security cameras, etc.
//        /// They are fixed in the scene but then their attributes are used to clone the
//        /// Viewpoints used by the RenderingContext...
//        /// </summary>
//        /// <remarks>
//        /// If we do not clone them in this way, then when using multiple viewports
//        /// they will all try to share same viewpoints and as a result influence the movement
//        /// of all RenderingContext cameras!
//        /// </remarks>
//        /// <param name="vpToClone"></param>
//        public Viewpoint(Viewpoint vpToClone) : 
//            this (Keystone.Resource.Repository.GetNewName(typeof(Viewpoint)), vpToClone.StartingRegionID)
//        {
//            // TODO: honestly, im not sure if this is best way to go yet, but
//            // this fixes the issue of multiple viewports sharing and manipulating the 
//            // same single viewpoint.  Cloning allows the original viewpoint to stay fixed in the scene
//            // where the designer put it, and allows each RenderingContext to then to have the
//            // same restrictions that were embedded into the original viewpoint
//
//            // - how do we get the region paged in and it's reference assigned?
//            //      - currently ClientPager.cs IS paging in the cloned viewpoints region
//            // - do we or don't we add the cloned viewpoint via AddChild to the region?
//            //      - currently ClientPager.cs IS adding it as a child
//            //      
//            // - how do we deal with stacks?
//
//            // todo; Anchor.cs as type of fixed viewpoint like for security cam?
//            mStartingTranslation = vpToClone.StartingTranslation;
//
//
//
//        }

        // This is used by the ClientPager.cs to determine which neighboring regions to load
        // This is necessary because Viewpoints are not usually serialized in place with the scene.  It's instead
        // serialized to the SceneInfo so that they can be read initially before any paging has taken place.  This is because
        // viewpoints are in fact required by the pager.  
        public string StartingRegionID {
            get { return mStartingRegionID; }
            set { mStartingRegionID = value; }
        }

        /// <summary>
        /// Starting translation is seperate from normal entity.Translation and is used to reset the Viewpoint's position to default.
        /// </summary>
        public Vector3d StartingTranslation { get { return mStartingTranslation; } set {mStartingTranslation = value;} }

        public string FocusEntityID { get { return mFocusEntityID; } set { mFocusEntityID = value; } }
        public bool AutoGenerated { get { return string.IsNullOrEmpty(mFocusEntityID); } }

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

            properties[0] = new Settings.PropertySpec("startingregion", typeof(string).Name);
            properties[1] = new Settings.PropertySpec("startingtranslation", mStartingTranslation.GetType().Name);
            properties[2] = new Settings.PropertySpec("focusentity", typeof(string).Name);

            if (!specOnly)
            {
                properties[0].DefaultValue = mStartingRegionID;
                properties[1].DefaultValue = mStartingTranslation;
                properties[2].DefaultValue = mFocusEntityID;
            }

            return properties;
        }

        // TODO: this should return any broken rules 
        public override void SetProperties(Settings.PropertySpec[] properties)
        {
            if (properties == null) return;
            base.SetProperties(properties);

            for (int i = 0; i < properties.Length; i++)
            {
            	if (properties[i].DefaultValue == null) continue;
                // use of a switch allows us to pass in all or a few of the propspecs depending
                // on whether we're loading from xml or changing a single property via server directive
                switch (properties[i].Name)
                {
                    case "startingregion":
                        mStartingRegionID = (string)properties[i].DefaultValue;
                        break;
                    case "startingtranslation":
                        mStartingTranslation = (Vector3d)properties[i].DefaultValue;
                        break;
                    case "focusentity":
                        mFocusEntityID = (string)properties[i].DefaultValue;
                        break;
                }
            }

            // TODO: Temp hack. Viewpoint should never inherit scale or rotation from any entity it's attached to.
            //       keep this code until all prefabs are saved with this setting.
            this.InheritRotation = false;
            this.InheritScale = false;

        }
        #endregion

		protected override void PropogateChangeFlags(Keystone.Enums.ChangeStates flags, Keystone.Enums.ChangeSource source)
		{
			// filter out BoundingBox dirty since a Viewpoint cannot affect BoundingBox of parents
			Keystone.Enums.ChangeStates filter = Keystone.Enums.ChangeStates.BoundingBoxDirty | Keystone.Enums.ChangeStates.BoundingBox_TranslatedOnly;
			if (source == Keystone.Enums.ChangeSource.Self && ((flags & filter) != Keystone.Enums.ChangeStates.None))
		    {
				// filter out the flags that are _in_ the fitler list
		   		flags &= ~filter;
		    }
			    
			base.PropogateChangeFlags(flags, source);
		} 
        /// <summary>
        /// Override for 1 reason only, Viewpoint is derived from Entity but it can have as parent a SceneInfo and not just an Entity 
        /// as is required for all other derived Entity nodes.
        /// </summary>
        /// <param name="flags"></param>
        protected override void NotifyParents(Enums.ChangeStates flags)
        {
            if (mParents == null || flags == Keystone.Enums.ChangeStates.None) return;

#if DEBUG
            if (mParents.Count >= 0)
                System.Diagnostics.Debug.Assert(mParents.Count == 1);
#endif
            // entity can only have 1 parent at most because it is not a shareable scene graph node
            // normally, we cast to (Entity) but for Viewpoint we cast to (Node) since SceneInfo 
            // node can also be node and not just Entity
            ((Node)mParents[0]).SetChangeFlags(flags, Keystone.Enums.ChangeSource.Child);
        }
    }
}
