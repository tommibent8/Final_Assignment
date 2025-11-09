using System.Text;
using System.Text.Json;
using Cryptocop.Software.API.Models;
using Cryptocop.Software.API.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Cryptocop.Software.API.Services.Implementations;

public class QueueService : IQueueService, IAsyncDisposable, IDisposable
{
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<QueueService> _logger;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private IConnection? _connection;
    private IChannel? _channel;

    public QueueService(IOptions<RabbitMqSettings> settings, ILogger<QueueService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    private async Task EnsureInitializedAsync()
    {
        if (_channel != null) return;

        await _initLock.WaitAsync();
        try
        {
            if (_channel != null) return;

            var factory = new ConnectionFactory
            {
                HostName = _settings.Host,
                Port = _settings.Port,
                UserName = _settings.Username,
                Password = _settings.Password
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            await _channel.ExchangeDeclareAsync(
                exchange: _settings.ExchangeName,
                type: ExchangeType.Direct,
                durable: true
            );

            _logger.LogInformation("RabbitMQ connection initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ connection");
            throw;
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task PublishMessageAsync(string routingKey, object body)
    {
        await EnsureInitializedAsync();

        if (_channel == null)
        {
            throw new InvalidOperationException("Channel not initialized");
        }

        var message = JsonSerializer.Serialize(body);
        var bodyBytes = Encoding.UTF8.GetBytes(message);

        var properties = new BasicProperties();

        await _channel.BasicPublishAsync(
            exchange: _settings.ExchangeName,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties,
            body: bodyBytes
        );

        _logger.LogInformation("Message published to '{RoutingKey}': {Message}", routingKey, message);
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel != null)
        {
            await _channel.CloseAsync();
            _channel.Dispose();
        }

        if (_connection != null)
        {
            await _connection.CloseAsync();
            _connection.Dispose();
        }

        _initLock.Dispose();
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        _initLock.Dispose();
    }
}