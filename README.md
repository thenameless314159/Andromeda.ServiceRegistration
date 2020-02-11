# Andromeda.ServiceRegistration

<center><img src="https://raw.githubusercontent.com/thenameless314159/Andromeda.ServiceRegistration/master/andromeda_icon1_small.png?token=AFMTCCIYOXAVR7UBEKWGKJK6JQP7G" width="160" height="168"><img src="https://raw.githubusercontent.com/thenameless314159/Andromeda.ServiceRegistration/master/ASP.NET-Core-Logo_2colors_Square_RGB.png?token=AFMTCCNPNVM6MBG7AF6E75K6JQTHI" width="180" height="168"><img src="https://raw.githubusercontent.com/thenameless314159/Andromeda.ServiceRegistration/master/NET-Core-Logo_2colors_Square_RGB.png?token=AFMTCCNORD45RRHKSS456HK6JQTJU" width="180" height="168"></center>

Extension library designed to simplify the dependency registration process within the *`Microsoft.Extensions.DependencyInjection`* base APIs.


## Getting started

 You can either use the default extension method [
```csharp 
AddAllServicesFrom(this IServiceCollection services, bool triggersAsyncSetup, params Assembly[] assemblies) 
```](https://github.com/thenameless314159/Andromeda.ServiceRegistration/blob/master/src/Andromeda.ServiceRegistration.Extensions/ServiceRegistrationExtensions.cs#L35) on your *IServiceCollection* instance : 
