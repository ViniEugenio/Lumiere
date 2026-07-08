namespace Lumiere.API.Extensions;

public static class APIExtensions
{
    public static void AddAPIExtensions(this IServiceCollection services)
    {
        services.AddControllers();
    }

    public static void AddAPIApplications(this WebApplication app)
    {
        app.UseHttpsRedirection();
        app.MapControllers();
    }
}
