using Lumiere.Application.DTOs;
using Lumiere.Application.Features.Users.Commands;
using Lumiere.Application.Resources;
using Lumiere.Domain.Common;
using Lumiere.Domain.Entities;
using Lumiere.Domain.Interfaces;
using MediatR;

namespace Lumiere.Application.Features.Users.Handlers;

public class CreateUserCommandHandler(IUserRepository userRepository)
    : IRequestHandler<CreateUserCommand, ResultDto<object>>
{
    public async Task<ResultDto<object>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await ValidateCreateUserAsync(request, cancellationToken);

        if (!validationResult.Succeeded)
        {
            return validationResult;
        }

        var result = new ResultDto<object>();
        var user = User.Create(request.Username, request.Email);

        Result<List<string>> createUserResult = await userRepository.CreateUserAsync(user, request.Password, cancellationToken);

        if (!createUserResult.Succeeded)
        {
            result.AddErrors(createUserResult.Errors);
            return result;
        }

        return result;
    }

    private async Task<ResultDto<object>> ValidateCreateUserAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        var result = new ResultDto<object>();

        var usernameExists = await userRepository.ExistsAsync(user => user.UserName == command.Username);

        if (usernameExists)
        {
            result.AddError(Errors.UsernameAlreadyInUse);
        }

        var emailExists = await userRepository.ExistsAsync(user => user.Email == command.Email);

        if (emailExists)
        {
            result.AddError(Errors.EmailAlreadyInUse);
        }

        return result;
    }
}
