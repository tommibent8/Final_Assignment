using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using IModel = RabbitMQ.Client.IModel;

namespace Cryptocop.Software.Worker.Payments;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _config;
    private IConnection? _connection;
    private IModel? _channel;
    private string _queueName = "";

    public Worker(ILogger<Worker> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }


    // This runs once, to set up the connection
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _config["RabbitMq:HostName"],
            UserName = _config["RabbitMq:UserName"],
            Password = _config["RabbitMq:Password"]
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        string exchangeName = _config["RabbitMq:ExchangeName"]!;
        _queueName = _config["RabbitMq:QueueName"]!;
        string routingKey = _config["RabbitMq:RoutingKey"]!;

        _channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, durable: true);
        _channel.QueueDeclare(_queueName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(_queueName, exchangeName, routingKey);

        _logger.LogInformation("Payment Worker connected to RabbitMQ.");
        return base.StartAsync(cancellationToken);
    }
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_channel == null) 
            return Task.CompletedTask;

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (sender, e) =>
        {
            var message = Encoding.UTF8.GetString(e.Body.ToArray());
            _logger.LogInformation($"📦 Received message: {message}");

            try
            {
                var order = JsonSerializer.Deserialize<OrderMessage>(message);
                if (order != null)
                {
                    bool isValid = LuhnCheck(order.CreditCardNumber);
                    _logger.LogInformation(isValid
                        ? $"Valid credit card for {order.Email}"
                        : $"Invalid credit card for {order.Email}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing message: {ex.Message}");
            }

            _channel?.BasicAck(e.DeliveryTag, multiple: false);
        };

        _channel.BasicConsume(_queueName, autoAck: false, consumer);
        return Task.CompletedTask;
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
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}
