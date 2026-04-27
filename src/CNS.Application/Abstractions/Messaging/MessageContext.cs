namespace CNS.Application.Abstractions.Messaging;

public sealed record MessageContext(
    string RequestId,
    string Channel,
    string Recipient,
    string? Subject,
    string Body,
    IReadOnlyDictionary<string, string>? Metadata,
    string? ProviderHint
);

