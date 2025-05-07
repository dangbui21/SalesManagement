using Microsoft.EntityFrameworkCore;
using PaymentService.Infrastructure.Data;
using PaymentService.Application.Mapping;
using PaymentService.Domain.Interfaces;
using PaymentService.Infrastructure.Repositories;
using PaymentService.Application.Interfaces;
using PaymentService.Application.Services;
using PaymentService.Infrastructure.MessageBus;
using PaymentService.Infrastructure.MessageBus.Events;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using PaymentService.Application.DTOS.Auth;
using PaymentService.Domain.Interfaces.Auth;
using PaymentService.Infrastructure.Auth;
using PaymentService.Application.Services.Auth;
var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(PaymentMappingProfile).Assembly);

// Register repositories
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// Register services
builder.Services.AddScoped<IPaymentService, PaymentServiceImpl>();
builder.Services.AddScoped<IMessageBusPublisher, RabbitMQMessageBusPublisher>();
builder.Services.AddScoped<IPaymentSimulator, PaymentSimulatorService>();

// Register token services
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<ITokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<ITokenValidator, JwtTokenValidator>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddSingleton<PaymentNotificationState>();
builder.Services.AddScoped<PaymentNotificationService>();

builder.Services.AddHostedService<RabbitMQPaymentSubscriber>();
builder.Services.AddHostedService<OrderEventSubscriber>();

// Thêm Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;
    
    options.RequireHttpsMetadata = false; // Set true trong production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Add services, controllers, swagger
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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
