using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
using OrderService.Infrastructure.Data;




namespace OrderService.Infrastructure.Factories
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<OrderDbContext>
    {
        public OrderDbContext CreateDbContext(string[] args)
        {
            // Đọc cấu hình từ file appsettings.json
            var parent = Directory.GetParent(Directory.GetCurrentDirectory());
            if (parent == null)
                throw new InvalidOperationException("Không tìm thấy thư mục cha, vui lòng kiểm tra đường dẫn!");

            var configuration = new ConfigurationBuilder()
                        .SetBasePath(parent.FullName) // Trỏ đến thư mục gốc của dự án
                        .AddJsonFile("OrderService.Api/appsettings.json") // Đọc file cấu hình từ OrderService.Api
                        .Build();


            // Lấy chuỗi kết nối từ cấu hình
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Tạo DbContextOptions và trả về DbContext
            var optionsBuilder = new DbContextOptionsBuilder<OrderDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new OrderDbContext(optionsBuilder.Options);
        }
    }
}
