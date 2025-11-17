using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SakuraHomeAPI.Data;
using SakuraHomeAPI.DTOs.Common;
using SakuraHomeAPI.DTOs.Notifications.Requests;
using SakuraHomeAPI.DTOs.Notifications.Responses;
using SakuraHomeAPI.Hubs;
using SakuraHomeAPI.Models.Entities;
using SakuraHomeAPI.Models.Enums;
using SakuraHomeAPI.Services.Interfaces;
using System.Text.Json;

namespace SakuraHomeAPI.Services.Implementations
{
    /// <summary>
    /// Notification service implementation
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotificationService> _logger;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(
            ApplicationDbContext context,
            ILogger<NotificationService> logger,
            IEmailService emailService,
            IConfiguration configuration,
            IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
            _configuration = configuration;
            _hubContext = hubContext;
        }

        public async Task<ApiResponse<NotificationResponseDto>> SendNotificationAsync(CreateNotificationRequestDto request)
        {
            try
            {
                var notification = new Notification
                {
                    UserId = request.UserId,
                    Title = request.Title,
                    Message = request.Message,
                    Type = request.Type,
                    ActionUrl = request.ActionUrl,
                    Data = request.Data != null ? JsonSerializer.Serialize(request.Data) : null,
                    Priority = (NotificationPriority)request.Priority,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    ScheduledTime = request.ScheduledTime,
                    ExpiryTime = request.ScheduledTime?.AddDays(30)
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                // ✅ THÊM: Gửi real-time notification qua SignalR
                await SendRealTimeNotificationAsync(notification);

                // ✅ GỬI EMAIL (nếu enable)
                if (request.SendEmail)
                {
                    var user = await _context.Users.FindAsync(request.UserId);
                    if (user != null && user.EmailNotifications)
                    {
                        try
                        {
                            await _emailService.SendEmailAsync(user.Email, "General", new
                            {
                                UserName = user.FullName,
                                Title = request.Title,
                                Message = request.Message,
                                ActionUrl = request.ActionUrl
                            }, request.Title);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to send email notification to {Email}", user.Email);
                            // Continue - email failure không block notification
                        }
                    }
                }

                var response = MapToNotificationResponseDto(notification);

                _logger.LogInformation("✅ Notification sent to user {UserId}: {Title}", request.UserId, request.Title);
                return ApiResponse.SuccessResult(response, "Notification sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to user {UserId}", request.UserId);
                return ApiResponse.ErrorResult<NotificationResponseDto>("Failed to send notification");
            }
        }

        /// <summary>
        /// ✅ THÊM: Send real-time notification via SignalR
        /// </summary>
        private async Task SendRealTimeNotificationAsync(Notification notification)
        {
            try
            {
                var notificationData = new
                {
                    id = notification.Id,
                    title = notification.Title,
                    message = notification.Message,
                    type = notification.Type.ToString(),
                    typeName = GetNotificationTypeName(notification.Type),
                    priority = notification.Priority.ToString(),
                    priorityName = GetPriorityName(notification.Priority),
                    actionUrl = notification.ActionUrl,
                    data = !string.IsNullOrEmpty(notification.Data) ?
                        JsonSerializer.Deserialize<Dictionary<string, object>>(notification.Data) : null,
                    createdAt = notification.CreatedAt,
                    isRead = notification.IsRead
                };

                // ✅ Gửi đến user cụ thể
                await _hubContext.Clients
                    .Group($"user_{notification.UserId}")
                    .SendAsync("ReceiveNotification", notificationData);

                // ✅ FIXED: Nếu là notification cho Admin/Staff/SuperAdmin, broadcast đến admin group
                var user = await _context.Users.FindAsync(notification.UserId);
                if (user != null && (user.Role == UserRole.SuperAdmin || user.Role == UserRole.Admin || user.Role == UserRole.Staff))
                {
                    await _hubContext.Clients
                        .Group("admins")
                        .SendAsync("ReceiveNotification", notificationData);
                    
                    _logger.LogInformation("✅ Real-time notification broadcasted to admins group");
                }

                _logger.LogInformation("✅ Real-time notification sent to user {UserId} via SignalR",
                    notification.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send real-time notification to user {UserId}",
                    notification.UserId);
                // Don't throw - notification is already saved in DB
            }
        }

        /// <summary>
        /// ✅ THÊM: Send real-time notification to all admins
        /// </summary>
        private async Task BroadcastToAdminsAsync(string title, string message, NotificationType type, string? actionUrl = null, Dictionary<string, object>? data = null)
        {
            try
            {
                var notificationData = new
                {
                    id = 0, // Temporary ID for broadcast
                    title = title,
                    message = message,
                    type = type.ToString(),
                    typeName = GetNotificationTypeName(type),
                    priority = NotificationPriority.High.ToString(),
                    priorityName = GetPriorityName(NotificationPriority.High),
                    actionUrl = actionUrl,
                    data = data,
                    createdAt = DateTime.UtcNow,
                    isRead = false
                };

                // ✅ Broadcast đến tất cả admin đang online
                await _hubContext.Clients
                    .Group("admins")
                    .SendAsync("ReceiveNotification", notificationData);

                _logger.LogInformation("✅ Real-time notification broadcasted to all admins: {Title}", title);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to broadcast notification to admins: {Title}", title);
            }
        }

        public async Task<ApiResponse> SendBulkNotificationAsync(CreateBulkNotificationRequestDto request)
        {
            try
            {
                List<Guid> targetUserIds;

                if (request.SendToAll)
                {
                    var query = _context.Users.Where(u => u.IsActive && !u.IsDeleted);
                    
                    if (request.RoleFilter?.Any() == true)
                    {
                        var roles = request.RoleFilter.Select(r => Enum.Parse<UserRole>(r)).ToList();
                        query = query.Where(u => roles.Contains(u.Role));
                    }
                    
                    if (request.RegisteredAfter.HasValue)
                    {
                        query = query.Where(u => u.CreatedAt >= request.RegisteredAfter.Value);
                    }
                    
                    if (request.LastActiveAfter.HasValue)
                    {
                        query = query.Where(u => u.LastLoginAt >= request.LastActiveAfter.Value);
                    }

                    targetUserIds = await query.Select(u => u.Id).ToListAsync();
                }
                else
                {
                    targetUserIds = request.UserIds ?? new List<Guid>();
                }

                var notifications = targetUserIds.Select(userId => new Notification
                {
                    UserId = userId,
                    Title = request.Title,
                    Message = request.Message,
                    Type = request.Type,
                    ActionUrl = request.ActionUrl,
                    Data = request.Data != null ? JsonSerializer.Serialize(request.Data) : null,
                    Priority = (NotificationPriority)request.Priority,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    ScheduledTime = request.ScheduledTime,
                    ExpiryTime = request.ScheduledTime?.AddDays(30)
                }).ToList();

                _context.Notifications.AddRange(notifications);
                await _context.SaveChangesAsync();

                if (request.SendEmail)
                {
                    var users = await _context.Users
                        .Where(u => targetUserIds.Contains(u.Id))
                        .ToListAsync();

                    foreach (var user in users)
                    {
                        await _emailService.SendEmailAsync(user.Email, "General Notification", new
                        {
                            UserName = user.FullName,
                            Title = request.Title,
                            Message = request.Message,
                            ActionUrl = request.ActionUrl
                        }, request.Title);
                    }
                }

                _logger.LogInformation("Bulk notification sent to {Count} users: {Title}", targetUserIds.Count, request.Title);
                return ApiResponse.SuccessResult($"Notification sent to {targetUserIds.Count} users");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bulk notification");
                return ApiResponse.ErrorResult("Failed to send bulk notification");
            }
        }

        public async Task<ApiResponse<List<NotificationResponseDto>>> GetUserNotificationsAsync(Guid userId, int page = 1, int pageSize = 20, bool unreadOnly = false)
        {
            try
            {
                var query = _context.Notifications
                    .Where(n => n.UserId == userId);

                if (unreadOnly)
                {
                    query = query.Where(n => !n.IsRead);
                }

                query = query.Where(n => n.ExpiryTime == null || n.ExpiryTime > DateTime.UtcNow);

                var notifications = await query
                    .OrderByDescending(n => n.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var response = notifications.Select(MapToNotificationResponseDto).ToList();

                return ApiResponse.SuccessResult(response, "Notifications retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications for user {UserId}", userId);
                return ApiResponse.ErrorResult<List<NotificationResponseDto>>("Failed to retrieve notifications");
            }
        }

        public async Task<ApiResponse<NotificationResponseDto>> GetNotificationAsync(int notificationId, Guid userId)
        {
            try
            {
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

                if (notification == null)
                {
                    return ApiResponse.ErrorResult<NotificationResponseDto>("Notification not found");
                }

                var response = MapToNotificationResponseDto(notification);
                return ApiResponse.SuccessResult(response, "Notification retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification {NotificationId} for user {UserId}", notificationId, userId);
                return ApiResponse.ErrorResult<NotificationResponseDto>("Failed to retrieve notification");
            }
        }

        public async Task<ApiResponse> MarkAsReadAsync(int notificationId, Guid userId)
        {
            try
            {
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

                if (notification == null)
                {
                    return ApiResponse.ErrorResult("Notification not found");
                }

                if (!notification.IsRead)
                {
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                return ApiResponse.SuccessResult("Notification marked as read");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as read for user {UserId}", notificationId, userId);
                return ApiResponse.ErrorResult("Failed to mark notification as read");
            }
        }

        public async Task<ApiResponse> MarkAllAsReadAsync(Guid userId)
        {
            try
            {
                var unreadNotifications = await _context.Notifications
                    .Where(n => n.UserId == userId && !n.IsRead)
                    .ToListAsync();

                var readTime = DateTime.UtcNow;
                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                    notification.ReadAt = readTime;
                }

                await _context.SaveChangesAsync();

                return ApiResponse.SuccessResult($"Marked {unreadNotifications.Count} notifications as read");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read for user {UserId}", userId);
                return ApiResponse.ErrorResult("Failed to mark all notifications as read");
            }
        }

        public async Task<ApiResponse> DeleteNotificationAsync(int notificationId, Guid userId)
        {
            try
            {
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

                if (notification == null)
                {
                    return ApiResponse.ErrorResult("Notification not found");
                }

                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();

                return ApiResponse.SuccessResult("Notification deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification {NotificationId} for user {UserId}", notificationId, userId);
                return ApiResponse.ErrorResult("Failed to delete notification");
            }
        }

        public async Task<ApiResponse<int>> GetUnreadCountAsync(Guid userId)
        {
            try
            {
                var count = await _context.Notifications
                    .Where(n => n.UserId == userId && !n.IsRead)
                    .Where(n => n.ExpiryTime == null || n.ExpiryTime > DateTime.UtcNow)
                    .CountAsync();

                return ApiResponse.SuccessResult(count, "Unread count retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count for user {UserId}", userId);
                return ApiResponse.ErrorResult<int>("Failed to get unread count");
            }
        }

        public async Task<ApiResponse> UpdateNotificationPreferencesAsync(Guid userId, UpdateNotificationPreferencesRequestDto request)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return ApiResponse.ErrorResult("User not found");
                }

                user.NotificationPreferences = JsonSerializer.Serialize(request);
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return ApiResponse.SuccessResult("Notification preferences updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification preferences for user {UserId}", userId);
                return ApiResponse.ErrorResult("Failed to update notification preferences");
            }
        }

        public async Task<ApiResponse<NotificationPreferencesResponseDto>> GetNotificationPreferencesAsync(Guid userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return ApiResponse.ErrorResult<NotificationPreferencesResponseDto>("User not found");
                }

                NotificationPreferencesResponseDto preferences;
                
                if (!string.IsNullOrEmpty(user.NotificationPreferences))
                {
                    preferences = JsonSerializer.Deserialize<NotificationPreferencesResponseDto>(user.NotificationPreferences)!;
                }
                else
                {
                    preferences = new NotificationPreferencesResponseDto
                    {
                        EmailNotifications = true,
                        SmsNotifications = false,
                        PushNotifications = true,
                        InAppNotifications = true,
                        OrderUpdates = true,
                        PromotionalOffers = true,
                        SecurityAlerts = true,
                        ProductUpdates = true,
                        NewsletterSubscription = false,
                        WeeklyDigest = false,
                        Language = "vi",
                        TimeZone = "Asia/Ho_Chi_Minh",
                        UpdatedAt = DateTime.UtcNow
                    };
                }

                return ApiResponse.SuccessResult(preferences, "Notification preferences retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification preferences for user {UserId}", userId);
                return ApiResponse.ErrorResult<NotificationPreferencesResponseDto>("Failed to get notification preferences");
            }
        }

        public async Task<ApiResponse> SendOrderStatusNotificationAsync(int orderId, OrderStatus newStatus, string? message = null)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.User)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return ApiResponse.ErrorResult("Order not found");
                }

                var title = GetOrderStatusNotificationTitle(newStatus, order.OrderNumber);
                var notificationMessage = message ?? GetOrderStatusNotificationMessage(newStatus, order.OrderNumber);

                var notification = new CreateNotificationRequestDto
                {
                    UserId = order.User.Id,
                    Title = title,
                    Message = notificationMessage,
                    Type = NotificationType.Order,
                    ActionUrl = $"/orders/{orderId}",
                    Priority = ConvertNotificationPriorityToPriority(GetOrderNotificationPriority(newStatus)),
                    SendEmail = ShouldSendEmailForOrderStatus(newStatus),
                    Data = new Dictionary<string, object>
                    {
                        { "orderId", orderId },
                        { "orderNumber", order.OrderNumber },
                        { "newStatus", newStatus.ToString() },
                        { "statusText", GetOrderStatusText(newStatus) }
                    }
                };

                var result = await SendNotificationAsync(notification);

                if (result.Success && notification.SendEmail)
                {
                    var orderForEmail = await _context.Orders.Include(o => o.User).FirstOrDefaultAsync(o => o.Id == orderId);
                    if (orderForEmail != null)
                    {
                        await _emailService.SendOrderStatusUpdateEmailAsync(orderForEmail, newStatus);
                    }
                }

                return result.Success ? 
                    ApiResponse.SuccessResult("Order status notification sent") :
                    ApiResponse.ErrorResult("Failed to send order status notification");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending order status notification for order {OrderId}", orderId);
                return ApiResponse.ErrorResult("Failed to send order status notification");
            }
        }

        public async Task<ApiResponse> SendOrderConfirmationNotificationAsync(int orderId)
        {
            return await SendOrderStatusNotificationAsync(orderId, OrderStatus.Confirmed);
        }

        public async Task<ApiResponse> SendOrderShipmentNotificationAsync(int orderId, string? trackingNumber = null)
        {
            var orderToCheck = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (orderToCheck == null)
            {
                return ApiResponse.ErrorResult("Order not found");
            }

            var result = await SendOrderStatusNotificationAsync(orderId, OrderStatus.Shipped);
            
            if (result.Success && !string.IsNullOrEmpty(trackingNumber))
            {
                await _emailService.SendShipmentNotificationEmailAsync(orderToCheck, trackingNumber);
            }

            return result.Success ? 
                ApiResponse.SuccessResult("Order shipment notification sent") :
                ApiResponse.ErrorResult("Failed to send order shipment notification");
        }

        public async Task<ApiResponse> SendOrderDeliveryNotificationAsync(int orderId)
        {
            var result = await SendOrderStatusNotificationAsync(orderId, OrderStatus.Delivered);
            
            if (result.Success)
            {
                var orderToCheck = await _context.Orders.Include(o => o.User).FirstOrDefaultAsync(o => o.Id == orderId);
                if (orderToCheck != null)
                {
                    await _emailService.SendDeliveryConfirmationEmailAsync(orderToCheck);
                }
            }

            return result;
        }

        public async Task<ApiResponse> SendPaymentConfirmationNotificationAsync(int orderId, string transactionId)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.User)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return ApiResponse.ErrorResult("Order not found");
                }

                var notification = new CreateNotificationRequestDto
                {
                    UserId = order.User.Id,
                    Title = "Xác nhận thanh toán",
                    Message = $"Thanh toán cho đơn hàng #{order.OrderNumber} đã xác nhận thành công.",
                    Type = NotificationType.Payment,
                    ActionUrl = $"/orders/{orderId}",
                    Priority = ConvertNotificationPriorityToPriority(NotificationPriority.High),
                    SendEmail = true,
                    Data = new Dictionary<string, object>
                    {
                        { "orderId", orderId },
                        { "orderNumber", order.OrderNumber },
                        { "transactionId", transactionId },
                        { "amount", order.TotalAmount },
                        { "currency", order.Currency ?? "VND" }
                    }
                };

                var result = await SendNotificationAsync(notification);
                return result.Success ? 
                    ApiResponse.SuccessResult("Payment confirmation notification sent") :
                    ApiResponse.ErrorResult("Failed to send payment confirmation notification");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending payment confirmation notification for order {OrderId}", orderId);
                return ApiResponse.ErrorResult("Failed to send payment confirmation notification");
            }
        }

        public async Task<ApiResponse> SendLowStockAlertAsync(int productId, int currentStock, int threshold)
        {
            try
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                {
                    return ApiResponse.ErrorResult("Product not found");
                }

                // ✅ FIXED: Lấy tất cả admin bao gồm SuperAdmin, Admin và Staff
                var adminUsers = await _context.Users
                    .Where(u => u.Role == UserRole.SuperAdmin || u.Role == UserRole.Admin || u.Role == UserRole.Staff)
                    .Where(u => u.IsActive && !u.IsDeleted)
                    .Select(u => u.Id)
                    .ToListAsync();

                var bulkNotification = new CreateBulkNotificationRequestDto
                {
                    UserIds = adminUsers,
                    Title = "Cảnh báo tồn kho thấp",
                    Message = $"Sản phẩm {product.Name} (SKU: {product.SKU}) chỉ còn {currentStock} sản phẩm trong kho (Ngưỡng cảnh báo: {threshold}).",
                    Type = NotificationType.Alert,
                    ActionUrl = $"/admin/products/{productId}",
                    Priority = ConvertNotificationPriorityToPriority(NotificationPriority.High),
                    SendEmail = true,
                    Data = new Dictionary<string, object>
                    {
                        { "productId", productId },
                        { "productName", product.Name },
                        { "sku", product.SKU },
                        { "currentStock", currentStock },
                        { "threshold", threshold }
                    }
                };

                return await SendBulkNotificationAsync(bulkNotification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending low stock alert for product {ProductId}", productId);
                return ApiResponse.ErrorResult("Failed to send low stock alert");
            }
        }

        public async Task<ApiResponse> SendPromotionalNotificationAsync(CreatePromotionalNotificationRequestDto request)
        {
            try
            {
                var bulkRequest = new CreateBulkNotificationRequestDto
                {
                    UserIds = request.TargetUserIds ?? new List<Guid>(),
                    Title = request.Title,
                    Message = request.Message,
                    Type = NotificationType.Promotion,
                    ActionUrl = request.ActionUrl,
                    Priority = ConvertNotificationPriorityToPriority(NotificationPriority.Normal),
                    SendEmail = request.SendEmail,
                    SendSms = request.SendSms,
                    ScheduledTime = request.ScheduledTime,
                    SendToAll = request.SendToAll,
                    RoleFilter = request.TargetRoles,
                    RegisteredAfter = request.RegisteredAfter,
                    LastActiveAfter = request.LastActiveAfter,
                    Data = new Dictionary<string, object>
                    {
                        { "campaignId", request.CampaignId ?? Guid.NewGuid().ToString() },
                        { "campaignSource", request.CampaignSource ?? "system" },
                        { "imageUrl", request.ImageUrl ?? "" },
                        { "expiryTime", request.ExpiryTime?.ToString() ?? "" }
                    }
                };

                return await SendBulkNotificationAsync(bulkRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending promotional notification");
                return ApiResponse.ErrorResult("Failed to send promotional notification");
            }
        }

        public async Task<ApiResponse> SendMaintenanceNotificationAsync(DateTime startTime, DateTime endTime, string message)
        {
            try
            {
                var bulkNotification = new CreateBulkNotificationRequestDto
                {
                    SendToAll = true,
                    Title = "Thông báo bảo trì hệ thống",
                    Message = $"Hệ thống sé bảo trì từ {startTime:dd/MM/yyyy HH:mm} đến {endTime:dd/MM/yyyy HH:mm}. {message}",
                    Type = NotificationType.System,
                    Priority = ConvertNotificationPriorityToPriority(NotificationPriority.High),
                    SendEmail = true,
                    ScheduledTime = DateTime.UtcNow,
                    Data = new Dictionary<string, object>
                    {
                        { "maintenanceStart", startTime },
                        { "maintenanceEnd", endTime },
                        { "estimatedDuration", (endTime - startTime).TotalHours },
                        { "type", "maintenance" }
                    }
                };

                return await SendBulkNotificationAsync(bulkNotification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending maintenance notification");
                return ApiResponse.ErrorResult("Failed to send maintenance notification");
            }
        }

        #region Private Helper Methods

        private NotificationResponseDto MapToNotificationResponseDto(Notification notification)
        {
            return new NotificationResponseDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                TypeName = GetNotificationTypeName(notification.Type),
                ActionUrl = notification.ActionUrl,
                Data = !string.IsNullOrEmpty(notification.Data) ? 
                    JsonSerializer.Deserialize<Dictionary<string, object>>(notification.Data) : null,
                IsRead = notification.IsRead,
                Priority = ConvertNotificationPriorityToPriority(notification.Priority),
                PriorityName = GetPriorityName(notification.Priority),
                CreatedAt = notification.CreatedAt,
                ReadAt = notification.ReadAt,
                ExpiryTime = notification.ExpiryTime,
                IsExpired = notification.ExpiryTime.HasValue && notification.ExpiryTime < DateTime.UtcNow
            };
        }

        private Priority ConvertNotificationPriorityToPriority(NotificationPriority notificationPriority)
        {
            return notificationPriority switch
            {
                NotificationPriority.Low => Priority.Low,
                NotificationPriority.Normal => Priority.Medium,
                NotificationPriority.High => Priority.High,
                NotificationPriority.Urgent => Priority.Urgent,
                NotificationPriority.Critical => Priority.Critical,
                _ => Priority.Medium
            };
        }

        private string GetNotificationTypeName(NotificationType type)
        {
            return type switch
            {
                NotificationType.General => "Thông báo chung",
                NotificationType.Order => "Đơn hàng",
                NotificationType.Payment => "Thanh toán",
                NotificationType.Shipping => "Vận chuyển",
                NotificationType.Product => "Sản phẩm",
                NotificationType.Promotion => "Khuyến mãi",
                NotificationType.System => "Hệ thống",
                NotificationType.Security => "Bảo mật",
                NotificationType.Marketing => "Marketing",
                NotificationType.Reminder => "Nhắc nhở",
                NotificationType.Alert => "Cảnh báo",
                NotificationType.Newsletter => "Bản tin",
                _ => "Không xác định"
            };
        }

        private string GetPriorityName(NotificationPriority priority)
        {
            return priority switch
            {
                NotificationPriority.Low => "Thấp",
                NotificationPriority.Normal => "Trung bình",
                NotificationPriority.High => "Cao",
                NotificationPriority.Urgent => "Khẩn cấp",
                NotificationPriority.Critical => "Nghiêm trọng",
                _ => "Không xác định"
            };
        }

        private string GetOrderStatusNotificationTitle(OrderStatus status, string orderNumber)
        {
            return status switch
            {
                OrderStatus.Confirmed => $"Đơn hàng #{orderNumber} đã được xác nhận",
                OrderStatus.Processing => $"Đơn hàng #{orderNumber} đang được xử lý",
                OrderStatus.Packed => $"Đơn hàng #{orderNumber} đã được đóng gói",
                OrderStatus.Shipped => $"Đơn hàng #{orderNumber} đang được vận chuyển",
                OrderStatus.Delivered => $"Đơn hàng #{orderNumber} đã được giao thành công",
                OrderStatus.Cancelled => $"Đơn hàng #{orderNumber} đã bị hủy",
                OrderStatus.Returned => $"Đơn hàng #{orderNumber} đã được trả lại",
                OrderStatus.Refunded => $"Đơn hàng #{orderNumber} đã được hoàn tiền",
                _ => $"Cập nhật đơn hàng #{orderNumber}"
            };
        }

        private string GetOrderStatusNotificationMessage(OrderStatus status, string orderNumber)
        {
            return status switch
            {
                OrderStatus.Confirmed => $"Đơn hàng #{orderNumber} của bạn đã được xác nhận và sẽ được xử lý sớm nhất.",
                OrderStatus.Processing => $"Đơn hàng #{orderNumber} đang được chuẩn bị và đóng gói.",
                OrderStatus.Packed => $"Đơn hàng #{orderNumber} đã được đóng gói và sẵn sàng giao hàng.",
                OrderStatus.Shipped => $"Đơn hàng #{orderNumber} đã được giao cho đơn vị vận chuyển.",
                OrderStatus.Delivered => $"Đơn hàng #{orderNumber} đã được giao thành công. Cảm ơn bạn đã mua hàng!",
                OrderStatus.Cancelled => $"Đơn hàng #{orderNumber} đã bị hủy. Nếu bạn đã thanh toán, tiền sẽ được hoàn lại.",
                OrderStatus.Returned => $"Đơn hàng #{orderNumber} đã được trả lại thành công.",
                OrderStatus.Refunded => $"Tiền của đơn hàng #{orderNumber} đã được hoàn lại vào tài khoản của bạn.",
                _ => $"Trạng thái đơn hàng #{orderNumber} đã được cập nhật."
            };
        }

        private NotificationPriority GetOrderNotificationPriority(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Confirmed => NotificationPriority.High,
                OrderStatus.Shipped => NotificationPriority.High,
                OrderStatus.Delivered => NotificationPriority.High,
                OrderStatus.Cancelled => NotificationPriority.Normal,
                OrderStatus.Returned => NotificationPriority.Normal,
                OrderStatus.Refunded => NotificationPriority.High,
                _ => NotificationPriority.Normal
            };
        }

        private bool ShouldSendEmailForOrderStatus(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Confirmed => true,
                OrderStatus.Shipped => true,
                OrderStatus.Delivered => true,
                OrderStatus.Cancelled => true,
                OrderStatus.Refunded => true,
                _ => false
            };
        }

        private string GetOrderStatusText(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "Chờ xử lý",
                OrderStatus.Confirmed => "Đã xác nhận",
                OrderStatus.Processing => "Đang xử lý",
                OrderStatus.Packed => "Đã đóng gói",
                OrderStatus.Shipped => "Đang vận chuyển",
                OrderStatus.OutForDelivery => "Đang giao hàng",
                OrderStatus.Delivered => "Đã giao hàng",
                OrderStatus.Cancelled => "Đã hủy",
                OrderStatus.Returned => "Đã trả hàng",
                OrderStatus.Refunded => "Đã hoàn tiền",
                OrderStatus.Completed => "Đã hoàn thành", 
                _ => "Không xác định"
            };
        }

        #endregion
    }
}