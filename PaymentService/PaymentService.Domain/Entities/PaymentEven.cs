using System;

namespace PaymentService.Domain.Entities
{
    public class PaymentEvent
    {
        public int Id { get; set; }
        public int PaymentId { get; set; }
        public string EventType { get; set; } = string.Empty; // ex: "PaymentSucceeded", "PaymentFailed"
        public string? Message { get; set; } // log chi tiết hoặc mô tả lỗi
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
