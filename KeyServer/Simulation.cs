using System;
using System.Collections.Generic;
using System.Diagnostics;
using Keystone.Elements;
using Keystone.Entities;
using Keystone.Portals;
using Keystone.Resource;
using Keystone.Physics;
using Keystone.Physics.BroadPhases;
using Keystone.Types;
using KeyCommon.DatabaseEntities;


namespace KeyGameServer
{
    internal class Interest
    {
        // what if we were to come up with an area of interest that actually
        // used AREAS (like our spatial nodes) to track interest...
        // The idea being that 10 ships in the same area could then simply share 
        // that same Area's interest data.  
        // Thus, the area knows it's interest and that interest is shareable by those 
        // that are in the area
        // And what if we could come up with some novel hierarchical AoI data structure?
        // Something like spare octree grid

    }

    /// <summary>
    /// Simulation is mostly about physics and movement whereas the Game object
    /// is about scoring and time limits and objectives and such.  Game is a business object 
    /// Similarly a KeyCommon.Entities.Player tracks player scores, kills and such and is not
    /// related to a Scene Entity "PlayerCharacter" or "NPC" etc   except that a KeyCommon.Entities.Player
    /// will have a link to it's corresponding controllable player entities (ship + avatar)
    /// </summary>
    public class Simulation : Keystone.Simulation.ISimulation
    {
        private Dictionary<uint, List<Entity>> mProduction;
        
        private const uint GRAVITY_TRANSMITTER = 0;

        private Dictionary<string, Player> mPlayers;
        private PlayerCharacter _currentTarget;
        private Game mGame;
        private Keystone.Simulation.GameTime mGameTime;
        private Keystone.Scene.Scene mScene;

        private bool FIXED_FREQUENCY_PHYSICS = true;

        private uint _physicsHertzInTimesPerSecond = 100;
        private float _fixedTimeStep = 0.1f;

        private double _elapsedSecondsPhysics;
        //private Collision.CollisionTest _collision;
        private float _gravity;
        private bool mPaused;
        private bool _isRunning;
        //private SimTime _time;

        private bool _collisionEnable = false;
        protected bool _disposed;

        //private JigLibX.Physics.PhysicsSystem _physicsSystem;
        private Keystone.Traversers.EntityQuery mEntityQuery;
        private Keystone.Simulation.Missions.Mission mCurrentMission;
        #region bepu
        //private Physics.Space _space;
        //private Physics.SimulationSettings _physicsSettings;
        #endregion

        public Simulation(float gravity, Game game)
        {
            if (game == null) Trace.WriteLine("Simulation.Ctor() - WARNING: Game argument == null");
            mGame = game;
            mEntityQuery = new Keystone.Traversers.EntityQuery();

            _gravity = gravity;
            //_time = new SimTime(10);
            _physicsHertzInTimesPerSecond = 100;
            _fixedTimeStep = (float)1.0d / (float)_physicsHertzInTimesPerSecond;

            #region bepu
            // _physicsSettings = new SimulationSettings();
            //// _physicsSettings.useSplitImpulsePositionCorrection = true;
            // _physicsSettings.gravity = new Vector3d(0, -9.81, 0f);
            // BruteForce broadphase = new BruteForce();
            // _space = new Space(broadphase);
            // _space.simulationSettings = _physicsSettings;
            #endregion

            // TODO: Switch to Jitter.
            // READ http://software.intel.com/en-us/blogs/2009/06/24/highlights-and-challenges-during-ghostbusters-development-part-2/
            // for ideas on parallelizing physics down the road
            #region JigLibX
            //_physicsSystem = new PhysicsSystem();
            //_physicsSystem.SetGravity(0, gravity, 0);

            //_physicsSystem.CollisionSystem = new JigLibX.Collision.CollisionSystemSAP();
            //_physicsSystem.EnableFreezing = true;
            //_physicsSystem.SolverType = PhysicsSystem.Solver.Normal;
            //_physicsSystem.CollisionSystem.UseSweepTests = true;

            //_physicsSystem.NumCollisionIterations = 10; // 8;
            //_physicsSystem.NumContactIterations = 10; // 8;
            //_physicsSystem.NumPenetrationRelaxtionTimesteps = 15;
            #endregion

        }

        public Keystone.Scene.Scene Scene { get { return mScene; } set { mScene = value; } }
        public Game Game { get { return mGame; } }

        public Keystone.Simulation.GameTime GameTime {get {return mGameTime;}}

        /// <summary>
        /// Simulation can only have one current mission at a time.
        /// </summary>
        public Keystone.Simulation.Missions.Mission CurrentMission { get { return mCurrentMission; } set { mCurrentMission = value; } }

        public uint PhysicsHertzInTimesPerSecond
        {
            get { return _physicsHertzInTimesPerSecond; }
            set
            {
                if (value == 0) throw new Exception("physics hertz should typically be 30 - 100");
                _physicsHertzInTimesPerSecond = value;
            }
        }

        #region ISimulation Members
        public void RegisterPhysicsObject(Entity entity)
        {
            throw new NotImplementedException();
        }

        public void UnRegisterPhysicsObject(Entity entity)
        {
            throw new NotImplementedException();
        }

        public void UnRegisterProducer(uint productID, Entity entity)
        {
            mProduction[productID].Remove(entity);
        }

        public void RegisterProducer(uint productID, Entity entity)
        {
            if (mProduction == null) mProduction = new Dictionary<uint, List<Entity>>();
            List<Entity> producers;
            bool exists = mProduction.TryGetValue(productID, out producers);
            if (!exists)
                mProduction[productID] = new List<Entity>();

            mProduction[productID].Add(entity);
        }

        
        //public void Register(Entity entity, uint productType, KeyCommon.Simulation.Consumption_Delegate consumptionHandlers)
        //{
        //}

        // TODO: isn't this inherently wrong since a transmitter exists as DomainObject component
        // and not an Entity component so the Transmitter is technically shared.  Yet, it's added
        // sepertely here for each instance
        //public void Register(Entity entity, Transmitter t)
        //{
        //    // TODO: client's should not necessarily have access to all transmitters and
        //    // receivers.  
        //    if (mTransmitters == null)
        //        mTransmitters = new Dictionary<uint, List<Transmitter>>();

        //    if (mTransmitters.ContainsKey(t.EmissionTypeFlag) == false)
        //    {
        //        mTransmitters.Add(t.EmissionTypeFlag, new List<Transmitter>());
        //    }
        //    // TODO: this is Simulation.. transmitters are stored per entity right?
        //    // the main differentiations for a transmitter are it's CRC which is it's uniqueness
        //    // and then the source and a cache of previously affected receivers...
        //    mTransmitters[t.EmissionTypeFlag].Add (t);
        //}

        //public void Register(Entity entity, Receiver r)
        //{

        //}
        public bool Paused
        {
            get { return mPaused; }
            set { mPaused = value; }
        }

        public bool CollisionEnabled
        {
            get { return _collisionEnable; }
            set { _collisionEnable = value; }
        }

        public bool Running
        {
            get { return _isRunning; }
            set { _isRunning = value; }
        }

        //public PlayerCharacter CurrentTarget
        //{
        //    get { return _currentTarget; }
        //    set { _currentTarget = value; }
        //}

        public void LoadMission(string sceneName, string missionName)
        { }

        public void EnableMission(bool enable)
        { }

