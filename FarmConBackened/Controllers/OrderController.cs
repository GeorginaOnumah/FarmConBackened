using FarmConBackened.DTOs.Common;
using FarmConBackened.DTOs.Order;
using FarmConBackened.Helpers.Responses;
using FarmConBackened.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmConBackened.Controllers
{

    [Route("api/orders")]
    [Authorize]
    public class OrderController : BaseController
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService) => _orderService = orderService;

        [HttpPost]
        [Authorize(Roles = "Buyer")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var order = await _orderService.CreateOrderAsync(CurrentUserId, dto);
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, ApiResponse<OrderDto>.Ok(order, "Order placed."));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetOrder(Guid id)
        {
            var order = await _orderService.GetOrderByIdAsync(id, CurrentUserId);
            return Ok(ApiResponse<OrderDto>.Ok(order));
        }

        [HttpGet("my")]
        [Authorize(Roles = "Buyer")]
        public async Task<IActionResult> GetMyOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _orderService.GetBuyerOrdersAsync(CurrentUserId, page, pageSize);
            return Ok(ApiResponse<PagedResult<OrderDto>>.Ok(result));
        }

        [HttpGet("farmer")]
        [Authorize(Roles = "Farmer")]
        public async Task<IActionResult> GetFarmerOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _orderService.GetFarmerOrdersAsync(CurrentUserId, page, pageSize);
            return Ok(ApiResponse<PagedResult<OrderDto>>.Ok(result));
        }

        [HttpPost("{id:guid}/accept")]
        [Authorize(Roles = "Farmer")]
        public async Task<IActionResult> AcceptOrder(Guid id)
        {
            var order = await _orderService.AcceptOrderAsync(CurrentUserId, id);
            return Ok(ApiResponse<OrderDto>.Ok(order, "Order accepted."));
        }

        [HttpPost("{id:guid}/decline")]
        [Authorize(Roles = "Farmer")]
        public async Task<IActionResult> DeclineOrder(Guid id, [FromBody] DeclineOrderDto dto)
        {
            var order = await _orderService.DeclineOrderAsync(CurrentUserId, id, dto.Reason);
            return Ok(ApiResponse<OrderDto>.Ok(order, "Order declined."));
        }

        [HttpPost("{id:guid}/cancel")]
        public async Task<IActionResult> CancelOrder(Guid id)
        {
            var order = await _orderService.CancelOrderAsync(CurrentUserId, id);
            return Ok(ApiResponse<OrderDto>.Ok(order, "Order cancelled."));
        }
    }
}
