using Microsoft.Extensions.DependencyInjection;
using Sample.AspNet5.Logic;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading.Tasks;

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
        }
    }
}
