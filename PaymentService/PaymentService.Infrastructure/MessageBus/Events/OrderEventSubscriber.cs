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
using PaymentService.Application.DTOs.Payment;
using PaymentService.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using PaymentService.Application.Services;

namespace PaymentService.Infrastructure.MessageBus.Events
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
                _channel.QueueBind(queue: consumeQueue, exchange: exchangeName, routingKey: "payment.succeeded");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while setting up RabbitMQ connection or declaring exchange/queue");
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                using var scope = _serviceProvider.CreateScope();
                var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();   
                var messageBus = scope.ServiceProvider.GetRequiredService<IMessageBusPublisher>();
                var notificationService = scope.ServiceProvider.GetRequiredService<PaymentNotificationService>();

                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var routingKey = ea.RoutingKey;
                    var deliveryTag = ea.DeliveryTag;

                    _logger.LogInformation("[Message Processing] Starting to process message - DeliveryTag: {DeliveryTag}, RoutingKey: {RoutingKey}", deliveryTag, routingKey);
                    _logger.LogInformation("[Message Content] {Message}", message);

                    if (routingKey == "order.created")
                    {
                        var orderCreated = JsonSerializer.Deserialize<OrderCreatedEventDto>(message);
                        _logger.LogInformation("[Order Created] Deserialized order data - OrderId: {OrderId}, Items Count: {ItemsCount}", 
                            orderCreated?.OrderId, orderCreated?.Items?.Count);

                        if (orderCreated?.Items == null || !orderCreated.Items.Any())
                        {
                            _logger.LogWarning("[Order Created] Empty or invalid order items for OrderId: {OrderId}", orderCreated?.OrderId);
                            _channel.BasicAck(ea.DeliveryTag, false);
                            return;
                        }

                        var totalAmount = orderCreated.Items.Sum(i => i.Quantity * i.UnitPrice);
                        _logger.LogInformation("[Order Created] Calculated total amount: {TotalAmount} for OrderId: {OrderId}", 
                            totalAmount, orderCreated.OrderId);

                        var createDto = new PaymentCreateDto
                        {
                            OrderId = orderCreated.OrderId,
                            Amount = totalAmount,
                            Status = (int)PaymentStatus.Pending
                        };

                        _logger.LogInformation("[Payment Creation] Attempting to create payment for OrderId: {OrderId}", orderCreated.OrderId);
                        var newPaymentId = await paymentService.CreatePaymentAsync(createDto);
                        
                        if (newPaymentId == 0)
                        {
                            _logger.LogWarning("[Payment Creation] Failed to create payment for OrderId: {OrderId}", orderCreated.OrderId);
                        }
                        else
                        {
                            _logger.LogInformation("[Payment Creation] Successfully created payment - OrderId: {OrderId}, PaymentId: {PaymentId}", 
                                orderCreated.OrderId, newPaymentId);
                            _logger.LogInformation("[Payment Creation] Waiting for manual payment simulation for OrderId: {OrderId}", 
                                orderCreated.OrderId);
                        }
                    }
                    else if (routingKey == "order.completed")
                    {
                        var orderCompleted = JsonSerializer.Deserialize<OrderCompletedEventDto>(message);
                        _logger.LogInformation("[Order Completed] Processing completion for OrderId: {OrderId}", 
                            orderCompleted?.OrderId);

                        if (orderCompleted != null)
                        {
                            _logger.LogInformation("[Payment Update] Attempting to mark payment as completed for OrderId: {OrderId}", 
                                orderCompleted.OrderId);
                            var updated = await paymentService.MarkPaymentAsCompletedAsync(orderCompleted.OrderId);
                            
                            if (updated)
                            {
                                _logger.LogInformation("[Payment Update] Successfully marked payment as Completed for OrderId: {OrderId}", 
                                    orderCompleted.OrderId);
                            }
                            else
                            {
                                _logger.LogWarning("[Payment Update] No payment found to mark as completed for OrderId: {OrderId}", 
                                    orderCompleted.OrderId);
                            }
                        }
                    }
                    else if (routingKey == "payment.succeeded")
                    {
                        _logger.LogInformation("[Payment Succeeded] Received payment.succeeded message: {Message}", message);
                    }

                    _logger.LogInformation("[Message Processing] Successfully processed message - DeliveryTag: {DeliveryTag}", deliveryTag);
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Message Processing] Error processing message - DeliveryTag: {DeliveryTag}", ea.DeliveryTag);
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            _channel.BasicConsume(queue: _configuration["RabbitMQ:ConsumeQueue"], autoAck: false, consumer: consumer);

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