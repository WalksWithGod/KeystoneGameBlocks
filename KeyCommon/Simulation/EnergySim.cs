using System;
using System.Collections.Generic;
using Keystone.Types;

namespace KeyCommon.Simulation
{
    // todo: I think all of the below is mostly obsolete.  We now just have production and consumption. 
    //       but perhaps some of the class members can be added to the production struct.
    // todo: we need to be able to transmit production results over the wire from loopback to client unless we postpone that to version 2.0.
    //       some production we can infer each frame and then correct when the actual server production comes through or if a change in a particluar type of production such as Thrust as occurs?
    
    //// for our first implementation, we only need concern ourselves with what is needed
    //// for gravity to be implemented.
    //// Thus scripts need way to create a Transmitter named "gravity" 
    //// that has a range, runs continousl (1:1 with simulation update)
    //// and a lifetime of -1 meaning infinte.
    //// question: but what about the location of the gravity and
    //// it's amount of pull?  is that set in the transmitter?
    //// Maybe yes?  

    //public class EnergyPropogation // GravityPropogation
    //{
    //    Dictionary <string, Transmitter> mTransmitters;
    //    Dictionary<string, Receiver> mReceivers;

    //    // TODO: outgoing should/could just be tracked within each Tranmsitter...?

    //    // mOutgoingQueue  // especially for those emissions that take multiple frames to be
    //                       // delivered because of distance seperation between transmitter and receiver

    //    /// <summary>
    //    /// Discovers receivers for a given transmitter
    //    /// </summary>
    //    private void Discover()
    //    { 
    //    }

    //    /// <summary>
    //    /// Sends an Emission to every valid and qualifying Receiver of that type.
    //    /// </summary>
    //    public void Transmit()
    //    {
    //    }

    //    public void Receive()
    //    {
    //    }
    //}



    //// each ship will track the gravity emitters that affect it
    //internal class Influences
    //{
    //    // IBody mReceptor; // the target ship
    //    // IBody[] mEmitters; // the bodies that emit gravity that will affect the target Receptor

    //    void Update()
    //    {
    //        // recalc the emitters.
    //        // this is done only when an emitter is added/removed from the target ship's sector
    //        // or when the target crosses sector boundary
    //    }
    //}


}

// TransmissionPropogation.cs
// {
// 
// }

// EnergyEmission.cs
// {
//      ITransmitter Source;

