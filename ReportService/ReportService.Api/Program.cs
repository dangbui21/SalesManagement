using Microsoft.EntityFrameworkCore;
using ReportService.Infrastructure.Data;
using ReportService.Application.Mapping;
// using ReportService.Application.Services;
//using ReportService.Domain.Interfaces;
using ReportService.Infrastructure.Repositories;
using ReportService.Application.Interfaces;
using ReportService.Domain.Interfaces;
using ReportService.Application.Services;
using ReportService.Infrastructure.MessageConsumers;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<ReportDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// AutoMapper
builder.Services.AddAutoMapper(typeof(ReportMappingProfile).Assembly);


// Dependency Injection
//builder.Services.AddScoped<IReportOrderService, ReportOrderService>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IHandleOrderCompleted, OrderCompletedHandler>();
builder.Services.AddScoped<IRevenueReportService, RevenueReportService>();
builder.Services.AddHostedService<OrderCompletedConsumer>();

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();
