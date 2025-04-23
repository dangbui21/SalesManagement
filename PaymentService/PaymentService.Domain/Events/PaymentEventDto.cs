namespace PaymentService.Domain.Events
{
    public class PaymentCreatedEventDto
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }

    public class PaymentUpdatedEventDto
    {
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class PaymentFailedEventDto
    {
        public int PaymentId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime FailedAt { get; set; } = DateTime.UtcNow;
    }

    public class PaymentDeletedEventDto
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public DateTime DeletedAt { get; set; } = DateTime.UtcNow;
    }
}
