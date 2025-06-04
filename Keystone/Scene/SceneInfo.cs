using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

using Keystone.Elements;
using Keystone.Entities;
using Keystone.IO;
using Keystone.Resource;
using Keystone.Traversers;
using Keystone.Types;

namespace Keystone.Scene
{
    // TODO: i think my current setup of having a root node that is NOT loaded from the SceneInfo is bad.
    // the following SceneInfo should exist in a  RootRegion node and not SceneInfo.
    // 
    public class SceneInfo : Node, IGroup
    {
        public string Author;
        public DateTime Created;
        public string Description;

        // NOTE: following are more for a Scene where as SceneInfo is also for prefabs
        // public string Filename;
        // public string ModName;
        // public string ModPath;
        
        
        
        // TODO: or i can maybe use an array of 1st level nodes
        public string FirstNodeTypeName = "";
        public string Guid;  // - guid's of the entities.  If an array of 1st level nodes, then will match
                                    // but what about sub nodes?  I think perhaps a prefab should only have an author based on
                                    // the entire prefab.  If you use other people's components, then so be it.
                                    // Changing an existing prefab should result in the guid changing.  Ugh, how do we manage this?
                                    // Maybe just not worry about it for now.  Only thing we have to do is 
                                    // when a player is in an actual game, the server validates the crc32 of their mods by basically just
                                    // doing a punkbuster style check.  But mainly it's just so we know the user can play a game
                                    // they've joined... that they have the required mods installed and installed properly.

        public SceneType Type;
        public bool SerializeEmptyZones; 

        private List <Viewpoint> mViewpoints;

		
        public SceneInfo(string id, string[] typeNames) : this(id) 
        {
            // TODO: for now we just store and track one but easy to convert to an array of first level children
            FirstNodeTypeName = typeNames[0];
        }

        public SceneInfo(string id) : base(id)
        {
        	Type = SceneType.SingleRegion;
        }

        
        /// <summary>
        /// Viewpoints describe places the camera rendering context can hook to
        /// in order to move throughout the scene.  Without any viewpoints, you cannot
        /// travel though the scene.
        /// The beauty of having all viewpoints listed here is they are known regardless
        /// of whether parts of the multi-zoned scene are paged in.  
        /// Vehicles/Ships also spawn at Viewpoints however once spawned there
        /// perhaps users can then access Viewpoints within the ship.   This would make
        /// vehicles the only type that can have it's own viewpoints within it's own SceneInfo.
        /// </summary>
        public Viewpoint[] Viewpoints
        {
            get
            {
                if (mViewpoints == null) return null;
                return mViewpoints.ToArray();
            }
        }

        
        #region ITraverser Members
        public override object Traverse(ITraverser target, object data)
        {
            return target.Apply(this, data);
        }
        internal override ChildSetter GetChildSetter()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Resource Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="specOnly">True returns the properties without any values assigned</param>
        /// <returns></returns>
        public override Settings.PropertySpec[] GetProperties(bool specOnly)
        {
            Settings.PropertySpec[] tmp = base.GetProperties(specOnly);
            Settings.PropertySpec[] properties = new Settings.PropertySpec[3 + tmp.Length];
            tmp.CopyTo(properties, 3);

            properties[0] = new Settings.PropertySpec("firstnodetypename", FirstNodeTypeName.GetType().Name);
            properties[1] = new Settings.PropertySpec("type", typeof(int).Name);
            properties[2] = new Settings.PropertySpec("serializeemptyzones", SerializeEmptyZones.GetType().Name);

            if (!specOnly)
            {
                properties[0].DefaultValue = FirstNodeTypeName;
                properties[1].DefaultValue = (int)Type;
                properties[2].DefaultValue = SerializeEmptyZones;
            }

            return properties;
        }

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
                    case "firstnodetypename":
                        FirstNodeTypeName = (string)properties[i].DefaultValue;
                        break;
                    case "type":
                        Type = (SceneType)properties[i].DefaultValue;
                        break;
                    case "serializeemptyzones":
                        // TODO: must update saved entities to include this property.
                        // i think we should simply instance them, and re-save them
                        // automatically and be done with it.  
                        // Ideally this property is irrelevant on Prefab entities.  It's only
                        // useful for entire galaxy gen.
                        if (properties[i].DefaultValue == null)
                        {
                            Trace.WriteLine("SceneInfo.cs.SetProperties() - 'serializeemptyzones' stored property value is null for scene with root type '" + FirstNodeTypeName + "'.");
                            continue;
                        }
                        SerializeEmptyZones = (bool)properties[i].DefaultValue;
                        break;
                }
            }
        }
