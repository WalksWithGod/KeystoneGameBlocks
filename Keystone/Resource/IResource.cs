using System;

namespace Keystone.Resource
{
    public interface IResource : IDisposable
    {
        string ID { get; }

        int RefCount { get; set; }

        string TypeName { get; }
    }
}