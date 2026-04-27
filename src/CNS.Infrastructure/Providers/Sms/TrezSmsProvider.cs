using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CNS.Application.Abstractions.Messaging;
using CNS.Contracts;

namespace CNS.Infrastructure.Providers.Sms;

public sealed class TrezSmsProvider(
    HttpClient httpClient,
    IOptions<TrezSmsOptions> options,
    ILogger<TrezSmsProvider> logger
) : IMessageProvider
{
    public string Name => options.Value.ProviderName;
    public string Channel => Channels.Sms;

    public async Task SendAsync(MessageContext ctx, CancellationToken ct)
    {
        var opt = options.Value;
        if (string.IsNullOrWhiteSpace(opt.Username) || string.IsNullOrWhiteSpace(opt.Password))
            throw new InvalidOperationException("Trez SMS credentials are not configured.");

        var message = ctx.Body;
        if (!string.IsNullOrWhiteSpace(opt.RemoveToken))
            message = message.Replace(opt.RemoveToken, "", StringComparison.Ordinal);

        var url =
            $"{opt.EndpointPath}?Username={WebUtility.UrlEncode(opt.Username)}" +
            $"&Password={WebUtility.UrlEncode(opt.Password)}" +
            $"&Mobile={WebUtility.UrlEncode(ctx.Recipient)}" +
            $"&Message={WebUtility.UrlEncode(message)}";

        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        using var res = await httpClient.SendAsync(req, ct);

        if (!res.IsSuccessStatusCode)
        {
            var body = await res.Content.ReadAsStringAsync(ct);
            logger.LogWarning(
                "Trez SMS failed. Status={StatusCode} Response={Response}",
                (int)res.StatusCode,
                body
            );

            res.EnsureSuccessStatusCode();
        }
    }
}

