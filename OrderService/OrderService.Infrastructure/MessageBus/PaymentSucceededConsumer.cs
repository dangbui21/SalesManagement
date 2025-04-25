using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderService.Domain.Events;
using OrderService.Domain.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace OrderService.Infrastructure.MessageBus
{
    public class PaymentSucceededConsumer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private IConnection? _connection;
        private IModel _channel;

        public PaymentSucceededConsumer(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;

            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:HostName"],
                UserName = _configuration["RabbitMQ:UserName"],
                Password = _configuration["RabbitMQ:Password"]
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            var queueName = _configuration["RabbitMQ:ConsumeQueue"];
            var exchangeName = "payment_exchange";

            // Declare exchange nếu cần routing mở rộng
            _channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Topic, durable: true);

            // Khai báo queue và bind với exchange
            _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: "payment.#");

            Console.WriteLine("[Startup] PaymentSucceededConsumer started");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;

                // Log nhận được tin nhắn
                Console.WriteLine($"[Received] RoutingKey: {routingKey}, Message: {message}");

                if (routingKey == "payment.succeeded")
                {
                    var paymentEvent = JsonSerializer.Deserialize<PaymentSucceededEventDto>(message);

                    if (paymentEvent != null)
                    {
                        // Log trước khi xử lý
                        Console.WriteLine($"[Handling] OrderId: {paymentEvent.OrderId}");

                        using var scope = _serviceProvider.CreateScope();
                        var handler = scope.ServiceProvider.GetRequiredService<IHandlePaymentSucceeded>();
                        await handler.HandleAsync(paymentEvent);
                    }
                }

                _channel.BasicAck(ea.DeliveryTag, false);
            };

            var queueName = _configuration["RabbitMQ:ConsumeQueue"];
            _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);

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
