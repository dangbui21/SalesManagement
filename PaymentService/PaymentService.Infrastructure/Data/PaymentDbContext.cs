using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;

namespace PaymentService.Infrastructure.Data
{
    public class PaymentDbContext : DbContext
    {
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options)
        {
        }

        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentEvent> PaymentEvents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Map enum PaymentStatus sang int
            modelBuilder.Entity<Payment>()
                .Property(p => p.Status)
                .HasConversion<int>();

            // Map enum PaymentEventType sang int
            modelBuilder.Entity<PaymentEvent>()
                .Property(e => e.EventType)
                .HasConversion<int>();
  

            base.OnModelCreating(modelBuilder);
            
             // Định nghĩa quan hệ 1-n giữa Payment và PaymentEvent
            modelBuilder.Entity<Payment>()
            .HasMany(p => p.PaymentEvents)
            .WithOne(pe => pe.Payment)
            .HasForeignKey(pe => pe.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
