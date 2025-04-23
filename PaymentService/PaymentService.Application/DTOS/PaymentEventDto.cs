using System;

namespace PaymentService.Application.DTOs
{
    public class PaymentEventDto
    {
        public int PaymentId { get; set; }
        public string EventType { get; set; } = string.Empty; // Ví dụ: "PaymentSucceeded", "PaymentFailed"
        public string? Message { get; set; } // Thông điệp mô tả chi tiết về sự kiện
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
