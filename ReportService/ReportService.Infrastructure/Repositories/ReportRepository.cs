using Microsoft.EntityFrameworkCore;
using ReportService.Domain.Entities;
using ReportService.Infrastructure.Data;
using ReportService.Domain.Interfaces;

namespace ReportService.Infrastructure.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly ReportDbContext _context;

        public ReportRepository(ReportDbContext context)
        {
            _context = context;
        }

       public async Task<ReportOrder> CreateReportOrderAsync(ReportOrder reportOrder)
        {
            var exists = await _context.ReportOrders
                .AnyAsync(r => r.OrderId == reportOrder.OrderId);

            if (exists)
            {
                Console.WriteLine($"[CreateReportOrderAsync] OrderId {reportOrder.OrderId} already exists. Skipping insert.");
                return reportOrder;
            }

            await _context.ReportOrders.AddAsync(reportOrder);
            await _context.SaveChangesAsync();

            Console.WriteLine($"[CreateReportOrderAsync] Saved OrderId {reportOrder.OrderId} to DB.");

            return reportOrder;
        }


       public async Task<ReportOrder?> GetReportOrderByIdAsync(int id)
        {
            return await _context.ReportOrders
                .Include(ro => ro.Items)
                .FirstOrDefaultAsync(ro => ro.Id == id);
        }

        public async Task<List<ReportOrder>> GetAllReportOrdersAsync()
        {
            return await _context.ReportOrders
                .Include(ro => ro.Items)
                .ToListAsync();
        }

        public async Task<ReportOrder> UpdateReportOrderAsync(ReportOrder reportOrder)
        {
            _context.ReportOrders.Update(reportOrder);
            await _context.SaveChangesAsync();
            return reportOrder;
        }

        public async Task<bool> DeleteReportOrderAsync(int id)
        {
            var reportOrder = await _context.ReportOrders.FindAsync(id);
            if (reportOrder == null)
            {
                return false;
            }

            _context.ReportOrders.Remove(reportOrder);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ReportOrder>> GetOrdersByDateRangeAsync(DateTime from, DateTime to)
        {
            return await _context.ReportOrders
                .Where(r => r.PaidAt >= from && r.PaidAt < to)
                .ToListAsync();
        }

    }
}
