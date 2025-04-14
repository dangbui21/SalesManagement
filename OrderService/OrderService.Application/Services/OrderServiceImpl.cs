using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;

namespace OrderService.Application.Services
{
    public class OrderServiceImpl : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderServiceImpl(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _orderRepository.GetAllOrdersAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            return await _orderRepository.GetOrderByIdAsync(id);
        }

        public async Task<int> CreateOrderAsync(Order order)
        {
            await _orderRepository.CreateOrderAsync(order);
            return order.Id;
        }
        public async Task<bool> UpdateOrderAsync(int id, OrderUpdateDto orderUpdateDto)
        {
            var order = await _orderRepository.GetOrderByIdAsync(id);
            if (order == null)
                return false;

            order.CustomerName = orderUpdateDto.CustomerName;
            order.CustomerEmail = orderUpdateDto.CustomerEmail;
            order.Status = (OrderStatus)orderUpdateDto.Status;

            order.Items.Clear();
            foreach (var itemDto in orderUpdateDto.Items)
            {
                order.Items.Add(new OrderItem
                {
                    ProductName = itemDto.ProductName,
                    Quantity = itemDto.Quantity,
                    UnitPrice = itemDto.UnitPrice
                });
            }

            return await _orderRepository.UpdateOrderAsync(order);
        }

        public async Task<IEnumerable<OrderDto>> GetAllOrderDtosAsync()
        {
            var orders = await _orderRepository.GetAllOrdersAsync();

            return orders.Select(order => new OrderDto
            {
                Id = order.Id,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                CreatedAt = order.CreatedAt,
                Status = order.Status.ToString(),
                Items = order.Items.Select(item => new OrderItemDto
                {
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                }).ToList(),
                TotalAmount = order.Items.Sum(item => item.UnitPrice * item.Quantity)
            });
        }

        public async Task<OrderDto?> GetOrderDtoByIdAsync(int id)
        {
            var order = await _orderRepository.GetOrderByIdAsync(id);
            if (order == null) return null;

            return new OrderDto
            {
                Id = order.Id,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                CreatedAt = order.CreatedAt,
                Status = order.Status.ToString(),
                Items = order.Items.Select(item => new OrderItemDto
                {
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                }).ToList(),
                TotalAmount = order.Items.Sum(item => item.UnitPrice * item.Quantity)
            };
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            var order = await _orderRepository.GetOrderByIdAsync(id);
            if (order == null)
                return false;

            return await _orderRepository.DeleteOrderAsync(order);
        }


    }
}
