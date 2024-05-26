using AutoMapper;
using Store.Data.Entities.IdentityEntities;
using Store.Data.Entities.OrderEntities;

namespace Store.Service.Services.OrderService.Dtos
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<Address, AddressDto>().ReverseMap();
            CreateMap<AddressDto, ShippingAddress>().ReverseMap();

            CreateMap<Order, OrderResultDto>()
                .ForMember(dest => dest.DeliveryMethodName, option => option.MapFrom(src => src.DeliveryMethod.ShortName))
                .ForMember(dest => dest.ShippingPrice, option => option.MapFrom(src => src.DeliveryMethod.Price));

            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(dest => dest.ProductItemId, option => option.MapFrom(src => src.ItemOrdered.ProductItemId))
                .ForMember(dest => dest.ProductName, option => option.MapFrom(src => src.ItemOrdered.ProductName))
                .ForMember(dest => dest.ProductUrl, option => option.MapFrom(src => src.ItemOrdered.ProductUrl))
                .ForMember(dest => dest.ProductUrl, option => option.MapFrom<OrderItemUrlResolver>()).ReverseMap();

        }
    }
}
