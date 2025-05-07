using System;
using PaymentService.Domain.Enums;

namespace PaymentService.Application.DTOs.Payment
{
    public class PaymentCreateDto
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public int Status { get; set; } = (int)PaymentStatus.Pending;
    }
}
