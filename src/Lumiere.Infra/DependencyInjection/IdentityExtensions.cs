using Lumiere.Domain.Entities;
using Lumiere.Infra.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Lumiere.Infra.DependencyInjection
{
    public static class IdentityExtensions
    {

        public static void AddIdentity(this IServiceCollection services)
        {

            services
                .AddIdentityCore<User>()
                .AddRoles<IdentityRole<int>>()
                .AddEntityFrameworkStores<AppDbContext>();

        }

    }
}
