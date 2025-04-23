using PaymentService.Domain.Enums;

namespace PaymentService.Application.DTOs
{
    public class PaymentUpdateDto
    {
        public decimal Amount { get; set; }
        public int Status { get; set; } = (int)PaymentStatus.Pending;
    }
}
