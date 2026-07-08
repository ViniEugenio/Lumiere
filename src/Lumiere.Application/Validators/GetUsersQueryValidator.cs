using FluentValidation;
using Lumiere.Application.DTOs;
using Lumiere.Application.Features.Users.Queries;

namespace Lumiere.Application.Validators;

public class GetUsersQueryValidator : AbstractValidator<GetUsersQuery>
{
    public GetUsersQueryValidator()
    {
        Include(new BasePaginationValidator<UserPaginated>());
    }
}
