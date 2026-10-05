using FluentValidation;
using Lumiere.Application.Features.Commands.Users.Login;
using Lumiere.Application.Interfaces.Repositories;
using Lumiere.Application.Interfaces.Services;
using Lumiere.Application.Resources;
using Microsoft.EntityFrameworkCore;

namespace Lumiere.Application.Validators
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {

        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;

        public LoginCommandValidator(IUserRepository userRepository, IPasswordService passwordService)
        {

            _userRepository = userRepository;
            _passwordService = passwordService;

            RuleFor(command => command.Email)
                .NotEmpty()
                .EmailAddress()
                .WithMessage(Errors.EmailInvalid);

            RuleFor(command => command.Password)
                .NotEmpty()
                .WithMessage(Errors.PasswordRequired);

            RuleFor(command => command)
                .MustAsync(ValidateLogin)
                .WithMessage(Errors.LoginInvalid);

        }

        private async Task<bool> ValidateLogin(LoginCommand command, CancellationToken cancellationToken)
        {

            var passwordHash = await _userRepository
                .Get(user => user.Email == command.Email)
                .Select(user => user.PasswordHash)
                .FirstOrDefaultAsync(cancellationToken);

            if(passwordHash == null)
            {
                return false;
            }

            return _passwordService
                .Verify(passwordHash, command.Password);

        }

    }
}
