using Microsoft.AspNetCore.Mvc;
using ReportService.Application.DTOs;
using AutoMapper;
using ReportService.Domain.Entities;
using ReportService.Domain.Interfaces;

namespace ReportService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RevenueReportController : ControllerBase
    {
        private readonly IRevenueReportService _service;

        public RevenueReportController(IRevenueReportService service)
        {
            _service = service;
        }

        [HttpGet("day")]
        public async Task<IActionResult> GetByDay([FromQuery] DateTime date)
            => Ok(await _service.GetRevenueByDayAsync(date));

        [HttpGet("month")]
        public async Task<IActionResult> GetByMonth([FromQuery] int year, [FromQuery] int month)
            => Ok(await _service.GetRevenueByMonthAsync(year, month));

        [HttpGet("year")]
        public async Task<IActionResult> GetByYear([FromQuery] int year)
            => Ok(await _service.GetRevenueByYearAsync(year));
    }

}
