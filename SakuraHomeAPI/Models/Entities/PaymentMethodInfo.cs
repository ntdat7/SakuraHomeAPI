using SakuraHomeAPI.Models.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SakuraHomeAPI.Models.Entities
{
    [Table("PaymentMethods")]
    public class PaymentMethodInfo : BaseEntity
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [MaxLength(50)]
        public string Code { get; set; }

        [MaxLength(500)]
        public string LogoUrl { get; set; }

        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;

        [Column(TypeName = "decimal(5,2)")]
        public decimal FeePercentage { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal FixedFee { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal MinAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal MaxAmount { get; set; } = 0;
    }
}