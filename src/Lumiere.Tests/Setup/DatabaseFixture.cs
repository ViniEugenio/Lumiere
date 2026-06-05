using Lumiere.Infra.Context;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace Lumiere.Tests.Setup;

public class DatabaseFixture : IAsyncLifetime
{

    private readonly MsSqlContainer _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    public AppDbContext DbContext { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(_container.GetConnectionString())
            .Options;

        DbContext = new AppDbContext(options);
        await DbContext.Database.MigrateAsync();

        await SeedAsync();
    }

    public async Task DisposeAsync()
    {
        await DbContext.DisposeAsync();
        await _container.DisposeAsync();
    }

    private async Task SeedAsync()
    {
        await DatabaseSeed.SeedAsync(DbContext);
    }
}
