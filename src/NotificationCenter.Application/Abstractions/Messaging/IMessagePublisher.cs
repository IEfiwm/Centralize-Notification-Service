using NotificationCenter.Contracts;

namespace NotificationCenter.Application.Abstractions.Messaging;

public interface IMessagePublisher
{
    Task PublishAsync(SendMessageRequested message, CancellationToken ct);
}

