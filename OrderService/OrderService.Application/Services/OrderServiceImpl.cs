using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Events;
using OrderService.Domain.Interfaces;

namespace OrderService.Application.Services

{
    public class OrderServiceImpl : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        private readonly IMessageBusPublisher _messageBusPublisher;

        public OrderServiceImpl(IOrderRepository orderRepository, IMessageBusPublisher messageBusPublisher)
        {
            _orderRepository = orderRepository;
            _messageBusPublisher = messageBusPublisher;
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
            var orderEvent = new OrderCreatedEventDto
            {
                OrderId = order.Id,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                Status = (int)order.Status,
                Items = order.Items.Select(i => new OrderItemEventDto
                {
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };

            await _messageBusPublisher.PublishOrderCreatedAsync(orderEvent);
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

            var isUpdated = await _orderRepository.UpdateOrderAsync(order);
            if (isUpdated)
            {
                // Gửi sự kiện OrderUpdatedEvent qua Message Bus
                var orderEvent = new OrderUpdatedEventDto
                {
                    OrderId = order.Id,
                    CustomerName = order.CustomerName,
                    CustomerEmail = order.CustomerEmail,
                    Status = (int)order.Status,
                    Items = order.Items.Select(i => new OrderItemEventDto
                    {
                        ProductName = i.ProductName,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice
                    }).ToList()
                };

                await _messageBusPublisher.PublishOrderUpdatedAsync(orderEvent);
            }

            return isUpdated;
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            var order = await _orderRepository.GetOrderByIdAsync(id);
            if (order == null)
                return false;

            var isDeleted = await _orderRepository.DeleteOrderAsync(order);
            if (isDeleted)
            {
                // Gửi sự kiện OrderDeletedEvent qua Message Bus
                var orderEvent = new OrderDeletedEventDto
                {
                    OrderId = order.Id
                };

                await _messageBusPublisher.PublishOrderDeletedAsync(orderEvent);
            }

            return isDeleted;
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
        // }
        // public async Task<bool> UpdateOrderPaymentStatusAsync(int orderId, string status)
        // {
        //     var order = await _orderRepository.GetOrderByIdAsync(orderId);
        //     if (order == null)
        //         return false;

        //     if (Enum.TryParse(status, out OrderStatus parsedStatus))
        //     {
        //         order.Status = parsedStatus;
        //     }
        //     else
        //     {
        //         // fallback: giả sử status từ Payment là "Succeeded" thì chuyển sang OrderStatus.Paid
        //         order.Status = OrderStatus.Completed;
        //     }

        //     return await _orderRepository.UpdateOrderAsync(order);
        }


    }
}
