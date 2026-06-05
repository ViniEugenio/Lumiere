using Lumiere.API.Endpoints;

namespace Lumiere.API.Extensions;

public static class EndpointRegistrationExtension
{
    public static void AddEndpoints(this WebApplication app)
    {
        var apiRoutes = app.MapGroup("api/");
        UserEndpoints.MapUserEndpoints(apiRoutes);
    }
}
