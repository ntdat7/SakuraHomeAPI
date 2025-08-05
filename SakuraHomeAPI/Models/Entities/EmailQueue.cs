using SakuraHomeAPI.Models.Base;
using SakuraHomeAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SakuraHomeAPI.Models.Entities
{
    [Table("EmailQueue")]
    public class EmailQueue : BaseEntity
    {
        [Required, MaxLength(255), EmailAddress]
        public string To { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? ToName { get; set; }

        [Required, MaxLength(255), EmailAddress]
        public string From { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? FromName { get; set; }

        [Required, MaxLength(500)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Body { get; set; } = string.Empty;

        public bool IsHtml { get; set; } = true;

        public EmailDeliveryStatus Status { get; set; } = EmailDeliveryStatus.Pending;
        
        public EmailType Type { get; set; } = EmailType.General;

        public int Attempts { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SentAt { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public DateTime? NextRetryAt { get; set; }

        [MaxLength(1000)]
        public string? ErrorMessage { get; set; }

        public int Priority { get; set; } = 5; // 1-10, 1 = highest
    }
}
