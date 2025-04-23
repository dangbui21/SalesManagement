using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using OrderService.Domain.Events;
using OrderService.Domain.Interfaces;
using RabbitMQ.Client;

namespace OrderService.Infrastructure.MessageBus
{
    public class RabbitMQMessageBusPublisher : IMessageBusPublisher
    {
        private readonly ILogger<RabbitMQMessageBusPublisher> _logger;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IConfiguration _configuration;
        private readonly string _exchangeName = "order_exchange";

        public RabbitMQMessageBusPublisher(
            ILogger<RabbitMQMessageBusPublisher> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            var hostName = _configuration["RabbitMQ:HostName"];
            var userName = _configuration["RabbitMQ:UserName"];
            var password = _configuration["RabbitMQ:Password"];
            var queueName = _configuration["RabbitMQ:QueueName"];

            var factory = new ConnectionFactory()
            {
                HostName = hostName,
                UserName = userName,
                Password = password
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Khai báo exchange
            _channel.ExchangeDeclare(
                exchange: _exchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                arguments: null
            );

            // Khai báo queue
            _channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            // Bind Queue tới Exchange
            _channel.QueueBind(
                queue: queueName,
                exchange: _exchangeName,
                routingKey: "order.#",
                arguments: null
            );
        }

        public Task PublishOrderCreatedAsync(OrderCreatedEventDto orderEvent)
            => PublishAsync("order.created", JsonSerializer.Serialize(orderEvent));

        public Task PublishOrderUpdatedAsync(OrderUpdatedEventDto orderEvent)
            => PublishAsync("order.updated", JsonSerializer.Serialize(orderEvent));

        public Task PublishOrderDeletedAsync(OrderDeletedEventDto orderEvent)
            => PublishAsync("order.deleted", JsonSerializer.Serialize(orderEvent));

        private async Task PublishAsync(string routingKey, string message)
        {
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(
                exchange: _exchangeName,
                routingKey: routingKey,
                basicProperties: null,
                body: body
            );

            _logger.LogInformation("Published to {Exchange} with RoutingKey {RoutingKey}", _exchangeName, routingKey);
            await Task.CompletedTask;
        }
    }
}
