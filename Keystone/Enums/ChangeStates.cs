using System;

namespace Keystone.Enums
{
    
    // to set flags use  Parent.ChangeState = ChangeStates.Moved | ChangeStates.Rotated | ChangeStates.Scaled
    // to check the status of a flag you would use a property "ChangeStates node.ChangeState" and then
    // the statement if (node.ChangeState & ChangeStates.Translated) == ChangeStates.Translated
    [Flags]
    public enum ChangeStates : int
    {
        // IMPORTANT: I thought about using some persistant flags here but that is a mistake.
        // ChangeStates should ONLY include those states which are only important at runtime.
        None = 0,
        ChildNodeAdded = 1 << 1,
        ChildNodeRemoved = 1 << 2,
        GeometryAdded = 1 << 3 | ChildNodeAdded, // WARNING! MUST be careful with combining '|' other flags here like GeometryAdded = 1 << 3 | ChildNodeAdded | BoundingBox_Dirty because when you clear the flag for ChildNodeAdded it will also clear the BoundingBox_Dirty when you dont intend to
        GeometryRemoved = 1 << 4 | ChildNodeRemoved, // TODO: In fact I should ban the practice altogether 

        ViewpointAdded = 1 << 5 | ChildNodeAdded,
        ViewpointRemoved = 1 << 6 | ChildNodeRemoved,


        BoundingBoxDirty = 1 << 7,  
        BoundingBox_TranslatedOnly = 1 << 8,
        MatrixDirty = 1 << 9,
        RegionMatrixDirty = 1 << 10,
        GlobalMatrixDirty = 1 << 11,
        Translated = 1 << 12,
        Rotated = 1 << 13,
        Scaled = 1 << 14,
        
        // NOTE: we don't combine flags because when we DisableChangeStates, it also disables ALL the relevant bits in the flag
        // Translated = 1 << 12 | BoundingBox_TranslatedOnly | MatrixDirty,
        // Rotated = 1 << 13 | BoundingBoxDirty  | MatrixDirty,
        // Scaled = 1 << 14 | BoundingBoxDirty | MatrixDirty,
        KeyFrameUpdated = 1 << 15,  // AnimationUpdated same thing
        AppearanceNodeChanged = 1 << 28, // MaterialNodeChanged, TextureNodeChanged
        AppearanceParameterChanged = 1 << 16,  // when Appearance parameters are changed 
        ShaderParameterValuesChanged = 1 << 17,
        ShaderFXLoaded = 1 << 18,
        ShaderFXUnloaded =1  << 19,
        EntityScriptLoaded = 1 << 20,    // scripts paged in or paged out (NOT just added/removed)
        DomainScriptUnloaded = 1 << 21, 
        BehaviorScriptLoaded = 1 << 22,
        BehaviorScriptUnloaded =1  << 23,
        TargetChanged = 1 << 24,  // typically used by animations which are directly parented to Entity so Entity will be only receiver
        EventHandlerChanged = 1 <<  25,

        EntityMoved = 1 << 26,  // required only for scene listener
        EntityResized = 1 << 27, // required only for scene listener
        PhysicsNodeAdded = 1 << 28,
        PhysicsNodeRemoved = 1 << 29,
        All = int.MaxValue
    }

    public enum ChangeSource : int
    {
        Self = 0,
        Parent = 1,
        Child = 2,
        Target = 3   // a target is used when an entity is notifying it's SceneNode(s) that it has moved.
    }
}