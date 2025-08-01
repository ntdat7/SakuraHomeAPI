using SakuraHomeAPI.Models.Base;
using SakuraHomeAPI.Models.Entities.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SakuraHomeAPI.Models.Entities
{
    [Table("UserActivities")]
    public class UserActivity : LogEntity
    {
        // Override UserId để dùng Guid thay vì int (nếu LogEntity vẫn dùng int)
        public new Guid? UserId { get; set; }

        [Required, MaxLength(100)]
        public string ActivityType { get; set; } // Login, Logout, ViewProduct, AddToCart, Purchase, etc.

        [MaxLength(500)]
        public string Description { get; set; }

        [MaxLength(1000)]
        public string Details { get; set; } // JSON string for additional data

        public int? RelatedEntityId { get; set; } // Product ID, Order ID, etc. - Changed from Guid? to int?

        [MaxLength(100)]
        public string RelatedEntityType { get; set; } // Product, Order, etc.

        // Override navigation property
        public new virtual User User { get; set; }

    }
}