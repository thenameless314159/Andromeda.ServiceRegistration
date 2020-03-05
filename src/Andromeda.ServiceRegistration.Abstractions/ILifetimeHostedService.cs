using System;

namespace Andromeda.ServiceRegistration.Abstractions
{
    /// <summary>
    /// Base interface for services that will be registered on the host
    /// application lifetime.
    /// </summary>
    public interface ILifetimeHostedService : IDisposable
    {
        void Start();
    }
}
