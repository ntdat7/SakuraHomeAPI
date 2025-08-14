using SakuraHomeAPI.Models.Base;
using SakuraHomeAPI.Models.Entities.Identity;
using SakuraHomeAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SakuraHomeAPI.Models.Entities
{
    [Table("Notifications")]
    public class Notification : AuditableEntity
    {
        [Required]
        public Guid UserId { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        public NotificationType Type { get; set; } = NotificationType.Info;

        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

        public bool IsRead { get; set; } = false;

        public DateTime? ReadAt { get; set; }

        [MaxLength(500)]
        public string ActionUrl { get; set; } = string.Empty;

        // Additional properties for rich notifications
        public string? Data { get; set; } // JSON data for additional context

        public DateTime? ScheduledTime { get; set; } // For scheduled notifications

        public DateTime? ExpiryTime { get; set; } // When notification expires

        // Navigation Properties
        public virtual User User { get; set; } = null!;
    }
}