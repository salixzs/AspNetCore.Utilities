using System.Reflection;
using Salix.AspNetCore.Utilities;
using Sample.Net6.MinimalApi;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.UseHttpsRedirection();


// <---- Here starts Home page composing
app.MapGet("/", (IConfiguration config) =>
{
    var configurationItems = new ConfigurationValuesLoader(config).GetConfigurationValues();
    var apiAssembly = Assembly.GetAssembly(typeof(Program));
    var indexPage = new IndexPage("Minimal API")
            .SetDescription("Demonstrating capabilities of Salix.AspNetCore.Utilities NuGet package with Minimal API approach.")
            .SetHostingEnvironment(app.Environment.EnvironmentName)
            .SetVersionFromAssembly(apiAssembly, 2) // Takes version from assembly - just first two numbers as specified
            .SetBuildTimeFromAssembly(apiAssembly)  // For this to work need non-deterministic AssemblyInfo.cs version set.
            .AddLinkButton("The service", "/api/weatherforecast")
            .AddLinkButton("The error", "/api/error")
            .SetConfigurationValues(configurationItems);
#if DEBUG
    indexPage.SetBuildMode("#DEBUG (Should not be in production!)");
#else
    indexPage.SetBuildMode("Release");
#endif
    return Results.Content(indexPage.GetContents(), "text/html");
});
// ----- > Here Home page composing/exposing ends


app.MapGet("/api/error", () => {
    throw new Exception("Just to test error handler.");
});


/// <summary>
/// The one and mighty Service we are serving in this minimalism.
/// </summary>
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};
app.MapGet("/api/weatherforecast", () => Enumerable.Range(1, 5).Select(index =>
       new WeatherForecast
       (
           DateTime.Now.AddDays(index),
           Random.Shared.Next(-20, 55),
           summaries[Random.Shared.Next(summaries.Length)]
       )).ToArray());

// Adding a quick API Error handler implementation.
app.UseMiddleware<ApiJsonErrorMiddleware>();

app.Run();

/// <summary>
/// Data Contract used in one and only service.
/// </summary>
internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(this.TemperatureC / 0.5556);
}
