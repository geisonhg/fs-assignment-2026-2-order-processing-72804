using AutoMapper;
using OrderManagement.API.Domain.Entities;
using Shared.Contracts.DTOs;

namespace OrderManagement.API.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductDto>();

        CreateMap<Order, OrderDto>()
            .ForMember(d => d.CustomerName,    o => o.MapFrom(s => s.Customer.Name))
            .ForMember(d => d.CustomerEmail,   o => o.MapFrom(s => s.Customer.Email))
            .ForMember(d => d.InventoryResult, o => o.MapFrom(s => s.InventoryRecord))
            .ForMember(d => d.PaymentResult,   o => o.MapFrom(s => s.PaymentRecord))
            .ForMember(d => d.ShipmentResult,  o => o.MapFrom(s => s.ShipmentRecord));

        CreateMap<Order, OrderSummaryDto>()
            .ForMember(d => d.CustomerName, o => o.MapFrom(s => s.Customer.Name));

        CreateMap<OrderItem,       OrderItemDto>();
        CreateMap<InventoryRecord, InventoryResultDto>();
        CreateMap<PaymentRecord,   PaymentResultDto>();
        CreateMap<ShipmentRecord,  ShipmentResultDto>();
    }
}
