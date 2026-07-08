using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Lumiere.Application.DependencyInjection;

public static class ValidatorsExtensions
{
    public static void AddValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(ApplicationExtensions).Assembly);
    }
}
