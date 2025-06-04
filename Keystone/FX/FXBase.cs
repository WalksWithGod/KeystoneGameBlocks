using System;
using System.Collections.Generic;
using Keystone.Cameras;
using Keystone.Elements;
using Keystone.RenderSurfaces;
using Keystone.Types;

namespace Keystone.FX
{
    public enum FX_SEMANTICS : int
    {
        FX_NONE = -1,
        FX_SKY = 0, // sky must be rendered first or Zak's FX_BLOOM won't work. Sky should be rendered with ZWrites turned off
        FX_WATER_LAKE = 1,
        FX_WATER_OCEAN = 2,

        FX_DIRECTIONAL_LAND_SHADOW = 3,
        FX_POINTLIGHT_MESH_SHADOW = 4, // TODO: must come after the landshadowmapping so that the proper shaders are set
        // _OR_ we need to update our code to always restore the previous shader and not just automatically clear it to null
        FX_CAMERA_BUBBLE = 5,
        FX_IMPOSTER = 6,
        
        FX_INSTANCER = 7,
        FX_RAIN = 9, 
        FX_GLOW = 10,       // glow and bloom are post
    	FX_SHADOW_DEPTH_PASS
    }

    /// <summary>
    /// Generally, if it involves a RenderSurface, an FXBase derived class needs to be used so that
    /// RenderBeforeClear(), RenderPost() etc can be handled.  This also means a TVCamera should
    /// be created specifically for that RS.
    /// </summary>
    public abstract class FXBase : IFXProvider
    {
        public delegate Stack<Entities.Entity> CullRequestCallback(Camera camera);
        public delegate void RenderCallback();
        public delegate void UpdateCallback();

        protected CullRequestCallback _cullrequestCB;
        protected RenderCallback mRenderCB;
        protected UpdateCallback _updateCB;
        
    	protected List<IFXSubscriber> _subscribers = new List<IFXSubscriber>();
        protected FXLayout _layout = FXLayout.Foreground; 
        protected bool _enabled;
        protected int _frequency;
        protected int _duration = -1;
        protected bool _notify;

        protected FX_SEMANTICS _semantic = FX_SEMANTICS.FX_NONE;

        #region IFXProvider Members
        public virtual bool Enable
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        public FX_SEMANTICS Semantic
        {
            get { return _semantic; }
        }

       public  FXLayout Layout { get {return _layout;} }

        public bool NotifyOnTranslation
        {
            get { return _notify; }
        }

        public int UpdateFrequency
        {
            get { return _frequency; }
            set { _frequency = value; }
        }

        public int Duration
        {
            get { return _duration; }
            set { _duration = value; }
        }

        public virtual void Notify(IFXSubscriber subscriber)
        {
            throw new NotImplementedException();
        }

        public virtual void Register(IFXSubscriber subscriber)
        {
            if (_subscribers.Contains(subscriber))
                throw new ArgumentException("FXBase.Register() -- Subscriber already exists in list.");
            _subscribers.Add(subscriber);
        }

        public virtual void UnRegister(IFXSubscriber subscriber)
        {
            if (_subscribers == null || _subscribers.Contains (subscriber) == false) 
        		throw new ArgumentOutOfRangeException ("FXBase.UnRegister() - ERROR: Subscriber does not exist.");
        	
            _subscribers.Remove(subscriber);
        }

        public virtual void SetRSResolution(RSResolution res)
        {
            throw new NotImplementedException();
        }

        public virtual void Update(double elapsedSeconds, RenderingContext context)
        {
            
            //default behavior is to do nothing. Derived class must override to change behavior.
        }

        public virtual void RenderBeforeClear(RenderingContext context)
        {
            //default behavior is to do nothing. Derived class must override to change behavior.
        }

        public virtual void Render(RenderingContext context)
        {
            //default behavior is to do nothing. Derived class must override to change behavior.
        }

        public virtual void RenderPost(RenderingContext context)
        {
            //default behavior is to do nothing. Derived class must override to change behavior.
        }

        #endregion
    }
}