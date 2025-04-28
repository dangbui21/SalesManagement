namespace ReportService.Application.DTOs;

public class CreateReportOrderDto
{
    public int OrderId { get; set; }
    public decimal TotalAmount { get; set; }
    public List<CreateReportOrderItemDto> Items { get; set; } = new();
}
