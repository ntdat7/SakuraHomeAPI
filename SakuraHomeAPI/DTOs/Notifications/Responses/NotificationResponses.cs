using SakuraHomeAPI.Models.Enums;

namespace SakuraHomeAPI.DTOs.Notifications.Responses
{
    /// <summary>
    /// Notification response
    /// </summary>
    public class NotificationResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public string TypeName { get; set; }
        public string? ActionUrl { get; set; }
        public Dictionary<string, object>? Data { get; set; }
        public bool IsRead { get; set; }
        public Priority Priority { get; set; }
        public string PriorityName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime? ExpiryTime { get; set; }
        public bool IsExpired { get; set; }
        public string? ImageUrl { get; set; }
        public string? CampaignId { get; set; }
    }

    /// <summary>
    /// Notification summary
    /// </summary>
    public class NotificationSummaryDto
    {
        public int TotalCount { get; set; }
        public int UnreadCount { get; set; }
        public int ReadCount { get; set; }
        public Dictionary<string, int> CountByType { get; set; } = new();
        public DateTime? LastNotificationTime { get; set; }
        public DateTime? LastReadTime { get; set; }
    }

    /// <summary>
    /// Notification preferences response
    /// </summary>
    public class NotificationPreferencesResponseDto
    {
        public bool EmailNotifications { get; set; }
        public bool SmsNotifications { get; set; }
        public bool PushNotifications { get; set; }
        public bool InAppNotifications { get; set; }

        // Specific notification types
        public bool OrderUpdates { get; set; }
        public bool PromotionalOffers { get; set; }
        public bool SecurityAlerts { get; set; }
        public bool ProductUpdates { get; set; }
        public bool NewsletterSubscription { get; set; }
        public bool WeeklyDigest { get; set; }

        // Timing preferences
        public TimeSpan? QuietHoursStart { get; set; }
        public TimeSpan? QuietHoursEnd { get; set; }
        public List<DayOfWeek>? NoNotificationDays { get; set; }

        public string Language { get; set; }
        public string TimeZone { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Real-time notification event
    /// </summary>
    public class NotificationEventDto
    {
        public string EventType { get; set; } // "notification_received", "notification_read", "notification_deleted"
        public NotificationResponseDto? Notification { get; set; }
        public int? UnreadCount { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Message { get; set; }
    }

    /// <summary>
    /// Notification statistics (for admin)
    /// </summary>
    public class NotificationStatsDto
    {
        public int TotalSent { get; set; }
        public int TotalRead { get; set; }
        public int TotalUnread { get; set; }
        public int TotalExpired { get; set; }
        public decimal ReadRate { get; set; }
        public Dictionary<string, int> SentByType { get; set; } = new();
        public Dictionary<string, int> ReadByType { get; set; } = new();
        public Dictionary<string, decimal> ReadRateByType { get; set; } = new();
        public Dictionary<string, int> SentByChannel { get; set; } = new();
        public List<DailyNotificationStatsDto> DailyStats { get; set; } = new();
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    /// <summary>
    /// Daily notification statistics
    /// </summary>
    public class DailyNotificationStatsDto
    {
        public DateTime Date { get; set; }
        public int Sent { get; set; }
        public int Read { get; set; }
        public int Clicked { get; set; }
        public decimal ReadRate { get; set; }
        public decimal ClickRate { get; set; }
    }

    /// <summary>
    /// Email delivery status
    /// </summary>
    public class EmailDeliveryStatusDto
    {
        public int EmailId { get; set; }
        public string To { get; set; }
        public string Subject { get; set; }
        public EmailStatus Status { get; set; }
        public string StatusName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? FailedAt { get; set; }
        public string? ErrorMessage { get; set; }
        public int Attempts { get; set; }
        public DateTime? NextRetryAt { get; set; }
    }

    /// <summary>
    /// Campaign performance
    /// </summary>
    public class CampaignPerformanceDto
    {
        public string CampaignId { get; set; }
        public string Name { get; set; }
        public int TotalSent { get; set; }
        public int TotalOpened { get; set; }
        public int TotalClicked { get; set; }
        public int TotalConverted { get; set; }
        public decimal OpenRate { get; set; }
        public decimal ClickRate { get; set; }
        public decimal ConversionRate { get; set; }
        public decimal Revenue { get; set; }
        public decimal ROI { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public Dictionary<string, int> ResultsByChannel { get; set; } = new();
    }
}