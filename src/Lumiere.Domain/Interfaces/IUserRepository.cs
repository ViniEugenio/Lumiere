using Lumiere.Domain.Entities;

namespace Lumiere.Domain.Interfaces;

public interface IUserRepository : IBaseRepository<User>
{
    Task CreateUserAsync(User user, string password, CancellationToken cancellationToken = default);
}
