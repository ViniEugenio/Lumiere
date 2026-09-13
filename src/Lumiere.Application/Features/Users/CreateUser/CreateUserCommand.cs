using Lumiere.Application.DTOs.Results;
using MediatR;

namespace Lumiere.Application.Features.Users.CreateUser
{
    public record CreateUserCommand(
        string FirstName,
        string LastName,
        string Email,
        string Password,
        string ConfirmPassword
    ) : IRequest<ResultDto>;
}
