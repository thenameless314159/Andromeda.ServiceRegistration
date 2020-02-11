using System;
using System.Threading;
using System.Threading.Tasks;
using Andromeda.ServiceRegistration.Abstractions;
using Andromeda.ServiceRegistration.Extensions.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Andromeda.ServiceRegistration.Extensions.Tests
{
    public class LifeTimeServiceTests
    {
        static LifeTimeServiceTests() => _services = ServicesProvider.Services;
        private static readonly IServiceProvider _services;

        [Fact]
        public async Task StartAsync_ShouldSetupServices()
        {
            var setupService = _services.GetRequiredService<ValueAsyncSetup>();
            Assert.False(setupService.IsSetup);
            var lifeTime = GetLifeTimeService();
            await lifeTime.StartAsync(new CancellationToken(false));
            Assert.True(setupService.IsSetup);
        }

        [Fact]
        public async Task StopAsync_ShouldDisposeServices()
        {
            var asyncDisposableService = _services.GetRequiredService<SingletonAsyncDisposable>();
            var disposableService = _services.GetRequiredService<SingletonDisposable>();
            Assert.False(asyncDisposableService.IsDisposed);
            Assert.False(disposableService.IsDisposed);
            var lifeTime = GetLifeTimeService();
            await lifeTime.StopAsync(new CancellationToken(false));
            Assert.True(asyncDisposableService.IsDisposed);
            Assert.True(disposableService.IsDisposed);
        }

        private static LifeTimeService GetLifeTimeService() => 
            new LifeTimeService(new AsyncSetupServicesOptions(), _services);
    }

    internal class ValueAsyncSetup : IAsyncSetup
    {
        public bool IsSetup { get; private set; }
        public ValueTask Setup()
        {
            IsSetup = true;
            return default;
        }
    }

    internal class SingletonAsyncDisposable : ISingleton, IAsyncDisposable
    {
        public bool IsDisposed { get; private set; }

        public ValueTask DisposeAsync()
        {
            IsDisposed = true;
            return default;
        }
    }

    internal class SingletonDisposable : ISingleton, IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}