        // TODO: MPJ June.20.2011 - Each Region could have it's own Simulation!  
        // http://www.gamedev.net/topic/525261-questions-on-zoning/  <-- Eve Online partitioning
        // this will also make it easy to have different physics models for different region types
        // eg. indoors on ships vs exterior of ship vs on land interior or exterior
        // For now however we wont worry about it... wait til we just get more gameplay working
        public double Update(Keystone.Simulation.GameTime gameTime)
        {
            if (_isRunning == false) return 0;

            mGameTime = gameTime ;
            
            Entity[] activeEntities = mScene.ActiveEntities;
            if (activeEntities != null)
                // update the simulated logic\scripts of entities
                for (int i = 0; i < activeEntities.Length; i++)
                    if (activeEntities[i].Enable)
                        // Entity.Update() 
                        //  - updates AnimationControllers
                        //  - performs behavior tree behaviors
                        //  - domainobject.OnUpdate() for updating game logic 
                        //    like NON FORCE production & consumption
                        activeEntities[i].Update(gameTime.ElapsedSeconds);

            // updates force production & consumption
            // TODO: should i update physics first thing and then
            //       update entities own logic & animations using the resulting actual elapsedseconds
            //       that were used as opposed to the full elapsedSeconds?  In this way
            //       we should accumulate here, not in UpdatePhysics and instead pass in
            //       the accumulated and return the remainder
            double fixedTimeStepRatio = UpdatePhysics(gameTime.ElapsedSeconds);


            UpdateProduction(gameTime.ElapsedSeconds); // update NON force production

			//
            // TODO: THIS IS KeyGameServer.Simulation.cs, and needs to be updated once KeyEdit.Simulation.Simulation.cs
            // 
            // notify listeners to apply position changes to nodes (done AFTER physics response)
            // TODO: but what if a node has reached bounds?  ideally physics would be the
            // one enforcing the bounds?  hrm...  the main bounds are world bounds, not zone
            // or interior region bounds since crossing those bounds simply transfers to the
            // adjacent region... right?  any special cases where this is not so?
            mScene.FinalizeEntityMovement(gameTime.Ticks);

            // for all entities that are registered for area of interest management,
            // update them round robin style
            // NOTE: The stale interest management results are retained across frames
            // for use in network group cast of data to interested parties, for refined
            // searches as a starting point upon which to run filters.
            // So typically we'd store interests by Vehicle...  we could potentially take some
            // short cuts with fleets that are flying close together and can share interest info

            // area of interest management
            mEntityQuery.Clear();

            // we update the basic area of interest for max range entities of all once every second
            // (for starters)

            return fixedTimeStepRatio;
        }

        // http://answers.unity3d.com/questions/10993/whats-the-difference-between-update-and-fixedupdat.html
        private double UpdatePhysics(double elapsedSeconds)
        {
            //if (_singleStep == true && keyState.IsKeyDown(Keys.Space) == false) 
            //{
            //    // don't intergrate so we can step at will
            //}
            //else
            //{
            _elapsedSecondsPhysics += elapsedSeconds; // add elapsed to remaining _elapsedPhysics from previous frame

            // http://gafferongames.com/game-physics/fix-your-timestep/
            // http://blog.allanbishop.com/box-2d-2-1a-tutorial-part-10-fixed-time-step/
            if (FIXED_FREQUENCY_PHYSICS)
            {
                const int MAX_STEPS = 10;

                int steps = (int)Math.Floor(_elapsedSecondsPhysics / _fixedTimeStep);
                if (steps > 0)
                {
                    _elapsedSecondsPhysics -= steps * _fixedTimeStep;
                }

                // TODO: the fixedTimeStepAccumulatorRatio is to be used with rendering
                //       to get the best interpolated physical position for rendering
                double fixedTimestepAccumulatorRatio = _elapsedSecondsPhysics / _fixedTimeStep;

                int stepsClamped = Math.Min(steps, MAX_STEPS); // clamping will cause potentially unsycrhonization but it's a safeguard to prevent the physics from spiraling behind and bogging down the entire game. 
                //System.Diagnostics.Debug.WriteLine("Steps clamed == " + stepsClamped.ToString());
                for (int i = 0; i < stepsClamped; ++i)
                {
                    // TODO: also what about networking interpolation?  is that automatically done here
                    //       so long as our script is running?
                    // TODO: we dont really need to update some productions at same htz
                    //       Electricity production for instance should be done maybe 1hz
                    //       gravity and thrust at higher hz.  how do we determine which are which
                    //       so we can update them in seperate loops with their own appropriate step
                    //       counts?
                    // TODO: also keep in mind that one entity can produce more than one thing...
                    //       so we'd have to produce seperately or allow for the scripts
                    //       to determine when to next call various parts of it's production update code
                    // TODO: one way to do this is to have PhysicalForceProduction seperate from 
                    //       other types of production.
                    //ResetSmoothStates(); // NOTE: in the Box2d implementation
                    //                     // he sets body.SmoothPosition, body.PreviousPosition all equal to
                    // the body.Position since Update/Integrate will change body.Position
                    // but potentially the callbacks during each Step will look to read body.SmoothPosition
                    // which will be stale.  But what i wonder is, what condition would require
                    // a callback to use the .SmoothPosition and not just the
                    // body.Position?  And if .SmoothPosition is always the same...
                    // well the only time it is not the same is after the final step
                    // and maybe that is why... but it seems to me then
                    // that callback should use body.PreviousPosition.  I do
                    // see why a callback might want to know the previousPosition
                    // though.  
                    // 
                    UpdateForceProduction(0, _fixedTimeStep);

                    // apply forces that were accumulated during production & consumption of physical forces
                    Acceleration(_fixedTimeStep);

                    // process collisions after application of forces and computing new translation & orientation

                    //_physicsSystem.Integrate(_fixedTimeStep);

                }

                SmoothSteps(fixedTimestepAccumulatorRatio);

                return fixedTimestepAccumulatorRatio;
            }
            else
            {
                if (elapsedSeconds < _fixedTimeStep)
                {
                    //_physicsSystem.Integrate((float)elapsedSeconds);
                    UpdateForceProduction(0, _fixedTimeStep);
                }
                else
                {
                    //_physicsSystem.Integrate(_fixedTimeStep);
                    UpdateForceProduction(0, _fixedTimeStep);
                }

                return 0;
            }
        }



         private void UpdateForceProduction(uint productID, double elapsedSeconds)
        {
            // divide the 1Hz producers up by type and update all elements of a particular type at once
            // then all the next, etc... 
            // so if i have 30 categories and 30 fps, i can do 1 category each frame.
            // This will make an ok 1.0 round robin implementation.
            // I might even be able to assign them the specific timeslice % or range across
            // 1 full second at which to update on... and potentially spreading each
            // category out over time within that slice range.
            if (mProduction  == null) return;

            Entity[] forceProducers = mProduction[productID].ToArray();

            for (int i = 0; i < forceProducers.Length; i++)
            {
                if (forceProducers[i].Script == null) continue; // should never happen!KeyCommon.Simulation.Production[] production = 

                // 1) Produce Forces
                KeyCommon.Simulation.Production[] production =
                    forceProducers[i].Script.UserProduction[productID](forceProducers[i].ID, elapsedSeconds);

                if (production == null) continue;

                for (int j = 0; j < production.Length; j++)
                {
                    Entity sourceEntity = mScene.GetEntity(production[j].SourceEntityID);
                    if (sourceEntity == null) continue;
                    // 2) Determine consumer distrubtion - find all valid consumers that match the terms of this production result
                    List<Entity> consumers = FindConsumers(mScene, sourceEntity, production[j]);
                    if (consumers == null) continue;

                    // 3) Consume - pass the production to the consumption handler
                    for (int k = 0; k < consumers.Count; k++)
                    {

                        // if this is a force consumer (eg thrust or gravity) cache
                        // the existing values as PreviousStep____ 
                        // TODO: mForceConsumers seperate from others?

                        // if this is a force consumer (eg thrust or gravity), we will 
                        // the resulting translations/rotations/scales will have been done 
                        // and be stored in mTranslation/mScale/mRotation

                        // for culling and visiblity we want to use the mPreviousStep____
                        // but for physics and collisions and spatial logic like pathing
                        // we want to use the current.
                        KeyCommon.Simulation.Consumption[] consumptionResult = consumers[k].Script.Consumers[production[j].ProductID](consumers[k].ID, production[j], elapsedSeconds);
                        // todo: handle consumptionResult
                    }
                }
            }

        }

