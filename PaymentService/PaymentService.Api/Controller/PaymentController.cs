using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using AutoMapper;
using PaymentService.Domain.Entities;

namespace PaymentService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IMapper _mapper;

        public PaymentController(IPaymentService paymentService, IMapper mapper)
        {
            _paymentService = paymentService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetAllPayments()
        {
            var payments = await _paymentService.GetAllPaymentDtosAsync();
            return Ok(payments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentDto>> GetPaymentById(int id)
        {
            var payment = await _paymentService.GetPaymentDtoByIdAsync(id);
            if (payment == null)
                return NotFound("Không tìm thấy thanh toán.");

            return Ok(payment);
        }

        [HttpGet("order/{orderId}")]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPaymentsByOrderId(int orderId)
        {
            var payments = await _paymentService.GetPaymentsByOrderIdAsync(orderId);
            return Ok(payments);
        }

        [HttpPost]
public async Task<ActionResult> CreatePayment([FromBody] PaymentCreateDto paymentCreateDto)
{
    // Tạo thanh toán và lấy ID
    int newPaymentId = await _paymentService.CreatePaymentAsync(paymentCreateDto);

    // Lấy PaymentDto từ ID vừa tạo
    var paymentDto = await _paymentService.GetPaymentDtoByIdAsync(newPaymentId);
    if (paymentDto == null)
        return NotFound("Không tìm thấy thanh toán vừa tạo.");

    // Trả về PaymentDto
    return CreatedAtAction(nameof(GetPaymentById), new { id = newPaymentId }, paymentDto);
}

[HttpPut("{id}")]
public async Task<IActionResult> UpdatePayment(int id, [FromBody] PaymentUpdateDto paymentUpdateDto)
{
    // Cập nhật thanh toán
    var result = await _paymentService.UpdatePaymentAsync(id, paymentUpdateDto);
    if (!result)
        return NotFound("Không tìm thấy thanh toán để cập nhật.");

    // Lấy PaymentDto sau khi cập nhật
    var updatedPayment = await _paymentService.GetPaymentDtoByIdAsync(id);
    if (updatedPayment == null)
        return NotFound("Không tìm thấy thanh toán sau khi cập nhật.");

    return Ok(updatedPayment); // Trả về PaymentDto đã được cập nhật
}



        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePayment(int id)
        {
            var result = await _paymentService.DeletePaymentAsync(id);
            if (!result)
                return NotFound("Không tìm thấy thanh toán để xóa.");

            return NoContent();
        }
    }
}
