using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SakuraHomeAPI.DTOs.Common;
using SakuraHomeAPI.DTOs.Orders.Requests;
using SakuraHomeAPI.DTOs.Orders.Responses;
using SakuraHomeAPI.Services.Interfaces;
using System.Security.Claims;

namespace SakuraHomeAPI.Controllers
{
    /// <summary>
    /// Order management controller
    /// </summary>
    [ApiController]
    [Route("api/orders")]
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

        // =================================================================
        // USER ENDPOINTS - Customer Order Operations
        // =================================================================

        /// <summary>
        /// Create new order from cart
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<OrderResponseDto>>> CreateOrder([FromBody] CreateOrderRequestDto request)
        {
            try
            {
                // Input validation
                if (request == null)
                {
                    _logger.LogWarning("CreateOrder called with null request");
                    return BadRequest(ApiResponseDto<OrderResponseDto>.ErrorResult("Request cannot be null"));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    _logger.LogWarning("CreateOrder validation failed: {Errors}", string.Join(", ", errors));
                    return BadRequest(ApiResponseDto<OrderResponseDto>.ErrorResult("Validation failed", errors));
                }

                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    _logger.LogWarning("CreateOrder called without authenticated user");
                    return Unauthorized(ApiResponseDto<OrderResponseDto>.ErrorResult("User not authenticated"));
                }

                _logger.LogInformation("Creating order for user {UserId} with {ItemCount} items",
                    userId.Value, request.Items?.Count ?? 0);

                var result = await _orderService.CreateOrderAsync(request, userId.Value);

                if (result.Success)
                {
                    _logger.LogInformation("Order created successfully: {OrderId} for user {UserId}",
                        result.Data?.Id, userId.Value);

                    return CreatedAtAction(nameof(GetOrder), new { orderId = result.Data?.Id },
                        ApiResponseDto<OrderResponseDto>.SuccessResult(result.Data, result.Message));
                }

                // Log the specific error from service
                _logger.LogWarning("Order creation failed for user {UserId}: {Message}",
                    userId.Value, result.Message);

                // Return more specific error codes based on error message
                if (result.Message.Contains("not found") || result.Message.Contains("not exist"))
                {
                    return NotFound(ApiResponseDto<OrderResponseDto>.ErrorResult(result.Message, result.Errors));
                }

                if (result.Message.Contains("insufficient") || result.Message.Contains("stock") ||
                    result.Message.Contains("not available"))
                {
                    return Conflict(ApiResponseDto<OrderResponseDto>.ErrorResult(result.Message, result.Errors));
                }

                if (result.Message.Contains("unauthorized") || result.Message.Contains("access denied") ||
                    result.Message.Contains("does not belong"))
                {
                    return Forbid();
                }

                return BadRequest(ApiResponseDto<OrderResponseDto>.ErrorResult(result.Message, result.Errors));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument in CreateOrder for user {UserId}", GetCurrentUserId());
                return BadRequest(ApiResponseDto<OrderResponseDto>.ErrorResult($"Invalid request: {ex.Message}"));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access in CreateOrder for user {UserId}", GetCurrentUserId());
                return Unauthorized(ApiResponseDto<OrderResponseDto>.ErrorResult("Access denied"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid operation in CreateOrder for user {UserId}", GetCurrentUserId());
                return BadRequest(ApiResponseDto<OrderResponseDto>.ErrorResult($"Operation failed: {ex.Message}"));
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Timeout in CreateOrder for user {UserId}", GetCurrentUserId());
                return StatusCode(408, ApiResponseDto<OrderResponseDto>.ErrorResult("Request timeout. Please try again."));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error in CreateOrder for user {UserId}", GetCurrentUserId());
                return StatusCode(500, ApiResponseDto<OrderResponseDto>.ErrorResult("Database error occurred. Please try again."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating order for user {UserId}: {Message}",
                    GetCurrentUserId(), ex.Message);

                var errorMessage = "An error occurred while creating the order";
#if DEBUG
                errorMessage += $": {ex.Message}";
#endif
                return StatusCode(500, ApiResponseDto<OrderResponseDto>.ErrorResult(errorMessage));
            }
        }

        /// <summary>
        /// Get user's orders with optional filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResult<OrderSummaryDto>>>> GetUserOrders([FromQuery] OrderFilterDto? filter = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponse.ErrorResult<PagedResult<OrderSummaryDto>>("User not authenticated"));

                var result = await _orderService.GetUserOrdersAsync(userId.Value, filter);

                if (result.Success && result.Data != null)
                {
                    var page = filter?.Page ?? 1;
                    var pageSize = filter?.PageSize ?? 20;
                    var totalCount = result.Data.Count;

                    var pagedResult = new PagedResult<OrderSummaryDto>
                    {
                        Items = result.Data,
                        TotalCount = totalCount,
                        PageNumber = page,
                        PageSize = pageSize
                    };

                    return Ok(ApiResponse.SuccessResult(pagedResult, result.Message ?? "Orders retrieved successfully"));
                }

                return BadRequest(ApiResponse.ErrorResult<PagedResult<OrderSummaryDto>>(result.Message ?? "Failed to retrieve orders"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders for user {UserId}", GetCurrentUserId());
                return StatusCode(500, ApiResponse.ErrorResult<PagedResult<OrderSummaryDto>>("An error occurred while retrieving orders"));
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
        /// Add note to order (customer note)
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
        // STAFF ONLY ENDPOINTS - Order Management & Processing
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
}