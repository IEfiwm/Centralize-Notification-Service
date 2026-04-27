using CNS.Domain.Common;

namespace CNS.Domain.Messaging;

public sealed class MessageLog : Entity
{
    public required string RequestId { get; init; }
    public required string Channel { get; init; }
    public required string Recipient { get; init; }
    public string? Subject { get; init; }
    public required string Body { get; init; }

    public string? ProviderHint { get; init; }
    public string? ProviderUsed { get; private set; }

    public MessageStatus Status { get; private set; } = MessageStatus.Queued;
    public string? Error { get; private set; }

    public DateTimeOffset EnqueuedAtUtc { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ProcessedAtUtc { get; private set; }

    public void MarkProcessing()
    {
        Status = MessageStatus.Processing;
        Error = null;
        Touch();
    }

    public void MarkSent(string providerUsed)
    {
        Status = MessageStatus.Sent;
        ProviderUsed = providerUsed;
        Error = null;
        ProcessedAtUtc = DateTimeOffset.UtcNow;
        Touch();
    }

    public void MarkFailed(string providerUsed, string error)
    {
        Status = MessageStatus.Failed;
        ProviderUsed = providerUsed;
        Error = error;
        ProcessedAtUtc = DateTimeOffset.UtcNow;
        Touch();
    }
}

