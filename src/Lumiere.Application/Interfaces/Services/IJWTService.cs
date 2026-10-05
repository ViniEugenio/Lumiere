using Lumiere.Application.DTOs.User;

namespace Lumiere.Application.Interfaces.Services
{
    public interface IJWTService
    {
        string GenerateJWT(JWTUserDataDto jwtUserData);
    }
}
