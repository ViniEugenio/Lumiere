using Lumiere.Domain.Interfaces;
using Lumiere.Infra.Security;
using Microsoft.Extensions.DependencyInjection;

namespace Lumiere.Infra.DependencyInjection;

public static class SecurityExtensions
{
    public static void AddSecurity(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher, PasswordHasher>();
    }
}
