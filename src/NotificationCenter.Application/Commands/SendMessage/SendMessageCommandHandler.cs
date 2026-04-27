using NotificationCenter.Application.Abstractions.Messaging;
using NotificationCenter.Application.Abstractions.Persistence;
using NotificationCenter.Contracts;
using NotificationCenter.Domain.Messaging;

namespace NotificationCenter.Application.Commands.SendMessage;

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

