/*
 * Created by SharpDevelop.
 * User: Hypnotron
 * Date: 1/13/2014
 * Time: 2:42 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace KeyCommon.Flags
{

    // TODO: most of these flags really shouldn't allow to be changed during ARCADE
	[Flags]
    public enum EntityAttributes : uint
    {
        // all enum values MUST have a proper value and shifting is an easy way to get it right.
        None = 0,                  // 0 always clears all bits
        ExplicitEvents = 1 << 1,   // only events declared in DomainObject can execute event handling code
        PickableInGame = 1 << 2,   // mouse picking collission specifically. individual models can also have their independant pickable states if the entire Entity.Pickable = true those will get tested individually
        VisibleInGame = 1 << 3,  
        //VisibleInEditor = 1 << 10,  // this is not needed.  Entities should always be visible and pickable in Editor
        
        Awake = 1 << 4,  // Awake = false is asleep and this entity does not get .Update() called
        // but remains in the Active list if it's not static. 
        Destroyed = 1 << 5, // Destroyed flag should probably never be saved in a prefab.  It's only used for runtime... well... maybe a \\save\\ ??
        Dynamic = 1 << 6,
        CollisionEnabled = 1 << 7, // entity-to-entity collisions specifically
        AutoGenerateFootprint = 1 << 8, // any footprint is ignored and we will autogenerate if the parent is CelledRegion and entity is created or rescaled


        CastShadows = 1 << 9,      // if cast shadows is against model, then we can disable for LOD
        ReceiveShadows = 1 << 10,
        // TODO: how to decide where overlay, fixedspace, instancing, shadows go in Entity or Model?
        Overlay = 1 << 11,
        LateRender = 1 << 12, // water patches render last of 3d renderables
                              // TODO: overlay and fixedscreenspace can apply to any selected model but useinstancing
                              // is model specific?
        UseFixedScreenSpaceSize = 1 << 13,
        LargeFrustum = 1 << 14,


        HasViewpoint = 1 << 15,
        PlayerControlled = 1 << 16,            // local or remote player controlled Vehicle


        ServerObject = 1 << 17, // Entities (eg. spawn points) that should only be added to Scene.ServerEntities and not Scene.ActiveEntities and updated by server (eg. Simulation.UpdateLoopback())
        MissionObject = 1 << 18,

        // -------------------------------------------------------
        // BEGIN - Entity TYPE flags used in traversals
        Light = 1 << 19,
        HUD = 1 << 20,
        Root = 1 << 21,
        Region = 1 << 22, // can be used to skip traversal of non Region entity children of exterior regions
        EmptyZone = 1 << 23,  // a place holder Zone that is generated on the fly and not stored to disk
                              // this allows us to only spend disk storage space on non empty zones and allows
                              // us to generate massive galaxies so long as very low percentage populated zones
        Background = 1 << 24,
        ContainerExterior = 1 << 25,
        Structure = 1 << 26, // can be used to skip traversal of interior regions? 
        Terrain = 1 << 27,
            
        Hardpoint = 1 << 28, // auto-tile capable terrain/wall/floor/ceiling pieces 


        Portal = 1 << 29,
        Occluder = 1 << 30,




        AllEntityAttributes =  Terrain | Hardpoint | Structure | Background | Root | Region | Light | ContainerExterior | MissionObject | Portal | Occluder | HUD,
        AllEntityAttributes_Except_HUD = AllEntityAttributes & (~HUD),
        AllEntityAttributes_Except_Region = AllEntityAttributes & ~(Region | Root),
        AllEntityAttributes_Except_Boundaries = AllEntityAttributes & ~(Region | Portal),
		AllEntityAttributes_Except_Structure = AllEntityAttributes & ~(Structure),  // TODO: since Structure is also ModeledEntity, when we test flags for _all_ except structure, then it will exclude on ModeledEntity also so this test will fail inadvertantly. Ideally, visual entities should only possess one of the available flag types, not all              
        AllEntityAttributes_Except_VisualEntities = AllEntityAttributes & ~(Hardpoint | Background | Structure),
        AllEntityAttributes_Except_Lights = AllEntityAttributes & ~Light,
        AllEntityAttributes_Except_Interior_Region = AllEntityAttributes & ~Structure,

        // END - Entity TYPE related flags
        // -------------------------------------------------------

		
        // NoUse = 1 << 31,   // << 31 does not work.
		All = uint.MaxValue         // sets all bits
    }
}