        // TODO: I should multithread this
        private void UpdateProduction(double elapsedSeconds)
        {
            if (mProduction == null) return;

            // TODO: maybe we only want to produce at a certain frequency
            //       and maybe we also want to thread and
            //       maybe we also want to do only x many per interval
            //       or put another way, load balance scheduling 
            // todo: we need sensor emission production and detection too
            uint productID = 2;
            List<Entity> producers = mProduction[7]; // 2 is Power, 0 is thrust, 1 is gravity, 7 is microwaves

            //System.Threading.Tasks.Parallel.For ();
            // http://stackoverflow.com/questions/10846550/disappointing-performance-with-parallel-for
            
            for (int i = 0; i < producers.Count; i++)
            {
                if (producers[i].Script == null) continue; // should never happen!
                // TODO: the act of "consumption" should add to the contacts and be sent over the wire.
                // So there is a distinction between what production/consumption is predicted client side
                // and what is done server side with results transmitted to clients.
                // TODO: how do we ensure that some production is only done server side
                //       if not running in loopback?  The "search" feature especially
                //       here which could use a lamda, 
                // 1) Produce User Products
                KeyCommon.Simulation.Production[] production =
                    producers[i].Script.UserProduction[productID](producers[i].ID, elapsedSeconds);

                // if production is null, consumers attached to this cannot be updated
                // and so on their next "tick" they will be starved for the product 
                if (production == null) continue;

                for (int j = 0; j < production.Length; j++)
                {
                    Entity sourceEntity = mScene.GetEntity(production[j].SourceEntityID);
                    if (sourceEntity == null) continue;
                    // 2) Determine consumer distrubtion - find all valid consumers that match the terms of this production result
                    List<Entity> consumers = FindConsumers(mScene, sourceEntity, production[j]);
                    if (consumers == null) continue;

                    // 3) Consume - pass the production to the consumption handler
                    for (int k = 0; k < consumers.Count; k++)
                    {
                    	// invoke delegate
                        KeyCommon.Simulation.Consumption[] consumptionResult = consumers[k].Script.Consumers[production[j].ProductID](consumers[k].ID, production[j], elapsedSeconds);
                        // todo: handle consumptionResult
                    }
                }
            }
        }

        private void Acceleration(double elapsedSeconds)
        {
            Vector3d translation, acceleration, velocity;

            Entity[] activeEntities = mScene.ActiveEntities;
            if (activeEntities == null) return;

            for (int i = 0; i < activeEntities.Length; i++)
            {
                if (activeEntities[i].Enable
                    && activeEntities[i] is Viewpoint == false)
                //&& mScene.mActiveEntities[i] is Proxy == false)
                {
                    Entity entity = activeEntities[i];

                    // note: even if no new forces, we still must rely on previous accumulated velocity and update
                    // position! So we can't just exist if no new forces
                    //if (entity.Force.Equals(Vector3d.Zero())) continue;
                    //if (entity.Name == "Earth")
                    //    System.Diagnostics.Debug.WriteLine("earth");
                    //    // TODO: worlds should be animated by ellipticalanimation not physics

                    entity.Translation = entity.LatestStepTranslation;
                    translation = entity.LatestStepTranslation; // entity.Translation;
                    acceleration = entity.Acceleration;
                    velocity = entity.Velocity;

                    // finally this will compute the acceleration based on force
                    Keystone.Physics.Newtonian.LeapFrogVerlet(entity.Force, ref translation, ref acceleration, ref velocity, elapsedSeconds, 0f);

                    entity.LatestStepTranslation = translation;
                    entity.Velocity = velocity;
                }
        	}
        }

        // "take the last state and the state before that as the two states (interpolation). 
        // Then we use the ratio on this. Of course, this means the renderer will always be 
        // 1 frame behind the physics but this should not be a problem. "
        private void SmoothSteps(double fixedTimestepAccumulatorRatio)
        {

            double oneMinusRatio = 1.0 - fixedTimestepAccumulatorRatio;

            Entity[] activeEntities = mScene.ActiveEntities;
            if (activeEntities == null) return;

            for (int i = 0; i < activeEntities.Length; i++)
                if (activeEntities[i].Enable
                    && activeEntities[i] is Viewpoint == false)
                //&& mScene.mActiveEntities[i] is Proxy == false)
                {
                    Entity entity = activeEntities[i];
                    if (entity.Force.Equals(Vector3d.Zero())) continue;
                    //if (entity.Name == "Earth")
                    //    System.Diagnostics.Debug.WriteLine("earth");
                    //    // TODO: worlds should be animated by ellipticalanimation not physics
                    entity.Translation =
                        fixedTimestepAccumulatorRatio * entity.LatestStepTranslation +
                        oneMinusRatio * entity.Translation;

                    //entity.Acceleration = Vector3d.Zero(); // acceleration;
                    entity.Force = Vector3d.Zero();

                }
        }

        // 
        //RegionNode.CustomQueryFilter mFilter = new RegionNode.CustomQueryFilter();

