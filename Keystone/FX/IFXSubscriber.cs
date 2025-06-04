namespace Keystone.FX
{
    // Interface implemented by scene Elements that interact with FXProviders like ShadowMap Provider.
    // to remove, whatever routine unloading the water would do 
    // foreach (IFXProvider p in element.FXProviders)
    //  element.Unsubscribe(p)
    // or simply element.UnsubscribeAll
    // and internally the Element would do 
    // p.UnRegister (me) : _fxProviders.Remove(p)
    // The point here is that we dont have to know in advance that a provider has a reference
    // to any particular Element we're destroying.  Each subscriber maintains a reference to all of its providers
    // and is responsible for unsubscribing from those providers prior to its own destruction.
    public interface IFXSubscriber
    {
        IFXProvider[] FXProviders { get; }
        string[] Providers { get; set; } // typenames of all the providers to be subscribed too (for deserialization purposes)
        FXSubscriberData[] FXData { get; set; }
        bool InFrustum { get; set; }
        void Subscribe(IFXProvider fxProvider);
        void UnSubscribe(IFXProvider fxProvider);
        void UnSubscribeAll();
    }
}