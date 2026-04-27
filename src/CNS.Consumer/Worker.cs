using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NotificationCenter.Application.Abstractions.Messaging;
using NotificationCenter.Application.Abstractions.Persistence;
using NotificationCenter.Contracts;
using NotificationCenter.Domain.Messaging;
using NotificationCenter.Infrastructure.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationCenter.Consumer;

public sealed class Worker(
    ILogger<Worker> logger,
    IServiceScopeFactory scopeFactory,
    IOptions<RabbitMqOptions> rabbitOptions
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var opt = rabbitOptions.Value;
        var factory = new ConnectionFactory
        {
            HostName = opt.HostName,
            Port = opt.Port,
            VirtualHost = opt.VirtualHost,
            UserName = opt.UserName,
            Password = opt.Password,
            DispatchConsumersAsync = true
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(opt.QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        channel.BasicQos(prefetchSize: 0, prefetchCount: 10, global: false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var evt = JsonSerializer.Deserialize<SendMessageRequested>(json);
                if (evt is null)
                {
                    channel.BasicAck(ea.DeliveryTag, multiple: false);
                    return;
                }

                await ProcessAsync(evt, stoppingToken);
                channel.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing message, will requeue");
                channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        channel.BasicConsume(queue: opt.QueueName, autoAck: false, consumer: consumer);

        logger.LogInformation("Consumer started. Listening on queue {Queue}", opt.QueueName);
        await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
    }

    private async Task ProcessAsync(SendMessageRequested evt, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var logs = scope.ServiceProvider.GetRequiredService<IRepository<MessageLog>>();
        var resolver = scope.ServiceProvider.GetRequiredService<IMessageProviderResolver>();

        var log = await logs.Query().FirstOrDefaultAsync(x => x.RequestId == evt.RequestId, ct);
        if (log is null)
        {
            logger.LogWarning("No MessageLog found for RequestId={RequestId}", evt.RequestId);
            return;
        }

        log.MarkProcessing();
        await logs.SaveChangesAsync(ct);

        var provider = resolver.Resolve(evt.Channel, evt.ProviderHint);

        try
        {
            await provider.SendAsync(new MessageContext(
                RequestId: evt.RequestId,
                Channel: evt.Channel,
                Recipient: evt.Recipient,
                Subject: evt.Subject,
                Body: evt.Body,
                Metadata: evt.Metadata,
                ProviderHint: evt.ProviderHint
            ), ct);

            log.MarkSent(provider.Name);
            await logs.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            log.MarkFailed(provider.Name, ex.Message);
            await logs.SaveChangesAsync(ct);
            throw;
        }
    }
}
