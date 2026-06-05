using Lumiere.Application.DTOs;
using Lumiere.Application.Features.Users.Commands;

namespace Lumiere.Application.Services.Interfaces;

public interface IUserService
{
    Task<ResultDto<object>> CreateUserAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken = default);
}
