using Lumiere.Infra.EnvConfigurationModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lumiere.Infra.Extensions
{
    public static class EnvConfigurationExtensions
    {

        public static void AddEnvConfigurations(this IServiceCollection services, IConfiguration configuration)
        {

            services
                .Configure<SecurityModel>(configuration.GetSection("Security"));

        }

    }
}
