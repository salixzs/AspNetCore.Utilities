using Microsoft.Extensions.DependencyInjection;
using Salix.AspNetCore.Utilities;
using Sample.AspNet5.Logic;

namespace Sample.AspNet5.Api
{
    public static class DependenciesSetup
    {
        /// <summary>
        /// Registers logic and other dependencies with ASP.Net IoC container (services).
        /// </summary>
        /// <param name="services">ASP.Net built in IoC container.</param>
        public static void RegisterLogicDependencies(this IServiceCollection services)
        {
            services.AddScoped<ISampleLogic, SampleLogic>();
            services.AddTransient<IConfigurationValuesLoader, ConfigurationValuesLoader>();
        }
    }
}
