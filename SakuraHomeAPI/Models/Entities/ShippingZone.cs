using SakuraHomeAPI.Models.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace SakuraHomeAPI.Models.Entities
{
    [Table("ShippingZones")]
    public class ShippingZone : BaseEntity
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Required, MaxLength(1000)]
        public string Countries { get; set; } // JSON array of country codes

        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;

        // Navigation Properties
        public virtual ICollection<ShippingRate> ShippingRates { get; set; } = new List<ShippingRate>();
    }
}