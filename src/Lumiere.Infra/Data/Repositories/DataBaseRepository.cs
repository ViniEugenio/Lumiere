using Lumiere.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Lumiere.Infra.Data.Repositories;

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
