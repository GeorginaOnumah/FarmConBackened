using FarmConBackened.DataLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmConBackened.Controllers
{

    [Route("api/deliveries")]
    [Authorize]
    public class DeliveryController : BaseController
    {
        private readonly IDeliveryService _deliveryService;
        public DeliveryController(IDeliveryService deliveryService) => _deliveryService = deliveryService;

        [HttpGet("track/{trackingCode}")]
        [AllowAnonymous]
        public async Task<IActionResult> Track(string trackingCode)
        {
            var delivery = await _deliveryService.TrackDeliveryAsync(trackingCode);
            return delivery != null ? Ok(ApiResponse<DeliveryDto>.Ok(delivery)) : NotFound(ApiResponse.Fail("Tracking code not found."));
        }

        [HttpGet("order/{orderId:guid}")]
        public async Task<IActionResult> GetByOrder(Guid orderId)
        {
            var delivery = await _deliveryService.GetDeliveryByOrderIdAsync(orderId, CurrentUserId);
            return Ok(ApiResponse<DeliveryDto>.Ok(delivery));
        }

        [HttpGet("my")]
        [Authorize(Roles = "Transporter")]
        public async Task<IActionResult> GetMyDeliveries([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _deliveryService.GetTransporterDeliveriesAsync(CurrentUserId, page, pageSize);
            return Ok(ApiResponse<PagedResult<DeliveryDto>>.Ok(result));
        }

        [HttpPost("{deliveryId:guid}/accept")]
        [Authorize(Roles = "Transporter")]
        public async Task<IActionResult> Accept(Guid deliveryId)
        {
            var delivery = await _deliveryService.AcceptDeliveryRequestAsync(deliveryId, CurrentUserId);
            return Ok(ApiResponse<DeliveryDto>.Ok(delivery, "Delivery accepted."));
        }

        [HttpPost("{deliveryId:guid}/decline")]
        [Authorize(Roles = "Transporter")]
        public async Task<IActionResult> Decline(Guid deliveryId)
        {
            var delivery = await _deliveryService.DeclineDeliveryRequestAsync(deliveryId, CurrentUserId);
            return Ok(ApiResponse<DeliveryDto>.Ok(delivery, "Delivery declined."));
        }

        [HttpPut("{deliveryId:guid}/status")]
        [Authorize(Roles = "Transporter")]
        public async Task<IActionResult> UpdateStatus(Guid deliveryId, [FromBody] UpdateDeliveryStatusDto dto)
        {
            var delivery = await _deliveryService.UpdateDeliveryStatusAsync(deliveryId, CurrentUserId, dto);
            return Ok(ApiResponse<DeliveryDto>.Ok(delivery, "Delivery status updated."));
        }

        [HttpPost("order/{orderId:guid}/assign")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignTransporter(Guid orderId, [FromBody] AssignTransporterDto dto)
        {
            var delivery = await _deliveryService.AssignTransporterAsync(orderId, dto.TransporterUserId);
            return Ok(ApiResponse<DeliveryDto>.Ok(delivery, "Transporter assigned."));
        }
    }
    public record AssignTransporterDto(Guid TransporterUserId);
}
