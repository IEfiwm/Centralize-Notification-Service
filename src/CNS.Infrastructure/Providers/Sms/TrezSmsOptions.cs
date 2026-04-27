namespace CNS.Infrastructure.Providers.Sms;

public sealed class TrezSmsOptions
{
    public string ProviderName { get; init; } = "trez";

    public string BaseUrl { get; init; } = "http://smspanel.Trez.ir/";
    public string EndpointPath { get; init; } = "SendMessageWithCode.ashx";

    public string Username { get; init; } = "";
    public string Password { get; init; } = "";

    public string RemoveToken { get; init; } = "لغو11";
}

