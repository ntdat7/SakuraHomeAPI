using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SakuraHomeAPI.DTOs.Notifications.Requests;
using SakuraHomeAPI.DTOs.Notifications.Responses;
using SakuraHomeAPI.Models.DTOs;
using SakuraHomeAPI.Services.Interfaces;
using System.Security.Claims;

namespace SakuraHomeAPI.Controllers
{
    /// <summary>
    /// Controller for notification management
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(
            INotificationService notificationService,
            ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Get current user's notifications
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<ApiResponse<List<NotificationResponseDto>>>> GetMyNotifications(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 20, 
            [FromQuery] bool unreadOnly = false)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponse.ErrorResult<List<NotificationResponseDto>>("User not authenticated"));

                var result = await _notificationService.GetUserNotificationsAsync(userId.Value, page, pageSize, unreadOnly);
                
                if (result.Success)
                    return Ok(ApiResponse.SuccessResult(result.Data, result.Message));
                
                return BadRequest(ApiResponse.ErrorResult<List<NotificationResponseDto>>(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user notifications");
                return StatusCode(500, ApiResponse.ErrorResult<List<NotificationResponseDto>>("An error occurred while retrieving notifications"));
            }
        }

        /// <summary>
        /// Get specific notification
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<NotificationResponseDto>>> GetNotification(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponse.ErrorResult<NotificationResponseDto>("User not authenticated"));

                var result = await _notificationService.GetNotificationAsync(id, userId.Value);
                
                if (result.Success)
                    return Ok(ApiResponse.SuccessResult(result.Data, result.Message));
                
                return NotFound(ApiResponse.ErrorResult<NotificationResponseDto>(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification {NotificationId}", id);
                return StatusCode(500, ApiResponse.ErrorResult<NotificationResponseDto>("An error occurred while retrieving the notification"));
            }
        }

        /// <summary>
        /// Get unread notification count
        /// </summary>
        [HttpGet("unread-count")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponse.ErrorResult<int>("User not authenticated"));

                var result = await _notificationService.GetUnreadCountAsync(userId.Value);
                
                if (result.Success)
                    return Ok(ApiResponse.SuccessResult(result.Data, result.Message));
                
                return BadRequest(ApiResponse.ErrorResult<int>(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread notification count");
                return StatusCode(500, ApiResponse.ErrorResult<int>("An error occurred while getting unread count"));
            }
        }

        /// <summary>
        /// Mark notification as read
        /// </summary>
        [HttpPatch("{id}/read")]
        [Authorize]
        public async Task<ActionResult<ApiResponse>> MarkAsRead(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponse.ErrorResult("User not authenticated"));

                var result = await _notificationService.MarkAsReadAsync(id, userId.Value);
                
                if (result.Success)
                    return Ok(ApiResponse.SuccessResult(result.Message));
                