//      Vector3d position;  // relative to it's ITransmitter source
//      Vector3d velocity; 
//      
// }
// 
// Graviton : EnergyEmission
// {
//      Source = src;   // used to get mass 
//      Target = target; // used to get distance from Source
//      
//      
// }



    // OBSOLETE - The "Transmitter is basically just the Entity itself and its Script.  We also now refer to Transmitter/Receiver as Production\Consumption. We will however store some of the members here as CustomProperties for the sscripts.
    /// <summary>
    /// Transmitters and Receivers, Producers and Consumers are defined by DomainObjectScript
    /// but where are their states?  And how do we tie ability to modify on/off and throttle
    /// with the core function of that engine along with secondary functions like consumption
    /// of resources?  Perhaps on/off, throttle is part of the main Engine.  State is stored there
    /// and then contextually, our scripts will update various states of stores, and generated
    /// thrust each frame.
    /// </summary>
    public class Transmitter
    {
        // breaker
        // throttle
        // direction
        // shape? (eg sphere, cone, ray)
        public string Name; // TODO: i think this is irrelevant largely except maybe for friendly purposes.
                            // we use the emissionTypeFlag instead for differentiating during simulation

        //public Keystone.Entities.Entity Source; // TODO: Error, here we have a Source entity in Transmitter
                                                // which is per instance data, yet Transmitter is stored under the shared
                                                // DomainObject which is NOT per instance.  I think 
                                                // at least it suggests that Transmitters and Receivers and
                                                // Producers and Consumers should be in Entity and not
                                                // here.  Although, if it were done that way
                                                // would it really even need a "Source" anymore?
                                                // as we would know the source is the parent
                                                // but then again, we dont seem to be tracking parent here.
                                                // And Transmitter is not treated as a Node but as 
                                                // some kind of scripted attribute.

        Receiver[] LastReceivers;  // cache of previous results (PER INSTANCE.) // todo: cache of last contacts should be stored by the Game01.dll Contacts.cs object

        // TODO: Is there an array of transmissions for some things such as an array
        // of transmitted lasers, bullets, sunflares?  The idea being that
        // a manager class to update those so that they coudl "expire" after traveling x distance
        // or something...  but then again, that would not help when the entity that
        // transmitted the transmission was destroyed.


        // -1 == one time, but what about "CONTINUOUS" ?
        // TODO: rather than a frequency in seconds, we should have this represent
        // the ration of timesteps so that 1:1 for every timestep
        // or if the overal hertz is 
        // TODO: If we move simulation.cs into EXE then we can just hardcode constants for the various frequencies for different productIDs.
        //       It's no different than thrust and gravitation frequency settings.  
        public int FrequencySeconds; // number of times this transmitter will emit an emission per second
    // todo: should the turn length/frequency amount really be left to the scripts to define when defining the production for each script?
    // todo: it wouldn't be a problem if the scripts only ran server side and the resulting production/consumption being sent over the wire to the client.
    // todo: lets postpone this for now and update every tick, but eventually, we need to know how long a "turn" represents in game time.  For instance,
    // todo: for now, lets just get "radar" and "contacts" icons rendering on screen.
       // todo: the sensor's need knowable friendly names so hud can find them and then grab the "contacts" details.
        // for space battles or The Sims, one turn might represent 10 minutes.  But there's a difference between gameTime vs Realtime and production update frequency isn't there?
                            // TODO: shouldn't we have seperate frequencies potentially for every type
                            // of emmission
                            // perhaps frequency must be in the Emission itself and so the Transmit()
                            // call must track the lastTime var
                            // The other option is that Transmitters are composite and
                            // are 1:1 with the emission type they produce.  So Entity.Transmitters 
                            // returns an array and our TransmitReceiveCycle will get all transmitters
                            // determine which are to be sent, discover 
          public int LastTransmitTime; // These members are properties of the Producer and perhaps we need seperate LastTime_RadarSignature_Emitted, etc as custom properties. WEll they are runtime only properties so we can store them in a private struct.  eg RadarSignatureState just like we do for HelmState and SensorsState.  Each productID needs to track these members. The consumer simply consumes and deducts from the producer's "store" eg "store_power", "store_fuel" or if the production is infinite such as a radarSignature, it just consumes.
          
          public int LifeTime; // in seconds. (-1 == infinite)
          public int Range;    // -1 == infinite. some of these properties values exist in the entity and can be obtained via GetCustomPropertyValue()
          public int Shape;    // surface sphere (a single radar pulse), cone, solid cone, solid sphere (eg heat from star or gravity)
          public uint EmissionTypeFlag; // 32 bits for now but maybe more. Could be made into c# bitflag arrays for unlimited length
                               // the basic idea however is that the type is irrelevant to keystone.dll
                               // all that matters is the receiver and transmitter share same emission type flag
          public int Flags;    // lineOfSight (whetherline of sight to receivers is reqt)


          public Transmitter(string name, uint flag)
          {
              Name = name;
              EmissionTypeFlag = flag;
          }

    }

// in our Star's constructor, we're going to add a GravityTransmitter
// that will upon ITransmitter.Transmit() release a Graviton emission
// the nature of a graviton is then to have a source emitter, 

    public class Receiver   // 
    {
        public string Name;
        public uint EmissionTypeFlag; // exact type flag is irrelevant so long as receiver and transmitter use same flag

        public Receiver(string name, uint flag)
        {
            Name = name;
            EmissionTypeFlag = flag;
        }

        public void Discover ()
        {
//          // find all transmitters that are in range
//          // client side, we may receive transmitters 
        }

//        public void Receive (Emission e)
//        {
////         
//        }
    }

