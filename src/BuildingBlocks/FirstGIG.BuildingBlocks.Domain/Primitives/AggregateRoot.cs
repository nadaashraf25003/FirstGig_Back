namespace FirstGIG.BuildingBlocks.Domain.Primitives;

public abstract class AggregateRoot : Entity
{
    protected AggregateRoot(Guid id) : base(id) { }
    protected AggregateRoot() { }

    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent) =>
        _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
