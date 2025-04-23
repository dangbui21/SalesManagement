namespace OrderService.Domain.Events
{
    public class OrderUpdatedEventDto
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerEmail { get; set; }
        public int Status { get; set; }
        public List<OrderItemEventDto> Items { get; set; } = new List<OrderItemEventDto>();
    }

}
