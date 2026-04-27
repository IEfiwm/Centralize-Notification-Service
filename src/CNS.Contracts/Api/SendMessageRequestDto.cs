namespace CNS.Contracts.Api;

public sealed record SendMessageRequestDto(
    string Channel,
    string Recipient,
    string? Subject,
    string Body,
    Dictionary<string, string>? Metadata,
    string? ProviderHint
);

