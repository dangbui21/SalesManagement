namespace ReportService.Application.DTOs;

public class ReportOrderItemDto
{
    public int Id { get; set; }
    public int ReportOrderId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
