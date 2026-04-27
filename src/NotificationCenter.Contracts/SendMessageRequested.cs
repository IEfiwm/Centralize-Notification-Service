using System.Text.Json.Serialization;

namespace NotificationCenter.Contracts;

public sealed record SendMessageRequested(
    string RequestId,
    string Channel,
    string Recipient,
    string? Subject,
    string Body,
    IReadOnlyDictionary<string, string>? Metadata,
    string? ProviderHint,
    DateTimeOffset EnqueuedAtUtc
)
{
    [JsonExtensionData]
    public Dictionary<string, object?>? ExtensionData { get; init; }
}

