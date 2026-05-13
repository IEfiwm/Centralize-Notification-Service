namespace CNS.Contracts.Api;

public sealed record QuickSendEmailRequestDto(string Email, string Message, string? ProviderHint = null);

