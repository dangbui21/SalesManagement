using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using PaymentService.Domain.Events;
using PaymentService.Domain.Interfaces;

namespace PaymentService.Infrastructure.MessageBus
{
    public class OrderEventSubscriber : BackgroundService
    {
        private readonly ILogger<OrderEventSubscriber> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMessageBusPublisher _messageBus;
        private IConnection _connection;
        private IModel _channel;

        public OrderEventSubscriber(
            ILogger<OrderEventSubscriber> logger,
            IConfiguration configuration,
            IMessageBusPublisher messageBus)
        {
            _logger = logger;
            _configuration = configuration;
            _messageBus = messageBus;

            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:HostName"],
                UserName = _configuration["RabbitMQ:UserName"],
                Password = _configuration["RabbitMQ:Password"]
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            var exchangeName = "order_exchange";
            var queueName = _configuration["RabbitMQ:PaymentQueue"];

            _channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Topic, durable: true);
            _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: "order.created");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var routingKey = ea.RoutingKey;

                    _logger.LogInformation("Received message: {RoutingKey} - {Message}", routingKey, message);

                    if (routingKey == "order.created")
                    {
                        var orderCreated = JsonSerializer.Deserialize<OrderCreatedEventDto>(message);

                        if (orderCreated == null)
                        {
                            _logger.LogWarning("Deserialization returned null for order.created");
                            return;
                        }

                        var totalAmount = orderCreated.Items.Sum(item => item.Quantity * item.UnitPrice);

                        var paymentEvent = new PaymentSucceededEventDto
                        {
                            PaymentId = orderCreated.OrderId, // Giả lập dùng OrderId
                            OrderId = orderCreated.OrderId,
                            Amount = totalAmount,
                            PaidAt = DateTime.UtcNow,
                            Status = "Succeeded"
                        };

                        await _messageBus.PublishPaymentSucceededAsync(paymentEvent);
                        _logger.LogInformation("Published PaymentSucceededEvent: {@PaymentEvent}", paymentEvent);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while processing received order event");
                }
            };

            _channel.BasicConsume(queue: _configuration["RabbitMQ:PaymentQueue"], autoAck: true, consumer: consumer);
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}
