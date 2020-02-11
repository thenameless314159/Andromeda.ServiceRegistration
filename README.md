# [![Build Status](https://travis-ci.com/thenameless314159/Astron.svg?branch=master)](https://travis-ci.com/thenameless314159/Astron)- Andromeda.ServiceRegistration -[![NuGet Badge](https://buildstats.info/nuget/astron)](https://www.nuget.org/packages/Astron/) 

<div style="text-align:center"><p align="center"><img src="https://raw.githubusercontent.com/thenameless314159/Andromeda.ServiceRegistration/master/andromeda_icon1_small.png?token=AFMTCCIYOXAVR7UBEKWGKJK6JQP7G" width="160" height="168"><img src="https://raw.githubusercontent.com/thenameless314159/Andromeda.ServiceRegistration/master/ASP.NET-Core-Logo_2colors_Square_RGB.png?token=AFMTCCNPNVM6MBG7AF6E75K6JQTHI" width="180" height="168"><img src="https://raw.githubusercontent.com/thenameless314159/Andromeda.ServiceRegistration/master/NET-Core-Logo_2colors_Square_RGB.png?token=AFMTCCNORD45RRHKSS456HK6JQTJU" width="180" height="168"></p></div>

Extension library designed to simplify the dependency registration process within the *`Microsoft.Extensions.DependencyInjection`* base APIs.


## Getting started

 There are 2 extension methods as an entry point for this library, the default one is [`AddAllServicesFrom`](https://github.com/thenameless314159/Andromeda.ServiceRegistration/blob/master/src/Andromeda.ServiceRegistration.Extensions/ServiceRegistrationExtensions.cs#L35) which can be used on your *IServiceCollection* instance : 

```csharp
public void ConfigureServices(IServiceCollection services)
{
	services.AddAllServicesFrom(triggersAsyncSetup: true, Assembly.GetEntryAssembly());
}

```

This method will register every class that implements one of the [**5 services interfaces**](https://github.com/thenameless314159/Andromeda.ServiceRegistration/tree/master/src/Andromeda.ServiceRegistration.Abstractions)
 into the *IServiceCollection*. These interfaces can be implementend by a concrete type **directly** (they'll be registered with their concrete type into the DI container) or **by an interface** that is then implemented by the concrete type (which will register them by their own interface type into the DI container).

If any [*IAsyncSetupWithProvider*](https://github.com/thenameless314159/Andromeda.ServiceRegistration/blob/master/src/Andromeda.ServiceRegistration.Abstractions/IAsyncSetup.cs), [*IAsyncSetup*](https://github.com/thenameless314159/Andromeda.ServiceRegistration/blob/master/src/Andromeda.ServiceRegistration.Abstractions/IAsyncSetup.cs) or [*ISingleton*](https://github.com/thenameless314159/Andromeda.ServiceRegistration/blob/master/src/Andromeda.ServiceRegistration.Abstractions/ISingleton.cs) also implement either the *IAsyncDisposable* or *IDisposable* interface, the registered service that we'll talk about next will also handle their disposal.
 
Also, a *IHostedService* responsible of setting up the [*IAsyncSetupWithProvider*](https://github.com/thenameless314159/Andromeda.ServiceRegistration/blob/master/src/Andromeda.ServiceRegistration.Abstractions/IAsyncSetup.cs) and [*IAsyncSetup*](https://github.com/thenameless314159/Andromeda.ServiceRegistration/blob/master/src/Andromeda.ServiceRegistration.Abstractions/IAsyncSetup.cs) services is automatically registered, therefore if you need thoses registered services setting up before that your own *IHostedService* starts you'll have to call this method before your own service registration. You can disable this feature by setting the '*triggersAsyncSetup*' arg to false. 
