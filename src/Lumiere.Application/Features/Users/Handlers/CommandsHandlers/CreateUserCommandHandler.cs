using Lumiere.Application.DTOs;
using Lumiere.Application.Features.Users.Commands;
using Lumiere.Application.Resources;
using Lumiere.Domain.Entities;
using Lumiere.Domain.Interfaces;
using MediatR;

namespace Lumiere.Application.Features.Users.Handlers.CommandsHandlers;

public class CreateUserCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    : IRequestHandler<CreateUserCommand, ResultDto<object>>
{
    public async Task<ResultDto<object>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await ValidateCreateUserAsync(request, cancellationToken);

        if (!validationResult.Succeeded)
        {
            return validationResult;
        }

        var user = User.Create(request.FirstName, request.LastName, request.Email);
        user.SetPassword(passwordHasher.Hash(request.Password));

        await userRepository.AddAsync(user, cancellationToken);

        return new ResultDto<object>();
    }

    private async Task<ResultDto<object>> ValidateCreateUserAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        var result = new ResultDto<object>();

        var emailExists = await userRepository
            .ExistsAsync(cancellationToken, user => user.Email == command.Email);

        if (emailExists)
        {
            result.AddError(Errors.EmailAlreadyInUse);
        }

        return result;
    }
}
