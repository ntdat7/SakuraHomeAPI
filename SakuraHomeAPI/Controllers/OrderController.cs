using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SakuraHomeAPI.Services.Interfaces;
using SakuraHomeAPI.DTOs.Orders.Requests;
using SakuraHomeAPI.DTOs.Orders.Responses;
using SakuraHomeAPI.DTOs.Common;
using SakuraHomeAPI.Models.Enums;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace SakuraHomeAPI.Controllers
{
    /// <summary>
    /// Order management controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderService orderService, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        /// <summary>
        /// Create new order from cart
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<OrderResponseDto>>> CreateOrder([FromBody] CreateOrderRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponseDto<OrderResponseDto>.ErrorResult("User not authenticated"));

                var result = await _orderService.CreateOrderAsync(request, userId.Value);

                if (result.Success)
                    return CreatedAtAction(nameof(GetOrder), new { orderId = result.Data?.Id },
                        ApiResponseDto<OrderResponseDto>.SuccessResult(result.Data, result.Message));

                return BadRequest(ApiResponseDto<OrderResponseDto>.ErrorResult(result.Message, result.Errors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order for user {UserId}", GetCurrentUserId());
                return StatusCode(500, ApiResponseDto<OrderResponseDto>.ErrorResult("An error occurred while creating the order"));
            }
        }

        /// <summary>
        /// Get user's orders with optional filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<List<OrderSummaryDto>>>> GetUserOrders([FromQuery] OrderFilterDto? filter = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponseDto<List<OrderSummaryDto>>.ErrorResult("User not authenticated"));

                var result = await _orderService.GetUserOrdersAsync(userId.Value, filter);

                if (result.Success)
                    return Ok(ApiResponseDto<List<OrderSummaryDto>>.SuccessResult(result.Data, result.Message));

                return BadRequest(ApiResponseDto<List<OrderSummaryDto>>.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders for user {UserId}", GetCurrentUserId());
                return StatusCode(500, ApiResponseDto<List<OrderSummaryDto>>.ErrorResult("An error occurred while retrieving orders"));
            }
        }

        /// <summary>
        /// Get specific order details
        /// </summary>
        [HttpGet("{orderId}")]
        public async Task<ActionResult<ApiResponseDto<OrderResponseDto>>> GetOrder(int orderId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponseDto<OrderResponseDto>.ErrorResult("User not authenticated"));

                var result = await _orderService.GetOrderAsync(orderId, userId.Value);

                if (result.Success)
                    return Ok(ApiResponseDto<OrderResponseDto>.SuccessResult(result.Data, result.Message));

                return NotFound(ApiResponseDto<OrderResponseDto>.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order {OrderId} for user {UserId}", orderId, GetCurrentUserId());
                return StatusCode(500, ApiResponseDto<OrderResponseDto>.ErrorResult("An error occurred while retrieving the order"));
            }
        }

        /// <summary>
        /// Update order details (only for pending orders)
        /// </summary>
        [HttpPut("{orderId}")]
        public async Task<ActionResult<ApiResponseDto<OrderResponseDto>>> UpdateOrder(int orderId, [FromBody] UpdateOrderRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponseDto<OrderResponseDto>.ErrorResult("User not authenticated"));

                var result = await _orderService.UpdateOrderAsync(orderId, request, userId.Value);

                if (result.Success)
                    return Ok(ApiResponseDto<OrderResponseDto>.SuccessResult(result.Data, result.Message));

                return BadRequest(ApiResponseDto<OrderResponseDto>.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order {OrderId} for user {UserId}", orderId, GetCurrentUserId());
                return StatusCode(500, ApiResponseDto<OrderResponseDto>.ErrorResult("An error occurred while updating the order"));
            }
        }

        /// <summary>
        /// Cancel order
        /// </summary>
        [HttpDelete("{orderId}")]
        public async Task<ActionResult<ApiResponseDto>> CancelOrder(int orderId, [FromBody] CancelOrderRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponseDto.ErrorResult("User not authenticated"));

                var result = await _orderService.CancelOrderAsync(orderId, request.Reason, userId.Value);

                if (result.Success)
                    return Ok(ApiResponseDto.SuccessResult(result.Message));

                return BadRequest(ApiResponseDto.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order {OrderId} for user {UserId}", orderId, GetCurrentUserId());
                return StatusCode(500, ApiResponseDto.ErrorResult("An error occurred while cancelling the order"));
            }
        }

        /// <summary>
        /// Get order status history
        /// </summary>
        [HttpGet("{orderId}/status-history")]
        public async Task<ActionResult<ApiResponseDto<List<OrderStatusHistoryDto>>>> GetOrderStatusHistory(int orderId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponseDto<List<OrderStatusHistoryDto>>.ErrorResult("User not authenticated"));

                // Verify user owns the order
                var orderResult = await _orderService.GetOrderAsync(orderId, userId.Value);
                if (!orderResult.Success)
                    return NotFound(ApiResponseDto<List<OrderStatusHistoryDto>>.ErrorResult("Order not found"));

                var result = await _orderService.GetOrderStatusHistoryAsync(orderId);

                if (result.Success)
                    return Ok(ApiResponseDto<List<OrderStatusHistoryDto>>.SuccessResult(result.Data, result.Message));

                return BadRequest(ApiResponseDto<List<OrderStatusHistoryDto>>.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting status history for order {OrderId}", orderId);
                return StatusCode(500, ApiResponseDto<List<OrderStatusHistoryDto>>.ErrorResult("An error occurred while retrieving order status history"));
            }
        }

        /// <summary>
        /// Add note to order
        /// </summary>
        [HttpPost("{orderId}/notes")]
        public async Task<ActionResult<ApiResponseDto<OrderResponseDto>>> AddOrderNote(int orderId, [FromBody] AddOrderNoteRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponseDto<OrderResponseDto>.ErrorResult("User not authenticated"));

                var result = await _orderService.AddOrderNoteAsync(orderId, request.Note, true, userId.Value);

                if (result.Success)
                    return Ok(ApiResponseDto<OrderResponseDto>.SuccessResult(result.Data, result.Message));

                return BadRequest(ApiResponseDto<OrderResponseDto>.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding note to order {OrderId}", orderId);
                return StatusCode(500, ApiResponseDto<OrderResponseDto>.ErrorResult("An error occurred while adding order note"));
            }
        }

        /// <summary>
        /// Get order notes
        /// </summary>
        [HttpGet("{orderId}/notes")]
        public async Task<ActionResult<ApiResponseDto<List<OrderNoteDto>>>> GetOrderNotes(int orderId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponseDto<List<OrderNoteDto>>.ErrorResult("User not authenticated"));

                var result = await _orderService.GetOrderNotesAsync(orderId, userId.Value);

                if (result.Success)
                    return Ok(ApiResponseDto<List<OrderNoteDto>>.SuccessResult(result.Data, result.Message));

                return BadRequest(ApiResponseDto<List<OrderNoteDto>>.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notes for order {OrderId}", orderId);
                return StatusCode(500, ApiResponseDto<List<OrderNoteDto>>.ErrorResult("An error occurred while retrieving order notes"));
            }
        }

        /// <summary>
        /// Validate order before creation
        /// </summary>
        [HttpPost("validate")]
        public async Task<ActionResult<ApiResponseDto<OrderValidationDto>>> ValidateOrder([FromBody] CreateOrderRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponseDto<OrderValidationDto>.ErrorResult("User not authenticated"));

                var result = await _orderService.ValidateOrderAsync(request, userId.Value);

                if (result.Success)
                    return Ok(ApiResponseDto<OrderValidationDto>.SuccessResult(result.Data, result.Message));

                return BadRequest(ApiResponseDto<OrderValidationDto>.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating order for user {UserId}", GetCurrentUserId());
                return StatusCode(500, ApiResponseDto<OrderValidationDto>.ErrorResult("An error occurred while validating the order"));
            }
        }

        /// <summary>
        /// Calculate order total
        /// </summary>
        [HttpPost("calculate-total")]
        public async Task<ActionResult<ApiResponseDto<decimal>>> CalculateOrderTotal([FromBody] CalculateOrderTotalRequestDto request)
        {
            try
            {
                var result = await _orderService.CalculateOrderTotalAsync(request.Items, request.ShippingAddressId, request.CouponCode);

                if (result.Success)
                    return Ok(ApiResponseDto<decimal>.SuccessResult(result.Data, result.Message));

                return BadRequest(ApiResponseDto<decimal>.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating order total");
                return StatusCode(500, ApiResponseDto<decimal>.ErrorResult("An error occurred while calculating order total"));
            }
        }

        /// <summary>
        /// Request order return
        /// </summary>
        [HttpPost("{orderId}/return")]
        public async Task<ActionResult<ApiResponseDto<OrderResponseDto>>> RequestReturn(int orderId, [FromBody] ReturnRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponseDto<OrderResponseDto>.ErrorResult("User not authenticated"));

                var result = await _orderService.RequestReturnAsync(orderId, request, userId.Value);

                if (result.Success)
                    return Ok(ApiResponseDto<OrderResponseDto>.SuccessResult(result.Data, result.Message));

                return BadRequest(ApiResponseDto<OrderResponseDto>.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting return for order {OrderId}", orderId);
                return StatusCode(500, ApiResponseDto<OrderResponseDto>.ErrorResult("An error occurred while processing return request"));
            }
        }

        // =================================================================
        // STAFF ONLY ENDPOINTS - Order Management
        // =================================================================

        /// <summary>
        /// Confirm order (Staff only)
        /// </summary>
        [HttpPatch("{orderId}/confirm")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponseDto<OrderResponseDto>>> ConfirmOrder(int orderId)
        {
            try
            {
                var staffId = GetCurrentUserId();
                if (!staffId.HasValue)
                    return Unauthorized(ApiResponseDto<OrderResponseDto>.ErrorResult("Staff not authenticated"));

                var result = await _orderService.ConfirmOrderAsync(orderId, staffId.Value);

                if (result.Success)
                    return Ok(ApiResponseDto<OrderResponseDto>.SuccessResult(result.Data, result.Message));

                return BadRequest(ApiResponseDto<OrderResponseDto>.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming order {OrderId}", orderId);
                return StatusCode(500, ApiResponseDto<OrderResponseDto>.ErrorResult("An error occurred while confirming the order"));
            }
        }

        /// <summary>
        /// Process order (Staff only)
        /// </summary>
        [HttpPatch("{orderId}/process")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponseDto<OrderResponseDto>>> ProcessOrder(int orderId, [FromBody] ProcessOrderRequestDto request)
        {
            try
            {
                var staffId = GetCurrentUserId();
                if (!staffId.HasValue)
                    return Unauthorized(ApiResponseDto<OrderResponseDto>.ErrorResult("Staff not authenticated"));

                var result = await _orderService.ProcessOrderAsync(orderId, request, staffId.Value);

                if (result.Success)
                    return Ok(ApiResponseDto<OrderResponseDto>.SuccessResult(result.Data, result.Message));

                return BadRequest(ApiResponseDto<OrderResponseDto>.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing order {OrderId}", orderId);
                return StatusCode(500, ApiResponseDto<OrderResponseDto>.ErrorResult("An error occurred while processing the order"));
            }
        }

        /// <summary>
        /// Ship order (Staff only)
        /// </summary>
        [HttpPatch("{orderId}/ship")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponseDto<OrderResponseDto>>> ShipOrder(int orderId, [FromBody] ShipOrderRequestDto request)
        {
            try
            {
                var staffId = GetCurrentUserId();
                if (!staffId.HasValue)
                    return Unauthorized(ApiResponseDto<OrderResponseDto>.ErrorResult("Staff not authenticated"));

                var result = await _orderService.ShipOrderAsync(orderId, request, staffId.Value);

                if (result.Success)
                    return Ok(ApiResponseDto<OrderResponseDto>.SuccessResult(result.Data, result.Message));

                return BadRequest(ApiResponseDto<OrderResponseDto>.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error shipping order {OrderId}", orderId);
                return StatusCode(500, ApiResponseDto<OrderResponseDto>.ErrorResult("An error occurred while shipping the order"));
            }
        }

        /// <summary>
        /// Mark order as delivered (Staff only)
        /// </summary>
        [HttpPatch("{orderId}/deliver")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponseDto<OrderResponseDto>>> DeliverOrder(int orderId)
        {
            try
            {
                var staffId = GetCurrentUserId();
                if (!staffId.HasValue)
                    return Unauthorized(ApiResponseDto<OrderResponseDto>.ErrorResult("Staff not authenticated"));

                var result = await _orderService.DeliverOrderAsync(orderId, staffId.Value);

                if (result.Success)
                    return Ok(ApiResponseDto<OrderResponseDto>.SuccessResult(result.Data, result.Message));

                return BadRequest(ApiResponseDto<OrderResponseDto>.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error delivering order {OrderId}", orderId);
                return StatusCode(500, ApiResponseDto<OrderResponseDto>.ErrorResult("An error occurred while marking order as delivered"));
            }
        }

        /// <summary>
        /// Update order status (Staff only)
        /// </summary>
        [HttpPatch("{orderId}/status")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponseDto<OrderResponseDto>>> UpdateOrderStatus(int orderId, [FromBody] UpdateOrderStatusRequestDto request)
        {
            try
            {
                var result = await _orderService.UpdateOrderStatusAsync(orderId, request.Status, request.Notes);

                if (result.Success)
                    return Ok(ApiResponseDto<OrderResponseDto>.SuccessResult(result.Data, result.Message));

                return BadRequest(ApiResponseDto<OrderResponseDto>.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for order {OrderId}", orderId);
                return StatusCode(500, ApiResponseDto<OrderResponseDto>.ErrorResult("An error occurred while updating order status"));
            }
        }

        /// <summary>
        /// Add staff note to order (Staff only)
        /// </summary>
        [HttpPost("{orderId}/staff-notes")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponseDto<OrderResponseDto>>> AddStaffNote(int orderId, [FromBody] AddStaffNoteRequestDto request)
        {
            try
            {
                var staffId = GetCurrentUserId();
                if (!staffId.HasValue)
                    return Unauthorized(ApiResponseDto<OrderResponseDto>.ErrorResult("Staff not authenticated"));

                var result = await _orderService.AddOrderNoteAsync(orderId, request.Note, request.IsCustomerVisible, staffId.Value);

                if (result.Success)
                    return Ok(ApiResponseDto<OrderResponseDto>.SuccessResult(result.Data, result.Message));

                return BadRequest(ApiResponseDto<OrderResponseDto>.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding staff note to order {OrderId}", orderId);
                return StatusCode(500, ApiResponseDto<OrderResponseDto>.ErrorResult("An error occurred while adding staff note"));
            }
        }

        /// <summary>
        /// Process return request (Staff only)
        /// </summary>
        [HttpPatch("{orderId}/return")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponseDto<OrderResponseDto>>> ProcessReturn(int orderId, [FromBody] ProcessReturnRequestDto request)
        {
            try
            {
                var staffId = GetCurrentUserId();
                if (!staffId.HasValue)
                    return Unauthorized(ApiResponseDto<OrderResponseDto>.ErrorResult("Staff not authenticated"));

                var result = await _orderService.ProcessReturnAsync(orderId, request, staffId.Value);

                if (result.Success)
                    return Ok(ApiResponseDto<OrderResponseDto>.SuccessResult(result.Data, result.Message));

                return BadRequest(ApiResponseDto<OrderResponseDto>.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing return for order {OrderId}", orderId);
                return StatusCode(500, ApiResponseDto<OrderResponseDto>.ErrorResult("An error occurred while processing return"));
            }
        }

        /// <summary>
        /// Get order statistics (Staff only)
        /// </summary>
        [HttpGet("stats")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponseDto<OrderStatsDto>>> GetOrderStats([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var result = await _orderService.GetOrderStatsAsync(null, fromDate, toDate);

                if (result.Success)
                    return Ok(ApiResponseDto<OrderStatsDto>.SuccessResult(result.Data, result.Message));

                return BadRequest(ApiResponseDto<OrderStatsDto>.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order statistics");
                return StatusCode(500, ApiResponseDto<OrderStatsDto>.ErrorResult("An error occurred while retrieving order statistics"));
            }
        }

        /// <summary>
        /// Get recent orders (Staff only)
        /// </summary>
        [HttpGet("recent")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponseDto<List<OrderSummaryDto>>>> GetRecentOrders([FromQuery] int count = 10)
        {
            try
            {
                var result = await _orderService.GetRecentOrdersAsync(count);

                if (result.Success)
                    return Ok(ApiResponseDto<List<OrderSummaryDto>>.SuccessResult(result.Data, result.Message));

                return BadRequest(ApiResponseDto<List<OrderSummaryDto>>.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent orders");
                return StatusCode(500, ApiResponseDto<List<OrderSummaryDto>>.ErrorResult("An error occurred while retrieving recent orders"));
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

    /// <summary>
    /// Cancel order request DTO
    /// </summary>
    public class CancelOrderRequestDto
    {
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Add order note request DTO
    /// </summary>
    public class AddOrderNoteRequestDto
    {
        [Required]
        [MaxLength(1000)]
        public string Note { get; set; } = string.Empty;
    }

    /// <summary>
    /// Add staff note request DTO
    /// </summary>
    public class AddStaffNoteRequestDto
    {
        [Required]
        [MaxLength(1000)]
        public string Note { get; set; } = string.Empty;

        public bool IsCustomerVisible { get; set; } = false;
    }

    /// <summary>
    /// Update order status request DTO
    /// </summary>
    public class UpdateOrderStatusRequestDto
    {
        [Required]
        public OrderStatus Status { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Calculate order total request DTO
    /// </summary>
    public class CalculateOrderTotalRequestDto
    {
        [Required]
        public List<OrderItemRequestDto> Items { get; set; } = new();

        public int? ShippingAddressId { get; set; }

        [MaxLength(20)]
        public string? CouponCode { get; set; }
    }
}