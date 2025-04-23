using System;
using PaymentService.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentService.Domain.Entities
{
    public class Payment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending; // e.g. "Succeeded", "Failed"
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

        public ICollection<PaymentEvent> PaymentEvents { get; set; } = new List<PaymentEvent>();
    }
}
