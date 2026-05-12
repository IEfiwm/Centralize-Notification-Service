using CNS.Application.Abstractions.Messaging;
using CNS.Contracts;
using CNS.Infrastructure.Providers.Sms.GhasedMehr;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CNS.Infrastructure.Providers.Sms.FaraPayamak
{
    public class FaraPayamakProvider(
        HttpClient httpClient,
        IOptions<FaraPayamakOptions> options,
        ILogger<FaraPayamakProvider> logger) : IMessageProvider
    {
        public string Name => options.Value.ProviderName;

        public string Channel => Channels.Sms;

        public async Task SendAsync(MessageContext ctx, CancellationToken ct)
        {
            FormUrlEncodedContent formContent = new(new[]
                    {
                      new KeyValuePair<string, string>("username", "Daric"),
                      new KeyValuePair<string, string>("password", "Daric.gold@1402"),
                      new KeyValuePair<string, string>("to", ctx.Recipient),
                      new KeyValuePair<string, string>("from", "10004561292549"),
                      new KeyValuePair<string, string>("text", ctx.Body),
                      new KeyValuePair<string, string>("isflash", "false")
                    });

            try
            {
                var response = await httpClient.PostAsync("https://rest.payamak-panel.com/api/SendSMS/SendSMS", formContent);

                response.EnsureSuccessStatusCode();
                string sendResult = await response.Content.ReadAsStringAsync();

                await Task.CompletedTask;
            }
            catch
            {
            }

        }
    }
}
