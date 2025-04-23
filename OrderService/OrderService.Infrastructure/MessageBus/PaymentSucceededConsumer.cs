using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using OrderService.Application.Interfaces;
using OrderService.Domain.Events;

public class PaymentSucceededConsumer : BackgroundService
{
    private readonly ILogger<PaymentSucceededConsumer> _logger;
    private readonly IOrderService _orderService;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _queueName = "order_update_queue";

    public PaymentSucceededConsumer(ILogger<PaymentSucceededConsumer> logger, IOrderService orderService, IConfiguration configuration)
    {
        _logger = logger;
        _orderService = orderService;

        var factory = new ConnectionFactory()
        {
            HostName = configuration["RabbitMQ:HostName"],
            UserName = configuration["RabbitMQ:UserName"],
            Password = configuration["RabbitMQ:Password"]
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Khai báo queue
        _channel.QueueDeclare(
            queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        // Bind queue với exchange
        _channel.QueueBind(
            queue: _queueName,
            exchange: "payment_exchange",
            routingKey: "payment.succeeded"
        );
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            var paymentEvent = JsonSerializer.Deserialize<PaymentSucceededEventDto>(message);
            _logger.LogInformation("Received payment.succeeded for OrderId: {OrderId}", paymentEvent.OrderId);

            await _orderService.UpdateOrderPaymentStatusAsync(paymentEvent.OrderId, paymentEvent.Status);
        };

        _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel.Close();
        _connection.Close();
        base.Dispose();
    }
}
