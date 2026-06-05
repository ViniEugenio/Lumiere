using System.Resources;

namespace Lumiere.Application.Resources;

internal static class Errors
{
    private static readonly ResourceManager _resourceManager =
        new("Lumiere.Application.Resources.Errors", typeof(Errors).Assembly);

    internal static string UsernameAlreadyInUse =>
        _resourceManager.GetString(nameof(UsernameAlreadyInUse))!;

    internal static string EmailAlreadyInUse =>
        _resourceManager.GetString(nameof(EmailAlreadyInUse))!;
}
