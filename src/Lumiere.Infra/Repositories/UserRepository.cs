using Lumiere.Domain.Common;
using Lumiere.Domain.Entities;
using Lumiere.Domain.Interfaces;
using Lumiere.Domain.ValueObjects;
using Lumiere.Infra.Context;
using System.Linq.Expressions;
using System.Security.Cryptography;

namespace Lumiere.Infra.Repositories;

public class UserRepository(AppDbContext context)
    : BaseRepository<User>(context), IUserRepository
{

    public async Task CreateUserAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        user.SetPassword(HashPassword(password));

        await AddAsync(user, cancellationToken);
    }

    private static string HashPassword(string password)
    {

        int saltSize = 16;
        int hashSize = 32;
        int iterations = 100_000;

        byte[] salt = RandomNumberGenerator.GetBytes(saltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, hashSize);

        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";

    }

    public async Task<BasePaginationResult<UserPaginated>> GetUsers(
        int page,
        int pageAmount,
        Expression<Func<User, bool>> filterExpression,
        Expression<Func<User, object>> orderByExpression,
        Expression<Func<User, UserPaginated>> selectorExpression)
    {
        return await GetAllPaginationAsync(page, pageAmount, filterExpression, orderByExpression, selectorExpression);
    }
}
