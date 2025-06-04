using System;
using System.Collections.Generic;
using Keystone.Elements;
using System.Diagnostics;



namespace Keystone.DomainObjects
{  

    public enum ComponentType :byte
    {
        Wall,
        Floor,
        Ceiling,
        Ladder,
        Door,
        Window,
        
        
        // TODO: I think the following is unnecessary since all we have to do is query
        // what things a component can Produce and/or Consume to know whether it's a power generator
        // engine, seat, bed, whatever
        Power,  // reactor, batteries, or other form of generator
        Engine, // propulsion or drive

        // life support and safety

        // accomodations
        Seat,
        Bed     // ugh this could get ugly? but path finding will need these hints
    }




    // the unscaled dimensions of a component.  
    // most components cannot be scaled at all, but some
    // such as power plants and engines, life support systems, can.
    public enum ComponentDimension
    {
        One_x_One,
        One_x_Two,
        One_x_Three,
        Two_x_Two,
        Two_x_Three,
        Three_x_Three
    }

    public enum ComponentOrientation : byte
    {
        Forward,
        Backward,
        Left,
        Right,
        Forward_Left,
        Forward_Right,
        Backward_Left,
        Backward_Right
    }

// Dec.12.2013 - Hypno - moved to user_constants.css
//    /// <summary>
//    /// These are runtime and edit time flags 
//    /// </summary>
//    public enum ComponentFlags : uint
//    {
//        None = 0,
//        CanScaleX = 1 << 0,  // can you scale\stretch the width 
//        CanScaleY = 1 << 1,  // can you scale the height
//        CanScaleZ = 1 << 2,  // can you scale the depth 
//        CanArmor = 1 << 3,
//        CanHavePortal = 1 << 4,
//        Climbable = 1 << 5   // eg ladder or stairs
//        // NPCMovable 
//        // Destroyable   ?  these are DomainObject settings right, not Entity?  
//        //                  but Physical would be more inherent to Entity and whether
//        //                  it contains a Physics child node?
//    }

    

    ///// <summary>
    ///// TODO: in essense, because they are shared, im not sure DomainObject is best name for this
    ///// but for now we'll keep it.  Really it's an EntityDefinition object
    ///// A domain object is a business object DEFINITION and can host only a few types of child nodes
    ///// namely Rules and Scripts. 
    ///// Domain Objects ARE shared because they do not persist values for custom properties. 
    ///// The entity which they are attached does that.
    ///// </summary>
    //public class DomainObject : Group  
    //                                  // IMPORTANT: Since the properties are created by the Script
    //                                  // it's really only the script that should handle validation
    //                                  // because only the script knows the datatypes of each property
    //                                  // inherently.
    //                                  // WAIT: If we define some floor object the user has designed
    //                                  // we certainly need to persist those design settings!
    //                                  // So we do know at least that the mCustomProperties[i].DefaultValue
    //                                  // must be persisted
    //                                  // And we do want it to be traversible.
    //                                  // So really we have two options...
    //                                  // Keep domainObjects as Nodes and seperate
    //                                  // Merge DomainObject data with Entity so we have just one thng.
    //                                  // We may do that in the long run but for now lets just 
    //                                  // use this seperate DomainObject as a test case
                                        
    //    // QUESTION: Can a DomainObject exist without a script?  In other words
    //    // can a DomainObject have simple serialized list of it's consumptions and outputs
    //    // without them being populated by a script?
    //    // Similarly, can rules be serialized and restored from xml too?
    //{

    //    private enum ScriptFlags : uint
    //    {
    //        None = 0,
    //        HasInitialization = 1 << 0,
    //        HasUpdate = 1 << 1,
    //        HasBuild = 1 << 2
    //    }

        
    //    private ComponentType ComponentType;      // TODO: type as noted above i think is irrelevant.   Type is inferred from the products and consumption capabilities of htis component
    //    private ComponentDimension Dimensions;    // TODO: for now we'll ignore this while testing floors and walls
    //    private uint[] mFootprint;
    //    private ComponentOrientation Orientation;  // this is entity instance specific, should not be here because DOmainObjects are definitions and are shared


    //    private uint mFlags; // component specific flags set by script.  NOT the same as custom flags which are per entity instance and reside in Entity
    //    internal Settings.PropertySpec[] mCustomProperties;
    //    private Dictionary<string, Rule> mRules;  // the rules can be created by the script upon initialization
    //    private DomainObjectScript mScript;
    //    // if a DomainObject must contain a script, why not just merge the Script with it?
    //    // we can have at least a DomainObjectBase that is abstract and contains most of that
    //    // and allow a few derived types for things like
    //    // interior components that contain orientation and dimensions
    //    // which can only be place by floorplan editor
    //    // And since this script is already different from the BehaviorScript there's no point
    //    // in trying to keep it modular so that both those types can share them... cuz they cant.
    //    // lastly, even if we do have DomainObject absorb the DomainObjectScript's properties 
    //    // we still dont have to have a script necessarily.  (actually not true since
    //    // we will be using the script as the resource name/id)  But even in that case
    //    // we can load a scriptless version of the DomainObject 
    //    // DomainObjectBase (abstract)
    //    //    ScriptedDomainObject
    //    //      InteriorObject     // or why not just flags?  no deep hierarchies
    //    //          Machine        // or why not just flags?  no deep hierarchies
    //    //
    //    //    SimpleDomainObject
    //    // 
    //    public DomainObject (string id) : base(id) 
    //    {

    //    }


