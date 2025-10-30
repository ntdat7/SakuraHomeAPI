using SakuraHomeAPI.DTOs.Notifications.Requests;
using SakuraHomeAPI.DTOs.Notifications.Responses;
using SakuraHomeAPI.DTOs.Common;
using SakuraHomeAPI.Models.Enums;

namespace SakuraHomeAPI.Services.Interfaces
{
    /// <summary>
    /// Interface for notification services
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Send notification to user
        /// </summary>
        Task<ApiResponse<NotificationResponseDto>> SendNotificationAsync(CreateNotificationRequestDto request);

        /// <summary>
        /// Send notification to multiple users
        /// </summary>
        Task<ApiResponse> SendBulkNotificationAsync(CreateBulkNotificationRequestDto request);

        /// <summary>
        /// Get user notifications
        /// </summary>
        Task<ApiResponse<List<NotificationResponseDto>>> GetUserNotificationsAsync(Guid userId, int page = 1, int pageSize = 20, bool unreadOnly = false);

        /// <summary>
        /// Get notification by ID
        /// </summary>
        Task<ApiResponse<NotificationResponseDto>> GetNotificationAsync(int notificationId, Guid userId);

        /// <summary>
        /// Mark notification as read
        /// </summary>
        Task<ApiResponse> MarkAsReadAsync(int notificationId, Guid userId);

        /// <summary>
        /// Mark all notifications as read
        /// </summary>
        Task<ApiResponse> MarkAllAsReadAsync(Guid userId);

        /// <summary>
        /// Delete notification
        /// </summary>
        Task<ApiResponse> DeleteNotificationAsync(int notificationId, Guid userId);

        /// <summary>
        /// Get unread notification count
        /// </summary>
        Task<ApiResponse<int>> GetUnreadCountAsync(Guid userId);

        /// <summary>
        /// Update notification preferences
        /// </summary>
        Task<ApiResponse> UpdateNotificationPreferencesAsync(Guid userId, UpdateNotificationPreferencesRequestDto request);

        /// <summary>
        /// Get notification preferences
        /// </summary>
        Task<ApiResponse<NotificationPreferencesResponseDto>> GetNotificationPreferencesAsync(Guid userId);

        // Order-specific notifications
        /// <summary>
        /// Send order status notification
        /// </summary>
        Task<ApiResponse> SendOrderStatusNotificationAsync(int orderId, OrderStatus newStatus, string? message = null);

        /// <summary>
        /// Send order confirmation notification
        /// </summary>
        Task<ApiResponse> SendOrderConfirmationNotificationAsync(int orderId);

        /// <summary>
        /// Send order shipment notification
        /// </summary>
        Task<ApiResponse> SendOrderShipmentNotificationAsync(int orderId, string? trackingNumber = null);

        /// <summary>
        /// Send order delivery notification
        /// </summary>
        Task<ApiResponse> SendOrderDeliveryNotificationAsync(int orderId);

        /// <summary>
        /// Send payment confirmation notification
        /// </summary>
        Task<ApiResponse> SendPaymentConfirmationNotificationAsync(int orderId, string transactionId);

        /// <summary>
        /// Send low stock alert to admin
        /// </summary>
        Task<ApiResponse> SendLowStockAlertAsync(int productId, int currentStock, int threshold);

        /// <summary>
        /// Send promotional notification
        /// </summary>
        Task<ApiResponse> SendPromotionalNotificationAsync(CreatePromotionalNotificationRequestDto request);

        /// <summary>
        /// Send system maintenance notification
        /// </summary>
        Task<ApiResponse> SendMaintenanceNotificationAsync(DateTime startTime, DateTime endTime, string message);
    }
}