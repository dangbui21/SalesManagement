
namespace OrderService.Domain.Events
{
    public class PaymentSucceededEventDto
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaidAt { get; set; }
        public string Status { get; set; } = "Succeeded";
    }
}
