using CNS.Application.Abstractions.Messaging;
using CNS.Application.Abstractions.Persistence;
using CNS.Contracts;
using CNS.Domain.Messaging;

namespace CNS.Application.Commands.SendMessage;

public sealed class SendMessageCommandHandler(
    IRepository<MessageLog> messageLogs,
    IMessagePublisher publisher
)
{
    public async Task HandleAsync(SendMessageCommand cmd, CancellationToken ct)
    {
        var log = new MessageLog
        {
            RequestId = cmd.RequestId,
            Channel = cmd.Request.Channel,
            Recipient = cmd.Request.Recipient,
            Subject = cmd.Request.Subject,
            Body = cmd.Request.Body,
            ProviderHint = cmd.Request.ProviderHint,
            EnqueuedAtUtc = DateTimeOffset.UtcNow
        };

        await messageLogs.AddAsync(log, ct);
        await messageLogs.SaveChangesAsync(ct);

        var evt = new SendMessageRequested(
            RequestId: cmd.RequestId,
            Channel: cmd.Request.Channel,
            Recipient: cmd.Request.Recipient,
            Subject: cmd.Request.Subject,
            Body: cmd.Request.Body,
            Metadata: cmd.Request.Metadata,
            ProviderHint: cmd.Request.ProviderHint,
            EnqueuedAtUtc: log.EnqueuedAtUtc
        );

        await publisher.PublishAsync(evt, ct);
    }
}