        private List<Entity> FindConsumers(Keystone.Scene.Scene scene, Entity sourceEntity, KeyCommon.Simulation.Production production)
        {

            // TODO: these "Distribution Type" is I think a weakness.  I think in the case of a
            //       emission that can be detected by opposing players sensors, distribution then
            //       requires specific logic that is specific for a particular sensor / emission pair.
            //       The "detection" logic is in a sense, the consumption of that sensor.  But that
            //       detection should be done server side and then the result returned to relevant clients.
            //       This way clients dont get to test if detection is made and have to be trusted to
            //       ignore information about source and direction of that detection if detection fails.
            //
            // TODO: could use a lambda instead of this 
            //       select case
            bool temp = false;


            // TODO: i think since the distribution filter is a scripted function and because
            // scripts cannot instance entities and must go through the API, then
            // that distribution filter in turn will need to make EntityAPI calls

                        // TODO: re sensor contacts, see notes in Vehicle.cs
            // So i want to do a filter for sensor contact where we have say
            // a omnidirectional radar picking up a "radar emission" product,
            // with configurable range via custom property
            // that has a bonus depending on the skill of the sensor operator
            // (the notion being that running a sensor requires being able to screen out false positives
            // the fact that this equipment is not perfect, the fact that tuning sensors is 
            // something that must be done continuously, 
            // So the range of a sensor is not the whole story.  
            //    - the other issue is thta previous sensor contact gives a bonus to 
            //      getting picked up on subsequent radar ticks.  
            //      - how does this look in script?  You have to query
            //      the parent vehicle entity's "contact" property and i think a "contact"
            //      being a Simulation.Contact (like Simulation.Product) that is a custom property in Vehicle
            //      we can grab the contact object directly.
            // struct Contact
            // {
            //      string ContactID;
            //      string[] Sensors; // the sensors that have detected this contact.  ships can have many sensors and of various types, but we only track one contact instance right?
            //      int Time;         // the time this Contact struct was created and thus we know the elapsed time this contact has been tracked.  The longer it's detected the better the odds of maintaining the contact detection on subsequent sensor ticks
            //      Vector3d Position;
            //      Orientation Heading; 
            //      Vector3d Acceleration;
            // }
            //
            // - radar emission production

            switch (production.DistributionMode)
            {
                case KeyCommon.Simulation.DistributionType.Parent:
                    return new List<Entity> { sourceEntity.Parent };
                    break;
                case KeyCommon.Simulation.DistributionType.Container:
                    string vehicleID = sourceEntity.Container.ID;
                    return new List<Entity> { scene.GetEntity(vehicleID) };
                    break;
                case KeyCommon.Simulation.DistributionType.List:
                    string[] entityIDs = production.DistributionList;
                    // TODO: this will let us know there is a custom property named
                    // "subcribers" 
                    List<Entity> entities = new List<Entity>();
                    for (int i = 0; i < entityIDs.Length; i++)
                    	entities.Add (scene.GetEntity(entityIDs[i]));

                    return entities;
                case KeyCommon.Simulation.DistributionType.Region:

                    if (sourceEntity.Region == null) return null;

                    Predicate<Entity> match;

                    if (production.DistributionFilterFunc != null)
                    {
                        match = e =>
                        {
                        	Keystone.DomainObjects.DomainObject dObj = e.Script;
                            if (dObj == null) return false;
                            if (dObj.Consumers == null) return false;
                            if (dObj.Consumers.ContainsKey(production.ProductID) == false) return false;
                            return production.DistributionFilterFunc(production, e.ID);
                        };
                    }
                    else
                    {
                        match = e =>
                        {
                            Keystone.DomainObjects.DomainObject dObj = e.Script;
                            if (dObj == null) return false;
                            if (dObj.Consumers == null) return false;
                            if (dObj.Consumers.ContainsKey(production.ProductID) == false) return false;
                            return true;
                        };
                    }

                    return sourceEntity.Region.RegionNode.Query(true, match);
                    // TODO: above query fails on Navigation tab planet view
                    return null;

                case KeyCommon.Simulation.DistributionType.Primitive:
                    return null;

                default:
                    return null;
            }
        }


        // the idea of registering production is we dont have to iterate through a bunch of
        // entities that do no produce anything.
        // the idea is also that for something like a wall or floor, that is 99.9% static
        // we dont have to Update() it for no reason either.  Instead we only have to 
        // call Entity.Produce() for those items which have production and return the array of
        // products and then iterate through them and distribute them according to the 
        // distribution channel specified be it a primitve shape where it's emitted to all
        // entities in that area, or a list of subscribers.
        // Rather than registeration which is clunky however, id like to be able to query
        // a flag on the entity instead to see if any production or consumption handlers exist
        // and for what types.
        //private void Transmit(double elapsedSeconds)
        //{
        //    if (mTransmitters == null || mTransmitters.Count == 0) return;

        //    // TODO: i think when a certain receiver leaves a region and enters a region
        //    // those events need to be forwarded to the Transmitters that are listening
        //    // for those events.  This is of course a way of dividing up areas of interest.
        //    // Transmitters can then get all receivers within a particular region easily...
        //    // and without having to constantly re-poll.  
        //    // But the question then is, what object is hosting these receivers (and transmitters)
        //    // I think most sensibly, it's Simulation itself but with dictionary by zone
        //    // for transmitters and receivers as opposed to one simple Transmitters by 
        //    // TODO: But via Zone.Children we know the first level children... and by Zone.SceneNode.Children
        //    // we can get all entities of that region via their SceneNode parents.
        //    // TODO: So in fact, Zone's / Region's should trigger our leave/enter events

        //    if (mTransmitters.ContainsKey(GRAVITY_TRANSMITTER))
        //        // TODO: although it's not necessarily relevant for gravity transmitters
        //        // for other types we should test if the entity to which the transmitter is connected
        //        // is enabled and how much damage and then invoke the script for that transmitter
        //        // to update... or something.  I mean, the main point is, this is not a generic
        //        // implementation where i call a TransmitGravity function that is hardcoded here
        //        // and where potentially i can have any kind of transmitters.
        //        TransmitGravity(elapsedSeconds);

        //}

        // IMPORTANT TO DO:  The world that the player is closest to should emit a gravity
        // that is equal to the amount that the planet has moved that frame so that player ships
        // are moved in relation to it.... that is _IF_ the players are in orbit but even if the orbit
        // is not stable or is constantly changing.  If we only use normal gravity of the planet there is
        // a danger that it wont match up well and player ships will end up not getting properly pulled
        // along.   Actually, a better way i think to think about it is that the player should take up
        // the same amount of rotation about the star as the world does... anyways, we'll obviously have
        // to figure this out as we start on gravity and moving ships and controlling ships 
        //private void TransmitGravity(double elapsedSeconds)
        //{
        //    for (int j = 0; j < mTransmitters[GRAVITY_TRANSMITTER].Count; j++)
        //    {
        //        Transmitter t = mTransmitters[GRAVITY_TRANSMITTER][j];
        //        // if (t.Active == false) continue; 

        //        // TODO: t.Source should be not used.  It is per instance data and transmitters
        //        // are shared aspects of a DomainObject!  Instead, when we register a transmitter
        //        // we should be registering it as part of registering the Entity as capable of
        //        // that transmission type and then we can grab the transmitter here during the 
        //        // gravity sim update
        //        Region r = t.Source.Region;

        //        // spatial query all the entities in the same region with Receivers for this type of Transmitter
        //        Entities.Entity[] entities = r.RegionNode.Query(true);

        //        if (entities == null || entities.Length == 0) continue;

        //        for (int i = 0; i < entities.Length; i++)
        //        {
        //            // does the entity contain a Receiver(s) for the emission type of this Transmitter?
        //            // TODO: can we look at cached receivers first and then only update the cached list
        //            // if this entity has changed regions or something?  or take advantage of the
        //            // octree?
        //            if (entities[i].DomainObject != null && ((entities[i].DomainObject.mEmissionReceptionFlags & t.EmissionTypeFlag) != 0))
        //            {
        //                // TODO: temp hack: 
        //                // no need to get the specific receiver reference.  We know it's gravity
        //                // then and all we want to do is apply this gravity function 

        //                // get the distance between the transmitter and the receiver
        //                //Vector3d distance = t.Source.Translation - entities[i].Translation;

        //                //double mass = 1.0;
        //                // get the mass of the transmitter
        //                if (t.Source is Celestial.Body)
        //                {
        //                    //double mass = ((Celestial.Body)t.Source).Mass;
        //                    //System.Diagnostics.Debug.WriteLine("Applying gravity to " + entities[i].TypeName + " with mass of " + mass.ToString());

        //                    // TODO: what if gravity and all transmissions traveled at speed of light?
        //                    // in this way, it would take time for communications from one sector
        //                    // to reach another?
        //                    // http://www.grav-sim.com/simulation.html
        //                    // compute the gravity of the transmitter
        //                    Vector3d gravity = NBodyPhysics.GravityVector((Celestial.Body)t.Source, entities[i]);
        //                    // add this gravity acceleration to the velocity of the ship
        //                    NBodyPhysics.Accelerate(entities[i], gravity, elapsedSeconds, 0f);
        //                }
        //            }
        //        }
        //        // we know it's gravity for now since this is testing
        //        // we must find all entities with gravity receptors in the transmission area
        //        // and then cache those values so we can re-use them and then only update
        //        // based on a rule such as (if a 

