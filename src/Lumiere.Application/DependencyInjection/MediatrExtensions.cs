using Lumiere.Application.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace Lumiere.Application.DependencyInjection;

public static class MediatrExtensions
{
    public static void AddMediatr(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(ApplicationExtensions).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
    }
}
