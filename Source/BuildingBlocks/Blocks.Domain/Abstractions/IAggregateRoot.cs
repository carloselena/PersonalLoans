namespace Blocks.Domain.Abstractions;

public interface IAggregateRoot
{
    Guid Id { get; }
    
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    void AddDomainEvent(IDomainEvent domainEvent);
    void ClearDomainEvents();
}