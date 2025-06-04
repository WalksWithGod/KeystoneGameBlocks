using System;

namespace Keystone.FX
{
    public class FXSubscriberData
    {
        public IFXProvider Provider;
        public bool InFrustum;
        public Object[] Data; // custom data that is subscript by FX_SEMANTIC.ID that can be set by the provider and used for any purpose
    }
}