namespace Lumiere.Domain.Entities;

public class Channel
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public bool Active { get; private set; }

    public int UserId { get; private set; }
    public User User { get; private set; } = null!;

    public static Channel Create(string name, string? description, int userId)
    {
        return new Channel
        {
            Name = name,
            Description = description,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            Active = true
        };
    }

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate() => Active = false;
    public void Activate() => Active = true;
}
