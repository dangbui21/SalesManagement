using AutoMapper;
using PaymentService.Application.DTOs;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Interfaces;
using PaymentService.Domain.Events;

namespace PaymentService.Application.Services
{
    public class PaymentServiceImpl : IPaymentService
    {
        private readonly IPaymentRepository _repository;
        private readonly IMapper _mapper;
        private readonly IMessageBusPublisher _messageBus;

        public PaymentServiceImpl(IPaymentRepository repository, IMapper mapper, IMessageBusPublisher messageBus)
        {
            _repository = repository;
            _mapper = mapper;
            _messageBus = messageBus;
        }

        public async Task<IEnumerable<PaymentDto>> GetAllPaymentDtosAsync()
        {
            var payments = await _repository.GetAllPaymentsAsync();
            return _mapper.Map<IEnumerable<PaymentDto>>(payments);
        }

        public async Task<PaymentDto?> GetPaymentDtoByIdAsync(int id)
        {
            var payment = await _repository.GetPaymentByIdAsync(id);
            return payment == null ? null : _mapper.Map<PaymentDto>(payment);
        }

        public async Task<IEnumerable<PaymentDto>> GetPaymentsByOrderIdAsync(int orderId)
        {
            var payments = await _repository.GetPaymentsByOrderIdAsync(orderId);
            return _mapper.Map<IEnumerable<PaymentDto>>(payments);
        }

        public async Task<int> CreatePaymentAsync(PaymentCreateDto dto)
        {
            var payment = _mapper.Map<Payment>(dto);
            await _repository.CreatePaymentAsync(payment);

            var eventDto = new PaymentCreatedEventDto
            {
                PaymentId = payment.Id,
                OrderId = payment.OrderId,
                Amount = payment.Amount,
                Status = payment.Status.ToString(),
                ProcessedAt = payment.ProcessedAt
            };
            await _messageBus.PublishPaymentCreatedAsync(eventDto);

            return payment.Id;
        }

        public async Task<bool> UpdatePaymentAsync(int id, PaymentUpdateDto dto)
        {
            var payment = await _repository.GetPaymentByIdAsync(id);
            if (payment == null) return false;

            _mapper.Map(dto, payment);
            await _repository.UpdatePaymentAsync(payment);

            var eventDto = new PaymentUpdatedEventDto
            {
                PaymentId = payment.Id,
                Amount = payment.Amount,
                Status = payment.Status.ToString(),
                UpdatedAt = DateTime.UtcNow
            };
            await _messageBus.PublishPaymentUpdatedAsync(eventDto);

            return true;
        }

        public async Task<bool> DeletePaymentAsync(int id)
        {
            var payment = await _repository.GetPaymentByIdAsync(id);
            if (payment == null) return false;

            await _repository.DeletePaymentAsync(id);

            var eventDto = new PaymentDeletedEventDto
            {
                PaymentId = payment.Id,
                DeletedAt = DateTime.UtcNow
            };
            await _messageBus.PublishPaymentDeletedAsync(eventDto);

            return true;
        }

        public async Task<bool> SimulatePaymentSuccessAsync(int orderId)
        {
            var payments = await _repository.GetPaymentsByOrderIdAsync(orderId);
            var payment = payments.FirstOrDefault();
            if (payment == null) return false;

            // Cập nhật trạng thái
            payment.Status = Domain.Enums.PaymentStatus.Succeeded;
            payment.ProcessedAt = DateTime.UtcNow;

            await _repository.UpdatePaymentAsync(payment);

            // Gửi event thành công
            var eventDto = new PaymentSucceededEventDto
            {
                PaymentId = payment.Id,
                OrderId = payment.OrderId,
                Amount = payment.Amount,
                PaidAt = payment.ProcessedAt,
                Status = "Succeeded"
            };
            await _messageBus.PublishPaymentSucceededAsync(eventDto);

            return true;
        }
    }
}
