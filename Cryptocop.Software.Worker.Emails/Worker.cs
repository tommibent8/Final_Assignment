using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Cryptocop.Software.Worker.Emails;

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

        _logger.LogInformation("📧 Email Worker connected to RabbitMQ.");
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
            _logger.LogInformation("📦 Received message for email: {Message}", message);

            try
            {
                var order = JsonSerializer.Deserialize<OrderMessage>(message);
                if (order != null)
                {
                    await SendEmailAsync(order);
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

    private async Task SendEmailAsync(OrderMessage order)
    {
        var apiKey = _config["SendGrid:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("SendGrid API key not configured.");
            return;
        }

        var client = new SendGridClient(apiKey);
        var from = new EmailAddress("no-reply@cryptocop.com", "Cryptocop Orders");
        var to = new EmailAddress(order.Email);
        var subject = "✅ Your Cryptocop order was successful!";

        var htmlContent = $@"
            <html>
            <body style='font-family: Arial;'>
                <h2>Order Confirmation</h2>
                <p>Hi {order.FullName},</p>
                <p>Your order (ID: {order.OrderId}) has been placed successfully on {order.OrderDate}.</p>
                <p><strong>Total:</strong> ${order.TotalPrice}</p>
                <h4>Items:</h4>
                <ul>
                    {string.Join("", order.Items.Select(i => $"<li>{i.ProductIdentifier} - {i.Quantity} × ${i.UnitPrice}</li>"))}
                </ul>
                <p> Will be sent to {order.Address}, {order.ZipCode} {order.City}, {order.Country} </p>   

                <p>Thank you for shopping with Cryptocop!</p>
            </body>
            </html>";

        _logger.LogInformation(htmlContent);

        var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);
        var response = await client.SendEmailAsync(msg);
        _logger.LogInformation("📨 Email sent to {Email}, status: {StatusCode}", order.Email, response.StatusCode);
    }

    private class OrderMessage
    {
        public string FullName { get; set; } = "";
        public string Address { get; set; } = "";
        public string City { get; set; } = "";
        public string Country { get; set; } = "";
        public string ZipCode { get; set; } = "";
        
        public int OrderId { get; set; }
        public string Email { get; set; } = "";
        public float TotalPrice { get; set; }
        public string OrderDate { get; set; } = "";
        public List<Item> Items { get; set; } = new();
    }

    private class Item
    {
        public string ProductIdentifier { get; set; } = "";
        public float Quantity { get; set; }
        public float UnitPrice { get; set; }
    }

    

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
