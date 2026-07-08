using Lumiere.Domain.Interfaces;
using System.Security.Cryptography;

namespace Lumiere.Infra.Security;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        int saltSize = 16;
        int hashSize = 32;
        int iterations = 100_000;

        byte[] salt = RandomNumberGenerator.GetBytes(saltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, hashSize);

        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }
}
