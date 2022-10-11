using BusinessLogic;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Salix.AspNetCore.Utilities;
using Salix.AspNetCore.Utilities.ExceptionHandling;
using SampleApi.ErrorHandler;
using SampleApi.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<ISampleLogic, SampleLogic>();
builder.Services.AddTransient<IWeatherLogic, WeatherLogic>();
builder.Services.AddHealthChecks()
    .Add(new HealthCheckRegistration("WeatherApi", sp => new WeatherApiHealthCheck(true), HealthStatus.Unhealthy, null, TimeSpan.FromSeconds(5)));
builder.Services.AddTransient<IConfigurationValuesLoader, ConfigurationValuesLoader>();

var app = builder.Build();

app.UseMiddleware<ApiJsonErrorMiddleware>(new ApiJsonExceptionOptions
{
    ShowStackTrace = true,
    OmitSources = new HashSet<string> { "middleware" },
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
