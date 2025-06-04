using System;

namespace Keystone.Entities
{
	/// <summary>
	/// Used for re-spawn location of playable units that have died or for spawning monsters.
	/// </summary>
	public class SpawnPoint : Entity 
	{
		private float mLastSpawnTime;
		private float mSpawnRateMilliseconds; 
		private string mPrefabRelativePath;
        private Entity[] mSpawnedEntities; // todo: we may only want to spawn a new entity when mSpawnedEntities == null or Length == 0


		//Check distance between playable unit's death position and the nearest point in your array 
		
		/// <summary>
		//
		/// </summary>
		internal SpawnPoint(string id) : base(id)
		{
			// visible in Editor only
			mEntityFlags &= ~KeyCommon.Flags.EntityAttributes.VisibleInGame;

            // TODO: a "spawn point" could perhaps just be a prefab itself
            //       that sets visibleingame == false and then uses custom script

            // use script to spawn. 

            // TODO: If we just use the script, then is this SpawnPoint entity even necessary?

            // spawning Entities should be driven by the server... perhaps server side script.
            // the script may need to obey certain game rules defined by the server and be able to determine victory conditions and end of game conditions... not just blindly spawn periodically.

            // thne script should call a API similar to what we do with EditorHost to instantiate a new Node and assign properties
 //           KeyCommon.Messages.Prefab_Load message = new KeyCommon.Messages.Prefab_Load(resourcePath);
 //           message.ParentID = parentID;
 //           AppMain.mNetClient.SendMessage(message);

            // for loopback purposes, i wonder if i can instantiate the entire hierarchy and then provide a single ID for Repository lookup? Typically i dont allow Entities to be shared but this wouldnt be a "share"

            // todo: how do we script missions?  I think the server should have a Mission.css script.  It can test a list of victory conditions to determine when the mission has succeeded or failed or is in progress.
            //  it can also determine when to spawn enemy vehicles.
        }
		
		
	}
}
