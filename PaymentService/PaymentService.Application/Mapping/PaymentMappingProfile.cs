using AutoMapper;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using PaymentService.Application.DTOs.Payment;
using System;
using PaymentService.Application.DTOs;

namespace PaymentService.Application.Mapping
{
    public class PaymentMappingProfile : Profile
    {
        public PaymentMappingProfile()
        {
            // Payment → PaymentDto (Enum → String)
            CreateMap<Payment, PaymentDto>()
                .ForMember(dest => dest.Status,
                           opt => opt.MapFrom(src => src.Status.ToString())); // Chuyển Enum thành string

            // PaymentDto → Payment (String → Enum)
            CreateMap<PaymentDto, Payment>()
                .ForMember(dest => dest.Status,
                           opt => opt.MapFrom(src => Enum.Parse<PaymentStatus>(src.Status))); // Chuyển string về Enum

            // PaymentCreateDto → Payment (int → Enum)
            CreateMap<PaymentCreateDto, Payment>()
                .ForMember(dest => dest.Status,
                           opt => opt.MapFrom(src => (PaymentStatus)src.Status)); // Chuyển int về Enum

            // PaymentUpdateDto → Payment (int → Enum)
            CreateMap<PaymentUpdateDto, Payment>()
                .ForMember(dest => dest.Status,
                           opt => opt.MapFrom(src => (PaymentStatus)src.Status)); // Chuyển int về Enum

            // PaymentEvent → PaymentEventDto
            CreateMap<PaymentEvent, PaymentEventDto>()
                .ForMember(dest => dest.EventType, 
                        opt => opt.MapFrom(src => src.EventType.ToString())); // Chuyển enum thành string

            // PaymentEventDto → PaymentEvent
            CreateMap<PaymentEventDto, PaymentEvent>()
                .ForMember(dest => dest.EventType, 
                        opt => opt.MapFrom(src => (PaymentEventType)Enum.Parse(typeof(PaymentEventType), src.EventType))); // Chuyển string về enum

        }
    }
}
