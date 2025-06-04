using System;
using Keystone.Traversers;
using Keystone.Types;
using System.Collections.Generic;
using Keystone.Entities;

namespace Keystone.Elements
{

    public enum SelectionMode
    {
        Render,
		Depth,  
		Reflection,		
        BoundsUpdate,
        MousePick,
        Collision,  // <-- Maybe instead of Model[] models = entity.Select(Normal, ) it uses Model[] models = entity.Select(Hull, ); 
        
		Occluder,
        CSGStencilSource,
        CSGStencilAccept,
        DebugRender   // model is for debug view or editor, not for runtime (eg. a camera model used for the viewpoint entity)
    }

    // these flags are not passed in to the Select() function but 
    // rather stored in the Selector node as Selector.SelectorFlags
    // they are set either via plugin or script.
    public enum SelectorNodeStyle 
    {
    	// sequence selects all child nodes that are enabled
        Sequence = 0,      
    	Selector = 1 << 0,
    	
    	// idea behind a Switch was for a geometry selector 
    	// for everything from csg punches to auto-tile segments
    	// and where the geometry returned was based on the "SelectPass" parameter.
    	// But I think Switch and Selector as fundamentally the same thing. 
    	// we don't need both.
        Switch = 1 << 1,   
        
        // LOD is a specialized Selector that uses a "distance from camera" parameter.
		// Question is, given other types of switches such as DamageModel switch
		// what is the parameter and who sets it?  If it's a 0 - 1.0 value of currentHealth / maxHealth
		// then we can index (like a lookup) into any arbitrary number of damage sub models			
        LOD = 1 << 2,

		// select using custom delegate function for things like damage models
        // open scene graph does this by allowing the Entity.Update() execute script
	    // to do  Entity.Selector.Enable        
        Custom = 1 << 3,   
        //Custom1 = 1 << 11,  // no damage
        //Custom2 = 1 << 12,  // light damage
        //Custom3 = 1 << 13,  // medium damage
        //Custom4 = 1 << 14,  // heavy damage
        //Custom5 = 1 << 15   // destroyed
        
        // used with Selector style option
        SelectFirstAvailable = 1 << 5,     // select the first child that is enabled
        SelectLastAvailable,      // iterate from last to first, selecting first enabled (good for collision test against lowest triangle count LOD mesh)
        SelectRandom,             // selects a random child so for instance different Destroyed models can be selected, but then the previous selection will be used from then on
        SelectPrevious,           // selects same child as was selected previously
        SelectAllAvailable,       // select all children that are enabled
    
        // Rendering pass only.
        // used to apply a transition between two meshes that are of the same
        // mask type (eg. CSGStencil or NormalRender)
        Interpolate
    }

    // -- above replaces following enums
    public struct SwitchNodeOptions
    {
        public double DistanceSquared;  // distance from Camera to the current Entity
        public SwitchPassType PassType;
        public SwitchMode Mode;

        public void SetSwitchMode(SwitchMode mode)
        {
            Mode = mode;
        }
    }

    
    /// <summary>
    /// The type of child 
    /// </summary>
    public enum SwitchMode
    {
        Geometry = 0,
        CSGStencil = 1,
        Collider = 2,   // you may want to render the collider instead of the geometry
        Switch = 3   // nested switch
    }

    // http://download.java.net/media/java3d/javadoc/1.3.2/javax/media/j3d/Switch.html
    public enum SwitchPassType : uint
    {       
        // pass types <-- this alone is usually enough to determine which child to return
        // these are used by ScaleDrawer.cs to know what render state options to set
        NormalRender,
        DebugRender,   // model is for debug view or editor, not for runtime (eg. a camera model used for the viewpoint entity)
        CSGStencil,
        MousePick,
        Collision, // Hull
        

        // switch selection  // these two are only evaluated if AUTOMATIC, these are simple rules
        //                   // 
        SelectLOD = 1 << 4,
        SelectFirst = 1 << 5,
        SelectLast = 1 << 6,
        SelectAll = 1 << 7, // render all child nodes for a Sequence node
        SelectFirstReady = 1 << 8, // if the selected child's TVResourceIsLoaded == false, find first ready
        
