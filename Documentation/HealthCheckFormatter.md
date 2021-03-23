# HealthCheck formatter

On ![Readme](./../README.md) page it is described how health check formatter end-result looks like.

## Usage

There are some things to be done when configuring HealthCheck in `Startup.cs` method `Configure`.
```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // ... here first goes anything should go in this method
    
    // You would want to set appropriate return codes for health check failures
    var healthCheckOptions = new HealthCheckOptions
    {
        ResultStatusCodes = new Dictionary<HealthStatus, int>
        {
            { HealthStatus.Healthy, StatusCodes.Status200OK },
            { HealthStatus.Degraded, StatusCodes.Status503ServiceUnavailable },
            { HealthStatus.Unhealthy, StatusCodes.Status503ServiceUnavailable },
        },
        // Here goes provided Formatter
        ResponseWriter = async (context, report) => await HealthCheckFormatter.JsonResponseWriter(context, report, isDevelopment).ConfigureAwait(false),
    };
    
    app.UseHealthChecks("/api/health", healthCheckOptions);
}
```

As returned Json is specific and differs from Standard Microsoft one-word "Healthy" (or "Unhealthy"), if you have monitoring system, adjust expected end-result from this formatter.