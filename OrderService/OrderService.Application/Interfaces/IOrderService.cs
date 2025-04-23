using OrderService.Application.DTOs;
using OrderService.Domain.Entities;

namespace OrderService.Application.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<Order?> GetOrderByIdAsync(int id);
        Task<int> CreateOrderAsync(Order order);
        Task<bool> UpdateOrderAsync(int id, OrderUpdateDto orderUpdateDto);
        Task<IEnumerable<OrderDto>> GetAllOrderDtosAsync();
        Task<OrderDto?> GetOrderDtoByIdAsync(int id);
        Task<bool> DeleteOrderAsync(int id);
        Task<bool> UpdateOrderPaymentStatusAsync(int orderId, string status);

    }
}