        //        //          Emission[] results;
        //        //          if (mDomainObject != null)
        //        //          {
        //        //              object tmp = mDomainObject.Execute("OnUpdate", new object[] { this.ID, elapsedMilliseconds });
        //        //              if (tmp != null) results == (Emission[])tmp;
        //        //          }
        //        //          return results;

        //    }
        //}


        //}

        // OBSOLETE - We only want a single NodeUpdated(SceneNode node) for all
        // since we then just add to list of mChangedNodes.  However
        // the following movement/physics code belongs in scripts that must be assigned to
        // entities!  Keep the code til that change occurs.
        //public void NodeUpdated(EntityNode node)
        //{
        //    EntityBase entity = node.Entity;

        // TODO: The below MUST BE in a Script 
        // that is called in Simulation.Update() { mChangedEntities[i].Update(elapsed);}
        // if running and regardless of suspended (e.g. in Edit mode)
        // then we do still want to update player positions, behavior and any animations
        //if (entity is PlayerCharacter)
        //{
        //    PlayerCharacter p = (PlayerCharacter)entity;

        //    //double terrainHeightAt = 35; //terrain.GetHeight(newPos.x, newPos.z);
        //    //newPos.y = terrainHeightAt;
        //    //Vector3d velocity = Engine.Core.Maths.VSubtract(newPos, oldpos);
        //    //    // compute proper velocity vector.  this is ok for now, but eventually not sure how to deal with Y value...what if we arent ontop of terrain but on mesh?

        //    //// do we need to worry ?
        //    //if (Engine.Core.Maths.VLength(velocity) < .05f)
        //    //    newPos = oldpos;
        //    //else
        //    //{
        //    //    if (_collisionEnable)
        //    //    {
        //    //        oldpos.y = p.Center.y;
        //    //        // create a collision info structure to track the collision status
        //    //        TCollisionPacket collision =
        //    //            new TCollisionPacket(oldpos, velocity, p.Height / 2, p.Height / 2, p.Height / 2);

        //    //        newPos = collideWithWorld(collision.ScaledPosition, collision.ScaledVelocity, ref collision);
        //    //        newPos =
        //    //            new Vector3d(newPos.x * collision.eRadius.x, newPos.y * collision.eRadius.y,
        //    //                            newPos.z * collision.eRadius.z);
        //    //    }
        //    //    //newPos = _player.Translation; 
        //    //    //newPos = _player.Center;
        //    //    //newPos.y += 5;
        //    //    //newPos = Slide(oldpos, newPos); // this doesnt work so well because the position is at his feet
        //    //    //adjust player for collision with terrain
        //    //    terrainHeightAt = 35; // terrain.GetHeight(newPos.x, newPos.z);
        //    //    //Vector3d start = p.Center;
        //    //    //start.y -= p.Height/2 - 5f;
        //    //    //PickResults[] result =  _scene.Pick(start, Engine.Core.Maths.VNormalize( p.Center - p.Translation) );
        //    //    //if (result != null && result.Length > 0)
        //    //    //    terrainHeightAt = result[0].Result.vCollisionImpact.y;
        //    //    //else
        //    //    //    terrainHeightAt = oldpos.y;

        //    //    //terrainHeightAt += p.Height;
        //    //    //p.Translation =
        //    //    //    new Vector3d(newPos.x, terrainHeightAt, newPos.z);
        //    //p.Translation = newPos;
        //    //}


        //    // Physics psuedo
        //    //int eventCount = Engine.Core.Physics.PollEvents();
        //    //for (int i = 0; i < eventCount; i++)
        //    //{
        //    //    type = Physics.GetEventType(i);
        //    //    collision = Physics.GetEventCollisionDesc(i);
        //    //    // get the object associated with the collision and fire an event
        //    //    Entity myEntity;
        //    //    int index = collision.iBody1;
        //    //    myEntity = GetEntityFromIndex(index);// 1st entity involved
        //    //    myEntity.PhysicsResponse(collision); // handle response using details of the collision
        //    //    index = collision.iBody2;
        //    //    myEntity = GetEntityFromIndex(index);// 2nd entity involved
        //    //    myEntity.PhysicsResponse(collision); // handle response using details of the collision

        //    //     collision object members
        //    //      collision.fForce
        //    //      collision.fNormalSpeed 
        //    //      collision.fTangentSpeed 
        //    //      collision.iBody1 
        //    //      collision.iBody2 
        //    //      collision.iMaterial1 
        //    //      collision.iMaterial2 
        //    //      collision.vNormal   
        //    //      collision.vPosition 
        //    //
        //    //      e.g. create a trashcan barrel and use a simple box for it. 
        //    //      int trashcan = Engine.Core.Physics.CreateBody(50f);
        //    //      Engine.Core.Physics.AddBox(trashcan, min, max);
        //    //    
        //    //      note; there is no way to set any user data to the rigidbody so when we pollevents
        //    //      and check the iBody1 and iBody2, to map those to any Entity object we have, we'll need to do a lookup like 
        //    //      string key = PhysicsManager.FindEntity(iBody1) ;  
        //    //      which internally just uses a HashTable to find the results.
        //    //      It will auto handle such things because you go through it PhysicsManager.Add(IEntity ent) ; 
        //    //    
        //    // }

        //    // p.Translation = Collision(p.Translation);

        //    // finally update the player's or npc's animation
        //    // TODO: we could technically cull first before doing this...
        //    // and then instead of having to call p.Actor.Update(elapsed) we can do a proper
        //    // traversal.  Remove the "update" within BonedModel
        //    // and then do proper Traversal node handling.  Then for LOD we can proper select the child 

        //    // im not even certain that animation updates to actor should be done in either cull or traversal...
        //    // i do know i dont want it in "render" because with multiple passes there could be several and normally
        //    // you always want the update dont ONCE and on or before the first pass so the most up to date is used in all  passes
        //    // that frame.  Hrm... during "CUll" you're going to be updating things like BoundingVolumes on lazy
        //    // updating and at this same time you're going to select the child LOD's (or could)
        //    // NOTE: When building a "state" list you're simply pushing the nodes you visit onto the stack
        //    // and not popping them off afterwards.  Then in the Draw traversal we can actually traverse it
        //    // identically except instead of itterating children, we itterate the stack.  I suppose we could just have a
        //    // simple State object wrapper that contains the node as well as the node's relation to the previous node.
        //    // this way we know when to pop nodes off the stack (e.g appearances for instance that must be known )


        //    //// Animate the boids wings if any.
        //    //// Only if the selected mesh is the animated -
        //    //// - one and the simulation is running.
        //    //if (leftWingFrame && !stopped && (timeSpan <= MAXIMUM_TIMESPAN))
        //    //{
        //    //    // Get the system time in milliseconds.
        //    //    ULONG milliTime = clock() * 1000 / CLOCKS_PER_SEC *
        //    //                                win -> getWingStrokesPerSecond();
        //    //    // Remove the seconds, just keeping the milliseconds.
        //    //    milliTime %= 1000;
        //    //    // Convert the milliseconds into radians.
        //    //    D3DVALUE radians = milliTime / 159.1549431;
        //    //    // Calculate the X vector component.
        //    //    D3DVALUE xVector = D3DVALUE(sin(radians));

        //    //    // Set the new orientation of the wings.
        //    //    leftWingFrame -> SetOrientation(meshFrame, 0, 0, 1,
        //    //                                                xVector, 2, 0);
        //    //    rightWingFrame -> SetOrientation(meshFrame, 0, 0, 1,
        //    //                                                -xVector, 2, 0);
        //    //}


        //    if (p == _currentTarget)
        //        DebugDraw.DrawBox(p.Actor.BoundingBox, new Matrix(p.Matrix), CONST_TV_COLORKEY.TV_COLORKEY_GREEN);


