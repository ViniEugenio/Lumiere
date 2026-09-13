using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lumiere.Infra.Extensions;

public static class InfrastructureExtensions
{
    public static void AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddContext(configuration);
        services.AddRepositories();
        services.AddSecurity();
    }
}
