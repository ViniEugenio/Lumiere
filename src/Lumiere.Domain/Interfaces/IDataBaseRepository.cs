namespace Lumiere.Domain.Interfaces;

public interface IDataBaseRepository
{
    Task ApplyMigrations(CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetPendingMigration(CancellationToken cancellationToken = default);
}