// what about the creation of these ProducerConsumer<T> ?
// is that done automatically upon Consumer/Producer's being added?
//
// TODO: DomainObject\Production.cs   <-- production/consuming is different but analgous
// in many ways to transmitting/receiving. 
// THE MAIN DIFFERENCE between  producer/consumer vs transmiting/receiving with emissions and receptrons
// is that emissions and receptrons require us to discover/resolve/locate the transmission & receiving
// groups, whereas production/consumption relationships are explicitly made.
// public class ProducerConsumerCycle
//{
//    mConsumers;
//    mProducers; // do we need seperate mConsumers or should each consumer subscribe to a producer
//                // furthermore, they should be able to subscribe with a priority
//    mProductionStore;
//    object mStoreLock; 
//
//    IMPORTANT: The following url shows that if our Producer/Consumer class is generic
//    then we can have separate ProducerConsumer's for each pair.   In this way
//    we then just assign consumers of a certain producer to that ProducerConsumer instance.
//    HOWEVER, our implementation i dont think needs seperate threads... we produce then we consume
//    serially.  We are not dealing with asychronous network receives for instance.  So even though
//    we can use threading to go thru each ProducerConsumer faster, we still go through them serially
//    because priority and order of operations is important.
//    http://www.randomskunk.com/2011/02/generic-producer-consumer-class-in-c.html
//    public void Produce()
//    {
//        lock(mStoreLock)
//        {
//          for (int i = 0; i < mProducers.Length; i++)
//          {
//
//            IProduct[] products = mProducers[i].Get();
//            for (int j = 0; j < products.Length; j++)
//                mProductionStore.Add(products[j]);
//          }
//        }	
//    }
// http://stackoverflow.com/questions/1656404/c-sharp-producer-consumer
// http://msdn.microsoft.com/en-us/library/dd997371.aspx
//    // seperate thread will now consume produced products
//    // based on their access to the various production stores
//    public void Consume()
//    {
//    }
//}





// TODO: look at notes in CelledRegion
// consider how emissions diffuse through it

//
// TODO: for Receptrons (Sensors) i would add a list of
// things that the component can detect
// And for Stimulus producers, I would add a list of source
// stimulus for it to broadcast and then
// interior wise, allow the collaborative diffusion to flow the various emissions
// until they timeout/falloff
// http://www.ttlg.com/forums/showthread.php?t=112713  <-- Vigil's comment is the 2nd one down and best explanation
// on how the types of product or stimulii etc is arbitrary.  all that matters is if you
// assign a stimulus of typeID = 33 then only a receptron that detects types of typeID=33 will
// respond to it.
// 

