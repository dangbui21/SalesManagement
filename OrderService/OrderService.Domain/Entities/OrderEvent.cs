using System;
using OrderService.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderService.Domain.Entities
{
    public class OrderEvent
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int OrderId { get; set; }
        public EventType EventType { get; set; } = EventType.OrderCreated; // e.g. "OrderCreated", "OrderUpdated"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
