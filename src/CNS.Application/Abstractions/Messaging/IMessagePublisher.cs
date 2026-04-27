using CNS.Contracts;

namespace CNS.Application.Abstractions.Messaging;

public interface IMessagePublisher
{
    Task PublishAsync(SendMessageRequested message, CancellationToken ct);
}

