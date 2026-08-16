namespace FirstGIG.BuildingBlocks.Domain.Primitives;

public abstract class Entity
{
    protected Entity(Guid id)
    {
        Id = id;
    }

    protected Entity() { }

    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    public void SetUpdatedAt() => UpdatedAt = DateTime.UtcNow;

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Entity? left, Entity? right) =>
        left is not null && right is not null && left.Equals(right);

    public static bool operator !=(Entity? left, Entity? right) => !(left == right);
}