        //}
        //// TODO: all of the following should be in a script.  And if our
        //// PhysicsBody is always in region specific coords then we need to convert
        //// them to entity relative instead (sword attached to player ragdoll hand)
        //if (entity.PhysicsBody != null)
        //{
        //    if (entity.PhysicsBody.IsActive)
        //    {
        //        float x, y, z;
        //        entity.PhysicsBody.GetPosition(out x, out y, out z);

        //        entity.Translation = new Vector3d(x, y, z);
        //        float m11, m12, m13, m21, m22, m23, m31, m32, m33;
        //        entity.PhysicsBody.GetOrientation(out m11, out m12, out m13,
        //                                                                                          out m21, out m22, out m23,
        //                                                                                           out m31, out m32, out m33);
        //        // TODO: further, RotationMatrix results in the Matrix being recomputed even if the rotation never changed
        //        // so we dont want to set this for nothing every frame just for the hell of it

        //        // TODO: this shouldnt have to be done, the Physics engine should be updating these values directly for the entity
        //        // or perhap, entity.RotationMatrix should simply be grabbing and setting entity.PhysicsBody.RotationMatrix()
        //        entity.RotationMatrix = new Matrix(m11, m12, m13, 0, m21, m22, m23, 0, m31, m32, m33, 0, 0, 0, 0, 1);
        //    }
        //}

        //}

        // the below code seems to use the same type of fixed frequency integration we do above but seems to add a variable
        // for passing to the Vector3.Lerp function.  In any case primarily i want to ensure that my code is correct.
        // http://code.google.com/p/slimdx/source/browse/tags/Nov%2008/samples/Games/Asteroids/Particles/ParticleEmitter.cs?r=798
        //    public void Update(GameTime gameTime, Vector3 newPosition)
        //    {
        //        if (gameTime.ElapsedGameTime > 0.0f && !float.IsInfinity(timeBetweenParticles))
        //        {
        //            Vector3 velocity = (newPosition - previousPosition) / gameTime.ElapsedGameTime;

        //            float totalTime = timeLeftOver + gameTime.ElapsedGameTime;
        //            float currentTime = -timeLeftOver;

        //            while (totalTime > timeBetweenParticles)
        //            {
        //                currentTime += timeBetweenParticles;
        //                totalTime -= timeBetweenParticles;

        //                Vector3 position = Vector3.Lerp(previousPosition, newPosition, currentTime / gameTime.ElapsedGameTime);

        //                particleSystem.SpawnParticle(position, velocity);
        //            }

        //            timeLeftOver = totalTime;
        //        }

        //        previousPosition = newPosition;
        //    }
        //}

        public void AddPlayer(Player player)
        {
            //mPlayers.Add(player.mID, player);

            //// send the player, the list of all other registered players
            //SendPlayerList(player);

            //// send the player the map spec
            //KeyCommon.MapSpec mapspec;
            //mapspec.mWidth = mGame.mWidth;
            //mapspec.mHeight = mGame.mHeight;
            //mapspec.Tiles = mGame.mTiles;

            //Lidgren.Network.NetConnectionBase connection  = (Lidgren.Network.NetConnectionBase)player.Tag;
            //// send the mapspec.  When the client receives this, they know they are now successfully connected to the server.
            //// NOTE: even when resuming a game, we can send a clientSpec that just has the width/height and no tiles
            //// since the client can refer to cached tiles.  If a client discovers for some reason it does not have a cached tile
            //// that the server thinks it should, the client can request it and the server will determine if it's allowed.
            //connection.SendMessage(mapspec, NetChannel.ReliableUnordered);

            //// send any new units and or cities 
            ////   - do we wait for user to request them?  If so, then we're potentially re-sending everything everytime the user re-connects
            ////     because we'd be trusting users to only request what they specifically don't already have
            ////     
        }

        private void SendPlayerList(Player player)
        {
            //    ' get list of all of the registerd players
            //    Dim p() As ClientServerCommon.Player
            //    p = DirectCast(mHost.mGamesStorageContext.RetreiveList(GetType(ClientServerCommon.Player).Name, "game_id", DbType.String, Game.mID), ClientServerCommon.Player())

            //    ' we endeavor to make sure that we only need to send the entire user list to any particular user ONCE
            //    ' and from there out, only join/parts of individuals.  Thus we sychronize access to adding/removing of mUsers
            //    Dim userList As New ClientServerCommon.UserList()
            //    userList.Scope = ClientServerCommon.Scope.Game ' game indicates it's only sent to all users in this SimHost.Simulation and not all players on any game on this GameServer
            //    ' add the user to the top of the list so that the client will be able to get their server assigned UserID first as opposed to last
            //    ' which is what would happen if we added it in the forloop below
            //    userList.AddUser(player.mName, player.mID)

            //    If (p IsNot Nothing) Then
            //        If (p.Length > 0) Then
            //            For Each item In p
            //                ' dont add the joined user to the user list twice. They were already added as first
            //                If item.mName <> player.mName Then
            //                    userList.AddUser(item.mName, item.mID)
            //                End If
            //            Next

            //            ' notify new user of the existing users
            //            Dim connection As Lidgren.Network.NetConnectionBase = DirectCast(player.Tag, Lidgren.Network.NetConnectionBase)
            //            Console.WriteLine("Sending userlist to user " + connection.ToString())
            //            connection.SendMessage(userList, userList.Channel)
            //        End If
            //    End If
        }

        public void RemovePlayer(Player player)
        {
            //    // TODO: i dont think we want to actually remove players, we just want to set their status to offline and then update the db
            //    mPlayers.Remove(player.mID);
            //    player.mOnline = False;
            //    mHost.mGamesStorageContext.Store(player);


            //    Dim userLeft As New ClientServerCommon.UserStatusChanged(ClientServerCommon.Enumerations.UserLeft);
            //    userLeft.UserName = player.mName;
            //    userLeft.UserID = player.mID;

            //    // notify all users
            //    Dim connections(mPlayers.Count - 1) As Lidgren.Network.NetConnectionBase;
            //    Dim i As Integer = 0;
            //    foreach KeyCommon.Player p in mPlayers.Values
            //    {
            //        connections(i) = DirectCast(p.Tag, NetConnectionBase);
            //        i += 1;
            //    }
            //    // see note above the Broadcast() method in NetServer.cs for why here we must use Groupcast and not Broadcast()
            //    mHost.mServer.Groupcast(userLeft, connections)
        }

        // TODO: move to Simulation.Game ?
        public void UserMessageReceived(Lidgren.Network.NetConnectionBase connection, Lidgren.Network.NetBuffer buffer)
        {
            KeyCommon.DatabaseEntities.Player player = (KeyCommon.DatabaseEntities.Player)connection.Tag;

            // if this is a server, the connection will be from the server... so we may very well ignore it...
            // but if this simulation is running on the server, each player will be important.
            // Probably best to make this abstract and have yet again... ClientSimulation, ServerSimulation


        }
        #endregion


        #region IDisposable Members
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // pass true if managed resources should be disposed. Otherwise false.
        private void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    DisposeManagedResources();
                    _disposed = true;
                }
                DisposeUnmanagedResources();
            }
        }

        protected virtual void DisposeManagedResources()
        {
            try
            {
                _isRunning = false;

            }
            catch (Exception ex)
            {
                Trace.WriteLine("Simulation.DisposeManagedResources() - " + ex.Message);
            }
        }

        protected virtual void DisposeUnmanagedResources()
        {
        }

        protected void CheckDisposed()
        {
            if (IsDisposed) throw new ObjectDisposedException("scene is disposed.");
        }

        protected bool IsDisposed
        {
            get { return _disposed; }
        }
        #endregion
    }
}
//using System;
//using System.Collections.Generic;
//using KeyCommon;
//using Lidgren.Network;
//using KeyCommon.Entities;

