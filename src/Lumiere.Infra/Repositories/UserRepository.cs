using Lumiere.Domain.Entities;
using Lumiere.Domain.Interfaces;
using Lumiere.Infra.Context;
using Microsoft.AspNetCore.Identity;

namespace Lumiere.Infra.Repositories;

public class UserRepository(AppDbContext context, UserManager<User> userManager)
    : BaseRepository<User>(context), IUserRepository
{
    public async Task<IdentityResult> CreateUserAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        return await userManager.CreateAsync(user, password);
    }
}
