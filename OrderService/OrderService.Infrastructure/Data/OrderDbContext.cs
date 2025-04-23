
using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Data
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options)
            : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderEvent> OrderEvents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Thiết lập quan hệ 1-n giữa Order và OrderItem
            modelBuilder.Entity<Order>()
            .HasMany(o => o.Items)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

            // Map enum OrderStatus sang int
            modelBuilder.Entity<Order>()
                .Property(o => o.Status)
                .HasConversion<int>();

            // Map enum EventType sang int
            modelBuilder.Entity<OrderEvent>()
                .Property(e => e.EventType)
                .HasConversion<int>();

            base.OnModelCreating(modelBuilder);

        }
    }
}
