using Lumiere.Application.DTOs.Results;
using MediatR;

namespace Lumiere.Application.Features.Commands.Users.Login
{
    public record LoginCommand(string Email, string Password) : IRequest<ResultDto>;
}
