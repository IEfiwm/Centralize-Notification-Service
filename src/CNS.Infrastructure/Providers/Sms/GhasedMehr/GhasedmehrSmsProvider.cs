using CNS.Application.Abstractions.Messaging;
using CNS.Contracts;
using CNS.Infrastructure.Providers.Sms.GhasedMehr.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Json;

namespace CNS.Infrastructure.Providers.Sms.GhasedMehr;

public sealed class GhasedmehrSmsProvider(
  HttpClient httpClient,
  IOptions<GhasedmehrSmsOptions> options,
  ILogger<GhasedmehrSmsProvider> logger
    ) : IMessageProvider
{
    public string Name => options.Value.ProviderName;
    public string Channel => Channels.Sms;

    private const string BaseUrl = "https://apiv2.ghasedmehr.ir";
    public static string? AccessToken { get; set; }

    public async Task SendAsync(MessageContext ctx, CancellationToken ct)
    {
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("channel", "user-interface");
        string url = $"{BaseUrl}/sso/api/v1/user/LoginWithPassword";

        var loginData = new
        {
            UserName = "daric",
            Password = "i2$W2N6wDO"
        };

        try
        {
            if (string.IsNullOrEmpty(AccessToken))
            {
                var loginResponse = await httpClient.PostAsJsonAsync(url, loginData);
                loginResponse.EnsureSuccessStatusCode();
                var login = await loginResponse.Content.ReadAsStringAsync();
                var loginResult = JsonConvert.DeserializeObject<GhasedmehrAuthResponse>(login);

                AccessToken = loginResult.Data.Token.Accesstoken;
            }
            httpClient.DefaultRequestHeaders.Add("Authorization", $@"Bearer {AccessToken}");

            var sendData = new
            {
                MediaType = 1,
                Message = ctx.Body,
                PhoneNumber = ctx.Recipient,
                Sender = "2000221002"
            };

            var sendResponse = await httpClient.PostAsJsonAsync("https://apiv2.ghasedmehr.ir/notification/api/v1/user/SendMessage/SendQuickMessage", sendData);

            sendResponse.EnsureSuccessStatusCode();
            string sendResult = await sendResponse.Content.ReadAsStringAsync();

            await Task.CompletedTask;
        }
        catch (HttpRequestException e)
        {
            AccessToken = "";
            Console.WriteLine($"Error sending message: {e.Message}");
        }
    }
}