#endregion

        // TODO: when this SceneInfo is finally added to Scene, we should notify all
        // IEntitySystem node children so that they may initialize
        // eg. script calls to grab Digest.Records
        // - script makes calls to create a HUD item?... is a Digest a hud? not really
        // its actual simulation
        // - entities when attaching to scene if are subscribed to IES need to checkin and go "live".
        //   - this way we can pass digest calculations to the live instance and disable that record
        #region IGroup Members
        public void AddChild(Viewpoint viewpoint)
        {
            if (mViewpoints == null) mViewpoints = new List<Viewpoint>();
            if (mViewpoints.Contains(viewpoint)) throw new Exception();
            mViewpoints.Add(viewpoint);

            System.Diagnostics.Debug.Assert(viewpoint.Serializable == true); // viewpoints added to SceneInfo should alway shave serialiable = true or else we can't save them
            this.AddChild ((Node) viewpoint);
        }

        
        private void AddChild(Node child)
        {
        	// a viewpoint is an Entity and Entities can only have other Entities for parents typically...
        	// unless perhaps it's a viewpoint and this gives us some problems because we cast _parents[0] to entity
        	// and in this case it would be a SceneInfo.
        	// For propogate change flags we tend to try to get the single Parent as entity, but maybe we should as IGroup

            // ChangeSource.Child will ensure this notification is sent upwards
            // NOTE: always set flags early before any .AddParent/.Add so that if those other methods
            // result in calls to update a bounding box for example, that we then don't return here only
            // to set those flags as dirty again after they've just been updated
            SetChangeFlags(Keystone.Enums.ChangeStates.ChildNodeAdded, Keystone.Enums.ChangeSource.Child);
        	child.AddParent(this);
        }
        
        public void RemoveChild(Node child)
        {
        	if (!(child is Viewpoint)) throw new Exception();
        	
        	if (child is Viewpoint)
        	{
	            if (mViewpoints.Contains((Viewpoint)child)) 
    	        	mViewpoints.Remove((Viewpoint)child);
        	}

        	Repository.DecrementRef(child);// decrement first

            child.RemoveParent(this);

            // ChangeSource.Child will ensure this notification is sent upwards
            SetChangeFlags(Keystone.Enums.ChangeStates.ChildNodeRemoved, Keystone.Enums.ChangeSource.Child);
        }

        public void RemoveChildren()
        {
        	int count = 0;
        	// remove from end of list working to front so indices dont change
            if (mViewpoints != null)
            {
                count = mViewpoints.Count;
            	for (int i = count - 1; i >= 0; i--)
                    RemoveChild(mViewpoints[i]);
            }

        }

        public Node[] Children
        {
            get 
            {
        		if (mViewpoints == null) return null;
        		
        		return mViewpoints.ToArray();
            }
        }

        
        public int ChildCount
        {
            get 
            { 
            	if (mViewpoints == null) return 0;
				return mViewpoints.Count ;
            }
        }
        
        public virtual void MoveChildOrder(string childID, bool down)
        {
            throw new NotImplementedException();
        }

        public void GetChildIDs(string[] filteredTypes, out string[] childIDs, out string[] childTypes)
        {
            throw new NotImplementedException();
        }

        public Node FindDescendantOfType(string typename, bool recurse)
        {
            return Group.FindDescendantOfType(this, typename, recurse);
        }

        public Node[] FindDescendantsOfType(string typename, bool recurse)
        {
            return Group.FindDescendantsOfType(this, typename, recurse);
        }

        /// <summary>
        /// Finds the first descendant that matches.
        /// </summary>
        public Node FindDescendant(Predicate<Keystone.Elements.Node> match)
        {
            throw new NotImplementedException();
        }

        public Node FindNodeAtDescendantIndex(int index)
        {
            throw new NotImplementedException();
        }


        #endregion
    }
}