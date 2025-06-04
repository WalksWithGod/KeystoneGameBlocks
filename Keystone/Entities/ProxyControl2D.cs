using System;
using System.Collections.Generic;

using KeyCommon.Flags;
using Keystone.Appearance;
using Keystone.Controls;
using Keystone.Elements;
using Keystone.Events;
using Keystone.Resource;

namespace Keystone.Entities
{
	
    public class ProxyControl2D : Control2D, IEntityProxy
    {

    	protected Entity mReferencedEntity; // does not have to be a modeled entity.  Lights for instance can have a proxy hud icon 
    	
       /// <summary>
       /// Unlike Proxy2D, ProxyControl2D uses Forms style mouse events that can be handled uniquely for each proxy instance if desired.
       /// This can be used to more easily do things like mouse rollover states for these 2D proxies or play sound FX when clicked, etc.
       /// </summary>
       /// <param name="id"></param>
       /// <param name="referencedEntity"></param>
        public ProxyControl2D(string id, Entity referencedEntity)
            : base(id)
        {
        	mReferencedEntity = referencedEntity;
        	
        	UseFixedScreenSpaceSize = true;
            ScreenSpaceSize = 0.05f;
            Dynamic = false;
            SetEntityAttributesValue((uint)EntityAttributes.HUD, true);
            SetFlagValue ((byte)NodeFlags.Serializable, false);
            
            // light icons for example are visible from far away when running editor
            MaxVisibleDistanceSquared = double.MaxValue; // AppMain.REGION_DIAMETER * AppMain.REGION_DIAMETER;
            Overlay = true;
        }

        #region ITraverer
        public override object Traverse(Traversers.ITraverser target, object data)
        {
            return target.Apply(this, data);
        }
        #endregion


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
        
        /// <summary>
        /// Creates a Billboard3d geometry based InputAwareProxy entity.  If a cached copy exists since
        /// previous frame, return it instead of creating a new one. 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="texturePath"></param>
        /// <param name="defaultMaterial"></param>
        /// <param name="mouseEnter">Can be null.</param>
        /// <param name="mouseLeave">Can be null.</param>
        /// <param name="mouseDown">Can be null.</param>
        /// <param name="mouseUp">Can be null.</param>
        /// <param name="mouseClick">Can be null.</param>
        /// <returns></returns>
        public static ProxyControl2D Create(Entity targetOfProxy, Model model, 
            EventHandler mouseEnter, EventHandler mouseLeave,
            EventHandler mouseDown, EventHandler mouseUp,
            EventHandler mouseClick)
        {

        	// TODO: shouldn't this be done in Repository.Factory.Create along with rest of static creation methods?
            ProxyControl2D proxy =
                new ProxyControl2D(Repository.GetNewName(typeof(Proxy3D)), targetOfProxy);

            proxy.AddChild(model);

            proxy.MouseEnter += mouseEnter;
            proxy.MouseLeave += mouseLeave;
            proxy.MouseDown += mouseDown;
            proxy.MouseUp += mouseUp;
            proxy.MouseClick += mouseClick;
            // no drag needed
            //menuButton.MouseDrag += MenuButton_OnMouseDrag;


            return proxy;
    	}
    }
}
