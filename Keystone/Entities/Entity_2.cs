using System;
using System.Collections.Generic;
using System.Diagnostics;
using Keystone.Elements;
using Keystone.Enum;
using Keystone.FX;
using Keystone.IO;
using Keystone.Portals;
using Keystone.Resource;
using Keystone.Traversers;
using Keystone.Types;
using MTV3D65;
using Keystone.Collision;
using Keystone.Extensions;
using KeyScript.Routes;
using KeyCommon.Flags;

namespace Keystone.Entities
{ 
 
	/// <summary>
	/// 
	/// </summary>
	/// <remarks>
	/// IPageableTVNode is for loading our Entity Script, but is also useful for loading Region's which inherit Entity
	/// </remarks>
    public abstract class Entity : BoundTransformGroup, IFXSubscriber, IPageableTVNode 
    {
        private const ushort DEFAULT_RENDER_PRIORITY = 128; // using a default value > 0 allows room to temporarily decrease
        //  a render priority rather than always be forced to raise 
        // the priority of other entities

        protected ushort mRenderPriority = DEFAULT_RENDER_PRIORITY; // todo: we are not saving this to xml, should we be?
        protected KeyCommon.Flags.EntityFlags  mEntityFlags;
        protected string mReferencedEntityID; // id of the entity this entity references when using as proxy
        // can mRefID also be useful when having a bunch of entities that are meant
        // to be defined by a single entity in the xmldb such that when it is changed
        // anywhere from within it's hiearchy, they are changed too?

        protected double _maxVisibleDistanceSq = -1;

  
      	protected Scene.Scene mScene; // a reference to the scene this entity is part of
        protected SceneNode mSceneNode;
        protected Behavior.Behavior mBehaviorRoot;
        protected Behavior.BehaviorContext mBehaviorContext;
        protected Animation.Animation[] mAnimations;
        protected Animation.AnimationController mAnimationController;
        

        protected DomainObjects.DomainObject mScript;
        protected uint _customFlags;  // these are flags scripts can define and interpret freely
        // different games built on KGB can have
        // different sets of flags for it's components
        System.Collections.Generic.Dictionary<string, object> mCustomPropertyValues;
        private string mCustomPropertyValuesPersistString;
        private string mCustomPropertyTypesPersistString;
        private string mCustomPropertyNamesPersistString;
        

        protected KeyCommon.Data.UserData mCustomData;
        protected CellFootprint mFootprint;

        protected Vector3i mTileLocation; // if parent is celledRegion then this value will contain the x,y,z offsets
                                          // for the footprint center. 

      	// IPageablve vars - domain objects are main data thats pageable, although Terrain Entities have heightfield data
        protected string mDomainObjectScriptResource;
        protected readonly object mSyncRoot;
        protected PageableNodeStatus _resourceStatus;

        #region JigLibX
       // public JigLibX.Physics.Body PhysicsBody;
        #endregion


        protected Entity(string id)
            : base(id)
        {
        	mSyncRoot = new object(); // for loading script and in case of Terrains, loading heightfield
        	
            // all entities are unique and cannot be shared thus it's ok to NOT have a static constructor for them.
            // because there will never be a need to search for an existing and then call IncrementRef on it.
            Pickable = true;
            Shareable = false; // entities can never be shared, only copied

            //System.Diagnostics.Debug.WriteLine ("Entity.ctor() - Entity flags = " + (uint)_entityFlags);
            // set any default flags
            SetEntityFlagValue((uint)EntityFlags.VisibleInGame, true);
            SetEntityFlagValue((uint)EntityFlags.Overlay, false);
			SetEntityFlagValue((uint)EntityFlags.AutoGenerateFootprint, false);

            // todo: flags for indicating the entity's load status regarding resources like meshes and textures?
            //       - problem is hiearchical nature of entities.  do we only look at non entity descendants? or
            //		 - i suppose why not all but have different flags?
            //       - in this way we can actually poll for the state in some cases like in assetplacmeent tool
            // - LoadedAll = LoadedScripts | LoadedGeometry | LoadedTextures | LoadedFX | LoadedChildEntities
            //		- how do we manage these flags in the entity?  
            // - add as a 8 bit flag?


            // are these ok by default?
            // non-modeled entities (having no representation in the game world) have no physical 
            // capabilities by default.  However, for trigger volume entities, collisionEnabled can
            // be set.
            SetEntityFlagValue((uint)EntityFlags.Dynamic, false); 
            SetEntityFlagValue((uint)EntityFlags.CollisionEnabled, false); 
        }


        #region ITraversable Members
        public override object Traverse(ITraverser target, object data)
        {
            throw new Exception("Entity.Traverse() - This method must be overriden by derived types.");
        }
        #endregion 

        internal override ChildSetter GetChildSetter()
        {
            throw new NotImplementedException();
        }

        #region ResourceBase members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="specOnly">True returns the properties without any values assigned</param>
        /// <returns></returns>
        public override Settings.PropertySpec[] GetProperties(bool specOnly)
        {

            Settings.PropertySpec[] tmp = base.GetProperties(specOnly);
            Settings.PropertySpec[] properties = new Settings.PropertySpec[8 + tmp.Length];
            tmp.CopyTo(properties, 8);

            properties[0] = new Settings.PropertySpec("entityflags", typeof(uint).Name);
            properties[1] = new Settings.PropertySpec("attachedtoboneid", typeof(int).Name);
            properties[2] = new Settings.PropertySpec("tilelocation", typeof(Vector3i).Name);
			properties[3] = new Settings.PropertySpec ("scriptresource", typeof (string).Name);
            // custom property values that are unique to each entity
            // note: custom property names/types/default values are defined in our DomainObjectScript node so
            // until that script is loaded, we cant seperate them out into seperate mCustomProperties
            // so instead we store them as a single xml attribute "customproperties" with the attribute
            // value consisting of a single comma seperate value list 
            properties[4] = new Settings.PropertySpec("custom_property_names", typeof(string).Name);
            properties[5] = new Settings.PropertySpec("custom_property_types", typeof(string).Name);
            properties[6] = new Settings.PropertySpec("custom_property_values", typeof(string).Name);
            properties[7] = new Settings.PropertySpec("footprint", typeof(string).Name);

            if (!specOnly)
            {
                properties[0].DefaultValue = (uint)mEntityFlags;
                properties[1].DefaultValue = AttachedToBoneID;
                properties[2].DefaultValue = mTileLocation;
                properties[3].DefaultValue = mDomainObjectScriptResource;             
                properties[4].DefaultValue = mCustomPropertyNamesPersistString;
                properties[5].DefaultValue = mCustomPropertyTypesPersistString;
                properties[6].DefaultValue = mCustomPropertyValuesPersistString;

                if (mFootprint == null)
                    properties[7].DefaultValue = null;
                else
                    properties[7].DefaultValue = mFootprint.ID;
            }

            return properties;
        }
        
//        public override void SetProperties(Settings.PropertySpec[] properties, bool validate, bool raiseChangeEvent, out int[] brokenCodes)
//        {
//        	brokenCodes = null; // init
//        
//        	if (properties == null || properties.Length == 0) return;
//        	
//        	// validation test will occur prior to setting any value
//        	if (validate && mDomainObject != null)
//            {
//                for (int i = 0; i < properties.Length; i++)
//                {
//                    System.Diagnostics.Debug.Assert(mCustomPropertyValues.ContainsKey(properties[i].Name));
//                    // NOTE: if any rule fails for any property, we return and do NOT apply ANY of them
//                    // todo: server should also track validation fail frequency
//                    bool result = false;
//                    if (mDomainObject != null)
//                    {
//                        result = mDomainObject.RulesValidate(properties[i].Name, properties[i].DefaultValue, out brokenCodes);
//                        // property will not be changed because rules validation failed
//                        // todo: do we want to abort all property changes if any single one in the array fails?
//                        // if we think of a "form" that asks for user info, normally you would only fail the ones
//                        // that failed and continue to process the rest...  however, our "brokencodes" array is designed
//                        // to be one array per property so we'd need a jagged array 
//                        if (result == false) return;
//                    }
//                }
//            }
//        	
//        	// todo: do not set values for those properties that have any broken codes set
//        	this.SetProperties (properties);
//        	
//        	if (raiseChangeEvent) 
//        	{
//	            for (int i = 0; i < properties.Length; i++)
//	            {
//	            	// todo: no need to raise change events for those properties that have broken codes set
//	            	
//	                // todo: we want to ensure we dont wind up with cyclic property changes when
//	                //       script changes one property after another has been changed which causes
//	                //       the OnCustomPropertyChanged to fire yet again.
//	                // change property and notify script that this property has changed
//	                // then script can set a flag based on the property or category of the property
//	                // SetCustomFlagValue (
//	                // so that on next update we can do a dirty update of component's state
//	                // and not certain parts of the components state if unnecessary.
//	                
//	                // raise the change event if it exists
//	                KeyScript.Events.Event e = null;
//	                if (mDomainObject.Events.TryGetValue(properties[i].Name, out e))
//		                e.Invoke (this.ID);
//	
//	
//	                // note: generally, if a script is modifying it's own properties, no change event
//	                // need be raised.  But if the property values are being modified outside of the entity, then
//	                // the event does need to be raised.  This event helps us avoid polling custom property values for changes
//	                // that the script would need to respond to.
//	                // note: however, what the changeevent typically does is set a flag so that the final response
//	                // can be done in Update().  This way multiple changes to various properties will still only result in
//	                // one response per tick. But the script is responsible for setting the flags because we dont want
//	                // to hardcode them.
//	            }  
//        	}
//        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// SetProperties does not need to do validation or rules checking because these properties
        /// are not game specific.  These are engine properties predominaly.  Furthermore,
        /// translation, scale and rotation are not modified here for real-time purposes. The physics engine
        /// and simulation engine does that through direct internal accessors.  This is also good for performance.
        /// Thus, change events that need to be raised in script during entity transforms will be done directly from those
		/// accessors.         
        /// </remarks>
        /// <param name="properties"></param>
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
                    case "entityflags":
                        mEntityFlags = (EntityFlags)(uint)properties[i].DefaultValue;
                        // todo: Dec.11.2013 temp hack to force AutoGenerateFootprint == false until this flag is properly initialized in all old prefabs created prior to EntityFlags rearrangement
                        SetEntityFlagValue((uint)EntityFlags.AutoGenerateFootprint, false);
                        
