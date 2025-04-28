using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using PaymentService.Domain.Events;
using PaymentService.Domain.Interfaces;
using PaymentService.Application.Interfaces;
using PaymentService.Application.DTOs;
using PaymentService.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace PaymentService.Infrastructure.MessageBus
{
    public class OrderEventSubscriber : BackgroundService
    {
        private readonly ILogger<OrderEventSubscriber> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        
        private IConnection? _connection;
        private IModel? _channel;

        public OrderEventSubscriber(
            ILogger<OrderEventSubscriber> logger,
            IConfiguration configuration,
            IServiceProvider serviceProvider
         )
        {
            _logger = logger;
            _configuration = configuration;
            _serviceProvider = serviceProvider;

            try
            {

                var host = _configuration["RabbitMQ:HostName"];
                var user = _configuration["RabbitMQ:UserName"];
                var pass = _configuration["RabbitMQ:Password"];
                var consumeQueue = _configuration["RabbitMQ:ConsumeQueue"];
                var exchangeName = "order_exchange";

                var factory = new ConnectionFactory
                {
                    HostName = host,
                    UserName = user,
                    Password = pass
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();


                _channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Topic, durable: true);
                _channel.QueueDeclare(queue: consumeQueue, durable: true, exclusive: false, autoDelete: false);
                _channel.QueueBind(queue: consumeQueue, exchange: exchangeName, routingKey: "order.#");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while setting up RabbitMQ connection or declaring exchange/queue");
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // if (_channel == null)
            // {
            //     _logger.LogError("RabbitMQ channel is null. Cannot consume messages.");
            //     return Task.CompletedTask;
            // }
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                using var scope = _serviceProvider.CreateScope();
                var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();   
                var messageBus = scope.ServiceProvider.GetRequiredService<IMessageBusPublisher>();

                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var routingKey = ea.RoutingKey;

                    _logger.LogInformation("Received message: {RoutingKey} - {Message}", routingKey, message);

                    if (routingKey == "order.created")
                    {
                        var orderCreated = JsonSerializer.Deserialize<OrderCreatedEventDto>(message);

                        if (orderCreated?.Items == null || !orderCreated.Items.Any())
                        {
                            _logger.LogWarning("Empty or invalid order items.");
                            return;
                        }

                        var totalAmount = orderCreated.Items.Sum(i => i.Quantity * i.UnitPrice);
                        var createDto = new PaymentCreateDto
                        {
                            OrderId = orderCreated.OrderId,
                            Amount = totalAmount,
                            Status = (int)PaymentStatus.Pending
                        };

                        var newPaymentId = await paymentService.CreatePaymentAsync(createDto);
                        if (newPaymentId == 0)
                            _logger.LogWarning("Failed to create payment.");
                        else
                            _logger.LogInformation("Created payment for OrderId: {OrderId}, PaymentId: {PaymentId}", orderCreated.OrderId, newPaymentId);
                            _logger.LogInformation("Waiting for manual payment simulation for OrderId: {OrderId}", orderCreated.OrderId);
                    }
                    // _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message.");
                }
                // finally
                // {
                //     // Đảm bảo luôn gọi BasicAck sau khi đã xử lý message
                //     _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                // }
            };

            _channel.BasicConsume(queue: _configuration["RabbitMQ:ConsumeQueue"], autoAck: true, consumer: consumer);

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _logger.LogInformation("Disposing resources. Closing RabbitMQ connection and channel.");
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}