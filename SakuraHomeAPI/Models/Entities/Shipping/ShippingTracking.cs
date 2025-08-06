using SakuraHomeAPI.Models.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SakuraHomeAPI.Models.Entities.Shipping
{
    [Table("ShippingTrackings")]
    public class ShippingTracking : BaseEntity
    {
        public int ShippingOrderId { get; set; }
        
        [Required, MaxLength(50)]
        public string Status { get; set; }
        
        [MaxLength(200)]
        public string StatusDescription { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        [MaxLength(100)]
        public string Location { get; set; }
        
        [MaxLength(500)]
        public string Notes { get; set; }
        
        // Navigation properties
        public virtual ShippingOrder ShippingOrder { get; set; }
    }
}