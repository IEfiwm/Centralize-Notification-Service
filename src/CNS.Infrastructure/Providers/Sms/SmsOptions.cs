namespace NotificationCenter.Infrastructure.Providers.Sms;

public sealed class SmsOptions
{
    public string ProviderName { get; init; } = "sample-sms";
    public string ApiKey { get; init; } = "CHANGE_ME";
    public string Sender { get; init; } = "NC";
}

