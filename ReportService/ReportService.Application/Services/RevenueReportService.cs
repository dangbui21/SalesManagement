using ReportService.Domain.Interfaces;
namespace ReportService.Application.Services;
public class RevenueReportService : IRevenueReportService
{
    private readonly IReportRepository _reportRepository;

    public RevenueReportService(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<decimal> GetRevenueByDayAsync(DateTime date)
    {
        var from = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        var to = from.AddDays(1);
        var reports = await _reportRepository.GetOrdersByDateRangeAsync(from, to);
        return reports.Sum(r => r.TotalAmount);
    }

    public async Task<decimal> GetRevenueByMonthAsync(int year, int month)
    {
        var from = DateTime.SpecifyKind(new DateTime(year, month, 1), DateTimeKind.Utc);
        var to = from.AddMonths(1);
        var reports = await _reportRepository.GetOrdersByDateRangeAsync(from, to);
        return reports.Sum(r => r.TotalAmount);
    }

    public async Task<decimal> GetRevenueByYearAsync(int year)
    {
        var from = DateTime.SpecifyKind(new DateTime(year, 1, 1), DateTimeKind.Utc);
        var to = from.AddYears(1);
        var reports = await _reportRepository.GetOrdersByDateRangeAsync(from, to);
        return reports.Sum(r => r.TotalAmount);
    }

}
