namespace Lumiere.Domain.Common;

public abstract class BaseEntity
{
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    public bool Active { get; protected set; }

    public void Activate() => Active = true;
    public void Deactivate() => Active = false;
}
