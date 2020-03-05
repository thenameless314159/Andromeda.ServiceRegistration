using Microsoft.Extensions.DependencyInjection.Extensions;
using Andromeda.ServiceRegistration.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;

namespace Andromeda.ServiceRegistration.Extensions
{
    public static class ServiceRegistrationExtensions
    {
        /// <summary>
        /// Configure the service registration logic, refer at the <see cref="ServiceRegistrationOptionsBuilder"/>
        /// documentation for more infos.
        /// </summary>
        /// <param name="services">The service collection where the dependencies will be registered.</param>
        /// <param name="configure">The configuration delegate of the service registration logic.</param>
        /// <returns>The service collection provided.</returns>
        /// /// <remarks>Must be called first on the ConfigureService method of your hosting logic if you need to triggers the <see cref="IAsyncSetup"/>, 
        /// else they might start after your own hosted services.</remarks>
        public static IServiceCollection ConfigureServices(this IServiceCollection services, Action<ServiceRegistrationOptionsBuilder> configure)
        {
            if(configure == default) throw new ArgumentNullException(nameof(configure));
            var builder = new ServiceRegistrationOptionsBuilder();
            configure(builder);

            return builder.RegisterAllDependencies(services); ;
        }

        /// <summary>
        /// Register all the services defined in the <see cref="assemblies"/> provided
        /// from the <see cref="Abstractions"/> interfaces : <see cref="IAsyncSetupWithProvider"/>,
        /// <see cref="IAsyncSetup"/>, <see cref="ISingleton"/>, <see cref="ITransient"/>, <see cref="IScoped"/>
        /// </summary>
        /// <param name="services">The service collection where the dependencies will be registered.</param>
        /// <param name="triggersAsyncSetupFirst">Whether <see cref="IAsyncSetup"/> must be triggered first on application startup.</param>
        /// <param name="assemblies">The assemblies where the services from <see cref="Abstractions"/> are defined.</param>
        /// <returns>The service collection provided.</returns>
        /// <remarks>Must be called first on the ConfigureService method of your hosting logic if you need to triggers the <see cref="IAsyncSetup"/>, 
        /// else they might start after your own hosted services.</remarks>
        public static IServiceCollection AddAllServicesFrom(this IServiceCollection services, bool triggersAsyncSetupFirst,
            params Assembly[] assemblies)
        {
            var asyncSetupWithProviderServices = assemblies.GetAll(typeof(IAsyncSetupWithProvider));
            var lifeTimeHostedServices = assemblies.GetAll(typeof(ILifetimeHostedService));
            var asyncSetupServices = assemblies.GetAll(typeof(IAsyncSetup));
            var singletonServices = assemblies.GetAll(typeof(ISingleton));
            var transientServices = assemblies.GetAll(typeof(ITransient));
            var scopedServices = assemblies.GetAll(typeof(IScoped));

            services.AddHostedService(sp =>
                    new LifeTimeService(new AsyncSetupServicesOptions
                    {
                        ExecuteAsyncSetupServicesFirst = triggersAsyncSetupFirst, 
                        ExecuteAsyncSetupWithProviderServicesFirst = !triggersAsyncSetupFirst
                    }, sp.GetRequiredService<IServiceProvider>()));

            return services
                .AddAsyncSetupWithProviderServices(asyncSetupWithProviderServices.ToArray())
                .AddLifetimeHostedServices(lifeTimeHostedServices.ToArray())
                .AddAsyncSetupServices(asyncSetupServices.ToArray())
                .AddSingletonServices(singletonServices.ToArray())
                .AddTransientServices(transientServices.ToArray())
                .AddScopedServices(scopedServices.ToArray());
        }

        internal static IServiceCollection AddAsyncSetupWithProviderServices(this IServiceCollection services,
            params Type[] serviceTypes) => services.AddDisposableServices(typeof(IAsyncSetupWithProvider),
            s => AsyncSetupWithProviderTypes.Add(s.ServiceType), serviceTypes);
        
        internal static IServiceCollection AddLifetimeHostedServices(this IServiceCollection services,
            params Type[] serviceTypes) => services.AddServices(typeof(ILifetimeHostedService), ServiceLifetime.Singleton, 
            (s, d) => LifetimeHostedServiceTypes.Add(d.ServiceType), serviceTypes);

        internal static IServiceCollection AddAsyncSetupServices(this IServiceCollection services,
            params Type[] serviceTypes) => services.AddDisposableServices(typeof(IAsyncSetup),
            s => AsyncSetupTypes.Add(s.ServiceType), serviceTypes);

        internal static IServiceCollection AddSingletonServices(this IServiceCollection services,
            params Type[] serviceTypes) => services.AddDisposableServices(typeof(ISingleton), default, serviceTypes);

        internal static IServiceCollection AddTransientServices(this IServiceCollection services,
            params Type[] serviceTypes) => services.AddServices(typeof(ITransient), ServiceLifetime.Transient, default, serviceTypes);

        internal static IServiceCollection AddScopedServices(this IServiceCollection services,
            params Type[] serviceTypes) => services.AddServices(typeof(IScoped), ServiceLifetime.Scoped, default, serviceTypes);

        private static IServiceCollection AddServices(this IServiceCollection services, Type serviceInterface, 
            ServiceLifetime lifetime, Action<Type, ServiceDescriptor> onCreated, params Type[] serviceTypes)
        {
            if (serviceTypes.Length < 1) return services;
            foreach (var service in serviceTypes)
            {
                var serviceType = service.GetInterfaces().FirstOrDefault(i => i != serviceInterface && i != typeof(IAsyncDisposable) && i != typeof(IDisposable));
                var serviceDesc = serviceType != default
                    ? new ServiceDescriptor(serviceType, service, lifetime)
                    : new ServiceDescriptor(service, service, lifetime);

                onCreated?.Invoke(service, serviceDesc);
                services.TryAdd(serviceDesc);
            }

            return services;
        }

        private static IServiceCollection AddDisposableServices(this IServiceCollection services, Type serviceInterface, Action<ServiceDescriptor> onCreated, params Type[] serviceTypes)
        {
            if (DisposableAsyncTypes == null) DisposableAsyncTypes = new List<Type>();
            if (DisposableTypes == null) DisposableTypes = new List<Type>();

            return services.AddServices(serviceInterface, ServiceLifetime.Singleton, (service, desc) =>
            {
                var isDisposableAsync = service.GetInterfaces().Contains(typeof(IAsyncDisposable));
                var isDisposable = service.GetInterfaces().Contains(typeof(IDisposable));
                if (isDisposableAsync) DisposableAsyncTypes.Add(desc.ServiceType);
                if (isDisposable) DisposableTypes.Add(desc.ServiceType);
                onCreated?.Invoke(desc);
            }, serviceTypes);
        }

        internal static IEnumerable<Type> GetAll(this IEnumerable<Assembly> assemblies, Type serviceInterface) => from assembly in assemblies
            from type in assembly.GetTypes()
            where !type.IsAbstract
                  && type.IsClass
                  && !type.IsGenericType
                  && type.GetInterfaces().Contains(serviceInterface)
            select type;

        internal static ICollection<Type> AsyncSetupWithProviderTypes { get; } = new List<Type>();
        internal static ICollection<Type> LifetimeHostedServiceTypes { get; } = new List<Type>();
        internal static ICollection<Type> AsyncSetupTypes { get; } = new List<Type>();
        internal static ICollection<Type> DisposableAsyncTypes { get; private set; }
        internal static ICollection<Type> DisposableTypes { get; private set; }
    }
}

