using Blocks.Domain.Time;

namespace Identity.Application.Outbox;

public class OutboxMessage
{
    public Guid Id { get; private set; }
    public string Type { get; private set; }
    public string Payload { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }
    public string? Error { get; private set; }
    public int RetryCount { get; private set; }

    public OutboxMessage(string type, string payload)
    {
        Type = type;
        Payload = payload;
        OccurredAt = DateProvider.UtcNow();
        Id = Guid.CreateVersion7();
    }

    public void MarkAsProcessed() => ProcessedAt = DateProvider.UtcNow();

    public void MarkAsFailed(string error)
    {
        Error = error;
        RetryCount++;
    }
}