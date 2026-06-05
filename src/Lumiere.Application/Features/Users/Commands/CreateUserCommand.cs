using Lumiere.Application.DTOs;
using MediatR;

namespace Lumiere.Application.Features.Users.Commands;

public class CreateUserCommand : IRequest<ResultDto<object>>
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}
