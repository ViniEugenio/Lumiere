using Isopoh.Cryptography.Argon2;
using Lumiere.Application.Interfaces.Services;
using Lumiere.Infra.EnvConfigurationModels;
using Microsoft.Extensions.Options;

namespace Lumiere.Infra.Security;

public class PasswordHasher(IOptions<SecurityModel> securityModelOptions) : IPasswordHasher
{

    public string Hash(string password)
    {
        return Argon2.Hash(password, securityModelOptions.Value.Pepper);
    }

}
