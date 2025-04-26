using Microsoft.EntityFrameworkCore;
using OrderService.Application.Interfaces;
using OrderService.Infrastructure.Data;
using OrderService.Infrastructure.Repositories;
using OrderService.Application.Mapping;
using OrderService.Application.Services;
using OrderService.Infrastructure.MessageBus;
using OrderService.Domain.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(typeof(OrderMappingProfile).Assembly);


// Add services, controllers, swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderServiceImpl>();
builder.Services.AddScoped<IMessageBusPublisher, RabbitMQMessageBusPublisher>();
builder.Services.AddHostedService<RabbitMQMessageBusSubscriber>();
builder.Services.AddScoped<IHandlePaymentSucceeded, HandlePaymentSucceeded>();
builder.Services.AddHostedService<PaymentSucceededConsumer>();



var app = builder.Build();

// using (var scope = app.Services.CreateScope())
// {
//     var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
//     db.Database.Migrate(); // Tự động tạo bảng nếu chưa có
// }

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();
