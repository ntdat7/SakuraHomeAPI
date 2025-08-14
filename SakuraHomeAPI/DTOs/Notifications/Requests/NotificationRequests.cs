using System.ComponentModel.DataAnnotations;
using SakuraHomeAPI.Models.Enums;

namespace SakuraHomeAPI.DTOs.Notifications.Requests
{
    /// <summary>
    /// Request to create a notification
    /// </summary>
    public class CreateNotificationRequestDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; }

        public NotificationType Type { get; set; } = NotificationType.General;

        [MaxLength(500)]
        public string? ActionUrl { get; set; }

        public Dictionary<string, object>? Data { get; set; }

        public bool SendEmail { get; set; } = false;

        public bool SendSms { get; set; } = false;

        public DateTime? ScheduledTime { get; set; }

        public Priority Priority { get; set; } = Priority.Medium;
    }

    /// <summary>
    /// Request to create bulk notifications
    /// </summary>
    public class CreateBulkNotificationRequestDto
    {
        [Required]
        public List<Guid> UserIds { get; set; } = new();

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; }

        public NotificationType Type { get; set; } = NotificationType.General;

        [MaxLength(500)]
        public string? ActionUrl { get; set; }

        public Dictionary<string, object>? Data { get; set; }

        public bool SendEmail { get; set; } = false;

        public bool SendSms { get; set; } = false;

        public DateTime? ScheduledTime { get; set; }

        public Priority Priority { get; set; } = Priority.Medium;

        /// <summary>
        /// Send to all users if true (ignores UserIds)
        /// </summary>
        public bool SendToAll { get; set; } = false;

        /// <summary>
        /// Filter users by role
        /// </summary>
        public List<string>? RoleFilter { get; set; }

        /// <summary>
        /// Filter users by registration date
        /// </summary>
        public DateTime? RegisteredAfter { get; set; }

        /// <summary>
        /// Filter users by last activity
        /// </summary>
        public DateTime? LastActiveAfter { get; set; }
    }

    /// <summary>
    /// Request to update notification preferences
    /// </summary>
    public class UpdateNotificationPreferencesRequestDto
    {
        public bool EmailNotifications { get; set; } = true;
        public bool SmsNotifications { get; set; } = false;
        public bool PushNotifications { get; set; } = true;
        public bool InAppNotifications { get; set; } = true;

        // Specific notification types
        public bool OrderUpdates { get; set; } = true;
        public bool PromotionalOffers { get; set; } = true;
        public bool SecurityAlerts { get; set; } = true;
        public bool ProductUpdates { get; set; } = true;
        public bool NewsletterSubscription { get; set; } = false;
        public bool WeeklyDigest { get; set; } = false;

        // Timing preferences
        public TimeSpan? QuietHoursStart { get; set; }
        public TimeSpan? QuietHoursEnd { get; set; }
        public List<DayOfWeek>? NoNotificationDays { get; set; }

        public string Language { get; set; } = "vi";
        public string TimeZone { get; set; } = "Asia/Ho_Chi_Minh";
    }

    /// <summary>
    /// Request to create promotional notification
    /// </summary>
    public class CreatePromotionalNotificationRequestDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; }

        [MaxLength(500)]
        public string? ActionUrl { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public DateTime? ScheduledTime { get; set; }

        public DateTime? ExpiryTime { get; set; }

        // Target audience
        public List<Guid>? TargetUserIds { get; set; }
        public List<string>? TargetRoles { get; set; }
        public bool SendToAll { get; set; } = false;

        // Filters
        public DateTime? RegisteredAfter { get; set; }
        public DateTime? LastActiveAfter { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public int? MinOrderCount { get; set; }

        // Channels
        public bool SendEmail { get; set; } = false;
        public bool SendSms { get; set; } = false;
        public bool SendPush { get; set; } = true;
        public bool SendInApp { get; set; } = true;

        // Campaign tracking
        [MaxLength(100)]
        public string? CampaignId { get; set; }

        [MaxLength(100)]
        public string? CampaignSource { get; set; }
    }

    /// <summary>
    /// Request to mark notification as read
    /// </summary>
    public class MarkNotificationReadRequestDto
    {
        [Required]
        public List<int> NotificationIds { get; set; } = new();
    }

    /// <summary>
    /// Request to delete notifications
    /// </summary>
    public class DeleteNotificationsRequestDto
    {
        [Required]
        public List<int> NotificationIds { get; set; } = new();
    }
}