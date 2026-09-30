namespace Lumiere.Domain.Entities;

public abstract class BaseEntity
{
    public int Id { get; private set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    public bool Active { get; protected set; }

    public void Activate() => Active = true;
    public void Deactivate() => Active = false;

    protected virtual T Create<T>()
    {

        CreatedAt = DateTime.UtcNow;
        Active = true;

        return default;

    }

}
