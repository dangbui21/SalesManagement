namespace ReportService.Domain.Interfaces;
public interface IRevenueReportService
{
    Task<decimal> GetRevenueByDayAsync(DateTime date);
    Task<decimal> GetRevenueByMonthAsync(int year, int month);
    Task<decimal> GetRevenueByYearAsync(int year);
}