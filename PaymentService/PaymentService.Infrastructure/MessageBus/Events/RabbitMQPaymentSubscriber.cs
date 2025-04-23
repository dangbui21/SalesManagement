using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using PaymentService.Domain.Events;

namespace PaymentService.Infrastructure.MessageBus
{
    public class RabbitMQPaymentSubscriber : BackgroundService
    {
        private readonly ILogger<RabbitMQPaymentSubscriber> _logger;
        private readonly IConfiguration _configuration;
        private IConnection _connection;
        private IModel _channel;

        public RabbitMQPaymentSubscriber(ILogger<RabbitMQPaymentSubscriber> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQ:HostName"],
                UserName = _configuration["RabbitMQ:UserName"],
                Password = _configuration["RabbitMQ:Password"]
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            var exchangeName = "payment_exchange";
            var queueName = _configuration["RabbitMQ:QueueName"];

            _channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Topic, durable: true);
            _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: "payment.#");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;

                _logger.LogInformation("Received Payment Event: {RoutingKey} - {Message}", routingKey, message);

                switch (routingKey)
                {
                    case "payment.created":
                        var created = JsonSerializer.Deserialize<PaymentCreatedEventDto>(message);
                        _logger.LogInformation("Payment Created: {@Payment}", created);
                        break;

                    case "payment.updated":
                        var updated = JsonSerializer.Deserialize<PaymentUpdatedEventDto>(message);
                        _logger.LogInformation("Payment Updated: {@Payment}", updated);
                        break;

                    case "payment.deleted":
                        var deleted = JsonSerializer.Deserialize<PaymentDeletedEventDto>(message);
                        _logger.LogInformation("Payment Deleted: {@Payment}", deleted);
                        break;

                    case "payment.failed":
                        var failed = JsonSerializer.Deserialize<PaymentFailedEventDto>(message);
                        _logger.LogWarning("Payment Failed: {@Payment}", failed);
                        break;

                    default:
                        _logger.LogWarning("Unknown Payment RoutingKey: {RoutingKey}", routingKey);
                        break;
                }
            };

            _channel.BasicConsume(
                queue: _configuration["RabbitMQ:QueueName"],
                autoAck: true,
                consumer: consumer
            );

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
