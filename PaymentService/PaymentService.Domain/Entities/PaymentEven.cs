using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PaymentService.Domain.Enums;

namespace PaymentService.Domain.Entities
{
    public class PaymentEvent
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int PaymentId { get; set; }
        public PaymentEventType EventType { get; set; } = PaymentEventType.Undefined; // ex: "PaymentSucceeded", "PaymentFailed"
        public string? Message { get; set; } // log chi tiết hoặc mô tả lỗi
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Payment Payment { get; set; } = null!;

    }
}
