using SakuraHomeAPI.Models.Base;
using SakuraHomeAPI.Models.Entities.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SakuraHomeAPI.Models.Entities
{
    [Table("ContactMessages")]
    public class ContactMessage : BaseEntity, IAuditable, ISoftDelete
    {
        public Guid? UserId { get; set; } // Nullable for anonymous messages

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required, MaxLength(255), EmailAddress]
        public string Email { get; set; }

        [MaxLength(20)]
        public string Phone { get; set; }

        [Required, MaxLength(200)]
        public string Subject { get; set; }

        [Required, MaxLength(2000)]
        public string Message { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, InProgress, Resolved, Closed

        [MaxLength(1000)]
        public string AdminResponse { get; set; }

        public DateTime? RespondedAt { get; set; }

        public Guid? RespondedBy { get; set; }

        // Audit properties (implementing IAuditable with int)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }

        // Soft delete properties (implementing ISoftDelete with int)
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public int? DeletedBy { get; set; }

        // Navigation Properties
        public virtual User User { get; set; }
    }
}