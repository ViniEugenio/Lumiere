using FluentValidation;
using Lumiere.Application.Features.Users.CreateUser;
using Lumiere.Application.Resources;

namespace Lumiere.Application.Validators
{
    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {

        public CreateUserCommandValidator()
        {

            RuleFor(command => command.FirstName)
                .NotEmpty()
                .WithMessage(Errors.FirstNameRequired);

            RuleFor(command => command.LastName)
                .NotEmpty()
                .WithMessage(Errors.LastNameRequired);

            RuleFor(command => command.Email)
                .NotEmpty()
                .EmailAddress()
                .WithMessage(Errors.EmailInvalid);

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

    }
}
