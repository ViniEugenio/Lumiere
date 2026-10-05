namespace Lumiere.Application.DTOs.User
{
    public record JWTUserDataDto(string Name, string Surname, string Email, List<int> Roles);
}
