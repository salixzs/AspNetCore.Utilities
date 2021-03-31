# [ConfigurationValidation](https://www.nuget.org/packages/ConfigurationValidation/) handlers

Extends configuration class validations, provided by package [ConfigurationValidation](https://www.nuget.org/packages/ConfigurationValidation/) ([Repo](https://github.com/salixzs/ConfigurationValidation)) for AspNet Core application.

## Configuration class registration extensions

Configuration classes are usually normal POCO classes, which are loaded by [IHostBuilder](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-5.0) at very start of application by binding entire configuration or separate section to this strongly typed POCO object.

[ConfigurationValidation](https://www.nuget.org/packages/ConfigurationValidation/) package exposes `IValidatableConfiguration` interface for such classes to implement, which requires to add specific `Validate()` method to them. Sample of such class can be seen in [ConfigurationValidation repo readme](https://github.com/salixzs/ConfigurationValidation/blob/main/README.md).

This package provides two extension methods to register such classes as both singletons (for injection to other classes) and IValidatableConfiguration instance for automated validation purpose. Extension should be called from Startup.cs method ConfigureServices.

```csharp
// IValidatableConfiguration classes
services.ConfigureValidatableSetting<SampleConfig>(_configuration.GetSection("SampleSection"));
// or with non-validatable POCOs
services.ConfigureSetting<SimpleConfig>(_configuration.GetSection("OtherSection"));
```

## Auto-validation by [IStartupFilter](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/startup?view=aspnetcore-5.0#extend-startup-with-startup-filters)

Package contains startup filter to run validation method on all registered IValidatableConfiguration class instances. In case any of them returns validation error - filter throws `ConfigurationValidationException` with all failed validations included in its `ValidationData` property.
This approach results in asp.net application startup failure and normally can be investigated through some monitoring tooling and environment logs, which is not very handy, especially in cloud environments. Still - such possibility exists and can be turned on with adding this line in Startup.cs method ConfigureServices (somewhere in the beginning).

```csharp
services.AddConfigurationValidation();
```

## Auto-validation by Error page

This approach will not prevent application from starting up, but will prevent its normal functionality. In reality (if configuration is incorrect) every URL which should be served by your application will return this yellow screen of death:

![Config error page](./config-error.JPG)

*(Page will not be shown if you also enabled IStartupFilter - it will take precedence and fail application in its startup.)*

You can enable this page by adding this line at the beginning (before UseMvc() or MapControllers()) of Startup.cs Configure method.

```csharp
app.UseMiddleware<ConfigurationValidationMiddleware>();
```

This middleware component will kick in for every request, validates all configuration objects and intercepts response with this page contents, preventing application to further process request.

In case you want to perform this validation and show error page only on certain request paths, there is a filtering option available for setup as described in [this DevTrends article](https://www.devtrends.co.uk/blog/conditional-middleware-based-on-request-in-asp.net-core).

## Auto-validation by [HealthCheck](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-5.0)

Package provides HealthCheck provider to plug-in into your health checking routines for application.
You can add this HealthCheck provider with HealthCheck configuration in Startup.cs ConfigureServices method:

```csharp
services.AddHealthChecks()
    .Add(new HealthCheckRegistration(
        "Configuration",
        sp => new ConfigurationHealthCheck(sp.GetServices<IValidatableConfiguration>(), isDevelopment),
        HealthStatus.Unhealthy,
        null));
```
Here `isDevelopment` is boolean variable, which normally takes `IWebHostEnvironment.IsDevelopment()` value to distinguish between development and production environments. Setting it to true will show attempted configuration value, which failed.

When using HealthCheck approach - do not forget to check it after deploying changes to your application as it will be the only place where configuration validations will be shown (if application will run with invalid configuration at all).
