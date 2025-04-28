using OrderService.Domain.Interfaces;
using OrderService.Domain.Events;

namespace OrderService.Application.Services

{
    public class HandlePaymentSucceeded : IHandlePaymentSucceeded
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMessageBusPublisher _messageBusPublisher;
       

        public HandlePaymentSucceeded(IOrderRepository orderRepository, IMessageBusPublisher messageBusPublisher)
        {
            _orderRepository = orderRepository;
            _messageBusPublisher = messageBusPublisher;
        }

        public async Task HandleAsync(PaymentSucceededEventDto paymentEvent)
        {
            var order = await _orderRepository.GetOrderByIdAsync(paymentEvent.OrderId);
            if (order == null) return;

            order.Status = Domain.Enums.OrderStatus.Completed;

            await _orderRepository.UpdateOrderAsync(order);

            var orderCompletedEvent = new OrderCompletedEventDto
            {
                OrderId = order.Id,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                Status = (int)order.Status,
                CreatedAt = order.CreatedAt,
                Items = order.Items.Select(i => new OrderItemEventDto
                {
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };

            await _messageBusPublisher.PublishOrderCompletedAsync(orderCompletedEvent);
        }
    }
}
