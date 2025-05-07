using AutoMapper;
using PaymentService.Application.DTOs.Payment;
using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Interfaces;
using PaymentService.Domain.Events;
using PaymentService.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace PaymentService.Application.Services
{
    public class PaymentServiceImpl : IPaymentService
    {
        private readonly IPaymentRepository _repository;
        private readonly IMapper _mapper;
        private readonly IMessageBusPublisher _messageBus;
        private readonly PaymentNotificationService _notificationService;
        private readonly ILogger<PaymentServiceImpl> _logger;

        public PaymentServiceImpl(
            IPaymentRepository repository, 
            IMapper mapper, 
            IMessageBusPublisher messageBus,
            PaymentNotificationService notificationService,
            ILogger<PaymentServiceImpl> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _messageBus = messageBus;
            _notificationService = notificationService;
            _logger = logger;
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

            payment.Status = PaymentStatus.Succeeded;
            payment.ProcessedAt = DateTime.UtcNow;
            await _repository.UpdatePaymentAsync(payment);

            // Bắt đầu gửi thông báo định kỳ
            var eventDto = new PaymentSucceededEventDto
            {
                PaymentId = payment.Id,
                OrderId = payment.OrderId,
                Amount = payment.Amount,
                PaidAt = payment.ProcessedAt
            };

            _notificationService.StartSendingNotifications(orderId, eventDto);
            return true;
        }

        public async Task<bool> MarkPaymentAsCompletedAsync(int orderId)
        {
            var payments = await _repository.GetPaymentsByOrderIdAsync(orderId);
            var payment = payments.FirstOrDefault();
            if (payment == null) return false;

            payment.Status = PaymentStatus.Succeeded;
            await _repository.UpdatePaymentAsync(payment);

            // Dừng gửi thông báo định kỳ
            _notificationService.StopSendingNotifications(orderId);
            return true;
        }
    }
}
