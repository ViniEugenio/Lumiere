using FluentValidation;
using Lumiere.Application.Features.Users.Commands;

namespace Lumiere.Application.Validators;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(command => command.Username)
            .NotEmpty()
            .MinimumLength(6);

        RuleFor(command => command.Password)
            .NotEmpty()
            .MinimumLength(6)
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least one non-alphanumeric character.");

        RuleFor(command => command.ConfirmPassword)
            .NotEmpty()
            .Equal(command => command.Password).WithMessage("ConfirmPassword must match Password.");
    }
}
