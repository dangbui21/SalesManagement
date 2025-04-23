using OrderService.Domain.Enums;

namespace OrderService.Application.DTOs
{
    public class OrderUpdateDto
    {
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerEmail { get; set; }
        public int Status { get; set; } = (int)OrderStatus.Pending;
        public List<OrderItemDto> Items { get; set; } = new();
    }
}
