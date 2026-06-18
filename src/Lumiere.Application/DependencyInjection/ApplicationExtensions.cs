using Microsoft.Extensions.DependencyInjection;

namespace Lumiere.Application.DependencyInjection;

public static class ApplicationExtensions
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddMediatr();
        services.AddValidators();
        services.AddServices();
    }
}
