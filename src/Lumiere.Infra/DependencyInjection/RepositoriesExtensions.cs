using Lumiere.Domain.Interfaces;
using Lumiere.Infra.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Lumiere.Infra.DependencyInjection;

public static class RepositoriesExtensions
{
    public static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IChannelRepository, ChannelRepository>();
        services.AddScoped<IDataBaseRepository, DataBaseRepository>();
    }
}
