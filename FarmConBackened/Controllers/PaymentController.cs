using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmConBackened.Controllers
{
    [Route("api/payments")]
    [Authorize]
    public class PaymentController : BaseController
    {
        private readonly IPaymentService _paymentService;
        public PaymentController(IPaymentService paymentService) => _paymentService = paymentService;

        [HttpPost("initiate")]
        [Authorize(Roles = "Buyer")]
        public async Task<IActionResult> InitiatePayment([FromBody] InitiatePaymentDto dto)
        {
            var result = await _paymentService.InitiatePaymentAsync(CurrentUserId, dto);
            return Ok(ApiResponse<object>.Ok(result, "Payment initiated."));
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyPayment([FromBody] PaymentVerificationDto dto)
        {
            var result = await _paymentService.VerifyPaymentAsync(dto);
            return Ok(ApiResponse<PaymentDto>.Ok(result, "Payment verified and held in escrow."));
        }

        [HttpPost("orders/{orderId:guid}/release-escrow")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReleaseEscrow(Guid orderId)
        {
            await _paymentService.ReleaseEscrowAsync(orderId);
            return Ok(ApiResponse.Ok("Escrow released to farmer."));
        }

        [HttpPost("orders/{orderId:guid}/refund")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Refund(Guid orderId)
        {
            await _paymentService.RefundPaymentAsync(orderId);
            return Ok(ApiResponse.Ok("Refund processed."));
        }
    }
}
