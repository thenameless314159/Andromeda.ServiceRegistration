namespace Andromeda.ServiceRegistration.Extensions
{
    public sealed class AsyncSetupServicesOptions
    {
        public bool ExecuteAsyncSetupWithProviderServicesFirst { get; set; }
        public bool ExecuteAsyncSetupServicesFirst { get; set; }
        public bool FireAndForgetAsyncSetupServices { get; set; }
        public bool TriggersAsyncSetupServices { get; set; } = true;
    }
}
