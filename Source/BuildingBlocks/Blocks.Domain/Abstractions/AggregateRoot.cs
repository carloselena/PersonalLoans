namespace Blocks.Domain.Abstractions;

public abstract class AggregateRoot : IAggregateRoot
{
    public Guid Id { get; protected set; } = Guid.Empty;
    
    private readonly List<IDomainEvent> _domainEvents = [];
    
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;
    
    
    public void AddDomainEvent(IDomainEvent domainEvent)
        => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents()
        => _domainEvents.Clear();
}