using SakuraHomeAPI.Models.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SakuraHomeAPI.Models.Entities
{
    [Table("ShippingRates")]
    public class ShippingRate : BaseEntity
    {
        [Required]
        public int ShippingZoneId { get; set; } // Changed from Guid to int to match ShippingZone.Id

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Rate { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? MinWeight { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? MaxWeight { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MinOrderAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? FreeShippingThreshold { get; set; }

        public int EstimatedDays { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual ShippingZone ShippingZone { get; set; }
    }
}