        // Rendering types only.
        // used to apply a transition between two meshes that are of the same
        // mask type (eg. CSGStencil or NormalRender)
        Interpolate = 1 << 9,  // 
        
        // ignore all if there mSelectedIndex != -1 and just use it.
        UseExistingSelection = 1 << 10,

        SelectCustom = 16,
        Custom1 = 1 << 11,  // no damage
        Custom2 = 1 << 12,  // light damage
        Custom3 = 1 << 13,  // medium damage
        Custom4 = 1 << 14,  // heavy damage
        Custom5 = 1 << 15   // destroyed
    }

    

    // TODO: I THINK THIS ENTIRE CLASS IS OBSOLETE - THIS WAS BACK WHEN I WAS TESTING CSG PUNCH STENCILS
    //       AND BEFORE I DECIDED IT WAS BETTER TO ALWAYS HAVE ONE GEOMETRY PER MODEL AND TO USE NESTED SWITCHES
    //       IF I WANTED MORE
    
    /// <summary>
    /// This is Switch specifically for Geometry.
    /// 1) It is the base class
    /// for LODSwitch that is based on distance parameter during Selection.
    /// But alone, this switch requires a specific subscript to be specified.
    /// 2) It is a switch for the different Rendering Modes.  So during normal render
    /// we can render for instance a visible Mesh, but during Stencil pass we can
    /// render a Stencil Punch geometry
    /// </summary>
    public class ModelSwitch : ModelSelector // aka SegmentSwitch
    {
        // http://www.codeproject.com/Articles/106884/Implementing-Auto-tiling-Functionality-in-a-Tile-M
        // http://gmc.yoyogames.com/index.php?showtopic=416796

        // when trying to change the appearance (mesh, textures, material) of an existing wall
        // we must be selecting a SegmentSwitch branch.  The underlying entity will never change
        // and we have no ability to change the style... it's always auto calculated. (even inner walls
        // cannot be forced single sided because double sided is required to have different appearance

        // SegmentSwitch holds the geometry info, but not the Appearance info.
        // This is annoying.  It would've been better to have Appearance associated with
        // each child....
        // However, I think our original rational is that appearance should never be tied to 
        // Geometry since different instances of same geometry can have different visuals.
        // 
        // That is why our Entity stores the last used appearance flags on it's geometry
        // so we know if we have to update the appearance of any selected Geometry prior to
        // rendering.
        //
        // So for walls for example, we simply need to enforce a specification that
        // walls have a specific set of child geometry under the SegmentSwitch.
        // LOD's are supported for each child.
        // The entity must contain an Appearance node that matches the SegmentSwitch
        // 
        // Our "wall placer" mode in PlacementTool must then do two things... it must
        // use a default wall entity and not any specific one it picks up... and then
        // it must use a geometry and material (texture & material) "brush" for setting the
        // visual appearance.  

        // WHat if we alter our Geometry edit plugin panel to also include editing of the 
        // appearance?  Specifically, what if we had a redesigned gui so we could edit
        // them side by side?  The only confusing part is to not make it seem as if they are
        // tied to one another.  You could conceivably use entirely different AppearanceSwitch
        // layouts on the same geometry layout.

        // TODO: should be a flag set in the Pass paramater to Select() i think
        bool mForceTransitions; // when switching between child indices, apply a transition
        private SwitchMode[] _modes; // TODO: in the new implementation, these modes will be model flags, not switch node vars


        internal ModelSwitch(string id)
            : base(id)
        {
            Shareable = false;  // transformable derived nodes can never be shared.
            this.mStyle = SelectorNodeStyle.Switch;
        }

