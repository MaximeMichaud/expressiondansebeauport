namespace Domain.Common;

public abstract class Entity
{
    public Guid Id { get; private set; }

    public void SetId(Guid id) => Id = id;
}