                        break;
                    case "attachedtoboneid":
                        AttachedToBoneID = (int)properties[i].DefaultValue;
                        break;;
                    case "custom_property_names":
                        mCustomPropertyNamesPersistString = (string)properties[i].DefaultValue;
                        break;
                   case "custom_property_types":
                        mCustomPropertyTypesPersistString = (string)properties[i].DefaultValue;
                        break;
                   case "custom_property_values": // changing just values
                        mCustomPropertyValuesPersistString = (string)properties[i].DefaultValue;
                        break;
                    case "custom_properties": // changing via propertyspec collection of parameter values
                        SetShaderParameterValues ((Settings.PropertySpec[])properties[i].DefaultValue);
                    case "tilelocation":
                        mTileLocation = (Vector3i)properties[i].DefaultValue;
                        break;
                    case "scriptresource":
                        mDomainObjectScriptResource = (string)properties[i].DefaultValue;
                        // todo: should i force load script here?  problem is... LoadTVResource() might load
                        // different things on a entity....
                        break;
                    case "footprint":
                        string footprintID = (string)properties[i].DefaultValue;
                        //if the footprintID is null, we can delete the footprint since it's null
                        if (string.IsNullOrEmpty(footprintID))
                        {
                            if (mFootprint != null)
                                RemoveChild(mFootprint);
                        }
                        else
                        {
                            if (mFootprint != null)
                            {
                                if (mFootprint.ID == footprintID)
                                    continue;

                                // footprint resource needs to be changed
                                RemoveChild(mFootprint);
                            }
                            try
                            {
                                // attemp to decode the new footprint
                                CellFootprint fp = CellFootprint.Create(footprintID);
                                if (fp.Data == null) throw new Exception("Error decoding footprint");
                                                                
                                AddChild(fp);
                            }
                            catch (Exception ex)
                            {
                                // footprint decode fails, catch exception but do nothing. 
                                // mFootprint is already null by the time we've reached this
                                System.Diagnostics.Debug.WriteLine ("Entity.SetProperties() - Footprint error: " + ex.Message );
                            }
                        }
                        break;
                }
            }
        }

        public virtual bool GetEntityFlagValue(uint flag)
        {
        	EntityFlags f = (EntityFlags)flag;
        	
        	return (mEntityFlags & f) != 0;
        }

        public virtual bool GetEntityFlagValue(string flagName)
        {
            switch (flagName)
            {
                case "visible":
                    return (mEntityFlags & EntityFlags.VisibleInGame) == EntityFlags.VisibleInGame;
                case "pickable":
                    return (mEntityFlags & EntityFlags.PickableInGame) == EntityFlags.PickableInGame;
                case "collidable":
                    return (mEntityFlags & EntityFlags.CollisionEnabled) == EntityFlags.CollisionEnabled;
                case "dynamic":
                    return (mEntityFlags & EntityFlags.Dynamic) == EntityFlags.Dynamic;
                case "awake":
                    return (mEntityFlags & EntityFlags.Awake) == EntityFlags.Awake;
                case "overlay":
                    return (mEntityFlags & EntityFlags.Overlay) == EntityFlags.Overlay;
                case "laterender":
                    return (mEntityFlags & EntityFlags.LateRender) == EntityFlags.LateRender;
                case "autogenfootprint":
                    return (mEntityFlags & EntityFlags.AutoGenerateFootprint) == EntityFlags.AutoGenerateFootprint;
               case "terrain":
                    return (mEntityFlags & EntityFlags.Terrain) == EntityFlags.Terrain;
                default:
#if DEBUG
                    throw new ArgumentOutOfRangeException("Entity flag '" + flagName + "' is undefined.");
#endif
                    return false;
					break;
            }
        }

        public virtual void SetEntityFlagValue(uint flag, bool value)
        {

        	EntityFlags f = (EntityFlags)flag;
            //throw new ArgumentOutOfRangeException("Entity flag '" + flag.ToString() + "' is undefined.");
            if (value)
                mEntityFlags |= f;
            else
                mEntityFlags &= ~f;            
        }

        public virtual void SetEntityFlagValue(string flagName, bool value)
        {
            switch (flagName)
            {
                case "visible":
                    SetEntityFlagValue((uint)EntityFlags.VisibleInGame, value);
                    break;
                case "pickable":
                    SetEntityFlagValue((uint)EntityFlags.PickableInGame, value);
                    break;
                case "collidable":
                    SetEntityFlagValue((uint)EntityFlags.CollisionEnabled, value);
                    break;
                case "dynamic":
                    SetEntityFlagValue((uint)EntityFlags.Dynamic, value);
                    break;
                case "awake":
                    SetEntityFlagValue((uint)EntityFlags.Awake, value);
                    break;
                case "overlay":
                    SetEntityFlagValue((uint)EntityFlags.Overlay, value);
                    break;
                case "laterender":
                    SetEntityFlagValue((uint)EntityFlags.LateRender, value);
                    break;
				case "autogenfootprint":
	                SetEntityFlagValue((uint)EntityFlags.AutoGenerateFootprint, value);
                    break;
               case "terrain":
                    SetEntityFlagValue ((uint)EntityFlags.Terrain, value);
               		break;     
                default:
                    throw new ArgumentOutOfRangeException("Entity flag '" + flagName + "' is undefined.");
            }
        }
        #endregion

        #region GameObject properties and methods
        public uint UserTypeID // todo: obsolete? used only by IEntityAPI.GetComponentsOfType()
        	                   // also script's already have QueryComponentType() so if anything mDomainObject.UserTypeID should return that
        {
            get 
            {
                if (mScript != null)
                    return mScript.UserTypeID;

                return 0;
            }
        }

        public KeyCommon.Data.UserData CustomData 
        {
        	get 
        	{
        		return mCustomData ;
        	}
        	set 
        	{
        		mCustomData = value;
        	}
        }
        
        // todo: changing the underlying DomainObject must clear and re-init the mCustomPropertyValues collection
        public Settings.PropertySpec[] GetCustomProperties(bool specOnly)
        {
            if (mScript == null) return null;

            Settings.PropertySpec[]  specs = mScript.CustomProperties;

            if (specs == null) return null;

            if (!specOnly)
                for (int i = 0; i < specs.Length; i++)
                {
                    object defaultValue = specs[i].DefaultValue;
                    specs[i].DefaultValue = GetCustomPropertyValue(specs[i].Name);
                    if (specs[i].DefaultValue == null)
                        specs[i].DefaultValue = defaultValue;
                }
            return specs;
        }

        private Settings.PropertySpec[] GetCustomPropertiesFromPersistStrings (string names, string types, string values)
       	{
        	if (string.IsNullOrEmpty (names) || string.IsNullOrEmpty(types) || string.IsNullOrEmpty(values)) return null;
        	
        	string[] delimitedNames = names.Split(new string[]{keymath.ParseHelper.English.XMLAttributeNestedDelimiter}, StringSplitOptions.None);
        	string[] delimitedTypes = types.Split(new string[]{keymath.ParseHelper.English.XMLAttributeNestedDelimiter}, StringSplitOptions.None);
        	string[] delimitedValues = values.Split(new string[]{keymath.ParseHelper.English.XMLAttributeNestedDelimiter}, StringSplitOptions.None);
        	
        	System.Diagnostics.Debug.Assert (delimitedNames.Length == delimitedTypes.Length && delimitedNames.Length == delimitedValues.Length);
        	
  
        	Settings.PropertySpec[] specs = new Settings.PropertySpec[delimitedValues.Length];
        	for (int i = 0; i < specs.Length; i++)
        	{
        		specs[i] = new Settings.PropertySpec(delimitedNames[i], delimitedTypes[i]);
        		specs[i].DefaultValue = Helpers.ExtensionMethods.ReadXMLAttribute(specs[i].TypeName, delimitedValues[i]);
        	}
        	
        	 
        	return specs;
        
        }

       	public Settings.PropertySpec[] GetCustomProperties()
       	{
       		return GetCustomPropertiesFromPersistStrings (mCustomPropertyNamesPersistString, mCustomPropertyTypesPersistString, mCustomPropertyValuesPersistString);
       	}
        
        // shader parameters are handled the same was as custom domainobject parameters
        public object GetCustomPropertyValue(string parameterName)
        {
            if (mCustomPropertyValues == null) return null;

            //System.Diagnostics.Debug.Assert(mParameterValues.ContainsKey(parameterName));
            object result = null;
            bool found = mCustomPropertyValues.TryGetValue(parameterName, out result);

            return result;
        }

        // todo: our hud will sometimes set these one at a time for a shader like our tilemask UV shader
        // and eventually we'll wind up trying to apply those shader values while we're still adding them
        // and the mShaderParameterValues will throw an exception that colleciton was modified.  It'd be nice
        // if we could make the list of values an array, but we need key value pairs
        public void SetCustomPropertyValue(string parameter, object value)
        {
            if (mCustomPropertyValues == null) mCustomPropertyValues = new Dictionary<string, object>();

            mCustomPropertyValues[parameter] = value;
            
            
            // TODO: we don't want to set this flag whenever a parameter has changed, I think mostly we care
            // if all parameters have been deserialized or changed in a similar "batch" manner... but runtime
            // parameter changes shouldn't need this i dont think... it's needless overhead.  we do still need
            // to be able to change parameters if necessary between instances, but that's a different concept
            SetChangeFlags(Enums.ChangeStates.ShaderParameterValuesChanged, Enums.ChangeSource.Self);
        }


        
        // shader parameters are handled similarly to Entity's domain object custom properties
        // however unlike DomainObject, here we do not validate using rules
        public void SetCustomPropertyValues(Settings.PropertySpec[] parameters)
        {
            if (mCustomPropertyValues == null) mCustomPropertyValues = new Dictionary<string, object>();

            for (int i = 0; i < parameters.Length; i++)
            {
                //System.Diagnostics.Debug.Assert(mParameterValues.ContainsKey(parameters[i].Name));
                // NOTE: We do not assign shader parameter values to shader itself.  Those are 
                // done on Appearance.Apply()
                mCustomPropertyValues[parameters[i].Name] = parameters[i].DefaultValue;                
            }

            // HACK: modify each of the three ersist strings after assigning new parameter values via propertyspec[]
            // WARNING: this overrides any and all previous existing persist Name, Types, and Values!
            mCustomPropertyNamesPersistString = Helpers.ExtensionMethods.CustomPropertyNamesToString (parameters);
            mCustomPropertyTypesPersistString = Helpers.ExtensionMethods.CustomPropertyTypesToString (parameters);
            mCustomPropertyValuesPersistString = Helpers.ExtensionMethods.CustomPropertyValuesToString (parameters);
            
            // assert all persist strings have same count of items in the delimited strings
            
            // TODO: we don't want to set this flag whenever a parameter has changed, I think mostly we care
            // if all parameters have been deserialized or changed in a similar "batch" manner... but runtime
            // parameter changes shouldn't need this i dont think... it's needless overhead.  we do still need
            // to be able to change parameters if necessary between instances, but that's a different concept
            SetChangeFlags(Enums.ChangeStates.ShaderParameterValuesChanged, Enums.ChangeSource.Self);
        }

        public void SetCustomPropertyValues(string[] propertyNames, object[] values, bool validate, bool raiseChangeEvent, out int[] brokenCodes)
        {

            System.Diagnostics.Debug.Assert(mScript != null, "Entity.SetCustomPropertyValues() - ERROR: DomainObject is null!"); 
            System.Diagnostics.Debug.Assert(mCustomPropertyValues != null);
            
            if (propertyNames == null || values == null)  throw new ArgumentNullException();
            if (propertyNames.Length != values.Length) throw new ArgumentException();
            
            brokenCodes = null;
                        	
            if (validate)
            {
                for (int i = 0; i < propertyNames.Length; i++)
                {
                	// NOTE: below assert is incorrect. Some custom properties we can flag as "not serializable" and so
                	//       when first instantiating the entity, it will not have the non serializable properties' values in
                	//       mCustomPropertyValues
                    //System.Diagnostics.Debug.Assert(mCustomPropertyValues.ContainsKey(propertyNames[i]));
                    // NOTE: if any rule fails for any property, we return and do NOT apply ANY of them
                    // todo: server should also track validation fail frequency
                    bool result = false;

                    // todo: ROUTES should be able to work similar to our rules
                    // rules are run using a delegate that was specified in the script
                    // that is why no script.Execute() has to be called
                    result = mScript.RulesValidate(propertyNames[i], values[i], out brokenCodes);
                    // property will not be changed because rules validation failed
                    if (result == false) return;   
                }
            }

            // still here.  Validation of all properties succeeded.  Let's apply the values
            // todo: when / how do i update any stats calcs?  Since here this apply doesnt actually
            // invoke any script code?   Yes we do want a "change".  yes we would like to set a sort
            // of dirty flag so that we can run an update when it's time to access changed properties
            // (eg when the script tries to access the properties on Update(), it can determine that
            // build flags have changed and so recompute build statistics) but then not have these
            // validated.
            for (int i = 0; i < propertyNames.Length; i++)
            {
                // todo: we want to ensure we dont wind up with cyclic property changes when
                //       script changes one property after another has been changed which causes
                //       the OnCustomPropertyChanged to fire yet again.
                // change property and notify script that this property has changed
                // then script can set a flag based on the property or category of the property
                // SetCustomFlagValue (
                // so that on next update we can do a dirty update of component's state
                // and not certain parts of the components state if unnecessary.
                
                // NOTE: below assert is incorrect. Some custom properties we can flag as "not serializable" and so
            	//       when first instantiating the entity, it will not have the non serializable properties' values in
            	//       mCustomPropertyValues
                //System.Diagnostics.Debug.Assert(mCustomPropertyValues.ContainsKey(propertyNames[i]));
                // assign the validated value
                if (mCustomPropertyValues.ContainsKey (propertyNames[i]))
	                mCustomPropertyValues[propertyNames[i]] = values[i];
				else 
					mCustomPropertyValues.Add (propertyNames[i], values[i]);
				
                // route any changes to properties that have routes
                // Routes are automaticly tied to changes rather than having to script it.  
                // In effect, it allows us to add triggers without having to require a script be written
                RouteRaiseEvent(propertyNames[i], values[i]);

                // note: generally, if a script is modifying it's own entity's custom properties, no change event
                // need be raised.  But if the custom property values are being modified outside of the entity, then
                // the event does need to be raised.  This event helps us avoid polling custom property values for changes
                // that the script would need to respond to.
                // note: however, what the changeevent typically does is set a flag so that the final response
                // can be done in Update().  This way multiple changes to various properties will still only result in
                // one response per tick. But the script is responsible for setting the flags because we dont want
                // to hardcode them.
                if (raiseChangeEvent == false) continue; 

                mScript.EventRaise (this.ID, propertyNames[i]);
                            	
               // int index = mDomainObject.GetCustomPropertyIndex(propertyNames[i]);
               // if (index < 0) throw new Exception("Entity.SetCustomPropertyValue() - Custom Property '" + propertyNames[i] + "' not found.");
               // mDomainObject.Execute("OnCustomPropertyChanged", new object[] { this.ID, index });
            }
        }


        // todo: should SetProperties return list of broken rules?
        // or should we have to call GetBrokenRules()
        public void SetCustomPropertyValues(Settings.PropertySpec[] properties, bool validate, bool raiseChangeEvent, out int[] brokenCodes)
        {
            // TODO: THis will break when we try to assign waypoint when accidentally clicking
            // "GOTO" hud menu item for a Vehicle that has no DomainObject script assigned to it.
            brokenCodes = null;

            if (properties == null || properties.Length == 0) return;

            string[] propertyNames = new string[properties.Length];
            object[] values = new object[propertyNames.Length];

            for (int i = 0; i < propertyNames.Length; i++)
            {
                propertyNames[i] = properties[i].Name;
                values[i] = properties[i].DefaultValue;
            }

            SetCustomPropertyValues(propertyNames, values, validate, raiseChangeEvent, out brokenCodes);
        }

        
        public void SetCustomPropertyValue(string customPropertyName, object value, bool validate, bool raiseChangeEvent, out int[] brokenCode)
        {
        	int[] dummy;
        	SetCustomPropertyValues (new string[]{customPropertyName}, new object[]{value}, validate, raiseChangeEvent, out dummy);
        	brokenCode = dummy;
        }


        /// <summary>
        /// Custom flag values are set (optionally) by the DomainObjectScript and are not saved in XML.
        /// </summary>
        /// <param name="flag"></param>
        /// <param name="value"></param>
        public void SetCustomFlagValue(uint flag, bool value)
        {
            if (value)
                _customFlags |= flag;
            else
                _customFlags &= ~flag;
        }

        public bool GetCustomFlagValue(uint flag)
        {
            return ((_customFlags & flag) == flag);
        }



        protected Dictionary<string, RouteSet> mEventListeners;

        // todo: AddRoute should be done through a NetMessage that is typically
        //       done via our editor gui when the routing of one field in one entity
        //       to a field in another entity is done.
        public void AddRoute(string propertyName, Route route)
        {
            if (mEventListeners == null) mEventListeners = new Dictionary<string, RouteSet>();

            route.Source.Entity = this;
            System.Diagnostics.Debug.Assert(route.Source.EntityID == this.ID);

            RouteSet set;
            bool setExists = mEventListeners.TryGetValue(propertyName, out set);

            if (setExists == false)
            {
                set = new RouteSet();
                mEventListeners.Add(propertyName, set);
            }

            set.AddRoute(route);
        }

        // todo: how do we restore route entity target references when entities are
        //       added or removed?  could we tie it into the Repository somehow?
        //       can all "routes" be stored in some global handler?  i dont really like that
        //       though because the current way we automatically have the starting source
        //       and then can easily just find if a route exists locally in the mEventListeners
        //       Can the RouteEndpoints be watching perhaps?
        public void RouteRaiseEvent(string propertyName, object value)
        {
            if (mEventListeners == null) return;
            if (!(mEventListeners.ContainsKey(propertyName))) 
                return;

            // get all routes for this property
            Route[] routes = mEventListeners[propertyName].Routes;

            if (routes == null || routes.Length == 0)
                return;

            // execute the routing from source to target
            for (int i = 0; i < routes.Length; i++)
            {
                // todo: here we need to use direct references to entities
                // but they should be stored where?  not in the Route itself
                // for animations we have a thing called a "AnimationController.cs"
                // that ReMaps entities within a "Track" where the "Track" represents a
                // per instance playback state as opposed to the shared animation itself.
                // So what are the parallels here?  Our "scripts" register Routes and these routes
                // cannot be removed because they are inherent to the specific Entity.  But the
                // resulting source and endpoints can be and so can the mapping to the actual
                // entity references and methods.

                // note: we do not validate or initiate raiseChangeEvent when the target's property
                // is changed due to a ROUTE?
                bool validate = false;
                bool raiseChangeEvent = false;
                int[] errorCodes;

                if (routes[i].Target.Entity != null)
                {
                    Entity target = (Entity)routes[i].Target.Entity;
                    target.SetCustomPropertyValues(
                        new string[] { routes[i].Target.MethodName },
                        new object[] { value },
                        validate,
                        raiseChangeEvent,
                        out errorCodes);
                }
            }
        }
    #endregion

        #region IPageableTVNode Members
        public object SyncRoot
        {
            get { return mSyncRoot; }
        }

        public virtual int TVIndex
        {
            get { throw new NotImplementedException(); }
        }

        public virtual bool TVResourceIsLoaded
        {
            get 
            { 
            	// NOTE: some entites don't use domainscript but still have db resources to load
            	// so rather than override, just check for .PageableNodeStatus == Loaded
            	return _resourceStatus == PageableNodeStatus.Loaded;
            }
        }

        public virtual string ResourcePath
        {
            get { if (mDomainObjectScriptResource == null) return "";  return mDomainObjectScriptResource.ToString(); }
            // cannot set domain script manually. Must be done through AddChild() & RemoveChild()
            // or through loading of the var from deserialization of the Entity
            // todo: wait, why not through setting path and then clearing the resourcestatus so LoadTVResource gets called?
            // i know there is a concern that when we try to run domainobject scripts that it is attached to its Entity by then.
            // well i see no reason why this should affect that ever. DO can never be loaded this way until entity.LoadTVResource is called
            // although 
            set { mDomainObjectScriptResource= value;} // throw new Exception(); }
        }

        public PageableNodeStatus PageStatus
        {
            get { return _resourceStatus; } // todo: shouldn't this return the mDomainObject's status instead?
            set { _resourceStatus = value; }
        }

        public virtual void UnloadTVResource()
        {
            // no script node to page out            
            if (mScript == null) return; 
            	
        	Keystone.IO.ClientPager.UnloadTVResource (mScript, false);
        	
            if (mScript.PageStatus != PageableNodeStatus.NotLoaded) 
            	throw new Exception ("Entity.UnLoadTVResourceSynchronously() - Error unloading script '" + mDomainObjectScriptResource + "'");

            // TODO: wont this occur on .RemoveAllChildren() anyway? Why are we removing script here?
            this.RemoveChild(mScript);
        }
        
        // todo: this should return a bool i think
        // todo: should this be "internal" also? sometimes we want to force load though from EXE so maybe cannot
        public virtual void LoadTVResource()
        {

            // if the computed path and the path retreived during GetProperties() 
            // are not the same, we have one of two scenarios
            if (string.IsNullOrEmpty(mDomainObjectScriptResource))
            {
                return;
            }

            try
            {
                // if the EntityScript node is already a child and is the same child
                // we can exit.
                if (mScript != null && mScript.ResourcePath == mDomainObjectScriptResource) 
                	//todo: if LoadTVResource() is called from worker_insert__xx  as opposed to being
                	//      called from Pager.cs, and if the domainobject is already cached, this will return
                	//       and _resourceStatus never gets set since it is only set in Pager.cs.  This could be a 
                	//       more fundamental problem where every call to LoadTVResource() not from Pager.cs is
                	//       not setting the _resourceStatus var properly.  TODO: I should ensure all worker .LoadTVResource() has to
                	//        go through ClientPager.LoadTVResourceSynchronously 
            	{
                	
                	// hack, force set of _resourceStatus and return without continuing on below to try and load in an already loaded script
                    _resourceStatus  = mScript.PageStatus; 
                	return;
            	}
                Keystone.DomainObjects.DomainObject scriptNode = 
                	(DomainObjects.DomainObject)Keystone.Resource.Repository.Create (mDomainObjectScriptResource, "DomainObject");

                Keystone.IO.ClientPager.LoadTVResource (scriptNode, false);
                if (scriptNode.PageStatus != PageableNodeStatus.Loaded) 
                	throw new Exception ("Entity.LoadTVResource() - Error loading script '" + mDomainObjectScriptResource + "'");

                // todo: if this node is "Cloned" then the celledRegion will be 
                this.AddChild(scriptNode);
                
                GetCustomPropertiesFromPersistStrings (mCustomPropertyNamesPersistString, mCustomPropertyTypesPersistString, mCustomPropertyValuesPersistString);
            }
            catch (System.IO.InvalidDataException ex)
            {
                System.Diagnostics.Debug.WriteLine("Entity.LoadTVResource() - " + ex.Message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Entity.LoadTVResource() - " + ex.Message);
            }           
        }

        public virtual void SaveTVResource(string filepath)
        {

        }
        #endregion
        
        /// <summary>
        /// Used primarily by Proxies to indicate the entity they are meant to represent.
        /// </summary>
        public string ReferencedEntityID { get { return mReferencedEntityID; } set { mReferencedEntityID = value; } }
        
        public Scene.Scene Scene 
        { 
            get { return mScene; } 
            internal set 
            { 
                mScene = value;
                //// todo: verify the following recursion is unnecessary because it occurs
                //// during the NotifyListener() EntityAttached and so recursion occurs there.
                //// But i could be wrong and may need to re-enable the below
                //// --
                //// the following results in recursive setting of all sub-entity scene references
                //// This is necessary because when you move a branch that has many sub-entities
                //// and attach it to a new Scene, it would only update the top most entity that was
                //// connected to the new scene and not the sub entities.
                //if (_children != null)
                //    foreach (Node child in _children)
                //        if (child is Entities.Entity)
                //            ((Entity)child).Scene = mScene;
            } 
        }

        public SceneNode SceneNode 
        { 
            get { return mSceneNode; }
            set { mSceneNode = value; }
        }

        public KeyCommon.Flags.EntityFlags  Flags { get { return mEntityFlags; } set { mEntityFlags = value; } }

        public ushort RenderPriority { get { return mRenderPriority; } set { mRenderPriority = value; } }

        public DomainObjects.DomainObject DomainObject { get { return mScript; } }
        
        public Behavior.Behavior Behavior { get { return mBehaviorRoot; } }


        /// <summary>
        /// Direct accessor to any available LODSwitch or Geometry child node. 
        /// No "setter" allowed. note: viewpoints and other non renderable types can have animations.
        /// </summary>
        public virtual Animation.AnimationController Animations
        {
            get
            {
                return mAnimationController;
            }
        }
        
        private bool mFootprintIsDirty = true;
        // todo: these flags should be set-able by script or property i suppose
        private Structure.TILEMASK_FLAGS mAutoGenFootprintFlags = Keystone.Portals.Structure.TILEMASK_FLAGS.COMPONENT_FLOOR_MOUNTED;
        public CellFootprint Footprint 
        {
        	get 
        	{ 
        		bool autoGenerateFP = GetEntityFlagValue ((uint)EntityFlags.AutoGenerateFootprint);
        		if (autoGenerateFP == false) return mFootprint ;
        		       		
        		// NOTE: mFootprint can be null, but for components to be placeable inside of Interiors they are required
	
	    		//Avoid needless re-autogeneration and only update when footprint is dirty (i.e mFootprintIsDirty == true)
	            //       todo: dirty footprint occurs when 
	            //       - component Scale is modified or any of it's child Model's scales are modified 
	            //		 - component is freshly loaded or it's models are finally loaded
	            //       - when in design mode.  How do we know?

		    	if (mFootprint == null || mFootprintIsDirty)
		    	{
		    		// the entity passed in must be at identify
                	CellFootprint footprint =  CellFootprint.Create ((ModeledEntity)this, mAutoGenFootprintFlags);
                		
                	// todo: here if geometry nodes are not fully loaded in, we will not be able to compute a footprint
                	//       so unless we force delayResourceLoading = false 
			    	if (footprint == null) throw new Exception ();
			    	
			    	// always ignore and replace any existing footprint if AutoGenerateFootprint = true from within this function
			    	// use the SetProperty so that the CellFootprint node is properly Added/Removed as child node
			    	SetProperty ("footprint", typeof(string), footprint.ID); 
			    	mFootprintIsDirty = false;
		    	}
        		return mFootprint;
        	}
        }


        /// <summary>
        /// The max distance squared at which this object is visible.
        /// Only used if _visibleBeyondFarPlane == true such as for stars and worlds
        /// </summary>
        public double MaxVisibleDistanceSquared
        {
            set { _maxVisibleDistanceSq = value; }
            get { return _maxVisibleDistanceSq; }
        }

        public Container Container
        {
            get 
            {
                if (mParents == null) return null;
                Entity parent = (Entity)mParents[0];
                while (parent != null)
                    if (parent is Keystone.Entities.Container)
                        return (Keystone.Entities.Container)parent;
                    else
                        parent = parent.Parent;

                return null; // entity has no container owner which is ok.
            }
        }

        public Entity Parent // NOTE: entities can only have one parent hence the hard coded [0] subscript
        {
            get
            {
                if (mParents == null) return null;
                System.Diagnostics.Debug.Assert(mParents.Count == 1, "Entity.getParent - Entities can only have one parent.");
                return (Entity)mParents[0];
            }
            //set { _parents[0] = value; } // we use child.AddParent() and rely on assert during dev to ensure this rule isnt broken.
        }

        /// <summary>
        /// Recurses up through parent hierarchy until the first Region is found.  This way
        /// a sword attached to an actor can find it's region by recursing upwards.
        /// </summary>
        public Region Region
        {
            get
            {
                if (mParents == null || mParents[0] == null) return null;
                
                if ((mParents[0] is Entity) == false) return null; // Viewpoint entity is attached to a SceneInfo IGroup
                Entity parent = (Entity)mParents[0];
                while (parent != null)
                {
                    if (parent is Region) return (Region)parent;
                    parent = parent.Parent;
                }
                return null;
            }
        }

        public bool Awake 
        { 
            get { return (mEntityFlags & EntityFlags.Awake) == EntityFlags.Awake; }
            set 
            {
                if (value)
                    mEntityFlags |= EntityFlags.Awake;
                else
                    mEntityFlags &= ~EntityFlags.Awake;
            }
        }

        // Visible = false means the entity could still be Enabled but is not rendered.
        // Region's can be Visible = false to hide all child Entities but those children will still be active
        // Children will NOT be recursed for rendering if a parent is Visible = false.
        // todo: is above child recurse note correct including considering that
        // an exterior Vehicle can not be rendered but still allow for traversal
        // of interior.
        // NOTE: If Pickable=true a Visible = false entity can still picked.

        public virtual bool Visible
        {
            get { return (mEntityFlags & EntityFlags.VisibleInGame) == EntityFlags.VisibleInGame; }
            set
            {
                if (value)
                    mEntityFlags |= EntityFlags.VisibleInGame;
                else
                    mEntityFlags &= ~EntityFlags.VisibleInGame;
            }
        }

        public virtual bool Pickable
        {
            get { return (mEntityFlags & EntityFlags.PickableInGame) == EntityFlags.PickableInGame; }
            set
            {
                if (value)
                    mEntityFlags |= EntityFlags.PickableInGame;
                else
                    mEntityFlags &= ~EntityFlags.PickableInGame;
            }
        }

        public virtual bool Overlay
        {
            get { return (mEntityFlags & EntityFlags.Overlay) == EntityFlags.Overlay; }
            set
            {
                if (value)
                    mEntityFlags |= EntityFlags.Overlay;
                else
                    mEntityFlags &= ~EntityFlags.Overlay;
            }
        }

		public override Vector3d Scale 
		{
			set 
			{ 
				if (value == mScale) return; // no change
				base.Scale = value;
								
				if (mScript != null && mScript.TVResourceIsLoaded)
					mScript.EventRaise (this.ID, "scale");
			}
		} 

        public double DistanceToSquared(Entity target)
        {
            if (this.Region == target.Region)
            {
                // DerivedTranslation takes into account hierarchical translation up the parent hierarchy to
                // the host Region
                return Vector3d.GetDistance3dSquared(target.DerivedTranslation, this.DerivedTranslation); 
            }
            
            // This works and takes into account scaling and inheritance and different regions.
            return Vector3d.GetDistance3dSquared (target.GlobalTranslation, this.GlobalTranslation);   
            // todo: i really do need to find all cases where we're computing distances
            // between objects and determine if i should be using this instead to ensure
            // we get correct global positions
            
        }

        public double DistanceTo(Entity target)
        {
            return Math.Sqrt (DistanceToSquared (target));   
        }
        
//        todo: this is a clusterfuck and unnecessary.  all i need to do is use entity.GlobalTranslation and target.GlobalTranslation minus any regions.GlobalTranslation
//        public Vector3d GetTranslationRelativeToRegion (Region r)
//        {
//        	// todo: this is buggy/untested/unverified function
//        	// todo: before i did a simple entity.GlobalTranslation - viewpoint.GlobalTranslation
//        	
//        	// and then didnt even make it camera space and it worked
//        	
//        	// get matrix to convert from one to the other
//        	Matrix source2dest = Matrix.Source2Dest(this.Region.GlobalMatrix, r.GlobalMatrix);
//        	Matrix transformedTargetMatrix = Matrix.Multiply(source2dest, Matrix.CreateTranslation (this.DerivedTranslation));            	
//        	Vector3d targetPosition = transformedTargetMatrix.GetTranslation();
//        	
//        	//#if DEBUG
//        	Matrix dest2source = Matrix.Source2Dest (r.GlobalMatrix, this.Region.GlobalMatrix);
//        	Matrix transformedSourceMatrix = Matrix.Multiply (dest2source, Matrix.CreateTranslation (targetPosition));
//        	Vector3d startingPosition = transformedSourceMatrix.GetTranslation();
//        	System.Diagnostics.Debug.Assert (startingPosition.Equals (this.DerivedTranslation));
//        	//#endif
//        	
//        	return targetPosition; 
//        }
        
		// SwitchParent in this context refers to an entity being removed from one part of the scene and insert into another.
        // The main purpose is to keep the entity in Repository while RemoveChild is called and then AddChild again in it's new place without the entity
        // getting removed from the Repository.  THATS why I'm thinking a "entityMoved" event isnt entirely necessary. Although
        // we do know it's sceneNode would need to be updated, but in reality, it's a Remove->Add operation and not a "move"
        // that would require special continuous collision or anything either.  
        //
        // For a regular movement related move, we can use coherence to first check we are still in the same region and only check
        // neighboring regions recursively (up to a depth) to find where it needs to move and then fall back to root search.
        // WARNING! For things like cargo being jettisoned or fighters or shuttles leaving a hangar bay, we need to make sure
        // inertia is applied to the child craft and taht we handle the transition properly even if the parent ship is moving fast
        // across zones, that the child craft then is moved into the correct zone

        // SwitchParent allows us to bypass the AddChild method completely and specifically
        // the resulting EntityAdded event which is supposed to be for when Entities are first loaded into the SceneGraph
        // we also want to prevent the removal of the Entity as it's ref count hits 0 on Repository.DecrementRef
        // What is the real issue with EntityAdded event though since it first is preceded by an EntityRemoved event?
        public virtual void SwitchParent (Entity newParent)
        {
        	IGroup previousParent = this.Parents[0];
        	Region previousRegion = this.Region;
        	
        	System.Diagnostics.Debug.Assert (newParent == previousParent == false, "Entity.SwitchParent() - Parents are one and the same!");
            if (newParent == null) throw new ArgumentNullException("Cannot move entity to null parent!");

            
            Repository.IncrementRef(this); // artificial increment this child node to make sure Repository doesn't delete during .RemoveChild()          
            previousParent.RemoveChild(this); // remove should not allow the SceneNode to be deleted and apparently it doesnt in BoundElementGroup.RemoveChild()
            SuperSetter setter = new SuperSetter(newParent);
            setter.Apply(this);
            Repository.DecrementRef(this); // decrement the child node that was artifically incremented in this method earlier
            
            
            this.Execute ("OnParentChanged", new object[]{this.ID, newParent.ID, previousParent.ID});
            
            if (previousRegion != this.Region)
            	this.Execute ("OnRegionChanged", new object[] {this.ID, this.Region.ID, previousRegion.ID});
            
            //SetChangeFlags(Enums.ChangeStates.GeometryAddedRemoved); // handled by the RemoveChild and AddChild calls
        }
        
        #region IGroup
        public override void RemoveParent(IGroup parent)
        {
            base.RemoveParent(parent);

            if (parent is Entity )
            {
                // note: it must be this child Entity which ishaving it's RemoveParent() called on it
                // to be the one to call .EntityDetached()
                // todo: I believe if the mScene is null here, it is because this item was already detached
                // from the scene but it's ref count was not yet 0 due to being manually incremented.  
                // So under that circustance it should be legitimate to have mScene == null so we test for it
                if (mScene != null)
                {	
                	mScene.EntityDetached(this);
                    // NOTE: simulation.UnRegister() occurs for producing DomainObjects here
                    // in RemoveChild()
                    
	            	// Here is correct location to unregister this entity from simulation
	            	// if it was a production entity.
	            	// todo: shouldnt this be done on Child Enties?
	            	if (this.DomainObject != null)
	            	{
						if (this.DomainObject.UserProduction != null)
	            	    	mScene.Simulation.UnRegisterProducer(this);
						if (this.DomainObject.ForceProduction != null)
	            	    	mScene.Simulation.UnRegisterForceProducer(this);
	            	}
                }
            }
        }

        public override void AddParent(IGroup parent)
        {
        	// must assign scene prior to Repository.IncrementRef() which occurs in base method
        	// todo: i think one fundamental problem we have is that when we add a child to the scene
        	// our method of assigning the Scene is just all fubar.  When an entity is connected to a scene
        	// via it's parent being connected, then that parent should immediately connect all children 
        	// BEFORE calling the base.AddParent() 
        	// I mean essentially what we want is for recursive set of all Scene's first.   
        	// That does imply it cannot happen in AddParent.  It should happen in Scene property
        	// 
        	//if (parent is Entity)
        		// todo: recursively here, once this Zone for example is added, 
        	//	this.mScene = ((Entity)parent).Scene ;
    
        	// AddParent last because we want the Repository.IncrementRef() to occur after we've
        	// attached this node to the scene.
        	base.AddParent(parent);

            if (parent is Entity && ((Entity)parent).Scene != null) // if Parent.Scene is null, we will recursively call EntityAttached from within EntityAttached on child nodes later on
            {
                Keystone.Scene.Scene scene =  ((Entity)parent).Scene;
                // note: it must be this child Entity which is having it's AddParent() called on it
                // to be the one to call .EntityAttached() because if the Parent does, the
                // Simulation will not know which child it's referring to.  But child Entities
                // on the other hand only ever have one parent.
                // note: Root never has this called because it has no parent so no AddParent() call ever occurs.
                // Root is Attached() by hand.
                scene.EntityAttached(this, scene);
            }

            
            // NOTE: These flags are necessary (i think) because a child entity can be added
            //       to it's parent or switch parents and that will result in dirty Global and Regional 
            //       matrices (but not Local) because the parent matrices are now different.
            // NOTE: These flags are source PARENT and so only propogate DOWN
            // NOTE: The bounding volumes and RegionMatrix must be flagged dirty too
            // because if this is a re-parent operation, then inherited translation, rotation and scale
            // will invalidate the region matrices
            // NOTE: It's vital to only propogate these DOWN 
            SetChangeFlags(Enums.ChangeStates.GlobalMatrixDirty | 
                Enums.ChangeStates.RegionMatrixDirty | 
                Keystone.Enums.ChangeStates.BoundingBoxDirty, Enums.ChangeSource.Parent);
            // // temp version without the bbox flags to see if this is ok (eg bbox flags are redudndant)
            //SetChangeFlags(Enums.ChangeStates.GlobalMatrixDirty |
            //    Enums.ChangeStates.RegionMatrixDirty, Enums.ChangeSource.Parent);
        }


        public virtual void AddChild(Entity child)
        {
            // NOTE: always set flags early before any .AddParent/.Add so that if those other methods
            // result in calls to update a bounding box for example, that we then don't return here only
            // to set those flags as dirty again after they've just been updated
            // NOTE: only ChangeStates.GeometryAdded here.  ChangeStates.EntityAttached must only occur
            // in the AddParent method! (and actually i think that's no longer true as i directly call
            // mScene.NodeAttached(this) from there
            SetChangeFlags(Keystone.Enums.ChangeStates.GeometryAdded |
                Keystone.Enums.ChangeStates.BoundingBoxDirty, Enums.ChangeSource.Child);
            
            base.AddChild(child); //child.Parent = this; // NOTE: even though this uses AddChild() from the Node base class, it should still only ever result in _parents[0] being used. All entities only have one parent 
            Trace.Assert(child.Parent == this); 
        }

        internal void AddChild(DomainObjects.DomainObject domainObject)
        {
            if (mScript != null) throw new ArgumentException("Entity.AddChild() - Previous DomainObject not removed.");


            mScript = domainObject;
            mDomainObjectScriptResource = mScript.ResourcePath;

            if (domainObject.PageStatus != PageableNodeStatus.Loaded) 
            	throw new Exception ("Entity.AddChild() - DomainObject script MUST be loaded at this time or else we cannot invoke 'InitializeEntity'" +  "Entity.AddChild() - DomainObject is now a 'resource' of Entity which means it's now guaranteed to be loaded before it's added as child.");
            
            base.AddChild(domainObject);
 
            // we need to restore the default properties
            // since this is a shared domain object and when added to a subsequent entity
            // although we dont need to initialize it again, we do need it's default properties
            GetCustomPropertiesFromPersistStrings(mCustomPropertyNamesPersistString, mCustomPropertyTypesPersistString, mCustomPropertyValuesPersistString);
			                
            // initialize entity with per-entity settings defined in the script
            // recall that the "Initialize" script is per-domain and "InitializeEntity" 
            // is per-entity
            // NOTE: we use mDomainObject.Execute here since mScriptIsInitilazed is not yet 'true'.
            mScript.Execute("InitializeEntity", new object[] { this.ID });
            mScriptIsInitialized = true;
	            
            
            // todo: here we can set the state of this Entity to "awake"
            //       which means we can notify the scene simulation so that
            //       it can now run register producers & forceproducers
            //       - the question i have is, when the entity is not awake
            //       and we are receiving state info from server about it, what
            //       do we do with that state info? as the script is unable to respond
            //       - what if we are not connected to the scene yet?
            
            bool autoGenerateFP = GetEntityFlagValue ((uint)EntityFlags.AutoGenerateFootprint);
            SetChangeFlags ( Keystone.Enums.ChangeStates.DomainScriptLoaded, Keystone.Enums.ChangeSource.Child);
        }

        public void AddChild(CellFootprint footprint)
        {
            if (mFootprint != null) throw new ArgumentException("Entity.AddChild() - Previous Footprint not removed.");
            mFootprint = footprint;

            base.AddChild(mFootprint);
        }

        public void AddChild(Behavior.Behavior behavior)
        {
            // all behaviors extend from a single root and thus there can only be
            // one direct behavior child within an Entity
            if (mBehaviorRoot != null) throw new ArgumentException("Entity.AddChild() - Previous Behavior not removed");
            mBehaviorRoot = behavior;
            base.AddChild(behavior);
            
            if (mBehaviorContext == null)
                mBehaviorContext = new Behavior.BehaviorContext(this, mBehaviorRoot);

            // todo: SetChangeFlags?  
        }

        public void AddChild(Animation.Animation animation)
        {
            base.AddChild(animation);

            mAnimations = mAnimations.ArrayAppend(animation);

            if (mAnimationController == null)
                mAnimationController = new Keystone.Animation.AnimationController(this);

            mAnimationController.Add(animation);
        }
                
        public override void RemoveChild(Node child)
        {
            base.RemoveChild(child);
            if (child is Entity)
            {
                // NOTE: EntityDetached is not called here, but in the call Entity.RemoveParent()
                SetChangeFlags(Enums.ChangeStates.GeometryRemoved |
                    Keystone.Enums.ChangeStates.BoundingBoxDirty, Enums.ChangeSource.Child);
            }
            else if (child is CellFootprint)
            {
                Debug.Assert(child == mFootprint);
                mFootprint = null;
            }
            else if (child is DomainObjects.DomainObject)
            {
                Debug.Assert(child == mScript);
                mDomainObjectScriptResource = null;
                mScript = null;
            }
            else if (child is Behavior.Behavior)
            {
                Debug.Assert(child == mBehaviorRoot);
                mBehaviorRoot = null;
                mBehaviorContext = null;
            }
            else if (child is Animation.Animation)
            {
                mAnimations = mAnimations.ArrayRemove((Animation.Animation)child);
                if (mAnimations == null || mAnimations.Length == 0)
                {
                    mAnimations = null;
                    mAnimationController = null;
                }
            }
        }
#endregion
        
        /// <summary>
        /// Flags are used for Lazy Updates.
        /// This method is also when SetChangeFlags is called.  It is critical that 
        /// no other actions except flag propogation occurs here because
        /// some flags will get passed along multiple times and that would result in 
        /// multiple unnecessary actions.  Instead, responses to various flag settings
        /// should always occur during Update or Render or reading of a property value like 
        /// BoundingBox and then the flag can be cleared.
        /// </summary>
        /// <param name="flags"></param>
        /// <param name="source"></param>
        protected override void PropogateChangeFlags(Keystone.Enums.ChangeStates flags, Enums.ChangeSource source)
        {
            // for entities, only movement and size related changes propogate up to parents in the spatial graph or entity graph
            Keystone.Enums.ChangeStates filter = Keystone.Enums.ChangeStates.Translated |
                Keystone.Enums.ChangeStates.BoundingBox_TranslatedOnly |
                Keystone.Enums.ChangeStates.Scaled |
                Keystone.Enums.ChangeStates.Rotated |
                Keystone.Enums.ChangeStates.BoundingBoxDirty |
                Keystone.Enums.ChangeStates.GeometryAdded |
                Keystone.Enums.ChangeStates.GeometryRemoved |
                Keystone.Enums.ChangeStates.KeyFrameUpdated |
            	// Keystone.Enums.ChangeStates.GlobalMatrixDirty | <-- never need to propogate because it only affects the current Entity.
                Keystone.Enums.ChangeStates.MatrixDirty |     
                Keystone.Enums.ChangeStates.RegionMatrixDirty;
            
            
        	// filter out the flags that are _not in_ the filter list
        	Keystone.Enums.ChangeStates filteredFlags = flags & filter;
            	
            if (filteredFlags != 0)
            {
                // if source of the flag is a child or self (and not a parent), notify parents
                if (mParents != null && (source == Enums.ChangeSource.Child || source == Enums.ChangeSource.Self))
                    NotifyParents(filteredFlags);

                // if source of the flag is a parent or self (and not a child), notify relevant children for relevant flags
                if (mChildren != null && (source == Enums.ChangeSource.Parent || source == Enums.ChangeSource.Self))
                    NotifyChildEntities(filteredFlags);

                // Scene.EntityMoved and
                // sceneNode's will need to have it's bounding volume dirty flag set
                if (mSceneNode != null)
                {
                	filter = Keystone.Enums.ChangeStates.GeometryAdded |       // June.12.2014 - Added .GeometryAdded and .GeometryRemoved because that 
	                              Keystone.Enums.ChangeStates.GeometryRemoved |     // flag is propogated after .LoadTVResource in Geometry nodes but
			                	  Keystone.Enums.ChangeStates.Scaled  |             // was not getting handled here so that SceneNode's could update their own bounding volumes.
                	              Keystone.Enums.ChangeStates.Translated |          // Thus we'd have to manually initiate a re-scale in plugin to get the SceneNode to recalc it's bbox
                	              Keystone.Enums.ChangeStates.BoundingBoxDirty |    // Oct.8.2014 - Added BoundingBox_TranslatedOnly and  BoundingBoxDirty since changes to Geometry 
                	              Keystone.Enums.ChangeStates.BoundingBox_TranslatedOnly | // bounding box can occur when there is no scaling/translation or rotation such as with adding new Minimesh Elements.
	                              Keystone.Enums.ChangeStates.Rotated;
	                
                	// filter out different set of flags that are _not_ in the list
					filteredFlags = flags & filter;                	
                	if (filteredFlags != 0)		
                	{
	                    // Dec.4.2012 - It's critical that we don't send the following mSceneNode.SetChangeFlags
	                    //              or mScene.EntityMoved() until we've notified parents and children as we have
	                    //              done above!
	                    // Dec.4.2012 - It's critical to notice here that because each Entity
	                    //              is setting BoundingBoxDirty flag on it's SceneNode
	                    //              that nested SceneNodes will all have thier flags set
	                    //              as dirty such that when the parent sceneNode is asked
	                    //              for it's bbox and it's dirty, it's UpdateBoundVolume()
	                    //              will in turn see that it's child's bbox is dirty and so
	                    //              will be updated as well.  
	                    //              THis is because this PropogateChangeFlags() occur immediately
	                    //              throughout the affected branches as our Update() is being performed.
	                    //              Meanwhile culling doesn't start until after Update().
	                    mSceneNode.SetChangeFlags(filteredFlags, Enums.ChangeSource.Target);   //"Target" refers to a SceneNode's Entity target as opposed to it's actual Child node.
	                    
	                    
	                    if (this is Proxy3D == false) // we dont move proxies in Simulation
	                    {
	                    	// note: EntityMoved flag is never propogated up or down.  It's only used in Update() for NotifyScene
		                    this.SetChangeFlags (Enums.ChangeStates.EntityMoved, source);
		                    if (mScene != null)
			                    mScene.EntityUpdated (this);
	                    }
                	}
                }
            
	            // Scene.EntityResized flag in Scene
	            filter = Keystone.Enums.ChangeStates.Scaled |
	                          Keystone.Enums.ChangeStates.Rotated |
	                          Keystone.Enums.ChangeStates.GeometryAdded |
	                          Keystone.Enums.ChangeStates.GeometryRemoved;
	            
	            if ((flags & filter) != 0)
	            {
                    if (this is Proxy3D == false) // we dont move proxies in Simulation
                    {
                    	// note: EntityResized flag is never propogated up or down.  It's only used in Update() for NotifyScene
		            	this.SetChangeFlags (Enums.ChangeStates.EntityResized, source);
		            	if (mScene != null)
		            		// todo: Zone and ZoneRoot are getting "Resize" when child enties are Rotating and propogating
		            		// that upward, i think they also respond to translated and boundingbox dirty and such unnecessarily
			            	mScene.EntityUpdated (this);
		            	// footprint is dirty 
		            	mFootprintIsDirty = true;
                    }
	            }
            }
        }


        protected override void NotifyParents(Enums.ChangeStates flags)
        {
            if (mParents == null || flags == Keystone.Enums.ChangeStates.None) return;

            // TODO: for Zones and fixed size Regions, we would like to skip
            //       SetChangeFlags() call for resize and translation and such flags
            //       from children!
            
#if DEBUG
            if (mParents.Count >= 0)
                System.Diagnostics.Debug.Assert(mParents.Count == 1);
#endif
            // entity (other than Viewpoint.cs) can only have 1 parent (and of type Entity) at most because it is not a shareable scene graph node
            Parent.SetChangeFlags(flags, Keystone.Enums.ChangeSource.Child);
        }


        // NOTE: this is also called in the BonedModel.Update() for handling child models attached to bones
        protected virtual void NotifyChildEntities(Enums.ChangeStates flags)
        {
            // this function is called by this class whenever the parent is translated, rotated or scaled. 
            // notify child entities that they will need to compute new world position and/or rotation and/or scale and related matrices
            // depending on what has changed in the parent
            if (mChildren == null || mChildren.Count == 0) return;
            Node[] children = mChildren.ToArray(); // to avoid issue of iterating _children and another child eleemnt being added during that for loop
            
            for (int i = 0; i < children.Length; i ++)
                // NOTE: Container overrides this to avoid notifying a Region child
                //       since region's have their own coordinate systems that are unaffected by
                //       a parent's transform change.
                // todo: but how can you get the flags to contain flags for Rotation, Scale and Translation at the same time
                // when we originally wanted to call NotifyChildEntities right after Translation and such has changed?
                // i mean, i could do something like MoveActor (position, scale, rotation) to ensure its just one call but... meh.
                if (children[i] is BoundTransformGroup) 
                    children[i].SetChangeFlags(flags, Enums.ChangeSource.Parent);
        }


        // NOTE: Activate() exists because sceneBase.EntityAttached() can do recursion to initiate the attaching
        //       of any child Entities once it's parent has been attached.  However, that does not mean that
        //       Activation will succeed.  DomainObject must also be loaded if one is specified in mResourcePath
        //       In the case of CelledRegion, celldatabase must be loaded as well
        // mScene != null, SceneNode != null, parent != null and domainObject != null (and domainObject.Loaded) if string.IsNullOrEmpty(mResourcePath) == false
        public virtual void Activate(bool value)
        {
            if (value)
            {
                mEntityFlags |= KeyCommon.Flags.EntityFlags.Awake;

                // we have to call this directly because Update() will never get called
                // which means NotifyListener() will never get called.
                mScene.EntityActivated(this);

            }
            else // deactivate
            {
                mEntityFlags &= ~KeyCommon.Flags.EntityFlags.Awake;
                mScene.EntityDeactivated(this);
            }
        }

        long mLastTick;
        public long LastTick {get {return mLastTick;} set {mLastTick = value;}}
        
        public virtual PickResults Collide(Vector3d start, Vector3d end, KeyCommon.Traversal.PickParameters parameters)
        {
            // NOTE: see notes in ModeledEntity.Collide();
            throw new NotImplementedException();
        }

        public struct EntityInput
        {
        	public string Name;  // AxisName
        	public bool Enabled; // if the Axis has begin/end states, enabled will indicate true if "begin"
        	public object Value; // value of the axis
        	public string Source;  
        }
                
        private object mInputQueueLock = new object();
        private System.Collections.Generic.Queue <EntityInput> mInputQueue;
        public void QueueInput (EntityInput input)
        {
        	lock (mInputQueueLock)
        	{
        		if (mInputQueue == null) mInputQueue = new Queue<Entity.EntityInput>();
        		mInputQueue.Enqueue (input);
        	}
        }
        
        public EntityInput[] DeQueueAll () 
        {
        	lock (mInputQueueLock)
        	{
	        	if (mInputQueue == null || mInputQueue.Count == 0) return null;
	
	        	EntityInput[] results = new Entity.EntityInput[mInputQueue.Count];
	        	
	        	for (int i = mInputQueue.Count - 1; i >= 0; i--)
	        		results[i] = mInputQueue.Dequeue();
	        	
	        	return results;
        	}
        }
        
        // todo: Water meshes and such that are rendered not by our Cullers, are still setting .Matrix and not .WorldMatrix

        /// <summary>
        /// Called by Simulation.Update _only_ against enabled ACTIVE entities.
        /// </summary>
        /// <param name="elapsedMilliseconds"></param>
        public virtual void Update(double elapsedSeconds)
        {
            // todo: What we need to fix the above is for our Simulation to have an Active
            // list of Entities.    So things like Walls, Floors and such
            // can deactivate and then re-activate when for instance the PhysicsEngine
            // tells us that the entity has been collided with. Or when
            // a Sound zone gets collided with by the Camera.

            if ((mChangeStates & Keystone.Enums.ChangeStates.DomainScriptLoaded) != 0)
            {
            	// Dec.16.2013 - we used to restore custom property values here but since
            	// DomainObject node is now a non-serializable resource of Entity, we can
            	// do that after loading the script here in LoadTVResource() so this
            	// test here is not useful anymore... but we'll keep it around
                // check registered flag, and if not, but parent != null, then register
                
                if (mScene == null) return; // if we are not connected to scene yet, we will NOT disable flag and will poll
                
                // still here?  then domainobejct AND scene are connected so we can register production then disable flag
				if (mScript.UserProduction != null)
    	        	mScene.Simulation.RegisterProducer(this);
				if (mScript.ForceProduction != null)
    	        	mScene.Simulation.RegisterForceProducer(this);
			
                DisableChangeFlags(Keystone.Enums.ChangeStates.DomainScriptLoaded);
            }
            if ((mChangeStates & Keystone.Enums.ChangeStates.BehaviorScriptLoaded) != 0)
            {
                DisableChangeFlags(Keystone.Enums.ChangeStates.BehaviorScriptLoaded);
            }
            
            bool targetChanged = (mChangeStates & Keystone.Enums.ChangeStates.TargetChanged) != 0;
            bool nodeAdded = (mChangeStates & Keystone.Enums.ChangeStates.ChildNodeAdded) != 0;
            bool nodeRemoved = (mChangeStates & Keystone.Enums.ChangeStates.ChildNodeRemoved) != 0;
            
            if (targetChanged || nodeAdded || nodeRemoved)
            {
                if (mAnimationController != null)
                {
                	// TODO: during paging i think some descendants retreived in .GetDescendants() may wind up being null? 
                	//       Or why did that happen once (so far)? havent tested deserialization of animations nearly enough...
                	// TODO: why am i filtering GroupAttribute? shouldn't it be possible to do a color interpolation animation on a mesh group?
                    // get a flag list of child nodes
                    List<Node> descendants = new List<Node> ();
                    Group.GetDescendants (Children, new Type[]{typeof(Entity), typeof (Appearance.GroupAttribute)}, ref descendants);
                    mAnimationController.ReMapAllAnimationsToTargets(descendants.ToArray());
                }
                // this flag will not propogate up any higher
                DisableChangeFlags(Enums.ChangeStates.ChildNodeAdded |
                    Keystone.Enums.ChangeStates.ChildNodeRemoved | 
                    Keystone.Enums.ChangeStates.TargetChanged);
            }


            if (mAnimationController != null)// && mAnimationSet.OverrideBehaviors)
            {
                // skip running behavior and instead follow animation playback
                // commands including ability to pause and advance to next frame 
                // only on demand
                // We could track the current animation since it is
                // a child here... allow our "ChangeNodeProperties" to
                // change a property in the AnimationSet itself ... yes..
                // AnimationSet.AnimationTestMode
                // AnimationSet.CurrentAnimation
                // etc

                mAnimationController.Update(this, elapsedSeconds);
            }

            if (!Core._Core.ScriptsEnabled) return;

            if (mBehaviorRoot != null && mBehaviorRoot.Enable)

                mBehaviorRoot.Perform(mBehaviorContext, elapsedSeconds);

            // todo: is there a way to do fixed frequency updates?
            using (CoreClient._CoreClient.Profiler.HookUp("OnUpdate"))
	            Execute("OnUpdate", new object[] { this.ID, elapsedSeconds });
        }


        private bool mScriptIsInitialized = false;
        public object Execute(string eventName, object[] args)
        {
        	if (mScript == null) return null;
        	
        	if (mScriptIsInitialized)
                return mScript.Execute(eventName, args);
        
			return null;        	
        }
        
        #region OBSOLETE - the below ExecuteScript supercedes this PerformBehavior
        ///// <summary>
        ///// Calls into our script dictionary after validating the authority and appropriateness of the Behavior.
        ///// Only Entities (not ResourceBase or non EntityBase derived Nodes) can have behaviors Peformed?
        ///// </summary>
        //public virtual void PerformBehavior(string behavior)
        //{
        //    throw new NotImplementedException("i believe ExecuteScript() function below now supercedes this?  Keeping around til i know for sure.");
        //    float elapsedMilliseconds = 0; // todo: umm... do we need to always pass elapsed?
        //    if (mScripts != null)
        //        if (mScripts.ContainsKey(behavior))
        //            mScripts[behavior].Execute(new object[] { this.ID, elapsedMilliseconds });
        //}
        #endregion 

        #region OBSOLETE ALSO - Now that we've switched to behavior trees, this is not needed
        //public void ExecuteScript(string key, object[] args)
        //{
        //    if (!Core._Core.ScriptsEnabled) return;

        //    if (mScripts != null)
        //    {
        //        if (mScripts.ContainsKey(key))
        //            if (mScripts[key].Enable)
        //            {
        //                try
        //                {
        //                    mScripts[key].Execute(args);
        //                }
        //                catch (Exception ex)
        //                {
        //                    Trace.WriteLine(string.Format("EntityBase.ExecuteScript() -- Error executing script '{0}'.  {1}", key, ex.Message));
        //                    mScripts[key].Enable = false; // disable any bad scripts
        //                }
        //            }
        //    }
        //}
        #endregion

        #region IBoundVolume
        // ModeledEntities have combined volume of child MODELS but NOT of child Entities!
        // Only EntityNode's use hierarchical bound volumes of child EntityNodes.
        protected override void UpdateBoundVolume()
        {
            DisableChangeFlags(Keystone.Enums.ChangeStates.BoundingBoxDirty | Enums.ChangeStates.BoundingBox_TranslatedOnly);

            //if (this.Name == "Neptune")
            //    System.Diagnostics.Debug.WriteLine("Neptune.BBOX");
            //// see if we can get away with just translating the existing bounding box?
            //if ((_changeStates & Enums.ChangeStates.BoundingBox_TranslatedOnly & Enums.ChangeStates.BoundingBoxDirty) == Keystone.Enums.ChangeStates.BoundingBox_TranslatedOnly)
            //{
            //    // todo: test the above & test to see if it only triggers the following code
            //    // if just BoundingBox_TranslatedOnly is set
            //    DisableChangeFlags(Enums.ChangeStates.BoundingBox_TranslatedOnly);

            //    _box.Max -= _translationDelta;
            //    _box.Min -= _translationDelta;
            //    _sphere = new BoundingSphere(_box);
            //}
            //else 
            if (mChildren != null && mChildren.Count > 0)
            {
                if (mBox == null)
                	mBox = new BoundingBox();
                else 
                	mBox.Reset();
                
                BoundingBox childboxes = new BoundingBox();
                for (int i = 0; i < mChildren.Count; i++)
                {
                    // NOTE: It was always a mistake for a NON REGION Entity.cs derived class
                    // to include the bounding volume of child _ENTITIES_ (child Models and ModelSelectors are OK). 
					// The SceneNode's are  responsible for Hierarchical bounding volumes and Entities bounding volumes are
                    // only for themselves and never include their child entities.
                    //
                    // NOTE: We combine Model/ModelSelector children boxes. We DO NOT combine child _ENTITIES_. (again
                    // only models/model selectors)  It is EntityNode sceneNodes responsibility to use hierarchical volumes that
                    // include child EntityNodes.
                    if (mChildren[i] is Model || mChildren[i] is ModelSelector) 
                    {
                        // note: model's are transformed by their RegionMatrix and not the RegionMatrix of this Entity.  
                        // But what do we do here when a ModelSelector has individual model's each with potentially different RegionMatrices?  
                        // Since if the ModelSelector is not transforming each invidiaul sub-model by it's RegionMatrix and it's not happening here either
                        // then it's not happening anywhere.  I suspect we should be
                        // calling to "Select()" all models relevant and independantly transforming and combining the resulting boxes.
                        BoundingBox transformedChildBox = BoundingBox.Transform(((BoundTransformGroup)mChildren[i]).BoundingBox, ((BoundTransformGroup)mChildren[i]).RegionMatrix);
                        childboxes.Combine(transformedChildBox);
                    }
                    //else if (_children[i] is Entity)
                    //{
                    //    // NOTE: we do NOT include child entities here.  Leave that to EntityNode.cs
                   //     childboxes = BoundingBox.Combine(childboxes, ((BoundTransformGroup)_children[i]).BoundingBox);
                    //}
                }
                mBox.Combine(childboxes);
            }

            if (mBox == null) return; // if the box still hasnt been set

            mSphere = new BoundingSphere(mBox);
            double radius = mSphere.Radius;

            // todo: with regards to planets, we could hardcode this such that
            // it's always visible within half region range for worlds, and 1/8th region range for moons
            // the problem is, in order to render even the icon for the world, it must pass
            // the cull test.   however, if we to treat it properly like a HUD item, we would
            // only find the worlds we detected via sensors (including visual sensors (telescopes and eyeballs))
            // In this way, the world would cull, but still show up in the hud.
            // This way the hud would independantly query for the nearby worlds and generate
            // proxies for them.
            // 
            // http://astrobob.areavoices.com/2012/01/05/what-would-the-sun-look-like-from-jupiter-or-pluto/
            // 30 arc minutes equal 1/2 degree or the diameter of the sun and moon. 
            double angleDegrees = 0.5d;  
            double angleRadians = Keystone.Utilities.MathHelper.DEGREES_TO_RADIANS * angleDegrees;
            // http://www.bautforum.com/showthread.php/106931-Calculating-angular-size
            // sin(angle/2) = radius/(distance+radius)
            _maxVisibleDistanceSq = radius / System.Math.Sin(angleRadians * .5); // / Radius;
            _maxVisibleDistanceSq *= _maxVisibleDistanceSq;

            
            //_maxVisibleDistanceSq = float.MaxValue;
            // below calc is same as above....hrm...
            double distance = radius / Math.Tan(angleRadians / 2d);
            
            
            distance *= distance;
        }
        #endregion

        #region IEntitySystemSubscriber members
        public void Subscribe (Keystone.Simulation.IEntitySystem system)
        {
        }
        #endregion
        
        #region IFXSubscriber Members // todo: is most of the FXprovider stuff now really relevant for Entity or should move to Models? 
        // and what we think of as IFXProviders for entities maybe should be IEntitySystems? like stardigest or a world economic simulation
        protected List<IFXProvider> _providers = new List<IFXProvider>();
        protected FXSubscriberData[] _subscriberData = new FXSubscriberData[System.Enum.GetNames(typeof (FX_SEMANTICS)).Length];
        protected bool _inFrustum;

        public IFXProvider[] FXProviders
        {
            get { return _providers.ToArray(); }
        }

        // typenames of all the providers to be subscribed too (for deserialization purposes)
        // we can start with a count and then itterate through typename0='bleh'  as far as deserializing the attributes
        // todo: this is i think supposed to be a string representation
        public string[] Providers
        {
            get { return null; } //  throw new NotImplementedException(); }
            set { } // throw new NotImplementedException(); }
        }

        public FXSubscriberData[] FXData
        {
            get { return _subscriberData; }
            set { _subscriberData = value; }
        }

        public bool InFrustum
        {
            get { return _inFrustum; }
            set
            {
                _inFrustum = value;
                foreach (FXSubscriberData data in _subscriberData)
                    if (data != null) data.InFrustum = value;
            }
        }

        public void Subscribe(IFXProvider fxProvider)
        {
            fxProvider.Register(this);
        }

        public void UnSubscribe(IFXProvider fxProvider)
        {
            fxProvider.UnRegister(this);
        }

        public void UnSubscribeAll()
        {
            foreach (IFXProvider p in _providers)
            {
                p.UnRegister(this);
            }
        }
        #endregion
        
    }
}