using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Lumiere.Application.Extensions;

public static class ValidatorsExtensions
{
    public static void AddValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(ApplicationExtensions).Assembly);
    }
}
