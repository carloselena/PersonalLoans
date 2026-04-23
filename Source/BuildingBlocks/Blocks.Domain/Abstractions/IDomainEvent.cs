namespace Blocks.Domain.Abstractions;

public interface IDomainEvent
{
    DateTimeOffset OccurredOn { get; }
}