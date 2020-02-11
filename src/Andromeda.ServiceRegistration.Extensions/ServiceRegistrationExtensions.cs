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
        public static IServiceCollection Configure(this IServiceCollection services, Action<ServiceRegistrationOptionsBuilder> configure)
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
        /// <param name="triggersAsyncSetup">Whether <see cref="IAsyncSetupWithProvider"/> and <see cref="IAsyncSetup"/> must be triggered on application startup.</param>
        /// <param name="assemblies">The assemblies where the services from <see cref="Abstractions"/> are defined.</param>
        /// <returns>The service collection provided.</returns>
        /// <remarks>Must be called first on the ConfigureService method of your hosting logic if you need to triggers the <see cref="IAsyncSetup"/>, 
        /// else they might start after your own hosted services.</remarks>
        public static IServiceCollection AddAllServicesFrom(this IServiceCollection services, bool triggersAsyncSetup,
            params Assembly[] assemblies)
        {
            var asyncSetupWithProviderServices = from assembly in assemblies
                                                 from type in assembly.GetTypes()
                                                 where !type.IsAbstract
                                                       && type.IsClass
                                                       && type.GetInterfaces().Contains(typeof(IAsyncSetupWithProvider))
                                                 select type;

            var asyncSetupServices = from assembly in assemblies
                                     from type in assembly.GetTypes()
                                     where !type.IsAbstract
                                           && type.IsClass
                                           && type.GetInterfaces().Contains(typeof(IAsyncSetup))
                                     select type;

            var singletonServices = from assembly in assemblies
                                    from type in assembly.GetTypes()
                                    where !type.IsAbstract
                                          && type.IsClass
                                          && type.GetInterfaces().Contains(typeof(ISingleton))
                                    select type;

            var transientServices = from assembly in assemblies
                                    from type in assembly.GetTypes()
                                    where !type.IsAbstract
                                          && type.IsClass
                                          && type.GetInterfaces().Contains(typeof(ITransient))
                                    select type;

            var scopedServices = from assembly in assemblies
                                 from type in assembly.GetTypes()
                                 where !type.IsAbstract
                                       && type.IsClass
                                       && type.GetInterfaces().Contains(typeof(IScoped))
                                 select type;

            services.AddHostedService(sp =>
                    new LifeTimeService(new AsyncSetupServicesOptions {TriggersAsyncSetupServices = triggersAsyncSetup},
                        sp.GetRequiredService<IServiceProvider>()));

            return services
                .AddAsyncSetupWithProviderServices(asyncSetupWithProviderServices.ToArray())
                .AddAsyncSetupServices(asyncSetupServices.ToArray())
                .AddSingletonServices(singletonServices.ToArray())
                .AddTransientServices(transientServices.ToArray())
                .AddScopedServices(scopedServices.ToArray());
        }

        internal static IServiceCollection AddAsyncSetupWithProviderServices(this IServiceCollection services,
            params Type[] serviceTypes)
        {
            if (serviceTypes.Length < 1) return services;
            if (DisposableAsyncTypes == null) DisposableAsyncTypes = new List<Type>();
            if (DisposableTypes == null) DisposableTypes = new List<Type>();

            foreach (var service in serviceTypes)
            {
                var serviceType = service.GetInterfaces().FirstOrDefault(i => i != typeof(IAsyncSetupWithProvider) && i != typeof(IAsyncDisposable) && i != typeof(IDisposable));
                var isDisposableAsync = service.GetInterfaces().Contains(typeof(IAsyncDisposable));
                var isDisposable = service.GetInterfaces().Contains(typeof(IDisposable));

                services.TryAdd(serviceType != default
                    ? new ServiceDescriptor(serviceType, service, ServiceLifetime.Singleton)
                    : new ServiceDescriptor(service, service, ServiceLifetime.Singleton));

                var toAdd = serviceType != default ? serviceType : service;
                AsyncSetupWithProviderTypes.Add(toAdd);
                if(isDisposable) DisposableTypes.Add(toAdd);
                if(isDisposableAsync) DisposableAsyncTypes.Add(toAdd);
            }
            return services;
        }

        internal static IServiceCollection AddAsyncSetupServices(this IServiceCollection services,
            params Type[] serviceTypes)
        {
            if (serviceTypes.Length < 1) return services;
            if (DisposableAsyncTypes == null) DisposableAsyncTypes = new List<Type>();
            if (DisposableTypes == null) DisposableTypes = new List<Type>();
            
            foreach (var service in serviceTypes)
            {
                var serviceType = service.GetInterfaces().FirstOrDefault(i => i != typeof(IAsyncSetup) && i != typeof(IAsyncDisposable) && i != typeof(IDisposable));
                var isDisposableAsync = service.GetInterfaces().Contains(typeof(IAsyncDisposable));
                var isDisposable = service.GetInterfaces().Contains(typeof(IDisposable));

                services.TryAdd(serviceType != default
                    ? new ServiceDescriptor(serviceType, service, ServiceLifetime.Singleton)
                    : new ServiceDescriptor(service, service, ServiceLifetime.Singleton));

                var toAdd = serviceType != default ? serviceType : service;
                AsyncSetupTypes.Add(toAdd);
                if (isDisposable) DisposableTypes.Add(toAdd);
                if (isDisposableAsync) DisposableAsyncTypes.Add(toAdd);
            }
            return services;
        }

        internal static IServiceCollection AddSingletonServices(this IServiceCollection services,
            params Type[] serviceTypes)
        {
            if (serviceTypes.Length < 1) return services;
            if (DisposableAsyncTypes == null) DisposableAsyncTypes = new List<Type>();
            if (DisposableTypes == null) DisposableTypes = new List<Type>();
            
            foreach (var service in serviceTypes)
            {
                var serviceType = service.GetInterfaces().FirstOrDefault(i => i != typeof(ISingleton) && i != typeof(IAsyncDisposable) && i != typeof(IDisposable));
                var isDisposableAsync = service.GetInterfaces().Contains(typeof(IAsyncDisposable));
                var isDisposable = service.GetInterfaces().Contains(typeof(IDisposable));

                services.TryAdd(serviceType != default
                    ? new ServiceDescriptor(serviceType, service, ServiceLifetime.Singleton)
                    : new ServiceDescriptor(service, service, ServiceLifetime.Singleton));

                var toAdd = serviceType != default ? serviceType : service;
                if (isDisposable) DisposableTypes.Add(toAdd);
                if (isDisposableAsync) DisposableAsyncTypes.Add(toAdd);
            }
            return services;
        }

        internal static IServiceCollection AddTransientServices(this IServiceCollection services,
            params Type[] serviceTypes)
        {
            if (serviceTypes.Length < 1) return services;
            foreach (var service in serviceTypes)
            {
                var serviceType = service.GetInterfaces().FirstOrDefault(i => i != typeof(ITransient));
                services.TryAdd(serviceType != default
                    ? new ServiceDescriptor(serviceType, service, ServiceLifetime.Transient)
                    : new ServiceDescriptor(service, service, ServiceLifetime.Transient));
            }
            return services;
        }

        internal static IServiceCollection AddScopedServices(this IServiceCollection services,
            params Type[] serviceTypes)
        {
            if (serviceTypes.Length < 1) return services;
            foreach (var service in serviceTypes)
            {
                var serviceType = service.GetInterfaces().FirstOrDefault(i => i != typeof(IScoped));
                services.TryAdd(serviceType != default
                    ? new ServiceDescriptor(serviceType, service, ServiceLifetime.Scoped)
                    : new ServiceDescriptor(service, service, ServiceLifetime.Scoped));
            }
            return services;
        }

        internal static ICollection<Type> AsyncSetupWithProviderTypes { get; } = new List<Type>();
        internal static ICollection<Type> AsyncSetupTypes { get; } = new List<Type>();
        internal static ICollection<Type> DisposableAsyncTypes { get; private set; }
        internal static ICollection<Type> DisposableTypes { get; private set; }
    }
}

