using Lumiere.Application.DTOs;
using Lumiere.Application.Features.Users.Commands;
using Lumiere.Application.Resources;
using Lumiere.Application.Services.Interfaces;
using Lumiere.Domain.Entities;
using Lumiere.Domain.Interfaces;

namespace Lumiere.Application.Services.Implementations;

public class UserService(IUserRepository userRepository) : IUserService
{
    public async Task<ResultDto<object>> CreateUserAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await ValidateCreateUserAsync(command, cancellationToken);

        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        var result = new ResultDto<object>();
        var user = User.Create(command.Username, command.Email);

        var identityResult = await userRepository.CreateUserAsync(user, command.Password, cancellationToken);

        if (!identityResult.Succeeded)
        {
            result.AddErrors(identityResult.Errors.Select(identityError => identityError.Description));
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
