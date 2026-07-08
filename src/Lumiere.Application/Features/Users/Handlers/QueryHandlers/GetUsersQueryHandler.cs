using Lumiere.Application.DTOs;
using Lumiere.Application.Features.Users.Queries;
using Lumiere.Domain.Common;
using Lumiere.Domain.Entities;
using Lumiere.Domain.Interfaces;
using MediatR;
using System.Linq.Expressions;

namespace Lumiere.Application.Features.Users.Handlers.QueryHandlers;

public class GetUsersQueryHandler(IUserRepository userRepository) : IRequestHandler<GetUsersQuery, ResultDto<BasePaginationResult<UserPaginated>>>
{
    public async Task<ResultDto<BasePaginationResult<UserPaginated>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        Expression<Func<User, bool>> filterExpression = user =>
            string.IsNullOrEmpty(request.Name) || user.FirstName.Contains(request.Name);

        Expression<Func<User, object>> orderByExpression = user => user.FirstName;

        Expression<Func<User, UserPaginated>> selectorExpression = user => new UserPaginated(
            user.FirstName,
            user.LastName,
            user.Email,
            user.Active);

        PaginationFilters<User, UserPaginated> paginationFilter = new(
            request.Page!.Value,
            request.PageAmount!.Value,
            filterExpression,
            orderByExpression,
            selectorExpression);

        BasePaginationResult<UserPaginated> usersPaginatedResult = await userRepository
            .GetAllPaginationAsync(paginationFilter, cancellationToken);

        ResultDto<BasePaginationResult<UserPaginated>> result = new();
        result.SetData(usersPaginatedResult);

        return result;
    }
}
