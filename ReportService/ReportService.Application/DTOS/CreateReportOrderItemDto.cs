namespace ReportService.Application.DTOs;

public class CreateReportOrderItemDto
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
