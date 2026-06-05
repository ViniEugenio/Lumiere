using Lumiere.Application.DTOs;
using Lumiere.Application.Features.Users.Commands;
using Lumiere.Application.Services.Interfaces;
using MediatR;

namespace Lumiere.Application.Features.Users.Handlers;

public class CreateUserCommandHandler(IUserService userService)
    : IRequestHandler<CreateUserCommand, ResultDto<object>>
{
    public async Task<ResultDto<object>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        return await userService.CreateUserAsync(request, cancellationToken);
    }
}
