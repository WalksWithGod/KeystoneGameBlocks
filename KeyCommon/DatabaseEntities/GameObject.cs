using System;
using System.Collections.Generic;
using Settings;
using Lidgren.Network;

namespace KeyCommon.DatabaseEntities
{
    /// <summary>
    /// Dec.29.2012
    /// A game object is differentiated from an Entity because it is not derived
    /// from Node.  A node is a something that is interactable with, within the scene
    /// by other scene node elements (i.e. Enities).  This includes triggers, sensor volumes,
    /// and characters and walls and rocks, etc.
    /// Game objects on the other hand, are interactable only in an abstract sense. Waypoints
    /// can guide Entities. Contracts can create a relationship between entities and their
    /// behaviors.  Sensor contacts provide a nugget of information with which users or ai 
    /// behaviors can make decisions upon.  
    /// Can GameObjects be serialized/deserialized?
    /// Yes.  They are stored permanentaly in serverside databases. For loopback
    /// the database is also client side but is managed by the loopback server.
    /// They may be cached client side.  Not sure yet.
    /// 
    /// GameObject derived types are well defined.  They have no "custom properties"
    /// as Entities do through scripted domainobject.  Thus they are friendly to 
    /// database storage.
    /// 
    /// GameObjects can be referenced by Entity scripts but themselves are not scriptable.
    /// They are also not shareable.  All GameObjects are unique.
    /// 
    /// Dec.26.2012
    /// A game object has no world representation or need to interact in the game space.
    /// So that means it is not a trigger, sensor, zone, modeled entity, etc.
    /// It is a game specific construct however which can be serialized over the wire
    /// and as such has properties both intrinsic and custom.  
    /// Since these constructs are game specific, the keystone.dll game engine has no
    /// need to know what they are.
    /// Orders, Sensor Contacts, Contracts, Spy Data, etc.  In a way, GameObjects can be non 
    /// concrete, where as an Entity is always concrete tangible physical or at least
    /// virtually physical as in case of triggers and proximity sensors and such.
    /// The real undecided issue is whether a GameObject can host a DomainObject and be
    /// scripted.  I'm thinking for now at least no.  A GameObject can be used by Scripted 
    /// Entities but by themselves, cannot.
    /// 
    ///  other game objects I think might be match, player table, players' captain avatar,
    ///  match result, server info (although perhaps some of these are not gameobjets
    ///  but DabaseObjects which GameObject also inherits)
    ///  biosphere stats, political stats, culture, government, and such.  
    ///       
    /// </summary>
    public abstract class GameObject : IRemotableType // should Entity inherit from GameObject thus
    // should this abstract implementation be in Keystone.dll
    // then inherited by classes in Game01.dll
    {
        protected long mID;      // serially incremented from server DB
        protected string mOwner; // the entity that owns this game object
        // if a contract is a GameObject, can it not be owned by two owners?
        // or perhaps it's owned by the server and is then tested periodically
        // for conditions of the contract's termination to be reached.

        public GameObject(long id)
        {
            mID = id;
        }

        public GameObject(string owner)
        {
            mOwner = owner;
        }

        public long ID { get { return mID; } }

        public string TypeName { get { return this.GetType().Name; } }

        public string Owner { get { return mOwner; } }


        #region IRemotableType Members
        public int Type
        {
            get {throw new NotImplementedException();}
        }

        public NetChannel Channel
        {
            get { return Lidgren.Network.NetChannel.ReliableInOrder1; ; }
        }

        public virtual void Read(NetBuffer buffer)
        {
            mID = buffer.ReadInt64();
            mOwner = buffer.ReadString();

        }

        public virtual void Write(NetBuffer buffer)
        {
            buffer.Write(mID);
            buffer.Write(mOwner);


        }
        #endregion


        // TODO: do i need these GetProperties and SetProperties methods if 
        //       GameObject's are known to the scripts anyway?  Only Keystone
        //       doesn't need to know about them and it just needs to binary
        //       send across the wire with NetBuffer Reads and Writes

        #region GameObject methods


        /// <summary>
        /// Returns just the property of the specified name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="specOnly"></param>
        /// <returns></returns>
        public virtual Settings.PropertySpec GetProperty(string name, bool specOnly)
        {
            Settings.PropertySpec[] properties = GetProperties(specOnly);
            if (properties == null) return null;
            for (int i = 0; i < properties.Length; i++)
                if (properties[i].Name == name) return properties[i];

            return null;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="specOnly">True returns the properties without any values assigned</param>
        /// <returns></returns>
        public virtual Settings.PropertySpec[] GetProperties(bool specOnly)
        {
            Settings.PropertySpec[] properties = new Settings.PropertySpec[2];

            properties[0] = new Settings.PropertySpec("id", typeof(long).Name);
            properties[1] = new Settings.PropertySpec("owner", typeof(string).Name);

            if (!specOnly)
            {
                properties[0].DefaultValue = mID;
                properties[1].DefaultValue = mOwner;
            }

            return properties;
        }

        public virtual void SetProperties(Settings.PropertySpec[] properties)
        {
            if (properties == null) return;

            for (int i = 0; i < properties.Length; i++)
            {
                // use of a switch allows us to pass in all or a few of the propspecs depending
                // on whether we're loading from xml or changing a single property via server directive
                switch (properties[i].Name)
                {
                    case "id":
                        mID = (long)properties[i].DefaultValue;
                        break;
                    case "owner":
                        mOwner = (string)properties[i].DefaultValue;
                        break;
                }
            }
        }
        #endregion


    }
}