// practical examples that we need to understand to create a consistant framework
//  
//  Example 1: Damage.cs
//     Derived Types
//       BulletDamage.cs
//       LaserDamage
//       FireDamage
//       RadiationDamage
//       CompoundDamage.cs  <-- can contain multiple Damage types
//  in actuality, we dont need seperate classes such as the above based on type.
//  if we do use seperate classes it'll be because of how the area affect works
//  whether there is splash fragmentation damage, splash explosive damage, concussion damage, etc.
//  kinetic damage only, etc.  Not really important to think about for now...  The point is
//  that damage is an object (stimulus) in the sense of our Receptrons and Stimulii.
//  or maybe we use a component object of damage.cs that describes the shape of the damage
//  be it a simple line, sphere, cone, random area in sphere, etc
//
//  Anything that is damageable, has a Damagable receptron(s) with id's for each type of damage
//  it is susceptible too.
//
//  Question: How does armor fit in here and how does frame strength and material fit in?
//            And memory reqts being kept reasonable
//  Question: How does scripting fit in?
//  Question: How does a complete failure in a wall implemented to allow depressurization?
//             or for poisoneous gases or radiation to diffuse?
//
//  I think one way to do this is if an area effect range is a sphere, then convert that sphere
//  to tile radius first and just grab neighbors, then in any partially covered tiles (where the sphere
// doesnt encompass fully) further test by radius.  
// 
// Similarly for diffusion we can simply use a type of artificial "vent" bit in the diffusion map
// so that gases will start to flow through any holes/cracks.
// And these vents / holes / cracks will have a flow rate and a big hole will dump a ton of air
// fast and in our order of operations, we can diffuse new air from life support first, then diffuse
// it throughout ship (including venting into space), then compute suffocation of any crew in areas lacking
// breathable air cuz of trace/insufficent amount.  
//
// Question: How do we diffuse some things (seems to entail cloning our gas entities for instance)
// interior vs exterior (like radio emissions external detection vs interior detection of a sound
// of an alien boarding party)
//
//-------------------------
// Example 2: Emission.cs
//     Derived Types 
//       Infrared
//       Visible
//       Ultraviolet 
//       Radio
//       Graviton
//       Bio
//       Gases <-- consider smoke bombs, biological weapons, etc
//       Sound (at least for interior noise detection or on worlds with atmosphere)
//       Radioactives
//          for the above, maybe emissions can have a subcomponent object that 
//          describes the shape of the emission such as a line, cone, sphere, 
//
//       Detection devices have a list of id's for each type of emission it can detect.
//       as well as the shape and direction of any detection arc.
//
//-------------------------
// Example 3: PowerGrid
//    - electrical producing devices emit energy to every device
//      that is in a cell that is "powered" 
//    - how do we implement this in a way that is fast?  dynamic(changes in real time based on damage,
//      or changes to on/off switch.
//
//-------------------------
// Example 4: Ventilation System 
//    - life support generates x units of "life support" to every tile that is in a room
//    that is traversable via open (unsealed) ventiliation ducts
//    
//-------------------------
// Example 5: Fatigue/Rest, Morale/Spirits, Damage/Repair/Healing, Learning/Experience
//

// What are Particle Systems NOT?
// - they are not entities
//    - if we want to find what ships are damaged from a shockwave we would simply find all in x radius,
//       and send them a Stimulus "shockwave" which the receptrons on their exterior hull will receive and handle.



//public abstract bool ValidateRule(DomainObject domainObject);
//public Rule Rule; // here a rule is a built in rule, or maybe it points to a script

// in the entity?
// that's sort of the problem I think with trying to put the validator
// in the property and not in the overall Entity instance.

// cuz normally it's Entity.DomainObject
//                   Entity.DomainObject.Rules
//                   Entity.DomainObject.SetProperties(Property[] specs)
//                            // within SetProperty
//                            specs[i].Rule.Validate (this)
//                                 or
//                            mRules[specs[i].RuleName].Validate(this)
//                            where in the above, a rule is a tyep of Rule node
//                            that is either a Script or a Rule that uses a delegate.
//
//                            and perhaps when a "DomainObject" is added to an Entity
//                            via AddChild(), new properties, methods, rules, scripts
//                            are added...  But it's this organization and taking into account
//                            plugin Edit tab population
//                            plugin Edit tab property changing
//                            serialization/deserialization
//                                   initialization of a DomainObject when added to an Entity
//                            runtime (arcade) property changing  
//
// and these Rules are assigned in Entity.GetProperties when building up the PropertySpec array
// and could potentially done via scripts as well.. hrm...
// Actually the above is better than the sample c# project where a "rule" is a property
// with a validator.  Here our properties are seperate and can have a "Rule" assigned 
// to it.  The question is how do we assign it?  Either by a "key" name to a dictionary of 
// rules... or a direct reference. 
// 

