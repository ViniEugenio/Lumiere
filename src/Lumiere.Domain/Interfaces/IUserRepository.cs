using Lumiere.Domain.Common;
using Lumiere.Domain.Entities;

namespace Lumiere.Domain.Interfaces;

public interface IUserRepository : IBaseRepository<User>
{
    Task<Result<List<string>>> CreateUserAsync(User user, string password, CancellationToken cancellationToken = default);
}
