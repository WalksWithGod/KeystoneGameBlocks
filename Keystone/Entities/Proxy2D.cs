using System;

namespace Keystone.Entities
{
	/// <summary>
	/// ProxyControl2D uses Forms style mouse events.  Proxy2D uses standard 
	/// Entity mouse pick handling.
	/// </summary>
	public class Proxy2D : ModeledEntity, IEntityProxy
	{
    	protected Entity mReferencedEntity; // does not have to be a modeled entity.  Lights for instance can have a proxy hud icon 
    	
       
        public Proxy2D(string id, Entity referencedEntity)
            : base(id)
        {
        	mReferencedEntity = referencedEntity;
        	
        	UseFixedScreenSpaceSize = true;
            ScreenSpaceSize = 0.05f;
            Dynamic = false;
            SetEntityAttributesValue((uint)KeyCommon.Flags.EntityAttributes.HUD, true);
            SetFlagValue ((byte)NodeFlags.Serializable, false);
            
            // light icons for example are visible from far away when running editor
            MaxVisibleDistanceSquared = double.MaxValue; // AppMain.REGION_DIAMETER * AppMain.REGION_DIAMETER;
            Overlay = true;
        }
        
        public Entity ReferencedEntity 
    	{
			get { return mReferencedEntity; }
		}
    	
    	
        public override string Name
        {
            get
            {
                return mReferencedEntity.Name;
            }
            set
            {
                mReferencedEntity.Name = value;
                base.Name = value;
            }
        }
        
        
	}
}
