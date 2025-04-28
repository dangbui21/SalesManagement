using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReportService.Domain.Entities
{
    public class ReportOrder
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime PaidAt { get; set; } = DateTime.UtcNow;
        public List<ReportOrderItem> Items { get; set; } = new List<ReportOrderItem>();
    }
}
