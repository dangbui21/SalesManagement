using OrderService.Domain.Events;
using System.Threading.Tasks;

namespace OrderService.Domain.Interfaces
{
    public interface IHandlePaymentSucceeded
    {
        Task HandleAsync(PaymentSucceededEventDto paymentEvent);
        
    }
}
