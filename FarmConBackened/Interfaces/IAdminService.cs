using FarmConBackened.DTOs.Admin;
using FarmConBackened.DTOs.Common;
using FarmConBackened.DTOs.Order;
using FarmConBackened.DTOs.Product;
using FarmConBackened.DTOs.User;
using FarmConBackened.Models.Audit;
using FarmConBackened.Models.Enum;
using FarmConBackened.Models.Products;

namespace FarmConBackened.Interfaces
{
    public interface IAdminService
    {
        Task<AdminDashboardDto> GetDashboardAsync();
        Task<PagedResult<UserDto>> GetUsersAsync(string? role, string? status, int page, int pageSize);
        Task<UserDto> UpdateUserStatusAsync(Guid userId, UpdateUserStatusDto dto);
        Task<PagedResult<ProductDto>> GetAllProductsAsync(int page, int pageSize, bool? approved);
        Task<bool> ApproveProductAsync(Guid productId);
        Task<bool> RemoveProductAsync(Guid productId, string reason);
        Task<PagedResult<OrderDto>> GetAllOrdersAsync(int page, int pageSize, OrderStatus? status);
        Task<MarketPrice> CreateMarketPriceAsync(Guid adminUserId, CreateMarketPriceDto dto);
        Task<MarketPrice> UpdateMarketPriceAsync(int id, CreateMarketPriceDto dto);
        Task<List<MarketPrice>> GetMarketPricesAsync();
        Task<List<AuditLog>> GetAuditLogsAsync(int page, int pageSize, Guid? userId);
    }
}
