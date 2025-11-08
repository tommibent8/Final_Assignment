using System.Text;
using Cryptocop.Software.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Cryptocop.Software.API.Services.Implementations;

public class QueueService : IQueueService, IDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;

    public QueueService(IConfiguration configuration)
    {
        // Get RabbitMQ connection settings from configuration
        var rabbitMqHost = configuration["RabbitMQ:Host"] ?? "localhost";
        var rabbitMqPort = int.Parse(configuration["RabbitMQ:Port"] ?? "5672");
        var rabbitMqUser = configuration["RabbitMQ:Username"] ?? "guest";
        var rabbitMqPassword = configuration["RabbitMQ:Password"] ?? "guest";

        var factory = new ConnectionFactory
        {
            HostName = rabbitMqHost,
            Port = rabbitMqPort,
            UserName = rabbitMqUser,
            Password = rabbitMqPassword
        };

        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

        // Declare a topic exchange
        _channel.ExchangeDeclareAsync(exchange: "cryptocop-exchange", type: ExchangeType.Topic, durable: true).GetAwaiter().GetResult();
    }

    public async Task PublishMessageAsync(string routingKey, object body)
    {
        // Serialize the object to JSON
        var message = JsonConvert.SerializeObject(body);
        var bodyBytes = Encoding.UTF8.GetBytes(message);

        // Publish the message using the channel
        await _channel.BasicPublishAsync(
            exchange: "cryptocop-exchange",
            routingKey: routingKey,
            mandatory: false,
            basicProperties: new BasicProperties(),
            body: bodyBytes
        );
    }

    public void Dispose()
    {
        if (_channel != null)
        {
            _channel.CloseAsync().GetAwaiter().GetResult();
            _channel.Dispose();
        }
        if (_connection != null)
        {
            _connection.CloseAsync().GetAwaiter().GetResult();
            _connection.Dispose();
        }
    }
}