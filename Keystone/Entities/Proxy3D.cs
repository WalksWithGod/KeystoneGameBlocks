using System;
using Keystone.Types;
using Keystone.Elements;
using Keystone.Resource;

namespace Keystone.Entities
{
	public interface IEntityProxy 
	{
		Entity ReferencedEntity {get;}
		string Name {get; set;}
	}
	
    /// <summary>
    /// A proxy
    /// 0) Does NOT get updated by Simulation.
    /// 1) Is NOT saved to prefab or scene.
	/// 2) Does NOT get added to the main Scene.  
	/// 3) It does NOT get added to the parent of the entity that it's referencing! 
	/// 4) It IS sandboxed to a branch of the scene reserved entirely for GUI
	/// so that its easy to enable/disable or swap out with something else. 
	/// This is very important to remember when thinking about how the hierarchical 
	/// transforms work!
	/// 5) The proxy does NOT use any hierarchical transform information of the 
	/// entity it hosts.  Likewise, a child attached to a proxy only gets the 
	/// hierarchical info of the Proxy.  NOT of the entity it's parent proxy
    /// is hosting!
    /// 
    /// The idea behind a proxy is similar to injecting an LOD replacement for an Entity
    /// at runtime, but without a need for that proxy to be inserted into the scene at the
    /// location of the entity it's referencing.
    /// With proxy there's no need to design the referenced entity ahead of time to have
    /// this LOD.  It provides freedom for being able to easily change the representation of
    /// the referenced entity at runtime, without having to open up a bunch of disparate
    /// entities and tweaking their LOD structure.
    /// However, since it reference the entity it's standing in for, that data is still
    /// available through the proxy when the user interfaces with it.
    /// 
    /// Furthermore, because proxys are not attached as children to either the entity it
    /// references or the parent of the entity it references, it can be used for hud indicators
    /// to show where off screen targets are located.  In other words, we can position
    /// proxys in locations in world space where the parent is not even located in order ot help
    /// guide the player to that target entity.
    /// </summary>
    public class Proxy3D : ModeledEntity, IEntityProxy
    {
        protected Entity mReferencedEntity; // does not have to be a modeled entity.  Lights for instance can have a proxy hud icon 
		protected string mReferencedEntityID;
		
		
		/// <summary>
		/// Similar to StarDigest, the referencedEntityID may be paged out for memory saving purposes.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="referencedEntityID"></param>
        public Proxy3D(string id, string referencedEntityID)
            : base(id)
        {
        	if (string.IsNullOrEmpty (referencedEntityID)) throw new ArgumentNullException();
            mReferencedEntityID = referencedEntityID;

        	UseFixedScreenSpaceSize = false;
            //ScreenSpaceSize = 0.05f;
            Dynamic = false;
           // SetEntityFlagValue((uint)EntityFlags.HUD, true);
            SetFlagValue ((byte)NodeFlags.Serializable, false);
            
            // light icons for example are visible from far away when running editor
            MaxVisibleDistanceSquared = double.MaxValue; // AppMain.REGION_DIAMETER * AppMain.REGION_DIAMETER;
            //Overlay = true;
            
            // TODO: if the referencedEntityID is paged out, we should page it in so we can grab the friendly name so it can be returned?
            //       
        }
        
        public Proxy3D(string id, Entity referencedEntity)
            : this(id, referencedEntity.ID)
        {
            if (referencedEntity == null) throw new ArgumentNullException();
            mReferencedEntity = referencedEntity;
            mFriendlyName = mReferencedEntity.Name;
        }

        public Entity ReferencedEntity { get { return mReferencedEntity; } }

        public override string Name
        {
            get
            {
            	// TODO: if the referencedEntity is paged out, should we set this Name one time
            	//       from a temp copy of a paged in clone so we never have to do it again?
            	//       recall "Name" is used very often for rendering labels
            	// TODO: it would be nice here to just read a sqlite db instead of the xml db
            	//       especially since the xml db is mostly about scenegraph, and less about actual simulation
            	//       and since digests can use simpler versions of simulation for mananaging many objects when
            	//       the camera is not in view.  When the camera is close, the simulation needs to do more real-time
            	//       simulation of movement which it doesnt have to do when far away.
            	//       - For now, we will simply use mName local var.  The other problem though is that the name may change
            	//       on the referenced entity so not sure how we will detect that change so that we can update it.
            	return base.Name;
                //return mReferencedEntity.Name;
            }
//            set
//            {
//                mReferencedEntity.Name = value;
//                base.Name = value;
//            }
        }


        // Dec.23.2012 - It is completely unnecessary to override any Transform 
        // properties or methods. At one point I confused myself and thought it was
        // but was totally over thinking the issue. There is no need.
        //public override Vector3d Translation
        //{
        //    get
        //    {
        //       // mReferencedEntity.der
        //        return mReferencedEntity.Translation;
        //    }
        //}

        //public new Vector3d DerivedTranslation
        //{
        //    get { return mReferencedEntity.DerivedTranslation; }
        //}

        //public override Matrix RegionMatrix
        //{
        //    get
        //    {
        //        return mReferencedEntity.RegionMatrix;
        //    }
        //}
    }
}