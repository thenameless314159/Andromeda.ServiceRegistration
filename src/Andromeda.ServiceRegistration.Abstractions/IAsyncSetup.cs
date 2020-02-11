using System.Threading.Tasks;

namespace Andromeda.ServiceRegistration.Abstractions
{
    /// <summary>
    /// Register the implementation as a singleton service and triggers its startup method
    /// when the application starts.
    /// </summary>
    public interface IAsyncSetup
    {
        ValueTask Setup();
    }
}
