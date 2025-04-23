using PaymentService.Domain.Entities;
using PaymentService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using PaymentService.Infrastructure.Data;


namespace PaymentService.Infrastructure.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly PaymentDbContext _context;

        public PaymentRepository(PaymentDbContext context)
        {
            _context = context;
        }

        public async Task<Payment?> GetPaymentByIdAsync(int id)
        {
            // Tìm payment theo id và bao gồm các PaymentEvents liên quan (nếu cần)
            return await _context.Payments
                .Include(p => p.PaymentEvents) // Kết nối với bảng PaymentEvent
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Payment>> GetAllPaymentsAsync()
        {
            return await _context.Payments
                .Include(p => p.PaymentEvents) // Bao gồm PaymentEvents
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByOrderIdAsync(int orderId)
        {
            return await _context.Payments
                .Where(p => p.OrderId == orderId)
                .Include(p => p.PaymentEvents) // Bao gồm PaymentEvents
                .ToListAsync();
        }

        public async Task CreatePaymentAsync(Payment payment)
        {
            // Thêm payment vào cơ sở dữ liệu
            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePaymentAsync(Payment payment)
        {
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePaymentAsync(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment != null)
            {
                _context.Payments.Remove(payment);
                await _context.SaveChangesAsync();
            }
        }

        // Phương thức để thêm sự kiện cho payment
        public async Task CreatePaymentEventAsync(PaymentEvent paymentEvent)
        {
            await _context.PaymentEvents.AddAsync(paymentEvent);
            await _context.SaveChangesAsync();
        }

        // Phương thức để lấy tất cả các sự kiện của payment
        public async Task<IEnumerable<PaymentEvent>> GetPaymentEventsByPaymentIdAsync(int paymentId)
        {
            return await _context.PaymentEvents
                .Where(pe => pe.PaymentId == paymentId)
                .ToListAsync();
        }
    }
}
