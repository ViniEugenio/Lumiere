using Isopoh.Cryptography.Argon2;
using Lumiere.Application.Interfaces.Services;
using Lumiere.Infra.EnvConfigurationModels;
using Microsoft.Extensions.Options;

namespace Lumiere.Infra.Security;

public class PasswordService(IOptions<SecurityModel> securityModelOptions) : IPasswordService
{

    public string Hash(string password)
    {
        return Argon2.Hash(password, securityModelOptions.Value.Pepper);
    }

    public bool Verify(string hash, string password)
    {
        return Argon2.Verify(hash, password, securityModelOptions.Value.Pepper);
    }

}
