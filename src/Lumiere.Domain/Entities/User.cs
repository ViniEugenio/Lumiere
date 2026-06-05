using Microsoft.AspNetCore.Identity;

namespace Lumiere.Domain.Entities;

public class User : IdentityUser<int>
{
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public bool Active { get; private set; }

    public ICollection<Canal> Canais { get; private set; } = [];

    public static User Create(string userName, string email)
    {
        return new User
        {
            UserName = userName,
            Email = email,
            CreatedAt = DateTime.UtcNow,
            Active = true
        };
    }

    public void Update(string userName, string email)
    {
        UserName = userName;
        Email = email;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate() => Active = false;
    public void Activate() => Active = true;
}
