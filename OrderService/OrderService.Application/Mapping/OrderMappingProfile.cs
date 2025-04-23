using AutoMapper;
using OrderService.Application.DTOs;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;


namespace OrderService.Application.Mapping
{
    public class OrderMappingProfile : Profile
    {
        public OrderMappingProfile()
        {
            // Entity -> DTO
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.Status,
                           opt => opt.MapFrom(src => ((OrderStatus)src.Status).ToString()));
    
            CreateMap<OrderItem, OrderItemDto>();

            // DTO -> Entity
            CreateMap<OrderDto, Order>()
                .ForMember(dest => dest.Status,
                           opt => opt.MapFrom(src => (int)Enum.Parse<OrderStatus>(src.Status)));
            CreateMap<OrderItemDto, OrderItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()); // Tự sinh Id

            CreateMap<OrderCreateDto, Order>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (OrderStatus)src.Status));

            CreateMap<OrderUpdateDto, Order>()
                .ForMember(dest => dest.Status,
                           opt => opt.MapFrom(src => (OrderStatus)src.Status));
        }
    }
}