//
// Map = Vehicle
//   Map.Floors = Vehicle.Floors
//      Map.Floors.Cells = cullalar data structure to help pathing and other AI
//      
//   **Since our components exist relatve to the Vehicle's entire interior
//   they should be children of the interior not of any deck or any room or any cell.
//      ** But spatially, they will be in Floors and Volumes and Cells.
//      ** Or these interior Floors/Volume can just be virtual.  They dont have to be full blown
//         sub-entities.  They can just be an internal only way inside CelledRegion to group cells.
//   **However to interact with the map, various components can detect diffusion particles
//     as well as emit them.  
//   ** Now it's seeming that we're going to potentially need a ton of flags
//   for all these various "scents" and so maybe we end up havng a variable bitarray
//      ** Scents are cumulative in that different objects can emit the same scent and that can
//         make the scent stronger.  So two people hiding somewhere maybe both emiting a scent
//         will be easier for a monster to detect than one person hiding by themself.
//      ** Scents are also bidirectional in the following ways
//          1) A scent can be emitted
//          2) A scent can be "scrubbed"(cleaned or removed) from a cell
//              a) Some scents can search and destroy other scents up to some lifespan time of how many they can destroy before they die themselves
//              b) scents can have a lifetime before they die too.
//          3) So yes there's a difference somewhat in how my scent system works vs how
//              a strict collaborative diffusion system works where the scent emitter is constantly
//              emiting i think?
//          4) A scent can obviously be actively detected by other player or npc agents.
//
//
//      receptron
//      public struct Scent : Product, IDiffusable
//      {
//          delegate void PropogationHandler;
//          byte id
//          byte lifetime
//          byte flags (can propogate, 
//          // maybe some inherited types of Scents can do destructions or scrub 
//          // of other scents during their propogation handlers
//          void Propogate(int elapsed) { mPropogationHandler.Invoke();}
//      }
//
//       Actually what if rather than above where Scene inherits Product, we instead
//       have a Diffusor which can accept a "Product" 
//       but... hrm... is a Scent the same as a Diffusor 
//       seems to me a Diffusor is a rule for distribution something
//       a scent is some type of emission made by an Entity and NOT a Product
//       But hrm.. what are stored fuels.. not Entities?  Products?  I would say that
//       the thing that stores the Fuel is the Entity and whatever it contains are Products
//       ????
//
//
//
//    
//
//   In the above scent example, a the diffusion calculation doesn't have to worry aobut
//   individual agent's different skill levels for detection or what have you, instead
//   the agent's own goal selection will apply a bias/bonus depending on it's skill level.
//   So using diffusion does not mean giving up flexibility in having unique acting units
//   with different personalities.
//
//  In terms of parellization in general, diffision is a server side thing mostly right?
//  So server side we'll be running different ships on different cpu's rather than worry about
//  breaking up the diffusion space on a ship by ship basis?
//
//   Cell is our light weight entity but can contain "items/components" and what if
//   just like in our old original GVD version of the concept, every wall in a "cell"
//   is a component but a special type along with floor and ceiling. YES!!!  
//    - in fact, making "walls" just components solves another problem with having
//      unique walls as well as wall braces.  Because a "wall brace" is just a type of
//      component that a user can place anywhere. It adds weight and can take up the 1 meter
//      cell it occupies but these "frames" can make a wall and room if placed on all walls
//      much tougher and more resilient to loss of integrity.  WOW!
//  
//    - Now it seems people who make isometric tile engines usually just use one floor
//    and two walls since max since a tile with 3 or 4 walls is not visible to the player
//    with a fixed isometric camera.  However, I'm thinking we'll be different because
//    our camera will be movable.
//   So if walls, floors, ceilings, ladder
//
//
// So our Cells being an array within a deck or some such, allows us to bypass
// having a TON of sceneNodes so instead just one SceneNode for the entire deck
// or at least for "Rooms" connected via portals and then our special SceneNodes
// will handle the rest.  So this is a key point because we use special PortalNodes for
// Portal sceneNodes and RegionNode's for REgions and not the default EntityNode

