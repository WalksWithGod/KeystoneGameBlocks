using System;
using Keystone.Elements;
using Keystone.Traversers;

namespace Keystone.Collision
{

    // im confused as to how this should be implemented. for tilemaps, the footprint could contain
    // a trigger area that gets painted on the tilemap.  So how do we do a collision / trigger
    // traversal so that we can notify Entities when their is a collision?
    //
    // Colliders need to track entities that Enter/Stay/Exit a trigger volume.
    //
    // How did i implement collision detection for the lasers and the robots demo I made?
    // I think i was using brute force Producer and Consumption tests in Simulation.cs.
    // Indeed, we do a Simuation.FindConsumers() call that does a RegionNode.Query() to find matching consumers
    // to whatever was produced.  We could optimize the Query() call perhaps if we provide coordinates 
    // or a AABB or RECT that make up min/max position of the Entity between frames, and then test that box
    // against the octree or quadtree.  We would do this call for every Entity that has moved and test against
    // other Entities both static and moving (perhaps in some cases skipping tests for collisions between NPCs since
    // our pathing will allow them to walk through each other for version 1.0).  The Query can behave differently
    // when it passes from Octree to Quadtree and Tilemap.

    // I think we need to seriously try to get 2D tilemap collision detection working, especially considering
    // we hardly use collissions for interior of ship in version 1.0.  its mostly for trigger volumes.
    // And since we only have to check the tiles the npc has passed through, it should be fast.  
    // We should be able to just query the footprint data and compare it with the traversed footprint of the NPC.
    // But we do need a collider/trigger node i believe?

    // I just need to track when an NPC walks onto a Trigger defined by the footprint.  That trigger is part
    // of the elevator i think or at the very least, it's placed and then linked to th elevator using the elevator's friendly name.
    // This means all friendly names need to be unique.
    // Or maybe the trigger is part of the TileMap itself? The tilemap raises the event when something enters/exits/stays inside a trigger volume?
    // Wait, lets get back to the idea of the TileMap itself hosting trigger 2d trigger volumes that are created when appropriate
    // components are placed on the map.  I would need to be able to paint the exit/entrance to the elevator on the outside of the elevator
    // component prefab footprint so the footprint would be need to be larger than just 1x1 cell for example.  It would need to overlap
    // into adjacent cells and i think i do have a restriction that the footprint needs to be centered on the component so we'd need to
    // overlap on at least 2 adjacent cells to make the footprint bigger to accommodate the trigger area.
    // But then there is still the question of lasers/bullets/grenades, etc.  How do those collide with things? And we use 2D collisions,
    // on a per quadtree deck basis, how do we handle crew firing at enemy boarding party from upper or lower level such as a reactor room
    // where the reactor or engines expand upward multiple floors?

    // 2d Tilemap based design is great for users as the Sims series proves.  It's superior to the design of FPSCreator in terms of ease of use.
    // Using 2d Tilemap for general purpose for the type of game i'm making which is really a 3D game, is not a good idea.  It should just be for ship design/layout/construction.
    // and the plotting of networks/links within the ship.

    // If TileMap is purely for starship floorplan design and navmesh generation, then our choice should be to use 3d collision detection.
    // Ideally it should be multi-threaded.  But does this mean that all our walls and components need to have physics objects connected to them? OMG!
    // Sims3 and Hospital Simulation don't have to worry about such things.  
    // Or for version 1.0 we just say "it's command simulation only, no combat inside" and therefore no need for collision other than for trigger areas.
    // Or we limit to just 2D quadtree collisions and we use the 2d area of the footprint to define the collision bounds so no need for any other
    // collision information per object.  (though adding a Collider2DTilemap or Collider2d doesnt seem too expensive) and for walls we just collide
    // using edge ray tests like we do when mouse picking to place walls.

    // Should i focus on 2d triggers first then worry about more general cases of collision?
    // Should i just use TVPhysics for Interiors?  Can i get away with just limited use of TVPhysics? (Triggers, but just use raycast for lasers/bullets/etc?)

    // Even for our collisions, we don't need super detailed collision impact points and such.  We basically just need to know if there was a collision and perhaps
    // the impact point which we can find from a ray test.  We don't need dynamics after the collision, except maybe for a grenade to keep it from going through a wall. hrm.
    // 
    // Going to try using Jitter Physics library.  I still need a way to notify when a 3d bounding volume needs a physics collider such as our elevator and door trigger areas.
    // This has to happen when adding the Entity to the Interior and reading the attributes of the footprint as it is applied to the TileMap.
    // Also, do we have seperate physics instances per floor or not? I think no because in the future, things should be able to be thrown or shot from one floor to another and
    // we dont want to have to manage removing Physics bodies from one instance and adding them to another. Or is there a way to do this during AssetPlacement of the Prefab?

    public class ColliderTileMap2D : Node
    {
        private bool mIsTrigger;

        internal ColliderTileMap2D (string id) : base (id)
        {
            Shareable = true;
        }

        public static ColliderTileMap2D Create(string id)
        {
            ColliderTileMap2D node = (ColliderTileMap2D)Keystone.Resource.Repository.Get(id);
            if (node != null) return node;

            return new ColliderTileMap2D(id);
        }

        public void TestPhysics()
        {
            MTV3D65.TVPhysics physics = new MTV3D65.TVPhysics();
            // physics.CreateNewSimulation()
            // physics.SetThreadCount
            //physics.SetWorldSize(min, max); // could create seperate worldsizes per floor?
            //physics.Initialize();

        }

        #region Traversable
        public override object Traverse(ITraverser target, object data)
        {
            throw new NotImplementedException();
        }

        internal override ChildSetter GetChildSetter()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region ResourceBase members
        public override Settings.PropertySpec[] GetProperties(bool specOnly)
        {
            Settings.PropertySpec[] tmp = base.GetProperties(specOnly);
            Settings.PropertySpec[] properties = new Settings.PropertySpec[1 + tmp.Length];
            tmp.CopyTo(properties, 1);

            properties[0] = new Settings.PropertySpec("istrigger", mIsTrigger.GetType().Name);
            //properties[1] = new Settings.PropertySpec("forward", _forward.GetType().Name);
            //properties[2] = new Settings.PropertySpec("loop", _looping.GetType().Name);

            if (!specOnly)
            {
                properties[0].DefaultValue = mIsTrigger;
                //properties[1].DefaultValue = _forward;
                //properties[2].DefaultValue = _looping;
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

                    case "istrigger":
                        mIsTrigger = (bool)properties[i].DefaultValue;
                        break;
                    //case "forward":
                    //    _forward = (bool)properties[i].DefaultValue;
                    //    break;
                    //case "loop":
                    //    _looping = (bool)properties[i].DefaultValue;
                    //    break;
                }
            }
        }
        #endregion

        // https://tag.wonderhowto.com/fps-creator-triggers/

        // https://github.com/jslee02/awesome-collision-detection

        // https://gamedev.stackexchange.com/questions/104954/2d-collision-detection-xna-c

        // https://devblogs.nvidia.com/thinking-parallel-part-ii-tree-traversal-gpu/#disqus_thread

        // https://developer.nvidia.com/gpugems/GPUGems3/gpugems3_ch32.html
        // https://www.leadwerks.com/community/topic/9000-getting-a-list-of-entities-in-a-collision-trigger/

        // TODO: AddChild (Collider3D child) {} // Add this method to Entity
        // TODO: AddChild (Collider2D child) {} // Add this method to Entity
        // TODO: AddChild (ColliderTileMap2D child) {} // Add this method to Entity



    }
}

