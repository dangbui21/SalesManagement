namespace ReportService.Domain.Events
{
    public class OrderItemEvent
    {
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
    public class OrderCompletedEvent
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerEmail { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderItemEvent> Items { get; set; } = new List<OrderItemEvent>();
        public decimal TotalAmount { get; set; }
    }

}
