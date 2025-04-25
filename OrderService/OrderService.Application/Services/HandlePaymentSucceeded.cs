using OrderService.Domain.Interfaces;
using OrderService.Domain.Events;

namespace OrderService.Application.Services
{
    public class HandlePaymentSucceeded : IHandlePaymentSucceeded
    {
        private readonly IOrderRepository _orderRepository;

        public HandlePaymentSucceeded(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task HandleAsync(PaymentSucceededEventDto paymentEvent)
        {
            var order = await _orderRepository.GetOrderByIdAsync(paymentEvent.OrderId);
            if (order == null) return;

            order.Status = Domain.Enums.OrderStatus.Completed;

            await _orderRepository.UpdateOrderAsync(order);
        }
    }
}
