using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using OrderService.Domain.Events;

namespace OrderService.Infrastructure.MessageBus
{
    public class RabbitMQMessageBusSubscriber : BackgroundService
    {
        private readonly ILogger<RabbitMQMessageBusSubscriber> _logger;
        private readonly IConfiguration _configuration;
        private IConnection _connection;
        private IModel _channel;

        public RabbitMQMessageBusSubscriber(ILogger<RabbitMQMessageBusSubscriber> logger, IConfiguration configuration)
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

            var exchangeName = "order_exchange";
            var queueName = _configuration["RabbitMQ:QueueName"];

            // Declare exchange và queue
            _channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Topic, durable: true);
            _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: "order.#");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;

                _logger.LogInformation("Received message: {RoutingKey} - {Message}", routingKey, message);

                // Xử lý logic phân loại
                switch (routingKey)
                {
                    case "order.created":
                        var createdOrder = JsonSerializer.Deserialize<OrderCreatedEventDto>(message);
                        _logger.LogInformation("New Order Created: {@Order}", createdOrder);
                        break;

                    case "order.updated":
                        var updatedOrder = JsonSerializer.Deserialize<OrderUpdatedEventDto>(message);
                        _logger.LogInformation("Order Updated: {@Order}", updatedOrder);
                        break;

                    case "order.deleted":
                        var deletedOrder = JsonSerializer.Deserialize<OrderDeletedEventDto>(message);
                        _logger.LogInformation("Order Deleted: {@Order}", deletedOrder);
                        break;

                    default:
                        _logger.LogWarning("Unknown RoutingKey: {RoutingKey}", routingKey);
                        break;
                }
            };

            _channel.BasicConsume(queue: _configuration["RabbitMQ:QueueName"], autoAck: true, consumer: consumer);

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}