        internal override ChildSetter GetChildSetter()
        {
            throw new NotImplementedException();
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
            Settings.PropertySpec[] properties = new Settings.PropertySpec[1 + tmp.Length];
            tmp.CopyTo(properties, 1);

            properties[0] = new Settings.PropertySpec("switchmodes", typeof(string).Name);
            if (!specOnly)
            {
                string result = "";
                if (_modes != null && _modes.Length > 0)
                {
                    string[] csv = new string[_modes.Length];
                    for (int i = 0; i < csv.Length; i++)
                        csv[i] = ((int)_modes[i]).ToString();

                    result = string.Join(",", csv);
                }
                properties[0].DefaultValue = result;
            }

            return properties;
        }

        // TODO: this should return any broken rules ?
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
                    case "switchmodes":
                        string tmp = (string)properties[i].DefaultValue;
                        if (string.IsNullOrEmpty(tmp) == false)
                        {
                            string[] vals = tmp.Split (new string[] {","}, StringSplitOptions.None);
                            _modes = new SwitchMode[vals.Length];
                            for (int j = 0; j < _modes.Length; j++)
                                _modes[j] = (SwitchMode)int.Parse(vals[j]);
                        }
                        break;
                }
            }
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="child"></param>
        /// <remarks></remarks>
        public void AddChild(Geometry child)
        {
            // TODO: the following line is obsolete because now appearances are per Model and not shared
            // across multiple geometries
            //if (ChildGeometryIsConsistant(child) == false) throw new ArgumentException("All LODs must be of same Geometry Type. eg. All actors, all meshes/billboards, or all Particle Systems");

            base.AddChild(child);

            System.Diagnostics.Debug.Assert(mChildren.Count > 0);
            
            if (_modes == null || _modes.Length == 0)
            {
                _modes = new SwitchMode[mChildren.Count];
                // NOTE: We _only_ set a default switchmode if 
                // there existed non previously.  Otherwise we are careful
                // not to overwrite any mode's deserialzed from xml db
                for (int i = 0; i < _modes.Length; i++)
                    _modes[i] = SwitchMode.Geometry;
            }
            else if (_modes.Length < mChildren.Count)
            {
                // we need to expand _childModes array but 
                // we do not want to overwrite any existing values
                SwitchMode[] tmp = new SwitchMode[mChildren.Count];
                Array.Copy(_modes, tmp, tmp.Length - 1);
                _modes = tmp;
                // TODO: how do i restore prefabs saved modes during deserialization?!
                _modes[mChildren.Count - 1] = SwitchMode.Geometry; // default is geometry
            }
            else
            {
                // no need to expand.  MOREOVER, even if _childModes is bigger than _children.Count
                // we _do_not_shrink childModes array.  This is because during deserialization
                // more child nodes may be added.
                _modes[mChildren.Count - 1] = SwitchMode.Geometry;
            }
            
            //SetChangeFlags (Keystone.Enums.ChangeStates.CSG...and .GeometryAdded ? 
            SetChangeFlags(Keystone.Enums.ChangeStates.BoundingBoxDirty, Keystone.Enums.ChangeSource.Child);
            
            // if we're connected, we need to notify the Entity.  Or is there a way such that
            // an Entity can query us directly?
            // Well, we can set flags of our own, and then the entity CAN query those
            // rather quickly via property check.
            //
        }

        // obsolete : Since now every Model can have it's own appearance, there is no need to
        // try and enforce that a given model lod's appearance can also be used with another 
        //private bool ChildGeometryIsConsistant(Geometry geometry)
        //{
        //    // TODO: does this fail if the Geometry is a nested lOD?
        //    // it doesn't have to.  we simply have to recurse it and make sure the ultimate
        //    // children are the same.
        //    Type t = geometry.GetType();

        //    foreach (Node child in _children)
        //    {
        //        if (child is Geometry)
        //            if (child.GetType() != t) return false;
        //    }

        //    return true;
        //}

        public override void RemoveChild(Node child)
        {
            // which index child is this?
            int index = -1;
            for (int i = 0; i < mChildren.Count; i++)
                if (mChildren[i] == child)
                {
                    index = i;
                    break;
                }
            System.Diagnostics.Debug.Assert(index >= 0,"Child node does not exist.");
            base.RemoveChild(child);

            // remove corresponding index in _childModes
            if (mChildren == null || mChildren.Count == 0)
                _modes = null;
            else
            {
                SwitchMode[] tmp = new SwitchMode[mChildren.Count];
                for (int i = index; i < mChildren.Count; i++)
                {
                    _modes[i] = _modes[i + 1];
                }

                Array.Copy(_modes, tmp, mChildren.Count);
                _modes = tmp;
            }
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="options"></param>
        ///// <returns></returns>
        // public override Model[]  Select(SwitchNodeOptions options)
        // {
        //    // recurses any child selector nodes in case of nested but only returns Models.
        //    throw new NotImplementedException();

        //    if (_children == null) return null;

        //    List<Model> results = new List<Model>();

        //    foreach (Node child in _children)
        //    {
        //        if (child is Model)
        //        {
        //            results.Add((Model)child);
        //        }
        //        else if (child is ModelSelector)
        //        {
        //            results.AddRange (((ModelSelector)child).Select(options));
        //        }
        //    }

        //    if (results.Count == 0) return null;
        //    return results.ToArray();
        //}

        //public Geometry Select(uint index)
        //{
        //    if (_children == null) return null;
        //    if (index + 1 > _children.Count) return null;
        //    return (Geometry)_children[(int)index];
        //}

        //public Geometry Select(SwitchNodeOptions options, out int selectedIndex)
        //{
        //    selectedIndex = -1;
        //    if (_children == null || _children.Count == 0) return null;

        //    // TODO: switch between a segment type.  To do this
        //    // the parent Entity must have a segment style set and here
        //    // we would select between styles.  I think having a dedicated
        //    // SegmentSwitch node would be better since there we could maybe still have
        //    // nested LODs for the different segments.
        //    switch (options.Mode)
        //    {
        //        case SwitchMode.Geometry :
        //            // find the first Geometry child
        //            for (int i = 0; i < _children.Count; i++)
        //                if (_modes[i] == SwitchMode.Geometry)
        //                    return (Geometry)_children[i];
        //            break;
        //        case SwitchMode.CSGStencil :
        //            for (int i = 0; i < _children.Count; i++)
        //                if (_modes[i] == SwitchMode.CSGStencil)
        //                    return (Geometry)_children[i];
        //            break;
        //    }
        //    //if (SelectedLOD > -1)
        //    //    selectedChild = (Geometry)_lod.Children[SelectedLOD];
        //    //else
        //    //    selectedChild = (Geometry)_lod.Children[0];

        //    return null;
        //}

        #region IBoundVolume Members
        /// <summary>
        /// GeometrySwitch nodes require special overridden BoundingVolume calcs.
        /// </summary>
        protected override void UpdateBoundVolume()
        {

            DisableChangeFlags(Keystone.Enums.ChangeStates.BoundingBox_TranslatedOnly | Keystone.Enums.ChangeStates.BoundingBoxDirty);
            if (mChildren == null || mChildren.Count == 0) return; 

            // TODO: can we perform a select here instead to retreive desired children and then
            // iterate through them and combine their boxes?  Because below we are always
            // returning after the first child is found in the switch.
            
            // this.Select (SelectionMode.BoundsUpdate, null, null);
            
            for (int i = 0; i < mChildren.Count; i++ )
            {
                if (mChildren[i] is Model)
                {
                    mBox = ((Model)mChildren[i]).BoundingBox;
                    break; // we return after first found
                    
                }
                else if (mChildren[i] is ModelSwitch) // NOTE: SequenceSwitch needs to take combined volume of all child geometry
                                                         // so we'll need to override this implementation
                {
                    // a nested GeometrySwitch/LODSwitch.
                    // So this is somewhat recursive.  We still want the first
                    // geometry we come to
                    mBox = ((ModelSwitch)mChildren[i]).BoundingBox;
                    if (mBox != null) break; // return after first found
                }
            }

            if (mBox != null)
            {
                mSphere = new BoundingSphere(mBox);
            }
        }
        #endregion
    }
}