using System;

namespace ReportService.Domain.Entities
{
    public class ReportOrderItem
    {
        public int Id { get; set; }
        public int ReportOrderId { get; set; } // foreign key tới ReportOrder
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; } // đơn giá
    }
}
