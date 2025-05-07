using PaymentService.Domain.Interfaces;
using PaymentService.Domain.Enums;
using PaymentService.Domain.Events;

namespace PaymentService.Application.Services
{
    public class PaymentSimulatorService : IPaymentSimulator
    {
        private readonly IPaymentRepository _repository;
        private readonly IMessageBusPublisher _messageBus;

        public PaymentSimulatorService(IPaymentRepository repository, IMessageBusPublisher messageBus)
        {
            _repository = repository;
            _messageBus = messageBus;
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
