using Microsoft.EntityFrameworkCore;
using ReportService.Domain.Entities;

namespace ReportService.Infrastructure.Data;

public class ReportDbContext : DbContext
{
    public ReportDbContext(DbContextOptions<ReportDbContext> options) : base(options)
    {
    }

    public DbSet<ReportOrder> ReportOrders { get; set; }
    public DbSet<ReportOrderItem> ReportOrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Cấu hình quan hệ 1-nhiều
        modelBuilder.Entity<ReportOrder>()
            .HasMany(ro => ro.Items)
            .WithOne(roi => roi.ReportOrder)
            .HasForeignKey(roi => roi.ReportOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
