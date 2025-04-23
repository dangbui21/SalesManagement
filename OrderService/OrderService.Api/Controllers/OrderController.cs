using Microsoft.AspNetCore.Mvc;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrderService.Domain.Interfaces;

namespace OrderService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly IOrderService _orderService;

        public OrderController(IOrderRepository orderRepository, IMapper mapper, IOrderService orderService)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            _orderService = orderService;
        }

        // GET: api/order
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrderDtosAsync();
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrderById(int id)
        {
            var order = await _orderService.GetOrderDtoByIdAsync(id);
            if (order == null)
                return NotFound("Không tìm thấy đơn hàng.");
            return Ok(order);
        }

        [HttpPost]
        public async Task<ActionResult> CreateOrder([FromBody] OrderCreateDto orderCreateDto)
        {
            var order = _mapper.Map<Order>(orderCreateDto);
            int orderId = await _orderService.CreateOrderAsync(order);
            return CreatedAtAction(nameof(GetOrderById), new { id = orderId }, orderId);
        }
        // PUT: api/order/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateOrder(int id, [FromBody] OrderUpdateDto orderUpdateDto)
        {
            var result = await _orderService.UpdateOrderAsync(id, orderUpdateDto);

            if (!result)
                return NotFound("Không tìm thấy đơn hàng hoặc cập nhật thất bại.");

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var result = await _orderService.DeleteOrderAsync(id);
            if (!result)
                return NotFound("Không tìm thấy đơn hàng để xóa.");

            return NoContent();
        }
    }
}
