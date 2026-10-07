using Lumiere.Application.Interfaces.Services;
using Lumiere.Infra.Security;
using Microsoft.Extensions.DependencyInjection;

namespace Lumiere.Infra.Extensions;

public static class SecurityExtensions
{
    public static void AddSecurity(this IServiceCollection services)
    {
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IJWTService, JWTService>();
    }
}