//First Let's say we have a battery that we've placed on a cell in our editor
//Second let's say we place a computer on another cell
//   - when editing we need to be able to know what component is in a cell so we can overwrite say a
//     Radio with a Computer.
//      - Yes that can be just a matter of brute force picking...
//      Why use cell?
//      Well for our collaborative diffusion a grid makes things easier...
//    - When editing i want to be able to define a "power link" as existing from the cell with the battery (x=5, z=2)
//      to the Computer at (x=10, z = 10) and explicitly write the array of indices of cells this passes through.
//       - Here clearly a cell is not just spatial... or maybe it is if the "Power link" is a seperate virtual entity
//       and if the Cell can still be dumb and always restored on load just from info in the "power link."
//    - In other words, can our "cells" always load the info they need from other entities?
//        1) Editing and knowing what cell you've hit so you can test whether the cell can accept some component
//          - ** In the case of a reactor that takes up 10 cells, the spatial structure
//          should tell you that so if trying to set some other component on the floor
//          you know the drop area is already occupied.
//              - ** SO THIS SUGGESTS TO US THAT THE SPATIAL NODE for an entity placed
//              under an InteriorNode that uses cells, must have a type of "CellNode"
//              that can either communicate or flat out contain the info of what indexed nodes
//              that node's entity represents.
//                  -- SO MOREOVER, these special CellNodes can exist in an octree or such
//                     to make iteration over them easier too.  So this also tells us something else
//                     since these CellNodes arent all the same size, connectivity should just like with Portals
//                     be done by the Cell's themselves in the Interior entity node.

//        2) Path finding to find a path from the garage to a sink.
//        3) Scent emissions and recepting
//            - Here a "scent" is a definete game type, and it either (or both) sets flags as it propogates and
//            reproduces via a simple cellular automata rule
//              - Here an Entity that's connected to the SceneGraph knows it can
//              find it's own cells to emit to via
//              interior = this.SceneNode.Parent.Entity;
//              InteriorCell[] cells = this.SceneNode.GetCells();
//              and thru these cells can sniff or emit knew data
//              - ** MAYBE ALL THESE SCENTS are represented as different Layers
//              BASICALLY DIFFUSION MAPS  <-- hrm...
//              These "maps" are 2d arrays of values that correspond to each cell
//              Power Map <-- different values for different power sources
//              Obviously for Scent Propogation to work at all, each Cell must be able to know
//              which of it's neighbors it can pass scents to and to do that it must know if walls
//              floors, ceilings, etc exist.
//              InteriorEntity.DiffusionMap[DIFFUSIONTYPEID] diffusionMaps;
//                     - here some maps can be empty, non existant THUS we dont need
//                     a seperate array in every cell that has to contain some NULL variable for unused scent

// One issue im having is that if a Cell is not really an entity, then no child entity like a wall
// actually exists in it, so how does this wall know what Cell it's in other than
// wallEntity.SceneNode.ParentSceneNode.Entity.Cells
//      - although because of that, an entity that spreads multiple cells is not hierarchical to all three either
//         so it makes sense to say no, it's not actually in any cell, but it is associated with cells
// Also if the cell IS purely spatial, and dumb (just a highway/transmission line) and only
// transmits information and does not really store it, then still, how does the Cell know
// what components are on it?  Seems the obvious way is
// InteriorSceneNode
//      CellNodes[] Cells <-- cells are unique in that they know who their neighbors are
//           // multiple EntityNodes can exist under a CellNode
//           SceneNode[] entityNodes  
//              EntityBase Entity  <-- always one entity per sceneNode
//       In the above since a CellNode is purely spatial, and since child entities of
//       the interior get placed under InteriorSceneNode by that InteriorSceneNode it is free
//       to assign that Entity to a cell instead and still retain a seperate bounding box in 
//       region relative coords for that Entity
//       - Also in the above HOW can an entity exist under a block of cells?  Like a reactor
//         that takes up cells x=1,z=10 thru x=10, z=10 ?
//
// The  CelledRegion entity itself has the cell dimensions so that it's CelledRegionNode
// gets re-constructed properly.
// 
