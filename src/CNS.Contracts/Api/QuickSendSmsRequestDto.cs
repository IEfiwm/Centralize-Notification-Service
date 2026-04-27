namespace CNS.Contracts.Api;

/// <summary>
/// Minimal payload for SMS: phone number + message text. Optional provider name (e.g. trez, sample-sms).
/// </summary>
public sealed record QuickSendSmsRequestDto(
    string Phone,
    string Message,
    string? ProviderHint = null
);
