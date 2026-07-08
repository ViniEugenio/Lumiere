using System.Resources;

namespace Lumiere.Application.Resources;

internal static class Errors
{
    private static readonly ResourceManager _resourceManager =
        new("Lumiere.Application.Resources.Errors", typeof(Errors).Assembly);

    internal static string EmailAlreadyInUse =>
        _resourceManager.GetString(nameof(EmailAlreadyInUse))!;

    internal static string ConfirmEmail =>
        _resourceManager.GetString(nameof(ConfirmEmail))!;

    internal static string PasswordUppercase =>
        _resourceManager.GetString(nameof(PasswordUppercase))!;

    internal static string PasswordLowercase =>
        _resourceManager.GetString(nameof(PasswordLowercase))!;

    internal static string PasswordLeastOneDigit =>
        _resourceManager.GetString(nameof(PasswordLeastOneDigit))!;

    internal static string PasswordNonAlphanumeric =>
        _resourceManager.GetString(nameof(PasswordNonAlphanumeric))!;

    internal static string ConfirmPassword =>
        _resourceManager.GetString(nameof(ConfirmPassword))!;

    internal static string FirstNameRequired =>
        _resourceManager.GetString(nameof(FirstNameRequired))!;

    internal static string LastNameRequired =>
        _resourceManager.GetString(nameof(LastNameRequired))!;

    internal static string EmailInvalid =>
        _resourceManager.GetString(nameof(EmailInvalid))!;

    internal static string InvalidPage =>
        _resourceManager.GetString(nameof(InvalidPage))!;

    internal static string InvalidPageAmount =>
        _resourceManager.GetString(nameof(InvalidPageAmount))!;
}
