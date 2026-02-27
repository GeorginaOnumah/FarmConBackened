using FarmConBackened.DTOs.Common;
using FarmConBackened.DTOs.Order;

namespace FarmConBackened.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDto> CreateOrderAsync(Guid buyerUserId, CreateOrderDto dto);
        Task<OrderDto> GetOrderByIdAsync(Guid orderId, Guid userId);
        Task<PagedResult<OrderDto>> GetBuyerOrdersAsync(Guid buyerUserId, int page, int pageSize);
        Task<PagedResult<OrderDto>> GetFarmerOrdersAsync(Guid farmerUserId, int page, int pageSize);
        Task<OrderDto> AcceptOrderAsync(Guid farmerUserId, Guid orderId);
        Task<OrderDto> DeclineOrderAsync(Guid farmerUserId, Guid orderId, string? reason);
        Task<OrderDto> CancelOrderAsync(Guid userId, Guid orderId);
    }
}
