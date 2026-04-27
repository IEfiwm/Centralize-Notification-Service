using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationCenter.Application.Abstractions.Messaging;
using NotificationCenter.Contracts;

namespace NotificationCenter.Infrastructure.Providers.Sms;

public sealed class SampleSmsProvider(IOptions<SmsOptions> options, ILogger<SampleSmsProvider> logger) : IMessageProvider
{
    public string Name => options.Value.ProviderName;
    public string Channel => Channels.Sms;

    public Task SendAsync(MessageContext ctx, CancellationToken ct)
    {
        // نمونه: جایگزین با API واقعی SMS شما (Kavenegar, Twilio, ...)
        logger.LogInformation(
            "SMS sent via {Provider}. To={Recipient} Body={Body}",
            Name,
            ctx.Recipient,
            ctx.Body
        );

        return Task.CompletedTask;
    }
}

