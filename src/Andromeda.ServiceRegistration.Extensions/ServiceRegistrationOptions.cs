namespace Andromeda.ServiceRegistration.Extensions
{
    public sealed class ServiceRegistrationOptions
    {
        public bool RegisterAsyncSetupWithProviderServices { get; set; }
        public bool RegisterLifetimeHostedServices { get; set; }
        public bool RegisterAsyncSetupServices { get; set; } 
        public bool RegisterSingletonServices { get; set; } 
        public bool RegisterTransientServices { get; set; }
        public bool RegisterScopedServices { get; set; }
    }
}
