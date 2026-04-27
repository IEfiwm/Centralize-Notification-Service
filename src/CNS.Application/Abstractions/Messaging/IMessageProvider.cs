namespace CNS.Application.Abstractions.Messaging;

public interface IMessageProvider
{
    string Name { get; }
    string Channel { get; }

    Task SendAsync(MessageContext ctx, CancellationToken ct);
}

