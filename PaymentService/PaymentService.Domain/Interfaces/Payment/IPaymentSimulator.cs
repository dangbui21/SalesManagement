using System.Threading.Tasks;

namespace PaymentService.Domain.Interfaces
{
    public interface IPaymentSimulator
    {
        Task<bool> SimulatePaymentSuccessAsync(int orderId);
    }
}
