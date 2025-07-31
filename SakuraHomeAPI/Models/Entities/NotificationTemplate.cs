using SakuraHomeAPI.Models.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SakuraHomeAPI.Models.Entities
{
    [Table("NotificationTemplates")]
    public class NotificationTemplate : BaseEntity
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required, MaxLength(200)]
        public string Subject { get; set; }

        [Required]
        public string BodyTemplate { get; set; }

        [MaxLength(50)]
        public string Type { get; set; } = "Email"; // Email, SMS, Push

        [MaxLength(5)]
        public string Language { get; set; } = "vi";

        public bool IsActive { get; set; } = true;
    }
}