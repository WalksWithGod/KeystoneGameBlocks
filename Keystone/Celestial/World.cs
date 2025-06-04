using System;
using System.Xml;
using Keystone.IO;
using Keystone.Resource;

namespace Keystone.Celestial
{
    public class World : Body 
    {
        // - TODO: I think SQL will be a must and I think our XML scenes might be able to handle Hierarchy and 3D Engine but not game specific data
        //
        // - TODO: generate worlds when in the system or in adjacent system 
        //			- does our Pager do this?  What triggers it?
        //			- do we generate worlds seperately as we get close to them too?
        //				- ideally yes.  The closer we are the more detail that gets generated.
        //			- how do we handle difference between client and server versions of the universe?

        // TODO: our pager works well with Regions, but we need a system for paging in IProcedural nodes
        //       When our culler visits them, (per renderingcontext) maybe we can queue paging in of the IProcedural
        //       interface IProcedural
        //		 {
        //			GenerationType Type;     // automatic, ondemand (ondemand is for a scan or from an away team mission that uncovers new species or civilization details)
        //			GenerationStatus Status; // NotGenerated, Generating, Generated, Disposing
        //			void Generate ();
        //		 }
        //
        //		- because we potentially need to traverse deep into a Region's child graph (eg. planet cities that get procedually generated)
        //  		- we could have a seperate traverser that is similar to our Pager that handles procedural generation and disposing (saving out to DB as well)    
        //			- it needs to be in charge of checking DB for existing data first

        // - TODO: do not generate worlds that are already on disk?
        // -       if we always regenerate, how do we include state changes that were made by the user?
        //			- eg. user has found and taken an artifact that was generated there
        //			- eg. user has carried out a one-time only mission that was available on that world
        //				- missions would need to be procedurally generated for the world as well and then a record for that mission's state if not a pristine mission, should exist in database
        //					- pristine missions are missions that the user has not accepted or acted on yet.
        //         - do we regenerate NPCs that exist on world too?
        //				- as in VIP/key people
        //		   - how do we simulate world events and happenings if a system is paged out?
        //			 - don't we need it on disk? in database?
        //			- server side can store on disk "key" / VIP worlds or systems and simulate them every X interval

        // Handling NPC advantages and disadvantages
        //	http://gamedev.stackexchange.com/questions/93587/handling-an-item-database-with-procedurally-generated-items
        // In the above stackexchange link, it would be like storing the db ID of the advantages and disadvantages as a varchar (or string)

        // Quest Generation
        // http://larc.unt.edu/techreports/LARC-2011-02.pdf
        // http://blogs.perl.org/users/ovid/2014/07/procedural-quest-generation-in-perl.html
        // 


        private Biosphere _biosphere;  // TODO: make these nodes? So i can have a proper Plugin.  Get rid of "moon" 
        private WorldType _worldType;
        //private byte Size;

        public World (string id)
            : base(id)
        {
            _biosphere = new Biosphere();
        }



        #region ITraversable Members
        public override object Traverse(Traversers.ITraverser target, object data)
        {
            return target.Apply(this, data);
        }
        #endregion

        

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

            // this is complicated because these classes cannot be treated like 
            // child XmlNode's because during our filemanager read we treat child xmlnodes as true Children()
            // and here Biosphere (and in return Biosphere has Atmosphere) is not a child but a composite structure
            // so i'm not sure how we want to handle this frankly.
            // We treat "Moons" as relational children to this world.

            // well one thing to consider is that Biosphere and in turn Atmosphere only exist as part of a World and cannot be stored seperately.
            // Thus during ReadXml, we can instance these objects here prior to setting their values.  
            // TODO: Where do i load the biosphere??>?>? 
            //_biosphere = new Biosphere();

            properties[0] = new Settings.PropertySpec("worldtype", typeof(int).Name);

            if (!specOnly)
            {
                properties[0].DefaultValue = (int)_worldType;
            }

            return properties;
        }

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
                    case "worldtype":
                        _worldType =(WorldType)(int)properties[i].DefaultValue;
                        break;
                }
            }
        }

        // worlds can have other worlds as children such as Planet -> Moon
        public void AddChild(World planet)
        {
            AddChild((Body) planet);
        }

        public Biosphere Biosphere
        {
            get { return _biosphere; }
            set { _biosphere = value; }
        }

        public WorldType WorldType
        {
            get { return _worldType; }
            set { _worldType = value; }
        }
    }
}