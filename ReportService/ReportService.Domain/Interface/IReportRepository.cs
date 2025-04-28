using ReportService.Domain.Entities;

namespace ReportService.Domain.Interfaces
{
    public interface IReportRepository
    {
        Task<ReportOrder> CreateReportOrderAsync(ReportOrder reportOrder);
        Task<ReportOrder?> GetReportOrderByIdAsync(int id);
        Task<List<ReportOrder>> GetAllReportOrdersAsync();
        Task<ReportOrder> UpdateReportOrderAsync(ReportOrder reportOrder);
        Task<bool> DeleteReportOrderAsync(int id);
    }
    
}
