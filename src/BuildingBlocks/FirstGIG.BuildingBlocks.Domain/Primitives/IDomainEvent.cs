namespace FirstGIG.BuildingBlocks.Domain.Primitives;

public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
}
