using System.Text;
using CreditCardValidator;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Cryptocop.Software.Worker.Payments;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    private IConnection _connection;
    private IChannel _channel;

    public Worker(ILogger<Worker> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        // Setup RabbitMQ connection
        var rabbitMqHost = _configuration["RabbitMQ:Host"] ?? "localhost";
        var rabbitMqPort = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672");
        var rabbitMqUser = _configuration["RabbitMQ:Username"] ?? "guest";
        var rabbitMqPassword = _configuration["RabbitMQ:Password"] ?? "guest";

        var factory = new ConnectionFactory
        {
            HostName = rabbitMqHost,
            Port = rabbitMqPort,
            UserName = rabbitMqUser,
            Password = rabbitMqPassword
        };

        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        // Declare exchange
        await _channel.ExchangeDeclareAsync(exchange: "cryptocop-exchange", type: ExchangeType.Topic, durable: true);

        // Declare queue
        await _channel.QueueDeclareAsync(queue: "payment-queue", durable: true, exclusive: false, autoDelete: false);

        // Bind queue to routing key 'create-order'
        await _channel.QueueBindAsync(queue: "payment-queue", exchange: "cryptocop-exchange", routingKey: "create-order");

        _logger.LogInformation("Payment Worker started and listening for messages");

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                _logger.LogInformation("Received order message");

                // Deserialize the order
                var order = JsonConvert.DeserializeObject<dynamic>(message);
                string creditCardNumber = order?.CreditCard;

                if (!string.IsNullOrEmpty(creditCardNumber))
                {
                    // Validate credit card using CreditCardValidator library
                    var detector = new CreditCardDetector(creditCardNumber);
                    bool isValid = detector.IsValid();
                    string cardBrand = detector.Brand.ToString();

                    // Print validation result to console
                    _logger.LogInformation("--------------------------------------------------");
                    _logger.LogInformation($"Credit Card Validation for Order #{order?.Id}");
                    _logger.LogInformation($"Card Number: {creditCardNumber}");
                    _logger.LogInformation($"Card Brand: {cardBrand}");
                    _logger.LogInformation($"Is Valid: {isValid}");
                    _logger.LogInformation($"Validation Message: {(isValid ? "Credit card is valid" : "Credit card is invalid")}");
                    _logger.LogInformation("--------------------------------------------------");
                }
                else
                {
                    _logger.LogWarning("No credit card number found in order");
                }

                // Acknowledge the message
                await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment validation");
            }
        };

        await _channel.BasicConsumeAsync(queue: "payment-queue", autoAck: false, consumer: consumer);

        // Keep the worker running
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel != null)
            await _channel.CloseAsync();
        if (_connection != null)
            await _connection.CloseAsync();
        await base.StopAsync(cancellationToken);
    }
}
