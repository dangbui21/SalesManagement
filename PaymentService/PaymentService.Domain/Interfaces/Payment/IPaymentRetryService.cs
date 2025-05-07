using System.Threading.Tasks;

namespace PaymentService.Domain.Interfaces
{
    public interface IPaymentRetryService
    {
        Task StartRetryForPaymentAsync(int paymentId, int orderId);
        Task StopRetryForPaymentAsync(int paymentId);
    }
} 