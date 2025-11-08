using System.Text;
using System.Text.Json;
using Cryptocop.Software.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata;
using RabbitMQ.Client;

namespace Cryptocop.Software.API.Services.Implementations;

public class QueueService : IQueueService, IDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel; 

    public QueueService()
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "guest",
            Password = "guest"
        };

        // New async connection pattern introduced in v7+
        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

        // Exchange declaration now requires async method as well
        _channel.ExchangeDeclareAsync(
            exchange: "cryptocop-exchange",
            type: ExchangeType.Direct,
            durable: true
        ).GetAwaiter().GetResult();
    }

    public async Task PublishMessageAsync(string routingKey, object body)
    {
        var message = JsonSerializer.Serialize(body);
        var bodyBytes = Encoding.UTF8.GetBytes(message);

        var properties = new BasicProperties(); 

        await _channel.BasicPublishAsync(
            exchange: "cryptocop-exchange",
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties,
            body: bodyBytes
        );

        Console.WriteLine($" Message published to '{routingKey}': {message}");
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}