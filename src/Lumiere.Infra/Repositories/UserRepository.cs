using Lumiere.Domain.Common;
using Lumiere.Domain.Entities;
using Lumiere.Domain.Interfaces;
using Lumiere.Infra.Context;
using Microsoft.AspNetCore.Identity;

namespace Lumiere.Infra.Repositories;

public class UserRepository(AppDbContext context, UserManager<User> userManager)
    : BaseRepository<User>(context), IUserRepository
{
    public async Task<Result<List<string>>> CreateUserAsync(User user, string password, CancellationToken cancellationToken = default)
    {

        Result<List<string>> createUserResult = new();
        IdentityResult identityResult = await userManager.CreateAsync(user, password);

        if (identityResult.Succeeded)
        {
            return createUserResult;
        }

        List<string> errors = [..

            identityResult
                .Errors
                .Select(error => error.Description)

        ];

        createUserResult.SetData(errors);

        return createUserResult;

    }
}
