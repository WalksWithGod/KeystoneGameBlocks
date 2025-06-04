using Keystone.Cameras;
using Keystone.RenderSurfaces;
using Keystone.Types ;

namespace Keystone.FX
{
    //NOTE: An XML element deserialized can have a list of Providers (as xml node attributes)
    // that the element can be subscribed too.  However, if you later unsubscribe that element
    // (say via editing in the editor) that will clear the provider from the list of attributes
    // that would get serialized for this element.  So next time you load the world, that attribute
    // wont exist so it wont be subscribed to that provider.  Of course you can select the element
    // and then via the editor gui, choose to add it to the provider again.  
    // This may seem obvious, but in the case of wanting to toggle between TV's water FX and Zak's ocean FX
    // you cant simply itterate thru the scene removing meshes using that water and then itterate 
    // through the tree again to find out which ones to add to the other water FX provider.  There'd be no way to tell.
    // You'd simply have to hardcode a routine to make the swap right then and there.  The point is,
    // one traversal would have to do both unsubscribe and subscriptions.  So like if you
    // unsubscribed all the waters, then a minute later you want to subscribe them alla gain, there'd be no way
    // to find those specific meshes.  Instead, a better way to do it is to simply set the IFXProvider.Enable = false
    // and that would turn off the entire FX for all subscribed meshes.
    // UPDATE: hrm, well i have added a string[] IFXSubscriber.Providers field
    // which can hold the typenames.  Maybe that could be checked during an itteration to re-add them to a Provider 
    // and not be a hack.
    public enum FXLayout
    {
        Background,   // these items are rendered before overlay items are drawn and thus before TVEngine.ClearDepthBuffer() occurs
        Foreground    //  these are regular foreground items, rendered after things like sky but still before ovleray

    }

    public interface IFXProvider
    {

        // TODO: shadow manager will need an extra method for the subscriber to notify the IFXProvider
        // that it's position, rotation or scale has changed.  OnTransform_Callback() or something
        bool Enable { get; set; }
        FX_SEMANTICS Semantic { get; }
        FXLayout Layout { get; }

        bool NotifyOnTranslation { get; }
        // some providers will need to be notified when the subscriber does something... in this case changes Translation,Scale or Rotation
        int UpdateFrequency { get; set; }
        int Duration { get; set; } // you may only want to run the FX for a short time, -1 is infinite duration

        void Notify(IFXSubscriber subscriber);
        void Register(IFXSubscriber subscriber);
        void UnRegister(IFXSubscriber subscriber);

        void SetRSResolution(RSResolution res);
        void Update(double elapsedSeconds, RenderingContext context);
        void RenderBeforeClear(RenderingContext context);
        void Render(RenderingContext context);
        void RenderPost(RenderingContext context);
    }
}