//namespace KeyGameServer
//{
//    internal class Simulation
//    {

//        private Game mGame;
    
//    //public Simulation (SimHost host , Game game, bool fixedFrequency, int timeStep)
//    //{
//    //    if (host == null || game == null) throw new ArgumentNullException();

//    //    mHost = host;
//    //    mPlayers = new Dictionary<int, Player>();
//    //    mGame = game;


//    //    // for games where only registered players can join the match, retreive the list of those players
//        // and then initialize their online status = false
//    //    ClientServerCommon.Player[] p =
//    //       (ClientServerCommon.Player[])mHost.mGamesStorageContext.RetreiveList(GetType(ClientServerCommon.Player).Name, "game_id", DbType.String, game.mID);
//    //    if (p != null) 
//        // {
//    //       for (int i = 0; i < p.Length; i++)
//    //            p[i].mOnline = false;
//    //        
//    //      }
//    //    }

//    //Public Property ActiveUsers() As User()
//    //    Get
//    //        'mGame.mSettings.mRules.GameType()

//    //    End Get
//    //    Set(ByVal value As User())

//    //    End Set
//    //End Property

  
//    public Game Game
//    {
//        get {return mGame;}
//        set { mGame = value;}
//    }

//    public void AddPlayer(Player player)
//    {
//        //mPlayers.Add(player.mID, player);

//        //// send the player, the list of all other registered players
//        //SendPlayerList(player);

//        //// send the player the map spec
//        //KeyCommon.MapSpec mapspec;
//        //mapspec.mWidth = mGame.mWidth;
//        //mapspec.mHeight = mGame.mHeight;
//        //mapspec.Tiles = mGame.mTiles;

//        //Lidgren.Network.NetConnectionBase connection  = (Lidgren.Network.NetConnectionBase)player.Tag;
//        //// send the mapspec.  When the client receives this, they know they are now successfully connected to the server.
//        //// NOTE: even when resuming a game, we can send a clientSpec that just has the width/height and no tiles
//        //// since the client can refer to cached tiles.  If a client discovers for some reason it does not have a cached tile
//        //// that the server thinks it should, the client can request it and the server will determine if it's allowed.
//        //connection.SendMessage(mapspec, NetChannel.ReliableUnordered);

//        //// send any new units and or cities 
//        ////   - do we wait for user to request them?  If so, then we're potentially re-sending everything everytime the user re-connects
//        ////     because we'd be trusting users to only request what they specifically don't already have
//        ////     
//    }

//    private void SendPlayerList(Player player)
//    {
//    //    ' get list of all of the registerd players
//    //    Dim p() As ClientServerCommon.Player
//    //    p = DirectCast(mHost.mGamesStorageContext.RetreiveList(GetType(ClientServerCommon.Player).Name, "game_id", DbType.String, Game.mID), ClientServerCommon.Player())

//    //    ' we endeavor to make sure that we only need to send the entire user list to any particular user ONCE
//    //    ' and from there out, only join/parts of individuals.  Thus we sychronize access to adding/removing of mUsers
//    //    Dim userList As New ClientServerCommon.UserList()
//    //    userList.Scope = ClientServerCommon.Scope.Game ' game indicates it's only sent to all users in this SimHost.Simulation and not all players on any game on this GameServer
//    //    ' add the user to the top of the list so that the client will be able to get their server assigned UserID first as opposed to last
//    //    ' which is what would happen if we added it in the forloop below
//    //    userList.AddUser(player.mName, player.mID)

//    //    If (p IsNot Nothing) Then
//    //        If (p.Length > 0) Then
//    //            For Each item In p
//    //                ' dont add the joined user to the user list twice. They were already added as first
//    //                If item.mName <> player.mName Then
//    //                    userList.AddUser(item.mName, item.mID)
//    //                End If
//    //            Next

//    //            ' notify new user of the existing users
//    //            Dim connection As Lidgren.Network.NetConnectionBase = DirectCast(player.Tag, Lidgren.Network.NetConnectionBase)
//    //            Console.WriteLine("Sending userlist to user " + connection.ToString())
//    //            connection.SendMessage(userList, userList.Channel)
//    //        End If
//    //    End If
//        }

//    public void RemovePlayer(Player player)
//    {
//    //    // TODO: i dont think we want to actually remove players, we just want to set their status to offline and then update the db
//    //    mPlayers.Remove(player.mID);
//    //    player.mOnline = False;
//    //    mHost.mGamesStorageContext.Store(player);


//    //    Dim userLeft As New ClientServerCommon.UserStatusChanged(ClientServerCommon.Enumerations.UserLeft);
//    //    userLeft.UserName = player.mName;
//    //    userLeft.UserID = player.mID;

//    //    // notify all users
//    //    Dim connections(mPlayers.Count - 1) As Lidgren.Network.NetConnectionBase;
//    //    Dim i As Integer = 0;
//    //    foreach KeyCommon.Player p in mPlayers.Values
//    //    {
//    //        connections(i) = DirectCast(p.Tag, NetConnectionBase);
//    //        i += 1;
//    //    }
//    //    // see note above the Broadcast() method in NetServer.cs for why here we must use Groupcast and not Broadcast()
//    //    mHost.mServer.Groupcast(userLeft, connections)
//    }


//    // temporary concepts
//    public void Tick(long elapsed )
//    {
//        //int startTime = 

//        //    update each AI entity for the provided time slice (this is to prevent us from doing nothing but AI updates for some undetermined interval
//        //    ' do AI stuff
//        //   Do while elapsed < mAITimeSlice
//       //    	entity = mAI.GetNext()  
//       //       entity.Update()
//        //       elapsed = Environment.TickCount() - startTime
//        //	loop

//        // update positions of all entities

//        // do other stuff required in the simulation

//        //mGame.mTurn += 1;
//        //    mSimulationTick = GetPerformanceCounter - iStartTime  ' how long it took to update the simulation.  We can even take averages over time.  
//    }

//    ////   this will be based on how long each SimulationTick Requires to run
//    //private void CalcAITimeSlice(ByVal iPercentage As Integer)
//    //{       
//    //    mAITimeSlice = 50;
//    //}
    
//    public void MessageProc(IncomingNetMessage message )
//    {
//        int command  = message.Buffer.ReadInt32();
//        Player player;
//        Lidgren.Network.NetConnectionBase connection ;

//        connection = message.m_sender;
//        player = (Player)connection.Tag;
    
//        switch (command)
//        {

//            case (int)Enumerations.Types.ChatMessage:
//                KeyCommon.Commands.ChatMessage chat = new KeyCommon.Commands.ChatMessage();
//                chat.Read(message.Buffer);
//                Console.WriteLine("Chat message sent {" + chat.Content + "}");

//                // manually set the chat.SenderID to match the connectionID.  We don't trust the original sender to have set this properly
//                chat.SenderID = player.PrimaryKey;
//                //NOTE: Broadcast shouldn't be used unless you truly intend to send to every user on the entire server, and not just this SImHost
//                // and if you do, do that, make sure that the clients either have a list of all the users so the SenderID can be looked up
//                // or have the client ignore the SenderID and perhaps have the server append the username instead so no lookup required
//                //mHost.mServer.Broadcast(chat, New Lidgren.Network.NetConnectionBase() {connection})
//                // notify all users
//                Lidgren.Network.NetConnectionBase[] connections = new Lidgren.Network.NetConnectionBase[mPlayers.Count ];  
//                int i  = 0;
//                foreach (Player p in mPlayers.Values)
//                {
//                    connections[i] = (NetConnectionBase)p.Tag;
//                    i++;
//                }
//                // see note above the Broadcast() method in NetServer.cs for why here we must use Groupcast and not Broadcast()
//                mHost.mServer.Groupcast(chat, connections);
//                break;

