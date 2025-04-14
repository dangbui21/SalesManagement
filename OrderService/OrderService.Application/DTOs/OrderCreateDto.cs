using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrderService.Domain.Enums;

namespace OrderService.Application.DTOs
{
    public class OrderCreateDto
    {
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerEmail { get; set; }
        public int Status { get; set; } = (int)OrderStatus.Pending;
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
    }

}
