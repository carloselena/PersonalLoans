namespace Blocks.IntegrationEvents;

public record UserCreatedIntegrationEvent(
    Guid UserId,
    DateTimeOffset OccuredAt) : IIntegrationEvent
{
    public Guid Id { get; } = Guid.CreateVersion7();
}