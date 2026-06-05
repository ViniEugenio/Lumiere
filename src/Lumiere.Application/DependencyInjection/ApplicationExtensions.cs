using FluentValidation;
using Lumiere.Application.Services.Implementations;
using Lumiere.Application.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Lumiere.Application.DependencyInjection;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ApplicationExtensions).Assembly));

        services.AddValidatorsFromAssembly(typeof(ApplicationExtensions).Assembly);

        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
