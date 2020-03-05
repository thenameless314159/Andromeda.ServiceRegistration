using static Andromeda.ServiceRegistration.Extensions.ServiceRegistrationExtensions;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Andromeda.ServiceRegistration.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;

// ReSharper disable InvertIf
namespace Andromeda.ServiceRegistration.Extensions
{
    public sealed class ServiceRegistrationOptionsBuilder
    {
        internal ICollection<ServiceDescriptor> _asyncSetupsToRegister = new List<ServiceDescriptor>();
        internal ServiceRegistrationOptions _registrationOptions = new ServiceRegistrationOptions();
        internal AsyncSetupServicesOptions _asyncSetupOptions = new AsyncSetupServicesOptions();
        internal ICollection<Assembly> _servicesAssemblies = new List<Assembly>();

        internal ServiceRegistrationOptionsBuilder()
        {
        }
        
        /// <summary>
        /// Configure the <see cref="ServiceRegistrationOptions"/> from a section of the <see cref="IConfiguration"/>
        /// which have the same name of the type.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        public ServiceRegistrationOptionsBuilder ConfigureRegistrationOptions(IConfiguration configuration) 
        {
            var newOptions = new ServiceRegistrationOptions();
            configuration.GetSection(nameof(ServiceRegistrationOptions))
                .Bind(newOptions);

            _registrationOptions = newOptions;
            return this;
        }

        /// <summary>
        /// Configure the <see cref="AsyncSetupServicesOptions"/> from a section of the <see cref="IConfiguration"/>
        /// which have the same name of the type.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        public ServiceRegistrationOptionsBuilder ConfigureAsyncSetupOptions(IConfiguration configuration) 
        {
            var newOptions = new AsyncSetupServicesOptions();
            configuration.GetSection(nameof(AsyncSetupServicesOptions))
                .Bind(newOptions);

            _asyncSetupOptions = newOptions;
            return this;
        }

        /// <summary>
        /// Configure the <see cref="ServiceRegistrationOptions"/> according to the provided delegate.
        /// </summary>
        /// <param name="configure">The configuration delegate.</param>
        public ServiceRegistrationOptionsBuilder ConfigureRegistrationOptions(Action<ServiceRegistrationOptions> configure) {
            configure(_registrationOptions);
            return this;
        }

        /// <summary>
        /// Add the specified assemblies to the services lookup.
        /// </summary>
        /// <param name="assemblies">The assemblies that contains the services defined with the <see cref="Abstractions"/> interfaces.</param>
        public ServiceRegistrationOptionsBuilder UseServiceAssemblies(params Assembly[] assemblies) {
            foreach(var assembly in assemblies) _servicesAssemblies.Add(assembly);
            return this;
        }

        public ServiceRegistrationOptionsBuilder ExecuteAsyncSetupWithProviderServicesFirst() {
            _asyncSetupOptions.ExecuteAsyncSetupWithProviderServicesFirst = true;
            _asyncSetupOptions.ExecuteAsyncSetupServicesFirst = false;
            return this;
        }

        public ServiceRegistrationOptionsBuilder ConfigureAsyncSetupWithProvider<T, TImpl>() 
            where T : IAsyncSetupWithProvider where TImpl : 
            class, T
        {
            _asyncSetupsToRegister.Add(new ServiceDescriptor(typeof(T), typeof(TImpl), ServiceLifetime.Singleton));
            AsyncSetupWithProviderTypes.Add(typeof(T));
            return this;
        }

        /// <summary>
        /// Add the specified assembly to the services lookup.
        /// </summary>
        /// <param name="assembly">The assembly that contains the services defined with the <see cref="Abstractions"/> interfaces.</param>
        public ServiceRegistrationOptionsBuilder UseServiceAssembly(Assembly assembly) {
            _servicesAssemblies.Add(assembly);
            return this;
        }

        public ServiceRegistrationOptionsBuilder ConfigureAsyncSetupWithProvider<T>() 
            where T : class, IAsyncSetupWithProvider
        {
            _asyncSetupsToRegister.Add(new ServiceDescriptor(typeof(T), typeof(T), ServiceLifetime.Singleton));
            AsyncSetupWithProviderTypes.Add(typeof(T));
            return this;
        }
        
        public ServiceRegistrationOptionsBuilder ConfigureAsyncSetup<T, TImpl>() 
            where T : IAsyncSetup where TImpl : class, T
        {
            _asyncSetupsToRegister.Add(new ServiceDescriptor(typeof(T), typeof(TImpl), ServiceLifetime.Singleton));
            AsyncSetupTypes.Add(typeof(T));
            return this;
        }
        
        public ServiceRegistrationOptionsBuilder ConfigureAsyncSetup<T>() 
            where T : class, IAsyncSetup
        {
            _asyncSetupsToRegister.Add(new ServiceDescriptor(typeof(T), typeof(T), ServiceLifetime.Singleton));
            AsyncSetupTypes.Add(typeof(T));
            return this;
        }

        public ServiceRegistrationOptionsBuilder ExecuteAsyncSetupServicesFirst() {
            _asyncSetupOptions.ExecuteAsyncSetupWithProviderServicesFirst = false;
            _asyncSetupOptions.ExecuteAsyncSetupServicesFirst = true;
            return this;
        }

        public ServiceRegistrationOptionsBuilder FireAndForgetAsyncSetupServices() {
            _asyncSetupOptions.FireAndForgetAsyncSetupServices = true;
            return this;
        }

        public ServiceRegistrationOptionsBuilder AwaitAsyncSetupServices() {
            _asyncSetupOptions.FireAndForgetAsyncSetupServices = false;
            return this;
        }

        public ServiceRegistrationOptionsBuilder RegisterAllServices()
        {
            _registrationOptions.RegisterAsyncSetupWithProviderServices = true;
            _registrationOptions.RegisterLifetimeHostedServices = true;
            _registrationOptions.RegisterAsyncSetupServices = true;
            _registrationOptions.RegisterSingletonServices = true;
            _registrationOptions.RegisterTransientServices = true;
            _registrationOptions.RegisterScopedServices = true;
            return this;
        }

        internal IServiceCollection RegisterAllDependencies(IServiceCollection services)
        {
            if (_registrationOptions.RegisterAsyncSetupWithProviderServices) services.AddAsyncSetupWithProviderServices(
                _servicesAssemblies.GetAll(typeof(IAsyncSetupWithProvider)).ToArray());
            if (_registrationOptions.RegisterLifetimeHostedServices) services.AddLifetimeHostedServices(
                _servicesAssemblies.GetAll(typeof(ILifetimeHostedService)).ToArray());
            if (_registrationOptions.RegisterAsyncSetupServices) services.AddAsyncSetupServices(
                _servicesAssemblies.GetAll(typeof(IAsyncSetup)).ToArray());
            if (_registrationOptions.RegisterSingletonServices) services.AddSingletonServices(
                _servicesAssemblies.GetAll(typeof(ISingleton)).ToArray());
            if (_registrationOptions.RegisterTransientServices) services.AddTransientServices(
                _servicesAssemblies.GetAll(typeof(ITransient)).ToArray());
            if (_registrationOptions.RegisterScopedServices) services.AddScopedServices(
                _servicesAssemblies.GetAll(typeof(IScoped)).ToArray());

            services.TryAdd(_asyncSetupsToRegister);
            return services.AddHostedService(sp =>
                new LifeTimeService(_asyncSetupOptions, 
                    sp.GetRequiredService<IServiceProvider>()));
        }
    }
}
