using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ReportService.Domain.Events;
using ReportService.Domain.Interfaces;

namespace ReportService.Infrastructure.MessageConsumers
{
    public class OrderCompletedConsumer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OrderCompletedConsumer> _logger;
        private IConnection _connection;
        private IModel _channel;

        public OrderCompletedConsumer(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<OrderCompletedConsumer> logger)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _logger = logger;
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:HostName"],
                UserName = _configuration["RabbitMQ:UserName"],
                Password = _configuration["RabbitMQ:Password"]
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            var queueName = _configuration["RabbitMQ:ConsumeQueue"]; // Lấy tên hàng đợi từ cấu hình
            var exchangeName = "order_exchange"; // Bạn có thể sử dụng exchange nếu cần

            // Declare exchange nếu cần routing mở rộng
            _channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Topic, durable: true);

            // Khai báo queue và bind với exchange
            _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: "order.completed");

            _logger.LogInformation("✅ OrderCompletedConsumer initialized, listening on queue: {QueueName}", queueName);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;

                _logger.LogInformation("✅ Received message from queue: {QueueName}, routingKey: {RoutingKey}, message: {Message}", _configuration["RabbitMQ:ConsumeQueue"], routingKey, message);

                // Log nhận được tin nhắn
                Console.WriteLine($"[Received] RoutingKey: {routingKey}, Message: {message}");

                if (routingKey == "order.completed")
                {
                    try
                    {
                        var orderEvent = JsonSerializer.Deserialize<OrderCompletedEvent>(message);

                        if (orderEvent != null)
                        {
                            // Log trước khi xử lý
                            Console.WriteLine($"[Handling] OrderId: {orderEvent.OrderId}");

                            using var scope = _serviceProvider.CreateScope();
                            var handler = scope.ServiceProvider.GetRequiredService<IHandleOrderCompleted>();
                            await handler.HandleAsync(orderEvent); // Gọi handler để xử lý
                        }
                        else
                        {
                        _logger.LogWarning("⚠️ Deserialization returned null for message: {Message}", message);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("❌ Error processing message: {Message}, Exception: {Exception}", message, ex.Message);
                    }   
                }

                _channel.BasicAck(ea.DeliveryTag, false); // Thông báo nhận thành công
            };

            _channel.BasicConsume(queue: _configuration["RabbitMQ:ConsumeQueue"], autoAck: false, consumer: consumer);

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
