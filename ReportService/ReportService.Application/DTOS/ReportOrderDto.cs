namespace ReportService.Application.DTOs;

public class ReportOrderDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime PaidAt { get; set; }
    public List<ReportOrderItemDto> Items { get; set; } = new();
}
