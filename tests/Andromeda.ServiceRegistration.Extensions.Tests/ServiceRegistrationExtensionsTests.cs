using static Andromeda.ServiceRegistration.Extensions.ServiceRegistrationExtensions;
using Andromeda.ServiceRegistration.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System;
using System.Linq;
using Andromeda.ServiceRegistration.Extensions.Tests.Helpers;
using Xunit;

namespace Andromeda.ServiceRegistration.Extensions.Tests
{
    internal class AsyncSetupWithProvider : IAsyncSetupWithProvider { public ValueTask Setup(IServiceProvider provider) => default; }
    internal class LifeTimeHosted : ILifetimeHostedService { public void Start() { } public void Dispose() { } }
    internal class AsyncSetup : IAsyncSetup { public ValueTask Setup() => default; }
    internal class Singleton : ISingleton { public bool Value => true; }
    internal class Transient : ITransient { }
    internal class Scoped : IScoped { }

    public class ServiceRegistrationExtensionsTests
    {
        static ServiceRegistrationExtensionsTests() => _services = ServicesProvider.Services;
        private static readonly IServiceProvider _services;

        [Fact]
        public void GetService_IAsyncSetupWithProvider_ShouldBeRegistered()
        {
            var serviceType = AsyncSetupWithProviderTypes.FirstOrDefault(t => t == typeof(AsyncSetupWithProvider));
            Assert.NotNull(serviceType);
            var service = (IAsyncSetupWithProvider)_services.GetService(serviceType);
            Assert.NotNull(service);
        }

        [Fact]
        public void GetService_ILifeTimeHostedService_ShouldBeRegistered()
        {
            var serviceType = LifetimeHostedServiceTypes.FirstOrDefault(t => t == typeof(LifeTimeHosted));
            Assert.NotNull(serviceType);
            var service = (ILifetimeHostedService)_services.GetService(serviceType);
            Assert.NotNull(service);
        }
        
        [Fact]
        public void GetService_IAsyncSetup_ShouldBeRegistered()
        {
            var serviceType = AsyncSetupTypes.FirstOrDefault(t => t == typeof(AsyncSetup));
            Assert.NotNull(serviceType);
            var service = (IAsyncSetup)_services.GetService(serviceType);
            Assert.NotNull(service);
        }

        [Fact]
        public void GetService_Singleton_ShouldBeRegistered()
        {
            var service = _services.GetRequiredService<Singleton>();
            Assert.NotNull(service);
            Assert.True(service.Value);
        }

        [Fact]
        public void GetService_Transient_ShouldBeRegistered()
        {
            var service1 = _services.GetRequiredService<Transient>();
            var service2 = _services.GetRequiredService<Transient>();
            Assert.NotNull(service1);
            Assert.NotNull(service2);
            Assert.NotEqual(service1, service2);
        }

        [Fact]
        public void GetService_Scoped_ShouldBeRegistered()
        {
            var scope = _services.CreateScope();
            var scoped1 = scope.ServiceProvider.GetRequiredService<Scoped>();
            var scoped2 = scope.ServiceProvider.GetRequiredService<Scoped>();
            Assert.NotNull(scoped1);
            Assert.NotNull(scoped2);
            Assert.Equal(scoped1, scoped2);

            var secondScope = _services.CreateScope();
            var scoped3 = secondScope.ServiceProvider.GetRequiredService<Scoped>();
            Assert.NotEqual(scoped1, scoped3);
        }
    }
}