//               // client explicitly requesting authorization to download files
//            case (int)Enumerations.Types.FileDownloadRequest:
//                //KeyCommon.Commands.FileDownloadTicketRequest request ;
//                //request.Read(message.Buffer);

//                //// TODO: verify the user is allowed to download the requested files.  For my part
//                //// i just have to show that the server is capable of creating the authorization packet and sending it to the client
//                //// so i'm just going to use the tag without verifying the files requsted are valid for this particular user
//                //string tag = request.FileIDs;

//                //// TODO: the webserver's host data should come from the config file with perhaps an alternate mirror address if the first is down
//                //string mFileServerName  = "ProjectEvoFileServer1";
//                //string mFileServerPassword  = "FileServerPassword1";
//                //string mFileServerAddress  = "http://192.168.1.64/secure.asp";
//                //int mFileServerPort  = 80;
//                //const int FILE_DOWNLOAD_TIMEOUT = int.MaxValue;
//                //byte[] sessionKey = player.mSessionKey;
//                //string userip = "192.168.1.65"; //  connection.RemoteEndpoint.Address.ToString ' this is problematic if we connect to a local game server and then try to authenticate with remote file server, the ip will not match
//                //Authentication.Reply reply = new Authentication.Reply(userip, player.mName, sessionKey, mFileServerName, mFileServerPassword, FILE_DOWNLOAD_TIMEOUT, tag);

//                //KeyCommon.FileDownloadAuthorization authorization = new KeyCommon.FileDownloadAuthorization() ;
//                //authorization.ReplyTicket = reply.ToBytes();
//                //authorization.Host = mFileServerAddress;
//                //authorization.Port = mFileServerPort;
//                //authorization.FileName = "download.zip";  //TODO: this needs to match what the client should have it named 

//                //connection.SendMessage(authorization, NetChannel.ReliableUnordered);
//                break;

//                // Appearance
//                // case ClientServerCommon.Enumerations.UnitAppeared

//                //case ClientServerCommon.Enumerations.CityAppeared

//                // case ClientServerCommon.Enumerations.ImprovementAppeared

//                // Commands
//            //case (int)KeyCommon.Commands.MoveUnit:
//            //    Console.WriteLine("MoveUnit requested.");
//            //    break;
//            //case (int)KeyCommon.Commands.BuildCity:
//            //    Console.WriteLine("BuildCity requested.");
//            //    break;
//            //case (int)KeyCommon.Commands.Attack:
//            //    Console.WriteLine("Attack requested.");
//            //    break;
//            //case (int)KeyCommon.Commands.Pillage:
//            //    Console.WriteLine("Pillage requested.");
//            //    break;

//            //case (int)KeyCommon.Commands.QueueImprovement: // farms, pastures, cottages, etc
//            //    Console.WriteLine("QueueConstruction requested.");
//            //    break;
//            //case (int)KeyCommon.Commands.QueueUnit:
//            //    Console.WriteLine("QueueUnit requested.");
//            //    break;
//            //case (int)KeyCommon.Commands.QueueResearch:
//            //    Console.WriteLine("QueueResearch requested.");
//            //    break;

//            case (int)Enumerations.Types.RetreiveUserList:
//        Console.WriteLine("Retreive User List requested."); //TODO: this doesnt belong here does it?
//                break;

//            default:
//                Console.WriteLine("Unexpected game command from User.");
//                break;
//        }
//    }
    
//    //    private void UpdateScene(float elapsed)
//    //    {

//    //        if (!_core.SceneManager.SceneLoaded)
//    //        {
//    //            System.Threading.Thread.Sleep(100);
//    //            return;
//    //        }

//    //        // simulation update is for updating positions of traveling things
//    //        // and keeping game state up to date.  But as far as 
//    //        // updating animations for moving things i'm not sure.  Ideally i wouldnt want to
//    //        // but i can forsee problems whereby the animation of an actor or particle system
//    //        // if too much time lapses between updates, where the actor or particle system cant cope
//    //        // and something screws up?  But ideally, i would want Scene.Update to actually just be
//    //        // scene Culler... and to flag the Visible path as it goes.  This means that it will update
//    //        //bounding volumes for things that have moved or scaled during the _simulation.Update() as needed.(i.e. lazy update)
//    //        // and for items that just move, we dont need to recompute the local box just apply the translation
//    //        // to the matrix 
//    //        if (_core.SceneManager.Scenes[0].Simulation.IsRunning)
//    //            _core.SceneManager.Scenes[0].Simulation.Update(elapsed);

//    //        _core.CommandProcessor.Update();

//    //        ////// apply all updates from the simulation.Update() to the SceneGraph
//    //        ////// in a sychronized way.  The rule is relatively simple.  Input can directly
//    //        ////// modify a "Model" such as the Player object or the logic behind a GUI scrollbar
//    //        ////// but it cannot actually modify the underlying "View" or scene graph element that represents
//    //        ////// the player (e.g. Actor) or Quad used for the GUI.
//    //        ////            Commands[] commands = _simulation.GetQueuedCommands();
//    //        ////            _sceneManager.Execute (commands);

//    //        ////        // get the results from the scene for use by the _simulation at start of next itteration
//    //        ////        // such as Pick Results


//    //        // run the simulation at fixed frequency
//    //        // TODO: is the fixed frequency just done by the Simulation.cs and everything else such as
//    //        // checking network, is done in real time?  I dont think there is much SceneManager.Update() does
//    //        // on server except page data in /out and I think such a pager really has to work differently on a server than a view centric client
//    //        //   _sceneManager.Update(elapsed);

//    //    }

//    //    public SceneBase LoadScene(string path)
//    //    {
//    //        try
//    //        {
//    //            FileManager.Open(path);
//    //            SceneBase scene = _core.SceneManager.Load(path);
//    //            scene.Simulation.IsRunning = true;
//    //            Trace.WriteLine("Scene Loaded.");
//    //            return scene;
//    //        }
//    //        catch
//    //        {
//    //            Trace.WriteLine("Error --  Scene Failed to Load.");
//    //            return null;
//    //        }
//    //    }
      
//               // {

//               //     //  load the world map

//               //     _core = new Core(BASE_PATH, DATA_PATH, _configPath, true);
//               //     _core.SceneManager = new ServerSceneManager("sceneManager", null, null, null);

//               //     // TODO: the SceneInfo is loaded but LoadScene isnt paging in our entire scene's zone which might be something
//               //     // the server is reqt to do unlike clients which uses a camera proximity pager
//               ////     if (LoadScene(DATA_PATH + DEFAULT_MAP) == null)
//               ////         return 0; //TODO: this must set a flag so we can abort the Execute() loop

//               //     Console.WriteLine("Scene loadeing succcessful.");
//               //     m_lastGameUpdate = Stopwatch.GetTimestamp();

//               //     _gameInfo = new KeyCommon.Entities.GameServerInfo();
//               //     _gameInfo.Port = mPort;
//               //     _gameInfo.IP = mAddress;
//               //     _gameInfo.Message = "this is a test";
//               //     _gameInfo.Name = "Test Game Server";

//               // }
//    }
//}
