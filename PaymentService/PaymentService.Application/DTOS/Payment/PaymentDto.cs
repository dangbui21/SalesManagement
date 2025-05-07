using System;
using PaymentService.Domain.Enums;

namespace PaymentService.Application.DTOs.Payment
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }
}
