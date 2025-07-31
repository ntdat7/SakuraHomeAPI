using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SakuraHomeAPI.Models.Base;
using SakuraHomeAPI.Models.Entities.Catalog;

namespace SakuraHomeAPI.Models.Entities.Products
{
    /// <summary>
    /// Product attribute values
    /// </summary>
    [Table("ProductAttributeValues")]
    public class ProductAttributeValue : AuditableEntity
    {
        public int ProductId { get; set; }
        public int AttributeId { get; set; }

        [Required]
        public string Value { get; set; }

        [MaxLength(255)]
        public string DisplayValue { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? NumericValue { get; set; }

        [MaxLength(7)]
        public string ColorCode { get; set; }

        public virtual Product Product { get; set; }
        public virtual ProductAttribute Attribute { get; set; }
        public virtual ICollection<Translation> Translations { get; set; } = new List<Translation>();

        #region Computed Properties

        [NotMapped]
        public string EffectiveDisplayValue =>
            !string.IsNullOrEmpty(DisplayValue) ? DisplayValue : Value;

        #endregion
    }
}
