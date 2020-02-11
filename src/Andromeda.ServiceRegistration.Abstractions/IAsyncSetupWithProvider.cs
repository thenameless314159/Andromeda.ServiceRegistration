using System.Threading.Tasks;
using System;

namespace Andromeda.ServiceRegistration.Abstractions
{
    /// <summary>
    /// Register the implementation as a singleton service and triggers its startup method
    /// with a scoped <see cref="IServiceProvider"/> when the application starts.
    /// </summary>
    public interface IAsyncSetupWithProvider
    {
        ValueTask Setup(IServiceProvider provider);
    }
}
