using PaymentService.Domain.Events;

namespace PaymentService.Domain.Interfaces
{
    public interface IMessageBusPublisher
    {
        Task PublishPaymentCreatedAsync(PaymentCreatedEventDto paymentEvent);
        Task PublishPaymentUpdatedAsync(PaymentUpdatedEventDto paymentEvent);
        Task PublishPaymentFailedAsync(PaymentFailedEventDto paymentEvent);
        Task PublishPaymentDeletedAsync(PaymentDeletedEventDto paymentEvent);
        Task PublishPaymentSucceededAsync(PaymentSucceededEventDto paymentEvent);
    }
}
