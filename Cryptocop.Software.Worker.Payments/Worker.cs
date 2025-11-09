using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Cryptocop.Software.Worker.Payments;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _config;
    private IConnection? _connection;
    private IChannel? _channel;
    private string _queueName = "";

    public Worker(ILogger<Worker> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }


    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _config["RabbitMq:HostName"],
            UserName = _config["RabbitMq:UserName"],
            Password = _config["RabbitMq:Password"]
        };

        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync();

        string exchangeName = _config["RabbitMq:ExchangeName"]!;
        _queueName = _config["RabbitMq:QueueName"]!;
        string routingKey = _config["RabbitMq:RoutingKey"]!;

        await _channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Direct, durable: true, cancellationToken: cancellationToken);
        await _channel.QueueDeclareAsync(_queueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);
        await _channel.QueueBindAsync(_queueName, exchangeName, routingKey, cancellationToken: cancellationToken);

        _logger.LogInformation("Payment Worker connected to RabbitMQ.");
        await base.StartAsync(cancellationToken);
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_channel == null)
            return;

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (sender, e) =>
        {
            var message = Encoding.UTF8.GetString(e.Body.ToArray());
            _logger.LogInformation("📦 Received message: {Message}", message);

            try
            {
                var order = JsonSerializer.Deserialize<OrderMessage>(message);
                if (order != null)
                {
                    bool isValid = LuhnCheck(order.CreditCardNumber);
                    _logger.LogInformation(isValid
                        ? "Valid credit card for {Email}"
                        : "Invalid credit card for {Email}", order.Email);
                }

                await _channel!.BasicAckAsync(e.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
                await _channel!.BasicNackAsync(e.DeliveryTag, multiple: false, requeue: true);
            }
        };

        await _channel.BasicConsumeAsync(_queueName, autoAck: false, consumer, stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    
    private bool LuhnCheck(string cardNumber)
    {
        int sum = 0;
        bool alternate = false;

        for (int i = cardNumber.Length - 1; i >= 0; i--)
        {
            int n = int.Parse(cardNumber[i].ToString());
            if (alternate)
            {
                n *= 2;
                if (n > 9) n -= 9;
            }
            sum += n;
            alternate = !alternate;
        }
        return sum % 10 == 0;
    }

    private class OrderMessage
    {
        public string Email { get; set; } = "";
        public string CreditCardNumber { get; set; } = "";
    }

   

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
