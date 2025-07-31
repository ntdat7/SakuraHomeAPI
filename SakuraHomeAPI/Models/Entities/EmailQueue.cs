using SakuraHomeAPI.Models.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SakuraHomeAPI.Models.Entities
{
    [Table("EmailQueue")]
    public class EmailQueue : BaseEntity
    {
        [Required, MaxLength(255), EmailAddress]
        public string ToEmail { get; set; }

        [MaxLength(255)]
        public string ToName { get; set; }

        [Required, MaxLength(255), EmailAddress]
        public string FromEmail { get; set; }

        [MaxLength(255)]
        public string FromName { get; set; }

        [Required, MaxLength(500)]
        public string Subject { get; set; }

        [Required]
        public string Body { get; set; }

        public bool IsHtml { get; set; } = true;

        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Sent, Failed

        public int AttemptCount { get; set; } = 0;
        public DateTime? SentAt { get; set; }
        public DateTime? ScheduledAt { get; set; }

        [MaxLength(1000)]
        public string ErrorMessage { get; set; }

        public int Priority { get; set; } = 5; // 1-10, 1 = highest
    }
}
