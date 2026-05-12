namespace CNS.Infrastructure.Providers.Sms.GhasedMehr;

public sealed class GhasedmehrSmsOptions
{
    public string ProviderName { get; init; } = "ghasedmehr";
    public string BaseUrl { get; init; } = "https://apiv2.ghasedmehr.ir/";
    public string Username { get; init; } = "";
    public string Password { get; init; } = "";
    public string Sender { get; init; } = "";
    public int MediaType { get; init; } = 1;
    public string Channel { get; init; } = "user-interface";
    public string SendMessagePath { get; init; } = "/notification/api/v1/user/SendMessage/SendQuickMessage";
    public string LoginPath { get; init; } = "/sso/api/v1/user/LoginWithPassword";
    public string RemoveToken { get; init; } = "لغو11";
}

