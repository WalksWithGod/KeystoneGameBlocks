using System;

namespace KeyCommon.DatabaseEntities
{
    // TODO: this code is completely unnecessary for KGB.  IT was designed for Evolution
    // Our KGB uses Helpers.ExtensionMethods GetProperties/SetProperties read/write mechanism 
    // for serializing and deserializing properties to/from Lidgren NetBuffer or to/from a database
    // record or any other format.
    // TODO: Here i am at this stage of the project and im trying to figure out
    //       whether a "gameobject" does not need to inherit from Node.cs.  I think the Scene Graph
    //       failings are impacting me now.  SceneGraph vs Component Entities vs Game Objects.
    //       We want to have scripted Entities (including Digests) and we also want
    //		 more lightweight gameobjects that we know are never visually represented in a 3D Scene Graph.
    //		 Things like "contract", "sensor contact", "crew station", "city", "weapon link", "power grid link", "radar component", "player", "character", "corporation", 
    //		 We know that the SceneGraph's primary role is for traversal of visible elements in a Transform
    //       hierarchy and for managing other visual and audio elements in the Scene.  It's not for general simulation of game objects.
    //		 GameObjects, can reference Scene Graph Entities, but GameObjects are not actually
    //		 part of the 3D Scene.  This is why things are so confusing.  I think things would be easier
    //		 if our Entity/ModeledEntity was RenderableObject and had a reference to a GameObject
    //		 that was a Star or City or Sensor Contact etc and that GameObject could be scripted.
    //		 But then what about Physics Nodes, Behavior nodes and ScriptNodes (DomainObjects) and  AI nodes?
    //		 Being able to serialize them into a xml db is nice.  Visibility Graph/DAG, Transform Graph/DAG.
    //       Being able to traverse the visibility graph, add renderable nodes to buckets, etc. And the
    //		 hierarchical partitioning is helpful.  GamePlayObjects perhaps need their own structure.
    //		 These can reference an Entity in the SceneGraph?  What does a game like Dwarf Fortress do?
    //		 It doesn't use a SceneGraph, but it simulates many gameObjects. Dwarves, monsters, tiles & resources,
    //		 water simulation, 
    //		 Ugh.  Or maybe just continue with what I'm doing?  It's too late for refactoring/overhauls.  Just get it done.  
    // 		 What does that mean?  It means staying with our current Entity/SG hybrid.  Player is then an Entity.  Vehicle
    //		 is referenced by Player.  Vehicle is a child of Player.  Components (radar, planetary survey array, launchers, computers, etc) as Entities as well.  
    //		 but they can be entities without ModeledEntities.  So maybe the question is, can we push
    //		 our entities into a DB like sqlite?  First, AGAIN, we must start with a Player object. Server must spawn it 
    //		 for the client to re-create.  
    //
    //		 Let's start by putting Player object as type of Entity directly under AppMain and/or AppMain.CoreClient
    //		 Let's have our universe generator, supply a spawnLocation entity that was specified by user during universe
    //		 gen configuration.  Then the spawnLocation can spawn a Player from server side to Client.  When a user connects
    //		 the game checks for a spawn point for that player and executes it?  The spawnpoint has a script that runs once
    //		 when the entity is activated by client connection event.  But what about when user saves, exits and resumes?
    //		 This is an issue with respect to saving/loading vs scene generation.  It also suggests that client is allowed
    //		 to modify the Scene database.  This is why it would be good to have seperate db's for Scene vs SavedState
    //			
    //		 Look, our Scene XMLDB is being used as a fully fledged game DB.  But this is going to stifle us as we
    //       look to add new GameObjects to the simulation.  The question is can our hierarchical structure be saved
    //		 to SQL db easily?  We investigated this some time ago and it was "no."  But we're trying to get Player object
    //		 instantiated and referenced when player "Connects" to loopback.  That's it.   How is the Player object read in,
    //		 recognized, referenced on Client after connecting to loopack and loading in Scene XMLD.
    //
    //		 What if we keep using PREFABS as XMLDB's but then reference those prefab relative paths in our sqlite db?
    //		 So what if our Player selects Vehicle, that vehicle is stored along with the playerID in db.	
    //		 A second command is sent from loopback to instantiate selected vehicle?	
    //
    //       1) app main
    //		 2) netClient created connect to server
    //		 3) scene to load passed in
    //		 4) scene is loaded and shared between loopback client and loopback server
    //		 5) _lookup user name and query db for user player vehicle
    //		 6) 	spawn command from server to client (or should we think of spawn more as "attaching" to loaded Vehicle? We need an event when player owned vehicle is instantiated)
    //				do we have an event for each Entity on instantiation where we could then query a bool IsLocalPlayer?
    //
    //		 Client
    //		 |_Connection
    //		 |
    //		 Player
    //          |_User
    //			|_Vehicle
    //			|_CharacterProfile
    //
    //		 
    public abstract class DatabaseEntity // TODO: Is this really different from a GameObject?  GameObject's like a "task" or "order" is stored in the .db aren't they?
    {
        protected long mPrimaryKey = 0;
        protected int mTypeID;

        public long PrimaryKey
        {
            get { return mPrimaryKey; }
            set { mPrimaryKey = value; }
        }

        // These members are identicle to IRemotableType except that it does not include the Channel
        //  to specify how this type should be sent. 
        // TODO: Should this implement the interface for IRemotableType?
        #region Entity members
        /// <summary>
        /// An integer value used to identify the type of Entity in the buffer so that our Factory.Create() method can generate the correct one.
        /// </summary>
        public int Type { get { return mTypeID; } }

        public virtual void Read(Lidgren.Network.NetBuffer buffer)
        {
            mPrimaryKey = buffer.ReadInt64();
        }

        public virtual void Write(Lidgren.Network.NetBuffer buffer)
        {
            buffer.Write(mPrimaryKey);
        }
        #endregion
    }
}
