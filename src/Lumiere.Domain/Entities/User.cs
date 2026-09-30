namespace Lumiere.Domain.Entities;

public class User : BaseEntity
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }

    public List<Channel> Channels { get; private set; } = [];
    public List<UserRole> Roles { get; private set; } = [];

    public static User Create(string firstName, string lastName, string email, string passwordHash)
    {
        return new User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            CreatedAt = DateTime.UtcNow,
            Active = true,
            PasswordHash = passwordHash
        };
    }

    public void Update(string firstName, string lastName, string email)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        UpdatedAt = DateTime.UtcNow;
    }
}
