using System;
using System.Collections.Generic;

namespace OrderService.Infrastructure.MessageBus.Events
{
    public class OrderCreatedEvent
    {
        public Guid OrderId { get; set; }  // ID của đơn hàng
        public string CustomerName { get; set; }  // Tên khách hàng
        public string? CustomerEmail { get; set; }  // Email khách hàng
        public int Status { get; set; }  // Trạng thái của đơn hàng
        public List<OrderItemEventDto> Items { get; set; }  // Các item trong đơn hàng

        public OrderCreatedEvent(Guid orderId, string customerName, string? customerEmail, int status, List<OrderItemEventDto> items)
        {
            OrderId = orderId;
            CustomerName = customerName;
            CustomerEmail = customerEmail;
            Status = status;
            Items = items;
        }
    }

    public class OrderItemEventDto
    {
        public string ProductName { get; set; }  // Tên sản phẩm
        public int Quantity { get; set; }  // Số lượng
        public decimal UnitPrice { get; set; }  // Giá đơn vị của sản phẩm

        public OrderItemEventDto(string productName, int quantity, decimal unitPrice)
        {
            ProductName = productName;
            Quantity = quantity;
            UnitPrice = unitPrice;
        }
    }
}
