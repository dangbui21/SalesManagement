using PaymentService.Application.DTOs;

namespace PaymentService.Application.Interfaces
{
    public interface IPaymentService
    {
       Task<IEnumerable<PaymentDto>> GetAllPaymentDtosAsync();
        Task<PaymentDto?> GetPaymentDtoByIdAsync(int id);
        Task<IEnumerable<PaymentDto>> GetPaymentsByOrderIdAsync(int orderId);
        Task<int> CreatePaymentAsync(PaymentCreateDto dto);
        Task<bool> UpdatePaymentAsync(int id, PaymentUpdateDto dto);
        Task<bool> DeletePaymentAsync(int id);
    }
}
