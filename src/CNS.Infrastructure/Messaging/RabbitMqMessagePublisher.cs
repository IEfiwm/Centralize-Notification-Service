using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using CNS.Application.Abstractions.Messaging;
using CNS.Contracts;
using RabbitMQ.Client;

namespace CNS.Infrastructure.Messaging;

public sealed class RabbitMqMessagePublisher(IOptions<RabbitMqOptions> options) : IMessagePublisher
{
    public Task PublishAsync(SendMessageRequested message, CancellationToken ct)
    {
        var opt = options.Value;

        //var factory = new ConnectionFactory
        //{
        //    HostName = opt.HostName,
        //    Port = opt.Port,
        //    VirtualHost = opt.VirtualHost,
        //    UserName = opt.UserName,
        //    Password = opt.Password,
        //    DispatchConsumersAsync = true
        //};

        var factory = new ConnectionFactory
        {
            HostName = "192.168.90.157",
            Port = 5672,
            VirtualHost = "/",
            UserName = "adminDaric",
            Password = "YET8fVhGngA96HUPZpk4zB",
            DispatchConsumersAsync = true
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(
            queue: opt.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        var props = channel.CreateBasicProperties();
        props.Persistent = true;
        props.ContentType = "application/json";
        props.MessageId = message.RequestId;
        props.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        channel.BasicPublish(
            exchange: "",
            routingKey: opt.QueueName,
            basicProperties: props,
            body: body
        );

        return Task.CompletedTask;
    }
}

