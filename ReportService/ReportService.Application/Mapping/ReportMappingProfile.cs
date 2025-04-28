using AutoMapper;
using ReportService.Domain.Entities;
using ReportService.Application.DTOs;

namespace ReportService.Application.Mapping
{
    public class ReportMappingProfile : Profile
    {
        public ReportMappingProfile()
        {
            CreateMap<ReportOrder, ReportOrderDto>();
            CreateMap<ReportOrderItem, ReportOrderItemDto>();

            CreateMap<CreateReportOrderDto, ReportOrder>();
            CreateMap<CreateReportOrderItemDto, ReportOrderItem>();
        }
    }
}
