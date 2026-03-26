using FarmConBackened.DTOs.Payment;
using FarmConBackened.Helpers.Extensions;
using FarmConBackened.Helpers.Responses;
using FarmConBackened.Interfaces;
using FarmConnect.Data;
using FarmConnect.Helpers;
using FarmConnect.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmConnect.Controllers
{
    [Route("api/payments")]
    [ApiController]
    [Produces("application/json")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IPaymentService paymentService,
            ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        private Guid CurrentUserId => User.GetUserId();

        // ── Initiate Payment ─────────────────────────────────────

        [HttpPost("initiate")]
        [Authorize(Roles = "Buyer")]
        public async Task<IActionResult> InitiatePayment([FromBody] InitiatePaymentDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _paymentService.InitiatePaymentAsync(CurrentUserId, dto);
            return Ok(ApiResponse<object>.Ok(result, "Payment initiated successfully"));
        }

        // ── Verify Payment ─────────────────────────────────────

        [HttpPost("verify")]
        [Authorize]
        public async Task<IActionResult> VerifyPayment([FromBody] PaymentVerificationDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _paymentService.VerifyPaymentAsync(dto);
            return Ok(ApiResponse<PaymentDto>.Ok(result, "Payment verified successfully"));
        }

        // ── Callback ─────────────────────────────────────

        [HttpGet("callback")]
        [AllowAnonymous]
        public async Task<IActionResult> PaystackCallback(
            [FromQuery] string reference)
        {
            var frontendUrl = HttpContext.RequestServices
                .GetRequiredService<IConfiguration>()["App:FrontendUrl"] ?? "http://localhost:3000";

            try
            {
                var dbContext = HttpContext.RequestServices
                    .GetRequiredService<AppDbContext>();

                var payment = await dbContext.Payments
                    .FirstOrDefaultAsync(p => p.GatewayReference == reference);

                if (payment == null)
                {
                    _logger.LogWarning("Unknown reference: {Ref}", reference);
                    return Redirect($"{frontendUrl}/payment/failed");
                }

                var dto = new PaymentVerificationDto
                {
                    GatewayReference = reference,
                    OrderId = payment.OrderId
                };

                await _paymentService.VerifyPaymentAsync(dto);

                return Redirect($"{frontendUrl}/payment/success?orderId={payment.OrderId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Callback failed: {Ref}", reference);
                return Redirect($"{frontendUrl}/payment/failed");
            }
        }

        // ── Webhook ─────────────────────────────────────

        [HttpPost("webhook/paystack")]
        [AllowAnonymous]
        public async Task<IActionResult> PaystackWebhook()
        {
            Request.EnableBuffering();

            using var reader = new StreamReader(Request.Body, leaveOpen: true);
            var rawBody = await reader.ReadToEndAsync();
            Request.Body.Position = 0;

            var signature = Request.Headers["x-paystack-signature"].ToString();

            if (string.IsNullOrEmpty(signature))
                return BadRequest("Missing signature");

            var paymentService = _paymentService as PaymentService;

            if (paymentService == null)
                return Ok();

            await paymentService.HandleWebhookAsync(rawBody, signature);

            return Ok();
        }

        // ── Release Escrow ─────────────────────────────────────

        [HttpPost("orders/{orderId:guid}/release-escrow")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReleaseEscrow(Guid orderId)
        {
            await _paymentService.ReleaseEscrowAsync(orderId);
            return Ok(ApiResponse.Ok("Escrow released successfully"));
        }

        // ── Refund ─────────────────────────────────────

        [HttpPost("orders/{orderId:guid}/refund")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Refund(Guid orderId)
        {
            await _paymentService.RefundPaymentAsync(orderId);
            return Ok(ApiResponse.Ok("Refund processed successfully"));
        }

        // ── Payment History ─────────────────────────────────────

        [HttpGet("orders/{orderId:guid}")]
        [Authorize]
        public async Task<IActionResult> GetOrderPayments(Guid orderId)
        {
            var payments = await _paymentService.GetOrderPaymentsAsync(CurrentUserId, orderId);
            return Ok(ApiResponse<List<PaymentDto>>.Ok(payments, "Success"));
        }
    }
}