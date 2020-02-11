using System;
using Microsoft.Extensions.DependencyInjection;

namespace Andromeda.ServiceRegistration.Extensions.Tests.Helpers
{
    internal static class ServicesProvider
    {
        public static IServiceProvider Services { get; }

        static ServicesProvider()
        {
            Services = new ServiceCollection().AddAllServicesFrom(true, typeof(ServicesProvider).Assembly)
                .BuildServiceProvider();
        }
    }
}
