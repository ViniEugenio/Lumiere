using Lumiere.Application.Services.Implementations;
using Lumiere.Application.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Lumiere.Application.DependencyInjection
{
    public static class ServicesExtensions
    {

        public static void AddServices(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IDataBaseService, DataBaseService>();
        }

    }
}
