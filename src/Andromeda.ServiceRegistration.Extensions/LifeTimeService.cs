using static Andromeda.ServiceRegistration.Extensions.ServiceRegistrationExtensions;
using Andromeda.ServiceRegistration.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Andromeda.ServiceRegistration.Extensions
{
    internal sealed class LifeTimeService : IHostedService
    {
        private readonly AsyncSetupServicesOptions _options;
        private readonly IServiceProvider _provider;

        public LifeTimeService(AsyncSetupServicesOptions options, IServiceProvider provider)
        {
            _provider = provider; 
            _options = options;

            if (LifetimeHostedServiceTypes.Count < 1) return;
            var hostLifetime = provider.GetService<IHostApplicationLifetime>();
            if (hostLifetime == default) return;

            foreach (var lifetimeService in LifetimeHostedServiceTypes)
            {
                var service = (ILifetimeHostedService) provider.GetService(lifetimeService);
                hostLifetime.ApplicationStopped.Register(service.Dispose);
                hostLifetime.ApplicationStarted.Register(service.Start);
            }
        }

        public Task StartAsync(CancellationToken token)
        {
            if (!_options.TriggersAsyncSetupServices) return Task.CompletedTask;
            if (_options.ExecuteAsyncSetupWithProviderServicesFirst) 
                return executeAsyncSetupWithProviderServicesFirst();
            if (_options.ExecuteAsyncSetupServicesFirst)
                return executeAsyncSetupServicesFirst();

            return Task.WhenAll(
                SetupAsyncSetupWithProvider(token), 
                SetupAsyncSetup(token));

            async Task executeAsyncSetupWithProviderServicesFirst() {
                await SetupAsyncSetupWithProvider(token); await SetupAsyncSetup(token);
            }
            async Task executeAsyncSetupServicesFirst() {
                await SetupAsyncSetup(token); await SetupAsyncSetupWithProvider(token);
            }
        }

        public Task StopAsync(CancellationToken token) => Task.WhenAll(
            DisposeAsyncAllServices(token),
            DisposeAllServices(token));

        private async Task SetupAsyncSetupWithProvider(CancellationToken token = default)
        {
            foreach (var serviceType in AsyncSetupWithProviderTypes)
            {
                if (token.IsCancellationRequested) break;
                var service = _provider.GetService(serviceType);
                if (service == null) throw new ArgumentNullException(nameof(service),
                    $"{serviceType.Name} is not registered in the service provider.");

                using var scope = _provider.CreateScope();
                if (!_options.FireAndForgetAsyncSetupServices)
                    await ((IAsyncSetupWithProvider)service).Setup(scope.ServiceProvider);
                else _ = ((IAsyncSetupWithProvider)service).Setup(scope.ServiceProvider);
            }
        }

        private async Task SetupAsyncSetup(CancellationToken token = default)
        {
            foreach (var serviceType in AsyncSetupTypes)
            {
                if (token.IsCancellationRequested) break;
                var service = _provider.GetService(serviceType);
                if (service == null) continue;

                if(!_options.FireAndForgetAsyncSetupServices) 
                    await ((IAsyncSetup) service).Setup();
                else _ = ((IAsyncSetup)service).Setup();
            }
        }

        private async Task DisposeAsyncAllServices(CancellationToken token = default)
        {
            foreach (var disposable in DisposableAsyncTypes)
            {
                if (token.IsCancellationRequested) break;
                var service = _provider.GetService(disposable);
                if (service == null) continue;

                try { await ((IAsyncDisposable) service).DisposeAsync(); }
                catch { /* exceptions fully discarded */ }
            }
        }

        private Task DisposeAllServices(CancellationToken token = default)
        {
            foreach (var disposable in DisposableTypes)
            {
                if (token.IsCancellationRequested) break;
                var service = _provider.GetService(disposable);
                if (service == null) continue;

                try { ((IDisposable)service).Dispose(); }
                catch { /* exceptions fully discarded */ }
            }

            return Task.CompletedTask;
        }
    }
}
