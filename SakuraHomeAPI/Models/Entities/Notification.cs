using SakuraHomeAPI.Models.Base;
using SakuraHomeAPI.Models.Entities.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SakuraHomeAPI.Models.Entities
{
    public class Notification : AuditableEntity
    {
        [Required]
        public Guid UserId { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; }

        [Required, MaxLength(1000)]
        public string Message { get; set; }

        [MaxLength(50)]
        public string Type { get; set; } = "Info"; // Info, Warning, Success, Error

        public bool IsRead { get; set; } = false;

        public DateTime? ReadAt { get; set; }

        [MaxLength(500)]
        public string ActionUrl { get; set; }

        // Navigation Properties
        public virtual User User { get; set; }
    }
}