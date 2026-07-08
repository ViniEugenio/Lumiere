using Lumiere.Domain.Interfaces;
using Lumiere.Infra.Context;
using Microsoft.EntityFrameworkCore;

namespace Lumiere.Infra.Repositories;

public class DataBaseRepository(AppDbContext context) : IDataBaseRepository
{
    public async Task ApplyMigrations(CancellationToken cancellationToken = default)
    {
        await context.Database.MigrateAsync(cancellationToken);
    }

    public async Task<IEnumerable<string>> GetPendingMigration(CancellationToken cancellationToken = default)
    {
        return await context.Database.GetPendingMigrationsAsync(cancellationToken);
    }
}
