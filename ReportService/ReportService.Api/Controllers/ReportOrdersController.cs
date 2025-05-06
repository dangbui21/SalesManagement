using Microsoft.AspNetCore.Mvc;
using ReportService.Application.DTOs;
using AutoMapper;
using ReportService.Domain.Entities;
using ReportService.Domain.Interfaces;

namespace ReportService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportOrdersController : ControllerBase
    {
        private readonly IReportRepository _reportRepository;
        private readonly IMapper _mapper;

        public ReportOrdersController(IReportRepository reportRepository, IMapper mapper)
        {
            _reportRepository = reportRepository;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReportOrderDto createDto)
        {
            var reportOrder = _mapper.Map<ReportOrder>(createDto);
            var created = await _reportRepository.CreateReportOrderAsync(reportOrder);
            var result = _mapper.Map<ReportOrderDto>(created);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var reportOrder = await _reportRepository.GetReportOrderByIdAsync(id);
            if (reportOrder == null) return NotFound();
            var result = _mapper.Map<ReportOrderDto>(reportOrder);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var reportOrders = await _reportRepository.GetAllReportOrdersAsync();
            var result = _mapper.Map<List<ReportOrderDto>>(reportOrders);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateReportOrderDto updateDto)
        {
            var existing = await _reportRepository.GetReportOrderByIdAsync(id);
            if (existing == null) return NotFound();

            _mapper.Map(updateDto, existing);
            var updated = await _reportRepository.UpdateReportOrderAsync(existing);
            var result = _mapper.Map<ReportOrderDto>(updated);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _reportRepository.DeleteReportOrderAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
