using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReportService.Domain.Entities
{
    public class ReportOrderItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int ReportOrderId { get; set; } // foreign key tới ReportOrder
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; } // đơn giá
        
        // Khóa ngoại
        [ForeignKey("ReportOrderId")]
        public ReportOrder? ReportOrder { get; set; }
    }
}
