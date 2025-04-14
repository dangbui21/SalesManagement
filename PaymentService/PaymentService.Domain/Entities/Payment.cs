using System;

namespace PaymentService.Domain.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending"; // e.g. "Succeeded", "Failed"
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }
}
