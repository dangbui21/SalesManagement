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
            var queueName = _configuration["RabbitMQ:PublishQueue"];

            _channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Topic, durable: true);
            _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: "payment.#");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var routingKey = ea.RoutingKey;
                    var deliveryTag = ea.DeliveryTag;

                    _logger.LogInformation("[Payment Subscriber] Received message - DeliveryTag: {DeliveryTag}, RoutingKey: {RoutingKey}", 
                        deliveryTag, routingKey);
                    _logger.LogInformation("[Payment Subscriber] Message content: {Message}", message);

                    switch (routingKey)
                    {
                        case "payment.created":
                            var created = JsonSerializer.Deserialize<PaymentCreatedEventDto>(message);
                            if (created == null)
                            {
                                _logger.LogError("[Payment Subscriber] Failed to deserialize payment.created message");
                                _channel.BasicNack(ea.DeliveryTag, false, true);
                                return;
                            }
                            _logger.LogInformation("[Payment Subscriber] Processing payment.created - PaymentId: {PaymentId}, OrderId: {OrderId}", 
                                created.PaymentId, created.OrderId);
                            _logger.LogInformation("[Payment Subscriber] Payment Created: {@Payment}", created);
                            break;

                        case "payment.updated":
                            var updated = JsonSerializer.Deserialize<PaymentUpdatedEventDto>(message);
                            if (updated == null)
                            {
                                _logger.LogError("[Payment Subscriber] Failed to deserialize payment.updated message");
                                _channel.BasicNack(ea.DeliveryTag, false, true);
                                return;
                            }
                            _logger.LogInformation("[Payment Subscriber] Processing payment.updated - PaymentId: {PaymentId}", 
                                updated.PaymentId);
                            _logger.LogInformation("[Payment Subscriber] Payment Updated: {@Payment}", updated);
                            break;

                        case "payment.deleted":
                            var deleted = JsonSerializer.Deserialize<PaymentDeletedEventDto>(message);
                            if (deleted == null)
                            {
                                _logger.LogError("[Payment Subscriber] Failed to deserialize payment.deleted message");
                                _channel.BasicNack(ea.DeliveryTag, false, true);
                                return;
                            }
                            _logger.LogInformation("[Payment Subscriber] Processing payment.deleted - PaymentId: {PaymentId}", 
                                deleted.PaymentId);
                            _logger.LogInformation("[Payment Subscriber] Payment Deleted: {@Payment}", deleted);
                            break;

                        case "payment.failed":
                            var failed = JsonSerializer.Deserialize<PaymentFailedEventDto>(message);
                            if (failed == null)
                            {
                                _logger.LogError("[Payment Subscriber] Failed to deserialize payment.failed message");
                                _channel.BasicNack(ea.DeliveryTag, false, true);
                                return;
                            }
                            _logger.LogWarning("[Payment Subscriber] Processing payment.failed - PaymentId: {PaymentId}", 
                                failed.PaymentId);
                            _logger.LogWarning("[Payment Subscriber] Payment Failed: {@Payment}", failed);
                            break;

                        case "payment.succeeded":  
                            var succeeded = JsonSerializer.Deserialize<PaymentSucceededEventDto>(message);
                            if (succeeded == null)
                            {
                                _logger.LogError("[Payment Subscriber] Failed to deserialize payment.succeeded message");
                                _channel.BasicNack(ea.DeliveryTag, false, true);
                                return;
                            }
                            _logger.LogInformation("[Payment Subscriber] Processing payment.succeeded - PaymentId: {PaymentId}", 
                                succeeded.PaymentId);
                            _logger.LogInformation("[Payment Subscriber] Payment Succeeded: {@Payment}", succeeded);
                            break;

                        default:
                            _logger.LogWarning("[Payment Subscriber] Unknown Payment RoutingKey: {RoutingKey}", routingKey);
                            _channel.BasicNack(ea.DeliveryTag, false, false);
                            return;
                    }

                    _logger.LogInformation("[Payment Subscriber] Successfully processed message - DeliveryTag: {DeliveryTag}", deliveryTag);
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Payment Subscriber] Error processing message");
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsume(
                queue: _configuration["RabbitMQ:PublishQueue"],
                autoAck: false,
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
