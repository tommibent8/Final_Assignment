using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Cryptocop.Software.Worker.Emails;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    private IConnection _connection;
    private IChannel _channel;

    public Worker(ILogger<Worker> logger, IConfiguration _configuration)
    {
        _logger = logger;
        this._configuration = _configuration;
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
        await _channel.QueueDeclareAsync(queue: "email-queue", durable: true, exclusive: false, autoDelete: false);

        // Bind queue to routing key 'create-order'
        await _channel.QueueBindAsync(queue: "email-queue", exchange: "cryptocop-exchange", routingKey: "create-order");

        _logger.LogInformation("Email Worker started and listening for messages");

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

                _logger.LogInformation("Received order message for email notification");

                // Deserialize the order
                var order = JsonConvert.DeserializeObject<dynamic>(message);

                // Send email via SendGrid
                await SendOrderConfirmationEmail(order);

                // Acknowledge the message
                await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing email notification");
            }
        };

        await _channel.BasicConsumeAsync(queue: "email-queue", autoAck: false, consumer: consumer);

        // Keep the worker running
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task SendOrderConfirmationEmail(dynamic order)
    {
        var apiKey = _configuration["SendGrid:ApiKey"];
        var fromEmail = _configuration["SendGrid:FromEmail"] ?? "noreply@cryptocop.com";
        var fromName = _configuration["SendGrid:FromName"] ?? "Cryptocop";

        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning("SendGrid API key not configured, skipping email send");
            return;
        }

        var client = new SendGridClient(apiKey);
        var from = new EmailAddress(fromEmail, fromName);
        var to = new EmailAddress((string)order.Email, (string)order.FullName);
        var subject = $"Order Confirmation - Order #{order.Id}";

        // Build HTML email content
        var htmlContent = BuildEmailHtml(order);

        var msg = MailHelper.CreateSingleEmail(from, to, subject, "", htmlContent);

        try
        {
            var response = await client.SendEmailAsync(msg);
            _logger.LogInformation($"Email sent to {order.Email} - Status: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email via SendGrid");
        }
    }

    private string BuildEmailHtml(dynamic order)
    {
        var orderItems = new StringBuilder();
        if (order.OrderItems != null)
        {
            foreach (var item in order.OrderItems)
            {
                orderItems.Append($@"
                    <tr>
                        <td style='padding: 10px; border-bottom: 1px solid #ddd;'>{item.ProductIdentifier}</td>
                        <td style='padding: 10px; border-bottom: 1px solid #ddd;'>{item.Quantity}</td>
                        <td style='padding: 10px; border-bottom: 1px solid #ddd;'>${item.UnitPrice:F2}</td>
                        <td style='padding: 10px; border-bottom: 1px solid #ddd;'>${item.TotalPrice:F2}</td>
                    </tr>");
            }
        }

        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #777; }}
        table {{ width: 100%; border-collapse: collapse; margin: 20px 0; }}
        th {{ background-color: #4CAF50; color: white; padding: 10px; text-align: left; }}
        td {{ padding: 10px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Order Confirmation</h1>
        </div>
        <div class='content'>
            <h2>Thank you for your order!</h2>
            <p><strong>Order #:</strong> {order.Id}</p>
            <p><strong>Order Date:</strong> {order.OrderDate}</p>

            <h3>Shipping Address:</h3>
            <p>
                {order.FullName}<br/>
                {order.StreetName} {order.HouseNumber}<br/>
                {order.City}, {order.ZipCode}<br/>
                {order.Country}
            </p>

            <h3>Order Items:</h3>
            <table>
                <thead>
                    <tr>
                        <th>Product</th>
                        <th>Quantity</th>
                        <th>Unit Price</th>
                        <th>Total</th>
                    </tr>
                </thead>
                <tbody>
                    {orderItems}
                </tbody>
            </table>

            <h3 style='text-align: right;'>Total Price: ${order.TotalPrice:F2}</h3>
        </div>
        <div class='footer'>
            <p>Thank you for shopping with Cryptocop!</p>
            <p>&copy; 2025 Cryptocop. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
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
