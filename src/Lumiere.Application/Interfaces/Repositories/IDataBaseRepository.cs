namespace Lumiere.Application.Interfaces.Repositories;

public interface IDataBaseRepository
{
    Task ApplyMigrations(CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetPendingMigration(CancellationToken cancellationToken = default);
}
