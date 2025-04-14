using System;

namespace ReportService.Domain.Entities
{
    public class ReportOrder
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime PaidAt { get; set; } = DateTime.UtcNow;
    }
}
