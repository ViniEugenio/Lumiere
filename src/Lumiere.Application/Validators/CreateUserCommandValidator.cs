using FluentValidation;
using Lumiere.Application.Features.Commands.Users.CreateUser;
using Lumiere.Application.Interfaces.Repositories;
using Lumiere.Application.Resources;

namespace Lumiere.Application.Validators
{
    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {

        private readonly IUserRepository _userRepository;

        public CreateUserCommandValidator(IUserRepository userRepository)
        {

            _userRepository = userRepository;

            RuleFor(command => command.FirstName)
                .NotEmpty()
                .WithMessage(Errors.FirstNameRequired);

            RuleFor(command => command.LastName)
                .NotEmpty()
                .WithMessage(Errors.LastNameRequired);

            RuleFor(command => command.Email)
                .NotEmpty()
                .EmailAddress()
                .WithMessage(Errors.EmailInvalid)
                .MustAsync(ValidateEmailInUse)
                .WithMessage(Errors.EmailAlreadyInUse);

            RuleFor(command => command.Password)
                .NotEmpty()
                .MinimumLength(6)
                .Matches(@"[A-Z]")
                .WithMessage(Errors.PasswordUppercase)
                .Matches(@"[a-z]")
                .WithMessage(Errors.PasswordLowercase)
                .Matches(@"[0-9]")
                .WithMessage(Errors.PasswordLeastOneDigit)
                .Matches(@"[^a-zA-Z0-9]")
                .WithMessage(Errors.PasswordNonAlphanumeric);

            RuleFor(command => command.ConfirmPassword)
                .NotEmpty()
                .Equal(command => command.Password).WithMessage(Errors.ConfirmPassword);

        }

        private async Task<bool> ValidateEmailInUse(string email, CancellationToken cancellationToken)
        {

            return !await _userRepository
                .Exists(cancellationToken, user =>
                
                    user.Email == email

                );

        }

    }
}
