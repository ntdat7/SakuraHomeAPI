using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SakuraHomeAPI.DTOs.Common;
using SakuraHomeAPI.DTOs.Payments.Requests;
using SakuraHomeAPI.DTOs.Payments.Responses;
using SakuraHomeAPI.Models.Enums;
using SakuraHomeAPI.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace SakuraHomeAPI.Controllers
{
    /// <summary>
    /// Payment processing controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        /// <summary>
        /// Get available payment methods
        /// </summary>
        [HttpPost("methods")]
        public async Task<ActionResult<ApiResponseDto<List<PaymentMethodDto>>>> GetPaymentMethods([FromBody] GetPaymentMethodsRequestDto request)
        {
            try
            {
                var result = await _paymentService.GetPaymentMethodsAsync(request);
                
                if (result.Success)
                    return Ok(ApiResponseDto<List<PaymentMethodDto>>.SuccessResult(result.Data, result.Message));
                
                return BadRequest(ApiResponseDto<List<PaymentMethodDto>>.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment methods");
                return StatusCode(500, ApiResponseDto<List<PaymentMethodDto>>.ErrorResult("An error occurred while retrieving payment methods"));
            }
        }

        /// <summary>
        /// Calculate payment fee for a method and amount
        /// </summary>
        [HttpPost("calculate-fee")]
        public async Task<ActionResult<ApiResponseDto<decimal>>> CalculatePaymentFee([FromBody] CalculatePaymentFeeRequestDto request)
        {
            try
            {
                var result = await _paymentService.CalculatePaymentFeeAsync(request.PaymentMethod, request.Amount);
                
                if (result.Success)
                    return Ok(ApiResponseDto<decimal>.SuccessResult(result.Data, result.Message));
                
                return BadRequest(ApiResponseDto<decimal>.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating payment fee");
                return StatusCode(500, ApiResponseDto<decimal>.ErrorResult("An error occurred while calculating payment fee"));
            }
        }

        /// <summary>
        /// Create a new payment
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ApiResponseDto<PaymentResponseDto>>> CreatePayment([FromBody] CreatePaymentRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponseDto<PaymentResponseDto>.ErrorResult("User not authenticated"));

                var result = await _paymentService.CreatePaymentAsync(request, userId.Value);
                
                if (result.Success)
                    return CreatedAtAction(nameof(GetPayment), new { transactionId = result.Data?.TransactionId }, 
                        ApiResponseDto<PaymentResponseDto>.SuccessResult(result.Data, result.Message));
                
                return BadRequest(ApiResponseDto<PaymentResponseDto>.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment");
                return StatusCode(500, ApiResponseDto<PaymentResponseDto>.ErrorResult("An error occurred while creating the payment"));
            }
        }

        /// <summary>
        /// Get payment by transaction ID
        /// </summary>
        [HttpGet("{transactionId}")]
        [Authorize]
        public async Task<ActionResult<ApiResponseDto<PaymentResponseDto>>> GetPayment(string transactionId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _paymentService.GetPaymentAsync(transactionId, userId);
                
                if (result.Success)
                    return Ok(ApiResponseDto<PaymentResponseDto>.SuccessResult(result.Data, result.Message));
                
                return NotFound(ApiResponseDto<PaymentResponseDto>.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment {TransactionId}", transactionId);
                return StatusCode(500, ApiResponseDto<PaymentResponseDto>.ErrorResult("An error occurred while retrieving the payment"));
            }
        }

        /// <summary>
        /// Get current user's payments
        /// </summary>
        [HttpGet("my-payments")]
        [Authorize]
        public async Task<ActionResult<ApiResponseDto<List<PaymentSummaryDto>>>> GetMyPayments([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponseDto<List<PaymentSummaryDto>>.ErrorResult("User not authenticated"));

                var result = await _paymentService.GetUserPaymentsAsync(userId.Value, page, pageSize);
                
                if (result.Success)
                    return Ok(ApiResponseDto<List<PaymentSummaryDto>>.SuccessResult(result.Data, result.Message));
                
                return BadRequest(ApiResponseDto<List<PaymentSummaryDto>>.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user payments");
                return StatusCode(500, ApiResponseDto<List<PaymentSummaryDto>>.ErrorResult("An error occurred while retrieving payments"));
            }
        }

        /// <summary>
        /// Get payments for a specific order
        /// </summary>
        [HttpGet("order/{orderId}")]
        [Authorize]
        public async Task<ActionResult<ApiResponseDto<List<PaymentSummaryDto>>>> GetOrderPayments(int orderId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _paymentService.GetOrderPaymentsAsync(orderId, userId);
                
                if (result.Success)
                    return Ok(ApiResponseDto<List<PaymentSummaryDto>>.SuccessResult(result.Data, result.Message));
                
                return BadRequest(ApiResponseDto<List<PaymentSummaryDto>>.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order payments for order {OrderId}", orderId);
                return StatusCode(500, ApiResponseDto<List<PaymentSummaryDto>>.ErrorResult("An error occurred while retrieving order payments"));
            }
        }

        /// <summary>
        /// Cancel a payment
        /// </summary>
        [HttpDelete("{transactionId}")]
        [Authorize]
        public async Task<ActionResult<ApiResponseDto>> CancelPayment(string transactionId, [FromBody] CancelPaymentRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponseDto.ErrorResult("User not authenticated"));

                var result = await _paymentService.CancelPaymentAsync(transactionId, request.Reason, userId.Value);
                
                if (result.Success)
                    return Ok(ApiResponseDto.SuccessResult(result.Message));
                
                return BadRequest(ApiResponseDto.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling payment {TransactionId}", transactionId);
                return StatusCode(500, ApiResponseDto.ErrorResult("An error occurred while cancelling the payment"));
            }
        }

        // =================================================================
        // GATEWAY-SPECIFIC ENDPOINTS
        // =================================================================

        /// <summary>
        /// Create VNPay payment
        /// </summary>
        [HttpPost("vnpay")]
        [Authorize]
        public async Task<ActionResult<ApiResponseDto<VNPayResponseDto>>> CreateVNPayPayment([FromBody] VNPayRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponseDto<VNPayResponseDto>.ErrorResult("User not authenticated"));

                var result = await _paymentService.CreateVNPayPaymentAsync(request, userId.Value);
                
                if (result.Success)
                    return Ok(ApiResponseDto<VNPayResponseDto>.SuccessResult(result.Data, result.Message));
                
                return BadRequest(ApiResponseDto<VNPayResponseDto>.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating VNPay payment");
                return StatusCode(500, ApiResponseDto<VNPayResponseDto>.ErrorResult("An error occurred while creating VNPay payment"));
            }
        }

        /// <summary>
        /// VNPay payment callback
        /// </summary>
        [HttpPost("vnpay/callback")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponseDto<PaymentCallbackResponseDto>>> VNPayCallback()
        {
            try
            {
                var vnpayData = Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString());
                var result = await _paymentService.ProcessVNPayCallbackAsync(vnpayData);
                
                if (result.Success)
                    return Ok(ApiResponseDto<PaymentCallbackResponseDto>.SuccessResult(result.Data, result.Message));
                
                return BadRequest(ApiResponseDto<PaymentCallbackResponseDto>.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing VNPay callback");
                return StatusCode(500, ApiResponseDto<PaymentCallbackResponseDto>.ErrorResult("An error occurred while processing VNPay callback"));
            }
        }

        /// <summary>
        /// Create MoMo payment
        /// </summary>
        [HttpPost("momo")]
        [Authorize]
        public async Task<ActionResult<ApiResponseDto<MoMoResponseDto>>> CreateMoMoPayment([FromBody] MoMoRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponseDto<MoMoResponseDto>.ErrorResult("User not authenticated"));

                var result = await _paymentService.CreateMoMoPaymentAsync(request, userId.Value);
                
                if (result.Success)
                    return Ok(ApiResponseDto<MoMoResponseDto>.SuccessResult(result.Data, result.Message));
                
                return BadRequest(ApiResponseDto<MoMoResponseDto>.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating MoMo payment");
                return StatusCode(500, ApiResponseDto<MoMoResponseDto>.ErrorResult("An error occurred while creating MoMo payment"));
            }
        }

        /// <summary>
        /// MoMo payment callback
        /// </summary>
        [HttpPost("momo/callback")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponseDto<PaymentCallbackResponseDto>>> MoMoCallback([FromBody] Dictionary<string, string> momoData)
        {
            try
            {
                var result = await _paymentService.ProcessMoMoCallbackAsync(momoData);
                
                if (result.Success)
                    return Ok(ApiResponseDto<PaymentCallbackResponseDto>.SuccessResult(result.Data, result.Message));
                
                return BadRequest(ApiResponseDto<PaymentCallbackResponseDto>.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing MoMo callback");
                return StatusCode(500, ApiResponseDto<PaymentCallbackResponseDto>.ErrorResult("An error occurred while processing MoMo callback"));
            }
        }

        /// <summary>
        /// Create bank transfer payment
        /// </summary>
        [HttpPost("bank-transfer")]
        [Authorize]
        public async Task<ActionResult<ApiResponseDto<BankTransferResponseDto>>> CreateBankTransfer([FromBody] BankTransferRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponseDto<BankTransferResponseDto>.ErrorResult("User not authenticated"));

                var result = await _paymentService.CreateBankTransferAsync(request, userId.Value);
                
                if (result.Success)
                    return Ok(ApiResponseDto<BankTransferResponseDto>.SuccessResult(result.Data, result.Message));
                
                return BadRequest(ApiResponseDto<BankTransferResponseDto>.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bank transfer");
                return StatusCode(500, ApiResponseDto<BankTransferResponseDto>.ErrorResult("An error occurred while creating bank transfer"));
            }
        }

        /// <summary>
        /// Create SePay payment
        /// </summary>
        [HttpPost("sepay")]
        [Authorize]
        public async Task<ActionResult<ApiResponseDto<SePayResponseDto>>> CreateSePayPayment([FromBody] SePayRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponseDto<SePayResponseDto>.ErrorResult("User not authenticated"));

                var result = await _paymentService.CreateSePayPaymentAsync(request, userId.Value);
                
                if (result.Success)
                    return Ok(ApiResponseDto<SePayResponseDto>.SuccessResult(result.Data, result.Message));
                
                return BadRequest(ApiResponseDto<SePayResponseDto>.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating SePay payment");
                return StatusCode(500, ApiResponseDto<SePayResponseDto>.ErrorResult("An error occurred while creating SePay payment"));
            }
        }

        /// <summary>
        /// SePay webhook callback
        /// </summary>
        [HttpPost("sepay/webhook")]
        [AllowAnonymous]
        public async Task<ActionResult<SePayWebhookResponseDto>> SePayWebhook([FromBody] SePayWebhookDto webhookData)
        {
            try
            {
                // Verify webhook signature (if SePay provides one)
                var apiKey = Request.Headers["Authorization"].ToString().Replace("Apikey ", "");
                
                var result = await _paymentService.ProcessSePayWebhookAsync(webhookData, apiKey);
                
                if (result.Success)
                {
                    return Ok(new SePayWebhookResponseDto
                    {
                        Success = true,
                        Message = result.Message,
                        TransactionId = result.Data?.TransactionId,
                        OrderNumber = result.Data?.OrderNumber,
                        Amount = result.Data?.Amount,
                        Status = result.Data?.Status
                    });
                }
                
                return Ok(new SePayWebhookResponseDto
                {
                    Success = false,
                    Message = result.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing SePay webhook");
                return Ok(new SePayWebhookResponseDto
                {
                    Success = false,
                    Message = "An error occurred while processing webhook"
                });
            }
        }

        /// <summary>
        /// Create QR Code payment
        /// </summary>
        [HttpPost("qrcode")]
        [Authorize]
        public async Task<ActionResult<ApiResponseDto<QRCodePaymentResponseDto>>> CreateQRCodePayment([FromBody] QRCodePaymentRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponseDto<QRCodePaymentResponseDto>.ErrorResult("User not authenticated"));

                var result = await _paymentService.CreateQRCodePaymentAsync(request, userId.Value);
                
                if (result.Success)
                    return Ok(ApiResponseDto<QRCodePaymentResponseDto>.SuccessResult(result.Data, result.Message));
                
                return BadRequest(ApiResponseDto<QRCodePaymentResponseDto>.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating QR Code payment");
                return StatusCode(500, ApiResponseDto<QRCodePaymentResponseDto>.ErrorResult("An error occurred while creating QR Code payment"));
            }
        }

        // =================================================================
        // STAFF ONLY ENDPOINTS
        // =================================================================

        /// <summary>
        /// Update payment status (Staff only)
        /// </summary>
        [HttpPatch("{transactionId}/status")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponseDto<PaymentResponseDto>>> UpdatePaymentStatus(string transactionId, [FromBody] UpdatePaymentStatusRequestDto request)
        {
            try
            {
                var staffId = GetCurrentUserId();
                if (!staffId.HasValue)
                    return Unauthorized(ApiResponseDto<PaymentResponseDto>.ErrorResult("Staff not authenticated"));

                var result = await _paymentService.UpdatePaymentStatusAsync(transactionId, request, staffId.Value);
                
                if (result.Success)
                    return Ok(ApiResponseDto<PaymentResponseDto>.SuccessResult(result.Data, result.Message));
                
                return BadRequest(ApiResponseDto<PaymentResponseDto>.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment status for {TransactionId}", transactionId);
                return StatusCode(500, ApiResponseDto<PaymentResponseDto>.ErrorResult("An error occurred while updating payment status"));
            }
        }

        /// <summary>
        /// Process refund (Staff only)
        /// </summary>
        [HttpPost("{transactionId}/refund")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponseDto<RefundResponseDto>>> ProcessRefund(string transactionId, [FromBody] RefundPaymentRequestDto request)
        {
            try
            {
                var staffId = GetCurrentUserId();
                if (!staffId.HasValue)
                    return Unauthorized(ApiResponseDto<RefundResponseDto>.ErrorResult("Staff not authenticated"));

                // Set transaction ID from route
                request.TransactionId = transactionId;

                var result = await _paymentService.ProcessRefundAsync(request, staffId.Value);
                
                if (result.Success)
                    return Ok(ApiResponseDto<RefundResponseDto>.SuccessResult(result.Data, result.Message));
                
                return BadRequest(ApiResponseDto<RefundResponseDto>.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund for {TransactionId}", transactionId);
                return StatusCode(500, ApiResponseDto<RefundResponseDto>.ErrorResult("An error occurred while processing refund"));
            }
        }

        /// <summary>
        /// Confirm bank transfer (Staff only)
        /// </summary>
        [HttpPost("{transactionId}/confirm-transfer")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponseDto<PaymentResponseDto>>> ConfirmBankTransfer(string transactionId, [FromBody] BankTransferRequestDto request)
        {
            try
            {
                var staffId = GetCurrentUserId();
                if (!staffId.HasValue)
                    return Unauthorized(ApiResponseDto<PaymentResponseDto>.ErrorResult("Staff not authenticated"));

                var result = await _paymentService.ConfirmBankTransferAsync(transactionId, request, staffId.Value);
                
                if (result.Success)
                    return Ok(ApiResponseDto<PaymentResponseDto>.SuccessResult(result.Data, result.Message));
                
                return BadRequest(ApiResponseDto<PaymentResponseDto>.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming bank transfer for {TransactionId}", transactionId);
                return StatusCode(500, ApiResponseDto<PaymentResponseDto>.ErrorResult("An error occurred while confirming bank transfer"));
            }
        }

        /// <summary>
        /// Get payment statistics (Staff only)
        /// </summary>
        [HttpGet("stats")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponseDto<PaymentStatsDto>>> GetPaymentStats([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null, [FromQuery] PaymentMethod? method = null)
        {
            try
            {
                var result = await _paymentService.GetPaymentStatsAsync(fromDate, toDate, method);
                
                if (result.Success)
                    return Ok(ApiResponseDto<PaymentStatsDto>.SuccessResult(result.Data, result.Message));
                
                return BadRequest(ApiResponseDto<PaymentStatsDto>.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment statistics");
                return StatusCode(500, ApiResponseDto<PaymentStatsDto>.ErrorResult("An error occurred while retrieving payment statistics"));
            }
        }

        #region Helper Methods

        private Guid? GetCurrentUserId()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdString, out var userId))
            {
                return userId;
            }
            return null;
        }

        #endregion
    }

    #region Request DTOs

    /// <summary>
    /// Request to calculate payment fee
    /// </summary>
    public class CalculatePaymentFeeRequestDto
    {
        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
    }

    /// <summary>
    /// Request to cancel payment
    /// </summary>
    public class CancelPaymentRequestDto
    {
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; }
    }

    #endregion
}