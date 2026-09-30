using Lumiere.Application.Interfaces.Repositories;
using Lumiere.Infra.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Lumiere.Infra.Extensions;

public static class RepositoriesExtensions
{
    public static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IChannelRepository, ChannelRepository>();
        services.AddScoped<IDataBaseRepository, DataBaseRepository>();
    }
}
