using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using CNS.Application.Abstractions.Messaging;
using CNS.Application.Abstractions.Persistence;
using CNS.Contracts;
using CNS.Domain.Messaging;
using CNS.Infrastructure.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CNS.Consumer;

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

        //var factory = new ConnectionFactory
        //{
        //    HostName = "localhost",
        //    Port = 5672,
        //    VirtualHost = "/",
        //    UserName = "admin",
        //    Password = "admin123",
        //    DispatchConsumersAsync = true
        //};

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        logger.LogWarning("🎯 WORKER EXECUTEASYNC IS RUNNING!");

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

            await logs.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            await logs.SaveChangesAsync(ct);
            throw;
        }
    }
}
