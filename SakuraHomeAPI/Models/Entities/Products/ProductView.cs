using SakuraHomeAPI.Models.Base;
using SakuraHomeAPI.Models.Entities.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SakuraHomeAPI.Models.Entities.Products
{
    [Table("ProductViews")]
    public class ProductView : BaseEntity
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid ProductId { get; set; }

        public int ViewCount { get; set; } = 1;

        public DateTime LastViewedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(45)]
        public string IpAddress { get; set; }

        [MaxLength(500)]
        public string UserAgent { get; set; }

        [MaxLength(200)]
        public string Referrer { get; set; }

        // Navigation Properties
        public virtual User User { get; set; }
        public virtual Product Product { get; set; }
    }
}