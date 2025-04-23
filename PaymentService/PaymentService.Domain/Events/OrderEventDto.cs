namespace PaymentService.Domain.Events
{
    public class OrderCreatedEventDto
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerEmail { get; set; }
        public int Status { get; set; }
        public List<OrderItemEventDto> Items { get; set; } = new List<OrderItemEventDto>();
        public decimal TotalAmount => Items.Sum(x => x.Quantity * x.UnitPrice);
    }

    public class OrderItemEventDto
    {
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class OrderUpdatedEventDto
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerEmail { get; set; }
        public int Status { get; set; }
        public List<OrderItemEventDto> Items { get; set; } = new List<OrderItemEventDto>();
    }

    public class OrderDeletedEventDto
    {
        public int OrderId { get; set; }
    }
}
