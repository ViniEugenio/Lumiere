using Lumiere.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Lumiere.Domain.Interfaces;

public interface IUserRepository : IBaseRepository<User>
{
    Task<IdentityResult> CreateUserAsync(User user, string password, CancellationToken cancellationToken = default);
}
