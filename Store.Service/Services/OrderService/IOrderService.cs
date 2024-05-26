using Store.Data.Entities;
using Store.Service.Services.OrderService.Dtos;

namespace Store.Service.Services.OrderService
{
    public interface IOrderService
    {
        Task<OrderResultDto> CreateOrderAsync(OrderDto input);
        Task<IReadOnlyList<OrderResultDto>> GetAllOrdersForUserAsync(string buyerEmail);
        Task<OrderResultDto> GetOrderByIdAsync(Guid id, string buyerEmail);
        Task<IReadOnlyList<DeliveryMethod>> GetAllDeliveryMethodsAsync();
    }
}
