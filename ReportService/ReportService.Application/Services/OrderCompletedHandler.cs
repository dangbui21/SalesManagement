using AutoMapper;
using ReportService.Application.DTOs;
using ReportService.Domain.Entities;
using ReportService.Domain.Events;
using ReportService.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ReportService.Application.Services
{
    public class OrderCompletedHandler : IHandleOrderCompleted
    {
        private readonly IReportRepository _reportRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderCompletedHandler> _logger;
        public OrderCompletedHandler(IReportRepository reportRepository, IMapper mapper, ILogger<OrderCompletedHandler> logger)
        {
            _reportRepository = reportRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task HandleAsync(OrderCompletedEvent orderCompletedEvent)
        {
            // B1: map sang CreateReportOrderDto
            var createDto = new CreateReportOrderDto
            {
                OrderId = orderCompletedEvent.OrderId,
                TotalAmount = orderCompletedEvent.TotalAmount,
                Items = orderCompletedEvent.Items.Select(i => new CreateReportOrderItemDto
                {
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    Price = i.UnitPrice
                }).ToList()
            };

            // B2: map sang entity
            var reportOrder = _mapper.Map<ReportOrder>(createDto);

            _logger.LogInformation("📝 Prepared ReportOrder: {@ReportOrder}", reportOrder);

            // B3: lưu DB
            await _reportRepository.CreateReportOrderAsync(reportOrder);
            _logger.LogInformation("✅ Report for OrderId {OrderId} saved to DB", orderCompletedEvent.OrderId);
        }
    }
}
