using Lumiere.Domain.Common;
using Lumiere.Domain.Entities;
using Lumiere.Domain.ValueObjects;
using System.Linq.Expressions;

namespace Lumiere.Domain.Interfaces;

public interface IUserRepository : IBaseRepository<User>
{
    Task CreateUserAsync(User user, string password, CancellationToken cancellationToken = default);
    Task<BasePaginationResult<UserPaginated>> GetUsers(
        int page,
        int pageAmount,
        Expression<Func<User, bool>> filterExpression,
        Expression<Func<User, object>> orderByExpression,
        Expression<Func<User, UserPaginated>> selectorExpression);
}