                return BadRequest(ApiResponse.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as read", id);
                return StatusCode(500, ApiResponse.ErrorResult("An error occurred while marking notification as read"));
            }
        }

        /// <summary>
        /// Mark all notifications as read
        /// </summary>
        [HttpPatch("mark-all-read")]
        [Authorize]
        public async Task<ActionResult<ApiResponse>> MarkAllAsRead()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponse.ErrorResult("User not authenticated"));

                var result = await _notificationService.MarkAllAsReadAsync(userId.Value);
                
                if (result.Success)
                    return Ok(ApiResponse.SuccessResult(result.Message));
                
                return BadRequest(ApiResponse.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                return StatusCode(500, ApiResponse.ErrorResult("An error occurred while marking all notifications as read"));
            }
        }

        /// <summary>
        /// Delete notification
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse>> DeleteNotification(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponse.ErrorResult("User not authenticated"));

                var result = await _notificationService.DeleteNotificationAsync(id, userId.Value);
                
                if (result.Success)
                    return Ok(ApiResponse.SuccessResult(result.Message));
                
                return BadRequest(ApiResponse.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification {NotificationId}", id);
                return StatusCode(500, ApiResponse.ErrorResult("An error occurred while deleting the notification"));
            }
        }

        /// <summary>
        /// Get notification preferences
        /// </summary>
        [HttpGet("preferences")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<NotificationPreferencesResponseDto>>> GetPreferences()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponse.ErrorResult<NotificationPreferencesResponseDto>("User not authenticated"));

                var result = await _notificationService.GetNotificationPreferencesAsync(userId.Value);
                
                if (result.Success)
                    return Ok(ApiResponse.SuccessResult(result.Data, result.Message));
                
                return BadRequest(ApiResponse.ErrorResult<NotificationPreferencesResponseDto>(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification preferences");
                return StatusCode(500, ApiResponse.ErrorResult<NotificationPreferencesResponseDto>("An error occurred while retrieving notification preferences"));
            }
        }

        /// <summary>
        /// Update notification preferences
        /// </summary>
        [HttpPut("preferences")]
        [Authorize]
        public async Task<ActionResult<ApiResponse>> UpdatePreferences([FromBody] UpdateNotificationPreferencesRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized(ApiResponse.ErrorResult("User not authenticated"));

                var result = await _notificationService.UpdateNotificationPreferencesAsync(userId.Value, request);
                
                if (result.Success)
                    return Ok(ApiResponse.SuccessResult(result.Message));
                
                return BadRequest(ApiResponse.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification preferences");
                return StatusCode(500, ApiResponse.ErrorResult("An error occurred while updating notification preferences"));
            }
        }

        // Admin endpoints

        /// <summary>
        /// Send notification to specific user (Admin only)
        /// </summary>
        [HttpPost("send")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponse<NotificationResponseDto>>> SendNotification([FromBody] CreateNotificationRequestDto request)
        {
            try
            {
                var result = await _notificationService.SendNotificationAsync(request);
                
                if (result.Success)
                    return CreatedAtAction(nameof(GetNotification), new { id = result.Data?.Id }, 
                        ApiResponse.SuccessResult(result.Data, result.Message));
                
                return BadRequest(ApiResponse.ErrorResult<NotificationResponseDto>(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification");
                return StatusCode(500, ApiResponse.ErrorResult<NotificationResponseDto>("An error occurred while sending the notification"));
            }
        }

        /// <summary>
        /// Send bulk notification (Admin only)
        /// </summary>
        [HttpPost("send-bulk")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponse>> SendBulkNotification([FromBody] CreateBulkNotificationRequestDto request)
        {
            try
            {
                var result = await _notificationService.SendBulkNotificationAsync(request);
                
                if (result.Success)
                    return Ok(ApiResponse.SuccessResult(result.Message));
                
                return BadRequest(ApiResponse.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bulk notification");
                return StatusCode(500, ApiResponse.ErrorResult("An error occurred while sending the bulk notification"));
            }
        }

        /// <summary>
        /// Send promotional notification (Admin only)
        /// </summary>
        [HttpPost("promotional")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponse>> SendPromotionalNotification([FromBody] CreatePromotionalNotificationRequestDto request)
        {
            try
            {
                var result = await _notificationService.SendPromotionalNotificationAsync(request);
                
                if (result.Success)
                    return Ok(ApiResponse.SuccessResult(result.Message));
                
                return BadRequest(ApiResponse.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending promotional notification");
                return StatusCode(500, ApiResponse.ErrorResult("An error occurred while sending the promotional notification"));
            }
        }

        /// <summary>
        /// Send maintenance notification (Admin only)
        /// </summary>
        [HttpPost("maintenance")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse>> SendMaintenanceNotification([FromBody] MaintenanceNotificationRequestDto request)
        {
            try
            {
                var result = await _notificationService.SendMaintenanceNotificationAsync(
                    request.StartTime, 
                    request.EndTime, 
                    request.Message);
                
                if (result.Success)
                    return Ok(ApiResponse.SuccessResult(result.Message));
                
                return BadRequest(ApiResponse.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending maintenance notification");
                return StatusCode(500, ApiResponse.ErrorResult("An error occurred while sending the maintenance notification"));
            }
        }

        /// <summary>
        /// Send low stock alert (System use)
        /// </summary>
        [HttpPost("low-stock")]
        [Authorize(Policy = "StaffOnly")]
        public async Task<ActionResult<ApiResponse>> SendLowStockAlert([FromBody] LowStockAlertRequestDto request)
        {
            try
            {
                var result = await _notificationService.SendLowStockAlertAsync(
                    request.ProductId, 
                    request.CurrentStock, 
                    request.Threshold);
                
                if (result.Success)
                    return Ok(ApiResponse.SuccessResult(result.Message));
                
                return BadRequest(ApiResponse.ErrorResult(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending low stock alert");
                return StatusCode(500, ApiResponse.ErrorResult("An error occurred while sending the low stock alert"));
            }
        }

        #region Helper Methods

        /// <summary>
        /// Get current user ID from JWT claims
        /// </summary>
        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        #endregion
    }

    /// <summary>
    /// Request for maintenance notification
    /// </summary>
    public class MaintenanceNotificationRequestDto
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Message { get; set; } = "";
    }

    /// <summary>
    /// Request for low stock alert
    /// </summary>
    public class LowStockAlertRequestDto
    {
        public int ProductId { get; set; }
        public int CurrentStock { get; set; }
        public int Threshold { get; set; }
    }
}