    //    #region ITraversable members
    //    public override object Traverse(Keystone.Traversers.ITraverser target, object data)
    //    {
    //        throw new NotImplementedException();
    //        //return target.Apply(this, data);
    //    }

    //    internal override Keystone.Traversers.ChildSetter GetChildSetter()
    //    {
    //        throw new NotImplementedException();
    //    }
    //    #endregion 


    //    #region ResourceBase members
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <param name="specOnly">True returns the properties without any values assigned</param>
    //    /// <returns></returns>
    //    public override Settings.PropertySpec[] GetProperties(bool specOnly)
    //    {
    //        return base.GetProperties(specOnly);
    //    }

    //    public override void SetProperties(Settings.PropertySpec[] properties)
    //    {
    //        if (properties == null) return;
    //        base.SetProperties(properties);

    //        List<KeyScript.Rules.Rule> broken = new List<KeyScript.Rules.Rule>();

    //    }
    //    #endregion

    //    public uint[] Footprint { get { return mFootprint; } set { mFootprint = value; } }

    //    public DomainObjectScript Script { get { return mScript; } }
        
    //    public virtual bool GetFlagValue(uint bitPosition)
    //    {
    //        return ((mFlags & bitPosition) == bitPosition);
    //    }

    //    public virtual void SetFlagValue(uint bitPosition, bool value)
    //    {
    //        if (value == true)
    //            mFlags |= bitPosition;
    //        else
    //            mFlags &= ~bitPosition;
    //    }

    //    /// <summary>
    //    /// Custom Properties are completely independant of our normal node Properties.
    //    /// Thus they do not use the standard Get/SetProperties functions.  Changing
    //    /// a DomainObject's properties from Plugin infers we're actually only changing
    //    /// the Custom Properties.
    //    /// IMPORTANT: This only stores the propertyspec definitions but NOT the values.
    //    /// The values which are entity specific are stored in the entities.
    //    /// </summary>
    //    /// <remarks>The CustomProperties are loaded via the script Initialize() and never via XML.</remarks>
    //    public Settings.PropertySpec[] CustomProperties
    //    {
    //        get { return mCustomProperties; }
    //        set { mCustomProperties = value; }
    //    }
        
    //    // NOTE: DomainObjects don't respond to most flags related to geometry loading, transforms,
    //    // entities attached/detached, SceneNode related.  
    //    protected override void PropogateChangeFlags(global::Keystone.Enums.ChangeStates flags, Enums.ChangeSource source)
    //    {
           
    //        switch (flags)
    //        {
    //            case Keystone.Enums.ChangeStates.ScriptLoaded:
    //                // when the script is loaded, we know it's connected to an entity?  Is 
    //                // that always true?  Hrm...  if not we'd have to check both for initialized
    //                // and for parent added and then if either event happens, we check for whether
    //                // Entity.IsDomainObjectInitialized flag (and this flag is per Entity because
    //                // entities share domainObjects and need to receive the CustomProperties
    //                // So in fact, the above assertion is FALSE.  We can't just initialize on
    //                // Script loaded.  We have to initialize for each entity after the script
    //                // is loaded
    //                // run the initialization script.  
    //                // TODO: I think the parent should call initialize... but the one thing
    //                // is the parent doesnt get the ID of the child... why didnt i include that
    //                // 
    //                object result = ExecuteScript("Initialize", new object[]{this.ID});
    //                NotifyParents(Keystone.Enums.ChangeStates.ScriptLoaded);
    //                break;
    //            case Keystone.Enums.ChangeStates.ScriptUnloaded:
    //                NotifyParents(Keystone.Enums.ChangeStates.ScriptUnloaded);
                                        

    //                break;

    //            default:
    //                break;
    //        }
    //    }


    //    public bool Validate(string propertyName, object value, out string brokenDescription)
    //    {
    //        brokenDescription = "";
    //        if (!(mRules.ContainsKey (propertyName))) return true;

    //        Rule rule = mRules[propertyName];
    //        bool result = rule.Validate(new object[] {value});
            
    //        if (!result) brokenDescription = rule.BrokenDescription;
    //        return result;
    //    }

    //    public void AddRule(string propertyName, Rule rule)
    //    {
    //        if (mRules == null) mRules = new Dictionary<string, Rule>();
    //        mRules.Add(propertyName, rule);
    //    }

    //    public void AddChild(DomainObjectScript script)
    //    {
    //        // only one script allowed
    //        if (mScript != null) throw new Exception("Existing script must be removed before it can be replaced.");
    //        mScript = script;
    //        base.AddChild(script);
    //        SetChangeFlags(Keystone.Enums.ChangeStates.ScriptLoaded, Keystone.Enums.ChangeSource.Self);
    //    }

    //    public override void RemoveChild(Node child)
    //    {
    //        if (child is DomainObjectScript)
    //        {
    //            if (mScript != child) throw new Exception("Existing script does not match child.");
    //            mScript = null;
    //            base.RemoveChild(child);
    //        }
    //    }
       

    //    #region scripts initialize domain objects and their entities and host validation rules
    //    public object ExecuteScript(string key, object[] args)
    //    {
    //        if (!Core._Core.ScriptsEnabled) return null;
    //        if (mScript == null) return null;
            
    //        try
    //        {
    //            return mScript.Execute(key, args);
    //        }
    //        catch (Exception ex)
    //        {
    //            Trace.WriteLine(string.Format("EntityBase.ExecuteScript() -- Error executing script '{0}'.  {1}", key, ex.Message));
    //            mScript.Enable = false; // disable any bad scripts
    //            return null;
    //        }
            
    //    }
    //    #endregion
    //}
}
