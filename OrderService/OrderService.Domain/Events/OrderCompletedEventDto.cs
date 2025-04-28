namespace OrderService.Domain.Events
{
    public class OrderCompletedEventDto
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerEmail { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderItemEventDto> Items { get; set; } = new List<OrderItemEventDto>();
        public decimal TotalAmount => Items.Sum(x => x.Quantity * x.UnitPrice); 
    }

}
