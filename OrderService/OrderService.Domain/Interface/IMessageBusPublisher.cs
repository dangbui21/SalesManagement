using OrderService.Domain.Events;

namespace OrderService.Domain.Interfaces
{
    public interface IMessageBusPublisher
    {
        Task PublishOrderCreatedAsync(OrderCreatedEventDto orderEvent);
        Task PublishOrderUpdatedAsync(OrderUpdatedEventDto orderEvent);
        Task PublishOrderDeletedAsync(OrderDeletedEventDto orderEvent);
    }
}
