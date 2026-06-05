using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Lumiere.Infra.Context;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var basePath = ResolveApiProjectPath();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("Default") ?? string.Empty;

        var sqlBuilder = new SqlConnectionStringBuilder(connectionString)
        {
            TrustServerCertificate = true
        };

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(sqlBuilder.ConnectionString);

        return new AppDbContext(optionsBuilder.Options);
    }

    private static string ResolveApiProjectPath()
    {
        var current = Directory.GetCurrentDirectory();

        var candidates = new[]
        {
            current,
            Path.Combine(current, "../Lumiere.API"),
            Path.Combine(current, "src", "Lumiere.API"),
        };

        return candidates.FirstOrDefault(p => File.Exists(Path.Combine(p, "appsettings.json")))
            ?? current;
    }
}
