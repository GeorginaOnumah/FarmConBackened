using FarmConBackened.Models.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmConBackened.Controllers
{
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : BaseController
    {
        private readonly IAdminService _adminService;
        public AdminController(IAdminService adminService) => _adminService = adminService;

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var dashboard = await _adminService.GetDashboardAsync();
            return Ok(ApiResponse<AdminDashboardDto>.Ok(dashboard));
        }

        // ── Users ─────────────────────────────────────────────────────────

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers([FromQuery] string? role, [FromQuery] string? status,
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _adminService.GetUsersAsync(role, status, page, pageSize);
            return Ok(ApiResponse<PagedResult<UserDto>>.Ok(result));
        }

        [HttpPut("users/{userId:guid}/status")]
        public async Task<IActionResult> UpdateUserStatus(Guid userId, [FromBody] UpdateUserStatusDto dto)
        {
            var user = await _adminService.UpdateUserStatusAsync(userId, dto);
            return Ok(ApiResponse<UserDto>.Ok(user, "User status updated."));
        }

        // ── Products ─────────────────────────────────────────────────────

        [HttpGet("products")]
        public async Task<IActionResult> GetProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] bool? approved = null)
        {
            var result = await _adminService.GetAllProductsAsync(page, pageSize, approved);
            return Ok(ApiResponse<PagedResult<ProductDto>>.Ok(result));
        }

        [HttpPost("products/{productId:guid}/approve")]
        public async Task<IActionResult> ApproveProduct(Guid productId)
        {
            await _adminService.ApproveProductAsync(productId);
            return Ok(ApiResponse.Ok("Product approved."));
        }

        [HttpDelete("products/{productId:guid}")]
        public async Task<IActionResult> RemoveProduct(Guid productId, [FromQuery] string reason = "Policy violation")
        {
            await _adminService.RemoveProductAsync(productId, reason);
            return Ok(ApiResponse.Ok("Product removed."));
        }

        // ── Orders ────────────────────────────────────────────────────────

        [HttpGet("orders")]
        public async Task<IActionResult> GetOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] OrderStatus? status = null)
        {
            var result = await _adminService.GetAllOrdersAsync(page, pageSize, status);
            return Ok(ApiResponse<PagedResult<OrderDto>>.Ok(result));
        }

        // ── Market Prices ─────────────────────────────────────────────────

        [HttpGet("market-prices")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMarketPrices()
        {
            var prices = await _adminService.GetMarketPricesAsync();
            return Ok(ApiResponse<object>.Ok(prices));
        }

        [HttpPost("market-prices")]
        public async Task<IActionResult> CreateMarketPrice([FromBody] CreateMarketPriceDto dto)
        {
            var price = await _adminService.CreateMarketPriceAsync(CurrentUserId, dto);
            return Ok(ApiResponse<object>.Ok(price, "Market price created."));
        }

        [HttpPut("market-prices/{id:int}")]
        public async Task<IActionResult> UpdateMarketPrice(int id, [FromBody] CreateMarketPriceDto dto)
        {
            var price = await _adminService.UpdateMarketPriceAsync(id, dto);
            return Ok(ApiResponse<object>.Ok(price, "Market price updated."));
        }

        // ── Audit Logs ────────────────────────────────────────────────────

        [HttpGet("audit-logs")]
        public async Task<IActionResult> GetAuditLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] Guid? userId = null)
        {
            var logs = await _adminService.GetAuditLogsAsync(page, pageSize, userId);
            return Ok(ApiResponse<object>.Ok(logs));
        }
    }
}
